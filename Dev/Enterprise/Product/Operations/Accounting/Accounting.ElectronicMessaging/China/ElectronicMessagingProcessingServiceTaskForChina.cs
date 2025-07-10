using Enterprise.Accounting.ElectronicMessaging.China;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForChina.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.China,
	},
	null)
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForChina.Code,
	"China E-Invoice Processing Service Task",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForChina),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.China,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.China
{
	public class ElectronicMessagingProcessingServiceTaskForChina : GlobalElectronicMessagingProcessingServiceTask
	{
		public const string Code = "ECN";

		public override string CountryCode => CountryCodes.China;
	}
}
