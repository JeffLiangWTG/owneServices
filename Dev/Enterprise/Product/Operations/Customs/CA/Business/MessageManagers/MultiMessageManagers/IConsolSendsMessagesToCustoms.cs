using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	public interface IConsolSendsMessagesToCustoms : ISendsMessagesToCustoms
	{
		SingleMessageManager[] WhichMessagesShouldWeRefresh(SingleMessageManager[] allManagers);
	}
}
