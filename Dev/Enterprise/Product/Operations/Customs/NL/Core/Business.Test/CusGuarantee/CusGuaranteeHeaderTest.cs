using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(CusGuaranteeHeader))]
class CusGuaranteeHeaderTest : EU.Business.Testing.CusGuaranteeHeaderAbstractTest
{
	public void TestGuaranteeCountrySpecificInstruction()
	{
		AssertType<GuaranteeCountrySpecificInstruction>(guaranteeHeader.CountrySpecificInstruction);
	}

	public void TestGetBookedAmountOnGuarantee()
	{
		var org1 = Factory.NewWithValidTestData<OrgHeader>();
		var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
		guaranteeHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		guaranteeHeader.CPH_Number = "GUA1";
		guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
		guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
		guaranteeHeader.CPH_SystemCreateTimeUtc = ZDate.Today;
		guaranteeHeader.CPH_Type = "TRA";
		guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
		guaranteeHeader.CPH_Balance = 1000.0m;
		guaranteeHeader.CPH_OH_PermitHolder = org1.PK;
		var transaction = guaranteeHeader.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, status: PermitTransactionStatusList.Codes.Confirmed, transactionType: PermitTransactionTypeList.Codes.OBL, isAggregated: true);

		AssertEquals("Booked amount returned should be 1000", (ZDecimal)1000, guaranteeHeader.GetBookedAmountOnGuarantee("OPENING"));
	}

	protected override void SetUp()
	{
		base.SetUp();
		guaranteeHeader = Factory.New<CusGuaranteeHeader>();
	}
	CusGuaranteeHeader guaranteeHeader;
}
