using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.FR.Business.CusGuarantee.Testing
{
	class CusGuaranteeHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCPH_Number()
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			guarantee.CPH_Type = PermitTransactionTypeList.Codes.TRA;
			guarantee.CPH_SubType = EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver;
			guarantee.CPH_Number = "12345678901234567890123";
			AssertNoMessageErrorContaining(guarantee.CPH_NumberInfo, "Guarantee reference must be ");

			guarantee.CPH_Type = GuaranteeTypeList.Codes.COD;
			guarantee.CPH_SubType = EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver;
			guarantee.Validation.ValidateCPH_Number();
			AssertHasMessageErrorContaining(guarantee.CPH_NumberInfo, "Guarantee reference must be ");

			guarantee.CPH_SubType = "";
			guarantee.Validation.ValidateCPH_Number();
			AssertNoMessageErrorContaining(guarantee.CPH_NumberInfo, "Guarantee reference must be ");

			guarantee.CPH_SubType = EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver;
			var additionalGuaranteeReference = guarantee.AdditionalGuaranteeReferences.AddNew();
			additionalGuaranteeReference.CY_Code = OrgCusAccountDeltaTTypeList.Codes.TR;
			guarantee.Validation.ValidateCPH_Number();
			AssertNoMessageErrorContaining(guarantee.CPH_NumberInfo, "Guarantee reference must be ");
		}

		public void TestCheckCPH_Type()
		{
			var message = "There should be an ENT Rule for guarantees of type COD or DEF.";
			var guarantee = Factory.New<CusGuaranteeHeader>();
			guarantee.CPH_Type = PermitTransactionTypeList.Codes.TRA;
			AssertNoError("No error if CPH_type is not COD or ENT.", guarantee.CPH_TypeInfo, message);

			guarantee.CPH_Type = GuaranteeTypeList.Codes.COD;
			AssertHasError("There should be an error as there is no rule and type is COD.", guarantee.CPH_TypeInfo, message);

			guarantee.CPH_Type = GuaranteeTypeList.Codes.DEF;
			AssertHasError("There should be an error as there is no rule and type is DEF.", guarantee.CPH_TypeInfo, message);

			var rule = guarantee.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = PermitRuleCodeList.Codes.MOD;

			guarantee.CPH_Type = GuaranteeTypeList.Codes.COD;
			AssertHasError("There should be an error as there is no ENT rule and type is COD.", guarantee.CPH_TypeInfo, message);

			guarantee.CPH_Type = GuaranteeTypeList.Codes.DEF;
			AssertHasError("There should be an error as there is no ENT rule and type is DEF.", guarantee.CPH_TypeInfo, message);

			rule.CPR_RuleCode = PermitRuleCodeList.Codes.ENT;
			guarantee.CPH_Type = GuaranteeTypeList.Codes.COD;
			AssertNoError("There should be no error as there is an ENT rule and type is COD.", guarantee.CPH_TypeInfo, message);

			guarantee.CPH_Type = GuaranteeTypeList.Codes.DEF;
			AssertNoError("There should be no error as there is an ENT rule and type is DEF.", guarantee.CPH_TypeInfo, message);
		}
	}
}
