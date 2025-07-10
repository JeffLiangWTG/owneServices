using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class WebCustomImagesRegistryItem : StronglyTypedRegistryItem<WebTrackerCustomImage[]>
	{
		public WebCustomImagesRegistryItem(string name, StringArrayRegistryItem urlsRegistryItem, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, urlsRegistryItem, category, caption, hint, storage, System.Array.Empty<WebTrackerCustomImage>())
		{
		}

		public WebCustomImagesRegistryItem(string name, StringArrayRegistryItem urlsRegistryItem, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, urlsRegistryItem, category, caption, hint, storage, options, System.Array.Empty<WebTrackerCustomImage>())
		{
		}

		public WebCustomImagesRegistryItem(string name, StringArrayRegistryItem urlsRegistryItem, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, WebTrackerCustomImage[] defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new WebCustomImagesRegistryDataType(), storage, defaultValue))
		{
			UrlsRegistryItem = urlsRegistryItem;
		}

		public WebCustomImagesRegistryItem(string name, StringArrayRegistryItem urlsRegistryItem, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, WebTrackerCustomImage[] defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new WebCustomImagesRegistryDataType(), storage, options, defaultValue))
		{
			UrlsRegistryItem = urlsRegistryItem;
		}

		public new WebCustomImagesRegistryDataType DataType
		{
			get { return (WebCustomImagesRegistryDataType)base.DataType; }
			set { base.DataType = value; }
		}

		readonly public StringArrayRegistryItem UrlsRegistryItem;
	}
}
