using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business;

public class CryptokiExternalPasswordValidation : MasterFiles.Business.GlbExternalPasswordValidation
{
	public CryptokiExternalPasswordValidation(CryptokiExternalPassword parent) : base(parent)
	{
	}

	protected new CryptokiExternalPassword Parent => (CryptokiExternalPassword)base.Parent;

	protected override void CheckGP_CertificateAuthority()
	{
		base.CheckGP_CertificateAuthority();

		ApplyMandatoryAndListValidations(Parent.GP_CertificateAuthorityInfo, ValidationCaptions.CryptokiExternalPassword.PleaseEnterCertificateAuthority);
	}

	protected override void CheckGP_Name()
	{
		base.CheckGP_Name();

		ApplyMandatoryAndListValidations(Parent.GP_NameInfo, ValidationCaptions.CryptokiExternalPassword.PleaseEnterChipset);
	}

	protected override void CheckGP_CertificateSerialNumber()
	{
		base.CheckGP_CertificateSerialNumber();

		if (Parent.GP_CertificateSerialNumber.IsEmpty)
		{
			Parent.GP_CertificateSerialNumberInfo.AddError(ValidationCaptions.CryptokiExternalPassword.PleaseEnterSerialNumber);
		}
	}

	void ApplyMandatoryAndListValidations(ZPropertyInfo propertyInfo, string mandatoryValidationErrorMessage)
	{
		if (propertyInfo.Value.IsEmpty)
		{
			propertyInfo.AddError(mandatoryValidationErrorMessage);
		}
		else
		{
			ListValidation.ErrorIfInvalidCode(propertyInfo);
		}
	}
}
