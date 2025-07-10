using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Testing
{
	public static class OrgHeaderExtensions
	{
		public static CusAuthorisationHeader SetupAuthorisationHeader(this OrgHeader header, string authType)
		{
			var cusAuthorisationHeader = header.Factory.NewWithValidTestData<CusAuthorisationHeader>();
			cusAuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusAuthorisationHeader.CPH_Type = authType;
			cusAuthorisationHeader.CPH_OH_PermitHolder = header.PK;
			cusAuthorisationHeader.CPH_OA_AppliesTo = header.Addresses.MainAddress.PK;

			return cusAuthorisationHeader;
		}

		public static OrgCusAccount SetupAccount(this OrgHeader header, ZString code, ZString type, ZString account, ZString issuer, ZString reportingPeriod, ZString representativeId)
		{
			var orgCusAccount = header.Factory.New<OrgCusAccount>();
			orgCusAccount.CZ_Code = code;
			orgCusAccount.CZ_Type = type;
			orgCusAccount.CZ_OH = header.PK;
			orgCusAccount.CZ_Account = account;
			orgCusAccount.CZ_Issuer = issuer;
			orgCusAccount.CZ_ReportingPeriod = reportingPeriod;
			orgCusAccount.CZ_RepresentativeID = representativeId;
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			return orgCusAccount;
		}

		public static OrgCusCode SetupCusCode(this OrgHeader header, ZString type, ZString code)
		{
			return header.CustomsCodes.AddNew(type, code, Core.Constants.CountryCodes.France);
		}
	}
}
