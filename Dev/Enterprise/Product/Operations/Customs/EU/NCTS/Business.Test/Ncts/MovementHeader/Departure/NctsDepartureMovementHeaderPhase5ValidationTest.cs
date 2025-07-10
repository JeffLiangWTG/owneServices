using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using Moq.Protected;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.CountryCodes;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes;
using AuthorizationCodes = Enterprise.Customs.Business.CusAuthorizationHeaderTypeList.Codes;
using RefCusTradeGroupCodes = Enterprise.Core.Constants.Customs.Universal.RefCusTradeGroup.Codes;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class NctsDepartureMovementHeaderPhase5ValidationTest : TestCaseWithFactory
{
	public void TestCheckBM_PortOfPresentationCode_MandatoryWhenTypeOfSecurityIsNotNON_NR0036() => CombineAssertions(() =>
	{
		const string msgError = "[NR0036] You have not entered a Country/Region Code or an UNLOCO for Place of Loading";

		testContext.DisableRule(c => c.IsRuleNR0036Active);
		var departureMovement = CreateDepartureMovement();

		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
		departureMovement.BM_PortOfPresentationCode = string.Empty;
		AssertNoMessageError("Rule is disabled", departureMovement.BM_PortOfPresentationCodeInfo, msgError);

		testContext.EnableRule(c => c.IsRuleNR0036Active);
		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(departureMovement.BM_PortOfPresentationCodeInfo, msgError);

		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
		departureMovement.BM_PortOfPresentationCode = string.Empty;
		AssertNoMessageError("Type of Security is 'NON'", departureMovement.BM_PortOfPresentationCodeInfo, msgError);
	});

	public void TestCheckBM_PortOfPresentationCode_Mandatory_WhenSimplifiedProcedureIsFalse_NR0088() => CombineAssertions(() =>
	{
		const string msgError = "[NR0088] You have not entered a Country/Region Code or an UNLOCO for Place of Loading.";

		testContext.DisableRule(c => c.IsRuleNR0088Active);
		var departureMovement = CreateDepartureMovement();

		departureMovement.IsSimplifiedNctsProcedure = false;
		departureMovement.BM_PortOfPresentationCode = string.Empty;
		AssertNoMessageError("Rule is disabled", departureMovement.BM_PortOfPresentationCodeInfo, msgError);

		testContext.EnableRule(c => c.IsRuleNR0088Active);
		departureMovement.IsSimplifiedNctsProcedure = false;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(departureMovement.BM_PortOfPresentationCodeInfo, msgError);

		departureMovement.IsSimplifiedNctsProcedure = true;
		departureMovement.BM_PortOfPresentationCode = string.Empty;
		AssertNoMessageError("Simplified Procedure is false", departureMovement.BM_PortOfPresentationCodeInfo, msgError);
	});

	public void TestCheckBM_GONumber_G0114Rule()
	{
		const string messageError = "[G0114] An ACR authorization is required during Simplified Procedure.";
		testContext.DisableRule(x => x.IsRuleG0114Active);

		var departureMovement = CreateDepartureMovement();
		departureMovement.IsSimplifiedNctsProcedure = true;

		departureMovement.Validation.ValidateBM_GONumber();
		AssertNoMessageError("When has ACR and IsSimplifiedNctsProcedure is Simplified Procedure", departureMovement.IsSimplifiedNctsProcedureInfo, messageError);

		testContext.EnableRule(x => x.IsRuleG0114Active);
		departureMovement.Validation.ValidateBM_GONumber();
		AssertHasMessageError("When No ACR and IsSimplifiedNctsProcedure is Simplified Procedure", departureMovement.IsSimplifiedNctsProcedureInfo, messageError);

		departureMovement.CusAuthorizationUsages.AddNew().AGC_Code = "ACR";
		departureMovement.Validation.ValidateBM_GONumber();
		AssertNoMessageError("When has ACR and IsSimplifiedNctsProcedure is Simplified Procedure", departureMovement.IsSimplifiedNctsProcedureInfo, messageError);
	}

	public void TestCheckBM_TOLCarrierID_Mandatory_RuleB1838()
	{
		const string message = "[B1838] Transport ID can't be empty.";

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		testContext.EnableRule(x => x.IsRuleB1838Active);
		var departureMovement = CreateDepartureMovement();

		CombineAssertions("When TP: ON, RuleB1838: Active", () =>
		{
			departureMovement.BM_TOLCarrierID = ZString.Empty;

			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			departureMovement.BM_RN_NKTOLCarrierNationality = "PL";
			departureMovement.Validation.ValidateBM_TOLCarrierID();
			AssertHasMessageError("BM_TOLCarrierID is empty, BM_ExportTransportMode = 2 and BM_RN_NKTOLCarrierNationality is not empty.", departureMovement.BM_TOLCarrierIDInfo, message);

			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			departureMovement.BM_RN_NKTOLCarrierNationality = "PL";
			departureMovement.Validation.ValidateBM_TOLCarrierID();
			AssertHasMessageError("BM_TOLCarrierID is empty, BM_ExportTransportMode != 2 and BM_RN_NKTOLCarrierNationality is not empty.", departureMovement.BM_TOLCarrierIDInfo, message);

			departureMovement.BM_RN_NKTOLCarrierNationality = ZString.Empty;
			departureMovement.Validation.ValidateBM_TOLCarrierID();
			AssertNoMessageError("BM_TOLCarrierID is empty, BM_ExportTransportMode != 2 but BM_RN_NKTOLCarrierNationality is empty.", departureMovement.BM_TOLCarrierIDInfo, message);

			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			AssertHasMessageError("BM_TOLCarrierID is empty, BM_ExportTransportMode = 2 but BM_RN_NKTOLCarrierNationality is empty.", departureMovement.BM_TOLCarrierIDInfo, message);

			foreach (var mode in new[] { ModeOfTransportList.Codes._1_SeaTransport, ModeOfTransportList.Codes._2_RailTransport })
			{
				foreach (var nationality in new[] { string.Empty, "PL" })
				{
					departureMovement.BM_ExportTransportMode = mode;
					departureMovement.BM_RN_NKTOLCarrierNationality = nationality;
					departureMovement.BM_TOLCarrierID = NctsTransportTypeOfIdList.Codes._10;
					departureMovement.Validation.ValidateBM_TOLCarrierID();
					AssertNoMessageError($"BM_TOLCarrierID is not empty, BM_ExportTransportMode = {mode} and BM_RN_NKTOLCarrierNationality is '{nationality}'.", departureMovement.BM_TOLCarrierIDInfo, message);
				}
			}
		});

		testContext.DisableRule(x => x.IsRuleB1838Active);
		departureMovement.BM_TOLCarrierID = ZString.Empty;

		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
		departureMovement.BM_RN_NKTOLCarrierNationality = "PL";
		departureMovement.Validation.ValidateBM_TOLCarrierID();
		AssertNoMessageError("When TP: ON, RuleB1838: Not Active, BM_TOLCarrierID is empty, BM_ExportTransportMode = 2 and BM_RN_NKTOLCarrierNationality is not empty.", departureMovement.BM_TOLCarrierIDInfo, message);

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		testContext.EnableRule(x => x.IsRuleB1838Active);
		departureMovement = CreateDepartureMovement();
		departureMovement.BM_TOLCarrierID = ZString.Empty;

		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
		departureMovement.BM_RN_NKTOLCarrierNationality = "PL";
		departureMovement.Validation.ValidateBM_TOLCarrierID();
		AssertNoMessageError("When TP: OFF, RuleB1838: Active, BM_TOLCarrierID is empty: BM_ExportTransportMode = 2 and BM_RN_NKTOLCarrierNationality is not empty.", departureMovement.BM_TOLCarrierIDInfo, message);
	}

	public void TestCheckBM_GONumber_NR0007Rule()
	{
		const string messageError = "[NR0007] You have not entered any Containers/Equipment.";

		testContext.EnableRule(decider => decider.IsRuleNR0007Active);

		var departureMovement = CreateDepartureMovement();

		departureMovement.IsSimplifiedNctsProcedure = true;
		AssertHasMessageError("IsSimplifiedNctsProcedure is true, no Containers/Equipment", departureMovement.IsSimplifiedNctsProcedureInfo, messageError);
		departureMovement.IsSimplifiedNctsProcedure = false;
		AssertNoMessageError("IsSimplifiedNctsProcedure is false, no Containers/Equipment", departureMovement.IsSimplifiedNctsProcedureInfo, messageError);

		departureMovement.Header.DepartureHeaderContainers.AddNew();
		departureMovement.IsSimplifiedNctsProcedure = true;
		AssertNoMessageError("IsSimplifiedNctsProcedure is true, has Containers/Equipment", departureMovement.IsSimplifiedNctsProcedureInfo, messageError);

		testContext.DisableRule(decider => decider.IsRuleNR0007Active);
		departureMovement.Header.DepartureHeaderContainers.RemoveAndDeleteAll();

		AssertNoMessageError("IsSimplifiedNctsProcedure is true, no Containers/Equipment, rule disabled", departureMovement.IsSimplifiedNctsProcedureInfo, messageError);
	}

	public void TestCheckBM_RN_NKCountryOfDispatch_E1301()
	{
		const string message = "[E1301] In transition period, which is now, Country of Dispatch must be empty";

		var departureMovement = CreateDepartureMovement();
		var targetInfo = departureMovement.BM_RN_NKCountryOfDispatchInfo;

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		testContext.EnableRule(decider => decider.IsRuleE1301Active);
		CombineAssertions(() =>
		{
			departureMovement.BM_RN_NKCountryOfDispatch = string.Empty;
			AssertNoMessageError("Dispatch Country not specified", targetInfo, message);

			departureMovement.BM_RN_NKCountryOfDispatch = China;
			AssertHasMessageError("Dispatch Country specified on movement header level", targetInfo, message);
		});

		testContext.DisableRule(decider => decider.IsRuleE1301Active);
		CombineAssertions(() =>
		{
			departureMovement.BM_RN_NKCountryOfDispatch = string.Empty;
			AssertNoMessageError("Dispatch Country not specified", targetInfo, message);

			departureMovement.BM_RN_NKCountryOfDispatch = China;
			AssertNoMessageError("Dispatch Country specified on movement header level", targetInfo, message);
		});

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		testContext.EnableRule(decider => decider.IsRuleE1301Active);
		CombineAssertions(() =>
		{
			departureMovement.BM_RN_NKCountryOfDispatch = string.Empty;
			AssertNoMessageError("Dispatch Country not specified", targetInfo, message);

			departureMovement.BM_RN_NKCountryOfDispatch = China;
			AssertNoMessageError("Dispatch Country specified on movement header level", targetInfo, message);
		});

		testContext.DisableRule(decider => decider.IsRuleE1301Active);
		CombineAssertions(() =>
		{
			departureMovement.BM_RN_NKCountryOfDispatch = string.Empty;
			AssertNoMessageError("Dispatch Country not specified", targetInfo, message);

			departureMovement.BM_RN_NKCountryOfDispatch = China;
			AssertNoMessageError("Dispatch Country specified on movement header level", targetInfo, message);
		});
	}

	public void TestCheckRuleC0101_1() => CombineAssertions(() =>
	{
		const string messageError =
			"[C0101, R0859] Reduced Dataset Indicator to be true requires an Authorization of Type 'TRD' (Transit Reduced Dataset).";

		testContext.EnableRule(decider => decider.IsRuleC0101_1Active);

		var departureMovement = CreateDepartureMovement();

		departureMovement.BM_ReducedDatasetIndicator = true;
		AssertHasMessageError("BM_ReducedDatasetIndicator is true, no Authorizations", departureMovement.BM_ReducedDatasetIndicatorInfo, messageError);
		departureMovement.BM_ReducedDatasetIndicator = false;
		AssertNoMessageError("BM_ReducedDatasetIndicator is false, no Authorizations", departureMovement.BM_ReducedDatasetIndicatorInfo, messageError);

		foreach (var (code, reducedDatasetIndicator, shouldHaveError) in GetTestCases())
		{
			departureMovement.CusAuthorizationUsages.RemoveAndDeleteAll();
			var cusAuthorizationUsage = departureMovement.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Code = code;
			departureMovement.BM_ReducedDatasetIndicator = reducedDatasetIndicator;

			if (shouldHaveError)
			{
				AssertHasMessageError($"When code is {code} and BM_ReducedDatasetIndicator is {reducedDatasetIndicator}", departureMovement.BM_ReducedDatasetIndicatorInfo, messageError);
			}
			else
			{
				AssertNoMessageError($"When code is {code} and BM_ReducedDatasetIndicator is {reducedDatasetIndicator}", departureMovement.BM_ReducedDatasetIndicatorInfo, messageError);
			}
		}

		testContext.DisableRule(decider => decider.IsRuleC0101_1Active);

		departureMovement.CusAuthorizationUsages.RemoveAndDeleteAll();

		departureMovement.BM_ReducedDatasetIndicator = true;
		AssertNoMessageError("Rule disabled, BM_ReducedDatasetIndicator is true, no Authorizations", departureMovement.BM_ReducedDatasetIndicatorInfo, messageError);
		departureMovement.BM_ReducedDatasetIndicator = false;
		AssertNoMessageError("Rule disabled, BM_ReducedDatasetIndicator is false, no Authorizations", departureMovement.BM_ReducedDatasetIndicatorInfo, messageError);

		IEnumerable<(string Code, bool ReducedDatasetIndicator, bool ShouldHaveError)> GetTestCases()
		{
			yield return (AuthorizationCodes.TransitReducedDataset, true, false);
			yield return (AuthorizationCodes.TransitReducedDataset, false, false);
			foreach (var code in new CusAuthorizationHeaderTypeList().GetAllCodes()
																.Except(AuthorizationCodes.TransitReducedDataset))
			{
				yield return (code, true, true);
				yield return (code, false, false);
			}
		}
	});

	public void TestCheckTirCarnetNumberCoreRuleR0990()
	{
		const string errorMessage = "(R0990) TIR Carnet Number can have a format of an10 or an11.";

		testContext.EnableRule(p => p.IsRuleR0990Active);

		var departureMovement = CreateDepartureMovement();
		var targetInfo = departureMovement.TirCarnetNumberInfo;
		departureMovement.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
		departureMovement.TirCarnetNumber = "1234567890";
		AssertNoMessageErrorContaining("an10 will not raise error", targetInfo, errorMessage);
		departureMovement.TirCarnetNumber = "123456789";
		AssertHasMessageErrorContaining("an9 will raise error", targetInfo, errorMessage);
		departureMovement.TirCarnetNumber = "12345678901";
		AssertNoMessageErrorContaining("an11 will not raise error", targetInfo, errorMessage);
		departureMovement.TirCarnetNumber = "123456789012";
		AssertHasMessageErrorContaining("an12 will raise error", targetInfo, errorMessage);

		var newFactory = new BusinessObjectFactory();
		testContext.Factory = newFactory;

		var nctsHeader = newFactory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		testContext.AddAutoCacheResetObject(nctsHeader);
		var newFactoryDepartureMovement = nctsHeader.MovementHeader;
		testContext.AddAutoCacheResetObject(newFactoryDepartureMovement);
		var newTargetInfo = newFactoryDepartureMovement.TirCarnetNumberInfo;
		testContext.AddAutoCacheResetObject(nctsHeader);
		testContext.AddAutoCacheResetObject(newFactoryDepartureMovement);

		testContext.DisableRule(p => p.IsRuleR0990Active);

		newFactoryDepartureMovement.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
		newFactoryDepartureMovement.TirCarnetNumber = "1234567890";
		AssertNoMessageErrorContaining("Inactive rule will not raise error", newTargetInfo, errorMessage);
		newFactoryDepartureMovement.TirCarnetNumber = "123456789";
		AssertNoMessageErrorContaining("Inactive rule will not raise error", newTargetInfo, errorMessage);
		newFactoryDepartureMovement.TirCarnetNumber = "12345678901";
		AssertNoMessageErrorContaining("Inactive rule will not raise error", newTargetInfo, errorMessage);
		newFactoryDepartureMovement.TirCarnetNumber = "123456789012";
		AssertNoMessageErrorContaining("Inactive rule will not raise error", newTargetInfo, errorMessage);
	}

	public void TestCheckBM_ReducedDatasetIndicator_R350()
	{
		const string errorMessage = "[R0350, R0352] An Authorization for Code=TRD is required when Reduced Data Set Indicator is Yes and Inland M.O.T. is one of these - 1(Sea Transport) or 2(Rail Transport) or 4(Air Transport).";

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;

		var movementHeader = nctsHeader.MovementHeader;
		movementHeader.BM_ReducedDatasetIndicator = false;
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;

		CombineAssertions(() =>
		{
			AssertNoMessageError("Initial state without error", movementHeader.BM_ReducedDatasetIndicatorInfo, errorMessage);
			movementHeader.BM_ReducedDatasetIndicator = true;
			AssertHasMessageError("Setter of BM_ReducedDatasetIndicator triggers check for rule R350", movementHeader.BM_ReducedDatasetIndicatorInfo, errorMessage);
		});
	}

	public void TestCheckBM_TypeOfSecurity_MandatoryValidation()
	{
		var departureMovement = CreateDepartureMovement();

		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(departureMovement.BM_TypeOfSecurityInfo);
	}

	public void TestCheckBM_TypeOfSecurity_InvalidCodeValidation()
	{
		var departureMovement = CreateDepartureMovement();

		ValidationTestHelper.AssertErrorIfInvalidCode(departureMovement.BM_TypeOfSecurityInfo, "XYZ", NctsTypeOfSecurityList.Codes.BTH);
	}

	public void TestCheckBM_AircraftIDAtDeparture_MutuallyExclusive_TransportAtDeparture()
	{
		var departureMovement = CreateDepartureMovement();

		const string message = "You may only enter either a Flight Number or an Aircraft ID.";
		var targetInfo = departureMovement.BM_AircraftIDAtDepartureInfo;
		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;

		CombineAssertions(() =>
		{
			departureMovement.BM_AircraftIDAtDeparture = "Aircraft 123";
			AssertNoMessageError("Only Aircraft ID is entered", targetInfo, message);

			departureMovement.BM_TransportAtDeparture = "Flight 123";
			departureMovement.BM_AircraftIDAtDeparture = "Aircraft 456";
			AssertHasMessageError("Both Aircraft ID and Flight Number are entered", targetInfo, message);
		});
	}

	public void TestCheckBM_AircraftIDAtDeparture_Mandatory_NR0031() => CombineAssertions(() =>
	{
		const string messageError = "[NR0031] You have not entered an Aircraft Identification.";

		testContext.EnableRule(c => c.IsRuleNR0031Active);

		var departureMovement = CreateDepartureMovement();
		departureMovement.Validation.ValidateBM_AircraftIDAtDeparture();
		AssertNoMessageError("Initial state", departureMovement.BM_AircraftIDAtDepartureInfo, messageError);

		departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._4_AirTransport;
		departureMovement.Validation.ValidateBM_AircraftIDAtDeparture();
		AssertHasMessageError("InlandTransportModeAtDeparture =  4", departureMovement.BM_AircraftIDAtDepartureInfo, messageError);

		departureMovement.BM_AircraftIDAtDeparture = "123";
		AssertNoMessageError("BM_AircraftIDAtDeparture not empty", departureMovement.BM_AircraftIDAtDepartureInfo, messageError);

		testContext.DisableRule(c => c.IsRuleNR0031Active);
		departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._4_AirTransport;
		departureMovement.BM_AircraftIDAtDeparture = ZString.Empty;
		AssertNoMessageError("BM_AircraftIDAtDeparture: no NR0031 message when rule disabled.", departureMovement.BM_AircraftIDAtDepartureInfo, messageError);
	});

	public void TestCheckBM_TransportAtDeparture_LLoydNumberRequired_RuleB1892Active()
	{
		var departureMovement = CreateDepartureMovement();
		var expectedMessageError = $"[B1892] In transition period, which is now, if no containers are used '{departureMovement.TransportAtDepartureInfo.HumanReadableName}' field must be filled in";

		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		departureMovement.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._10;

		testContext.EnableRule(c => c.IsRuleB1892Active);

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		testContext.EnableRule(c => c.IsRuleB1892Active);

		departureMovement.BM_TransportAtDeparture = "Zd10L";
		AssertNoMessageErrorContaining("In TP, Lloyds Number is filled with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		departureMovement.BM_TransportAtDeparture = ZString.Empty;
		AssertHasMessageErrorContaining("In TP, Lloyds Number is empty with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		var container = departureMovement.Header.DepartureHeaderContainers.AddNew();
		container.BC_Mode = ContainerModes.Containerised;
		departureMovement.Validation.ValidateBM_TransportAtDeparture();

		AssertNoMessageErrorContaining("In TP, Lloyds Number is empty and HeaderContainers collection has a containerised entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		AssertNoMessageErrorContaining("Outside TP,Lloyds Number is empty with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);
	}

	public void TestCheckBM_TransportAtDeparture_VesselNameRequired_InSeaTransport_RuleB1892Active()
	{
		var departureMovement = CreateDepartureMovement();
		var expectedMessageError = $"[B1892] In transition period, which is now, if no containers are used '{departureMovement.TransportAtDepartureInfo.HumanReadableName}' field must be filled in";

		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		departureMovement.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._11;

		testContext.EnableRule(c => c.IsRuleB1892Active);
		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);

		departureMovement.BM_TransportAtDeparture = "Zd10L";
		AssertNoMessageErrorContaining("In TP,Vessel name is filled with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		departureMovement.BM_TransportAtDeparture = ZString.Empty;
		AssertHasMessageErrorContaining("In TP, Vessel name is empty with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		var container = departureMovement.Header.DepartureHeaderContainers.AddNew();
		container.BC_Mode = ContainerModes.Containerised;
		departureMovement.Validation.ValidateBM_TransportAtDeparture();

		AssertNoMessageErrorContaining("In TP, Vessel name is empty and HeaderContainers collection has a containerised entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		AssertNoMessageErrorContaining("Outside TP, Vessel name is empty with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);
	}

	public void TestCheckBM_TransportAtDeparture_WagonNumberRequired_RuleB1892Active()
	{
		var departureMovement = CreateDepartureMovement();
		var expectedMessageError = $"[B1892] In transition period, which is now, if no containers are used '{departureMovement.TransportAtDepartureInfo.HumanReadableName}' field must be filled in";

		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
		departureMovement.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._20;

		testContext.EnableRule(c => c.IsRuleB1892Active);
		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);

		departureMovement.BM_TransportAtDeparture = "Zd10L";
		AssertNoMessageErrorContaining("In TP, Wagon number is filled with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		departureMovement.BM_TransportAtDeparture = ZString.Empty;
		AssertHasMessageErrorContaining("In TP, Wagon number is empty with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		var container = departureMovement.Header.DepartureHeaderContainers.AddNew();
		container.BC_Mode = ContainerModes.Containerised;
		departureMovement.Validation.ValidateBM_TransportAtDeparture();

		AssertNoMessageErrorContaining("In TP, Wagon number is empty and HeaderContainers collection has a containerised entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		AssertNoMessageErrorContaining("Outside TP, Wagon number is empty with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);
	}

	public void TestCheckBM_TransportAtDeparture_TrainNumberRequired_RuleB1892Active()
	{
		var departureMovement = CreateDepartureMovement();
		var expectedMessageError = $"[B1892] In transition period, which is now, if no containers are used '{departureMovement.TransportAtDepartureInfo.HumanReadableName}' field must be filled in";

		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
		departureMovement.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._21;

		testContext.EnableRule(c => c.IsRuleB1892Active);

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		departureMovement.BM_TransportAtDeparture = "Cfls3P";
		AssertNoMessageErrorContaining("In TP, Train number is filled with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		departureMovement.BM_TransportAtDeparture = ZString.Empty;
		AssertHasMessageErrorContaining("In TP, Train number is empty with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		var container = departureMovement.Header.DepartureHeaderContainers.AddNew();
		container.BC_Mode = ContainerModes.Containerised;
		departureMovement.Validation.ValidateBM_TransportAtDeparture();

		AssertNoMessageErrorContaining("In TP, Train number is empty and HeaderContainers collection has a containerised entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		AssertNoMessageErrorContaining("Outside TP, Train number is empty with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);
	}

	public void TestCheckBM_TransportAtDeparture_TransportIDRequired_InRoadTransport_RuleB1892Active()
	{
		var departureMovement = CreateDepartureMovement();
		var expectedMessageError = $"[B1892] In transition period, which is now, if no containers are used '{departureMovement.TransportAtDepartureInfo.HumanReadableName}' field must be filled in";

		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
		departureMovement.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._30;

		testContext.EnableRule(c => c.IsRuleB1892Active);
		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		departureMovement.BM_TransportAtDeparture = "Cfls3P";
		AssertNoMessageErrorContaining("In TP, Transport ID is filled with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		departureMovement.BM_TransportAtDeparture = ZString.Empty;
		AssertHasMessageErrorContaining("In TP, Transport ID is empty with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		var container = departureMovement.Header.DepartureHeaderContainers.AddNew();
		container.BC_Mode = ContainerModes.Containerised;
		departureMovement.Validation.ValidateBM_TransportAtDeparture();

		AssertNoMessageErrorContaining("In TP, Transport ID is empty and HeaderContainers collection has a containerised entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		AssertNoMessageErrorContaining("Outside TP, Transport ID is empty with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);
	}

	public void TestCheckBM_TransportAtDeparture_FlightNumberRequired_RuleB1892Active()
	{
		var departureMovement = CreateDepartureMovement();
		var expectedMessageError = $"[B1892] In transition period, which is now, if no containers are used '{departureMovement.TransportAtDepartureInfo.HumanReadableName}' field must be filled in";

		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
		departureMovement.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._40;

		testContext.EnableRule(c => c.IsRuleB1892Active);
		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);

		departureMovement.BM_TransportAtDeparture = "Cfls3P";
		AssertNoMessageErrorContaining("In TP, Flight Number is filled with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		departureMovement.BM_TransportAtDeparture = ZString.Empty;
		AssertHasMessageErrorContaining("In TP, Flight Number is empty with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		var container = departureMovement.Header.DepartureHeaderContainers.AddNew();
		container.BC_Mode = ContainerModes.Containerised;
		departureMovement.Validation.ValidateBM_TransportAtDeparture();

		AssertNoMessageErrorContaining("In TP, Flight Number is empty and HeaderContainers collection has a containerised entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		AssertNoMessageErrorContaining("Outside TP, Flight Number is empty with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);
	}

	public void TestCheckBM_TransportAtDeparture_RegistrationNumberRequired_RuleB1892Active()
	{
		var departureMovement = CreateDepartureMovement();
		var expectedMessageError = $"[B1892] In transition period, which is now, if no containers are used '{departureMovement.TransportAtDepartureInfo.HumanReadableName}' field must be filled in";

		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
		departureMovement.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._41;

		testContext.EnableRule(c => c.IsRuleB1892Active);

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		departureMovement.BM_TransportAtDeparture = "Cfls3P";
		AssertNoMessageErrorContaining("In TP, Registration No. is filled with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		departureMovement.BM_TransportAtDeparture = ZString.Empty;
		AssertHasMessageErrorContaining("In TP, Registration No. is empty with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		var container = departureMovement.Header.DepartureHeaderContainers.AddNew();
		container.BC_Mode = ContainerModes.Containerised;
		departureMovement.Validation.ValidateBM_TransportAtDeparture();

		AssertNoMessageErrorContaining("In TP, Registration No is empty and HeaderContainers collection has a containerised entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		AssertNoMessageErrorContaining("Outside TP, Registration No. is empty with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);
	}

	public void TestCheckBM_TransportAtDeparture_ENICodeRequired_RuleB1892Active()
	{
		var departureMovement = CreateDepartureMovement();
		var expectedMessageError = $"[B1892] In transition period, which is now, if no containers are used '{departureMovement.TransportAtDepartureInfo.HumanReadableName}' field must be filled in";

		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
		departureMovement.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._80;

		testContext.EnableRule(c => c.IsRuleB1892Active);

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		departureMovement.BM_TransportAtDeparture = "Ax013Y";
		AssertNoMessageErrorContaining("In TP, ENI Code is filled with no containerized entry", departureMovement.BM_TransportAtDepartureInfo, expectedMessageError);

		departureMovement.BM_TransportAtDeparture = ZString.Empty;
		AssertHasMessageErrorContaining("In TP, ENI Code is empty with no containerized entry", departureMovement.BM_TransportAtDepartureInfo, expectedMessageError);

		var container = departureMovement.Header.DepartureHeaderContainers.AddNew();
		container.BC_Mode = ContainerModes.Containerised;
		departureMovement.Validation.ValidateBM_TransportAtDeparture();

		AssertNoMessageErrorContaining("In TP, ENI Code is empty and HeaderContainers collection has a containerised entry", departureMovement.BM_TransportAtDepartureInfo, expectedMessageError);

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		AssertNoMessageErrorContaining("Outside TP, ENI Code is empty with no containerized entry", departureMovement.BM_TransportAtDepartureInfo, expectedMessageError);
	}

	public void TestCheckBM_TransportAtDeparture_VesselNameRequired_InInlandWterways_RuleB1892Active()
	{
		var departureMovement = CreateDepartureMovement();
		var expectedMessageError = $"[B1892] In transition period, which is now, if no containers are used '{departureMovement.TransportAtDepartureInfo.HumanReadableName}' field must be filled in";

		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
		departureMovement.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._81;

		testContext.EnableRule(c => c.IsRuleB1892Active);

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		departureMovement.BM_TransportAtDeparture = "Ax013Y";
		AssertNoMessageErrorContaining("In TP, Vessel name is filled with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		departureMovement.BM_TransportAtDeparture = ZString.Empty;
		AssertHasMessageErrorContaining("In TP, Vessel name is empty with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		var container = departureMovement.Header.DepartureHeaderContainers.AddNew();
		container.BC_Mode = ContainerModes.Containerised;
		departureMovement.Validation.ValidateBM_TransportAtDeparture();

		AssertNoMessageErrorContaining("In TP, Vessel name is empty and HeaderContainers collection has a containerised entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		AssertNoMessageErrorContaining("Outside TP, Vessel name is empty with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);
	}

	public void TestCheckBM_TransportAtDeparture_TransportIDRequired_InOwnPropulsionSystem_RuleB1892Active()
	{
		var departureMovement = CreateDepartureMovement();
		var expectedMessageError = $"[B1892] In transition period, which is now, if no containers are used '{departureMovement.TransportAtDepartureInfo.HumanReadableName}' field must be filled in";

		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;

		testContext.EnableRule(c => c.IsRuleB1892Active);

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		departureMovement.BM_TransportAtDeparture = "Ax013Y";
		AssertNoMessageErrorContaining("In TP, Transport ID  is filled with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		departureMovement.BM_TransportAtDeparture = ZString.Empty;
		AssertHasMessageErrorContaining("In TP, Transport ID is empty with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		var container = departureMovement.Header.DepartureHeaderContainers.AddNew();
		container.BC_Mode = ContainerModes.Containerised;
		departureMovement.Validation.ValidateBM_TransportAtDeparture();

		AssertNoMessageErrorContaining("In TP, Transport ID is empty and HeaderContainers collection has a containerised entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		AssertNoMessageErrorContaining("Outside TP, Transport ID is empty with no containerized entry", departureMovement.TransportAtDepartureInfo, expectedMessageError);
	}

	public void TestCheckBM_TransportAtDeparture_Mandatory_TR0049()
	{
		var testCases = new[]
		{
			NctsTransportTypeOfIdList.Codes._10, NctsTransportTypeOfIdList.Codes._11, NctsTransportTypeOfIdList.Codes._20, NctsTransportTypeOfIdList.Codes._21,
			NctsTransportTypeOfIdList.Codes._30, NctsTransportTypeOfIdList.Codes._40, NctsTransportTypeOfIdList.Codes._41, NctsTransportTypeOfIdList.Codes._80,
			NctsTransportTypeOfIdList.Codes._81
		};

		foreach (var testCase in testCases)
		{
			testContext.DisableRule(decider => decider.IsRuleTR0049Active);
			var departureMovement = CreateDepartureMovement();
			var messageError = $"[TR0049] You have not entered a {departureMovement.BM_TransportAtDepartureInfo.HumanReadableName}.";
			departureMovement.BM_TransportAtDepartureType = testCase;
			AssertNullOrEmpty("Precondition", departureMovement.BM_TransportAtDeparture);

			CombineAssertions($"TransportAtDepartureType = {testCase}", () =>
			{
				departureMovement.Validation.ValidateBM_TransportAtDeparture();
				AssertNoMessageError("Rule TR0049 disabled", departureMovement.BM_TransportAtDepartureInfo, messageError);

				testContext.EnableRule(decider => decider.IsRuleTR0049Active);
				departureMovement.Validation.ValidateBM_TransportAtDeparture();
				AssertHasMessageError("Rule TR0049 enabled", departureMovement.BM_TransportAtDepartureInfo, messageError);

				departureMovement.BM_TransportAtDeparture = "ABC";
				AssertNoMessageError("TransportAtDeparture not empty", departureMovement.BM_TransportAtDepartureInfo, messageError);
				departureMovement.BM_TransportAtDeparture = ZString.Empty;
			});
		}
	}

	public void TestCheckBM_TransportAtDeparture_Mandatory_TR0054() => CombineAssertions(() =>
	{
		const string messageError = "[TR0054] Please capture first Transport ID in field Wagon No./Train No. before using grid Additional Wagon Numbers.";

		testContext.DisableRule(r => r.IsRuleTR0054Active);
		var departureMovement = CreateDepartureMovement();
		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
		departureMovement.AdditionalWagons.AddNew();
		AssertNullOrEmpty("Precondition", departureMovement.BM_TransportAtDeparture);

		departureMovement.Validation.ValidateBM_TransportAtDeparture();
		AssertNoMessageError("Rule TR0054 disabled", departureMovement.BM_TransportAtDepartureInfo, messageError);

		testContext.EnableRule(r => r.IsRuleTR0054Active);
		departureMovement.Validation.ValidateBM_TransportAtDeparture();
		AssertHasMessageError("Rule TR0054 enabled", departureMovement.BM_TransportAtDepartureInfo, messageError);

		departureMovement.BM_TransportAtDeparture = "ABC";
		AssertNoMessageError("TransportAtDeparture not empty", departureMovement.BM_TransportAtDepartureInfo, messageError);

		departureMovement.AdditionalWagons.RemoveAndDeleteAll();
		departureMovement.BM_TransportAtDeparture = ZString.Empty;
		AssertNoMessageError("AdditionalWagons are empty, TransportAtDeparture is empty", departureMovement.BM_TransportAtDepartureInfo, messageError);
	});

	public void TestCheckBM_TransportAtDeparture_Mandatory_NR0031() => CombineAssertions(() =>
	{
		testContext.EnableRule(x => x.IsRuleNR0031Active);
		var departureMovement = CreateDepartureMovement();
		var messageError = $"[NR0031] You have not entered a {departureMovement.TransportAtDepartureInfo.HumanReadableName}.";

		assertTransportAtDeparture(ModeOfTransportList.Codes._2_RailTransport);
		assertTransportAtDeparture(ModeOfTransportList.Codes._3_RoadTransport);
		assertTransportAtDeparture(ModeOfTransportList.Codes._4_AirTransport, departureMovement.AircraftIDAtDepartureInfo);
		assertTransportAtDeparture(ModeOfTransportList.Codes._8_InlandWaterwayTransport);
		assertTransportAtDeparture(ModeOfTransportList.Codes._9_OwnPropulsion);

		void assertTransportAtDeparture(string inlandTransportModeAtDeparture, ZPropertyInfo additionalProperty = null)
		{
			departureMovement.InlandTransportModeAtDeparture = ZString.Empty;
			departureMovement.Validation.ValidateBM_TransportAtDeparture();
			AssertNoMessageError("Initial state", departureMovement.TransportAtDepartureInfo, messageError);

			departureMovement.InlandTransportModeAtDeparture = inlandTransportModeAtDeparture;
			departureMovement.Validation.ValidateBM_TransportAtDeparture();
			AssertHasMessageError($"InlandTransportModeAtDeparture = {inlandTransportModeAtDeparture}", departureMovement.TransportAtDepartureInfo, messageError);

			departureMovement.TransportAtDeparture = "1";
			AssertNoMessageError("TransportAtDeparture not empty", departureMovement.TransportAtDepartureInfo, messageError);

			if (additionalProperty != null)
			{
				departureMovement.TransportAtDeparture = ZString.Empty;
				AssertHasMessageError($"InlandTransportModeAtDeparture = {inlandTransportModeAtDeparture}", departureMovement.TransportAtDepartureInfo, messageError);

				additionalProperty.Value = (ZString)"1";
				departureMovement.Validation.ValidateBM_TransportAtDeparture();
				AssertNoMessageError($"{additionalProperty.HumanReadableName} not empty", departureMovement.TransportAtDepartureInfo, messageError);
			}
		}

		testContext.DisableRule(x => x.IsRuleNR0031Active);
		departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._2_RailTransport;
		departureMovement.TransportAtDeparture = ZString.Empty;
		AssertNoMessageError("BM_TransportAtDeparture: no NR0031 message when rule disabled.", departureMovement.TransportAtDepartureInfo, messageError);
	});

	public void TestCheckBM_ExportDate_WhenActive()
	{
		const string messageError = "[C0839] You have not entered a Date Limit.";
		var departureMovement = CreateDepartureMovement();

		CombineAssertions(() =>
		{
			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0839Active)))
			{
				var validation = departureMovement.Validation;

				departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
				departureMovement.BM_ExportDate = ZDateTime.Empty;
				departureMovement.IsSimplifiedNctsProcedure = true;
				validation.ValidateBM_ExportDate();
				AssertNoMessageError("If the AdditionalDeclarationType is 'D', the export date can be empty", departureMovement.BM_ExportDateInfo, messageError);

				departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
				validation.ValidateBM_ExportDate();
				AssertHasMessageError("If the AdditionalDeclarationType is not 'D' and is simplified NctsProcedure, the export date can not be empty", departureMovement.BM_ExportDateInfo, messageError);

				departureMovement.IsSimplifiedNctsProcedure = false;
				validation.ValidateBM_ExportDate();
				AssertNoMessageError("If is not simplified NctsProcedure, the export date can be empty", departureMovement.BM_ExportDateInfo, messageError);

				departureMovement.BM_ExportDate = ZDateTime.Now;
				departureMovement.IsSimplifiedNctsProcedure = true;
				validation.ValidateBM_ExportDate();
				AssertNoMessageError("If the AdditionalDeclarationType is not 'D', the export date is valid, and then there should be no message error", departureMovement.BM_ExportDateInfo, messageError);
			}
		});
	}

	public void TestCheckBM_ExportDate_WhenNotActive()
	{
		const string messageError = "[C0839] You have not entered a Date Limit.";
		var departureMovement = CreateDepartureMovement();
		var propertyInfo = departureMovement.BM_ExportDateInfo;

		ValidationRuleConfigurationTestHelper.AssertNoNotificationsWithInactiveRule(Factory, propertyInfo,
			messageError, nameof(ValidationRuleConfiguration.IsRuleC0839Active),
			() =>
			{
				departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
				departureMovement.IsSimplifiedNctsProcedure = true;
				departureMovement.BM_ExportDate = ZDateTime.Empty;
			},
			() =>
			{
				departureMovement.BM_ExportDate = ZDateTime.Now;
			});
	}

	public void TestCheckBM_ExportDate_CN839()
	{
		const string messageError = "[CN839] Date Limit must be filled";

		var departureMovement = CreateDepartureMovement();
		var cusAuthorizationUsage = departureMovement.CusAuthorizationUsages.AddNew();

		CombineAssertions(() =>
		{
			testContext.EnableRule(r => r.IsRuleCN839Active);

			var validation = departureMovement.Validation;

			cusAuthorizationUsage.AGC_Code = AuthorizationCodes.AuthorizedConsignorTransit;
			departureMovement.BM_ExportDate = ZDateTime.Empty;
			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;

			validation.ValidateBM_ExportDate();
			AssertNoMessageError("Additional Declaration type is D, export date is empty, AGC code is ACR", departureMovement.BM_ExportDateInfo, messageError);

			departureMovement.BM_ExportDate = DateTime.Now;
			validation.ValidateBM_ExportDate();

			AssertNoMessageError("Additional Declaration type is D, export date is not empty, AGC code is ACR", departureMovement.BM_ExportDateInfo, messageError);

			departureMovement.BM_ExportDate = ZDateTime.Empty;
			cusAuthorizationUsage.AGC_Code = AuthorizationCodes.TransitReducedDataset;

			validation.ValidateBM_ExportDate();
			AssertNoMessageError("Additional Declaration type is D, export date is empty, AGC code is not ACR", departureMovement.BM_ExportDateInfo, messageError);

			departureMovement.BM_ExportDate = ZDateTime.Empty;
			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			cusAuthorizationUsage.AGC_Code = AuthorizationCodes.AuthorizedConsignorTransit;

			validation.ValidateBM_ExportDate();
			AssertHasMessageError("Additional Declaration type is not D, export date is empty, AGC code is ACR", departureMovement.BM_ExportDateInfo, messageError);

			testContext.DisableRule(r => r.IsRuleCN839Active);
			cusAuthorizationUsage.AGC_Code = AuthorizationCodes.AuthorizedConsignorTransit;
			departureMovement.BM_ExportDate = ZDateTime.Empty;
			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;

			validation.ValidateBM_ExportDate();
			AssertNoMessageError("Additional Declaration type is D, export date is empty, AGC code is ACR", departureMovement.BM_ExportDateInfo, messageError);

			departureMovement.BM_ExportDate = DateTime.Now;
			validation.ValidateBM_ExportDate();

			AssertNoMessageError("Additional Declaration type is D, export date is not empty, AGC code is ACR", departureMovement.BM_ExportDateInfo, messageError);

			departureMovement.BM_ExportDate = ZDateTime.Empty;
			cusAuthorizationUsage.AGC_Code = AuthorizationCodes.TransitReducedDataset;

			validation.ValidateBM_ExportDate();
			AssertNoMessageError("Additional Declaration type is D, export date is empty, AGC code is not ACR", departureMovement.BM_ExportDateInfo, messageError);

			departureMovement.BM_ExportDate = ZDateTime.Empty;
			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			cusAuthorizationUsage.AGC_Code = AuthorizationCodes.AuthorizedConsignorTransit;

			validation.ValidateBM_ExportDate();
			AssertNoMessageError("Additional Declaration type is not D, export date is empty, AGC code is ACR", departureMovement.BM_ExportDateInfo, messageError);
		});
	}

	public void TestCheckBM_ExportDate_C0839_1() => CombineAssertions(() =>
	{
		const string messageError = "[C0839-1] You have not entered a Date Limit.";

		testContext.EnableRule(r => r.IsRuleC0839_1Active);

		var departureMovement = CreateDepartureMovement();
		var validation = departureMovement.Validation;

		departureMovement.BM_ExportDate = ZDateTime.Empty;
		departureMovement.IsSimplifiedNctsProcedure = true;
		validation.ValidateBM_ExportDate();
		AssertHasMessageError("IsSimplifiedNctsProcedure, export date is empty", departureMovement.BM_ExportDateInfo, messageError);

		departureMovement.BM_ExportDate = DateTime.Now;
		departureMovement.IsSimplifiedNctsProcedure = true;
		validation.ValidateBM_ExportDate();
		AssertNoMessageError("IsSimplifiedNctsProcedure, export date is filled", departureMovement.BM_ExportDateInfo, messageError);

		departureMovement.BM_ExportDate = ZDateTime.Empty;
		departureMovement.IsSimplifiedNctsProcedure = false;
		validation.ValidateBM_ExportDate();
		AssertNoMessageError("IsSimplifiedNctsProcedure = false, export date is empty", departureMovement.BM_ExportDateInfo, messageError);

		testContext.DisableRule(r => r.IsRuleC0839_1Active);
		departureMovement.BM_ExportDate = ZDateTime.Empty;
		departureMovement.IsSimplifiedNctsProcedure = true;
		validation.ValidateBM_ExportDate();
		AssertNoMessageError("IsSimplifiedNctsProcedure = true, export date is empty, rule is disabled", departureMovement.BM_ExportDateInfo, messageError);
	});

	public void TestCheckBM_ExportDate_TR0092_WhenActive()
	{
		var departureMovement = CreateDepartureMovement();

		const string errorMessage1 = "[TR0092] Date Limit cannot be before Acceptance Date";
		const string errorMessage2 = "[TR0092] Date Limit cannot be in the past";

		testContext.EnableRule(v => v.IsRuleTR0092Active);
		departureMovement.BM_EntryDate = ZDateTime.Empty;
		departureMovement.BM_ExportDate = ZDateTime.Empty;
		AssertNoMessageErrors("When Entry Date is Empty, expected no error for Export Date when it is Empty", departureMovement.BM_ExportDateInfo);

		departureMovement.BM_ExportDate = ZDateTime.Today.AddDays(-1);
		AssertNoMessageError("When Entry Date is Empty, expected error for Export Date when it is less than Today", departureMovement.BM_ExportDateInfo, errorMessage1);
		AssertHasMessageError("When Entry Date is Empty, expected error for Export Date when it is less than Today", departureMovement.BM_ExportDateInfo, errorMessage2);

		departureMovement.BM_ExportDate = ZDateTime.Today;
		AssertNoMessageErrors("When Entry Date is Empty, expected no error for Export Date when it is Empty", departureMovement.BM_ExportDateInfo);

		departureMovement.BM_EntryDate = ZDateTime.Today;
		departureMovement.BM_ExportDate = ZDateTime.Empty;
		AssertNoMessageErrors("When Entry Date is not Empty, expected no error for Export Date when it is Empty", departureMovement.BM_ExportDateInfo);

		departureMovement.BM_ExportDate = departureMovement.BM_EntryDate.AddDays(-1);
		AssertHasMessageError("When Entry Date is not Empty, expected error for Export Date when it is less than Entry Date", departureMovement.BM_ExportDateInfo, errorMessage1);
		AssertNoMessageError("When Entry Date is not Empty, expected error for Export Date when it is less than Entry Date", departureMovement.BM_ExportDateInfo, errorMessage2);

		departureMovement.BM_ExportDate = departureMovement.BM_EntryDate;
		AssertNoMessageError("When Entry Date is not Empty, expected error for Export Date when it is same as Entry Date", departureMovement.BM_ExportDateInfo, errorMessage1);
		AssertNoMessageError("When Entry Date is not Empty, expected error for Export Date when it is same as Entry Date", departureMovement.BM_ExportDateInfo, errorMessage2);

		departureMovement.BM_ExportDate = departureMovement.BM_EntryDate.AddDays(1);
		AssertNoMessageError("When Entry Date is not Empty, expected error for Export Date when it is larger than Entry Date", departureMovement.BM_ExportDateInfo, errorMessage1);
		AssertNoMessageError("When Entry Date is not Empty, expected error for Export Date when it is larger than Entry Date", departureMovement.BM_ExportDateInfo, errorMessage2);
	}

	public void TestCheckBM_ExportDate_TR0092_WhenNotActive()
	{
		testContext.DisableRule(v => v.IsRuleTR0092Active);
		var departureMovement = CreateDepartureMovement();
		departureMovement.BM_EntryDate = ZDateTime.Empty;
		departureMovement.BM_ExportDate = ZDateTime.Today.AddDays(-1);
		AssertNoMessageErrors("No validation when Rule is inactive", departureMovement.BM_ExportDateInfo);

		departureMovement.BM_EntryDate = ZDateTime.Today;
		departureMovement.BM_ExportDate = departureMovement.BM_EntryDate.AddDays(-1);
		AssertNoMessageErrors("No validation when Rule is inactive", departureMovement.BM_ExportDateInfo);
	}

	public void TestCheckBM_TransportAtDepartureTrailer1RegNo_MutuallyExclusive_TransportAtDeparture()
	{
		const string message = "You may only enter either a Train Number or a Wagon Number.";
		var departureMovement = CreateDepartureMovement();
		var targetInfo = departureMovement.BM_TransportAtDepartureTrailer1RegNoInfo;
		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;

		CombineAssertions(() =>
		{
			//Testing validation is not valid when no funcs record
			AssertTrainAndWagonNumberNoErrorMessage(departureMovement.BM_TransportAtDepartureTrailer1RegNoInfo);

			testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
			departureMovement.BM_TransportAtDepartureTrailer1RegNo = "Wagon 123";
			AssertNoMessageError("Only Wagon Number is entered", targetInfo, message);

			departureMovement.BM_TransportAtDeparture = "Train 123";
			departureMovement.BM_TransportAtDepartureTrailer1RegNo = "Wagon 456";
			AssertHasMessageError("Both Train Number and Wagon Number are entered", targetInfo, message);

			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			departureMovement.BM_TransportAtDeparture = "Transport 123";
			departureMovement.BM_TransportAtDepartureTrailer1RegNo = "Trailer 123";
			AssertNoMessageError("Both Train Number and Wagon Number are entered, transport mode <> 2", targetInfo, message);

			testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
			AssertTrainAndWagonNumberNoErrorMessage(departureMovement.BM_TransportAtDepartureTrailer1RegNoInfo);
		});
	}

	public void TestCheckBM_InlandTransportMode_Calls_ValidateBM_ReducedDatasetIndicator()
	{
		const string errorMessage = "[R0350, R0352] An Authorization for Code=TRD is required when Reduced Data Set Indicator is Yes and Inland M.O.T. is one of these - 1(Sea Transport) or 2(Rail Transport) or 4(Air Transport).";

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;

		var movementHeader = nctsHeader.MovementHeader;
		movementHeader.BM_ReducedDatasetIndicator = true;
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;

		CombineAssertions(() =>
		{
			AssertNoMessageError("Initial state without error", movementHeader.BM_ReducedDatasetIndicatorInfo, errorMessage);
			const string codeToRaiseError = ModeOfTransportList.Codes._2_RailTransport;
			movementHeader.BM_InlandTransportMode = codeToRaiseError;
			AssertHasMessageError("Setter of BM_InlandTransportMode triggers ValidateBM_ReducedDatasetIndicator", movementHeader.BM_ReducedDatasetIndicatorInfo, errorMessage);
		});
	}

	public void TestBM_TransportAtDepartureTrailer1RegNo_RequiredWhenAdditionalWagonNumbersEnteredOutsideTransitionPeriod()
	{
		var departureMovement = CreateDepartureMovement();

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
		departureMovement.InlandTransportList.AddNew();
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(departureMovement.BM_TransportAtDepartureTrailer1RegNoInfo, "Wagon Number", "Required when additional wagon numbers are present outside the transition period.");
	}

	public void TestCheckVesselNameAtDeparture_WhenRuleE1103Active()
	{
		const string message = "[E1103] Vessel Name should be less than or equal to 27 Char.";
		const string VesselNameMoreThan27 = "message more than 27 characters";
		const string VesselNameLessThanOrEqual27 = "It a test";

		var departureMovement = CreateDepartureMovement();

		CombineAssertions(() =>
		{
			testContext.EnableRule(v => v.IsRuleE1103Active);
			testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);

			departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
			departureMovement.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._81;
			departureMovement.VesselNameAtDeparture = VesselNameMoreThan27;
			AssertHasMessageError("It would be wrong if the name is more than 27 characters", departureMovement.VesselNameAtDepartureInfo, message);

			departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			departureMovement.VesselNameAtDeparture = VesselNameMoreThan27;
			AssertNoMessageError("There is no notification should be shown", departureMovement.VesselNameAtDepartureInfo, message);

			departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
			departureMovement.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._81;
			departureMovement.VesselNameAtDeparture = VesselNameLessThanOrEqual27;
			AssertNoMessageError("There is no notification should be shown", departureMovement.VesselNameAtDepartureInfo, message);

			testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
			departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
			departureMovement.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._81;
			departureMovement.VesselNameAtDeparture = VesselNameMoreThan27;
			AssertNoMessageError("There is no notification should be shown", departureMovement.VesselNameAtDepartureInfo, message);
		});
	}

	public void TestCheckVesselNameAtDeparture_WhenRuleE1103NotActive()
	{
		const string message = "[E1103] Vessel Name should be less than or equal to 27 Char.";
		const string VesselNameMoreThan27 = "message more than 27 characters";

		testContext.DisableRule(v => v.IsRuleE1103Active);
		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);

		var departureMovement = CreateDepartureMovement();
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
		departureMovement.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._81;
		departureMovement.VesselNameAtDeparture = VesselNameMoreThan27;
		AssertCollectionNotContains("There is no notification to be shown", departureMovement.VesselNameAtDepartureInfo.Notifications.Select(e => e.Message), x => x.Contains(message));
	}

	public void TestCheckBM_RN_NKTransportAtDepartureCountry_Mandatory_TR0050() => CombineAssertions(() =>
	{
		var departureMovement = CreateDepartureMovement();
		var messageError = $"[TR0050] You have not entered a {departureMovement.BM_RN_NKTransportAtDepartureCountryInfo.HumanReadableName}.";

		testContext.DisableRule(decider => decider.IsRuleTR0050Active);
		departureMovement = CreateDepartureMovement();

		departureMovement.BM_TransportAtDeparture = "ABC";
		AssertNullOrEmpty("Precondition", departureMovement.BM_RN_NKTransportAtDepartureCountry);

		departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureCountry();
		AssertNoMessageError("Rule TR0050 disabled", departureMovement.BM_RN_NKTransportAtDepartureCountryInfo, messageError);

		testContext.EnableRule(decider => decider.IsRuleTR0050Active);
		departureMovement = CreateDepartureMovement();
		departureMovement.BM_TransportAtDeparture = "ABC";

		departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureCountry();
		AssertHasMessageError("Rule TR0050 enabled and TransportAtDepartureCountry empty", departureMovement.BM_RN_NKTransportAtDepartureCountryInfo, messageError);

		departureMovement.BM_RN_NKTransportAtDepartureCountry = "DE";
		AssertNoMessageError("Rule TR0050 enabled and TransportAtDepartureCountry not empty ", departureMovement.BM_RN_NKTransportAtDepartureCountryInfo, messageError);
	});

	public void TestCheckBM_RN_NKTransportAtDepartureCountry_Mandatory_NR0035() => CombineAssertions(() =>
	{
		var departureMovement = CreateDepartureMovement();
		var messageError = $"[NR0035] You have not entered a {departureMovement.BM_RN_NKTransportAtDepartureCountryInfo.HumanReadableName}.";

		departureMovement.InlandTransportModeAtDeparture = "3";
		AssertNullOrEmpty("Precondition", departureMovement.BM_RN_NKTransportAtDepartureCountry);

		testContext.DisableRule(decider => decider.IsRuleNR0035Active);
		departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureCountry();
		AssertNoMessageError("Rule NR0035 disabled", departureMovement.BM_RN_NKTransportAtDepartureCountryInfo, messageError);

		testContext.EnableRule(decider => decider.IsRuleNR0035Active);
		departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureCountry();
		AssertHasMessageError("Rule NR0035 enabled and BM_TransportAtDeparture different to 1 or 2 and BM_RN_NKTransportAtDepartureCountry empty", departureMovement.BM_RN_NKTransportAtDepartureCountryInfo, messageError);

		departureMovement.InlandTransportModeAtDeparture = "1";
		departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureCountry();
		AssertNoMessageError("Rule NR0035 enabled and BM_TransportAtDeparture 1 and BM_RN_NKTransportAtDepartureCountry empty", departureMovement.BM_RN_NKTransportAtDepartureCountryInfo, messageError);

		departureMovement.InlandTransportModeAtDeparture = "2";
		departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureCountry();
		AssertNoMessageError("Rule NR0035 enabled and BM_TransportAtDeparture 2 and BM_RN_NKTransportAtDepartureCountry empty", departureMovement.BM_RN_NKTransportAtDepartureCountryInfo, messageError);

		departureMovement.InlandTransportModeAtDeparture = "4";
		departureMovement.BM_RN_NKTransportAtDepartureCountry = "DE";
		AssertNoMessageError("Rule NR0035 enabled and BM_TransportAtDeparture different to 1 or 2 and BM_RN_NKTransportAtDepartureCountry not empty", departureMovement.BM_RN_NKTransportAtDepartureCountryInfo, messageError);
	});

	public void TestCheckBM_AdditionalDeclarationType_ListValidation()
	{
		var departureMovement = CreateDepartureMovement();
		ValidationTestHelper.AssertInvalidCodeMessageError(departureMovement.BM_AdditionalDeclarationTypeInfo, "X", NctsTypeOfAdditionalDeclarationList.Codes.A);
	}

	public void TestCheckBM_AdditionalDeclarationType_Mandatory_TR0017_WhenActive()
	{
		const string messageError = $"{ValidationRuleMessagePrefixes.TR0017}{MandatoryValidation.YouHaveNotEntered}";

		var departureMovement = CreateDepartureMovement();
		CombineAssertions(() =>
		{
			var nctsConfiguration = BuildConfigurationAdditionalDeclarationType(true, true);
			using (ObjectFactory.Substitute("NCTS.NctsConfiguration", nctsConfiguration))
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(departureMovement.BM_AdditionalDeclarationTypeInfo, messageError, "UseAdditionalDeclarationType is true");
			}
			nctsConfiguration = BuildConfigurationAdditionalDeclarationType(false, true);
			using (ObjectFactory.Substitute("NCTS.NctsConfiguration", nctsConfiguration))
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(departureMovement.BM_AdditionalDeclarationTypeInfo, messageError, "UseAdditionalDeclarationType is false");
			}
		});
	}

	public void TestCheckBM_AdditionalDeclarationType_Mandatory_TR0017_WhenInactive()
	{
		var departureMovement = CreateDepartureMovement();
		var nctsConfiguration = BuildConfigurationAdditionalDeclarationType(true, false);
		using (ObjectFactory.Substitute("NCTS.NctsConfiguration", nctsConfiguration))
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(departureMovement.BM_AdditionalDeclarationTypeInfo);
		}
	}

	public void TestCheckBM_AdditionalDeclarationType_NR0073()
	{
		const string messageError = "[NR0073] Location of Goods must be entered if declaration is not a pre-lodged declaration";
		testContext.EnableRule(c => c.IsRuleNR0073Active);

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		var movementHeader = nctsHeader.MovementHeader;
		var propertyInfo = movementHeader.BM_AdditionalDeclarationTypeInfo;

		CombineAssertions(() =>
		{
			movementHeader.BM_AdditionalDeclarationType = "D";
			AssertNoMessageError("Never an error when additional declaration type is D", propertyInfo, messageError);

			movementHeader.BM_AdditionalDeclarationType = "A";
			AssertHasMessageError("When additional declaration type is not D and no location is entered there must be a message error", propertyInfo, messageError);

			movementHeader.GoodsLocation.CGL_Type = "X";
			movementHeader.Validation.ValidateBM_AdditionalDeclarationType();
			AssertNoMessageError("When location CGL_Type is entered, no message error", propertyInfo, messageError);
			movementHeader.GoodsLocation.CGL_Type = "";

			movementHeader.GoodsLocation.CGL_Qualifier = "X";
			movementHeader.Validation.ValidateBM_AdditionalDeclarationType();
			AssertNoMessageError("When location CGL_Qualifier is entered, no message error", propertyInfo, messageError);
			movementHeader.GoodsLocation.CGL_Qualifier = "";

			movementHeader.GoodsLocation.AdditionalIdentifier = "X";
			movementHeader.Validation.ValidateBM_AdditionalDeclarationType();
			AssertNoMessageError("When location AdditionalIdentifier is entered, no message error", propertyInfo, messageError);
			movementHeader.GoodsLocation.AdditionalIdentifier = "";

			movementHeader.GoodsLocation.Address.E2_Postcode = "X";
			movementHeader.Validation.ValidateBM_AdditionalDeclarationType();
			AssertNoMessageError("When location Address.E2_Postcode is entered, no message error", propertyInfo, messageError);
			movementHeader.GoodsLocation.Address.E2_Postcode = "";

			movementHeader.GoodsLocation.Address.E2_RN_NKCountryCode = "X";
			movementHeader.Validation.ValidateBM_AdditionalDeclarationType();
			AssertNoMessageError("When location Address.E2_RN_NKCountryCode is entered, no message error", propertyInfo, messageError);
			movementHeader.GoodsLocation.Address.E2_RN_NKCountryCode = "";

			testContext.DisableRule(c => c.IsRuleNR0073Active);
			movementHeader.Validation.ValidateBM_AdditionalDeclarationType();
			AssertNoMessageError("When the rule is disabled, no message error", propertyInfo, messageError);
		});
	}

	KeyObjectHandleDictionaryObject BuildConfigurationAdditionalDeclarationType(ZBool useAdditionalDeclarationType, ZBool isRuleTR0017Active)
	{
		var departureMovement = CreateDepartureMovement();
		departureMovement.Factory.ClearCachedValue<NctsConfiguration>($"NctsConfiguration_{GlbCompany.CurrentCompany.GC_RN_NKCountryCode}");

		var validationDeciderMock = new Mock<INctsDepartureMovementHeaderPhase5ValidationDecider>() { CallBase = true };
		validationDeciderMock.Setup(x => x.IsRuleTR0017Active).Returns(isRuleTR0017Active);

		var movementHeaderMock = new Mock<MovementHeaderConfiguration>() { CallBase = true };
		movementHeaderMock.Protected().Setup<INctsDepartureMovementHeaderPhase5ValidationDecider>("GetDeparturePhase5ValidationDecider")
						.Returns(validationDeciderMock.Object);

		var nctsConfigurationMock = new Mock<NctsConfiguration>() { CallBase = true };
		nctsConfigurationMock.Protected().Setup<ZBool>("UseAdditionalDeclarationTypeCore").Returns(useAdditionalDeclarationType);
		nctsConfigurationMock.Protected().Setup<MovementHeaderConfiguration>("GetNewMovementHeaderConfiguration")
						.Returns(movementHeaderMock.Object);

		var objectHandleMock = new Mock<ObjectHandle>();
		objectHandleMock.Setup(x => x.GetObject()).Returns(nctsConfigurationMock.Object);

		var nctsConfiguration = new KeyObjectHandleDictionaryObject { { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, objectHandleMock.Object } };
		return nctsConfiguration;
	}

	public void TestCheckBM_TransportAtDepartureType_WhenInvalidCode_ShouldBeError()
	{
		var departureMovement = CreateDepartureMovement();
		ValidationTestHelper.AssertErrorIfInvalidCode(departureMovement.BM_TransportAtDepartureTypeInfo, "AA", NctsTransportTypeOfIdList.Codes._10);
	}

	public void TestCheckBM_TransportAtDepartureType_WhenCodeIsNotInTheListButValid_ShouldBeMessageError_Code_99_OwnPropulsion()
	{
		var departureMovement = CreateDepartureMovement();

		departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._9_OwnPropulsion;
		ValidationTestHelper.AssertInvalidCodeMessageError(departureMovement.BM_TransportAtDepartureTypeInfo,
															"99", NctsTransportTypeOfIdList.Codes._10);
	}

	public void TestCheckBM_TransportAtDepartureType_Mandatory_B1891_1()
	{
		const string message = "[B1891-1] You have not entered a Type of Identification for Transport Departure.";

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		testContext.EnableRule(x => x.IsRuleB1891_1Active);

		var departureMovement = CreateDepartureMovement();
		departureMovement.BM_TransportAtDepartureType = ZString.Empty;
		AssertHasMessageError(departureMovement.BM_TransportAtDepartureTypeInfo, message);

		testContext.DisableRule(x => x.IsRuleB1891_1Active);
		departureMovement.Validation.ValidateBM_TransportAtDepartureType();
		AssertNoMessageError(departureMovement.BM_TransportAtDepartureTypeInfo, message);
		testContext.EnableRule(x => x.IsRuleB1891_1Active);

		departureMovement.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._10;
		AssertNoMessageError(departureMovement.BM_TransportAtDepartureTypeInfo, message);
		departureMovement.BM_TransportAtDepartureType = ZString.Empty;

		var headerContainer = departureMovement.Header.DepartureHeaderContainers.AddNew();
		headerContainer.BC_Mode = ContainerModes.Containerised;
		departureMovement.BM_TransportAtDepartureType = ZString.Empty;
		AssertNoMessageError(departureMovement.BM_TransportAtDepartureTypeInfo, message);
		departureMovement.Header.DepartureHeaderContainers.RemoveAndDeleteAll();

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		AssertEquals("Precondition", false, departureMovement.IsInPhase5TransitionPeriod);
		departureMovement.BM_TransportAtDepartureType = ZString.Empty;
		AssertNoMessageError(departureMovement.BM_TransportAtDepartureTypeInfo, message);
	}

	public void TestCheckBM_TransportAtDepartureType_Mandatory_NR0031() => CombineAssertions(() =>
	{
		const string messageError = "[NR0031] You have not entered a Type of Identification.";
		testContext.EnableRule(x => x.IsRuleNR0031Active);
		var departureMovement = CreateDepartureMovement();

		departureMovement.Validation.ValidateBM_TransportAtDepartureType();
		AssertNoMessageError("Initial state", departureMovement.BM_TransportAtDepartureTypeInfo, messageError);

		departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._9_OwnPropulsion;
		departureMovement.Validation.ValidateBM_TransportAtDepartureType();
		AssertHasMessageError("InlandTransportModeAtDeparture =  9", departureMovement.BM_TransportAtDepartureTypeInfo, messageError);

		departureMovement.BM_TransportAtDepartureType = "12";
		AssertNoMessageError("BM_AircraftIDAtDeparture not empty", departureMovement.BM_TransportAtDepartureTypeInfo, messageError);

		testContext.DisableRule(x => x.IsRuleNR0031Active);
		departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._9_OwnPropulsion;
		departureMovement.BM_TransportAtDepartureType = ZString.Empty;
		AssertNoMessageError("BM_AircraftIDAtDeparture: no NR0031 message when rule disabled.", departureMovement.BM_TransportAtDepartureTypeInfo, messageError);
	});

	public void TestCheckBM_ActiveBorderIdentificationType() => CombineAssertions(() =>
	{
		const string message = "[R0789] You have not entered Transport Border Details.";

		testContext.EnableRule(x => x.IsRuleR0789Active);
		var departureMovement = CreateDepartureMovement();

		ValidationTestHelper.AssertErrorIfInvalidCode(departureMovement.BM_ActiveBorderIdentificationTypeInfo, "AA", NctsTransportTypeOfIdList.Codes._10);

		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		departureMovement.BM_ActiveBorderIdentificationType = ZString.Empty;

		departureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
		AssertNoMessageError("BM_ActiveBorderIdentificationTypeInfo not mandatory", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);

		var office = departureMovement.CustomsOfficesForDeparture.AddNew();
		office.CY_Code = "TRA";
		departureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
		AssertHasMessageError("BM_ActiveBorderIdentificationTypeInfo mandatory when office TRA", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);

		departureMovement.BM_ExportTransportMode = ZString.Empty;
		departureMovement.BM_ActiveBorderIdentificationType = ZString.Empty;
		departureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
		AssertNoMessageError("BM_ActiveBorderIdentificationTypeInfo not mandatory when BM_ExportTransportMode is Empty", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);

		testContext.DisableRule(x => x.IsRuleR0789Active);
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		departureMovement.BM_ActiveBorderIdentificationType = ZString.Empty;
		departureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
		AssertNoMessageError("BM_ActiveBorderIdentificationTypeInfo not mandatory when IsRuleR0789Active disabled", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);
	});

	public void TestCheckBM_ActiveBorderIdentificationType_Mandatory_B1806() => CombineAssertions(() =>
	{
		const string message = "[B1806] You have not entered a Type Of Identification.";

		testContext.EnableRule(c => c.IsRuleB1806Active);
		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);

		var departureMovement = CreateDepartureMovement();
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
		departureMovement.BM_ActiveBorderIdentificationType = ZString.Empty;
		AssertHasMessageError("BM_ExportTransportMode is not 5 and BM_ActiveBorderIdentificationType is empty.", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);

		departureMovement.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._40;
		AssertNoMessageError("BM_ExportTransportMode is not 5 and BM_ActiveBorderIdentificationType is not empty.", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);

		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._5_PostalConsignment;
		departureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
		AssertNoMessageError("BM_ExportTransportMode is 5 and BM_ActiveBorderIdentificationType is not empty.", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);

		departureMovement.BM_ActiveBorderIdentificationType = ZString.Empty;
		AssertNoMessageError("BM_ExportTransportMode is 5 and BM_ActiveBorderIdentificationType is empty.", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);

		testContext.DisableRule(c => c.IsRuleB1806Active);
		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);

		departureMovement = CreateDepartureMovement();
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
		departureMovement.BM_ActiveBorderIdentificationType = ZString.Empty;
		AssertNoMessageError("Rule disabled, BM_ExportTransportMode is not 5 and BM_ActiveBorderIdentificationType is empty.", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);

		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._5_PostalConsignment;
		departureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
		AssertNoMessageError("Rule disabled, BM_ExportTransportMode is 5 and BM_ActiveBorderIdentificationType is not empty.", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);

		departureMovement.BM_ActiveBorderIdentificationType = ZString.Empty;
		AssertNoMessageError("Rule disabled, BM_ExportTransportMode is 5 and BM_ActiveBorderIdentificationType is empty.", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);
	});

	public void TestCheckBM_ActiveBorderIdentificationType_Mandatory_RuleB1838()
	{
		const string message = "[B1838] Type of ID can't be empty.";

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		testContext.EnableRule(x => x.IsRuleB1838Active);
		CombineAssertions("When TP: ON, RuleB1838: Active", () =>
		{
			var departureMovement = CreateDepartureMovement();
			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			departureMovement.BM_RN_NKTOLCarrierNationality = "PL";
			departureMovement.BM_ActiveBorderIdentificationType = ZString.Empty;
			departureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
			AssertHasMessageError("BM_ActiveBorderIdentificationType is empty, BM_ExportTransportMode = 2 and BM_RN_NKTOLCarrierNationality is not empty.", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);

			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			departureMovement.BM_RN_NKTOLCarrierNationality = "PL";
			departureMovement.BM_ActiveBorderIdentificationType = ZString.Empty;
			departureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
			AssertHasMessageError("BM_ActiveBorderIdentificationType is empty, BM_ExportTransportMode != 2 and BM_RN_NKTOLCarrierNationality is not empty.", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);

			departureMovement.BM_RN_NKTOLCarrierNationality = ZString.Empty;
			departureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
			AssertNoMessageError("BM_ActiveBorderIdentificationType is empty, BM_ExportTransportMode != 2 but BM_RN_NKTOLCarrierNationality is empty.", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);

			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			departureMovement.BM_ActiveBorderIdentificationType = ZString.Empty;
			AssertHasMessageError("BM_ActiveBorderIdentificationType is empty, BM_ExportTransportMode = 2 but BM_RN_NKTOLCarrierNationality is empty.", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);

			foreach (var mode in new[] { ModeOfTransportList.Codes._1_SeaTransport, ModeOfTransportList.Codes._2_RailTransport })
			{
				foreach (var nationality in new[] { string.Empty, "PL" })
				{
					departureMovement.BM_ExportTransportMode = mode;
					departureMovement.BM_RN_NKTOLCarrierNationality = nationality;
					departureMovement.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._10;
					departureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
					AssertNoMessageError($"BM_ActiveBorderIdentificationType is not empty, BM_ExportTransportMode = {mode} and BM_RN_NKTOLCarrierNationality is '{nationality}'.", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);
				}
			}
		});

		testContext.DisableRule(x => x.IsRuleB1838Active);
		var departureMovement = CreateDepartureMovement();
		departureMovement.BM_ActiveBorderIdentificationType = ZString.Empty;
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
		departureMovement.BM_RN_NKTOLCarrierNationality = "PL";
		departureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
		AssertNoMessageError("When TP: ON, RuleB1838: Not Active, BM_ActiveBorderIdentificationType is empty, BM_ExportTransportMode = 2 and BM_RN_NKTOLCarrierNationality is not empty.", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		testContext.EnableRule(x => x.IsRuleB1838Active);

		departureMovement = CreateDepartureMovement();
		departureMovement.BM_ActiveBorderIdentificationType = ZString.Empty;

		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
		departureMovement.BM_RN_NKTOLCarrierNationality = "PL";
		departureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
		AssertNoMessageError("When TP: OFF, RuleB1838: Active, BM_ActiveBorderIdentificationType is empty: BM_ExportTransportMode = 2 and BM_RN_NKTOLCarrierNationality is not empty.", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);
	}

	public void TestCheckBM_ActiveBorderIdentificationType_Mandatory_C0806()
	{
		const string message = "[C0806] You have not entered a Type of Identification.";

		testContext.EnableRule(rule => rule.IsRuleC0806Active);
		var departureMovement = CreateDepartureMovement();
		var allSecurityTypes = new NctsTypeOfSecurityList().GetAllCodes();
		var typesOfEntExiBth = new ZString[] { NctsTypeOfSecurityList.Codes.ENT, NctsTypeOfSecurityList.Codes.EXI, NctsTypeOfSecurityList.Codes.BTH };

		CombineAssertions(() =>
		{
			testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;

			departureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
			AssertNoMessageError("Empty departure movement.", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);

			allSecurityTypes.ForEach(securityType =>
			{
				AssertMandatoryWhenSecurityTypeIsEntOrExiOrBth(securityType, typesOfEntExiBth.Contains(securityType));
			});

			testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
			testContext.DisableRule(rule => rule.IsRuleC0806Active);
			allSecurityTypes.SelectMany(GetTestCase).ForEach(testCase =>
			{
				testCase.Invoke();
				var testCaseName = $"Rule C0806 is disabled, BM_TypeOfSecurity = {departureMovement.BM_TypeOfSecurity}, BM_AdditionalDeclarationType = {departureMovement.BM_AdditionalDeclarationType}, BM_ActiveBorderIdentificationType = {departureMovement.BM_ActiveBorderIdentificationType}.";
				AssertNoMessageError(testCaseName, departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);
			});
		});
		return;

		void AssertMandatoryWhenSecurityTypeIsEntOrExiOrBth(ZString securityType, bool isEntExiBth)
		{
			departureMovement.BM_TypeOfSecurity = securityType;

			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			departureMovement.BM_ActiveBorderIdentificationType = ZString.Empty;
			if (isEntExiBth)
			{
				AssertHasMessageError($"TypeOfSecurity is {securityType}, AdditionalDeclarationType is A and ActiveBorderIdentificationType is empty.", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);
			}
			else
			{
				AssertNoMessageError($"TypeOfSecurity is {securityType}, AdditionalDeclarationType is A and ActiveBorderIdentificationType is empty.", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);
			}

			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			departureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
			AssertNoMessageError($"TypeOfSecurity is {securityType}, AdditionalDeclarationType is not A and ActiveBorderIdentificationType is empty.", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);

			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			departureMovement.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._40;
			AssertNoMessageError($"TypeOfSecurity is {securityType}, AdditionalDeclarationType is A and ActiveBorderIdentificationType is not empty.", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);

			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			departureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
			AssertNoMessageError($"TypeOfSecurity is {securityType}, AdditionalDeclarationType is not A and ActiveBorderIdentificationType is not empty.", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);
		}

		IEnumerable<Action> GetTestCase(string securityType)
		{
			yield return () =>
			{
				departureMovement.BM_TypeOfSecurity = securityType;
				departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
				departureMovement.BM_ActiveBorderIdentificationType = ZString.Empty;
			};

			yield return () =>
			{
				departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
				departureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
			};

			yield return () =>
			{
				departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
				departureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
			};
		}
	}

	public void TestCheckBM_ActiveBorderIdentificationType_WhenCodeIsNotInTheListButValid_ShouldBeMessageError_Code_99_OwnPropulsion()
	{
		var departureMovement = Factory.New<NctsDepartureMovementHeaderForValidationTest>();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		departureMovement.BM_BH = nctsHeader.PK;

		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
		ValidationTestHelper.AssertInvalidCodeMessageError(departureMovement.BM_ActiveBorderIdentificationTypeInfo,
															"99", NctsTransportTypeOfIdList.Codes._10);
	}

	public void TestCheckBM_ConveyanceNumber()
	{
		testContext.DisableRule(rule => rule.IsRuleC0531Active);

		var departureMovement = CreateDepartureMovement();
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
		departureMovement.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._40;
		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
		const string message = "You have not entered a Conveyance Number.";

		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(departureMovement.BM_ConveyanceNumberInfo, message, "BM_ConveyanceNumber 40");
			departureMovement.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._10;
			ValidationTestHelper.AssertFieldIsNotMandatory(departureMovement.BM_ConveyanceNumberInfo, message, "BM_ConveyanceNumber 10");
		});
	}

	public void TestCheckBM_ConveyanceNumberR0315_ExceedLengthMessage()
	{
		const string conveyanceNumberExceedLengthMessageError = "[R0315] In the Conveyance No., the entered Flight Number should not exceed 8 character length.";
		var departureMovement = CreateDepartureMovement();
		const string conveyanceNumberLengthMoreThanEight = "123456789";

		CombineAssertions(() =>
		{
			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			AssertNoMessageError("The validation does not activate", departureMovement.BM_ConveyanceNumberInfo, conveyanceNumberExceedLengthMessageError);

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleR0315Active)))
			{
				departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
				departureMovement.BM_ConveyanceNumber = conveyanceNumberLengthMoreThanEight;
				AssertHasMessageError("The length of conveyancenumber exceeds the maximum", departureMovement.BM_ConveyanceNumberInfo, conveyanceNumberExceedLengthMessageError);
			}

			using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleR0315Active)))
			{
				departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
				departureMovement.BM_ConveyanceNumber = conveyanceNumberLengthMoreThanEight;
				AssertNoMessageError("The rule R0315 does not activate", departureMovement.BM_ConveyanceNumberInfo, conveyanceNumberExceedLengthMessageError);
			}
		});
	}

	public void TestCheckBM_ConveyanceNumberR0315_ContainNonAlphanumeric()
	{
		const string conveyanceNumberContainNonAlphanumericMessageError = "[R0315] In the Conveyance No., the entered Flight Number should not contain non-alphanumeric characters.";
		var departureMovement = CreateDepartureMovement();
		const string conveyanceNumberContianNonalphanumeric = "123456#7";

		CombineAssertions(() =>
		{
			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			AssertNoMessageError("The validation does not activate", departureMovement.BM_ConveyanceNumberInfo, conveyanceNumberContainNonAlphanumericMessageError);

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleR0315Active)))
			{
				departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
				departureMovement.BM_ConveyanceNumber = conveyanceNumberContianNonalphanumeric;
				AssertHasMessageError("The conveyancenumber is invalid", departureMovement.BM_ConveyanceNumberInfo, conveyanceNumberContainNonAlphanumericMessageError);
			}

			using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleR0315Active)))
			{
				departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
				departureMovement.BM_ConveyanceNumber = conveyanceNumberContianNonalphanumeric;
				AssertNoMessageError("The rule R0315 does not activate", departureMovement.BM_ConveyanceNumberInfo, conveyanceNumberContainNonAlphanumericMessageError);
			}
		});
	}

	public void TestCheckBM_ConveyanceNumberR0315_ContianLowercase()
	{
		const string conveyanceNumberContainsLowerCaseMessageError = "[R0315] In the Conveyance No., the entered Flight Number should not contain lowercase alphabets.";
		var departureMovement = CreateDepartureMovement();
		const string conveyanceNumberContianLowercase = "12345q12";

		CombineAssertions(() =>
		{
			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			AssertNoMessageError("The validation does not activate", departureMovement.BM_ConveyanceNumberInfo, conveyanceNumberContainsLowerCaseMessageError);

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleR0315Active)))
			{
				departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
				departureMovement.BM_ConveyanceNumber = conveyanceNumberContianLowercase;
				AssertHasMessageError("The conveyancenumber contains lowercase", departureMovement.BM_ConveyanceNumberInfo, conveyanceNumberContainsLowerCaseMessageError);
			}

			using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleR0315Active)))
			{
				departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
				departureMovement.BM_ConveyanceNumber = conveyanceNumberContianLowercase;
				AssertNoMessageError("The rule R0315 does not activate", departureMovement.BM_ConveyanceNumberInfo, conveyanceNumberContainsLowerCaseMessageError);
			}
		});
	}

	public void TestCheckBM_ConveyanceNumberR0315_IsRight()
	{
		const string conveyanceNumberValid = "hya1234";
		var departureMovement = CreateDepartureMovement();

		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
		departureMovement.BM_ConveyanceNumber = conveyanceNumberValid;
		AssertNoErrors("The length of conveyancenumber is right", departureMovement.BM_ConveyanceNumberInfo);
	}

	public void TestCheckBM_ConveyanceNumber_CheckRuleC0531()
	{
		var errorMessage = "[C0531] You have not entered a Conveyance Number.";

		testContext.EnableRule(rule => rule.IsRuleC0531Active);

		var departureMovement = CreateDepartureMovement();

		departureMovement.BM_SubApplicationCode = Common.EU.NctsMoveHeaderType.Codes.Departure;
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
		departureMovement.BM_ConveyanceNumber = "123";
		departureMovement.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._40;

		var validation = departureMovement.Validation;

		CombineAssertions(() =>
		{
			AssertNoMessageError("No error when Conveyance Number is entered", departureMovement.BM_ConveyanceNumberInfo, errorMessage);

			departureMovement.BM_ConveyanceNumber = string.Empty;
			AssertHasMessageErrorButOnlyOnce("C0531 Conveyance Number is mandatory");

			testContext.DisableRule(rule => rule.IsRuleC0531Active);
			validation.ValidateBM_ConveyanceNumber();
			AssertNoMessageError("Rule C0531 is disabled", departureMovement.BM_ConveyanceNumberInfo, errorMessage);
			testContext.EnableRule(rule => rule.IsRuleC0531Active);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			validation.ValidateBM_ConveyanceNumber();
			AssertHasMessageErrorButOnlyOnce("C0531 Conveyance Number is mandatory for BM_TypeOfSecurity ENT");

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
			validation.ValidateBM_ConveyanceNumber();
			AssertHasMessageErrorButOnlyOnce("C0531 Conveyance Number is mandatory for BM_TypeOfSecurity EXI");

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			validation.ValidateBM_ConveyanceNumber();
			AssertNoMessageError("C0531 Conveyance Number is not mandatory as BM_TypeOfSecurity is not BTH, ENT, EXI", departureMovement.BM_ConveyanceNumberInfo, errorMessage);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
			departureMovement.BM_SubApplicationCode = "D";
			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			validation.ValidateBM_ConveyanceNumber();
			AssertNoMessageError("C0531 Conveyance Number is not mandatory due to BM_ExportTransportMode", departureMovement.BM_ConveyanceNumberInfo, errorMessage);
		});

		void AssertHasMessageErrorButOnlyOnce(string assertionMessage)
		{
			AssertHasMessageError(assertionMessage, departureMovement.BM_ConveyanceNumberInfo, errorMessage);
			var messages = string.Join("\n", departureMovement.BM_ConveyanceNumberInfo.GetMessageErrors().Select(x => x.Message));
			Assert($"Mandatory message shown more than once: {assertionMessage}\n{messages}", departureMovement.BM_ConveyanceNumberInfo.GetMessageErrors().Where(m => m.Message.Contains(MandatoryValidation.YouHaveNotEntered)).Count() <= 1);
		}
	}

	public void TestCheckBM_CustomsOfficeAtBorder_Mandatory() => CombineAssertions(() =>
	{
		var departureMovement = CreateDepartureMovement();
		departureMovement.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._40;
		var targetInfo = departureMovement.BM_CustomsOfficeAtBorderInfo;

		departureMovement.BM_ExportTransportMode = "4";
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
		departureMovement.BM_ExportTransportMode = "5";
		ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);
		departureMovement.BM_ExportTransportMode = "";
		ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);
	});

	public void TestCheckBM_CustomsOfficeAtBorder_Mandatory_C0806()
	{
		const string messagePrefix = "[C0806] You have not entered a Customs Office.";
		const string message = "You have not entered a Customs Office.";
		testContext.EnableRule(rule => rule.IsRuleC0806Active);
		var departureMovement = CreateDepartureMovement();
		departureMovement.BM_ExportTransportMode = "4";
		var allSecurityTypes = new NctsTypeOfSecurityList().GetAllCodes();
		var typsOfEntExiBth = new ZString[] { NctsTypeOfSecurityList.Codes.ENT, NctsTypeOfSecurityList.Codes.EXI, NctsTypeOfSecurityList.Codes.BTH };

		CombineAssertions(() =>
		{
			testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
			departureMovement.Validation.ValidateBM_CustomsOfficeAtBorder();
			AssertNoMessageError("Empty departure movement.", departureMovement.BM_CustomsOfficeAtBorderInfo, messagePrefix);

			allSecurityTypes.ForEach(securityType =>
			{
				AssertMandatoryWhenSecurityTypeIsEntOrExiOrBth(securityType, typsOfEntExiBth.Contains(securityType));
			});

			testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
			testContext.DisableRule(rule => rule.IsRuleC0806Active);
			allSecurityTypes.SelectMany(GetTestCase).ForEach(testCase =>
			{
				testCase.Invoke();
				var testCaseName = $"Rule C0806 is disabled, BM_TypeOfSecurity = {departureMovement.BM_TypeOfSecurity}, BM_AdditionalDeclarationType = {departureMovement.BM_AdditionalDeclarationType}, BM_CustomsOfficeAtBorder = {departureMovement.BM_CustomsOfficeAtBorder}.";
				AssertNoMessageError(testCaseName, departureMovement.BM_CustomsOfficeAtBorderInfo, messagePrefix);
			});
		});

		void AssertMandatoryWhenSecurityTypeIsEntOrExiOrBth(ZString securityType, bool isEntExiBth)
		{
			departureMovement.BM_TypeOfSecurity = securityType;

			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			departureMovement.BM_CustomsOfficeAtBorder = ZString.Empty;
			if (isEntExiBth)
			{
				AssertHasMessageError($"TypeOfSecurity is {securityType}, AdditionalDeclarationType is A and CustomsOfficeAtBorder is empty.", departureMovement.BM_CustomsOfficeAtBorderInfo, messagePrefix);
			}
			else
			{
				AssertNoMessageError($"TypeOfSecurity is {securityType}, AdditionalDeclarationType is A and CustomsOfficeAtBorder is empty.", departureMovement.BM_CustomsOfficeAtBorderInfo, messagePrefix);
				AssertHasMessageError($"TypeOfSecurity is {securityType}, AdditionalDeclarationType is A and CustomsOfficeAtBorder is empty.", departureMovement.BM_CustomsOfficeAtBorderInfo, message);
			}

			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			departureMovement.Validation.ValidateBM_CustomsOfficeAtBorder();
			AssertNoMessageError($"TypeOfSecurity is {securityType}, AdditionalDeclarationType is not A and CustomsOfficeAtBorder is empty.", departureMovement.BM_CustomsOfficeAtBorderInfo, messagePrefix);
			AssertHasMessageError($"TypeOfSecurity is {securityType}, AdditionalDeclarationType is A and CustomsOfficeAtBorder is empty.", departureMovement.BM_CustomsOfficeAtBorderInfo, message);

			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			departureMovement.BM_CustomsOfficeAtBorder = "CUSOFF1";
			AssertNoMessageError($"TypeOfSecurity is {securityType}, AdditionalDeclarationType is A and CustomsOfficeAtBorder is not empty.", departureMovement.BM_CustomsOfficeAtBorderInfo, message);

			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			departureMovement.Validation.ValidateBM_CustomsOfficeAtBorder();
			AssertNoMessageError($"TypeOfSecurity is {securityType}, AdditionalDeclarationType is not A and CustomsOfficeAtBorder is not empty.", departureMovement.BM_CustomsOfficeAtBorderInfo, messagePrefix);
		}

		IEnumerable<Action> GetTestCase(string securityType)
		{
			yield return () =>
			{
				departureMovement.BM_TypeOfSecurity = securityType;
				departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
				departureMovement.BM_CustomsOfficeAtBorder = ZString.Empty;
			};

			yield return () =>
			{
				departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
				departureMovement.Validation.ValidateBM_CustomsOfficeAtBorder();
			};

			yield return () =>
			{
				departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
				departureMovement.Validation.ValidateBM_CustomsOfficeAtBorder();
			};
		}
	}

	public void TestCheckBM_CustomsOfficeAtBorder_TechnicalRuleTR0052()
	{
		var departureMovement = CreateDepartureMovement();
		const string errorText = "[TR0052]";

		CombineAssertions(() =>
		{
			departureMovement.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._40;
			var office1 = departureMovement.CustomsOfficesForDeparture.AddNew();
			office1.CY_Data = "CUSOFF1";
			office1.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
			var office2 = departureMovement.CustomsOfficesForDeparture.AddNew();
			office2.CY_Data = "CUSOFF2";
			office2.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
			departureMovement.BM_CustomsOfficeAtBorder = "CUSOFF1";

			using var testContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory);
			testContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0052Active));

			departureMovement.Validation.ValidateBM_CustomsOfficeAtBorder();
			AssertNoMessageErrorContaining("Office of departure is filled in, but Rule TR0052 is inactive", departureMovement.BM_CustomsOfficeAtBorderInfo, errorText);

			testContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0052Active));
			departureMovement.Validation.ValidateBM_CustomsOfficeAtBorder();

			AssertHasMessageErrorContaining("Office of departure is filled in, Rule TR0052 is active", departureMovement.BM_CustomsOfficeAtBorderInfo, errorText);

			departureMovement.BM_CustomsOfficeAtBorder = "CUSOFF2";
			AssertNoMessageErrorContaining("Office of Transit is filled in", departureMovement.BM_CustomsOfficeAtBorderInfo, errorText);
		});
	}

	public void TestCheckBM_TypeOfSecurity_EXI_CusCodeTXTIsNeed_TR0048()
	{
		const string message = "[TR0048] You have not entered an Office of Exit for Transit with Purpose 'TXT'.";
		var departureMovement = CreateDepartureMovement();

		CombineAssertions(() =>
		{
			testContext.EnableRule(decider => decider.IsRuleTR0048Active);
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			AssertNoMessageError("BM_TypeOfSecurity 'ENT', no TXT", departureMovement.BM_TypeOfSecurityInfo, message);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
			departureMovement.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			AssertHasMessageError("BM_TypeOfSecurity 'EXI', no TXT", departureMovement.BM_TypeOfSecurityInfo, message);

			departureMovement.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
			departureMovement.Validation.ValidateBM_TypeOfSecurity();
			AssertNoMessageError("BM_TypeOfSecurity 'EXI', has TXT", departureMovement.BM_TypeOfSecurityInfo, message);

			testContext.DisableRule(decider => decider.IsRuleTR0048Active);
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
			departureMovement.CustomsOffices.RemoveAndDeleteAll();
			departureMovement.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			AssertNoMessageError("BM_TypeOfSecurity 'EXI', no TXT", departureMovement.BM_TypeOfSecurityInfo, message);
		});
	}

	public void TestCheckBM_TypeOfSecurity_BTH_CusCodeTXTIsNeed_TR0048()
	{
		const string message = "[TR0048] You have not entered an Office of Exit for Transit with Purpose 'TXT'.";
		var departureMovement = CreateDepartureMovement();

		CombineAssertions(() =>
		{
			testContext.EnableRule(decider => decider.IsRuleTR0048Active);
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			AssertNoMessageError("BM_TypeOfSecurity 'ENT', no TXT", departureMovement.BM_TypeOfSecurityInfo, message);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
			departureMovement.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			AssertHasMessageError("BM_TypeOfSecurity 'BTH', no TXT", departureMovement.BM_TypeOfSecurityInfo, message);

			departureMovement.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
			departureMovement.Validation.ValidateBM_TypeOfSecurity();
			AssertNoMessageError("BM_TypeOfSecurity 'BTH', has TXT", departureMovement.BM_TypeOfSecurityInfo, message);

			testContext.DisableRule(decider => decider.IsRuleTR0048Active);
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
			departureMovement.CustomsOffices.RemoveAndDeleteAll();
			departureMovement.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			AssertNoMessageError("BM_TypeOfSecurity 'BTH', no TXT", departureMovement.BM_TypeOfSecurityInfo, message);
		});
	}

	public void TestCheckBM_TypeOfSecurity_EXI_CusCodeTXTIsNeed_TR0053()
	{
		const string message = "[TR0053] You have not entered an Office of Exit for Transit with Purpose 'TXT'.";
		testContext.EnableRule(rule => rule.IsRuleTR0053Active);
		var departureMovement = CreateDepartureMovement();

		CombineAssertions(() =>
		{
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			AssertNoMessageError("BM_TypeOfSecurity 'ENT', no TXT nor TRA", departureMovement.BM_TypeOfSecurityInfo, message);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
			departureMovement.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			AssertHasMessageError("BM_TypeOfSecurity 'EXI', no TXT nor TRA", departureMovement.BM_TypeOfSecurityInfo, message);

			var txtOffice = departureMovement.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
			departureMovement.Validation.ValidateBM_TypeOfSecurity();
			AssertNoMessageError("BM_TypeOfSecurity 'EXI', has TXT", departureMovement.BM_TypeOfSecurityInfo, message);

			departureMovement.CustomsOffices.Remove(txtOffice);
			departureMovement.Validation.ValidateBM_TypeOfSecurity();
			AssertHasMessageError("BM_TypeOfSecurity 'EXI', no TXT nor TRA", departureMovement.BM_TypeOfSecurityInfo, message);

			departureMovement.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
			departureMovement.Validation.ValidateBM_TypeOfSecurity();
			AssertNoMessageError("BM_TypeOfSecurity 'EXI', has TRA", departureMovement.BM_TypeOfSecurityInfo, message);

			testContext.DisableRule(rule => rule.IsRuleTR0053Active);
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
			departureMovement.CustomsOffices.RemoveAndDeleteAll();
			departureMovement.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			AssertNoMessageError("BM_TypeOfSecurity 'EXI', no TXT nor TRA", departureMovement.BM_TypeOfSecurityInfo, message);
		});
	}

	public void TestCheckBM_TypeOfSecurity_BTH_CusCodeTXTIsNeed_TR0053()
	{
		const string message = "[TR0053] You have not entered an Office of Exit for Transit with Purpose 'TXT'.";
		testContext.EnableRule(rule => rule.IsRuleTR0053Active);
		var departureMovement = CreateDepartureMovement();

		CombineAssertions(() =>
		{
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			AssertNoMessageError("BM_TypeOfSecurity 'ENT', no TXT", departureMovement.BM_TypeOfSecurityInfo, message);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
			departureMovement.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			AssertHasMessageError("BM_TypeOfSecurity 'BTH', no TXT nor TRA", departureMovement.BM_TypeOfSecurityInfo, message);

			var txtOffice = departureMovement.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
			departureMovement.Validation.ValidateBM_TypeOfSecurity();
			AssertNoMessageError("BM_TypeOfSecurity 'BTH', has TXT", departureMovement.BM_TypeOfSecurityInfo, message);

			departureMovement.CustomsOffices.Remove(txtOffice);
			departureMovement.Validation.ValidateBM_TypeOfSecurity();
			AssertHasMessageError("BM_TypeOfSecurity 'BTH', no TXT nor TRA", departureMovement.BM_TypeOfSecurityInfo, message);

			departureMovement.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
			departureMovement.Validation.ValidateBM_TypeOfSecurity();
			AssertNoMessageError("BM_TypeOfSecurity 'BTH', has TRA", departureMovement.BM_TypeOfSecurityInfo, message);

			testContext.DisableRule(rule => rule.IsRuleTR0053Active);
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
			departureMovement.CustomsOffices.RemoveAndDeleteAll();
			departureMovement.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			AssertNoMessageError("BM_TypeOfSecurity 'BTH', no TXT nor TRA", departureMovement.BM_TypeOfSecurityInfo, message);
		});
	}

	public void TestCheckBM_TypeOfSecurity_CountriesOfRoutingRequired_C0586()
	{
		var departureMovement = CreateDepartureMovement();
		var countriesOfRouting = departureMovement.Header.CountriesOfRouting;
		const string errorMessage = "[C0586] You have not entered a Country/Region Of Routing.";
		CombineAssertions(() =>
		{
			testContext.EnableRule(x => x.IsRuleC0586Active);
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			AssertNoMessageError($"SecurityType is NON and Country Of Routing count is {countriesOfRouting.Count}.", departureMovement.BM_TypeOfSecurityInfo, errorMessage);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			AssertHasMessageError($"SecurityType is ENT and Country Of Routing count is {countriesOfRouting.Count}", departureMovement.BM_TypeOfSecurityInfo, errorMessage);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
			AssertHasMessageError($"SecurityType is ENT and Country Of Routing count is {countriesOfRouting.Count}", departureMovement.BM_TypeOfSecurityInfo, errorMessage);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
			AssertHasMessageError($"SecurityType is ENT and Country Of Routing count is {countriesOfRouting.Count}", departureMovement.BM_TypeOfSecurityInfo, errorMessage);

			testContext.DisableRule(x => x.IsRuleC0586Active);
			departureMovement.Validation.ValidateBM_TypeOfSecurity();
			AssertNoMessageError("Rule is disabled", departureMovement.BM_TypeOfSecurityInfo, errorMessage);

			testContext.EnableRule(x => x.IsRuleC0586Active);

			countriesOfRouting.AddNew();

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			AssertNoMessageError($"SecurityType is NON and Country Of Routing count is {countriesOfRouting.Count}.", departureMovement.BM_TypeOfSecurityInfo, errorMessage);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			AssertNoMessageError($"SecurityType is ENT and Country Of Routing count is {countriesOfRouting.Count}", departureMovement.BM_TypeOfSecurityInfo, errorMessage);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
			AssertNoMessageError($"SecurityType is ENT and Country Of Routing count is {countriesOfRouting.Count}", departureMovement.BM_TypeOfSecurityInfo, errorMessage);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
			AssertNoMessageError($"SecurityType is ENT and Country Of Routing count is {countriesOfRouting.Count}", departureMovement.BM_TypeOfSecurityInfo, errorMessage);
		});
	}

	public void TestCheckBM_TypeOfSecurity_CountriesOfRoutingRequired_RuleB1848() => CombineAssertions(() =>
	{
		const string errorMessage = "[B1848] You have not entered a Country/Region Of Routing.";

		var departureMovement = CreateDepartureMovement();
		var countriesOfRouting = departureMovement.Header.CountriesOfRouting;

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		testContext.EnableRule(x => x.IsRuleB1848Active);
		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
		AssertNoMessageError($"SecurityType is NON and Country Of Routing count is {countriesOfRouting.Count}.", departureMovement.BM_TypeOfSecurityInfo, errorMessage);

		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
		AssertHasMessageError($"SecurityType is ENT and Country Of Routing count is {countriesOfRouting.Count}", departureMovement.BM_TypeOfSecurityInfo, errorMessage);

		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
		AssertHasMessageError($"SecurityType is EXI and Country Of Routing count is {countriesOfRouting.Count}", departureMovement.BM_TypeOfSecurityInfo, errorMessage);

		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
		AssertHasMessageError($"SecurityType is ENT and Country Of Routing count is {countriesOfRouting.Count}", departureMovement.BM_TypeOfSecurityInfo, errorMessage);

		testContext.DisableRule(x => x.IsRuleB1848Active);
		departureMovement.Validation.ValidateBM_TypeOfSecurity();
		AssertNoMessageError("Rule is disabled", departureMovement.BM_TypeOfSecurityInfo, errorMessage);

		testContext.EnableRule(x => x.IsRuleB1848Active);

		countriesOfRouting.AddNew();

		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
		AssertNoMessageError($"SecurityType is NON and Country Of Routing count is {countriesOfRouting.Count}.", departureMovement.BM_TypeOfSecurityInfo, errorMessage);

		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
		AssertNoMessageError($"SecurityType is ENT and Country Of Routing count is {countriesOfRouting.Count}", departureMovement.BM_TypeOfSecurityInfo, errorMessage);

		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
		AssertNoMessageError($"SecurityType is EXI and Country Of Routing count is {countriesOfRouting.Count}", departureMovement.BM_TypeOfSecurityInfo, errorMessage);

		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
		AssertNoMessageError($"SecurityType is ENT and Country Of Routing count is {countriesOfRouting.Count}", departureMovement.BM_TypeOfSecurityInfo, errorMessage);

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		testContext.EnableRule(x => x.IsRuleB1848Active);
		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
		AssertNoMessageError("Is not in NCTSTransitionPeriod", departureMovement.BM_TypeOfSecurityInfo, errorMessage);
	});

	public void TestCheckBM_TypeOfSecurity_CountriesRequiringSecurity_RuleNR0018() => CombineAssertions(() =>
	{
		const string message = "[NR0018] Destination Country is outside the European Security Zone.";
		var countriesRequiringSecurity = new[] { Turkey, Algeria };
		var countriesNotRequiringSecurity = new[] { Germany, Switzerland, Norway };

		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		var eusecTradeGroup = helper.CreateTradeGroup(RefDataGroupingCodes.EuropeanUnionEUN, RefCusTradeGroupCodes.EUForSafetyAndSecurity, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		foreach (var country in countriesNotRequiringSecurity)
		{
			helper.AddCountry(eusecTradeGroup, country);
		}
		Factory.Save();

		var departureMovement = CreateDepartureMovement();

		testContext.EnableRule(x => x.IsRuleNR0018Active);

		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;

		foreach (var country in countriesRequiringSecurity)
		{
			departureMovement.BM_RL_NKDestinationPort = country;
			departureMovement.Validation.ValidateBM_TypeOfSecurity();
			AssertHasWarning($"Insecure country: {country}", departureMovement.BM_TypeOfSecurityInfo, message);
		}

		foreach (var country in countriesNotRequiringSecurity)
		{
			departureMovement.BM_RL_NKDestinationPort = country;
			departureMovement.Validation.ValidateBM_TypeOfSecurity();
			AssertNoWarning($"Secure country: {country}", departureMovement.BM_TypeOfSecurityInfo, message);
		}

		departureMovement.BM_RL_NKDestinationPort = countriesRequiringSecurity[0];
		foreach (var typeOfSecurity in new NctsTypeOfSecurityList().GetAllCodes().Where(x => x != NctsTypeOfSecurityList.Codes.NON))
		{
			departureMovement.BM_TypeOfSecurity = typeOfSecurity;
			AssertNoWarning($"Type of security: {typeOfSecurity}", departureMovement.BM_TypeOfSecurityInfo, message);
		}

		departureMovement.BM_RL_NKDestinationPort = ZString.Empty;
		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
		AssertNoWarning("Destination country is empty", departureMovement.BM_TypeOfSecurityInfo, message);

		testContext.DisableRule(x => x.IsRuleNR0018Active);
		departureMovement.BM_RL_NKDestinationPort = countriesRequiringSecurity[0];
		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
		AssertNoWarning("NR0018 disabled", departureMovement.BM_TypeOfSecurityInfo, message);
	});

	public void TestRuleC0586AndB1848NotActiveTogetherToAvoidDuplicateMessageAtBM_TypeOfSecurity() => CombineAssertions(() =>
	{
		var departureMovement = CreateDepartureMovement();
		var validationDecider = departureMovement.ValidationDecider as INctsDepartureMovementHeaderPhase5ValidationDecider;

		AssertEquals("When Transition period OFF", false, (validationDecider?.IsRuleC0586Active ?? false) && (validationDecider?.IsRuleB1848Active ?? false));

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		AssertEquals("When Transition period ON", false, (validationDecider?.IsRuleC0586Active ?? false) && (validationDecider?.IsRuleB1848Active ?? true));
	});

	public void TestCheckBM_TypeOfSecurity_CountryOfDispatchAndCountryOfDestination_RuleB1848_1()
	{
		const string messageError = "[B1848-1] You need to supply at least 2 records (Country of Dispatch and Destination) in the Country/Region of Routing.";

		testContext.EnableRule(decider => decider.IsRuleB1848_1Active);

		var departureMovement = CreateDepartureMovement();
		var targetInfo = departureMovement.BM_TypeOfSecurityInfo;
		var countriesOfRounting = departureMovement.Header.CountriesOfRouting;
		CombineAssertions(() =>
		{
			foreach (var securityType in new[] { NctsTypeOfSecurityList.Codes.ENT, NctsTypeOfSecurityList.Codes.BTH, NctsTypeOfSecurityList.Codes.EXI })
			{
				testContext.EnableRule(decider => decider.IsRuleB1848_1Active);
				departureMovement.BM_TypeOfSecurity = securityType;
				AssertHasMessageError($"Rule active, BM_TypeOfSecurity == '{securityType}', 0 countryOfRouting", targetInfo, messageError);

				testContext.DisableRule(decider => decider.IsRuleB1848_1Active);
				departureMovement.Validation.ValidateBM_TypeOfSecurity();
				AssertNoMessageError($"Rule not active, BM_TypeOfSecurity == '{securityType}', 0 countryOfRouting", targetInfo, messageError);

				testContext.EnableRule(decider => decider.IsRuleB1848_1Active);
				countriesOfRounting.AddNew();
				departureMovement.Validation.ValidateBM_TypeOfSecurity();
				AssertHasMessageError($"Rule active, BM_TypeOfSecurity == '{securityType}', 1 countryOfRouting", targetInfo, messageError);

				testContext.DisableRule(decider => decider.IsRuleB1848_1Active);
				departureMovement.Validation.ValidateBM_TypeOfSecurity();
				AssertNoMessageError($"Rule not active, BM_TypeOfSecurity == '{securityType}', 1 countryOfRouting", targetInfo, messageError);

				testContext.EnableRule(decider => decider.IsRuleB1848_1Active);
				countriesOfRounting.AddNew();
				departureMovement.Validation.ValidateBM_TypeOfSecurity();
				AssertNoMessageError($"Rule active, BM_TypeOfSecurity == '{securityType}', 2 countriesOfRouting", targetInfo, messageError);

				countriesOfRounting.RemoveAndDeleteAll();
			}

			testContext.EnableRule(decider => decider.IsRuleB1848_1Active);
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			AssertNoMessageError("Rule active, BM_TypeOfSecurity == 'NON', 0 countriyOfRouting", targetInfo, messageError);

			countriesOfRounting.AddNew();
			departureMovement.Validation.ValidateBM_TypeOfSecurity();
			AssertNoMessageError("Rule active, BM_TypeOfSecurity == 'NON', 1 countryOfRouting", targetInfo, messageError);
		});
	}

	public void TestCheckBM_TOLCarrierID_Mandatory() => CombineAssertions(() =>
	{
		var departureMovement = CreateDepartureMovement();
		var targetInfo = departureMovement.BM_TOLCarrierIDInfo;
		departureMovement.BM_ExportTransportMode = "4";
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
		departureMovement.BM_ExportTransportMode = "5";
		ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);
		departureMovement.BM_ExportTransportMode = "";
		ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);
	});

	public void TestCheckBM_TOLCarrierID_Mandatory_C0806()
	{
		const string messagePrefix = "[C0806] You have not entered";
		const string message = "You have not entered";
		testContext.EnableRule(rule => rule.IsRuleC0806Active);
		var departureMovement = CreateDepartureMovement();
		departureMovement.BM_ExportTransportMode = "4";
		var allSecurityTypes = new NctsTypeOfSecurityList().GetAllCodes();
		var typsOfEntExiBth = new ZString[] { NctsTypeOfSecurityList.Codes.ENT, NctsTypeOfSecurityList.Codes.EXI, NctsTypeOfSecurityList.Codes.BTH };

		CombineAssertions(() =>
		{
			testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
			departureMovement.Validation.ValidateBM_TOLCarrierID();
			AssertNoMessageErrorContaining("Empty departure movement.", departureMovement.BM_TOLCarrierIDInfo, messagePrefix);

			allSecurityTypes.ForEach(securityType =>
			{
				AssertMandatoryWhenSecurityTypeIsEntOrExiOrBth(securityType, typsOfEntExiBth.Contains(securityType));
			});

			testContext.DisableRule(rule => rule.IsRuleC0806Active);
			allSecurityTypes.SelectMany(GetTestCase).ForEach(testCase =>
			{
				testCase.Invoke();
				var testCaseName = $"Rule C0806 is disabled, BM_TypeOfSecurity = {departureMovement.BM_TypeOfSecurity}, BM_AdditionalDeclarationType = {departureMovement.BM_AdditionalDeclarationType}, BM_TOLCarrierID = {departureMovement.BM_TOLCarrierID}";
				AssertNoMessageErrorContaining(testCaseName, departureMovement.BM_TOLCarrierIDInfo, messagePrefix);
			});
		});
		return;

		void AssertMandatoryWhenSecurityTypeIsEntOrExiOrBth(ZString securityType, bool isEntExitBth)
		{
			departureMovement.BM_TypeOfSecurity = securityType;

			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			departureMovement.BM_TOLCarrierID = ZString.Empty;
			if (isEntExitBth)
			{
				AssertHasMessageErrorContaining($"TypeOfSecurity is {securityType}, AdditionalDeclarationType is A and BM_TOLCarrierID is empty.", departureMovement.BM_TOLCarrierIDInfo, messagePrefix);
			}
			else
			{
				AssertNoMessageErrorContaining($"TypeOfSecurity is {securityType}, AdditionalDeclarationType is A and BM_TOLCarrierID is empty.", departureMovement.BM_TOLCarrierIDInfo, messagePrefix);
				AssertHasMessageErrorContaining($"TypeOfSecurity is {securityType}, AdditionalDeclarationType is A and BM_TOLCarrierID is empty.", departureMovement.BM_TOLCarrierIDInfo, message);
			}

			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			departureMovement.Validation.ValidateBM_TOLCarrierID();
			AssertNoMessageErrorContaining($"TypeOfSecurity is {securityType}, AdditionalDeclarationType is not A and BM_TOLCarrierID is empty.", departureMovement.BM_TOLCarrierIDInfo, messagePrefix);
			AssertHasMessageErrorContaining($"TypeOfSecurity is {securityType}, AdditionalDeclarationType is A and BM_TOLCarrierID is empty.", departureMovement.BM_TOLCarrierIDInfo, message);

			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			departureMovement.BM_TOLCarrierID = "NC15REG";
			AssertNoMessageErrorContaining($"TypeOfSecurity is {securityType}, AdditionalDeclarationType is A and BM_TOLCarrierID is not empty.", departureMovement.BM_TOLCarrierIDInfo, messagePrefix);

			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			departureMovement.Validation.ValidateBM_TOLCarrierID();
			AssertNoMessageErrorContaining($"TypeOfSecurity is {securityType}, AdditionalDeclarationType is not A and BM_TOLCarrierID is not empty.", departureMovement.BM_TOLCarrierIDInfo, messagePrefix);
		}

		IEnumerable<Action> GetTestCase(string securityType)
		{
			yield return () =>
			{
				departureMovement.BM_TypeOfSecurity = securityType;
				departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
				departureMovement.BM_TOLCarrierID = ZString.Empty;
			};

			yield return () =>
			{
				departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
				departureMovement.Validation.ValidateBM_TOLCarrierID();
			};

			yield return () =>
			{
				departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
				departureMovement.Validation.ValidateBM_TOLCarrierID();
			};
		}
	}

	public void TestCheckBM_TOLCarrierID_MaxLength()
	{
		var departureMovement = CreateDepartureMovement();

		CombineAssertions(() =>
		{
			departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals("Max Length (NCTS4)", 27, departureMovement.BM_TOLCarrierIDInfo.MaxLength);

			departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertEquals("Max Length (NCTS5) and not in phase5 transition period", 35, departureMovement.BM_TOLCarrierIDInfo.MaxLength);
			UniversalValidationHelperTest.AssertMaxLengthE1103(departureMovement.Header, departureMovement.BM_TOLCarrierIDInfo, false);
		});
	}

	public void TestCheckBM_RN_NKTOLCarrierNationality_Mandatory_C0806()
	{
		const string messagePrefix = "[C0806] You have not entered a Nationality.";
		const string message = "You have not entered a Nationality.";
		testContext.EnableRule(rule => rule.IsRuleC0806Active);
		var departureMovement = CreateDepartureMovement();
		var allSecurityTypes = new NctsTypeOfSecurityList().GetAllCodes();
		var typsOfEntExiBth = new ZString[] { NctsTypeOfSecurityList.Codes.ENT, NctsTypeOfSecurityList.Codes.EXI, NctsTypeOfSecurityList.Codes.BTH };

		CombineAssertions(() =>
		{
			testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
			departureMovement.Validation.ValidateBM_RN_NKTOLCarrierNationality();
			AssertNoMessageError("Empty departure movement.", departureMovement.BM_RN_NKTOLCarrierNationalityInfo, messagePrefix);
			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			allSecurityTypes.ForEach(securityType =>
			{
				AssertMandatoryWhenSecurityTypeIsEntOrExiOrBth(securityType, typsOfEntExiBth.Contains(securityType));
			});

			testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			departureMovement.BM_RN_NKTOLCarrierNationality = ZString.Empty;
			AssertNoMessageError("In the transition period", departureMovement.BM_RN_NKTOLCarrierNationalityInfo, messagePrefix);

			testContext.DisableRule(rule => rule.IsRuleC0806Active);
			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			departureMovement.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._21;
			departureMovement.BM_RN_NKTOLCarrierNationality = ZString.Empty;

			AssertNoMessageError("TypeOfSecurity is, AdditionalDeclarationType is not A and BM_RN_NKTOLCarrierNationality is not empty.", departureMovement.BM_RN_NKTOLCarrierNationalityInfo, messagePrefix);
			AssertHasMessageError("TypeOfSecurity is, AdditionalDeclarationType is not A and BM_RN_NKTOLCarrierNationality is not empty.", departureMovement.BM_RN_NKTOLCarrierNationalityInfo, message);
		});
		return;

		void AssertMandatoryWhenSecurityTypeIsEntOrExiOrBth(ZString securityType, bool isEntExiBth)
		{
			departureMovement.BM_TypeOfSecurity = securityType;

			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			departureMovement.BM_RN_NKTOLCarrierNationality = ZString.Empty;
			if (isEntExiBth)
			{
				AssertHasMessageError($"TypeOfSecurity is {securityType}, AdditionalDeclarationType is A and BM_RN_NKTOLCarrierNationality is empty.", departureMovement.BM_RN_NKTOLCarrierNationalityInfo, messagePrefix);
			}
			else
			{
				AssertNoMessageError($"TypeOfSecurity is {securityType}, AdditionalDeclarationType is A and BM_RN_NKTOLCarrierNationality is empty.", departureMovement.BM_RN_NKTOLCarrierNationalityInfo, messagePrefix);
			}

			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			departureMovement.Validation.ValidateBM_RN_NKTOLCarrierNationality();
			AssertNoMessageError($"TypeOfSecurity is {securityType}, AdditionalDeclarationType is not A and BM_RN_NKTOLCarrierNationality is empty.", departureMovement.BM_RN_NKTOLCarrierNationalityInfo, messagePrefix);

			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			departureMovement.BM_RN_NKTOLCarrierNationality = Germany;
			AssertNoMessageError($"TypeOfSecurity is {securityType}, AdditionalDeclarationType is A and BM_RN_NKTOLCarrierNationality is not empty.", departureMovement.BM_RN_NKTOLCarrierNationalityInfo, messagePrefix);

			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			departureMovement.BM_RN_NKTOLCarrierNationality = Germany;
			AssertNoMessageError($"TypeOfSecurity is {securityType}, AdditionalDeclarationType is not A and BM_RN_NKTOLCarrierNationality is not empty.", departureMovement.BM_RN_NKTOLCarrierNationalityInfo, messagePrefix);
		}
	}

	public void TestCheckBM_RN_NKTOLCarrierNationality_ValidCode()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCountryListOfAU_DE_FR("NCNAT", "EUN");
		Factory.Save();

		var departureMovement = CreateDepartureMovement();
		departureMovement.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._10;

		ValidationTestHelper.AssertInvalidCodeMessageError(departureMovement.BM_RN_NKTOLCarrierNationalityInfo, "-X", "DE");
	}

	public void TestCheckBM_GrossWeight_IsValid()
	{
		const string message = "[R0994] Gross Weight must be equal or greater than the sum of all Gross Weights on House Consignments.";
		testContext.EnableRule(rule => rule.IsRuleR0994Active);
		var departureMovement = CreateDepartureMovement();
		var targetInfo = departureMovement.BM_GrossWeightInfo;
		var bill1 = departureMovement.Header.Bills.AddNew();
		bill1.B0_Weight = 11.111m;
		var bill2 = departureMovement.Header.Bills.AddNew();
		bill2.B0_Weight = 12.222m;

		CombineAssertions(() =>
		{
			departureMovement.BM_GrossWeight = 23.333m;
			AssertNoMessageError("R0994: ON, GrossWeight is Valid", targetInfo, message);

			departureMovement.BM_GrossWeight = 22.222m;
			AssertHasMessageError("R0994: ON, GrossWeight is invalid", targetInfo, message);

			testContext.DisableRule(rule => rule.IsRuleR0994Active);
			departureMovement.BM_GrossWeight = 23.333m;
			AssertNoMessageError("R0994: OFF, GrossWeight is Valid", targetInfo, message);

			departureMovement.BM_GrossWeight = 22.222m;
			AssertNoMessageError("R0994: OFF, GrossWeight is invalid", targetInfo, message);
		});
	}

	public void TestCheckBM_GrossWeight_ConditionRuleR0994_1_WhenActive() => CombineAssertions(() =>
	{
		const string message = "[R0994-1] Total Gross Weight on declaration should be equal or greater than the sum of Gross Weight of all House Consignments (100 kg)";
		var departureMovement = CreateDepartureMovement();
		var bill1 = departureMovement.Header.Bills.AddNew();
		var bill2 = departureMovement.Header.Bills.AddNew();
		bill1.B0_Weight = 50m;
		bill1.B0_WeightUQ = "KG";
		bill2.B0_Weight = 50000m;
		bill2.B0_WeightUQ = "G";

		using var testContext = departureMovement.CreateDeparturePhase5ValidationTestContext();
		testContext.EnableRule(x => x.IsRuleR0994_1Active);

		departureMovement.BM_GrossWeight = 101m;
		AssertHasWarning("BM_GrossWeight is more than Sum of Gross Weights of all House Consignments.", departureMovement.BM_GrossWeightInfo, message);

		departureMovement.BM_GrossWeight = 100m;
		AssertNoWarning("BM_GrossWeight is equal to Sum of Gross Weights of all House Consignments.", departureMovement.BM_GrossWeightInfo, message);

		departureMovement.BM_GrossWeight = 99m;
		AssertHasWarning("BM_GrossWeight is less than Sum of Gross Weights of all House Consignments.", departureMovement.BM_GrossWeightInfo, message);
	});

	public void TestCheckBM_GrossWeight_ConditionRuleR0994_1_WhenInactive() => CombineAssertions(() =>
	{
		const string messageError = "[R0994-1] Total Gross Weight on declaration should be equal or greater than the sum of Gross Weight of all House Consignments (100 kg)";
		var departureMovement = CreateDepartureMovement();
		var bill1 = departureMovement.Header.Bills.AddNew();
		var bill2 = departureMovement.Header.Bills.AddNew();
		bill1.B0_Weight = 50m;
		bill1.B0_WeightUQ = "KG";
		bill2.B0_Weight = 50000m;
		bill2.B0_WeightUQ = "G";

		AssertOnGrossWeightValue(101m);
		AssertOnGrossWeightValue(100m);
		AssertOnGrossWeightValue(99m);
		return;

		void AssertOnGrossWeightValue(ZDecimal grossWeight)
		{
			using var testContext = departureMovement.CreateDeparturePhase5ValidationTestContext();
			testContext.DisableRule(x => x.IsRuleR0994_1Active);

			departureMovement.BM_GrossWeight = grossWeight;
			AssertCollectionNotContains($"Expected notifications would not contain {messageError} on {grossWeight}m value", departureMovement.BM_GrossWeightInfo.Notifications.Select(e => e.Message), x => x.Contains(messageError));
			testContext.VerifyRule(x => x.IsRuleR0994_1Active, messagePrefix: $"{grossWeight}m");
		}
	});

	public void TestCheckBM_GrossWeight_RuleE1109()
	{
		var departureMovement = CreateDepartureMovement();
		var info = departureMovement.BM_GrossWeightInfo;
		const string message = "[E1109] Entered Gross Weight exceeding the max value supported Inside Transition Period (Total 11 digits with maximum of 3 decimals).";

		UniversalValidationHelperTest.AssertCheckMaxValueAndMaxDecimalLengthForWeightIfPhase5TransitionPeriod(info, message);
	}

	public void TestCheckRuleC0337_2()
	{
		const string message = "[C0337-2] Method of payment can be captured at consignment or house consignment level, but not at both levels.";
		var departureMovement = CreateDepartureMovement();
		var bill = departureMovement.Header.Bills.AddNew();

		CombineAssertions(() =>
		{
			testContext.EnableRule(decider => decider.IsRuleC0337_2Active);

			AssertNoMessageError("BM_MethodOfPayment and B0_TransportPaymentMethod are empty", departureMovement.BM_MethodOfPaymentInfo, message);

			departureMovement.BM_MethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;
			AssertNoMessageError("BM_MethodOfPayment not empty, B0_TransportPaymentMethod is empty", departureMovement.BM_MethodOfPaymentInfo, message);

			bill.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.Cheque;
			departureMovement.Validation.ValidateBM_MethodOfPayment();
			AssertHasMessageError("BM_MethodOfPayment and B0_TransportPaymentMethod not empty", departureMovement.BM_MethodOfPaymentInfo, message);

			testContext.DisableRule(decider => decider.IsRuleC0337_2Active);
			departureMovement.Validation.ValidateBM_MethodOfPayment();
			AssertNoMessageError("Rule C0337_2 Disabled, BM_MethodOfPayment and B0_TransportPaymentMethod not empty", departureMovement.BM_MethodOfPaymentInfo, message);
		});
	}

	public void TestCheckMandatoryGoodsItem()
	{
		const string message = "[TR0067] You need to supply at least one goods item.";

		var departureMovement = CreateDepartureMovement();

		using var testContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory);
		CombineAssertions(() =>
		{
			testContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0067Active));

			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageError("GoodsItems mandatory; TR0067 disabled", departureMovement.BM_InBondEntryTypeInfo, message);

			testContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0067Active));

			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertHasMessageError("GoodsItems mandatory; TR0067 enabled", departureMovement.BM_InBondEntryTypeInfo, message);

			var bill = departureMovement.Header.Bills.AddNew();
			bill.GoodsItems.AddNew();
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageError("GoodsItems mandatory", departureMovement.BM_InBondEntryTypeInfo, message);
		});
	}

	public void TestCheckBM_RN_NKCountryOfDispatch()
	{
		var departureMovement = CreateDepartureMovement();

		ValidationTestHelper.AssertInvalidCodeMessageError(departureMovement.BM_RN_NKCountryOfDispatchInfo, "@@", "AU");
	}

	public void TestCheckRuleC0909_WhenActive()
	{
		const string message = "[C0909] You have not entered Country Of Dispatch. It is required either on Declaration or House Consignment or House Consignment Item.";
		var departureMovement = CreateDepartureMovement();
		var bill = departureMovement.Header.Bills.AddNew();
		var goodsItem = bill.GoodsItems.AddNew();
		CombineAssertions(() =>
		{
			testContext.EnableRule(c => c.IsRuleC0909Active);
			departureMovement.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T2;
			goodsItem.BY_RN_NKCountryOfDispatch = China;
			bill.B0_RN_NKCountryOfExport = China;
			departureMovement.BM_RN_NKCountryOfDispatch = China;
			AssertNoMessageError("MovementHeader's BM_InBondEntryType is not TIR", departureMovement.BM_RN_NKCountryOfDispatchInfo, message);

			departureMovement.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
			departureMovement.Validation.ValidateBM_RN_NKCountryOfDispatch();
			AssertNoMessageError("MovementHeader's BM_InBondEntryType is TIR", departureMovement.BM_RN_NKCountryOfDispatchInfo, message);

			departureMovement.BM_RN_NKCountryOfDispatch = ZString.Empty;
			AssertNoMessageError("Dispatch Country haven't specified on declaration level", departureMovement.BM_RN_NKCountryOfDispatchInfo, message);

			bill.B0_RN_NKCountryOfExport = ZString.Empty;
			departureMovement.BM_RN_NKCountryOfDispatch = China;
			AssertNoMessageError("Dispatch Country haven't specified on house consignment level", departureMovement.BM_RN_NKCountryOfDispatchInfo, message);

			bill.B0_RN_NKCountryOfExport = China;
			goodsItem.BY_RN_NKCountryOfDispatch = ZString.Empty;
			departureMovement.Validation.ValidateBM_RN_NKCountryOfDispatch();
			AssertNoMessageError("Dispatch Country haven't specified on goods items level", departureMovement.BM_RN_NKCountryOfDispatchInfo, message);

			bill.B0_RN_NKCountryOfExport = ZString.Empty;
			departureMovement.Validation.ValidateBM_RN_NKCountryOfDispatch();
			AssertNoMessageError("Dispatch Country haven't specified on goods items or house consignment level", departureMovement.BM_RN_NKCountryOfDispatchInfo, message);

			bill.B0_RN_NKCountryOfExport = China;
			departureMovement.BM_RN_NKCountryOfDispatch = ZString.Empty;
			AssertNoMessageError("Dispatch Country haven't specified on goods items or declaration level", departureMovement.BM_RN_NKCountryOfDispatchInfo, message);

			bill.B0_RN_NKCountryOfExport = ZString.Empty;
			goodsItem.BY_RN_NKCountryOfDispatch = China;
			departureMovement.Validation.ValidateBM_RN_NKCountryOfDispatch();
			AssertNoMessageError("Dispatch Country haven't specified on house consignment or declaration level", departureMovement.BM_RN_NKCountryOfDispatchInfo, message);

			goodsItem.BY_RN_NKCountryOfDispatch = ZString.Empty;
			departureMovement.Validation.ValidateBM_RN_NKCountryOfDispatch();
			AssertHasMessageError("Dispatch Country haven't specified on goods items, house consignment or declaration level", departureMovement.BM_RN_NKCountryOfDispatchInfo, message);
		});
	}

	public void TestCheckRuleC0909_WhenInactive()
	{
		const string message = "[C0909] You have not entered Country Of Dispatch. It is required either on Declaration or House Consignment or House Consignment Item.";

		testContext.DisableRule(c => c.IsRuleC0909Active);
		var departureMovement = CreateDepartureMovement();
		var bill = departureMovement.Header.Bills.AddNew();
		bill.GoodsItems.AddNew();
		departureMovement.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
		departureMovement.Validation.ValidateBM_RN_NKCountryOfDispatch();

		AssertCollectionNotContains($"Expected notifications would not contain {message}", departureMovement.BM_RN_NKCountryOfDispatchInfo.Notifications.Select(e => e.Message), x => x.Contains(message));
	}

	public void TestCheckTirCarnetNumber_MaxLength()
	{
		const string messageError = "The maximum length for TIR Carnet Num. is 12 characters.";
		var departureMovement = CreateDepartureMovement();
		var targetInfo = departureMovement.TirCarnetNumberInfo;
		departureMovement.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
		CombineAssertions(() =>
		{
			departureMovement.TirCarnetNumber = ZString.Empty.PadLeft(12, 'A');
			AssertNoMessageError("Valid length for TirCarnetNumber", targetInfo, messageError);

			departureMovement.TirCarnetNumber = ZString.Empty.PadLeft(13, 'A');
			AssertHasMessageError("Invalid length for TirCarnetNumber", targetInfo, messageError);
		});
	}

	public void TestCheckTirCarnetNumber_ConditionR990()
	{
		const string errorMessageAlgorithm = "(R0990) TIR Carnet Number is not as per IRU Algorithm.";
		const string errorMessageLength = "(R0990) TIR Carnet Number can have a format of an10 or an11.";
		var departureMovement = CreateDepartureMovement();
		var targetInfo = departureMovement.TirCarnetNumberInfo;
		departureMovement.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;

		CombineAssertions(() =>
		{
			testContext.EnableRule(v => v.IsRuleR0990Active);
			departureMovement.TirCarnetNumber = "25000000";
			AssertNoMessageErrorContaining("Valid TIR Carnet Number", targetInfo, errorMessageAlgorithm);
			departureMovement.TirCarnetNumber = "25000001";
			AssertHasMessageErrorContaining("Invalid TIR Carnet Number", targetInfo, errorMessageAlgorithm);
			AssertHasMessageErrorContaining("Invalid TIR Carnet Number", targetInfo, errorMessageLength);
			departureMovement.TirCarnetNumber = "FR25000001";
			AssertNoMessageErrorContaining("Valid TIR Carnet Number", targetInfo, errorMessageLength);
		});
	}

	public void TestBM_ActiveBorderIdentificationTypeMandatoryIfCheckCusTransportMeansExist()
	{
		var message = "Enter the first Transport in the fields of Transport Border";
		var departureMovement = CreateDepartureMovement();
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
		departureMovement.BM_ActiveBorderIdentificationType = ZString.Empty;

		CombineAssertions(() =>
		{
			AssertNoError("No CusTransportMeans, no error", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);

			departureMovement.AdditionalTransportAtBorderList.AddNew();
			AssertHasError("CusTransportMeans exist, has error", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);

			departureMovement.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._40;
			AssertNoError("CusTransportMeans exist, no error", departureMovement.BM_ActiveBorderIdentificationTypeInfo, message);
		});
	}

	public void TestCheckBM_RL_NKDestinationPort_RuleC0343_WhenActive() => CombineAssertions(() =>
	{
		const string messageError = "[C0343] You have not entered Country of Destination. It is required either on Declaration or House Consignment Item.";
		var departureMovement = CreateDepartureMovement();
		var nctsBill = departureMovement.Header.Bills.AddNew();

		testContext.EnableRule(c => c.IsRuleC0343Active);

		departureMovement.BM_RL_NKDestinationPort = ZString.Empty;
		nctsBill.B0_RN_NKCountryOfDestination = ZString.Empty;
		AssertHasMessageError("Empty BM_RL_NKDestinationPort and B0_RN_NKCountryOfDestination", departureMovement.BM_RL_NKDestinationPortInfo, messageError);

		nctsBill.B0_RN_NKCountryOfDestination = ZString.Empty;
		departureMovement.BM_RL_NKDestinationPort = Germany;
		AssertNoMessageError("Not empty BM_RL_NKDestinationPort and empty B0_RN_NKCountryOfDestination", departureMovement.BM_RL_NKDestinationPortInfo, messageError);

		nctsBill.B0_RN_NKCountryOfDestination = Germany;
		departureMovement.BM_RL_NKDestinationPort = ZString.Empty;
		AssertNoMessageError("Empty BM_RL_NKDestinationPort and not empty B0_RN_NKCountryOfDestination", departureMovement.BM_RL_NKDestinationPortInfo, messageError);
	});

	public void TestCheckBM_RL_NKDestinationPort_RuleC0343_WhenNotActive()
	{
		const string messageError = "[C0343] You have not not entered Country of Destination. It is required either on Declaration or House Consignment Item.";
		var departureMovement = CreateDepartureMovement();
		departureMovement.Header.Bills.AddNew();
		testContext.DisableRule(c => c.IsRuleC0343Active);
		departureMovement.BM_RL_NKDestinationPort = ZString.Empty;
		departureMovement.Validation.ValidateBM_RL_NKDestinationPort();

		AssertCollectionNotContains($"Expected notifications would not contain {messageError}", departureMovement.BM_RL_NKDestinationPortInfo.Notifications.Select(e => e.Message), x => x.Contains(messageError));
	}

	public void TestCheckBM_RL_NKDestinationPort_RuleC0343_2() => CombineAssertions(() =>
	{
		const string messageError = "[C0343-2] Destination Country/Region must be filled either on Declaration, House Consignment or Goods Item Level.";
		testContext.EnableRule(c => c.IsRuleC0343_2Active);
		var departureMovement = CreateDepartureMovement();
		var nctsBill = departureMovement.Header.Bills.AddNew();
		var goodsItem1 = nctsBill.GoodsItems.AddNew();

		nctsBill.B0_RN_NKCountryOfDestination = ZString.Empty;
		goodsItem1.BY_RN_NKCountryOfDestination = ZString.Empty;
		departureMovement.BM_RL_NKDestinationPort = ZString.Empty;
		AssertHasMessageError("All fields are empty", departureMovement.BM_RL_NKDestinationPortInfo, messageError);

		nctsBill.B0_RN_NKCountryOfDestination = Germany;
		goodsItem1.BY_RN_NKCountryOfDestination = AlandIslands;
		departureMovement.BM_RL_NKDestinationPort = Germany;
		departureMovement.Validation.ValidateBM_RL_NKDestinationPort();
		AssertHasMessageError("All the three fields have CountryOfDestination", departureMovement.BM_RL_NKDestinationPortInfo, messageError);

		nctsBill.B0_RN_NKCountryOfDestination = ZString.Empty;
		goodsItem1.BY_RN_NKCountryOfDestination = ZString.Empty;
		departureMovement.BM_RL_NKDestinationPort = Germany;
		AssertNoMessageError("Only Details tab has country of destination", departureMovement.BM_RL_NKDestinationPortInfo, messageError);

		nctsBill.B0_RN_NKCountryOfDestination = ZString.Empty;
		goodsItem1.BY_RN_NKCountryOfDestination = AlandIslands;
		departureMovement.BM_RL_NKDestinationPort = ZString.Empty;
		departureMovement.Validation.ValidateBM_RL_NKDestinationPort();
		AssertNoMessageError("Only Goods Items have country of destination", departureMovement.BM_RL_NKDestinationPortInfo, messageError);

		nctsBill.B0_RN_NKCountryOfDestination = Germany;
		goodsItem1.BY_RN_NKCountryOfDestination = ZString.Empty;
		departureMovement.Validation.ValidateBM_RL_NKDestinationPort();
		AssertNoMessageError("Only House Consignment has country of destination ", departureMovement.BM_RL_NKDestinationPortInfo, messageError);

		testContext.DisableRule(c => c.IsRuleC0343_2Active);
		nctsBill.B0_RN_NKCountryOfDestination = ZString.Empty;
		goodsItem1.BY_RN_NKCountryOfDestination = ZString.Empty;
		departureMovement.BM_RL_NKDestinationPort = ZString.Empty;
		departureMovement.Validation.ValidateBM_RL_NKDestinationPort();
		AssertNoMessageError("No message error should show when rule is not active.", departureMovement.BM_RL_NKDestinationPortInfo, messageError);
	});

	public void TestCheckBM_RL_NKDestinationPort_RuleC0343_2_ForMultipleGoodsItems() => CombineAssertions(() =>
	{
		const string messageError = "[C0343-2] Destination Country/Region must be filled either on Declaration, House Consignment or Goods Item Level.";
		testContext.EnableRule(c => c.IsRuleC0343_2Active);
		var departureMovement = CreateDepartureMovement();
		var nctsBill = departureMovement.Header.Bills.AddNew();
		nctsBill.B0_RN_NKCountryOfDestination = ZString.Empty;

		var goodsItem1 = nctsBill.GoodsItems.AddNew();
		var goodsItem2 = nctsBill.GoodsItems.AddNew();

		goodsItem1.BY_RN_NKCountryOfDestination = France;
		goodsItem2.BY_RN_NKCountryOfDestination = Germany;
		departureMovement.BM_RL_NKDestinationPort = ZString.Empty;
		AssertNoMessageError("Not empty BY_RN_NKCountryOfDestination for All Goods Items", departureMovement.BM_RL_NKDestinationPortInfo, messageError);

		goodsItem1.BY_RN_NKCountryOfDestination = ZString.Empty;
		goodsItem2.BY_RN_NKCountryOfDestination = ZString.Empty;
		departureMovement.Validation.ValidateBM_RL_NKDestinationPort();
		AssertHasMessageError("Empty BY_RN_NKCountryOfDestination for All Goods Items", departureMovement.BM_RL_NKDestinationPortInfo, messageError);

		goodsItem1.BY_RN_NKCountryOfDestination = France;
		goodsItem2.BY_RN_NKCountryOfDestination = ZString.Empty;
		departureMovement.Validation.ValidateBM_RL_NKDestinationPort();
		AssertHasMessageError("Empty BY_RN_NKCountryOfDestination for Some Goods Items", departureMovement.BM_RL_NKDestinationPortInfo, messageError);

		testContext.DisableRule(c => c.IsRuleC0343_2Active);
		departureMovement.Validation.ValidateBM_RL_NKDestinationPort();
		AssertNoMessageError("No message error should show when rule is not active.", departureMovement.BM_RL_NKDestinationPortInfo, messageError);
	});

	public void TestBM_InBondEntryType_RuleB1836()
	{
		const string errorMessage = "[B1836] Customs Office with Purpose='TRA' is required.";

		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(NctsMovementType.Codes.Departure);
		testContext.AddAutoCacheResetObject(header);

		var movementHeader = header.MovementHeader;
		testContext.AddAutoCacheResetObject(movementHeader);

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		testContext.EnableRule(c => c.IsRuleB1836Active);

		movementHeader.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5;
		AssertHasMessageError("BM_InBondEntryType = T.", movementHeader.BM_InBondEntryTypeInfo, errorMessage);

		movementHeader.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
		AssertHasMessageError("BM_InBondEntryType = T2.", movementHeader.BM_InBondEntryTypeInfo, errorMessage);

		movementHeader.BM_InBondEntryType = ZString.Empty;
		AssertNoMessageError("BM_InBondEntryType is not T or T2", movementHeader.BM_InBondEntryTypeInfo, errorMessage);

		testContext.DisableRule(c => c.IsRuleB1836Active);
		movementHeader.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("The rule B1836 is inactive.", movementHeader.BM_InBondEntryTypeInfo, errorMessage);

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		testContext.EnableRule(c => c.IsRuleB1836Active);

		movementHeader.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("Outside Phase5 Transition Period.", movementHeader.BM_InBondEntryTypeInfo, errorMessage);
	}

	public void TestBM_InBondEntryType_RuleC0030_Active_RuleB1836_Inactive_NotInPhase5TransitionPeriod()
	{
		var header = Factory.New<NctsHeader>();
		testContext.AddAutoCacheResetObject(header);
		header.SetMovementType(NctsMovementType.Codes.Departure);

		var movementHeader = header.MovementHeader;
		testContext.AddAutoCacheResetObject(movementHeader);

		const string errorMessage = "[C0030] You have not entered a Customs Office Of Transit Declared (TRA).";
		testContext.EnableRule(c => c.IsRuleC0030Active);
		testContext.DisableRule(c => c.IsRuleB1836Active);

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;
		AssertHasMessageError("BM_InBondEntryType T2", movementHeader.BM_InBondEntryTypeInfo, errorMessage);

		movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T;
		AssertNoMessageError("BM_InBondEntryType T", movementHeader.BM_InBondEntryTypeInfo, errorMessage);

		var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
		goodsItem.BY_Type = NctsPhase5DeclarationTypeList.Codes.T2;
		movementHeader.Validation.ValidateBM_InBondEntryType();
		AssertHasMessageError("BM_InBondEntryType T, BY_Type T2", movementHeader.BM_InBondEntryTypeInfo, errorMessage);

		goodsItem.BY_Type = NctsPhase5DeclarationTypeList.Codes.T2F;
		movementHeader.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("BM_InBondEntryType T, BY_Type Not T2", movementHeader.BM_InBondEntryTypeInfo, errorMessage);

		movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
		movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;
		AssertNoMessageError("HasTransitOffice, BM_InBondEntryType T2", movementHeader.BM_InBondEntryTypeInfo, errorMessage);

		movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T;
		goodsItem.BY_Type = NctsPhase5DeclarationTypeList.Codes.T2;
		movementHeader.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("HasTransitOffice, BM_InBondEntryType T2 , BY_Type T2", movementHeader.BM_InBondEntryTypeInfo, errorMessage);
	}

	public void TestBM_InBondEntryType_RuleC0030_Active_RuleB1836_Active_InPhase5TransitionPeriod()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(NctsMovementType.Codes.Departure);

		var movementHeader = header.MovementHeader;

		const string errorMessage = "[C0030] You have not entered a Customs Office Of Transit Declared (TRA).";
		testContext.EnableRule(c => c.IsRuleC0030Active);
		testContext.EnableRule(c => c.IsRuleB1836Active);

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;
		AssertEquals("BM_InBondEntryType T2", false, movementHeader.BM_InBondEntryTypeInfo.Notifications.Select(x => x.Message).Contains(errorMessage));

		movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T;
		header.Bills.AddNew().GoodsItems.AddNew().BY_Type = NctsPhase5DeclarationTypeList.Codes.T2;
		movementHeader.Validation.ValidateBM_InBondEntryType();
		AssertEquals("BM_InBondEntryType T, BY_Type T2", false, movementHeader.BM_InBondEntryTypeInfo.Notifications.Select(x => x.Message).Contains(errorMessage));
	}

	public void TestBM_InBondEntryType_RuleC0030_Active_RuleB1836_Inactive_InPhase5TransitionPeriod()
	{
		testContext.DisableRule(c => c.IsRuleB1836Active);
		testContext.EnableRule(c => c.IsRuleC0030Active);
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(NctsMovementType.Codes.Departure);

		var movementHeader = header.MovementHeader;

		const string errorMessage = "[C0030] You have not entered a Customs Office Of Transit Declared (TRA).";

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;
		AssertEquals("BM_InBondEntryType T2", true, movementHeader.BM_InBondEntryTypeInfo.Notifications.Select(x => x.Message).Contains(errorMessage));

		movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T;
		header.Bills.AddNew().GoodsItems.AddNew().BY_Type = NctsPhase5DeclarationTypeList.Codes.T2;
		movementHeader.Validation.ValidateBM_InBondEntryType();
		AssertEquals("BM_InBondEntryType T, BY_Type T2", true, movementHeader.BM_InBondEntryTypeInfo.Notifications.Select(x => x.Message).Contains(errorMessage));
	}

	public void TestBM_InBondEntryType_RuleC0030_Inactive_RuleB1836_Inactive_NotInPhase5TransitionPeriod()
	{
		testContext.DisableRule(c => c.IsRuleC0030Active);
		testContext.DisableRule(c => c.IsRuleB1836Active);

		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(NctsMovementType.Codes.Departure);

		var movementHeader = header.MovementHeader;

		const string errorMessage = "[C0030] You have not entered a Customs Office Of Transit Declared (TRA).";
		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;
		AssertEquals("BM_InBondEntryType T2", false, movementHeader.BM_InBondEntryTypeInfo.Notifications.Select(x => x.Message).Contains(errorMessage));

		movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T;
		header.Bills.AddNew().GoodsItems.AddNew().BY_Type = NctsPhase5DeclarationTypeList.Codes.T2;
		movementHeader.Validation.ValidateBM_InBondEntryType();
		AssertEquals("BM_InBondEntryType T, BY_Type T2", false, movementHeader.BM_InBondEntryTypeInfo.Notifications.Select(x => x.Message).Contains(errorMessage));
	}

	public void TestBM_InBondEntryType_RuleC0030_Inactive_RuleB1836_Inactive_InPhase5TransitionPeriod()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(NctsMovementType.Codes.Departure);

		var movementHeader = header.MovementHeader;

		const string errorMessage = "[C0030] You have not entered a Customs Office Of Transit Declared (TRA).";
		testContext.DisableRule(c => c.IsRuleC0030Active);
		testContext.DisableRule(c => c.IsRuleB1836Active);

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;
		AssertEquals("BM_InBondEntryType T2", false, movementHeader.BM_InBondEntryTypeInfo.Notifications.Select(x => x.Message).Contains(errorMessage));

		movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T;
		header.Bills.AddNew().GoodsItems.AddNew().BY_Type = NctsPhase5DeclarationTypeList.Codes.T2;
		movementHeader.Validation.ValidateBM_InBondEntryType();
		AssertEquals("BM_InBondEntryType T, BY_Type T2", false, movementHeader.BM_InBondEntryTypeInfo.Notifications.Select(x => x.Message).Contains(errorMessage));
	}

	public void TestBM_InBondEntryType_RuleRP16()
	{
		const string messageError = "[RP16] You have not entered Guarantee.";
		var departureMovement = CreateDepartureMovement();
		var nctsHeader = departureMovement.Header;

		CombineAssertions(() =>
		{
			using var testContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory);
			testContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleRP16Active));
			var cusAuthorizationUsage = departureMovement.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Code = AuthorizationCodes.AuthorizedConsignorTransit;
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.T2PlusSanMarino;
			AssertNoError("The RP rule is inactive", departureMovement.BM_InBondEntryTypeInfo, messageError);

			testContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleRP16Active));
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertHasError("The guarantee shouldn't be null", departureMovement.BM_InBondEntryTypeInfo, messageError);

			departureMovement.Guarantees.AddNew().PW_ActivityCode = "1";
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.TirDeclaration;
			AssertNoError("The rule RP16 should be skipped when the declaration type is TIR", departureMovement.BM_InBondEntryTypeInfo, messageError);

			nctsHeader.MovementHeader.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.T2PlusSanMarino;
			AssertNoError("The guarantee is not null", departureMovement.BM_InBondEntryTypeInfo, messageError);

			departureMovement.Guarantees.RemoveAll();
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.T2PlusSanMarino;
			AssertHasError("The guarantee shouldn't be null", departureMovement.BM_InBondEntryTypeInfo, messageError);

			cusAuthorizationUsage.AGC_Code = AuthorizationCodes.EndUse;
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertNoError("There is an AGC Code in Authorizationusage is not 0", departureMovement.BM_InBondEntryTypeInfo, messageError);
		});
	}

	public void TestCheckBM_InBondEntryType_RulePLR0601() => CombineAssertions(() =>
	{
		TestCheckBM_InBondEntryType_R601_Phase5(true);
		TestCheckBM_InBondEntryType_R601_Phase5(false);
		return;

		void TestCheckBM_InBondEntryType_R601_Phase5(bool isRuleActive)
		{
			testContext.SetRuleStatus(v => v.IsRulePLR0601Active, isRuleActive);
			var departureMovement = CreateDepartureMovement();
			var nctsHeader = departureMovement.Header;
			const string documentCodeToTriggerRule = "C651";

			nctsHeader.Bills.AddNew()
					.GoodsItems.AddNew()
					.SupportingDocuments.AddNew().CSI_Code = documentCodeToTriggerRule;

			ValidateInBondEntryTypeAndAssertR601MessageError(departureMovement, "For NCTS5", isRuleActive);
		}
	});

	public void TestCheckBM_InBondEntryType_RulePLR0601_SupportingDocument_Code() => CombineAssertions("R0601 is active only for supporting documents with codes C651 and C658", () =>
	{
		TestCheckBM_InBondEntryType_R601SupportingDocument_Code(true);
		TestCheckBM_InBondEntryType_R601SupportingDocument_Code(false);
		return;

		void TestCheckBM_InBondEntryType_R601SupportingDocument_Code(bool isRuleActive)
		{
			testContext.SetRuleStatus(v => v.IsRulePLR0601Active, isRuleActive);
			var departureMovement = CreateDepartureMovement();
			var nctsHeader = departureMovement.Header;
			departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();

			ValidateInBondEntryTypeAndAssertR601MessageError(departureMovement, "Without supporting documents", false);

			var document = goodsItem.SupportingDocuments.AddNew();
			foreach (var activeCode in new[] { "C651", "C658" })
			{
				document.CSI_Code = activeCode;
				ValidateInBondEntryTypeAndAssertR601MessageError(departureMovement, $"Active document code {activeCode}", isRuleActive);
			}

			const string ignoredDocumentCode = "C600";
			document.CSI_Code = ignoredDocumentCode;
			ValidateInBondEntryTypeAndAssertR601MessageError(departureMovement, "Ignored document code", false);
		}
	});

	public void TestCheckBM_InBondEntryType_RulePLR0601_SupportingDocument_TypeT1() => CombineAssertions("R0601 accepts T1 for empty BY_Type", () =>
	{
		TestCheckBM_InBondEntryType_R601SupportingDocument_TypeT1(true);
		TestCheckBM_InBondEntryType_R601SupportingDocument_TypeT1(false);
		return;

		void TestCheckBM_InBondEntryType_R601SupportingDocument_TypeT1(bool isRuleActive)
		{
			testContext.SetRuleStatus(v => v.IsRulePLR0601Active, isRuleActive);
			var departureMovement = CreateDepartureMovement();
			var nctsHeader = departureMovement.Header;
			const string documentCodeToTriggerRule = "C651";
			departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			goodsItem.SupportingDocuments.AddNew().CSI_Code = documentCodeToTriggerRule;

			departureMovement.BM_InBondEntryType = "T1";
			goodsItem.BY_Type = "T2";
			ValidateInBondEntryTypeAndAssertR601MessageError(departureMovement, "BY_Type is not empty", isRuleActive);

			goodsItem.BY_Type = "";
			ValidateInBondEntryTypeAndAssertR601MessageError(departureMovement, "BY_Type is empty", false);
		}
	});

	public void TestCheckBM_InBondEntryType_RulePLR0601_SupportingDocument_TypeT() => CombineAssertions("R0601 accepts T for BY_Type=T1", () =>
	{
		TestCheckBM_InBondEntryType_R601SupportingDocument_TypeT(true);
		TestCheckBM_InBondEntryType_R601SupportingDocument_TypeT(false);
		return;

		void TestCheckBM_InBondEntryType_R601SupportingDocument_TypeT(bool isRuleActive)
		{
			testContext.SetRuleStatus(v => v.IsRulePLR0601Active, isRuleActive);
			var departureMovement = CreateDepartureMovement();
			var nctsHeader = departureMovement.Header;
			const string documentCodeToTriggerRule = "C651";
			departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			goodsItem.SupportingDocuments.AddNew().CSI_Code = documentCodeToTriggerRule;

			departureMovement.BM_InBondEntryType = "T";
			goodsItem.BY_Type = "";
			ValidateInBondEntryTypeAndAssertR601MessageError(departureMovement, "BY_Type is not T1", isRuleActive);

			goodsItem.BY_Type = "T1";
			departureMovement.BM_InBondEntryType = "T";
			ValidateInBondEntryTypeAndAssertR601MessageError(departureMovement, "BY_Type is T1", false);
		}
	});

	public void TestCheckBM_InBondEntryType_RulePLR0601_PreviousDocument_Code() => CombineAssertions("R0601 is active only for previous documents with codes C651 and C658", () =>
	{
		TestCheckBM_InBondEntryType_R601PreviousDocument_Code(true);
		TestCheckBM_InBondEntryType_R601PreviousDocument_Code(false);
		return;

		void TestCheckBM_InBondEntryType_R601PreviousDocument_Code(bool isRuleActive)
		{
			testContext.SetRuleStatus(v => v.IsRulePLR0601Active, isRuleActive);
			var departureMovement = CreateDepartureMovement();
			var nctsHeader = departureMovement.Header;
			departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();

			ValidateInBondEntryTypeAndAssertR601MessageError(departureMovement, "Without previous documents", false, forPreviousDocs: true);

			var document = goodsItem.PreviousDocuments.AddNew();
			foreach (var activeCode in new[] { "C651", "C658" })
			{
				document.CSI_Code = activeCode;
				ValidateInBondEntryTypeAndAssertR601MessageError(departureMovement, $"Active document code {activeCode}", isRuleActive, forPreviousDocs: true);
			}

			const string ignoredDocumentCode = "C600";
			document.CSI_Code = ignoredDocumentCode;
			ValidateInBondEntryTypeAndAssertR601MessageError(departureMovement, "Ignored document code", false, forPreviousDocs: true);
		}
	});

	public void TestCheckBM_InBondEntryType_RulePLR0601_PreviousDocument_TypeT2() => CombineAssertions("R0601 accepts T2 for empty BY_Type", () =>
	{
		TestCheckBM_InBondEntryType_R601PreviousDocument_TypeT2(true);
		TestCheckBM_InBondEntryType_R601PreviousDocument_TypeT2(false);
		return;

		void TestCheckBM_InBondEntryType_R601PreviousDocument_TypeT2(bool isRuleActive)
		{
			testContext.SetRuleStatus(v => v.IsRulePLR0601Active, isRuleActive);
			var departureMovement = CreateDepartureMovement();
			var nctsHeader = departureMovement.Header;
			const string documentCodeToTriggerRule = "C651";
			departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			goodsItem.PreviousDocuments.AddNew().CSI_Code = documentCodeToTriggerRule;

			departureMovement.BM_InBondEntryType = "T2";
			goodsItem.BY_Type = "T1";
			ValidateInBondEntryTypeAndAssertR601MessageError(departureMovement, "BY_Type is not empty", isRuleActive, forPreviousDocs: true);

			goodsItem.BY_Type = "";
			ValidateInBondEntryTypeAndAssertR601MessageError(departureMovement, "BY_Type is empty", false, forPreviousDocs: true);
		}
	});

	public void TestCheckBM_InBondEntryType_RulePLR0601_PreviousDocument_TypeT2F() => CombineAssertions("R0601 accepts T2F for empty BY_Type", () =>
	{
		TestCheckBM_InBondEntryType_R601PreviousDocument_TypeT2F(true);
		TestCheckBM_InBondEntryType_R601PreviousDocument_TypeT2F(false);
		return;

		void TestCheckBM_InBondEntryType_R601PreviousDocument_TypeT2F(bool isRuleActive)
		{
			testContext.SetRuleStatus(v => v.IsRulePLR0601Active, isRuleActive);
			var departureMovement = CreateDepartureMovement();
			var nctsHeader = departureMovement.Header;
			const string documentCodeToTriggerRule = "C651";
			departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			goodsItem.PreviousDocuments.AddNew().CSI_Code = documentCodeToTriggerRule;

			departureMovement.BM_InBondEntryType = "T2F";
			goodsItem.BY_Type = "T1";
			ValidateInBondEntryTypeAndAssertR601MessageError(departureMovement, "BY_Type is not empty", isRuleActive, forPreviousDocs: true);

			goodsItem.BY_Type = "";
			ValidateInBondEntryTypeAndAssertR601MessageError(departureMovement, "BY_Type is empty", false, forPreviousDocs: true);
		}
	});

	public void TestCheckBM_InBondEntryType_RulePLR0601_PreviousDocument_TypeT() => CombineAssertions("R0601 accepts T for BY_Type in [T2, T2F]", () =>
	{
		TestCheckBM_InBondEntryType_R601PreviousDocument_TypeT(true);
		TestCheckBM_InBondEntryType_R601PreviousDocument_TypeT(false);
		return;

		void TestCheckBM_InBondEntryType_R601PreviousDocument_TypeT(bool isRuleActive)
		{
			testContext.SetRuleStatus(v => v.IsRulePLR0601Active, isRuleActive);
			var departureMovement = CreateDepartureMovement();
			var nctsHeader = departureMovement.Header;
			const string documentCodeToTriggerRule = "C651";
			departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			goodsItem.PreviousDocuments.AddNew().CSI_Code = documentCodeToTriggerRule;

			goodsItem.BY_Type = "T1";
			departureMovement.BM_InBondEntryType = "T";
			ValidateInBondEntryTypeAndAssertR601MessageError(departureMovement, "BY_Type is not in [T2, T2F]", isRuleActive, forPreviousDocs: true);

			goodsItem.BY_Type = "T2";
			departureMovement.BM_InBondEntryType = "T";
			ValidateInBondEntryTypeAndAssertR601MessageError(departureMovement, "BY_Type is T2", false, forPreviousDocs: true);

			goodsItem.BY_Type = "T2F";
			departureMovement.BM_InBondEntryType = "T";
			ValidateInBondEntryTypeAndAssertR601MessageError(departureMovement, "BY_Type is T2F", false, forPreviousDocs: true);
		}
	});

	public void TestCheckBM_InBondEntryType_R0900_4()
	{
		const string messageError = "[R0900-4] Guarantee data group is mandatory";

		var departureMovement = CreateDepartureMovement();
		departureMovement.Guarantees.AddNew();

		CombineAssertions(() =>
		{
			testContext.EnableRule(x => x.IsRuleR0900_4Active);
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageError("When EnableRule IsRuleR0900_4Active and Guarantees is not empty, no error", departureMovement.BM_InBondEntryTypeInfo, messageError);

			departureMovement.Guarantees.DeleteAll();
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertHasMessageErrorContaining("When EnableRule IsRuleR0900_4Active and Guarantees is empty, error", departureMovement.BM_InBondEntryTypeInfo, messageError);

			testContext.DisableRule(x => x.IsRuleR0900_4Active);
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageError("When DisableRule IsRuleR0900_4Active and Guarantees is empty, no error", departureMovement.BM_InBondEntryTypeInfo, messageError);
		});
	}

	public void TestCheckBM_ExportTransportMode_C0599()
	{
		var departureMovement = CreateDepartureMovement();

		const string errorMessage = "[C0599] You have not entered a Transport Border Mode of Transport.";
		departureMovement.CustomsOfficesForDeparture.RemoveAndDeleteAll();
		departureMovement.CustomsOfficesForDeparture.AddNew();

		testContext.EnableRule(decider => decider.IsRuleC0599Active);

		CombineAssertions(() =>
		{
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			departureMovement.BM_ExportTransportMode = ZString.Empty;
			AssertNoMessageError("No Error When TypeOfSecurity is NON, AdditionalDeclarationType is not A and ExportTransportMode is Empty", departureMovement.BM_ExportTransportModeInfo, errorMessage);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			departureMovement.BM_ExportTransportMode = ZString.Empty;
			AssertNoMessageError("No Error When TypeOfSecurity is not Non, AdditionalDeclarationType is not A and ExportTransportMode is Empty", departureMovement.BM_ExportTransportModeInfo, errorMessage);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
			AssertNoMessageError("No Error When TypeOfSecurity is not Non, AdditionalDeclarationType is A and ExportTransportMode is Not Empty", departureMovement.BM_ExportTransportModeInfo, errorMessage);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			departureMovement.BM_ExportTransportMode = ZString.Empty;
			AssertHasMessageError("Show Error When TypeOfSecurity is not Non, AdditionalDeclarationType is A and ExportTransportMode is Empty", departureMovement.BM_ExportTransportModeInfo, errorMessage);

			testContext.DisableRule(decider => decider.IsRuleC0599Active);
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			departureMovement.BM_ExportTransportMode = ZString.Empty;
			AssertNoMessageError("Rule C0599 disabled, no Error When TypeOfSecurity is not Non, AdditionalDeclarationType is A and ExportTransportMode is Empty", departureMovement.BM_ExportTransportModeInfo, errorMessage);
		});
	}

	public void TestCheckBM_ExportTransportMode_B1889_WhenActive()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010);
		Factory.Save();

		const string errorMessage = "[B1889] You have not entered a Transport Border Mode of Transport.";

		CombineAssertions(() =>
		{
			testContext.EnableRule(decider => decider.IsRuleB1889Active);
			testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
			var departureMovement = CreateDepartureMovement();
			departureMovement.CustomsOfficesForDeparture.RemoveAndDeleteAll();
			var office = departureMovement.CustomsOfficesForDeparture.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "PL000001");
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			departureMovement.BM_ExportTransportMode = ZString.Empty;
			office.CY_Data = "PL";
			AssertNoMessageError("No Error When TypeOfSecurity is Non, Departure Customs Office is not in CL010 and ExportTransportMode is Empty", departureMovement.BM_ExportTransportModeInfo, errorMessage);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
			office.CY_Data = "PL";
			AssertNoMessageError("No Error When TypeOfSecurity is not Non, Departure Customs Office is not in CL010 and ExportTransportMode is not Empty", departureMovement.BM_ExportTransportModeInfo, errorMessage);

			departureMovement.CustomsOfficesForDeparture.RemoveAndDeleteAll();
			office = departureMovement.CustomsOfficesForDeparture.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "PL000001");
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
			departureMovement.BM_ExportTransportMode = ZString.Empty;
			office.CY_Data = "PL";
			departureMovement.Validation.ValidateAll();
			AssertHasMessageError("Show Error When TypeOfSecurity is not Non, Departure Customs Office is not in CL010 and ExportTransportMode is Empty", departureMovement.BM_ExportTransportModeInfo, errorMessage);

			departureMovement.CustomsOfficesForDeparture.RemoveAndDeleteAll();
			office = departureMovement.CustomsOfficesForDeparture.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "AU000001");
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
			departureMovement.BM_ExportTransportMode = ZString.Empty;
			office.CY_Data = "AU";
			AssertNoMessageError("Show Error When TypeOfSecurity is not Non, Departure Customs Office is in CL010 and ExportTransportMode is Empty", departureMovement.BM_ExportTransportModeInfo, errorMessage);
		});
	}

	public void TestCheckBM_ExportTransportMode_R0789_1()
	{
		const string expectedErrorMessage = "[R0789-1] You have not entered Transport Border Details.";

		testContext.EnableRule(x => x.IsRuleR0789_1Active);
		var departureMovement = CreateDepartureMovement();
		departureMovement.BM_ExportTransportMode = ZString.Empty;
		CombineAssertions("When RuleR0789-1 Active", () =>
		{
			var office = departureMovement.CustomsOfficesForDeparture.AddNew();
			office.CY_Code = "TRA";
			departureMovement.Validation.ValidateBM_ExportTransportMode();
			AssertNoMessageError("When Office Of Transit TRA is present and BM_ExportTransportMode is Empty", departureMovement.BM_ExportTransportModeInfo, expectedErrorMessage);

			office.CY_Code = "DES";
			departureMovement.Validation.ValidateBM_ExportTransportMode();
			AssertHasMessageError("When Office Of Transit TRA is not present and BM_ExportTransportMode is Empty", departureMovement.BM_ExportTransportModeInfo, expectedErrorMessage);

			departureMovement.BM_ExportTransportMode = "1";
			departureMovement.Validation.ValidateBM_ExportTransportMode();
			AssertNoMessageError("When Office Of Transit TRA is not present and BM_ExportTransportMode is not Empty", departureMovement.BM_ExportTransportModeInfo, expectedErrorMessage);
		});

		testContext.DisableRule(x => x.IsRuleR0789_1Active);
		departureMovement.BM_ExportTransportMode = ZString.Empty;
		departureMovement.Validation.ValidateBM_ExportTransportMode();
		AssertNoMessageError("When RuleR0789-1 is inactive and Office Of Transit TRA is not present and BM_ExportTransportMode is Empty", departureMovement.BM_ExportTransportModeInfo, expectedErrorMessage);
	}

	public void TestCheckBM_ExportTransportMode_C0599_1()
	{
		const string errorMessage = "[C0599-1] You have not entered a Border Method of Transport.";
		testContext.EnableRule(rule => rule.IsRuleC0599_1Active);
		var departureMovement = CreateDepartureMovement();

		CombineAssertions(() =>
		{
			testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);

			departureMovement.BM_ExportTransportMode = ZString.Empty;
			AssertHasMessageError("ExportTransportMode is empty.", departureMovement.BM_ExportTransportModeInfo, errorMessage);

			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			AssertNoMessageError("ExportTransportMode is not empty.", departureMovement.BM_ExportTransportModeInfo, errorMessage);

			departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
			departureMovement.BM_ExportTransportMode = ZString.Empty;
			AssertNoMessageError("NationalTransitSwitzerland.", departureMovement.BM_ExportTransportModeInfo, errorMessage);

			testContext.DisableRule(rule => rule.IsRuleC0599_1Active);
			departureMovement.BM_InBondEntryType = ZString.Empty;
			departureMovement.Validation.ValidateBM_ExportTransportMode();
			AssertNoMessageError("ExportTransportMode is empty and rule C0599_1 is off", departureMovement.BM_ExportTransportModeInfo, errorMessage);

			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			AssertNoMessageError("ExportTransportMode is not empty and rule C0599_1 is off", departureMovement.BM_ExportTransportModeInfo, errorMessage);

			testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
			testContext.EnableRule(rule => rule.IsRuleC0599_1Active);

			departureMovement.BM_ExportTransportMode = ZString.Empty;
			AssertNoMessageError("ExportTransportMode is empty and NC5TP is off.", departureMovement.BM_ExportTransportModeInfo, errorMessage);
		});
	}

	public void TestCheckBM_ExportTransportMode_C0599_2()
	{
		var entOrExiOrBth = new[] { NctsTypeOfSecurityList.Codes.ENT, NctsTypeOfSecurityList.Codes.EXI, NctsTypeOfSecurityList.Codes.BTH };
		testContext.EnableRule(rule => rule.IsRuleC0599_2Active);
		var departureMovement = CreateDepartureMovement();
		departureMovement.BM_ExportTransportMode = ZString.Empty;

		const string errorMessage = "[C0599-2] You have not entered a Border Method of Transport.";

		CombineAssertions(() =>
		{
			foreach (var securityType in entOrExiOrBth)
			{
				departureMovement.BM_TypeOfSecurity = securityType;
				departureMovement.BM_ExportTransportMode = ZString.Empty;
				AssertHasMessageError($"The rule is ON and SecurityType is {securityType} and ExportTransportMode is empty.", departureMovement.BM_ExportTransportModeInfo, errorMessage);

				testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
				departureMovement.Validation.ValidateBM_ExportTransportMode();
				AssertNoMessageError($"The rule is ON and SecurityType is {securityType} and ExportTransportMode is empty.", departureMovement.BM_ExportTransportModeInfo, errorMessage);
				testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);

				departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
				AssertNoMessageError($"The rule is ON and SecurityType is {securityType} and ExportTransportMode is not empty.", departureMovement.BM_ExportTransportModeInfo, errorMessage);
			}

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			departureMovement.BM_ExportTransportMode = ZString.Empty;
			AssertNoMessageError("The rule is ON and SecurityType is not in [ENT, EXI, BTH].", departureMovement.BM_ExportTransportModeInfo, errorMessage);

			testContext.DisableRule(rule => rule.IsRuleC0599_2Active);
			foreach (var securityType in entOrExiOrBth.Concat([NctsTypeOfSecurityList.Codes.NON]))
			{
				departureMovement.BM_TypeOfSecurity = securityType;
				departureMovement.Validation.ValidateBM_ExportTransportMode();
				AssertNoMessageError($"The rule is OFF and SecurityType is {securityType} and ExportTransportMode is empty.", departureMovement.BM_ExportTransportModeInfo, errorMessage);
			}
		});
	}

	public void TestCheckBM_InBondEntryType_CheckRuleR0601_1_When_ConsignmentItemAdditionalInfos_HasExciseGoods()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(Latvia, "Latvia", eun);
		helper.CreateNewOrGetExistingCusCodeType(Code_CL234, "CusCodeTypeCL234");
		helper.CreateCusCodeList("EUN", Code_CL234, "TE", "Document Type Excise", new ZDateTime(2022, 01, 01), new ZDateTime(2040, 12, 31));
		Factory.Save();

		const string error = "Please enter 'N380' in Previous Document and 'T1/TIR' as Declaration type at Consignment level when Additional Reference Type at Consignment item level has Excise Codes.";

		testContext.EnableRule(x => x.IsRuleR0601_1Active);
		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);

		var departureMovement1 = CreateDepartureMovement();
		var headerPreviousDocuments1 = departureMovement1.Header.PreviousDocuments.AddNew();
		var goodsItem1 = departureMovement1.Header.Bills.AddNew().GoodsItems.AddNew();
		testContext.AddAutoCacheResetObject(goodsItem1);
		var goodsItemAdditionalInfos1 = goodsItem1.AdditionalInfos.AddNew();
		testContext.AddAutoCacheResetObject(goodsItemAdditionalInfos1);

		var info = departureMovement1.BM_InBondEntryTypeInfo;
		goodsItemAdditionalInfos1.CSI_SubType = "REF";
		goodsItemAdditionalInfos1.CSI_Code = "YY";
		departureMovement1.BM_InBondEntryType = "ZZ";
		headerPreviousDocuments1.CSI_Code = "A123";
		departureMovement1.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("No error in transition period when rule is active and rule does not apply", info, error);

		goodsItemAdditionalInfos1.CSI_Code = "TE";
		departureMovement1.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("No error in transition period when rule is active and rule not satisfied", info, error);

		departureMovement1.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T1;
		headerPreviousDocuments1.CSI_Code = "N380";
		departureMovement1.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("No error in transition period when rule is active and rule is satisfied", info, error);

		departureMovement1.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
		departureMovement1.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("No error in transition period when rule is active and rule is satisfied", info, error);

		var newFactory = new BusinessObjectFactory();
		testContext.Factory = newFactory;
		testContext.EnableRule(p => p.IsRuleR0601_1Active);
		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);

		var nctsHeader = newFactory.New<NctsHeader>();
		testContext.AddAutoCacheResetObject(nctsHeader);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		var departureMovement = nctsHeader.MovementHeader;
		testContext.AddAutoCacheResetObject(departureMovement);
		var headerPreviousDocuments = nctsHeader.PreviousDocuments.AddNew();
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		var goodsItemAdditionalInfos = goodsItem.AdditionalInfos.AddNew();

		info = departureMovement.BM_InBondEntryTypeInfo;
		goodsItemAdditionalInfos.CSI_SubType = "REF";
		goodsItemAdditionalInfos.CSI_Code = "YY";
		departureMovement.BM_InBondEntryType = "ZZ";
		headerPreviousDocuments.CSI_Code = "A123";
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("No error outside transition period when rule is active and rule does not apply", info, error);

		goodsItemAdditionalInfos.CSI_Code = "TE";
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertHasMessageError("Has error outside transition period when rule is active and rule not satisfied", info, error);

		departureMovement.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T1;
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertHasMessageError("Has error outside transition period when rule is active and rule not satisfied (no N380)", info, error);

		departureMovement.BM_InBondEntryType = "ZZ";
		headerPreviousDocuments.CSI_Code = "N380";
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertHasMessageError("Has error outside transition period when rule is active and rule not satisfied (wrong BM_InBondEntryType)", info, error);

		departureMovement.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T1;
		headerPreviousDocuments.CSI_Code = "N380";
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("No error outside transition period when rule is active and rule is satisfied", info, error);

		departureMovement.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("No error outside transition period when rule is active and rule is satisfied", info, error);

		newFactory = new BusinessObjectFactory();
		testContext.Factory = newFactory;
		testContext.DisableRule(p => p.IsRuleR0601_1Active);
		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);

		nctsHeader = newFactory.New<NctsHeader>();
		testContext.AddAutoCacheResetObject(nctsHeader);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		departureMovement = nctsHeader.MovementHeader;
		testContext.AddAutoCacheResetObject(departureMovement);
		headerPreviousDocuments = nctsHeader.PreviousDocuments.AddNew();
		goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		goodsItem.BY_Type = string.Empty;
		goodsItemAdditionalInfos = goodsItem.AdditionalInfos.AddNew();
		info = departureMovement.BM_InBondEntryTypeInfo;
		goodsItemAdditionalInfos.CSI_SubType = "REF";
		goodsItemAdditionalInfos.CSI_Code = "YY";
		departureMovement.BM_InBondEntryType = "ZZ";
		headerPreviousDocuments.CSI_Code = "A123";
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("No error in transition period when rule is not active and rule does not apply", info, error);

		goodsItemAdditionalInfos.CSI_Code = "TE";
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("No error in transition period when rule is not active and rule not satisfied", info, error);

		departureMovement.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T1;
		headerPreviousDocuments.CSI_Code = "N380";
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("No error in transition period when rule is not active and rule is satisfied", info, error);

		departureMovement.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("No error in transition period when rule is not active and rule is satisfied", info, error);
	}

	public void TestCheckBM_InBondEntryType_CheckRuleR0601_1_When_GoodsItemsSupportingDocuments_HasExciseGoods()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(Latvia, "Latvia", eun);
		helper.CreateNewOrGetExistingCusCodeType(Code_CL234, "CusCodeTypeCL234");
		helper.CreateCusCodeList("EUN", Code_CL234, "TE", "Document Type Excise", new ZDateTime(2022, 01, 01), new ZDateTime(2040, 12, 31));
		Factory.Save();

		const string error = "Declaration type should be T2 or T2F at Consignment level or Consignment Item level when Supporting Document Type at Consignment item level has Excise Codes.";

		testContext.EnableRule(x => x.IsRuleR0601_1Active);
		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);

		var departureMovement1 = CreateDepartureMovement();
		var goodsItem1 = departureMovement1.Header.Bills.AddNew().GoodsItems.AddNew();
		var goodsItemSupportingDocuments1 = goodsItem1.SupportingDocuments.AddNew();

		var info = departureMovement1.BM_InBondEntryTypeInfo;
		goodsItem1.BY_Type = "XX";
		goodsItemSupportingDocuments1.CSI_Code = "YY";
		departureMovement1.BM_InBondEntryType = "ZZ";
		departureMovement1.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("No error in transition period when rule is active and rule does not apply", info, error);

		goodsItem1.BY_Type = string.Empty;
		goodsItemSupportingDocuments1.CSI_Code = "TE";
		departureMovement1.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("No error in transition period when rule is active and rule not satisfied", info, error);

		departureMovement1.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T1;
		departureMovement1.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("No error in transition period when rule is active and rule is satisfied", info, error);

		departureMovement1.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
		departureMovement1.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("No error in transition period when rule is active and rule is satisfied", info, error);

		var newFactory = new BusinessObjectFactory();
		testContext.Factory = newFactory;
		testContext.EnableRule(p => p.IsRuleR0601_1Active);
		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);

		var nctsHeader = newFactory.New<NctsHeader>();
		testContext.AddAutoCacheResetObject(nctsHeader);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var departureMovement = nctsHeader.MovementHeader;
		testContext.AddAutoCacheResetObject(departureMovement);
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		var goodsItemSupportingDocuments = goodsItem.SupportingDocuments.AddNew();

		info = departureMovement.BM_InBondEntryTypeInfo;
		goodsItem.BY_Type = "XX";
		goodsItemSupportingDocuments.CSI_Code = "YY";
		departureMovement.BM_InBondEntryType = "ZZ";
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("No error outside transition period when rule is active and rule does not apply", info, error);

		goodsItem.BY_Type = "XX";
		goodsItemSupportingDocuments.CSI_Code = "TE";
		departureMovement.BM_InBondEntryType = "ZZ";
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("No error outside transition period when rule is active and rule does not apply (BY_Type populated)", info, error);

		goodsItem.BY_Type = string.Empty;
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertHasMessageError("Has error outside transition period when rule is active and rule not satisfied", info, error);

		departureMovement.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("No error outside transition period when rule is active and rule is satisfied (T2)", info, error);

		departureMovement.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2F;
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("No error outside transition period when rule is active and rule is satisfied (T2F)", info, error);

		newFactory = new BusinessObjectFactory();
		testContext.Factory = newFactory;
		testContext.DisableRule(p => p.IsRuleR0601_1Active);
		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);

		nctsHeader = newFactory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		departureMovement = nctsHeader.MovementHeader;
		goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		goodsItem.BY_Type = string.Empty;
		goodsItemSupportingDocuments = goodsItem.SupportingDocuments.AddNew();
		info = departureMovement.BM_InBondEntryTypeInfo;
		goodsItem.BY_Type = "XX";
		goodsItemSupportingDocuments.CSI_Code = "YY";
		departureMovement.BM_InBondEntryType = "ZZ";
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("No error in transition period when rule is not active and rule does not apply", info, error);

		goodsItem.BY_Type = string.Empty;
		goodsItemSupportingDocuments.CSI_Code = "TE";
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("No error in transition period when rule is not active and rule not satisfied", info, error);

		departureMovement.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T1;
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("No error in transition period when rule is not active and rule is satisfied (T2)", info, error);

		departureMovement.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("No error in transition period when rule is not active and rule is satisfied (T2F)", info, error);
	}

	public void TestCheckBM_SpecificCircumstance()
	{
		var departureMovement = CreateDepartureMovement();

		ValidationTestHelper.AssertInvalidCodeMessageError(departureMovement.BM_SpecificCircumstanceInfo, "X", NctsSpecificCircumstanceIndicatorList.Codes.A20);
	}

	public void TestCheckBM_PortOfPresentationCode_ListValidation_UNLOCO()
	{
		var departureMovement = CreateDepartureMovement();

		var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
		ValidationTestHelper.AssertInvalidCodeMessageError(departureMovement.BM_PortOfPresentationCodeInfo, "XXXXX", unloco.Code);
	}

	public void TestCheckBM_PortOfPresentationCode_ListValidation_CountryCode()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
		Factory.Save();

		var departureMovement = CreateDepartureMovement();
		ValidationTestHelper.AssertInvalidCodeMessageError(departureMovement.BM_PortOfPresentationCodeInfo, "XX", "DE");
	}

	public void TestCheckBM_PortOfPresentationCode_RuleB1893()
	{
		const string expectedMessageError = "[B1893] You have not entered a Place of Loading.";

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		testContext.EnableRule(x => x.IsRuleB1893Active);
		var departureMovement = CreateDepartureMovement();

		CombineAssertions("When RuleB1893 Active, TP: ON", () =>
		{
			departureMovement.BM_AdditionalDeclarationType = "D";
			departureMovement.BM_PortOfPresentationCode = ZString.Empty;
			AssertNoMessageError("Security is Empty and BM_PortOfPresentationCode is Empty.", departureMovement.BM_PortOfPresentationCodeInfo, expectedMessageError);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			departureMovement.BM_PortOfPresentationCode = ZString.Empty;
			AssertNoMessageError("Security = NON and BM_PortOfPresentationCode is Empty.", departureMovement.BM_PortOfPresentationCodeInfo, expectedMessageError);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			departureMovement.BM_PortOfPresentationCode = ZString.Empty;
			AssertHasMessageError("Security != NON and BM_PortOfPresentationCode is Empty.", departureMovement.BM_PortOfPresentationCodeInfo, expectedMessageError);

			departureMovement.BM_PortOfPresentationCode = "IEABC";
			AssertNoMessageError("Security != NON and BM_PortOfPresentationCode is not Empty.", departureMovement.BM_PortOfPresentationCodeInfo, expectedMessageError);
		});

		testContext.DisableRule(x => x.IsRuleB1893Active);
		departureMovement.BM_PortOfPresentationCode = ZString.Empty;
		AssertNoMessageError("When RuleB1893 Disable, TP: ON", departureMovement.BM_PortOfPresentationCodeInfo, expectedMessageError);

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		departureMovement = CreateDepartureMovement();
		departureMovement.BM_PortOfPresentationCode = ZString.Empty;
		AssertNoMessageError("When RuleB1893 Disable, TP: OFF", departureMovement.BM_PortOfPresentationCodeInfo, expectedMessageError);
	}

	public void TestCheckBM_PortOfPresentationCode_RuleC0387Active()
	{
		var departureMovement = CreateDepartureMovement();
		var expectedMessageError = "[C0387] You have not entered an UNLOCO (Port Of Loading) Or a Country.";

		testContext.EnableRule(c => c.IsRuleC0387Active);

		departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;

		departureMovement.BM_PortOfPresentationCode = ZString.Empty;
		AssertHasMessageError("When BM_PortOfPresentationCode is empty", departureMovement.BM_PortOfPresentationCodeInfo, expectedMessageError);

		departureMovement.BM_PortOfPresentationCode = "AC";
		AssertNoMessageError("When BM_PortOfPresentationCode is not empty", departureMovement.BM_PortOfPresentationCodeInfo, expectedMessageError);
	}

	public void TestCheckBM_PortOfPresentationCode_RuleC0387InActive()
	{
		testContext.DisableRule(c => c.IsRuleC0387Active);

		var departureMovement = CreateDepartureMovement();
		departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
		departureMovement.BM_PortOfPresentationCode = ZString.Empty;
		AssertNoNotifications(departureMovement.BM_PortOfPresentationCodeInfo);
	}

	public void TestCheckBM_PortOfPresentationCode_MandatoryRuleC0403()
	{
		var expectedMessageError = "[C0403] You have not entered a Country/Region Code or an UNLOCO for Place of Loading.";

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		testContext.DisableRule(x => x.IsRuleB1893Active);
		testContext.EnableRule(x => x.IsRuleC0403Active);
		var departureMovement = CreateDepartureMovement();
		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;

		CombineAssertions("When RuleC0403: Enable, RuleB1893: Disable, TP: OFF", () =>
		{
			departureMovement.BM_AdditionalDeclarationType = "D";
			departureMovement.Validation.ValidateBM_PortOfPresentationCode();
			AssertNoMessageError("BM_AdditionalDeclarationType is D", departureMovement.BM_PortOfPresentationCodeInfo, expectedMessageError);

			departureMovement.BM_AdditionalDeclarationType = "A";
			departureMovement.BM_PortOfPresentationCode = "IEABC";
			AssertNoMessageError("BM_AdditionalDeclarationType = 'A' and BM_PortOfPresentationCode is entered",
								departureMovement.BM_PortOfPresentationCodeInfo, expectedMessageError);

			departureMovement.BM_PortOfPresentationCode = ZString.Empty;
			AssertHasMessageError("BM_AdditionalDeclarationType = 'A' and BM_PortOfPresentationCode is not entered",
								departureMovement.BM_PortOfPresentationCodeInfo, expectedMessageError);
		});

		testContext.EnableRule(x => x.IsRuleB1893Active);
		departureMovement = CreateDepartureMovement();
		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
		departureMovement.BM_AdditionalDeclarationType = "A";
		departureMovement.BM_PortOfPresentationCode = ZString.Empty;
		AssertHasMessageError("When RuleC0403: Enable, RuleB1893: Enable, TP: OFF, PortOfPresentationCode is not entered",
							departureMovement.BM_PortOfPresentationCodeInfo, expectedMessageError);

		testContext.DisableRule(x => x.IsRuleC0403Active);
		departureMovement = CreateDepartureMovement();
		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
		departureMovement.BM_AdditionalDeclarationType = "A";
		departureMovement.BM_PortOfPresentationCode = ZString.Empty;
		AssertNoMessageError("When RuleC0403: Disable, RuleB1893: Enable, TP: OFF, PortOfPresentationCode is not entered",
							departureMovement.BM_PortOfPresentationCodeInfo, expectedMessageError);

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		testContext.EnableRule(x => x.IsRuleB1893Active);
		testContext.EnableRule(x => x.IsRuleC0403Active);
		departureMovement = CreateDepartureMovement();
		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
		departureMovement.BM_AdditionalDeclarationType = "A";
		departureMovement.BM_PortOfPresentationCode = ZString.Empty;
		AssertNoMessageError("When RuleC0403: Enable, RuleB1893: Enable, TP: ON, PortOfPresentationCode is not entered",
							departureMovement.BM_PortOfPresentationCodeInfo, expectedMessageError);

		testContext.DisableRule(x => x.IsRuleB1893Active);
		departureMovement = CreateDepartureMovement();
		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
		departureMovement.BM_AdditionalDeclarationType = "A";
		departureMovement.BM_PortOfPresentationCode = ZString.Empty;
		AssertHasMessageError("When RuleC0403: Enable, RuleB1893: Disable, TP: ON, PortOfPresentationCode is not entered",
							departureMovement.BM_PortOfPresentationCodeInfo, expectedMessageError);

		testContext.DisableRule(x => x.IsRuleC0403Active);
		departureMovement = CreateDepartureMovement();
		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
		departureMovement.BM_AdditionalDeclarationType = "A";
		departureMovement.BM_PortOfPresentationCode = ZString.Empty;
		AssertNoMessageError("When RuleC0403: Disable, RuleB1893: Disable, TP: ON, PortOfPresentationCode is not entered",
							departureMovement.BM_PortOfPresentationCodeInfo, expectedMessageError);
	}

	public void TestCheckBM_ForeignDestPortKCode_ListValidation_UNLOCO()
	{
		var departureMovement = CreateDepartureMovement();
		var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
		ValidationTestHelper.AssertInvalidCodeMessageError(departureMovement.BM_ForeignDestPortKCodeInfo, "XXXXX", unloco.Code);
	}

	public void TestCheckBM_ForeignDestPortKCode_ListValidation_CountryCode()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
		Factory.Save();

		var departureMovement = CreateDepartureMovement();
		ValidationTestHelper.AssertInvalidCodeMessageError(departureMovement.BM_ForeignDestPortKCodeInfo, "XX", "DE");
	}

	public void TestCheckBM_ForeignDestPortKCode_MandatoryWhenTypeOfSecurityIsNotNON_NR0038()
	{
		const string msgError = "[NR0038] You have not entered a Country/Region Code or an UNLOCO for Place of Unloading";

		CombineAssertions(() =>
		{
			testContext.DisableRule(c => c.IsRuleNR0038Active);
			var departureMovement = CreateDepartureMovement();

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
			departureMovement.BM_ForeignDestPortKCode = string.Empty;
			AssertNoMessageError("Rule is disabled", departureMovement.BM_ForeignDestPortKCodeInfo, msgError);

			testContext.EnableRule(c => c.IsRuleNR0038Active);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(departureMovement.BM_ForeignDestPortKCodeInfo, msgError);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			departureMovement.BM_ForeignDestPortKCode = string.Empty;
			AssertNoMessageError("Type of Security is 'NON'", departureMovement.BM_ForeignDestPortKCodeInfo, msgError);
		});
	}

	public void TestCheckBM_ForeignDestPortKCode_MandatoryValidation_RuleC0191_1Active() => CombineAssertions(() =>
	{
		const string expectedMessageError = "[C0191-1] You have not entered a Country/Region Code or an UNLOCO for Place of Unloading";

		testContext.EnableRule(p => p.IsRuleC0191_1Active);

		var departureMovement = CreateDepartureMovement();
		var foreignDestPortKCodeInfo = departureMovement.BM_ForeignDestPortKCodeInfo;

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
		departureMovement.BM_ForeignDestPortKCode = ZString.Empty;
		departureMovement.Validation.ValidateBM_ForeignDestPortKCode();
		AssertHasMessageErrorContaining("Outside TP Security=ENT and Place of Unloading NOT entered", foreignDestPortKCodeInfo, expectedMessageError);

		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
		departureMovement.Validation.ValidateBM_ForeignDestPortKCode();
		AssertHasMessageErrorContaining("Outside TP Security=BTH and Place of Unloading NOT entered", foreignDestPortKCodeInfo, expectedMessageError);

		departureMovement.BM_ForeignDestPortKCode = "VAL";
		departureMovement.Validation.ValidateBM_ForeignDestPortKCode();
		AssertNoMessageErrorContaining("Outside TP Security=ENT and Place of Unloading entered", foreignDestPortKCodeInfo, expectedMessageError);

		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
		departureMovement.BM_ForeignDestPortKCode = ZString.Empty;
		departureMovement.Validation.ValidateBM_ForeignDestPortKCode();
		AssertNoMessageErrorContaining("Outside TP Security=NON and Place of Unloading NOT entered", foreignDestPortKCodeInfo, expectedMessageError);

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
		departureMovement.BM_ForeignDestPortKCode = ZString.Empty;
		departureMovement.Validation.ValidateBM_ForeignDestPortKCode();
		AssertNoMessageErrorContaining("During TP, Security=ENT and Place of Unloading NOT entered", foreignDestPortKCodeInfo, expectedMessageError);
	});

	public void TestCheckBM_ForeignDestPortKCode_MandatoryValidation_RuleC0191_1Inactive()
	{
		testContext.DisableRule(p => p.IsRuleC0191_1Active);
		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);

		var departureMovement = CreateDepartureMovement();
		var foreignDestPortKCodeInfo = departureMovement.BM_ForeignDestPortKCodeInfo;

		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
		departureMovement.BM_ForeignDestPortKCode = ZString.Empty;
		departureMovement.Validation.ValidateBM_ForeignDestPortKCode();
		AssertNoNotifications(foreignDestPortKCodeInfo);
	}

	public void TestCheckBM_ForeignDestPortKCode_MandatoryValidation_RuleC0191_2Active() => CombineAssertions(() =>
	{
		const string expectedMessageError = "[C0191-2] You have not entered an UNLOCO (Place Of Unloading) Or a Country.";

		testContext.EnableRule(p => p.IsRuleC0191_2Active);

		var departureMovement = CreateDepartureMovement();
		var foreignDestPortKCodeInfo = departureMovement.BM_ForeignDestPortKCodeInfo;

		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
		departureMovement.BM_ForeignDestPortKCode = ZString.Empty;
		departureMovement.Validation.ValidateBM_ForeignDestPortKCode();
		AssertHasMessageErrorContaining("Security <> NON and Place of Unloading NOT entered", foreignDestPortKCodeInfo, expectedMessageError);

		departureMovement.BM_ForeignDestPortKCode = "VAL";
		departureMovement.Validation.ValidateBM_ForeignDestPortKCode();
		AssertNoMessageErrorContaining("Security <> NON and Place of Unloading entered", foreignDestPortKCodeInfo, expectedMessageError);

		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
		departureMovement.BM_ForeignDestPortKCode = ZString.Empty;
		departureMovement.Validation.ValidateBM_ForeignDestPortKCode();
		AssertNoMessageErrorContaining("Security=NON and Place of Unloading NOT entered", foreignDestPortKCodeInfo, expectedMessageError);
	});

	public void TestCheckBM_ForeignDestPortKCode_MandatoryValidation_RuleC0191_2Inactive()
	{
		testContext.DisableRule(p => p.IsRuleC0191_2Active);

		var departureMovement = CreateDepartureMovement();
		var foreignDestPortKCodeInfo = departureMovement.BM_ForeignDestPortKCodeInfo;

		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
		departureMovement.BM_ForeignDestPortKCode = ZString.Empty;
		departureMovement.Validation.ValidateBM_ForeignDestPortKCode();
		AssertNoNotifications(foreignDestPortKCodeInfo);
	}

	public void TestCheckBM_ForeignDestPortKCode_NotTransmitted_WhenRuleC0191_1Active() => CombineAssertions(() =>
	{
		const string expectedMessageWarning = "[C0191-1] Country/Region Code/UNLOCO is not transmitted to customs if Security = NON";

		testContext.EnableRule(p => p.IsRuleC0191_1Active);
		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);

		var departureMovement = CreateDepartureMovement();
		var foreignDestPortKCodeInfo = departureMovement.BM_ForeignDestPortKCodeInfo;
		departureMovement.BM_TypeOfSecurity = "NON";
		departureMovement.BM_ForeignDestPortKCode = "AU";
		departureMovement.Validation.ValidateBM_ForeignDestPortKCode();
		AssertHasWarning("Outside TP Security=NON, BM_ForeignDestPortKCode is NOT empty", foreignDestPortKCodeInfo, expectedMessageWarning);

		departureMovement.BM_TypeOfSecurity = "ENT";
		departureMovement.Validation.ValidateBM_ForeignDestPortKCode();
		AssertNoWarning("Outside TP Security=ENT, BM_ForeignDestPortKCode is NOT empty", foreignDestPortKCodeInfo, expectedMessageWarning);

		departureMovement.BM_TypeOfSecurity = "NON";
		departureMovement.BM_ForeignDestPortKCode = ZString.Empty;
		departureMovement.Validation.ValidateBM_ForeignDestPortKCode();
		AssertNoWarning("Outside TP Security=NON, BM_ForeignDestPortKCode is empty", foreignDestPortKCodeInfo, expectedMessageWarning);

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		departureMovement = CreateDepartureMovement();
		foreignDestPortKCodeInfo = departureMovement.BM_ForeignDestPortKCodeInfo;
		departureMovement.BM_TypeOfSecurity = "NON";
		departureMovement.BM_ForeignDestPortKCode = "VAL";
		departureMovement.Validation.ValidateBM_ForeignDestPortKCode();
		AssertNoWarning("During TP, Security=NON, BM_ForeignDestPortKCode is NOT empty", foreignDestPortKCodeInfo, expectedMessageWarning);
	});

	public void TestCheckBM_ForeignDestPortKCode_NotRequired_WhenRuleC0191_1Inactive()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
		Factory.Save();

		var departureMovement = CreateDepartureMovement();
		var foreignDestPortKCodeInfo = departureMovement.BM_ForeignDestPortKCodeInfo;

		testContext.DisableRule(p => p.IsRuleC0191_1Active);
		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);

		departureMovement.BM_TypeOfSecurity = "NON";
		departureMovement.BM_ForeignDestPortKCode = "AU";
		departureMovement.Validation.ValidateBM_ForeignDestPortKCode();
		AssertNoNotifications(foreignDestPortKCodeInfo);
	}

	public void TestCheckBM_ForeignDestPortKCode_RuleC0387Active()
	{
		var departureMovement = CreateDepartureMovement();
		var expectedMessageError = "[C0387] You have not entered an UNLOCO (Place Of Unloading) Or a Country.";

		testContext.EnableRule(c => c.IsRuleC0387Active);

		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;

		departureMovement.BM_ForeignDestPortKCode = ZString.Empty;
		AssertNoMessageError("When BM_ForeignDestPortKCode is empty", departureMovement.BM_ForeignDestPortKCodeInfo, expectedMessageError);

		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
		departureMovement.Validation.ValidateBM_ForeignDestPortKCode();
		AssertHasMessageError("When BM_ForeignDestPortKCode is empty", departureMovement.BM_ForeignDestPortKCodeInfo, expectedMessageError);

		departureMovement.BM_ForeignDestPortKCode = "ABC";
		AssertNoMessageError("When BM_ForeignDestPortKCode is not empty", departureMovement.BM_ForeignDestPortKCodeInfo, expectedMessageError);
	}

	public void TestCheckBM_ForeignDestPortKCode_RuleC0387InActive()
	{
		var departureMovement = CreateDepartureMovement();

		testContext.DisableRule(c => c.IsRuleC0387Active);

		departureMovement.BM_ForeignDestPortKCode = ZString.Empty;
		AssertNoNotifications(departureMovement.BM_ForeignDestPortKCodeInfo);
	}

	public void TestCheckBM_ForeignDestPortKCode_RuleB1858_1_MustBeEmpty()
	{
		testContext.EnableRule(rule => rule.IsRuleB1858_1Active);
		var movementHeader = CreateDepartureMovement();
		var expectedMessage = "[B1858-1] " + $"Please do not enter a {movementHeader.BM_ForeignDestPortKCodeInfo.HumanReadableName}.";
		CombineAssertions(() =>
		{
			testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
			testContext.EnableRule(rule => rule.IsRuleB1858_1Active);

			movementHeader.BM_TypeOfSecurity = "NON";
			movementHeader.Validation.ValidateBM_ForeignDestPortKCode();
			AssertNoMessageError("When NC5TP = ON, RuleB1858_1 = ON, Security = NON, ForeignDestPortKCode empty", movementHeader.BM_ForeignDestPortKCodeInfo, expectedMessage);

			movementHeader.BM_ForeignDestPortKCode = "XYZ";
			AssertHasMessageError("When NC5TP = ON, RuleB1858_1 = ON, Security = NON, ForeignDestPortKCode filled", movementHeader.BM_ForeignDestPortKCodeInfo, expectedMessage);

			movementHeader.BM_TypeOfSecurity = "BTH";
			movementHeader.Validation.ValidateBM_ForeignDestPortKCode();
			AssertNoMessageError("When NC5TP = ON, RuleB1858_1 = ON, Security = BTH, ForeignDestPortKCode filled", movementHeader.BM_ForeignDestPortKCodeInfo, expectedMessage);

			testContext.DisableRule(rule => rule.IsRuleB1858_1Active);
			movementHeader.BM_TypeOfSecurity = "NON";
			movementHeader.Validation.ValidateBM_ForeignDestPortKCode();
			AssertNoMessageError("When NC5TP = ON, RuleB1858_1 = OFF, Security = BTH, ForeignDestPortKCode filled", movementHeader.BM_ForeignDestPortKCodeInfo, expectedMessage);

			testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
			testContext.EnableRule(rule => rule.IsRuleB1858_1Active);

			movementHeader.Validation.ValidateBM_ForeignDestPortKCode();
			AssertNoMessageError("When NC5TP = OFF, RuleB1858_1 = ON, Security = NON, ForeignDestPortKCode filled", movementHeader.BM_ForeignDestPortKCodeInfo, expectedMessage);
		});
	}

	public void TestCheckBM_ForeignDestPortKCode_RuleB1858_1_MustBeFilled() => CombineAssertions(() =>
	{
		testContext.EnableRule(rule => rule.IsRuleB1858_1Active);
		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);

		var movementHeader = CreateDepartureMovement();
		var expectedMessage = "[B1858-1] " + $"You have not entered a {movementHeader.BM_ForeignDestPortKCodeInfo.HumanReadableName}.";
		movementHeader.BM_TypeOfSecurity = "BTH";
		movementHeader.BM_SpecificCircumstance = NctsSpecificCircumstanceIndicatorList.Codes.A20;
		movementHeader.Validation.ValidateBM_ForeignDestPortKCode();
		AssertHasMessageError("When NC5TP = ON, RuleB1858_1 = ON, Security = BTH and Circumstance = A20, ForeignDestPortKCode empty", movementHeader.BM_ForeignDestPortKCodeInfo, expectedMessage);

		movementHeader.BM_ForeignDestPortKCode = "XYZ";
		AssertNoMessageError("When NC5TP = ON, RuleB1858_1 = ON, Security = BTH and Circumstance = A20, ForeignDestPortKCode filled", movementHeader.BM_ForeignDestPortKCodeInfo, expectedMessage);

		movementHeader.BM_TypeOfSecurity = "NON";
		movementHeader.BM_ForeignDestPortKCode = ZString.Empty;
		AssertNoMessageError("When NC5TP = ON, RuleB1858_1 = ON, Security = NON and Circumstance = A20, ForeignDestPortKCode empty", movementHeader.BM_ForeignDestPortKCodeInfo, expectedMessage);

		movementHeader.BM_TypeOfSecurity = "BTH";
		movementHeader.BM_SpecificCircumstance = NctsSpecificCircumstanceIndicatorList.Codes.XXX;
		movementHeader.Validation.ValidateBM_ForeignDestPortKCode();
		AssertNoMessageError("When NC5TP = ON, RuleB1858_1 = ON, Security = BTH and Circumstance = XXX, ForeignDestPortKCode empty", movementHeader.BM_ForeignDestPortKCodeInfo, expectedMessage);

		testContext.DisableRule(x => x.IsRuleB1858_1Active);

		movementHeader.BM_SpecificCircumstance = NctsSpecificCircumstanceIndicatorList.Codes.A20;
		movementHeader.Validation.ValidateBM_ForeignDestPortKCode();
		AssertNoMessageError("When NC5TP = ON, RuleB1858_1 = OFF, Security = BTH and Circumstance = A20, ForeignDestPortKCode empty", movementHeader.BM_ForeignDestPortKCodeInfo, expectedMessage);

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		testContext.EnableRule(x => x.IsRuleB1858_1Active);

		movementHeader.Validation.ValidateBM_ForeignDestPortKCode();
		AssertNoMessageError("When NC5TP = OFF, RuleB1858_1 = ON,Security = BTH and Circumstance = A20, ForeignDestPortKCode empty", movementHeader.BM_ForeignDestPortKCodeInfo, expectedMessage);
	});

	public void TestCheckBM_PlaceOfLoading_RuleB1893_2()
	{
		const string expectedMessageError = "[B1893-2] You have not entered a Location for the Place of Loading.";

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		testContext.EnableRule(x => x.IsRuleB1893_2Active);
		var departureMovement = CreateDepartureMovement();
		CombineAssertions("When RuleB1893-2 Active, TP: ON", () =>
		{
			departureMovement.BM_AdditionalDeclarationType = "D";
			departureMovement.BM_PortOfPresentationCode = ZString.Empty;
			departureMovement.BM_PlaceOfLoading = ZString.Empty;
			AssertNoMessageError("Security is Empty, BM_PortOfPresentationCode is Empty and BM_PlaceOfLoadingInfo is Empty.", departureMovement.BM_PlaceOfLoadingInfo, expectedMessageError);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			departureMovement.BM_PortOfPresentationCode = "IEC";
			departureMovement.BM_PlaceOfLoading = ZString.Empty;
			AssertNoMessageError("Security = NON, BM_PortOfPresentationCode.Length != 5 and BM_PlaceOfLoadingInfo is Empty.", departureMovement.BM_PlaceOfLoadingInfo, expectedMessageError);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			departureMovement.BM_PlaceOfLoading = ZString.Empty;
			AssertHasMessageError("Security != NON, BM_PortOfPresentationCode.Length != 5 and BM_PlaceOfLoadingInfo is Empty.", departureMovement.BM_PlaceOfLoadingInfo, expectedMessageError);

			departureMovement.BM_PlaceOfLoading = "AB";
			AssertNoMessageError("Security != NON, BM_PortOfPresentationCode.Length != 5 and BM_PlaceOfLoadingInfo is not Empty.", departureMovement.BM_PlaceOfLoadingInfo, expectedMessageError);

			departureMovement.BM_PortOfPresentationCode = ZString.Empty;
			departureMovement.BM_PlaceOfLoading = ZString.Empty;
			AssertNoMessageError("Security != NON, BM_PortOfPresentationCode is Empty and BM_PlaceOfLoadingInfo is Empty.", departureMovement.BM_PlaceOfLoadingInfo, expectedMessageError);

			departureMovement.BM_PortOfPresentationCode = "ABCDE";
			departureMovement.BM_PlaceOfLoading = ZString.Empty;
			AssertNoMessageError("Security != NON, BM_PortOfPresentationCode.Length = 5 and BM_PlaceOfLoadingInfo is Empty.", departureMovement.BM_PlaceOfLoadingInfo, expectedMessageError);
		});

		testContext.DisableRule(x => x.IsRuleB1893_2Active);
		departureMovement.BM_PlaceOfLoading = ZString.Empty;
		AssertNoMessageError("When RuleB1893-2 Disable, TP: ON", departureMovement.BM_PlaceOfLoadingInfo, expectedMessageError);

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		departureMovement = CreateDepartureMovement();
		departureMovement.BM_PlaceOfLoading = ZString.Empty;
		AssertNoMessageError("When RuleB1893-2 Disable, TP: OFF", departureMovement.BM_PlaceOfLoadingInfo, expectedMessageError);
	}

	public void TestCheckBM_PlaceOfLoading_NR0037()
	{
		const string msgError = "[NR0037] You have not entered a Place of Loading.";

		testContext.EnableRule(c => c.IsRuleNR0037Active);

		var departureMovement = CreateDepartureMovement();
		departureMovement.BM_PortOfPresentationCode = ZString.Empty;
		departureMovement.BM_PlaceOfLoading = ZString.Empty;
		departureMovement.Validation.ValidateBM_PlaceOfLoading();
		AssertNoMessageError("Not required when BM_PortOfPresentationCode is empty", departureMovement.BM_PlaceOfLoadingInfo, msgError);

		departureMovement.BM_PortOfPresentationCode = "IE";
		departureMovement.Validation.ValidateBM_PlaceOfLoading();
		AssertHasMessageError("Required when BM_PortOfPresentationCode is entered", departureMovement.BM_PlaceOfLoadingInfo, msgError);

		testContext.DisableRule(c => c.IsRuleNR0037Active);

		departureMovement.Validation.ValidateBM_PlaceOfLoading();
		AssertNoMessageError("Not required when rule is disabled", departureMovement.BM_PlaceOfLoadingInfo, msgError);

		testContext.EnableRule(c => c.IsRuleNR0037Active);

		departureMovement.BM_PlaceOfLoading = "ABC";
		departureMovement.Validation.ValidateBM_PlaceOfLoading();
		AssertNoMessageError("No error as entered", departureMovement.BM_PlaceOfLoadingInfo, msgError);
	}

	public void TestCheckBM_PlaceOfLoading_RuleC0387Active() => CombineAssertions(() =>
	{
		var departureMovement = CreateDepartureMovement();
		var expectedMessageError = "[C0387] You have not entered a Place Of Loading Location Description.";

		testContext.EnableRule(c => c.IsRuleC0387Active);

		departureMovement.BM_PortOfPresentationCode = "AC";
		departureMovement.BM_PlaceOfLoading = ZString.Empty;
		AssertHasMessageError("When BM_PortOfPresentationCode equals to 2 characters and BM_PlaceOfLoading is Empty", departureMovement.BM_PlaceOfLoadingInfo, expectedMessageError);

		departureMovement.BM_PortOfPresentationCode = "AC";
		departureMovement.BM_PlaceOfLoading = "AI";
		AssertNoMessageError("When BM_PortOfPresentationCode equals to 2 characters and BM_PlaceOfLoading is Not Empty", departureMovement.BM_PlaceOfLoadingInfo, expectedMessageError);

		departureMovement.BM_PortOfPresentationCode = "ABCDE";
		departureMovement.BM_PlaceOfLoading = ZString.Empty;
		AssertNoMessageError("When BM_PortOfPresentationCode equals to 5 characters and BM_PlaceOfLoading is Empty", departureMovement.BM_PlaceOfLoadingInfo, expectedMessageError);
	});

	public void TestCheckBM_PlaceOfLoading_RuleC0387InActive()
	{
		testContext.DisableRule(c => c.IsRuleC0387Active);

		var departureMovement = CreateDepartureMovement();
		departureMovement.BM_PortOfPresentationCode = "AC";
		departureMovement.Validation.ValidateBM_PlaceOfLoading();
		AssertNoNotifications(departureMovement.BM_PlaceOfLoadingInfo);
	}

	public void TestCheckBM_PlaceOfLoading_MandatoryRuleC0403_1()
	{
		var expectedMessageError = "[C0403-1] You have not entered a Place of Loading.";

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		testContext.EnableRule(x => x.IsRuleC0403_1Active);
		var departureMovement = CreateDepartureMovement();
		CombineAssertions("When RuleC0403_1: Active, TP: OFF", () =>
		{
			departureMovement.BM_AdditionalDeclarationType = "";
			departureMovement.BM_PlaceOfLoading = "";
			AssertNoMessageError("BM_AdditionalDeclarationType = '' and BM_PlaceOfLoading = ''", departureMovement.BM_PlaceOfLoadingInfo, expectedMessageError);

			departureMovement.BM_AdditionalDeclarationType = "D";
			departureMovement.BM_PlaceOfLoading = "";
			AssertNoMessageError("BM_AdditionalDeclarationType = 'D' and BM_PlaceOfLoading = ''", departureMovement.BM_PlaceOfLoadingInfo, expectedMessageError);

			departureMovement.BM_AdditionalDeclarationType = "A";
			departureMovement.BM_PlaceOfLoading = "";
			AssertHasMessageError("BM_AdditionalDeclarationType = 'A' and BM_PlaceOfLoading = ''", departureMovement.BM_PlaceOfLoadingInfo, expectedMessageError);

			departureMovement.BM_PlaceOfLoading = "ABC";
			AssertNoMessageError("BM_AdditionalDeclarationType = 'A' and BM_PlaceOfLoading = 'ABC'", departureMovement.BM_PlaceOfLoadingInfo, expectedMessageError);

			departureMovement.BM_PortOfPresentationCode = "12345";
			departureMovement.BM_PlaceOfLoading = "";
			AssertNoMessageError("BM_PortOfPresentationCode is 12345, BM_AdditionalDeclarationType = 'A' and BM_PlaceOfLoading = ''", departureMovement.BM_PlaceOfLoadingInfo, expectedMessageError);
		});

		testContext.DisableRule(x => x.IsRuleC0403_1Active);
		departureMovement = CreateDepartureMovement();
		departureMovement.BM_PortOfPresentationCode = ZString.Empty;
		departureMovement.BM_PlaceOfLoading = "";
		AssertNoMessageError("When C0403_1 is disabled", departureMovement.BM_PlaceOfLoadingInfo, expectedMessageError);

		testContext.EnableRule(x => x.IsRuleC0403_1Active);
		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);

		departureMovement = CreateDepartureMovement();
		departureMovement.BM_PortOfPresentationCode = ZString.Empty;
		departureMovement.BM_PlaceOfLoading = "";
		AssertNoMessageError("In TP, when BM_AdditionalDeclarationType = 'A' and BM_PlaceOfLoading is not entered", departureMovement.BM_PlaceOfLoadingInfo, expectedMessageError);
	}

	public void TestCheckBM_PlaceOfLoading_NR0080Active() => CombineAssertions(() =>
	{
		var expectedMessageWarning = "[NR0080] Place of Loading is mandatory except for pre-declarations.";

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		testContext.EnableRule(x => x.IsRuleNR0080Active);
		var departureMovement = CreateDepartureMovement();
		departureMovement.BM_PlaceOfLoading = "";
		AssertNoWarning("No Warning cause is not FinalPeriod", departureMovement.BM_PlaceOfLoadingInfo, expectedMessageWarning);

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		departureMovement = CreateDepartureMovement();
		departureMovement.BM_PlaceOfLoading = "ABC";
		AssertNoWarning("No Warning cause is FinalPeriod and but Place of Loading is not Empty", departureMovement.BM_PlaceOfLoadingInfo, expectedMessageWarning);

		departureMovement.BM_PlaceOfLoading = "";
		AssertHasWarning("Warning cause is FinalPeriod and Place of Loading is Empty", departureMovement.BM_PlaceOfLoadingInfo, expectedMessageWarning);

		testContext.DisableRule(x => x.IsRuleNR0080Active);
		departureMovement.Validation.ValidateBM_PlaceOfLoading();
		AssertNoWarning("No Warning cause IsRuleNR0080Active is disable", departureMovement.BM_PlaceOfLoadingInfo, expectedMessageWarning);
	});

	public void TestCheckBM_PlaceOfUnloading_C0387Active()
	{
		var departureMovement = CreateDepartureMovement();
		var expectedMessageError = "[C0387] You have not entered a Place Of Unloading Location Description.";

		testContext.EnableRule(c => c.IsRuleC0387Active);

		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;

		departureMovement.BM_ForeignDestPortKCode = "";
		departureMovement.BM_PlaceOfUnloading = "";
		AssertNoMessageError("When BM_ForeignDestPortKCode not equals to 2 characters and BM_PlaceOfUnloading is Empty", departureMovement.BM_PlaceOfUnloadingInfo, expectedMessageError);

		departureMovement.BM_ForeignDestPortKCode = "AC";
		departureMovement.BM_PlaceOfUnloading = ZString.Empty;
		AssertHasMessageError("When BM_ForeignDestPortKCode equals to 2 characters and BM_PlaceOfUnloading is Empty", departureMovement.BM_PlaceOfUnloadingInfo, expectedMessageError);

		departureMovement.BM_ForeignDestPortKCode = "AC";
		departureMovement.BM_PlaceOfUnloading = "AI";
		AssertNoMessageError("When BM_ForeignDestPortKCode equals to 2 characters and BM_PlaceOfUnloading is Not Empty", departureMovement.BM_PlaceOfUnloadingInfo, expectedMessageError);

		departureMovement.BM_ForeignDestPortKCode = "ABCDE";
		departureMovement.BM_PlaceOfUnloading = ZString.Empty;
		AssertNoMessageError("When BM_ForeignDestPortKCode equals to 5 characters and BM_PlaceOfUnloading is Empty", departureMovement.BM_PlaceOfUnloadingInfo, expectedMessageError);
	}

	public void TestCheckBM_PlaceOfUnloading_C0387InActive()
	{
		var departureMovement = CreateDepartureMovement();

		testContext.DisableRule(c => c.IsRuleC0387Active);

		departureMovement.BM_ForeignDestPortKCode = "AC";
		departureMovement.BM_PlaceOfUnloading = ZString.Empty;
		AssertNoNotifications(departureMovement.BM_PlaceOfUnloadingInfo);
	}

	public void TestCheckBM_PlaceOfUnloading_NR0039()
	{
		testContext.EnableRule(c => c.IsRuleNR0039Active);

		var departureMovement = CreateDepartureMovement();
		var msgError = "[NR0039] You have not entered a Place of Unloading.";

		departureMovement.BM_ForeignDestPortKCode = ZString.Empty;
		departureMovement.BM_PlaceOfUnloading = ZString.Empty;
		departureMovement.Validation.ValidateBM_PlaceOfUnloading();
		AssertNoMessageError("Not required when BM_ForeignDestPortKCode is empty", departureMovement.BM_PlaceOfUnloadingInfo, msgError);

		departureMovement.BM_ForeignDestPortKCode = "IE";
		departureMovement.Validation.ValidateBM_PlaceOfUnloading();
		AssertHasMessageError("Required when BM_ForeignDestPortKCode is not empty", departureMovement.BM_PlaceOfUnloadingInfo, msgError);

		testContext.DisableRule(c => c.IsRuleNR0039Active);

		departureMovement.Validation.ValidateBM_PlaceOfUnloading();
		AssertNoMessageError("Not required when rule is disabled", departureMovement.BM_PlaceOfUnloadingInfo, msgError);

		testContext.EnableRule(c => c.IsRuleNR0039Active);

		departureMovement.BM_PlaceOfUnloading = "ABC";
		departureMovement.Validation.ValidateBM_PlaceOfUnloading();
		AssertNoMessageError("No error as entered", departureMovement.BM_PlaceOfUnloadingInfo, msgError);
	}

	public void TestCheckBM_PlaceOfUnloading_MandatoryValidation_RuleC0191_1() => CombineAssertions(() =>
	{
		const string expectedMessageError = "[C0191-1] You have not entered a Place of Unloading.";

		testContext.EnableRule(p => p.IsRuleC0191_1Active);
		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);

		var movementHeader = CreateDepartureMovement();
		var placeOfUnloadingInfo = movementHeader.BM_PlaceOfUnloadingInfo;

		movementHeader.BM_TypeOfSecurity = "ENT";
		movementHeader.BM_ForeignDestPortKCode = "AB";
		movementHeader.BM_PlaceOfUnloading = ZString.Empty;
		movementHeader.Validation.ValidateBM_PlaceOfUnloading();
		AssertHasMessageError("Outside TP Security=ENT, BM_ForeignDestPortKCode=AB, Place of Unloading is NOT entered", placeOfUnloadingInfo, expectedMessageError);

		movementHeader.BM_TypeOfSecurity = "BTH";
		movementHeader.Validation.ValidateBM_PlaceOfUnloading();
		AssertHasMessageError("Outside TP Security=BTH, BM_ForeignDestPortKCode=AB, Place of Unloading is NOT entered", placeOfUnloadingInfo, expectedMessageError);

		movementHeader.BM_ForeignDestPortKCode = "1";
		movementHeader.Validation.ValidateBM_PlaceOfUnloading();
		AssertNoMessageError("Outside TP Security=BTH, BM_ForeignDestPortKCode=1, Place of Unloading is NOT entered", placeOfUnloadingInfo, expectedMessageError);

		movementHeader.BM_ForeignDestPortKCode = "AB1";
		movementHeader.Validation.ValidateBM_PlaceOfUnloading();
		AssertNoMessageError("Outside TP Security=BTH, BM_ForeignDestPortKCode=AB1, Place of Unloading is NOT entered", placeOfUnloadingInfo, expectedMessageError);

		movementHeader.BM_ForeignDestPortKCode = "AB";
		movementHeader.BM_PlaceOfUnloading = "PLACE";
		movementHeader.Validation.ValidateBM_PlaceOfUnloading();
		AssertNoMessageError("Outside TP Security=BTH, BM_ForeignDestPortKCode=AB, Place of Unloading is entered", placeOfUnloadingInfo, expectedMessageError);

		movementHeader.BM_TypeOfSecurity = "NON";
		movementHeader.BM_ForeignDestPortKCode = "AB";
		movementHeader.BM_PlaceOfUnloading = ZString.Empty;
		AssertNoMessageError("Outside TP Security=NON, BM_ForeignDestPortKCode=AB, Place of Unloading is NOT entered", placeOfUnloadingInfo, expectedMessageError);

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		movementHeader = CreateDepartureMovement();
		placeOfUnloadingInfo = movementHeader.BM_PlaceOfUnloadingInfo;
		movementHeader.BM_TypeOfSecurity = "ENT";
		movementHeader.BM_ForeignDestPortKCode = "AB";
		movementHeader.BM_PlaceOfUnloading = ZString.Empty;
		movementHeader.Validation.ValidateBM_PlaceOfUnloading();
		AssertNoMessageError("During TP, Security=ENT, BM_ForeignDestPortKCode=AB, Place of Unloading is NOT entered", placeOfUnloadingInfo, expectedMessageError);
	});

	public void TestCheckBM_PlaceOfUnloading_NotTransmitted_RuleC0191_1Active() => CombineAssertions(() =>
	{
		const string expectedMessageWarning = "[C0191-1] Place of Unloading is not transmitted to customs if Security = NON";

		testContext.EnableRule(p => p.IsRuleC0191_1Active);
		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);

		var movementHeader = CreateDepartureMovement();
		var placeOfUnloadingInfo = movementHeader.BM_PlaceOfUnloadingInfo;

		movementHeader.BM_TypeOfSecurity = "NON";
		movementHeader.BM_PlaceOfUnloading = "VAL";
		movementHeader.Validation.ValidateBM_PlaceOfUnloading();
		AssertHasWarning("Outside TP Security=NON, BM_PlaceOfUnloading is NOT empty", placeOfUnloadingInfo, expectedMessageWarning);

		movementHeader.BM_TypeOfSecurity = "ENT";
		movementHeader.Validation.ValidateBM_PlaceOfUnloading();
		AssertNoWarning("Outside TP Security=ENT, BM_PlaceOfUnloading is NOT empty", placeOfUnloadingInfo, expectedMessageWarning);

		movementHeader.BM_TypeOfSecurity = "NON";
		movementHeader.BM_PlaceOfUnloading = ZString.Empty;
		movementHeader.Validation.ValidateBM_PlaceOfUnloading();
		AssertNoWarning("Outside TP Security=NON, BM_PlaceOfUnloading is empty", placeOfUnloadingInfo, expectedMessageWarning);

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);

		movementHeader = CreateDepartureMovement();
		placeOfUnloadingInfo = movementHeader.BM_PlaceOfUnloadingInfo;
		movementHeader.BM_TypeOfSecurity = "NON";
		movementHeader.BM_ForeignDestPortKCode = ZString.Empty;
		movementHeader.BM_PlaceOfUnloading = "VAL";
		movementHeader.Validation.ValidateBM_PlaceOfUnloading();
		AssertNoWarning("During TP, Security=NON, BM_PlaceOfUnloading is NOT empty", placeOfUnloadingInfo, expectedMessageWarning);
	});

	public void TestCheckBM_PlaceOfUnloading_NotRequired_RuleC0191_1Inactive()
	{
		var movementHeader = CreateDepartureMovement();
		var placeOfUnloadingInfo = movementHeader.BM_PlaceOfUnloadingInfo;

		testContext.DisableRule(p => p.IsRuleC0191_1Active);
		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		movementHeader.BM_TypeOfSecurity = "NON";
		movementHeader.BM_PlaceOfUnloading = "VAL";
		movementHeader.Validation.ValidateBM_PlaceOfUnloading();
		AssertNoNotifications(placeOfUnloadingInfo);
	}

	public void TestCheckGoodsLocationDescription()
	{
		const string message = "There are errors within the 'Location of Goods', please click on 'More..' to view the error information.";

		var departureMovement = CreateDepartureMovement();
		var cusGoodsLocation = departureMovement.GoodsLocation;
		cusGoodsLocation.CGL_Qualifier = "Y";
		departureMovement.Validation.ValidateGoodsLocationDescription();

		CombineAssertions(() =>
		{
			AssertHasMessageError("There should be an error on GoodsLocationDescription when the linked GoodsLocation has errors.", departureMovement.GoodsLocationDescriptionInfo, message);

			cusGoodsLocation.Address.E2_GovRegNum = "ABC";
			departureMovement.Validation.ValidateGoodsLocationDescription();
			AssertNoMessageError("There should be no error on GoodsLocationDescription when the linked GoodsLocation doesn't have errors.", departureMovement.GoodsLocationDescriptionInfo, message);

			using (cusGoodsLocation.GetValidationSuspender())
			{
				cusGoodsLocation.Address.E2_Email = "A";
			}
			departureMovement.Validation.ValidateGoodsLocationDescription();
			AssertHasError("ValidateGoodsLocationDescription triggers validation on GoodsLocation and GoodsLocationAddress", departureMovement.GoodsLocationDescriptionInfo, message);
		});
	}

	void ValidateInBondEntryTypeAndAssertR601MessageError(NctsDepartureMovementHeader departureMovement, string message, bool expectMessageError, bool forPreviousDocs = false)
	{
		const string SupportingDocErrorMessage = "(R0601) Invalid declaration type - T or T1 is allowed.";
		const string PreviousDocErrorMessage = "(R0601) Invalid declaration Type - T or T2 or T2F is allowed.";

		var errorMessage = forPreviousDocs ? PreviousDocErrorMessage : SupportingDocErrorMessage;
		var nctsHeader = departureMovement.Header;
		departureMovement.Validation.ValidateBM_InBondEntryType();
		if (expectMessageError)
		{
			AssertHasMessageErrorContaining($"{message}, IsRulePLR0601Active={nctsHeader.Configuration.ValidationRuleConfiguration.IsRulePLR0601Active}", departureMovement.BM_InBondEntryTypeInfo, errorMessage);
		}
		else
		{
			AssertNoMessageErrorContaining($"{message}, IsRulePLR0601Active={nctsHeader.Configuration.ValidationRuleConfiguration.IsRulePLR0601Active}", departureMovement.BM_InBondEntryTypeInfo, errorMessage);
		}
	}

	public void TestCheckConditionR0020() => CombineAssertions(() =>
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Code_CL112, "CusCodeTypeCL112");
		helper.CreateNewOrGetExistingCusCodeType(Code_CL178, "CusCodeTypeCL178");
		helper.CreateCusCodeList("EUN", Code_CL112, Norway, "Norway", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateCusCodeList("EUN", Code_CL178, "123", "Something", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		Factory.Save();

		testContext.DisableRule(x => x.IsRuleR0020Active);

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var departureMovement = nctsHeader.MovementHeader;
		var messageError = "[R0020] Previous Document of Type in CL178 is required either at Consignment level or for all Goods Item when Country of Customs Office of Departure is from the CL112 (Country Codes CTC) list and the Declaration Type is T2/T2F.";
		var officeOfDeparture = departureMovement.CustomsOffices.AddNew();
		officeOfDeparture.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
		officeOfDeparture.CY_Data = Latvia;
		var propertyInfo = departureMovement.BM_InBondEntryTypeInfo;

		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("R0020 inactive", propertyInfo, messageError);

		testContext.EnableRule(c => c.IsRuleR0020Active);

		departureMovement.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("No Departure office within CL112", propertyInfo, messageError);

		officeOfDeparture.CY_Data = Norway;
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertHasMessageError("T2 - Office within CL112 without CL178 Previous Document", propertyInfo, messageError);

		departureMovement.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2F;
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertHasMessageError("T2F - Office within CL112 without CL178 Previous Document", propertyInfo, messageError);

		departureMovement.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T;
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("Not T2 or T2F", propertyInfo, messageError);

		departureMovement.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;
		var nctsHeaderPreviousDocument = nctsHeader.PreviousDocuments.AddNew();
		nctsHeaderPreviousDocument.CSI_Code = "123";
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("Ncts Header Previous Document in CL178", propertyInfo, messageError);

		nctsHeaderPreviousDocument.CSI_Code = ZString.Empty;
		var goodsItemPreviousDocument = nctsHeader.Bills.AddNew().GoodsItems.AddNew().PreviousDocuments.AddNew();
		goodsItemPreviousDocument.CSI_Code = "123";
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("Goods Item Previous Document in CL178", propertyInfo, messageError);

		goodsItemPreviousDocument.CSI_Code = "999";
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertHasMessageError("Previous document is not in CL178", propertyInfo, messageError);
	});

	public void TestCheckBM_InBondEntryType_R0020_1() => CombineAssertions(() =>
	{
		const string messageError = "[R0020-1] Previous Document of Type in CL178 is required either at Consignment level or for all Goods Item when the Declaration Type is T2/T2F.";

		UniversalReferenceTestDataHelper.EnsureCountriesAreInRefCusCodeList(Factory, Code_CL178, ("123", "Something"));
		Factory.Save();

		string[] entryTypesRequiringCL178 = [NctsPhase5DeclarationTypeList.Codes.T2, NctsPhase5DeclarationTypeList.Codes.T2F];
		var entryTypesNotRequiringCL178 = new NctsPhase5DeclarationTypeList().GetAllCodes().Except(entryTypesRequiringCL178);

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		testContext.AddAutoCacheResetObject(nctsHeader);
		var departureMovement = nctsHeader.MovementHeader;
		testContext.AddAutoCacheResetObject(departureMovement);
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		testContext.AddAutoCacheResetObject(goodsItem);

		testContext.EnableRule(x => x.IsRuleR0020_1Active);

		foreach (var entryType in entryTypesRequiringCL178)
		{
			departureMovement.BM_InBondEntryType = entryType;
			AssertHasMessageError($"{departureMovement.BM_InBondEntryType}: No previous document", departureMovement.BM_InBondEntryTypeInfo, messageError);
		}

		foreach (var entryType in entryTypesNotRequiringCL178)
		{
			departureMovement.BM_InBondEntryType = entryType;
			AssertNoMessageError($"{departureMovement.BM_InBondEntryType}: No previous document", departureMovement.BM_InBondEntryTypeInfo, messageError);
		}

		var headerPreviousDocument = nctsHeader.PreviousDocuments.AddNew();
		headerPreviousDocument.CSI_Code = "123";

		foreach (var entryType in entryTypesRequiringCL178)
		{
			departureMovement.BM_InBondEntryType = entryType;
			AssertNoMessageError($"{departureMovement.BM_InBondEntryType}: CL178 previous document at consigment level", departureMovement.BM_InBondEntryTypeInfo, messageError);
		}

		headerPreviousDocument.CSI_Code = "999";

		foreach (var entryType in entryTypesRequiringCL178)
		{
			departureMovement.BM_InBondEntryType = entryType;
			AssertHasMessageError($"{departureMovement.BM_InBondEntryType}: Other previous document at consigment level", departureMovement.BM_InBondEntryTypeInfo, messageError);
		}

		var itemPreviousDocument = goodsItem.PreviousDocuments.AddNew();
		itemPreviousDocument.CSI_Code = "123";

		foreach (var entryType in entryTypesRequiringCL178)
		{
			departureMovement.BM_InBondEntryType = entryType;
			AssertNoMessageError($"{departureMovement.BM_InBondEntryType}: CL178 previous document at consigment item level", departureMovement.BM_InBondEntryTypeInfo, messageError);
		}

		itemPreviousDocument.CSI_Code = "999";

		foreach (var entryType in entryTypesRequiringCL178)
		{
			departureMovement.BM_InBondEntryType = entryType;
			AssertHasMessageError($"{departureMovement.BM_InBondEntryType}: Other previous document at consigment item level", departureMovement.BM_InBondEntryTypeInfo, messageError);
		}

		testContext.DisableRule(x => x.IsRuleR0020_1Active);
		itemPreviousDocument.CSI_Code = "999";
		foreach (var entryType in entryTypesRequiringCL178)
		{
			departureMovement.BM_InBondEntryType = entryType;
			AssertNoMessageError("R0020-1 inactive", departureMovement.BM_InBondEntryTypeInfo, messageError);
		}
	});

	public void TestCheckBM_InBondEntryType_R0911()
	{
		const string expectedMessageError = "[R0911] Based on Customs Office of Departure and Destination you selected, Declaration Type should be T2 or T2F.";
		const string declarationTypeNotT2OrT2F = "T1";

		var factory = Factory;
		UniversalReferenceTestDataHelper.EnsureCountriesAreInRefCusCodeList(factory, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, (Italy, "Italy"));
		factory.Save();

		testContext.EnableRule(rule => rule.IsRuleR0911Active);

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var movementHeader = nctsHeader.MovementHeader;

		var inBondEntryTypeInfo = movementHeader.BM_InBondEntryTypeInfo;

		CombineAssertions("When declarationType in SET {T2, T2F}, R0911 Irrelevant", () =>
		{
			SetupNCTSOfficeOfDeparture(needR0911StartWIthSM: true);
			SetupNCTSOfficeOfDestination(needR0911CountryCodeInCL010Italy: true);

			movementHeader.BM_InBondEntryType = "T2";
			AssertNoMessageError("When declaration type {movementHeader.BM_InBondEntryType}", inBondEntryTypeInfo, expectedMessageError);

			movementHeader.BM_InBondEntryType = "T2F";
			AssertNoMessageError("When declaration type {movementHeader.BM_InBondEntryType}", inBondEntryTypeInfo, expectedMessageError);
		});

		CombineAssertions($"When declarationType is {declarationTypeNotT2OrT2F}, Check R0911", () =>
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			movementHeader = nctsHeader.MovementHeader;
			inBondEntryTypeInfo = movementHeader.BM_InBondEntryTypeInfo;
			movementHeader.BM_InBondEntryType = declarationTypeNotT2OrT2F;
			movementHeader.CustomsOffices.RemoveAndDeleteAll();
			SetupNCTSOfficeOfDeparture(needR0911StartWIthSM: true);
			SetupNCTSOfficeOfDestination(needR0911CountryCodeInCL010Italy: true);
			AssertHasMessageError("Departure start with SM and Destination CountryCode In CL010", inBondEntryTypeInfo, expectedMessageError);

			movementHeader.CustomsOffices.RemoveAndDeleteAll();
			SetupNCTSOfficeOfDeparture(needR0911StartWIthSM: false);
			SetupNCTSOfficeOfDestination(needR0911CountryCodeInCL010Italy: true);
			AssertNoMessageError("Departure NOT start with SM and Destination CountryCode In CL010", inBondEntryTypeInfo, expectedMessageError);

			movementHeader.CustomsOffices.RemoveAndDeleteAll();
			SetupNCTSOfficeOfDeparture(needR0911StartWIthSM: true);
			SetupNCTSOfficeOfDestination(needR0911CountryCodeInCL010Italy: false);
			AssertNoMessageError("Departure start with SM and Destination CountryCode NOT In CL010", inBondEntryTypeInfo, expectedMessageError);

			testContext.DisableRule(rule => rule.IsRuleR0911Active);

			movementHeader.BM_InBondEntryType = declarationTypeNotT2OrT2F;
			movementHeader.CustomsOffices.RemoveAndDeleteAll();
			SetupNCTSOfficeOfDeparture(needR0911StartWIthSM: true);
			SetupNCTSOfficeOfDestination(needR0911CountryCodeInCL010Italy: true);
			AssertNoMessageError("Disable Rule- Departure start with SM and Destination CountryCode In CL010", inBondEntryTypeInfo, expectedMessageError);
		});

		void SetupNCTSOfficeOfDeparture(bool needR0911StartWIthSM)
		{
			var customsOfficeOfDeparture = nctsHeader.MovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			customsOfficeOfDeparture.CY_Data = needR0911StartWIthSM ? "SM123" : "ZZ123";
		}

		void SetupNCTSOfficeOfDestination(bool needR0911CountryCodeInCL010Italy)
		{
			const string countryCodeNotInCL010 = "IL";
			var customsOfficeOfDestination = nctsHeader.MovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			customsOfficeOfDestination.CY_Data = needR0911CountryCodeInCL010Italy ? $"{Italy}123" : $"{countryCodeNotInCL010}123";
		}
	}

	public void TestCheckAdditionalTransportAtBorderListCount()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NCNAT);
		Factory.Save();

		var departureMovement = CreateDepartureMovement();
		departureMovement.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "AT275100");
		departureMovement.BM_ActiveBorderIdentificationType = "10";
		departureMovement.Validation.ValidateAll();
		AssertNoNotifications("When AdditionalTransportAtBorderList Count = 0", departureMovement.AdditionalTransportAtBorderListCountInfo);

		var additionalTransportBorder = departureMovement.AdditionalTransportAtBorderList.AddNew();
		additionalTransportBorder.TPM_IdentificationNumber = "ABC";
		departureMovement.Validation.ValidateAll();
		AssertHasMessageErrorContaining("When AdditionalTransportAtBorderList Count = 1 and has inner notifications", departureMovement.AdditionalTransportAtBorderListCountInfo, "There are errors within the 'Additional Transport Border Means', please click on 'More..' to view the error information.");

		additionalTransportBorder.TPM_CustomsOffice = "123";
		additionalTransportBorder.TPM_RN_NKTransportNationality = "DE";
		departureMovement.Validation.ValidateAll();
		AssertNoNotifications("When AdditionalTransportAtBorderList Count = 1 and has not inner notifications", departureMovement.AdditionalTransportAtBorderListCountInfo);
	}

	public void TestCheckBM_InlandTransportMode_RuleB1091()
	{
		const string errorMessage = "[B1091] Inland M.O.T selected is not valid during transition period.";

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		testContext.EnableRule(x => x.IsRuleB1091Active);
		var departureMovement = CreateDepartureMovement();
		var targetInfo = departureMovement.InlandTransportModeAtDepartureInfo;
		departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._5_PostalConsignment;
		AssertHasMessageError($"In P5 Transition period, B1091 is active and Inland Transport Mode is {ModeOfTransportList.Codes._5_PostalConsignment}", targetInfo, errorMessage);

		departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._1_SeaTransport;
		AssertNoMessageError($"No B1091 error when Inland Transport Mode is not {ModeOfTransportList.Codes._5_PostalConsignment}", targetInfo, errorMessage);

		testContext.DisableRule(x => x.IsRuleB1091Active);
		departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._5_PostalConsignment;
		AssertNoMessageError("In P5 Transition period, Rule B1091 inactive", targetInfo, errorMessage);

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		departureMovement = CreateDepartureMovement();
		targetInfo = departureMovement.InlandTransportModeAtDepartureInfo;
		testContext.EnableRule(x => x.IsRuleB1091Active);
		departureMovement.Validation.ValidateBM_InlandTransportMode();
		AssertNoMessageError("Outside P5 Transition period, Rule B1091 is active", targetInfo, errorMessage);
	}

	public void TestCheckBM_InBondEntryType_B1922_DeclarationTypeNotT1OrTIR()
	{
		const string errorMessage = "[B1922] Declaration Type must be T1 or TIR.";
		testContext.EnableRule(x => x.IsRuleB1922Active);

		var (departureMovement, goodsItem) = SetupDataForB1922Test();

		var additionalInfo = goodsItem.AdditionalInfos.AddNew();
		additionalInfo.CSI_Code = "XYZ";
		additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;

		var previousDocument = goodsItem.PreviousDocuments.AddNew();
		previousDocument.CSI_Code = NctsTypeOfPreviousDocument.Codes.N830;

		var targetInfo = departureMovement.BM_InBondEntryTypeInfo;
		CombineAssertions(() =>
		{
			testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
			departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
			AssertHasMessageError("Rule active, in Phase5TransitionPeriod, Declaration Type is not T1 or TIR", targetInfo, errorMessage);

			departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
			AssertNoMessageError("Declaration Type is T1", targetInfo, errorMessage);

			departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.TirDeclaration;
			AssertNoMessageError("Declaration Type is TIR", targetInfo, errorMessage);

			additionalInfo.CSI_Code = "ABC";
			departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
			AssertNoMessageError("Goods item have no AdditionalInfo with CL234 Type", targetInfo, errorMessage);

			additionalInfo.CSI_Code = "XYZ";
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageError("Goods item having Additional Info with CL234 Type does not have SubType REF", targetInfo, errorMessage);

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			previousDocument.CSI_Code = NctsTypeOfPreviousDocument.Codes.C658;
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageError("Goods item have no PreviousDocument with Type N830", targetInfo, errorMessage);

			previousDocument.CSI_Code = NctsTypeOfPreviousDocument.Codes.N830;
			testContext.DisableRule(x => x.IsRuleB1922Active);
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageError("No error message as rule is disabled", targetInfo, errorMessage);

			testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
			testContext.EnableRule(x => x.IsRuleB1922Active);
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageError("Rule active, outside Phase5TransitionPeriod", targetInfo, errorMessage);
		});
	}

	public void TestCheckBM_InBondEntryType_B1922_DeclarationTypeNotT2OrT2F()
	{
		const string errorMessage = "[B1922] Declaration Type must be T2 or T2F.";

		testContext.EnableRule(x => x.IsRuleB1922Active);
		var (departureMovement, goodsItem) = SetupDataForB1922Test();

		var additionalInfo = goodsItem.AdditionalInfos.AddNew();
		additionalInfo.CSI_Code = "XYZ";
		additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;

		var targetInfo = departureMovement.BM_InBondEntryTypeInfo;

		CombineAssertions(() =>
		{
			testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
			departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
			AssertHasMessageError("Rule active, in Phase5TransitionPeriod, Declaration Type is not T2 or T2F", targetInfo, errorMessage);

			departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
			AssertNoMessageError("Declaration Type is T2", targetInfo, errorMessage);

			departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedureBetweenDifferentFiscalTerritories;
			AssertNoMessageError("Declaration Type is T2F", targetInfo, errorMessage);

			additionalInfo.CSI_Code = "ABC";
			departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
			AssertNoMessageError("Goods item have no AdditionalInfo with CL234 Type", targetInfo, errorMessage);

			additionalInfo.CSI_Code = "XYZ";
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageError("Goods item having Additional Info with CL234 Type does not have SubType REF", targetInfo, errorMessage);

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			testContext.DisableRule(x => x.IsRuleB1922Active);
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageError("No error message as rule is disabled", targetInfo, errorMessage);

			testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
			testContext.EnableRule(x => x.IsRuleB1922Active);
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageError("Rule active, outside Phase5TransitionPeriod", targetInfo, errorMessage);
		});
	}

	public void TestCheckBM_CustomsOfficeAtBorder_G0789_1()
	{
		const string expectedMessageError = "[G0789-1] Customs Office at Border should be the same as Customs Office of Transit, or Customs office of Exit, or Customs Office of Destination.";
		testContext.EnableRule(x => x.IsRuleG0789_1Active);
		var movementHeader = CreateDepartureMovement();
		movementHeader.CustomsOffices.RemoveAndDeleteAll();

		CombineAssertions("When Rule enabled", () =>
		{
			movementHeader.BM_CustomsOfficeAtBorder = "";
			AssertNoMessageErrorContaining("When BM_CustomsOfficeAtBorder = '' and all other offices are not defined", movementHeader.BM_CustomsOfficeAtBorderInfo, expectedMessageError);

			movementHeader.BM_CustomsOfficeAtBorder = "IT444444";
			AssertNoMessageErrorContaining("When BM_CustomsOfficeAtBorder is filled and all other offices are not defined", movementHeader.BM_CustomsOfficeAtBorderInfo, expectedMessageError);

			var transitCustomsOffice = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
			var exitCustomsOffice = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
			var destinationCustomsOffice = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);

			movementHeader.BM_CustomsOfficeAtBorder = "";
			AssertNoMessageErrorContaining("When BM_CustomsOfficeAtBorder = '' and all other offices are defined but empty", movementHeader.BM_CustomsOfficeAtBorderInfo, expectedMessageError);

			movementHeader.BM_CustomsOfficeAtBorder = "IT444444";
			AssertNoMessageErrorContaining("When BM_CustomsOfficeAtBorder is filled and all other offices are defined but empty", movementHeader.BM_CustomsOfficeAtBorderInfo, expectedMessageError);

			transitCustomsOffice.CY_Data = "IT111111";
			exitCustomsOffice.CY_Data = "IT222222";
			destinationCustomsOffice.CY_Data = "IT333333";

			movementHeader.BM_CustomsOfficeAtBorder = "";
			AssertNoMessageErrorContaining("When BM_CustomsOfficeAtBorder = '' and all other offices are populated", movementHeader.BM_CustomsOfficeAtBorderInfo, expectedMessageError);

			movementHeader.BM_CustomsOfficeAtBorder = "IT444444";
			AssertHasMessageErrorContaining("When BM_CustomsOfficeAtBorder is filled and different to other offices", movementHeader.BM_CustomsOfficeAtBorderInfo, expectedMessageError);

			movementHeader.BM_CustomsOfficeAtBorder = "IT111111";
			AssertNoMessageErrorContaining("When BM_CustomsOfficeAtBorder is filled and equal to office of transit", movementHeader.BM_CustomsOfficeAtBorderInfo, expectedMessageError);

			movementHeader.BM_CustomsOfficeAtBorder = "IT222222";
			AssertNoMessageErrorContaining("When BM_CustomsOfficeAtBorder is filled and equal to office of exit", movementHeader.BM_CustomsOfficeAtBorderInfo, expectedMessageError);

			movementHeader.BM_CustomsOfficeAtBorder = "IT333333";
			AssertNoMessageErrorContaining("When BM_CustomsOfficeAtBorder is filled and equal to office of destination", movementHeader.BM_CustomsOfficeAtBorderInfo, expectedMessageError);
		});

		testContext.DisableRule(x => x.IsRuleG0789_1Active);
		movementHeader.BM_CustomsOfficeAtBorder = "IT444444";
		AssertNoMessageErrorContaining("When Rule disabled, BM_CustomsOfficeAtBorder is filled and different to other offices", movementHeader.BM_CustomsOfficeAtBorderInfo, expectedMessageError);
	}

	public void TestCheckBM_InBondEntryType_ConditionC0035() => CombineAssertions(() =>
	{
		const string messageTag = "(C035)";

		testContext.EnableRule(x => x.IsRuleC035Active);

		var departureMovement = CreateDepartureMovement();
		var nctsHeader = departureMovement.Header;
		var bill = nctsHeader.Bills.AddNew();
		bill.GoodsItems.AddNew();

		NCTSTestHelper.CreateCustomsOfficeForTest(departureMovement, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "NO123456", ZDateTime.Empty);
		departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.TirDeclaration;
		AssertNoMessageErrorContaining("Tir type", departureMovement.BM_InBondEntryTypeInfo, messageTag);

		departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
		AssertHasMessageErrorContaining("T2 type", departureMovement.BM_InBondEntryTypeInfo, messageTag);

		bill.GoodsItems.AddNew().PreviousDocuments.AddNew();
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageErrorContaining("With previous document", departureMovement.BM_InBondEntryTypeInfo, messageTag);
		bill.GoodsItems.DeleteAll();

		departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedureBetweenDifferentFiscalTerritories;
		AssertHasMessageErrorContaining("T2F type", departureMovement.BM_InBondEntryTypeInfo, messageTag);

		departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5;
		AssertNoMessageErrorContaining("T type", departureMovement.BM_InBondEntryTypeInfo, messageTag);

		testContext.DisableRule(x => x.IsRuleC035Active);
		departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
		AssertNoMessageErrorContaining("Rule C035 inactive", departureMovement.BM_InBondEntryTypeInfo, messageTag);
	});

	public void TestCheckBM_InBondEntryType_ConditionC0035_1() => CombineAssertions(() =>
	{
		const string messageTag = "[C0035-1]";

		testContext.InstallAdditionalRuleDecider<IRuleC0035_1Decider>();
		testContext.EnableRuleDecider<IRuleC0035_1Decider>(x => x.IsActive);

		var departureMovement = CreateDepartureMovement();
		var nctsHeader = departureMovement.Header;
		var bill = nctsHeader.Bills.AddNew();
		var goodsItem = bill.GoodsItems.AddNew();

		var entryTypesRequiringPreviousDocument = new[] { NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure, NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedureBetweenDifferentFiscalTerritories };
		var entryTypesNotRequiringPreviousDocument = new[] { NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5, NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure, NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland };

		foreach (var entryType in entryTypesRequiringPreviousDocument)
		{
			departureMovement.BM_InBondEntryType = entryType;
			AssertHasMessageErrorContaining(departureMovement.BM_InBondEntryTypeInfo, messageTag);
		}

		foreach (var entryType in entryTypesNotRequiringPreviousDocument)
		{
			departureMovement.BM_InBondEntryType = entryType;
			AssertNoMessageErrorContaining(departureMovement.BM_InBondEntryTypeInfo, messageTag);
		}

		departureMovement.BM_InBondEntryType = entryTypesRequiringPreviousDocument[0];

		nctsHeader.PreviousDocuments.AddNew();
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageErrorContaining("Previous document at consignment level", departureMovement.BM_InBondEntryTypeInfo, messageTag);
		nctsHeader.PreviousDocuments.RemoveAll();

		goodsItem.PreviousDocuments.AddNew();
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageErrorContaining("Previous document at consignment item level", departureMovement.BM_InBondEntryTypeInfo, messageTag);
		goodsItem.PreviousDocuments.RemoveAll();

		testContext.DisableRuleDecider<IRuleC0035_1Decider>(x => x.IsActive);
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageErrorContaining("C0035_1 inactive", departureMovement.BM_InBondEntryTypeInfo, messageTag);
	});

	public void TestCheckBM_RN_NKTOLCarrierNationality_ConditionB1850() => CombineAssertions(() =>
	{
		var expectedError = "[B1850] In transition period, which is now, 'Nationality' field must be entered";
		testContext.EnableRule(rule => rule.IsRuleB1850Active);
		var movementHeader = CreateDepartureMovement();

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		movementHeader.BM_ExportTransportMode = ZString.Empty;
		AssertNoMessageError("Rule B1850 is active, NC5TP is ON, ExportTransportMode is empty", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		AssertHasMessageError("Rule B1850 is active, NC5TP is ON, ExportTransportMode == 1", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
		AssertNoMessageError("Rule B1850 is active, NC5TP is ON, ExportTransportMode == 2", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
		AssertHasMessageError("Rule B1850 is active, NC5TP is ON, ExportTransportMode == 3", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
		AssertHasMessageError("Rule B1850 is active, NC5TP is ON, ExportTransportMode == 4", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._5_PostalConsignment;
		AssertHasMessageError("Rule B1850 is active, NC5TP is ON, ExportTransportMode == 5", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._7_FixedTransportInstallations;
		AssertHasMessageError("Rule B1850 is active, NC5TP is ON, ExportTransportMode == 7", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
		AssertHasMessageError("Rule B1850 is active, NC5TP is ON, ExportTransportMode == 8", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
		AssertHasMessageError("Rule B1850 is active, NC5TP is ON, ExportTransportMode == 9", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		movementHeader.BM_ExportTransportMode = ZString.Empty;
		AssertNoMessageError("Rule B1850 is active, NC5TP is OFF, ExportTransportMode is empty", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		AssertNoMessageError("Rule B1850 is active, NC5TP is OFF, ExportTransportMode == 1", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
		AssertNoMessageError("Rule B1850 is active, NC5TP is OFF, ExportTransportMode == 2", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
		AssertNoMessageError("Rule B1850 is active, NC5TP is OFF, ExportTransportMode == 3", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
		AssertNoMessageError("Rule B1850 is active, NC5TP is OFF, ExportTransportMode == 4", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._5_PostalConsignment;
		AssertNoMessageError("Rule B1850 is active, NC5TP is OFF, ExportTransportMode == 5", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._7_FixedTransportInstallations;
		AssertNoMessageError("Rule B1850 is active, NC5TP is OFF, ExportTransportMode == 7", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
		AssertNoMessageError("Rule B1850 is active, NC5TP is OFF, ExportTransportMode == 8", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
		AssertNoMessageError("Rule B1850 is active, NC5TP is OFF, ExportTransportMode == 9", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		testContext.DisableRule(rule => rule.IsRuleB1850Active);
		movementHeader.BM_ExportTransportMode = ZString.Empty;
		AssertNoMessageError("Rule B1850 is inactive, ExportTransportMode is empty", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		AssertNoMessageError("Rule B1850 is inactive, ExportTransportMode == 1", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
		AssertNoMessageError("Rule B1850 is inactive, ExportTransportMode == 2", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
		AssertNoMessageError("Rule B1850 is inactive, ExportTransportMode == 3", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
		AssertNoMessageError("Rule B1850 is inactive, ExportTransportMode == 4", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._5_PostalConsignment;
		AssertNoMessageError("Rule B1850 is inactive, ExportTransportMode == 5", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._7_FixedTransportInstallations;
		AssertNoMessageError("Rule B1850 is inactive, ExportTransportMode == 7", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
		AssertNoMessageError("Rule B1850 is inactive, ExportTransportMode == 8", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
		AssertNoMessageError("Rule B1850 is inactive, ExportTransportMode == 9", movementHeader.BM_RN_NKTOLCarrierNationalityInfo, expectedError);
	});

	public void TestBM_TypeOfSecurity_RuleBR5410()
	{
		var expectedError = "[BR5410] Security cannot be 'ENT' or 'BTH'";

		CombineAssertions("When rule is disabled", () =>
		{
			testContext.DisableRule(x => x.IsRuleBR5410Active);
			var movementHeader = CreateDepartureMovement();
			var typeOfSecurityInfo = movementHeader.BM_TypeOfSecurityInfo;
			movementHeader.BM_TypeOfSecurity = "ENT";
			AssertNoMessageError("Rule BR5410 is not active, TypeOfSecurity is ENT", typeOfSecurityInfo, expectedError);
			movementHeader.BM_TypeOfSecurity = "BTH";
			AssertNoMessageError("Rule BR5410 is not active, TypeOfSecurity is BTH", typeOfSecurityInfo, expectedError);
			movementHeader.BM_TypeOfSecurity = "";
			AssertNoMessageError("Rule BR5410 is not active, TypeOfSecurity is empty", typeOfSecurityInfo, expectedError);
		});

		CombineAssertions("When rule is enabled", () =>
		{
			testContext.EnableRule(x => x.IsRuleBR5410Active);
			var movementHeader = CreateDepartureMovement();
			var typeOfSecurityInfo = movementHeader.BM_TypeOfSecurityInfo;
			movementHeader.BM_TypeOfSecurity = "ENT";
			AssertHasMessageError("Rule BR5410 is active, TypeOfSecurity is ENT", typeOfSecurityInfo, expectedError);
			movementHeader.BM_TypeOfSecurity = "BTH";
			AssertHasMessageError("Rule BR5410 is active, TypeOfSecurity is BTH", typeOfSecurityInfo, expectedError);
			movementHeader.BM_TypeOfSecurity = "";
			AssertNoMessageError("Rule BR5410 is active, TypeOfSecurity is empty", typeOfSecurityInfo, expectedError);
		});
	}

	public void TestRuleNR0056()
	{
		var departureMovement = CreateDepartureMovement();

		CombineAssertions("No goods items; RuleNR0056 is disabled.", () =>
		{
			testContext.DisableRule(x => x.IsRuleNR0056Active);
			departureMovement = CreateDepartureMovement();
			AssertHasRuleNR0056MessageError(departureMovement, false, NctsTypeOfDeclaration.Codes.TirDeclaration);
			AssertHasRuleNR0056MessageError(departureMovement, false, ZString.Empty);
		});

		CombineAssertions("No goods items; RuleNR0056 is enabled.", () =>
		{
			testContext.EnableRule(x => x.IsRuleNR0056Active);
			departureMovement = CreateDepartureMovement();
			AssertHasRuleNR0056MessageError(departureMovement, false, NctsTypeOfDeclaration.Codes.TirDeclaration);
			AssertHasRuleNR0056MessageError(departureMovement, false, ZString.Empty);
		});

		CombineAssertions("One goods item with no documents; RuleNR0056 is disabled.", () =>
		{
			testContext.DisableRule(x => x.IsRuleNR0056Active);
			departureMovement = CreateDepartureMovement();
			departureMovement.Header.Bills.AddNew().GoodsItems.AddNew();
			AssertHasRuleNR0056MessageError(departureMovement, false, NctsTypeOfDeclaration.Codes.TirDeclaration);
			AssertHasRuleNR0056MessageError(departureMovement, false, ZString.Empty);
		});

		CombineAssertions("One goods item with no documents; RuleNR0056 is enabled.", () =>
		{
			testContext.EnableRule(x => x.IsRuleNR0056Active);
			departureMovement = CreateDepartureMovement();
			departureMovement.Header.Bills.AddNew().GoodsItems.AddNew();
			AssertHasRuleNR0056MessageError(departureMovement, true, NctsTypeOfDeclaration.Codes.TirDeclaration);
			AssertHasRuleNR0056MessageError(departureMovement, false, ZString.Empty);
		});

		CombineAssertions("One goods item with no N952 previous documents; RuleNR0056 is disabled.", () =>
		{
			testContext.DisableRule(x => x.IsRuleNR0056Active);
			departureMovement = CreateDepartureMovement();
			var goodsItem1 = departureMovement.Header.Bills.AddNew().GoodsItems.AddNew();
			var goodsItem1Doc1 = goodsItem1.PreviousDocuments.AddNew();
			goodsItem1Doc1.CSI_Code = ZString.Empty;
			AssertHasRuleNR0056MessageError(departureMovement, false, NctsTypeOfDeclaration.Codes.TirDeclaration);
			AssertHasRuleNR0056MessageError(departureMovement, false, ZString.Empty);
		});

		CombineAssertions("One goods item with no N952 previous documents; RuleNR0056 is enabled.", () =>
		{
			testContext.EnableRule(x => x.IsRuleNR0056Active);
			departureMovement = CreateDepartureMovement();
			var goodsItem1 = departureMovement.Header.Bills.AddNew().GoodsItems.AddNew();
			var goodsItem1Doc1 = goodsItem1.PreviousDocuments.AddNew();
			goodsItem1Doc1.CSI_Code = ZString.Empty;
			AssertHasRuleNR0056MessageError(departureMovement, true, NctsTypeOfDeclaration.Codes.TirDeclaration);
			AssertHasRuleNR0056MessageError(departureMovement, false, ZString.Empty);
		});

		var goodsItem1 = departureMovement.Header.Bills.AddNew().GoodsItems.AddNew();
		var goodsItem1Doc1 = goodsItem1.PreviousDocuments.AddNew();
		goodsItem1Doc1.CSI_Code = NctsTypeOfPreviousDocument.Codes.N952;

		CombineAssertions("One goods item with one N952 previous document; RuleNR0056 is disabled.", () =>
		{
			testContext.DisableRule(x => x.IsRuleNR0056Active);
			departureMovement = CreateDepartureMovement();
			AssertHasRuleNR0056MessageError(departureMovement, false, NctsTypeOfDeclaration.Codes.TirDeclaration);
			AssertHasRuleNR0056MessageError(departureMovement, false, ZString.Empty);
		});

		CombineAssertions("One goods item with one N952 previous document; RuleNR0056 is enabled.", () =>
		{
			testContext.EnableRule(x => x.IsRuleNR0056Active);
			departureMovement = CreateDepartureMovement();
			AssertHasRuleNR0056MessageError(departureMovement, false, NctsTypeOfDeclaration.Codes.TirDeclaration);
			AssertHasRuleNR0056MessageError(departureMovement, false, ZString.Empty);
		});

		var goodsItem1Doc2 = goodsItem1.PreviousDocuments.AddNew();
		goodsItem1Doc2.CSI_Code = ZString.Empty;

		CombineAssertions("One goods item with multiple previous docs and only one N952 previous document; RuleNR0056 is disabled.", () =>
		{
			testContext.DisableRule(x => x.IsRuleNR0056Active);
			departureMovement = CreateDepartureMovement();
			AssertHasRuleNR0056MessageError(departureMovement, false, NctsTypeOfDeclaration.Codes.TirDeclaration);
			AssertHasRuleNR0056MessageError(departureMovement, false, ZString.Empty);
		});

		CombineAssertions("One goods item with multiple previous docs and only one N952 previous document; RuleNR0056 is enabled.", () =>
		{
			testContext.EnableRule(x => x.IsRuleNR0056Active);
			departureMovement = CreateDepartureMovement();
			AssertHasRuleNR0056MessageError(departureMovement, false, NctsTypeOfDeclaration.Codes.TirDeclaration);
			AssertHasRuleNR0056MessageError(departureMovement, false, ZString.Empty);
		});

		CombineAssertions("Multiple goods items with multiple previous docs and only one goods item with N952 previous document; RuleNR0056 is disabled.", () =>
		{
			testContext.DisableRule(x => x.IsRuleNR0056Active);
			departureMovement = CreateDepartureMovement();
			var goodsItem1 = departureMovement.Header.Bills.AddNew().GoodsItems.AddNew();
			var goodsItem1Doc1 = goodsItem1.PreviousDocuments.AddNew();
			goodsItem1Doc1.CSI_Code = NctsTypeOfPreviousDocument.Codes.N952;
			var goodsItem2 = departureMovement.Header.Bills.AddNew().GoodsItems.AddNew();
			var goodsItem2Doc1 = goodsItem2.PreviousDocuments.AddNew();
			goodsItem2Doc1.CSI_Code = ZString.Empty;
			var goodsItem2Doc2 = goodsItem2.PreviousDocuments.AddNew();
			goodsItem2Doc2.CSI_Code = ZString.Empty;
			AssertHasRuleNR0056MessageError(departureMovement, false, NctsTypeOfDeclaration.Codes.TirDeclaration);
			AssertHasRuleNR0056MessageError(departureMovement, false, ZString.Empty);
		});

		CombineAssertions("Multiple goods items with multiple previous docs and only one goods item with N952 previous document; RuleNR0056 is enabled.", () =>
		{
			testContext.EnableRule(x => x.IsRuleNR0056Active);
			departureMovement = CreateDepartureMovement();
			var goodsItem1 = departureMovement.Header.Bills.AddNew().GoodsItems.AddNew();
			var goodsItem1Doc1 = goodsItem1.PreviousDocuments.AddNew();
			goodsItem1Doc1.CSI_Code = NctsTypeOfPreviousDocument.Codes.N952;
			var goodsItem2 = departureMovement.Header.Bills.AddNew().GoodsItems.AddNew();
			var goodsItem2Doc1 = goodsItem2.PreviousDocuments.AddNew();
			goodsItem2Doc1.CSI_Code = ZString.Empty;
			var goodsItem2Doc2 = goodsItem2.PreviousDocuments.AddNew();
			goodsItem2Doc2.CSI_Code = ZString.Empty;
			AssertHasRuleNR0056MessageError(departureMovement, true, NctsTypeOfDeclaration.Codes.TirDeclaration);
			AssertHasRuleNR0056MessageError(departureMovement, false, ZString.Empty);
		});

		CombineAssertions("Multiple goods items with multiple previous docs and all goods items have a N952 previous document; RuleNR0056 is disabled.", () =>
		{
			testContext.DisableRule(x => x.IsRuleNR0056Active);
			departureMovement = CreateDepartureMovement();
			var goodsItem1 = departureMovement.Header.Bills.AddNew().GoodsItems.AddNew();
			var goodsItem1Doc1 = goodsItem1.PreviousDocuments.AddNew();
			goodsItem1Doc1.CSI_Code = NctsTypeOfPreviousDocument.Codes.N952;
			var goodsItem2 = departureMovement.Header.Bills.AddNew().GoodsItems.AddNew();
			var goodsItem2Doc1 = goodsItem2.PreviousDocuments.AddNew();
			goodsItem2Doc1.CSI_Code = ZString.Empty;
			var goodsItem2Doc2 = goodsItem2.PreviousDocuments.AddNew();
			goodsItem2Doc2.CSI_Code = NctsTypeOfPreviousDocument.Codes.N952;
			AssertHasRuleNR0056MessageError(departureMovement, false, NctsTypeOfDeclaration.Codes.TirDeclaration);
			AssertHasRuleNR0056MessageError(departureMovement, false, ZString.Empty);
		});

		CombineAssertions("Multiple goods items with multiple previous docs and all goods items have a N952 previous document; RuleNR0056 is enabled.", () =>
		{
			testContext.EnableRule(x => x.IsRuleNR0056Active);
			departureMovement = CreateDepartureMovement();
			var goodsItem1 = departureMovement.Header.Bills.AddNew().GoodsItems.AddNew();
			var goodsItem1Doc1 = goodsItem1.PreviousDocuments.AddNew();
			goodsItem1Doc1.CSI_Code = NctsTypeOfPreviousDocument.Codes.N952;
			var goodsItem2 = departureMovement.Header.Bills.AddNew().GoodsItems.AddNew();
			var goodsItem2Doc1 = goodsItem2.PreviousDocuments.AddNew();
			goodsItem2Doc1.CSI_Code = ZString.Empty;
			var goodsItem2Doc2 = goodsItem2.PreviousDocuments.AddNew();
			goodsItem2Doc2.CSI_Code = NctsTypeOfPreviousDocument.Codes.N952;
			AssertHasRuleNR0056MessageError(departureMovement, false, NctsTypeOfDeclaration.Codes.TirDeclaration);
			AssertHasRuleNR0056MessageError(departureMovement, false, ZString.Empty);
		});

		CombineAssertions("Multiple goods items with multiple previous docs and all goods items have at least one N952 previous document; RuleNR0056 is disabled.", () =>
		{
			testContext.DisableRule(x => x.IsRuleNR0056Active);
			departureMovement = CreateDepartureMovement();
			var goodsItem1 = departureMovement.Header.Bills.AddNew().GoodsItems.AddNew();
			var goodsItem1Doc1 = goodsItem1.PreviousDocuments.AddNew();
			goodsItem1Doc1.CSI_Code = NctsTypeOfPreviousDocument.Codes.N952;
			var goodsItem2 = departureMovement.Header.Bills.AddNew().GoodsItems.AddNew();
			var goodsItem2Doc1 = goodsItem2.PreviousDocuments.AddNew();
			goodsItem2Doc1.CSI_Code = ZString.Empty;
			var goodsItem2Doc2 = goodsItem2.PreviousDocuments.AddNew();
			goodsItem2Doc2.CSI_Code = NctsTypeOfPreviousDocument.Codes.N952;
			AssertHasRuleNR0056MessageError(departureMovement, false, NctsTypeOfDeclaration.Codes.TirDeclaration);
			AssertHasRuleNR0056MessageError(departureMovement, false, ZString.Empty);
		});

		CombineAssertions("Multiple goods items with multiple previous docs and all goods items have at least one N952 previous document; RuleNR0056 is enabled.", () =>
		{
			testContext.EnableRule(x => x.IsRuleNR0056Active);
			departureMovement = CreateDepartureMovement();
			var goodsItem1 = departureMovement.Header.Bills.AddNew().GoodsItems.AddNew();
			var goodsItem1Doc1 = goodsItem1.PreviousDocuments.AddNew();
			goodsItem1Doc1.CSI_Code = NctsTypeOfPreviousDocument.Codes.N952;
			var goodsItem2 = departureMovement.Header.Bills.AddNew().GoodsItems.AddNew();
			var goodsItem2Doc1 = goodsItem2.PreviousDocuments.AddNew();
			goodsItem2Doc1.CSI_Code = ZString.Empty;
			var goodsItem2Doc2 = goodsItem2.PreviousDocuments.AddNew();
			goodsItem2Doc2.CSI_Code = NctsTypeOfPreviousDocument.Codes.N952;
			AssertHasRuleNR0056MessageError(departureMovement, false, NctsTypeOfDeclaration.Codes.TirDeclaration);
			AssertHasRuleNR0056MessageError(departureMovement, false, ZString.Empty);
		});
	}

	public void TestCheckBM_InBondEntryType_RuleR0507() => CombineAssertions(() =>
	{
		var departureMovement = CreateDepartureMovement();
		var targetInfo = departureMovement.BM_InBondEntryTypeInfo;
		var nctsHeader = departureMovement.Header;
		var nctsBill = nctsHeader.Bills.AddNew();
		var goodsItem = nctsBill.GoodsItems.AddNew();
		var humanReadableName = goodsItem.BY_TypeInfo.HumanReadableName;

		departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5;
		var singleItemMessageError = $"[R0507] For Declaration Type = ‘T’ (Mixed transit) there must be at least two Consignment Items with different {humanReadableName}.";
		var generalMessageError = $"[R0507] {humanReadableName} must be different for at least one of the consignment items.";
		using var testContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory);

		testContext.AddAutoCacheResetObject(goodsItem);

		testContext.EnableRule(x => x.IsRuleR0507Active);
		goodsItem.BY_Type = "A";
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertHasMessageError("Single consignment item, should trigger error message", targetInfo, singleItemMessageError);

		var goodsItem2 = nctsBill.GoodsItems.AddNew();
		testContext.AddAutoCacheResetObject(goodsItem2);
		goodsItem2.BY_Type = "A";
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertHasMessageError($"Consignment items with same {humanReadableName}", targetInfo, generalMessageError);

		goodsItem.BY_Type = "B";
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError($"Consignment items with different {humanReadableName}", targetInfo, generalMessageError);

		var goodsItemOfOtherBill = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		goodsItemOfOtherBill.BY_Type = "A";
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertHasMessageError($"Consignment items with same {humanReadableName} but in different Bill but the new bill has single consignment item", targetInfo, singleItemMessageError);

		testContext.DisableRule(x => x.IsRuleR0507Active);
		goodsItem.BY_Type = "A";
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError($"Consignment items with same {humanReadableName} but RuleR0507 deactivated", targetInfo, generalMessageError);
	});

	public void TestCheckAuthorizations_TR0051()
	{
		var expectedWarning = "[TR0051] When an ACR authorization is declared, it is mandatory to declare also an SSE authorization.";
		var departureMovement = CreateDepartureMovement();

		testContext.EnableRule(decider => decider.IsRuleTR0051Active);
		var authDep = departureMovement.CusAuthorizationUsages.AddNew();
		authDep.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
		departureMovement.Validation.ValidateAll();
		AssertHasRowWarning("Is validated because IsRuleTR0051Active is true", authDep, expectedWarning);

		testContext.DisableRule(decider => decider.IsRuleTR0051Active);
		departureMovement.Validation.ValidateAll();
		AssertNoRowWarningContaining("Is not validated because IsRuleTR0051Active is false", authDep, expectedWarning);
	}

	public void TestCheckBM_RN_NKTransportAtDepartureTrailer1Nationality()
	{
		var departureMovement = CreateDepartureMovement();
		var targetInfo = departureMovement.BM_RN_NKTransportAtDepartureTrailer1NationalityInfo;

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		CombineAssertions("TP : OFF, B2101 is applicable and B1897 is not applicable.", () =>
		{
			testContext.EnableRule(x => x.IsRuleB2101Active);
			testContext.EnableRule(x => x.IsRuleB1897Active);

			departureMovement.BM_TransportAtDepartureTrailer1RegNo = "ABC";
			departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer1Nationality();
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(targetInfo);

			testContext.DisableRule(x => x.IsRuleB2101Active);
			departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer1Nationality();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
		});

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		CombineAssertions("TP : ON, B2101 is not applicable and B1897 is applicable.", () =>
		{
			testContext.EnableRule(x => x.IsRuleB2101Active);
			testContext.EnableRule(x => x.IsRuleB1897Active);

			departureMovement.BM_TransportAtDepartureTrailer1RegNo = "ABC";
			departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer1Nationality();
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(targetInfo);

			testContext.DisableRule(x => x.IsRuleB1897Active);
			departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer1Nationality();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
		});
	}

	public void TestCheckBM_RN_NKTransportAtDepartureTrailer2Nationality()
	{
		var departureMovement = CreateDepartureMovement();
		var targetInfo = departureMovement.BM_RN_NKTransportAtDepartureTrailer2NationalityInfo;

		testContext.SetNctsTransitionPeriod(false, RefDataGroupingCodes.EuropeanUnionEUN);
		CombineAssertions("TP : OFF, B2101 is applicable and B1897 is not applicable.", () =>
		{
			testContext.EnableRule(x => x.IsRuleB1897Active);
			testContext.EnableRule(x => x.IsRuleB2101Active);

			departureMovement.BM_TransportAtDepartureTrailer2RegNo = "ABC";
			departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer2Nationality();
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(targetInfo);

			testContext.DisableRule(x => x.IsRuleB2101Active);
			departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer2Nationality();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
		});

		testContext.SetNctsTransitionPeriod(true, RefDataGroupingCodes.EuropeanUnionEUN);
		CombineAssertions("TP : ON, B2101 is not applicable and B1897 is applicable.", () =>
		{
			testContext.EnableRule(x => x.IsRuleB1897Active);

			departureMovement.BM_TransportAtDepartureTrailer2RegNo = "ABC";
			departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer2Nationality();
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(targetInfo);

			testContext.DisableRule(x => x.IsRuleB1897Active);
			departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer2Nationality();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
		});
	}

	public void TestValidateGuarantees()
	{
		var messageErrorForTR0086 = "[TR0086] Total liability amount does not match with the sum of duties and taxes of all Goods Items";
		SetUpTariff();

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);

		var bill = header.Bills.AddNew();
		var goodsItem1 = bill.GoodsItems.AddNew();
		goodsItem1.BY_HarmonisedTariff = "0304798000";
		goodsItem1.BY_MonetaryValue = 1_000m;

		var goodsItem2 = bill.GoodsItems.AddNew();
		goodsItem2.BY_HarmonisedTariff = "0304798000";
		goodsItem2.BY_MonetaryValue = 1_000m;

		var movementHeader = header.MovementHeader;
		var guarantee = movementHeader.Guarantees.AddNew();
		var guarantee2 = movementHeader.Guarantees.AddNew();

		CombineAssertions("Pre-req", () =>
		{
			AssertEquals("goodsItem1: VatAmount", 59.5m, goodsItem1.VatAmount);
			AssertEquals("goodsItem1: DutyAmount", 120m, goodsItem1.DutyAmount);
			AssertEquals("goodsItem1: AntiDumpingDutyAmount", 50m, goodsItem1.AntiDumpingDutyAmount);
			AssertEquals("goodsItem1: CountervailingDutyAmount", 20m, goodsItem1.CountervailingDutyAmount);

			AssertEquals("goodsItem2: VatAmount", 59.5m, goodsItem2.VatAmount);
			AssertEquals("goodsItem2: DutyAmount", 120m, goodsItem2.DutyAmount);
			AssertEquals("goodsItem2: AntiDumpingDutyAmount", 50m, goodsItem2.AntiDumpingDutyAmount);
			AssertEquals("goodsItem2: CountervailingDutyAmount", 20m, goodsItem2.CountervailingDutyAmount);
		});

		CombineAssertions(() =>
		{
			testContext.EnableRule(x => x.IsRuleTR0086Active);
			var validation = (NctsDepartureMovementHeaderPhase5Validation)movementHeader.Validation;
			validation.ValidateGuarantees();
			AssertNoRowWarningContaining(guarantee, messageErrorForTR0086);
			AssertNoRowWarningContaining(guarantee2, messageErrorForTR0086);

			guarantee.PW_Override = true;
			guarantee.PW_BondAmount = 489m;
			guarantee2.PW_Override = true;
			guarantee2.PW_BondAmount = 10m;

			validation.ValidateGuarantees();
			AssertNoRowWarningContaining(guarantee, messageErrorForTR0086);
			AssertNoRowWarningContaining(guarantee2, messageErrorForTR0086);

			guarantee.PW_BondAmount = 20m;
			validation.ValidateGuarantees();
			AssertHasRowWarningContaining(guarantee, messageErrorForTR0086);
			AssertHasRowWarningContaining(guarantee2, messageErrorForTR0086);

			guarantee2.PW_Override = false;
			guarantee.PW_Override = false;
			validation.ValidateGuarantees();
			AssertNoRowWarningContaining(guarantee, messageErrorForTR0086);
			AssertNoRowWarningContaining(guarantee2, messageErrorForTR0086);

			guarantee2.PW_Override = true;
			guarantee.PW_Override = true;
			guarantee.PW_BondAmount = 489m;
			guarantee2.PW_BondAmount = 10m;
			guarantee2.PW_SuretyCode = "HAL";
			validation.ValidateGuarantees();
			AssertHasRowWarningContaining(guarantee, messageErrorForTR0086);
			AssertHasRowWarningContaining(guarantee2, messageErrorForTR0086);

			guarantee2.PW_BondAmount = 5m;
			validation.ValidateGuarantees();
			AssertNoRowWarningContaining(guarantee, messageErrorForTR0086);
			AssertNoRowWarningContaining(guarantee2, messageErrorForTR0086);

			guarantee2.PW_SuretyCode = "ZER";
			guarantee2.PW_BondAmount = 0m;
			guarantee.PW_BondAmount = 499m;
			validation.ValidateGuarantees();
			AssertHasRowWarningContaining(guarantee, messageErrorForTR0086);
			AssertHasRowWarningContaining(guarantee2, messageErrorForTR0086);

			guarantee.PW_SuretyCode = "ZER";
			validation.ValidateGuarantees();
			AssertNoRowWarningContaining(guarantee, messageErrorForTR0086);
			AssertNoRowWarningContaining(guarantee2, messageErrorForTR0086);

			testContext.DisableRule(x => x.IsRuleTR0086Active);
			guarantee2.PW_Override = true;
			guarantee.PW_Override = true;
			guarantee.PW_BondAmount = 489m;
			guarantee2.PW_BondAmount = 10m;
			validation.ValidateGuarantees();
			AssertNoRowWarningContaining(guarantee, messageErrorForTR0086);
			AssertNoRowWarningContaining(guarantee2, messageErrorForTR0086);

			guarantee.PW_BondAmount = 20m;
			validation.ValidateGuarantees();
			AssertNoRowWarningContaining(guarantee, messageErrorForTR0086);
			AssertNoRowWarningContaining(guarantee2, messageErrorForTR0086);
		});
	}

	void SetUpTariff()
	{
		var eunCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
		var minDate = ZDateTime.MinSmallDateTimeValue;
		var maxDate = ZDateTime.MaxSmallDateTimeValue;
		var helper = new UniversalReferenceTestDataHelper(Factory);

		var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(eunCode);
		_ = helper.CreateNewOrGetExistingDataGrouping(Latvia, "Latvia", parentDataGrouping);
		var tariffType = helper.CreateNewOrGetExistingTariffType(eunCode, "IMP");
		Factory.Save();

		var tariff = helper.CreateTariff(eunCode, tariffType.PK, "0304798000", minDate, maxDate);
		var preference1 = helper.CreatePreferenceForCountryAndGrouping("100", "100", eunCode, "EUN");
		var tradeGroup = helper.CreateTradeGroup(eunCode, "AD", minDate, maxDate);
		_ = helper.AddCountry(tradeGroup, "EU");

		void SetupDataForRateType(string rateType, string rateCode, string formula)
		{
			var cusRateType = helper.CreateCusRateType(eunCode, rateType);
			var cusRateCode = helper.CreateCusRateCode(Factory, rateCode, cusRateType.PK);
			var cusRate = helper.CreateRefCusRate(tariff.PK, cusRateCode.PK, minDate, maxDate, formula, preference1.PK);
			_ = helper.CreateCusApplicability(cusRate.PK, tradeGroup, minDate, maxDate);
		}

		SetupDataForRateType(Universal.Constants.RateTypes.Duty, "A00", "VFD * 0.12");
		SetupDataForRateType(Universal.Constants.RateTypes.AntiDumping, "RC1", "VFD * 0.05");
		SetupDataForRateType(Universal.Constants.RateTypes.Countervailing, "CV1", "VFD * 0.02");

		_ = helper.CreateTaxOrFee("RID", 0.05m, Latvia);
		_ = helper.CreateNewOrGetExistingVATApplicability(tariff, Latvia, "RID");

		Factory.Save();
	}

	void AssertHasRuleNR0056MessageError(NctsDepartureMovementHeader departureMovement, bool expectedError, ZString declarationType)
	{
		const string errorMessage = "[NR0056] A TIR declaration requires a type of document 'N952' in 'Previous documents' of all items.";

		departureMovement.BM_InBondEntryType = declarationType;
		departureMovement.Validation.ValidateBM_InBondEntryType();
		var inBondEntryTypeInfo = departureMovement.BM_InBondEntryTypeInfo;

		if (expectedError)
		{
			AssertHasMessageErrorContaining($"[DeclarationType: {declarationType}] Document with type N952 missing in Previous Documents.", inBondEntryTypeInfo, errorMessage);
		}
		else
		{
			AssertNoMessageError($"[DeclarationType: {declarationType}] Document with type N952 not expected to be available in Previous Documents.", inBondEntryTypeInfo, errorMessage);
		}
	}

	void AssertTrainAndWagonNumberNoErrorMessage(ZPropertyInfo targetInfo)
	{
		var departureMovement = CreateDepartureMovement();

		const string message = "You may only enter either a Train Number or a Wagon Number.";
		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;

		departureMovement.BM_TransportAtDepartureTrailer1RegNo = "Wagon 123";
		departureMovement.BM_TransportAtDeparture = "Train 123";
		AssertNoMessageError("Both Train Number and Wagon Number are entered, no error message", targetInfo, message);

		departureMovement.BM_TransportAtDeparture = string.Empty;
		AssertNoMessageError("Train Number is empty no error message", targetInfo, message);

		departureMovement.BM_TransportAtDepartureTrailer1RegNo = string.Empty;
		AssertNoMessageError("Train Number and Wagon number are empty, no error message", targetInfo, message);
	}

	(NctsDepartureMovementHeader, NctsDepartureCargoDesc) SetupDataForB1922Test()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(Latvia, "Latvia", eun);
		helper.CreateNewOrGetExistingCusCodeType(Code_CL234, "CusCodeTypeCL234");
		helper.CreateCusCodeList(RefDataGroupingCodes.EuropeanUnionEUN, Code_CL234, code: "XYZ", description: "Code Description", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		Factory.Save();

		var departureMovement = CreateDepartureMovement();
		testContext.AddAutoCacheResetObject(departureMovement);
		var bill = departureMovement.Header.Bills.AddNew();
		testContext.AddAutoCacheResetObject(bill);
		var goodsItem = bill.GoodsItems.AddNew();
		testContext.AddAutoCacheResetObject(goodsItem);
		return (departureMovement, goodsItem);
	}

	NctsDepartureMovementHeader CreateDepartureMovement()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		testContext.AddAutoCacheResetObject(nctsHeader);
		var movementHeader = nctsHeader.MovementHeader;
		testContext.AddAutoCacheResetObject(movementHeader);
		return movementHeader;
	}

	protected override void SetUp()
	{
		base.SetUp();
		testContext = new(Factory);
	}

	protected override void TearDown()
	{
		base.TearDown();
		testContext?.Dispose();
	}

	MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider> testContext;

	#region NctsDepartureMovementHeaderForValidationTest

	sealed class NctsDepartureMovementHeaderForValidationTest : NctsDepartureMovementHeader
	{
		public NctsDepartureMovementHeaderForValidationTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override CusInBondMoveHeaderLookups GetNewPhase5Lookups()
		{
			return new LookupsForTest(this);
		}

		protected override CusInBondMoveHeaderLookups GetNewPhase4Lookups()
		{
			return new LookupsForTest(this);
		}

		sealed class LookupsForTest : NctsDepartureMovementHeaderPhase5Lookups
		{
			public LookupsForTest(NctsDepartureMovementHeader parent) : base(parent)
			{
			}

			protected override CodeDescriptionPairList TransportAtBorderTypeOfIdListCore
			{
				get
				{
					var listWithout99 = new NctsTransportTypeOfIdList();
					listWithout99.RemoveCode(NctsTransportTypeOfIdList.Codes._99);

					return listWithout99;
				}
			}
		}
	}

	#endregion
}
