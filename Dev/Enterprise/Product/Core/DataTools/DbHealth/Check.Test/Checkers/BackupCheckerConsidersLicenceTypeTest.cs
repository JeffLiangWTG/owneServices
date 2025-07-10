using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.DbHealth.Check
{
	sealed class BackupCheckerConsidersLicenceTypeTest : TransactionedTestCase
	{
		public void TestCheckConsidersLicenceType()
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			var checker = new BackupCheckerForLicenceTypeTesting();
			using (DbConnection connection = Db.NewAdminConnection(Db.DatabaseName))
			{
				((IChecker)checker).Check(connection, new DbHealthWarningList(), new DummyLogger());
				Assert("Should have checked backups in a production system.", checker.didCheckBackup);
			}

			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Test);
			checker = new BackupCheckerForLicenceTypeTesting();
			using (DbConnection connection = Db.NewAdminConnection(Db.DatabaseName))
			{
				((IChecker)checker).Check(connection, new DbHealthWarningList(), new DummyLogger());
				Assert("Should not have checked backups in a production system.", !checker.didCheckBackup);
			}

			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
		}

		class BackupCheckerForLicenceTypeTesting : BackupChecker
		{
			public bool didCheckBackup;

			protected override void DoCheckBackupIsBeingPerformedPeriodically(IEnumerable<string> dbNames, DbConnection connection, DbHealthWarningList warningList)
			{
				didCheckBackup = true;
			}
		}
	}
}
