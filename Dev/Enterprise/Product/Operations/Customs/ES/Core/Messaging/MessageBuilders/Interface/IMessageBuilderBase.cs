using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IMessageBuilderBase
	{
		ZString MessageType { get; }
		ZString MessageSubType { get; }
		IESEDIMessageCollectionProvider Provider { get; }
		ZString UnsignedMessageText { get; set; }
		ZString GetSignedMessageText();
	}
}
