using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AE.Manifest.Business;

static class ValidationHelper
{
	public static void CheckOrgContactInfo(OrgAddress orgAddress, ZPropertyInfo targetInfo)
	{
		if (orgAddress == null)
		{
			return;
		}

		if (orgAddress.OA_Phone.IsEmpty
			&& orgAddress.OA_Email.IsEmpty
			&& (orgAddress.Header?.MainWebURL.PU_URL ?? ZString.Empty).IsEmpty)
		{
			targetInfo.AddMessageError(Res.GetString("17463A84-C8F3-423E-9FC7-6D1F91B879C0"
				, "Organization requires a Phone Number, Email Address or Website URL"));
		}
	}

	public static void CheckAnyMPCICode(OrgAddress orgAddress, ZPropertyInfo targetInfo)
	{
		if (orgAddress == null)
		{
			return;
		}

		var orgHeaderAE_MPC = orgAddress.Header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber, Core.Constants.CountryCodes.UnitedArabEmirates) ?? ZString.Empty;
		if (orgHeaderAE_MPC.IsEmpty)
		{
			targetInfo.AddMessageError(Res.GetString("20387675-10C1-4837-BA9F-25C9A0BA0588"
								, "{0} Requires a 'MPCI Party Id (MPC)' Registration Code to be Configured", targetInfo.HumanReadableName.ToString()));
		}
	}
}
