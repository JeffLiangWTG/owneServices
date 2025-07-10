using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class HVLVEnablePartyScreeningRegistryItem : StronglyTypedRegistryItem<HVLVEnablePartyScreening>
	{
		public HVLVEnablePartyScreeningRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new HVLVEnablePartyScreeningRegistryDataType(), storage, options))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.HVLVEnablePartyScreeningRegistryItemEditor, Enterprise.Registry.GUI")]
	public class HVLVEnablePartyScreeningRegistryDataType : NonPersistentBusinessObjectRegistryDataType<HVLVEnablePartyScreening> { }
}
