using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public static class EnvironmentHelper
{
	public static ZString CheckMessageSendingEnvironmentForEdec()
	{
		if (GlbCompany.CurrentCompany.GC_CustomsRegistrationNo.IsEmpty)
		{
			return Res.GetString("22d8bb73-32ac-4a3a-ac62-21a65dac8d22", "Customs Registration Number is not configured for the current company. Please contact your system administrator.");
		}
		return ZString.Empty;
	}

	public static ZString CheckMessageSendingEnvironmentForPassarAndChartera()
	{
		if (EnvironmentHelper.GetBusinessPartnerId().IsEmpty)
		{
			return Res.GetString("460E8FC6-D716-45DE-B8FB-EB655DD22986", "Business Partner ID (BID) is not configured for the company or branch. Please contact your system administrator.");
		}

		var companyWrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany);
		if (!companyWrapper.TokenCredentialsEnabled)
		{
			return Res.GetString("44FA8F80-E933-42B1-877B-7BA43CF2EB86", "Communication Tokens are not configured for the company. Please contact your system administrator.");
		}

		return ZString.Empty;
	}

	public static ZString GetBusinessPartnerId()
	{
		var customsRegNo = GlbBranch.CurrentBranch.OrgProxy.GetCHCustomsRegNo(OrgCusCode.SwissCodeTypes.BID);
		if (customsRegNo.IsEmpty)
		{
			customsRegNo = GlbCompany.CurrentCompany.OrgProxy.GetCHCustomsRegNo(OrgCusCode.SwissCodeTypes.BID);
		}

		return customsRegNo;
	}
}
