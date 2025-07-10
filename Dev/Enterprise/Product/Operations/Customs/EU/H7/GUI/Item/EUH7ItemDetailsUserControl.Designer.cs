using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	partial class EUH7ItemDetailsUserControl
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
			this.DynamicPackedItemDetailsPanel = new DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaPackedItem);
			//
			// DynamicPackedItemDetailsPanel
			//
			this.DynamicPackedItemDetailsPanel.AllowDrop = true;
			this.DynamicPackedItemDetailsPanel.AutoScroll = true;
			this.DynamicPackedItemDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicPackedItemDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DynamicPackedItemDetailsPanel.Name = "DynamicPackedItemDetailsPanel";
			this.DynamicPackedItemDetailsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DynamicPackedItemDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1003, 169, true);
			this.DynamicPackedItemDetailsPanel.TabIndex = 1;
			//
			// EUH7ItemDetailsUserControl
			//
			this.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("9edb9432-a6fa-4f4e-8258-9dc0e6b38568", "Packed Item Details");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DynamicPackedItemDetailsPanel);
			this.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Name = "EUH7ItemDetailsUserControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 391, true);
			this.TabIndex = 0;

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected DynamicLayoutPanel DynamicPackedItemDetailsPanel;
	}
}
