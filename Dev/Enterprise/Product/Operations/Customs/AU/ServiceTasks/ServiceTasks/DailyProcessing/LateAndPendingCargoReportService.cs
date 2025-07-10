using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.ServiceTasks;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Messaging.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(LateAndPendingCargoReportService.LateAndPendingCargoReportsServiceCode,
	LateAndPendingCargoReportService.LateAndPendingCargoReportsServiceName,
	"AUC",
	typeof(LateAndPendingCargoReportService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Australia,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1Minute",
	DefaultScheduleRunEvery = "1day",
	DefaultScheduleStartAtLocal = "9hours",
	ActiveByDefault = true
	)]

namespace Enterprise.Customs.AU.ServiceTasks
{
	public class LateAndPendingCargoReportService : MultiCompanyCustomsMessagingService
	{
		public const string LateAndPendingCargoReportsServiceCode = "AUL";
		public const string LateAndPendingCargoReportsServiceName = "Late and Pending Cargo Report";

		protected override ZString LegacyBatchProcessorCode
		{
			get { return ZString.Empty; }
		}

		protected override ICustomsServiceTaskProcess GetNewProcess()
		{
			var factory = new BusinessObjectFactory();
			return new CargoReportWorkflow(factory);
		}

		protected override void ProcessOneCompanyCore(CancellationToken token)
		{
			if (CustomsDataRegistry.Instance.GroupToSendLateSeaCargoReportAndWarningsTo.Value != ZGuid.Empty || CustomsDataRegistry.Instance.GroupToSendLateAirCargoReportAndWarningsTo.Value != ZGuid.Empty)
			{
				base.ProcessOneCompanyCore(token);
			}
		}

		protected override string RequiredCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.Australia; }
		}
	}
}
