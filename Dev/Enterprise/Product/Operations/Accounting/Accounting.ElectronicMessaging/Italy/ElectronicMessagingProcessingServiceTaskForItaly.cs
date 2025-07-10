using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.Italy;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForItaly.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.Italy,
	},
	null)
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForItaly.Code,
	"E-Reporting Invoice Processing Service Task For Italy",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForItaly),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Italy,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.Italy
{
	#region SuppressResourceStringsCheckRegion

	// Murray Note: new countries should inherit from GlobalElectronicMessagingProcessingServiceTask rather than ElectronicMessagingProcessingServiceTask.
	// Please talk to Murray if you think it is a good idea to inherit from ElectronicMessagingProcessingServiceTask for your new country.
	public class ElectronicMessagingProcessingServiceTaskForItaly : ElectronicMessagingProcessingServiceTask
	{
		public const string Code = "EIT";

		public override string CountryCode => CountryCodes.Italy;

		public override ZString MessageName => "Electronic Invoice";

		public override ZString TaskName => "E-Reporting Invoice Processing";

		protected override EInvoicingBatchCreatorBase GetEInvoiceBatchCreator(GlbCompany company)
		{
			return new EInvoicingBatchCreatorForItaly(company);
		}

		protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeCreator(GlbCompany company)
		{
			return new EDIInterchangeCreatorForItaly(company);
		}

		protected override BaseEInvoicingDataValidator GetEInvoicingDataValidator(GlbCompany company)
		{
			return new EInvoicingDataValidatorForItaly(company);
		}
	}

	#endregion
}
