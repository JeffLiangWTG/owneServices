using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class EuOfficeCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestFRCStandingDataValidationOfficeCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eunZZZ);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IEDUB100", "DUBLIN PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeFR000001 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR000001", "Central Community Transit Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeFR000001.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "EXP");
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			var office = dec.CustomsOffices.AddNew();
			AssertNoWarningContaining(office.CY_DataInfo, "An office code is needed");
			office.CY_Data = "FR000001";
			AssertNoWarningContaining(office.CY_DataInfo, "An office code is needed");
			AssertNoMessageErrorContaining(office.CY_DataInfo, "not a valid office");

			office.CY_Data = "";
			AssertHasMessageErrorContaining(office.CY_DataInfo, "An office code is needed");

			office.CY_Data = "FR888888";
			AssertHasMessageErrorContaining(office.CY_DataInfo, "not a valid office");
			AssertNoMessageErrorContaining(office.CY_DataInfo, "EU customs office codes should start with the country/region prefix");

			office.CY_Data = "IEDUB100";
			AssertNoMessageErrorContaining(office.CY_DataInfo, "not a valid office");

			office.CY_Data = "00000000";
			AssertHasMessageErrorContaining(office.CY_DataInfo, "EU customs office codes should start with the country/region prefix");

			office.CY_Data = "US000001";
			AssertHasWarningContaining(office.CY_DataInfo, "United States is not listed as being in the European Union. Check the value of your office code or check that your list of economic groupings is up to date.");

			office.CY_Data = "XX000001";
			AssertHasWarningContaining(office.CY_DataInfo, "not listed as a country");
		}

		public void TestCheckOfficeDoesNotFulfillAllRoles()
		{
			const string message = "According to reference data, this office does not fulfill this role";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var cusofWithExt = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR000001", "Has attribute EXT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusofWithExt.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
			var cusofWithEin = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR000002", "Has attribute EIN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusofWithEin.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExitInland);
			var cusofWithoutExtAndEin = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR000003", "No attributes EXT and EIN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.New<JobDeclarationForCustomsOfficeRequirementTest>();
			var officeHelper = declaration.CustomsOfficeRequirementHelper;
			officeHelper.SetOtherRequirements(new List<CustomsOfficeRequirement>
			{
				new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfExport, false, false)
				{
					OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExit, EuOfficeCodesTypes.Codes.OfficeOfExitInland },
				}
			});

			var office = declaration.CustomsOffices.AddNew();
			office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExport;
			CombineAssertions(() =>
			{
				office.CY_Data = cusofWithExt.ZZD_Code;
				AssertNoMessageError("Customs office has attribute EXT", office.CY_DataInfo, message);

				office.CY_Data = cusofWithEin.ZZD_Code;
				AssertNoMessageError("Customs office has attribute EIN", office.CY_DataInfo, message);

				office.CY_Data = cusofWithoutExtAndEin.ZZD_Code;
				AssertHasMessageError("Customs office has no attributes EXT and EIN", office.CY_DataInfo, message);
			});
		}

		public void TestCheckOnlyOneOfficeOfThisTypeExists()
		{
			var declaration = Factory.New<JobDeclarationForCustomsOfficeRequirementTest>();
			var officeHelper = declaration.CustomsOfficeRequirementHelper;
			officeHelper.SetOtherRequirements(new List<CustomsOfficeRequirement>
			{
				new CustomsOfficeRequirement
				{
					OfficeRole = "CAU",
					MaxOfficeCountLimit = 2
				}
			});

			declaration.CustomsOffices.AddNew("CAU");
			var office2 = declaration.CustomsOffices.AddNew("CAU");
			AssertNoMessageError(office2.CY_CodeInfo, "Only one office of type CAU is allowed");

			officeHelper.SetOtherRequirements(new List<CustomsOfficeRequirement>
			{
				new CustomsOfficeRequirement
				{
					OfficeRole = "CAU"
				}
			});

			office2.RunPreSaveValidation();
			AssertHasMessageError(office2.CY_CodeInfo, "Only one office of type CAU is allowed");
		}

		public void TestMaxOfficesOfThisTypeSupported()
		{
			const string message = "Only a maximum of 2 Customs Offices with Purpose 'CAU' may be specified.";
			var declaration = Factory.New<JobDeclarationForCustomsOfficeRequirementTest>();
			var officeHelper = declaration.CustomsOfficeRequirementHelper;
			CombineAssertions(() =>
			{
				officeHelper.SetOtherRequirements(new List<CustomsOfficeRequirement>
				{
					new CustomsOfficeRequirement
					{
						OfficeRole = "CAU",
						MaxOfficeCountLimit = 2
					}
				});

				declaration.CustomsOffices.AddNew("CAU");
				var newOffice = declaration.CustomsOffices.AddNew("CAU");
				AssertNoMessageError("2 CAU offices: MaxOfficeCountLimit 2", newOffice.CY_CodeInfo, message);

				newOffice = declaration.CustomsOffices.AddNew("CAU");
				AssertHasMessageError("3 CAU offices: MaxOfficeCountLimit 2", newOffice.CY_CodeInfo, message);

				officeHelper.SetOtherRequirements(new List<CustomsOfficeRequirement>
				{
					new CustomsOfficeRequirement
					{
						OfficeRole = "CAU",
						MaxOfficeCountLimit = 3
					}
				});

				newOffice.Validation.ValidateCY_Code();
				AssertNoMessageError("3 CAU offices: MaxOfficeCountLimit 3", newOffice.CY_CodeInfo, message);

				officeHelper.SetOtherRequirements(new List<CustomsOfficeRequirement>
				{
					new CustomsOfficeRequirement
					{
						OfficeRole = "CAU",
						MaxOfficeCountLimit = null
					}
				});
				newOffice.Validation.ValidateCY_Code();
				AssertNoMessageError("MaxOfficeCountLimit is null", newOffice.CY_CodeInfo, message);
			});
		}

		public void TestCheckRuleR0676()
		{
			var messageError = "[R0676] Presentation Customs Office exist but Authorization with type code CCL(C513) is missing.";
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var authorisation1 = entryInstruction1.CusAuthorizationUsages.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var authorisation2 = entryInstruction2.CusAuthorizationUsages.AddNew();
			var office = declaration.CustomsOffices.AddNew();
			office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
			var info = office.CY_CodeInfo;
			var validation = office.Validation;

			CombineAssertions(() =>
			{
				using (var context = new CustomsOfficeValidationDeciderTestContext(declaration, isUCC6: true))
				{
					context.DisableRule(r => r.IsRuleR0676Active);
					validation.ValidateCY_Code();
					AssertNoMessageError("IsRuleR0676Active is disabled", info, messageError);

					context.EnableRule(r => r.IsRuleR0676Active);
					validation.ValidateCY_Code();
					AssertHasMessageError("IsRuleR0676Active is enabled", info, messageError);

					office.CY_Code = EuOfficeCodesTypes.Codes.ActualExitOffice;
					AssertNoMessageError("Not OfficeOfPresentation", info, messageError);

					authorisation1.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CentralizedClearance;
					office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
					validation.ValidateCY_Code();
					AssertHasMessageError("1 of 2 Entry instructions have CCL authorisation", info, messageError);

					authorisation2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CentralizedClearance;
					validation.ValidateCY_Code();
					AssertNoMessageError("All Entry instructions have CCL authorisation", info, messageError);
				}
			});
		}
	}
}
