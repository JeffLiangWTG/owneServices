using System.Threading;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using EDIInterchange = Enterprise.Customs.KR.Business.EDIInterchange;
using EDIMessage = Enterprise.Customs.KR.Business.EDIMessage;
using GlbCompanyWrapper = Enterprise.Customs.KR.Business.GlbCompanyWrapper;

[assembly: HostedService(
	Enterprise.Customs.KR.ServiceTasks.InBoundServiceTask.Code,
	"KR Customs Message Processor",
	Enterprise.Customs.KR.ServiceTasks.OutBoundServiceTask.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.KR.ServiceTasks.InBoundServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.KoreaSouth,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.KR.ServiceTasks.InBoundServiceTask.Code,
	EDIInterchangeSchema.Constants.TableName,
	new[] { EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
				 EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
				 EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
				 EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIMessage.ApplicationCodes.KRCustoms },
	"KR Customs Interchanges Inbound"
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.KR.ServiceTasks.InBoundServiceTask.Code,
	EDIMessageSchema.Constants.TableName,
	new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.KRCustoms
	},
	"KR Customs Messages Inbound"
	)]

namespace Enterprise.Customs.KR.ServiceTasks
{
	public class InBoundServiceTask : CustomsServiceTask
	{
		public const string MessageServiceTaskCategory = "KRC";
		public const string Code = "KRI";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public const int DefaultPeriodInMinutes = 1;

		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var company in GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.KoreaSouth))
			{
				if (GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company).IsValidForMessaging)
				{
					token.ThrowIfCancellationRequested();
					using (DisposableEnvironment.ForCompany(company.GC_Code))
					{
						RunTaskHandleEmailSendFailure(() =>
						{
							((IInboundInterchangeProcessor)new KRCInboundInterchangeProcessor(Logger)).Execute(token);
							new KRCIncomingMessageProcessor(Logger).ExecuteBatch(token);
						});
					}
				}
			}
		}
	}
}
