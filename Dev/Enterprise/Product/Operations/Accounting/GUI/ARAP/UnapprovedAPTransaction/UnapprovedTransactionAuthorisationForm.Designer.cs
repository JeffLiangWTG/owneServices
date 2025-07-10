using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI.ARAP.UnapprovedAPTransaction
{
    partial class UnapprovedTransactionAuthorisationForm
    {
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        new void InitializeComponent()
        {
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zLabelUnapprovedTransactions = new Enterprise.ZArchitecture.ZLabel();
			this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MaxAuthorisationLevelTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AuthorisationLevelTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zButtonApproveAll = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zButtonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CandidatesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPanel1.SuspendLayout();
			this.zPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CandidatesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 558, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(798, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.UnapprovedTransactionConverter);
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.zLabelUnapprovedTransactions);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(798, 32, true);
			this.zPanel1.TabIndex = 0;
			// 
			// zLabelUnapprovedTransactions
			// 
			this.zLabelUnapprovedTransactions.AutoSize = true;
			this.zLabelUnapprovedTransactions.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("UnapprovedTransactionAuthorisationForm|03291278-d9c1-413c-aeeb-43389aaada52", "Highlight transactions and right click to approve");
			this.zLabelUnapprovedTransactions.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 9, true);
			this.zLabelUnapprovedTransactions.Name = "zLabelUnapprovedTransactions";
			this.zLabelUnapprovedTransactions.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 13, true);
			this.zLabelUnapprovedTransactions.TabIndex = 0;
			// 
			// zPanel2
			// 
			this.zPanel2.Controls.Add(this.MaxAuthorisationLevelTextBox);
			this.zPanel2.Controls.Add(this.AuthorisationLevelTextBox);
			this.zPanel2.Controls.Add(this.zButtonApproveAll);
			this.zPanel2.Controls.Add(this.zButtonCancel);
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 512, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(798, 46, true);
			this.zPanel2.TabIndex = 2;
			// 
			// MaxAuthorisationLevelTextBox
			// 
			this.BindingSource.SetBindingMember(this.MaxAuthorisationLevelTextBox, "Candidates.MaxAuthorisationLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UnapprovedTransactionConverter)(null)).Candidates)).SyncRoot)).MaxAuthorisationLevel)));
			this.MaxAuthorisationLevelTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MaxAuthorisationLevelTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("UnapprovedTransactionAuthorisationForm|4c5c7a33-76ad-462d-bcdb-d240d37fa4c5", "Maximum Variance Approval", "Issuing Company's Maximum Variance Approval Level.");
			this.MaxAuthorisationLevelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 13, true);
			this.MaxAuthorisationLevelTextBox.Name = "MaxAuthorisationLevelTextBox";
			this.MaxAuthorisationLevelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.MaxAuthorisationLevelTextBox.TabIndex = 1;
			// 
			// AuthorisationLevelTextBox
			// 
			this.BindingSource.SetBindingMember(this.AuthorisationLevelTextBox, "Candidates.AuthorisationLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UnapprovedTransactionConverter)(null)).Candidates)).SyncRoot)).AuthorisationLevel)));
			this.AuthorisationLevelTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AuthorisationLevelTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("UnapprovedTransactionAuthorisationForm|03eb8a5c-1cfc-4e76-a0fb-860c59b6ded7", "Variance Approval", "Variance Approval Level", "Invoice Variance Approval Level.");
			this.AuthorisationLevelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 13, true);
			this.AuthorisationLevelTextBox.Name = "AuthorisationLevelTextBox";
			this.AuthorisationLevelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.AuthorisationLevelTextBox.TabIndex = 0;
			// 
			// zButtonApproveAll
			// 
			this.zButtonApproveAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zButtonApproveAll.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("UnapprovedTransactionAuthorisationForm|1e03a4c8-f9d3-42f4-8c67-6471bd39a02c", "Approve &All");
			this.zButtonApproveAll.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(627, 11, true);
			this.zButtonApproveAll.Name = "zButtonApproveAll";
			this.zButtonApproveAll.AutoSize = true ;
			this.zButtonApproveAll.TabIndex = 2;
			this.zButtonApproveAll.UseVisualStyleBackColor = true;
			this.zButtonApproveAll.Click += new System.EventHandler(this.zButtonApproveAll_Click);
			// 
			// zButtonCancel
			// 
			this.zButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zButtonCancel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("UnapprovedTransactionAuthorisationForm|071a70da-4ba3-497e-b89d-8722ed17a5a6", "&Close");
			this.zButtonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(708, 11, true);
			this.zButtonCancel.Name = "zButtonCancel";
			this.zButtonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zButtonCancel.TabIndex = 3;
			this.zButtonCancel.UseVisualStyleBackColor = true;
			this.zButtonCancel.Click += new System.EventHandler(this.zButtonCancel_Click);
			// 
			// CandidatesGrid
			// 
			this.CandidatesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CandidatesGrid, "Candidates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UnapprovedTransactionConverter)(null)).Candidates)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UnapprovedTransactionConverter)(null)).Candidates)).SyncRoot)).AH_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UnapprovedTransactionConverter)(null)).Candidates)).SyncRoot)).AH_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UnapprovedTransactionConverter)(null)).Candidates)).SyncRoot)).AH_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UnapprovedTransactionConverter)(null)).Candidates)).SyncRoot)).AH_TransactionCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UnapprovedTransactionConverter)(null)).Candidates)).SyncRoot)).AH_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UnapprovedTransactionConverter)(null)).Candidates)).SyncRoot)).AH_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UnapprovedTransactionConverter)(null)).Candidates)).SyncRoot)).AH_DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UnapprovedTransactionConverter)(null)).Candidates)).SyncRoot)).AH_RX_NKTransactionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UnapprovedTransactionConverter)(null)).Candidates)).SyncRoot)).AH_OSTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UnapprovedTransactionConverter)(null)).Candidates)).SyncRoot)).AH_OutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UnapprovedTransactionConverter)(null)).Candidates)).SyncRoot)).AH_FullyPaidDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UnapprovedTransactionConverter)(null)).Candidates)).SyncRoot)).AH_ChequeOrReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UnapprovedTransactionConverter)(null)).Candidates)).SyncRoot)).DepositBatchNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UnapprovedTransactionConverter)(null)).Candidates)).SyncRoot)).DirectDebitNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UnapprovedTransactionConverter)(null)).Candidates)).SyncRoot)).JobNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UnapprovedTransactionConverter)(null)).Candidates)).SyncRoot)).AH_IsCancelled)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UnapprovedTransactionConverter)(null)).Candidates)).SyncRoot)).IntercompanyOrgProxy)));
			this.CandidatesGrid.CaptionVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("UnapprovedTransactionAuthorisationForm|3c7a97e4-ff68-4b96-9a6a-aaab90064759", "Creditor");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "AH_OH";
			zTextBoxColumnStyleInfo1.ColumnName = "AH_TransactionType";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("UnapprovedTransactionAuthorisationForm|5f05bc67-6f3d-46e5-a942-42c6b81845fa", "Transaction Num.", "Transaction Number");
			zTextBoxColumnStyleInfo2.ColumnName = "AH_TransactionNum";
			zTextBoxColumnStyleInfo3.ColumnName = "AH_TransactionCategory";
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("UnapprovedTransactionAuthorisationForm|cf3c2ebb-83a6-42ce-80af-04c5d52905da", "Post Date");
			zDateEditColumnStyleInfo1.ColumnName = "AH_PostDate";
			zDateEditColumnStyleInfo2.ColumnName = "AH_InvoiceDate";
			zDateEditColumnStyleInfo3.ColumnName = "AH_DueDate";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "AH_RX_NKTransactionCurrency";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("UnapprovedTransactionAuthorisationForm|a7c04691-ee77-4b3b-9615-58f5ca6e5941", "Transaction Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "AH_OSTotal";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "AH_OutstandingAmount";
			zDateEditColumnStyleInfo4.ColumnName = "AH_FullyPaidDate";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("UnapprovedTransactionAuthorisationForm|de3ddb12-6de9-4593-99be-09c51d23d71f", "Check Or Reference");
			zTextBoxColumnStyleInfo4.ColumnName = "AH_ChequeOrReference";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("UnapprovedTransactionAuthorisationForm|9f8d9960-284c-412f-ad02-5f829470d10d", "Deposit Batch Number");
			zTextBoxColumnStyleInfo5.ColumnName = "DepositBatchNumber";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("UnapprovedTransactionAuthorisationForm|44c4e9a4-b8fc-4a92-9dc6-83d0167072d6", "Direct Debit Number");
			zTextBoxColumnStyleInfo6.ColumnName = "DirectDebitNumber";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("UnapprovedTransactionAuthorisationForm|cc2fba32-ac5b-4bef-934d-2b2d420d9c8d", "Job Number");
			zTextBoxColumnStyleInfo7.ColumnName = "JobNumber";
			zCheckBoxColumnStyleInfo1.ColumnName = "AH_IsCancelled";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("UnapprovedTransactionAuthorisationForm|b2b099ba-d0d5-4507-93ab-5bd194c3670c", "Intercompany OrgProxy");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "IntercompanyOrgProxy";
			this.CandidatesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.CandidatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CandidatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CandidatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CandidatesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CandidatesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.CandidatesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.CandidatesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CandidatesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CandidatesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.CandidatesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.CandidatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.CandidatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.CandidatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.CandidatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.CandidatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CandidatesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CandidatesGrid.GridId = "f0942bd2-c549-41a8-8d1b-f05bf148d373";
			this.CandidatesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CandidatesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CandidatesGrid.IsWholeRowSelectedOnClick = true;
			this.CandidatesGrid.LayoutKey = "CandidatesGrid";
			this.CandidatesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			this.CandidatesGrid.Name = "CandidatesGrid";
			this.CandidatesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(798, 480, true);
			this.CandidatesGrid.TabIndex = 1;
			// 
			// UnapprovedTransactionAuthorisationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(798, 582, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("UnapprovedTransactionAuthorisationForm|25f02f44-447a-4c78-9ae3-e011f8e325e5", "Invoice Approval Form");
			this.Controls.Add(this.CandidatesGrid);
			this.Controls.Add(this.zPanel1);
			this.Controls.Add(this.zPanel2);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.UnapprovedTransactionConverter);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ARAP.Invoicing.UnapprovedTransactionConverter";
			this.Name = "UnapprovedTransactionAuthorisationForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zPanel2, 0);
			this.Controls.SetChildIndex(this.zPanel1, 0);
			this.Controls.SetChildIndex(this.CandidatesGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CandidatesGrid)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private Enterprise.ZArchitecture.GUI.ZPanel zPanel1;
		private Enterprise.ZArchitecture.GUI.ZPanel zPanel2;
        private Enterprise.ZArchitecture.GUI.ZButton zButtonApproveAll;
        private Enterprise.ZArchitecture.GUI.ZButton zButtonCancel;
        private Enterprise.ZArchitecture.ZLabel zLabelUnapprovedTransactions;
		private Enterprise.ZArchitecture.ZGrid CandidatesGrid;
		private Enterprise.ZArchitecture.ZTextBox AuthorisationLevelTextBox;
		private Enterprise.ZArchitecture.ZTextBox MaxAuthorisationLevelTextBox;
    }
}
