using CargoWise.Types;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.AU.ServiceTasks.MessageProcessorService.Code,
	Enterprise.Customs.AU.ServiceTasks.MessageProcessorService.Name,
	"AUC",
	typeof(Enterprise.Customs.AU.ServiceTasks.MessageProcessorService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Australia,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1Minute",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.AU.ServiceTasks.MessageProcessorService.Code,
EDIMessageSchema.Constants.TableName,
new[]
{
	EDIMessageSchema.Constants.EM_IsActive        + "=Y",
	EDIMessageSchema.Constants.EM_ReceiveTransmit + "=RCV",
	EDIMessageSchema.Constants.EM_Status          + "=QUE",
	EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
	EDIMessageSchema.Constants.EM_ApplicationCode + "=CMR",
	EDIMessageSchema.Constants.EM_MessageType     + "!=CRS",
	EDIMessageSchema.Constants.EM_MessageType     + "!=SEI",
	EDIMessageSchema.Constants.EM_MessageType     + "!=URR",
	EDIMessageSchema.Constants.EM_MessageType     + "!=URE",
	EDIMessageSchema.Constants.EM_MessageType     + "!=CTL"
},
"AU Customs CMR messages inbound")]
[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.AU.ServiceTasks.MessageProcessorService.Code,
EDIMessageSchema.Constants.TableName,
new[]
{
	EDIMessageSchema.Constants.EM_IsActive        + "=Y",
	EDIMessageSchema.Constants.EM_ReceiveTransmit + "=RCV",
	EDIMessageSchema.Constants.EM_Status          + "=QUE",
	EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
	EDIMessageSchema.Constants.EM_ApplicationCode + "=PRA"
},
"AU Customs PRA messages inbound")]
[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.AU.ServiceTasks.MessageProcessorService.Code,
EDIMessageSchema.Constants.TableName,
new[]
{
	EDIMessageSchema.Constants.EM_IsActive        + "=Y",
	EDIMessageSchema.Constants.EM_ReceiveTransmit + "=RCV",
	EDIMessageSchema.Constants.EM_Status          + "=QUE",
	EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
	EDIMessageSchema.Constants.EM_ApplicationCode + "=EXD"
},
"AU Customs ExDocs messages inbound")]
[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.AU.ServiceTasks.MessageProcessorService.Code,
EDIMessageSchema.Constants.TableName,
new[]
{
	EDIMessageSchema.Constants.EM_IsActive        + "=Y",
	EDIMessageSchema.Constants.EM_ReceiveTransmit + "=RCV",
	EDIMessageSchema.Constants.EM_Status          + "=QUE",
	EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
	EDIMessageSchema.Constants.EM_ApplicationCode + "=NEX"
},
"AU Customs NexDocs messages inbound")]
[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.AU.ServiceTasks.MessageProcessorService.Code,
EDIMessageSchema.Constants.TableName,
new[]
{
	EDIMessageSchema.Constants.EM_IsActive        + "=Y",
	EDIMessageSchema.Constants.EM_ReceiveTransmit + "=RCV",
	EDIMessageSchema.Constants.EM_Status          + "=QUE",
	EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
	EDIMessageSchema.Constants.EM_ApplicationCode + "=COL"
},
"AU Customs COLS messages inbound")]

namespace Enterprise.Customs.AU.ServiceTasks
{
	public class MessageProcessorService : MultiCompanyCustomsMessagingService
	{
		protected override ZString LegacyBatchProcessorCode
		{
			get { return ZString.Empty; }
		}

		protected override ICustomsServiceTaskProcess GetNewProcess()
		{
			return new Declaration.Business.AUCMessageProcessor();
		}

		protected override string RequiredCountry
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}

		public const string Code = "AUP";
		public const string Name = "Australian Customs Message Processor";
	}
}


