using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class NeoUpgradeLicencesRegistryItem : StronglyTypedRegistryItem<NeoUpgradeLicenceCollection, NeoUpgradeLicenceCollection>
	{
		public NeoUpgradeLicencesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, new NeoUpgradeLicenceCollection())
		{
		}

		public NeoUpgradeLicencesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, NeoUpgradeLicenceCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new NeoUpgradeLicencesRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.NeoUpgradeLicencesRegistryEditor, ZClientEDI")]
	public class NeoUpgradeLicencesRegistryDataType : NonPersistentBusinessObjectRegistryDataType<NeoUpgradeLicenceCollection>
	{
		public NeoUpgradeLicencesRegistryDataType()
		{
		}
	}
}

