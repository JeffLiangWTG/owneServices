using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class StringEffectiveDateRegistryItem : StronglyTypedRegistryItem<StringEffectiveDate>
	{
		public StringEffectiveDateRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new StringEffectiveDateRegistryDataType(), storage))
		{
		}

		public StringEffectiveDateRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, StringEffectiveDate defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new StringEffectiveDateRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.StringEffectiveDateRegistryItemEditor, Enterprise.Registry.GUI")]
	public class StringEffectiveDateRegistryDataType : NonPersistentBusinessObjectRegistryDataType<StringEffectiveDate>
	{
	}
}
