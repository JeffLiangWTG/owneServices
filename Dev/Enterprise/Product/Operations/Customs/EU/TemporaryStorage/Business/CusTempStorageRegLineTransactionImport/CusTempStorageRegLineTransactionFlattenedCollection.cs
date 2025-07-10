using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeaderImport
{
	public class CusTempStorageRegLineTransactionFlattenedCollection : NonPersistentBusinessObjectCollection<CusTempStorageRegLineTransactionFlattened>
	{
		public CusTempStorageRegLineTransactionFlattenedCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new CusTempStorageRegLineTransactionFlattened();
	}
}
