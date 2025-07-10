using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.SalesAndMarketing
{
	class UpdateGlbCompanyCampaignSendSettingsNegativeValuesToZero : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Set negative values to zero for non-negative GlbCompanyCampaignSendSettings column GSC_ContactLimitPerOrganizationInHorizontal";

		#region ITransformationIndexProvider.IndexProvider

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(GlbCompanyCampaignSendSettingsSchema.Instance)
					.Key(GlbCompanyCampaignSendSettingsSchema.Constants.GSC_ContactLimitPerOrganizationInHorizontal)
					.Where("[GSC_ContactLimitPerOrganizationInHorizontal]<(0)")
					.GetInfo();
				return indexProvider;
			}
		}

		#endregion

		protected override void OfflinePostUpgradeTransform()
		{
			var updateGlbCompanyCampaignSendSettingsNegativeValuesToZero = @"
UPDATE dbo.GlbCompanyCampaignSendSettings
SET GSC_ContactLimitPerOrganizationInHorizontal = 0,
GSC_SystemLastEditUser = '~BP',
GSC_SystemLastEditTimeUtc = GetUtcDate() 
WHERE GSC_ContactLimitPerOrganizationInHorizontal < 0;
";
			Db.Connection.ExecuteNonQuery(updateGlbCompanyCampaignSendSettingsNegativeValuesToZero);
		}
	}
}
