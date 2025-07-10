using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.Taiwan;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForTaiwan.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.Taiwan,
	},
	null)
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForTaiwan.Code,
	"E-Reporting Invoice Processing Service Task For Taiwan",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForTaiwan),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Taiwan,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.Taiwan
{
	#region SuppressResourceStringsCheckRegion

	// Murray Note: new countries should inherit from GlobalElectronicMessagingProcessingServiceTask rather than ElectronicMessagingProcessingServiceTask.
	// Please talk to Murray if you think it is a good idea to inherit from ElectronicMessagingProcessingServiceTask for your new country.
	public class ElectronicMessagingProcessingServiceTaskForTaiwan : ElectronicMessagingProcessingServiceTask
	{
		public const string Code = "ETW";

		public override string CountryCode => CountryCodes.Taiwan;

		public override ZString MessageName => "Electronic Invoice";

		public override ZString TaskName => "E-Reporting Invoice Processing";

		protected override EInvoicingBatchCreatorBase GetEInvoiceBatchCreator(GlbCompany company)
		{
			return new EInvoicingBatchCreatorForTaiwan(company);
		}

		protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeCreator(GlbCompany company)
		{
			return new EDIInterchangeCreatorForTaiwan(company);
		}

		protected override BaseEInvoicingDataValidator GetEInvoicingDataValidator(GlbCompany company)
		{
			return new EInvoicingDataValidatorForTaiwan(company);
		}
	}

	#endregion
}
