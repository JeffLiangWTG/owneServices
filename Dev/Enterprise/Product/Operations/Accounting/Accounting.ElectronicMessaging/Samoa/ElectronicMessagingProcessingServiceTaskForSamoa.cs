using Enterprise.Accounting.ElectronicMessaging.Samoa;
using Enterprise.Accounting.ElectronicMessaging.TaxCore;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForSamoa.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.WesternSamoa,
	},
	null)
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForSamoa.Code,
	"E-Reporting Invoice Processing Service Task For Samoa",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForSamoa),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.WesternSamoa,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.Samoa
{
	#region SuppressResourceStringsCheckRegion

	public class ElectronicMessagingProcessingServiceTaskForSamoa : TaxCoreElectronicMessagingProcessingServiceTask
	{
		public const string Code = "EWS";

		public override string CountryCode => CountryCodes.WesternSamoa;
	}

	#endregion
}
