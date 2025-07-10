using System;
using CargoWise.Types;

namespace Enterprise.DocumentScanning.Business
{
	public class StorageDocsUnallocatedCollectionView : StorageDocsCollectionViewBase
	{
		public StorageDocsUnallocatedCollectionView(StorageDocsUnallocatedCollection collection)
			: base(collection)
		{
		}

		public new StorageDocsUnallocated this[int index]
		{
			get { return (StorageDocsUnallocated)base[index]; }
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly || CollectionToFilter.ReadOnly; }
		}

		public new StorageDocsUnallocated AddNew()
		{
			return (StorageDocsUnallocated)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk)
		{
			return typeof(StorageDocsUnallocated);
		}
	}
}
