using CargoWise.Types;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.AU.ServiceTasks.ConcurrentMessageProcessorService.Code,
	Enterprise.Customs.AU.ServiceTasks.ConcurrentMessageProcessorService.Name,
	"AUC",
	typeof(Enterprise.Customs.AU.ServiceTasks.ConcurrentMessageProcessorService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Australia,
	CanRunInAnyBranch = true,
	AllowsMultipleInstances = true,
	MinimumPeriod = "1Minute",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
)]

[assembly: HostedServiceBusinessObjectBinding(
	Enterprise.Customs.AU.ServiceTasks.ConcurrentMessageProcessorService.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=RCV",
		EDIMessageSchema.Constants.EM_Status          + "=QUE",
		EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=CMR",
		EDIMessageSchema.Constants.EM_MessageType     + "=SEI"
	},
	Enterprise.Customs.AU.ServiceTasks.ConcurrentMessageProcessorService.QueueNamePrefix + " (CMR/SEI)"
)]

[assembly: HostedServiceBusinessObjectBinding(
	Enterprise.Customs.AU.ServiceTasks.ConcurrentMessageProcessorService.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=RCV",
		EDIMessageSchema.Constants.EM_Status          + "=QUE",
		EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=CMR",
		EDIMessageSchema.Constants.EM_MessageType     + "=URR"
	},
	Enterprise.Customs.AU.ServiceTasks.ConcurrentMessageProcessorService.QueueNamePrefix + " (CMR/URR)"
)]

[assembly: HostedServiceBusinessObjectBinding(
	Enterprise.Customs.AU.ServiceTasks.ConcurrentMessageProcessorService.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=RCV",
		EDIMessageSchema.Constants.EM_Status          + "=QUE",
		EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=CMR",
		EDIMessageSchema.Constants.EM_MessageType     + "=URE"
	},
	Enterprise.Customs.AU.ServiceTasks.ConcurrentMessageProcessorService.QueueNamePrefix + " (CMR/URE)"
)]

namespace Enterprise.Customs.AU.ServiceTasks
{
	public class ConcurrentMessageProcessorService : MultiCompanyCustomsMessagingService
	{
		protected override ZString LegacyBatchProcessorCode
		{
			get { return ZString.Empty; }
		}

		protected override ICustomsServiceTaskProcess GetNewProcess()
		{
			return new Declaration.Business.AUCConcurrentMessageProcessor();
		}

		protected override string RequiredCountry
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}

		public const string Code = "AUZ";
		public const string Name = "Australian Customs Message Processor (Concurrent)";
		public const string QueueNamePrefix = "Australian Customs Message Processor Queue";
	}
}


