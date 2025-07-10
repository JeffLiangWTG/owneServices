using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;
using UniversalReferenceConstants = Enterprise.Customs.IT.Business.UniversalReferenceConstants;
using ValidationCaptions = Enterprise.Customs.IT.Business.ValidationCaptions;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsDepartureMovementHeaderPhase4ValidationTest : BusinessObjectLookupsTestCase
{
	public void TestCheckMandatoryGuarantee()
	{
		const string errorMessage = "You need to supply a valid guarantee.";

		var departureMovement = nctsHeader.MovementHeader;
		var departureMovementHeaderValidationForTest = new NctsDepartureMovementHeaderPhase4ValidationForTest(departureMovement);

		departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
		CombineAssertions("When ShouldCheckGuarantees", () =>
		{
			departureMovementHeaderValidationForTest.ValidateBM_InBondEntryType();
			AssertHasMessageErrorContaining("No Guarantees", departureMovement.BM_InBondEntryTypeInfo, errorMessage);

			var guarantee = nctsHeader.Guarantees.AddNew();
			departureMovementHeaderValidationForTest.ValidateBM_InBondEntryType();
			AssertNoMessageError("One Guarantee", departureMovement.BM_InBondEntryTypeInfo, errorMessage);

			guarantee.PW_BondType = "B";
			departureMovementHeaderValidationForTest.ValidateBM_InBondEntryType();
			AssertNoMessageError("One Guarantee of Type B", departureMovement.BM_InBondEntryTypeInfo, errorMessage);
		});

		departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
		CombineAssertions("When does not ShouldCheckGuarantees", () =>
		{
			nctsHeader.Guarantees.RemoveAndDeleteAll();
			departureMovementHeaderValidationForTest.ValidateBM_InBondEntryType();
			AssertNoMessageError("No Guarantees", departureMovement.BM_InBondEntryTypeInfo, errorMessage);
		});
	}

	public void TestCheckGoodsLocations()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var departureMovement = nctsHeader.MovementHeader;

		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var requirement = nctsHeader.DocAddresses.FindOrCreateWithRequirement(nctsHeader.ConsignorJobDocAddressRequirement);
		requirement.E2_OA_Address = orgHeader.MainAddress.PK;

		var authorisationHeader = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, permitHolder: nctsHeader.Consignor.OrganisationPK, "999999", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));

		var authorisationRule1 = CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, ITCusAuthorisationRuleTypeList.Codes.Location, "MYLOC1", "MYLOC1 RULE DESCRIPTION");
		var linkedRule1 = authorisationRule1.LinkedCusAuthorisationRules.AddNew();
		linkedRule1.CPR_RuleCode = Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
		linkedRule1.CPR_ValueFrom = "IT137100";

		var authorisationRule2 = CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, ITCusAuthorisationRuleTypeList.Codes.Location, "MYLOC2", "MYLOC2 RULE DESCRIPTION");
		var linkedRule2 = authorisationRule2.LinkedCusAuthorisationRules.AddNew();
		linkedRule2.CPR_RuleCode = Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
		linkedRule2.CPR_ValueFrom = "IT137100";

		nctsHeader.Authorization = "999999";
		var customsOfficeOfTypeDEP = nctsHeader.CustomsOffices.AddNew();
		customsOfficeOfTypeDEP.CY_Code = "DEP";
		customsOfficeOfTypeDEP.CY_Data = "IT137100";
		NctsLookupsTestUtility.AssertLookups("Valid locations are filtered by Customs Office equal to DEP", departureMovement.Lookups.LocationOfGoodsCodeList, ("MYLOC1", "MYLOC1 RULE DESCRIPTION"), ("MYLOC2", "MYLOC2 RULE DESCRIPTION"));

		departureMovement.BM_LocationOfGoodsCode = "MYLOCX";
		AssertHasMessageError(departureMovement.BM_LocationOfGoodsCodeInfo, ListValidation.InvalidCodeMessageError.ToString());

		departureMovement.BM_LocationOfGoodsCode = "MYLOC1";
		AssertNoMessageError(departureMovement.BM_LocationOfGoodsCodeInfo, ListValidation.InvalidCodeMessageError.ToString());
		AssertNoMessageError(departureMovement.BM_LocationOfGoodsCodeInfo, ValidationCaptions.NctsDepartureMovementHeader.NotEnteredGoodsLocationCode);

		departureMovement.BM_LocationOfGoodsCode = ZString.Empty;
		AssertHasMessageError(departureMovement.BM_LocationOfGoodsCodeInfo, ValidationCaptions.NctsDepartureMovementHeader.NotEnteredGoodsLocationCode);

		nctsHeader.Authorization = ZString.Empty;
		departureMovement.Validation.ValidateAll();
		AssertNoMessageError(departureMovement.BM_LocationOfGoodsCodeInfo, ValidationCaptions.NctsDepartureMovementHeader.NotEnteredGoodsLocationCode);
	}

	public void TestCheckPaymentParty()
	{
		departureMovement.PaymentParty = ZString.Empty;
		AssertNoMessageErrorContaining("Allowed to be empty", departureMovement.PaymentPartyInfo, ListValidation.InvalidCodeMessageError.ToString());

		departureMovement.PaymentParty = NctsPaymentPartyList.Codes.ConsigneesAccount;
		AssertNoMessageErrorContaining($"Allowed to be {NctsPaymentPartyList.Codes.ConsigneesAccount}", departureMovement.PaymentPartyInfo, ListValidation.InvalidCodeMessageError.ToString());

		departureMovement.PaymentParty = NctsPaymentPartyList.Codes.DeclarantsAccount;
		AssertNoMessageErrorContaining($"Allowed to be {NctsPaymentPartyList.Codes.DeclarantsAccount}", departureMovement.PaymentPartyInfo, ListValidation.InvalidCodeMessageError.ToString());

		departureMovement.PaymentParty = "X";
		AssertHasMessageErrorContaining("Invalid value", departureMovement.PaymentPartyInfo, ListValidation.InvalidCodeMessageError.ToString());
	}

	public void TestCheckPaymentPartyTriggeredOnValidateAll()
	{
		departureMovement.PaymentParty = "X";
		Factory.Save();

		var departureMovementOnSeparateFactory = new BusinessObjectFactory().Load<NctsDepartureMovementHeader>(departureMovement.PK);
		AssertNoMessageErrorContaining("PRE-CONDITION", departureMovementOnSeparateFactory.PaymentPartyInfo, ListValidation.InvalidCodeMessageError.ToString());

		departureMovementOnSeparateFactory.Validation.ValidateAll();
		AssertHasMessageErrorContaining("POST-CONDITION", departureMovementOnSeparateFactory.PaymentPartyInfo, ListValidation.InvalidCodeMessageError.ToString());
	}

	public void TestCheckDefermentAccountNumberListValidation()
	{
		var consignee = Factory.NewWithValidTestData<OrgHeader>();
		consignee.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "1", Core.Constants.CountryCodes.Italy);
		consignee.CustomsCodes.AddNew(ItalyOrgCusCodeInfo.OrgCusCodes.DefermentApprovaNumberForTrieste, "2", Core.Constants.CountryCodes.Italy);
		departureMovement.PaymentParty = NctsPaymentPartyList.Codes.ConsigneesAccount;
		var requirement = nctsHeader.DocAddresses.FindOrCreateWithRequirement(nctsHeader.ConsigneeJobDocAddressRequirement);
		requirement.E2_OA_Address = consignee.MainAddress.PK;

		departureMovement.DefermentAccountNumber = ZString.Empty;
		AssertNoMessageErrorContaining("Field is not mandatory", departureMovement.DefermentAccountNumberInfo, ListValidation.InvalidCodeMessageError.ToString());

		departureMovement.DefermentAccountNumber = "1";
		AssertNoMessageErrorContaining("'1' is a valid value", departureMovement.DefermentAccountNumberInfo, ListValidation.InvalidCodeMessageError.ToString());

		departureMovement.DefermentAccountNumber = "2";
		AssertNoMessageErrorContaining("'2' is a valid value", departureMovement.DefermentAccountNumberInfo, ListValidation.InvalidCodeMessageError.ToString());

		departureMovement.DefermentAccountNumber = "X";
		AssertHasMessageErrorContaining("Invalid value", departureMovement.DefermentAccountNumberInfo, ListValidation.InvalidCodeMessageError.ToString());
	}

	public void TestCheckDefermentAccountNumberMaxLengthValidation()
	{
		var expectedMessageError = ValidationCaptions.Shared.GetFieldExceedsCustomsMaxLengthCaption(7);

		departureMovement.DefermentAccountNumber = "1234567";
		AssertNoMessageErrorContaining("Not exceeding max length", departureMovement.DefermentAccountNumberInfo, expectedMessageError);

		departureMovement.DefermentAccountNumber = "12345678";
		AssertHasMessageErrorContaining("Exceeding max length", departureMovement.DefermentAccountNumberInfo, expectedMessageError);
	}

	public void TestCheckDefermentAccountNumberTriggeredOnValidateAll()
	{
		var expectedMessageError = ValidationCaptions.Shared.GetFieldExceedsCustomsMaxLengthCaption(7);

		departureMovement.DefermentAccountNumber = "12345678";
		Factory.Save();

		var departureMovementOnSeparateFactory = new BusinessObjectFactory().Load<NctsDepartureMovementHeader>(departureMovement.PK);
		AssertNoMessageErrorContaining("PRE-CONDITION", departureMovementOnSeparateFactory.DefermentAccountNumberInfo, expectedMessageError);

		departureMovementOnSeparateFactory.Validation.ValidateAll();
		AssertHasMessageErrorContaining("POST-CONDITION", departureMovementOnSeparateFactory.DefermentAccountNumberInfo, expectedMessageError);
	}

	public void TestCheckBM_RL_NKDestinationPortWithSingleGoodItem()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var nctsDescOne = nctsHeader.MovementHeader.GoodsItems.AddNew();

		nctsDescOne.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Italy;
		nctsHeader.MovementHeader.BM_RL_NKDestinationPort = ZString.Empty;
		nctsHeader.MovementHeader.Validation.ValidateBM_RL_NKDestinationPort();
		AssertNoMessageErrorContaining(nctsHeader.MovementHeader.BM_RL_NKDestinationPortInfo, ValidationCaptions.NctsDepartureMovementHeader.NotEnteredDestinationCountry);

		nctsHeader.MovementHeader.GoodsItems.DeleteAll();
		var nctsDescEmpty = nctsHeader.MovementHeader.GoodsItems.AddNew();
		nctsDescEmpty.BY_RN_NKCountryOfDestination = ZString.Empty;
		var nctsDescFilled = nctsHeader.MovementHeader.GoodsItems.AddNew();
		nctsDescFilled.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.France;
		nctsDescOne = nctsHeader.MovementHeader.GoodsItems.AddNew();
		nctsDescOne.BY_RN_NKCountryOfDestination = ZString.Empty;
		nctsHeader.MovementHeader.BM_RL_NKDestinationPort = ZString.Empty;
		nctsHeader.MovementHeader.Validation.ValidateBM_RL_NKDestinationPort();
		AssertNoMessageErrorContaining(nctsHeader.MovementHeader.BM_RL_NKDestinationPortInfo, ValidationCaptions.NctsDepartureMovementHeader.NotEnteredDestinationCountry);

		nctsHeader.MovementHeader.GoodsItems.DeleteAll();
		nctsDescOne = nctsHeader.MovementHeader.GoodsItems.AddNew();
		nctsDescOne.BY_RN_NKCountryOfDestination = ZString.Empty;
		nctsHeader.MovementHeader.BM_RL_NKDestinationPort = ZString.Empty;
		nctsHeader.MovementHeader.Validation.ValidateBM_RL_NKDestinationPort();
		AssertHasMessageErrorContaining(nctsHeader.MovementHeader.BM_RL_NKDestinationPortInfo, ValidationCaptions.NctsDepartureMovementHeader.NotEnteredDestinationCountry);

		nctsHeader.MovementHeader.GoodsItems.DeleteAll();
		var nctsDescWithValidValue = nctsHeader.MovementHeader.GoodsItems.AddNew();
		nctsDescWithValidValue.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.France;
		nctsHeader.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Italy;
		nctsHeader.MovementHeader.Validation.ValidateBM_RL_NKDestinationPort();
		AssertHasMessageErrorContaining(nctsHeader.MovementHeader.BM_RL_NKDestinationPortInfo, ValidationCaptions.NctsDepartureMovementHeader.DestinationCountryDeclarationDifferentGoodItems);

		nctsHeader.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.France;
		nctsHeader.MovementHeader.Validation.ValidateBM_RL_NKDestinationPort();
		AssertNoMessageErrorContaining(nctsHeader.MovementHeader.BM_RL_NKDestinationPortInfo, ValidationCaptions.NctsDepartureMovementHeader.DestinationCountryDeclarationDifferentGoodItems);
	}

	public void TestCheckBM_RL_NKDestinationPortWithMultipleGoodItems()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		var nctsDescOne = nctsHeader.MovementHeader.GoodsItems.AddNew();
		nctsDescOne.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.France;
		var nctsDescTwo = nctsHeader.MovementHeader.GoodsItems.AddNew();
		nctsDescTwo.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.UnitedKingdom;
		nctsHeader.MovementHeader.Validation.ValidateBM_RL_NKDestinationPort();
		AssertNoMessageErrorContaining(nctsHeader.MovementHeader.BM_RL_NKDestinationPortInfo, ValidationCaptions.NctsDepartureMovementHeader.DestinationCountryDeclarationDifferentGoodItems);

		nctsHeader.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Italy;
		nctsHeader.MovementHeader.Validation.ValidateBM_RL_NKDestinationPort();
		AssertHasMessageErrorContaining(nctsHeader.MovementHeader.BM_RL_NKDestinationPortInfo, ValidationCaptions.NctsDepartureMovementHeader.DestinationCountryDeclarationDifferentGoodItems);

		nctsHeader.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.UnitedKingdom;
		nctsHeader.MovementHeader.Validation.ValidateBM_RL_NKDestinationPort();
		AssertNoMessageErrorContaining(nctsHeader.MovementHeader.BM_RL_NKDestinationPortInfo, ValidationCaptions.NctsDepartureMovementHeader.DestinationCountryDeclarationDifferentGoodItems);

		nctsHeader.MovementHeader.GoodsItems.DeleteAll();
		nctsDescOne = nctsHeader.MovementHeader.GoodsItems.AddNew();
		nctsDescOne.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.France;
		nctsDescTwo = nctsHeader.MovementHeader.GoodsItems.AddNew();
		nctsDescTwo.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.UnitedKingdom;
		nctsHeader.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Italy;
		var nctsDescThree = nctsHeader.MovementHeader.GoodsItems.AddNew();
		nctsDescThree.BY_RN_NKCountryOfDestination = ZString.Empty;
		nctsHeader.MovementHeader.Validation.ValidateBM_RL_NKDestinationPort();
		AssertNoMessageErrorContaining(nctsHeader.MovementHeader.BM_RL_NKDestinationPortInfo, ValidationCaptions.NctsDepartureMovementHeader.DestinationCountryDeclarationDifferentGoodItems);

		nctsHeader.MovementHeader.GoodsItems.DeleteAll();
		nctsHeader.MovementHeader.GoodsItems.AddNew();
		nctsHeader.MovementHeader.GoodsItems.AddNew();
		nctsHeader.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.France;
		AssertNoMessageErrorContaining(nctsHeader.MovementHeader.BM_RL_NKDestinationPortInfo, ValidationCaptions.NctsDepartureMovementHeader.DestinationCountryDeclarationDifferentGoodItems);
	}

	public void TestCheckBM_RL_NKDestinationPort()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		nctsHeader.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.France;
		nctsHeader.MovementHeader.Validation.ValidateBM_RL_NKDestinationPort();
		AssertNoMessageErrorContaining(nctsHeader.MovementHeader.BM_RL_NKDestinationPortInfo, ValidationCaptions.NctsDepartureMovementHeader.NotEnteredDestinationCountry);

		nctsHeader.MovementHeader.BM_RL_NKDestinationPort = ZString.Empty;
		nctsHeader.MovementHeader.Validation.ValidateBM_RL_NKDestinationPort();
		AssertHasMessageErrorContaining(nctsHeader.MovementHeader.BM_RL_NKDestinationPortInfo, ValidationCaptions.NctsDepartureMovementHeader.NotEnteredDestinationCountry);

		var nctsDescEmpty = nctsHeader.MovementHeader.GoodsItems.AddNew();
		nctsDescEmpty.BY_RN_NKCountryOfDestination = ZString.Empty;
		var nctsDescFilled = nctsHeader.MovementHeader.GoodsItems.AddNew();
		nctsDescFilled.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.France;

		nctsHeader.MovementHeader.BM_RL_NKDestinationPort = ZString.Empty;
		nctsHeader.MovementHeader.Validation.ValidateBM_RL_NKDestinationPort();
		AssertNoMessageErrorContaining(nctsHeader.MovementHeader.BM_RL_NKDestinationPortInfo, ValidationCaptions.NctsDepartureMovementHeader.NotEnteredDestinationCountry);
	}

	public void TestIsExportDateMandatory()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var departureMovementHeaderValidationForTest = new NctsDepartureMovementHeaderPhase4ValidationForTest(nctsHeader.MovementHeader);
		AssertEquals("IsExportDateMandatory", true, departureMovementHeaderValidationForTest.IsExportDateMandatoryExposed);
	}

	public void TestCheckBM_ControlChannelListValidation()
	{
		departureMovement.BM_ControlChannel = "XYZ";
		AssertHasErrorContaining("When value is invalid", departureMovement.BM_ControlChannelInfo, ListValidation.InvalidCodeError);

		departureMovement.BM_ControlChannel = ZString.Empty;
		AssertNoErrors("When field is empty", departureMovement.BM_ControlChannelInfo);

		departureMovement.BM_ControlChannel = CustomsChannelCodeList.Codes.AutomaticControl;
		AssertNoErrors($"When value is a valid one ('{CustomsChannelCodeList.Codes.AutomaticControl}')", departureMovement.BM_ControlChannelInfo);

		departureMovement.BM_ControlChannel = CustomsChannelCodeList.Codes.DocumentControl;
		AssertNoErrors($"When value is a valid one ('{CustomsChannelCodeList.Codes.DocumentControl}')", departureMovement.BM_ControlChannelInfo);

		departureMovement.BM_ControlChannel = CustomsChannelCodeList.Codes.Inspection;
		AssertNoErrors($"When value is a valid one ('{CustomsChannelCodeList.Codes.Inspection}')", departureMovement.BM_ControlChannelInfo);

		departureMovement.BM_ControlChannel = CustomsChannelCodeList.Codes.ScannerControl;
		AssertNoErrors($"When value is a valid one ('{CustomsChannelCodeList.Codes.ScannerControl}')", departureMovement.BM_ControlChannelInfo);
	}

	public void TestCheckDefermentAccountNumber()
	{
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		var fee = goodsItem.Fees.AddNew();
		AssertNoErrors("When field and MoP are empty", departureMovement.DefermentAccountNumberInfo);

		departureMovement.DefermentAccountNumber = "11111";
		AssertHasMessageErrorContaining("Deferement Account Number must be empty if MoP is not equal to 'E', 'F' or 'G'", departureMovement.DefermentAccountNumberInfo, ValidationCaptions.NctsDepartureMovementHeader.ApprovalDeferNoMustBeEmpty);

		departureMovement.DefermentAccountNumber = "11111";
		fee.BFE_MethodOfPayment = UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentE;
		var phase4Validation = (NctsDepartureMovementHeaderPhase4Validation)departureMovement.Validation;
		phase4Validation.ValidateDefermentAccountNumber();
		AssertNoErrors("No errors should be displayed when Deferement Account Number is defined and MoP is equal to 'E', 'F' or 'G'", departureMovement.DefermentAccountNumberInfo);

		departureMovement.DefermentAccountNumber = "";
		AssertHasMessageErrorContaining("When Deferement Account Number must be empty if MoP is not equal to 'E', 'F' or 'G'", departureMovement.DefermentAccountNumberInfo, ValidationCaptions.NctsDepartureMovementHeader.ApprovalDeferNoMustBeFilled);
	}

	public void TestCheckBM_ExportTransportMode()
	{
		departureMovement.BM_ExportTransportMode = "X";
		AssertNoMessageErrorContaining(departureMovement.BM_ExportTransportModeInfo, MandatoryValidation.YouHaveNotEntered);

		departureMovement.BM_ExportTransportMode = "";
		AssertHasMessageErrorContaining(departureMovement.BM_ExportTransportModeInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckBM_ExportDate_MandatoryValidation()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_ExportDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(departureMovement.BM_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);

			departureMovement.BM_ExportDate = new ZDateTime(2021, 01, 01);
			AssertNoMessageErrorContaining(departureMovement.BM_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckBM_ExportDate_CannotBeInThePast()
	{
		CombineAssertions(() =>
		{
			var dateLimitCannotBeInThePast = ValidationCaptions.NctsDepartureMovementHeader.DateLimitCannotBeInThePast;

			departureMovement.BM_CustomsStatus = "";
			departureMovement.BM_ExportDate = ZDateTime.Empty;
			AssertNoMessageErrorContaining("When both BM_ExportDate and BM_CustomsStatus are empty", departureMovement.BM_ExportDateInfo, dateLimitCannotBeInThePast);

			departureMovement.BM_ExportDate = ZDateTime.Today.AddDays(-1);
			AssertHasMessageErrorContaining("When BM_ExportDate is in the past and BM_CustomsStatus is empty", departureMovement.BM_ExportDateInfo, dateLimitCannotBeInThePast);

			departureMovement.BM_ExportDate = ZDateTime.Today;
			AssertNoMessageErrorContaining("When BM_ExportDate is not in the past and BM_CustomsStatus is empty", departureMovement.BM_ExportDateInfo, dateLimitCannotBeInThePast);

			departureMovement.BM_CustomsStatus = "XYZ";
			departureMovement.BM_ExportDate = ZDateTime.Empty;
			AssertNoMessageErrorContaining("When BM_ExportDate is empty and BM_CustomsStatus is not empty", departureMovement.BM_ExportDateInfo, dateLimitCannotBeInThePast);

			departureMovement.BM_ExportDate = ZDateTime.Today.AddDays(-1);
			AssertNoMessageErrorContaining("When BM_ExportDate is in the past and BM_CustomsStatus is not empty", departureMovement.BM_ExportDateInfo, dateLimitCannotBeInThePast);

			departureMovement.BM_ExportDate = ZDateTime.Today;
			AssertNoMessageErrorContaining("When BM_ExportDate is not in the past and BM_CustomsStatus is not empty", departureMovement.BM_ExportDateInfo, dateLimitCannotBeInThePast);
		});
	}

	public void TestCheckBM_ExportDate_LessThan8DaysFromNow()
	{
		CombineAssertions(() =>
		{
			var dateLimitIsLessThan8DaysFromNow = ValidationCaptions.NctsDepartureMovementHeader.DateLimitIsLessThan8DaysFromNow;

			departureMovement.BM_CustomsStatus = "";
			departureMovement.BM_ExportDate = ZDateTime.Empty;
			AssertNoWarningContaining("When both BM_ExportDate and BM_CustomsStatus are empty", departureMovement.BM_ExportDateInfo, dateLimitIsLessThan8DaysFromNow);

			departureMovement.BM_ExportDate = ZDateTime.Today.AddDays(5);
			AssertHasWarningContaining("When BM_ExportDate is in the future but less than 8 days from current date and BM_CustomsStatus is empty", departureMovement.BM_ExportDateInfo, dateLimitIsLessThan8DaysFromNow);

			departureMovement.BM_ExportDate = ZDateTime.Today.AddDays(9);
			AssertNoWarningContaining("When BM_ExportDate is in the future but greater than 8 days from current date and BM_CustomsStatus is empty", departureMovement.BM_ExportDateInfo, dateLimitIsLessThan8DaysFromNow);

			departureMovement.BM_ExportDate = ZDateTime.Today;
			AssertNoWarningContaining("When BM_ExportDate is today and BM_CustomsStatus is empty", departureMovement.BM_ExportDateInfo, dateLimitIsLessThan8DaysFromNow);

			departureMovement.BM_CustomsStatus = "XYZ";
			departureMovement.BM_ExportDate = ZDateTime.Empty;
			AssertNoWarningContaining("When BM_ExportDate is empty and BM_CustomsStatus is not empty", departureMovement.BM_ExportDateInfo, dateLimitIsLessThan8DaysFromNow);

			departureMovement.BM_ExportDate = ZDateTime.Today.AddDays(5);
			AssertNoWarningContaining("When BM_ExportDate is in the future but less than 8 days from current date and BM_CustomsStatus is not empty", departureMovement.BM_ExportDateInfo, dateLimitIsLessThan8DaysFromNow);

			departureMovement.BM_ExportDate = ZDateTime.Today.AddDays(9);
			AssertNoWarningContaining("When BM_ExportDate is in the future but greater than 8 days from current date and BM_CustomsStatus is not empty", departureMovement.BM_ExportDateInfo, dateLimitIsLessThan8DaysFromNow);

			departureMovement.BM_ExportDate = ZDateTime.Today;
			AssertNoWarningContaining("When BM_ExportDate is today and BM_CustomsStatus is not empty", departureMovement.BM_ExportDateInfo, dateLimitIsLessThan8DaysFromNow);
		});
	}

	public void TestCheckBM_TOLCarrierCode_ConditionC010DisabledForTir()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			departureMovement.Validation.ValidateBM_TOLCarrierCode();
			AssertHasMessageErrorContaining("When InbondEntryType != TIR, C010 error message should be shown", departureMovement.BM_TOLCarrierCodeInfo, "C010");
			departureMovement.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
			departureMovement.Validation.ValidateBM_TOLCarrierCode();
			AssertNoMessageErrorContaining("When InbondEntryType = TIR, C010 validation should be disabled", departureMovement.BM_TOLCarrierCodeInfo, "C010");
		});
	}

	public void TestCheckParticipantType()
	{
		departureMovement.ParticipantType = "XYZ";
		AssertHasErrorContaining("Invalid participant code", departureMovement.ParticipantTypeInfo, ListValidation.InvalidCodeError);

		departureMovement.ParticipantType = "STD";
		AssertNoErrorContaining("Valid participant code", departureMovement.ParticipantTypeInfo, ListValidation.InvalidCodeError);

		departureMovement.ParticipantType = "GRP";
		AssertNoErrorContaining("Valid participant code", departureMovement.ParticipantTypeInfo, ListValidation.InvalidCodeError);

		departureMovement.ParticipantType = ZString.Empty;
		AssertHasErrorContaining("Empty participant code", departureMovement.ParticipantTypeInfo, MandatoryValidation.MustBeEntered);
	}

	public void TestCheckBM_PlaceOfUnloading_ListValidation()
	{
		departureMovement.BM_PlaceOfUnloading = "XYZ";
		AssertNoMessageErrors(departureMovement.BM_PlaceOfUnloadingInfo);
	}

	public void TestCheckGuarantees()
	{
		const string needToSupplyValidGuaranteeErrorMessage = "You need to supply a valid guarantee.";

		CombineAssertions(() =>
		{
			departureMovement.BM_InBondEntryType = "T";
			AssertHasMessageErrorContaining("When InBondEntryType is not TIR and No Guarantees", departureMovement.BM_InBondEntryTypeInfo, needToSupplyValidGuaranteeErrorMessage);

			departureMovement.BM_InBondEntryType = "TIR";
			AssertNoMessageError("When InBondEntryType is TIR", departureMovement.BM_InBondEntryTypeInfo, needToSupplyValidGuaranteeErrorMessage);
		});
	}

	public void TestCheckBM_PlaceOfLoading()
	{
		CombineAssertions(() =>
		{
			nctsHeader.BH_FTZMove = false;
			departureMovement.BM_PlaceOfLoading = ZString.Empty;
			AssertNoMessageErrorContaining("No error message should appear when BH_FTZMove is false", departureMovement.BM_PlaceOfLoadingInfo, "C191");

			nctsHeader.BH_FTZMove = true;
			departureMovement.BM_PlaceOfLoading = ZString.Empty;
			AssertHasMessageErrorContaining("Error message should appear when BH_FTZMove is true", departureMovement.BM_PlaceOfLoadingInfo, "C191");

			departureMovement.BM_PlaceOfLoading = "GBLON";
			AssertNoNotifications("No error message should appear adding a valid UNLOCO", departureMovement.BM_PlaceOfLoadingInfo);

			departureMovement.BM_PlaceOfLoading = "AABBB";
			AssertNoNotifications("No error message should appear adding an invalid UNLOCO", departureMovement.BM_PlaceOfLoadingInfo);
		});
	}

	public void TestCheckTirCarnetExpiryDateMandatory()
	{
		departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
		departureMovement.Validation.ValidateTirCarnetExpiryDate();
		AssertNoNotifications("No error message should appear when BM_InBondEntryType is TIR", departureMovement.TirCarnetExpiryDateInfo);
	}

	public void TestCheckBM_AdditionalTextSafetyAndSecurityWithMultipleGoodsItems()
	{
		nctsHeader.BH_FTZMove = true;
		var validation = departureMovement.Validation;

		departureMovement.BM_AdditionalText = ZString.Empty;
		var goodsItems1 = departureMovement.GoodsItems.AddNew();
		var goodsItems2 = departureMovement.GoodsItems.AddNew();

		goodsItems1.BY_CommercialReferenceNumber = "54321";
		goodsItems2.BY_CommercialReferenceNumber = "54321";
		validation.ValidateBM_AdditionalText();
		AssertNoMessageErrors("When BM_AdditionalText is empty and all goods items BY_CommercialReferenceNumber are filled then there should be no error message", departureMovement.BM_AdditionalTextInfo);

		goodsItems1.BY_CommercialReferenceNumber = ZString.Empty;
		validation.ValidateBM_AdditionalText();
		AssertNoMessageErrors("When BM_AdditionalText is empty and one good items BY_CommercialReferenceNumber is filled but not all of them are filled then there should be no error message", departureMovement.BM_AdditionalTextInfo);

		goodsItems1.BY_CommercialReferenceNumber = "54321";
		departureMovement.BM_AdditionalText = "54321";
		validation.ValidateBM_AdditionalText();
		AssertHasMessageErrorContaining("When BM_AdditionalText is filled and all goods items BY_CommercialReferenceNumber are filled then an error message should appear", departureMovement.BM_AdditionalTextInfo, "The field Commercial Reference Number cannot be set both at Header and Goods Items level");

		goodsItems2.BY_CommercialReferenceNumber = ZString.Empty;
		validation.ValidateBM_AdditionalText();
		AssertHasMessageErrorContaining("When BM_AdditionalText is filled and one good items BY_CommercialReferenceNumber is filled but not all of them are filled then an error message should appear", departureMovement.BM_AdditionalTextInfo, "The field Commercial Reference Number cannot be set both at Header and Goods Items level");

		goodsItems1.BY_CommercialReferenceNumber = ZString.Empty;
		validation.ValidateBM_AdditionalText();
		AssertNoMessageErrorContaining("When BM_AdditionalText is filled and all goods items BY_CommercialReferenceNumber are also empty then there should be no error message", departureMovement.BM_AdditionalTextInfo, "The field Commercial Reference Number cannot be set both at Header and Goods Items level");
	}

	public void TestCheckBM_AdditionalTextSafetyAndSecurityWithOneOrNoGoodItems()
	{
		nctsHeader.BH_FTZMove = true;
		var validation = departureMovement.Validation;

		departureMovement.BM_AdditionalText = ZString.Empty;
		var goodsItems = departureMovement.GoodsItems.AddNew();
		goodsItems.BY_CommercialReferenceNumber = ZString.Empty;
		validation.ValidateBM_AdditionalText();

		AssertNoMessageErrors("When BM_AdditionalText is empty and BY_CommercialReferenceNumber there should be no error message", departureMovement.BM_AdditionalTextInfo);

		departureMovement.BM_AdditionalText = "54321";
		validation.ValidateBM_AdditionalText();
		AssertNoMessageErrorContaining("When BM_AdditionalText is filled and BY_CommercialReferenceNumber there should be no error message", departureMovement.BM_AdditionalTextInfo, "The field Commercial Reference Number cannot be set both at Header and Goods Items level");

		goodsItems.BY_CommercialReferenceNumber = "54321";
		validation.ValidateBM_AdditionalText();
		AssertHasMessageErrorContaining("When BM_AdditionalText is filled and BY_CommercialReferenceNumber is filled then an error message should appear", departureMovement.BM_AdditionalTextInfo, "The field Commercial Reference Number cannot be set both at Header and Goods Items level");
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
		departureMovement = nctsHeader.MovementHeader;
	}
	NctsHeader nctsHeader;
	NctsDepartureMovementHeader departureMovement;

	sealed class NctsDepartureMovementHeaderPhase4ValidationForTest : NctsDepartureMovementHeaderPhase4Validation
	{
		public NctsDepartureMovementHeaderPhase4ValidationForTest(NctsDepartureMovementHeader parent) : base(parent)
		{
		}

		public ZBool IsExportDateMandatoryExposed => IsExportDateMandatory;
	}
}
