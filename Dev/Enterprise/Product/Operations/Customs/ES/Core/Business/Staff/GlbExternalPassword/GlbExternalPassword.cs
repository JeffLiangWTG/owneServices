using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.ES.Business
{
	public class GlbExternalPassword : GlbExternalPasswordWithCertificate, IxTMessageAttributeProvider
	{
		public GlbExternalPassword(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable(true)]
		public GlbExternalPasswordAuthorisationCollection Authorisations
		{
			get
			{
				if (authorisations == null)
				{
					authorisations = new GlbExternalPasswordAuthorisationCollection(this);
					authorisations.Load();
					RegisterEditableChildObject(authorisations);
				}
				return authorisations;
			}
		}
		GlbExternalPasswordAuthorisationCollection authorisations;

		public override void Delete()
		{
			Authorisations.RemoveAndDeleteAll();
			base.Delete();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = PasswordTypesList.Codes.ESB;
		}

		public override ZString ConfigurationName => "ESCustomsStaffCredentials";

		public override object CreateCredentialData()
		{
			var result = new Group()
			{
				Type = GP_PasswordType,
				Status = GetCredentialStatus(),
				Reference = GP_Name,
				Items = CreateCredentialItems()
			};

			return result;
		}

		protected override object[] CreateCredentialItems()
		{
			var credential = CredentialSender.CreateCredential(ZString.Empty, GP_UserID, ZString.Empty);
			var certificate = CredentialSender.CreateCertificate(GP_Certificate, CurrentDecryptedCertificatePassphrase);
			return new object[] { credential, certificate };
		}

		protected override ZPropertyInfo[] CredentialApplicableInfos()
			=> new ZPropertyInfo[] { GP_NameInfo, GP_UserIDInfo, GP_CertificateInfo, GP_CertificatePassPhraseInfo };

		protected override bool ShouldSendDeleteCredential()
		{
			return !GP_NameInfo.OriginalValue.IsEmpty
				|| !GP_UserIDInfo.OriginalValue.IsEmpty
				|| !GP_CertificateInfo.OriginalValue.IsEmpty
				|| !GP_CertificatePassPhraseInfo.OriginalValue.IsEmpty;
		}

		public new GlbExternalPasswordValidation Validation => (GlbExternalPasswordValidation)GetNewValidation();
		protected override Enterprise.MasterFiles.Business.GlbExternalPasswordValidation GetNewValidation() => new GlbExternalPasswordValidation(this);

		public Dictionary<string, string> GetMessageAttrDictionary()
		{
			var t = (IxTMessageAttributeProvider)this;
			return t.GetXTMsgAttrProviderForGlbExternalPasswordSendingCertificatePEM();
		}
	}
}
