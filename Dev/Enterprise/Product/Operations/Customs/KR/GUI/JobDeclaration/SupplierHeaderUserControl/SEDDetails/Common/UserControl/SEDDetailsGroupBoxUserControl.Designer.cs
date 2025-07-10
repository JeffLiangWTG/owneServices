
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.KR.GUI
{
	partial class SEDDetailsGroupBoxUserControl
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
			this.SupplierGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SupplierDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.CertificateOfOriginGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CertificateOfOriginDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.ManufacturerGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ManufacturerDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.ImporterDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.ImporterGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SupplierGroupBox.SuspendLayout();
			this.CertificateOfOriginGroupBox.SuspendLayout();
			this.ManufacturerGroupBox.SuspendLayout();
			this.ImporterGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
			// 
			// SupplierGroupBox
			// 
			this.SupplierGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("424E9E65-B704-41A0-9067-7D13FB51C312", "Supplier");
			this.SupplierGroupBox.Controls.Add(this.SupplierDynamicLayoutPanel);
			this.SupplierGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.SupplierGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupplierGroupBox.Name = "SupplierGroupBox";
			this.SupplierGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(505, 65, true);
			this.SupplierGroupBox.TabIndex = 15;
			this.SupplierGroupBox.TabStop = false;
			// 
			// SupplierDynamicLayoutPanel
			// 
			this.SupplierDynamicLayoutPanel.AllowDrop = true;
			this.SupplierDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupplierDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 16, true);
			this.SupplierDynamicLayoutPanel.Name = "SupplierDynamicLayoutPanel";
			this.SupplierDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 47, true);
			this.SupplierDynamicLayoutPanel.TabIndex = 1;
			// 
			// CertificateOfOriginGroupBox
			// 
			this.CertificateOfOriginGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("9A42A418-AF39-4ABB-A05E-A6413CC07DFD", "Certificate Of Origin");
			this.CertificateOfOriginGroupBox.Controls.Add(this.CertificateOfOriginDynamicLayoutPanel);
			this.CertificateOfOriginGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.CertificateOfOriginGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 65, true);
			this.CertificateOfOriginGroupBox.Name = "CertificateOfOriginGroupBox";
			this.CertificateOfOriginGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(505, 117, true);
			this.CertificateOfOriginGroupBox.TabIndex = 16;
			this.CertificateOfOriginGroupBox.TabStop = false;
			// 
			// CertificateOfOriginDynamicLayoutPanel
			// 
			this.CertificateOfOriginDynamicLayoutPanel.AllowDrop = true;
			this.CertificateOfOriginDynamicLayoutPanel.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
			this.CertificateOfOriginDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CertificateOfOriginDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 16, true);
			this.CertificateOfOriginDynamicLayoutPanel.Name = "CertificateOfOriginDynamicLayoutPanel";
			this.CertificateOfOriginDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 99, true);
			this.CertificateOfOriginDynamicLayoutPanel.TabIndex = 1;
			// 
			// ManufacturerGroupBox
			// 
			this.ManufacturerGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("5770BC06-8B0D-4BD2-92CA-F7858F53FB70", "Manufacturer");
			this.ManufacturerGroupBox.Controls.Add(this.ManufacturerDynamicLayoutPanel);
			this.ManufacturerGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ManufacturerGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 181, true);
			this.ManufacturerGroupBox.Name = "ManufacturerGroupBox";
			this.ManufacturerGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(505, 117, true);
			this.ManufacturerGroupBox.TabIndex = 17;
			this.ManufacturerGroupBox.TabStop = false;
			// 
			// ManufacturerDynamicLayoutPanel
			// 
			this.ManufacturerDynamicLayoutPanel.AllowDrop = true;
			this.ManufacturerDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ManufacturerDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 16, true);
			this.ManufacturerDynamicLayoutPanel.Name = "ManufacturerDynamicLayoutPanel";
			this.ManufacturerDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 99, true);
			this.ManufacturerDynamicLayoutPanel.TabIndex = 1;
			// 
			// ImporterDynamicLayoutPanel
			// 
			this.ImporterDynamicLayoutPanel.AllowDrop = true;
			this.ImporterDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImporterDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 16, true);
			this.ImporterDynamicLayoutPanel.Name = "ImporterDynamicLayoutPanel";
			this.ImporterDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 45, true);
			this.ImporterDynamicLayoutPanel.TabIndex = 1;
			// 
			// ImporterGroupBox
			// 
			this.ImporterGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("C023D2C9-4ED7-4516-A96C-4770B25A5D76", "Importer");
			this.ImporterGroupBox.Controls.Add(this.ImporterDynamicLayoutPanel);
			this.ImporterGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ImporterGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 298, true);
			this.ImporterGroupBox.Name = "ImporterGroupBox";
			this.ImporterGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(505, 63, true);
			this.ImporterGroupBox.TabIndex = 18;
			this.ImporterGroupBox.TabStop = false;
			// 
			// SEDDetailsGroupBoxUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ImporterGroupBox);
			this.Controls.Add(this.ManufacturerGroupBox);
			this.Controls.Add(this.CertificateOfOriginGroupBox);
			this.Controls.Add(this.SupplierGroupBox);
			this.Name = "SEDDetailsGroupBoxUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(505, 375, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupplierGroupBox.ResumeLayout(false);
			this.SupplierGroupBox.PerformLayout();
			this.CertificateOfOriginGroupBox.ResumeLayout(false);
			this.CertificateOfOriginGroupBox.PerformLayout();
			this.ManufacturerGroupBox.ResumeLayout(false);
			this.ManufacturerGroupBox.PerformLayout();
			this.ImporterGroupBox.ResumeLayout(false);
			this.ImporterGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.ZGroupBox SupplierGroupBox;
		private ZArchitecture.GUI.DynamicLayoutPanel SupplierDynamicLayoutPanel;
		internal ZArchitecture.GUI.ZGroupBox CertificateOfOriginGroupBox;
		private ZArchitecture.GUI.DynamicLayoutPanel CertificateOfOriginDynamicLayoutPanel;
		internal ZArchitecture.GUI.ZGroupBox ManufacturerGroupBox;
		private ZArchitecture.GUI.DynamicLayoutPanel ManufacturerDynamicLayoutPanel;
		private ZArchitecture.GUI.DynamicLayoutPanel ImporterDynamicLayoutPanel;
		internal ZArchitecture.GUI.ZGroupBox ImporterGroupBox;
	}
}
