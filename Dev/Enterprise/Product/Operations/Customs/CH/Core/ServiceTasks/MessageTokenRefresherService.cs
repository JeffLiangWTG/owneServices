using System.Threading;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.ServiceTasks;
using Enterprise.Customs.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedService(
	ServiceTaskApplicationCodeList.Codes.TokenRefresher,
	ServiceTaskApplicationCodeList.Descriptions.TokenRefresher,
	BranchMessagingService.MessageServiceTaskCategory,
	typeof(MessageTokenRefresherService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Switzerland,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "30seconds",
	ActiveByDefault = true
	)]

namespace Enterprise.Customs.CH.ServiceTasks;

public class MessageTokenRefresherService : BranchMessagingService
{
	protected override void RunTaskForEachBranch(CancellationToken token)
	{
		CompanyMessageSender.SendTokensRefresh(Logger, token);
	}

	[HostedServiceRequirement]
	public static string IsRequired() => CertificateRequirementChecker.ExistsCompanyWithCertificate(CountryCodes.Switzerland, PasswordTypesList.Codes.CHT) ? string.Empty : (NoResString)"There is no Token Credential configured in Switzerland.";
}
