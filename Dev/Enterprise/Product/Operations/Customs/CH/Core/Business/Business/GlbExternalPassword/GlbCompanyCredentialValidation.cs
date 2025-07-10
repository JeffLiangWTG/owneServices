using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class GlbCompanyCredentialValidation : GlbExternalPasswordWithCertificateValidation
{
	public GlbCompanyCredentialValidation(GlbCompanyCredential parent) : base(parent)
	{
	}

	const int DateThreshold = 30;

	protected override void CheckCurrentDecryptedCertificatePassphrase()
	{
		base.CheckCurrentDecryptedCertificatePassphrase();

		var certificate = Parent.GP_Certificate;
		var password = Parent.CurrentDecryptedCertificatePassphrase;

		if (!certificate.IsEmpty)
		{
			if (password.IsEmpty)
			{
				Parent.CurrentDecryptedCertificatePassphraseInfo.AddError(Res.GetString("0EF8758B-9FFD-4D4F-8FDD-668719EEFED7", "Please enter a Certificate Password."));
			}

			else if (!IsValidPassword)
			{
				Parent.CurrentDecryptedCertificatePassphraseInfo.AddError(Res.GetString("E8A65DFE-D6E9-4F80-AE0F-C71A3AB694E5", "The Certificate or accompanying password is invalid."));
			}
		}
	}

	protected override void CheckGP_Certificate()
	{
		base.CheckGP_Certificate();

		if (!Parent.CurrentDecryptedCertificatePassphrase.IsEmpty && Parent.GP_Certificate.IsEmpty)
		{
			Parent.GP_CertificateInfo.AddError(Res.GetString("C3FE1EFA-54DC-451D-9A0F-D70E1BF0913F", "Please enter a Certificate."));
		}
	}

	bool IsValidPassword => Parent.GP_PasswordStatus == PasswordStatusList.Codes.Valid;

	protected override void CheckGP_ExpiryDate()
	{
		base.CheckGP_ExpiryDate();
		var issueDate = Parent.GP_IssueDate;
		var expiryDate = Parent.GP_ExpiryDate;

		if (!issueDate.IsEmpty && ZDateTime.Now < issueDate)
		{
			Parent.GP_ExpiryDateInfo.AddWarning(Res.GetString("5C9E9108-7203-4E2A-8636-BD2DEF52C726", "The certificate is not valid yet."));
		}

		if (!expiryDate.IsEmpty && expiryDate < ZDateTime.Now)
		{
			Parent.GP_ExpiryDateInfo.AddWarning(Res.GetString("4310686B-4777-4761-BD6B-0062D67D6CAD", "The certificate has expired."));
		}
		else if (!(expiryDate.IsEmpty || !expiryDate.IsValid) && (expiryDate - ZDateTime.Now).TotalDays < DateThreshold)
		{
			Parent.GP_ExpiryDateInfo.AddWarning(Res.GetString("8D5DA988-40E3-44C4-B76D-316D015DA52C", "The certificate will shortly expire."));
		}
	}

	protected override void CheckGP_ExpiryDateIsValidZDateTimeRange()
	{
	}

	protected new GlbCompanyCredential Parent => (GlbCompanyCredential)base.Parent;
	protected override bool IsCertificateMandatory => false;
	protected override bool IsCurrentDecryptedCertificatePassphraseMandatory => false;
}
