using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.Client.UPE.Module.Testing
{
	public class UPEReportsTest : TestCaseWithFactory
	{
		public void TestModuleID()
		{
			using (UPEReportsModule module = new UPEReportsModule())
			{
				AssertEquals("ModuleID should be correct and unique for this reports module so the menu item records are stored in the correct place", ClientModuleRegistration.Reports, module.ID);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (UPEReportsModule module = new UPEReportsModule())
			{
				AssertEquals("Client-specific module should have SecurityCheckpoint None", Env.Security.None, module.SecurityCheckpoint);
			}
		}
	}
}
