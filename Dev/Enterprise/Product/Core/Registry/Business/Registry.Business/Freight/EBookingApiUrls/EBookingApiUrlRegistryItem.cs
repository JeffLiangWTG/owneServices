using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class EBookingApiUrlRegistryItem : StronglyTypedRegistryItem<EBookingApiUrls>
	{
		public EBookingApiUrlRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, EBookingApiUrls defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new EBookingApiUrlRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.EBookingApiUrlRegistryItemEditor, Enterprise.Registry.GUI")]
	public class EBookingApiUrlRegistryDataType : NonPersistentBusinessObjectRegistryDataType<EBookingApiUrls>
	{
	}
}
