using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	sealed class AsycudaSupportingDocumentUserControlTest : TestCaseWithFactory
	{
		public void TestAdditionalTabPage()
		{
			using (var control = new AsycudaSupportingDocumentUserControl())
			{
				AssertEquals("Control's additional tab caption", "Supporting Documents", ((IAdditionalTabPage)control).AdditionalTabPageCaption.Caption);
				AssertEquals("Control's tab page sequence", 1, ((IAdditionalTabPage)control).TabPageSequence);
				AssertEquals("Control should be visible", true, ((IAdditionalTabPage)control).AdditionalControlVisibility.isVisible.Invoke(null));
				AssertSame("Control should be referenced correctly", control, ((IAdditionalTabPage)control).AdditionalTabPageUserControl);
			}
		}
	}
}
