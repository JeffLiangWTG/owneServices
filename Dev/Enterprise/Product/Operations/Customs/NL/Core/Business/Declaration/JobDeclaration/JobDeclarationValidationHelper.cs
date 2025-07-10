using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business;

public static class JobDeclarationValidationHelper
{
	public static bool IsAddressFilled(OrgAddress orgAddress) => orgAddress == null || (!orgAddress.CompanyName.IsEmpty && !orgAddress.OA_Address1.IsEmpty && !orgAddress.OA_RN_NKCountryCode.IsEmpty && !orgAddress.OA_City.IsEmpty && !orgAddress.OA_PostCode.IsEmpty);
}
