using System;
using System.ComponentModel;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class GlbCompanyCredential : GlbExternalPasswordWithCertificate
	{
		public GlbCompanyCredential(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			actionOnSuccessfullSaving = () =>
			{
				if (!IsDeleted && !IsDeleting)
				{
					GP_PasswordStatus = GlbARExternalPassword.PasswordAwaitingCode;
				}
			};
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			GP_PasswordType = PasswordTypesList.Codes.ARB;
		}

		#region Overrided properties

		public override ZBlob GP_Certificate
		{
			get => base.GP_Certificate;
			set
			{
				var oldValue = GP_Certificate;
				base.GP_Certificate = value;
				if (!IsCopying && oldValue != GP_Certificate)
				{
					CalculateGP_CertificatePassphraseStatus();
				}
			}
		}

		[MaxLength(10)]
		public override ZString CurrentDecryptedCertificatePassphrase
		{
			get => base.CurrentDecryptedCertificatePassphrase;
			set
			{
				var oldValue = CurrentDecryptedCertificatePassphrase;
				base.CurrentDecryptedCertificatePassphrase = value;
				if (!IsCopying && oldValue != CurrentDecryptedCertificatePassphrase)
				{
					CalculateGP_CertificatePassphraseStatus();
				}
			}
		}

		[ReadOnly(true)]
		public override ZString GP_PasswordStatus { get => base.GP_PasswordStatus; set => base.GP_PasswordStatus = value; }

		#endregion

		#region Password status

		public ZString PasswordStatus => Lookups.PasswordStatusList.GetDescriptionFromCode(base.GP_PasswordStatus) ?? base.GP_PasswordStatus;

		protected override void ChangeStatusWhenUpdateCurrentPassword(ZString oldValue)
		{
			var newValue = GP_CurrentPassword;
			if (oldValue != newValue)
			{
				GP_PasswordStatus = GetCredentialStatus();
			}
		}

		#endregion

		protected override void ClearDataDefaultedFromCertificate()
		{
			GP_ExpiryDate = ZDateTime.Empty;
		}

		protected override void DefaultDataFromCertificate(X509Certificate2 certificate)
		{
			GP_ExpiryDate = certificate.NotAfter;
		}

		protected override GlbExternalPasswordValidation GetNewValidation() => new GlbCompanyCredentialValidation(this);

		public new GlbCompanyCredentialValidation Validation => (GlbCompanyCredentialValidation)GetNewValidation();

		public override CredentialRecipient CredentialRecipient => CredentialRecipient.DirectxT;

		public override ZString ConfigurationName => GlbARExternalPassword.ConfigurationName;

		protected override string InterchangeTypeForSending => GlbARExternalPassword.InterchangeTypeForSending;

		protected override object[] CreateCredentialItems()
		{
			var certificate = CredentialSender.CreateCertificate(GP_Certificate, CurrentDecryptedCertificatePassphrase);
			return new object[] { certificate };
		}

		protected override ZPropertyInfo[] CredentialApplicableInfos()
					=> new ZPropertyInfo[] { GP_CertificateInfo, GP_CertificatePassPhraseInfo };

		protected override bool ShouldSendDeleteCredential()
		{
			return !GP_CertificateInfo.OriginalValue.IsEmpty
				|| !GP_CertificatePassPhraseInfo.OriginalValue.IsEmpty;
		}

		protected override GlbExternalPasswordLookups GetNewLookups()
		{
			return new GlbCompanyCredentialLookups(this);
		}

		protected override void RegisterConfigurationForSending()
		{
			ExternalPasswordConfigurationToSender.RegisterForSending(Factory, ConfigurationName, Company, Group, Staff, InterchangeTypeForSending, CredentialRecipient, actionOnSuccessfullSaving);
		}

		readonly Action actionOnSuccessfullSaving;
	}
}
