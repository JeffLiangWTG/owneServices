using System;
using System.Data;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business.Testing
{
	sealed class StmUpgradeToPreserveTest : TransactionedTestCase
	{
		public void TestEmptyDbs()
		{
			PopulateStmUpgrade();
			RunRestore();
			AssertStmUpgradeCount(0);
		}

		public void TestEmptyOrgDb()
		{
			PopulateStmUpgrade();
			RunRestore(
				new StmUpgrade(new Version(1, 2, 3, 4), "CUR", AnyDate),
				new StmUpgrade(new Version(1, 2, 3, 5), "RDY", AnyDate)
			);
			AssertStmUpgradeCount(2);
			AssertCurrentVersion(new Version(1, 2, 3, 4));
		}

		public void TestEmptyRestoreDb()
		{
			PopulateStmUpgrade(
				new StmUpgrade(new Version(1, 2, 3, 4), "CUR", AnyDate),
				new StmUpgrade(new Version(1, 2, 3, 5), "RDY", DateTime.UtcNow),
				new StmUpgrade(new Version(1, 2, 3, 0), "RDY", DateTime.UtcNow.AddYears(-1))
			);
			RunRestore();
			AssertStmUpgradeCount(2);
			AssertCurrentVersion(new Version(1, 2, 3, 4));
		}

		public void TestFullMerge()
		{
			PopulateStmUpgrade(
				new StmUpgrade(new Version(1, 2, 3, 4), "CUR", AnyDate),
				new StmUpgrade(new Version(1, 2, 3, 5), "RDY", AnyDate),
				new StmUpgrade(new Version(1, 0, 0, 0), "APL", AnyDate)
			);
			RunRestore(
				new StmUpgrade(new Version(1, 2, 3, 5), "RDY", AnyDate),
				new StmUpgrade(new Version(1, 0, 0, 1), "RDY", AnyDate),
				new StmUpgrade(new Version(1, 0, 0, 0), "CUR", AnyDate)
			);
			AssertStmUpgradeCount(4);
			AssertCurrentVersion(new Version(1, 0, 0, 0));
		}

		public void TestNoRestoreCurrent()
		{
			PopulateStmUpgrade(
				new StmUpgrade(new Version(1, 2, 3, 4), "CUR", AnyDate),
				new StmUpgrade(new Version(1, 2, 3, 5), "RDY", DateTime.UtcNow)
			);
			RunRestore(
				new StmUpgrade(new Version(1, 2, 3, 4), "RDY", AnyDate),
				new StmUpgrade(new Version(1, 1, 1, 1), "RDY", AnyDate)
			);
			AssertStmUpgradeCount(3);
			AssertCurrentVersion(new Version(1, 2, 3, 4));
		}

		#region Implementation

		void PopulateStmUpgrade(params StmUpgrade[] upgrades)
		{
			Db.Connection.ExecuteNonQuery("truncate table StmUpgrade");

			foreach (StmUpgrade upgrade in upgrades)
			{
				using (DbCommand cmd = Db.Connection.Command(
					@"insert into dbo.StmUpgrade (SZ_PK, SZ_MajorVersion, SZ_MinorVersion, SZ_Release, SZ_Patch, SZ_Status, SZ_ExeVersionDate, SZ_SystemCreateTimeUtc, SZ_SystemCreateUser, SZ_SystemLastEditTimeUtc, SZ_SystemLastEditUser) 
					  values (NEWID(), @MajorVersion, @MinorVersion, @Release, @Patch, @Status, @ExeDate, GetUtcDate(), '~BP', GetUtcDate(), '~BP')"))
				{
					cmd.AddParameter("MajorVersion", SqlDbType.Int, upgrade.Version.Major);
					cmd.AddParameter("MinorVersion", SqlDbType.Int, upgrade.Version.Minor);
					cmd.AddParameter("Release", SqlDbType.Int, upgrade.Version.Build);
					cmd.AddParameter("Patch", SqlDbType.Int, upgrade.Version.Revision);
					cmd.AddParameter("Status", SqlDbType.VarChar, upgrade.Status);
					cmd.AddParameter("ExeDate", SqlDbType.DateTime, upgrade.ExeDate);
					cmd.ExecuteNonQuery();
				}
			}
		}

		void RunRestore(params StmUpgrade[] upgrades)
		{
			StmUpgradeToPreserve preserver = new StmUpgradeToPreserve();
			Db.Connection.ExecuteNonQuery(preserver.GetPopulateTemporaryDataScript(Db.DatabaseName, Db.DatabaseName));
			PopulateStmUpgrade(upgrades);
			Db.Connection.ExecuteNonQuery(preserver.GetClearDataToBeOverwrittenByTestDataScript(Db.DatabaseName, Db.DatabaseName));
			Db.Connection.ExecuteNonQuery(preserver.GetCopyTempDbDataToTestDbScript(Db.DatabaseName, Db.DatabaseName));
		}

		void AssertStmUpgradeCount(int expectedValue)
		{
			AssertEquals(expectedValue, (int)Db.Connection.ExecuteScalar("select count(*) from dbo.StmUpgrade"));
		}

		void AssertCurrentVersion(Version expectedVersion)
		{
			AssertEquals(expectedVersion, new Version((string)Db.Connection.ExecuteScalar("select " + StmUpgradeToPreserve.VersionFunction + " from dbo.StmUpgrade where SZ_Status = 'CUR'")));
		}

		struct StmUpgrade
		{
			public StmUpgrade(Version version, string status, DateTime exeDate)
			{
				this.Version = version;
				this.Status = status;
				this.ExeDate = exeDate;
			}

			public readonly Version Version;
			public readonly string Status;
			public readonly DateTime ExeDate;
		}

		readonly DateTime AnyDate = DateTime.UtcNow.AddMonths(new Random().Next(-100, 100));

		#endregion
	}
}
