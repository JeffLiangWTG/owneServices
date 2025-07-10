using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core
{
	class RebrandCargoWiseAccounts : DataTransformation
	{
#pragma warning disable EDI012 // This is referring to specific products
		public override string UserDescription => "Upgrade CargoWise One accounts to CargoWise Next: CWSupport; CWPostMaster";
#pragma warning restore EDI012

		protected override void OfflinePostUpgradeTransform()
		{
			const string sql = """
			BEGIN TRY

				IF EXISTS (SELECT * FROM dbo.GlbStaff WHERE GS_LoginName = 'CW1Support' AND GS_IsSystemAccount = 1)
				BEGIN
					UPDATE dbo.GlbStaff
					SET
						GS_LoginName = GS_LoginName + '_',
						GS_SystemLastEditUser = '~BP',
						GS_SystemLastEditTimeUtc = GETUTCDATE()
					WHERE
						GS_LoginName LIKE 'CWSupport%'
						AND GS_IsSystemAccount = 0;

					UPDATE dbo.GlbStaff
					SET
						GS_LoginName = 'CWSupport',
						GS_FullName = 'CargoWise Support',
						GS_SystemLastEditUser = '~BP',
						GS_SystemLastEditTimeUtc = GETUTCDATE()
					WHERE
						GS_LoginName = 'CW1Support'
						AND GS_IsSystemAccount = 1;
				END;

				IF EXISTS (SELECT * FROM dbo.GlbStaff WHERE GS_LoginName = 'CW1PostMaster' AND GS_IsSystemAccount = 1)
				BEGIN
					UPDATE dbo.GlbStaff
					SET
						GS_LoginName = GS_LoginName + '_',
						GS_SystemLastEditUser = '~BP',
						GS_SystemLastEditTimeUtc = GETUTCDATE()
					WHERE
						GS_LoginName LIKE 'CWPostMaster%'
						AND GS_IsSystemAccount = 0;

					UPDATE dbo.GlbStaff
					SET
						GS_LoginName = 'CWPostMaster',
						GS_FullName = 'CargoWise Post Master',
						GS_SystemLastEditUser = '~BP',
						GS_SystemLastEditTimeUtc = GETUTCDATE()
					WHERE
						GS_LoginName = 'CW1PostMaster'
						AND GS_IsSystemAccount = 1;
				END;

				-- Transformations must be idempotent, and could be run when merging data from two systems.
				-- If the new registry item already exists, we don't want to overwrite it.
				IF NOT EXISTS (SELECT * FROM dbo.StmData WHERE SD_Name = 'CWSupportLoginTokenCertificate')
				BEGIN
					UPDATE dbo.StmData
					SET
						SD_Name = 'CWSupportLoginTokenCertificate',
						SD_SystemLastEditUser = '~BP',
						SD_SystemLastEditTimeUtc = GETUTCDATE()
					WHERE
						SD_Name = 'CW1SupportLoginTokenCertificate';
				END;

				IF NOT EXISTS (SELECT * FROM dbo.StmData WHERE SD_Name = 'CWSupportLoginTokenPrivateKey')
				BEGIN
					UPDATE dbo.StmData
					SET
						SD_Name = 'CWSupportLoginTokenPrivateKey',
						SD_SystemLastEditUser = '~BP',
						SD_SystemLastEditTimeUtc = GETUTCDATE()
					WHERE
						SD_Name = 'CW1SupportLoginTokenPrivateKey';
				END;

			END TRY
			BEGIN CATCH
				THROW;
			END CATCH;
			""";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
