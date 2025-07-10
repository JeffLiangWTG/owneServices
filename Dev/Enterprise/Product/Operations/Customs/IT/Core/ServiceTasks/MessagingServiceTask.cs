using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IT.ServiceTasks;

public abstract class MessagingServiceTask : Customs.ServiceTasks.CustomsServiceTask
{
	[HostedServiceRequirement]
	public static string IsRequired() => Customs.ServiceTasks.CertificateRequirementChecker.ExistsCompanyWithCertificate(CountryCodes.Italy, PasswordTypesList.Codes.ITM, PasswordStatusList.Codes.Valid)
		? string.Empty
		: (NoResString)"There are no valid company certificates configured in Italy.";

	public const string MessageServiceTaskCategory = "ITC";
}
