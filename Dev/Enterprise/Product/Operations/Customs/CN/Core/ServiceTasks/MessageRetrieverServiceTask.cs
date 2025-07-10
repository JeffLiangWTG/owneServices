using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	code: Enterprise.Customs.CN.ServiceTasks.ServiceTaskApplicationCodeList.Codes.CNMessageRetrievingServiceTask,
	description: Enterprise.Customs.CN.ServiceTasks.ServiceTaskApplicationCodeList.Descriptions.CNMessageRetrievingServiceTask,
	category: "CNC",
	type: typeof(Enterprise.Customs.CN.ServiceTasks.MessageRetrieverServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.China,
	CanRunInAnyBranch = true,
	MinimumPeriod = "60Second",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: Enterprise.Customs.CN.ServiceTasks.ServiceTaskApplicationCodeList.Codes.CNMessageRetrievingServiceTask,
	table: EDIInterchangeSchema.Constants.TableName,
	predicates: new[] {
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.CNSingleWindow,
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GenericMessageDelivery,
	},
	queueName: Enterprise.Customs.CN.ServiceTasks.ServiceTaskApplicationCodeList.Codes.CNMessageRetrievingServiceTask
)]

namespace Enterprise.Customs.CN.ServiceTasks
{
	public class MessageRetrieverServiceTask : GMDCustomsMessagingService
	{
		protected override IEnumerable<ZString> InterchangeTypes
		{
			get
			{
				yield return GenericMessageDeliveryInterchangeTypeList.Codes.CNSingleWindow;
			}
		}

		protected override GMDInboundInterchangeProcessor GetNewGMDInboundInterchangeProcessor()
		{
			return new Business.InboundInterchangeProcessor();
		}

		[HostedServiceRequirement]
		public static string CheckCNSWClientSetting() => ServiceTaskEnvironmentChecker.CheckCNSWClientSetting();
	}
}
