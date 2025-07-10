using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using CargoWise.Common;
using Enterprise.Client.EDI.ServiceTasks.GraphAPI;
using WTG.AzureApplicationIntegration;

namespace Enterprise.Client.EDI.ServiceTasks.AzureApplicationProcessing
{
	public class AzureApplicationManagement
	{
		readonly string tenantId;
		readonly string graphClientId;

		public AzureApplicationManagement(string tenantId, string graphClientId)
		{
			if (string.IsNullOrEmpty(tenantId))
			{
				throw new ArgumentNullException(nameof(tenantId));
			}

			if (string.IsNullOrEmpty(graphClientId))
			{
				throw new ArgumentNullException(nameof(graphClientId));
			}

			this.tenantId = tenantId;
			this.graphClientId = graphClientId;
		}

		IGraphService GraphService => graphService ??= GetGraphService();
		IGraphService graphService;

		protected virtual IGraphService GetGraphService()
		{
			return GraphServiceFactory.Instance.CreateGraphService(tenantId, graphClientId);
		}

		public string CreateApplication(string applicationName)
		{
			return DoAzureOperationWithRetry(
				() => GraphService.CreateApplicationAsync(applicationName).GetAwaiter().GetResult(),
				"Create Application Error");
		}

		public void AddApplicationCertificates(string clientId, params X509Certificate2[] certificates)
		{
			DoAzureOperationWithRetry(() =>
			{
				GraphService.AddCertificatesToApplicationAsync(clientId, certificates).GetAwaiter().GetResult();
			}, "Add Certificates Error");
		}

		public void RenewApplicationCertificateAndKeepLatestOnly(string clientId, X509Certificate2 certificate)
		{
			DoAzureOperationWithRetry(() =>
			{
				GraphService.AddCertificateToApplicationAndRemoveOthersAsync(clientId, certificate).GetAwaiter()
					.GetResult();
			}, "Add Certificate And Remove Others Error");
		}

		public void RemoveCertificatesFromApplication(string clientId, params string[] thumbprints)
		{
			DoAzureOperationWithRetry(() =>
			{
				GraphService.RemoveCertificatesFromApplicationAsync(clientId, thumbprints).GetAwaiter().GetResult();
			}, "Remove Certificates Error");
		}

		public void OverrideRedirectUrl(string clientId, string[] webAppUrls, string[] spaAppUrls, string[] installedAppUrls)
		{
			DoAzureOperationWithRetry(
				() => GraphService
					.OverrideRedirectUrlsAsync(clientId, webAppUrls, spaAppUrls, installedAppUrls)
					.GetAwaiter().GetResult(), "Override Redirect Url Error");
		}

		public DateTime GetApplicationSecretExpirationDate(string clientId)
		{
			return DoAzureOperationWithRetry(() =>
			{
				var endDateTimeOffset = GraphService.GetMaxExpirationDateAsync(clientId).GetAwaiter().GetResult();
				return endDateTimeOffset?.UtcDateTime ?? DateTime.MinValue;
			}, "Get Github Action Secret Expiration Date Error");
		}

		void DoAzureOperationWithRetry(Action azureOperation, string errorMessage)
		{
			_ = DoAzureOperationWithRetry(() =>
			{
				azureOperation.Invoke();
				return 0;
			}, errorMessage);
		}

		T DoAzureOperationWithRetry<T>(Func<T> azureOperation, string errorMessage)
		{
			var retryCount = 3;
			var result = default(T);
			AzureApplicationManagementException exception = null;
			while (--retryCount >= 0)
			{
				try
				{
					result = azureOperation.Invoke();
					break;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (exception == null)
					{
						exception = new AzureApplicationManagementException(errorMessage, ex);
					}
					else
					{
						exception.InnerExceptions.Add(ex);
					}

					if (retryCount == 0)
					{
						throw exception;
					}

					Thread.Sleep(1000);
				}
			}

			return result;
		}
	}
}

