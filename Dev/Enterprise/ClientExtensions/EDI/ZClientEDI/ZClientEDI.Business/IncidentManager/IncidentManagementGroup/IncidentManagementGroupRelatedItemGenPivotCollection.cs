using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentManagementGroupRelatedItemGenPivotCollection : WorkTaskRelatedItemGenPivotCollection
	{
		public IncidentManagementGroupRelatedItemGenPivotCollection(IWorkTaskRelatedItemSource master, RelatedLinkType relatedLinkType = RelatedLinkType.TwoWay)
			: base(master, relatedLinkType)
		{
		}

		protected override IPivotBusinessObjectCollection[] GetNewPivotCollections(BusinessObject master)
		{
			var workItemCascadeGenPivotCollection = new WorkItemCascadeGenPivotCollection(master);
			var pivotCollections = base.GetNewPivotCollections(master).Append(workItemCascadeGenPivotCollection);

			return pivotCollections.ToArray();
		}
	}
}
