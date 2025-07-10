using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class NewsAnnouncementSectionTypeRegistryItem : TranslatableRegistryItem<NewsAnnouncementSectionTypeCollection, NewsAnnouncementSectionTypeCollection>
	{
		public NewsAnnouncementSectionTypeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new NewsAnnouncementSectionTypeRegistryDataType(), storage, RegistryOptions.Default))
		{
		}

		public NewsAnnouncementSectionTypeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, NewsAnnouncementSectionTypeCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new NewsAnnouncementSectionTypeRegistryDataType(), storage, RegistryOptions.Default, defaultValue))
		{
		}

		public override bool IsTranslatable => true;

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get
			{
				foreach (CodeDescriptionPair pair in new NewsSectionTypeList())
				{
					yield return (ResourceString)pair.MultilingualDescription;
				}
			}
		}

		public override int MaxLength => 256;

		public override IEnumerable<string> GetCaptions(NewsAnnouncementSectionTypeCollection value)
		{
			foreach (var item in value)
			{
				yield return item.Description;
			}
		}

		protected override NewsAnnouncementSectionTypeCollection Convert(NewsAnnouncementSectionTypeCollection value)
		{
			foreach (NewsAnnouncementSectionType item in value)
			{
				item.Description = GetMultilingualString(item.EnglishDescription);
			}

			return value;
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.NewsAnnouncementSectionTypeRegistryItemEditor, Enterprise.Registry.GUI")]
	public class NewsAnnouncementSectionTypeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<NewsAnnouncementSectionTypeCollection>
	{
	}
}
