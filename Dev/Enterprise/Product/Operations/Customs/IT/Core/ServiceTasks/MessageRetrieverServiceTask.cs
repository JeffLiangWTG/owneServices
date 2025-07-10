using System.Threading;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly:
	HostedService(
		Enterprise.Customs.IT.ServiceTasks.ServiceTaskCodeList.Codes.MessageRetriever,
		Enterprise.Customs.IT.ServiceTasks.ServiceTaskCodeList.Descriptions.MessageRetriever,
		Enterprise.Customs.IT.ServiceTasks.MessagingServiceTask.MessageServiceTaskCategory,
		typeof(Enterprise.Customs.IT.ServiceTasks.MessageRetrieverServiceTask),
		RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Italy,
		CanRunInAnyBranch = true,
		MinimumPeriod = "30seconds",
		DefaultScheduleRunEvery = "15minutes",
		ActiveByDefault = true
	)]

[assembly:
	HostedServiceBusinessObjectBinding(
		Enterprise.Customs.IT.ServiceTasks.ServiceTaskCodeList.Codes.MessageRetriever,
		EDIInterchangeSchema.Constants.TableName,
		new[] {
			EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
			EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
			EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
			EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.ITCustoms
		},
		Enterprise.Customs.IT.ServiceTasks.MessageRetrieverServiceTask.MessageRetrieverForEHubDescription
	)]

[assembly:
	HostedServiceBusinessObjectBinding(
		Enterprise.Customs.IT.ServiceTasks.ServiceTaskCodeList.Codes.MessageRetriever,
		EDIInterchangeSchema.Constants.TableName,
		new[] {
			EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
			EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
			EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
			EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ITCustomsXTrade,
		},
		Enterprise.Customs.IT.ServiceTasks.MessageRetrieverServiceTask.MessageRetrieverForxTDescription
	)]

namespace Enterprise.Customs.IT.ServiceTasks;

public class MessageRetrieverServiceTask : MessagingServiceTask
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
					RunTaskHandleEmailSendFailure(() => ITCInboundInterchangeProcessor.UnpackInterchanges(Logger, token));
				}
			}
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message String")]
	public const string MessageRetrieverForEHubDescription = "IT Customs Message Retriever for eHub";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message String")]
	public const string MessageRetrieverForxTDescription = "IT Customs Message Retriever for xT";
}
