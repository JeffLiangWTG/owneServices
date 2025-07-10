using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	public class DctPaidOrderProviderTest : TestCaseWithFactory
	{
		DctPaidOrderProvider provider;

		public void TestImporterName()
		{
			provider = new DctPaidOrderProvider(new DctPaidOrder { ImporterName = "MR Test ONeill" });
			AssertEquals("MR Test ONeill", provider.ImporterName);
		}
		public void TestmporterName_XlsxField() => typeof(DctPaidOrderProvider).TestXlsxField(nameof(DctPaidOrderProvider.ImporterName), -2, "Importer Name");

		public void TestPeriod()
		{
			provider = new DctPaidOrderProvider(new DctPaidOrder { Period = "20220801" });
			AssertEquals(new ZDateTime(2022, 8, 1), provider.Period);
		}
		public void TestPeriod_XlsxField() => typeof(DctPaidOrderProvider).TestXlsxField(nameof(DctPaidOrderProvider.Period), -1, "Period");
	}
}
