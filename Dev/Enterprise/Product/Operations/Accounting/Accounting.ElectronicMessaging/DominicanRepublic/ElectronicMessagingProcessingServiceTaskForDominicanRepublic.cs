using Enterprise.Accounting.ElectronicMessaging.DominicanRepublic;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForDominicanRepublic.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.DominicanRepublic,
	},
	"DO Newly created transactions")
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForDominicanRepublic.Code,
	"E-Reporting Invoice Processing Service Task For Dominican Republic",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForDominicanRepublic),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.DominicanRepublic,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.DominicanRepublic
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("ElectronicMessagingProcessingServiceTaskForDominicanRepublic is under implementation.")]
	public class ElectronicMessagingProcessingServiceTaskForDominicanRepublic : GlobalElectronicMessagingProcessingServiceTask
	{
		public const string Code = "EDO";
		public override string CountryCode => CountryCodes.DominicanRepublic;
	}
}
