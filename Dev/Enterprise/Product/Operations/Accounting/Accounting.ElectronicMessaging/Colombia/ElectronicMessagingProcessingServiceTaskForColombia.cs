using Enterprise.Accounting.ElectronicMessaging.Colombia;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForColombia.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.Colombia,
	},
	null)
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForColombia.Code,
	"E-Reporting Invoice Processing Service Task For Colombia",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForColombia),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Colombia,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.Colombia
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("ElectronicMessagingProcessingServiceTaskForColombia is under implementation.")]
	public class ElectronicMessagingProcessingServiceTaskForColombia : GlobalElectronicMessagingProcessingServiceTask
	{
		public const string Code = "ECO";

		public override string CountryCode => CountryCodes.Colombia;
	}
}
