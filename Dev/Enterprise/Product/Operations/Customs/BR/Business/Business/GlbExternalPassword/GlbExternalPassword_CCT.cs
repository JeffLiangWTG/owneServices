using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.MasterFiles.Integration.Customs.BR;
using Enterprise.Messaging.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class GlbExternalPassword_CCT : GlbExternalPasswordWithCertificate, IGlbExternalPassword_CCT
		, IxTMessageAttributeProvider
	{
		public GlbExternalPassword_CCT(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implement

		protected override ZString HumanReadableNameCore => Res.GetString("44c00436-757c-48b2-88a6-5f0ac621cf7d", "CCT Certificate");

		public override ZString ConfigurationName => CredentialConfigurationName;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		public const string CredentialConfigurationName = "Advanced Air Cargo Report - Brazil";

		protected override ZPropertyInfo[] CredentialApplicableInfos()
		{
			return new ZPropertyInfo[] { GP_CertificateInfo, GP_CertificatePassPhraseInfo };
		}

		public override object CreateCredentialData()
		{
			return !GP_Certificate.IsEmpty && !CurrentDecryptedCertificatePassphrase.IsEmpty
				? CredentialSender.CreateCertificate(GP_Certificate, CurrentDecryptedCertificatePassphrase)
				: null;
		}

		protected override bool ShouldSendDeleteCredential()
		{
			return !GP_CertificateInfo.OriginalValue.IsEmpty || !GP_CertificatePassPhraseInfo.OriginalValue.IsEmpty;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = PasswordTypesList.Codes.CCT;
			GP_PasswordStatus = ZString.Empty;
		}

		public new GlbExternalPasswordValidation_CCT Validation => (GlbExternalPasswordValidation_CCT)GetNewValidation();

		protected override GlbExternalPasswordValidation GetNewValidation()
		{
			return new GlbExternalPasswordValidation_CCT(this);
		}

		#endregion

		#region Properties

		[ReadOnly(true)]
		public override ZString GP_PasswordStatus
		{
			get => base.GP_PasswordStatus;
			set => base.GP_PasswordStatus = value;
		}

		public override ZBlob GP_Certificate
		{
			get => base.GP_Certificate;
			set
			{
				var oldValue = GP_Certificate;
				base.GP_Certificate = value;
				DefaultSubscriptionIfNeeded(oldValue);
			}
		}

		void DefaultSubscriptionIfNeeded(ZBlob oldValue)
		{
			if (!IsCopying && !GP_Certificate.IsEmpty && oldValue != GP_Certificate)
			{
				var subscriptions = EventSubscriptions;

				if (!subscriptions.Cast<GlbExternalPassword_BRS>().Any(subscription => subscription.GP_UserID == EventIdList.Codes.DuexHistoric))
				{
					var newSubscription = subscriptions.AddNew();
					newSubscription.GP_UserID = EventIdList.Codes.DuexHistoric;
				}
			}
		}

		internal EventSubscriptionCollection EventSubscriptions => BRGlbStaffWrapper.Get(Staff).EventSubscriptions;

		#endregion

		public Dictionary<string, string> GetMessageAttrDictionary()
		{
			var t = (IxTMessageAttributeProvider)this;
			return t.GetXTMsgAttrProviderForGlbExternalPasswordSendingCertificatePEM();
		}

		public bool IsValidCertificate => !GP_ExpiryDate.IsInThePast() && GP_PasswordStatus == BRPasswordStatusList.Codes.Valid;
	}
}
