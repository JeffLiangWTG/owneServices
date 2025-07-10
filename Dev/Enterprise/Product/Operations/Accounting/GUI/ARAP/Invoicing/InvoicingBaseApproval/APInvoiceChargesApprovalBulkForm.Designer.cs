using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.ZArchitecture;
using System.Windows.Forms;
namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	partial class APInvoiceChargesApprovalBulkForm
	{
		private System.ComponentModel.IContainer components = null;

		protected override void SetVisibleCore(bool value)
		{
			base.SetVisibleCore(value);
			if (((APInvoiceChargesApprovalBulk)BusinessEntity).IsChargeHidingApplied())
			{
				this.ChargeHidingMessageLabel.Visible = true;
			}
		}

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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MaximumAmountToApproveCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ChargeHidingMessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.apDetailsTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.requisitionDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.requisitionStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.transactionNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.creditorTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BottomPanel.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.TopGridPanel.SuspendLayout();
			this.MiddlePanel.SuspendLayout();
			this.TopGridGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TopGrid)).BeginInit();
			this.TopGrid.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DetailsGrid)).BeginInit();
			this.DetailsGrid.SuspendLayout();
			this.TopSingleRequestPanel.SuspendLayout();
			this.DetailsTopPanel.SuspendLayout();
			this.CreatedUserCodeFindBox.SuspendLayout();
			this.ApprovingUserCodeFindBox.SuspendLayout();
			this.CreatedTimeDateEdit.SuspendLayout();
			this.ApprovalDateEdit.SuspendLayout();
			this.RequestingBranchGuidFindBox.SuspendLayout();
			this.ApprovalStatusDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.apDetailsTopPanel.SuspendLayout();
			this.requisitionDateEdit.SuspendLayout();
			this.requisitionStatusDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BottomPanel
			// 
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 358, true);
			// 
			// MiddlePanel
			// 
			this.MiddlePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 242, true);
			this.MiddlePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(726, 116, true);
			// 
			// TopGrid
			// 
			this.BindingSource.SetBindingMember(this.TopGrid, "Approvals");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).JobNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).XP_RequestID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).XP_GB_RequestingBranch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).XP_ApprovalStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).XP_GS_NKApprovingUser1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).XP_ApprovalDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).XP_ReasonDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).XP_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).XP_SystemCreateTimeUtc)));
			zTextBoxColumnStyleInfo1.ColumnName = "JobNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "XP_RequestID";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "XP_GB_RequestingBranch";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.ColumnName = "XP_ApprovalStatus";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "XP_GS_NKApprovingUser1";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zDateEditColumnStyleInfo1.ColumnName = "XP_ApprovalDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "XP_ReasonDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo5.ColumnName = "XP_SystemCreateUser";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDateEditColumnStyleInfo2.ColumnName = "XP_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TopGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TopGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TopGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.TopGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.TopGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.TopGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.TopGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.TopGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.TopGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.apDetailsTopPanel);
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(726, 116, true);
			this.DetailsGroupBox.Controls.SetChildIndex(this.DetailsTopPanel, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.apDetailsTopPanel, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.DetailsGrid, 0);
			// 
			// DetailsGrid
			// 
			this.BindingSource.SetBindingMember(this.DetailsGrid, "Approvals.PostingDetails.FilteredCharges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.FilteredCharges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequestChargeDetails)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.FilteredCharges)).SyncRoot)).JobNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequestChargeDetails)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.FilteredCharges)).SyncRoot)).ChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequestChargeDetails)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.FilteredCharges)).SyncRoot)).Branch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequestChargeDetails)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.FilteredCharges)).SyncRoot)).Department)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequestChargeDetails)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.FilteredCharges)).SyncRoot)).CostCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequestChargeDetails)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.FilteredCharges)).SyncRoot)).OSCostAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequestChargeDetails)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.FilteredCharges)).SyncRoot)).LocalCostAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequestChargeDetails)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.FilteredCharges)).SyncRoot)).PlaceOfSupply)));
			zTextBoxColumnStyleInfo6.ColumnName = "JobNumber";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "ChargeCode";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.ColumnName = "Branch";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo9.ColumnName = "Department";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.ColumnName = "CostCurrency";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "OSCostAmount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "LocalCostAmount";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.ColumnName = "PlaceOfSupply";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.DetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.DetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(723, 49, true);
			// 
			// TopSingleRequestPanel
			// 
			this.TopSingleRequestPanel.Controls.Add(this.ChargeHidingMessageLabel);
			this.TopSingleRequestPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(726, 130, true);
			this.TopSingleRequestPanel.Controls.SetChildIndex(this.ChargeHidingMessageLabel, 0);
			this.TopSingleRequestPanel.Controls.SetChildIndex(this.ReasonDescriptionTextBox, 0);
			this.TopSingleRequestPanel.Controls.SetChildIndex(this.JobNumberTextBox, 0);
			this.TopSingleRequestPanel.Controls.SetChildIndex(this.RequestingBranchGuidFindBox, 0);
			this.TopSingleRequestPanel.Controls.SetChildIndex(this.ApprovalDateEdit, 0);
			this.TopSingleRequestPanel.Controls.SetChildIndex(this.CreatedTimeDateEdit, 0);
			this.TopSingleRequestPanel.Controls.SetChildIndex(this.ApprovingUserCodeFindBox, 0);
			this.TopSingleRequestPanel.Controls.SetChildIndex(this.CreatedUserCodeFindBox, 0);
			this.TopSingleRequestPanel.Controls.SetChildIndex(this.ApprovalStatusDropEdit, 0);
			// 
			// DetailsTopPanel
			// 
			this.DetailsTopPanel.Controls.Add(this.MaximumAmountToApproveCalcEdit);
			this.DetailsTopPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.DetailsTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 89, true);
			this.DetailsTopPanel.TabIndex = 2;
			// 
			// CreatedUserCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.CreatedUserCodeFindBox, "Approvals.XP_SystemCreateUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).XP_SystemCreateUser)));
			// 
			// ApprovingUserCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.ApprovingUserCodeFindBox, "Approvals.XP_GS_NKApprovingUser1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).XP_GS_NKApprovingUser1)));
			// 
			// CreatedTimeDateEdit
			// 
			this.BindingSource.SetBindingMember(this.CreatedTimeDateEdit, "Approvals.XP_SystemCreateTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).XP_SystemCreateTimeUtc)));
			// 
			// ApprovalDateEdit
			// 
			this.BindingSource.SetBindingMember(this.ApprovalDateEdit, "Approvals.XP_ApprovalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).XP_ApprovalDate)));
			// 
			// RequestingBranchGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.RequestingBranchGuidFindBox, "Approvals.XP_GB_RequestingBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).XP_GB_RequestingBranch)));
			this.RequestingBranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			// 
			// JobNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.JobNumberTextBox, "Approvals.JobNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).JobNumber)));
			// 
			// ReasonDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReasonDescriptionTextBox, "Approvals.XP_ReasonDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).XP_ReasonDescription)));
			// 
			// ApprovalStatusDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ApprovalStatusDropEdit, "Approvals.XP_ApprovalStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).XP_ApprovalStatus)));
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 394, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk);
			// 
			// MaximumAmountToApproveCalcEdit
			// 
			this.MaximumAmountToApproveCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MaximumAmountToApproveCalcEdit, "Approvals.PostingDetails.MaxAmountToApprove");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.MaxAmountToApprove)));
			this.MaximumAmountToApproveCalcEdit.CaptionResourceString = null;
			this.MaximumAmountToApproveCalcEdit.DecimalPlaces = 2;
			this.MaximumAmountToApproveCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(578, 3, true);
			this.MaximumAmountToApproveCalcEdit.Name = "MaximumAmountToApproveCalcEdit";
			this.MaximumAmountToApproveCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 17, true);
			this.MaximumAmountToApproveCalcEdit.TabIndex = 0;
			this.MaximumAmountToApproveCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ChargeHidingMessageLabel
			// 
			this.ChargeHidingMessageLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ed03ea62-a3cb-4683-b325-48e7e0d8d47b", "Note: Charges posted to branch / dept outside login permission are not listed.");
			this.ChargeHidingMessageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ChargeHidingMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 103, true);
			this.ChargeHidingMessageLabel.Name = "ChargeHidingMessageLabel";
			this.ChargeHidingMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 20, true);
			this.ChargeHidingMessageLabel.TabIndex = 8;
			this.ChargeHidingMessageLabel.Visible = false;
			// 
			// apDetailsTopPanel
			// 
			this.apDetailsTopPanel.Controls.Add(this.requisitionDateEdit);
			this.apDetailsTopPanel.Controls.Add(this.requisitionStatusDropEdit);
			this.apDetailsTopPanel.Controls.Add(this.transactionNumberTextBox);
			this.apDetailsTopPanel.Controls.Add(this.creditorTextBox);
			this.apDetailsTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.apDetailsTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.apDetailsTopPanel.Name = "apDetailsTopPanel";
			this.apDetailsTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(723, 26, true);
			this.apDetailsTopPanel.TabIndex = 0;
			// 
			// requisitionDateEdit
			// 
			this.requisitionDateEdit.AllowDrop = true;
			this.requisitionDateEdit.AutoCompleteMonthThreshold = 1;
			this.requisitionDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.requisitionDateEdit, "Approvals.RequisitionDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).RequisitionDate)));
			this.requisitionDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(599, 4, true);
			this.requisitionDateEdit.Name = "requisitionDateEdit";
			this.requisitionDateEdit.TabIndex = 3;
			// 
			// requisitionStatusDropEdit
			// 
			this.requisitionStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.requisitionStatusDropEdit, "Approvals.RequisitionStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).RequisitionStatus)));
			this.requisitionStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(443, 4, true);
			this.requisitionStatusDropEdit.Name = "requisitionStatusDropEdit";
			this.requisitionStatusDropEdit.PreBoundMaxLength = 3;
			this.requisitionStatusDropEdit.ShouldResizeByMaxLength = true;
			this.requisitionStatusDropEdit.ShowDescriptionBox = false;
			this.requisitionStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 17, true);
			this.requisitionStatusDropEdit.TabIndex = 2;
			// 
			// transactionNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.transactionNumberTextBox, "Approvals.PostingDetails.TransactionNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.TransactionNumber)));
			this.transactionNumberTextBox.CaptionResourceString = null;
			this.transactionNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 4, true);
			this.transactionNumberTextBox.Name = "transactionNumberTextBox";
			this.transactionNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 17, true);
			this.transactionNumberTextBox.TabIndex = 1;
			// 
			// creditorTextBox
			// 
			this.BindingSource.SetBindingMember(this.creditorTextBox, "Approvals.PostingDetails.Creditor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceChargesApprovalRequest)(((System.Collections.IList)(((Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk)(null)).Approvals)).SyncRoot)).PostingDetails.Creditor)));
			this.creditorTextBox.CaptionResourceString = null;
			this.creditorTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 4, true);
			this.creditorTextBox.Name = "creditorTextBox";
			this.creditorTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 17, true);
			this.creditorTextBox.TabIndex = 0;
			// 
			// APInvoiceChargesApprovalBulkForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(726, 418, true);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.TransactionApproval.APInvoiceChargesApprovalBulk);
			this.Name = "APInvoiceChargesApprovalBulkForm";
			this.Text = "APInvoiceChargesApprovalBulkForm";
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
			((System.ComponentModel.ISupportInitialize)(this.DetailsGrid)).EndInit();
			this.DetailsGrid.ResumeLayout(false);
			this.DetailsGrid.PerformLayout();
			this.TopSingleRequestPanel.ResumeLayout(false);
			this.TopSingleRequestPanel.PerformLayout();
			this.DetailsTopPanel.ResumeLayout(false);
			this.DetailsTopPanel.PerformLayout();
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
			this.apDetailsTopPanel.ResumeLayout(false);
			this.apDetailsTopPanel.PerformLayout();
			this.requisitionDateEdit.ResumeLayout(true);
			this.requisitionDateEdit.PerformLayout();
			this.requisitionStatusDropEdit.ResumeLayout(true);
			this.requisitionStatusDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZCalcEdit MaximumAmountToApproveCalcEdit;
		private ZLabel ChargeHidingMessageLabel;
		private ZArchitecture.GUI.ZPanel apDetailsTopPanel;
		private ZTextBox transactionNumberTextBox;
		private ZTextBox creditorTextBox;
		private ZArchitecture.GUI.ZDropEdit requisitionStatusDropEdit;
		private ZArchitecture.GUI.ZDateEdit requisitionDateEdit;
	}
}
