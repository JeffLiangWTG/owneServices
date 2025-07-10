namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class SendToBackActionTest : NodeNetworkActionsTestCase
	{
		public void TestApplicability()
		{
			AssertActionIsNotAllowedForRootDiagramButAllowedForChildShapes((networkViewModel) => new SendToBackAction(networkViewModel), AccessibilityLevel.Applicability);
		}

		public void TestEnabledness()
		{
			AssertActionIsNotAllowedForRootDiagramButAllowedForChildShapes((networkViewModel) => new SendToBackAction(networkViewModel), AccessibilityLevel.Enabledness);
		}

		protected override void TestGetIconNameCore()
		{
			AssertActionHasCorrectIconName(networkViewModel => new SendToBackAction(networkViewModel), "SendToBack");
		}
	}
}
