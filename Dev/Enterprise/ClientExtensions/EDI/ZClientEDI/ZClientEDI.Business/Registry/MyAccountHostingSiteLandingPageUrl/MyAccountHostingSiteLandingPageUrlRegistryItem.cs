using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class MyAccountHostingSiteLandingPageUrlRegistryItem : StronglyTypedRegistryItem<MyAccountHostingSiteLandingPageUrlCollection>
	{
		public MyAccountHostingSiteLandingPageUrlRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, MyAccountHostingSiteLandingPageUrlCollection defaultValue)
			: this(name, category, caption, hint, RegistryStorageFlags.Company, defaultValue) { }

		public MyAccountHostingSiteLandingPageUrlRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, MyAccountHostingSiteLandingPageUrlCollection defaultValue)
			: base(new MyAccountHostingSiteLandingPageUrlRegistryItemImpl(name, category, caption, hint, storage, defaultValue)) { }

		class MyAccountHostingSiteLandingPageUrlRegistryItemImpl : RegistryItemImpl
		{
			public MyAccountHostingSiteLandingPageUrlRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, MyAccountHostingSiteLandingPageUrlCollection defaultValue)
				: base(name, category, caption, hint, new MyAccountHostingSiteLandingPageUrlRegistryDataType(), storage, defaultValue) { }
		}
	}

	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.MyAccountHostingSiteLandingPageUrlRegistryItemEditor, ZClientEDI")]
	public class MyAccountHostingSiteLandingPageUrlRegistryDataType : NonPersistentBusinessObjectRegistryDataType<MyAccountHostingSiteLandingPageUrlCollection>
	{ }
}
