using System;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLJournals
{
	partial class GLJournalApprovalBulkForm
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
            if (eDocPlugIn != null)
            {
                eDocPlugIn.Dispose();
            }
            base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.JournalTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.JournalTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.EDocsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            ((System.ComponentModel.ISupportInitialize)(this.TopGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DetailsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// TopGrid
			// 
			this.BindingSource.SetBindingMember(this.TopGrid, "Approvals");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.GLJournalApprovalBulk)(null)).Approvals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.GeneralLedger.GLJournals.GLJournalApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.GLJournalApprovalBulk)(null)).Approvals)).SyncRoot)).XP_RequestID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.GeneralLedger.GLJournals.GLJournalApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.GLJournalApprovalBulk)(null)).Approvals)).SyncRoot)).JournalNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.GeneralLedger.GLJournals.GLJournalApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.GLJournalApprovalBulk)(null)).Approvals)).SyncRoot)).XP_GB_RequestingBranch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.GeneralLedger.GLJournals.GLJournalApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.GLJournalApprovalBulk)(null)).Approvals)).SyncRoot)).XP_ApprovalStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.GeneralLedger.GLJournals.GLJournalApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.GLJournalApprovalBulk)(null)).Approvals)).SyncRoot)).XP_GS_NKApprovingUser1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.GeneralLedger.GLJournals.GLJournalApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.GLJournalApprovalBulk)(null)).Approvals)).SyncRoot)).XP_ApprovalDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.GeneralLedger.GLJournals.GLJournalApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.GLJournalApprovalBulk)(null)).Approvals)).SyncRoot)).XP_ReasonDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.GeneralLedger.GLJournals.GLJournalApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.GLJournalApprovalBulk)(null)).Approvals)).SyncRoot)).XP_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.GeneralLedger.GLJournals.GLJournalApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.GLJournalApprovalBulk)(null)).Approvals)).SyncRoot)).XP_SystemCreateTimeUtc)));
			zTextBoxColumnStyleInfo1.ColumnName = "XP_RequestID";
			zTextBoxColumnStyleInfo2.ColumnName = "JournalNumber";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "XP_GB_RequestingBranch";
			zGuidFindBoxColumnStyleInfo1.Width = 50;
			zTextBoxColumnStyleInfo3.ColumnName = "XP_ApprovalStatus";
			zTextBoxColumnStyleInfo3.Width = 50;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "XP_GS_NKApprovingUser1";
			zCodeFindBoxColumnStyleInfo1.Width = 85;
			zDateEditColumnStyleInfo1.ColumnName = "XP_ApprovalDate";
			zTextBoxColumnStyleInfo4.ColumnName = "XP_ReasonDescription";
			zTextBoxColumnStyleInfo4.Width = 120;
			zTextBoxColumnStyleInfo5.ColumnName = "XP_SystemCreateUser";
			zTextBoxColumnStyleInfo5.Width = 70;
			zDateEditColumnStyleInfo2.ColumnName = "XP_SystemCreateTimeUtc";
			this.TopGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TopGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TopGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.TopGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.TopGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.TopGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.TopGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.TopGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.TopGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.TopGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 93, true);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.JournalTabControl);
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 201, true);
			// 
			// DetailsGrid
			// 
			this.DetailsGrid.Dock = System.Windows.Forms.DockStyle.None;
			this.DetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 31, true);
			this.DetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 34, true);
			this.DetailsGrid.TabIndex = 100;
			this.DetailsGrid.Visible = false;
			// 
			// DetailsTopPanel
			// 
			this.DetailsTopPanel.Dock = System.Windows.Forms.DockStyle.None;
			this.DetailsTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 36, true);
			this.DetailsTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 29, true);
			this.DetailsTopPanel.TabIndex = 101;
			this.DetailsTopPanel.Visible = false;
			// 
			// CreatedUserCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.CreatedUserCodeFindBox, "Approvals.XP_SystemCreateUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.GeneralLedger.GLJournals.GLJournalApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.GLJournalApprovalBulk)(null)).Approvals)).SyncRoot)).XP_SystemCreateUser)));
			// 
			// ApprovingUserCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.ApprovingUserCodeFindBox, "Approvals.XP_GS_NKApprovingUser1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.GeneralLedger.GLJournals.GLJournalApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.GLJournalApprovalBulk)(null)).Approvals)).SyncRoot)).XP_GS_NKApprovingUser1)));
			// 
			// CreatedTimeDateEdit
			// 
			this.BindingSource.SetBindingMember(this.CreatedTimeDateEdit, "Approvals.XP_SystemCreateTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.GeneralLedger.GLJournals.GLJournalApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.GLJournalApprovalBulk)(null)).Approvals)).SyncRoot)).XP_SystemCreateTimeUtc)));
			// 
			// ApprovalDateEdit
			// 
			this.BindingSource.SetBindingMember(this.ApprovalDateEdit, "Approvals.XP_ApprovalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.GeneralLedger.GLJournals.GLJournalApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.GLJournalApprovalBulk)(null)).Approvals)).SyncRoot)).XP_ApprovalDate)));
			// 
			// RequestingBranchGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.RequestingBranchGuidFindBox, "Approvals.XP_GB_RequestingBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.GeneralLedger.GLJournals.GLJournalApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.GLJournalApprovalBulk)(null)).Approvals)).SyncRoot)).XP_GB_RequestingBranch)));
			// 
			// JobNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.JobNumberTextBox, "Approvals.XP_RequestID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.GeneralLedger.GLJournals.GLJournalApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.GLJournalApprovalBulk)(null)).Approvals)).SyncRoot)).XP_RequestID)));
			// 
			// ReasonDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReasonDescriptionTextBox, "Approvals.XP_ReasonDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.GeneralLedger.GLJournals.GLJournalApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.GLJournalApprovalBulk)(null)).Approvals)).SyncRoot)).XP_ReasonDescription)));
			// 
			// ApprovalStatusDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ApprovalStatusDropEdit, "Approvals.XP_ApprovalStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.GeneralLedger.GLJournals.GLJournalApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.GLJournalApprovalBulk)(null)).Approvals)).SyncRoot)).XP_ApprovalStatus)));
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 454, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.TransactionApproval.GLJournalApprovalBulk);
			// 
			// JournalTabControl
			// 
			this.JournalTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.JournalTabControl.Controls.Add(this.JournalTabPage);
            this.JournalTabControl.Controls.Add(this.EDocsTabPage);
            this.JournalTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JournalTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.JournalTabControl.Name = "JournalTabControl";
			this.JournalTabControl.SelectedIndex = 0;
			this.JournalTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 182, true);
			this.JournalTabControl.TabIndex = 2;
			// 
			// JournalTabPage
			// 
			this.JournalTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("697125a0-b0b2-41df-a663-c625274ccca4", "Journal");
			this.JournalTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.JournalTabPage.Name = "JournalTabPage";
			this.JournalTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.JournalTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(851, 155, true);
			this.JournalTabPage.TabIndex = 0;
			this.JournalTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.JournalTabPage_InitializeTab));
            // 
            // EDocsTabPage
            // 
            this.EDocsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("64ff112c-15d9-44c7-969d-e2f0774f7527", "eDocs");
            this.EDocsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.EDocsTabPage.Name = "EDocsTabPage";
            this.EDocsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.EDocsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(855, 141, true);
            this.EDocsTabPage.TabIndex = 1;
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Accounting.Business.GeneralLedger.GLJournals.GLJournal)(((Enterprise.Accounting.Business.GeneralLedger.GLJournals.GLJournalApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.GLJournalApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.Journal)));
			// 
			// GLJournalApprovalBulkForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 478, true);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.TransactionApproval.GLJournalApprovalBulk);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(740, 480, true);
			this.Name = "GLJournalApprovalBulkForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "GLJournalApprovalBulkForm";
			((System.ComponentModel.ISupportInitialize)(this.TopGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DetailsGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		void JournalTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.glJournalUserControl = new Enterprise.Accounting.GUI.GeneralLedger.GLJournals.GLJournalUserControl();
			this.JournalTabPage.SuspendLayout();
			this.JournalTabPage.Controls.Add(this.glJournalUserControl);
			// 
			// glJournalUserControl
			// 
			this.glJournalUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.glJournalUserControl, "Approvals.PostingDetails.Journal");
			this.glJournalUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glJournalUserControl.ShowApprovalRequestControls = false;
			this.glJournalUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.glJournalUserControl.Name = "glJournalUserControl";
			this.glJournalUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(845, 149, true);
			this.glJournalUserControl.TabIndex = 0;
			this.JournalTabPage.ResumeLayout(true);
		}

		#endregion

		private ZArchitecture.GUI.ZTabControl JournalTabControl;
		private ZArchitecture.GUI.ZTabPage JournalTabPage;
		private GLJournalUserControl glJournalUserControl;
        private ZArchitecture.GUI.ZTabPage EDocsTabPage;

    }
}
