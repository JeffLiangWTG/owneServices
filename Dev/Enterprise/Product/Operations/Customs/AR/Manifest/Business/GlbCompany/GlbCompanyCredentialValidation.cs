using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class GlbCompanyCredentialValidation : GlbExternalPasswordWithCertificateValidation
	{
		public GlbCompanyCredentialValidation(GlbCompanyCredential parent)
			: base(parent)
		{
		}

		protected new GlbCompanyCredential Parent => (GlbCompanyCredential)base.Parent;

		protected override void CheckCurrentDecryptedCertificatePassphrase()
		{
			base.CheckCurrentDecryptedCertificatePassphrase();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.CurrentDecryptedCertificatePassphraseInfo);
		}

		protected override bool IsCertificateMandatory => false;
		protected override bool IsCurrentDecryptedCertificatePassphraseMandatory => false;
	}
}
