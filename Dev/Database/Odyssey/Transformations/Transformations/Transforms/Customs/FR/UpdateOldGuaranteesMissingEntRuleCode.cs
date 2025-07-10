using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.FR
{
	class UpdateOldGuaranteesMissingEntRuleCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update old FR COD and DEF guarantees to have ENT rule code with Both value.";

		const string sql = @"
INSERT INTO dbo.CusPermitRule
	(CPR_PK, CPR_CPH_PermitHeader, CPR_RuleCode, CPR_ValueFrom,CPR_SystemCreateTimeUtc, CPR_SystemCreateUser, CPR_SystemLastEditTimeUtc, CPR_SystemLastEditUser)
	SELECT NEWID(), CPH_PK, 'ENT', 'BTH', GetUtcDate(), '~BP', GetUtcDate(), '~BP'
	FROM dbo.CusPermitHeader
	WHERE
			CPH_Type IN ('COD', 'DEF')
			AND CPH_RN_NKCountryCode IN ('FR', 'GF', 'GP', 'MQ', 'YT', 'RE', 'MF', 'BL')
			AND CPH_PK NOT IN (SELECT CPR_CPH_PermitHeader FROM dbo.CusPermitRule WHERE CPR_RuleCode = 'ENT')";

		protected override void OfflinePostUpgradeTransform()
		{
			Db.Connection.ExecuteNonQuery(sql);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(CusPermitHeaderSchema.Instance)
					.Key(CusPermitHeaderSchema.Constants.CPH_RN_NKCountryCode)
					.Include(CusPermitHeaderSchema.PK.Name)
					.Where("([CPH_RN_NKCountryCode] IN ('FR', 'GF', 'GP', 'MQ', 'YT', 'RE', 'MF', 'BL')) AND ([CPH_Type] IN ('COD', 'DEF'))")
					.GetInfo();

				indexProvider.New(CusPermitRuleSchema.Instance)
					.Key(CusPermitRuleSchema.Constants.CPR_CPH_PermitHeader)
					.Where("([CPR_RuleCode]='ENT')")
					.GetInfo();
				return indexProvider;
			}
		}
	}
}
