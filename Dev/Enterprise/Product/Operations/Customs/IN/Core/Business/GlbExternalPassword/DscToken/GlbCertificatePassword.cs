using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IN.Business;

public class GlbCertificatePassword : GlbExternalPassword
{
	public GlbCertificatePassword(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	[ReadOnlyMember(nameof(IsSerialNumberFilled))]
	[List(nameof(Lookups) + "." + nameof(GlbCertificatePasswordLookups.CertificateAuthorities))]
	[ResourceStringData("B44BDF20-EDDD-43F8-9802-4D95B7BD8A9E", Caption = "Certificate Authority")]
	public override ZString GP_CertificateAuthority => base.GP_CertificateAuthority;

	[ReadOnlyMember(nameof(IsSerialNumberFilled))]
	[List(nameof(Lookups) + "." + nameof(GlbCertificatePasswordLookups.Chipsets))]
	[ResourceStringData("9ED0AF8A-1559-47B9-B2B3-A613C22BE33E", Caption = "Chipset Manufacturer")]
	public override ZString GP_Name
	{
		get => base.GP_Name;
		set
		{
			var oldValue = GP_Name;
			base.GP_Name = value;
			if (!IsCopying && oldValue != GP_Name)
			{
				GP_CertificateSerialNumber = ZString.Empty;
			}
		}
	}

	[ReadOnly(true)]
	[ResourceStringData("6F852331-ABFB-4CF4-9767-10422AF8E9AE", Caption = "Certificate SN")]
	public override ZString GP_CertificateSerialNumber => base.GP_CertificateSerialNumber;

	public ZString LibraryName => Lookups.Chipsets.GetDescriptionFromCode(GP_Name);

	public new GlbCertificatePasswordLookups Lookups => (GlbCertificatePasswordLookups)base.Lookups;

	public void ClearDetails()
	{
		GP_CertificateAuthority = ZString.Empty;
		GP_Name = ZString.Empty;
		GP_CertificateSerialNumber = ZString.Empty;
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		GP_PasswordType = PasswordTypesList.Codes.INX;
	}

	protected override GlbExternalPasswordLookups GetNewLookups()
	{
		return new GlbCertificatePasswordLookups(this);
	}

	protected override GlbExternalPasswordValidation GetNewValidation()
	{
		return new GlbCertificatePasswordValidation(this);
	}

	protected override ZString HumanReadableNameCore => Res.GetString("8267F800-6298-4DD5-8E7B-D373725BD826", "Credentials > Digital Signature Certificate Token Profile");

	ZBool IsSerialNumberFilled => !GP_CertificateSerialNumber.IsEmpty;
}
