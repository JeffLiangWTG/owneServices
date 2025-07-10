using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.AU.ServiceTasks.InterchangeProcessorService.Code,
	Enterprise.Customs.AU.ServiceTasks.InterchangeProcessorService.Name,
	"AUC",
	typeof(Enterprise.Customs.AU.ServiceTasks.InterchangeProcessorService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Australia,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1Minute",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.AU.ServiceTasks.InterchangeProcessorService.Code,
EDIInterchangeSchema.Constants.TableName,
new[] { EDIInterchangeSchema.Constants.EI_IsActive + "=Y", EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=RCV", EDIInterchangeSchema.Constants.EI_Status + "=QUE",
EDIInterchangeSchema.Constants.EI_ApplicationCode + "=CMR" },
	"AU Customs CMR interchanges inbound")]
[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.AU.ServiceTasks.InterchangeProcessorService.Code,
EDIInterchangeSchema.Constants.TableName,
new[] { EDIInterchangeSchema.Constants.EI_IsActive + "=Y", EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=RCV", EDIInterchangeSchema.Constants.EI_Status + "=QUE",
EDIInterchangeSchema.Constants.EI_ApplicationCode + "=EXD" },
	"AU Customs ExDocs interchanges inbound")]
[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.AU.ServiceTasks.InterchangeProcessorService.Code,
EDIInterchangeSchema.Constants.TableName,
new[] { EDIInterchangeSchema.Constants.EI_IsActive + "=Y", EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=RCV", EDIInterchangeSchema.Constants.EI_Status + "=QUE",
EDIInterchangeSchema.Constants.EI_ApplicationCode + "=PRA" },
	"AU Customs PRA interchanges inbound")]

namespace Enterprise.Customs.AU.ServiceTasks
{
	public class InterchangeProcessorService : MultiCompanyCustomsMessagingService
	{
		protected override ZString LegacyBatchProcessorCode
		{
			get { return ZString.Empty; }
		}

		protected override ICustomsServiceTaskProcess GetNewProcess()
		{
			return new AUCInboundInterchangeProcessor(GetNewLogger());
		}

		protected override string RequiredCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.Australia; }
		}
		public const string Code = "AUD";
		public const string Name = "Australian Customs Interchange Processor";
	}
}


