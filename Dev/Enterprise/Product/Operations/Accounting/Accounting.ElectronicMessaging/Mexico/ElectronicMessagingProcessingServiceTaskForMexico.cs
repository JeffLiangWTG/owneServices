using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Mexico;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForMexico.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.Mexico,
	},
	"MX Newly created transactions")
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForMexico.Code,
	"Mexico E-Invoice Processing Service Task",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForMexico),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Mexico,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]

namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	public class ElectronicMessagingProcessingServiceTaskForMexico : GlobalElectronicMessagingProcessingServiceTask
	{
		public const string Code = "EMX";

		public override string CountryCode => CountryCodes.Mexico;
	}
}
