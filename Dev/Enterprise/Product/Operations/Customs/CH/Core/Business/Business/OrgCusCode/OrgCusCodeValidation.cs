using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

internal class OrgCusCodeValidation : MasterFiles.Business.OrgCusCodeValidation, Integration.Customs.CH.IOrgCusCodeValidation
{
	public OrgCusCodeValidation(AutoOrgCusCode parent) : base(parent)
	{
	}

	protected override void CheckOK_CustomsRegNo()
	{
		base.CheckOK_CustomsRegNo();

		switch (Parent.OK_RN_NKCodeCountry)
		{
			case Core.Constants.CountryCodes.Switzerland:
				switch (Parent.OK_CodeType)
				{
					case OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator:
						if (!Regex.IsMatch(Parent.OK_CustomsRegNo, "^[0-9]{7}$", RegexOptions.IgnoreCase))
						{
							Parent.OK_CustomsRegNoInfo.AddError(Res.GetString("21ca3bef-e567-4310-8eb0-57cf611f66a7", "Record only the seven-digit numeric part of the Swiss AEO number (nnnnnnn).", Parent.OK_RN_NKCodeCountry));
						}
						break;
					case OrgCusCode.SwissCodeTypes.UID:
					case OrgCusCode.CodeTypes.VATCode:
						new CHMod11Validator().Validate(Parent.OK_CodeType, Parent.OK_CustomsRegNoInfo);
						break;
					case OrgCusCode.SwissCodeTypes.CAD:
					case OrgCusCode.SwissCodeTypes.CAV:
						if (!Regex.IsMatch(Parent.OK_CustomsRegNo, "^[0-9]{5,8}$", RegexOptions.IgnoreCase))
						{
							Parent.OK_CustomsRegNoInfo.AddError(Res.GetString("0F5CF16A-609C-4102-86B2-BA01CC89BE40", "{0} {1} number should consist of 5 to 8 numeric digits.", Parent.OK_RN_NKCodeCountry, Parent.OK_CodeType));
						}
						break;
					case OrgCusCode.SwissCodeTypes.CTP:
						if (!Regex.IsMatch(Parent.OK_CustomsRegNo, "^[1-9][0-9]{0,5}$", RegexOptions.IgnoreCase))
						{
							Parent.OK_CustomsRegNoInfo.AddWarning(Res.GetString("4A2B07CD-A8DB-4AB3-B129-1A5483685952", "{0} {1} numeric value should be between 1 and 999999.", Parent.OK_RN_NKCodeCountry, Parent.OK_CodeType));
						}
						break;
					case OrgCusCode.SwissCodeTypes.BID:
						if (!Regex.IsMatch(Parent.OK_CustomsRegNo, "^1[0-9]{9}$", RegexOptions.IgnoreCase))
						{
							Parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("4C023F9B-38BF-44E8-843D-49C256A3D7DF", "You have not entered a valid Business Partner ID."));
						}
						break;
				}
				break;
			case Core.Constants.CountryCodes.Liechtenstein:
				switch (Parent.OK_CodeType)
				{
					case OrgCusCode.CodeTypes.VATCode:
						if (!LIRegistrationNumberValidator.IsValidVATNumber(Parent.OK_CustomsRegNo))
						{
							Parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("F4F4F6E7-3862-4930-BB2E-CA9EFBA29E96", "{0} {1} number should consist of 5 numeric digits.", Parent.OK_RN_NKCodeCountry, Parent.OK_CodeType));
						}
						break;
				}
				break;
		}
	}
}
