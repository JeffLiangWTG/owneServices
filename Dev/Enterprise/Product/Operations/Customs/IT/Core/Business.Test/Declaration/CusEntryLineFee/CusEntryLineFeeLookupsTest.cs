using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusEntryLineFeeLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCommonLookupsHelper()
	{
		var entryLineFee = Factory.New<CusEntryLineFee>();
		var lookupsForTest = new CusEntryLineFeeLookupsForTest(entryLineFee);
		AssertType<TaxLookupsCommon>("Type", lookupsForTest.CommonLookupsHelperExposed);
	}

	public void TestChargeTypeListContainsVatExemptionCodes()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		var entryLine = Factory.New<CusEntryLine>();
		invoiceLine.JI_CL = entryLine.PK;
		CombineAssertions(() =>
		{
			var chargeTypeList = entryLine.Fees.AddNew().Lookups.ChargeTypeList;
			const string vatExemption406 = UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatExemptionCode406;
			const string vatExemption407 = UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatExemptionCode407;
			Assert($"Should contain {vatExemption406}", chargeTypeList.ContainsCode(vatExemption406));
			Assert($"Should contain {vatExemption407}", chargeTypeList.ContainsCode(vatExemption407));
		});
	}

	public void TestRateOverrideReasonList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var entryLineFee = entryLine.Fees.AddNew();

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertEquals("When JE_MessageType=IMP", "ADD, OVR, EXC", entryLineFee.Lookups.RateOverrideReasonList.CodesAsString);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertEquals("When JE_MessageType=EXP", "ADD, OVR, EXC", entryLineFee.Lookups.RateOverrideReasonList.CodesAsString);
		});
	}
}

class CusEntryLineFeeLookupsForTest : CusEntryLineFeeLookups
{
	public CusEntryLineFeeLookupsForTest(EU.Business.Declaration.CusEntryLineFee parent) : base(parent)
	{
	}
	public EU.Business.Declaration.MultiLineAddInfos.TaxLookupsCommon CommonLookupsHelperExposed => CommonLookupsHelper;
}
