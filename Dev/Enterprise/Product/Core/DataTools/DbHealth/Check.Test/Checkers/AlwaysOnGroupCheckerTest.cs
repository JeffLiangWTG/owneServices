using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DbHealth.Check.Testing
{
	[TestedType(typeof(AlwaysOnGroupChecker))]
	sealed class AlwaysOnGroupCheckerTest : CheckerTestCaseBase
	{
		public void TestCheckForDatabasesNotIncludedInSecondaryReplica_NoException()
		{
			var checker = new AlwaysOnGroupChecker.AlwaysOnSecondaryServerChecker(Guid.NewGuid(), new string[] { Db.DatabaseName, Db.DatabaseName });
			AssertNoExceptionThrown(() => { checker.CheckForDatabasesNotIncludedInSecondaryReplica(Db.ServerName); });
		}

		[UseSnapshotProtection]
		public void TestCheck()
		{
			IChecker alwaysOnGroupChecker = new AlwaysOnGroupChecker();
			AssertEquals("Description", "Check all operational databases included in the SQL AlwaysOn Availability Group", alwaysOnGroupChecker.Description);
			alwaysOnGroupChecker.Check(Db.Connection, null, new TestServiceLogger());
		}

		protected override IChecker GetNewCheckerInstance()
		{
			return new AlwaysOnGroupChecker();
		}
	}
}
