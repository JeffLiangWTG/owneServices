using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Registry.Business
{
	public class ShippingAgentRegistryItem : StronglyTypedRegistryItem<ShippingAgentObject>
	{
		public ShippingAgentRegistryItem(ZString name, ZString catergory, ZString caption, ZString hint)
			: base(new RegistryItemImpl(name, (NoResString)catergory, (NoResString)caption, (NoResString)hint, new ShippingAgentRegistryDataType(), RegistryStorageFlags.Company, RegistryOptions.Default, new ShippingAgentObject()))
		{
		}
	}

	[RegistryEditor("Enterprise.Client.UPE.Registry.GUI.ShippingAgentRegistryItemEditor, ZClientUPE")]
	public class ShippingAgentRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ShippingAgentObject>
	{
	}
}
