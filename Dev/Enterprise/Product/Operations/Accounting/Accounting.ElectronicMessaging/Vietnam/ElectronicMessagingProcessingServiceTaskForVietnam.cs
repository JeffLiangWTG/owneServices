using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.Vietnam;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForVietnam.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.VietNam,
	},
	null)
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForVietnam.Code,
	"Vietnam E-Invoice Processing Service Task",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForVietnam),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.VietNam,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.Vietnam
{
	#region SuppressResourceStringsCheckRegion

	// Murray Note: new countries should inherit from GlobalElectronicMessagingProcessingServiceTask rather than ElectronicMessagingProcessingServiceTask.
	// Please talk to Murray if you think it is a good idea to inherit from ElectronicMessagingProcessingServiceTask for your new country.
	public class ElectronicMessagingProcessingServiceTaskForVietnam : ElectronicMessagingProcessingServiceTask
	{
		public const string Code = "EVN";

		public override string CountryCode => CountryCodes.VietNam;

		public override ZString MessageName => "Electronic Invoice";

		public override ZString TaskName => "Vietnam E-Reporting Invoice Processing";

		protected override EInvoicingBatchCreatorBase GetEInvoiceBatchCreator(GlbCompany company)
		{
			return new EInvoicingBatchCreatorForVietnam(company);
		}

		protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeCreator(GlbCompany company)
		{
			return new EDIInterchangeCreatorForVietnam(company);
		}

		protected override BaseEInvoicingDataValidator GetEInvoicingDataValidator(GlbCompany company)
		{
			return new EInvoicingDataValidatorForVietnam(company);
		}
	}

	#endregion
}
