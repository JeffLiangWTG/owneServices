using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.AsycudaCustoms.ServiceTasks.AsycudaGMDCustomsMessagingServiceTask.Code,
	Enterprise.Customs.AsycudaCustoms.ServiceTasks.AsycudaGMDCustomsMessagingServiceTask.FriendlyName,
	Enterprise.Customs.AsycudaCustoms.ServiceTasks.AsycudaGMDCustomsMessagingServiceTask.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.AsycudaCustoms.ServiceTasks.AsycudaGMDCustomsMessagingServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.AsycudaCustoms.ServiceTasks.AsycudaGMDCustomsMessagingServiceTask.Code,
	EDIInterchangeSchema.Constants.TableName,
	new[] { EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GenericMessageDelivery,
		EDIInterchangeSchema.Constants.EI_InterchangeType + "=ZZA" },
	"Asycuda GMD interchanges inbound"
)]

namespace Enterprise.Customs.AsycudaCustoms.ServiceTasks
{
	public class AsycudaGMDCustomsMessagingServiceTask : GMDCustomsMessagingService
	{
		public const string MessageServiceTaskCategory = "ESV";
		public const string Code = "AMP";
		public const string FriendlyName = "Asycuda Incoming GMD Message Processor"; // Service Task Description

		protected override IEnumerable<ZString> InterchangeTypes => fInterchangeTypes;

		readonly List<ZString> fInterchangeTypes = new List<ZString>(new ZString[] { "ZZA" });

		protected override GMDInboundInterchangeProcessor GetNewGMDInboundInterchangeProcessor() => new AsycudaGMDInboundInterchangeProcessor(InterchangeTypes, ServiceLogger);
	}
}
