using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(LicencedStringRegistryItem))]
	sealed class LicensedStringRegistryItemTest : StronglyTypedRegistryItemTestCase<string>
	{
		public void TestConstuctor()
		{
			var stringRegistryItem = new LicencedStringRegistryItem(() => true, "Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.Company);
			AssertEquals(stringRegistryItem.IsLicensed, true);
			AssertEquals("Name", stringRegistryItem.Name);
			AssertEquals("Category", stringRegistryItem.Category);
			AssertEquals("Caption", stringRegistryItem.Caption);
			AssertEquals("Hint", stringRegistryItem.Hint);
			AssertEquals(RegistryStorageFlags.Company, stringRegistryItem.Storage);

			stringRegistryItem = new LicencedStringRegistryItem(() => { return false; }, "Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.Company);
			AssertEquals(stringRegistryItem.IsLicensed, false);
		}

		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
		{
			return new LicencedStringRegistryItem(() => true, "", null, null, null, RegistryStorageFlags.All);
		}
	}
}
