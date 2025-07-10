using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Testing;

public static class MessageProviderDataHelper
{
	public static void SetupOrgheaderAndAddress(OrgHeader header, OrgAddress address, string eori = null, string headerFullName = "BE Declarant")
	{
		header.OH_FullName = headerFullName;
		address.OA_Address1 = "1 test avenue";
		address.OA_Address2 = "";
		address.Postcode = "1200";
		address.City = "Brussle";
		address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;

		if (!string.IsNullOrEmpty(eori))
		{
			SetupEORI(address, eori);
		}
	}
	public static void SetupEORI(OrgAddress address, string eori)
	{
		SetupEORI(address.CustomsCodes.AddNew(), eori);
	}

	public static void SetupEORI(OrgHeader header, string eori)
	{
		SetupEORI(header.CustomsCodes.AddNew(), eori);
	}

	public static void SetupEORI(OrgHeader header, string eori, string codeType)
	{
		SetupEORI(header.CustomsCodes.AddNew(), eori, codeType);
	}

	static void SetupEORI(OrgCusCode code, string eori)
	{
		code.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
		code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Belgium;
		code.OK_CustomsRegNo = eori;
	}

	static void SetupEORI(OrgCusCode code, string eori, string codeType)
	{
		code.OK_CodeType = codeType;
		code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Belgium;
		code.OK_CustomsRegNo = eori;
	}
}
