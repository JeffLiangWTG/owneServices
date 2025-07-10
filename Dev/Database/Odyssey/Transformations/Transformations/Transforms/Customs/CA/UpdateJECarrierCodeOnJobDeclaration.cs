using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA
{
	class UpdateJECarrierCodeOnJobDeclaration : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update JE_CarrierCode On CA JobDeclaration";

		protected override void OfflinePostUpgradeTransform()
		{
			var script = @"
IF EXISTS(SELECT NULL FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'CA')
BEGIN
	UPDATE
		dbo.JobDeclaration
	SET
		JE_CarrierCode = AddInfo.Value,
		JE_AddInfo = AddInfo.AddInfoValue,
		JE_SystemLastEditTimeUtc = GETUTCDATE(),
		JE_SystemLastEditUser = '~BP',
		JE_AutoVersion = (JE_AutoVersion + 1) % 32768
	FROM
		dbo.JobDeclaration
		CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JE_AddInfo, 'CarrierCode') as AddInfo
	WHERE
		JE_AddInfo LIKE '%CarrierCode=%'
		AND JE_DataModel = 'CA'
END
";
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_JobDeclaration_UpdateAutoVersion", JobDeclarationSchema.Constants.TableName))
			{
				Db.Connection.ExecuteNonQuery(script);
			}
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(JobDeclarationSchema.Instance)
					.Key(JobDeclarationSchema.Constants.JE_DataModel)
					.Include(JobDeclarationSchema.Constants.JE_AddInfo, JobDeclarationSchema.Constants.JE_CarrierCode, JobDeclarationSchema.Constants.JE_SystemLastEditTimeUtc, JobDeclarationSchema.Constants.JE_SystemLastEditUser, "JE_AutoVersion")
					.Where("[JE_DataModel]='CA'")
					.GetInfo();
				return indexProvider;
			}
		}
	}
}
