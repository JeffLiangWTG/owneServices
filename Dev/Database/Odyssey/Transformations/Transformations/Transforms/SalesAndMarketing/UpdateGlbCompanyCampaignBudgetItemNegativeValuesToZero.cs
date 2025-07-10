using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.SalesAndMarketing
{
	class UpdateGlbCompanyCampaignBudgetItemNegativeValuesToZero : DataTransformation
	{
		public override string UserDescription => "Set negative values to zero for non-negative GlbCompanyCampaignBudgetItem columns";

		protected override void OfflinePostUpgradeTransform()
		{
			var updateGlbCompanyCampaignBudgetItemNegativeValuesToZero = @"
BEGIN TRY
	UPDATE dbo.GlbCompanyCampaignBudgetItem
	SET G9_ExchangeRate = 0,
	G9_SystemLastEditUser = '~BP',
	G9_SystemLastEditTimeUtc = GetUtcDate() 
	WHERE G9_ExchangeRate < 0;
							
	UPDATE dbo.GlbCompanyCampaignBudgetItem
	SET G9_FlatAmount = 0,
	G9_SystemLastEditUser = '~BP',
	G9_SystemLastEditTimeUtc = GetUtcDate() 
	WHERE G9_FlatAmount < 0;

	UPDATE dbo.GlbCompanyCampaignBudgetItem
	SET G9_PerUnitAmount = 0,
	G9_SystemLastEditUser = '~BP',
	G9_SystemLastEditTimeUtc = GetUtcDate() 
	WHERE G9_PerUnitAmount < 0;
END TRY
BEGIN CATCH
	THROW;
END CATCH";
			Db.Connection.ExecuteNonQuery(updateGlbCompanyCampaignBudgetItemNegativeValuesToZero);
		}
	}
}
