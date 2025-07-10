using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class WebCustomCssRegistryItem : StronglyTypedRegistryItem<WebTrackerCustomCss[]>
	{
		public WebCustomCssRegistryItem(string name, StringArrayRegistryItem urlsRegistryItem, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, urlsRegistryItem, category, caption, hint, storage, System.Array.Empty<WebTrackerCustomCss>())
		{
		}

		public WebCustomCssRegistryItem(string name, StringArrayRegistryItem urlsRegistryItem, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, urlsRegistryItem, category, caption, hint, storage, options, System.Array.Empty<WebTrackerCustomCss>())
		{
		}

		public WebCustomCssRegistryItem(string name, StringArrayRegistryItem urlsRegistryItem, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, WebTrackerCustomCss[] defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new WebCustomCssRegistryDataType(), storage, defaultValue))
		{
			UrlsRegistryItem = urlsRegistryItem;
		}

		public WebCustomCssRegistryItem(string name, StringArrayRegistryItem urlsRegistryItem, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, WebTrackerCustomCss[] defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new WebCustomCssRegistryDataType(), storage, options, defaultValue))
		{
			UrlsRegistryItem = urlsRegistryItem;
		}

		public new WebCustomCssRegistryDataType DataType
		{
			get { return (WebCustomCssRegistryDataType)base.DataType; }
			set { base.DataType = value; }
		}

		readonly public StringArrayRegistryItem UrlsRegistryItem;
	}
}
