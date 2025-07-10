using Enterprise.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.EConversation.ServiceTasks
{
	public interface IEmailReaderFactory
	{
		IEmailReader Create(IMailboxSettings settings, ILogger logger);
	}
}
