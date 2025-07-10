using System.Threading;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Registry;
using Enterprise.Customs.IT.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly:
	HostedService(
		Enterprise.Customs.IT.ServiceTasks.ServiceTaskCodeList.Codes.MessageProcessor,
		Enterprise.Customs.IT.ServiceTasks.ServiceTaskCodeList.Descriptions.MessageProcessor,
		Enterprise.Customs.IT.ServiceTasks.MessagingServiceTask.MessageServiceTaskCategory,
		typeof(MessageProcessorServiceTask),
		RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Italy,
		CanRunInAnyBranch = true,
		MinimumPeriod = "30seconds",
		DefaultScheduleRunEvery = "15minutes",
		ActiveByDefault = true
	)]

[assembly:
	HostedServiceBusinessObjectBinding(
		Enterprise.Customs.IT.ServiceTasks.ServiceTaskCodeList.Codes.MessageProcessor,
		EDIMessageSchema.Constants.TableName,
		new[] {
			EDIMessageSchema.Constants.EM_Status + "=" + Enterprise.Messaging.Business.EDIMessage.Status.Queued,
			EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
			EDIMessageSchema.Constants.EM_IsActive + "=Y",
			EDIMessageSchema.Constants.EM_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.ITCustoms,
			#pragma warning disable CW1161 // Res.GetString Analyzer
			EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
			#pragma warning restore CW1161 // Res.GetString Analyzer
		},
		MessageProcessorServiceTask.MessageProcessorForEHubDescription
	)]

[assembly:
	HostedServiceBusinessObjectBinding(
		Enterprise.Customs.IT.ServiceTasks.ServiceTaskCodeList.Codes.MessageProcessor,
		EDIMessageSchema.Constants.TableName,
		new[] {
			EDIMessageSchema.Constants.EM_Status + "=" + Enterprise.Messaging.Business.EDIMessage.Status.Queued,
			EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
			EDIMessageSchema.Constants.EM_IsActive + "=Y",
			EDIMessageSchema.Constants.EM_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.ITCustomsXTrade,
			#pragma warning disable CW1161 // Res.GetString Analyzer
			EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
			#pragma warning restore CW1161 // Res.GetString Analyzer
		},
		MessageProcessorServiceTask.MessageProcessorForxTDescription
	)]

namespace Enterprise.Customs.IT.ServiceTasks;

public class MessageProcessorServiceTask : MessagingServiceTask
{
	protected override void RunTaskCore(CancellationToken token)
	{
		if (!ITCustomsDataRegistry.Instance.UseUCMPForCategoryITC.Value)
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.Italy))
			{
				token.ThrowIfCancellationRequested();

				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() => ITCIncomingMessageProcessingHub.ProcessMessages(Logger, token));
				}
			}
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message String")]
	public const string MessageProcessorForEHubDescription = "IT Customs Message Processor for eHub";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message String")]
	public const string MessageProcessorForxTDescription = "IT Customs Message Processor for xT";
}
