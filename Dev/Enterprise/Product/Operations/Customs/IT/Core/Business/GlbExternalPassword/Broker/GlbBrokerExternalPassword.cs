using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;

namespace Enterprise.Customs.IT.Business;

public sealed class GlbBrokerExternalPassword : GlbExternalPassword
{
	public new class Schema : GlbExternalPasswordWithCertificate.Schema
	{
		public const string AccountNumber = "AccountNumber";
	}

	public GlbBrokerExternalPassword(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public sealed override ZBlob GP_Certificate
	{
		get => base.GP_Certificate;
		set
		{
			var oldValue = GP_Certificate;
			base.GP_Certificate = value;
			if (!IsCopying && oldValue != GP_Certificate)
			{
				CopyCertificateInfo(GP_CertificateInfo);
			}
		}
	}

	public sealed override ZString CurrentDecryptedCertificatePassphrase
	{
		get => base.CurrentDecryptedCertificatePassphrase;
		set
		{
			var oldValue = CurrentDecryptedCertificatePassphrase;
			base.CurrentDecryptedCertificatePassphrase = value;
			if (!IsCopying && oldValue != CurrentDecryptedCertificatePassphrase)
			{
				CopyCertificateInfo(CurrentDecryptedCertificatePassphraseInfo);
			}
		}
	}

	[List(nameof(Lookups) + "." + nameof(GlbBrokerExternalPasswordLookups.Nodes))]
	public override ZString GP_UserID { get => base.GP_UserID; set => base.GP_UserID = value; }

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		GP_GC = ZGuid.Empty;
	}

	void CopyCertificateInfo(ZPropertyInfo propertyInfo)
	{
		GlbStaffWrapper.Get(Staff).PasswordCollection.CopyCertificateInfo(this, propertyInfo);
	}

	public new GlbBrokerExternalPasswordLookups Lookups => (GlbBrokerExternalPasswordLookups)base.Lookups;
	protected override MasterFiles.Business.GlbExternalPasswordLookups GetNewLookups() => new GlbBrokerExternalPasswordLookups(this);

	protected override MasterFiles.Business.GlbExternalPasswordValidation GetNewValidation() => new GlbBrokerExternalPasswordValidation(this);

	protected override ZString PasswordType => PasswordTypesList.Codes.ITB;

	protected override bool IsCertificateValid => !AccountNumber.IsEmpty && !GP_UserID.IsEmpty && base.IsCertificateValid;

	Account Account => Lookups.AllAccountsWithCompany.FirstOrDefault(x => x.Account.AccountNode == GP_UserID)?.Account;

	public sealed override ZString ConfigurationName => ITCustomsSubscribers;

	public ZString AccountNumber => Account?.AccountNumber ?? ZString.Empty;

	protected override object[] CreateCredentialItems()
	{
		var mailbox = CredentialSender.CreateItem(Constants.ItemTypes.MailBoxID, AccountNumber);
		var node = CredentialSender.CreateCredential(Constants.CredentialDetails.Current, GP_UserID, ZString.Empty);
		var certificate = CredentialSender.CreateCertificate(GP_Certificate, CurrentDecryptedCertificatePassphrase);
		return new object[] { mailbox, node, certificate };
	}

	public const string ITCustomsSubscribers = "ITCustomsSubscribers";

	protected override ZPropertyInfo[] CredentialApplicableInfos() => new ZPropertyInfo[] { GP_UserIDInfo, GP_CertificateInfo, GP_CertificatePassPhraseInfo };

	protected override bool ShouldSendDeleteCredential()
	{
		return !GP_UserIDInfo.OriginalValue.IsEmpty
			|| !GP_CertificateInfo.OriginalValue.IsEmpty
			|| !GP_CertificatePassPhraseInfo.OriginalValue.IsEmpty;
	}
}
