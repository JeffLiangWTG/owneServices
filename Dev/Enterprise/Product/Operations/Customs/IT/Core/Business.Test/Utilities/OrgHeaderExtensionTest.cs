using System;
using System.Linq;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class OrgHeaderExtensionTest : TestCaseWithFactory
{
	public void TestIsNaturalPersonIndividual()
	{
		AssertEquals("IsNaturalPersonIndividual", false, (null as OrgHeader).IsNaturalPersonIndividual());

		organization.OH_Category = "BUS";
		AssertEquals("IsNaturalPersonIndividual", false, organization.IsNaturalPersonIndividual());

		organization.OH_Category = "NAT";
		AssertEquals("IsNaturalPersonIndividual", true, organization.IsNaturalPersonIndividual());
	}

	public void TestGetEoriOrTcuTraderNumber()
	{
		var orgAddress = organization.MainAddress;
		CombineAssertions("GetEoriOrTcuTraderNumber", () =>
		{
			AssertEquals("null OrgAddress.", ZString.Empty, (null as OrgAddress).GetEoriOrTcuTraderNumber());
			AssertEquals("null JobDocAddress.", ZString.Empty, (null as JobDocAddress).GetEoriOrTcuTraderNumber());
			AssertEquals("empty OrgAddress.", ZString.Empty, orgAddress.GetEoriOrTcuTraderNumber());

			orgAddress.CustomsCodes.AddNew("EOR", "385040449", "IT");
			orgAddress.CustomsCodes.AddNew("TCU", "ABB3332", "IT");
			AssertEquals("Return EOR when present.", "IT385040449", orgAddress.GetEoriOrTcuTraderNumber());

			orgAddress.CustomsCodes.RemoveAll(x => x.OK_CodeType == "EOR");
			AssertEquals("Fallback to TCU when no EOR present.", "ITABB3332", orgAddress.GetEoriOrTcuTraderNumber());
		});
	}

	public void TestGetFiscalCode()
	{
		AssertEquals("GetFiscalCode return value", ZString.Empty, organization.GetFiscalCode());
		AssertEquals("GetFiscalCode return value", ZString.Empty, (null as OrgHeader).GetFiscalCode());

		var fiscalCode = organization.CustomsCodes.AddNew();
		fiscalCode.OK_CodeType = "COD";
		fiscalCode.OK_CustomsRegNo = "FISCAL_CODE";
		fiscalCode.OK_RN_NKCodeCountry = "IT";

		AssertEquals("GetFiscalCode return value", "FISCAL_CODE", organization.GetFiscalCode());
	}

	public void TestGetEoriCode()
	{
		CombineAssertions(() =>
		{
			AssertEquals("GetEoriCode return value", ZString.Empty, organization.GetEoriCode());
			AssertEquals("GetEoriCode return value", ZString.Empty, organization.GetEoriCode(countryCode: true));
			AssertEquals("GetEoriCode return value", ZString.Empty, (null as OrgHeader).GetEoriCode());
			AssertEquals("GetEoriCode return value", ZString.Empty, (null as OrgHeader).GetEoriCode(countryCode: true));
		});

		var eoriCode = organization.CustomsCodes.AddNew();
		eoriCode.OK_CodeType = "EOR";
		eoriCode.OK_CustomsRegNo = "EORI_CODE";
		eoriCode.OK_RN_NKCodeCountry = "IT";

		CombineAssertions(() =>
		{
			AssertEquals("GetEoriCode return value", "EORI_CODE", organization.GetEoriCode());
			AssertEquals("GetEoriCode with countryCode", "ITEORI_CODE", organization.GetEoriCode(countryCode: true));
		});
	}

	public void TestHasAeoFSCode()
	{
		AssertEquals("HasAeoFSCode return value", ZBool.False, organization.HasAeoFSCode());
		AssertEquals("HasAeoFSCode return value", ZBool.False, (null as OrgHeader).HasAeoFSCode());

		var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
		orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEOF1234", Core.Constants.CountryCodes.Italy);

		var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
		orgHeader2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO", Core.Constants.CountryCodes.Italy);

		AssertEquals("HasAeoFSCode return value", ZBool.True, orgHeader1.HasAeoFSCode());
		AssertEquals("HasAeoFSCode return value", ZBool.False, orgHeader2.HasAeoFSCode());
	}

	public void TestGetRexCode()
	{
		AssertEquals("GetRexCode return value", ZString.Empty, organization.GetRexCode());
		AssertEquals("GetRexCode return value", ZString.Empty, (null as OrgHeader).GetRexCode());

		var rexCode = organization.CustomsCodes.AddNew();
		rexCode.OK_CodeType = "REX";
		rexCode.OK_CustomsRegNo = "REX_CODE";
		rexCode.OK_RN_NKCodeCountry = "IT";

		AssertEquals("GetRexCode return value", "REX_CODE", organization.GetRexCode());
	}

	public void TestGetCusCode()
	{
		AssertEquals("GetCusCode return value", null, organization.GetCusCode("EOR"));
		AssertEquals("GetCusCode return value", null, (null as OrgHeader).GetCusCode("EOR"));
		AssertEquals("GetCusCode return value", null, organization.GetCusCode(""));

		var eoriCode = organization.CustomsCodes.AddNew();
		eoriCode.OK_CodeType = "EOR";
		eoriCode.OK_CustomsRegNo = "EORI_CODE";
		eoriCode.OK_RN_NKCodeCountry = "DE";

		var eoriCusCode = organization.GetCusCode("EOR");
		AssertNotNull("GetCusCode return value", eoriCusCode);
		CombineAssertions("Eori CusCode", () =>
		{
			AssertEquals("OK_CodeType", "EOR", eoriCode.OK_CodeType);
			AssertEquals("OK_CustomsRegNo", "EORI_CODE", eoriCode.OK_CustomsRegNo);
			AssertEquals("OK_RN_NKCodeCountry", "DE", eoriCode.OK_RN_NKCodeCountry);
		});

		var fiscalCode = organization.CustomsCodes.AddNew();
		fiscalCode.OK_CodeType = "COD";
		fiscalCode.OK_CustomsRegNo = "FISCAL_CODE";
		fiscalCode.OK_RN_NKCodeCountry = "DE";

		var fiscalCusCode = organization.GetCusCode("COD");
		AssertNull("GetCusCode return value", fiscalCusCode);

		fiscalCode.OK_RN_NKCodeCountry = "IT";
		fiscalCusCode = organization.GetCusCode("COD");
		CombineAssertions("FiscalCode CusCode", () =>
		{
			AssertEquals("OK_CodeType", "COD", fiscalCusCode.OK_CodeType);
			AssertEquals("OK_CustomsRegNo", "FISCAL_CODE", fiscalCusCode.OK_CustomsRegNo);
			AssertEquals("OK_RN_NKCodeCountry", "IT", fiscalCusCode.OK_RN_NKCodeCountry);
		});
	}

	public void TestGetVatCode()
	{
		AssertEquals("VAT (IVA) code not available", ZString.Empty, organization.GetVatCode());
		AssertEquals("VAT (IVA) code not available (for null organization)", ZString.Empty, (null as OrgHeader).GetVatCode());

		var vatCode = organization.CustomsCodes.AddNew();
		vatCode.OK_CodeType = "IVA";
		vatCode.OK_CustomsRegNo = "IVA_CODE";
		vatCode.OK_RN_NKCodeCountry = "IT";

		AssertEquals("VAT (IVA) code available", "IVA_CODE", organization.GetVatCode());
	}

	public void TestGetAeoCode()
	{
		AssertEquals("AEO code not available", ZString.Empty, organization.GetAeoCode());
		AssertEquals("AEO code not available (for null organization)", ZString.Empty, (null as OrgHeader).GetAeoCode());

		var jpAeo = organization.CustomsCodes.AddNew();
		jpAeo.OK_CodeType = "AEO";
		jpAeo.OK_CustomsRegNo = "AEO_JP";
		jpAeo.OK_RN_NKCodeCountry = "JP";
		AssertEquals("AEO code not available (for non EU country)", ZString.Empty, organization.GetAeoCode());

		var frAeo = organization.CustomsCodes.AddNew();
		frAeo.OK_CodeType = "AEO";
		frAeo.OK_CustomsRegNo = "AEO_FR";
		frAeo.OK_RN_NKCodeCountry = "FR";

		AssertEquals("AEO code available", "AEO_FR", organization.GetAeoCode());
	}

	public void TestGetCustomsCodeTypeListRequiredForTrader()
	{
		AssertExceptionThrown<ArgumentNullException>(() => (null as OrgHeader).GetCustomsCodeTypeListRequiredForEUTrader());

		organization.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		AssertArrayEqualsByElements("CodeType list for natural person", new ZString[] { "EOR", "COD" }, organization.GetCustomsCodeTypeListRequiredForEUTrader());

		organization.OH_Category = OrgConstants.Category.Business;
		AssertArrayEqualsByElements("CodeType list for business organization", new ZString[] { "EOR", "IVA" }, organization.GetCustomsCodeTypeListRequiredForEUTrader());
	}

	public void TestGetCustomsCodeTypeListRequiredForTraderIgnoringEori()
	{
		AssertExceptionThrown<ArgumentNullException>(() => (null as OrgHeader).GetCustomsCodeTypeListRequiredForEUTrader(ignoreEoriCusCode: true));

		organization.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		AssertArrayEqualsByElements("CodeType list for natural person", new ZString[] { "COD" }, organization.GetCustomsCodeTypeListRequiredForEUTrader(ignoreEoriCusCode: true));

		organization.OH_Category = OrgConstants.Category.Business;
		AssertArrayEqualsByElements("CodeType list for business organization", new ZString[] { "IVA" }, organization.GetCustomsCodeTypeListRequiredForEUTrader(ignoreEoriCusCode: true));
	}

	public void TestGetFirstCusCodeMatchingTypeInOrder()
	{
		AssertExceptionThrown<ArgumentNullException>(() => (null as OrgHeader).GetFirstCusCodeMatchingTypeInOrder("EOR"));

		var eorCusCode = organization.CustomsCodes.AddNew();
		eorCusCode.OK_CodeType = "EOR";
		eorCusCode.OK_CustomsRegNo = "EORI CODE";
		eorCusCode.OK_RN_NKCodeCountry = "IT";

		var codCusCode = organization.CustomsCodes.AddNew();
		codCusCode.OK_CodeType = "COD";
		codCusCode.OK_CustomsRegNo = "FISCAL CODE";
		codCusCode.OK_RN_NKCodeCountry = "IT";

		var firstCusCode = organization.GetFirstCusCodeMatchingTypeInOrder("EOR", "COD");
		AssertNotNull("EOR CusCode available", firstCusCode);
		AssertEquals("EOR", firstCusCode.OK_CodeType);

		organization.CustomsCodes.RemoveAndDelete(eorCusCode);
		firstCusCode = organization.GetFirstCusCodeMatchingTypeInOrder("EOR", "COD");
		AssertNotNull("COD CusCode available", firstCusCode);
		AssertEquals("COD", firstCusCode.OK_CodeType);

		organization.CustomsCodes.RemoveAndDeleteAll();
		firstCusCode = organization.GetFirstCusCodeMatchingTypeInOrder("EOR", "COD");
		AssertNull("No CusCode available", firstCusCode);
	}

	public void TestGetAddressAsASingleLineForCustomsMessage()
	{
		var mainAddress = organization.MainAddress;

		mainAddress.Address1 = "";
		mainAddress.Address2 = "";
		AssertEquals("", mainAddress.GetAddressAsASingleLineForCustomsMessage());

		mainAddress.Address1 = " ";
		mainAddress.Address2 = " ";
		AssertEquals("", mainAddress.GetAddressAsASingleLineForCustomsMessage());

		mainAddress.Address1 = "ABCDEF";
		mainAddress.Address2 = " ";
		AssertEquals("ABCDEF", mainAddress.GetAddressAsASingleLineForCustomsMessage());

		mainAddress.Address1 = "ABCDEF";
		mainAddress.Address2 = "GHI";
		AssertEquals("ABCDEF GHI", mainAddress.GetAddressAsASingleLineForCustomsMessage());

		mainAddress.Address1 = " ";
		mainAddress.Address2 = "GHI";
		AssertEquals("GHI", mainAddress.GetAddressAsASingleLineForCustomsMessage());
	}

	public void TestGetDefermentApprovalNumberCode()
	{
		AssertEquals("DAN code not available", ZString.Empty, organization.GetDefermentApprovalNumberCode());
		AssertEquals("DAN code not available (for null organization)", ZString.Empty, (null as OrgHeader).GetDefermentApprovalNumberCode());

		var vatCode = organization.CustomsCodes.AddNew();
		vatCode.OK_CodeType = "DAN";
		vatCode.OK_CustomsRegNo = "DAN_CODE";
		vatCode.OK_RN_NKCodeCountry = "IT";

		AssertEquals("DAN code available", "DAN_CODE", organization.GetDefermentApprovalNumberCode());
	}

	public void TestGetDefermentApprovalNumberForTriesteCode()
	{
		AssertEquals("DAT code not available", ZString.Empty, organization.GetDefermentApprovalNumberForTriesteCode());
		AssertEquals("DAT code not available (for null organization)", ZString.Empty, (null as OrgHeader).GetDefermentApprovalNumberForTriesteCode());

		var vatCode = organization.CustomsCodes.AddNew();
		vatCode.OK_CodeType = "DAT";
		vatCode.OK_CustomsRegNo = "DAT_CODE";
		vatCode.OK_RN_NKCodeCountry = "IT";

		AssertEquals("DAN code available", "DAT_CODE", organization.GetDefermentApprovalNumberForTriesteCode());
	}

	public void TestGetControlledPremisesIDs()
	{
		AssertContainsExactElementsInAnyOrder("When organisation is null, GetControlledPremisesIDs", Array.Empty<ZString>(), (null as OrgHeader).GetControlledPremisesIDs().ToArray());

		var organisation = Factory.New<OrgHeader>();
		organisation.OH_Code = "ORGH1";
		organisation.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
		var cusCode = organisation.CustomsCodes.AddNew();

		cusCode.OK_CodeType = "CPA";
		cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;
		cusCode.OK_CustomsRegNo = "1234";

		AssertContainsExactElementsInAnyOrder("When organisation does not have a CCP, GetControlledPremisesID", Array.Empty<ZString>(), organisation.GetControlledPremisesIDs());

		cusCode.OK_CodeType = "CCP";
		AssertContainsExactElementsInAnyOrder("When organisation have a CCP, GetControlledPremisesID", new[] { "1234" }, organisation.GetControlledPremisesIDs());
	}

	public void TestHasLocAuthorisation()
	{
		AssertEquals("When organisation is null, HasLocAuthorisation", false, (null as OrgHeader).HasLocAuthorisation(""));

		var organisation = Factory.New<OrgHeader>();
		organisation.OH_Code = "ORGH1";
		organisation.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
		var cusCode = organisation.CustomsCodes.AddNew("CCP", "C123456XIT");
		cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;
		AssertEquals("When Organisation does not have any authorisations with LOC rule equal to entered CCP", false, organisation.HasLocAuthorisation("123456X"));

		var authorisationHeader = Factory.New<CusAuthorisationHeader>();
		authorisationHeader.CPH_Type = "CWP";
		authorisationHeader.CPH_Number = "123456X";
		authorisationHeader.CPH_OH_PermitHolder = organisation.PK;
		authorisationHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
		authorisationHeader.CPH_EndDate = ZDate.Empty;
		authorisationHeader.CPH_ApplicationCode = "AUT";
		authorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
		var locRule = authorisationHeader.CusAuthorisationRules.AddNew();
		locRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
		locRule.CPR_ValueFrom = "123456X";
		Factory.Save();

		AssertEquals("When Organisation have at least one authorisation with LOC rule equal to entered CCP", true, organisation.HasLocAuthorisation("C123456XIT"));
		AssertEquals("When Organisation have at least one authorisation with LOC rule equal to entered CCP", false, organisation.HasLocAuthorisation("XXXX"));

		authorisationHeader.CPH_ApplicationCode = "GUA";
		Factory.Save();
		AssertEquals("When Organisation does not have any valid authorisations [CPH_ApplicationCode: GUA] with LOC rule equal to entered CCP", false, organisation.HasLocAuthorisation("C123456XIT"));

		authorisationHeader.CPH_ApplicationCode = "AUT";
		authorisationHeader.CPH_RN_NKCountryCode = "FR";
		Factory.Save();
		AssertEquals("When Organisation does not have any valid authorisations [CPH_RN_NKCountryCode: FR] with LOC rule equal to entered CCP", false, organisation.HasLocAuthorisation("C123456XIT"));

		authorisationHeader.CPH_RN_NKCountryCode = "IT";
		authorisationHeader.CPH_IsActive = false;
		Factory.Save();
		AssertEquals("When Organisation does not have any valid authorisations [CPH_IsActive: false] with LOC rule equal to entered CCP", false, organisation.HasLocAuthorisation("C123456XIT"));

		authorisationHeader.CPH_IsActive = true;
		authorisationHeader.CPH_StartDate = ZDate.Today.AddMonths(1);
		Factory.Save();
		AssertEquals("When Organisation does not have any valid authorisations [CPH_StartDate: greater than today] with LOC rule equal to entered CCP", false, organisation.HasLocAuthorisation("C123456XIT"));

		authorisationHeader.CPH_StartDate = ZDate.Today.AddMonths(-2);
		authorisationHeader.CPH_EndDate = ZDate.Today.AddMonths(-1);
		Factory.Save();
		AssertEquals("When Organisation does not have any valid authorisations [CPH_EndDate : less than Today] with LOC rule equal to entered CCP", false, organisation.HasLocAuthorisation("C123456XIT"));

		authorisationHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
		authorisationHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
		Factory.Save();
		AssertEquals("When Organisation have at least one authorisation with LOC rule equal to entered CCP", true, organisation.HasLocAuthorisation("C123456XIT"));
	}

	public void TestGetCustomsCodeInfoReturnsEmpty()
	{
		var emptyCustomsCodeInfo = OrgHeaderExtension.GetCustomsCodeInfo(null, null);
		CombineAssertions("Empty", () =>
		{
			AssertEquals(ZString.Empty, emptyCustomsCodeInfo.Id);
			AssertEquals(ZString.Empty, emptyCustomsCodeInfo.IdCountryCode);
		});
	}

	public void TestGetCustomsCodeInfoForNaturalPerson()
	{
		var address = organization.Addresses.AddNew();
		var customsCode = organization.CustomsCodes.AddNew();

		address.OA_RN_NKCountryCode = "DE";
		organization.OH_Category = "NAT";
		customsCode.OK_CodeType = "EOR";
		customsCode.OK_RN_NKCodeCountry = "DE";
		customsCode.OK_CustomsRegNo = "385040449";
		var codCusCode = organization.CustomsCodes.AddNew();
		codCusCode.OK_CodeType = "COD";
		codCusCode.OK_RN_NKCodeCountry = "IT";
		codCusCode.OK_CustomsRegNo = "123456789";

		var customsCodeInfo = OrgHeaderExtension.GetCustomsCodeInfo(organization, address.Country);
		CombineAssertions("NAT - EOR", () =>
		{
			AssertEquals("DE", customsCodeInfo.IdCountryCode);
			AssertEquals("385040449", customsCodeInfo.Id);
		});

		organization.CustomsCodes.RemoveAndDelete(customsCode);
		customsCodeInfo = OrgHeaderExtension.GetCustomsCodeInfo(organization, address.Country);
		CombineAssertions("NAT - COD", () =>
		{
			AssertEquals("IT", customsCodeInfo.IdCountryCode);
			AssertEquals("123456789", customsCodeInfo.Id);
		});

		organization.CustomsCodes.RemoveAndDelete(codCusCode);
		customsCodeInfo = OrgHeaderExtension.GetCustomsCodeInfo(organization, address.Country);
		CombineAssertions("NAT - No CusCode", () =>
		{
			AssertEquals("DE", customsCodeInfo.IdCountryCode);
			AssertEquals("0", customsCodeInfo.Id);
		});

		customsCodeInfo = OrgHeaderExtension.GetCustomsCodeInfo(organization, address.Country, zeroIfHasNotValidCustomsCode: false);
		CombineAssertions("NAT - No Code, No Zero", () =>
		{
			AssertEquals("", customsCodeInfo.IdCountryCode);
			AssertEquals("", customsCodeInfo.Id);
		});
	}

	public void TestGetCustomsCodeInfoForBusinessOrganisation()
	{
		var address = organization.Addresses.AddNew();
		var customsCode = organization.CustomsCodes.AddNew();

		address.OA_RN_NKCountryCode = "DE";
		organization.OH_Category = "BUS";
		customsCode.OK_CodeType = "EOR";
		customsCode.OK_RN_NKCodeCountry = "DE";
		customsCode.OK_CustomsRegNo = "385040449";
		var ivaCusCode = organization.CustomsCodes.AddNew();
		ivaCusCode.OK_CodeType = "IVA";
		ivaCusCode.OK_RN_NKCodeCountry = "IT";
		ivaCusCode.OK_CustomsRegNo = "123456789";

		var customsCodeInfo = OrgHeaderExtension.GetCustomsCodeInfo(organization, address.Country);
		CombineAssertions("BUS - EOR", () =>
		{
			AssertEquals("DE", customsCodeInfo.IdCountryCode);
			AssertEquals("385040449", customsCodeInfo.Id);
		});

		organization.CustomsCodes.RemoveAndDelete(customsCode);
		customsCodeInfo = OrgHeaderExtension.GetCustomsCodeInfo(organization, address.Country);
		CombineAssertions("BUS - IVA", () =>
		{
			AssertEquals("IT", customsCodeInfo.IdCountryCode);
			AssertEquals("123456789", customsCodeInfo.Id);
		});

		organization.CustomsCodes.RemoveAndDelete(ivaCusCode);
		customsCodeInfo = OrgHeaderExtension.GetCustomsCodeInfo(organization, address.Country);
		CombineAssertions("BUS - No CusCode", () =>
		{
			AssertEquals("DE", customsCodeInfo.IdCountryCode);
			AssertEquals("0", customsCodeInfo.Id);
		});

		customsCodeInfo = OrgHeaderExtension.GetCustomsCodeInfo(organization, address.Country, zeroIfHasNotValidCustomsCode: false);
		CombineAssertions("BUS - No CusCode, No Zero", () =>
		{
			AssertEquals("", customsCodeInfo.IdCountryCode);
			AssertEquals("", customsCodeInfo.Id);
		});
	}

	public void TestGetDefermentApprovalNumberList()
	{
		AssertExceptionThrown<ArgumentNullException>("organization is required", () => OrgHeaderExtension.GetDefermentApprovalNumberList(null));

		var defermentApprovalNumberList = organization.GetDefermentApprovalNumberList();
		AssertEquals("No items", ZString.Empty, defermentApprovalNumberList.CodesAsString);

		organization.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "XXX", Core.Constants.CountryCodes.Italy);
		var cusCodeDAT = organization.CustomsCodes.AddNew(ItalyOrgCusCodeInfo.OrgCusCodes.DefermentApprovaNumberForTrieste, "YYY", Core.Constants.CountryCodes.Italy);
		defermentApprovalNumberList = organization.GetDefermentApprovalNumberList();
		AssertEquals("Two items", "XXX, YYY", defermentApprovalNumberList.CodesAsString);

		organization.CustomsCodes.RemoveAndDelete(cusCodeDAT);
		defermentApprovalNumberList = organization.GetDefermentApprovalNumberList();
		AssertEquals("Single item found", "XXX", defermentApprovalNumberList.CodesAsString);
	}

	public void TestGetTcuCode()
	{
		CombineAssertions(() =>
		{
			AssertEquals("When called on organization without a TCU code, GetTcuCode return value", ZString.Empty, organization.GetTcuCode());
			AssertEquals("When called on null OrgHeader, GetTcuCode return value", ZString.Empty, (null as OrgHeader).GetTcuCode());
		});

		organization.CustomsCodes.AddNew("MSC", "385040455", "IT");
		organization.CustomsCodes.AddNew("EOR", "385040450", "IT");
		AssertEquals("When organization does not have TCU code, GetTcuCode return value", ZString.Empty, organization.GetTcuCode());

		organization.CustomsCodes.AddNew("TCU", "TCU_CODE", "IT");
		AssertEquals("When organization has the TCU code, GetTcuCode return value", "ITTCU_CODE", organization.GetTcuCode());
	}

	protected override void SetUp()
	{
		base.SetUp();
		organization = Factory.NewWithValidTestData<OrgHeader>();
	}

	OrgHeader organization;
}
