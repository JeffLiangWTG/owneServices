using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.SalesAndMarketing
{
	public class UpdateCampaignItemRecipientTableCodeValueOPP : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update Campaign Item Recipient Table Code Value OPP to a contact table code";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
UPDATE dbo.GlbCompanyCampaignItem
SET G8_RecipientTableCode = CASE
		WHEN OC_PK IS NOT NULL THEN 'OC'
		WHEN O1_PK IS NOT NULL THEN 'O1'
		WHEN GS_PK IS NOT NULL THEN 'GS'
		WHEN HA_PK IS NOT NULL THEN 'HA' END,
	G8_SystemLastEditTimeUtc = SYSUTCDATETIME(),
	G8_SystemLastEditUser = '~BP'
	FROM dbo.GlbCompanyCampaignItem
	LEFT JOIN dbo.OrgContact ON OC_PK = G8_RecipientID
	LEFT JOIN dbo.OrgColdCallRegister ON O1_PK = G8_RecipientID
	LEFT JOIN dbo.GlbStaff ON GS_PK = G8_RecipientID 
	LEFT JOIN dbo.HRJobApplicant ON HA_PK = G8_RecipientID
WHERE G8_RecipientTableCode = 'OPP' AND (OC_PK IS NOT NULL OR O1_PK IS NOT NULL OR GS_PK IS NOT NULL OR HA_PK IS NOT NULL)";

			Db.Connection.ExecuteNonQuery(sql);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(GlbCompanyCampaignItemSchema.Instance)
					.Key(GlbCompanyCampaignItemSchema.Constants.G8_RecipientTableCode)
					.Where($"[{GlbCompanyCampaignItemSchema.Constants.G8_RecipientTableCode}]='OPP'")
					.Include(GlbCompanyCampaignItemSchema.Constants.PK,
						GlbCompanyCampaignItemSchema.Constants.G8_RecipientID,
						GlbCompanyCampaignItemSchema.Constants.G8_SystemLastEditTimeUtc,
						GlbCompanyCampaignItemSchema.Constants.G8_SystemLastEditUser)
					.GetInfo();
				return indexProvider;
			}
		}
	}
}
