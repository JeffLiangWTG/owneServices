using CargoWise.EntityFramework.Testing;
using Enterprise.CommissionManagement.Business;
using Enterprise.CommissionManagement.GUI;

namespace Enterprise.CommissionManagement.Module.Testing
{
	public class CommissionManagementFilterControlGuiStateTest : TestCaseWithFactory
	{
		public void TestMainSplitterPosition()
		{
			using (var control = new CommissionManagementFilterControl(new ViewCommissionLineCollection(Factory), new CommissionManagementFilterBusinessObject()))
			{
				var guiState = new CommissionManagementFilterControlGuiState(control);
				guiState.MainSplitterPosition = 1;
				AssertEquals(1, guiState.MainSplitterPosition);
				guiState.MainSplitterPosition = 100;
				AssertEquals(100, guiState.MainSplitterPosition);
			}
		}
	}
}
