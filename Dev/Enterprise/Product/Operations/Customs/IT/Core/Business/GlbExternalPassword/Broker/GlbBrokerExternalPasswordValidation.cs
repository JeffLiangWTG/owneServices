using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public class GlbBrokerExternalPasswordValidation : GlbExternalPasswordValidation
{
	public GlbBrokerExternalPasswordValidation(GlbBrokerExternalPassword parent) : base(parent)
	{
	}

	protected new GlbBrokerExternalPassword Parent => (GlbBrokerExternalPassword)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		CheckCertificateOrXadesCertificateEntered();
	}

	protected override void CheckGP_UserID()
	{
		base.CheckGP_UserID();

		var targetPropertyInfo = Parent.GP_UserIDInfo;
		var userID = Parent.GP_UserID;

		if (userID.IsEmpty)
		{
			targetPropertyInfo.AddError(ValidationCaptions.GlbBrokerExternalPassword.NodeMustHaveValue);
		}
		else
		{
			CheckNodeMustBelongToValidAccount(targetPropertyInfo, userID);
			CheckNodeMustBeUnique(targetPropertyInfo, userID);
		}
	}

	protected override bool IsCertificateMandatory => false;

	protected override bool IsCurrentDecryptedCertificatePassphraseMandatory => false;

	protected override void CheckCurrentDecryptedCertificatePassphraseCore()
	{
		base.CheckCurrentDecryptedCertificatePassphraseCore();
		if (Parent.CurrentDecryptedCertificatePassphrase.IsEmpty && !Parent.GP_Certificate.IsEmpty)
		{
			Parent.CurrentDecryptedCertificatePassphraseInfo.AddError(ValidationCaptions.GlbBrokerExternalPassword.WhenCertificateIsLoadedPasswordIsMandatory);
		}
	}

	void CheckNodeMustBeUnique(ZPropertyInfo targetPropertyInfo, ZString userID)
	{
		var parentCollection = GlbStaffWrapper.Get(Parent.Staff)
			?.PasswordCollection?
			.Cast<GlbExternalPassword>();

		if (parentCollection?.Any(x => x.GP_UserID == userID && x.PK != Parent.PK) ?? false)
		{
			targetPropertyInfo.AddError(ValidationCaptions.GlbBrokerExternalPassword.NodeMustBeUnique);
		}
	}

	void CheckNodeMustBelongToValidAccount(ZPropertyInfo targetPropertyInfo, ZString userID)
	{
		if (!Parent.Lookups.Nodes.ContainsCode(userID))
		{
			targetPropertyInfo.AddError(ValidationCaptions.GlbBrokerExternalPassword.TheNodeMustBelongToValidAccountInRegistry);
		}
	}

	void CheckCertificateOrXadesCertificateEntered()
	{
		Parent.RemoveRowError(ValidationCaptions.GlbBrokerExternalPassword.PleaseEnterCertificateOrXadesCertificate);
		if (Parent.GP_Certificate.IsEmpty && GlbStaffWrapper.Get(Parent.Staff)?.CryptokiCertificateCollection?.GetCryptokiCertificate() == null)
		{
			Parent.AddRowError(ValidationCaptions.GlbBrokerExternalPassword.PleaseEnterCertificateOrXadesCertificate);
		}
	}
}
