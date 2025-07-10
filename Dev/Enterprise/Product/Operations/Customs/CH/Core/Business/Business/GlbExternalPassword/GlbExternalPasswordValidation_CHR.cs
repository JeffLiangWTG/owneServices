using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class GlbExternalPasswordValidation_CHR : GlbExternalPasswordValidation
{
	public GlbExternalPasswordValidation_CHR(GlbExternalPassword_CHR parent)
		: base(parent)
	{
	}

	protected new GlbExternalPassword_CHR Parent => (GlbExternalPassword_CHR)base.Parent;

	protected override void CheckGP_Certificate()
	{
		base.CheckGP_Certificate();

		MandatoryValidation.CheckEntered(Parent.GP_CertificateInfo);
	}
}
