using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	partial class AsycudaBillUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.dynamicBilllDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.BillSpecificPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaBill);
			// 
			// dynamicBilllDetailsPanel
			// 
			this.dynamicBilllDetailsPanel.AllowDrop = true;
			this.dynamicBilllDetailsPanel.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.dynamicBilllDetailsPanel, ".");
			this.dynamicBilllDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dynamicBilllDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.dynamicBilllDetailsPanel.Name = "dynamicBilllDetailsPanel";
			this.dynamicBilllDetailsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.dynamicBilllDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1163, 169, true);
			this.dynamicBilllDetailsPanel.TabIndex = 1;
			// 
			// BillSpecificPanel
			// 
			this.BillSpecificPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BillSpecificPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 169, true);
			this.BillSpecificPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.BillSpecificPanel.Name = "BillSpecificPanel";
			this.BillSpecificPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1163, 217, true);
			this.BillSpecificPanel.TabIndex = 5;
			// 
			// AsycudaBillUserControl
			// 
			this.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1163, 0, true);
			this.Controls.Add(this.dynamicBilllDetailsPanel);
			this.Controls.Add(this.BillSpecificPanel);
			this.Name = "AsycudaBillUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1163, 498, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal DynamicLayoutPanel dynamicBilllDetailsPanel;
		ZPanel BillSpecificPanel;
	}
}
