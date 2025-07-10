using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using CoreConstants = Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NctsHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestBH_CommunicationLanguage()
	{
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(nctsHeader.BH_CommunicationLanguageInfo, new[] { new ZString("IE") }, new[] { new ZString(SwissCustomsLanguageList.Codes.German), new ZString(SwissCustomsLanguageList.Codes.French), new ZString(SwissCustomsLanguageList.Codes.Italian) });
	}

	public void TestBH_CommunicationLanguage_NP70180() => CombineAssertions(() =>
	{
		const string message = "[NP70180] In an arrival declaration (NT007) at least one MRN or Additional Goods or Additional Transit Operation must be present.";

		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		nctsHeader.Validation.ValidateBH_CommunicationLanguage();
		AssertHasMessageError("All empty", nctsHeader.BH_CommunicationLanguageInfo, message);

		nctsHeader.CommonMovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.CHActive;
		nctsHeader.Validation.ValidateBH_CommunicationLanguage();
		AssertNoMessageError($"Has BM_CustomsStatus={nctsHeader.CommonMovementHeader.BM_CustomsStatus}", nctsHeader.BH_CommunicationLanguageInfo, message);
		nctsHeader.CommonMovementHeader.BM_CustomsStatus = ZString.Empty;

		nctsHeader.ArrivalMovementHeader.MovementReferenceNumbers.AddNew();
		nctsHeader.Validation.ValidateBH_CommunicationLanguage();
		AssertNoMessageError("Has MovementReferenceNumbers", nctsHeader.BH_CommunicationLanguageInfo, message);
		nctsHeader.ArrivalMovementHeader.MovementReferenceNumbers.RemoveAndDeleteAll();

		nctsHeader.ArrivalMovementHeader.SupernumeraryGoods.AddNew();
		nctsHeader.Validation.ValidateBH_CommunicationLanguage();
		AssertNoMessageError("Has SupernumeraryGoods", nctsHeader.BH_CommunicationLanguageInfo, message);
		nctsHeader.ArrivalMovementHeader.SupernumeraryGoods.RemoveAndDeleteAll();

		nctsHeader.ArrivalMovementHeader.AdditionalTransitOperations.AddNew();
		nctsHeader.Validation.ValidateBH_CommunicationLanguage();
		AssertNoMessageError("Has AdditionalTransitOperations", nctsHeader.BH_CommunicationLanguageInfo, message);
		nctsHeader.ArrivalMovementHeader.SupernumeraryGoods.RemoveAndDeleteAll();

		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.Validation.ValidateBH_CommunicationLanguage();
		AssertNoMessageError("Not Arrival", nctsHeader.BH_CommunicationLanguageInfo, message);
	});

	public void TestCheckRuleNS30128_CH()
	{
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var principal = nctsHeader.Principal;
		principal.E2_AddressOverride = true;
		var docAddress = Factory.New<OrgHeader>().MainAddress;
		principal.E2_OA_Address = docAddress.PK;

		var errorMessage = "[NS30128] Principal organization does not have a valid BP-ID, UID or DUNS registration number";
		var principalPkInfo = principal.OrganisationPKInfo;

		CombineAssertions(() =>
		{
			docAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			principal.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining(principalPkInfo, errorMessage);

			docAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
			principal.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining("Country Code CH", principalPkInfo, errorMessage);

			docAddress.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "123", Core.Constants.CountryCodes.Switzerland);
			principal.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining("DUN Customs Code exists", principalPkInfo, errorMessage);

			docAddress.Header.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.UID, "456", Core.Constants.CountryCodes.Switzerland);
			principal.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining("UID Customs Code exists", principalPkInfo, errorMessage);

			docAddress.Header.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "789", Core.Constants.CountryCodes.Switzerland);
			principal.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining("BID Customs Code exists", principalPkInfo, errorMessage);
		});
	}

	public void TestCheckRuleNS30128_EU() => AssertIdentificationNumber(Core.Constants.CountryCodes.Germany);

	public void TestCheckRuleNS30128_GB() => AssertIdentificationNumber(Core.Constants.CountryCodes.UnitedKingdom);

	void AssertIdentificationNumber(string countryCode)
	{
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var principal = nctsHeader.Principal;
		var docAddress = Factory.New<OrgHeader>().MainAddress;
		principal.E2_OA_Address = docAddress.PK;

		var errorMessage = "[NS30128] Principal organization does not have a valid EORI or TCU registration number";
		var principalPkInfo = nctsHeader.Principal.OrganisationPKInfo;

		CombineAssertions(() =>
		{
			AssertNoMessageErrorContaining(principalPkInfo, errorMessage);

			docAddress.OA_RN_NKCountryCode = countryCode;
			principal.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining($"Country Code {countryCode}", principalPkInfo, errorMessage);

			docAddress.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123", countryCode);
			principal.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining($"TCU Customs Code exists for {countryCode}", principalPkInfo, errorMessage);

			docAddress.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "456", countryCode);
			principal.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining($"Eori Customs Code exists for {countryCode}", principalPkInfo, errorMessage);
		});
	}

	public void TestCheckRuleNS30128_NO()
	{
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var principal = nctsHeader.Principal;
		var docAddress = Factory.New<OrgHeader>().MainAddress;
		principal.E2_OA_Address = docAddress.PK;

		var errorMessage = "[NS30128] Principal organization does not have a valid ORG registration number";
		var principalPkInfo = nctsHeader.Principal.OrganisationPKInfo;

		CombineAssertions(() =>
		{
			AssertNoMessageErrorContaining(principalPkInfo, errorMessage);

			docAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Norway;
			principal.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining("Country Code Norway", principalPkInfo, errorMessage);

			docAddress.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.OrganizationNumber, "XXX", Core.Constants.CountryCodes.Switzerland);
			principal.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining("ORG Customs Code exists for Country != Norway", principalPkInfo, errorMessage);

			docAddress.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.OrganizationNumber, "123", Core.Constants.CountryCodes.Norway);
			principal.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining("ORG Customs Code exists for Norway", principalPkInfo, errorMessage);
		});
	}

	public void TestCheckRuleNS30128_TR()
	{
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var principal = nctsHeader.Principal;
		var docAddress = Factory.New<OrgHeader>().MainAddress;
		principal.E2_OA_Address = docAddress.PK;

		var errorMessage = "[NS30128] Principal organization does not have a valid VAT registration number";
		var principalPkInfo = nctsHeader.Principal.OrganisationPKInfo;

		CombineAssertions(() =>
		{
			AssertNoMessageErrorContaining(principalPkInfo, errorMessage);

			docAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
			principal.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining("Country Code Turkey", principalPkInfo, errorMessage);

			docAddress.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "XXX", Core.Constants.CountryCodes.Switzerland);
			principal.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining("VAT Customs Code exists for Country != Turkey", principalPkInfo, errorMessage);

			docAddress.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123", Core.Constants.CountryCodes.Turkey);
			principal.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining("VAT Customs Code exists for Turkey", principalPkInfo, errorMessage);
		});
	}

	public void TestCheckMovementReferenceNumber() => CombineAssertions(() =>
	{
		const string message = "The MRN version has changed because FOCBS has modified the transit movement. If you want to browse new transit movement data, please send NC016 message.";

		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		nctsHeader.MovementReferenceEntryNumber.CE_EntryStatus = ZString.Empty;
		nctsHeader.Validation.ValidateMovementReferenceNumber();
		AssertNoWarning("mrn not updated", nctsHeader.MovementReferenceNumberInfo, message);

		nctsHeader.MovementReferenceEntryNumber.CE_EntryStatus = CoreConstants.EntryStatusCodes.New;
		nctsHeader.Validation.ValidateMovementReferenceNumber();
		AssertHasWarning("mrn updated", nctsHeader.MovementReferenceNumberInfo, message);
	});

	public void TestEitherDispatchCountryOnGoodsItemOrOnHeaderMustBeFilled()
	{
		var validation = new NctsHeaderValidationExposed(nctsHeader);
		AssertEquals(validation.EitherDispatchCountryOnGoodsItemOrOnHeaderMustBeFilledExposed, false);
	}

	public void TestCheckArrivalMrnFromUser()
	{
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		nctsHeader.ArrivalMrnFromUser = ZString.Empty;
		nctsHeader.Validation.ValidateArrivalMrnFromUser();
		AssertNoNotifications(nctsHeader.ArrivalMrnFromUserInfo);
	}

	public void TestCheckCY_Code_No_TR0002_Arrival() => CombineAssertions(() =>
	{
		const string messageErrorRuleSubstring = "[TR0002]";

		using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
		{
			ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0002Active));

			var dptNctsHeader = Factory.New<NctsHeader>();
			dptNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var dptMovementHeader = dptNctsHeader.MovementHeader;
			dptMovementHeader.CustomsOffices.RemoveAndDeleteAll();
			dptMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
			dptMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
			dptMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
			dptMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
			dptMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
			dptMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
			dptMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
			dptMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
			dptMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
			var customsOfficeCode = dptMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
			AssertHasMessageErrorContaining("Customs Offices max number must be checked in Departure (TR0002)", customsOfficeCode.CY_CodeInfo, messageErrorRuleSubstring);

			var arvNctsHeader = Factory.New<NctsHeader>();
			arvNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var arvMovementHeader = arvNctsHeader.ArrivalMovementHeader;
			arvMovementHeader.CustomsOffices.RemoveAndDeleteAll();
			arvMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
			arvMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
			arvMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
			customsOfficeCode = arvMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
			AssertNoMessageErrorContaining("No checks for Customs Offices max number in Arrival", customsOfficeCode.CY_CodeInfo, messageErrorRuleSubstring);
		}
	});

	public void CheckDestinationCustomsOfficeCodeForArrival()
	{
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		nctsHeader.ArrivalMrnFromUser = ZString.Empty;
		nctsHeader.Validation.ValidateDestinationCustomsOfficeCodeForArrival();
		AssertNoNotifications(nctsHeader.DestinationCustomsOfficeCodeForArrivalInfo);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
	}
	NctsHeader nctsHeader;
}

public class NctsHeaderValidationExposed : NctsHeaderPhase5Validation
{
	public NctsHeaderValidationExposed(NctsHeader parent) : base(parent)
	{
	}

	public bool EitherDispatchCountryOnGoodsItemOrOnHeaderMustBeFilledExposed => base.EitherDispatchCountryOnGoodsItemOrOnHeaderMustBeFilled;
}
