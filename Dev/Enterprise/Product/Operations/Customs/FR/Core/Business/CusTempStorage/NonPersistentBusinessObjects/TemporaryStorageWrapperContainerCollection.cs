using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class TemporaryStorageWrapperContainerCollection : NonPersistentBusinessObjectCollection<TemporaryStorageWrapperContainer>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject() => new TemporaryStorageWrapperContainer();
	}
}
