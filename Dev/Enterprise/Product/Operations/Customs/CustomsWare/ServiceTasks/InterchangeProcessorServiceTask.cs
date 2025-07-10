using System.Threading;
using Enterprise.Customs.CustomsWare.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ApplicationCodeList.Codes.CustomsWare,
	"CustomsWare Status Processor",
	"EUC",
	typeof(Enterprise.Customs.CustomsWare.ServiceTasks.InterchangeProcessorServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Switzerland + "," + Enterprise.Core.Constants.CountryCodes.Ireland + "," + Enterprise.Core.Constants.CountryCodes.Netherlands + ","
							+ Enterprise.Core.Constants.CountryCodes.UnitedArabEmirates + "," + Enterprise.Core.Constants.CountryCodes.Belgium + "," + Enterprise.Core.Constants.CountryCodes.Germany,
	CanRunInAnyBranch = true,
	MinimumPeriod = "10Minutes",
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(ApplicationCodeList.Codes.CustomsWare,
	EDIInterchangeSchema.Constants.TableName,
	new[] { EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
				 EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
				 EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
				 EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + ApplicationCodeList.Codes.CustomsWare },
	"CustomsWare status interchanges inbound"
	)]
namespace Enterprise.Customs.CustomsWare.ServiceTasks
{
	public class InterchangeProcessorServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		protected sealed override void RunTaskCore(CancellationToken token)
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany())
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						IncomingInterchangeProcessor.ExecuteBatch(token);
						IncomingMessageProcessor.ExecuteBatch(token);
					});
				}
			}
		}

		IncomingMessageProcessor IncomingMessageProcessor
		{
			get { return incomingMessageProcessor ?? (incomingMessageProcessor = new IncomingMessageProcessor() { Logger = this.Logger }); }
		}
		IncomingMessageProcessor incomingMessageProcessor;

		IncomingInterchangeProcessor IncomingInterchangeProcessor
		{
			get { return incomingInterchangeProcessor ?? (incomingInterchangeProcessor = new IncomingInterchangeProcessor() { Logger = this.Logger }); }
		}
		IncomingInterchangeProcessor incomingInterchangeProcessor;
	}
}
