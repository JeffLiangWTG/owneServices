using CargoWise.EntityFramework.Testing;

namespace Enterprise.CommissionManagement.Module.Testing
{
	public class CommissionLinesPreviewPaneGuiStateTest : TestCaseWithFactory
	{
		public void TestSplitterDistance()
		{
			using (var control = new CommissionLinesPreviewPane())
			{
				var guiState = new CommissionLinesPreviewPaneGuiState(control);
				guiState.SplitterDistance = 1;
				AssertEquals(1, guiState.SplitterDistance);
				guiState.SplitterDistance = 100;
				AssertEquals(100, guiState.SplitterDistance);
			}
		}
	}
}
