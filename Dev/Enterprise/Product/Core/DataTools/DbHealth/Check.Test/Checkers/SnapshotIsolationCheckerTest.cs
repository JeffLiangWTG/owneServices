using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbHealth.Check
{
	[TestedType(typeof(SnapshotIsolationChecker))]
	sealed class SnapshotIsolationCheckerTest : CheckerTestCaseBase
	{
		public void TestOnMasterDatabase()
		{
			using (var conn = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, Db.SqlMasterDb))
			{
				var warningList = new DbHealthWarningList();
				new SnapshotIsolationChecker().Check(conn, warningList, null);
				AssertEquals(1, warningList.Count);
				AssertEquals("Snapshot Isolation is not enabled on your database, this may cause performance and locking problems", warningList[0].Description);
			}
		}

		public void TestOnMainDatabase()
		{
			var warningList = new DbHealthWarningList();
			new SnapshotIsolationChecker().Check(Db.Connection, warningList, null);
			AssertEquals(0, warningList.Count);
		}

		protected override IChecker GetNewCheckerInstance()
		{
			return new SnapshotIsolationChecker();
		}
	}
}
