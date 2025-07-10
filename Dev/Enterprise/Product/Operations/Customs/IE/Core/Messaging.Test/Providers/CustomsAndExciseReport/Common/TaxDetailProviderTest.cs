using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	public class TaxDetailProviderTest : TestCaseWithFactory
	{
		TaxDetailProvider provider;

		public void TestValues()
		{
			provider = new TaxDetailProvider(new TaxDetail() { Mrn = "22IEDUB4BBFC22PER2", Version = 1, _1D3 = 0, _1A1 = 0, _1B2 = 0, _A00 = 5, _1B3 = 0, _1D5 = 0, _A45 = 0, _B00 = 5, _1D6 = 0, _A35 = 0, _B00EX = 0, _1S1 = 0, _1E1 = 0, _A40 = 0, _A30 = 0, _1C1 = 0, _2E2 = 0, _A20 = 0 });
			CombineAssertions(() =>
			{
				AssertEquals("22IEDUB4BBFC22PER2", provider.Mrn);
				AssertEquals(1, provider.Version);
				AssertEquals(0, provider._1D3);
				AssertEquals(0, provider._1A1);
				AssertEquals(0, provider._1B2);
				AssertEquals(5, provider._A00);
				AssertEquals(0, provider._1B3);
				AssertEquals(0, provider._1D5);
				AssertEquals(0, provider._A45);
				AssertEquals(5, provider._B00);
				AssertEquals(0, provider._1D6);
				AssertEquals(0, provider._A35);
				AssertEquals(0, provider._B00EX);
				AssertEquals(0, provider._1S1);
				AssertEquals(0, provider._1E1);
				AssertEquals(0, provider._A40);
				AssertEquals(0, provider._A30);
				AssertEquals(0, provider._1C1);
				AssertEquals(0, provider._2E2);
				AssertEquals(0, provider._A20);
			});
		}

		public void TestXlsxFields()
		{
			typeof(TaxDetailProvider).TestXlsxField(nameof(TaxDetailProvider.Mrn), 1, "MRN");
			typeof(TaxDetailProvider).TestXlsxField(nameof(TaxDetailProvider.Version), 2, "Version");
			typeof(TaxDetailProvider).TestXlsxField(nameof(TaxDetailProvider._1D3), 3, "1D3");
			typeof(TaxDetailProvider).TestXlsxField(nameof(TaxDetailProvider._1A1), 4, "1A1");
			typeof(TaxDetailProvider).TestXlsxField(nameof(TaxDetailProvider._1B2), 5, "1B2");
			typeof(TaxDetailProvider).TestXlsxField(nameof(TaxDetailProvider._A00), 6, "A00");
			typeof(TaxDetailProvider).TestXlsxField(nameof(TaxDetailProvider._1B3), 7, "1B3");
			typeof(TaxDetailProvider).TestXlsxField(nameof(TaxDetailProvider._1D5), 8, "1D5");
			typeof(TaxDetailProvider).TestXlsxField(nameof(TaxDetailProvider._A45), 9, "A45");
			typeof(TaxDetailProvider).TestXlsxField(nameof(TaxDetailProvider._B00), 10, "B00");
			typeof(TaxDetailProvider).TestXlsxField(nameof(TaxDetailProvider._1D6), 11, "1D6");
			typeof(TaxDetailProvider).TestXlsxField(nameof(TaxDetailProvider._A35), 12, "A35");
			typeof(TaxDetailProvider).TestXlsxField(nameof(TaxDetailProvider._B00EX), 13, "B00EX");
			typeof(TaxDetailProvider).TestXlsxField(nameof(TaxDetailProvider._1S1), 14, "1S1");
			typeof(TaxDetailProvider).TestXlsxField(nameof(TaxDetailProvider._1E1), 15, "1E1");
			typeof(TaxDetailProvider).TestXlsxField(nameof(TaxDetailProvider._A40), 16, "A40");
			typeof(TaxDetailProvider).TestXlsxField(nameof(TaxDetailProvider._A30), 17, "A30");
			typeof(TaxDetailProvider).TestXlsxField(nameof(TaxDetailProvider._1C1), 18, "1C1");
			typeof(TaxDetailProvider).TestXlsxField(nameof(TaxDetailProvider._2E2), 19, "2E2");
			typeof(TaxDetailProvider).TestXlsxField(nameof(TaxDetailProvider._A20), 20, "A20");
		}
	}
}
