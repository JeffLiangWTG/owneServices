using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Jordan;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForJordan.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.Jordan,
	},
	null)
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForJordan.Code,
	"Jordan E-Invoice Processing Service Task",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForJordan),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Jordan,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.Jordan
{
	public class ElectronicMessagingProcessingServiceTaskForJordan : GlobalElectronicMessagingProcessingServiceTask
	{
		public const string Code = "EJO";

		public override string CountryCode => CountryCodes.Jordan;
	}
}
