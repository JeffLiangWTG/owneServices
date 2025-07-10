using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Uruguay;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForUruguay.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.Uruguay,
	},
	"UY Newly created transactions")
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForUruguay.Code,
	"E-Reporting Invoice Processing Service Task For Uruguay",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForUruguay),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Uruguay,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.Uruguay
{
	public class ElectronicMessagingProcessingServiceTaskForUruguay : GlobalElectronicMessagingProcessingServiceTask
	{
		public const string Code = "EUY";

		public override string CountryCode => CountryCodes.Uruguay;
	}
}
