namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public abstract class PNTSMessagePrettier<TMessage> : UCCMessagePrettier<TMessage>
		where TMessage : class
	{
		public PNTSMessagePrettier(PNTSMessageDataObject<TMessage> messageDataObject) : base(messageDataObject)
		{
		}

		public new PNTSMessageDataObject<TMessage> MessageDataObject => (PNTSMessageDataObject<TMessage>)base.MessageDataObject;
	}
}
