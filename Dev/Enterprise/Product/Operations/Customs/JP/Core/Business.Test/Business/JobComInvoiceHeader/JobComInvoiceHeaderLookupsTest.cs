using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderLookups))]
	sealed class JobComInvoiceHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceHeaderLookupsTest
	{
		public void TestJZ_IncoTerm_List()
		{
			var incoTermsCodeList = invoiceLookups.JZ_IncoTerm_List;
			CombineAssertions(() =>
			{
				AssertSame("Cached", incoTermsCodeList, invoiceLookups.JZ_IncoTerm_List);

				var expectedCodes = new[] { "C&F", "C&I", "CFR", "CIF", "CIP", "CPT", "DAF", "DAP", "DAT", "DDP", "DDU", "DEQ", "DES", "DPU", "EXW", "FAS", "FC1", "FC2", "FCA", "FOB" };
				AssertArrayEqualsByElements(expectedCodes, incoTermsCodeList.GetAllCodes());
			});
		}

		public void TestValuationTypeCodeList()
		{
			AssertType<ValuationTypeCodeList>(invoiceLookups.ValuationTypeCodeList);
		}

		public void TestInvoiceTypes()
		{
			AssertType<RepresentativeInvoiceTypes>(invoiceLookups.InvoiceTypes);
		}

		public void TestInsuranceTypes()
		{
			var insTypesList = invoiceLookups.InsuranceTypes;
			AssertType<InsuranceTypes>(insTypesList);
			AssertEquals(5, insTypesList.Count);
			Assert(insTypesList.ContainsCode("B"));
			Assert(insTypesList.ContainsCode("D"));
		}

		public void TestFreightRatesTypes()
		{
			var freightRatesList = invoiceLookups.FreightTypes;
			AssertType<FreightRatesTypes>(freightRatesList);
			AssertEquals(21, freightRatesList.Count);
			Assert(freightRatesList.ContainsCode("B"));
			Assert(!freightRatesList.ContainsCode("D"));
			Assert(freightRatesList.ContainsCode("8"));
		}

		public void TestInvoicePriceClassificationList()
		{
			var testList = invoiceLookups.InvoiceAmountType;
			AssertType<InvoicePriceClassificationList>(testList);
			AssertEquals("List should contain 4 codes", 4, testList.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoiceLookups = invoice.Lookups;
		}

		JobComInvoiceHeaderLookups invoiceLookups;
	}
}
