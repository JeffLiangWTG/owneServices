using CargoWise.Customs.FR.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public abstract class MessageBuilderManager<TObjectToSend> where TObjectToSend : IMessageSendingObject
	{
		public abstract IMessageBuilderBase NewMessageBuilder(TObjectToSend objectToSend);

		public abstract ZString BuilderType { get; }
	}
}
