using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(ImportSeaTextRegistryItem))]
	sealed class ImportSeaTextRegistryItemTest : DocumentOpenCloseTextRegistryItemTest
	{
		public override void TestConstructor()
		{
			ResourceString defaultText2 = ResString.GetMultilingualString("7a095898-ba74-4311-ba6c-c87d85e5ad06", "Default-Text-2");

			ImportSeaTextRegistryItem registryItem1 = new ImportSeaTextRegistryItem("Name1", (NoResString)"/Category1", (NoResString)"Caption1", (NoResString)"Hint1", RegistryOptions.Default);
			ImportSeaTextRegistryItem registryItem2 = new ImportSeaTextRegistryItem("Name2", (NoResString)"/Category2", (NoResString)"Caption2", (NoResString)"Hint2", defaultText2, RegistryOptions.Default);

			AssertRegistryItemProperties(registryItem1, "Name1", "Documents/Forwarding/Shipment/Category1/Import Sea", "Caption1", "Hint1", RegistryStorageFlags.All, string.Empty);
			AssertRegistryItemProperties(registryItem2, "Name2", "Documents/Forwarding/Shipment/Category2/Import Sea", "Caption2", "Hint2", RegistryStorageFlags.All, defaultText2);
		}

		protected override void AssertAdditionalRegistryItemProperties(DocumentOpenCloseTextRegistryItem registryItem)
		{
			AssertEquals("DepartmentsAllowed", DepartmentFlags.ImportSea, registryItem.DepartmentsAllowed);
		}

		protected override StronglyTypedRegistryItem<MultilingualString, string> GetNewRegistryItem()
		{
			return new ImportSeaTextRegistryItem("", null, null, null, RegistryOptions.Default);
		}
	}
}
