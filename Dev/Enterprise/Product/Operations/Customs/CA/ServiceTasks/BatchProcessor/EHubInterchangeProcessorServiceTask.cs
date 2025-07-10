using System.Threading;
using Enterprise.Customs.CA.Business.BatchProcessor;
using Enterprise.Customs.CA.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	EHubInterchangeProcessorServiceTask.InterchangeProcessorServiceCode,
	EHubInterchangeProcessorServiceTask.InterchangeProcessorServiceName,
	MessageProcessorServiceTask.MessageServiceTaskCategory,
	typeof(EHubInterchangeProcessorServiceTask),
	MinimumPeriod = "10Seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
)]

[assembly: HostedServiceBusinessObjectBinding(EHubInterchangeProcessorServiceTask.InterchangeProcessorServiceCode,
EDIInterchangeSchema.Constants.TableName,
new[] { EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
	EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
	EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
	EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.CAACI },
	"CA Customs ACI Interchanges inbound")]
[assembly: HostedServiceBusinessObjectBinding(EHubInterchangeProcessorServiceTask.InterchangeProcessorServiceCode,
EDIInterchangeSchema.Constants.TableName,
new[] { EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
	EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
	EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
	EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.CAEXP },
		"CA Customs EXP Interchanges inbound")]
[assembly: HostedServiceBusinessObjectBinding(EHubInterchangeProcessorServiceTask.InterchangeProcessorServiceCode,
EDIInterchangeSchema.Constants.TableName,
new[] { EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
	EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
	EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
	EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.CAIMP },
		"CA Customs IMP Interchanges inbound")]
[assembly: HostedServiceBusinessObjectBinding(EHubInterchangeProcessorServiceTask.InterchangeProcessorServiceCode,
EDIInterchangeSchema.Constants.TableName,
new[] { EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
	EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
	EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
	EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.CACustoms },
		"CA Customs Interchanges inbound")]

namespace Enterprise.Customs.CA.ServiceTasks
{
	/// <summary>
	/// Canadian Customs Interchange Processor, processes interchanges from in box
	/// </summary>
	public sealed class EHubInterchangeProcessorServiceTask : ServiceProviderImplUnderDefaultBranch
	{
		public const string InterchangeProcessorServiceCode = "CAE";
		public const string InterchangeProcessorServiceName = "Canadian Customs eHub Interchange Processor";

		[HostedServiceRequirement]
		public static string CheckSendCAViaEHub() => HostedServiceRequirementAttribute.CheckValueIsNotEqualTo(eHubMessagingRegistry.Instance.SendCAViaEHub, false);

		[HostedServiceRequirement]
		public static string CheckCompanyInCanadaOrAppliesAllCountries() => BatchProcessorUtilities.CheckCompanyInCanadaOrAppliesAllCountries();

		protected override void RunTaskMain(CancellationToken token)
		{
			using (var processor = new CACInboundeHubInterchangeProcessor())
			{
				processor.Logger.OnLogInfoAdded += (log, logType) => ServiceLogger.Log(logType, log);
				processor.ExecuteBatch(token);
			}
		}
	}
}
