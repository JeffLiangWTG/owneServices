using CargoWise.Data;
using Enterprise.RemotePrinting.Server.RPSCore;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.RemotePrinting.Server
{
	[CodeAlive("Using OwinStartup see web.config appSettings")]
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			AppHubConfiguration.Resolver.Register(typeof(RemotePrintHubDispatcher), () => new RemotePrintHubDispatcher(AppHubConfiguration));

			app.MapWhen(ShouldUseLegacy, c => { })
				.MapSignalR<RemotePrintHubDispatcher>("/signalr", AppHubConfiguration);

			SetupSignalRMaxIncomingWebSocketMessageSize();
		}

		HubConfiguration appHubConfiguration;
		protected HubConfiguration AppHubConfiguration => appHubConfiguration ??= new HubConfiguration { EnableDetailedErrors = true };

		bool ShouldUseLegacy(IOwinContext c)
			=> c.Request.Path.Value.Contains(nameof(RemotePrintingService)) || string.IsNullOrEmpty(c.Request.Path.Value.Trim('/'));

		protected void SetupSignalRMaxIncomingWebSocketMessageSize()
		{
			var maxIncomingMessageSizeInKB = WebPrintSignalRIncomingMaxSize.DefaultMaxSize;

			try
			{
				using (var connection = DbHelper.NewConnection())
				{
					maxIncomingMessageSizeInKB = RegistryData.WebPrintSignalRIncomingMaxSize(connection);
				}
			}
			catch (DatabaseUpgradeInProgressException)
			{
				// Database is upgraded, webUpgradeManager timer thread will download upgraded package and restart web application.
				// Ignore this exception and let webUpgradeManager finish its work.
			}
			catch (DatabaseUpgradedException)
			{
				// Database is upgraded, webUpgradeManager timer thread will download upgraded package and restart web application.
				// Ignore this exception and let webUpgradeManager finish its work.
			}
			catch (RemotePrintingDbConnectionException ex) when (ex.InnerException is DatabaseUpgradeInProgressException || ex.InnerException is DatabaseUpgradedException)
			{
				// Database is upgraded, webUpgradeManager timer thread will download upgraded package and restart web application.
				// Ignore this exception and let webUpgradeManager finish its work.
			}

			if (maxIncomingMessageSizeInKB > 0)
			{
				GlobalHost.Configuration.MaxIncomingWebSocketMessageSize = maxIncomingMessageSizeInKB * 1_024;
			}
			else if (maxIncomingMessageSizeInKB == 0)
			{
				GlobalHost.Configuration.MaxIncomingWebSocketMessageSize = null;
			}
		}
	}
}
