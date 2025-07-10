using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CusTempStorageDecCollectionView : BusinessObjectCollectionView<CusTempStorageDec>
	{
		public CusTempStorageDecCollectionView(BusinessObjectCollection collectionToFilter) : base(collectionToFilter)
		{
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var storageDec = element as CusTempStorageDec;
			return !storageDec?.IsDataEmpty ?? false;
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
