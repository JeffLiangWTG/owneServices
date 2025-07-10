using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class TaxRegimeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTaxRegimeList()
		{
			var taxRegimeCode = "1";
			ReferenceTestDataHelper.CreateReferenceDataForIPITaxRegimeList(Factory);
			ReferenceTestDataHelper.CreateReferenceDataForTaxRegimeList(Factory, BRJobMessageTypeList.Codes.ImportSiscomex);
			ReferenceTestDataHelper.CreateReferenceDataForICMSTaxRegimeList(Factory);
			ReferenceTestDataHelper.CreateReferenceDataForPISLegalBaseList(Factory, taxRegimeCode);
			ReferenceTestDataHelper.CreateReferenceDataForDutyLegalBaseList(Factory, taxRegimeCode);

			var ipiTaxRegimeList = invoiceLine.IPITaxRegimeSupportingInfo.Lookups.TaxRegimeList;
			CombineAssertions(() =>
			{
				AssertEquals(5, ipiTaxRegimeList.Count);
				AssertEquals("1, 2, 3, 4, 5", ipiTaxRegimeList.CodesAsString);
			});

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			var pisTaxRegimeList = invoiceLine.PisCofinsTaxRegimeSupportingInfo.Lookups.TaxRegimeList;

			CombineAssertions(() =>
			{
				AssertEquals(2, pisTaxRegimeList.Count);
				AssertEquals("1, 2", pisTaxRegimeList.CodesAsString);
			});

			var dutyTaxRegimeList = invoiceLine.DutyTaxRegimeSupportingInfo.Lookups.TaxRegimeList;
			CombineAssertions(() =>
			{
				AssertEquals(3, dutyTaxRegimeList.Count);
				AssertEquals("1, 2, 3", dutyTaxRegimeList.CodesAsString);
			});

			var icmsTaxRegimeList = invoiceLine.ICMSTaxRegimeSupportingInfo.Lookups.TaxRegimeList;
			CombineAssertions(() =>
			{
				AssertEquals(9, icmsTaxRegimeList.Count);
				AssertEquals("1, 2, 3, 4, 5, 6, 7, 8, 9", icmsTaxRegimeList.CodesAsString);
			});

			var fmmTaxRegimeList = invoiceLine.FMMTaxRegimeSupportingInfo.Lookups.TaxRegimeList;
			CombineAssertions(() =>
			{
				AssertEquals(1, fmmTaxRegimeList.Count);
				AssertEquals("E", fmmTaxRegimeList.CodesAsString);
			});
		}

		public void TestLegalBaseList()
		{
			var taxRegimeCode = "1";
			ReferenceTestDataHelper.CreateReferenceDataForPISLegalBaseList(Factory, taxRegimeCode);
			ReferenceTestDataHelper.CreateReferenceDataForDutyLegalBaseList(Factory, taxRegimeCode);

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			invoiceLine.PisCofinsTaxRegime = taxRegimeCode;
			invoiceLine.DutyTaxRegime = taxRegimeCode;

			var legalBaseCodesList = invoiceLine.PisCofinsTaxRegimeSupportingInfo.Lookups.LegalBaseList;
			CombineAssertions(() =>
			{
				AssertEquals(2, legalBaseCodesList.Count);
				AssertEquals("01, 02", legalBaseCodesList.CodesAsString);
			});

			legalBaseCodesList = invoiceLine.DutyTaxRegimeSupportingInfo.Lookups.LegalBaseList;
			CombineAssertions(() =>
			{
				AssertEquals(1, legalBaseCodesList.Count);
				AssertEquals("01", legalBaseCodesList.CodesAsString);
			});
		}

		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		}
	}
}
