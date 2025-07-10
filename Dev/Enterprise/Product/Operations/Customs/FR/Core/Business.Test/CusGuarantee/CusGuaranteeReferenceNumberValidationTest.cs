using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.FR.Business.CusGuarantee.Testing
{
	public class CusGuaranteeReferenceNumberValidationTest : CusCodeDataValidationTest
	{
		public void TestCheckCY_Data()
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			guarantee.CPH_Type = GuaranteeTypeList.Codes.COD;
			guarantee.CPH_SubType = EUNctsGuaranteeTypeList.Codes.FlatRateVoucher;

			var cusGuaranteeReferenceNumber = guarantee.AdditionalGuaranteeReferences.AddNew();
			cusGuaranteeReferenceNumber.CY_Code = OrgCusAccountDeltaTTypeList.Codes.TR;
			cusGuaranteeReferenceNumber.CY_Data = "123456789012345678901234";
			cusGuaranteeReferenceNumber.Validation.ValidateCY_Data();
			AssertNoMessageErrorContaining(cusGuaranteeReferenceNumber.CY_DataInfo, "Guarantee reference must be ");

			cusGuaranteeReferenceNumber.CY_Data = "12345678901234567890123";
			AssertHasMessageErrorContaining(cusGuaranteeReferenceNumber.CY_DataInfo, "Guarantee reference must be ");

			guarantee.CPH_SubType = EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver;
			cusGuaranteeReferenceNumber.Validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(cusGuaranteeReferenceNumber.CY_DataInfo, "Guarantee reference must be ");

			cusGuaranteeReferenceNumber.CY_Data = "12345678901234567";
			cusGuaranteeReferenceNumber.Validation.ValidateCY_Data();
			AssertNoMessageErrorContaining(cusGuaranteeReferenceNumber.CY_DataInfo, "Guarantee reference must be ");

			cusGuaranteeReferenceNumber.CY_Data = "1234567890123456";
			AssertHasMessageErrorContaining(cusGuaranteeReferenceNumber.CY_DataInfo, "Guarantee reference must be ");

			guarantee.CPH_SubType = EUNctsGuaranteeTypeList.Codes.CashDepositGuarantee;
			cusGuaranteeReferenceNumber.Validation.ValidateCY_Data();
			AssertNoMessageErrorContaining(cusGuaranteeReferenceNumber.CY_DataInfo, "Guarantee reference must be ");

			guarantee.CPH_SubType = EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver;
			guarantee.CPH_Type = GuaranteeTypeList.Codes.AI2;
			cusGuaranteeReferenceNumber.Validation.ValidateCY_Data();
			AssertNoMessageErrorContaining(cusGuaranteeReferenceNumber.CY_DataInfo, "Guarantee reference must be ");

			guarantee.CPH_Type = GuaranteeTypeList.Codes.COD;
			cusGuaranteeReferenceNumber.CY_Code = OrgCusAccountDeltaGTypeList.Codes.G1;
			cusGuaranteeReferenceNumber.Validation.ValidateCY_Data();
			AssertNoMessageErrorContaining(cusGuaranteeReferenceNumber.CY_DataInfo, "Guarantee reference must be ");
		}
	}
}
