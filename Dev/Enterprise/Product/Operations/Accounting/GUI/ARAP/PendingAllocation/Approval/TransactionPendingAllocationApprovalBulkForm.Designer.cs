using System;
namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	partial class TransactionPendingAllocationApprovalBulkForm
	{
		System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.detailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.transactionTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.importedXMLTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BottomPanel.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.TopGridPanel.SuspendLayout();
			this.MiddlePanel.SuspendLayout();
			this.TopGridGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TopGrid)).BeginInit();
			this.TopGrid.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.TopSingleRequestPanel.SuspendLayout();
			this.CreatedUserCodeFindBox.SuspendLayout();
			this.ApprovingUserCodeFindBox.SuspendLayout();
			this.CreatedTimeDateEdit.SuspendLayout();
			this.ApprovalDateEdit.SuspendLayout();
			this.RequestingBranchGuidFindBox.SuspendLayout();
			this.ApprovalStatusDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.detailsTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BottomPanel
			// 
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 629, true);
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 36, true);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(547, 5, true);
			// 
			// TopGridPanel
			// 
			this.TopGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 112, true);
			// 
			// MiddlePanel
			// 
			this.MiddlePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 412, true);
			// 
			// TopGridGroupBox
			// 
			this.TopGridGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 112, true);
			// 
			// TopGrid
			// 
			this.BindingSource.SetBindingMember(this.TopGrid, "Approvals");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).JobNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).XP_GB_RequestingBranch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).XP_ApprovalStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).XP_GS_NKApprovingUser1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).XP_ApprovalDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).XP_ReasonDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).XP_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).XP_SystemCreateTimeUtc)));
			zTextBoxColumnStyleInfo1.ColumnName = "JobNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "XP_GB_RequestingBranch";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.ColumnName = "XP_ApprovalStatus";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "XP_GS_NKApprovingUser1";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zDateEditColumnStyleInfo1.ColumnName = "XP_ApprovalDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "XP_ReasonDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.ColumnName = "XP_SystemCreateUser";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDateEditColumnStyleInfo2.ColumnName = "XP_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TopGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TopGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.TopGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TopGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.TopGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.TopGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.TopGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.TopGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.TopGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 93, true);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.detailsTabControl);
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 412, true);
			// 
			// TopSingleRequestPanel
			// 
			this.TopSingleRequestPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 105, true);
			// 
			// CreatedUserCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.CreatedUserCodeFindBox, "Approvals.XP_SystemCreateUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).XP_SystemCreateUser)));
			// 
			// ApprovingUserCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.ApprovingUserCodeFindBox, "Approvals.XP_GS_NKApprovingUser1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).XP_GS_NKApprovingUser1)));
			// 
			// CreatedTimeDateEdit
			// 
			this.BindingSource.SetBindingMember(this.CreatedTimeDateEdit, "Approvals.XP_SystemCreateTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).XP_SystemCreateTimeUtc)));
			// 
			// ApprovalDateEdit
			// 
			this.BindingSource.SetBindingMember(this.ApprovalDateEdit, "Approvals.XP_ApprovalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).XP_ApprovalDate)));
			// 
			// RequestingBranchGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.RequestingBranchGuidFindBox, "Approvals.XP_GB_RequestingBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).XP_GB_RequestingBranch)));
			this.RequestingBranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			// 
			// JobNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.JobNumberTextBox, "Approvals.JobNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).JobNumber)));
			// 
			// ReasonDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReasonDescriptionTextBox, "Approvals.XP_ReasonDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).XP_ReasonDescription)));
			// 
			// ApprovalStatusDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ApprovalStatusDropEdit, "Approvals.XP_ApprovalStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).XP_ApprovalStatus)));
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 665, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk);
			// 
			// detailsTabControl
			// 
			this.detailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.detailsTabControl.Controls.Add(this.transactionTabPage);
			this.detailsTabControl.Controls.Add(this.importedXMLTabPage);
			this.detailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.detailsTabControl.Name = "detailsTabControl";
			this.detailsTabControl.SelectedIndex = 0;
			this.detailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 393, true);
			this.detailsTabControl.TabIndex = 0;
			// 
			// transactionTabPage
			// 
			this.transactionTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2c9a2e5c-e224-47e8-9e78-353bb538dedf", "Transaction");
			this.transactionTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.transactionTabPage.Name = "transactionTabPage";
			this.transactionTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.transactionTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 372, true);
			this.transactionTabPage.TabIndex = 0;
			this.transactionTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.transactionTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.NumberOfSupportingDocuments)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.TransactionDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.BranchPK)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.DepartmentPK)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.CreditorPK)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.DueDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.TransactionNumber)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.PostDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.Description)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.CurrencyCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.ExRate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.OSExTaxAmount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.OSTaxAmount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.LocalExTaxAmount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.LocalTaxAmount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.AddressPK)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.ContactPK)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.PlaceOfSupply)));
			// 
			// importedXMLTabPage
			// 
			this.importedXMLTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a13bca33-7c35-4160-b74b-04765f23d486", "Imported XML");
			this.importedXMLTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.importedXMLTabPage.Name = "importedXMLTabPage";
			this.importedXMLTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.importedXMLTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 372, true);
			this.importedXMLTabPage.TabIndex = 1;
			this.importedXMLTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.importedXMLTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.UniversalTransaction)));
			// 
			// TransactionPendingAllocationApprovalBulkForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 689, true);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocationApprovalBulk);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(740, 540, true);
			this.Name = "TransactionPendingAllocationApprovalBulkForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "TransactionPendingAllocationApprovalBulkForm";
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.TopGridPanel.ResumeLayout(false);
			this.TopGridPanel.PerformLayout();
			this.MiddlePanel.ResumeLayout(false);
			this.MiddlePanel.PerformLayout();
			this.TopGridGroupBox.ResumeLayout(false);
			this.TopGridGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TopGrid)).EndInit();
			this.TopGrid.ResumeLayout(false);
			this.TopGrid.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.TopSingleRequestPanel.ResumeLayout(false);
			this.TopSingleRequestPanel.PerformLayout();
			this.CreatedUserCodeFindBox.ResumeLayout(true);
			this.CreatedUserCodeFindBox.PerformLayout();
			this.ApprovingUserCodeFindBox.ResumeLayout(true);
			this.ApprovingUserCodeFindBox.PerformLayout();
			this.CreatedTimeDateEdit.ResumeLayout(true);
			this.CreatedTimeDateEdit.PerformLayout();
			this.ApprovalDateEdit.ResumeLayout(true);
			this.ApprovalDateEdit.PerformLayout();
			this.RequestingBranchGuidFindBox.ResumeLayout(true);
			this.RequestingBranchGuidFindBox.PerformLayout();
			this.ApprovalStatusDropEdit.ResumeLayout(true);
			this.ApprovalStatusDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.detailsTabControl.ResumeLayout(false);
			this.detailsTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		void transactionTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.AH_NumberOfSupportingDocumentsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AH_InvoiceDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.BranchFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.departmentGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.creditorOrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.AH_DueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AH_TransactionNumTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AH_PostDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AH_DescTextbox = new Enterprise.ZArchitecture.ZTextBox();
			this.CurrencyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OSExTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OSTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LocalExTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LocalTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AddressGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ContactGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.placeOfSupplyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.DetailsGrid)).BeginInit();
			this.DetailsGrid.SuspendLayout();
			this.DetailsTopPanel.SuspendLayout();
			this.transactionTabPage.SuspendLayout();
			this.AH_InvoiceDateEdit.SuspendLayout();
			this.BranchFindBox.SuspendLayout();
			this.departmentGuidFindBox.SuspendLayout();
			this.creditorOrganisationFindBox.SuspendLayout();
			this.AH_DueDateEdit.SuspendLayout();
			this.AH_PostDateEdit.SuspendLayout();
			this.AddressGuidFindBox.SuspendLayout();
			this.ContactGuidFindBox.SuspendLayout();
			// 
			// DetailsGrid
			// 
			this.DetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 298, true);
			this.DetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 71, true);
			this.DetailsGrid.Visible = false;
			// 
			// DetailsTopPanel
			// 
			this.DetailsTopPanel.Controls.Add(this.placeOfSupplyTextBox);
			this.DetailsTopPanel.Controls.Add(this.ContactGuidFindBox);
			this.DetailsTopPanel.Controls.Add(this.AddressGuidFindBox);
			this.DetailsTopPanel.Controls.Add(this.LocalTaxAmountCalcEdit);
			this.DetailsTopPanel.Controls.Add(this.LocalExTaxAmountCalcEdit);
			this.DetailsTopPanel.Controls.Add(this.OSTaxAmountCalcEdit);
			this.DetailsTopPanel.Controls.Add(this.OSExTaxAmountCalcEdit);
			this.DetailsTopPanel.Controls.Add(this.ExRateCalcEdit);
			this.DetailsTopPanel.Controls.Add(this.AH_NumberOfSupportingDocumentsCalcEdit);
			this.DetailsTopPanel.Controls.Add(this.AH_InvoiceDateEdit);
			this.DetailsTopPanel.Controls.Add(this.BranchFindBox);
			this.DetailsTopPanel.Controls.Add(this.departmentGuidFindBox);
			this.DetailsTopPanel.Controls.Add(this.creditorOrganisationFindBox);
			this.DetailsTopPanel.Controls.Add(this.AH_DueDateEdit);
			this.DetailsTopPanel.Controls.Add(this.CurrencyTextBox);
			this.DetailsTopPanel.Controls.Add(this.AH_TransactionNumTextBox);
			this.DetailsTopPanel.Controls.Add(this.AH_PostDateEdit);
			this.DetailsTopPanel.Controls.Add(this.AH_DescTextbox);
			this.DetailsTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DetailsTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 295, true);
			this.transactionTabPage.Controls.Add(this.DetailsGrid);
			this.transactionTabPage.Controls.Add(this.DetailsTopPanel);
			// 
			// AH_NumberOfSupportingDocumentsCalcEdit
			// 
			this.AH_NumberOfSupportingDocumentsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AH_NumberOfSupportingDocumentsCalcEdit, "Approvals.PostingDetails.NumberOfSupportingDocuments");
			this.AH_NumberOfSupportingDocumentsCalcEdit.CaptionResourceString = null;
			this.AH_NumberOfSupportingDocumentsCalcEdit.DecimalPlaces = 2;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(439, 26, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.Name = "AH_NumberOfSupportingDocumentsCalcEdit";
			this.AH_NumberOfSupportingDocumentsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 17, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.TabIndex = 3;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Text = "0";
			this.AH_NumberOfSupportingDocumentsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AH_InvoiceDateEdit
			// 
			this.AH_InvoiceDateEdit.AllowDrop = true;
			this.AH_InvoiceDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_InvoiceDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AH_InvoiceDateEdit, "Approvals.PostingDetails.TransactionDate");
			this.AH_InvoiceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 3, true);
			this.AH_InvoiceDateEdit.Name = "AH_InvoiceDateEdit";
			this.AH_InvoiceDateEdit.TabIndex = 0;
			// 
			// BranchFindBox
			// 
			this.BranchFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchFindBox, "Approvals.PostingDetails.BranchPK");
			this.BranchFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.BranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 187, true);
			this.BranchFindBox.Name = "BranchFindBox";
			this.BranchFindBox.ShouldResize = true;
			this.BranchFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 17, true);
			this.BranchFindBox.TabIndex = 13;
			// 
			// departmentGuidFindBox
			// 
			this.departmentGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.departmentGuidFindBox, "Approvals.PostingDetails.DepartmentPK");
			this.departmentGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.departmentGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 210, true);
			this.departmentGuidFindBox.Name = "departmentGuidFindBox";
			this.departmentGuidFindBox.ShouldResize = true;
			this.departmentGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 17, true);
			this.departmentGuidFindBox.TabIndex = 14;
			// 
			// creditorOrganisationFindBox
			// 
			this.creditorOrganisationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.creditorOrganisationFindBox, "Approvals.PostingDetails.CreditorPK");
			this.creditorOrganisationFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.creditorOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 49, true);
			this.creditorOrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.creditorOrganisationFindBox.Name = "creditorOrganisationFindBox";
			this.creditorOrganisationFindBox.ShouldResize = true;
			this.creditorOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 17, true);
			this.creditorOrganisationFindBox.TabIndex = 4;
			// 
			// AH_DueDateEdit
			// 
			this.AH_DueDateEdit.AllowDrop = true;
			this.AH_DueDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_DueDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AH_DueDateEdit, "Approvals.PostingDetails.DueDate");
			this.AH_DueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 72, true);
			this.AH_DueDateEdit.Name = "AH_DueDateEdit";
			this.AH_DueDateEdit.TabIndex = 5;
			// 
			// AH_TransactionNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.AH_TransactionNumTextBox, "Approvals.PostingDetails.TransactionNumber");
			this.AH_TransactionNumTextBox.CaptionResourceString = null;
			this.AH_TransactionNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(439, 3, true);
			this.AH_TransactionNumTextBox.Name = "AH_TransactionNumTextBox";
			this.AH_TransactionNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 17, true);
			this.AH_TransactionNumTextBox.TabIndex = 1;
			// 
			// AH_PostDateEdit
			// 
			this.AH_PostDateEdit.AllowDrop = true;
			this.AH_PostDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_PostDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AH_PostDateEdit, "Approvals.PostingDetails.PostDate");
			this.AH_PostDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 26, true);
			this.AH_PostDateEdit.Name = "AH_PostDateEdit";
			this.AH_PostDateEdit.TabIndex = 2;
			// 
			// AH_DescTextbox
			// 
			this.AH_DescTextbox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AH_DescTextbox, "Approvals.PostingDetails.Description");
			this.AH_DescTextbox.CaptionResourceString = null;
			this.AH_DescTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AH_DescTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 164, true);
			this.AH_DescTextbox.Name = "AH_DescTextbox";
			this.AH_DescTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 17, true);
			this.AH_DescTextbox.TabIndex = 12;
			// 
			// CurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.CurrencyTextBox, "Approvals.PostingDetails.CurrencyCode");
			this.CurrencyTextBox.CaptionResourceString = null;
			this.CurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 95, true);
			this.CurrencyTextBox.Name = "CurrencyTextBox";
			this.CurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 17, true);
			this.CurrencyTextBox.TabIndex = 6;
			// 
			// ExRateCalcEdit
			// 
			this.ExRateCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ExRateCalcEdit, "Approvals.PostingDetails.ExRate");
			this.ExRateCalcEdit.CaptionResourceString = null;
			this.ExRateCalcEdit.DecimalPlaces = 2;
			this.ExRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 95, true);
			this.ExRateCalcEdit.Name = "ExRateCalcEdit";
			this.ExRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 17, true);
			this.ExRateCalcEdit.TabIndex = 7;
			this.ExRateCalcEdit.Text = "0";
			this.ExRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OSExTaxAmountCalcEdit
			// 
			this.OSExTaxAmountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.OSExTaxAmountCalcEdit, "Approvals.PostingDetails.OSExTaxAmount");
			this.OSExTaxAmountCalcEdit.CaptionResourceString = null;
			this.OSExTaxAmountCalcEdit.DecimalPlaces = 2;
			this.OSExTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 118, true);
			this.OSExTaxAmountCalcEdit.Name = "OSExTaxAmountCalcEdit";
			this.OSExTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 17, true);
			this.OSExTaxAmountCalcEdit.TabIndex = 8;
			this.OSExTaxAmountCalcEdit.Text = "0";
			this.OSExTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OSTaxAmountCalcEdit
			// 
			this.OSTaxAmountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.OSTaxAmountCalcEdit, "Approvals.PostingDetails.OSTaxAmount");
			this.OSTaxAmountCalcEdit.CaptionResourceString = null;
			this.OSTaxAmountCalcEdit.DecimalPlaces = 2;
			this.OSTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 141, true);
			this.OSTaxAmountCalcEdit.Name = "OSTaxAmountCalcEdit";
			this.OSTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 17, true);
			this.OSTaxAmountCalcEdit.TabIndex = 10;
			this.OSTaxAmountCalcEdit.Text = "0";
			this.OSTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LocalExTaxAmountCalcEdit
			// 
			this.LocalExTaxAmountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.LocalExTaxAmountCalcEdit, "Approvals.PostingDetails.LocalExTaxAmount");
			this.LocalExTaxAmountCalcEdit.CaptionResourceString = null;
			this.LocalExTaxAmountCalcEdit.DecimalPlaces = 2;
			this.LocalExTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(445, 118, true);
			this.LocalExTaxAmountCalcEdit.Name = "LocalExTaxAmountCalcEdit";
			this.LocalExTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 17, true);
			this.LocalExTaxAmountCalcEdit.TabIndex = 9;
			this.LocalExTaxAmountCalcEdit.Text = "0";
			this.LocalExTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LocalTaxAmountCalcEdit
			// 
			this.LocalTaxAmountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.LocalTaxAmountCalcEdit, "Approvals.PostingDetails.LocalTaxAmount");
			this.LocalTaxAmountCalcEdit.CaptionResourceString = null;
			this.LocalTaxAmountCalcEdit.DecimalPlaces = 2;
			this.LocalTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(445, 141, true);
			this.LocalTaxAmountCalcEdit.Name = "LocalTaxAmountCalcEdit";
			this.LocalTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 17, true);
			this.LocalTaxAmountCalcEdit.TabIndex = 11;
			this.LocalTaxAmountCalcEdit.Text = "0";
			this.LocalTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AddressGuidFindBox
			// 
			this.AddressGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddressGuidFindBox, "Approvals.PostingDetails.AddressPK");
			this.AddressGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.AddressGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 233, true);
			this.AddressGuidFindBox.Name = "AddressGuidFindBox";
			this.AddressGuidFindBox.ShouldResize = true;
			this.AddressGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 17, true);
			this.AddressGuidFindBox.TabIndex = 15;
			// 
			// ContactGuidFindBox
			// 
			this.ContactGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContactGuidFindBox, "Approvals.PostingDetails.ContactPK");
			this.ContactGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.ContactGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 256, true);
			this.ContactGuidFindBox.Name = "ContactGuidFindBox";
			this.ContactGuidFindBox.ShouldResize = true;
			this.ContactGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 17, true);
			this.ContactGuidFindBox.TabIndex = 16;
			// 
			// placeOfSupplyTextBox
			// 
			this.BindingSource.SetBindingMember(this.placeOfSupplyTextBox, "Approvals.PostingDetails.PlaceOfSupply");
			this.placeOfSupplyTextBox.CaptionResourceString = null;
			this.placeOfSupplyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(501, 72, true);
			this.placeOfSupplyTextBox.Name = "placeOfSupplyTextBox";
			this.placeOfSupplyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 17, true);
			this.placeOfSupplyTextBox.TabIndex = 17;
			((System.ComponentModel.ISupportInitialize)(this.DetailsGrid)).EndInit();
			this.DetailsGrid.ResumeLayout(false);
			this.DetailsGrid.PerformLayout();
			this.DetailsTopPanel.ResumeLayout(false);
			this.DetailsTopPanel.PerformLayout();
			this.transactionTabPage.PerformLayout();
			this.AH_InvoiceDateEdit.ResumeLayout(true);
			this.AH_InvoiceDateEdit.PerformLayout();
			this.BranchFindBox.ResumeLayout(true);
			this.BranchFindBox.PerformLayout();
			this.departmentGuidFindBox.ResumeLayout(true);
			this.departmentGuidFindBox.PerformLayout();
			this.creditorOrganisationFindBox.ResumeLayout(true);
			this.creditorOrganisationFindBox.PerformLayout();
			this.AH_DueDateEdit.ResumeLayout(true);
			this.AH_DueDateEdit.PerformLayout();
			this.AH_PostDateEdit.ResumeLayout(true);
			this.AH_PostDateEdit.PerformLayout();
			this.AddressGuidFindBox.ResumeLayout(true);
			this.AddressGuidFindBox.PerformLayout();
			this.ContactGuidFindBox.ResumeLayout(true);
			this.ContactGuidFindBox.PerformLayout();
			this.transactionTabPage.ResumeLayout(true);

		}

		void importedXMLTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.importedInvoiceXMLControl = new Enterprise.Accounting.GUI.ImportedInvoiceXMLControl();
			this.importedXMLTabPage.SuspendLayout();
			this.importedInvoiceXMLControl.SuspendLayout();
			this.importedXMLTabPage.Controls.Add(this.importedInvoiceXMLControl);
			// 
			// importedInvoiceXMLControl
			// 
			this.importedInvoiceXMLControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.importedInvoiceXMLControl, "Approvals.PostingDetails.UniversalTransaction");
			this.importedInvoiceXMLControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.importedInvoiceXMLControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.importedInvoiceXMLControl.Name = "importedInvoiceXMLControl";
			this.importedInvoiceXMLControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 366, true);
			this.importedInvoiceXMLControl.TabIndex = 0;
			this.importedXMLTabPage.PerformLayout();
			this.importedInvoiceXMLControl.ResumeLayout(true);
			this.importedInvoiceXMLControl.PerformLayout();
			this.importedXMLTabPage.ResumeLayout(true);

		}

		#endregion

		private ZArchitecture.ZCalcEdit AH_NumberOfSupportingDocumentsCalcEdit;
		private ZArchitecture.GUI.ZGuidFindBox BranchFindBox;
		private ZArchitecture.GUI.ZGuidFindBox departmentGuidFindBox;
		private MasterFiles.GUI.ZOrganisationFindBox creditorOrganisationFindBox;
		private ZArchitecture.ZCalcEdit LocalTaxAmountCalcEdit;
		private ZArchitecture.ZCalcEdit LocalExTaxAmountCalcEdit;
		private ZArchitecture.ZCalcEdit OSTaxAmountCalcEdit;
		private ZArchitecture.ZCalcEdit OSExTaxAmountCalcEdit;
		private ZArchitecture.ZCalcEdit ExRateCalcEdit;
		private ZArchitecture.GUI.ZGuidFindBox ContactGuidFindBox;
		private ZArchitecture.GUI.ZGuidFindBox AddressGuidFindBox;
		private ZArchitecture.GUI.ZTabControl detailsTabControl;
		private ZArchitecture.GUI.ZTabPage transactionTabPage;
		private ZArchitecture.GUI.ZTabPage importedXMLTabPage;
		private ImportedInvoiceXMLControl importedInvoiceXMLControl;
		private ZArchitecture.ZTextBox placeOfSupplyTextBox;
		public ZArchitecture.GUI.ZDateEdit AH_InvoiceDateEdit;
		public ZArchitecture.GUI.ZDateEdit AH_DueDateEdit;
		public ZArchitecture.ZTextBox AH_TransactionNumTextBox;
		public ZArchitecture.GUI.ZDateEdit AH_PostDateEdit;
		public ZArchitecture.ZTextBox AH_DescTextbox;
		public ZArchitecture.ZTextBox CurrencyTextBox;
	}
}
