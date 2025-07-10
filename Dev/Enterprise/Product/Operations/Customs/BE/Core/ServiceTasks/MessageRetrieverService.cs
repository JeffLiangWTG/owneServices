using System.Threading;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedService(
	Enterprise.Customs.BE.ServiceTasks.MessageRetrieverService.Code,
	Enterprise.Customs.BE.ServiceTasks.MessageRetrieverService.FriendlyName,
	Enterprise.Customs.BE.ServiceTasks.MessageRetrieverService.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.BE.ServiceTasks.MessageRetrieverService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Belgium,
	MinimumPeriod = "60Seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.BE.ServiceTasks.MessageRetrieverService.Code,
	EDIInterchangeSchema.Constants.TableName,
	new[] { EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
				 EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
				 EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
				 EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + ApplicationCodeList.Codes.BECustoms },
	Enterprise.Customs.BE.ServiceTasks.MessageRetrieverService.FriendlyName
	)]

namespace Enterprise.Customs.BE.ServiceTasks;

public class MessageRetrieverService : MessagingServiceTask
{
	public const string Code = BEMessageServiceTypeList.Codes.BeCustomsInterchangesInbound;
	public const string FriendlyName = BEMessageServiceTypeList.Descriptions.BeCustomsInterchangesInbound;

	protected override void RunTaskCore(CancellationToken token)
	{
		using (DisposableEnvironment.ForBranch(GlbBranch.GetFirstActiveBranch(Core.Constants.CountryCodes.Belgium).PK.ToGuid()))
		{
			token.ThrowIfCancellationRequested();
			RunTaskHandleEmailSendFailure(() =>
			{
				using (var processor = new BECInboundInterchangeProcessor())
				{
					processor.ExecuteBatch(token);
				}
			});
		}
	}

	[HostedServiceRequirement]
	public static string IsRequired() =>
		CertificateRequirementChecker.ExistsCompanyWithCertificate(CountryCodes.Belgium, PasswordTypesList.Codes.BEC, PasswordStatusList.Codes.PasswordOK)
			? string.Empty : (NoResString)"There is no Credential configured in Belgium."; // information for logging only.
}
