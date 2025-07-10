using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.BufferManagement.Module.Test
{
	public abstract class BMControllerTest : ZControllerBasherTest
	{
		public void TestBMControllers_MustOverrideUrlFlag_ShouldReturnFalse()
		{
			AssertEquals("PAVE modules should have ZController.MakeUrlsOnlyOpenableForCurrentCompany return false.", false, Controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}
	}
}
