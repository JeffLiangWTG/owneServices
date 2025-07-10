using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngineCore.Registry
{
	public class HybridDocumentBrandCollectionRegistryItem : ClientAndAgentBrandingRegistryItem
	{
		public HybridDocumentBrandCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(name, category, caption, hint, new HybridDocumentBrandRegistryDataType(), storage)
		{
		}

		public HybridDocumentBrandCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(name, category, caption, hint, new HybridDocumentBrandRegistryDataType(), storage, options)
		{
		}
		#region class HybridDocumentBrandRegistryDataType

		[RegistryEditor("Enterprise.DocumentEngineCore.GUI.Registry.ClientAndAgentBrandingRegistryItemEditor, Enterprise.DocumentEngineCore.GUI")]
		internal class HybridDocumentBrandRegistryDataType : ClientAndAgentBrandingRegistryDataType<HybridDocumentBrandCollection>
		{
			public HybridDocumentBrandRegistryDataType() { }
		}

		#endregion
	}
}
