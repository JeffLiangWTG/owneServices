namespace Enterprise.Customs.BR.Business
{
	public class ExportDeclarationMessageManager : DeclarationMessageManager<ExportDeclarationMessageSendingObject>
	{
		public ExportDeclarationMessageManager(ExportDeclarationMessageSendingObject messageSender) : base(messageSender)
		{
		}

		public override string MessageFriendlyName => MessageTypeList.Descriptions.CDE;

		protected override string OriginalMessageType => ExportEntryActionCodeList.Codes.ORI;

		protected override string AmendmentMessageType => ExportEntryActionCodeList.Codes.RET;

		protected override string WithdrawalMessageType => null;

		public override bool HasActiveMessages => base.HasActiveMessages || IsWaitingForResponse;
	}
}
