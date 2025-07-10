using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	public class UnpaidOrderProviderTest : TestCaseWithFactory
	{
		UnpaidOrderProvider provider;

		public void TestMrn()
		{
			provider = new UnpaidOrderProvider(new UnpaidOrder { Mrn = "22IEDUB4BBFC22PER2" });
			AssertEquals("22IEDUB4BBFC22PER2", provider.Mrn);
		}
		public void TestMrn_XlsxField() => typeof(UnpaidOrderProvider).TestXlsxField(nameof(UnpaidOrderProvider.Mrn), 1, "MRN");

		public void TestVersion()
		{
			provider = new UnpaidOrderProvider(new UnpaidOrder { Version = 1 });
			AssertEquals(1, provider.Version);
		}
		public void TestVersion_XlsxField() => typeof(UnpaidOrderProvider).TestXlsxField(nameof(UnpaidOrderProvider.Version), 2, "Version");

		public void TestTaxTotal()
		{
			provider = new UnpaidOrderProvider(new UnpaidOrder { TaxTotal = 1000.0M });
			AssertEquals(1000.0M, provider.TaxTotal);
		}
		public void TestTaxTotal_XlsxField() => typeof(UnpaidOrderProvider).TestXlsxField(nameof(UnpaidOrderProvider.TaxTotal), 3, "Tax Total");
	}
}
