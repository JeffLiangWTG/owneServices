namespace Enterprise.Customs.CA.GUI
{
	partial class OrganisationDetailPlugInUserControl
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
			this.components = new System.ComponentModel.Container();
			this.DetailTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.organisationDetailUserControl1 = new Enterprise.Customs.CA.GUI.OrganisationDetailsUserControl();
			this.B3SendingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.organisationB3SendingUserControl1 = new Enterprise.Customs.CA.GUI.OrganisationB3SendingUserControl();
			this.LVSConsolidationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.organisationLVSConsolidationUserControl1 = new Enterprise.Customs.CA.GUI.OrganisationLVSConsolidationUserControl();
			this.CSATabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.organisationCSAUserControl1 = new Enterprise.Customs.CA.GUI.OrganisationCSAUserControl();
			this.AuditActionsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.organisationAuditActionsUserControl1 = new Enterprise.Customs.CA.GUI.OrganisationAuditActionsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.organisationDetailUserControl1.SuspendLayout();
			this.B3SendingTabPage.SuspendLayout();
			this.organisationB3SendingUserControl1.SuspendLayout();
			this.LVSConsolidationTabPage.SuspendLayout();
			this.organisationLVSConsolidationUserControl1.SuspendLayout();
			this.CSATabPage.SuspendLayout();
			this.organisationCSAUserControl1.SuspendLayout();
			this.AuditActionsTabPage.SuspendLayout();
			this.organisationAuditActionsUserControl1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.OrgImpAddInfo);
			// 
			// DetailTabControl
			// 
			this.DetailTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.DetailTabControl.Controls.Add(this.DetailsTabPage);
			this.DetailTabControl.Controls.Add(this.B3SendingTabPage);
			this.DetailTabControl.Controls.Add(this.LVSConsolidationTabPage);
			this.DetailTabControl.Controls.Add(this.CSATabPage);
			this.DetailTabControl.Controls.Add(this.AuditActionsTabPage);
			this.DetailTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailTabControl.Name = "DetailTabControl";
			this.DetailTabControl.SelectedIndex = 0;
			this.DetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(722, 409, true);
			this.DetailTabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8ccc28c2-f5b9-487e-b431-07ad063a1765", "CA Details");
			this.DetailsTabPage.Controls.Add(this.organisationDetailUserControl1);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(714, 382, true);
			this.DetailsTabPage.TabIndex = 0;
			this.DetailsTabPage.UseVisualStyleBackColor = true;
			// 
			// organisationDetailUserControl1
			// 
			this.organisationDetailUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.organisationDetailUserControl1, ".");
			this.organisationDetailUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.organisationDetailUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.organisationDetailUserControl1.Name = "organisationDetailUserControl1";
			this.organisationDetailUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 67, true);
			this.organisationDetailUserControl1.TabIndex = 0;
			// 
			// B3SendingTabPage
			// 
			this.B3SendingTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b175bbcb-402e-4835-91ae-a3408fa1c2b3", "Accounting Declaration");
			this.B3SendingTabPage.Controls.Add(this.organisationB3SendingUserControl1);
			this.B3SendingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.B3SendingTabPage.Name = "B3SendingTabPage";
			this.B3SendingTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.B3SendingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(714, 382, true);
			this.B3SendingTabPage.TabIndex = 1;
			this.B3SendingTabPage.UseVisualStyleBackColor = true;
			// 
			// organisationB3SendingUserControl1
			// 
			this.organisationB3SendingUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.organisationB3SendingUserControl1, ".");
			this.organisationB3SendingUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.organisationB3SendingUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.organisationB3SendingUserControl1.Name = "organisationB3SendingUserControl1";
			this.organisationB3SendingUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 67, true);
			this.organisationB3SendingUserControl1.TabIndex = 0;
			// 
			// LVSConsolidationTabPage
			// 
			this.LVSConsolidationTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c65a1096-fe51-49eb-b054-57e761400144", "LVS");
			this.LVSConsolidationTabPage.Controls.Add(this.organisationLVSConsolidationUserControl1);
			this.LVSConsolidationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LVSConsolidationTabPage.Name = "LVSConsolidationTabPage";
			this.LVSConsolidationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(714, 382, true);
			this.LVSConsolidationTabPage.TabIndex = 2;
			// 
			// organisationLVSConsolidationUserControl1
			// 
			this.organisationLVSConsolidationUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.organisationLVSConsolidationUserControl1, ".");
			this.organisationLVSConsolidationUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.organisationLVSConsolidationUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.organisationLVSConsolidationUserControl1.Name = "organisationLVSConsolidationUserControl1";
			this.organisationLVSConsolidationUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(714, 382, true);
			this.organisationLVSConsolidationUserControl1.TabIndex = 0;
			// 
			// CSATabPage
			// 
			this.CSATabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a1ce787a-04cc-4315-a42a-8b191d72f66b", "CSA");
			this.CSATabPage.Controls.Add(this.organisationCSAUserControl1);
			this.CSATabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CSATabPage.Name = "CSATabPage";
			this.CSATabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(714, 382, true);
			this.CSATabPage.TabIndex = 3;
			// 
			// organisationCSAUserControl1
			// 
			this.organisationCSAUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.organisationCSAUserControl1, ".");
			this.organisationCSAUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.organisationCSAUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.organisationCSAUserControl1.Name = "organisationCSAUserControl1";
			this.organisationCSAUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(714, 382, true);
			this.organisationCSAUserControl1.TabIndex = 0;
			// 
			// AuditActionsTabPage
			// 
			this.AuditActionsTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c367c210-0e76-4278-9a23-3e77f108ff34", "Audit Actions");
			this.AuditActionsTabPage.Controls.Add(this.organisationAuditActionsUserControl1);
			this.AuditActionsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AuditActionsTabPage.Name = "AuditActionsTabPage";
			this.AuditActionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(714, 382, true);
			this.AuditActionsTabPage.TabIndex = 3;
			// 
			// organisationAuditActionsUserControl1
			// 
			this.organisationAuditActionsUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.organisationAuditActionsUserControl1, ".");
			this.organisationAuditActionsUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.organisationAuditActionsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.organisationAuditActionsUserControl1.Name = "organisationAuditActionsUserControl1";
			this.organisationAuditActionsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(714, 382, true);
			this.organisationAuditActionsUserControl1.TabIndex = 0;
			// 
			// OrganisationDetailPlugInUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DetailTabControl);
			this.Name = "OrganisationDetailPlugInUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(722, 409, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailTabControl.ResumeLayout(false);
			this.DetailTabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.organisationDetailUserControl1.ResumeLayout(true);
			this.organisationDetailUserControl1.PerformLayout();
			this.B3SendingTabPage.ResumeLayout(false);
			this.B3SendingTabPage.PerformLayout();
			this.organisationB3SendingUserControl1.ResumeLayout(true);
			this.organisationB3SendingUserControl1.PerformLayout();
			this.LVSConsolidationTabPage.ResumeLayout(false);
			this.LVSConsolidationTabPage.PerformLayout();
			this.organisationLVSConsolidationUserControl1.ResumeLayout(true);
			this.organisationLVSConsolidationUserControl1.PerformLayout();
			this.CSATabPage.ResumeLayout(false);
			this.CSATabPage.PerformLayout();
			this.organisationCSAUserControl1.ResumeLayout(true);
			this.organisationCSAUserControl1.PerformLayout();
			this.AuditActionsTabPage.ResumeLayout(false);
			this.AuditActionsTabPage.PerformLayout();
			this.organisationAuditActionsUserControl1.ResumeLayout(true);
			this.organisationAuditActionsUserControl1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTabControl DetailTabControl;
		private ZArchitecture.GUI.ZTabPage DetailsTabPage;
		private ZArchitecture.GUI.ZTabPage B3SendingTabPage;
		private ZArchitecture.GUI.ZTabPage LVSConsolidationTabPage;
		private OrganisationDetailsUserControl organisationDetailUserControl1;
		private OrganisationB3SendingUserControl organisationB3SendingUserControl1;
		private ZArchitecture.GUI.ZTabPage CSATabPage;
		private OrganisationCSAUserControl organisationCSAUserControl1;
		private ZArchitecture.GUI.ZTabPage AuditActionsTabPage;
		private OrganisationAuditActionsUserControl organisationAuditActionsUserControl1;
		private OrganisationLVSConsolidationUserControl organisationLVSConsolidationUserControl1;
	}
}
