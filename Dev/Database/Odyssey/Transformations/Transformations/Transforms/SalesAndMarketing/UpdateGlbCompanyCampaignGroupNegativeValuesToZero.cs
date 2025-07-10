using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.SalesAndMarketing
{
	class UpdateGlbCompanyCampaignGroupNegativeValuesToZero : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Set negative values to zero for non-negative GlbCompanyCampaignGroup column GCG_GroupColor";

		#region ITransformationIndexProvider.IndexProvider

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(GlbCompanyCampaignGroupSchema.Instance)
					.Key(GlbCompanyCampaignGroupSchema.Constants.GCG_GroupColor)
					.Where("[GCG_GroupColor]<(0)")
					.GetInfo();
				return indexProvider;
			}
		}

		#endregion

		protected override void OfflinePostUpgradeTransform()
		{
			var updateGlbCompanyCampaignGroupNegativeValuesToZero = @"
UPDATE dbo.GlbCompanyCampaignGroup
SET GCG_GroupColor = 0,
GCG_SystemLastEditUser = '~BP',
GCG_SystemLastEditTimeUtc = GetUtcDate() 
WHERE GCG_GroupColor < 0;
";
			Db.Connection.ExecuteNonQuery(updateGlbCompanyCampaignGroupNegativeValuesToZero);
		}
	}
}
