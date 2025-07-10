using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class GlbCompanyTokenCredentialsValidation : GlbExternalPasswordValidation
{
	public GlbCompanyTokenCredentialsValidation(GlbCompanyTokenCredentials parent)
		: base(parent)
	{
	}

	protected new GlbCompanyTokenCredentials Parent => (GlbCompanyTokenCredentials)base.Parent;

	protected override void CheckGP_Certificate()
	{
		base.CheckGP_Certificate();

		MandatoryValidation.CheckEntered(Parent.GP_CertificateInfo);
	}

	protected override void CheckGP_UserID()
	{
		base.CheckGP_UserID();

		MandatoryValidation.CheckEntered(Parent.GP_UserIDInfo);
	}

	protected override void CheckCurrentDecryptedPassword()
	{
		base.CheckCurrentDecryptedPassword();

		MandatoryValidation.CheckEntered(Parent.CurrentDecryptedPasswordInfo);
	}
}
