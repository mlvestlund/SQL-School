using System.Data.SqlClient;

namespace School
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string connectionString = @"Data Source=(localdb)\.;Initial Catalog=SchoolDB;Integrated Security=True";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                while (true)
                {
                    Console.WriteLine("Menu");
                    Console.WriteLine("1. Retrieve all students");
                    Console.WriteLine("2. Retrieve all students in a specific class");
                    Console.WriteLine("3. Add new staff");
                    Console.WriteLine("4. Retrieve staff");
                    Console.WriteLine("5. Retrieve all grades set in the last month");
                    Console.WriteLine("6. Average grade per course");
                    Console.WriteLine("7. Add new students");
                    Console.Write("Enter number of your option: ");


                    int choice = int.Parse(Console.ReadLine());
                    switch (choice)
                    {
                        case 1:
                            GetAllStudents(connection);
                            break;
                        case 2:
                            GetAllStudentsInClass(connection);
                            break;
                        case 3:
                            AddNewStaff(connection);
                            break;
                        case 4:
                            GetStaff(connection);
                            break;
                        case 5:
                            GetRecentGrades(connection);
                            break;
                        case 6:
                            GetAverageGradePerCourse(connection);
                            break;
                        case 7:
                            AddNewStudent(connection);
                            break;
                        default:
                            Console.WriteLine("Invalid input");
                            break;
                    }

                    Console.Write("Press enter to return to menu");
                    Console.ReadLine();
                    Console.Clear();
                }
            }
        }


        static void GetAllStudents(SqlConnection connection)
        {
            Console.WriteLine("Sorting");
            Console.WriteLine("1. First name (A-Z)");
            Console.WriteLine("2. First name (Z-A)");
            Console.WriteLine("3. Last name (A-Z)");
            Console.WriteLine("4. Last name (Z-A)");

            Console.Write("Enter number of your option: ");
            int sortChoice = int.Parse(Console.ReadLine());

            string sortField;
            string sortOrder;

            switch (sortChoice)
            {
                case 1:
                    sortField = "FirstName";
                    sortOrder = "ASC";
                    break;
                case 2:
                    sortField = "FirstName";
                    sortOrder = "DESC";
                    break;
                case 3:
                    sortField = "LastName";
                    sortOrder = "ASC";
                    break;
                case 4:
                    sortField = "LastName";
                    sortOrder = "DESC";
                    break;
                default:
                    Console.WriteLine("Invalid input. Sorted as default: First name ascending");
                    sortField = "FirstName";
                    sortOrder = "ASC";
                    break;
            }

            using (SqlCommand command = new SqlCommand($"SELECT * FROM Students ORDER BY {sortField} {sortOrder}", connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    int count = 1;
                    while (reader.Read())
                    {
                        Console.WriteLine($"{count}. {reader["FirstName"]} {reader["LastName"]}");
                        count++;
                    }
                }
            }
        }

        static void GetAllStudentsInClass(SqlConnection connection)
        {


            using (SqlCommand command = new SqlCommand($"SELECT className FROM Classes", connection))
            {
                List<string> classNames = new List<string>();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Console.WriteLine("Class names");
                    int count = 1;
                    while (reader.Read())
                    {
                        string className = reader.GetString(reader.GetOrdinal("ClassName"));
                        classNames.Add(className);
                        Console.WriteLine($"{count}. {className}");
                        count++;
                    }
                }
                Console.Write("Enter number of your option: ");
                int input = int.Parse(Console.ReadLine());

                if (input > 0 && input <= classNames.Count)
                {
                    string classNameChoice = classNames[input - 1];
                    using (SqlCommand classStudentsCommand = new SqlCommand("SELECT FirstName, LastName FROM Students WHERE ClassId = (SELECT ClassId FROM Classes WHERE ClassName = @ClassName)", connection))
                    {
                        classStudentsCommand.Parameters.AddWithValue("@ClassName", classNameChoice);

                        using (SqlDataReader classStudentsReader = classStudentsCommand.ExecuteReader())
                        {
                            int classStudentsCount = 1;
                            Console.WriteLine($"Students in class {classNameChoice}:");
                            while (classStudentsReader.Read())
                            {
                                Console.WriteLine($"{classStudentsCount}. {classStudentsReader["FirstName"]} {classStudentsReader["LastName"]}");
                                classStudentsCount++;
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input.");
                }

            }
        }

        static void AddNewStaff(SqlConnection connection)
        {
            Console.WriteLine("Fill in information about the new employee");
            Console.Write("First name: ");
            string firstName = Console.ReadLine();
            Console.Write("Last name: ");
            string lastName = Console.ReadLine();

            using (SqlCommand command = new SqlCommand("INSERT INTO Staff (FirstName, LastName) VALUES (@FirstName, @LastName)", connection))
            {
                command.Parameters.AddWithValue("@FirstName", firstName);
                command.Parameters.AddWithValue("@LastName", lastName);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    Console.WriteLine("New record added successfully!");
                }
                else
                {
                    Console.WriteLine("Failed to add a new record.");
                }
            }
        }

        static void GetStaff(SqlConnection connection)
        {
            Console.WriteLine("Do you want to see all employees (1) or within a specific role (2)?");
            Console.Write("Enter number of your option: ");
            int input = int.Parse(Console.ReadLine());

            string sqlQuery = "SELECT FirstName, LastName FROM Staff";

            if (input == 2)
            {
                sqlQuery = $"{sqlQuery} WHERE StaffRoleId = (SELECT StaffRoleId FROM StaffRoles WHERE StaffRole = @StaffRole)";

                using (SqlCommand command = new SqlCommand("SELECT StaffRole FROM StaffRoles", connection))
                {
                    List<string> staffRoles = new List<string>();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        Console.WriteLine("Staff roles");
                        int count = 1;
                        while (reader.Read())
                        {
                            string staffRole = reader.GetString(reader.GetOrdinal("StaffRole"));
                            staffRoles.Add(staffRole);
                            Console.WriteLine($"{count}. {staffRole}");
                            count++;
                        }
                    }

                    Console.Write("Enter number of your option: ");
                    int staffRoleInput = int.Parse(Console.ReadLine());
                    string staffRoleChoice = staffRoles[staffRoleInput - 1];
                    using (SqlCommand staffRoleCommand = new SqlCommand(sqlQuery, connection))
                    {
                        staffRoleCommand.Parameters.AddWithValue("@StaffRole", staffRoleChoice);
                        using (SqlDataReader staffRoleReader = staffRoleCommand.ExecuteReader())
                        {
                            int staffRoleCount = 1;
                            Console.WriteLine($"Personell with role {staffRoleChoice}");
                            while (staffRoleReader.Read())
                            {
                                Console.WriteLine($"{staffRoleCount}. {staffRoleReader["FirstName"]} {staffRoleReader["LastName"]}");
                                staffRoleCount++;
                            }
                        }
                    }
                }
            }
            else if (input == 1)
            {
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int count = 1;
                        Console.WriteLine("All Personell");
                        while (reader.Read())
                        {
                            Console.WriteLine($"{count}. {reader["FirstName"]} {reader["LastName"]}");
                            count++;
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Invalid input.");
            }
        }

        static void GetRecentGrades(SqlConnection connection)
        {
            using (SqlCommand command = new SqlCommand("SELECT Students.FirstName, Students.LastName, Courses.CourseName, Grades.Grade FROM Students JOIN Grades ON Students.StudentId = Grades.StudentId JOIN Courses ON Grades.CourseId = Courses.CourseId WHERE Grades.DateOfGrade >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) - 1, 0)", connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Console.WriteLine("All grades set in the last month");
                    while (reader.Read())
                    {
                        string firstName = reader["FirstName"].ToString();
                        string lastName = reader["LastName"].ToString();
                        string courseName = reader["CourseName"].ToString();
                        int grade = Convert.ToInt32(reader["Grade"]);

                        Console.WriteLine($"Student: {firstName} {lastName}, Course: {courseName}, Grade: {grade}");
                    }
                }
            }
        }

        static void GetAverageGradePerCourse(SqlConnection connection)
        {
            using (SqlCommand command = new SqlCommand("SELECT CourseName, AVG(Grade) AS AvgGrade, MAX(Grade) AS MaxGrade, MIN(Grade) AS MinGrade FROM Courses INNER JOIN Grades ON Courses.CourseId = Grades.CourseId GROUP BY Courses.CourseId, Courses.CourseName", connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Console.WriteLine("Average grade per course");
                    while (reader.Read())
                    {
                        string courseName = reader["CourseName"].ToString();
                        int avgGrade = Convert.ToInt32(reader["AvgGrade"]);
                        int maxGrade = Convert.ToInt32(reader["MaxGrade"]);
                        int minGrade = Convert.ToInt32(reader["MinGrade"]);

                        Console.WriteLine($"Course: {courseName}, Average Grade: {avgGrade}, Max Grade: {maxGrade}, Min Grade: {minGrade}");
                    }
                }
            }
        }
        static void AddNewStudent(SqlConnection connection)
        {
            Console.WriteLine("Fill in information about the new student");
            Console.Write("First name: ");
            string firstName = Console.ReadLine();
            Console.Write("Last name: ");
            string lastName = Console.ReadLine();

            using (SqlCommand command = new SqlCommand("INSERT INTO Students (FirstName, LastName) VALUES (@FirstName, @LastName)", connection))
            {
                command.Parameters.AddWithValue("@FirstName", firstName);
                command.Parameters.AddWithValue("@LastName", lastName);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    Console.WriteLine("New record added successfully!");
                }
                else
                {
                    Console.WriteLine("Failed to add a new record.");
                }
            }
        }
    }
}