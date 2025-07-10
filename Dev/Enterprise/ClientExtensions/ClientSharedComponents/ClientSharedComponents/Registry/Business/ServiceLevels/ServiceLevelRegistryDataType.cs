using Enterprise.Registry.Business;

namespace Enterprise.ClientSharedComponents.Registry
{
	[RegistryEditor("Enterprise.ClientSharedComponents.Registry.ServiceLevelRegistryItemEditor, Enterprise.ClientSharedComponents.GUI")]
	public class ServiceLevelRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ServiceLevelRegistryBusinessObjectCollection>
	{
		public ServiceLevelRegistryDataType()
		{
		}
	}
}
