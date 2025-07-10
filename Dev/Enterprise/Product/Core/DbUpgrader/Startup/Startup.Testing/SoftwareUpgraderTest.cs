using System;
using System.Data;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.Upgrades;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class SoftwareUpgraderTest : TransactionedTestCase
	{
		public void TestRemoveOldUpgradePackages()
		{
			var dELPackage = AddUpgradePackage(new Version(1, 0, 0, 1), "DEL", new DateTime(2010, 1, 1));
			var nAPPackage = AddUpgradePackage(new Version(1, 0, 0, 2), "NAP", new DateTime(2010, 1, 1));
			var oBSPackage = AddUpgradePackage(new Version(1, 0, 0, 3), "OBS", new DateTime(2010, 1, 1));
			var aPLPackage1 = AddUpgradePackage(new Version(1, 0, 0, 4), "APL", new DateTime(2010, 1, 2));
			var aPLPackage2 = AddUpgradePackage(new Version(1, 0, 0, 5), "APL", new DateTime(2010, 1, 3));
			var aPLPackage3 = AddUpgradePackage(new Version(1, 0, 0, 6), "APL", new DateTime(2010, 1, 4));
			var aPLPackage4 = AddUpgradePackage(new Version(1, 0, 0, 7), "APL", new DateTime(2010, 1, 5));
			var cURPackage = AddUpgradePackage(new Version(1, 0, 1, 0), "CUR", new DateTime(2010, 1, 6));
			var rDYPackage = AddUpgradePackage(new Version(1, 0, 1, 1), "RDY", new DateTime(2010, 1, 6));

			SoftwareUpgrader.Instance.RemoveOldUpgradePackages();

			AssertEquals(false, UpgradePackageExists(dELPackage));
			AssertEquals(false, UpgradePackageExists(nAPPackage));
			AssertEquals(false, UpgradePackageExists(oBSPackage));
			AssertEquals(false, UpgradePackageExists(aPLPackage1));
			AssertEquals(false, UpgradePackageExists(aPLPackage2));
			AssertEquals(true, UpgradePackageExists(aPLPackage3));
			AssertEquals(true, UpgradePackageExists(aPLPackage4));
			AssertEquals(true, UpgradePackageExists(cURPackage));
			AssertEquals(true, UpgradePackageExists(rDYPackage));
		}

		[ExpectNoExceptions]
		public void TestCommitSoftwareUpgradeInDatabaseWithNoUser()
		{
			AssertNull(Env.LoginController.UpgradeLogonStaffCode);
			var version = new Version(1, 0, 1, 1);
			var package = AddUpgradePackage(version, "RDY", new DateTime(2010, 1, 6));
			var upgrade = new UpgradeInfo(package, version);
			SoftwareUpgrader.Instance.CommitSoftwareUpgradeInDatabase(upgrade, null);
		}

		Guid AddUpgradePackage(Version version, string status, DateTime statusTime)
		{
			var pk = Guid.NewGuid();
			using (var cmd = Db.Connection.Command("insert into dbo.StmUpgrade (SZ_PK, SZ_ExeVersionDate, SZ_MajorVersion, SZ_MinorVersion, SZ_Release, SZ_Patch, SZ_Status, SZ_StatusTime, SZ_SystemCreateTimeUtc, SZ_SystemCreateUser, SZ_SystemLastEditTimeUtc, SZ_SystemLastEditUser) values (@Pk, GetDate(), @MajorVersion, @MinorVersion, @Release, @Patch, @Status, @StatusTime, GetUtcDate(), '~BP', GetUtcDate(), '~BP')"))
			{
				cmd.AddParameter("Pk", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("MajorVersion", SqlDbType.Int, version.Major);
				cmd.AddParameter("MinorVersion", SqlDbType.Int, version.Minor);
				cmd.AddParameter("Release", SqlDbType.Int, version.Revision);
				cmd.AddParameter("Patch", SqlDbType.Int, version.Build);
				cmd.AddParameter("Status", SqlDbType.VarChar, status);
				cmd.AddParameter("StatusTime", SqlDbType.DateTime, statusTime);
				cmd.ExecuteNonQuery();
			}
			return pk;
		}

		bool UpgradePackageExists(Guid pk)
		{
			using (var cmd = Db.Connection.Command("select count(*) from dbo.StmUpgrade where SZ_PK = @Pk"))
			{
				cmd.AddParameter("Pk", SqlDbType.UniqueIdentifier, pk);
				return (int)cmd.ExecuteScalar() > 0;
			}
		}
	}
}
