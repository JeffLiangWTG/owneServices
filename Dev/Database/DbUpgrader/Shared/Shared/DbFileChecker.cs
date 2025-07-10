#if DEBUG
using System;
using System.IO;

namespace Enterprise.DbUpgrader.Shared
{
	public class DbFileChecker
	{
		public DbFileChecker(string dbFileName)
		{
			if (String.IsNullOrEmpty(dbFileName))
			{
				throw new ArgumentException("File name cannot be blank", nameof(dbFileName));
			}
			this.dbFileName = dbFileName;
		}

		public bool IsDbInFile(string serverName, string databaseName)
		{
			bool result = false;
			serverName = serverName.Trim();
			databaseName = databaseName.Trim();

			if (File.Exists(dbFileName))
			{
				using (StreamReader reader = new StreamReader(dbFileName))
				{
					string line;

					while ((line = reader.ReadLine()) != null)
					{
						if (IsCurrentDatabase(line, serverName, databaseName))
						{
							result = true;
							break;
						}
					}
				}
			}

			return result;
		}

		bool IsCurrentDatabase(string fileLine, string serverName, string databaseName)
		{
			bool result = false;
			string[] fileDbInfo = fileLine.Split(',');

			if (fileDbInfo.Length == 2)
			{
				string fileServerName = fileDbInfo[0].Trim();
				string fileDatabaseName = fileDbInfo[1].Trim();

				if (String.Equals(fileServerName, serverName, StringComparison.CurrentCultureIgnoreCase) &&
					String.Equals(fileDatabaseName, databaseName, StringComparison.CurrentCultureIgnoreCase))
				{
					result = true;
				}
			}

			return result;
		}

		readonly string dbFileName;
	}
}
#endif
