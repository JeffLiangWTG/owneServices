using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class SummarizeULDSLACConfigRegistryItem : StronglyTypedRegistryItem<SummarizeULDSLACConfigCollection>
	{
		public SummarizeULDSLACConfigRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
		: base(new SummarizeULDSLACConfigRegistryItemImpl(name, category, caption, hint, storage, option))
		{
		}
	}

	public class SummarizeULDSLACConfigRegistryItemImpl : RegistryItemImpl
	{
		public SummarizeULDSLACConfigRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
			: base(name, category, caption, hint, new SummarizeULDSLACConfigDataType(), storage, option)
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.SummarizeULDSLACConfigItemEditor, Enterprise.Registry.GUI")]
	public class SummarizeULDSLACConfigDataType : NonPersistentBusinessObjectRegistryDataType<SummarizeULDSLACConfigCollection>
	{
	}
}
