using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUDSBEntryHeaderIAccInvoiceDataProviderTest : TestCaseWithFactory
	{
		public void TestEntryChargeTypeList()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var provider = new AUDSBEntryHeaderIAccInvoiceDataProvider(entryHeader, true);
			AssertEquals((new CusEntryChargeTypeList()).Count - 1, provider.EntryChargeTypeList.Count);
			AssertNotContains(CusEntryChargeTypeList.Codes.AQISServicePaymentAmount, provider.EntryChargeTypeList.CodesAsString);

			provider = new AUDSBEntryHeaderIAccInvoiceDataProvider(entryHeader, false);
			AssertEquals((new CusEntryChargeTypeList()).Count, provider.EntryChargeTypeList.Count);
			AssertContains(CusEntryChargeTypeList.Codes.AQISServicePaymentAmount, provider.EntryChargeTypeList.CodesAsString);
		}

		public void TestUniqueNumber()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.EntryNumber = "EntryNumberTST";
			var provider = new AUDSBEntryHeaderIAccInvoiceDataProvider(entryHeader, true);
			AssertEquals("EntryNumberTST", provider.UniqueNumber);
		}
	}
}
