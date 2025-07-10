using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(NewsAnnouncementSectionTypeRegistryItem))]
	sealed class NewsAnnouncementSectionTypeRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<NewsAnnouncementSectionTypeCollection>
	{
		protected override StronglyTypedRegistryItem<NewsAnnouncementSectionTypeCollection, NewsAnnouncementSectionTypeCollection> GetNewRegistryItem()
		{
			return new NewsAnnouncementSectionTypeRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override NewsAnnouncementSectionTypeCollection ValidValue
		{
			get
			{
				var result = new NewsAnnouncementSectionTypeCollection();
				var item = result.AddNew();
				item.Code = NewsSectionSortTypeList.Codes.Alphabetically;
				item.Description = (NoResString)"Some description";
				item.OrderItemsBy = NewsSectionSortTypeList.Codes.PublishedTime;
				return result;
			}
		}

		public void TestDefaultStrings()
		{
			var registryItem = GetNewRegistryItem() as NewsAnnouncementSectionTypeRegistryItem;
			var expected = new NewsSectionTypeList().Cast<CodeDescriptionPair>().Select(s => s.MultilingualDescription);

			AssertContainsExactElementsInAnyOrder("Right DefaultStrings", expected, registryItem.DefaultStrings);
		}

		public void TestGetCaptions()
		{
			var registryItem = GetNewRegistryItem() as NewsAnnouncementSectionTypeRegistryItem;
			var registryValue = GetDefaultSectionTypeCollection();
			var expected = new NewsSectionTypeList().Cast<CodeDescriptionPair>().Select(s => s.MultilingualDescription);

			AssertContainsExactElementsInAnyOrder("Right DefaultStrings", expected, registryItem.GetCaptions(registryValue));
		}

		public void TestConvert()
		{
			var registryItem = new NewsAnnouncementSectionTypeRegistryItemForTest("", null, null, null, RegistryStorageFlags.System);
			var registryValue = GetDefaultSectionTypeCollection(false);
			var convertResults = registryItem.ConvertForTest(registryValue);
			foreach (var item in convertResults)
			{
				Assert("Convert can get ResourceString", item.Description is ResourceString);
			}
		}

		NewsAnnouncementSectionTypeCollection GetDefaultSectionTypeCollection(bool multilingual = true)
		{
			var defaultValue = new NewsAnnouncementSectionTypeCollection();
			foreach (CodeDescriptionPair item in new NewsSectionTypeList())
			{
				defaultValue.Add(new NewsAnnouncementSectionType()
				{
					Code = item.Code,
					Description = multilingual ? item.MultilingualDescription : (NoResString)item.Description,
					OrderItemsBy = NewsSectionSortTypeList.Codes.PublishedTime,
					SystemDefined = true,
				});
			}
			return defaultValue;
		}

		sealed class NewsAnnouncementSectionTypeRegistryItemForTest : NewsAnnouncementSectionTypeRegistryItem
		{
			public NewsAnnouncementSectionTypeRegistryItemForTest(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage) : base(name, category, caption, hint, storage)
			{
			}

			public NewsAnnouncementSectionTypeCollection ConvertForTest(NewsAnnouncementSectionTypeCollection value)
			{
				return Convert(value);
			}
		}
	}
}
