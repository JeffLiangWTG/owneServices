using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.KoreaSouth;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	KoreaSouthElectronicMessagingProcessingSubmitRequestServiceTask.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.KoreaSouth,
	},
	null)
]

[assembly: HostedService(
	KoreaSouthElectronicMessagingProcessingSubmitRequestServiceTask.Code,
	"South Korea E-Invoice Processing Service Task",
	"ACC",
	typeof(KoreaSouthElectronicMessagingProcessingSubmitRequestServiceTask),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.KoreaSouth,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "1hour",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	public class KoreaSouthElectronicMessagingProcessingSubmitRequestServiceTask : KoreaSouthElectronicMessagingProcessingServiceTask
	{
		public const string Code = "EKR";

		protected override EInvoicingBatchCreatorBase GetEInvoiceBatchCreator(GlbCompany company)
		{
			return new KoreaSouthEInvoicingSubmitRequestBatchCreator(company);
		}
	}
}
