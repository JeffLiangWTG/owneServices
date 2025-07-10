using CargoWise.Data;
using Enterprise.Integration;

namespace Enterprise.DbHealth.Check.Checkers
{
	internal class DbNameChecker : IChecker
	{
		public string Description => "Check that database name has correct case.";

		const string WarningMessage_Source = "Database name checker";
		const string WarningMessage_Description = "Main database name case passed to process controller is inconsistent with database name on the server.";
		const string WarningMessage_Action = "Please change the name of the main database passed to process controller to have correct case.";

		public void Check(DbConnection connection, DbHealthWarningList warningList, ILogger logger)
		{
			using (((ICurrentDbControl)connection).UseDatabase(Db.DatabaseName))
			{
				if (connection.CurrentDatabase != Db.DatabaseName)
				{
					warningList.Add(new DatabaseWarning(WarningMessage_Source, DatabaseWarning.DbNameWarning, WarningMessage_Description, WarningMessage_Action));
					logger.Warning(WarningMessage_Action);
				}
			}
		}
	}
}
