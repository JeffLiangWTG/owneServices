using Enterprise.Accounting.ElectronicMessaging.Fiji;
using Enterprise.Accounting.ElectronicMessaging.TaxCore;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForFiji.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.Fiji,
	},
	null)
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForFiji.Code,
	"E-Reporting Invoice Processing Service Task For Fiji",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForFiji),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Fiji,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.Fiji
{
	#region SuppressResourceStringsCheckRegion

	public class ElectronicMessagingProcessingServiceTaskForFiji : TaxCoreElectronicMessagingProcessingServiceTask
	{
		public const string Code = "EFJ";

		public override string CountryCode => CountryCodes.Fiji;
	}

	#endregion
}
