using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentRelatedItemGenPivotCollection : WorkTaskRelatedItemGenPivotCollection
	{
		public SupportIncidentRelatedItemGenPivotCollection(IWorkTaskRelatedItemSource master, RelatedLinkType relatedLinkType = RelatedLinkType.TwoWay)
			: base(master, relatedLinkType)
		{
			this.relatedLinkType = relatedLinkType;
		}

		readonly RelatedLinkType relatedLinkType;

		protected override IPivotBusinessObjectCollection[] GetNewPivotCollections(BusinessObject master)
		{
			var includeChildren = relatedLinkType != RelatedLinkType.MasterAlwaysChild;
			var includeParents = relatedLinkType != RelatedLinkType.MasterAlwaysParent;

			var orgOpportunityGenPivotCollection = new OrgOpportunityGenPivotCollection(master, includeChildren, includeParents);
			var incidentManagementLinkCollection = new IncidentManagementGroupActivityCollection(master, includeChildren, includeParents);
			var pivotCollections = base.GetNewPivotCollections(master).Append(orgOpportunityGenPivotCollection).Append(incidentManagementLinkCollection);

			return pivotCollections.ToArray();
		}
	}
}
