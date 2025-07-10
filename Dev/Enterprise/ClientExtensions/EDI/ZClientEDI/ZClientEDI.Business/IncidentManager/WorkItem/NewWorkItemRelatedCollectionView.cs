using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[ModuleID("WorkItem")]
	public class NewWorkItemRelatedCollectionView : BusinessObjectCollectionView<NewWorkItem>
	{
		public NewWorkItemRelatedCollectionView(WorkTaskRelatedItemCollection collectionToFilter)
			: base(collectionToFilter)
		{
			Rebuild();
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var item = (IWorkTaskRelatedItem)element;
			return item.Type == WorkTaskRelatedItemTypes.WorkItem;
		}

		public override System.Type GetTypeOfElementsFromPK(CargoWise.Types.ZGuid pK)
		{
			return typeof(NewWorkItem);
		}
	}
}
