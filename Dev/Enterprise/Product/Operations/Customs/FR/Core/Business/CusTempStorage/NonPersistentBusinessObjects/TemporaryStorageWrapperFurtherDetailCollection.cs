using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class TemporaryStorageWrapperFurtherDetailCollection : NonPersistentBusinessObjectCollection<TemporaryStorageWrapperFurtherDetail>
	{
		public TemporaryStorageWrapperFurtherDetailCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new TemporaryStorageWrapperFurtherDetail();
	}
}
