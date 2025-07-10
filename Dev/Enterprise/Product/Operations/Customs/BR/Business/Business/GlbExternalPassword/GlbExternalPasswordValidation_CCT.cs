using System.Linq;
using System.Security.Cryptography;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class GlbExternalPasswordValidation_CCT : GlbExternalPasswordWithCertificateValidation
	{
		public GlbExternalPasswordValidation_CCT(GlbExternalPassword_CCT parent)
			: base(parent)
		{
		}

		protected new GlbExternalPassword_CCT Parent => (GlbExternalPassword_CCT)base.Parent;

		protected override void CheckGP_ExpiryDate()
		{
			base.CheckGP_ExpiryDate();
			var expiryDate = Parent.GP_ExpiryDate;
			if (expiryDate >= ZDateTime.Now && expiryDate < ZDateTime.Now.AddMonths(1))
			{
				Parent.GP_ExpiryDateInfo.AddWarning(Res.GetString("7c93b5fc-34fc-4890-a77e-cf0e9cf2f584", "This certificate will expire soon – the expiry date is within one month."));
			}
		}

		protected override void AddGP_ExpiryDateExpiredNotification()
		{
			Parent.GP_ExpiryDateInfo.AddError(Res.GetString("a961e279-7fcf-4a65-aad7-7dcf09462f46", "This certificate is not valid – the expiry date is in the past."));
		}

		protected override void CheckGP_IssueDateIsValidZDateTimeRange() { }

		protected override void CheckGP_ExpiryDateIsValidZDateTimeRange() { }

		protected override bool IsCurrentDecryptedCertificatePassphraseMandatory => Parent.GP_Certificate != null && !Parent.GP_Certificate.IsEmpty;
		protected override bool IsCertificateMandatory => Parent.EventSubscriptions.Count > 0;

		protected override void CheckGP_Certificate()
		{
			base.CheckGP_Certificate();
			if (!Parent.GP_Certificate.IsEmpty && Parent.GP_CertificateInfo.HasChanges && !Parent.GP_CertificateInfo.OriginalValue.IsEmpty && (ZDateTime)Parent.GP_ExpiryDateInfo.OriginalValue > ZDateTime.Now && Parent.EventSubscriptions.Where(x => !x.GP_MailBoxID.IsEmpty).Any())
			{
				Parent.GP_CertificateInfo.AddError(Res.GetString("28ABBD02-ACB9-4E4E-9672-55853F32ECD9", "Please cancel the active Subscription(s) before replacing the Certificate."));
			}

			try
			{
				var certificateChainErrors = CertificateValidationHelper.GetInvalidCertificateChainErrors(Parent.GP_Certificate, Parent.CurrentDecryptedCertificatePassphrase);
				if (!string.IsNullOrEmpty(certificateChainErrors))
				{
					Parent.GP_CertificateInfo.AddError(certificateChainErrors);
				}
			}
			catch (CryptographicException)
			{
			}
		}

		public static void CheckValidCertificate(GlbExternalPassword_CCT certificate, ZPropertyInfo targetInfo)
		{
			if (!certificate?.IsValidCertificate ?? true)
			{
				targetInfo.AddMessageError(Res.GetString("47f63b17-af21-46f7-b8d2-e7597368dd8d", "Digital Certificate not informed or expired or invalid. Please press F3 in the field and enter a valid digital certificate in Credentials tab."));
			}
		}
	}
}
