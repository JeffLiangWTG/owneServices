using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.Testing
{
	sealed class CusAuthorisationRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCPR_Description()
		{
			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit; 

			var authorisationRule = authorizationHeader.CusAuthorisationRules.AddNew();
			authorisationRule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;

			authorisationRule.CPR_Description = ZString.Empty;
			AssertHasMessageErrorContaining("MessageError is expected for authorization type 'ACE' when the LOC rule description is empty.", authorisationRule.CPR_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			authorizationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
			authorisationRule.CPR_Description = ZString.Empty;
			AssertHasMessageErrorContaining("MessageError is expected for authorization type 'ACR' when the LOC rule description is empty.", authorisationRule.CPR_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			authorizationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
			authorisationRule.CPR_Description = ZString.Empty;
			AssertHasMessageErrorContaining("MessageError is expected for authorization type 'ACT' when the LOC rule description is empty.", authorisationRule.CPR_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			authorisationRule.CPR_Description = "Description";
			AssertNoMessageErrorContaining("No MessageError is expected for any of the authorization type ACE, ACR, ACT when the LOC rule description is entered.", authorisationRule.CPR_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCPR_ValueFrom_WhenCPR_RuleCodeIsCONValueCanBeEnteredBeyondTheList()
		{
			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.EndUse;

			var authorisationRule = authorizationHeader.CusAuthorisationRules.AddNew();

			authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.TRA;
			authorisationRule.CPR_ValueFrom = "RandomValue";
			AssertHasErrorContaining("Error is expected on entering value other than that on the list for rules other than CON.", authorisationRule.CPR_ValueFromInfo, ListValidation.InvalidCodeError);

			authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.CON;
			authorisationRule.CPR_ValueFrom = "RandomValue";
			AssertNoErrorContaining("No error is expected on entering value other than that on the list for CON rule.", authorisationRule.CPR_ValueFromInfo, ListValidation.InvalidCodeError);
		}
	}
}
