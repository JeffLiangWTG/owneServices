using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Registry
{
	public class UpdateEnableInwardProcessingFunctionForFRAndDERegistryDataTransformation : RegistryDataTransformation
	{
		public override string UserDescription => "Override EnableInwardProcessing registry for FR and DE companies";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"-- Insert Registry
INSERT dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_Type, SD_IsLogged, SD_BinaryValue, SD_SystemCreateTimeUtc, SD_SystemCreateUser, SD_SystemLastEditTimeUtc, SD_SystemLastEditUser)
SELECT
	NEWID(), @targetName, GC_PK, @targetType, @isLogged, @binaryValue, GETUTCDATE(), 'E', GETUTCDATE(), 'E'
FROM
	dbo.GlbCompany AS owner
WHERE GC_RN_NKCountryCode IN ('DE', 'FR', 'GF', 'GP', 'MQ', 'YT', 'RE', 'MF', 'BL')
AND NOT EXISTS
	(
		SELECT NULL
		FROM dbo.StmData AS target
		WHERE 1=1
			AND target.SD_Name = @targetName
			AND
			(
				target.SD_Owner is NULL
				OR target.SD_Owner = owner.GC_PK
			))";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@targetName", "EnableInwardProcessing", StmDataSchema.SD_Name);
				cmd.AddParameterBasedOnDbColumn("@targetType", "BOL", StmDataSchema.SD_Type);
				cmd.AddParameterBasedOnDbColumn("@isLogged", 1, StmDataSchema.SD_IsLogged);
				cmd.AddParameterBasedOnDbColumn("@binaryValue", Encoding.Unicode.GetBytes(bool.TrueString), StmDataSchema.SD_BinaryValue);

				cmd.ExecuteNonQuery();
			}
		}
	}
}
