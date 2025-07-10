using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class CashAdvanceModuleTest : ZModuleBasherTest
	{
		public void TestLicenseCheckpoints()
		{
			using (var module = new ARCashAdvanceModule())
			{
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestRecentItemsSection_ShouldBeNotShown()
		{
			using (var apCashAdvaneceModule = new APCashAdvanceModule())
			using (var arCashAdvaneceModule = new ARCashAdvanceModule())
			{
				AssertEquals("Recent items section should be not shown in AR cash advance module", true, arCashAdvaneceModule.DoNotShowRecentItems);
				AssertEquals("Recent items section should be not shown in AP cash advance module", true, apCashAdvaneceModule.DoNotShowRecentItems);
			}
		}
	}
}
