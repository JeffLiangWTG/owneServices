using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.Customs.EU.Manifest.ICS2.ServiceTasks;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	code: ServiceTaskApplicationCodeList.Codes.EUP,
	description: ServiceTaskApplicationCodeList.Descriptions.EUP,
	category: Constants.ServiceTaskCategory,
	type: typeof(MessageProcessorServiceTask),
	MinimumPeriod = "60Seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.EUP,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.IC2,
	},
	queueName: "EU ICS2 Message Processor"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.EUP,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.PreProcessedOK,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.IC2,
	},
	queueName: "EU ICS2 Message Pre-Processing"
)]

namespace Enterprise.Customs.EU.Manifest.ICS2.ServiceTasks
{
	public class MessageProcessorServiceTask : BranchMessageProcessorService
	{
		protected override IEnumerable<ZString> MessageTypes => Enumerable.Empty<ZString>();

		protected override IEnumerable<ZString> ApplicationCodes => new ZString[] { EDIMessage.ApplicationCodes.IC2 };

		protected override BranchCustomsMessageProcessor GetNewBranchCustomsMessageProcessor() => new EUICS2BranchCustomsMessageProcessor();
	}
}
