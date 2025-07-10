namespace Enterprise.Customs.KR.GUI
{
	partial class ValuationDeclarationTemplateUserControl
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
			this.SupplierOrganisationControl = new Enterprise.Customs.GUI.ZOrganisationControlWithMiscellaneous();
			this.ImporterOrganisationControl = new Enterprise.Customs.GUI.ZOrganisationControlWithMiscellaneous();
			this.LeftPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.PayerGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PayerOrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.EntryDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.entryDetailPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.ValuationMethodCUserControl = new Enterprise.Customs.KR.GUI.ValuationMethodCUserControl();
			this.ValuationMethodDUserControl = new Enterprise.Customs.KR.GUI.ValuationMethodDUserControl();
			this.AuthorGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AuthorPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.ResponsiblePersonGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ResponsiblePersonPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SupplierOrganisationControl.SuspendLayout();
			this.ImporterOrganisationControl.SuspendLayout();
			this.LeftPanel.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.PayerGroupBox.SuspendLayout();
			this.PayerOrganisationFindBox.SuspendLayout();
			this.EntryDetailsGroupBox.SuspendLayout();
			this.ValuationMethodCUserControl.SuspendLayout();
			this.ValuationMethodDUserControl.SuspendLayout();
			this.AuthorGroupBox.SuspendLayout();
			this.ResponsiblePersonGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
			// 
			// SupplierOrganisationControl
			// 
			this.SupplierOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplierOrganisationControl, "JE_OH_Supplier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_OH_Supplier)));
			this.SupplierOrganisationControl.BindToMiscellaneousFields = "JE_SupplierMiscFields";
			this.SupplierOrganisationControl.Captions = new string[0];
			this.SupplierOrganisationControl.IsCaptionOverridden = false;
			this.SupplierOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SupplierOrganisationControl.Name = "SupplierOrganisationControl";
			this.SupplierOrganisationControl.OrgAddressFormatter = null;
			this.SupplierOrganisationControl.PopupCaption = "";
			this.SupplierOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 151, true);
			this.SupplierOrganisationControl.TabIndex = 0;
			// 
			// ImporterOrganisationControl
			// 
			this.ImporterOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterOrganisationControl, "JE_OH_Importer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_OH_Importer)));
			this.ImporterOrganisationControl.BindToMiscellaneousFields = "JE_ImporterMiscFields";
			this.ImporterOrganisationControl.Captions = new string[0];
			this.ImporterOrganisationControl.IsCaptionOverridden = false;
			this.ImporterOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 161, true);
			this.ImporterOrganisationControl.Name = "ImporterOrganisationControl";
			this.ImporterOrganisationControl.OrgAddressFormatter = null;
			this.ImporterOrganisationControl.PopupCaption = "";
			this.ImporterOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 151, true);
			this.ImporterOrganisationControl.TabIndex = 1;
			// 
			// LeftPanel
			// 
			this.LeftPanel.Controls.Add(this.DetailsGroupBox);
			this.LeftPanel.Controls.Add(this.PayerGroupBox);
			this.LeftPanel.Controls.Add(this.SupplierOrganisationControl);
			this.LeftPanel.Controls.Add(this.ImporterOrganisationControl);
			this.LeftPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.LeftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LeftPanel.Name = "LeftPanel";
			this.LeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 810, true);
			this.LeftPanel.TabIndex = 2;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("5099aa36-9ca9-41c4-9036-f186d875f0de", "Details");
			this.DetailsGroupBox.Controls.Add(this.DetailsPanel);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 376, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 210, true);
			this.DetailsGroupBox.TabIndex = 4;
			this.DetailsGroupBox.TabStop = false;
			// 
			// DetailsPanel
			// 
			this.DetailsPanel.AllowDrop = true;
			this.DetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.DetailsPanel.Name = "DetailsPanel";
			this.DetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 193, true);
			this.DetailsPanel.TabIndex = 3;
			// 
			// PayerGroupBox
			// 
			this.PayerGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("FF44F3EB-F90B-47F0-929A-8887C39EA2AA", "Payer");
			this.PayerGroupBox.Controls.Add(this.PayerOrganisationFindBox);
			this.PayerGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 319, true);
			this.PayerGroupBox.Name = "PayerGroupBox";
			this.PayerGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 51, true);
			this.PayerGroupBox.TabIndex = 3;
			this.PayerGroupBox.TabStop = false;
			// 
			// PayerOrganisationFindBox
			// 
			this.PayerOrganisationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PayerOrganisationFindBox, "JE_OH_DutyPayer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_OH_DutyPayer)));
			this.PayerOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.PayerOrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.PayerOrganisationFindBox.Name = "PayerOrganisationFindBox";
			this.PayerOrganisationFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PayerOrganisationFindBox.ParentType = null;
			this.PayerOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 17, true);
			this.PayerOrganisationFindBox.TabIndex = 6;
			// 
			// EntryDetailsGroupBox
			// 
			this.EntryDetailsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("598a5449-c4b4-4b2d-a372-16282a0920a8", "Entry Details");
			this.EntryDetailsGroupBox.Controls.Add(this.entryDetailPanel);
			this.EntryDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.EntryDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(259, 0, true);
			this.EntryDetailsGroupBox.Name = "EntryDetailsGroupBox";
			this.EntryDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(934, 100, true);
			this.EntryDetailsGroupBox.TabIndex = 6;
			this.EntryDetailsGroupBox.TabStop = false;
			// 
			// entryDetailPanel
			// 
			this.entryDetailPanel.AllowDrop = true;
			this.entryDetailPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.entryDetailPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.entryDetailPanel.Name = "entryDetailPanel";
			this.entryDetailPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(930, 83, true);
			this.entryDetailPanel.TabIndex = 0;
			// 
			// ValuationMethodCUserControl
			// 
			this.ValuationMethodCUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValuationMethodCUserControl, ".");
			this.ValuationMethodCUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValuationMethodCUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(259, 196, true);
			this.ValuationMethodCUserControl.Name = "ValuationMethodCUserControl";
			this.ValuationMethodCUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(934, 614, true);
			this.ValuationMethodCUserControl.TabIndex = 9;
			// 
			// ValuationMethodDUserControl
			// 
			this.ValuationMethodDUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValuationMethodDUserControl, ".");
			this.ValuationMethodDUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValuationMethodDUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(259, 196, true);
			this.ValuationMethodDUserControl.Name = "ValuationMethodDUserControl";
			this.ValuationMethodDUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(934, 614, true);
			this.ValuationMethodDUserControl.TabIndex = 10;
			// 
			// AuthorGroupBox
			// 
			this.AuthorGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("02863278-8a06-4c11-affe-7a8993f8ec9e", "Author");
			this.AuthorGroupBox.Controls.Add(this.AuthorPanel);
			this.AuthorGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.AuthorGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(259, 100, true);
			this.AuthorGroupBox.Name = "AuthorGroupBox";
			this.AuthorGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(934, 48, true);
			this.AuthorGroupBox.TabIndex = 7;
			this.AuthorGroupBox.TabStop = false;
			// 
			// AuthorPanel
			// 
			this.AuthorPanel.AllowDrop = true;
			this.AuthorPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AuthorPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.AuthorPanel.Name = "AuthorPanel";
			this.AuthorPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(930, 31, true);
			this.AuthorPanel.TabIndex = 0;
			// 
			// ResponsiblePersonGroupBox
			// 
			this.ResponsiblePersonGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("2565fc70-2b0d-44f4-935a-a09e4394751f", "Responsible Person");
			this.ResponsiblePersonGroupBox.Controls.Add(this.ResponsiblePersonPanel);
			this.ResponsiblePersonGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ResponsiblePersonGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(259, 148, true);
			this.ResponsiblePersonGroupBox.Name = "ResponsiblePersonGroupBox";
			this.ResponsiblePersonGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(934, 48, true);
			this.ResponsiblePersonGroupBox.TabIndex = 8;
			this.ResponsiblePersonGroupBox.TabStop = false;
			// 
			// ResponsiblePersonPanel
			// 
			this.ResponsiblePersonPanel.AllowDrop = true;
			this.ResponsiblePersonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ResponsiblePersonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ResponsiblePersonPanel.Name = "ResponsiblePersonPanel";
			this.ResponsiblePersonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(930, 31, true);
			this.ResponsiblePersonPanel.TabIndex = 0;
			// 
			// ValuationDeclarationTemplateUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ValuationMethodDUserControl);
			this.Controls.Add(this.ValuationMethodCUserControl);
			this.Controls.Add(this.ResponsiblePersonGroupBox);
			this.Controls.Add(this.AuthorGroupBox);
			this.Controls.Add(this.EntryDetailsGroupBox);
			this.Controls.Add(this.LeftPanel);
			this.Name = "ValuationDeclarationTemplateUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1193, 810, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupplierOrganisationControl.ResumeLayout(true);
			this.SupplierOrganisationControl.PerformLayout();
			this.ImporterOrganisationControl.ResumeLayout(true);
			this.ImporterOrganisationControl.PerformLayout();
			this.LeftPanel.ResumeLayout(false);
			this.LeftPanel.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.PayerGroupBox.ResumeLayout(false);
			this.PayerGroupBox.PerformLayout();
			this.PayerOrganisationFindBox.ResumeLayout(true);
			this.PayerOrganisationFindBox.PerformLayout();
			this.EntryDetailsGroupBox.ResumeLayout(false);
			this.EntryDetailsGroupBox.PerformLayout();
			this.ValuationMethodCUserControl.ResumeLayout(true);
			this.ValuationMethodCUserControl.PerformLayout();
			this.ValuationMethodDUserControl.ResumeLayout(true);
			this.ValuationMethodDUserControl.PerformLayout();
			this.AuthorGroupBox.ResumeLayout(false);
			this.AuthorGroupBox.PerformLayout();
			this.ResponsiblePersonGroupBox.ResumeLayout(false);
			this.ResponsiblePersonGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZTabControl zTabControl2;
		private ZArchitecture.GUI.ZTabPage zTabPage1;
		private ZArchitecture.GUI.ZTabPage zTabPage2;
		private ZArchitecture.GUI.ZTabControl zTabControl3;
		private Customs.GUI.ZOrganisationControlWithMiscellaneous SupplierOrganisationControl;
		private Customs.GUI.ZOrganisationControlWithMiscellaneous ImporterOrganisationControl;
		private ZArchitecture.GUI.ZPanel LeftPanel;
		private ZArchitecture.GUI.ZGroupBox PayerGroupBox;
		private Enterprise.MasterFiles.GUI.ZOrganisationFindBox PayerOrganisationFindBox;
		private ZArchitecture.GUI.DynamicLayoutPanel DetailsPanel;
		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private ZArchitecture.GUI.ZGroupBox EntryDetailsGroupBox;
		private ZArchitecture.GUI.DynamicLayoutPanel entryDetailPanel;
		private ValuationMethodCUserControl ValuationMethodCUserControl;
		private ValuationMethodDUserControl ValuationMethodDUserControl;
		private ZArchitecture.GUI.ZGroupBox AuthorGroupBox;
		private ZArchitecture.GUI.ZGroupBox ResponsiblePersonGroupBox;
		private ZArchitecture.GUI.DynamicLayoutPanel AuthorPanel;
		private ZArchitecture.GUI.DynamicLayoutPanel ResponsiblePersonPanel;
	}
}
