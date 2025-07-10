using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Mauritius;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForMauritius.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.Mauritius,
	},
	null)
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForMauritius.Code,
	"Mauritius E-Invoice Processing Service Task",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForMauritius),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Mauritius,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.Mauritius
{
	public class ElectronicMessagingProcessingServiceTaskForMauritius : GlobalElectronicMessagingProcessingServiceTask
	{
		public const string Code = "EMU";

		public override string CountryCode => CountryCodes.Mauritius;
	}
}
