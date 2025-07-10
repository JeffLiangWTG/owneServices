using System;
using System.Windows.Forms;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	partial class AlternateGLAccountsForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnhookEvents();
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
		ZDropEdit accountTypeDropEdit;

		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.alternateAccountGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.alternateGLAccountWithAttributeGridControl = new Enterprise.Accounting.GUI.ARAP.Invoicing.AlternateGLAccountWithAttributeGridControl();
			this.singleAlternateGLAccountControl = new Enterprise.Accounting.GUI.ARAP.Invoicing.SingleAlternateGLAccountControl();
			this.cashFlowCategoryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.parentAccountGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.accountTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.unitsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.chartGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.postingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.chartAndAccountTypeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.parentAccountGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.editAlternateAccountTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.detailTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.formTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.formDetailTabPage = new ZTabPage();
			this.formLogTabPage = new ZLogsTabPage();
			this.relatedAlternateAccountsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.alternateAccountGroupBox.SuspendLayout();
			this.alternateGLAccountWithAttributeGridControl.SuspendLayout();
			this.singleAlternateGLAccountControl.SuspendLayout();
			this.cashFlowCategoryDropEdit.SuspendLayout();
			this.parentAccountGuidFindBox.SuspendLayout();
			this.accountTypeDropEdit.SuspendLayout();
			this.unitsDropEdit.SuspendLayout();
			this.chartGuidFindBox.SuspendLayout();
			this.postingButtonsUserControl.SuspendLayout();
			this.chartAndAccountTypeGroupBox.SuspendLayout();
			this.parentAccountGroupBox.SuspendLayout();
			this.editAlternateAccountTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 690, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(742, 24, true);
			this.MainStatusBar.TabIndex = 10;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.AlternateGLAccounts);
			// 
			// alternateAccountGroupBox
			// 
			this.alternateAccountGroupBox.Controls.Add(this.alternateGLAccountWithAttributeGridControl);
			this.alternateAccountGroupBox.Controls.Add(this.singleAlternateGLAccountControl);
			this.alternateAccountGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 204, true);
			this.alternateAccountGroupBox.Name = "alternateAccountGroupBox";
			this.alternateAccountGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(673, 430, true);
			this.alternateAccountGroupBox.TabIndex = 6;
			this.alternateAccountGroupBox.TabStop = false;
			this.alternateAccountGroupBox.Text = Res.GetString("418327E9-2C43-496B-93A8-2C19E6CDC503", "Alternate Account");
			this.alternateAccountGroupBox.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			// 
			// alternateGLAccountWithAttributeGridControl
			// 
			this.alternateGLAccountWithAttributeGridControl.AllowDrop = true;
			this.alternateGLAccountWithAttributeGridControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.alternateGLAccountWithAttributeGridControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Accounting.Business.AlternateGLAccounts)(((((System.Collections.IList)(((Enterprise.Accounting.Business.AlternateGLAccounts)(null)).AlternateGLAccountsWithAttributeSet)).SyncRoot)))));
			this.alternateGLAccountWithAttributeGridControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 15, true);
			this.alternateGLAccountWithAttributeGridControl.Name = "alternateGLAccountWIthAttributeGridControl";
			this.alternateGLAccountWithAttributeGridControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 202, true);
			this.alternateGLAccountWithAttributeGridControl.TabIndex = 1;
			this.alternateGLAccountWithAttributeGridControl.Dock = DockStyle.Fill;
			// 
			// singleAlternateGLAccountControl
			// 
			this.singleAlternateGLAccountControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.singleAlternateGLAccountControl, ".");
			this.singleAlternateGLAccountControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 15, true);
			this.singleAlternateGLAccountControl.Name = "singleAlternateGLAccountControl";
			this.singleAlternateGLAccountControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 237, true);
			this.singleAlternateGLAccountControl.TabIndex = 0;
			this.singleAlternateGLAccountControl.Dock = DockStyle.Fill;
			// 
			// cashFlowCategoryDropEdit
			// 
			this.cashFlowCategoryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cashFlowCategoryDropEdit, "CashFlowCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.AlternateGLAccounts)(null)).CashFlowCategory)));
			this.cashFlowCategoryDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("46c4ff8f-bad0-48fe-957a-dd95f8e0d87a", "Cash Flow Cat.", "Cash Flow Category");
			this.cashFlowCategoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 46, true);
			this.cashFlowCategoryDropEdit.Name = "cashFlowCategoryDropEdit";
			this.cashFlowCategoryDropEdit.PreBoundMaxLength = 3;
			this.cashFlowCategoryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 15, true);
			this.cashFlowCategoryDropEdit.TabIndex = 0;
			this.cashFlowCategoryDropEdit.TabStop = false;
			this.cashFlowCategoryDropEdit.ReadOnly = true;
			// 
			// parentAccountGuidFindBox
			// 
			this.parentAccountGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.parentAccountGuidFindBox, "ParentGLAccountPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.AlternateGLAccounts)(null)).ParentGLAccountPK)));
			this.parentAccountGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b66700d4-7cfa-40d2-8049-4c0dc39ec423", "Account Number");
			this.parentAccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 18, true);
			this.parentAccountGuidFindBox.Name = "parentAccountGuidFindBox";
			this.parentAccountGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.parentAccountGuidFindBox.ParentType = null;
			this.parentAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 15, true);
			this.parentAccountGuidFindBox.TabIndex = 5;
			// 
			// accountTypeDropEdit
			// 
			this.accountTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.accountTypeDropEdit, "AccountType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.AlternateGLAccounts)(null)).AccountType)));
			this.accountTypeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("eb715adf-e540-48f2-b13b-8721cd2f4422", "Account Type");
			this.accountTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 49, true);
			this.accountTypeDropEdit.Name = "accountTypeDropEdit";
			this.accountTypeDropEdit.PreBoundMaxLength = 3;
			this.accountTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 15, true);
			this.accountTypeDropEdit.TabIndex = 3;
			// 
			// unitsDropEdit
			// 
			this.unitsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.unitsDropEdit, "FirstAlternateGLAccountWithAttributeSet+AlternateGLAccount+StatisticalUnits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.AlternateGLAccounts)(null)).FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.StatisticalUnits)));
			this.unitsDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("42e3ff1c-afe0-4b28-a742-bbe811c390bf", "Units");
			this.unitsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 46, true);
			this.unitsDropEdit.Name = "unitsDropEdit";
			this.unitsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 15, true);
			this.unitsDropEdit.TabIndex = 0;
			this.unitsDropEdit.TabStop = false;
			this.unitsDropEdit.ReadOnly = true;
			// 
			// chartGuidFindBox
			// 
			this.chartGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.chartGuidFindBox, "ChartPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.AlternateGLAccounts)(null)).ChartPK)));
			this.chartGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("286c8eaa-92df-4a8a-bb53-689cdec02fa4", "Chart Code");
			this.chartGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 20, true);
			this.chartGuidFindBox.Name = "chartGuidFindBox";
			this.chartGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.chartGuidFindBox.ParentType = null;
			this.chartGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 15, true);
			this.chartGuidFindBox.TabIndex = 2;
			// 
			// postingButtonsUserControl
			// 
			this.postingButtonsUserControl.AllowDrop = true;
			this.postingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.postingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(494, 663, true);
			this.postingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsUserControl.Name = "postingButtonsUserControl";
			this.postingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsUserControl.TabIndex = 20;
			// 
			// chartAndAccountTypeGroupBox
			// 
			this.chartAndAccountTypeGroupBox.Controls.Add(this.chartGuidFindBox);
			this.chartAndAccountTypeGroupBox.Controls.Add(this.accountTypeDropEdit);
			this.chartAndAccountTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 13, true);
			this.chartAndAccountTypeGroupBox.Name = "chartAndAccountTypeGroupBox";
			this.chartAndAccountTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(713, 82, true);
			this.chartAndAccountTypeGroupBox.TabIndex = 1;
			this.chartAndAccountTypeGroupBox.TabStop = false;
			this.chartAndAccountTypeGroupBox.Text = Res.GetString("9DB84C9F-EBCA-4820-9D10-76D37E61A7C4", "Chart and Account Type");
			// 
			// parentAccountGroupBox
			// 
			this.parentAccountGroupBox.Controls.Add(this.cashFlowCategoryDropEdit);
			this.parentAccountGroupBox.Controls.Add(this.parentAccountGuidFindBox);
			this.parentAccountGroupBox.Controls.Add(this.unitsDropEdit);
			this.parentAccountGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 107, true);
			this.parentAccountGroupBox.Name = "parentAccountGroupBox";
			this.parentAccountGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(713, 82, true);
			this.parentAccountGroupBox.TabIndex = 4;
			this.parentAccountGroupBox.TabStop = false;
			this.parentAccountGroupBox.Text = Res.GetString("A4705A78-60B2-4AF2-B233-BFF8E1C4416F", "Parent Account");
			// 
			// formTabControl
			// 
			this.formTabControl.Controls.Add(this.formDetailTabPage);
			this.formTabControl.Controls.Add(this.formLogTabPage);
			this.formTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.formTabControl.Name = "formTabControl";
			this.formTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(726, 650, true);
			this.formTabControl.TabIndex = 0;
			// 
			// formDetailTabPage
			// 
			this.formDetailTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.formDetailTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.formDetailTabPage.Name = "formDetailTabPage";
			this.formDetailTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.formDetailTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 640, true);
			this.formDetailTabPage.TabIndex = 1;
			this.formDetailTabPage.Text = Res.GetString("BFCB5009-113F-4D11-B47F-291E38F8E337", "Details");
			this.formDetailTabPage.Controls.Add(this.editAlternateAccountTabControl);
			this.formDetailTabPage.Controls.Add(this.parentAccountGroupBox);
			this.formDetailTabPage.Controls.Add(this.alternateAccountGroupBox);
			this.formDetailTabPage.Controls.Add(this.chartAndAccountTypeGroupBox);
			// 
			// formLogTabPage
			// 
			this.formLogTabPage.ExcludeFromBindingOnSave = true;
			this.formLogTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.formLogTabPage.Name = "formLogTabPage";
			this.formLogTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.formLogTabPage.ShouldBeReadOnlyInViewMode = false;
			this.formLogTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 640, true);
			this.formLogTabPage.TabIndex = 11;
			// 
			// editAlternateAccountTabControl
			// 
			this.editAlternateAccountTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.editAlternateAccountTabControl.Controls.Add(this.detailTabPage);
			this.editAlternateAccountTabControl.Controls.Add(this.relatedAlternateAccountsTabPage);
			this.editAlternateAccountTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 204, true);
			this.editAlternateAccountTabControl.Name = "editAlternateAccountTabControl";
			this.editAlternateAccountTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 410, true);
			this.editAlternateAccountTabControl.TabIndex = 2;
			// 
			// detailTabPage
			// 
			this.detailTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.detailTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.detailTabPage.Name = "detailTabPage";
			this.detailTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.detailTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 376, true);
			this.detailTabPage.TabIndex = 0;
			this.detailTabPage.Text = Res.GetString("BFCB5009-113F-4D11-B47F-291E38F8E337", "Details");
			this.detailTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.detailTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.AlternateGLAccounts)(null)).FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.AlternateGLAccountAttributes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccAlternateGLAccountAttribute)(((System.Collections.IList)(((Enterprise.Accounting.Business.AlternateGLAccounts)(null)).FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.AlternateGLAccountAttributes)).SyncRoot)).AAA_Attribute)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccAlternateGLAccountAttribute)(((System.Collections.IList)(((Enterprise.Accounting.Business.AlternateGLAccounts)(null)).FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.AlternateGLAccountAttributes)).SyncRoot)).ValueDescription)));
			// 
			// relatedAlternateAccountsTabPage
			// 
			this.relatedAlternateAccountsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.relatedAlternateAccountsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.relatedAlternateAccountsTabPage.Name = "relatedAlternateAccountsTabPage";
			this.relatedAlternateAccountsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.relatedAlternateAccountsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(702, 376, true);
			this.relatedAlternateAccountsTabPage.TabIndex = 1;
			this.relatedAlternateAccountsTabPage.Text = Res.GetString("B04D2787-6625-4C30-AACD-6DBFD153F88A", "Related Alternate Accounts");
			this.relatedAlternateAccountsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.relatedAlternateAccountsTabPage_InitializeTab));
			// 
			// AlternateGLAccountsForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(742, 714, true);
			this.Controls.Add(this.formTabControl);
			this.Controls.Add(this.postingButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.AlternateGLAccounts);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(742, 714, true);
			this.Name = "AlternateGLAccountsForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.formTabControl, 0);
			this.Controls.SetChildIndex(this.postingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.alternateAccountGroupBox.ResumeLayout(false);
			this.alternateAccountGroupBox.PerformLayout();
			this.alternateGLAccountWithAttributeGridControl.ResumeLayout(true);
			this.alternateGLAccountWithAttributeGridControl.PerformLayout();
			this.singleAlternateGLAccountControl.ResumeLayout(true);
			this.singleAlternateGLAccountControl.PerformLayout();
			this.cashFlowCategoryDropEdit.ResumeLayout(true);
			this.cashFlowCategoryDropEdit.PerformLayout();
			this.parentAccountGuidFindBox.ResumeLayout(true);
			this.parentAccountGuidFindBox.PerformLayout();
			this.accountTypeDropEdit.ResumeLayout(true);
			this.accountTypeDropEdit.PerformLayout();
			this.unitsDropEdit.ResumeLayout(true);
			this.unitsDropEdit.PerformLayout();
			this.chartGuidFindBox.ResumeLayout(true);
			this.chartGuidFindBox.PerformLayout();
			this.postingButtonsUserControl.ResumeLayout(true);
			this.postingButtonsUserControl.PerformLayout();
			this.chartAndAccountTypeGroupBox.ResumeLayout(false);
			this.chartAndAccountTypeGroupBox.PerformLayout();
			this.parentAccountGroupBox.ResumeLayout(false);
			this.parentAccountGroupBox.PerformLayout();
			this.editAlternateAccountTabControl.ResumeLayout(false);
			this.editAlternateAccountTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}


		void detailTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			ZTextBoxColumnStyleInfo attributeTypeTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo attributeValueTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			this.singleAlternateGLAccountInTabControl = new SingleAlternateGLAccountControl();
			this.alternateAccountDetailGroupBox = new ZGroupBox();
			this.attributesGroupBox = new ZGroupBox();
			this.alternateAccountAttributeGrid = new ZGrid();
			this.detailTabPage.SuspendLayout();
			this.singleAlternateGLAccountInTabControl.SuspendLayout();
			this.alternateAccountDetailGroupBox.SuspendLayout();
			this.attributesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.alternateAccountAttributeGrid)).BeginInit();
			this.alternateAccountAttributeGrid.SuspendLayout();
			this.alternateAccountDetailGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.detailTabPage.Controls.Add(this.attributesGroupBox);
			this.detailTabPage.Controls.Add(this.alternateAccountDetailGroupBox);
			// 
			// singleAlternateGLAccountInTabControl
			// 
			this.singleAlternateGLAccountInTabControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.singleAlternateGLAccountInTabControl, ".");
			this.singleAlternateGLAccountInTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 21, true);
			this.singleAlternateGLAccountInTabControl.Name = "singleAlternateGLAccountInTabControl";
			this.singleAlternateGLAccountInTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(694, 204, true);
			this.singleAlternateGLAccountInTabControl.TabIndex = 0;
			// 
			// alternateAccountDetailGroupBox
			// 
			this.alternateAccountDetailGroupBox.BackColor = System.Drawing.SystemColors.Control;
			this.alternateAccountDetailGroupBox.Controls.Add(this.singleAlternateGLAccountInTabControl);
			this.alternateAccountDetailGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 4, true);
			this.alternateAccountDetailGroupBox.Name = "alternateAccountDetailGroupBox";
			this.alternateAccountDetailGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(699, 225, true);
			this.alternateAccountDetailGroupBox.TabIndex = 1;
			this.alternateAccountDetailGroupBox.TabStop = false;
			this.alternateAccountDetailGroupBox.Text = Res.GetString("30368041-2700-4F57-84F1-B2CC91453D57", "Alternate Account");
			// 
			// attributesGroupBox
			// 
			this.attributesGroupBox.Anchor = ((AnchorStyles)((AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom)));
			this.attributesGroupBox.Controls.Add(this.alternateAccountAttributeGrid);
			this.attributesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 230, true);
			this.attributesGroupBox.Name = "attributesGroupBox";
			this.attributesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(699, 141, true);
			this.attributesGroupBox.TabIndex = 2;
			this.attributesGroupBox.TabStop = false;
			this.attributesGroupBox.Text = Res.GetString("D29BC5D6-3639-4B8C-9D6C-63DAC0CA5BDD", "Attributes");
			// 
			// alternateAccountAttributeGrid
			// 
			this.alternateAccountAttributeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.alternateAccountAttributeGrid, "FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.AlternateGLAccountAttributes");
			this.alternateAccountAttributeGrid.CaptionVisible = false;
			attributeTypeTextBoxColumnStyleInfo.ColumnName = "AAA_Attribute";
			attributeTypeTextBoxColumnStyleInfo.CaptionResourceString = Res.GetData("6E47BCFF-E430-48BF-84CD-69AD8C82E709", "Attribute Type");
			attributeTypeTextBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			attributeTypeTextBoxColumnStyleInfo.IsReadOnly = true;
			attributeTypeTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			attributeValueTextBoxColumnStyleInfo.ColumnName = "ValueDescription";
			attributeValueTextBoxColumnStyleInfo.CaptionResourceString = Res.GetData("17BCBFCA-FAE3-40DA-AEAF-935FF677FE4F", "Attribute Value");
			attributeValueTextBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			attributeValueTextBoxColumnStyleInfo.IsReadOnly = true;
			attributeValueTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.alternateAccountAttributeGrid.ColumnStyles.Add(attributeTypeTextBoxColumnStyleInfo);
			this.alternateAccountAttributeGrid.ColumnStyles.Add(attributeValueTextBoxColumnStyleInfo);
			this.alternateAccountAttributeGrid.GridId = "3cabc89d-9fc6-4a4e-9749-ed656f71bddd";
			this.alternateAccountAttributeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.alternateAccountAttributeGrid.LayoutKey = "alternateAccountAttributeGrid";
			this.alternateAccountAttributeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(-3, 14, true);
			this.alternateAccountAttributeGrid.Name = "alternateAccountAttributeGrid";
			this.alternateAccountAttributeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(696, 76, true);
			this.alternateAccountAttributeGrid.TabIndex = 0;
			this.detailTabPage.PerformLayout();
			this.singleAlternateGLAccountInTabControl.ResumeLayout(true);
			this.singleAlternateGLAccountInTabControl.PerformLayout();
			this.singleAlternateGLAccountInTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.alternateAccountDetailGroupBox.ResumeLayout(false);
			this.alternateAccountDetailGroupBox.PerformLayout();
			this.attributesGroupBox.ResumeLayout(false);
			this.attributesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.alternateAccountAttributeGrid)).EndInit();
			this.alternateAccountAttributeGrid.ResumeLayout(false);
			this.alternateAccountAttributeGrid.PerformLayout();
			this.alternateAccountAttributeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailTabPage.ResumeLayout(true);
		}

		void relatedAlternateAccountsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.alternateGLAccountWithAttributeGridInTabControl = new AlternateGLAccountWithAttributeGridControl();
			this.alternateGLAccountWithAttributeGridInTabControl.SetReadOnlyIncludingChildren(true);
			this.relatedAlternateAccountsTabPage.SuspendLayout();
			this.alternateGLAccountWithAttributeGridInTabControl.SuspendLayout();
			this.relatedAlternateAccountsTabPage.Controls.Add(this.alternateGLAccountWithAttributeGridInTabControl);
			// 
			// alternateGLAccountWithAttributeGridInTabControl
			// 
			this.alternateGLAccountWithAttributeGridInTabControl.AllowDrop = true;
			this.alternateGLAccountWithAttributeGridInTabControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.alternateGLAccountWithAttributeGridInTabControl, ".");
			this.alternateGLAccountWithAttributeGridInTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.alternateGLAccountWithAttributeGridInTabControl.Name = "alternateGLAccountWithAttributeGridInTabControl";
			this.alternateGLAccountWithAttributeGridInTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 409, true);
			this.alternateGLAccountWithAttributeGridInTabControl.TabIndex = 0;
			this.relatedAlternateAccountsTabPage.PerformLayout();
			this.alternateGLAccountWithAttributeGridInTabControl.ResumeLayout(true);
			this.alternateGLAccountWithAttributeGridInTabControl.PerformLayout();
			this.alternateGLAccountWithAttributeGridInTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.relatedAlternateAccountsTabPage.ResumeLayout(true);
		}


		private ZArchitecture.GUI.ZGroupBox alternateAccountGroupBox;
		private ZDropEdit unitsDropEdit;
		private ZGuidFindBox parentAccountGuidFindBox;
		private ZGuidFindBox chartGuidFindBox;
		private Core.Forms.ZPostingButtonsUserControl postingButtonsUserControl;
		private ZDropEdit cashFlowCategoryDropEdit;
		private ZGroupBox chartAndAccountTypeGroupBox;
		private ZGroupBox parentAccountGroupBox;
		private ARAP.Invoicing.SingleAlternateGLAccountControl singleAlternateGLAccountControl;
		private ARAP.Invoicing.AlternateGLAccountWithAttributeGridControl alternateGLAccountWithAttributeGridControl;
		private ZTabControl formTabControl;
		private ZTabPage formDetailTabPage;
		private ZTabPage formLogTabPage;
		private ZTabControl editAlternateAccountTabControl;
		private ZTabPage detailTabPage;
		private ZTabPage relatedAlternateAccountsTabPage;
		private ARAP.Invoicing.SingleAlternateGLAccountControl singleAlternateGLAccountInTabControl;
		private ZGroupBox alternateAccountDetailGroupBox;
		private ZGroupBox attributesGroupBox;
		private ZGrid alternateAccountAttributeGrid;
		private ARAP.Invoicing.AlternateGLAccountWithAttributeGridControl alternateGLAccountWithAttributeGridInTabControl;
	}
}
