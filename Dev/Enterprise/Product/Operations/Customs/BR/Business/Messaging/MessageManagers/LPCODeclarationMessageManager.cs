namespace Enterprise.Customs.BR.Business
{
	public class LPCODeclarationMessageManager : DeclarationMessageManager<LPCODeclarationMessageSendingObject>
	{
		public LPCODeclarationMessageManager(LPCODeclarationMessageSendingObject messageSender) : base(messageSender)
		{
		}

		public override string MessageFriendlyName => MessageTypeList.Descriptions.LPC;

		protected override string OriginalMessageType => LPCOEntryActionCodeList.Codes.ORI;

		protected override string AmendmentMessageType => null;

		protected override string WithdrawalMessageType => null;
	}
}
