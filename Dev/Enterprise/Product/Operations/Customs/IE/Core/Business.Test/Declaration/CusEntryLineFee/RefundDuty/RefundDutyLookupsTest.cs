using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	public class RefundDutyLookupsTest : TestCaseWithFactory
	{
		public void TestTaxTypes()
		{
			SetupRateCodes();

			var declaration = Factory.New<JobDeclaration>();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var refDuty = entryLine.RefundDuties.AddNew(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat);

			CombineAssertions("When JE_MessageType = IMP", () =>
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				var chargeTypeList = refDuty.Lookups.TaxTypes;
				AssertEquals("Contains A00", true, chargeTypeList.ContainsCode("A00"));
				AssertEquals("Contains B00", true, chargeTypeList.ContainsCode("B00"));
			});
		}

		void SetupRateCodes()
		{
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);

			var rateType1 = refDataHelper.CreateCusRateType(dataGroupingCode: "IE", rateType: "IMP", description: "Export");
			refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "A00", rateType1.PK);
			refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "B00", rateType1.PK);

			Factory.Save();
		}
	}
}
