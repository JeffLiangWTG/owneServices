using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class FilterRuleFilterStripControl
	{
		private void InitializeComponent()
		{
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// SplitContainer
			// 
			this.SplitContainer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.IsSplitterFixed = true;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 174, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(87);
			this.SplitContainer.TabIndex = 7;
			this.SplitContainer.TabStop = false;
			// 
			// FilterRuleFilterStripControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.Name = "FilterRuleFilterStripControl";
			this.Controls.SetChildIndex(this.FilterStripsPanel, 0);
			this.Controls.SetChildIndex(this.AddStripButton, 0);
			this.Controls.SetChildIndex(this.ToolStripRecordsFoundLabel, 0);
			this.Controls.SetChildIndex(this.SplitContainer, 0);
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
