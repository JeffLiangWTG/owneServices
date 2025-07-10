using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CH.Business;

public class GlbCompanyCredential : GlbExternalPasswordWithCertificate
												, IxTMessageAttributeProvider
{
	public GlbCompanyCredential(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

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

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		GP_PasswordType = PasswordTypesList.Codes.CHC;
	}

	protected override void ClearDataDefaultedFromCertificate()
	{
		GP_ExpiryDate = ZDateTime.Empty;
	}

	protected override void DefaultDataFromCertificate(X509Certificate2 certificate)
	{
		GP_ExpiryDate = certificate.NotAfter;
		GP_IssueDate = certificate.NotBefore;
	}

	protected override GlbExternalPasswordValidation GetNewValidation() => new GlbCompanyCredentialValidation(this);

	public new GlbCompanyCredentialValidation Validation => (GlbCompanyCredentialValidation)GetNewValidation();

	public Dictionary<string, string> GetMessageAttrDictionary()
	{
		var t = (IxTMessageAttributeProvider)this;
		return t.GetXTMsgAttrProviderForGlbExternalPasswordSendingCertificatePEM();
	}
}
