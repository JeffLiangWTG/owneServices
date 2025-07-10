using System.Collections.Generic;
using System.Threading;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Registry;
using Enterprise.Customs.IT.ServiceTasks;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

#region Hosted Service Attributes

[assembly:
	HostedService(
		ServiceTaskCodeList.Codes.MessageSender,
		ServiceTaskCodeList.Descriptions.MessageSender,
		MessagingServiceTask.MessageServiceTaskCategory,
		typeof(MessageSenderServiceTask),
		RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Italy,
		CanRunInAnyBranch = true,
		MinimumPeriod = "1minute",
		DefaultScheduleRunEvery = "15minutes",
		ActiveByDefault = true
	)]
[assembly:
	HostedServiceBusinessObjectBinding(
		ServiceTaskCodeList.Codes.MessageSender,
		EDIMessageSchema.Constants.TableName,
		new[] {
			EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
			EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
			EDIMessageSchema.Constants.EM_IsActive + "=Y",
			EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.ITCustoms,
			#pragma warning disable CW1161 // Res.GetString Analyzer
			EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
			#pragma warning restore CW1161 // Res.GetString Analyzer
		},
		MessageSenderServiceTask.MessageSenderForEHubDescription
	)]

[assembly:
	HostedServiceBusinessObjectBinding(
		ServiceTaskCodeList.Codes.MessageSender,
		EDIMessageSchema.Constants.TableName,
		new[] {
			EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
			EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
			EDIMessageSchema.Constants.EM_IsActive + "=Y",
			EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.ITCustomsXTrade,
			#pragma warning disable CW1161 // Res.GetString Analyzer
			EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
			#pragma warning restore CW1161 // Res.GetString Analyzer
		},
		MessageSenderServiceTask.MessageSenderForXTradeDescription
	)]

#endregion

namespace Enterprise.Customs.IT.ServiceTasks;

public class MessageSenderServiceTask : MessagingServiceTask
{
	protected override void RunTaskCore(CancellationToken token)
	{
		if (!ITCustomsDataRegistry.Instance.UseUCMPForCategoryITC.Value)
		{
			foreach (var companyCode in DisposableEnvironment.GetActiveCompanies(Core.Constants.CountryCodes.Italy))
			{
				token.ThrowIfCancellationRequested();

				ProcessMessagesForCompany(companyCode, token);
			}
		}
	}

	void ProcessMessagesForCompany(string companyCode, CancellationToken token)
	{
		using (DisposableEnvironment.ForCompany(companyCode))
		{
			foreach (var outgoingMessageProcessor in AllOutgoingMessageProcessors)
			{
				outgoingMessageProcessor.ProcessMessage(token);
			}
		}
	}

	#region Implementation

	IEnumerable<OutgoingMessageProcessor> AllOutgoingMessageProcessors => allOutgoingMessageProcessors ?? (allOutgoingMessageProcessors = AllOutgoingMessageProcessorsLoader.GetAll(Logger));
	IEnumerable<OutgoingMessageProcessor> allOutgoingMessageProcessors;

	#endregion

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message String")]
	public const string MessageSenderForEHubDescription = "IT Customs Message Sender for eHub";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message String")]
	public const string MessageSenderForXTradeDescription = "IT Customs Message Sender for XTrade";
}
