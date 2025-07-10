using System;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.CA.ServiceTasks.B3AutoSendingServiceTask.Code,
	Enterprise.Customs.CA.ServiceTasks.B3AutoSendingServiceTask.FriendlyName,
	"CAC",
	typeof(Enterprise.Customs.CA.ServiceTasks.B3AutoSendingServiceTask),
	MinimumPeriod = "6Hours",
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Canada,
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true
)]

namespace Enterprise.Customs.CA.ServiceTasks
{
	public class B3AutoSendingServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		public const string Code = "CAB";
		public const string FriendlyName = "Canadian Customs CAD Auto-Sending";

		bool shouldContinueProcessing = true;

		[HostedServiceRequirement]
		public static string CheckAutoB3SendingIsEnabled() => HostedServiceRequirementAttribute.CheckValueIsNotEqualTo(CACustomsDataRegistry.Instance.ActivateAutoB3Sending, false);

		protected override void RunTaskCore(CancellationToken token)
		{
			ValidateEnvironment();
			if (shouldContinueProcessing)
			{
				foreach (var branch in new GlbBranch.Loader(new BusinessObjectFactory()).LoadAllBranchesInThisCountryActiveOnly(Core.Constants.CountryCodes.Canada))
				{
					token.ThrowIfCancellationRequested();
					using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
					{
						RunTaskHandleEmailSendFailure(GetB3AutoSender().Process);
					}
				}
			}
			else
			{
				ServiceLogger.Log(LogType.Error, "Processing suspended due to previous error. Exception will be thrown to terminate service task.");
				throw new ArgumentException(FriendlyName + " aborted, see log for details.");
			}
		}

		void ValidateEnvironment()
		{
			if (!CACustomsDataRegistry.Instance.ActivateAutoB3Sending.Value)
			{
				ServiceLogger.Log(LogType.Error, FriendlyName + " Service Task not allowed, registry Customs->Canada->Testing and Development->Activate Auto Entry Sending is off.");
				shouldContinueProcessing = false;
			}
		}

#if DEBUG
		protected virtual
#endif
		IB3AutoSender GetB3AutoSender() => new B3AutoSender(ServiceLogger);
	}
}
