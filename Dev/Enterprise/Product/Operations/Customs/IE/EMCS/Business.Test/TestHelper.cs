using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	public static class TestHelper
	{
		public static void ModifyOrgCusCode(this OrgCusCode orgCusCode, ZGuid orgHeaderPK, ZString cusCodeType, ZString countryCode, ZString cusCode, ZGuid premisesAddressPK = default)
		{
			orgCusCode.OK_OH = orgHeaderPK;
			orgCusCode.OK_RN_NKCodeCountry = countryCode;
			orgCusCode.OK_CodeType = cusCodeType;
			orgCusCode.OK_CustomsRegNo = cusCode;
			if (premisesAddressPK != ZGuid.Empty)
			{
				orgCusCode.OK_OA_PremisesAddress = premisesAddressPK;
			}
		}
	}
}
