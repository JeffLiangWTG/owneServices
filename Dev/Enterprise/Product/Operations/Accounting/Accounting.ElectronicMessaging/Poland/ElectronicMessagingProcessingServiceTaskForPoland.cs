using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Poland;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForPoland.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.Poland,
	},
	null)
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForPoland.Code,
	"Poland E-Invoice Processing Service Task",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForPoland),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Poland,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.Poland
{
	public sealed class ElectronicMessagingProcessingServiceTaskForPoland : GlobalElectronicMessagingProcessingServiceTask
	{
		public const string Code = "EPL";

		public override string CountryCode => CountryCodes.Poland;
	}
}
