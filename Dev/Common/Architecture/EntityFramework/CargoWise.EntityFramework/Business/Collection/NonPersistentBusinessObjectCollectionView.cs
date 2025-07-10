using System;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	public abstract class NonPersistentBusinessObjectCollectionView<T> : BusinessObjectCollectionView<T>, INonPersistentBusinessObjectCollectionView where T : NonPersistentBusinessObject
	{
		protected NonPersistentBusinessObjectCollectionView(BusinessObjectCollection collectionToFilter)
			: base(collectionToFilter)
		{
		}

		protected NonPersistentBusinessObjectCollectionView()
			: this(null)
		{
		}

		public override void Load(ZQuery filter)
		{
			ErrorReporter.ReportOnce("NonPersistentNoLoad" + GetType().FullName, "Cannot Load() on a NonPersistentBusinessObjectCollection");
		}

		internal override BusinessObject CreateNewBusinessObject()
		{
			BusinessObject newElement = CreateNonPersistentBusinessObject();
			HookupElementToCollection(newElement);

			return newElement;
		}

		protected abstract T CreateNonPersistentBusinessObject();

		protected override BusinessObject AddNewCore()
		{
			return CollectionToFilter.AddNew();
		}

		protected override BusinessObject AddNewCore(Type bizoType)
		{
			return CollectionToFilter.AddNew();
		}
	}
}
