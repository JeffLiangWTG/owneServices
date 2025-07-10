using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business.MessageGeneration
{
	public interface INctsMessageSender
	{
		bool CreateMessage(NctsHeader nctsHeader, ISendsMessagesToCustoms sendMessagesToCustoms, NctsMessageFunctionSet messageFunction);
	}
}
