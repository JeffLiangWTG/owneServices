using Enterprise.Accounting.ElectronicMessaging.Chile;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForChile.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.Chile,
	},
	null)
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForChile.Code,
	"E-Reporting Invoice Processing Service Task For Chile",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForChile),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Chile,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.Chile
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("ElectronicMessagingProcessingServiceTaskForChile is under implementation.")]
	public class ElectronicMessagingProcessingServiceTaskForChile : GlobalElectronicMessagingProcessingServiceTask
	{
		public const string Code = "ECL";

		public override string CountryCode => CountryCodes.Chile;
	}
}
