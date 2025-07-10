using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.SalesAndMarketing
{
	public class UpdateIncorrectOpportunityProfitFieldsToRevenue : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update profit fields to revenue for opportunities and scopes where the revenue is greater than 0 but less than the profit";

		public TransformationIndexProvider IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(CrmOpportunitySchema.Instance)
					.Key(CrmOpportunitySchema.Constants.COP_EstimatedProfit, CrmOpportunitySchema.Constants.COP_EstimatedRevenue)
					.Where("[COP_EstimatedRevenue]<>(0)")
					.Include(CrmOpportunitySchema.Constants.COP_OH_Organization)
					.Include(CrmOpportunitySchema.Constants.COP_SystemLastEditTimeUtc)
					.Include(CrmOpportunitySchema.Constants.COP_SystemLastEditUser)
					.GetInfo();
				indexProvider.New(CrmOpportunityScopeSchema.Instance)
					.Key(CrmOpportunityScopeSchema.Constants.COS_EstimatedProfit, CrmOpportunityScopeSchema.Constants.COS_EstimatedRevenue)
					.Where("[COS_EstimatedRevenue]<>(0)")
					.Include(CrmOpportunityScopeSchema.Constants.COS_COP_Opportunity)
					.Include(CrmOpportunityScopeSchema.Constants.COS_ScopeID)
					.Include(CrmOpportunityScopeSchema.Constants.COS_SystemLastEditTimeUtc)
					.Include(CrmOpportunityScopeSchema.Constants.COS_SystemLastEditUser)
					.GetInfo();
				return indexProvider;
			}
		}

		protected override void OfflinePostUpgradeTransform()
		{
			const string sql = @"
BEGIN TRY
	UPDATE dbo.CrmOpportunity
	SET COP_EstimatedProfit = COP_EstimatedRevenue,
		COP_SystemLastEditUser = '~BP',
		COP_SystemLastEditTimeUtc = GetUtcDate()
	WHERE COP_EstimatedRevenue <> 0
		AND COP_EstimatedProfit > COP_EstimatedRevenue

	UPDATE dbo.CrmOpportunityScope
	SET COS_EstimatedProfit = COS_EstimatedRevenue,
		COS_SystemLastEditUser = '~BP',
		COS_SystemLastEditTimeUtc = GetUtcDate()
	WHERE COS_EstimatedRevenue <> 0
		AND COS_EstimatedProfit > COS_EstimatedRevenue
END TRY
BEGIN CATCH
	THROW
END CATCH
";

			using (DataTransformationHelper.SuspendTriggerIfExists("TG_CheckNoOpportunityScopeOverlaps", CrmOpportunityScopeSchema.Constants.TableName))
			{
				Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}
