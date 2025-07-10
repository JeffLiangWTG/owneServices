using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Registry
{
	public class CopyInvoiceChargesForExportRegistryItemToImportForFRCompanies : RegistryDataTransformation
	{
		public override string UserDescription => "Copy InvoiceChargesForExport registry to InvoiceChargesForImport for FR companies";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"-- CopyRegistry
INSERT dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_IsLogged, SD_BinaryValue, SD_GuidValue, SD_SystemCreateTimeUtc, SD_SystemCreateUser, SD_SystemLastEditTimeUtc, SD_SystemLastEditUser)
SELECT
	NEWID(), @targetName, SD_Owner, SD_DepartmentGuid, SD_Type, SD_IsLogged, SD_BinaryValue, SD_GuidValue, GETUTCDATE(), 'E', GETUTCDATE(), 'E'
FROM
	dbo.StmData AS source
WHERE 1=1
	AND SD_Name = @sourceName
	AND SD_BinaryValue <> @binaryValue
	AND SD_Owner IN (
		SELECT GC_PK
		FROM dbo.GlbCompany
		WHERE GC_RN_NKCountryCode IN ('FR', 'GF', 'GP', 'MQ', 'YT', 'RE', 'MF', 'BL')
		)
	AND NOT EXISTS
	(
		SELECT NULL
		FROM dbo.StmData AS target
		WHERE 1=1
			AND target.SD_Name = @targetName
			AND
			(1=2
				OR target.SD_Owner is NULL AND source.SD_Owner is NULL
				OR target.SD_Owner = source.SD_Owner
			)
			AND
			(1=2
				OR target.SD_DepartmentGuid is NULL AND source.SD_DepartmentGuid is NULL
				OR target.SD_DepartmentGuid = source.SD_DepartmentGuid
			)
	)
";

			using (var cmd = Db.Connection.Command(sql))
				{
					cmd.AddParameterBasedOnDbColumn("@sourceName", sourceRegistryItemName, StmDataSchema.SD_Name);
					cmd.AddParameterBasedOnDbColumn("@targetName", targetRegistryItemName, StmDataSchema.SD_Name);
					cmd.AddParameterBasedOnDbColumn("@binaryValue", Encoding.Unicode.GetBytes("VAL"), StmDataSchema.SD_BinaryValue);

				cmd.ExecuteNonQuery();
				}
		}

		const string sourceRegistryItemName = "InvoiceChargesForExport";
		const string targetRegistryItemName = "InvoiceChargesForImport";
	}
}
