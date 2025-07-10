using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Data;

namespace CargoWise.Bi.Maintenance
{
	public class FileGrowthScriptRunner
	{
		public FileGrowthScriptRunner(DbConnection connection)
		{
			this.connection = connection;
		}
		readonly DbConnection connection;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test Cases")]
		const string FileGrowthMasterStateParameter = "LAST_FILE_AUTOGROWTH_CHECK_UTC_DT";
		const string FileGrowth = "128MB";

		public void Run(string databaseName)
		{
			var dbFileCollection = new DataBaseFileGrowthCollection(connection, databaseName);
			if (dbFileCollection.DataFiles.Any() || dbFileCollection.LogFiles.Any())
			{
				string fileGrowthScript = GenerateFileGrowthScript(databaseName, dbFileCollection);
				using (((ICurrentDbControl)connection).UseDatabase(databaseName))
				{
					connection.ExecuteNonQuery(fileGrowthScript);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		string GenerateFileGrowthScript(string databaseName, DataBaseFileGrowthCollection dataBaseFileGrowthCollection)
		{
			var script = new StringBuilder();
			string alterQuery = "ALTER DATABASE [{0}] MODIFY FILE ( NAME = N'{1}', FILEGROWTH = {2} );";

			foreach (string dbFile in dataBaseFileGrowthCollection.DataFiles.Concat(dataBaseFileGrowthCollection.LogFiles))
			{
				script.AppendFormat(CultureInfo.InvariantCulture,
					alterQuery,
					/*0*/databaseName,
					/*1*/dbFile,
					/*2*/FileGrowth
				);
			}

			return script.ToString();
		}
	}
}
