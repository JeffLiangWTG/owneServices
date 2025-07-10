using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class WebSecurityMappingRegistryItem : StronglyTypedRegistryItem<WebSecurityMappingCollection, WebSecurityMappingCollection>
	{
		public WebSecurityMappingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, WebSecurityMappingRegistryEditorInfo editorInfo, RegistryStorageFlags storage)
			: this(name, category, caption, hint, editorInfo, storage, new WebSecurityMappingCollection())
		{
		}

		public WebSecurityMappingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, WebSecurityMappingRegistryEditorInfo editorInfo, RegistryStorageFlags storage, WebSecurityMappingCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new WebSecurityMappingRegistryDataType(), editorInfo, storage, RegistryOptions.Default, defaultValue))
		{
		}
	}

	public class WebSecurityMappingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<WebSecurityMappingCollection>
	{
		public WebSecurityMappingRegistryDataType()
		{
		}
	}
}

