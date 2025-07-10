using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class JobBillingExRateConfigModule : ZPopupModule
	{
		public override ModuleIdentifier ID => ModuleIDs.JobBillingExRateSysConfig;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.JobExRateSysConfigs;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Accountant;

		protected override ZPopupController GetNewController()
		{
			return (ZSingletonController)ZControllerFactory.Create(ControllerIDs.JobBillingExRateSysConfig);
		}
	}
}
