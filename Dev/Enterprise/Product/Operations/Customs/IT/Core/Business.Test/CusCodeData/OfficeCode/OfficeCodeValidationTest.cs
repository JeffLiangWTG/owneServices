using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class OfficeCodeValidationTest : CusCodeDataValidationTest
{
	public void TestCheckCY_Code_ValidateRuleR0676_ExportUCC6()
	{
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		var expectedMessageError = "[R0676] A 'CCL' code must be present in Entry instruction > authorizations > type";
		var customsOffice = customsOffices.AddNew("PRE", "");
		var cclAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = "EXP";

			CombineAssertions(() =>
			{
				customsOffice.Validation.ValidateCY_Code();
				AssertHasMessageErrorContaining("When no Autorization has been provided, error message is expected", customsOffice.CY_CodeInfo, expectedMessageError);

				cclAuthorizationUsage.AGC_Code = "ARG";

				customsOffice.Validation.ValidateCY_Code();
				AssertHasMessageErrorContaining("When no Autorization of type CCL has been provided, error message is expected", customsOffice.CY_CodeInfo, expectedMessageError);

				cclAuthorizationUsage.AGC_Code = "CCL";
				customsOffice.Validation.ValidateCY_Code();
				AssertNoMessageErrorContaining("When Autorization of type CCL has been provided, no error message is expected", customsOffice.CY_CodeInfo, expectedMessageError);

				cclAuthorizationUsage.AGC_Code = "ARG";
				customsOffice.CY_Code = "XXX";
				customsOffice.Validation.ValidateCY_Code();
				AssertNoMessageErrorContaining("When Customs Office is not PRE, even if autorization of type CCL has not been provided, no error message is expected", customsOffice.CY_CodeInfo, expectedMessageError);
			});
		}
	}

	public void TestCheckCY_Code_ValidateRuleR0676_Import()
	{
		declaration.JE_MessageType = "IMP";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		var customsOffice = customsOffices.AddNew("PRE", "");
		var cclAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();

		cclAuthorizationUsage.AGC_Code = "ARG";
		customsOffice.Validation.ValidateCY_Code();
		AssertNoMessageErrors("In IMP declaration, when no Autorization of type CCL has been provided, no error message is expected", customsOffice.CY_CodeInfo);
	}

	public void TestCheckCY_CodeValidateRuleR096()
	{
		var expectedMessageError = "When Country of Office of Destination is AD then Country of Office of Transit must be AD";

		var officeOfTransit = customsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfTransit, "");
		var officeOfDestination = customsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfDestination, "");
		AssertNotNull("[PRE-CONDITION] Office of Destination");

		var officeOfTransitValidation = officeOfTransit.Validation;
		CombineAssertions(() =>
		{
			officeOfDestination.CY_Data = "AD0001";
			officeOfTransit.CY_Data = "IT00001";
			officeOfTransitValidation.ValidateCY_Code();
			AssertHasMessageErrorContaining("When Country of Office of Destination is 'AD' and Country of Office of Transit is not 'AD'", officeOfTransit.CY_CodeInfo, expectedMessageError);

			officeOfTransit.CY_Data = "";
			officeOfTransitValidation.ValidateCY_Code();
			AssertNoMessageErrorContaining("When Country of Office of Destination is 'AD' and Country of Office of Transit is empty", officeOfTransit.CY_CodeInfo, expectedMessageError);

			officeOfTransit.CY_Data = "AD00002";
			officeOfTransitValidation.ValidateCY_Code();
			AssertNoMessageErrorContaining("When Country of Office of Destination is 'AD' and Country of Office of Transit is 'AD'", officeOfTransit.CY_CodeInfo, expectedMessageError);

			officeOfDestination.CY_Data = "IT00002";
			officeOfTransit.CY_Data = "AD0001";
			officeOfTransitValidation.ValidateCY_Code();
			AssertNoMessageErrorContaining("When Country of Office of Destination is not 'AD' and Country of Office of Transit is not 'AD'", officeOfTransit.CY_CodeInfo, expectedMessageError);
		});
	}

	public void TestCY_DataPRE_ListValidation()
	{
		var expectedMessage = "Office of Presentation for Centralized Clearance: The value you selected is not in the list.";
		SetUpCustomsOffice();
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageVersion = "XML";
		var office = declaration.CustomsOffices.AddNew();
		office.CY_Code = "PRE";

		office.CY_Data = "IT000000";
		AssertHasMessageErrorContaining(office.CY_DataInfo, expectedMessage);

		office.CY_Data = "IT654321";
		AssertNoMessageErrorContaining(office.CY_DataInfo, expectedMessage);
	}

	public void TestCY_DataSVO_ListValidation()
	{
		var expectedMessage = "Supervising Office: The value you selected is not in the list.";
		SetUpCustomsOffice();
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageVersion = "XML";
		var office = declaration.CustomsOffices.AddNew();
		office.CY_Code = "SVO";

		office.CY_Data = "IT000000";
		AssertHasMessageErrorContaining(office.CY_DataInfo, expectedMessage);

		office.CY_Data = "IT654321";
		AssertNoMessageErrorContaining(office.CY_DataInfo, expectedMessage);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";

		customsOffices = declaration.CustomsOffices;
	}
	JobDeclaration declaration;
	OfficeCodeCollection customsOffices;

	void SetUpCustomsOffice()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eunGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eunGrouping);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		var codeIT654321 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT654321", "Central Community Transit Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();
	}
}
