using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(JobBillingExRateConfigModule))]
	public class JobBillingExRateConfigModuleTest : ZPopupModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.JobBillingExRateSysConfig;
		}
	}
}
