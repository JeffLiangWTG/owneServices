using System.Threading;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.GB.EMCS.ServiceTasks.EMCSSenderServiceTask.Code
	, Enterprise.Customs.GB.EMCS.ServiceTasks.EMCSSenderServiceTask.FriendlyName
	, Enterprise.Customs.GB.EMCS.ServiceTasks.EMCSSenderServiceTask.MessageServiceTaskCategory
	, typeof(Enterprise.Customs.GB.EMCS.ServiceTasks.EMCSSenderServiceTask)
	, RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom
	, CanRunInAnyBranch = true
	, MinimumPeriod = "300Seconds"
	, DefaultScheduleRunEvery = "15minutes"
)]

[assembly: HostedServiceBusinessObjectBinding(
	Enterprise.Customs.GB.EMCS.ServiceTasks.EMCSSenderServiceTask.Code
	, EDIMessageSchema.Constants.TableName
	, new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.GbCustomsEMCS,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Transmit
	},
	"UK Customs EMCS messages outbound"
)]

namespace Enterprise.Customs.GB.EMCS.ServiceTasks
{
	public class EMCSSenderServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Enterprise.Core.Constants.CountryCodes.UnitedKingdom))
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						var log = new LoggingInformation();
						new EMCSOutgoingMessageProcessor(log).ProcessMessage(token);
					});
				}
			}
		}
		
		public const string Code = "EMO";
		public const string FriendlyName = "Excise Movement and Control System (EMCS) outbound messages";
		public const string MessageServiceTaskCategory = "GBC";
	}
}
