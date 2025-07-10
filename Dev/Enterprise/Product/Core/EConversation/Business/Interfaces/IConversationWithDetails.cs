using CargoWise.Types;

namespace Enterprise.EConversation.Business
{
	public interface IConversationWithDetails : IConversationProvider
	{
		ZString Summary { get; }
		ZString DetailedDescription { get; }
	}
}
