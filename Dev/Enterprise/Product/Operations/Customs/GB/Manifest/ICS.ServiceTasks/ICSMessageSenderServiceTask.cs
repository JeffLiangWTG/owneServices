using System.Threading;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.GB.ICS.ServiceTasks.ICSMessageSenderServiceTask.Code
	, Enterprise.Customs.GB.ICS.ServiceTasks.ICSMessageSenderServiceTask.FriendlyName
	, Enterprise.Customs.GB.ICS.ServiceTasks.ICSMessageSenderServiceTask.MessageServiceTaskCategory
	, typeof(Enterprise.Customs.GB.ICS.ServiceTasks.ICSMessageSenderServiceTask)
	, RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom
	, CanRunInAnyBranch = true
	, MinimumPeriod = "300Seconds"
	, DefaultScheduleRunEvery = "15minutes"
)]

[assembly: HostedServiceBusinessObjectBinding(
	Enterprise.Customs.GB.ICS.ServiceTasks.ICSMessageSenderServiceTask.Code
	, EDIMessageSchema.Constants.TableName
	, new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GbMessageICSGreatBritain,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Transmit
	},
	"UK Customs ICS Great Britain messages outbound"
)]

[assembly: HostedServiceBusinessObjectBinding(
	Enterprise.Customs.GB.ICS.ServiceTasks.ICSMessageSenderServiceTask.Code
	, EDIMessageSchema.Constants.TableName
	, new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GbMessageICSNorthernIreland,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Transmit
	},
	"UK Customs ICS Northern Ireland messages outbound"
)]

namespace Enterprise.Customs.GB.ICS.ServiceTasks
{
	public class ICSMessageSenderServiceTask : Customs.ServiceTasks.CustomsServiceTask
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
						new ICSOutgoingMessageProcessor(log).ProcessMessage(token);
					});
				}
			}
		}

		public const string Code = "ICS";
		public const string FriendlyName = "Import Control System (ICS)”, ";
		public const string MessageServiceTaskCategory = "GBC";
	}
}
