using System.Threading;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.ES.ServiceTasks.MessageSendingService.Code,
	Enterprise.Customs.ES.ServiceTasks.MessageSendingService.FriendlyName,
	Enterprise.Customs.ES.ServiceTasks.MessagingService.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.ES.ServiceTasks.MessageSendingService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Spain,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.ES.ServiceTasks.MessageSendingService.Code,
	EDIMessageSchema.Constants.TableName,
	new[] {
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.ESCustomsMessage,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
	},
	Enterprise.Customs.ES.ServiceTasks.MessageSendingService.FriendlyName)]

namespace Enterprise.Customs.ES.ServiceTasks
{
	public class MessageSendingService : Customs.ServiceTasks.CustomsServiceTask
	{
		public const string Code = "ESS";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string FriendlyName = "ES Customs Message Sending";

		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.Spain))
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					new ESCOutboundMessageProcessor(Logger).ProcessMessage(token);
				}
			}
		}
	}
}
