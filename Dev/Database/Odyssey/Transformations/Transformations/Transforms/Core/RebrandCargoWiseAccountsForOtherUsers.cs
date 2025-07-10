using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core
{
	class RebrandCargoWiseAccountsForOtherUsers : DataTransformation
	{
#pragma warning disable EDI012 // This is referring to specific products
		public override string UserDescription => "Upgrade CargoWise One accounts to CargoWise Next: CWWeb; CWService";
#pragma warning restore EDI012

		protected override void OfflinePostUpgradeTransform()
		{
			const string sql = """
			BEGIN TRY

				IF EXISTS (SELECT * FROM dbo.GlbStaff WHERE GS_Code = 'ZZ' AND GS_LoginName = 'CW1Web')
				BEGIN
					UPDATE dbo.GlbStaff
					SET
						GS_LoginName = 'CWWeb',
						GS_FullName = 'CargoWise Web',
						GS_SystemLastEditUser = '~BP',
						GS_SystemLastEditTimeUtc = GETUTCDATE()
					WHERE
						GS_Code = 'ZZ';
				END;

				IF EXISTS (SELECT * FROM dbo.GlbStaff WHERE GS_Code = '~BP' AND GS_LoginName = 'CW1Service')
				BEGIN
					UPDATE dbo.GlbStaff
					SET
						GS_LoginName = 'CWService',
						GS_FullName = 'CargoWise Service',
						GS_SystemLastEditUser = '~BP',
						GS_SystemLastEditTimeUtc = GETUTCDATE()
					WHERE
						GS_Code = '~BP';
				END;

				IF EXISTS (SELECT * FROM dbo.GlbStaff WHERE GS_Code = '~AD' AND GS_LoginName = 'CW1AutoDataImport')
				BEGIN
					UPDATE dbo.GlbStaff
					SET
						GS_LoginName = 'CWAutoDataImport',
						GS_SystemLastEditUser = '~BP',
						GS_SystemLastEditTimeUtc = GETUTCDATE()
					WHERE
						GS_Code = '~AD';
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
