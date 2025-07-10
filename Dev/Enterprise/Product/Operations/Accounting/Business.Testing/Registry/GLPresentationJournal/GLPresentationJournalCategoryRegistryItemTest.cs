using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(GLPresentationJournalCategoryRegistryItem))]
	class GLPresentationJournalCategoryRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<GLPresentationJournalCategoryCollection>
	{
		public void TestConstructor()
		{
			var item = new GLPresentationJournalCategoryRegistryItem("a", (NoResString)"b", (NoResString)"c", (NoResString)"d", RegistryStorageFlags.System);
			AssertEquals("Name", "a", item.Name);
			AssertEquals("Category", "b", item.Category);
			AssertEquals("Caption", "c", item.Caption);
			AssertEquals("Hint", "d", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("DefaultValue.Count", 1, item.DefaultValue.Count);

			item = new GLPresentationJournalCategoryRegistryItem("e", (NoResString)"f", (NoResString)"g", (NoResString)"h", RegistryStorageFlags.Company);
			AssertEquals("Name", "e", item.Name);
			AssertEquals("Category", "f", item.Category);
			AssertEquals("Caption", "g", item.Caption);
			AssertEquals("Hint", "h", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, item.Storage);
			AssertEquals("DefaultValue.Count", 1, item.DefaultValue.Count);

			item = new GLPresentationJournalCategoryRegistryItem("i", (NoResString)"j", (NoResString)"k", (NoResString)"l", RegistryStorageFlags.Branch, 3);
			AssertEquals("Name", "i", item.Name);
			AssertEquals("Category", "j", item.Category);
			AssertEquals("Caption", "k", item.Caption);
			AssertEquals("Hint", "l", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Branch, item.Storage);
			AssertEquals("DefaultValue.Count", 1, item.DefaultValue.Count);
		}

		protected override StronglyTypedRegistryItem<GLPresentationJournalCategoryCollection, GLPresentationJournalCategoryCollection> GetNewRegistryItem()
		{
			return new GLPresentationJournalCategoryRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override GLPresentationJournalCategoryCollection ValidValue
		{
			get
			{
				var collection = new GLPresentationJournalCategoryCollection();
				var element = collection.AddNew();
				element.Code = "x";
				element.Description = (NoResString)"Desc";
				element.Bool = true;
				return collection;
			}
		}
	}
}
