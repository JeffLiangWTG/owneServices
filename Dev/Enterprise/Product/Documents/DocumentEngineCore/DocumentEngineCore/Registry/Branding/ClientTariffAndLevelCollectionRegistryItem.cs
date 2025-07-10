using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngineCore.Registry
{
	public class ClientTariffAndLevelCollectionRegistryItem : ClientAndAgentBrandingRegistryItem
	{
		public ClientTariffAndLevelCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(name, category, caption, hint, new ClientTariffAndLevelRegistryDataType(), storage)
		{
		}

		public ClientTariffAndLevelCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(name, category, caption, hint, new ClientTariffAndLevelRegistryDataType(), storage, options)
		{
		}

		#region class ClientTariffAndLevelRegistryDataType

		[RegistryEditor("Enterprise.DocumentEngineCore.GUI.Registry.DocumentBrandingRegistryItemEditor, Enterprise.DocumentEngineCore.GUI")]
		internal class ClientTariffAndLevelRegistryDataType : ClientAndAgentBrandingRegistryDataType<ClientTariffAndLevelCollection>
		{
			public ClientTariffAndLevelRegistryDataType() { }
		}

		#endregion
	}
}
