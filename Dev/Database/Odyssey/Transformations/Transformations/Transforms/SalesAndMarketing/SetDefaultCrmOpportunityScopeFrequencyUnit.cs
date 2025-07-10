using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.SalesAndMarketing
{
	public class SetDefaultCrmOpportunityScopeFrequencyUnit : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Set default opportunity scope overall Frequency unit to ('Y')";

		#region ITransformationIndexProvider.IndexProvider

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(CrmOpportunityScopeSchema.Instance)
					.Key(CrmOpportunityScopeSchema.Constants.COS_ScopeFrequencyUnit)
					.Where("[COS_ScopeFrequencyUnit]=''")
					.GetInfo();

				return indexProvider;
			}
		}

		#endregion

		protected override void OfflinePostUpgradeTransform()
		{
			var setDefaultFrequencyUnit = @"
IF EXISTS(SELECT NULL FROM sys.triggers where name = 'TG_CheckNoOpportunityScopeOverlaps' AND type = 'TR')
BEGIN
	DISABLE TRIGGER TG_CheckNoOpportunityScopeOverlaps ON dbo.CrmOpportunityScope
END;

UPDATE dbo.CrmOpportunityScope
SET COS_ScopeFrequencyUnit = 'Y',
COS_SystemLastEditUser = '~BP',
COS_SystemLastEditTimeUtc = GetUtcDate()
WHERE COS_ScopeFrequencyUnit = ''

IF EXISTS(SELECT NULL FROM sys.triggers where name = 'TG_CheckNoOpportunityScopeOverlaps' AND type = 'TR')
BEGIN
	ENABLE TRIGGER TG_CheckNoOpportunityScopeOverlaps ON dbo.CrmOpportunityScope
END;
";
			Db.Connection.ExecuteNonQuery(setDefaultFrequencyUnit);
		}
	}
}
