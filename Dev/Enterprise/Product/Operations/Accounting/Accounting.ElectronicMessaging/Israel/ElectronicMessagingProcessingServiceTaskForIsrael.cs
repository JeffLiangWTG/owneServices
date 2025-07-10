using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Israel;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForIsrael.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.Israel,
	},
	null)
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForIsrael.Code,
	"E-Reporting Invoice Processing Service Task For Israel",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForIsrael),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Israel,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.Israel
{
	// Murray Note: new countries should inherit from GlobalElectronicMessagingProcessingServiceTask rather than ElectronicMessagingProcessingServiceTask.
	// Please talk to Murray if you think it is a good idea to inherit from ElectronicMessagingProcessingServiceTask for your new country.
	[WTG.StaticAnalysis.Annotation.CodeAlive("ElectronicMessagingProcessingServiceTaskForIsrael is under implementation.")]
	public class ElectronicMessagingProcessingServiceTaskForIsrael : GlobalElectronicMessagingProcessingServiceTask
	{
		public ElectronicMessagingProcessingServiceTaskForIsrael()
			: base(dataProvider: new ElectronicMessagingProcessingServiceTaskDataProvider())
		{
		}

		public const string Code = "EIL";
		public override string CountryCode => CountryCodes.Israel;

		protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeCreator(GlbCompany company)
		{
			var countryFactory = GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(CountryCode);
			return new EDIInterchangeCreatorForIsrael(company, countryFactory);
		}
	}
}
