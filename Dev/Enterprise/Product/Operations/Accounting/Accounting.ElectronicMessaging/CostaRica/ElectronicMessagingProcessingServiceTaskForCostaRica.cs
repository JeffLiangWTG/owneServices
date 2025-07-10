using Enterprise.Accounting.ElectronicMessaging.CostaRica;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForCostaRica.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.CostaRica,
	},
	"CR Newly created transactions")
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForCostaRica.Code,
	"E-Reporting Invoice Processing Service Task for Costa Rica",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForCostaRica),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.CostaRica,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.CostaRica
{
	public class ElectronicMessagingProcessingServiceTaskForCostaRica : GlobalElectronicMessagingProcessingServiceTask
	{
		public const string Code = "ECR";

		public override string CountryCode => CountryCodes.CostaRica;
	}
}
