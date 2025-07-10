using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.IN.Business;

[SystemDefinedValues]
public class GlbLoginPassword : GlbExternalPassword
{
	public GlbLoginPassword(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : GlbExternalPassword.Schema
	{
		public const string NeedCopyOfEmails = "NeedCopyOfEmails";
		public const string CopyToMailBox = "CopyToMailBox";
		public const string AutoGenerateEmailId = "AutoGenerateEmailId";

		public const int LoginIDMaxLength = 25;
		public const int PasswordMaxLength = 25;
		public const int EmailIDMaxLength = 200;
	}

	protected override ZString HumanReadableNameCore => Res.GetString("31269959-2293-4A2A-8218-6521A673C865", "INC Certificate");

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		GP_PasswordType = PasswordTypesList.Codes.INC;
	}

	[MaxLength(Schema.LoginIDMaxLength)]
	[ResourceStringData("CADF4398-2205-4F0E-BDC0-884EAF77F19C", Caption = "ICEGATE Login Id")]
	public override ZString GP_UserID => base.GP_UserID;

	[MaxLength(Schema.PasswordMaxLength)]
	[ResourceStringData("BFCA9A7F-9402-456F-9E80-FD3BE811AEE0", Caption = "Password")]
	public override ZString CurrentDecryptedPassword => base.CurrentDecryptedPassword;

	[MaxLength(Schema.EmailIDMaxLength)]
	[ReadOnly(true)]
	[ResourceStringData("CADF4398-2205-4F0E-BDC0-884EAF77F19C", Caption = "ICEGATE Email Id")]
	public override ZString GP_MailBoxID => base.GP_MailBoxID;

	public ZString CopyToMailBox => NeedCopyOfEmails ? Staff?.GS_EmailAddress ?? ZString.Empty : ZString.Empty;

	public ZPropertyInfo CopyToMailBoxInfo => GetZPropertyInfo(Schema.CopyToMailBox);

	[ResourceStringData("0FDD3661-89A8-4C09-B635-AB01B55DBF93", Caption = "Need Copy of Emails?", MediumCaption = "Need Copy?", ShortCaption = "Need Copy?", FullDescription = "The email ID mentioned here will receive a copy of each outgoing/incoming CW1 email specified under the ICEGATE email ID.")]
	public ZBool NeedCopyOfEmails
	{
		get => this.GetSystemDefinedValue<ZBool>(Schema.NeedCopyOfEmails);
		set
		{
			var oldValue = NeedCopyOfEmails;
			this.SetSystemDefinedValue(Schema.NeedCopyOfEmails, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateCopyToMailBox();
			}
			NeedCopyOfEmailsInfo.RefreshBinding(oldValue);
		}
	}

	public ZPropertyInfo NeedCopyOfEmailsInfo => GetZPropertyInfo(Schema.NeedCopyOfEmails);

	[ResourceStringData("CADF4398-2205-4F0E-BDC0-884EAF77F19C", Caption = "ICEGATE Email Id")]
	public ZBool AutoGenerateEmailId
	{
		get => !GP_MailBoxID.IsEmpty;
		set
		{
			var oldValue = AutoGenerateEmailId;
			if (value != oldValue)
			{
				GP_MailBoxID = value && Staff is GlbStaff staff && Company is GlbCompany company ? $"{company.LicenceKeyIdentifier}_{staff.GS_Code}@ic.wisegrid.net" : ZString.Empty;
				AutoGenerateEmailIdInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo AutoGenerateEmailIdInfo => GetZPropertyInfo(Schema.AutoGenerateEmailId);

	public new GlbLoginPasswordValidation Validation => (GlbLoginPasswordValidation)base.Validation;

	protected override GlbExternalPasswordValidation GetNewValidation() => new GlbLoginPasswordValidation(this);
}
