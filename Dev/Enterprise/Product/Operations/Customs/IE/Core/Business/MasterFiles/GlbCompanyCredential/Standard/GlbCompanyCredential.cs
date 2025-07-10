using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.IE.Business
{
	public class GlbCompanyCredential : GlbExternalPasswordWithCertificate, IxTMessageAttributeProvider
	{
		public GlbCompanyCredential(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = PasswordTypesList.Codes.IER;
		}

		#region Overrided properties

		[ResourceStringData("{0E5D2793-66FD-4D40-8622-202C3B642DFA}", Caption = "Message Sender EORI")]
		public override ZString GP_MailBoxID { get => base.GP_MailBoxID; set => base.GP_MailBoxID = value; }

		[BusinessObjectTestExclude]
		public override ZString CurrentDecryptedCertificatePassphrase
		{
			get => base.CurrentDecryptedCertificatePassphrase;
			set
			{
				base.CurrentDecryptedCertificatePassphrase = ExternalPasswordHelper.GetHashedPassword(value);
			}
		}

		[ReadOnly(true)]
		public override ZString GP_PasswordStatus { get => base.GP_PasswordStatus; set => base.GP_PasswordStatus = value; }

		#endregion

		#region Password status

		public ZString PasswordStatus => Lookups.PasswordStatusList.GetDescriptionFromCode(base.GP_PasswordStatus) ?? base.GP_PasswordStatus;

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

		Dictionary<string, string> IxTMessageAttributeProvider.GetMessageAttrDictionary()
		{
			var t = (IxTMessageAttributeProvider)this;
			return t.GetXTMsgAttrProviderForGlbExternalPasswordSendingCertificatePEM();
		}

		public new GlbCompanyCredentialValidation Validation => (GlbCompanyCredentialValidation)GetNewValidation();

		protected override bool IsCertificateValid => GP_ExpiryDate.IsInTheFutureUtc() && base.IsCertificateValid;

		public override void OnSaving()
		{
			if (IsInDatabase && (!GP_MailBoxIDInfo.OriginalValue.Equals(GP_MailBoxID) || !GP_CertificateInfo.OriginalValue.Equals(GP_Certificate)))
			{
				TransactionIDManager.SetTransactionNumbersAsUsed(Company, ZGuid.Empty, CusTransactionNumberTypeList.Codes.IECustoms);
			}
			base.OnSaving();
		}
	}
}
