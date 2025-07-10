using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(StringRegistryItem))]
	sealed class StringRegistryItemTest : StronglyTypedRegistryItemTestCase<string>
	{
		public void TestMultiCategories()
		{
			var item = new StringRegistryItem("HELLO", new[] { (NoResString)"WORLD", (NoResString)"PEOPLE" }, (NoResString)"GREETING", (NoResString)"HINT", new StringRegistryDataType(), null, RegistryStorageFlags.Company, RegistryOptions.Default, "");
			AssertEquals(2, item.Categories.Length);
			AssertEquals("WORLD", item.Categories[0]);
			AssertEquals("PEOPLE", item.Categories[1]);
		}

		public void TestNewStringRegistryItem()
		{
			StringRegistryItem stringRegistryItem = new StringRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", new StringRegistryDataType(CharacterCase.Upper), RegistryStorageFlags.Company, RegistryOptions.CannotCallParameterlessValueGetter);
			AssertEquals("Name", stringRegistryItem.Name);
			AssertEquals("Category", stringRegistryItem.Category);
			AssertEquals("Caption", stringRegistryItem.Caption);
			AssertEquals("Hint", stringRegistryItem.Hint);
			AssertEquals(CharacterCase.Upper, ((StringRegistryDataType)stringRegistryItem.DataType).CharacterCase);
			AssertEquals(RegistryStorageFlags.Company, stringRegistryItem.Storage);
			AssertEquals(RegistryOptions.CannotCallParameterlessValueGetter, stringRegistryItem.Options);
		}

		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
		{
			return new StringRegistryItem("", null, null, null, RegistryStorageFlags.All);
		}
	}
}
