using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbHealth.Check.Testing
{
	[TestedType(typeof(GhostRecordsChecker))]
	sealed class GhostRecordsCheckerTest : CheckerTestCaseBase
	{
		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestGhostRecordsChecker()
		{
			var mock = new Mock<ILogger>();
			CheckerToTest.Check(Db.Connection, new DbHealthWarningList(), mock.Object);
		}

		protected override IChecker GetNewCheckerInstance()
		{
			return new GhostRecordsChecker();
		}
	}
}
