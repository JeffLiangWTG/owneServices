using Enterprise.Integration;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MailManager.ExternalMailInterface.CommonInterfaces;
using Enterprise.Registry.Business;

namespace Enterprise.EConversation.ServiceTasks
{
	public class EmailReaderFactory : IEmailReaderFactory
	{
		public IEmailReader Create(IMailboxSettings settings, ILogger logger)
		{
			if (settings is IOAuth2MailboxSettings { UseOAuth2: true } oAuth2Settings)
			{
				var configuration = new MailServerConfiguration(oAuth2Settings);
				IMailDownloader downloader = oAuth2Settings.UseGraphApi
					? new GraphMailDownloader(configuration)
					: new MailDownloader(configuration);

				return new MailDownloaderEmailReader(configuration.Server, $"Tenant:{configuration.TenantId}, ApplicationId:{configuration.ApplicationId}", downloader, logger);
			}
			else
			{
				var configuration = new MailServerConfiguration(settings);
				return new MailProtocolEmailReader(configuration.Server, configuration.UserName, configuration.GetMailProtocol());
			}
		}
	}
}
