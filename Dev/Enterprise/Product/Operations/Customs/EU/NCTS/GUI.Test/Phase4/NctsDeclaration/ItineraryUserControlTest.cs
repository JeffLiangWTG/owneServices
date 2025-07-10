using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class ItineraryUserControlTest : TestCaseWithFactory
	{
		public void TestCaptionRenderingEnabled()
		{
			using (var control = new ItineraryUserControl())
			{
				AssertEquals("CaptionRenderingEnabled", true, control.CaptionRenderingEnabled);
			}
		}
	}
}
