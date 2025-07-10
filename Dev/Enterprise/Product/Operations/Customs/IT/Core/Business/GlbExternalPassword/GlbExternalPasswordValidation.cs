using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public abstract class GlbExternalPasswordValidation : GlbExternalPasswordWithCertificateValidation
{
	public GlbExternalPasswordValidation(GlbExternalPassword parent)
		: base(parent)
	{
	}

	protected sealed override void CheckGP_ExpiryDateIsValidZDateTimeRange()
	{
	}
}
