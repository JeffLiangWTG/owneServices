using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(BillOfLadingAndDateUserControl))]
	class BillOfLadingAndDateUserControlTest : TestCaseWithFactory
	{
		public void TestControlsExistence()
		{
			using var control = new BillOfLadingAndDateUserControl();
			TestUtility.AssertControlExistance(control, "BillOfLadingTextBox", "BillOfLading");
			TestUtility.AssertControlExistance(control, "BillOfLadingDateDateEdit", "BillOfLadingDate");
		}
	}
}
