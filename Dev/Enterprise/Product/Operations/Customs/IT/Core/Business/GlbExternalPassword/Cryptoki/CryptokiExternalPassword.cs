using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public class CryptokiExternalPassword : MasterFiles.Business.GlbExternalPassword, ICryptokiGlbExternalPassword
{
	public CryptokiExternalPassword(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		GP_PasswordType = PasswordTypesList.Codes.ITX;
		GP_GC = ZGuid.Empty;
	}

	public new CryptokiExternalPasswordLookups Lookups => (CryptokiExternalPasswordLookups)base.Lookups;
	protected override MasterFiles.Business.GlbExternalPasswordLookups GetNewLookups() => new CryptokiExternalPasswordLookups(this);

	public new CryptokiExternalPasswordValidation Validation => (CryptokiExternalPasswordValidation)base.Validation;
	protected override MasterFiles.Business.GlbExternalPasswordValidation GetNewValidation() => new CryptokiExternalPasswordValidation(this);

	[List(nameof(Lookups) + "." + nameof(CryptokiExternalPasswordLookups.CertificateAuthorityList))]
	[ResourceStringData("Enterprise.Customs.IT.Business.CryptokiExternalPassword|CertificateAuthority", Caption = "IT Customs Certificate Authority", ShortCaption = "CA", MediumCaption = "Certificate Authority")]
	public override ZString GP_CertificateAuthority { get => base.GP_CertificateAuthority; set => base.GP_CertificateAuthority = value; }

	[List(nameof(Lookups) + "." + nameof(CryptokiExternalPasswordLookups.ChipsetList))]
	[ResourceStringData("Enterprise.Customs.IT.Business.CryptokiExternalPassword|Chipset", Caption = "IT Customs Digital Signature Chipset Manufacturer", ShortCaption = "Chipset", MediumCaption = "Chipset Manufacturer")]
	public override ZString GP_Name
	{
		get => base.GP_Name;
		set
		{
			var oldValue = GP_Name;
			base.GP_Name = value;
			if (!IsCopying && oldValue != GP_Name)
			{
				ClearCertificateRelatedFields();
			}
		}
	}

	[MaxLength(32)]
	[ReadOnly(true)]
	[ResourceStringData("Enterprise.Customs.IT.Business.CryptokiExternalPassword|CertificateSerialNumber", Caption = "IT Customs Certificate Serial Number", ShortCaption = "Certificate SN", MediumCaption = "Certificate Serial Number")]
	public override ZString GP_CertificateSerialNumber { get => base.GP_CertificateSerialNumber; set => base.GP_CertificateSerialNumber = value; }

	public ITokenPinStore TokenPinStore => tokenPinStore ?? (tokenPinStore = new TokenPinStore());
	ITokenPinStore tokenPinStore;

	public void PopulateCertificateRelatedFields(CryptokiCertificate cryptokiCertificate)
	{
		Argument.NotNull(cryptokiCertificate, nameof(cryptokiCertificate));

		GP_CertificateSerialNumber = cryptokiCertificate.SerialNumber;
		GP_IssueDate = cryptokiCertificate.NotBefore;
		GP_ExpiryDate = cryptokiCertificate.NotAfter;
	}

	void ClearCertificateRelatedFields()
	{
		GP_CertificateSerialNumber = ZString.Empty;
		GP_IssueDate = ZDateTime.Empty;
		GP_ExpiryDate = ZDateTime.Empty;
	}

	#region eHub credential exchange must not be active

	protected override bool ShouldSendCredential() => false;

	protected override bool ShouldSendDeleteCredential() => false;

	#endregion
}
