using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

public class NLTaxLookupsCommonTest : TestCaseWithFactory
{
	public void TestTypeList()
	{
		SetupRefData();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		var fee = entryLine.Fees.AddNew();

		var nlTaxLookupsCommon = new NLTaxLookupsCommon(fee);
		AssertContainsExactElementsInAnyOrder(new ZString[] { "A40", "A41", "B00" }, nlTaxLookupsCommon.TypeList.GetAllCodes());
	}

	void SetupRefData()
	{
		var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
	
		var rateType1 = refDataHelper.CreateCusRateType(dataGroupingCode: "EUN", rateType: "EXP", description: "Export");
		var rateType2 = refDataHelper.CreateCusRateType(dataGroupingCode: "EUN", rateType: "MOE", description: "MiscellaneousOnlyForExport");
		var rateType3 = refDataHelper.CreateCusRateType(dataGroupingCode: "EUN", rateType: "IMP", description: "IMP");
		refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "A00", rateType1.PK);
		refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "A30", rateType2.PK);
		refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "A40", rateType3.PK);
		refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "A41", rateType3.PK);
		refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "087", rateType3.PK);

		var eunDataGrouping = refDataHelper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
		refDataHelper.CreateNewOrGetExistingDataGrouping(currentCountryCode, "HeNan", parent: eunDataGrouping);
		Factory.Save();
	}
}

