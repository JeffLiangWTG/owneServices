namespace CargoWise.EntityFramework.Testing
{
	sealed class DummySubsetCollection : SubsetBusinessObjectCollection<DummyBaseBusinessObject>
	{
		public DummySubsetCollection(BusinessObjectCollection collectionToFilter)
			: base(collectionToFilter)
		{
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return ((DummyBaseBusinessObject)element).Z0_Description == "INVIEW";
		}

		public int RebuildCallCount;
		public int RebuildCallCount_WithIsRebuildingTrue;
		public int OnAddedCallCount;

		protected override void RebuildCore()
		{
			RebuildCallCount++;
			base.RebuildCore();
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			OnAddedCallCount++;
			if (IsRebuilding)
			{
				RebuildCallCount_WithIsRebuildingTrue++;
			}
			base.OnAdded(bizOAdded);
		}
	}
}
