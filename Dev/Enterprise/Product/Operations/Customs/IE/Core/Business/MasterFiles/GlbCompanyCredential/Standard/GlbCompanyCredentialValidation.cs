using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business
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
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CurrentDecryptedCertificatePassphraseInfo);
		}

		protected override void CheckGP_MailBoxID()
		{
			if (Parent.GP_PasswordStatus.EqualsIgnoringCase(PasswordStatusList.Codes.Valid))
			{
				var eori = Parent.GP_MailBoxID;
				if (eori.IsEmpty)
				{
					var info = Parent.GP_MailBoxIDInfo;
					info.AddError(MandatoryValidation.MustBeEnteredMessage(MandatoryValidation.GetErrorFieldFromProperyInfo(info)));
				}
				else if (!EuEoriProviderAndValidator.ValidEORIorTCUIFormat(eori, Parent.Factory))
				{
					Parent.GP_MailBoxIDInfo.AddMessageError(Res.GetString("{A273E229-DC6E-469C-8A50-2B2A0C2FB5FB}", "Please enter a valid EORI format: A member state or third country code [a2] plus a unique identifier [an..15]"));
				}
			}
		}

		protected override bool IsCertificateMandatory => false;
	}
}
