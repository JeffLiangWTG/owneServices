using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.Business
{
	public static class ValidationHelper
	{
		public static bool HasEORIOrTCUCustomsCode(OrgHeader org, ZString country)
		{
			return
				org != null
				&& org.CustomsCodes.OfType<OrgCusCode>()
					.Any(code =>
						code.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori
						|| (code.OK_CodeType == OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU && code.OK_RN_NKCodeCountry == country));
		}
	}
}
