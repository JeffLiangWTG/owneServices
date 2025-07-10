using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class InternetAddressListRegistryItem : StronglyTypedRegistryItem<InternetAddressRuleset>
	{
		public InternetAddressListRegistryItem(
			string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			RegistryStorageFlags storage,
			RegistryOptions options,
			InternetAddressRuleset defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new InternetAddressListRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.InternetAddressListRegistryItemEditor, Enterprise.Registry.GUI")]
	public class InternetAddressListRegistryDataType : NonPersistentBusinessObjectRegistryDataType<InternetAddressRuleset>
	{
	}
}
