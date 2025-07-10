using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.SaudiArabia;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForSaudiArabia.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + " IN ('" + EInvoicingPivotState.Queued + "', '" + EInvoicingPivotState.Succeed + "', '" +  EInvoicingPivotState.Failed + "')",
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.SaudiArabia,
	},
	null)
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForSaudiArabia.Code,
	"E-Reporting Invoice Processing Service Task For Saudi Arabia",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForSaudiArabia),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.SaudiArabia,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	AllowsMultipleInstances = false, //Never allow running multiple instances of this service task, as Invoices are processed one by one in SaudiArabia
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.SaudiArabia
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("ElectronicMessagingProcessingServiceTaskForSaudiArabia is under implementation.")]
	public class ElectronicMessagingProcessingServiceTaskForSaudiArabia : GlobalElectronicMessagingProcessingServiceTask
	{
		public ElectronicMessagingProcessingServiceTaskForSaudiArabia() : base(new ElectronicMessagingProcessingServiceTaskDataProviderForSaudiArabia())
		{ }

		public const string Code = "ESA";
		public override string CountryCode => CountryCodes.SaudiArabia;

		protected override BaseEInvoicingDataValidator GetEInvoicingDataValidator(GlbCompany company)
		{
			return new EInvoicingDataValidatorForSaudiArabia(company);
		}
	}
}
