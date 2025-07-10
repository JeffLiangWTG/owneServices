using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	class CusAuthorisationHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCPH_Number_MinimumRepetitionsOfRuleCodeUSEIfHeaderTypeSDE()
		{
			var expectedError = GetMinimumRepetitionError(1, CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration);

			NUnit.Framework.Assert.Multiple(() =>
			{
				authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
				authorizationHeader.CPH_Number = "FR123456";
				AssertNoError("CPH_Type 'SDE', CPH_Number starts with 'FR', no rule", authorizationNumberInfo, expectedError);

				authorizationHeader.CPH_Number = "DE123456";
				AssertHasError("CPH_Type 'SDE', CPH_Number starts with 'DE', no rule", authorizationNumberInfo, expectedError);

				authorizationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Usage;
				authorizationHeader.Validation.ValidateCPH_Number();
				AssertNoError("CPH_Type 'SDE', CPH_Number starts with 'DE', rule code 'USE'", authorizationNumberInfo, expectedError);
			});
		}

		public void TestCheckCPH_Number_MinimumRepetitionsOfRuleCodeUSEIfHeaderTypeEIR()
		{
			var expectedError = GetMinimumRepetitionError(1, CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords);

			NUnit.Framework.Assert.Multiple(() =>
			{
				authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
				authorizationHeader.CPH_Number = "FR123456";
				AssertNoError("CPH_Type 'EIR', CPH_Number starts with 'FR', no rule", authorizationNumberInfo, expectedError);

				authorizationHeader.CPH_Number = "DE123456";
				AssertHasError("CPH_Type 'EIR', CPH_Number starts with 'DE', no rule", authorizationNumberInfo, expectedError);

				authorizationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Usage;
				authorizationHeader.Validation.ValidateCPH_Number();
				AssertNoError("CPH_Type 'EIR', CPH_Number starts with 'DE', rule code 'USE'", authorizationNumberInfo, expectedError);
			});
		}

		public void TestCheckCPH_Number_Format()
		{
			var validationErrorsACE = "Digit 01+02: Country Code 'DE' required.\r\n" +
				"Digit 03-05: EU Authorization Type 'ACE' is required.\r\n" +
				"Digit 06-09: Office Code of the issuing Main Customs Office required.\r\n" +
				"Digit 10+11: Authorization Number of Type 'ACE' requires 'ZE'.\r\n" +
				"Digit 12-17: Sequence Number of Customs Office for national Authorization Type required.";

			var validationErrorsACT = "Digit 01+02: Country Code 'DE' required.\r\n" +
				"Digit 03-05: EU Authorization Type 'ACT' is required.\r\n" +
				"Digit 06-09: Office Code of the issuing Main Customs Office required.\r\n" +
				"Digit 10+11: Authorization Number of Type 'ACT' requires 'ZT'.\r\n" +
				"Digit 12-17: Sequence Number of Customs Office for national Authorization Type required.";

			var validationErrorLength = "Authorization Number must have 17 digits.";

			NUnit.Framework.Assert.Multiple(() =>
			{
				authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
				authorizationHeader.CPH_Number = "FRABCX234XXAA0123";
				AssertHasMessageError("CPH_Type 'ACE', invalid number", authorizationNumberInfo, validationErrorsACE);

				authorizationHeader.CPH_Number = "DEACE1234ZE00012";
				AssertHasMessageError("CPH_Type 'ACE', Invalid length", authorizationNumberInfo, validationErrorLength);

				authorizationHeader.CPH_Number = "DEACE1234ZE000123";
				AssertNoMessageErrors("CPH_Type 'ACE', Valid number", authorizationNumberInfo);

				authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				authorizationHeader.CPH_Number = "FRABCX234XXAA0123";
				AssertHasMessageError("CPH_Type 'ACT', invalid number", authorizationNumberInfo, validationErrorsACT);

				authorizationHeader.CPH_Number = "DEACT1234ZT00012";
				AssertHasMessageError("CPH_Type 'ACT', Invalid length", authorizationNumberInfo, validationErrorLength);

				authorizationHeader.CPH_Number = "DEACT1234ZT000123";
				AssertNoMessageErrors("CPH_Type 'ACT', Valid number", authorizationNumberInfo);
			});
		}

		public void TestCheckCPH_OH_PermitHolder()
		{
			const string message = "The captured Authorization Type requires a German EORI-Number in Authorization Holders Organization (Registration Numbers / Codes).";
			var header = Factory.New<OrgHeader>();

			NUnit.Framework.Assert.Multiple(() =>
			{
				authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
				authorizationHeader.Validation.ValidateCPH_OH_PermitHolder();
				AssertNoMessageError("CPH_Type is ACE, no PermitHolder empty", authorizationHeader.CPH_OH_PermitHolderInfo, message);

				authorizationHeader.CPH_OH_PermitHolder = header.PK;
				AssertHasMessageError("CPH_Type is ACE", authorizationHeader.CPH_OH_PermitHolderInfo, message);

				authorizationHeader.CPH_Type = "-XX";
				authorizationHeader.Validation.ValidateCPH_OH_PermitHolder();
				AssertNoMessageError("CPH_Type is not ACE/ACT", authorizationHeader.CPH_OH_PermitHolderInfo, message);

				authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				authorizationHeader.Validation.ValidateCPH_OH_PermitHolder();
				AssertHasMessageError("CPH_Type is ACT", authorizationHeader.CPH_OH_PermitHolderInfo, message);

				header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789012345", Core.Constants.CountryCodes.Greece);
				authorizationHeader.Validation.ValidateCPH_OH_PermitHolder();
				AssertHasMessageError("CPH_Type is ACT and Holder has a Registration Number, wrong country", authorizationHeader.CPH_OH_PermitHolderInfo, message);

				header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789012345", Core.Constants.CountryCodes.Germany);
				authorizationHeader.Validation.ValidateCPH_OH_PermitHolder();
				AssertNoMessageError("CPH_Type is ACT and Holder has a Registration Number", authorizationHeader.CPH_OH_PermitHolderInfo, message);

				authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
				authorizationHeader.Validation.ValidateCPH_OH_PermitHolder();
				AssertNoMessageError("CPH_Type is ACE and Holder has a Registration Number", authorizationHeader.CPH_OH_PermitHolderInfo, message);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestHelper.CreateCL010CoutryList(Factory);
			authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationRule = authorizationHeader.CusAuthorisationRules.AddNew();
			authorizationNumberInfo = authorizationHeader.CPH_NumberInfo;
		}

		CusAuthorisationHeader authorizationHeader;
		CusAuthorisationRule authorizationRule;
		ZPropertyInfo authorizationNumberInfo;

		ZString GetMinimumRepetitionError(ZInt minRequired, ZString ruleType, ZString authorizationType) => $"You are required to have at least {minRequired} authorization rule of type '{ruleType}' for authorization type '{authorizationType}'.";
	}
}
