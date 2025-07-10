using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using Enterprise.Client.EDI.ServiceTasks;
using Enterprise.Client.EDI.ServiceTasks.ApplicationProcessing;
using Enterprise.Client.EDI.ServiceTasks.ApplicationProcessing.ApplicationHandler;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(ApplicationProcessingServiceTask.Code,
	ApplicationProcessingServiceTask.Description,
	"CSP",
	typeof(ApplicationProcessingServiceTask),
	MinimumPeriod = "1hour",
	DefaultScheduleRunEvery = "1hour",
	ActiveByDefault = true,
	CanRunInAnyBranch = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	ClientSpecificCode = Clients.EDI,
	ServiceTaskCode = ApplicationProcessingServiceTask.Code,
	Table = EdiIdentityApplicationSchema.Constants.TableName,
	Predicates = new[]
	{
		EdiIdentityApplicationSchema.Constants.IDA_IsRollback + "=Y",
		EdiIdentityApplicationSchema.Constants.IDA_IsActive + "=Y"
	},
	QueueName = "Rollback Applications Queue"
)]

[assembly: HostedServiceBusinessObjectBinding(
	ClientSpecificCode = Clients.EDI,
	ServiceTaskCode = ApplicationProcessingServiceTask.Code,
	Table = EdiIdentityApplicationSchema.Constants.TableName,
	Predicates = new[]
	{
		EdiIdentityApplicationSchema.Constants.IDA_RedirectUrlStatus + "=" + EdiIdentityApplicationRedirectUrlStatus.Codes.Nudged
	},
	QueueName = "Nudged Applications Queue"
)]

[assembly: HostedServiceBusinessObjectBinding(
	ClientSpecificCode = Clients.EDI,
	ServiceTaskCode = ApplicationProcessingServiceTask.Code,
	Table = EdiIdentityApplicationSchema.Constants.TableName,
	Predicates = new[]
	{
		EdiIdentityApplicationSchema.Constants.IDA_ClientID + "=" // Client ID is empty
	},
	QueueName = "Azure Application Initialization Queue"
)]

[assembly: HostedServiceBusinessObjectBinding(
	ClientSpecificCode = Clients.EDI,
	ServiceTaskCode = ApplicationProcessingServiceTask.Code,
	Table = EdiIdentityCertificateSchema.Constants.TableName,
	Predicates = new[]
	{
		EdiIdentityCertificateSchema.Constants.ICE_ProcessingStatus + "=" + EdiIdentityCertificateProcessingStatus.Codes.PRC,
		EdiIdentityCertificateSchema.Constants.ICE_IsActive + "=Y"
	},
	QueueName = "Processing Certificate Queue"
)]

[assembly: HostedServiceBusinessObjectBinding(
	ClientSpecificCode = Clients.EDI,
	ServiceTaskCode = ApplicationProcessingServiceTask.Code,
	Table = EdiIdentityCertificateSchema.Constants.TableName,
	Predicates = new[]
	{
		EdiIdentityCertificateSchema.Constants.ICE_ProcessingStatus + "=" + EdiIdentityCertificateProcessingStatus.Codes.CAN,
		EdiIdentityCertificateSchema.Constants.ICE_IsActive + "=Y"
	},
	QueueName = "Revoked Certificate Queue"
)]

[assembly: HostedServiceBusinessObjectBinding(
	ClientSpecificCode = Clients.EDI,
	ServiceTaskCode = ApplicationProcessingServiceTask.Code,
	Table = LicenceDatabaseSchema.Constants.TableName,
	Predicates = new[]
	{
		LicenceDatabaseSchema.Constants.LD_IsActive + "=N",
		LicenceDatabaseSchema.Constants.LD_Product + "=CW1"
	},
	QueueName = "Applications with Inactive Licenses Queue"
)]

namespace Enterprise.Client.EDI.ServiceTasks
{
	public class ApplicationProcessingServiceTask : ServiceProviderImpl
	{
		public const string Code = "AAP";
		public const string Description = "Identity Application Processing Service Task";

		public ApplicationProcessingServiceTask()
		{
			this.azureApplicationManagementCreator = new AzureApplicationManagementCreator();
		}

		internal ApplicationProcessingServiceTask(IAzureApplicationManagementCreator azureApplicationManagementCreator,
			ILogger logger)
		{
			this.azureApplicationManagementCreator = azureApplicationManagementCreator;
			ServiceLogger = logger;
		}

		bool CheckCanRunServiceTask(out string message)
		{
			message = string.Empty;

			if (EDIDataRegistry.Instance.AzureApplicationManagementTenantID.Value.IsNullOrEmpty())
			{
				message += $"{EDIDataRegistry.Instance.AzureApplicationManagementTenantID.GetLocationInEnglish()}\r\n";
			}

			if (EDIDataRegistry.Instance.AzureApplicationManagementClientID.Value.IsNullOrEmpty())
			{
				message += $"{EDIDataRegistry.Instance.AzureApplicationManagementClientID.GetLocationInEnglish()}\r\n";
			}

			var systemToSystemCertificate = SystemDataRegistry.Instance.SystemToSystemCertificate.Value;
			if (systemToSystemCertificate.CertificateBytes.Length == 0)
			{
				message += $"{SystemDataRegistry.Instance.SystemToSystemCertificate.GetLocationInEnglish()} (hidden registry item)";
			}

			return message.IsNullOrEmpty();
		}

		public override void RunTask(CancellationToken token)
		{
			if (!CheckCanRunServiceTask(out var message))
			{
				ServiceLogger.Log(LogType.Warning, $"{Description} cannot run because the following registries don't have valid value.\r\n{message}");
				return;
			}

			ServiceLogger.Log(LogType.Information, Description + " started");
			Run(token);
			ServiceLogger.Log(LogType.Information, Description + " finished");
		}

		void Run(CancellationToken token)
		{
			var sqlText = $@"SELECT DISTINCT {EdiIdentityApplicationSchema.Constants.PK}
FROM {EdiIdentityApplicationSchema.Constants.SqlSchemaName}.{EdiIdentityApplicationSchema.Constants.TableName}
LEFT JOIN {EdiIdentityCertificateSchema.Constants.SqlSchemaName}.{EdiIdentityCertificateSchema.Constants.TableName}
	ON {EdiIdentityApplicationSchema.Constants.PK} = {EdiIdentityCertificateSchema.Constants.ICE_IDA}
LEFT JOIN {LicenceDatabaseSchema.Constants.SqlSchemaName}.{LicenceDatabaseSchema.Constants.TableName}
	ON {LicenceDatabaseSchema.Constants.PK} = {EdiIdentityApplicationSchema.Constants.IDA_LD}
WHERE {EdiIdentityApplicationSchema.Constants.IDA_IsActive} = 1 AND {EdiIdentityApplicationSchema.Constants.IDA_ProcessingStatus} != '{EdiIdentityApplicationProcessingStatus.Codes.FAL}'
AND (
{EdiIdentityApplicationSchema.Constants.IDA_ClientID} = ''

OR {LicenceDatabaseSchema.Constants.LD_IsActive} = 0

OR {EdiIdentityApplicationSchema.Constants.IDA_RedirectUrlStatus} = '{EdiIdentityApplicationRedirectUrlStatus.Codes.Nudged}'
OR ({EdiIdentityApplicationSchema.Constants.IDA_RedirectUrlStatus} ='{EdiIdentityApplicationRedirectUrlStatus.Codes.Scheduled}' AND
{EdiIdentityApplicationSchema.Constants.IDA_RedirectUrlLastSyncTimeUtc} < '{ZDateTime.UtcNow.AddDays(0 - EDIDataRegistry.Instance.AzureApplicationRedirectUrlSyncInterval.Value).ToISO8601String()}')

OR {EdiIdentityApplicationSchema.Constants.IDA_IsRollback} = 1

OR ({EdiIdentityCertificateSchema.Constants.ICE_IsActive} = 1 AND (
{EdiIdentityCertificateSchema.Constants.ICE_ProcessingStatus} = '{EdiIdentityCertificateProcessingStatus.Codes.PRC}' OR
{EdiIdentityCertificateSchema.Constants.ICE_IsCertificateRevoked} = 1 OR
{EdiIdentityCertificateSchema.Constants.ICE_CertificateExpiryDate} < '{ZDateTime.UtcNow.ToISO8601String()}')))

OR ({EdiIdentityApplicationSchema.Constants.IDA_IsActive} = 0 AND {LicenceDatabaseSchema.Constants.LD_IsActive} = 1)";

			var dataQuery = new ZNonPersistentDataQuery(sqlText);
			var applicationQueue = new DbOnlyBusinessObjectQueue<EdiIdentityApplication>(dataQuery);

			applicationQueue.Process((application, e) =>
			{
				e.Cancel |= token.IsCancellationRequested;
				if (!e.Cancel)
				{
					ProcessApplication(application);
				}
			}, 1);

			var message = @$"Processed {processedFailedCount + processedSuccessfulCount} application(s). {processedSuccessfulCount} {(processedSuccessfulCount > 1 ? "were" : "was")} successful and {processedFailedCount} failed";

			ServiceLogger.Log(LogType.Information, message);

			CheckGitHubActionSecret();
		}

		void ProcessApplication(EdiIdentityApplication application)
		{
			try
			{
				IApplicationHandler handler;
				if (application.IsCustomerApplication)
				{
					var applicationHandlerChainBuilder = new CustomerApplicationHandlerChainBuilder();
					handler = applicationHandlerChainBuilder.BuildApplicationHandlerChain();
				}
				else
				{
					var azureApplicationManagement = azureApplicationManagementCreator.CreateAzureApplicationManagement(application.Tenant.IDT_TenantId, application.Tenant.IDT_GraphClientId);
					var applicationHandlerChainBuilder = new AzureApplicationHandlerChainBuilder(azureApplicationManagement, ServiceLogger);
					handler = applicationHandlerChainBuilder.BuildApplicationHandlerChain(application);
				}
				handler.Handle(application);

				processedSuccessfulCount++;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				processedFailedCount++;

				application.IDA_ProcessingStatus = EdiIdentityApplicationProcessingStatus.Codes.FAL;
				application.Factory.Save();

				var message = $"Failed to process application for {application.IDA_ApplicationName}{System.Environment.NewLine}{ex}";
				ServiceLogger.Log(LogType.Error, message);
				ServiceTaskEmailNotification.SendEmailToGroup($"{Description} Process Application Error", message,
					EDIDataRegistry.Instance.CertProcessingNotificationGroup, ServiceLogger);
			}
		}

		void CheckGitHubActionSecret()
		{
			var nextRunDate = EDIDataRegistry.Instance.GithubActionSecretNextCheckDate.Value;
			if (nextRunDate < ZDateTime.UtcNow)
			{
				try
				{
					var azureManagementTenantId = EDIDataRegistry.Instance.AzureApplicationManagementTenantID.Value;
					var azureManagementClientId = EDIDataRegistry.Instance.AzureApplicationManagementClientID.Value;
					var azureApplicationManagement = azureApplicationManagementCreator.CreateAzureApplicationManagement(azureManagementTenantId, azureManagementClientId);
					var expirationDate = azureApplicationManagement.GetApplicationSecretExpirationDate(azureManagementClientId);

					if (expirationDate < ZDateTime.UtcNow.AddMonths(1))
					{
						var body = $"Github Action Secret is about to expire or has already expired, please renew it. Expiration date: {expirationDate:d}";
						ServiceTaskEmailNotification.SendEmailToGroup("Github Action Secret Notification", body, EDIDataRegistry.Instance.CertProcessingNotificationGroup, ServiceLogger);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var message = $"Failed in Github action secret strategy: {ex}";

					ServiceLogger.Log(LogType.Error, message);
					ServiceTaskEmailNotification.SendEmailToGroup($"{EDICertificateProcessingServiceTask.Description} Github Action Secret Error", message, EDIDataRegistry.Instance.CertProcessingNotificationGroup, ServiceLogger);
				}

				EDIDataRegistry.Instance.GithubActionSecretNextCheckDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(7).ToDateTime());
			}

			ServiceLogger.Log(LogType.Information, "Checked the secret expiry date for Github Action");
		}

		readonly IAzureApplicationManagementCreator azureApplicationManagementCreator;

		int processedFailedCount;
		int processedSuccessfulCount;
	}
}
