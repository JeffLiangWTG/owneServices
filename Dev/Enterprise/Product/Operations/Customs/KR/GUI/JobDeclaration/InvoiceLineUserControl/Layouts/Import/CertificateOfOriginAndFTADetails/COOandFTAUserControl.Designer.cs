using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	partial class COOandFTAUserControl
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
			this.FTAApplicationLeftAreaPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CertificateOfOriginIssueGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CertificateOfOriginIssuePanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.CertificateOfOriginGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CertificateOfOriginPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.FTAApplicationRightAreaPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DetailedFTAGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DetailedFTAPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.FTADetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FTADetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FTAApplicationLeftAreaPanel.SuspendLayout();
			this.CertificateOfOriginIssueGroupBox.SuspendLayout();
			this.CertificateOfOriginGroupBox.SuspendLayout();
			this.FTAApplicationRightAreaPanel.SuspendLayout();
			this.DetailedFTAGroupBox.SuspendLayout();
			this.FTADetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
			// 
			// FTAApplicationLeftAreaPanel
			// 
			this.FTAApplicationLeftAreaPanel.Controls.Add(this.CertificateOfOriginIssueGroupBox);
			this.FTAApplicationLeftAreaPanel.Controls.Add(this.CertificateOfOriginGroupBox);
			this.FTAApplicationLeftAreaPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.FTAApplicationLeftAreaPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FTAApplicationLeftAreaPanel.Name = "FTAApplicationLeftAreaPanel";
			this.FTAApplicationLeftAreaPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 311, true);
			this.FTAApplicationLeftAreaPanel.TabIndex = 10;
			// 
			// CertificateOfOriginIssueGroupBox
			// 
			this.CertificateOfOriginIssueGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("843fc978-d77d-48ee-84d7-a5dff7b09480", "C/O Issuance Details");
			this.CertificateOfOriginIssueGroupBox.Controls.Add(this.CertificateOfOriginIssuePanel);
			this.CertificateOfOriginIssueGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CertificateOfOriginIssueGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 183, true);
			this.CertificateOfOriginIssueGroupBox.Name = "CertificateOfOriginIssueGroupBox";
			this.CertificateOfOriginIssueGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 128, true);
			this.CertificateOfOriginIssueGroupBox.TabIndex = 5;
			this.CertificateOfOriginIssueGroupBox.TabStop = false;
			// 
			// CertificateOfOriginIssuePanel
			// 
			this.CertificateOfOriginIssuePanel.AllowDrop = true;
			this.CertificateOfOriginIssuePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CertificateOfOriginIssuePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.CertificateOfOriginIssuePanel.Name = "CertificateOfOriginIssuePanel";
			this.CertificateOfOriginIssuePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(647, 111, true);
			this.CertificateOfOriginIssuePanel.TabIndex = 0;
			// 
			// CertificateOfOriginGroupBox
			// 
			this.CertificateOfOriginGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("64b3b747-b8db-4681-aa3f-a862bd7770ed", "Certificate of Origin");
			this.CertificateOfOriginGroupBox.Controls.Add(this.CertificateOfOriginPanel);
			this.CertificateOfOriginGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.CertificateOfOriginGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CertificateOfOriginGroupBox.Name = "CertificateOfOriginGroupBox";
			this.CertificateOfOriginGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 183, true);
			this.CertificateOfOriginGroupBox.TabIndex = 4;
			this.CertificateOfOriginGroupBox.TabStop = false;
			// 
			// CertificateOfOriginPanel
			// 
			this.CertificateOfOriginPanel.AllowDrop = true;
			this.CertificateOfOriginPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CertificateOfOriginPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.CertificateOfOriginPanel.Name = "CertificateOfOriginPanel";
			this.CertificateOfOriginPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(647, 166, true);
			this.CertificateOfOriginPanel.TabIndex = 0;
			// 
			// FTAApplicationRightAreaPanel
			// 
			this.FTAApplicationRightAreaPanel.Controls.Add(this.DetailedFTAGroupBox);
			this.FTAApplicationRightAreaPanel.Controls.Add(this.FTADetailsGroupBox);
			this.FTAApplicationRightAreaPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FTAApplicationRightAreaPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(651, 0, true);
			this.FTAApplicationRightAreaPanel.Name = "FTAApplicationRightAreaPanel";
			this.FTAApplicationRightAreaPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 311, true);
			this.FTAApplicationRightAreaPanel.TabIndex = 11;
			// 
			// DetailedFTAGroupBox
			// 
			this.DetailedFTAGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("3702fa06-ce06-4498-9d8c-73d07dc278ff", "Detailed FTA (DHR)");
			this.DetailedFTAGroupBox.Controls.Add(this.DetailedFTAPanel);
			this.DetailedFTAGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailedFTAGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 249, true);
			this.DetailedFTAGroupBox.Name = "DetailedFTAGroupBox";
			this.DetailedFTAGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 62, true);
			this.DetailedFTAGroupBox.TabIndex = 5;
			this.DetailedFTAGroupBox.TabStop = false;
			// 
			// DetailedFTAPanel
			// 
			this.DetailedFTAPanel.AllowDrop = true;
			this.DetailedFTAPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailedFTAPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.DetailedFTAPanel.Name = "DetailedFTAPanel";
			this.DetailedFTAPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 45, true);
			this.DetailedFTAPanel.TabIndex = 0;
			// 
			// FTADetailsGroupBox
			// 
			this.FTADetailsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("af55451e-12c6-4a82-8333-379b67b962c9", "FTA Details");
			this.FTADetailsGroupBox.Controls.Add(this.FTADetailsPanel);
			this.FTADetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.FTADetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FTADetailsGroupBox.Name = "FTADetailsGroupBox";
			this.FTADetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 249, true);
			this.FTADetailsGroupBox.TabIndex = 4;
			this.FTADetailsGroupBox.TabStop = false;
			// 
			// FTADetailsPanel
			// 
			this.FTADetailsPanel.AllowDrop = true;
			this.FTADetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FTADetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.FTADetailsPanel.Name = "FTADetailsPanel";
			this.FTADetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 232, true);
			this.FTADetailsPanel.TabIndex = 0;
			// 
			// COOandFTAUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FTAApplicationRightAreaPanel);
			this.Controls.Add(this.FTAApplicationLeftAreaPanel);
			this.Name = "COOandFTAUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1049, 311, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FTAApplicationLeftAreaPanel.ResumeLayout(false);
			this.FTAApplicationLeftAreaPanel.PerformLayout();
			this.CertificateOfOriginIssueGroupBox.ResumeLayout(false);
			this.CertificateOfOriginIssueGroupBox.PerformLayout();
			this.CertificateOfOriginGroupBox.ResumeLayout(false);
			this.CertificateOfOriginGroupBox.PerformLayout();
			this.FTAApplicationRightAreaPanel.ResumeLayout(false);
			this.FTAApplicationRightAreaPanel.PerformLayout();
			this.DetailedFTAGroupBox.ResumeLayout(false);
			this.DetailedFTAGroupBox.PerformLayout();
			this.FTADetailsGroupBox.ResumeLayout(false);
			this.FTADetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZPanel FTAApplicationLeftAreaPanel;
		private ZGroupBox CertificateOfOriginIssueGroupBox;
		private DynamicLayoutPanel CertificateOfOriginIssuePanel;
		private ZGroupBox CertificateOfOriginGroupBox;
		private DynamicLayoutPanel CertificateOfOriginPanel;
		private ZPanel FTAApplicationRightAreaPanel;
		private ZGroupBox DetailedFTAGroupBox;
		private DynamicLayoutPanel DetailedFTAPanel;
		private ZGroupBox FTADetailsGroupBox;
		private DynamicLayoutPanel FTADetailsPanel;
	}
}
