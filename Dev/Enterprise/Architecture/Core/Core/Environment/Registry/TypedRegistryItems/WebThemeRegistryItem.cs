using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class WebThemeRegistryItem : StronglyTypedRegistryItem<WebTrackerTheme[]>
	{
		public WebThemeRegistryItem(string name, StringArrayRegistryItem urlsRegistryItem, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, urlsRegistryItem, category, caption, hint, storage, RegistryOptions.Default)
		{
		}

		public WebThemeRegistryItem(string name, StringArrayRegistryItem urlsRegistryItem, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, urlsRegistryItem, category, caption, hint, storage, options, System.Array.Empty<WebTrackerTheme>())
		{
		}

		public WebThemeRegistryItem(string name, StringArrayRegistryItem urlsRegistryItem, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, WebTrackerTheme[] defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new WebThemeRegistryDataType(), storage, defaultValue))
		{
			UrlsRegistryItem = urlsRegistryItem;
		}

		public WebThemeRegistryItem(string name, StringArrayRegistryItem urlsRegistryItem, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, WebTrackerTheme[] defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new WebThemeRegistryDataType(), storage, options, defaultValue))
		{
			UrlsRegistryItem = urlsRegistryItem;
		}

		public new WebThemeRegistryDataType DataType
		{
			get { return (WebThemeRegistryDataType)base.DataType; }
			set { base.DataType = value; }
		}

		readonly public StringArrayRegistryItem UrlsRegistryItem;
	}
}
