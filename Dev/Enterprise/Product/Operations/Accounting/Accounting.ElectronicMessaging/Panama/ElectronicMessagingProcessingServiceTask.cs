using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Panama;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	 ElectronicMessagingProcessingServiceTask.Code,
	 AccEInvoicingTransactionPivotSchema.Constants.TableName,
	 new[]
	 {
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Succeed,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.Panama,
	 },
	 "PA Transactions that depend on their original transactions")
]

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTask.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.Panama,
	},
	"PA Newly created transactions")
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTask.Code,
	"E-Reporting Invoice Processing Service Task For Panama",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTask),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Panama,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.Panama
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("ElectronicMessagingProcessingServiceTaskForPanama is under implementation.")]
	public class ElectronicMessagingProcessingServiceTask : GlobalElectronicMessagingProcessingServiceTask
	{
		public const string Code = "EPA";
		public override string CountryCode => CountryCodes.Panama;
	}
}
