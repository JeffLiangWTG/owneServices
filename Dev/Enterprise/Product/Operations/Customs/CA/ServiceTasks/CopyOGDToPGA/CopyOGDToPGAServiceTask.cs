using System.Threading;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.BatchProcessor;
using Enterprise.Customs.CA.ServiceTasks;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	CopyOGDToPGAServiceTask.Code,
	CopyOGDToPGAServiceTask.FriendlyName,
	MessageProcessorServiceTask.MessageServiceTaskCategory,
	typeof(CopyOGDToPGAServiceTask),
	MinimumPeriod = "1Hours",
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Canada,
	DefaultScheduleRunEvery = "1hour",
	ActiveByDefault = true
)]

namespace Enterprise.Customs.CA.ServiceTasks
{
	public class CopyOGDToPGAServiceTask : ServiceProviderImplUnderDefaultBranch
	{
		public const string Code = "CCP";
		public const string FriendlyName = "Canadian Copy OGD To PGA Processing";

		[HostedServiceRequirement]
		public static string IsRequired() => BatchProcessorUtilities.CompanyInCanada ? string.Empty : "There is no company configured in Canada.";

		protected override void RunTaskMain(CancellationToken token)
		{
			var copProcessor = new CopyOGDToPGADataForProductsProcessor(ServiceLogger);
			copProcessor.Process(token);
		}
	}
}
