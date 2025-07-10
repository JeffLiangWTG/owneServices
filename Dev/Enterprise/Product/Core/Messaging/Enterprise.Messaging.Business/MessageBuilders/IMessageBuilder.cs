using Enterprise.Messaging.Business.MessageBuilders;

namespace Enterprise.Messaging.MessageBuilders
{
	public interface IMessageBuilder
	{
		IMessageBuilderResult PopulateMessages();
	}
}
