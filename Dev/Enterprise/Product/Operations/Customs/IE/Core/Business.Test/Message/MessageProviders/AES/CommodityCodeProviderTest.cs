using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class CommodityCodeProviderTest : DataProviderTestCase<CommodityCodeProvider>
	{
		#region Public test methods

		public void TestCombinedNomenclatureCode()
		{
			entryLine.CL_AdValoremTariff = "1234567890123";
			AssertEquals("CombinedNomenclatureCode", "78", Provider.CombinedNomenclatureCode);
		}

		public void TestHarmonizedCode()
		{
			entryLine.CL_AdValoremTariff = "1234567890123";
			AssertEquals("HarmonizedCode", "123456", Provider.HarmonizedCode);
		}

		public void TestTaricAdditonalCodes()
		{
			AssertEquals("TaricAdditonalCodes", 1, Provider.TaricAdditonalCodes.Count);
			AssertEquals("A", Provider.TaricAdditonalCodes.First());
		}

		public void TestNationalAdditionalCodes()
		{
			AssertEquals("NationalAdditionalCodes", 1, Provider.NationalAdditionalCodes.Count);
			AssertEquals("B", Provider.NationalAdditionalCodes.First());
		}

		#endregion

		#region Overridings & inherits

		protected override CommodityCodeProvider GetProvider() => new CommodityCodeProvider(entryLineWrapper);

		protected override void SetUp()
		{
			base.SetUp();
			(_, entryLineWrapper, invoiceLines) = MessageProviderTestHelper.SetupBasicTestBizObjsMultipleInvoiceLinesSingleEntryLine(Factory);
			entryLine = entryLineWrapper.EntryLine;
			var firstInvoiceLine = invoiceLines[0];
			firstInvoiceLine.JI_SupplementaryCode1 = "A";
			firstInvoiceLine.JI_NationalAdditionalCode1 = "B";
		}

		EntryLineWrapper entryLineWrapper;
		JobComInvoiceLine[] invoiceLines;
		CusEntryLine entryLine;

		#endregion
	}
}
