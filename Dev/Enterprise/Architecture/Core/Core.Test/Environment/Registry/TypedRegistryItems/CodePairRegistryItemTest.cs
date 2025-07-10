using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(CodePairRegistryItem))]
	public class CodePairRegistryItemTest : StronglyTypedRegistryItemTestCase<string>
	{
		public void TestConstructor()
		{
			var lookUpListProvider = new CodeDescriptionPairListProvider(() =>
			{
				var lookUpList = new CodeDescriptionPairList();
				lookUpList.AddPair("ABC", "XYZ");
				return lookUpList;
			});

			CodePairRegistryItem item = new CodePairRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", lookUpListProvider, RegistryStorageFlags.System, "ABC");
			AssertEquals("Name", "Name", item.Name);
			AssertEquals("Category", "Category", item.Category);
			AssertEquals("Caption", "Caption", item.Caption);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("DefaultValue", "ABC", item.DefaultValue);

			CodePairRegistryDataType dataType = (CodePairRegistryDataType)item.DataType;
			AssertEquals("DataType.LookUpList.Count", 1, dataType.LookUpList.Count);
			AssertEquals("DataType.LookUpList.GetDescriptionFromCode(\"ABC\")", "XYZ", dataType.LookUpList.GetDescriptionFromCode("ABC"));
		}

		public void TestConstructorWithMultipleCategories()
		{
			MultilingualString[] categories = new MultilingualString[] { (NoResString)"One", (NoResString)"Two" };
			CodePairRegistryItem item = new CodePairRegistryItem("Name", categories, (NoResString)"Caption", (NoResString)"Hint", OLookUpEditType.AirDensity, RegistryStorageFlags.System, "ABC");
			AssertEquals(2, item.Categories.Length);
			AssertEquals("One", item.Categories[0]);
			AssertEquals("Two", item.Categories[1]);
		}

		public void TestLookUpListIsLazyLoaded()
		{
			var listIsCreated = false;

			var lookUpListProvider = new CodeDescriptionPairListProvider(() =>
			{
				var lookUpList = new CodeDescriptionPairList();
				lookUpList.AddPair("ABC", "XYZ");
				listIsCreated = true;
				return lookUpList;
			});

			CodePairRegistryItem item = new CodePairRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", lookUpListProvider, RegistryStorageFlags.System, "ABC");
			CodePairRegistryDataType dataType = (CodePairRegistryDataType)item.DataType;
			Assert("LookUpList should not be loaded without it being accessed", !listIsCreated);

			var checkingProperty = dataType.LookUpList.Count;
			Assert("LookUpList should now be loaded after it is accessed by count", listIsCreated);
		}

		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
		{
			return new CodePairRegistryItem("", null, null, null, OLookUpEditType.AccountOrderType, RegistryStorageFlags.System);
		}
	}
}
