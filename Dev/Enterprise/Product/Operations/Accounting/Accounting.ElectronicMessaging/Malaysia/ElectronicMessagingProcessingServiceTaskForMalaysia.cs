using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Malaysia;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForMalaysia.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.Malaysia,
	},
	null)
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForMalaysia.Code,
	"Malaysia E-Invoice Processing Service Task",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForMalaysia),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Malaysia,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.Malaysia
{
	public class ElectronicMessagingProcessingServiceTaskForMalaysia : GlobalElectronicMessagingProcessingServiceTask
	{
		public const string Code = "EMY";

		public override string CountryCode => CountryCodes.Malaysia;
	}
}
