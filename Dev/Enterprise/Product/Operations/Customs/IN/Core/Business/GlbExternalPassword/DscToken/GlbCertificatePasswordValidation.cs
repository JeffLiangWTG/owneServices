using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Business;

public class GlbCertificatePasswordValidation : MasterFiles.Business.GlbExternalPasswordValidation
{
	public GlbCertificatePasswordValidation(GlbCertificatePassword parent) : base(parent)
	{
	}

	protected override void CheckGP_CertificateAuthority()
	{
		base.CheckGP_CertificateAuthority();
		var parent = Parent;
		var targetInfo = parent.GP_CertificateAuthorityInfo;
		ListValidation.ErrorIfInvalidCode(targetInfo);
		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(targetInfo, parent.GP_NameInfo, MandatoryValidation.YouHaveNotEnteredMessage(targetInfo.HumanReadableName));
	}

	protected override void CheckGP_Name()
	{
		base.CheckGP_Name();
		var parent = Parent;
		var targetInfo = parent.GP_NameInfo;
		ListValidation.ErrorIfInvalidCode(targetInfo);
		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(targetInfo, parent.GP_CertificateAuthorityInfo, MandatoryValidation.YouHaveNotEnteredMessage(targetInfo.HumanReadableName));
	}

	protected override void CheckGP_CertificateSerialNumber()
	{
		base.CheckGP_CertificateSerialNumber();
		var parent = Parent;
		var targetInfo = parent.GP_CertificateSerialNumberInfo;
		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(targetInfo, parent.GP_NameInfo, MandatoryValidation.YouHaveNotEnteredMessage(targetInfo.HumanReadableName));
		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(targetInfo, parent.GP_CertificateAuthorityInfo, MandatoryValidation.YouHaveNotEnteredMessage(targetInfo.HumanReadableName));
	}
}
