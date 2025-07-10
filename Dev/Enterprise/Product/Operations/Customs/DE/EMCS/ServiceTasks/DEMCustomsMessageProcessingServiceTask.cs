using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DE.EMCS.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.DE.ServiceTasks.ServiceTaskApplicationCodeList.Codes.DEMMessageProcessing,
	Enterprise.Customs.DE.ServiceTasks.ServiceTaskApplicationCodeList.Descriptions.DEMMessageProcessing,
	"DEC",
	typeof(Enterprise.Customs.DE.EMCS.ServiceTasks.DEMCustomsMessageProcessingServiceTask),
	MinimumPeriod = "60Seconds",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Germany,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.DE.ServiceTasks.ServiceTaskApplicationCodeList.Codes.DEMMessageProcessing,
	EDIMessageSchema.Constants.TableName,
	new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.DECustomsEmcsSystem
	},
	"DE Emcs Message Processing"
	)]

namespace Enterprise.Customs.DE.EMCS.ServiceTasks
{
	public class DEMCustomsMessageProcessingServiceTask : Customs.ServiceTasks.BranchMessageProcessorService
	{
		protected override IEnumerable<ZString> MessageTypes => Enumerable.Empty<ZString>();

		protected override IEnumerable<ZString> ApplicationCodes => new ZString[] { EDIMessage.ApplicationCodes.DECustomsEmcsSystem };

		protected override BranchCustomsMessageProcessor GetNewBranchCustomsMessageProcessor() => new EMCSBranchCustomsMessageProcessor();
	}
}
