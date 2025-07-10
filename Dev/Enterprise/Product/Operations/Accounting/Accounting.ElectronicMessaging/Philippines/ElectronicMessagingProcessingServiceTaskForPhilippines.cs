using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Philippines;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForPhilippines.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.Philippines,
	},
	null)
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForPhilippines.Code,
	"Philippines E-Invoice Processing Service Task",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForPhilippines),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Philippines,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.Philippines
{
	public sealed class ElectronicMessagingProcessingServiceTaskForPhilippines : GlobalElectronicMessagingProcessingServiceTask
	{
		public const string Code = "EPH";

		public override string CountryCode => CountryCodes.Philippines;
	}
}
