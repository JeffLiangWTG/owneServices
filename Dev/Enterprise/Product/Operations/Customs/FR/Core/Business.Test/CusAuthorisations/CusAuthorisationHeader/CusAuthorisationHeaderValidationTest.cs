using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.CusAuthorisation.Testing
{
	class CusAuthorisationHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAUTRuleRequirement()
		{
			var authorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			authorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var authorizationTypeNeedingAUTRuleWhenNotAdHoc = new string[] { Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing,
																			 Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing,
																			 Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission,
																			 Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1,
																			 Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2,
																			 Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP,
																			 Customs.Business.CusAuthorizationHeaderTypeList.Codes.EndUse };

			var authorizationTypeNotNeedingAUTRule = new string[] { Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryStorage,
																			 CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation,
																			 CusAuthorizationHeaderTypeList.Codes.OtherThanOpo,
																			 CusAuthorizationHeaderTypeList.Codes.TemporaryExportation };

			foreach (var authorizationType in authorizationTypeNeedingAUTRuleWhenNotAdHoc)
			{
				authorisationHeader.CPH_Type = authorizationType;

				authorisationHeader.CPH_IsAdHoc = true;
				authorisationHeader.Validation.ValidateCPH_Number();
				AssertNoErrorContaining("OPO / IPO / TEA /CWP/ CW1 / CW2 / EUS type of authorizations should not require any AUT rule if AdHoc", authorisationHeader.CPH_NumberInfo, "You are required to have at least 1 authorization rule of type 'AUT'");

				authorisationHeader.CPH_IsAdHoc = false;
				authorisationHeader.Validation.ValidateCPH_Number();
				AssertHasErrorContaining("OPO / IPO / TEA /CWP/ CW1 / CW2 / EUS type of authorizations should require an AUT rule if not AdHoc.", authorisationHeader.CPH_NumberInfo, "You are required to have at least 1 authorization rule of type 'AUT'");
			}

			foreach (var authorizationType in authorizationTypeNotNeedingAUTRule)
			{
				authorisationHeader.CPH_Type = authorizationType;

				authorisationHeader.CPH_IsAdHoc = true;
				authorisationHeader.Validation.ValidateCPH_Number();
				AssertNoMessageErrorContaining("Other authorization types should not require any AUT rule.", authorisationHeader.CPH_NumberInfo, "You are required to have at least 1 authorization rule of type 'AUT'");

				authorisationHeader.CPH_IsAdHoc = false;
				authorisationHeader.Validation.ValidateCPH_Number();
				AssertNoMessageErrorContaining("Other authorization types should not require any AUT rule.", authorisationHeader.CPH_NumberInfo, "You are required to have at least 1 authorization rule of type 'AUT'");
			}
		}

		public void TestCheckCPH_NumberFormat()
		{
			var authorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			authorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var message = "Please note for specific regime, French Customs has set the maximum length to 30 characters. No special char allowed.";

			authorisationHeader.CPH_Number = "";
			AssertMandatoryValidationError(authorisationHeader.CPH_NumberInfo, true);

			authorisationHeader.CPH_Number = "123";
			AssertNoWarningContaining("Invalid length warning.", authorisationHeader.CPH_NumberInfo, message);

			authorisationHeader.CPH_Number = "1234567a9A4561";
			AssertNoWarningContaining("No warning expected.", authorisationHeader.CPH_NumberInfo, message);

			authorisationHeader.CPH_Number = "1234567a9A4561z";
			AssertNoWarningContaining("No warning expected.", authorisationHeader.CPH_NumberInfo, message);

			authorisationHeader.CPH_Number = "12345678!74561";
			AssertNoWarningContaining("No restriction on authorization number if not specific regime.", authorisationHeader.CPH_NumberInfo, message);

			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.OtherThanOpo;

			authorisationHeader.CPH_Number = "123";
			AssertNoWarningContaining("No invalid length warning on authorization number.", authorisationHeader.CPH_NumberInfo, message);

			authorisationHeader.CPH_Number = "1234567874561";
			AssertNoWarningContaining("No invalid length warning on authorization number expected.", authorisationHeader.CPH_NumberInfo, message);

			authorisationHeader.CPH_Number = "1234567a9A4561z1234567a9A4561za1";
			AssertHasWarningContaining("Length on authorization number should not exceed 30 for specific regime.", authorisationHeader.CPH_NumberInfo, message);

			authorisationHeader.CPH_Number = "12345678!74561";
			AssertHasWarningContaining("No special char allowed in authorization number for specific regime.", authorisationHeader.CPH_NumberInfo, message);
		}
	}
}
