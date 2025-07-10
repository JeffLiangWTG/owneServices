using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class NctsDepartureMovementHeaderValidationTest : NctsCommonMovementHeaderValidationAbstractTest<INctsDepartureMovementHeaderPhase5ValidationDecider>
{
	public void TestCheckBM_RN_NKTOLCarrierNationality()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(departureMovement.BM_RN_NKTOLCarrierNationalityInfo, "11", Constants.CountryCodes.Germany);
	}

	public void TestCheckBM_InBondEntryType_Mandatory()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(departureMovement.BM_InBondEntryTypeInfo);
	}

	public void TestCheckFromWarehouseOrgPK()
	{
		var header1 = Factory.NewWithValidTestData<OrgHeader>();
		header1.OH_Code = "WH1";
		header1.OH_IsWarehouseClient = true;
		header1.OrganisationTypes = OrganisationTypes.WarehouseClient;
		var address1 = header1.Addresses.AddNew();
		address1.OA_Address1 = "ADD1";
		address1.AddAddressType(OrgAddressType.Office);

		var header2 = Factory.NewWithValidTestData<OrgHeader>();
		header2.OH_Code = "WH2";
		header2.OrganisationTypes = OrganisationTypes.Consignor;
		var address2 = header2.Addresses.AddNew();
		address2.OA_Address1 = "ADD2";
		address2.AddAddressType(OrgAddressType.Office);

		Factory.Save();

		departureMovement.BM_OA_WarehouseAddress = address1.PK;
		ValidationTestHelper.AssertErrorIfInvalidPK(departureMovement.FromWarehouseOrgPKInfo, address2.Header.PK, address1.Header.PK);
	}

	public void TestCheckBM_OA_WarehouseAddress()
	{
		departureMovement.FromWarehouseOrgPK = ZGuid.NewZGuid();
		departureMovement.BM_OA_WarehouseAddress = ZGuid.Empty;
		departureMovement.Validation.ValidateBM_OA_WarehouseAddress();

		CombineAssertions(() =>
		{
			AssertHasWarning(departureMovement.BM_OA_WarehouseAddressInfo, "Warehouse will not be saved because no address is selected.");

			departureMovement.BM_OA_WarehouseAddress = ZGuid.NewZGuid();
			AssertNoWarning(departureMovement.BM_OA_WarehouseAddressInfo, "Warehouse will not be saved because no address is selected.");

			departureMovement.FromWarehouseOrgPK = ZGuid.Empty;
			departureMovement.BM_OA_WarehouseAddress = ZGuid.Empty;
			AssertNoWarning(departureMovement.BM_OA_WarehouseAddressInfo, "Warehouse will not be saved because no address is selected.");
		});
	}

	public void TestCheckBM_InBondEntryType_CheckCustomsOffices()
	{
		CombineAssertions(() =>
		{
			departureMovement.RunPreSaveValidation();
			AssertHasMessageError("DEP is mandatory", departureMovement.BM_InBondEntryTypeInfo, "The declaration requires an office of type NCTS Office of departure with purpose DEP.");
			AssertHasMessageError("DES is mandatory", departureMovement.BM_InBondEntryTypeInfo, "The declaration requires an office of type NCTS Office of destination with purpose DES.");
			AssertNoMessageError("TRA is optional", departureMovement.BM_InBondEntryTypeInfo, "The declaration requires an office of type NCTS Office of transit with purpose TRA.");
			departureMovement.SetMixedConsignment();
			AssertHasMessageError("TRA is mandatory now", departureMovement.BM_InBondEntryTypeInfo, "The declaration requires an office of type NCTS Office of transit with purpose TRA.");
		});
	}

	public void TestCheckMandatoryGuarantee()
	{
		const string errorMessage = "You need to supply a valid guarantee.";

		var departureMovement = (NctsDepartureMovementHeader)GetMovementHeaderForTest();

		CombineAssertions("When ShouldCheckGuarantees", () =>
		{
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertHasMessageErrorContaining("No Guarantees", departureMovement.BM_InBondEntryTypeInfo, errorMessage);
			var guarantee = nctsHeader.Guarantees.AddNew();
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageError("One Guarantee", departureMovement.BM_InBondEntryTypeInfo, errorMessage);
		});
	}

	public void TestCheckBM_InBondEntryType_ConditionC035_Active() => CombineAssertions(() =>
	{
		testContext.EnableRule(r => r.IsRuleC035Active);

		NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "NO123456", ZDateTime.Empty);
		departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.TirDeclaration;
		AssertNoMessageErrorContaining("Tir type", departureMovement.BM_InBondEntryTypeInfo, "C035");
		departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
		AssertHasMessageErrorContaining("T2 type", departureMovement.BM_InBondEntryTypeInfo, "C035");
		nctsHeader.MovementHeader.GoodsItems.AddNew().PreviousDocuments.AddNew();
		departureMovement.SetMixedConsignment();
		AssertNoMessageErrorContaining("T- type", departureMovement.BM_InBondEntryTypeInfo, "C035");
	});

	[ExpectNoExceptions]
	public void TestCheckBM_InBondEntryType_ConditionC035_Inactive() => CombineAssertions(() =>
	{
		testContext.AssertNoNotifications(departureMovement.BM_InBondEntryTypeInfo, "C035", r => r.IsRuleC035Active, false, () =>
		{
			NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "NO123456", ZDateTime.Empty);
			departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
		});
	});

	public void TestCheckConditionC547_Active() => CombineAssertions(() =>
	{
		const string message = "Transport document must be present";
		testContext.EnableRule(r => r.IsRuleC547Active);

		var movementHeaderForTest = (NctsDepartureMovementHeader)GetMovementHeaderForTest();
		movementHeaderForTest.Header.BH_FTZMove = true;
		var goodsItem = movementHeaderForTest.GoodsItems.AddNew();
		movementHeaderForTest.Validation.ValidateBM_InBondEntryType();

		AssertHasMessageErrorContaining("No transport document", movementHeaderForTest.BM_InBondEntryTypeInfo, message);

		movementHeaderForTest.BM_BTAIndicator = SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators;
		movementHeaderForTest.Validation.ValidateBM_InBondEntryType();
		AssertHasMessageErrorContaining("No transport document, BTAIndicator Not A", movementHeaderForTest.BM_InBondEntryTypeInfo, message);

		movementHeaderForTest.BM_BTAIndicator = SpecificCircumstanceIndicator.Codes.PostalAndExpressConsignments;
		movementHeaderForTest.Validation.ValidateBM_InBondEntryType();

		AssertNoMessageErrorContaining("No transport document, BTAIndicator A", movementHeaderForTest.BM_InBondEntryTypeInfo, message);

		movementHeaderForTest.BM_BTAIndicator = ZString.Empty;
		goodsItem.SupportingDocuments.AddNew();
		AssertNoMessageErrorContaining("No transport document", movementHeaderForTest.BM_InBondEntryTypeInfo, message);
	});

	public void TestCheckConditionC547_Inactive() => CombineAssertions(() =>
	{
		const string message = "Transport document must be present";
		testContext.DisableRule(r => r.IsRuleC547Active);

		var movementHeaderForTest = (NctsDepartureMovementHeader)GetMovementHeaderForTest();
		movementHeaderForTest.Header.BH_FTZMove = true;
		movementHeaderForTest.GoodsItems.AddNew();
		movementHeaderForTest.Validation.ValidateBM_InBondEntryType();

		AssertEquals("No transport document", false, movementHeaderForTest.BM_InBondEntryTypeInfo.Notifications.Select(x => x.Message).Any(x => x.Contains(message)));

		testContext.VerifyRule(r => r.IsRuleC547Active);
	});

	public void TestCheckBM_InBondEntryType_ConditionC904()
	{
		const string expectedMessageError = "Principal Trader must have a TIR Carnet reference. Open the Trader for editing, choose 'config', 'registration numbers' and enter a registration code of type 'TIR'(C904)";
		CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.TirDeclaration;
			AssertNoMessageError("Tir type", departureMovement.BM_InBondEntryTypeInfo, expectedMessageError);

			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", nctsHeader.Principal, "", "OSCORP", "F", "U", "C", "GBKK", "GB", "TIN12345");
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageError("No TIR for Principal", departureMovement.BM_InBondEntryTypeInfo, expectedMessageError);

			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", nctsHeader.Principal, "", "OSCORP", "F", "U", "C", "GBKK", "GB", "TIN12345");
			departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.TirDeclaration;
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageError("TIR for Principal", departureMovement.BM_InBondEntryTypeInfo, expectedMessageError);
		});

		CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.TirDeclaration;
			AssertHasMessageError("Tir type", departureMovement.BM_InBondEntryTypeInfo, expectedMessageError);

			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", nctsHeader.Principal, "", "OSCORP", "F", "U", "C", "GBKK", "GB", "TIN12345");
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertHasMessageError("No TIR for Principal", departureMovement.BM_InBondEntryTypeInfo, expectedMessageError);

			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", nctsHeader.Principal, "", "OSCORP", "F", "U", "C", "GBKK", "GB", "TIN12345", "TIR12345");
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageError("TIR for Principal", departureMovement.BM_InBondEntryTypeInfo, expectedMessageError);
		});
	}

	public void TestCheckBM_InBondEntryType_ConditionR020()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
			AssertHasMessageErrorContaining("T2 Type", departureMovement.BM_InBondEntryTypeInfo, "R020");
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			var previousDocument1 = goodsItem.PreviousDocuments.AddNew();
			previousDocument1.CSI_Code = "T1";
			previousDocument1.CSI_ReferenceNumber = "NOT VALID";
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertHasMessageErrorContaining("Previous Document T1", departureMovement.BM_InBondEntryTypeInfo, "R020");
			var previousDocument2 = goodsItem.PreviousDocuments.AddNew();
			previousDocument2.CSI_Code = "T2";
			previousDocument2.CSI_ReferenceNumber = "VALID";
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageErrorContaining("Previous Document T2", departureMovement.BM_InBondEntryTypeInfo, "R020");
		});
	}

	public void TestCheckBM_InBondEntryType_ConditionR902_Active() => CombineAssertions(() =>
	{
		testContext.EnableRule(r => r.IsRuleR902Active);
		var transitOffice = NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "TRANSIT", ZDateTime.Empty);
		departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.TirDeclaration;
		AssertHasMessageErrorContaining("Transit Office and Tir Type", departureMovement.BM_InBondEntryTypeInfo, "R902");
		transitOffice.Delete();
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageErrorContaining("No Transit Office", departureMovement.BM_InBondEntryTypeInfo, "R902");
	});

	[ExpectNoExceptions]
	public void TestCheckBM_InBondEntryType_ConditionR902_Inactive()
	{
		testContext.AssertNoNotifications(departureMovement.BM_InBondEntryTypeInfo, "R902", r => r.IsRuleR902Active, false, () =>
		{
			NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "TRANSIT", ZDateTime.Empty);
			departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.TirDeclaration;
		});
	}

	public void TestCheckBM_InBondEntryType_ConditionR903_Active() => CombineAssertions(() =>
	{
		testContext.EnableRule(r => r.IsRuleR903Active);
		nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = true;
		departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.TirDeclaration;
		AssertHasMessageErrorContaining("Tir and Simplified", departureMovement.BM_InBondEntryTypeInfo, "R903");
		nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = false;
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageErrorContaining("Not simplified", departureMovement.BM_InBondEntryTypeInfo, "R903");
	});

	[ExpectNoExceptions]
	public void TestCheckBM_InBondEntryType_ConditionR903_Inactive()
	{
		testContext.AssertNoNotifications(departureMovement.BM_InBondEntryTypeInfo, "R903", r => r.IsRuleR903Active, false, () =>
		{
			nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = true;
			departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.TirDeclaration;
		});
	}

	public void TestCheckBM_InBondEntryType_ConditionR0909_ItalyToSanMarino() => CombineAssertions(() =>
	{
		const string errorText = "R909";
		testContext.EnableRule(x => x.IsRuleR0909Active);
		NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "IT123456", ZDateTime.Empty);
		NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "SM123456", ZDateTime.Empty);
		departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
		AssertHasMessageErrorContaining("Not T2SM type", departureMovement.BM_InBondEntryTypeInfo, errorText);

		testContext.DisableRule(x => x.IsRuleR0909Active);
		departureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageErrorContaining("Not T2SM type, but Rule R0909 is inactive", departureMovement.BM_InBondEntryTypeInfo, errorText);

		testContext.EnableRule(x => x.IsRuleR0909Active);
		departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.T2PlusSanMarino;
		AssertNoMessageErrorContaining("Is T2SM type", departureMovement.BM_InBondEntryTypeInfo, errorText);
	});

	[TestDate(2022, 07, 01)]
	public void TestCheckBM_InBondEntryType_ConditionR0909_EuropeToSanMarino()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, "CL010 Desc.");
		helper.CreateCusCodeList("EUN", EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL148, "AT", "Austria", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 12, 31));
		helper.CreateCusCodeList("EUN", EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, "CY", "Cyprus", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 12, 31));
		Factory.Save();

		var departureOffice = NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "CY123456", ZDateTime.Empty, true);
		NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "SM123456", ZDateTime.Empty);

		const string errorText = "R909";

		CombineAssertions(() =>
		{
			testContext.EnableRule(x => x.IsRuleR0909Active);
			departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.TirDeclaration;
			AssertHasMessageErrorContaining("Departure office in CL010 country, not T or T2 type", departureMovement.BM_InBondEntryTypeInfo, errorText);

			testContext.DisableRule(x => x.IsRuleR0909Active);
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageErrorContaining("Departure office in CL010 country, not T or T2 type. But Rule R0909 is inactive", departureMovement.BM_InBondEntryTypeInfo, errorText);

			testContext.EnableRule(x => x.IsRuleR0909Active);
			departureOffice.CY_Data = "AT123456";
			AssertNoMessageErrorContaining("Departure office not in CL010 country", departureMovement.BM_InBondEntryTypeInfo, errorText);

			departureOffice.CY_Data = "CY123456";
			departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedureBetweenDifferentFiscalTerritories;
			AssertNoMessageErrorContaining("Departure office in CL010 country, T2 type", departureMovement.BM_InBondEntryTypeInfo, errorText);

			departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5;
			AssertNoMessageErrorContaining("Departure office in CL010 country, Mixed consignment", departureMovement.BM_InBondEntryTypeInfo, errorText);
		});
	}

	public void TestCheckBM_InBondEntryType_ConditionR911()
	{
		testContext.EnableRule(c => c.IsRuleR911Active);
		NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "SM123456", ZDateTime.Empty);
		NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "FR123456", ZDateTime.Empty);
		departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
		AssertHasMessageErrorContaining("T1 type When Rule R0911 Active", departureMovement.BM_InBondEntryTypeInfo, "R911");
		departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
		AssertNoMessageErrorContaining("T2 type When Rule R0911 Active", departureMovement.BM_InBondEntryTypeInfo, "R911");

		testContext.DisableRule(c => c.IsRuleR911Active);
		departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
		AssertNoMessageErrorContaining("T1 type When Rule R0911 Disabled", departureMovement.BM_InBondEntryTypeInfo, "R911");
	}

	public void TestCheckBM_RL_NKForeignDestPort()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(departureMovement.BM_RL_NKForeignDestPortInfo, "X~123", "GBLON");
	}

	public void TestCheckBM_RL_NKForeignDestPort_CheckConditionC191_Active() => CombineAssertions(() =>
	{
		testContext.EnableRule(r => r.IsRuleC191Active);

		nctsHeader.BH_FTZMove = true;
		departureMovement.Validation.ValidateBM_RL_NKForeignDestPort();
		AssertHasMessageErrorContaining("Is FTZ Empty Desitnation", departureMovement.BM_RL_NKForeignDestPortInfo, "C191");
		departureMovement.BM_RL_NKForeignDestPort = "GBLON";
		AssertNoMessageErrorContaining("Is FTZ and Value", departureMovement.BM_RL_NKForeignDestPortInfo, "C191");
		nctsHeader.BH_FTZMove = false;
		departureMovement.BM_RL_NKForeignDestPort = ZString.Empty;
		AssertNoMessageErrorContaining("Not FTZ", departureMovement.BM_RL_NKForeignDestPortInfo, "C191");
	});

	[ExpectNoExceptions]
	public void TestCheckBM_RL_NKForeignDestPort_CheckConditionC191_Inactive() => CombineAssertions(() =>
	{
		testContext.AssertNoNotifications(departureMovement.BM_RL_NKForeignDestPortInfo, "C191", r => r.IsRuleC191Active, false, () =>
		{
			nctsHeader.BH_FTZMove = true;
			departureMovement.Validation.ValidateBM_RL_NKForeignDestPort();
		});
	});

	public void TestCheckBM_InlandTransportMode_List()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(departureMovement.BM_InlandTransportModeInfo, "A", ModeOfTransportList.Codes._1_SeaTransport);
	}

	public void TestCheckBM_ExportTransportMode_List()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(departureMovement.BM_ExportTransportModeInfo, "10", ModeOfTransportList.Codes._3_RoadTransport);
	}

	public void TestCheckBM_ExportTransportMode_ConditionC599_Active() => CombineAssertions(() =>
	{
		testContext.EnableRule(r => r.IsRuleC599Active);

		nctsHeader.BH_FTZMove = false;
		departureMovement.Validation.ValidateBM_ExportTransportMode();
		AssertNoMessageErrorContaining("Not FTZ", departureMovement.BM_ExportTransportModeInfo, "C599");
		nctsHeader.BH_FTZMove = true;
		departureMovement.Validation.ValidateBM_ExportTransportMode();
		AssertHasMessageErrorContaining("Is FTZ and non EU", departureMovement.BM_ExportTransportModeInfo, "C599");
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		AssertNoMessageErrorContaining("Transport not empty", departureMovement.BM_ExportTransportModeInfo, "C599");
	});

	[ExpectNoExceptions]
	public void TestCheckBM_ExportTransportMode_ConditionC599_Inactive() => CombineAssertions(() =>
	{
		testContext.AssertNoNotifications(departureMovement.BM_ExportTransportModeInfo, "C599", r => r.IsRuleC599Active, false, () =>
		{
			nctsHeader.BH_FTZMove = true;
			departureMovement.Validation.ValidateBM_ExportTransportMode();
		});
	});

	public void TestCheckBM_RN_NKTransportAtDepartureCountry_List()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NCNAT);
		Factory.Save();

		ValidationTestHelper.AssertInvalidCodeMessageError(departureMovement.BM_RN_NKTransportAtDepartureCountryInfo, "Z~", Constants.CountryCodes.France);
	}

	public void TestCheckBM_TOLCarrierCode_List()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NCNAT);
		Factory.Save();

		ValidationTestHelper.AssertInvalidCodeMessageError(departureMovement.BM_TOLCarrierCodeInfo, "Z!", Constants.CountryCodes.France);
	}

	public void TestCheckBM_BTAIndicator_List()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(departureMovement.BM_BTAIndicatorInfo, "Z", SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators);
	}

	public void TestCheckBM_BTAIndicator_ConditionC587()
	{
		CombineAssertions(() =>
		{
			nctsHeader.BH_FTZMove = true;
			departureMovement.BM_BTAIndicator = SpecificCircumstanceIndicator.Codes.ShipAndAircraftSupplies;
			AssertNoMessageErrorContaining("Indicator Is Supplies", departureMovement.BM_BTAIndicatorInfo, "C587");
			departureMovement.BM_BTAIndicator = SpecificCircumstanceIndicator.Codes.RailModeOfTransport;
			AssertHasMessageErrorContaining("Indicator not Supplies", departureMovement.BM_BTAIndicatorInfo, "C587");
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departureMovement.Validation.ValidateBM_BTAIndicator();
			AssertNoMessageErrorContaining("When Phase5, no validation is executed.", departureMovement.BM_BTAIndicatorInfo, "C587");
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var itinerary = nctsHeader.Itinerary.AddNew();
			itinerary.CountryCode = Constants.CountryCodes.Norway;
			departureMovement.Validation.ValidateBM_BTAIndicator();
			AssertNoMessageErrorContaining("Has itinerary", departureMovement.BM_BTAIndicatorInfo, "C587");
		});
	}

	public void TestCheckBM_MethodOfPayment()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(departureMovement.BM_MethodOfPaymentInfo, "X", TransportChargesModeOfPayment.Codes.AccountHolderWithCarrier);
	}

	public void TestCheckBM_GS_NKCusAgent_List()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(departureMovement.BM_GS_NKCusAgentInfo, "X~", GlbStaff.CurrentUser.GS_Code);
	}

	public void TestCheckBM_ExportDate()
	{
		CombineAssertions(() =>
		{
			departureMovement.IsSimplifiedNctsProcedure = true;
			departureMovement.BM_ExportDate = ZDateTime.Now;
			AssertNoMessageErrorContaining("Has Export Date", departureMovement.BM_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);
			departureMovement.BM_ExportDate = ZDate.Empty;
			AssertHasMessageErrorContaining("Simplified Ncts procedure", departureMovement.BM_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);
			departureMovement.IsSimplifiedNctsProcedure = false;
			departureMovement.Validation.ValidateBM_ExportDate();
			AssertNoMessageErrorContaining("Not simplified", departureMovement.BM_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckBM_PlaceOfUnloading_RuleC589_Active() => CombineAssertions(() =>
	{
		const string errorMessage = "Place of Unloading is required for a Safety and Security movement.";
		testContext.EnableRule(r => r.IsRuleC589Active);

		departureMovement.BM_BTAIndicator = SpecificCircumstanceIndicator.Codes.PostalAndExpressConsignments;
		nctsHeader.BH_FTZMove = true;
		departureMovement.Validation.ValidateBM_PlaceOfUnloading();
		AssertHasMessageErrorContaining("Postal Security Movement Empty Unloading", departureMovement.BM_PlaceOfUnloadingInfo, errorMessage);
		nctsHeader.PlaceOfUnloadingCode = "GBLHR";
		AssertNoMessageErrorContaining("Postal Security Movement Entered Unloading", departureMovement.BM_PlaceOfUnloadingInfo, errorMessage);
		departureMovement.BM_BTAIndicator = SpecificCircumstanceIndicator.Codes.RailModeOfTransport;
		nctsHeader.PlaceOfUnloadingCode = ZString.Empty;
		AssertHasMessageErrorContaining("Rail Security Movement Empty Unloading", departureMovement.BM_PlaceOfUnloadingInfo, errorMessage);
		nctsHeader.BH_FTZMove = false;
		departureMovement.Validation.ValidateBM_PlaceOfUnloading();
		AssertNoMessageErrorContaining("Not Security Movement", departureMovement.BM_PlaceOfUnloadingInfo, errorMessage);
		nctsHeader.BH_FTZMove = true;
		departureMovement.BM_BTAIndicator = SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators;
		departureMovement.Validation.ValidateBM_PlaceOfUnloading();
		AssertNoMessageErrorContaining("Authorized Operator Security Movement Empty Unloading", departureMovement.BM_PlaceOfUnloadingInfo, errorMessage);
	});

	public void TestCheckBM_PlaceOfUnloading_RuleC589_Inactive() => CombineAssertions(() =>
	{
		const string errorMessage = "Place of Unloading is required for a Safety and Security movement.";
		testContext.DisableRule(r => r.IsRuleC589Active);

		departureMovement.BM_BTAIndicator = SpecificCircumstanceIndicator.Codes.PostalAndExpressConsignments;
		nctsHeader.BH_FTZMove = true;
		departureMovement.Validation.ValidateBM_PlaceOfUnloading();
		AssertEquals("Postal Security Movement Empty Unloading", false, departureMovement.BM_PlaceOfUnloadingInfo.Notifications.Select(x => x.Message).Any(x => x.Contains(errorMessage)));

		departureMovement.BM_BTAIndicator = SpecificCircumstanceIndicator.Codes.RailModeOfTransport;
		nctsHeader.PlaceOfUnloadingCode = ZString.Empty;
		AssertEquals("Rail Security Movement Empty Unloading", false, departureMovement.BM_PlaceOfUnloadingInfo.Notifications.Select(x => x.Message).Any(x => x.Contains(errorMessage)));

		testContext.VerifyRule(r => r.IsRuleC589Active);
	});

	public void TestCheckBM_RL_NKDestinationPort_List()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
		Factory.Save();

		ValidationTestHelper.AssertInvalidCodeMessageError(departureMovement.BM_RL_NKDestinationPortInfo, "X~", Constants.CountryCodes.Germany);
	}

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
			departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
			departureMovement.BM_RL_NKDestinationPort = "XX";
			AssertHasMessageErrorContaining("XX is not in List 9", departureMovement.BM_RL_NKDestinationPortInfo, errorMessage);

			departureMovement.BM_RL_NKDestinationPort = Constants.CountryCodes.Germany;
			AssertNoMessageErrorContaining("DE is in List 9", departureMovement.BM_RL_NKDestinationPortInfo, errorMessage);

			departureMovement.BM_RL_NKDestinationPort = "";
			AssertNoMessageErrorContaining("Empty and not read only", departureMovement.BM_RL_NKDestinationPortInfo, errorMessage);

			goodItem.BY_RN_NKCountryOfDestination = Constants.CountryCodes.Germany;
			departureMovement.Validation.ValidateBM_RL_NKDestinationPort();
			AssertNoMessageErrorContaining("Validate it only if not read only", departureMovement.BM_RL_NKDestinationPortInfo, errorMessage);
		});
	}

	public void TestCheckBM_RL_NKDestinationPort_ListC0009_GB()
	{
		const string errorMessage = "For T2 Declarations [15A] and/or [17A] must be a country from Code List 9 (Countries if European Union + NCTS Contracting Parties).";

		var company = Factory.NewWithValidTestData<GlbCompany>();
		company.SetCountry("GB");
		var xxxBranch = Factory.New<GlbBranch>();
		xxxBranch.GB_RL_NKHomePort = "GBNRW";
		xxxBranch.GB_Code = "XXX";
		xxxBranch.GB_GC = company.PK;
		Factory.Save();

		using (DisposableEnvironment.ForBranch(xxxBranch.PK.ToGuid()))
		{
			var gbNctsHeader = Factory.New<NctsHeader>();
			gbNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			gbNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var gbMovement = gbNctsHeader.MovementHeader;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);
			Factory.Save();

			CombineAssertions(() =>
			{
				gbMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
				gbMovement.BM_RL_NKDestinationPort = "XX";
				AssertHasMessageErrorContaining("Not List 9", gbMovement.BM_RL_NKDestinationPortInfo, errorMessage);

				gbMovement.BM_RL_NKDestinationPort = Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes;
				AssertNoMessageErrorContaining("In List 9", gbMovement.BM_RL_NKDestinationPortInfo, errorMessage);
			});
		}
	}

	public void TestCheckBM_RL_NKDestinationPort_MustBeFilled()
	{
		CombineAssertions(() =>
		{
			const string message = "Destination Country must be filled either in Declaration tab or Goods tab but not both.";
			departureMovement.BM_RL_NKDestinationPort = Constants.CountryCodes.China;
			AssertNoMessageError("Only filled in Declaration tab", departureMovement.BM_RL_NKDestinationPortInfo, message);

			departureMovement.BM_RL_NKDestinationPort = "";
			AssertNoMessageError("Neither tab is filled", departureMovement.BM_RL_NKDestinationPortInfo, message);

			var goodItem = departureMovement.GoodsItems.AddNew();
			goodItem.BY_RN_NKCountryOfDestination = Constants.CountryCodes.China;
			departureMovement.Validation.ValidateBM_RL_NKDestinationPort();
			AssertNoMessageError("Only filled in Goods tab", departureMovement.BM_RL_NKDestinationPortInfo, message);

			departureMovement.BM_RL_NKDestinationPort = Constants.CountryCodes.Taiwan;
			AssertHasMessageError("Filled in both tabs", departureMovement.BM_RL_NKDestinationPortInfo, message);
		});
	}

	public void TestCheckTirCarnetNumber()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
			departureMovement.Validation.ValidateTirCarnetNumber();
			AssertHasMessageErrorContaining("TIR with No Carnet", departureMovement.TirCarnetNumberInfo, MandatoryValidation.YouHaveNotEntered);
			departureMovement.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
			departureMovement.Validation.ValidateTirCarnetNumber();
			AssertNoMessageErrorContaining("Not TIR", departureMovement.TirCarnetNumberInfo, MandatoryValidation.YouHaveNotEntered);
			departureMovement.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
			departureMovement.TirCarnetNumber = "TESTNUM";
			AssertNoMessageErrorContaining("TIR with Carnet", departureMovement.TirCarnetNumberInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckTirCarnetExpiryDate()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
			departureMovement.Validation.ValidateTirCarnetExpiryDate();
			AssertHasMessageErrorContaining("TIR with No Expiry", departureMovement.TirCarnetExpiryDateInfo, MandatoryValidation.YouHaveNotEntered);
			departureMovement.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
			departureMovement.Validation.ValidateTirCarnetExpiryDate();
			AssertNoMessageErrorContaining("Not TIR", departureMovement.TirCarnetExpiryDateInfo, MandatoryValidation.YouHaveNotEntered);
			departureMovement.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
			departureMovement.TirCarnetExpiryDate = ZDateTime.Now;
			AssertNoMessageErrorContaining("TIR with Expiry Date", departureMovement.TirCarnetExpiryDateInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckBM_GrossWeightUQ() => CombineAssertions(() =>
	{
		AssertNoErrorContaining(departureMovement.BM_GrossWeightUQInfo, MandatoryValidation.MustBeEntered);
		AssertNoErrorContaining(departureMovement.BM_GrossWeightUQInfo, ListValidation.InvalidCodeError);

		departureMovement.BM_GrossWeightUQ = ZString.Empty;
		AssertHasErrorContaining(departureMovement.BM_GrossWeightUQInfo, MandatoryValidation.MustBeEntered);

		departureMovement.BM_GrossWeightUQ = new("ZZ");
		ValidationTestHelper.AssertErrorIfInvalidCode(departureMovement.BM_GrossWeightUQInfo, new("ZZ"), new("KG"));

		foreach (var weightUnit in Constants.Weight.Codes)
		{
			departureMovement.BM_GrossWeightUQ = weightUnit;
			AssertNoErrorContaining(departureMovement.BM_GrossWeightUQInfo, ListValidation.InvalidCodeError);
		}
	});

	public void TestCheckBM_CustomsOfficeAtBorder()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var office = departureMovement.CustomsOffices.AddNew();
		office.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
		office.CY_Data = "FR001";
		ValidationTestHelper.AssertInvalidCodeMessageError(departureMovement.BM_CustomsOfficeAtBorderInfo, "XYZ", "FR001");
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		departureMovement = nctsHeader.MovementHeader;
		testContext.AddAutoCacheResetObject(nctsHeader);
		testContext.AddAutoCacheResetObject(departureMovement);
	}

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader departureMovement;

	protected override NctsCommonMovementHeader GetMovementHeaderForTest() => departureMovement;
}
