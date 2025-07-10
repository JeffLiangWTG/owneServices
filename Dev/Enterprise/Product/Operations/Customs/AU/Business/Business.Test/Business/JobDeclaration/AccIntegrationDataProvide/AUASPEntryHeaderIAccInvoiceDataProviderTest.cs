using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUASPEntryHeaderIAccInvoiceDataProviderTest : TestCaseWithFactory
	{
		public void TestEntryChargeTypeList()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var provider = new AUASPEntryHeaderIAccInvoiceDataProvider(entryHeader);
			AssertEquals(new AUASPEntryChargeTypeList(), provider.EntryChargeTypeList);
		}

		public void TestUniqueNumber()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.EntryNumber = "EntryNumberTST";
			var provider = new AUASPEntryHeaderIAccInvoiceDataProvider(entryHeader);
			AssertEquals("ASP/EntryNumberTST", provider.UniqueNumber);
		}

		public void TestAPInvoiceNumberAlwaysIncludeChargeCode()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var provider = new AUASPEntryHeaderIAccInvoiceDataProvider(entryHeader);
			Assert(provider.APInvoiceNumberAlwaysIncludeChargeCode);
		}

		public void TestMatchCustomsChargesToClear()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var provider = new AUASPEntryHeaderIAccInvoiceDataProvider(entryHeader);

			CombineAssertions(() =>
			{
				AssertEquals("Empty", true, provider.MatchCustomsChargesToClear("", ""));
				AssertEquals("InvoiceNum", true, provider.MatchCustomsChargesToClear("INV987654321", ""));
				AssertEquals("Description", true, provider.MatchCustomsChargesToClear("", "Some random description"));
				AssertEquals("Both filled", true, provider.MatchCustomsChargesToClear("INV987654321", "Another descriptoin"));
			});
		}
	}
}
