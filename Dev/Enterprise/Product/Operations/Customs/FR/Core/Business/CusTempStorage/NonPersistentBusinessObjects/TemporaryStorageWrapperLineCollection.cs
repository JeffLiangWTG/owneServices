using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class TemporaryStorageWrapperLineCollection : NonPersistentBusinessObjectCollection<TemporaryStorageWrapperLine>
	{
		public TemporaryStorageWrapperLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new TemporaryStorageWrapperLine();
	}
}
