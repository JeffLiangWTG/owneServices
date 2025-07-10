using CargoWise.Customs.BR.MessageContracts.ImportLicense.Outgoing;
using CargoWise.Types;
using Enterprise.Customs.BR.Business.ImportLicense;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public partial class ImportLicenseMessageSendingObject : DeclarationMessageSendingObject
	{
		public ImportLicenseMessageSendingObject(CusEntryHeader header) : base(header)
		{
		}

		public override ZString GetMessageOwner() => ZString.Empty;

		public override ZString GetMessageTypeForEDIMessage() => MessageTypeList.Codes.LIC;

		protected override ZString GetDefaultMessageType() => ImportLicenseActionCodeList.Codes.ORI;

		protected override bool MessageType_ReadOnly => true;

		public override CodeDescriptionPairList MessageTypesList => Factory.GetCachedValue<ImportLicenseActionCodeList>();

		public override ZString GetApplicationReference() => ZString.Empty;

		public override ZString GetMessageText() => new ImportLicenseMessageBuilder(new ImportLicenseProvider(this)).GetMessageText();

		protected override Customs.Business.JobDeclarationMessageSendingObjectValidation GetNewValidation()
		{
			return new ImportLicenseMessageSendingObjectValidation(this);
		}

		public new ImportLicenseMessageSendingObjectValidation Validation => (ImportLicenseMessageSendingObjectValidation)base.Validation;

		#region SetDefaultValuesFromEntry

		protected override void SetMessageSendingObjectDefaultValues()
		{
			base.SetMessageSendingObjectDefaultValues();
			ShouldSend = true;
		}

		#endregion
	}
}
