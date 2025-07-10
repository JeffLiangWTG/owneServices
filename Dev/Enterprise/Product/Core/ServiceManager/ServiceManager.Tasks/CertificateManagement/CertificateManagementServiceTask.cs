using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.SystemToSystemTrust;
using CargoWise.SystemToSystemTrust.DataContracts;
using CargoWise.SystemToSystemTrust.Extensions;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Tasks.CertificateManagement;
using Newtonsoft.Json;
using ServiceManager.Integration.ServiceTasks.CW;
using WTG.TrustedMessaging.MyAccount.Models;

[assembly: HostedService(
	"TCM",
	"System To System Trust Certificate Management",
	"SYS",
	typeof(CertificateManagementServiceTask),
	AllowsMultipleInstances = false,
	IsMandatory = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1day",
	MaximumPeriod = "1week",
	DefaultScheduleRunEvery = "1day",
	ActiveByDefault = true)]

namespace Enterprise.ServiceManager.Tasks.CertificateManagement
{
	public class CertificateManagementServiceTask : ServiceProviderImpl
	{
		readonly ISystemToSystemTrustApiHelper apiHelper;
		readonly IAuthenticationService authenticationService;
		readonly IApplicationRedirectUrlProcessor redirectUrlProcessor;
		readonly ITokenServicesFactory tokenServicesFactory;
		readonly ICertificateManager certificateManager;

		public CertificateManagementServiceTask() : this(new SystemToSystemTrustApiHelper(), new AuthenticationService(), new ApplicationRedirectUrlProcessor(), ObjectFactory.Get<ITokenServicesFactory>())
		{
		}

		CertificateManagementServiceTask(ISystemToSystemTrustApiHelper systemToSystemTrustApiHelper, IAuthenticationService authenticationService, IApplicationRedirectUrlProcessor applicationRedirectUrlProcessor, ITokenServicesFactory tokenServicesFactory)
			: this(new CertificateManager(systemToSystemTrustApiHelper, authenticationService), systemToSystemTrustApiHelper, authenticationService, applicationRedirectUrlProcessor, tokenServicesFactory)
		{
		}

		internal CertificateManagementServiceTask(
			ICertificateManager certificateManager,
			ISystemToSystemTrustApiHelper systemToSystemTrustApiHelper,
			IAuthenticationService authenticationService,
			IApplicationRedirectUrlProcessor applicationRedirectUrlProcessor,
			ITokenServicesFactory tokenServicesFactory)
		{
			this.certificateManager = certificateManager ?? throw new ArgumentNullException(nameof(certificateManager));
			apiHelper = systemToSystemTrustApiHelper ?? throw new ArgumentNullException(nameof(systemToSystemTrustApiHelper));
			this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
			redirectUrlProcessor = applicationRedirectUrlProcessor ?? throw new ArgumentNullException(nameof(applicationRedirectUrlProcessor));
			this.tokenServicesFactory = tokenServicesFactory;
			this.tokenServicesFactory = tokenServicesFactory ?? throw new ArgumentNullException(nameof(tokenServicesFactory));
		}

		public override void RunTask(CancellationToken token)
		{
			RunTaskAsync(token).Wait(token);
		}

		async Task RunTaskAsync(CancellationToken token)
		{
			try
			{
				if (!CanRun())
				{
					return;
				}

				Log(LogType.Information, "Task started.");

				Log(LogType.Information, $"SystemToSystemTrust api endpoint: {apiHelper.SystemToSystemTrustApiEndpoint}");

				var tokenConfigWriterService = tokenServicesFactory.GetTokenConfigWriterService();
				var isDataValid = await ValidateS2STRegistryValueAsync(tokenConfigWriterService, token).ConfigureAwait(false);
				if (isDataValid)
				{
					var valid = await ProcessCertificateAsync(tokenConfigWriterService, token).ConfigureAwait(false);
					if (valid)
					{
						var needProcessRedirectUrls = await ProcessOIDCClientId(token).ConfigureAwait(false);
						if (needProcessRedirectUrls)
						{
							await ProcessRedirectUrlsAsync(token).ConfigureAwait(false);
						}
					}
					else
					{
						Log(LogType.Information, "Skip sending redirect url due to no client info.");
					}
				}

				Log(LogType.Information, "Task completed.");
			}
			catch (Exception ex) when (
				ex is SystemToSystemTrustCertificateManagementException
				|| ex is CertificateManagementException
				|| ex is ArgumentNullException
				|| ex is InvalidOperationException)
			{
				ServiceLogger.Log(LogType.Error, ex.Message, ex);
			}
		}

		async Task<bool> ValidateS2STRegistryValueAsync(ITokenConfigWriterService tokenConfigWriterService, CancellationToken cancellationToken)
		{
			var clientId = SystemToSystemTrustRegistryItemValue.ClientId;

			if (string.IsNullOrEmpty(clientId))
			{
				return true;
			}

			if (CheckIfApplicationNotExists(clientId))
			{
				await ResetAccessTokenAsync(
					tokenConfigWriterService,
					$"Reset the SystemToSystemTrustInfo because the application with client id '{clientId}' does not exist.",
					cancellationToken);
				return false;
			}

			if (SystemToSystemTrustRegistryItemValue.CertificateBytes == null || SystemToSystemTrustRegistryItemValue.CertificateBytes.Length == 0)
			{
				return true;
			}

			if (certificateManager.IsExpired(SystemToSystemTrustRegistryItemValue.CertificateBytes))
			{
				await ResetAccessTokenAsync(
					tokenConfigWriterService,
					"Reset the SystemToSystemTrustInfo because the certificate has expired.",
					cancellationToken);
				return false;
			}

			var databaseNumberInApplication = await certificateManager.LoadDatabaseNumberByClientIdAsync(cancellationToken).ConfigureAwait(false);
			var databaseNumberInRegistry = RegKey.DatabaseNumber;
			if (string.IsNullOrEmpty(databaseNumberInApplication) || databaseNumberInApplication != databaseNumberInRegistry.ToString())
			{
				await ResetAccessTokenAsync(
					tokenConfigWriterService,
					$"Reset the SystemToSystemTrustInfo because the database number in application '{databaseNumberInApplication}' does not match registry '{databaseNumberInRegistry}'.",
					cancellationToken);
				return false;
			}

			return true;
		}

		async Task ResetAccessTokenAsync(ITokenConfigWriterService tokenConfigWriterService, string message, CancellationToken cancellationToken)
		{
			Log(LogType.Warning, message);
			var resetAccessTokenRequest = new ResetAccessTokenRequest();
			_ = await tokenConfigWriterService.ResetAccessTokenAsync(resetAccessTokenRequest, cancellationToken).ConfigureAwait(false);
		}

		async Task<bool> ProcessCertificateAsync(ITokenConfigWriterService tokenConfigWriterService, CancellationToken cancellationToken)
		{
			var systemToSystemTrustInfo = SystemToSystemTrustRegistryItemValue;

			var operationId = systemToSystemTrustInfo.OperationId;
			if (string.IsNullOrEmpty(operationId))
			{
				bool isTrustInfoSetup = systemToSystemTrustInfo.IsSetUp();
				if (isTrustInfoSetup && !certificateManager.IsAboutToExpire(systemToSystemTrustInfo.CertificateBytes))
				{
					Log(LogType.Information, "Certificate is valid and current.");
					return true;
				}

				Log(
					LogType.Information,
					isTrustInfoSetup ? "Certificate expires in less than 2 months. Starting rollover certificate request." : "Starting initial certificate request.");

				var request = new PrepareNewCertificateRequest();
				var response = await tokenConfigWriterService.PrepareNewCertificateAsync(request, cancellationToken).ConfigureAwait(false);
				var csr = response?.Csr;
				if (isTrustInfoSetup)
				{
					Log(LogType.Debug, "Requesting new certificate with access token.");
					operationId = certificateManager.RolloverCertificate(systemToSystemTrustInfo.ClientId, csr);
					Log(LogType.Information, "Certificate rollover requested.");
				}
				else
				{
					Log(LogType.Debug, "Requesting new certificate with password.");
					operationId = certificateManager.RequestCertificate(csr);
					Log(LogType.Information, "New certificate requested.");
				}

				var request2 = new SetOperationIdRequest(csr, operationId);
				_ = await tokenConfigWriterService.SetOperationIdAsync(request2, cancellationToken).ConfigureAwait(false);

				Thread.Sleep(apiHelper.SecondsDelayedBetweenRequests);
			}

			Log(LogType.Information, "Starting certificate download.");

			var certificateDownloaded = false;
			for (var i = 0; i < 3; i++)
			{
				var (tenantId, clientId, certificate, _) = certificateManager.DownloadCertificate(operationId);

				if (clientId.IsNullOrEmpty() || tenantId.IsNullOrEmpty() || certificate.IsNullOrEmpty())
				{
					Log(LogType.Information, $"Certificate is currently being processed. Download of the certificate will be attempted after {apiHelper.SecondsDelayedBetweenRequests.Seconds} seconds.");
					Thread.Sleep(apiHelper.SecondsDelayedBetweenRequests);
				}
				else
				{
					var request = new SetNewCertificateCredentialsRequest(operationId, tenantId, clientId, certificate);
					await tokenConfigWriterService.SetNewCertificateCredentialsAsync(request, cancellationToken).ConfigureAwait(false);
					certificateDownloaded = true;
					Log(LogType.Information, "New certificate acquired.");
					break;
				}
			}

			if (!certificateDownloaded)
			{
				Log(LogType.Information, "Certificate is still being processed after 3 retries. Download of the certificate will be attempted next scheduled run.");
			}
			return certificateDownloaded;
		}

		async Task ProcessRedirectUrlsAsync(CancellationToken cancellationToken)
		{
			await redirectUrlProcessor.ProcessAsync(apiHelper, authenticationService, ServiceLogger, cancellationToken).ConfigureAwait(false);
		}

		async Task<bool> ProcessOIDCClientId(CancellationToken cancellationToken)
		{
			var oidcConfigRegistryItemValue = SystemDataRegistry.Instance.OIDCConfig.Value;
			if (!oidcConfigRegistryItemValue.IsOIDCEnabled)
			{
				Log(LogType.Information, "Skip registering OIDC because the OIDC is not enabled.");
				return false;
			}

			if (!SystemDataRegistry.Instance.IsOIDCFederatedWithWTG.Value)
			{
				Log(LogType.Information, "Skip registering OIDC because the OIDC flow is not using WTG B2C for Identity Federation.");
				return false;
			}

			var oidcApplicationRequest = new IdentityOidcApplicationRequest
			{
				AuthorityUrl = oidcConfigRegistryItemValue.AuthorityURL.ToString()
			};
			var jsonContent = JsonConvert.SerializeObject(oidcApplicationRequest);
			var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
			var accessToken = await authenticationService.GetAccessTokenAsync(cancellationToken);
			var response = apiHelper.SystemToSystemTrustApiPost("application/oidcregister", stringContent, accessToken);
			if (response.Success)
			{
				var clientId = response.Content;
				if (clientId.IsNullOrEmpty())
				{
					Log(LogType.Information, $"OIDC application is not created in Azure yet. AuthorityUrl: '{oidcApplicationRequest.AuthorityUrl}'.");
					return false;
				}

				var clientIdInRegistryItem = SystemDataRegistry.Instance.OIDCClientIDForWebApplications.Value;
				if (clientIdInRegistryItem.IsNullOrEmpty() || clientIdInRegistryItem != clientId)
				{
					SystemDataRegistry.Instance.OIDCClientIDForWebApplications.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, clientId);
					Log(LogType.Information, $"Successfully registered OIDC client ID: '{clientId}'.");
					return true;
				}

				Log(LogType.Information, $"OIDC client ID '{clientId}' is already registered and matches the value in the registry.");
				return true;
			}

			Log(LogType.Error, $"OIDC registration failed with error: {response.Content}");
			return false;
		}

		bool CanRun()
		{
			var canRun = true;

			var enabledRegistry = HostedServiceRequirementAttribute.CheckValueIsEqualTo(SystemDataRegistry.Instance.EnableCertificateManagementServiceTask, true);
			if (!string.IsNullOrEmpty(enabledRegistry))
			{
				Log(LogType.Warning, enabledRegistry);
				canRun = false;
			}

			var emptyUrlMessage = HostedServiceRequirementAttribute.CheckValueIsNotNullOrEmptyString(WebDataRegistry.Instance.CargoWiseUserPortalUrl);
			if (!string.IsNullOrEmpty(emptyUrlMessage))
			{
				Log(LogType.Warning, emptyUrlMessage);
				canRun = false;
			}

			return canRun;
		}

		IProductRegistrationKey RegKey => ObjectFactory.Get<IProductRegistration>().Key;

		ISystemToSystemTrustInfo SystemToSystemTrustRegistryItemValue
		{
			get => SystemDataRegistry.Instance.SystemToSystemCertificate.Value;
		}

		#region Helpers

		void Log(LogType type, string message) => ServiceLogger?.Log(type, message);

		bool CheckIfApplicationNotExists(string clientId)
		{
			var response = apiHelper.SystemToSystemTrustApiGet("application/" + clientId);
			return response.StatusCode == HttpStatusCode.NotFound;
		}
		#endregion
	}
}
