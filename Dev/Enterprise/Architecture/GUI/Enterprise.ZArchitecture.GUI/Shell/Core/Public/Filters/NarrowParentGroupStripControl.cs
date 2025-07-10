using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	class NarrowParentGroupStripControl : GroupStripControl
	{
		public NarrowParentGroupStripControl()
		{
			FilterStripGroupBox.Layout += (sender, args) =>
			{
				UpdateLayout(Width, shouldAlwaysLayoutControls: true);
			};
		}

		protected override void SetWidths(int targetWidth)
		{
			ControlDpiScalingHelper.SetLeft(ref AddStripButton, FilterStripGroupBox.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(ButtonPadding), false);
			ControlDpiScalingHelper.SetLeft(ref DeleteStripButton, AddStripButton.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(ButtonPadding), false);
			ControlDpiScalingHelper.SetLeft(ref FilterCategoriesToolStrip, DeleteStripButton.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(ButtonPadding), false);
		}

		internal const int ButtonPadding = 10;
	}
}
