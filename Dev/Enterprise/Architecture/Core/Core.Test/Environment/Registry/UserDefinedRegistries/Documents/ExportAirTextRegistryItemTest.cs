using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(ExportAirTextRegistryItem))]
	sealed class ExportAirTextRegistryItemTest : DocumentOpenCloseTextRegistryItemTest
	{
		public override void TestConstructor()
		{
			ResourceString defaultText2 = ResString.GetMultilingualString("03ede990-dee1-4147-b9d5-58272847c2f5", "Default-Text-2");

			ExportAirTextRegistryItem registryItem1 = new ExportAirTextRegistryItem("Name1", (NoResString)"/Category1", (NoResString)"Caption1", (NoResString)"Hint1", RegistryOptions.Default);
			ExportAirTextRegistryItem registryItem2 = new ExportAirTextRegistryItem("Name2", (NoResString)"/Category2", (NoResString)"Caption2", (NoResString)"Hint2", defaultText2, RegistryOptions.Default);

			AssertRegistryItemProperties(registryItem1, "Name1", "Documents/Forwarding/Consol/Category1/Export Air", "Caption1", "Hint1", RegistryStorageFlags.All, string.Empty);
			AssertRegistryItemProperties(registryItem2, "Name2", "Documents/Forwarding/Consol/Category2/Export Air", "Caption2", "Hint2", RegistryStorageFlags.All, defaultText2);
		}

		protected override void AssertAdditionalRegistryItemProperties(DocumentOpenCloseTextRegistryItem registryItem)
		{
			AssertEquals("DepartmentsAllowed", DepartmentFlags.ExportAir, registryItem.DepartmentsAllowed);
		}

		protected override StronglyTypedRegistryItem<MultilingualString, string> GetNewRegistryItem()
		{
			return new ExportAirTextRegistryItem("", null, null, null, RegistryOptions.Default);
		}
	}
}
