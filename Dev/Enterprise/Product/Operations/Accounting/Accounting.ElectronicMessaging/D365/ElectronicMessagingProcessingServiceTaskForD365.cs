using Enterprise.Accounting.ElectronicMessaging.D365;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForD365.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.UnitedKingdom,
	},
	null)
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForD365.Code,
	"D365 E-Accounting Processing Service Task",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForD365),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.UnitedKingdom,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.D365
{
	public class ElectronicMessagingProcessingServiceTaskForD365 : GlobalElectronicMessagingProcessingServiceTask
	{
		public const string Code = "DYN";

		public override string CountryCode => CountryCodes.UnitedKingdom;
	}
}
