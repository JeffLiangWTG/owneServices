using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	partial class ZFilterStripCommonControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ZFilterStripControlCommon
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.BackColor = System.Drawing.Color.Transparent;
			this.CaptionRenderingEnabled = false;
			this.Name = "ZFilterStripControlCommon";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			RecentItemsLabel.ContextMenu = new ContextMenu();
			this.ResumeLayout(false);
			this.PerformLayout();

			// 
			// ToolStripPermissionsLabel
			//
			this.ToolStripPermissionsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ToolStripPermissionsLabel.AutoSize = true;
			this.ToolStripPermissionsLabel.ForeColor = System.Drawing.Color.Red;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ToolStripPermissionsLabel, false);
			this.ToolStripPermissionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(800, 20, true);
			this.ToolStripPermissionsLabel.Name = "ToolStripPermissionsLabel";
			this.ToolStripPermissionsLabel.TabIndex = 5;

			this.Controls.Add(this.ToolStripPermissionsLabel);
		}

		#endregion

		protected ZLabel ToolStripPermissionsLabel;
	}
}
