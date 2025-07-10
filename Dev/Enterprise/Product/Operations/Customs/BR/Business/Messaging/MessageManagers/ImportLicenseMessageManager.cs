namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseMessageManager : DeclarationMessageManager<ImportLicenseMessageSendingObject>
	{
		public ImportLicenseMessageManager(ImportLicenseMessageSendingObject messageSender) : base(messageSender)
		{
		}

		public override string MessageFriendlyName => MessageTypeList.Descriptions.LIC;

		protected override string OriginalMessageType => ImportLicenseActionCodeList.Codes.ORI;

		protected override string AmendmentMessageType => null;

		protected override string WithdrawalMessageType => null;
	}
}
