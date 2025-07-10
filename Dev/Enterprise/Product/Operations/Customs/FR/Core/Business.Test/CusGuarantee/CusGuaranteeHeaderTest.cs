using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusGuarantee.Testing
{
	[TestedType(typeof(CusGuaranteeHeader))]
	class CusGuaranteeHeaderTest : EU.Business.Testing.CusGuaranteeHeaderAbstractTest
	{
		public void TestCustomsGuaranteeFriendlyName()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "SPJ";

			var guarantee = CreateGuaranteeHeader("Reference", declarant.PK, "TestMatchAddress");
			guarantee.CPH_Type = GuaranteeTypeList.Codes.COD;
			guarantee.CPH_StartDate = ZDate.Today;

			AssertEquals("CustomsGuaranteeFriendlyName comes from customs reference of guarantee.", "Reference/SPJ", guarantee.CustomsGuaranteeFriendlyName);
		}

		public void TestAddressCodes()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "SPJ";
			var guarantee = Factory.New<CusGuaranteeHeader>();
			guarantee.CPH_OH_PermitHolder = declarant.PK;
			guarantee.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			guarantee.CPH_StartDate = ZDate.Today;
			guarantee.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			guarantee.CPH_Number = "1234";
			AssertEquals("Count of AddressCodes should be 0 because no rule of type ADD has been added to tested guarantee rules.", 0, guarantee.AddressCodes.Length);

			var addressRule1 = guarantee.CusGuaranteeRules.AddNew();
			addressRule1.CPR_RuleCode = PermitRuleCodeList.Codes.ADD;
			addressRule1.CPR_ValueFrom = "AAAA";

			var addressRule2 = guarantee.CusGuaranteeRules.AddNew();
			addressRule2.CPR_RuleCode = PermitRuleCodeList.Codes.ADD;
			addressRule2.CPR_ValueFrom = "BBBB";

			AssertContainsExactElementsInAnyOrder("AddressCodes should contain all tested guarantee rules CPR_ValueFrom properties.", new ZString[] { "AAAA", "BBBB" }, guarantee.AddressCodes);
		}

		public void TestCustomsGuaranteeFriendlyNameForDeltaT()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "SPJ";

			var guarantee = CreateGuaranteeHeader("Reference", declarant.PK, "TestMatchAddress");
			guarantee.CPH_Type = GuaranteeTypeList.Codes.COD;
			guarantee.CPH_StartDate = ZDate.Today;
			var additionalRef = guarantee.AdditionalGuaranteeReferences.AddNew();
			additionalRef.CY_Code = OrgCusAccountDeltaTTypeList.Codes.TR;
			additionalRef.CY_Data = "CODReference";
			AssertEquals("CustomsGuaranteeFriendlyName comes from customs reference of guarantee.", "Reference/CODReference/SPJ", guarantee.CustomsGuaranteeFriendlyNameForDeltaT);
		}

		public void TestSubTypeReadOnly()
		{
			CombineAssertions(() =>
			{
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals("FR guarantee sub type should be read only when its type defined has no sub type", true, guaranteeHeader.CPH_SubType_ReadOnly);
				guaranteeHeader.CPH_Type = GuaranteeTypeList.Codes.DEF;
				AssertEquals("FR guarantee sub type should not be read only when its type defined has sub types", false, guaranteeHeader.CPH_SubType_ReadOnly);
			});
		}

		public void TestLookups()
		{
			AssertType<CusGuaranteeHeaderLookups>(guaranteeHeader.Lookups);
		}

		public void TestIsPermitGuaranteeType()
		{
			CombineAssertions(() =>
			{
				guaranteeHeader.CPH_Type = GuaranteeTypeList.Codes.COD;
				AssertEquals("Is Permit Guarantee Type", true, guaranteeHeader.IsPermitGuaranteeType);

				guaranteeHeader.CPH_Type = GuaranteeTypeList.Codes.ALT;
				AssertEquals("Not Permit Guarantee Type", false, guaranteeHeader.IsPermitGuaranteeType);
			});
		}

		public void TestGuaranteeModeCode()
		{
			AssertEquals(ZString.Empty, guaranteeHeader.GuaranteeModeCode);
			var rule = guaranteeHeader.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = PermitRuleCodeList.Codes.MOD;
			AssertEquals(GuaranteeModeCodeList.Codes.Guarantee, guaranteeHeader.GuaranteeModeCode);
			rule.CPR_ValueFrom = ZString.Empty;
			AssertEquals(ZString.Empty, guaranteeHeader.GuaranteeModeCode);
		}

		public void TestGuaranteeModeDescription()
		{
			AssertEquals(ZString.Empty, guaranteeHeader.GuaranteeModeDescription);
			var rule = guaranteeHeader.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = PermitRuleCodeList.Codes.MOD;
			AssertEquals(GuaranteeModeCodeList.Descriptions.Guarantee, guaranteeHeader.GuaranteeModeDescription);
			rule.CPR_ValueFrom = ZString.Empty;
			AssertEquals(ZString.Empty, guaranteeHeader.GuaranteeModeDescription);
		}

		CusGuaranteeHeader CreateGuaranteeHeader(ZString number, ZGuid orgHeaderPK, ZString addressCode, string ruleCode = EU.Business.PermitRuleCodeList.Codes.ADD, string countryCode = Core.Constants.CountryCodes.France)
		{
			var result = Factory.New<CusGuaranteeHeader>();
			result.CPH_OH_PermitHolder = orgHeaderPK;
			result.CPH_RN_NKCountryCode = countryCode;
			result.CPH_StartDate = ZDate.Today;
			result.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			result.CPH_Number = number;
			if (!string.IsNullOrEmpty(ruleCode))
			{
				var rule1 = result.CusGuaranteeRules.AddNew();
				rule1.CPR_RuleCode = ruleCode;
				rule1.CPR_ValueFrom = addressCode;
			}
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			guaranteeHeader = Factory.New<CusGuaranteeHeader>();
		}
		CusGuaranteeHeader guaranteeHeader;
	}
}
