using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow
{
	[TestedType(typeof(TG_UPD_PkgPackage_MarksAndNumbersVersion))]
	class TG_UPD_PkgPackage_MarksAndNumbersVersionTest : DbCreateScriptTest
	{
		public void TestMarksAndNumbersVersion()
		{
			var packageJobPK = Guid.NewGuid();
			var pkgPackagePK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery($"INSERT INTO dbo.PkgPackageJob (KJ_PK, KJ_ParentID, KJ_ParentTableCode, KJ_SystemCreateTimeUtc, KJ_SystemCreateUser, KJ_SystemLastEditTimeUtc, KJ_SystemLastEditUser) VALUES ('{packageJobPK}', NewID(), 'Z0', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.PkgPackageHeader (KPH_PK, KPH_PackageID, KPH_SystemCreateTimeUtc, KPH_SystemCreateUser, KPH_SystemLastEditTimeUtc, KPH_SystemLastEditUser) VALUES('6862745D-A87E-45B6-AED5-7E9F5746659E', 'Parent', GetUtcDate(), 'S1', GetUtcDate(), '~BP')");

			TestConnection.ExecuteNonQuery($"INSERT INTO dbo.PkgPackage (KP_PK, KP_KPH_PackageHeader, KP_KJ_ParentPackageJob, KP_Sequence, KP_PackageQty, KP_F3_NKPackType, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser) VALUES('{pkgPackagePK}', '6862745D-A87E-45B6-AED5-7E9F5746659E', '{packageJobPK}', 1, 1 , 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			AssertEquals((short)0, TestConnection.ExecuteScalar<short>($"SELECT KP_MarksAndNumbersVersion FROM dbo.PkgPackage WHERE KP_PK='{pkgPackagePK}'"));

			TestConnection.ExecuteNonQuery($"UPDATE dbo.PkgPackage SET KP_Sequence=2, KP_SystemLastEditTimeUtc =  GETUTCDATE(), KP_SystemLastEditUser ='BP~' WHERE KP_PK='{pkgPackagePK}'");
			AssertEquals((short)0, TestConnection.ExecuteScalar<short>($"SELECT KP_MarksAndNumbersVersion FROM dbo.PkgPackage WHERE KP_PK='{pkgPackagePK}'"));

			TestConnection.ExecuteNonQuery($"UPDATE dbo.PkgPackage SET KP_MarksAndNumbers='test', KP_SystemLastEditTimeUtc =  GETUTCDATE(), KP_SystemLastEditUser ='BP~' WHERE KP_PK='{pkgPackagePK}'");
			AssertEquals((short)1, TestConnection.ExecuteScalar<short>($"SELECT KP_MarksAndNumbersVersion FROM dbo.PkgPackage WHERE KP_PK='{pkgPackagePK}'"));
		}
	}
}

