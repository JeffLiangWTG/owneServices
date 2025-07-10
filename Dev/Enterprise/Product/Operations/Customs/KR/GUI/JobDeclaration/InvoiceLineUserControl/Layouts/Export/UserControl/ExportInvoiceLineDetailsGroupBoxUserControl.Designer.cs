
namespace Enterprise.Customs.KR.GUI
{
	partial class ExportInvoiceLineDetailsGroupBoxUserControl
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
            this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ExportDetailsUserControl = new Enterprise.Customs.KR.GUI.ExportDetailsUserControl();
            this.QuantityAndWeightGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ExportQuantityAndWeightUserControl = new Enterprise.Customs.KR.GUI.ExportQuantityAndWeightUserControl();
            this.CertificateOfOriginGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.CertificateOfOriginDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
            this.ReExportGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ReExportDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.DetailsGroupBox.SuspendLayout();
            this.ExportDetailsUserControl.SuspendLayout();
            this.QuantityAndWeightGroupBox.SuspendLayout();
            this.ExportQuantityAndWeightUserControl.SuspendLayout();
            this.CertificateOfOriginGroupBox.SuspendLayout();
            this.ReExportGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobComInvoiceLine);
            // 
            // DetailsGroupBox
            // 
            this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("9239e4a2-4d06-4586-9622-ce7711929944", "Details");
            this.DetailsGroupBox.Controls.Add(this.ExportDetailsUserControl);
            this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 0, true);
            this.DetailsGroupBox.Name = "DetailsGroupBox";
            this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(697, 173, true);
            this.DetailsGroupBox.TabIndex = 0;
            this.DetailsGroupBox.TabStop = false;
            // 
            // DetailsUserControl
            // 
            this.ExportDetailsUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ExportDetailsUserControl, ".");
            this.ExportDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExportDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 17, true);
            this.ExportDetailsUserControl.Name = "ExportDetailsUserControl";
            this.ExportDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(692, 154, true);
            this.ExportDetailsUserControl.TabIndex = 0;
            // 
            // QuantityAndWeightGroupBox
            // 
            this.QuantityAndWeightGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("ecffff9f-4821-4cfd-98f4-954eed462e03", "Quantity and Weight");
            this.QuantityAndWeightGroupBox.Controls.Add(this.ExportQuantityAndWeightUserControl);
            this.QuantityAndWeightGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 173, true);
            this.QuantityAndWeightGroupBox.Name = "QuantityAndWeightGroupBox";
            this.QuantityAndWeightGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(697, 108, true);
            this.QuantityAndWeightGroupBox.TabIndex = 1;
            this.QuantityAndWeightGroupBox.TabStop = false;
						// 
						// ExportQuantityAndWeightUserControl
						// 
						this.ExportQuantityAndWeightUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ExportQuantityAndWeightUserControl, ".");
            this.ExportQuantityAndWeightUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExportQuantityAndWeightUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 17, true);
            this.ExportQuantityAndWeightUserControl.Name = "ExportQuantityAndWeightUserControl";
            this.ExportQuantityAndWeightUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(692, 89, true);
            this.ExportQuantityAndWeightUserControl.TabIndex = 0;
            // 
            // CertificateOfOriginGroupBox
            // 
            this.CertificateOfOriginGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("418938f7-7756-4a53-b070-4e896f9f49ff", "Certificate of Origin");
            this.CertificateOfOriginGroupBox.Controls.Add(this.CertificateOfOriginDynamicLayoutPanel);
            this.CertificateOfOriginGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(707, 0, true);
            this.CertificateOfOriginGroupBox.Name = "CertificateOfOriginGroupBox";
            this.CertificateOfOriginGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(341, 173, true);
            this.CertificateOfOriginGroupBox.TabIndex = 2;
            this.CertificateOfOriginGroupBox.TabStop = false;
            // 
            // CertificateOfOriginDynamicLayoutPanel
            // 
            this.CertificateOfOriginDynamicLayoutPanel.AllowDrop = true;
            this.CertificateOfOriginDynamicLayoutPanel.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.CertificateOfOriginDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CertificateOfOriginDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 17, true);
            this.CertificateOfOriginDynamicLayoutPanel.Name = "CertificateOfOriginDynamicLayoutPanel";
            this.CertificateOfOriginDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 154, true);
            this.CertificateOfOriginDynamicLayoutPanel.TabIndex = 0;
            // 
            // ReExportGroupBox
            // 
            this.ReExportGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("a423fb79-1655-46ef-a05a-e41188500d0b", "Re-Export");
            this.ReExportGroupBox.Controls.Add(this.ReExportDynamicLayoutPanel);
            this.ReExportGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(707, 173, true);
            this.ReExportGroupBox.Name = "ReExportGroupBox";
            this.ReExportGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(341, 108, true);
            this.ReExportGroupBox.TabIndex = 3;
            this.ReExportGroupBox.TabStop = false;
            // 
            // ReExportDynamicLayoutPanel
            // 
            this.ReExportDynamicLayoutPanel.AllowDrop = true;
            this.ReExportDynamicLayoutPanel.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.ReExportDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReExportDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 17, true);
            this.ReExportDynamicLayoutPanel.Name = "ReExportDynamicLayoutPanel";
            this.ReExportDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 89, true);
            this.ReExportDynamicLayoutPanel.TabIndex = 0;
            // 
            // InvoiceLineDetailsGroupBoxUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.ReExportGroupBox);
            this.Controls.Add(this.CertificateOfOriginGroupBox);
            this.Controls.Add(this.QuantityAndWeightGroupBox);
            this.Controls.Add(this.DetailsGroupBox);
            this.Name = "InvoiceLineDetailsGroupBoxUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1050, 296, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.DetailsGroupBox.ResumeLayout(false);
            this.DetailsGroupBox.PerformLayout();
            this.ExportDetailsUserControl.ResumeLayout(true);
            this.ExportDetailsUserControl.PerformLayout();
            this.QuantityAndWeightGroupBox.ResumeLayout(false);
            this.QuantityAndWeightGroupBox.PerformLayout();
            this.ExportQuantityAndWeightUserControl.ResumeLayout(true);
            this.ExportQuantityAndWeightUserControl.PerformLayout();
            this.CertificateOfOriginGroupBox.ResumeLayout(false);
            this.CertificateOfOriginGroupBox.PerformLayout();
            this.ReExportGroupBox.ResumeLayout(false);
            this.ReExportGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private ZArchitecture.GUI.ZGroupBox QuantityAndWeightGroupBox;
		private ZArchitecture.GUI.ZGroupBox CertificateOfOriginGroupBox;
		private ZArchitecture.GUI.DynamicLayoutPanel CertificateOfOriginDynamicLayoutPanel;
		private ZArchitecture.GUI.ZGroupBox ReExportGroupBox;
		private ZArchitecture.GUI.DynamicLayoutPanel ReExportDynamicLayoutPanel;
		private KR.GUI.ExportDetailsUserControl ExportDetailsUserControl;
		private KR.GUI.ExportQuantityAndWeightUserControl ExportQuantityAndWeightUserControl;
	}
}
