using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.IdentityRedirectUrl.Business;
using Enterprise.Client.EDI.ServiceTasks.AzureApplicationProcessing;
using Enterprise.Integration;

namespace Enterprise.Client.EDI.ServiceTasks.ApplicationProcessing.ApplicationHandler
{
	class RedirectUrlHandler : BaseApplicationHandler
	{
		readonly AzureApplicationManagement azureApplicationManagement;
		readonly ILogger logger;

		public RedirectUrlHandler(AzureApplicationManagement azureApplicationManagement, ILogger logger)
		{
			this.azureApplicationManagement = azureApplicationManagement;
			this.logger = logger;
		}

		protected override void HandleCore(EdiIdentityApplication application)
		{
			OverrideRedirectUrlCore(application);

			ZExceptionReporting.ProcessWithConcurrencyHandling(application.Factory.Save, () =>
			{
				application.Reload();
				OverrideRedirectUrlCore(application);
			});

			logger.Log(LogType.Information, $"Synchronized redirect URLs for Azure Application '{application.IDA_ApplicationName}' onto the tenant '{application.Tenant.IDT_Name}'");
		}

		public override bool Applicable(EdiIdentityApplication application)
		{
			return application.NeedSyncRedirectUrls();
		}

		void OverrideRedirectUrlCore(EdiIdentityApplication application)
		{
			var (webAppUrls, spaAppUrls, installedAppUrls) = GetRedirectUrls(application);
			azureApplicationManagement.OverrideRedirectUrl(application.IDA_ClientID, webAppUrls, spaAppUrls, installedAppUrls);

			if (application.NeedRollbackAzureApplication())
			{
				application.IDA_RedirectUrlStatus = EdiIdentityApplicationRedirectUrlStatus.Codes.None;
				application.RedirectUrls.DeleteAll();
			}
			else
			{
				application.IDA_RedirectUrlStatus = EdiIdentityApplicationRedirectUrlStatus.Codes.Scheduled;
				application.IDA_RedirectUrlLastSyncTimeUtc = ZDateTime.UtcNow;
			}
		}

		(string[] WebAppUrls, string[] SpaAppUrls, string[] InstalledAppUrls) GetRedirectUrls(EdiIdentityApplication application)
		{
			if (application.NeedRollbackAzureApplication())
			{
				return (Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
			}

			var redirectUrls = application.RedirectUrls;

			var webAppUrls = redirectUrls
				.Where(url => url.IAR_RedirectType == EdiIdentityRedirectType.Codes.Web)
				.Select(url => url.IAR_RedirectUrl.ToString()).ToArray();

			var spaAppUrls = redirectUrls
				.Where(url => url.IAR_RedirectType == EdiIdentityRedirectType.Codes.SinglePage)
				.Select(url => url.IAR_RedirectUrl.ToString()).ToArray();

			var installedAppUrls = redirectUrls
				.Where(url => url.IAR_RedirectType == EdiIdentityRedirectType.Codes.InstalledClient)
				.Select(url => url.IAR_RedirectUrl.ToString()).ToArray();

			return (webAppUrls, spaAppUrls, installedAppUrls);
		}
	}
}
