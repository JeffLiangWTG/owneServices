using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	public class PctPaidOrderProviderTest : TestCaseWithFactory
	{
		PctPaidOrderProvider provider;

		public void TestImporterName()
		{
			provider = new PctPaidOrderProvider(new PctPaidOrder { ImporterName = "MR Test ONeill" });
			AssertEquals("MR Test ONeill", provider.ImporterName);
		}
		public void TestImporterName_XlsxField() => typeof(PctPaidOrderProvider).TestXlsxField(nameof(PctPaidOrderProvider.ImporterName), -1, "Importer Name");
	}
}
