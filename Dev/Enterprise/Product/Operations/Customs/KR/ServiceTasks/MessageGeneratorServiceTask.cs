using System.Threading;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;
using GlbCompanyWrapper = Enterprise.Customs.KR.Business.GlbCompanyWrapper;

[assembly: HostedService(
	Enterprise.Customs.KR.ServiceTasks.MessageGeneratorServiceTask.Code,
	"KR Customs DLT Message Generator",
	Enterprise.Customs.KR.ServiceTasks.MessageGeneratorServiceTask.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.KR.ServiceTasks.MessageGeneratorServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.KoreaSouth,
	CanRunInAnyBranch = true,
	MinimumPeriod = "2minutes",
	DefaultScheduleRunEvery = "2minutes",
	ActiveByDefault = true
	)]

namespace Enterprise.Customs.KR.ServiceTasks
{
	public class MessageGeneratorServiceTask : CustomsServiceTask
	{
		public const string MessageServiceTaskCategory = "KRC";
		public const string Code = "KRD";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public const int DefaultPeriodInMinutes = 1;

		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var company in GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.KoreaSouth))
			{
				if (GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company).IsValidForMessaging)
				{
					token.ThrowIfCancellationRequested();
					using (DisposableEnvironment.ForCompany(company.GC_Code))
					{
						RunTaskHandleEmailSendFailure(() =>
						{
							new DocumentListMessageGenerator(Logger).GenerateEdiMessage(token, company);
						});
					}
				}
			}
		}
	}
}
