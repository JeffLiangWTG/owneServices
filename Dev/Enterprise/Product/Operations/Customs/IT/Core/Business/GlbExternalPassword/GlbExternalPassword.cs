using System.Data;
using System.Security.Cryptography.X509Certificates;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public abstract class GlbExternalPassword : GlbExternalPasswordWithCertificate
{
	protected GlbExternalPassword(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		GP_PasswordType = PasswordType;
	}

	protected abstract ZString PasswordType { get; }

	#region Properties

	protected sealed override bool GP_UserID_ReadOnly => false;

	[MaxLength(32)]
	[ResourceStringData("GlbExternalPassword_IT|CurrentDecryptedCertificatePassphrase", Caption = "Certificate Password")]
	public override ZString CurrentDecryptedCertificatePassphrase
	{
		get => base.CurrentDecryptedCertificatePassphrase;
		set => base.CurrentDecryptedCertificatePassphrase = value;
	}

	[ResourceStringData("GlbExternalPassword_IT|PasswordStatus", Caption = "Password Status")]
	public ZString PasswordStatus => Lookups.PasswordStatusList.GetDescriptionFromCode(base.GP_PasswordStatus) ?? base.GP_PasswordStatus;

	[ResourceStringData("GlbExternalPassword_IT|GP_ExpiryDate", Caption = "Expiry Date")]
	public override ZDateTime GP_ExpiryDate { get => base.GP_ExpiryDate; set => base.GP_ExpiryDate = value; }

	#endregion

	public new GlbExternalPasswordLookups Lookups => (GlbExternalPasswordLookups)base.Lookups;
	public new GlbExternalPasswordValidation Validation => (GlbExternalPasswordValidation)GetNewValidation();

	protected override void ClearDataDefaultedFromCertificate()
	{
		GP_ExpiryDate = ZDateTime.Empty;
	}

	protected override void DefaultDataFromCertificate(X509Certificate2 certificate)
	{
		GP_ExpiryDate = certificate.NotAfter;
	}
}
