using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.India;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForIndia.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.India,
	},
	null)
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForIndia.Code,
	"India E-Invoice Processing Service Task",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForIndia),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.India,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.India
{
	#region SuppressResourceStringsCheckRegion

	public class ElectronicMessagingProcessingServiceTaskForIndia : GlobalElectronicMessagingProcessingServiceTask
	{
		public const string Code = "EIN";

		public override string CountryCode => CountryCodes.India;
	}

	#endregion
}
