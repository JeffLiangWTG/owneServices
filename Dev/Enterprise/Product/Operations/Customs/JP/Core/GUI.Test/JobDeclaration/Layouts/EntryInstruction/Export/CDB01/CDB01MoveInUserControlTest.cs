using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI.UserControls.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing;

[TestedType(typeof(CDB01MoveInUserControl))]
sealed class CDB01MoveInUserControlTest : TestCaseWithFactory
{
	public void TestControls()
	{
		using (var control = new CDB01MoveInUserControl())
		{
			TestHelper.AssertControlExists(control, "MoveInDateDateEdit", "MoveInDate");
			TestHelper.AssertControlExists(control, "MoveInGroupBox", "");
			TestHelper.AssertControlExists(control, "IsMoveInNoticeEntryNumSystemGeneratedCheckBox", "RequestMoveInNotice");
			TestHelper.AssertControlExists(control, "MoveInDestinationCodeFindBox", "MoveInDestination");
			TestHelper.AssertControlExists(control, "MoveInNoticeTextBox", "MoveInNotice");
			TestHelper.AssertControlExists(control, "MoveInQuantityCalcEdit", "MoveInQuantity");
			TestHelper.AssertControlExists(control, "MoveInWeightCalcEdit", "MoveInWeight");
		}
	}
}
