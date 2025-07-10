using System;
using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public sealed class GlbCertificateProvider : IGlbCertificateProvider
{
	public GlbCertificateProvider(GlbStaff glbStaff = null)
	{
		this.glbStaff = glbStaff ?? GlbStaff.CurrentUser;
	}

	public IGlbMauExternalPassword GetMauCertificatePassword(string userId)
	{
		var currentCompanyWrapper = GlbCompanyWrapper.Get(GlbCompany.CurrentCompany);

		return currentCompanyWrapper
			.PasswordCollection.Cast<GlbMauExternalPassword>()
			.SingleOrDefault(x => x.GP_UserID == userId);
	}

	public ICryptokiGlbExternalPassword GetCryptokiCertificate()
	{
#if DEBUG
		if (glbStaff.IsSupportUserAndIsNotTestEnviroment())
		{
			return null;
		}
#endif
		var cryptokiCertificate = CurrentStaffWrapper.CryptokiCertificateCollection.GetCryptokiCertificate();
		return cryptokiCertificate ?? throw new InvalidOperationException("Cannot get the Cryptoki certificate for the current user");
	}

	public IAutomaticSignatureExternalPassword AutomaticSignaturePassword => automaticSignaturePassword ??= GetAutomaticSignaturePassword();
	IAutomaticSignatureExternalPassword automaticSignaturePassword;

	public bool HasValidAutomaticSignaturePassword => AutomaticSignaturePassword is not null && AutomaticSignaturePassword.IsConfigurationActive;

	#region Implementation

	IAutomaticSignatureExternalPassword GetAutomaticSignaturePassword()
		=> CurrentStaffWrapper.AutomaticSignaturePasswordCollection
			.Cast<IAutomaticSignatureExternalPassword>()
			.FirstOrDefault();

	GlbStaffWrapper CurrentStaffWrapper => currentStaffWrapper ??= GlbStaffWrapper.Get(glbStaff);
	GlbStaffWrapper currentStaffWrapper;

	readonly GlbStaff glbStaff;

	#endregion
}
