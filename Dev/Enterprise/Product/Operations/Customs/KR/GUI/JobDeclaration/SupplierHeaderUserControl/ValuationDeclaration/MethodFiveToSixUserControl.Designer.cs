namespace Enterprise.Customs.KR.GUI
{
	partial class MethodFiveToSixUserControl
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
            this.AmountAgreedUponWithCustomsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.AmountAgreedUponWithCustomsDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
            this.AdditionalCostGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.AdditionalCostDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.AmountAgreedUponWithCustomsGroupBox.SuspendLayout();
            this.AdditionalCostGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobComInvoiceHeader);
            // 
            // AmountAgreedUponWithCustomsGroupBox
            // 
            this.AmountAgreedUponWithCustomsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("AF4D8F7C-86B9-4373-A808-43E3358CFD17", "Amount Agreed Upon With Customs");
			this.AmountAgreedUponWithCustomsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.AmountAgreedUponWithCustomsGroupBox.Controls.Add(this.AmountAgreedUponWithCustomsDynamicLayoutPanel);
            this.AmountAgreedUponWithCustomsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 21, true);
            this.AmountAgreedUponWithCustomsGroupBox.Name = "AmountAgreedUponWithCustomsGroupBox";
            this.AmountAgreedUponWithCustomsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(625, 44, true);
            this.AmountAgreedUponWithCustomsGroupBox.TabIndex = 0;
            this.AmountAgreedUponWithCustomsGroupBox.TabStop = false;
            // 
            // AmountAgreedUponWithCustomsDynamicLayoutPanel
            // 
            this.AmountAgreedUponWithCustomsDynamicLayoutPanel.AllowDrop = true;
            this.AmountAgreedUponWithCustomsDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AmountAgreedUponWithCustomsDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 16, true);
            this.AmountAgreedUponWithCustomsDynamicLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
            this.AmountAgreedUponWithCustomsDynamicLayoutPanel.Name = "AmountAgreedUponWithCustomsDynamicLayoutPanel";
            this.AmountAgreedUponWithCustomsDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 26, true);
            this.AmountAgreedUponWithCustomsDynamicLayoutPanel.TabIndex = 1;
            // 
            // AdditionalCostGroupBox
            // 
            this.AdditionalCostGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("76326E73-2EF5-4D74-8000-522DB41ED093", "Additional Cost");
            this.AdditionalCostGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AdditionalCostGroupBox.Controls.Add(this.AdditionalCostDynamicLayoutPanel);
            this.AdditionalCostGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 104, true);
            this.AdditionalCostGroupBox.Name = "AdditionalCostGroupBox";
            this.AdditionalCostGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(625, 136, true);
            this.AdditionalCostGroupBox.TabIndex = 1;
            this.AdditionalCostGroupBox.TabStop = false;
            // 
            // AdditionalCostDynamicLayoutPanel
            // 
            this.AdditionalCostDynamicLayoutPanel.AllowDrop = true;
            this.AdditionalCostDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AdditionalCostDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 16, true);
            this.AdditionalCostDynamicLayoutPanel.Name = "AdditionalCostDynamicLayoutPanel";
            this.AdditionalCostDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 118, true);
            this.AdditionalCostDynamicLayoutPanel.TabIndex = 5;
			// 
			// MethodFiveToSixUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.AdditionalCostGroupBox);
            this.Controls.Add(this.AmountAgreedUponWithCustomsGroupBox);
            this.Name = "MethodFiveToSixUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(625, 180, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.AmountAgreedUponWithCustomsGroupBox.ResumeLayout(false);
            this.AmountAgreedUponWithCustomsGroupBox.PerformLayout();
            this.AdditionalCostGroupBox.ResumeLayout(false);
            this.AdditionalCostGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZGroupBox AmountAgreedUponWithCustomsGroupBox;
		public ZArchitecture.GUI.ZGroupBox AdditionalCostGroupBox;
		private ZArchitecture.GUI.DynamicLayoutPanel AmountAgreedUponWithCustomsDynamicLayoutPanel;
		private ZArchitecture.GUI.DynamicLayoutPanel AdditionalCostDynamicLayoutPanel;
	}
}
