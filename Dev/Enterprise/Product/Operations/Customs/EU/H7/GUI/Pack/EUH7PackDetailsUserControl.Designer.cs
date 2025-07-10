using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	partial class EUH7PackDetailsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.DynamicPackDetailsPanel = new DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaPack);
			//
			// DynamicPackDetailsPanel
			//
			this.DynamicPackDetailsPanel.AllowDrop = true;
			this.DynamicPackDetailsPanel.AutoScroll = true;
			this.DynamicPackDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicPackDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DynamicPackDetailsPanel.Name = "DynamicPackDetailsPanel";
			this.DynamicPackDetailsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DynamicPackDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1003, 169, true);
			this.DynamicPackDetailsPanel.TabIndex = 1;
			//
			// EUH7PackDetailsUserControl
			//
			this.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("9edb9432-a6fa-4f4e-8258-9dc0e6b38568", "Pack Details");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DynamicPackDetailsPanel);
			this.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Name = "EUH7PackDetailsUserControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 391, true);
			this.TabIndex = 0;

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected DynamicLayoutPanel DynamicPackDetailsPanel;
	}
}
