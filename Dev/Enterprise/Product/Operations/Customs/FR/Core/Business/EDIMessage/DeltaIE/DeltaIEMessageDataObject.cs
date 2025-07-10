using CargoWise.Customs.FR.MessageContracts;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class DeltaIEMessageDataObject : MessageDataObject
	{
		public DeltaIEMessageDataObject(FREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new DeltaIEMessagePrettier(this);
	}

	public abstract class DeltaIEMessageDataObject<TMessage> : MessageDataObject<TMessage>
		where TMessage : class
	{
		public DeltaIEMessageDataObject(DeltaIEFREDIMessage message) : base(message)
		{
		}

		protected override TMessage GetResponseMessage() => JsonHelper.DeserializeMessageWithNeverRequiredContractResolver<TMessage>(EDIMessage.EM_MessageText);
	}
}
