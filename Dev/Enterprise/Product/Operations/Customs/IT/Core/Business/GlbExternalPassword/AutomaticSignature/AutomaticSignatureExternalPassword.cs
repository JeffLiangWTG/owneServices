using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public sealed class AutomaticSignatureExternalPassword : MasterFiles.Business.GlbExternalPassword, IAutomaticSignatureExternalPassword
{
	public AutomaticSignatureExternalPassword(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : MasterFiles.Business.GlbExternalPassword.Schema
	{
		public new const int GP_MailBoxIDMaxLength = 10;
		public new const int GP_UserIDMaxLength = 100;
		public new const int GP_NameMaxLength = 16;
	}

	[ResourceStringData("C66F4A9A-971D-4F3C-B896-57F67D2FE4F6", Caption = "Enabled")]
	public ZBool IsConfigurationActive
	{
		get => GP_PasswordStatus == PasswordStatusList.Codes.Valid;
		set
		{
			GP_PasswordStatus = value ? PasswordStatusList.Codes.Valid : ZString.Empty;
			IsConfigurationActiveInfo.RefreshBinding();
		}
	}

	public ZPropertyInfo IsConfigurationActiveInfo => GetZPropertyInfo(nameof(IsConfigurationActive));

	[List(nameof(Lookups) + "." + nameof(AutomaticSignatureExternalPasswordLookups.DelegateList))]
	[MaxLength(Schema.GP_MailBoxIDMaxLength)]
	[ResourceStringData("3A702F5A-3B3C-4039-938C-41B871F24D1C", Caption = "Delegate")]
	public override ZString GP_MailBoxID { get => base.GP_MailBoxID; set => base.GP_MailBoxID = value; }

	[MaxLength(Schema.GP_UserIDMaxLength)]
	[ResourceStringData("BFC8CED7-F02A-43AC-B5DA-7F155A11EA17", Caption = "User")]
	public override ZString GP_UserID { get => base.GP_UserID; set => base.GP_UserID = value; }

	[MaxLength(Schema.GP_NameMaxLength)]
	[ResourceStringData("CD9C5649-33AD-4E8F-A18F-997C4A7753CE", Caption = "User Fiscal Code")]
	public override ZString GP_Name { get => base.GP_Name; set => base.GP_Name = value; }

	public new AutomaticSignatureExternalPasswordLookups Lookups => (AutomaticSignatureExternalPasswordLookups)base.Lookups;

	protected override MasterFiles.Business.GlbExternalPasswordLookups GetNewLookups() => new AutomaticSignatureExternalPasswordLookups(this);

	public new MasterFiles.Business.GlbExternalPasswordValidation Validation => (AutomaticSignatureExternalPasswordValidation)base.Validation;

	ZString IAutomaticSignatureExternalPassword.DelegateName => Lookups.DelegateList.GetDescriptionFromCode(GP_MailBoxID);

	protected override MasterFiles.Business.GlbExternalPasswordValidation GetNewValidation() => new AutomaticSignatureExternalPasswordValidation(this);

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		GP_PasswordType = PasswordTypesList.Codes.ITA;
		GP_CertificateAuthority = CertificateAuthorityList.Codes.Aruba;
	}
}
