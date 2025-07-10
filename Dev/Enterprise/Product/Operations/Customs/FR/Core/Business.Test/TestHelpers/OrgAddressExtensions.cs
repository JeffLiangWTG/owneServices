using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Testing
{
	public static class OrgAddressExtensions
	{
		public static CusAuthorisationHeader SetupAuthorisationHeader(this OrgAddress address, string authType)
		{
			var cusAuthorisationHeader = address.Factory.NewWithValidTestData<CusAuthorisationHeader>();
			cusAuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusAuthorisationHeader.CPH_Type = authType;
			cusAuthorisationHeader.CPH_OH_PermitHolder = address.Header?.PK ?? ZGuid.Empty;
			cusAuthorisationHeader.CPH_OA_AppliesTo = address.PK;

			return cusAuthorisationHeader;
		}
	}
}
