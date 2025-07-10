using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Serbia;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ServiceTaskCode = ElectronicMessagingProcessingServiceTaskForSerbia.Code,
	Table = AccEInvoicingTransactionPivotSchema.Constants.TableName,
	Predicates = new[]
	{
		$"{AccEInvoicingTransactionPivotSchema.Constants.AIP_Status} = {EInvoicingPivotState.Queued}",
		$"{AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode} = {CountryCodes.Serbia}",
	})
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForSerbia.Code,
	"Serbia E-Invoice Processing Service Task",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForSerbia),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Serbia,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "30minutes",
	ActiveByDefault = true)
]

namespace Enterprise.Accounting.ElectronicMessaging.Serbia
{
	public class ElectronicMessagingProcessingServiceTaskForSerbia : GlobalElectronicMessagingProcessingServiceTask
	{
		public const string Code = "SRB";

		public override string CountryCode => CountryCodes.Serbia;
	}
}
