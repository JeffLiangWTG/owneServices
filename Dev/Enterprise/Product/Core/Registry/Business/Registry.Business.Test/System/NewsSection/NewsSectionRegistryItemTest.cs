using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(NewsSectionRegistryItem))]
	sealed class NewsSectionRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<NewsSectionCollection>
	{
		protected override StronglyTypedRegistryItem<NewsSectionCollection, NewsSectionCollection> GetNewRegistryItem()
		{
			return new NewsSectionRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		public void TestRegistryItemNotCached()
		{
			var item = GetNewRegistryItem();
			AssertEquals(RegistryOptions.NotCached, item.Options);
		}

		protected override NewsSectionCollection ValidValue
		{
			get
			{
				var result = new NewsSectionCollection();
				var item = result.AddNew();
				item.SectionID = NewsSectionTypeList.Codes.ClientStaffNews;
				return result;
			}
		}
	}
}
