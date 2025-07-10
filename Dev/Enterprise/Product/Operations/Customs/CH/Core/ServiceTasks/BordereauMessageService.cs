using System.Threading;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.ServiceTasks;
using Enterprise.Customs.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedService(
	ServiceTaskApplicationCodeList.Codes.BordereauMessageSender,
	ServiceTaskApplicationCodeList.Descriptions.BordereauMessageSender,
	BranchMessagingService.MessageServiceTaskCategory,
	typeof(BordereauMessageService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Switzerland,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1day",
	DefaultScheduleStartAtUtc = "3hours",
	MinimumPeriod = "15min",
	ActiveByDefault = true
	)]

namespace Enterprise.Customs.CH.ServiceTasks;

public class BordereauMessageService : BranchMessagingService
{
	protected override void RunTaskForEachBranch(CancellationToken token)
	{
		CompanyMessageSender.SendBordereauRequest(Logger, token);
	}

	[HostedServiceRequirement]
	public static string IsRequired() => CertificateRequirementChecker.ExistsCompanyWithCertificate(CountryCodes.Switzerland, PasswordTypesList.Codes.CHC) ? string.Empty : (NoResString)"There is no Certificate Credential configured in Switzerland.";
}
