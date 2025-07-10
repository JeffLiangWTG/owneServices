using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business
{
	public class GlbCompanyCredential : GlbExternalPasswordWithCertificate
	{
		public GlbCompanyCredential(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("99BDC008-A40C-440B-BEDF-6F779B329C9C", Caption = "User ID")]
		public override ZString GP_Name
		{
			get => base.GP_Name;
			set
			{
				var oldValue = base.GP_Name;
				base.GP_Name = value;
				if (oldValue != value)
				{
					RefreshCertificateAndStatusReason();
				}
			}
		}

		[ResourceStringData("6A38AF49-05E4-4413-A30E-877CB1912476", Caption = "Mailbox")]
		public override ZString GP_MailBoxID
		{
			get => base.GP_MailBoxID;
			set
			{
				var oldValue = base.GP_MailBoxID;
				base.GP_MailBoxID = value;
				if (oldValue != value)
				{
					RefreshCertificateAndStatusReason();
				}
			}
		}

		[ResourceStringData("D13AB2C9-87E0-4804-A6A9-BC07501BB383", Caption = "Sender ID")]
		public override ZString GP_UserID
		{
			get => base.GP_UserID;
			set => base.GP_UserID = value;
		}

		[ResourceStringData("AED32A0D-8923-4639-B638-BADE3ED48E32", Caption = "Certificate Password")]
		public override ZString CurrentDecryptedCertificatePassphrase
		{
			get => base.CurrentDecryptedCertificatePassphrase;
			set => base.CurrentDecryptedCertificatePassphrase = value;
		}

		[ReadOnly(true)]
		[ResourceStringData("253A73B3-0D44-4F45-A513-F4152A0A488A", Caption = "Status")]
		public override ZString GP_PasswordStatus
		{
			get => base.GP_PasswordStatus;
			set => base.GP_PasswordStatus = value;
		}

		[ReadOnly(true)]
		[ResourceStringData("896FA482-0D43-42A8-A6CB-42119F38DF1F", Caption = "Status Reason")]
		public override ZString GP_StatusReason
		{
			get => base.GP_StatusReason;
			set => base.GP_StatusReason = value;
		}
		void RefreshCertificateAndStatusReason()
		{
			DefaultDataFromCertificate();
			if (GP_PasswordStatus == PasswordStatusList.Codes.Valid)
			{
				GP_StatusReason = ZString.Empty;
			}
		}
		public new GlbCompanyCredentialValidation Validation => (GlbCompanyCredentialValidation)GetNewValidation();
		protected override GlbExternalPasswordValidation GetNewValidation() => new GlbCompanyCredentialValidation(this);

		protected override void ClearDataDefaultedFromCertificate()
		{
			base.ClearDataDefaultedFromCertificate();
			GP_StatusReason = ZString.Empty;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = PasswordTypesList.Codes.KRB;
		}
	}
}
