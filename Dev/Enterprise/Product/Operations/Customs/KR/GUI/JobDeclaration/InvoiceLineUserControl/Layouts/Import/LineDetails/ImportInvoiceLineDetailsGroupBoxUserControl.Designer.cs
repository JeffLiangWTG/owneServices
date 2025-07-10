using Enterprise.Customs.GUI;

namespace Enterprise.Customs.KR.GUI
{
	partial class ImportInvoiceLineDetailsGroupBoxUserControl
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
            this.DutyAndTaxInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.DutyAndTaxInfoPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
            this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.DetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
            this.QuantityAndWeightGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.QuantityAndWeightPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.DutyAndTaxInfoGroupBox.SuspendLayout();
            this.DetailsGroupBox.SuspendLayout();
            this.QuantityAndWeightGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
            // 
            // DutyAndTaxInfoGroupBox
            // 
            this.DutyAndTaxInfoGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("f5191c69-99f8-4c45-8cb5-7d30f38b274a", "Duty and Tax Info");
            this.DutyAndTaxInfoGroupBox.Controls.Add(this.DutyAndTaxInfoPanel);
            this.DutyAndTaxInfoGroupBox.Dock = System.Windows.Forms.DockStyle.Right;
            this.DutyAndTaxInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(859, 0, true);
            this.DutyAndTaxInfoGroupBox.Name = "DutyAndTaxInfoGroupBox";
            this.DutyAndTaxInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 395, true);
            this.DutyAndTaxInfoGroupBox.TabIndex = 20;
            this.DutyAndTaxInfoGroupBox.TabStop = false;
            // 
            // DutyAndTaxInfoPanel
            // 
            this.DutyAndTaxInfoPanel.AllowDrop = true;
            this.DutyAndTaxInfoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DutyAndTaxInfoPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.DutyAndTaxInfoPanel.Name = "DutyAndTaxInfoPanel";
            this.DutyAndTaxInfoPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 378, true);
            this.DutyAndTaxInfoPanel.TabIndex = 0;
            // 
            // DetailsGroupBox
            // 
            this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("40b005f0-0b41-450e-9620-7f0a759a7a52", "Details");
            this.DetailsGroupBox.Controls.Add(this.DetailsPanel);
            this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.DetailsGroupBox.Name = "DetailsGroupBox";
            this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 168, true);
            this.DetailsGroupBox.TabIndex = 18;
            this.DetailsGroupBox.TabStop = false;
            // 
            // DetailsPanel
            // 
            this.DetailsPanel.AllowDrop = true;
            this.DetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.DetailsPanel.Name = "DetailsPanel";
            this.DetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(855, 151, true);
            this.DetailsPanel.TabIndex = 0;
            // 
            // QuantityAndWeightGroupBox
            // 
            this.QuantityAndWeightGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("aa13180c-867f-4b8b-b335-c2124ad089fd", "Quantity and Weight");
            this.QuantityAndWeightGroupBox.Controls.Add(this.QuantityAndWeightPanel);
            this.QuantityAndWeightGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.QuantityAndWeightGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 168, true);
            this.QuantityAndWeightGroupBox.Name = "QuantityAndWeightGroupBox";
            this.QuantityAndWeightGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 227, true);
            this.QuantityAndWeightGroupBox.TabIndex = 19;
            this.QuantityAndWeightGroupBox.TabStop = false;
            // 
            // QuantityAndWeightPanel
            // 
            this.QuantityAndWeightPanel.AllowDrop = true;
            this.QuantityAndWeightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.QuantityAndWeightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.QuantityAndWeightPanel.Name = "QuantityAndWeightPanel";
            this.QuantityAndWeightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(855, 210, true);
            this.QuantityAndWeightPanel.TabIndex = 0;
            // 
            // ImportInvoiceLineDetailsGroupBoxUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.QuantityAndWeightGroupBox);
            this.Controls.Add(this.DetailsGroupBox);
            this.Controls.Add(this.DutyAndTaxInfoGroupBox);
            this.Name = "ImportInvoiceLineDetailsGroupBoxUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1249, 395, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.DutyAndTaxInfoGroupBox.ResumeLayout(false);
            this.DutyAndTaxInfoGroupBox.PerformLayout();
            this.DetailsGroupBox.ResumeLayout(false);
            this.DetailsGroupBox.PerformLayout();
            this.QuantityAndWeightGroupBox.ResumeLayout(false);
            this.QuantityAndWeightGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private ZArchitecture.GUI.ZGroupBox QuantityAndWeightGroupBox;
		private ZArchitecture.GUI.ZGroupBox DutyAndTaxInfoGroupBox;
		private ZArchitecture.GUI.DynamicLayoutPanel DetailsPanel;
		private ZArchitecture.GUI.DynamicLayoutPanel QuantityAndWeightPanel;
		private ZArchitecture.GUI.DynamicLayoutPanel DutyAndTaxInfoPanel;
	}
}
