namespace Enterprise.Accounting.GUI.GeneralLedger.GLConsolidations
{
	partial class GLConsolidationGroupForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.GroupMembersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.ParentGroupGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GroupMembersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.GroupDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ActionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CreateEliminationJournalButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GenerateExportFilesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EventTabPage = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GroupMembersGrid)).BeginInit();
			this.GroupMembersGroupBox.SuspendLayout();
			this.TabControl.SuspendLayout();
			this.GroupDetailsTabPage.SuspendLayout();
			this.ActionsGroupBox.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 565, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.GeneralLedger.GLConsolidations.AccConsolidationGroup);
			// 
			// GroupMembersGrid
			// 
			this.GroupMembersGrid.AllowNavigation = false;
			this.GroupMembersGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.GroupMembersGrid, "GroupMembers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.GeneralLedger.GLConsolidations.AccConsolidationGroup)(null)).GroupMembers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.GeneralLedger.GLConsolidations.AccConsolidationMember)(((System.Collections.IList)(((Enterprise.Accounting.Business.GeneralLedger.GLConsolidations.AccConsolidationGroup)(null)).GroupMembers)).SyncRoot)).YM_OH_Organisation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.GeneralLedger.GLConsolidations.AccConsolidationMember)(((System.Collections.IList)(((Enterprise.Accounting.Business.GeneralLedger.GLConsolidations.AccConsolidationGroup)(null)).GroupMembers)).SyncRoot)).YM_GC_Company)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.GeneralLedger.GLConsolidations.AccConsolidationMember)(((System.Collections.IList)(((Enterprise.Accounting.Business.GeneralLedger.GLConsolidations.AccConsolidationGroup)(null)).GroupMembers)).SyncRoot)).Description)));
			this.GroupMembersGrid.CaptionVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "YM_OH_Organisation";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "YM_GC_Company";
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			this.GroupMembersGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.GroupMembersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.GroupMembersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.GroupMembersGrid.CopySelectedRowsAllowed = true;
			this.GroupMembersGrid.GridId = "7DB47344-3D23-42B1-833D-03E8CAB42B5E";
			this.GroupMembersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GroupMembersGrid.LayoutKey = "GroupMembersGrid";
			this.GroupMembersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.GroupMembersGrid.Name = "GroupMembersGrid";
			this.GroupMembersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(689, 278, true);
			this.GroupMembersGrid.TabIndex = 0;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(477, 536, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.PostingButtonsUserControl.TabIndex = 1;
			// 
			// ParentGroupGuidFindBox
			// 
			this.ParentGroupGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ParentGroupGuidFindBox, "YR_YR_ConsolidationGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.GeneralLedger.GLConsolidations.AccConsolidationGroup)(null)).YR_YR_ConsolidationGroup)));
			this.ParentGroupGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 76, true);
			this.ParentGroupGuidFindBox.Name = "ParentGroupGuidFindBox";
			this.ParentGroupGuidFindBox.PopupCaption = null;
			this.ParentGroupGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 20, true);
			this.ParentGroupGuidFindBox.TabIndex = 2;
			// 
			// CodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CodeTextBox, "YR_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.GeneralLedger.GLConsolidations.AccConsolidationGroup)(null)).YR_Code)));
			this.CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 24, true);
			this.CodeTextBox.Name = "CodeTextBox";
			this.CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 20, true);
			this.CodeTextBox.TabIndex = 0;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "YR_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.GeneralLedger.GLConsolidations.AccConsolidationGroup)(null)).YR_Description)));
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 50, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 20, true);
			this.DescriptionTextBox.TabIndex = 1;
			// 
			// GroupMembersGroupBox
			// 
			this.GroupMembersGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.GroupMembersGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("990c69b7-7464-4352-ac95-602b22789e1c", "Group Members");
			this.GroupMembersGroupBox.Controls.Add(this.GroupMembersGrid);
			this.GroupMembersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 125, true);
			this.GroupMembersGroupBox.Name = "GroupMembersGroupBox";
			this.GroupMembersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 303, true);
			this.GroupMembersGroupBox.TabIndex = 0;
			this.GroupMembersGroupBox.TabStop = false;
			// 
			// TabControl
			// 
			this.TabControl.Controls.Add(this.GroupDetailsTabPage);
			this.TabControl.Controls.Add(this.EventTabPage);
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 5, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 525, true);
			this.TabControl.TabIndex = 0;
			// 
			// GroupDetailsTabPage
			// 
			this.GroupDetailsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("cc553d9a-2948-407f-aa1e-c504081066cf", "Consolidation Group");
			this.GroupDetailsTabPage.Controls.Add(this.ActionsGroupBox);
			this.GroupDetailsTabPage.Controls.Add(this.zGroupBox1);
			this.GroupDetailsTabPage.Controls.Add(this.GroupMembersGroupBox);
			this.GroupDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GroupDetailsTabPage.Name = "GroupDetailsTabPage";
			this.GroupDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(721, 498, true);
			this.GroupDetailsTabPage.TabIndex = 0;
			// 
			// ActionsGroupBox
			// 
			this.ActionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ActionsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4fda05e5-57b4-4393-81e1-6f3d832caf9f", "Actions");
			this.ActionsGroupBox.Controls.Add(this.CreateEliminationJournalButton);
			this.ActionsGroupBox.Controls.Add(this.GenerateExportFilesButton);
			this.ActionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 434, true);
			this.ActionsGroupBox.Name = "ActionsGroupBox";
			this.ActionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 54, true);
			this.ActionsGroupBox.TabIndex = 2;
			this.ActionsGroupBox.TabStop = false;
			// 
			// CreateEliminationJournalButton
			// 
			this.CreateEliminationJournalButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7f548631-cc83-4109-8e55-d9106cb3cc88", "Generate Elimination Journals");
			this.CreateEliminationJournalButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 19, true);
			this.CreateEliminationJournalButton.Name = "CreateEliminationJournalButton";
			this.CreateEliminationJournalButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 23, true);
			this.CreateEliminationJournalButton.TabIndex = 8;
			this.CreateEliminationJournalButton.UseVisualStyleBackColor = true;
			this.CreateEliminationJournalButton.Click += new System.EventHandler(this.CreateEliminationJournalCreateButton_Click);
			// 
			// GenerateExportFilesButton
			// 
			this.GenerateExportFilesButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3c7bf446-147b-4348-b359-7c4bc94fcafd", "Generate Export File");
			this.GenerateExportFilesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.GenerateExportFilesButton.Name = "GenerateExportFilesButton";
			this.GenerateExportFilesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 23, true);
			this.GenerateExportFilesButton.TabIndex = 6;
			this.GenerateExportFilesButton.UseVisualStyleBackColor = true;
			this.GenerateExportFilesButton.Click += new System.EventHandler(this.GenerateExportFilesButton_Click);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b3ddd329-a7c9-43ec-a855-47bd3b934094", "Group Details");
			this.zGroupBox1.Controls.Add(this.DescriptionTextBox);
			this.zGroupBox1.Controls.Add(this.ParentGroupGuidFindBox);
			this.zGroupBox1.Controls.Add(this.CodeTextBox);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 7, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 112, true);
			this.zGroupBox1.TabIndex = 1;
			this.zGroupBox1.TabStop = false;
			// 
			// EventTabPage
			// 
			this.EventTabPage.ExcludeFromBindingOnSave = true;
			this.EventTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EventTabPage.Name = "EventTabPage";
			this.EventTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(721, 498, true);
			this.EventTabPage.TabIndex = 1;
			// 
			// GLConsolidationGroupForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4e75c8b0-9ba1-44ca-b2ec-54c1271e4754", "Consolidation Group");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 589, true);
			this.Controls.Add(this.TabControl);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.GeneralLedger.GLConsolidations.AccConsolidationGroup);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 627, true);
			this.Name = "GLConsolidationGroupForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.TabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GroupMembersGrid)).EndInit();
			this.GroupMembersGroupBox.ResumeLayout(false);
			this.TabControl.ResumeLayout(false);
			this.GroupDetailsTabPage.ResumeLayout(false);
			this.ActionsGroupBox.ResumeLayout(false);
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		internal ZArchitecture.ZGrid GroupMembersGrid;
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private ZArchitecture.GUI.ZGuidFindBox ParentGroupGuidFindBox;
		private ZArchitecture.ZTextBox CodeTextBox;
		private ZArchitecture.ZTextBox DescriptionTextBox;
		private ZArchitecture.GUI.ZGroupBox GroupMembersGroupBox;
		private ZArchitecture.GUI.ZTemplateTabControl TabControl;
		private ZArchitecture.GUI.ZTabPage GroupDetailsTabPage;
		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.GUI.ZLogsTabPage EventTabPage;
		private ZArchitecture.GUI.ZGroupBox ActionsGroupBox;
		private ZArchitecture.GUI.ZButton GenerateExportFilesButton;

		private ZArchitecture.GUI.ZButton CreateEliminationJournalButton;
	}
}
