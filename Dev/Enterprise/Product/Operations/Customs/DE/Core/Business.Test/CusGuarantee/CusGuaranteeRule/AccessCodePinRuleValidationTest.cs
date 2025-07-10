using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class AccessCodePinRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCPR_ValueFrom()
		{
			const string messageError = "Access Codes must have 4 digits.";

			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			var guaranteeRule = guaranteeHeader.AdditionalAccessCodes.AddNew();
			guaranteeRule.CPR_ValueFrom = "123";
			guaranteeRule.Validation.ValidateCPR_ValueFrom();
			CombineAssertions(() =>
			{
				AssertHasMessageError("TRA but empty", guaranteeRule.CPR_ValueFromInfo, messageError);

				guaranteeRule.CPR_ValueFrom = "1234";
				guaranteeRule.Validation.ValidateCPR_ValueFrom();
				AssertNoMessageError("Valid", guaranteeRule.CPR_ValueFromInfo, messageError);

				guaranteeHeader.CPH_Type = "COM";
				guaranteeRule.CPR_ValueFrom = "123";
				guaranteeRule.Validation.ValidateCPR_ValueFrom();
				AssertNoMessageError("COM", guaranteeRule.CPR_ValueFromInfo, messageError);
			});
		}
	}
}
