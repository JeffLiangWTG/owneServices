using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentGroupAutoCascadeWorkItemCollection : WorkTaskRelatedItemGenPivotCollection<NewWorkItem>
	{
		public IncidentGroupAutoCascadeWorkItemCollection(IWorkTaskRelatedItemSource master, RelatedLinkType relatedLinkType = RelatedLinkType.TwoWay) : base(master, relatedLinkType)
		{
			this.relatedLinkType = relatedLinkType;
		}

		readonly RelatedLinkType relatedLinkType;

		protected override IPivotBusinessObjectCollection[] GetNewPivotCollections(BusinessObject master)
		{
			var includeChildren = relatedLinkType != RelatedLinkType.MasterAlwaysChild;
			var includeParents = relatedLinkType != RelatedLinkType.MasterAlwaysParent;

			return new IPivotBusinessObjectCollection[] { new WorkItemCascadeGenPivotCollection(master, includeChildren, includeParents) };
		}
	}
}
