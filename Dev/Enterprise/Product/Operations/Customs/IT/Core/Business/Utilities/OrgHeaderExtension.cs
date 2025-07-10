using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public static class OrgHeaderExtension
{
	public static ZBool IsNaturalPersonIndividual(this OrgHeader organisation) => (organisation?.OH_Category ?? ZString.Empty) == OrgConstants.Category.NaturalPersonIndividual;

	public static string GetEoriOrTcuTraderNumber(this OrgAddress orgAddress)
	{
		CargoWise.Customs.IT.MessageContracts.ITrader trader = new MessageSending.AidaXml.Export.EoriOrTcuTraderWrapper(orgAddress);
		return trader.IdentificationNumber;
	}

	public static string GetEoriOrTcuTraderNumber(this JobDocAddress docAddress) => GetEoriOrTcuTraderNumber(docAddress?.Address);

	public static ZString GetFiscalCode(this OrgHeader organisation) => GetCustomsRegNo(organisation, ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale);

	public static ZString GetEoriCode(this OrgHeader organisation, bool countryCode = false)
	{
		if (countryCode)
		{
			var eoriCusCode = GetCusCode(organisation, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
			return eoriCusCode != null
				? new ZString(eoriCusCode.OK_RN_NKCodeCountry + eoriCusCode.OK_CustomsRegNo)
				: ZString.Empty;
		}
		return GetCustomsRegNo(organisation, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
	}

	public static ZString GetTcuCode(this OrgHeader organisation)
	{
		var tcuCusCode = GetCusCode(organisation, OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU);
		return tcuCusCode != null
			? new ZString(tcuCusCode.OK_RN_NKCodeCountry + tcuCusCode.OK_CustomsRegNo)
			: ZString.Empty;
	}

	public static ZString GetVatCode(this OrgHeader organisation) => GetCustomsRegNo(organisation, OrgCusCode.CodeTypes.IVA);

	public static ZString GetAeoCode(this OrgHeader organisation)
	{
		return organisation?.CustomsCodes
			?.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator)
			?.FirstOrDefault(x => ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnion(x.OK_RN_NKCodeCountry))
			?.OK_CustomsRegNo ?? ZString.Empty;
	}

	public static bool HasAeoFSCode(this OrgHeader organisation)
	{
		const string AuthorizedEconomicOperatorCustomsSimplificationsSecurityAndSafety = "F";
		const string AuthorizedEconomicOperatorSecurityAndSafety = "S";

		var aeoCode = organisation?.GetAeoCode().Left(4) ?? ZString.Empty;
		return aeoCode.EndsWith(AuthorizedEconomicOperatorCustomsSimplificationsSecurityAndSafety) || aeoCode.EndsWith(AuthorizedEconomicOperatorSecurityAndSafety);
	}

	public static ZString GetDefermentApprovalNumberCode(this OrgHeader organisation) => GetCustomsRegNo(organisation, OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber);

	public static ZString GetDefermentApprovalNumberForTriesteCode(this OrgHeader organisation) => GetCustomsRegNo(organisation, ItalyOrgCusCodeInfo.OrgCusCodes.DefermentApprovaNumberForTrieste);

	public static ZString GetRexCode(this OrgHeader organisation) => GetCustomsRegNo(organisation, OrgCusCode.EuropeanUnionSharedCodeTypes.RegisteredExporterNumber);

	static ZString GetCustomsRegNo(OrgHeader organisation, ZString codeType) => GetCusCode(organisation, codeType)?.OK_CustomsRegNo ?? ZString.Empty;

	public static OrgCusCode GetCusCode(this OrgHeader organisation, ZString codeType)
	{
		var customsCodes = organisation?.CustomsCodes;
		return codeType == ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale
			? customsCodes?.GetOrgCusCodeObjectForCodeAndCountry(ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale, Core.Constants.CountryCodes.Italy)
			: customsCodes?.GetOrgCusCodesForCodeIgnoringCountry(codeType)?.Cast<OrgCusCode>()?.FirstOrDefault();
	}

	public static ZString[] GetCustomsCodeTypeListRequiredForEUTrader(this OrgHeader organisation, bool ignoreEoriCusCode = false)
	{
		Argument.NotNull(organisation, nameof(organisation));

		var requiredCusCodes = new List<ZString>();
		if (!ignoreEoriCusCode)
		{
			requiredCusCodes.Add(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
		}
		requiredCusCodes.Add(organisation.IsNaturalPersonIndividual() ? ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale : OrgCusCode.CodeTypes.IVA);
		return requiredCusCodes.ToArray();
	}

	public static OrgCusCode GetFirstCusCodeMatchingTypeInOrder(this OrgHeader organisation, params ZString[] codeTypes)
	{
		Argument.NotNull(organisation, nameof(organisation));
		foreach (var type in codeTypes)
		{
			var orgCusCode = organisation.GetCusCode(type);
			if (orgCusCode != null)
			{
				return orgCusCode;
			}
		}
		return null;
	}

	public static ZString GetAddressAsASingleLineForCustomsMessage(this IDocAddress docAddress) => FormattableString.Invariant($"{docAddress.E2_Address1} {docAddress.E2_Address2}").Trim();

	public static IEnumerable<ZString> GetControlledPremisesIDs(this OrgHeader organisation) => organisation?.CustomsCodes?.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.CodeTypes.ControlledPremisesID).Select(x => x.OK_CustomsRegNo) ?? Enumerable.Empty<ZString>();

	public static bool HasLocAuthorisation(this OrgHeader organisation, string ccpValue)
	{
		return organisation != null && CusAuthorisationHeader.Loader.HolderHasValidAuthorisationWithSpecificRule(organisation.Factory, organisation.PK, Core.Constants.CountryCodes.Italy, ZDateTime.Today, CusAuthorisationRuleTypeList.Codes.Location, CcpHelper.GetWarehouseCodeFromCcp(ccpValue));
	}

	public static OrgHeaderCustomsCodeInfo GetCustomsCodeInfo(this OrgHeader organisation, RefCountry addressCountry, bool zeroIfHasNotValidCustomsCode = true)
	{
		const string zero = "0";

		if (organisation == null || addressCountry == null)
		{
			return GetEmptyCustomsCodeInfo();
		}

		var requiredCodeTypeList = organisation.GetCustomsCodeTypeListRequiredForEUTrader();
		var customsCode = organisation.GetFirstCusCodeMatchingTypeInOrder(requiredCodeTypeList);

		return customsCode != null
			? new OrgHeaderCustomsCodeInfo(customsCode.OK_RN_NKCodeCountry, customsCode.OK_CustomsRegNo)
			: GetZeroOrEmptyCustomsCode();

		OrgHeaderCustomsCodeInfo GetZeroOrEmptyCustomsCode() => zeroIfHasNotValidCustomsCode
				? new OrgHeaderCustomsCodeInfo(addressCountry.Code, zero)
				: GetEmptyCustomsCodeInfo();

		OrgHeaderCustomsCodeInfo GetEmptyCustomsCodeInfo() => new OrgHeaderCustomsCodeInfo(ZString.Empty, ZString.Empty);
	}

	public static CodeDescriptionPairList GetDefermentApprovalNumberList(this OrgHeader organization)
	{
		Argument.NotNull(organization, nameof(organization));

		return new CodeDescriptionPairList()
			.AddPairIfNotEmpty(organization.GetDefermentApprovalNumberCode())
			.AddPairIfNotEmpty(organization.GetDefermentApprovalNumberForTriesteCode());
	}

	static CodeDescriptionPairList AddPairIfNotEmpty(this CodeDescriptionPairList codeDescriptionPairList, ZString value)
	{
		if (!value.IsEmpty)
		{
			codeDescriptionPairList.AddPair(value);
		}
		return codeDescriptionPairList;
	}
}
