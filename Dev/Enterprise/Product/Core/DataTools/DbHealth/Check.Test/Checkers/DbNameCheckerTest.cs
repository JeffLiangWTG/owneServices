using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbHealth.Check.Checkers;
using Enterprise.Integration;
using Moq;

namespace Enterprise.DbHealth.Check.Test.Checkers
{
	class DbNameCheckerTest : CheckerTestCaseBase
	{
		public void TestDbNameCaseIsConsistentWarning()
		{
			var dbName = Db.DatabaseName;
			var serverName = Db.ServerName;
			using (new DisposableAction(() =>
			{
				Db.ClearServerDetails();
				Db.InitializeDatabaseDetails(serverName, dbName);
			}))
			{
				var loggerMock = new Mock<ILogger>();
				Db.ClearServerDetails();
				Db.InitializeDatabaseDetails(serverName, dbName.ToLower());
				AssertNotEquals(Db.DatabaseName, dbName);
				var warningList = new DbHealthWarningList();
				this.CheckerToTest.Check(Db.Connection, warningList, loggerMock.Object);

				AssertEquals(1, warningList.Count);
				AssertEquals(warningList[0].WarningType, DatabaseWarning.DbNameWarning);
				loggerMock.Verify(l => l.Log(LogType.Warning, "Please change the name of the main database passed to process controller to have correct case."));
			}
		}

		protected override IChecker GetNewCheckerInstance()
		{
			return new DbNameChecker();
		}
	}
}
