using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public static class OrgHeaderExtensions
{
	public static ZString GetUIDNumber(this OrgHeader organization)
	{
		return FormatCHMod11Number(organization.GetCHCustomsRegNo(OrgCusCode.SwissCodeTypes.UID));
	}

	public static ZString GetVATNumber(this OrgHeader organization)
	{
		return FormatCHMod11Number(organization.GetCHCustomsRegNo(OrgCusCode.CodeTypes.VATCode));
	}

	public static ZString GetAEONumber(this OrgHeader organization)
	{
		return organization.GetCHCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator);
	}

	public static ZString GetBIDNumber(this OrgHeader organization)
	{
		return organization.GetCHCustomsRegNo(OrgCusCode.SwissCodeTypes.BID);
	}

	public static ZString GetCHCustomsRegNo(this OrgHeader organization, ZString code, bool ignoreIssuingCountry = false, ZGuid addressPK = default)
	{
		var number = organization?.GetCustomsRegNoAndCountry(code, addressPK).Number ?? ZString.Empty;

		if (ignoreIssuingCountry && number.IsEmpty)
		{
			number = organization?.CustomsCodes.GetCustomsRegNo(code, ZString.Empty, addressPK) ?? ZString.Empty;
		}
		return number;
	}

	static (ZString Number, ZString CountryCode) GetCustomsRegNoAndCountry(this OrgHeader organization, ZString code, ZGuid addressPK = default)
	{
		var issuingCountry = organization != null && (code == OrgCusCode.CodeTypes.VATCode || code == OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator) ? organization.CountryCode : new ZString(Core.Constants.CountryCodes.Switzerland);
		var number = organization?.CustomsCodes.GetCustomsRegNo(code, issuingCountry, addressPK) ?? ZString.Empty;
		return (number, issuingCountry);
	}

	public static ZString GetCustomsRegNo(this OrgHeader organization, ZString code, ZString country, ZGuid addressPK = default)
	{
		return organization?.CustomsCodes.GetCustomsRegNo(code, country, addressPK) ?? ZString.Empty;
	}

	public static OrgCusCode[] GetCHCustomsRegNoList(this OrgHeader organization, ZString codeType) => organization.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(codeType);

	public static ZString GetIdentificationNumberForCH(this OrgHeader organization, bool isMultipleDUNEnabled = true, ZGuid addressPK = default)
	{
		var identificationNumber = ZString.Empty;
		if (organization != null)
		{
			identificationNumber = organization.GetCHCustomsRegNo(OrgCusCode.SwissCodeTypes.BID);
			if (identificationNumber.IsEmpty)
			{
				identificationNumber = organization.GetCHCustomsRegNo(OrgCusCode.SwissCodeTypes.UID);
			}
			if (identificationNumber.IsEmpty)
			{
				var dunList = organization.GetCHCustomsRegNoList(OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
				if ((isMultipleDUNEnabled && !dunList.IsNullOrEmpty()) || dunList.Length == 1)
				{
					identificationNumber = (dunList.FirstOrDefault(d => d.OK_OA_PremisesAddress == addressPK) ?? dunList.First()).OK_CustomsRegNo;
				}
			}
		}
		return identificationNumber;
	}

	public static ZString GetIdentificationNumberForEU(this OrgHeader organization)
	{
		var identificationNumber = ZString.Empty;
		if (organization != null)
		{
			identificationNumber = organization.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, ZString.Empty);
			if (identificationNumber.IsEmpty)
			{
				identificationNumber = organization.GetCustomsRegNo(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, ZString.Empty);
			}
		}
		return identificationNumber;
	}

	static ZString FormatCHMod11Number(ZString customsCode)
	{
		var result = customsCode.KeepAlphanumericCharacters();
		if (!result.IsEmpty)
		{
			if (result.StartsWith("E"))
			{
				result = Core.Constants.CountryCodes.Switzerland + result;
			}
		}
		return result;
	}

	public static bool HasValidNumber(this OrgHeader organization, ZString code)
	{
		var (number, country) = organization?.GetCustomsRegNoAndCountry(code) ?? (ZString.Empty, ZString.Empty);

		var isValid = !number.IsEmpty;
		if (isValid)
		{
			switch (code)
			{
				case OrgCusCode.SwissCodeTypes.UID:
					isValid = CHMod11Validator.IsValid(number);
					break;
				case OrgCusCode.CodeTypes.VATCode:
					isValid = country == Core.Constants.CountryCodes.Liechtenstein
									? LIRegistrationNumberValidator.IsValidVATNumber(number) : CHMod11Validator.IsValid(number);
					break;
			}
		}
		return isValid;
	}

	public static bool IsPrivatePerson(this OrgHeader organization) => organization.OH_Category == OrgConstants.Category.NaturalPersonIndividual;
}
