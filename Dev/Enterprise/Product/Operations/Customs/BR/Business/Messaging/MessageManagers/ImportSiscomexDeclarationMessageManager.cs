namespace Enterprise.Customs.BR.Business
{
	public class ImportSiscomexDeclarationMessageManager : DeclarationMessageManager<ImportSiscomexMessageSendingObject>
	{
		public ImportSiscomexDeclarationMessageManager(ImportSiscomexMessageSendingObject messageSender) : base(messageSender)
		{
		}

		public override string MessageFriendlyName => MessageTypeList.Descriptions.LIS;

		protected override string OriginalMessageType => ImportSiscomexActionCodeList.Codes.ORI;

		protected override string AmendmentMessageType => null;

		protected override string WithdrawalMessageType => null;
	}
}
