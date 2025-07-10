using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class NctsDepartureMovementHeaderPhase4ValidationTest : TestCaseWithFactory
{
	public void TestCheckBM_TransportAtDeparture_ConditionTR9090_Active() => CombineAssertions(() =>
	{
		const string notRequiredErrorMessage = "Identity of Means of Transport at departure cannot be used.";
		const string requiredErrorMessage = "Identity of Means of Transport at departure is required.";
		testContext.EnableRule(r => r.IsRuleTR9090Active);

		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._5_PostalConsignment;
		departureMovement.BM_TransportAtDeparture = "NOTALLOWED";
		AssertHasMessageErrorContaining("Postal Consignment entered", departureMovement.BM_TransportAtDepartureInfo, notRequiredErrorMessage);
		departureMovement.BM_TransportAtDeparture = ZString.Empty;
		AssertNoMessageErrorContaining("Postal Consignment empty", departureMovement.BM_TransportAtDepartureInfo, notRequiredErrorMessage);

		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		departureMovement.Validation.ValidateBM_TransportAtDeparture();
		AssertHasMessageErrorContaining("Sea Transport Empty", departureMovement.BM_TransportAtDepartureInfo, requiredErrorMessage);
		NCTSTestHelper.AddContainerAndSealsForTest(nctsHeader, "CONTAINER1", "SEAL1", "SEAL2", "", "");
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		NCTSTestHelper.SetContainerPivotForTest(goodsItem, 0);
		departureMovement.Validation.ValidateBM_TransportAtDeparture();
		AssertNoMessageErrorContaining("Sea Transport Containerised", departureMovement.BM_TransportAtDepartureInfo, requiredErrorMessage);
	});

	public void TestCheckBM_TransportAtDeparture_ConditionTR9090_Inactive() => CombineAssertions(() =>
	{
		const string notRequiredErrorMessage = "Identity of Means of Transport at departure cannot be used.";
		const string requiredErrorMessage = "Identity of Means of Transport at departure is required.";
		testContext.DisableRule(r => r.IsRuleTR9090Active);

		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._5_PostalConsignment;
		departureMovement.BM_TransportAtDeparture = "NOTALLOWED";
		AssertEquals("Postal Consignment entered", false, departureMovement.BM_TransportAtDepartureInfo.Notifications.Select(x => x.Message).Any(x => x.Contains(notRequiredErrorMessage)));

		departureMovement.BM_TransportAtDeparture = ZString.Empty;
		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		departureMovement.Validation.ValidateBM_TransportAtDeparture();
		AssertEquals("Sea Transport Empty", false, departureMovement.BM_TransportAtDepartureInfo.Notifications.Select(x => x.Message).Any(x => x.Contains(requiredErrorMessage)));

		testContext.VerifyRule(r => r.IsRuleTR9090Active);
	});

	public void TestCheckBM_RN_NKTransportAtDepartureTrailer1Nationality() => CombineAssertions(() =>
	{
		var targetInfo = departureMovement.BM_RN_NKTransportAtDepartureTrailer1NationalityInfo;
		ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);

		departureMovement.BM_TransportAtDepartureTrailer1RegNo = "ABC";
		departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer1Nationality();
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
	});

	public void TestCheckBM_RN_NKTransportAtDepartureTrailer2Nationality() => CombineAssertions(() =>
	{
		var targetInfo = departureMovement.BM_RN_NKTransportAtDepartureTrailer2NationalityInfo;
		ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);

		departureMovement.BM_TransportAtDepartureTrailer2RegNo = "ABC";
		departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer2Nationality();
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
	});

	public void TestCheckBM_RL_NKDestinationPort_ListC0009()
	{
		const string errorMessage = "For T2 Declarations [15A] and/or [17A] must be a country from Code List 9 (Countries if European Union + NCTS Contracting Parties).";

		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
		helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);
		Factory.Save();

		var goodItem = departureMovement.GoodsItems.AddNew();

		CombineAssertions(() =>
		{
			departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
			departureMovement.BM_RL_NKDestinationPort = "XX";
			AssertHasMessageErrorContaining("XX is not in List 9", departureMovement.BM_RL_NKDestinationPortInfo, errorMessage);

			departureMovement.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Germany;
			AssertNoMessageErrorContaining("DE is in List 9", departureMovement.BM_RL_NKDestinationPortInfo, errorMessage);

			departureMovement.BM_RL_NKDestinationPort = "";
			AssertNoMessageErrorContaining("Empty and not read only", departureMovement.BM_RL_NKDestinationPortInfo, errorMessage);

			goodItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Germany;
			departureMovement.Validation.ValidateBM_RL_NKDestinationPort();
			AssertNoMessageErrorContaining("Validate it only if not read only", departureMovement.BM_RL_NKDestinationPortInfo, errorMessage);
		});
	}

	public void TestCheckBM_RN_NKTransportAtDepartureCountry_ConditionTR9095_Active() => CombineAssertions(() =>
	{
		const string cannotBeUsed = "Nationality of Means of Transport at departure cannot be used.";
		const string required = "Nationality of Means of Transport at departure is required.";
		testContext.EnableRule(r => r.IsRuleTR9095Active);

		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._5_PostalConsignment;
		departureMovement.BM_RN_NKTransportAtDepartureCountry = "NA";
		AssertHasMessageErrorContaining("Postal Consignment no Departure Country", departureMovement.BM_RN_NKTransportAtDepartureCountryInfo, cannotBeUsed);
		departureMovement.BM_RN_NKTransportAtDepartureCountry = "";
		AssertNoMessageErrorContaining("No Departure Country", departureMovement.BM_RN_NKTransportAtDepartureCountryInfo, cannotBeUsed);
		AssertNoMessageErrorContaining("Transport mode is set to 5, not Containerised and Departure Empty", departureMovement.BM_RN_NKTransportAtDepartureCountryInfo, required);
		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
		departureMovement.BM_RN_NKTransportAtDepartureCountry = "";
		AssertHasMessageErrorContaining("Transport mode is set to 4, not Containerised and Departure Empty", departureMovement.BM_RN_NKTransportAtDepartureCountryInfo, required);
		NCTSTestHelper.AddContainerAndSealsForTest(nctsHeader, "CONTAINER1", "SEAL1", "SEAL2", "", "");
		var goodsItem1 = nctsHeader.MovementHeader.GoodsItems.AddNew();
		NCTSTestHelper.SetContainerPivotForTest(goodsItem1, 0);
		departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureCountry();
		AssertNoMessageErrorContaining("Containerised", departureMovement.BM_RN_NKTransportAtDepartureCountryInfo, required);
	});

	public void TestCheckBM_RN_NKTransportAtDepartureCountry_ConditionTR9095_Inactive() => CombineAssertions(() =>
	{
		const string cannotBeUsed = "Nationality of Means of Transport at departure cannot be used.";
		const string required = "Nationality of Means of Transport at departure is required.";
		testContext.DisableRule(r => r.IsRuleTR9095Active);

		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._5_PostalConsignment;
		departureMovement.BM_RN_NKTransportAtDepartureCountry = "NA";
		AssertEquals("Postal Consignment no Departure Country", false, departureMovement.BM_RN_NKTransportAtDepartureCountryInfo.Notifications.Select(x => x.Message).Any(x => x.Contains(cannotBeUsed)));

		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
		departureMovement.BM_RN_NKTransportAtDepartureCountry = ZString.Empty;
		AssertEquals("Transport mode is set to 4, not Containerised and Departure Empty", false, departureMovement.BM_RN_NKTransportAtDepartureCountryInfo.Notifications.Select(x => x.Message).Any(x => x.Contains(required)));

		testContext.VerifyRule(r => r.IsRuleTR9095Active);
	});

	public void TestCheckBM_TOLCarrierCode_ConditionC010_Active() => CombineAssertions(() =>
	{
		const string ruleCode = "C010";
		testContext.EnableRule(r => r.IsRuleC010Active);

		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		departureMovement.Validation.ValidateBM_TOLCarrierCode();
		AssertHasMessageErrorContaining("Sea", departureMovement.BM_TOLCarrierCodeInfo, ruleCode);
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
		departureMovement.Validation.ValidateBM_TOLCarrierCode();
		AssertNoMessageErrorContaining("Rail", departureMovement.BM_TOLCarrierCodeInfo, ruleCode);
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
		departureMovement.Validation.ValidateBM_TOLCarrierCode();
		AssertHasMessageErrorContaining("Road", departureMovement.BM_TOLCarrierCodeInfo, ruleCode);
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
		departureMovement.Validation.ValidateBM_TOLCarrierCode();
		AssertHasMessageErrorContaining("Air", departureMovement.BM_TOLCarrierCodeInfo, ruleCode);
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._5_PostalConsignment;
		departureMovement.Validation.ValidateBM_TOLCarrierCode();
		AssertNoMessageErrorContaining("Post", departureMovement.BM_TOLCarrierCodeInfo, ruleCode);
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._7_FixedTransportInstallations;
		departureMovement.Validation.ValidateBM_TOLCarrierCode();
		AssertNoMessageErrorContaining("Fixed", departureMovement.BM_TOLCarrierCodeInfo, ruleCode);
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
		departureMovement.Validation.ValidateBM_TOLCarrierCode();
		AssertHasMessageErrorContaining("Inland Water", departureMovement.BM_TOLCarrierCodeInfo, ruleCode);
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
		departureMovement.Validation.ValidateBM_TOLCarrierCode();
		AssertHasMessageErrorContaining("Own Propulsion", departureMovement.BM_TOLCarrierCodeInfo, ruleCode);
		departureMovement.BM_TOLCarrierCode = Core.Constants.CountryCodes.Germany;
		AssertNoMessageErrorContaining("Carrier Code entered", departureMovement.BM_TOLCarrierCodeInfo, ruleCode);
	});

	[ExpectNoExceptions]
	public void TestCheckBM_TOLCarrierCode_ConditionC010_Inactive() => CombineAssertions(() =>
	{
		const string ruleCode = "C010";
		testContext.AssertNoNotifications(departureMovement.BM_TOLCarrierCodeInfo, ruleCode, r => r.IsRuleC010Active, false,
			() =>
			{
				departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
				departureMovement.Validation.ValidateBM_TOLCarrierCode();
			},
			() =>
			{
				departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
				departureMovement.Validation.ValidateBM_TOLCarrierCode();
			},
			() =>
			{
				departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
				departureMovement.Validation.ValidateBM_TOLCarrierCode();
			},
			() =>
			{
				departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
				departureMovement.Validation.ValidateBM_TOLCarrierCode();
			},
			() =>
			{
				departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
				departureMovement.Validation.ValidateBM_TOLCarrierCode();
			});
	});

	public void TestCheckBM_TOLCarrierID_ConditionC011_Active() => CombineAssertions(() =>
	{
		const string ruleCode = "C011";

		testContext.EnableRule(r => r.IsRuleC011Active);

		departureMovement.BM_TOLCarrierCode = Core.Constants.CountryCodes.UnitedKingdom;
		departureMovement.Validation.ValidateBM_TOLCarrierID();

		AssertHasMessageErrorContaining("Carrier ID empty", departureMovement.BM_TOLCarrierIDInfo, ruleCode);
		departureMovement.BM_TOLCarrierCode = ZString.Empty;
		departureMovement.Validation.ValidateBM_TOLCarrierID();
		AssertNoMessageErrorContaining("Carrier Code also empty", departureMovement.BM_TOLCarrierIDInfo, ruleCode);
		departureMovement.BM_BTAIndicator = SpecificCircumstanceIndicator.Codes.RailModeOfTransport;
		departureMovement.Validation.ValidateBM_TOLCarrierID();
		AssertHasMessageErrorContaining("Rail Transport", departureMovement.BM_TOLCarrierIDInfo, ruleCode);
		departureMovement.BM_TOLCarrierID = "ID";
		AssertNoMessageErrorContaining("Carrier ID entered", departureMovement.BM_TOLCarrierIDInfo, ruleCode);
	});

	[ExpectNoExceptions]
	public void TestCheckBM_TOLCarrierID_ConditionC011_Inactive() => CombineAssertions(() =>
	{
		const string ruleCode = "C011";

		testContext.AssertNoNotifications(departureMovement.BM_TOLCarrierIDInfo, ruleCode, r => r.IsRuleC011Active, false,
			() =>
			{
				departureMovement.BM_TOLCarrierCode = Core.Constants.CountryCodes.UnitedKingdom;
				departureMovement.Validation.ValidateBM_TOLCarrierID();
			},
			() =>
			{
				departureMovement.BM_TOLCarrierCode = ZString.Empty;
				departureMovement.BM_BTAIndicator = SpecificCircumstanceIndicator.Codes.RailModeOfTransport;
				departureMovement.Validation.ValidateBM_TOLCarrierID();
			});
	});

	public void TestCheckBM_ConveyanceNumber_ConditionC531_Active() => CombineAssertions(() =>
	{
		const string ruleCode = "C531";
		testContext.EnableRule(r => r.IsRuleC531Active);

		nctsHeader.BH_FTZMove = true;
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
		departureMovement.Validation.ValidateBM_ConveyanceNumber();
		AssertHasMessageErrorContaining("Empty Conveyance Number", departureMovement.BM_ConveyanceNumberInfo, MandatoryValidation.YouHaveNotEntered);
		departureMovement.BM_ConveyanceNumber = "A1A123F";
		AssertNoWarningContaining("Valid A1A123F", departureMovement.BM_ConveyanceNumberInfo, ruleCode);
		departureMovement.BM_ConveyanceNumber = "AAA1234Z";
		AssertNoWarningContaining("Valid AAA1234Z", departureMovement.BM_ConveyanceNumberInfo, ruleCode);
		departureMovement.BM_ConveyanceNumber = "1111234";
		AssertNoWarningContaining("Valid 1111234", departureMovement.BM_ConveyanceNumberInfo, ruleCode);
		departureMovement.BM_ConveyanceNumber = "$1111234";
		AssertHasWarningContaining("Invalid structure", departureMovement.BM_ConveyanceNumberInfo, ruleCode);
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		departureMovement.Validation.ValidateBM_ConveyanceNumber();
		AssertNoWarningContaining("Not air", departureMovement.BM_ConveyanceNumberInfo, ruleCode);
	});

	public void TestCheckBM_ConveyanceNumber_ConditionC531_Inactive() => CombineAssertions(() =>
	{
		const string ruleCode = "C531";
		testContext.DisableRule(r => r.IsRuleC531Active);

		nctsHeader.BH_FTZMove = true;
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
		departureMovement.Validation.ValidateBM_ConveyanceNumber();
		AssertEquals("Empty Conveyance Number", false, departureMovement.BM_ConveyanceNumberInfo.Notifications.Select(x => x.Message).Any(x => x.Contains(MandatoryValidation.YouHaveNotEntered)));

		departureMovement.BM_ConveyanceNumber = "$1111234";
		AssertEquals("Invalid structure", false, departureMovement.BM_ConveyanceNumberInfo.Notifications.Select(x => x.Message).Any(x => x.Contains(ruleCode)));
	});

	public void TestCheckMandatoryGoodsItem() => CombineAssertions(() =>
	{
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertHasMessageError("GoodsItems mandatory", departureMovement.BM_InBondEntryTypeInfo, "You need to supply at least one goods item.");

		departureMovement.GoodsItems.AddNew();
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("GoodsItems mandatory", departureMovement.BM_InBondEntryTypeInfo, "You need to supply at least one goods item.");
	});

	public void TestCheckBM_RL_NKDestinationPort_MustBeFilled() => CombineAssertions(() =>
	{
		const string message = "Destination Country must be filled either in Declaration tab or Goods tab.";
		departureMovement.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.China;
		AssertNoMessageError("Filled in Declaration tab", departureMovement.BM_RL_NKDestinationPortInfo, message);

		departureMovement.BM_RL_NKDestinationPort = "";
		AssertHasMessageError("Neither tab is filled", departureMovement.BM_RL_NKDestinationPortInfo, message);

		var goodItem = departureMovement.GoodsItems.AddNew();
		goodItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.China;
		departureMovement.Validation.ValidateBM_RL_NKDestinationPort();
		AssertNoMessageError("Filled in Goods tab", departureMovement.BM_RL_NKDestinationPortInfo, message);

		departureMovement.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Taiwan;
		AssertNoMessageError("Filled in both tabs", departureMovement.BM_RL_NKDestinationPortInfo, message);
	});

	public void TestCheckBM_SealType_ListValidation()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(departureMovement.BM_SealTypeInfo, "XX", SealTypeList.Codes.ContainerSeal);
	}

	public void TestCheckBM_SealType_MandatoryValidation() => CombineAssertions(() =>
	{
		departureMovement.BM_SealType = ZString.Empty;
		AssertNoMessageErrorContaining(departureMovement.BM_SealTypeInfo, MandatoryValidation.YouHaveNotEntered);

		departureMovement.BM_SealQty = 1;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(departureMovement.BM_SealTypeInfo);
	});

	public void TestCheckBM_SealQty() => CombineAssertions(() =>
	{
		departureMovement.BM_SealQty = -1;
		AssertHasErrorContaining(departureMovement.BM_SealQtyInfo, MandatoryValidation.ValueCannotBeNegative);
		departureMovement.BM_SealQty = 0;
		AssertNoErrorContaining(departureMovement.BM_SealQtyInfo, MandatoryValidation.ValueCannotBeNegative);
	});

	public void TestCheckBM_SealQty_Package_EqZero_SealEntered() => CombineAssertions(() =>
	{
		departureMovement.BM_SealType = SealTypeList.Codes.PackageSeal;
		departureMovement.BM_SealQty = 0;
		var packageSeal = departureMovement.Header.Seals.AddNew();

		departureMovement.Validation.ValidateBM_SealQty();
		AssertNoMessageError(departureMovement.BM_SealQtyInfo, "The maximum number of seals entered in the package's grid should be 0.");
		packageSeal.CY_Data = "SEAL1";
		departureMovement.Validation.ValidateBM_SealQty();
		AssertHasMessageError(departureMovement.BM_SealQtyInfo, "The maximum number of seals entered in the package's grid should be 0.");
	});

	public void TestCheckBM_SealQty_Package_NoSealEntered() => CombineAssertions(() =>
	{
		departureMovement.BM_SealQty = 1;
		departureMovement.BM_SealType = SealTypeList.Codes.PackageSeal;

		// at least one seal has to be entered, whenever Qty > 0
		AssertHasMessageError(departureMovement.BM_SealQtyInfo, "You have not entered a Seal Number.");
		var packageSeal1 = departureMovement.Header.Seals.AddNew();

		AssertHasMessageError(departureMovement.BM_SealQtyInfo, "You have not entered a Seal Number.");
		packageSeal1.CY_Data = "Seal 1";
		AssertNoErrorContaining(departureMovement.BM_SealQtyInfo, "You have not entered a Seal Number.");
	});

	public void TestCheckBM_SealQty_Package() => CombineAssertions(() =>
	{
		departureMovement.BM_SealQty = 2;
		departureMovement.BM_SealType = SealTypeList.Codes.PackageSeal;

		var packageSeal1 = departureMovement.Header.Seals.AddNew();
		packageSeal1.CY_Data = "Seal 1";
		var packageSeal2 = departureMovement.Header.Seals.AddNew();
		packageSeal2.CY_Data = ZString.Empty;
		AssertNoMessageError(departureMovement.BM_SealQtyInfo, "The maximum number of seals entered in the package's grid should be 2.");
		packageSeal2.CY_Data = "Seal 2";
		departureMovement.Validation.ValidateBM_SealQty();
		AssertNoMessageError(departureMovement.BM_SealQtyInfo, "The maximum number of seals entered in the package's grid should be 2.");
		var packageSeal3 = departureMovement.Header.Seals.AddNew();
		packageSeal3.CY_Data = "Seal 3";
		departureMovement.Validation.ValidateBM_SealQty();
		AssertHasMessageError(departureMovement.BM_SealQtyInfo, "The maximum number of seals entered in the package's grid should be 2.");
	});

	public void TestCheckBM_SealQty_Container_EqZero_SealEntered() => CombineAssertions(() =>
	{
		departureMovement.BM_SealType = SealTypeList.Codes.ContainerSeal;
		departureMovement.BM_SealQty = 0;

		AssertNoMessageError(departureMovement.BM_SealQtyInfo, "The maximum number of seal numbers entered in the container's grid should be 0.");
		var containerSeal1 = departureMovement.Header.DepartureHeaderContainers.AddNew();
		containerSeal1.Seal1 = "Seal 1";
		departureMovement.Validation.ValidateBM_SealQty();
		AssertHasMessageError(departureMovement.BM_SealQtyInfo, "The maximum number of seal numbers entered in the container's grid should be 0.");
	});

	public void TestCheckBM_SealQty_Container_NoSealEntered() => CombineAssertions(() =>
	{
		departureMovement.BM_SealQty = 1;
		departureMovement.BM_SealType = SealTypeList.Codes.ContainerSeal;

		// at least one seal has to be entered, whenever Qty > 0
		AssertHasMessageError(departureMovement.BM_SealQtyInfo, "You have not entered a Seal Number.");
		var containerSeal1 = departureMovement.Header.DepartureHeaderContainers.AddNew();

		AssertHasMessageError(departureMovement.BM_SealQtyInfo, "You have not entered a Seal Number.");
		containerSeal1.Seal1 = "Seal 1";
		AssertNoErrorContaining(departureMovement.BM_SealQtyInfo, "You have not entered a Seal Number.");
	});

	public void TestCheckBM_SealQty_Container() => CombineAssertions(() =>
	{
		departureMovement.BM_SealQty = 2;
		departureMovement.BM_SealType = ZString.Empty;
		var container1 = departureMovement.Header.DepartureHeaderContainers.AddNew();
		container1.Seal1 = "Seal 1.1";
		departureMovement.Validation.ValidateBM_SealQty();
		AssertNoMessageError(departureMovement.BM_SealQtyInfo, "The maximum number of seal numbers entered in the container's grid should be 2.");
		container1.Seal2 = "Seal 1.2";
		departureMovement.Validation.ValidateBM_SealQty();
		AssertNoMessageError(departureMovement.BM_SealQtyInfo, "The maximum number of seal numbers entered in the container's grid should be 2.");
		var container2 = departureMovement.Header.DepartureHeaderContainers.AddNew();
		container2.Seal1 = "Seal 2.1";
		departureMovement.Validation.ValidateBM_SealQty();
		AssertHasMessageError(departureMovement.BM_SealQtyInfo, "The maximum number of seal numbers entered in the container's grid should be 2.");
		departureMovement.BM_SealQty = 3;
		departureMovement.Validation.ValidateBM_SealQty();
		AssertNoMessageError(departureMovement.BM_SealQtyInfo, "The maximum number of seal numbers entered in the container's grid should be 3.");
		container2.Seal2 = "Seal 2.2";
		departureMovement.Validation.ValidateBM_SealQty();
		AssertHasMessageError(departureMovement.BM_SealQtyInfo, "The maximum number of seal numbers entered in the container's grid should be 3.");
	});

	public void TestCheckBM_SealQty_SealType_Negative() => CombineAssertions(() =>
	{
		departureMovement.BM_SealQty = -2;
		departureMovement.BM_SealType = SealTypeList.Codes.PackageSeal;
		departureMovement.Validation.ValidateBM_SealQty();
		AssertEquals("PackageSeal", false, departureMovement.BM_SealQtyInfo.HasMessageError("The maximum number of seals entered in the package's grid should be -2."));

		departureMovement.BM_SealType = ZString.Empty;
		departureMovement.Validation.ValidateBM_SealQty();
		AssertEquals("ContainerSeal", false, departureMovement.BM_SealQtyInfo.HasMessageError("The maximum number of seal numbers entered in the container's grid should be -2."));
	});

	public void TestCheckBM_InBondEntryType_ConditionC900() => CombineAssertions(() =>
	{
		const string expectedError = "A TIR declaration requires a guarantee of type B (TIR). (C900)";
		departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
		AssertHasMessageError("No Bond Type", departureMovement.BM_InBondEntryTypeInfo, expectedError);
		var guarantee = nctsHeader.Guarantees.AddNew();
		guarantee.PW_BondType = EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.MovementsCarriedUnderTheTirConvention;
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError("Bond Type B", departureMovement.BM_InBondEntryTypeInfo, expectedError);
	});

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		departureMovement = nctsHeader.MovementHeader;
		testContext = departureMovement.CreateDeparturePhase4ValidationTestContext();
	}

	protected override void TearDown()
	{
		base.TearDown();
		testContext?.Dispose();
	}

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader departureMovement;
	MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase4ValidationDecider> testContext;
}
