using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.OpportunityManager.Business
{
	public class EDIOrgOpportunityCollectionForIncident : EDIOrgOpportunityCollection
	{
		readonly SupportIncident supportIncident;

		public EDIOrgOpportunityCollectionForIncident(BusinessObjectFactory factory, SupportIncident supportIncident) : base(factory)
		{
			this.supportIncident = supportIncident;
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var existingAdditionalFilter = base.CreateAdditionalFilter();
			if (!existingAdditionalFilter.IsEmpty)
			{
				return existingAdditionalFilter;
			}

			var query = new ZDBOnlyQuery(typeof(OrgOpportunity));
			var subQuery = new ZDBOnlySubQuery(typeof(RelatedActivityPivot), RelatedActivityPivotSchema.RAP_ChildActivityID, notIn: true);
			subQuery.AddToFilter(RelatedActivityPivotSchema.RAP_ParentActivityID, SQLComparisonOperator.NotEqual, supportIncident.PK);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX notifications, BusinessObject selectedBusinessObject)
		{
			var opportunity = (OrgOpportunity)selectedBusinessObject;
			var existingParent = opportunity.RelatedParentActivityPivotCollection.Activities.FirstOrDefault();

			if (existingParent != null)
			{
				var message = $"{System.Environment.NewLine}{existingParent.HumanReadableName} is already the parent of {opportunity.HumanReadableName}. {opportunity.HumanReadableName} can only have one parent.";
				notifications.Add(message);
			}
		}
	}
}
