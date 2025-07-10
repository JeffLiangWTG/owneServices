using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(ExportSeaTextRegistryItem))]
	sealed class ExportSeaTextRegistryItemTest : DocumentOpenCloseTextRegistryItemTest
	{
		public override void TestConstructor()
		{
			var defaultText2 = ResString.GetMultilingualString("6769c2ab-1ac7-42c3-a788-3cafcd262dc1", "Default-Text-2");

			ExportSeaTextRegistryItem registryItem1 = new ExportSeaTextRegistryItem("Name1", (NoResString)"/Category1", (NoResString)"Caption1", (NoResString)"Hint1", RegistryOptions.Default);
			ExportSeaTextRegistryItem registryItem2 = new ExportSeaTextRegistryItem("Name2", (NoResString)"/Category2", (NoResString)"Caption2", (NoResString)"Hint2", defaultText2, RegistryOptions.Default);

			AssertRegistryItemProperties(registryItem1, "Name1", "Documents/Forwarding/Consol/Category1/Export Sea", "Caption1", "Hint1", RegistryStorageFlags.All, string.Empty);
			AssertRegistryItemProperties(registryItem2, "Name2", "Documents/Forwarding/Consol/Category2/Export Sea", "Caption2", "Hint2", RegistryStorageFlags.All, defaultText2);
		}

		protected override void AssertAdditionalRegistryItemProperties(DocumentOpenCloseTextRegistryItem registryItem)
		{
			AssertEquals("DepartmentsAllowed", DepartmentFlags.ExportSea, registryItem.DepartmentsAllowed);
		}

		protected override StronglyTypedRegistryItem<MultilingualString, string> GetNewRegistryItem()
		{
			return new ExportSeaTextRegistryItem("", null, null, null, RegistryOptions.Default);
		}
	}
}
