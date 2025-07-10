using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(CodePairWithAdditionalEventRegistryItem))]
	sealed class CodePairWithAdditionalEventRegistryItemTest : StronglyTypedRegistryItemTestCase<string>
	{
		public void TestConstructor()
		{
			var lookUpListProvider = new CodeDescriptionPairListProvider(() =>
			{
				var lookUpList = new CodeDescriptionPairList();
				lookUpList.AddPair("ABC", "XYZ");
				return lookUpList;
			});
			var item = new CodePairWithAdditionalEventRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", lookUpListProvider, false, false, new ComboBoxRegistryEditorInfo(lookUpListProvider), null, RegistryStorageFlags.System, RegistryOptions.Default, "ABC", false);
			AssertEquals("Name", "Name", item.Name);
			AssertEquals("Category", "Category", item.Category);
			AssertEquals("Caption", "Caption", item.Caption);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("DefaultValue", "ABC", item.DefaultValue);

			var dataType = (CodePairRegistryDataType)item.DataType;
			AssertEquals("DataType.LookUpList.Count", 1, dataType.LookUpList.Count);
			AssertEquals("DataType.LookUpList.GetDescriptionFromCode(\"ABC\")", "XYZ", dataType.LookUpList.GetDescriptionFromCode("ABC"));
		}

		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
		{
			var lookUpListProvider = new CodeDescriptionPairListProvider(() => new CodeDescriptionPairList());
			return new CodePairWithAdditionalEventRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", lookUpListProvider, false, false, new ComboBoxRegistryEditorInfo(lookUpListProvider), null, RegistryStorageFlags.System, RegistryOptions.Default, "", false);
		}
	}
}
