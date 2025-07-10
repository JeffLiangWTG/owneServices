using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class GlbCompanyCredentialValidation : MasterFiles.Business.GlbExternalPasswordWithCertificateValidation
	{
		public GlbCompanyCredentialValidation(GlbCompanyCredential parent)
			: base(parent)
		{
		}

		protected override void CheckGP_MailBoxID()
		{
			base.CheckGP_MailBoxID();
			if (IsValidationRequired)
			{
				MandatoryValidation.CheckEntered(Parent.GP_MailBoxIDInfo);
			}
		}

		protected override void CheckGP_Name()
		{
			base.CheckGP_Name();
			if (IsValidationRequired)
			{
				MandatoryValidation.CheckEntered(Parent.GP_NameInfo);
			}
		}

		protected override bool IsCertificateMandatory => false;
		protected override bool IsCurrentDecryptedCertificatePassphraseMandatory => IsValidationRequired;
		bool IsValidationRequired => Parent.GP_Certificate != null && !Parent.GP_Certificate.IsEmpty;
	}
}
