using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class MiscOptionsUserControlTest : TestCaseWithFactory
	{
		public void TestBranchFindBox()
		{
			using (var miscOptionsUserControl = new MiscOptionsUserControl())
			{
				var branchFindBox = miscOptionsUserControl.FindSingleOrDefault<ZGuidFindBox>("BranchFindBox");
				AssertNotNull(branchFindBox);
				Assert("Visible", branchFindBox.Visible);
				AssertEquals("PreBoundMaxLength", 3, branchFindBox.PreBoundMaxLength);
			}
		}
	}
}
