namespace Enterprise.Customs.KR.GUI
{
	partial class MethodFourUserControl
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
            this.SalesOfHighestQuantityGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.SalesOfHighestQuantityDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
            this.DeductionCostGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.DeductionCostDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SalesOfHighestQuantityGroupBox.SuspendLayout();
            this.DeductionCostGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobComInvoiceHeader);
			// 
			// SalesOfHighestQuantityGroupBox
			// 
			this.SalesOfHighestQuantityGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("3559A1A1-6537-4066-9B72-9B74EA2354A6", "Sales of highest quantity");
            this.SalesOfHighestQuantityGroupBox.Controls.Add(this.SalesOfHighestQuantityDynamicLayoutPanel);
            this.SalesOfHighestQuantityGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.SalesOfHighestQuantityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.SalesOfHighestQuantityGroupBox.Name = "SalesOfHighestQuantityGroupBox";
            this.SalesOfHighestQuantityGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 87, true);
            this.SalesOfHighestQuantityGroupBox.TabIndex = 0;
            this.SalesOfHighestQuantityGroupBox.TabStop = false;
            // 
            // SalesOfHighestQuantityDynamicLayoutPanel
            // 
            this.SalesOfHighestQuantityDynamicLayoutPanel.AllowDrop = true;
            this.SalesOfHighestQuantityDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SalesOfHighestQuantityDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 16, true);
            this.SalesOfHighestQuantityDynamicLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
            this.SalesOfHighestQuantityDynamicLayoutPanel.Name = "SalesOfHighestQuantityDynamicLayoutPanel";
            this.SalesOfHighestQuantityDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 69, true);
            this.SalesOfHighestQuantityDynamicLayoutPanel.TabIndex = 1;
            // 
            // DeductionCostGroupBox
            // 
            this.DeductionCostGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("0736B41F-1C05-491A-BA19-FB7267159467", "Deduction Cost");
            this.DeductionCostGroupBox.Controls.Add(this.DeductionCostDynamicLayoutPanel);
            this.DeductionCostGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DeductionCostGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 87, true);
            this.DeductionCostGroupBox.Name = "DeductionCostGroupBox";
            this.DeductionCostGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 134, true);
            this.DeductionCostGroupBox.TabIndex = 1;
            this.DeductionCostGroupBox.TabStop = false;
            // 
            // DeductionCostDynamicLayoutPanel
            // 
            this.DeductionCostDynamicLayoutPanel.AllowDrop = true;
            this.DeductionCostDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DeductionCostDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 16, true);
            this.DeductionCostDynamicLayoutPanel.Name = "DeductionCostDynamicLayoutPanel";
            this.DeductionCostDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 117, true);
            this.DeductionCostDynamicLayoutPanel.TabIndex = 5;
            // 
            // MethodFourUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.DeductionCostGroupBox);
            this.Controls.Add(this.SalesOfHighestQuantityGroupBox);
            this.Name = "MethodFourUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 221, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.SalesOfHighestQuantityGroupBox.ResumeLayout(false);
            this.SalesOfHighestQuantityGroupBox.PerformLayout();
            this.DeductionCostGroupBox.ResumeLayout(false);
            this.DeductionCostGroupBox.PerformLayout();
			this.CaptionRenderingEnabled = true;
			this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZGroupBox SalesOfHighestQuantityGroupBox;
		public ZArchitecture.GUI.ZGroupBox DeductionCostGroupBox;
		private ZArchitecture.GUI.DynamicLayoutPanel SalesOfHighestQuantityDynamicLayoutPanel;
		private ZArchitecture.GUI.DynamicLayoutPanel DeductionCostDynamicLayoutPanel;
	}
}
