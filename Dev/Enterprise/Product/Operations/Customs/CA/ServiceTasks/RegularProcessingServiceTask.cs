using System;
using System.Globalization;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.CA.ServiceTasks.RegularProcessingServiceTask.Code,
	Enterprise.Customs.CA.ServiceTasks.RegularProcessingServiceTask.FriendlyName,
	"CAC",
	typeof(Enterprise.Customs.CA.ServiceTasks.RegularProcessingServiceTask),
	MinimumPeriod = "1Hours",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Canada,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1hour",
	ActiveByDefault = true
)]

namespace Enterprise.Customs.CA.ServiceTasks
{
	public class RegularProcessingServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		public const string Code = "CRP";
		public const string FriendlyName = "Canadian Regular Processing";

		protected override void RunTaskCore(CancellationToken token)
		{
			var rtsProcessor = new RefTariffSynchronizeProcessor(ServiceLogger);
			rtsProcessor.Process();

			var caaProcessor = new CalculateAccountingAgeProcessor(ServiceLogger);
			var rtuProcessor = new RequeueTariffUpdateMessageProcessor(ServiceLogger);
			var expProcessor = new SetExceptionCodeProcessor(ServiceLogger);

			foreach (var company in new GlbCompany.Loader(new BusinessObjectFactory()).LoadCompanies(Core.Constants.CountryCodes.Canada))
			{
				token.ThrowIfCancellationRequested();
				if (ValidateEnvironment(company))
				{
					var branch = company.FirstActiveBranch;
					if (branch != null)
					{
						using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
						{
							ServiceLogger.Information(string.Format(CultureInfo.CurrentCulture, "Regular Processing for Company {0}.", company.GC_Code));
							caaProcessor.Process(company.PK);
							rtuProcessor.Process();
							expProcessor.Process(company);
						}
					}
				}
			}
		}

		bool ValidateEnvironment(GlbCompany company)
		{
			var result = true;
			if (string.IsNullOrEmpty(CACustomsDataRegistry.Instance.AccountSecurityNo.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty)))
			{
				ServiceLogger.Warning(string.Format(CultureInfo.CurrentCulture, "Account Security No hasn't been setup for Company {0}. Please set it up in Registry -> {1}",
						company.GC_Code, ((IRegistryItemInternals)CACustomsDataRegistry.Instance.AccountSecurityNo).Location));
				result = false;
			}

			return result;
		}
	}
}
