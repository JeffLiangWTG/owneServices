using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ServiceTasks;
using Enterprise.Client.EDI.ServiceTasks.EDICertProcessing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(EDICertificateProcessingServiceTask.Code,
	EDICertificateProcessingServiceTask.Description,
	"CSP",
	typeof(EDICertificateProcessingServiceTask),
	MinimumPeriod = "30Minutes",
	DefaultScheduleRunEvery = "30Minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	ClientSpecificCode = Clients.EDI,
	ServiceTaskCode = EDICertificateProcessingServiceTask.Code,
	Table = EdiIdentityCertificateSchema.Constants.TableName,
	Predicates = new[]
	{
		EdiIdentityCertificateSchema.Constants.ICE_ProcessingStatus + "=QUE"
	},
	QueueName = "Issue Certificates Queue"
)]

[assembly: HostedServiceBusinessObjectBinding(
	ClientSpecificCode = Clients.EDI,
	ServiceTaskCode = EDICertificateProcessingServiceTask.Code,
	Table = EdiIdentityCertificateSchema.Constants.TableName,
	Predicates = new[]
	{
		EdiIdentityCertificateSchema.Constants.ICE_ProcessingStatus + "=CAN"
	},
	QueueName = "Revoke Certificates Queue"
)]
namespace Enterprise.Client.EDI
{
	public class EDICertificateProcessingServiceTask : ServiceProviderImpl
	{
		public const string Code = "CPS";
		public const string Description = "Certificate Processing for System to System Trust Service Task";

		public EDICertificateProcessingServiceTask()
		{
		}

		internal EDICertificateProcessingServiceTask(AwsCertManagementService awsCertManagementService, ILogger logger)
		{
			this.awsCertManagementService = awsCertManagementService;
			ServiceLogger = logger;
		}

		bool CanRunServiceTask => CheckAWSCredentials();

		bool CheckAWSCredentials() => EDIDataRegistry.Instance.AWSPrivateCAListManager.Value.Cast<AWSPrivateCA>().Any(x => x.IsEnabled && !string.IsNullOrEmpty(x.AccessKey) && !string.IsNullOrEmpty(x.SecretKey));

		public override void RunTask(CancellationToken token)
		{
			if (CanRunServiceTask)
			{
				ServiceLogger.Log(LogType.Information, Description + " started");
				Run(token);
				ServiceLogger.Log(LogType.Information, Description + " finished");
			}
			else
			{
				ServiceLogger.Log(LogType.Information, string.Format(
					CultureInfo.InvariantCulture, "{0} has not been started, please check the registry items under category {1}.",
					Description,
					EDIDataRegistry.Instance.AWSPrivateCAListManager.Category));
			}
		}

		void Run(CancellationToken token)
		{
			var sqlText = $@"SELECT {EdiIdentityCertificateSchema.Constants.PK}
FROM {EdiIdentityCertificateSchema.Constants.SqlSchemaName}.{EdiIdentityCertificateSchema.Constants.TableName}
WHERE {EdiIdentityCertificateSchema.Constants.ICE_ProcessingStatus} = '{EdiIdentityCertificateProcessingStatus.Codes.QUE}'
OR ({EdiIdentityCertificateSchema.Constants.ICE_IsCertificateRevoked} = 0
AND {EdiIdentityCertificateSchema.Constants.ICE_ProcessingStatus} = '{EdiIdentityCertificateProcessingStatus.Codes.CAN}'
AND {EdiIdentityCertificateSchema.Constants.ICE_CertificateThumbprint} != '')";

			var dataQuery = new ZNonPersistentDataQuery(sqlText);
			var certificateRequestsQueue = new DbOnlyBusinessObjectQueue<EdiIdentityCertificate>(dataQuery);
			certificateRequestsQueue.Process((certificateRequest, e) =>
			{
				e.Cancel |= token.IsCancellationRequested;
				if (!e.Cancel)
				{
					ProcessCertificate(certificateRequest);
				}
			}, 1);

			ServiceLogger.Log(LogType.Information, $"Processed {processedFailedCount + processedSuccessfulCount} certificate request(s), {processedSuccessfulCount} {(processedSuccessfulCount > 1 ? "were" : "was")} successful and {processedFailedCount} failed");
		}

		void ProcessCertificate(EdiIdentityCertificate certificateRequest)
		{
			var operation = string.Empty;

			try
			{
				operation = certificateRequest.ICE_ProcessingStatus == EdiIdentityCertificateProcessingStatus.Codes.QUE ? "Issue" : "Revoke";

				if (certificateRequest.ICE_ProcessingStatus == EdiIdentityCertificateProcessingStatus.Codes.QUE)
				{
					var issuedCertificate = awsCertManagementService.IssueCertificate(certificateRequest);

					certificateRequest.ICE_CertificateData = issuedCertificate.RawData;
					certificateRequest.ICE_CertificateThumbprint = issuedCertificate.Thumbprint;
					certificateRequest.ICE_CertificateValidDate = issuedCertificate.NotBefore.ToUniversalTime();
					certificateRequest.ICE_CertificateExpiryDate = issuedCertificate.NotAfter.ToUniversalTime();
					certificateRequest.ICE_CertificateIssuedTo = ExtractCommonName(issuedCertificate.Subject, CommonName);
					certificateRequest.ICE_CertificateIssuedBy = ExtractCommonName(issuedCertificate.Issuer, CommonName);
					certificateRequest.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.PRC;
				}
				else if (certificateRequest.ICE_ProcessingStatus == EdiIdentityCertificateProcessingStatus.Codes.CAN
						 && !certificateRequest.ICE_IsCertificateRevoked)
				{
					var isCertificateRevoked = awsCertManagementService.RevokeCertificate(certificateRequest);
					certificateRequest.ICE_IsCertificateRevoked = isCertificateRevoked;
				}

				certificateRequest.Factory.Save();
				processedSuccessfulCount++;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				processedFailedCount++;
				certificateRequest.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.FAL;
				certificateRequest.Factory.Save();

				var message = $"Failed to process certificate for application {certificateRequest.Application.IDA_ApplicationName}{System.Environment.NewLine}{ex}";
				ServiceLogger.Log(LogType.Error, message);
				ServiceTaskEmailNotification.SendEmailToGroup($"{Description} {operation} Certificate Error", message, EDIDataRegistry.Instance.CertProcessingNotificationGroup, ServiceLogger);
			}
		}

		string ExtractCommonName(string input, string commonName)
		{
			var match = Regex.Match(input, $"{commonName}=\"([^\"\"]+)\"", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			var result = match.Success ?
							match.Groups[0].Value :
							input.Split(',').FirstOrDefault(x => x.Contains($"{commonName}="))?.Trim();
			return result;
		}

		const string CommonName = "CN";

		readonly AwsCertManagementService awsCertManagementService = new();

		int processedFailedCount;
		int processedSuccessfulCount;
	}
}
