using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class NarrowParentGroupStripControlTest : TestCaseWithFactory
	{
		public void TestResizeGroupBox_ShouldUpdateLayoutBasedOnGroupBox()
		{
			using (var control = new NarrowParentGroupStripControl())
			using (var strip = new ZFilterStrip())
			{
				control.UpdateLayout(control.Width, shouldAlwaysLayoutControls: true);

				AssertEquals(control.FilterStripGroupBox.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(NarrowParentGroupStripControl.ButtonPadding), control.AddStripButton.Left);
				AssertEquals(control.AddStripButton.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(NarrowParentGroupStripControl.ButtonPadding), control.DeleteStripButton.Left);
				AssertEquals(control.DeleteStripButton.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(NarrowParentGroupStripControl.ButtonPadding), control.FilterCategoriesToolStrip.Left);

				var originalDeleteLeft = control.DeleteStripButton.Left;
				var originalCategoryLeft = control.FilterCategoriesToolStrip.Left;

				control.FilterStripGroupBox.Controls.Add(strip);
				control.UpdateLayout(control.Width, shouldAlwaysLayoutControls: true);

				AssertEquals(control.FilterStripGroupBox.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(NarrowParentGroupStripControl.ButtonPadding), control.AddStripButton.Left);
				AssertEquals(control.AddStripButton.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(NarrowParentGroupStripControl.ButtonPadding), control.DeleteStripButton.Left);
				AssertEquals(control.DeleteStripButton.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(NarrowParentGroupStripControl.ButtonPadding), control.FilterCategoriesToolStrip.Left);

				AssertEquals(true, control.DeleteStripButton.Left > originalDeleteLeft);
				AssertEquals(true, control.FilterCategoriesToolStrip.Left > originalCategoryLeft);
			}
		}
	}
}
