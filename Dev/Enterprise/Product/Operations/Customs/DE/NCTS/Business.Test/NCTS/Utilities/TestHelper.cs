using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	internal static class TestHelper
	{
		internal static OrgAddress GetTestOrgAddress(BusinessObjectFactory factory, string companyName)
		{
			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_FullName = companyName;
			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_Address1 = "Address part 1";
			orgAddress.OA_Address2 = "Address part 2";
			orgAddress.OA_PostCode = "41516";
			orgAddress.OA_City = "Entenhausen";
			orgAddress.OA_RN_NKCountryCode = "DE";
			return orgAddress;
		}
	}
}
