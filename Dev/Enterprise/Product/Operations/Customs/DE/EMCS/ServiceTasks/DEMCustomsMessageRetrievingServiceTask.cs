using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.DE.EMCS.Business;
using Enterprise.Customs.DE.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(ServiceTaskApplicationCodeList))]

[assembly: HostedService(ServiceTaskApplicationCodeList.Codes.DEMMessageRetrieving,
	ServiceTaskApplicationCodeList.Descriptions.DEMMessageRetrieving,
	"DEC",
	typeof(Enterprise.Customs.DE.EMCS.ServiceTasks.DEMCustomsMessageRetrievingServiceTask),
	MinimumPeriod = "60Seconds",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Germany,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(ServiceTaskApplicationCodeList.Codes.DEMMessageRetrieving,
	EDIInterchangeSchema.Constants.TableName,
	new[] {
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.DECustomsEmcsSystem,
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GenericMessageDelivery
	},
	ServiceTaskApplicationCodeList.Descriptions.DEMMessageRetrieving
	)]

namespace Enterprise.Customs.DE.EMCS.ServiceTasks
{
	public class DEMCustomsMessageRetrievingServiceTask : Customs.ServiceTasks.GMDCustomsMessagingService
	{
		protected override IEnumerable<ZString> InterchangeTypes => new ZString[] { GenericMessageDeliveryInterchangeTypeList.Codes.DECustomsEmcsSystem };
		protected override GMDInboundInterchangeProcessor GetNewGMDInboundInterchangeProcessor() => new DEMInboundInterchangeProcessor();
	}
}
