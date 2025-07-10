using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.BR
{
	public class RenameSystemUtilizationFeeOverrideToUtilizationFeeOverride : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Rename SystemUtilizationFeeOverride To UtilizationFeeOverride";

		protected override void OfflinePostUpgradeTransform()
		{
			RenameSystemUtilizationFeeOverrideToUtilizationFeeOverrideMethod();
		}

		void RenameSystemUtilizationFeeOverrideToUtilizationFeeOverrideMethod()
		{
			var sql = @"UPDATE dbo.CusEntryInstruction
SET
	CEI_Addinfo = REPLACE(CEI_AddInfo, 'SystemUtilizationFeeOverride=', 'UtilizationFeeOverride='),
	CEI_SystemLastEditTimeUtc = GETUTCDATE(),
	CEI_SystemLastEditUser = '~BP'
FROM
	dbo.CusEntryInstruction
WHERE
	CEI_AddInfo LIKE '%SystemUtilizationFeeOverride=%' AND
	CEI_DataModel = 'BR'
";
			Db.Connection.ExecuteNonQuery(sql);
		}

		public TransformationIndexProvider IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(CusEntryInstructionSchema.Instance)
					.Key(CusEntryInstructionSchema.Constants.CEI_DataModel)
					.Include(CusEntryInstructionSchema.Constants.CEI_AddInfo, CusEntryInstructionSchema.Constants.CEI_SystemLastEditTimeUtc, CusEntryInstructionSchema.Constants.CEI_SystemLastEditUser)
					.Where("[CEI_DataModel]='BR'")
					.GetInfo();
				return indexProvider;
			}
		}
	}
}
