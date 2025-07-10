using Enterprise.Customs.IT.MessageBuilders;

namespace Enterprise.Customs.IT.Messaging.MessageStructure;

public abstract class SadCustomsMessage : ISadCustomsMessage
{
	string ISadCustomsMessage.Serialize()
	{
		var messageSerializer = new TabbedFlatFileMessageSerializer();
		return messageSerializer.Serialize(this);
	}
}
