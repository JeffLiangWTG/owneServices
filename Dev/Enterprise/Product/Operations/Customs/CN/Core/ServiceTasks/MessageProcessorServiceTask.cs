using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	code: Enterprise.Customs.CN.ServiceTasks.ServiceTaskApplicationCodeList.Codes.CNMessageProcessingServiceTask,
	description: Enterprise.Customs.CN.ServiceTasks.ServiceTaskApplicationCodeList.Descriptions.CNMessageProcessingServiceTask,
	category: "CNC",
	type: typeof(Enterprise.Customs.CN.ServiceTasks.MessageProcessorServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.China,
	CanRunInAnyBranch = true,
	MinimumPeriod = "60Second",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: Enterprise.Customs.CN.ServiceTasks.ServiceTaskApplicationCodeList.Codes.CNMessageProcessingServiceTask,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIInterchange.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.CNSingleWindow,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	},
	queueName: Enterprise.Customs.CN.ServiceTasks.ServiceTaskApplicationCodeList.Codes.CNMessageProcessingServiceTask
)]

namespace Enterprise.Customs.CN.ServiceTasks
{
	public class MessageProcessorServiceTask : BranchMessageProcessorService
	{
		protected override IEnumerable<ZString> MessageTypes => Enumerable.Empty<ZString>();

		protected override IEnumerable<ZString> ApplicationCodes
		{
			get
			{
				yield return GenericMessageDeliveryInterchangeTypeList.Codes.CNSingleWindow;
			}
		}

		protected override BranchCustomsMessageProcessor GetNewBranchCustomsMessageProcessor()
		{
			return new CSWBranchCustomsMessageProcessor();
		}

		[HostedServiceRequirement]
		public static string CheckCNSWClientSetting() => ServiceTaskEnvironmentChecker.CheckCNSWClientSetting();
	}
}
