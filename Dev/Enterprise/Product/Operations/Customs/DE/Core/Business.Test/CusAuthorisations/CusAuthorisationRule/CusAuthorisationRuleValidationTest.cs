using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class CusAuthorisationRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCPR_RuleCode_RuleCodeLOCMustBeUniqueOrMustHaveDifferentCustomsOffices()
		{
			var message = "This Location Code with same CUS value already exists for this authorization.";

			NUnit.Framework.Assert.Multiple(() =>
			{
				authorisationRule1.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
				authorisationRule1.CPR_ValueFrom = "0001";
				var linkedRule1 = authorisationRule1.LinkedCusAuthorisationRules.AddNew();
				linkedRule1.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
				linkedRule1.CPR_ValueFrom = "DE003202";
				authorisationRule1.Validation.ValidateCPR_RuleCode();
				AssertNoError("Only 1 Code-Value LOC-0001 and customs office 'DE003202'", authorisationRule1.CPR_RuleCodeInfo, message);

				authorisationRule2.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
				authorisationRule2.Validation.ValidateCPR_RuleCode();
				AssertNoError("Another Code-Value LOC-Empty", authorisationRule2Info, message);
				authorisationRule2.CPR_ValueFrom = "0002";
				authorisationRule2.Validation.ValidateCPR_RuleCode();
				AssertNoError("Another Code-Value LOC-0002", authorisationRule2Info, message);

				authorisationRule2.CPR_ValueFrom = "0001";
				authorisationRule2.Validation.ValidateCPR_RuleCode();
				AssertNoError("Repeated Code-Values LOC-0001, but no linked rules", authorisationRule2Info, message);
				var linkedRule2 = authorisationRule2.LinkedCusAuthorisationRules.AddNew();
				linkedRule2.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
				authorisationRule2.Validation.ValidateCPR_RuleCode();
				AssertNoError("Repeated Code-Values LOC-0001, linked rule CUS with emty value", authorisationRule2Info, message);
				linkedRule2.CPR_ValueFrom = "DE003202";
				authorisationRule2.Validation.ValidateCPR_RuleCode();
				AssertHasError("Repeated Code-Values LOC-0001, linked rule CUS with same value", authorisationRule2Info, message);

				linkedRule2.CPR_ValueFrom = "DE003205";
				authorisationRule2.Validation.ValidateCPR_RuleCode();
				AssertNoError("Repeated Code-Values LOC-0001, linked rule CUS with different value", authorisationRule2Info, message);
			});
		}

		public void TestCheckCPR_RuleCode_MaximumRepetitionsOfRuleCodeUSEIfHeaderTypeSDE()
		{
			var expectedError = GetMaximumRepetitionsError(1, CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration);

			NUnit.Framework.Assert.Multiple(() =>
			{
				authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
				authorizationHeader.CPH_Number = "FR123456";
				authorisationRule1.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Usage;
				authorisationRule2.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Usage;
				AssertHasError("CPH_Type 'SDE', 2 'USE'-rules", authorisationRule2Info, expectedError);

				authorisationRule1.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
				authorisationRule2.Validation.ValidateCPR_RuleCode();
				AssertNoError("CPH_Type 'SDE', 1 'USE'-rule", authorisationRule2Info, expectedError);
			});
		}

		public void TestCheckCPR_RuleCode_MaximumRepetitionsOfRuleCodeUSEIfHeaderTypeEIR()
		{
			var expectedError = GetMaximumRepetitionsError(1, CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords);

			NUnit.Framework.Assert.Multiple(() =>
			{
				authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
				authorizationHeader.CPH_Number = "FR123456";
				authorisationRule1.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Usage;
				authorisationRule2.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Usage;
				authorisationRule2.Validation.ValidateCPR_RuleCode();
				AssertHasError("CPH_Type 'EIR', 2 'USE'-rules", authorisationRule2Info, expectedError);

				authorisationRule1.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
				authorisationRule2.Validation.ValidateCPR_RuleCode();
				AssertNoError("CPH_Type 'EIR', 1 'USE'-rule", authorisationRule2Info, expectedError);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorisationRule1 = authorizationHeader.CusAuthorisationRules.AddNew();
			authorisationRule2 = authorizationHeader.CusAuthorisationRules.AddNew();
			authorisationRule2Info = authorisationRule2.CPR_RuleCodeInfo;
		}
		CusAuthorisationHeader authorizationHeader;
		CusAuthorisationRule authorisationRule1;
		CusAuthorisationRule authorisationRule2;
		ZPropertyInfo authorisationRule2Info;

		ZString GetMaximumRepetitionsError(ZInt maxAllowed, ZString ruleType, ZString authorisationType) => $"You are allowed to have a maximum of {maxAllowed} authorization rules of type '{ruleType}' for authorization type '{authorisationType}'.";
	}
}
