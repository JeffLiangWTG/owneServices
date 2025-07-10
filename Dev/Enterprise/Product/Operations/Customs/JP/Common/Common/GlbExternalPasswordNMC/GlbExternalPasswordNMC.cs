using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.JP.Common
{
	[SystemDefinedValues]
	public class GlbExternalPasswordNMC : GlbExternalPasswordWithPasswordType
	{
		public GlbExternalPasswordNMC(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static class GenAddOnColumnConstants
		{
			public const string ShouldReceiveColumnName = "JP_ShouldReceive";
		}

		public override string PasswordTypeCode => JPPasswordType.Codes.NMC;

		public override string PasswordTypeDescription => JPPasswordType.Descriptions.NMC;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ShouldReceive = true;
		}

		[ResourceStringData("E859ECEA-125E-4F2C-879D-441C9F357606", Caption = "Mailbox")]
		public override ZString GP_MailBoxID { get => base.GP_MailBoxID; set => base.GP_MailBoxID = value; }

		public ZString MailboxDomain
		{
			get
			{
				var key = Env.Instance.IsProductionSystem ? Constants.NACCSProdMailboxKey : Constants.NACCSTestMailboxKey;
				var naccsMailbox = new RefSysConfig.Loader(Factory).GetStringValue(key);
				var splittedMailbox = naccsMailbox.Split('@');
				return splittedMailbox.Length == 2 ? $"@{splittedMailbox[1]}" : ZString.Empty;
			}
		}

		public ZString FullMailBoxAddress => GP_MailBoxID + MailboxDomain;

		[ReadOnly(true)]
		[ResourceStringData("BDA09E7E-0876-4DED-BB7A-A7CC832184BD", Caption = "Status")]
		public override ZString GP_PasswordStatus { get => base.GP_PasswordStatus; set => base.GP_PasswordStatus = value; }

		[ResourceStringData("97480879-DBC0-4C0F-8D94-5C57836B078E", Caption = "Password")]
		public override ZString CurrentDecryptedPassword { get => base.CurrentDecryptedPassword; set => base.CurrentDecryptedPassword = value; }

		[ReadOnlyMember(nameof(ShouldReceive_Readonly))]
		[ResourceStringData("582BE0CF-BFC1-4354-83DF-BE45E5372882", Caption = "Should Receive?")]
		public ZBool ShouldReceive
		{
			get => this.GetSystemDefinedValue<ZBool>(GenAddOnColumnConstants.ShouldReceiveColumnName);
			set
			{
				var oldValue = ShouldReceive;
				if (oldValue != value)
				{
					this.SetSystemDefinedValue(GenAddOnColumnConstants.ShouldReceiveColumnName, value);
					ShouldReceiveInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ShouldReceiveInfo => GetZPropertyInfo(nameof(ShouldReceive));

		bool ShouldReceive_Readonly => GP_MailBoxID.IsEmpty;

		public new GlbExternalPasswordNMCValidation Validation => (GlbExternalPasswordNMCValidation)base.Validation;

		protected override GlbExternalPasswordValidation GetNewValidation() => new GlbExternalPasswordNMCValidation(this);

		public new GlbExternalPasswordNMCLookups Lookups => (GlbExternalPasswordNMCLookups)base.Lookups;

		protected override GlbExternalPasswordLookups GetNewLookups() => new GlbExternalPasswordNMCLookups(this);
	}
}
