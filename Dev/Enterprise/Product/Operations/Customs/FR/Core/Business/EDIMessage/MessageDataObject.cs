using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public abstract class MessageDataObject
	{
		protected MessageDataObject(FREDIMessage message)
		{
			EDIMessage = message;
		}

		protected readonly FREDIMessage EDIMessage;
		public BusinessObjectFactory Factory => EDIMessage.Factory;

		public FREDIMessagePrettier Prettier => prettier ?? (prettier = CreatePrettier());
		FREDIMessagePrettier prettier;

		protected abstract FREDIMessagePrettier CreatePrettier();

		public bool CanSetMessageInterpretation => CanSetMessageInterpretationCore;

		protected virtual bool CanSetMessageInterpretationCore => false;

		public ZString MessageText => EDIMessage.EM_MessageText;
	}

	public abstract class MessageDataObject<TMessage> : MessageDataObject
		where TMessage : class
	{
		protected MessageDataObject(FREDIMessage message) : base(message)
		{
		}

		protected virtual TMessage GetResponseMessage() => Extensions.Deserialize<TMessage>(EDIMessage.EM_MessageText);

		public TMessage ResponseMessage => responseMessage ?? (responseMessage = GetResponseMessage());
		TMessage responseMessage;
	}
}
