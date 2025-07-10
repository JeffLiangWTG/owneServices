using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.FR.Business.CusAuthorisation.Testing
{
	class CusAuthorisationRuleRequirementHelperTest : BusinessObjectValidationTestCase
	{
		public void TestGetRequirement()
		{
			var authorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			CombineAssertions("Rule requirements", () =>
			{
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, CusAuthorisationRuleTypeList.Codes.AUT, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, CusAuthorisationRuleTypeList.Codes.CLE, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, CusAuthorisationRuleTypeList.Codes.CNT, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, CusAuthorisationRuleTypeList.Codes.CON, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, CusAuthorisationRuleTypeList.Codes.INF, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, CusAuthorisationRuleTypeList.Codes.NAT, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, CusAuthorisationRuleTypeList.Codes.OFC, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, CusAuthorisationRuleTypeList.Codes.PCD, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, CusAuthorisationRuleTypeList.Codes.PCP, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, CusAuthorisationRuleTypeList.Codes.PCV, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, CusAuthorisationRuleTypeList.Codes.STO, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, CusAuthorisationRuleTypeList.Codes.SUB, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, CusAuthorisationRuleTypeList.Codes.TRA, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, CusAuthorisationRuleTypeList.Codes.USE, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, CusAuthorisationRuleTypeList.Codes.WAR, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, CusAuthorisationRuleTypeList.Codes.AUT, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, CusAuthorisationRuleTypeList.Codes.CLE, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, CusAuthorisationRuleTypeList.Codes.CNT, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, CusAuthorisationRuleTypeList.Codes.CON, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, CusAuthorisationRuleTypeList.Codes.INF, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, CusAuthorisationRuleTypeList.Codes.NAT, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, CusAuthorisationRuleTypeList.Codes.OFC, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, CusAuthorisationRuleTypeList.Codes.PCD, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, CusAuthorisationRuleTypeList.Codes.PCP, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, CusAuthorisationRuleTypeList.Codes.PCV, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, CusAuthorisationRuleTypeList.Codes.STO, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, CusAuthorisationRuleTypeList.Codes.SUB, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, CusAuthorisationRuleTypeList.Codes.TRA, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, CusAuthorisationRuleTypeList.Codes.USE, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, CusAuthorisationRuleTypeList.Codes.WAR, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission, CusAuthorisationRuleTypeList.Codes.AUT, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission, CusAuthorisationRuleTypeList.Codes.CLE, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission, CusAuthorisationRuleTypeList.Codes.CNT, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission, CusAuthorisationRuleTypeList.Codes.CON, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission, CusAuthorisationRuleTypeList.Codes.INF, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission, CusAuthorisationRuleTypeList.Codes.NAT, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission, CusAuthorisationRuleTypeList.Codes.OFC, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission, CusAuthorisationRuleTypeList.Codes.PCD, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission, CusAuthorisationRuleTypeList.Codes.PCP, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission, CusAuthorisationRuleTypeList.Codes.PCV, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission, CusAuthorisationRuleTypeList.Codes.STO, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission, CusAuthorisationRuleTypeList.Codes.SUB, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission, CusAuthorisationRuleTypeList.Codes.TRA, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission, CusAuthorisationRuleTypeList.Codes.USE, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission, CusAuthorisationRuleTypeList.Codes.WAR, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, CusAuthorisationRuleTypeList.Codes.AUT, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, CusAuthorisationRuleTypeList.Codes.CLE, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, CusAuthorisationRuleTypeList.Codes.CNT, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, CusAuthorisationRuleTypeList.Codes.CON, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, CusAuthorisationRuleTypeList.Codes.INF, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, CusAuthorisationRuleTypeList.Codes.NAT, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, CusAuthorisationRuleTypeList.Codes.OFC, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, CusAuthorisationRuleTypeList.Codes.PCD, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, CusAuthorisationRuleTypeList.Codes.PCP, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, CusAuthorisationRuleTypeList.Codes.PCV, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, CusAuthorisationRuleTypeList.Codes.STO, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, CusAuthorisationRuleTypeList.Codes.SUB, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, CusAuthorisationRuleTypeList.Codes.TRA, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, CusAuthorisationRuleTypeList.Codes.USE, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, CusAuthorisationRuleTypeList.Codes.WAR, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, CusAuthorisationRuleTypeList.Codes.AUT, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, CusAuthorisationRuleTypeList.Codes.CLE, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, CusAuthorisationRuleTypeList.Codes.CNT, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, CusAuthorisationRuleTypeList.Codes.CON, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, CusAuthorisationRuleTypeList.Codes.INF, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, CusAuthorisationRuleTypeList.Codes.NAT, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, CusAuthorisationRuleTypeList.Codes.OFC, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, CusAuthorisationRuleTypeList.Codes.PCD, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, CusAuthorisationRuleTypeList.Codes.PCP, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, CusAuthorisationRuleTypeList.Codes.PCV, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, CusAuthorisationRuleTypeList.Codes.STO, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, CusAuthorisationRuleTypeList.Codes.SUB, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, CusAuthorisationRuleTypeList.Codes.TRA, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, CusAuthorisationRuleTypeList.Codes.USE, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, CusAuthorisationRuleTypeList.Codes.WAR, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, CusAuthorisationRuleTypeList.Codes.AUT, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, CusAuthorisationRuleTypeList.Codes.CLE, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, CusAuthorisationRuleTypeList.Codes.CNT, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, CusAuthorisationRuleTypeList.Codes.CON, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, CusAuthorisationRuleTypeList.Codes.INF, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, CusAuthorisationRuleTypeList.Codes.NAT, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, CusAuthorisationRuleTypeList.Codes.OFC, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, CusAuthorisationRuleTypeList.Codes.PCD, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, CusAuthorisationRuleTypeList.Codes.PCP, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, CusAuthorisationRuleTypeList.Codes.PCV, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, CusAuthorisationRuleTypeList.Codes.STO, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, CusAuthorisationRuleTypeList.Codes.SUB, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, CusAuthorisationRuleTypeList.Codes.TRA, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, CusAuthorisationRuleTypeList.Codes.USE, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, CusAuthorisationRuleTypeList.Codes.WAR, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.EndUse, CusAuthorisationRuleTypeList.Codes.AUT, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.EndUse, CusAuthorisationRuleTypeList.Codes.CLE, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.EndUse, CusAuthorisationRuleTypeList.Codes.CNT, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.EndUse, CusAuthorisationRuleTypeList.Codes.CON, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.EndUse, CusAuthorisationRuleTypeList.Codes.INF, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.EndUse, CusAuthorisationRuleTypeList.Codes.NAT, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.EndUse, CusAuthorisationRuleTypeList.Codes.OFC, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.EndUse, CusAuthorisationRuleTypeList.Codes.PCD, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.EndUse, CusAuthorisationRuleTypeList.Codes.PCP, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.EndUse, CusAuthorisationRuleTypeList.Codes.PCV, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.EndUse, CusAuthorisationRuleTypeList.Codes.STO, 1, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.EndUse, CusAuthorisationRuleTypeList.Codes.SUB, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.EndUse, CusAuthorisationRuleTypeList.Codes.TRA, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.EndUse, CusAuthorisationRuleTypeList.Codes.USE, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.EndUse, CusAuthorisationRuleTypeList.Codes.WAR, 0, 1);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit, CusAuthorisationRuleTypeList.Codes.Location, 0, 9999);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, CusAuthorisationRuleTypeList.Codes.Location, 0, 9999);
				AssertRuleRequirement(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir, CusAuthorisationRuleTypeList.Codes.Location, 0, 9999);
			});
		}

		void AssertRuleRequirement(CusAuthorisationHeader authorisationHeader, ZString authorizationType, ZString ruleType, int expectedMinRequired, int expectedMaxAllowed)
		{
			var requirement = CusAuthorisationRuleRequirementHelper.GetRequirement(authorisationHeader, authorizationType, ruleType);
			var not = expectedMinRequired == 0 ? "not " : string.Empty;
			AssertEquals($"Rule {ruleType} should {not}be required for authorisations of type {authorizationType}.", expectedMinRequired, requirement.MinRequired);
			AssertEquals($"Authorisations of type {authorizationType} should not allow more than {expectedMaxAllowed} rule(s) of type {ruleType}.", expectedMaxAllowed, requirement.MaxAllowed);
		}

		public void TestIsAuthorizationShortCodeMandatory()
		{
			var authorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			var authorizationTypes = new string[] { Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing,
												Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing,
												Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission,
												Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1,
												Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2,
												Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP,
												Customs.Business.CusAuthorizationHeaderTypeList.Codes.EndUse };
			foreach (var authorizationType in authorizationTypes)
			{
				authorisationHeader.CPH_IsAdHoc = false;
				Assert("IsAuthorizationShortCodeMandatory should be true when applied against non adHoc authorisation of type IPO, OPO, TEA, CW1, CW2, CWP and EUS.", CusAuthorisationRuleRequirementHelper.IsAuthorizationShortCodeMandatory(authorisationHeader, authorizationType));

				authorisationHeader.CPH_IsAdHoc = true;
				AssertEquals("IsAuthorizationShortCodeMandatory should always be false when applied against an adHoc authorisation.", false, CusAuthorisationRuleRequirementHelper.IsAuthorizationShortCodeMandatory(authorisationHeader, authorizationType));
			}
		}

		public void TestIsDescriptionMandatory()
		{
			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			var authorizationTypes = new string[] { Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit,
													Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit,
													Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir };

			var authorisationRule = authorizationHeader.CusAuthorisationRules.AddNew();

			foreach (var authorizationType in authorizationTypes)
			{
				authorisationRule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
				AssertEquals("IsDescriptionMandatory should return TRUE for authorization types ACE, ACR, and ACT with rule code 'Location'.", true, CusAuthorisationRuleRequirementHelper.IsDescriptionMandatory(authorizationType, authorisationRule.CPR_RuleCode));

				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.OFC;
				AssertEquals("IsDescriptionMandatory should return FALSE for authorization types ACE, ACR, and ACT with rule code other than 'Location'.", false, CusAuthorisationRuleRequirementHelper.IsDescriptionMandatory(authorizationType, authorisationRule.CPR_RuleCode));
			}
		}

		public void TestCheckValueFrom_CLE()
		{
			cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.CLE;
			ValidationTestHelper.AssertErrorIfInvalidCode(cusAuthorisationRule.CPR_ValueFromInfo, "TEST", YesNoList.Codes.Yes);
		}

		public void TestCheckValueFrom_CNT()
		{
			cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.CNT;

			CombineAssertions(() =>
			{
				cusAuthorisationRule.CPR_ValueFrom = "-1";
				AssertHasError("Invalid", cusAuthorisationRule.CPR_ValueFromInfo, CusAuthorisationRuleRequirementHelper.ErrorMessageIfNegative(CusAuthorisationRuleTypeList.Codes.CNT));

				cusAuthorisationRule.CPR_ValueFrom = "5";
				AssertNoError("Valid", cusAuthorisationRule.CPR_ValueFromInfo, CusAuthorisationRuleRequirementHelper.ErrorMessageIfNegative(CusAuthorisationRuleTypeList.Codes.CNT));
			});
		}

		public void TestCheckValueFrom_WAR()
		{
			cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.WAR;
			CombineAssertions(() =>
			{
				cusAuthorisationRule.CPR_ValueFrom = "1";
				AssertHasError("Invalid", cusAuthorisationRule.CPR_ValueFromInfo, CusAuthorisationRuleRequirementHelper.ErrorMessageIfNotGreaterThan1(CusAuthorisationRuleTypeList.Codes.WAR));

				cusAuthorisationRule.CPR_ValueFrom = "5";
				AssertNoError("Valid", cusAuthorisationRule.CPR_ValueFromInfo, CusAuthorisationRuleRequirementHelper.ErrorMessageIfNotGreaterThan1(CusAuthorisationRuleTypeList.Codes.WAR));
			});
		}

		public void TestCheckValueFrom_STO()
		{
			cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.STO;
			CombineAssertions(() =>
			{
				cusAuthorisationRule.CPR_ValueFrom = "1";
				AssertHasMessageError("Invalid", cusAuthorisationRule.CPR_ValueFromInfo, CusAuthorisationRuleRequirementHelper.ErrorMessageIfNotGreaterThan1(CusAuthorisationRuleTypeList.Codes.STO));

				cusAuthorisationRule.CPR_ValueFrom = "5";
				AssertNoMessageError("Valid", cusAuthorisationRule.CPR_ValueFromInfo, CusAuthorisationRuleRequirementHelper.ErrorMessageIfNotGreaterThan1(CusAuthorisationRuleTypeList.Codes.STO));

				cusAuthorisationRule.CPR_ValueFrom = "100";
				AssertHasMessageError("Invalid", cusAuthorisationRule.CPR_ValueFromInfo, CusAuthorisationRuleRequirementHelper.ErrorMessageIfNotLessThan100(CusAuthorisationRuleTypeList.Codes.STO));
			});
		}

		public void TestCheckValueFrom_PCD()
		{
			AssertErrorWhenValueFromOutside0To100(CusAuthorisationRuleTypeList.Codes.PCD);
		}

		public void TestCheckValueFrom_PCP()
		{
			AssertErrorWhenValueFromOutside0To100(CusAuthorisationRuleTypeList.Codes.PCP);
		}

		public void TestCheckValueFrom_PCV()
		{
			AssertErrorWhenValueFromOutside0To100(CusAuthorisationRuleTypeList.Codes.PCV);
		}

		public void TestCheckValueFrom_OFC()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR000530", "Anger", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE001235", "Berlin", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			cusAuthorisationRule_OFC.AuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation;
			cusAuthorisationRule_OFC.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.OFC;
			cusAuthorisationRule_OFC.CPR_ValueFrom = "anything just not empty";
			Factory.Save();

			ValidationTestHelper.AssertErrorIfInvalidCode(cusAuthorisationRule_OFC.CPR_ValueFromInfo, new ZString("DE001235"), new ZString("FR000530"));
		}

		public void TestCheckValueFrom_USEWithCW1()
		{
			cusAuthorisationRule_USE.AuthorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			ValidationTestHelper.AssertErrorIfInvalidCode(cusAuthorisationRule_USE.CPR_ValueFromInfo, AuthorizationRuleUseValueFromList.Codes.IST, AuthorizationRuleUseValueFromList.Codes.ENE);
		}

		public void TestCheckValueFrom_USEWithCW2()
		{
			cusAuthorisationRule_USE.AuthorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;
			ValidationTestHelper.AssertErrorIfInvalidCode(cusAuthorisationRule_USE.CPR_ValueFromInfo, AuthorizationRuleUseValueFromList.Codes.IST, AuthorizationRuleUseValueFromList.Codes.ENE);
		}

		public void TestCheckValueFrom_USEWithCWP_PSA()
		{
			cusAuthorisationRule_USE.AuthorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
			ValidationTestHelper.AssertErrorIfInvalidCode(cusAuthorisationRule_USE.CPR_ValueFromInfo, AuthorizationRuleUseValueFromList.Codes.ENE, AuthorizationRuleUseValueFromList.Codes.PSA);
		}

		public void TestCheckValueFrom_USEWithCWP_U()
		{
			cusAuthorisationRule_USE.AuthorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
			ValidationTestHelper.AssertErrorIfInvalidCode(cusAuthorisationRule_USE.CPR_ValueFromInfo, AuthorizationRuleUseValueFromList.Codes.ENE, AuthorizationRuleUseValueFromList.Codes.U);
		}

		public void TestCheckValueFrom_USEWithCWP_PAA()
		{
			cusAuthorisationRule_USE.AuthorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
			ValidationTestHelper.AssertErrorIfInvalidCode(cusAuthorisationRule_USE.CPR_ValueFromInfo, AuthorizationRuleUseValueFromList.Codes.ENE, AuthorizationRuleUseValueFromList.Codes.PAA);
		}

		public void TestCheckValueFrom_USEWithTST_IST()
		{
			cusAuthorisationRule_USE.AuthorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			ValidationTestHelper.AssertErrorIfInvalidCode(cusAuthorisationRule_USE.CPR_ValueFromInfo, AuthorizationRuleUseValueFromList.Codes.PAA, AuthorizationRuleUseValueFromList.Codes.IST);
		}

		public void TestCheckValueFrom_USEWithTST_LAD()
		{
			cusAuthorisationRule_USE.AuthorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			ValidationTestHelper.AssertErrorIfInvalidCode(cusAuthorisationRule_USE.CPR_ValueFromInfo, AuthorizationRuleUseValueFromList.Codes.PAA, AuthorizationRuleUseValueFromList.Codes.LAD);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusAuthorisationRule = Factory.NewWithValidTestData<CusAuthorisationRule>();
			cusAuthorisationRule.AuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusAuthorisationRule.AuthorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;

			cusAuthorisationRule_USE = Factory.NewWithValidTestData<CusAuthorisationRule>();
			cusAuthorisationRule_USE.AuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusAuthorisationRule_USE.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.USE;
			cusAuthorisationRule_USE.CPR_ValueFrom = "anything just not empty";

			cusAuthorisationRule_OFC = Factory.NewWithValidTestData<CusAuthorisationRule>();
			cusAuthorisationRule_OFC.AuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
		}
		CusAuthorisationRule cusAuthorisationRule;
		CusAuthorisationRule cusAuthorisationRule_USE;
		CusAuthorisationRule cusAuthorisationRule_OFC;

		void AssertErrorWhenValueFromOutside0To100(ZString ruleCode)
		{
			cusAuthorisationRule.CPR_RuleCode = ruleCode;
			CombineAssertions(() =>
			{
				cusAuthorisationRule.CPR_ValueFrom = "-1";
				AssertHasMessageError("Less than 0", cusAuthorisationRule.CPR_ValueFromInfo, CusAuthorisationRuleRequirementHelper.ErrorMessageIfOutOfBoundaries(ruleCode));

				cusAuthorisationRule.CPR_ValueFrom = "0";
				AssertNoMessageError("Equal to 0", cusAuthorisationRule.CPR_ValueFromInfo, CusAuthorisationRuleRequirementHelper.ErrorMessageIfOutOfBoundaries(ruleCode));

				cusAuthorisationRule.CPR_ValueFrom = "50";
				AssertNoMessageError("Between 0 and 100", cusAuthorisationRule.CPR_ValueFromInfo, CusAuthorisationRuleRequirementHelper.ErrorMessageIfOutOfBoundaries(ruleCode));

				cusAuthorisationRule.CPR_ValueFrom = "100";
				AssertNoMessageError("Equal to 100", cusAuthorisationRule.CPR_ValueFromInfo, CusAuthorisationRuleRequirementHelper.ErrorMessageIfOutOfBoundaries(ruleCode));

				cusAuthorisationRule.CPR_ValueFrom = "101";
				AssertHasMessageError("Greater than 100", cusAuthorisationRule.CPR_ValueFromInfo, CusAuthorisationRuleRequirementHelper.ErrorMessageIfOutOfBoundaries(ruleCode));
			});
		}

		public void TestCusAuthorisationRuleRequirementHelperMessageErrorConstant()
		{
			CombineAssertions("Assert ErrorMessage text:", () =>
			{
				var ruleCode = CusAuthorisationRuleTypeList.Codes.CNT;
				AssertEquals(CusAuthorisationRuleRequirementHelper.ErrorMessageIfNotGreaterThan1(ruleCode), "Rule code: CNT. The value entered in Value From field should be greater than 1.");
				AssertEquals(CusAuthorisationRuleRequirementHelper.ErrorMessageIfOutOfBoundaries(ruleCode), "Rule code: CNT. The value entered in Value From field should be comprise between 0 and 100 included.");
				AssertEquals(CusAuthorisationRuleRequirementHelper.ErrorMessageIfNegative(ruleCode), "Rule code: CNT. The value entered in Value From field should be greater than -1.");
				AssertEquals(CusAuthorisationRuleRequirementHelper.ErrorMessageIfNotLessThan100(ruleCode), "Rule code: CNT. The value entered in Value From field should be less than 100.");
			});
		}
	}
}
