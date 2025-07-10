using Enterprise.Accounting.ElectronicMessaging.Egypt;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForEgypt.Code,
	"Egypt E-Invoice Processing Service Task",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForEgypt),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Egypt,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "5minutes",
	MaximumPeriod = "1hour",
	DefaultScheduleRunEvery = "30minutes",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.Egypt
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("ElectronicMessagingProcessingServiceTaskForEgypt is under implementation.")]
	public class ElectronicMessagingProcessingServiceTaskForEgypt : GlobalElectronicMessagingProcessingServiceTask
	{
		public const string Code = "EEG";

		public override string CountryCode => CountryCodes.Egypt;
	}
}
