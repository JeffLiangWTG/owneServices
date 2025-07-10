namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class BringToFrontActionTest : NodeNetworkActionsTestCase
	{
		public void TestApplicability()
		{
			AssertActionIsNotAllowedForRootDiagramButAllowedForChildShapes((networkViewModel) => new BringToFrontAction(networkViewModel), AccessibilityLevel.Applicability);
		}

		public void TestEnabledness()
		{
			AssertActionIsNotAllowedForRootDiagramButAllowedForChildShapes((networkViewModel) => new BringToFrontAction(networkViewModel), AccessibilityLevel.Enabledness);
		}

		protected override void TestGetIconNameCore()
		{
			AssertActionHasCorrectIconName(networkViewModel => new BringToFrontAction(networkViewModel), "BringToFront");
		}
	}
}
