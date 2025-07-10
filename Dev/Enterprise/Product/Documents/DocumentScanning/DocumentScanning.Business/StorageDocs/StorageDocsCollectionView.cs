using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentScanning.Business
{
	public class StorageDocsCollectionView : StorageDocsCollectionViewBase
	{
		public StorageDocsCollectionView(BusinessObjectCollection collection)
			: base(collection)
		{
		}

		public StorageDocsCollectionView(StorageMain parent, BusinessObjectCollection collection)
			: base(parent, collection)
		{
		}
#if DEBUG
		public bool IsThisPartOfTheCollectionExposed(BusinessObject element) => IsThisPartOfTheCollection(element);
#endif
		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			StorageDocsBase bizO = element as StorageDocsBase;
			return base.IsThisPartOfTheCollection(element) && bizO.IsImageFile;
		}

		public new StorageDocs this[int index]
		{
			get { return (StorageDocs)Elements[index]; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public new StorageDocs AddNew()
		{
			return (StorageDocs)base.AddNew();
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly || CollectionToFilter.ReadOnly; }
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(StorageDocs);
		}
	}
}
