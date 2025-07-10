using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	partial class AsycudaBillPartiesUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.DynamicBillPartiesPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.BillPartiesSpecificPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaBill);
			// 
			// DynamicBillPartiesPanel
			// 
			this.DynamicBillPartiesPanel.AllowDrop = true;
			this.DynamicBillPartiesPanel.AutoScroll = true;
			this.DynamicBillPartiesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicBillPartiesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DynamicBillPartiesPanel.Name = "DynamicBillPartiesPanel";
			this.DynamicBillPartiesPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DynamicBillPartiesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1163, 281, true);
			this.DynamicBillPartiesPanel.TabIndex = 1;
			// 
			// BillPartiesSpecificPanel
			// 
			this.BillPartiesSpecificPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BillPartiesSpecificPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 281, true);
			this.BillPartiesSpecificPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.BillPartiesSpecificPanel.Name = "BillPartiesSpecificPanel";
			this.BillPartiesSpecificPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1163, 217, true);
			this.BillPartiesSpecificPanel.TabIndex = 5;
			// 
			// AsycudaBillPartiesUserControl
			// 
			this.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1165, 0, true);
			this.Controls.Add(this.DynamicBillPartiesPanel);
			this.Controls.Add(this.BillPartiesSpecificPanel);
			this.Name = "AsycudaBillPartiesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1165, 327, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal DynamicLayoutPanel DynamicBillPartiesPanel;
		ZPanel BillPartiesSpecificPanel;
	}
}
