using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class COAndFTADetailsUserControl
	{

		private void InitializeComponent()
		{
            this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
            this.CertificateOfOriginIssueGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.CertificateOfOriginIssuePanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
            this.CertificateOfOriginGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.CertificateOfOriginPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
            this.FTADetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ManufacturerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
            this.SplitContainer.Panel1.SuspendLayout();
            this.SplitContainer.Panel2.SuspendLayout();
            this.SplitContainer.SuspendLayout();
            this.CertificateOfOriginIssueGroupBox.SuspendLayout();
            this.CertificateOfOriginGroupBox.SuspendLayout();
            this.FTADetailsGroupBox.SuspendLayout();
            this.ManufacturerAddressControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
            // 
            // SplitContainer
            // 
            this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.SplitContainer.Name = "SplitContainer";
            // 
            // SplitContainer.Panel1
            // 
            this.SplitContainer.Panel1.Controls.Add(this.CertificateOfOriginIssueGroupBox);
            this.SplitContainer.Panel1.Controls.Add(this.CertificateOfOriginGroupBox);
            this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1323, 351, true);
            this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            // 
            // SplitContainer.Panel2
            // 
            this.SplitContainer.Panel2.Controls.Add(this.FTADetailsGroupBox);
            this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
            this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(759);
            this.SplitContainer.SplitterWidth = 3;
            this.SplitContainer.TabIndex = 1;
            // 
            // CertificateOfOriginIssueGroupBox
            // 
            this.CertificateOfOriginIssueGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("5eca460f-71ba-409e-9146-72376c3f527b", "C/O Issuance Details");
            this.CertificateOfOriginIssueGroupBox.Controls.Add(this.CertificateOfOriginIssuePanel);
            this.CertificateOfOriginIssueGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CertificateOfOriginIssueGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 155, true);
            this.CertificateOfOriginIssueGroupBox.Name = "CertificateOfOriginIssueGroupBox";
            this.CertificateOfOriginIssueGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(759, 196, true);
            this.CertificateOfOriginIssueGroupBox.TabIndex = 7;
            this.CertificateOfOriginIssueGroupBox.TabStop = false;
            // 
            // CertificateOfOriginIssuePanel
            // 
            this.CertificateOfOriginIssuePanel.AllowDrop = true;
            this.CertificateOfOriginIssuePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CertificateOfOriginIssuePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.CertificateOfOriginIssuePanel.Name = "CertificateOfOriginIssuePanel";
            this.CertificateOfOriginIssuePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 177, true);
            this.CertificateOfOriginIssuePanel.TabIndex = 0;
            // 
            // CertificateOfOriginGroupBox
            // 
            this.CertificateOfOriginGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("000374fd-4e1e-4d6f-8501-a04ee9bddeec", "Certificate of Origin");
            this.CertificateOfOriginGroupBox.Controls.Add(this.CertificateOfOriginPanel);
            this.CertificateOfOriginGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.CertificateOfOriginGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.CertificateOfOriginGroupBox.Name = "CertificateOfOriginGroupBox";
            this.CertificateOfOriginGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(759, 155, true);
            this.CertificateOfOriginGroupBox.TabIndex = 6;
            this.CertificateOfOriginGroupBox.TabStop = false;
            // 
            // CertificateOfOriginPanel
            // 
            this.CertificateOfOriginPanel.AllowDrop = true;
            this.CertificateOfOriginPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CertificateOfOriginPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.CertificateOfOriginPanel.Name = "CertificateOfOriginPanel";
            this.CertificateOfOriginPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 136, true);
            this.CertificateOfOriginPanel.TabIndex = 0;
            // 
            // FTADetailsGroupBox
            // 
            this.FTADetailsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("9301CCF3-CACB-4002-A48F-5C0C2AA64528", "FTA Details");
            this.FTADetailsGroupBox.Controls.Add(this.ManufacturerAddressControl);
            this.FTADetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.FTADetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.FTADetailsGroupBox.Name = "FTADetailsGroupBox";
            this.FTADetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 68, true);
            this.FTADetailsGroupBox.TabIndex = 1;
            this.FTADetailsGroupBox.TabStop = false;
            // 
            // ManufacturerAddressControl
            // 
            this.ManufacturerAddressControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ManufacturerAddressControl, "Invoices.JZ_OA_ManufacturerAddress");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_OA_ManufacturerAddress)));
            this.ManufacturerAddressControl.BindToOrgList = "Lookups.Organisations";
            this.ManufacturerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 23, true);
            this.ManufacturerAddressControl.Name = "ManufacturerAddressControl";
            this.ManufacturerAddressControl.PopupCaption = "";
            this.ManufacturerAddressControl.ShowAddress = false;
            this.ManufacturerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
            this.ManufacturerAddressControl.TabIndex = 20;
            // 
            // COAndFTADetailsUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.SplitContainer);
            this.Name = "COAndFTADetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1323, 351, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.SplitContainer.Panel1.ResumeLayout(false);
            this.SplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
            this.SplitContainer.ResumeLayout(false);
            this.SplitContainer.PerformLayout();
            this.CertificateOfOriginIssueGroupBox.ResumeLayout(false);
            this.CertificateOfOriginIssueGroupBox.PerformLayout();
            this.CertificateOfOriginGroupBox.ResumeLayout(false);
            this.CertificateOfOriginGroupBox.PerformLayout();
            this.FTADetailsGroupBox.ResumeLayout(false);
            this.FTADetailsGroupBox.PerformLayout();
            this.ManufacturerAddressControl.ResumeLayout(true);
            this.ManufacturerAddressControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
		private ZGroupBox FTADetailsGroupBox;
		private ZAddressControl ManufacturerAddressControl;
		private ZGroupBox CertificateOfOriginIssueGroupBox;
		private DynamicLayoutPanel CertificateOfOriginIssuePanel;
		private ZGroupBox CertificateOfOriginGroupBox;
		private DynamicLayoutPanel CertificateOfOriginPanel;
	}
}
