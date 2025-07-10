using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.Forms
{
	public partial class ZPostingButtonsUserControl
	{

		#region Component Designer generated code

		public ZToolStripButton SaveButton;
		public ZToolStripButton SaveAndCloseButton;
		public ZToolStripButton CloseButton;
		ZToolStrip toolStrip;
		System.ComponentModel.Container components = null;

		void InitializeComponent()
		{
			this.toolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// toolStrip
			// 
			this.toolStrip.AutoSize = true;
			this.toolStrip.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left); //Right doesn't work - German sticks off-screen, and there's no autosize fixing it
			this.toolStrip.Dock = System.Windows.Forms.DockStyle.None; //Right doesn't work - it just gets mashed off-screen even in English
			this.toolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.toolStrip.TabIndex = 0;
			this.toolStrip.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.toolStrip.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.toolStrip.GripStyle = ToolStripGripStyle.Hidden;
			this.toolStrip.BackColor = Color.Transparent;
			// 
			// ZPostingButtonsUserControl
			// 
			this.AutoSize = true;
			//this.AutoSizeMode = AutoSizeMode.GrowAndShrink; //The promised shrinking didn't happen.
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.toolStrip);
			this.Name = "ZPostingButtonsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
