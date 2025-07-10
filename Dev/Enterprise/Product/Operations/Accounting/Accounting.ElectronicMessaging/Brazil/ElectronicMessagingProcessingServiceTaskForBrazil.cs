using Enterprise.Accounting.ElectronicMessaging.Brazil;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForBrazil.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.Brazil,
	},
	null)
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForBrazil.Code,
	"Brazil E-Invoice Processing Service Task",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForBrazil),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Brazil,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.Brazil
{
	public class ElectronicMessagingProcessingServiceTaskForBrazil : GlobalElectronicMessagingProcessingServiceTask
	{
		public const string Code = "EBR";

		public override string CountryCode => CountryCodes.Brazil;
	}
}
