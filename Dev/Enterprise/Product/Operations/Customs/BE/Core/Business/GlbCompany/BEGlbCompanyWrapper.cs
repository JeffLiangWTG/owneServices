using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.BE;

namespace Enterprise.Customs.BE.Business;

public class BEGlbCompanyWrapper : GlbCompanyWrapper, IBEGlbCompanyWrapper
{
	protected BEGlbCompanyWrapper(GlbCompany company)
		: base(company)
	{
	}

	#region GlbExternalPassword

	[ChildEditable]
	public GlbExternalPassword_BEC Credential
	{
		get
		{
			if (credential == null)
			{
				credential = GetGlbExternalPasswordOrCreateNew<GlbExternalPassword_BEC>(PasswordTypesList.Codes.BEC);
				RegisterEditableChildObject(credential);
			}

			return credential;
		}
	}
	GlbExternalPassword_BEC credential;

	#endregion

	IGlbExternalPassword IBEGlbCompanyWrapper.BrokerageCredentials => Credential;

	public override bool IsValidWrapper => Company.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Belgium && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Belgium;
}
