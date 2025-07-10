using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI.UserControls.Testing;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	class MiscOptionsLayoutUserControlTest : TestCaseWithFactory
	{
		public void TestControlsExist()
		{
			using (var control = new MiscOptionsLayoutUserControl())
			{
				TestHelper.AssertControlExists(control, "AsycudaRelatedDeclarationsUserControl", ".");
			}
		}
	}
}
