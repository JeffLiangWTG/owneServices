using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	public class TaxLookupsCommonTest : TestCaseWithFactory
	{
		public void TestTypeList()
		{
			SetupRefData();
			CheckAndAssertTypeList(new ZString[] { "A40", "A41", "B00" }, MessageTypeList.Codes.Import);
		}

		void CheckAndAssertTypeList(ZString[] expectedValue, ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var fee = entryLine.Fees.AddNew();

			var taxLookupsCommon = new TaxLookupsCommon(fee);
			var typeListOfCodes = taxLookupsCommon.TypeList.GetAllCodes();
			AssertContainsExactElementsInAnyOrder(expectedValue, typeListOfCodes);
		}

		void SetupRefData()
		{
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);

			var rateType1 = refDataHelper.CreateCusRateType(dataGroupingCode: "IE", rateType: "EXP", description: "Export");
			var rateType2 = refDataHelper.CreateCusRateType(dataGroupingCode: "IE", rateType: "MOE", description: "MiscellaneousOnlyForExport");
			var rateType3 = refDataHelper.CreateCusRateType(dataGroupingCode: "IE", rateType: "IMP", description: "IMP");

			refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "A00", rateType1.PK);
			refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "A30", rateType2.PK);
			refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "A40", rateType3.PK);
			refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "A41", rateType3.PK);

			var eunDataGrouping = refDataHelper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
			refDataHelper.CreateNewOrGetExistingDataGrouping(currentCountryCode, "HeNan", parent: eunDataGrouping);
			Factory.Save();
		}
	}
}
