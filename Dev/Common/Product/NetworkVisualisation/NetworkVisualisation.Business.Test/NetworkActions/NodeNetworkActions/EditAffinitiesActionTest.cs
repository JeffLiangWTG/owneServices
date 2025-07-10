namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class EditAffinitiesActionTest : NodeNetworkActionsTestCase
	{
		public void TestApplicability()
		{
			AssertActionIsAllowedForRootDiagramButNotAllowedForChildShapes((networkViewModel) => new EditAffinitiesAction(networkViewModel), AccessibilityLevel.Applicability);
		}

		public void TestEnabledness()
		{
			AssertActionIsAllowedForRootDiagramButNotAllowedForChildShapes((networkViewModel) => new EditAffinitiesAction(networkViewModel), AccessibilityLevel.Enabledness);
		}

		protected override void TestGetIconNameCore()
		{
			AssertActionHasCorrectIconName(networkViewModel => new EditAffinitiesAction(networkViewModel), "Edit");
		}
	}
}
