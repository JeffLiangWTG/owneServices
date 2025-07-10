using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.SalesAndMarketing
{
	class UpdateGlbCompanyCampaignNegativeValuesToZero : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Set negative values to zero for non-negative GlbCompanyCampaign columns";

		#region ITransformationIndexProvider.IndexProvider

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(GlbCompanyCampaignSchema.Instance)
					.Key(GlbCompanyCampaignSchema.Constants.G0_BatchCountDefault)
					.Where("[G0_BatchCountDefault]<(0)")
					.GetInfo();

				indexProvider.New(GlbCompanyCampaignSchema.Instance)
					.Key(GlbCompanyCampaignSchema.Constants.G0_LastSentBatchNumber)
					.Where("[G0_LastSentBatchNumber]<(0)")
					.GetInfo();

				indexProvider.New(GlbCompanyCampaignSchema.Instance)
					.Key(GlbCompanyCampaignSchema.Constants.G0_QuestionsPerWebPage)
					.Where("[G0_QuestionsPerWebPage]<(0)")
					.GetInfo();

				return indexProvider;
			}
		}

		#endregion

		protected override void OfflinePostUpgradeTransform()
		{
			var updateGlbCompanyCampaignNegativeValuesToZero = @"
UPDATE dbo.GlbCompanyCampaign
SET G0_BatchCountDefault = 0,
G0_SystemLastEditUser = '~BP',
G0_SystemLastEditTimeUtc = GetUtcDate() 
WHERE G0_BatchCountDefault < 0;
							
UPDATE dbo.GlbCompanyCampaign
SET G0_LastSentBatchNumber = 0,
G0_SystemLastEditUser = '~BP',
G0_SystemLastEditTimeUtc = GetUtcDate() 
WHERE G0_LastSentBatchNumber < 0;

UPDATE dbo.GlbCompanyCampaign
SET G0_QuestionsPerWebPage = 0,
G0_SystemLastEditUser = '~BP',
G0_SystemLastEditTimeUtc = GetUtcDate() 
WHERE G0_QuestionsPerWebPage < 0;
";
			Db.Connection.ExecuteNonQuery(updateGlbCompanyCampaignNegativeValuesToZero);
		}
	}
}
