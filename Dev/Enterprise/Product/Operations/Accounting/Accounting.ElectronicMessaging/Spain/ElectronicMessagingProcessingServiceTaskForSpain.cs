using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Spain;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForSpain.Code,
	"E-Reporting Invoice Processing Service Task For Spain",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForSpain),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Spain,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "5minutes",
	MaximumPeriod = "1hour",
	DefaultScheduleRunEvery = "30minutes",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.Spain
{
	public class ElectronicMessagingProcessingServiceTaskForSpain : GlobalElectronicMessagingProcessingServiceTask
	{
		public const string Code = "EES";

		public override string CountryCode => CountryCodes.Spain;
	}
}
