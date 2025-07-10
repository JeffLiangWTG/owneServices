using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.DE.ServiceTasks.ServiceTaskApplicationCodeList.Codes.DEAMessageRetrieving,
	Enterprise.Customs.DE.ServiceTasks.ServiceTaskApplicationCodeList.Descriptions.DEAMessageRetrieving,
	"DEC",
	typeof(Enterprise.Customs.DE.ServiceTasks.DEACustomsMessageRetrievingServiceTask),
	MinimumPeriod = "60Seconds",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Germany,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.DE.ServiceTasks.ServiceTaskApplicationCodeList.Codes.DEAMessageRetrieving,
	EDIInterchangeSchema.Constants.TableName,
	new[] {
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.DECustomsAtlasSystem,
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GenericMessageDelivery
	},
	Enterprise.Customs.DE.ServiceTasks.ServiceTaskApplicationCodeList.Descriptions.DEAMessageRetrieving
	)]

namespace Enterprise.Customs.DE.ServiceTasks
{
	public class DEACustomsMessageRetrievingServiceTask : Customs.ServiceTasks.GMDCustomsMessagingService
	{
		protected override IEnumerable<ZString> InterchangeTypes => new ZString[] { GenericMessageDeliveryInterchangeTypeList.Codes.DECustomsAtlasSystem };
		protected override GMDInboundInterchangeProcessor GetNewGMDInboundInterchangeProcessor() => new DEAInboundInterchangeProcessor();
	}
}
