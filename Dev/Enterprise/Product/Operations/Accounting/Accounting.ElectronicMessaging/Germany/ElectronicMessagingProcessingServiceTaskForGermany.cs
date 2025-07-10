using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.Germany;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForGermany.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.Germany,
	},
	null)
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForGermany.Code,
	"E-Reporting Invoice Processing Service Task For Germany",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForGermany),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Germany,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.Germany
{
	#region SuppressResourceStringsCheckRegion

	// Murray Note: new countries should inherit from GlobalElectronicMessagingProcessingServiceTask rather than ElectronicMessagingProcessingServiceTask.
	// Please talk to Murray if you think it is a good idea to inherit from ElectronicMessagingProcessingServiceTask for your new country.
	public class ElectronicMessagingProcessingServiceTaskForGermany : ElectronicMessagingProcessingServiceTask
	{
		public const string Code = "EDE";

		public override string CountryCode => CountryCodes.Germany;

		public override ZString MessageName => "Electronic Invoice";

		public override ZString TaskName => "E-Reporting Invoice Processing (Germany)";

		protected override EInvoicingBatchCreatorBase GetEInvoiceBatchCreator(GlbCompany company)
		{
			return new EInvoicingBatchCreatorForGermany(company);
		}

		protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeCreator(GlbCompany company)
		{
			return new EDIInterchangeCreatorForGermany(company);
		}

		protected override BaseEInvoicingDataValidator GetEInvoicingDataValidator(GlbCompany company)
		{
			return new EInvoicingDataValidatorForGermany(company);
		}
	}

	#endregion
}
