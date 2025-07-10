using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class CountryOfRoutingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Data_Phase4()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
			Factory.Save();

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			ValidationTestHelper.AssertInvalidCodeMessageError(countryOfRouting.CY_DataInfo, "YY", "DE");
		}

		public void TestCheckCY_Data_Phase5_InvalidCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
			Factory.Save();

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			ValidationTestHelper.AssertInvalidCodeMessageError(countryOfRouting.CY_DataInfo, "YY", "DE");
		}

		public void TestCheckCY_Data_Phase5()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var departureMovement = nctsHeader.MovementHeader;
			string errorMessage = "You have not entered";
			string errorMessageC0586 = "[C0586] You have not entered";

			using (var ruleContext = new CountryOfRoutingValidationDeciderTestContext<ICountryOfRoutingDeparturePhase5ValidationDecider>(Factory))
			{
				ruleContext.EnableRule(x => x.IsRuleC0586Active);

				CombineAssertions(() =>
				{
					departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
					countryOfRouting.CY_Data = ZString.Empty;
					AssertHasMessageErrorContaining("SecurityType is NON and country is empty.", countryOfRouting.CY_DataInfo, errorMessage);

					countryOfRouting.CY_Data = "PL";
					AssertNoMessageErrorContaining("SecurityType is NON and country is not empty.", countryOfRouting.CY_DataInfo, errorMessage);

					departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
					countryOfRouting.CY_Data = ZString.Empty;
					AssertHasMessageErrorContaining("SecurityType is ENT and country is empty.", countryOfRouting.CY_DataInfo, errorMessageC0586);

					countryOfRouting.CY_Data = "IT";
					AssertNoMessageErrorContaining("SecurityType is ENT and country is not empty.", countryOfRouting.CY_DataInfo, errorMessageC0586);

					departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
					countryOfRouting.CY_Data = ZString.Empty;
					AssertHasMessageErrorContaining("SecurityType is EXI and country is empty.", countryOfRouting.CY_DataInfo, errorMessageC0586);

					countryOfRouting.CY_Data = "PL";
					AssertNoMessageErrorContaining("SecurityType is EXI and country is not empty.", countryOfRouting.CY_DataInfo, errorMessageC0586);

					departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
					countryOfRouting.CY_Data = ZString.Empty;
					AssertHasMessageErrorContaining("SecurityType is BTH and country is empty.", countryOfRouting.CY_DataInfo, errorMessageC0586);

					countryOfRouting.CY_Data = "ES";
					AssertNoMessageErrorContaining("SecurityType is BTH and country is not empty.", countryOfRouting.CY_DataInfo, errorMessageC0586);
				});
			}

			using (var ruleContext = new CountryOfRoutingValidationDeciderTestContext<ICountryOfRoutingDeparturePhase5ValidationDecider>(Factory))
			{
				ruleContext.DisableRule(x => x.IsRuleC0586Active);

				CombineAssertions(() =>
				{
					departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
					countryOfRouting.CY_Data = ZString.Empty;
					AssertHasMessageErrorContaining("SecurityType is NON and country is empty.", countryOfRouting.CY_DataInfo, errorMessage);

					countryOfRouting.CY_Data = "PL";
					AssertNoMessageErrorContaining("SecurityType is NON and country is not empty.", countryOfRouting.CY_DataInfo, errorMessage);

					departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
					countryOfRouting.CY_Data = ZString.Empty;
					AssertHasMessageErrorContaining("SecurityType is ENT and country is empty.", countryOfRouting.CY_DataInfo, errorMessage);

					countryOfRouting.CY_Data = "IT";
					AssertNoMessageErrorContaining("SecurityType is ENT and country is not empty.", countryOfRouting.CY_DataInfo, errorMessage);

					departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
					countryOfRouting.CY_Data = ZString.Empty;
					AssertHasMessageErrorContaining("SecurityType is EXI and country is empty.", countryOfRouting.CY_DataInfo, errorMessage);

					countryOfRouting.CY_Data = "PL";
					AssertNoMessageErrorContaining("SecurityType is EXI and country is not empty.", countryOfRouting.CY_DataInfo, errorMessage);

					departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
					countryOfRouting.CY_Data = ZString.Empty;
					AssertHasMessageErrorContaining("SecurityType is BTH and country is empty.", countryOfRouting.CY_DataInfo, errorMessage);

					countryOfRouting.CY_Data = "ES";
					AssertNoMessageErrorContaining("SecurityType is BTH and country is not empty.", countryOfRouting.CY_DataInfo, errorMessage);
				});
			}
		}

		public void TestCheckC0030CommonRule_Phase5()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "CL112 Desc.");
			helper.CreateCusCodeList(RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "PL301080", "Polland", startDate, endDate);
			helper.CreateCusCodeList(RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "AD000001", "Andorra", startDate, endDate);
			Factory.Save();

			using (var ruleContext = new CountryOfRoutingValidationDeciderTestContext<ICountryOfRoutingDeparturePhase5ValidationDecider>(Factory))
			{
				UniversalReferenceTestDataHelper.RunAssertionsInPhase5TransitionPeriod(() =>
				{
					ruleContext.EnableRule(x => x.IsRuleC0030Active);
					ruleContext.DisableRule(x => x.IsRuleB1836Active);

					countryOfRouting.CY_Data = "PL301080";
					countryOfRouting.Validation.ValidateCY_Data();
					AssertHasMessageError("TP: ON, C0030: Active, B1836: Inactive, CL112 code: true, Custom office for transit is not entered", countryOfRouting.CY_DataInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);

					countryOfRouting.CY_Data = "PL301070";
					AssertNoMessageError("TP: ON, C0030: Active, B1836: Inactive, CL112 code: false, Custom office for transit is not required", countryOfRouting.CY_DataInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);

					var movementHeader = nctsHeader.MovementHeader;
					var transitOffice = movementHeader.CustomsOfficesForDeparture.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
					transitOffice.CY_Data = "TraOffic";
					countryOfRouting.CY_Data = "PL301080";
					AssertNoMessageError("TP: ON, C0030: Active, B1836: Inactive, CL112 code: true, Custom office for transit exists", countryOfRouting.CY_DataInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);

					movementHeader.CustomsOfficesForDeparture.RemoveAndDeleteAll();

					ruleContext.DisableRule(x => x.IsRuleC0030Active);
					ruleContext.DisableRule(x => x.IsRuleB1836Active);

					AssertNoMessageError("TP: ON, C0030: Inactive, B1836: Inactive, CL112 code: true, Custom office for transit is not required", countryOfRouting.CY_DataInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);

					ruleContext.EnableRule(x => x.IsRuleC0030Active);
					ruleContext.EnableRule(x => x.IsRuleB1836Active);

					AssertNoMessageError("TP: ON, C0030: Active, B1836: Active, CL112 code: true, Custom office for transit is not required", countryOfRouting.CY_DataInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);

					ruleContext.DisableRule(x => x.IsRuleC0030Active);
					ruleContext.EnableRule(x => x.IsRuleB1836Active);

					AssertNoMessageError("TP: ON, C0030: Inactive, B1836: Active, CL112 code: true, Custom office for transit is not required", countryOfRouting.CY_DataInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);
				});

				UniversalReferenceTestDataHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
				{
					ruleContext.EnableRule(x => x.IsRuleC0030Active);
					ruleContext.DisableRule(x => x.IsRuleB1836Active);

					countryOfRouting.CY_Data = "PL301080";
					AssertHasMessageError("TP: OFF, C0030: Active, B1836: Inactive, CL112 code: true, Custom office for transit is not entered", countryOfRouting.CY_DataInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);

					countryOfRouting.CY_Data = "PL301070";
					AssertNoMessageError("TP: OFF, C0030: Active, B1836: Inactive, CL112 code: false, Custom office for transit is not required", countryOfRouting.CY_DataInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);

					var movementHeader = nctsHeader.MovementHeader;
					var transitOffice = movementHeader.CustomsOfficesForDeparture.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
					transitOffice.CY_Data = "TraOffic";
					countryOfRouting.CY_Data = "PL301080";
					AssertNoMessageError("TP: OFF, C0030: Active, B1836: Inactive, CL112 code: true, Custom office for transit exists", countryOfRouting.CY_DataInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);

					movementHeader.CustomsOfficesForDeparture.RemoveAndDeleteAll();

					ruleContext.DisableRule(x => x.IsRuleC0030Active);
					ruleContext.DisableRule(x => x.IsRuleB1836Active);

					AssertNoMessageError("TP: OFF, C0030: Inactive, B1836: Inactive, CL112 code: true, Custom office for transit is not required", countryOfRouting.CY_DataInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);

					ruleContext.EnableRule(x => x.IsRuleC0030Active);
					ruleContext.EnableRule(x => x.IsRuleB1836Active);

					countryOfRouting.CY_Data = "PL301080";
					AssertHasMessageError("TP: OFF, C0030: Active, B1836: Active, CL112 code: true, Custom office for transit is not entered", countryOfRouting.CY_DataInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);

					countryOfRouting.CY_Data = "PL301070";
					AssertNoMessageError("TP: OFF, C0030: Active, B1836: Active, CL112 code: false, Custom office for transit is not required", countryOfRouting.CY_DataInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);

					transitOffice = movementHeader.CustomsOfficesForDeparture.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
					transitOffice.CY_Data = "TraOffic";
					countryOfRouting.CY_Data = "PL301080";
					AssertNoMessageError("TP: OFF, C0030: Active, B1836: Active, CL112 code: true, Custom office for transit exists", countryOfRouting.CY_DataInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);

					movementHeader.CustomsOfficesForDeparture.RemoveAndDeleteAll();

					ruleContext.DisableRule(x => x.IsRuleC0030Active);
					ruleContext.EnableRule(x => x.IsRuleB1836Active);

					AssertNoMessageError("TP: OFF, C0030: Inactive, B1836: Active, CL112 code: true, Custom office for transit is not required", countryOfRouting.CY_DataInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			countryOfRouting = nctsHeader.CountriesOfRouting.AddNew();
		}

		NctsHeader nctsHeader;
		CountryOfRouting countryOfRouting;
	}
}
