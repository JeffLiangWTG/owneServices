using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.KoreaSouth;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedService(
	KoreaSouthElectronicMessagingProcessingQueryResultServiceTask.Code,
	"South Korea E-Invoice Query Result Processing Service",
	"ACC",
	typeof(KoreaSouthElectronicMessagingProcessingQueryResultServiceTask),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.KoreaSouth,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "5minutes",
	MaximumPeriod = "1hour",
	DefaultScheduleRunEvery = "1hour",
	ActiveByDefault = true)
]

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	public class KoreaSouthElectronicMessagingProcessingQueryResultServiceTask : KoreaSouthElectronicMessagingProcessingServiceTask
	{
		public const string Code = "EKQ";
		protected override EInvoicingBatchCreatorBase GetEInvoiceBatchCreator(GlbCompany company)
		{
			return new KoreaSouthEInvoicingQueryResultBatchCreator(company);
		}
	}
}
