using System;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DummyViewCollection : BusinessObjectCollectionView<DummyBaseBusinessObject>
	{
		public DummyViewCollection(BusinessObjectCollection collectionToFilter)
			: base(collectionToFilter)
		{
		}

		protected override void RebuildCore()
		{
			base.RebuildCore();
			OnRebuild?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler OnRebuild;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return ((DummyBaseBusinessObject)element).Z0_Description == "INVIEW";
		}
	}
}
