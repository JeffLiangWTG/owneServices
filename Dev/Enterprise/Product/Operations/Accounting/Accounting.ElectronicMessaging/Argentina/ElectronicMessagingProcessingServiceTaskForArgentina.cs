using Enterprise.Accounting.ElectronicMessaging.Argentina;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForArgentina.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.Argentina,
	},
	"AR Newly created transactions")
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForArgentina.Code,
	"Argentina E-Invoice Processing Service Task",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForArgentina),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Argentina,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.Argentina
{
	public class ElectronicMessagingProcessingServiceTaskForArgentina : GlobalElectronicMessagingProcessingServiceTask
	{
		public const string Code = "EAR";

		public override string CountryCode => CountryCodes.Argentina;
	}
}
