using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Hungary;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForHungary.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	[
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.Hungary,
	],
	null)
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForHungary.Code,
	"Hungary E-Invoice Processing Service Task",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForHungary),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Hungary,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.Hungary
{
	public class ElectronicMessagingProcessingServiceTaskForHungary : GlobalElectronicMessagingProcessingServiceTask
	{
		public const string Code = "EHG";

		public override string CountryCode => CountryCodes.Hungary;
	}
}
