using System.Threading;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.ServiceTasks;
using Enterprise.Customs.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedService(
	ServiceTaskApplicationCodeList.Codes.PassarMessagePoll,
	ServiceTaskApplicationCodeList.Descriptions.PassarMessagePoll,
	BranchMessagingService.MessageServiceTaskCategory,
	typeof(PassarMessagePollingService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Switzerland,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "30seconds",
	ActiveByDefault = true
	)]

namespace Enterprise.Customs.CH.ServiceTasks;

public class PassarMessagePollingService : BranchMessagingService
{
	protected override void RunTaskForEachBranch(CancellationToken token)
	{
		CompanyMessageSender.SendRequests(Logger, token);
	}

	[HostedServiceRequirement]
	public static string IsRequired() => CertificateRequirementChecker.ExistsCompanyWithCertificate(CountryCodes.Switzerland, PasswordTypesList.Codes.CHT) ? string.Empty : (NoResString)"There is no Token Credential configured in Switzerland.";
}
