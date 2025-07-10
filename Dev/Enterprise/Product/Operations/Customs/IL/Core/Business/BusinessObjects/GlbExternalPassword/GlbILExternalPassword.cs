using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.Messaging.Integration;
using xTMessagingConstants = Enterprise.xTMessaging.Shared.Constants;

[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(xTMessagingConstants))]
namespace Enterprise.Customs.IL.Business
{
	public class GlbILExternalPassword : GlbExternalPasswordWithCertificate, IxTMessageAttributeProvider
	{
		public GlbILExternalPassword(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			actionOnSuccessfullSaving = () =>
			{
				if (!IsDeleted && !IsDeleting)
				{
					GP_PasswordStatus = IsCertificateValid ? Constants.GlbILExternalPasswordLookups.PasswordAwa : PasswordStatusList.Codes.Invalid;
				}
			};
		}

		#region Password

		[BusinessObjectTestExclude]
		[ResourceStringData("C8DF65AC-ACC4-44A1-AF3C-C58537EA850D", Caption = "Private Key Password")]
		public override ZString CurrentDecryptedCertificatePassphrase
		{
			get => base.CurrentDecryptedCertificatePassphrase;
			set
			{
				base.CurrentDecryptedCertificatePassphrase = value;
			}
		}

		#endregion

		#region Password Status

		[ResourceStringData("4C2AA5AD-968D-44B7-A734-57FE94A3053E", Caption = "Private Key Status")]
		public ZString PasswordStatus => Lookups.PasswordStatusList.GetDescriptionFromCode(base.GP_PasswordStatus) ?? base.GP_PasswordStatus;

		[ResourceStringData("CF2303DA-B9A4-4A12-9E9B-F4C47A1D47CF", Caption = "Password Status")]
		public ZString UserPasswordStatus => Lookups.PasswordStatusList.GetDescriptionFromCode(base.GP_PasswordStatus) ?? base.GP_PasswordStatus;

		public virtual ZPropertyInfo UserPasswordStatusInfo
		{
			[DebuggerStepThrough]
			get
			{
				return GetZPropertyInfo(nameof(UserPasswordStatus));
			}
		}

		[ReadOnly(true)]
		public override ZString GP_PasswordStatus { get => base.GP_PasswordStatus; set => base.GP_PasswordStatus = value; }

		#endregion

		#region GP_MailBoxID

		[ReadOnly(true)]
		[ResourceStringData("98308695-E4C1-45B7-B2C9-6A02F65E9BDB", Caption = "Sender VAT")]
		public override ZString GP_MailBoxID { get => base.GP_MailBoxID; set => base.GP_MailBoxID = value; }

		#endregion

		#region GP_ExpireDate

		[ReadOnly(true)]
		[ResourceStringData("7D0CCB4A-E968-421D-9B4C-151A4B260503", Caption = "Expiry Date")]
		public override ZDateTime GP_ExpiryDate { get => base.GP_ExpiryDate; set => base.GP_ExpiryDate = value; }

		#endregion

		[List(nameof(Lookups) + "." + nameof(Lookups.Staff))]
		[ResourceStringData("A5AA8074-88C6-40EF-BCF8-C1C53E7E0B9E", Caption = "User Code")]
		public override ZString GP_UserID { get => base.GP_UserID; set => base.GP_UserID = value; }

		protected override bool GP_UserID_ReadOnly => false;

		public override CredentialRecipient CredentialRecipient => CredentialRecipient.DirectxT;

		public override ZString ConfigurationName => Constants.GlbILExternalPassword.ConfigurationName;

		protected override string InterchangeTypeForSending => Constants.GlbILExternalPassword.InterchangeTypeForSending;

		public new GlbILExternalPasswordLookups Lookups => (GlbILExternalPasswordLookups)base.Lookups;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = PasswordTypesList.Codes.ILC;
		}

		public Dictionary<string, string> GetMessageAttrDictionary()
		{
			var t = (IxTMessageAttributeProvider)this;
			return t.GetXTMsgAttrProviderForGlbExternalPasswordSendingCertificateThumbprint();
		}

		protected override GlbExternalPasswordValidation GetNewValidation()
		{
			return new GlbExternalPasswordWithCertificateValidation(this);
		}

		protected override void ClearDataDefaultedFromCertificate()
		{
			GP_ExpiryDate = ZDateTime.Empty;
			GP_MailBoxID = ZString.Empty;
		}

		protected override void DefaultDataFromCertificate(X509Certificate2 certificate)
		{
			const string pattern = @"O=([^,]+)";
			GP_ExpiryDate = certificate.NotAfter;
			var match = Regex.Match(certificate.Subject, pattern);
			if (match.Success)
			{
				GP_MailBoxID = match.Groups[1].Value;
			}
		}

		protected override object[] CreateCredentialItems()
		{
			var certificate = CredentialSender.CreateCertificate(GP_Certificate, CurrentDecryptedCertificatePassphrase);
			return new object[] { certificate };
		}

		protected override ZPropertyInfo[] CredentialApplicableInfos()
					=> new ZPropertyInfo[] { GP_UserIDInfo, GP_CertificateInfo, GP_CertificatePassPhraseInfo };

		protected override bool ShouldSendDeleteCredential()
		{
			return !GP_UserIDInfo.OriginalValue.IsEmpty
				|| !GP_CertificateInfo.OriginalValue.IsEmpty
				|| !GP_CertificatePassPhraseInfo.OriginalValue.IsEmpty;
		}

		protected override GlbExternalPasswordLookups GetNewLookups() => new GlbILExternalPasswordLookups(this);

		protected override void RegisterConfigurationForSending()
			=> ExternalPasswordConfigurationToSender.RegisterForSending(Factory, ConfigurationName, Company, Group, Staff, InterchangeTypeForSending, CredentialRecipient, actionOnSuccessfullSaving);

		readonly Action actionOnSuccessfullSaving;
	}
}
