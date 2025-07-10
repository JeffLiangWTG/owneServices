using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Latvia;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForLatvia.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.Latvia,
	},
	null)
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForLatvia.Code,
	"Latvia E-Invoice Processing Service Task",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForLatvia),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Latvia,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.Latvia
{
	public class ElectronicMessagingProcessingServiceTaskForLatvia : GlobalElectronicMessagingProcessingServiceTask
	{
		public const string Code = "ELV";

		public override string CountryCode => CountryCodes.Latvia;
	}
}
