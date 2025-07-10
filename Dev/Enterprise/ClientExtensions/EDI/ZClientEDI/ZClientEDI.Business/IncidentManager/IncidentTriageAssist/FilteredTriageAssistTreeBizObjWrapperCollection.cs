using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class FilteredTriageAssistTreeBizObjWrapperCollection : BusinessObjectCollectionView<TriageAssistTreeBizObjWrapper>
	{
		public FilteredTriageAssistTreeBizObjWrapperCollection(TriageAssistBusinessObject parent)
			: base(parent.TriageAssistTreeWrapperCollection)
		{
			Rebuild();
		}

		new TriageAssistTreeBizObjWrapperCollection CollectionToFilter => (TriageAssistTreeBizObjWrapperCollection)collectionToFilter;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var item = (TriageAssistTreeBizObjWrapper)element;
			return item.IsPublishedToAssist && (!CollectionToFilter.Parent.ShowFocusedTriageNodesOnly || item.IsFocused);
		}

		protected override void RebuildCore()
		{
			try
			{
				using (SuspendCountChanged(null))
				{
					base.RebuildCore();
				}
			}
			finally
			{
				OnCountChanged(null);
			}
		}
	}
}
