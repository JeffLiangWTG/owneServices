using System.Threading;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.GB.GVMS.ServiceTasks.GVMSSenderServiceTask.Code
	, Enterprise.Customs.GB.GVMS.ServiceTasks.GVMSSenderServiceTask.FriendlyName
	, Enterprise.Customs.GB.GVMS.ServiceTasks.GVMSSenderServiceTask.MessageServiceTaskCategory
	, typeof(Enterprise.Customs.GB.GVMS.ServiceTasks.GVMSSenderServiceTask)
	, RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom
	, CanRunInAnyBranch = true
	, MinimumPeriod = "300Seconds"
	, DefaultScheduleRunEvery = "15minutes"
)]

[assembly: HostedServiceBusinessObjectBinding(
	Enterprise.Customs.GB.GVMS.ServiceTasks.GVMSSenderServiceTask.Code
	, EDIMessageSchema.Constants.TableName
	, new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.GbCustomsGVMSManifest,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Transmit
	},
	"UK Customs GVMS messages outbound"
)]

namespace Enterprise.Customs.GB.GVMS.ServiceTasks
{
	public class GVMSSenderServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.UnitedKingdom))
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						var log = new LoggingInformation();
						new GVMSOutgoingMessageProcessor(log).ProcessMessage(token);
					});
				}
			}
		}
		
		public const string Code = "GVU";
		public const string FriendlyName = "Goods Vehicle Movement System (GVMS)”, ";
		public const string MessageServiceTaskCategory = "GBC";
	}
}
