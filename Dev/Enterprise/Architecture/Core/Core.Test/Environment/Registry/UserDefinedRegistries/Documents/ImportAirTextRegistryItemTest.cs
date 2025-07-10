using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(ImportAirTextRegistryItem))]
	sealed class ImportAirTextRegistryItemTest : DocumentOpenCloseTextRegistryItemTest
	{
		public override void TestConstructor()
		{
			ResourceString defaultText2 = ResString.GetMultilingualString("b25a8ba5-09b8-436c-b679-c99a1ead2b5a", "Default-Text-2");

			ImportAirTextRegistryItem registryItem1 = new ImportAirTextRegistryItem("Name1", (NoResString)"/Category1", (NoResString)"Caption1", (NoResString)"Hint1", RegistryOptions.Default);
			ImportAirTextRegistryItem registryItem2 = new ImportAirTextRegistryItem("Name2", (NoResString)"/Category2", (NoResString)"Caption2", (NoResString)"Hint2", defaultText2, RegistryOptions.Default);

			AssertRegistryItemProperties(registryItem1, "Name1", "Documents/Forwarding/Shipment/Category1/Import Air", "Caption1", "Hint1", RegistryStorageFlags.All, string.Empty);
			AssertRegistryItemProperties(registryItem2, "Name2", "Documents/Forwarding/Shipment/Category2/Import Air", "Caption2", "Hint2", RegistryStorageFlags.All, defaultText2);
		}

		protected override void AssertAdditionalRegistryItemProperties(DocumentOpenCloseTextRegistryItem registryItem)
		{
			AssertEquals("DepartmentsAllowed", DepartmentFlags.ImportAir, registryItem.DepartmentsAllowed);
		}

		protected override StronglyTypedRegistryItem<MultilingualString, string> GetNewRegistryItem()
		{
			return new ImportAirTextRegistryItem("", null, null, null, RegistryOptions.Default);
		}
	}
}
