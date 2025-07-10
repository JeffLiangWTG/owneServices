using CargoWise.Types;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.AU.ServiceTasks.CTLMessageProcessorService.Code,
	Enterprise.Customs.AU.ServiceTasks.CTLMessageProcessorService.Name,
	"AUC",
	typeof(Enterprise.Customs.AU.ServiceTasks.CTLMessageProcessorService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Australia,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1Minute",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.AU.ServiceTasks.CTLMessageProcessorService.Code,
EDIMessageSchema.Constants.TableName,
new[]
{
	EDIMessageSchema.Constants.EM_IsActive        + "=Y",
	EDIMessageSchema.Constants.EM_ReceiveTransmit + "=RCV",
	EDIMessageSchema.Constants.EM_Status          + "=QUE",
	EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
	EDIMessageSchema.Constants.EM_ApplicationCode + "=CMR",
	EDIMessageSchema.Constants.EM_MessageType     + "=CTL"
},
	"AU Customs CMR CONTRL messages inbound")]

namespace Enterprise.Customs.AU.ServiceTasks
{
	public class CTLMessageProcessorService : MultiCompanyCustomsMessagingService
	{
		protected override ZString LegacyBatchProcessorCode
		{
			get { return ZString.Empty; }
		}

		protected override ICustomsServiceTaskProcess GetNewProcess()
		{
			return new Declaration.Business.AUCCTLMessageProcessor();
		}

		protected override string RequiredCountry
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}

		public const string Code = "AUA";
		public const string Name = "Australian Customs CTL Message Processor";
	}
}


