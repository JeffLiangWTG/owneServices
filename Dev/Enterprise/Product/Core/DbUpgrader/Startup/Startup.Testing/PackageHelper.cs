using System;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	class PackageHelper
	{
		internal static bool InsertUpgradePackage(Guid pk, Version version, string status)
		{
			var insertSql = $@"
						INSERT INTO dbo.StmUpgrade (
							SZ_PK, SZ_Type, SZ_ExeVersionDate, SZ_MajorVersion, SZ_MinorVersion, SZ_Release, SZ_Patch, 
							SZ_MajorDataVersion, SZ_MinorDataVersion, SZ_UpgradeNotes, SZ_Reference,
							SZ_Status, SZ_StatusTime, SZ_StatusComment,
							SZ_SystemCreateTimeUtc, SZ_SystemCreateUser, SZ_SystemLastEditTimeUtc, SZ_SystemLastEditUser)
							VALUES(
							'{pk}', 'EDP', CURRENT_TIMESTAMP, {version.Major}, {version.Minor}, {version.Build},  {version.Revision},
							0, 0, '', '',
							'{status}', CURRENT_TIMESTAMP, 'AutoDeployTestedShelf',
							GetUtcDate(), '~BP', GetUtcDate(), '~BP')
						";
			return Db.Connection.ExecuteNonQuery(insertSql) > 0;
		}
	}
}
