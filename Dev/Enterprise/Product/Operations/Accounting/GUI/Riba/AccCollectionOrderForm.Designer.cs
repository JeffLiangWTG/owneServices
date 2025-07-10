namespace Enterprise.Accounting.GUI.Riba
{
	public partial class AccCollectionOrderForm
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

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.WorkflowTabPage = new MasterFiles.GUI.ZWorkflowTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PostingControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.PostingControl.SuspendLayout();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 431, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1030, 23, true);
			this.MainStatusBar.TabIndex = 3;
			//
			// MessageStatusBarPanel
			//
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(256);
			//
			// ErrorStatusBarPanel
			//
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(257);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.Riba.AccCollectionOrder);
			//
			// MainTabControl
			//
			this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainTabControl.Controls.Add(this.MainTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Controls.Add(this.zLogsTabPage1);
			this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1030, 391, true);
			this.MainTabControl.TabIndex = 0;
			//
			// MainTabPage
			//
			this.MainTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccCollectionOrderForm|6635B1F1-7791-4E42-9E3A-5E48678232DE", "Order Details");
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1022, 364, true);
			this.MainTabPage.TabIndex = 0;
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).CollectionOrderLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).CollectionOrderLines)).SyncRoot)).IncludeInOrder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).CollectionOrderLines)).SyncRoot)).TransactionNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).CollectionOrderLines)).SyncRoot)).TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).CollectionOrderLines)).SyncRoot)).CollectionAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).CollectionOrderLines)).SyncRoot)).CollectionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).CollectionOrderLines)).SyncRoot)).JobInvoicingNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).CollectionOrderLines)).SyncRoot)).InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).CollectionOrderLines)).SyncRoot)).PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).CollectionOrderLines)).SyncRoot)).DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).CollectionOrderLines)).SyncRoot)).AOL_IsCancelled)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).CollectionOrderLines)).SyncRoot)).OSInvoiceAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).CollectionOrderLines)).SyncRoot)).OSOutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).CollectionOrderLines)).SyncRoot)).OSCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).CollectionOrderLines)).SyncRoot)).LocalInvoiceAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).CollectionOrderLines)).SyncRoot)).LocalOutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).CollectionOrderLines)).SyncRoot)).LocalCurrency)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).ACO_OH_Debtor)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).CollectionRequestAccountName)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).CollectionRequestAccountNumber)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).ACO_RX_NKCurrency)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).CollectionRequestIBANNumber)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).CollectionRequestBankName)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).CollectionRequestBankBsb)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).CollectionRequestBankSwift)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).CollectionRequestBankCountry)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).ACO_IsCancelled)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).ACO_CancelledReason)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).ACO_BatchNumber)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).ACO_OrderNumber)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).ACO_CollectionDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).BankAccount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(null)).ACO_DepositedDate)));
			//
			// WorkflowTabPage
			//
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1022, 364, true);
			this.WorkflowTabPage.TabIndex = 2;
			this.WorkflowTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.WorkflowTabPage_InitializeTab));
			//
			// zLogsTabPage1
			//
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1022, 364, true);
			this.zLogsTabPage1.TabIndex = 1;
			//
			// BottomPanel
			//
			this.BottomPanel.Controls.Add(this.PostingControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 391, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1030, 40, true);
			this.BottomPanel.TabIndex = 2;
			//
			// PostingControl
			//
			this.PostingControl.AllowDrop = true;
			this.PostingControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(719, 10, true);
			this.PostingControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingControl.Name = "PostingControl";
			this.PostingControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 27, true);
			this.PostingControl.TabIndex = 0;
			//
			// AccCollectionOrderForm
			//
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("F6309A92-A2EE-47DA-B24C-E1D6DA029916", "Collection Order");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1030, 454, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Enterprise.Accounting.Business.Riba.AccCollectionOrder);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.Riba.AccCollectionOrder";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 290, true);
			this.Name = "AccCollectionOrderForm";
			this.RememberFormSize = false;
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.PostingControl.ResumeLayout(true);
			this.PostingControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TransactionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CollectionOrderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OrderSummaryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AccountBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AccountNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AccountNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CurrencyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IBANNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BankNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BankBrnCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SwiftCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CountryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CancelledCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RejectedReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BatchNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OrderNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CollectionDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.BankIntoCodeFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DepositedDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TotalAmountCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zCalcFindBox1 = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.zPostingButtonsUserControl1 = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.MainTabPage.SuspendLayout();
			this.TopPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TransactionsGrid)).BeginInit();
			this.TransactionsGrid.SuspendLayout();
			this.CollectionOrderGroupBox.SuspendLayout();
			this.OrderSummaryGroupBox.SuspendLayout();
			this.AccountBoundGuidFindBox.SuspendLayout();
			this.CollectionDateDateEdit.SuspendLayout();
			this.BankIntoCodeFindBox.SuspendLayout();
			this.DepositedDateDateEdit.SuspendLayout();
			this.TotalAmountCalcFindBox.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.zCalcFindBox1.SuspendLayout();
			this.zPostingButtonsUserControl1.SuspendLayout();
			this.MainTabPage.Controls.Add(this.CollectionOrderGroupBox);
			this.MainTabPage.Controls.Add(this.TopPanel);
			this.MainTabPage.Controls.Add(this.zPanel1);
			//
			// TopPanel
			//
			this.TopPanel.Controls.Add(this.OrderSummaryGroupBox);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1022, 215, true);
			this.TopPanel.TabIndex = 0;
			//
			// TransactionsGrid
			//
			this.TransactionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TransactionsGrid, "CollectionOrderLines");
			this.TransactionsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.Caption = "";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("66afaeaf-d371-4657-867e-0e84e2a594d7", "Include");
			zCheckBoxColumnStyleInfo1.ColumnName = "IncludeInOrder";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("bf53add9-7cd9-412e-8e76-8db189b27e2f", "Transaction No.");
			zTextBoxColumnStyleInfo1.ColumnName = "TransactionNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e83db575-278e-4f84-b280-b0f2d9229899", "Transaction Type");
			zTextBoxColumnStyleInfo2.ColumnName = "TransactionType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1ed5179e-7748-4acb-a35c-daccfcd600d5", "Collection Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "CollectionAmount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e5eac8fe-1b6f-4241-8b25-46ef6de07a9c", "Collection Currency");
			zTextBoxColumnStyleInfo3.ColumnName = "CollectionCurrency";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("00e3f09a-ebd7-47ed-8ddb-e334662fa5e1", "Job Invoicing No.");
			zTextBoxColumnStyleInfo4.ColumnName = "JobInvoicingNumber";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e4b06bc8-3933-4c4b-895e-342f440b8673", "Invoice Date");
			zDateEditColumnStyleInfo1.ColumnName = "InvoiceDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6d30ab98-0866-49bb-8a49-c0814b15d02a", "Post Date");
			zDateEditColumnStyleInfo2.ColumnName = "PostDate";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a62dc198-0923-4eb6-ac16-2e49cba093e9", "Due Date");
			zDateEditColumnStyleInfo3.ColumnName = "DueDate";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("baccdca4-8c53-4a6b-a3db-623c0bdbc26f", "Is Rejected");
			zCheckBoxColumnStyleInfo2.ColumnName = "AOL_IsCancelled";
			zCheckBoxColumnStyleInfo2.IsReadOnly = true;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c5c86323-2305-46c2-82f7-6611905a4649", "OS Invoice Amt");
			zCalcEditColumnStyleInfo2.ColumnName = "OSInvoiceAmount";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("91c0590c-c882-4638-b00a-141a0053e7ce", "OS Outstanding Amt");
			zCalcEditColumnStyleInfo3.ColumnName = "OSOutstandingAmount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("658d5ffe-191e-4a6d-a1b3-0bbb7834819e", "Invoice Currency");
			zTextBoxColumnStyleInfo5.ColumnName = "OSCurrency";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e83c0b9a-24ea-4aa0-8483-0ea545fddd14", "Local Invoice Amt");
			zCalcEditColumnStyleInfo4.ColumnName = "LocalInvoiceAmount";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7d6a0d5e-9bd2-481c-9657-89558a9d081b", "Local Outstanding Amt");
			zCalcEditColumnStyleInfo5.ColumnName = "LocalOutstandingAmount";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4ec9077e-3d2f-4de4-a89e-4ac4d9c1d3bd", "Local Currency");
			zTextBoxColumnStyleInfo6.ColumnName = "LocalCurrency";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TransactionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TransactionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.TransactionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.TransactionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.TransactionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.TransactionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.TransactionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.TransactionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.TransactionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.TransactionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.TransactionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransactionsGrid.GridId = "D0192A7E-A6A9-4AB5-B802-B330F3884EAE";
			this.TransactionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TransactionsGrid.LayoutKey = "TransactionsGrid";
			this.TransactionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TransactionsGrid.Name = "TransactionsGrid";
			this.TransactionsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.TransactionsGrid.ShouldSetErrorsOnTabPage = false;
			this.TransactionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 94, true);
			this.TransactionsGrid.TabIndex = 1;
			//
			// CollectionOrderGroupBox
			//
			this.CollectionOrderGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9AC6F8C7-5512-4188-A168-0611321EF7D4", "Transaction Details");
			this.CollectionOrderGroupBox.Controls.Add(this.TransactionsGrid);
			this.CollectionOrderGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CollectionOrderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 215, true);
			this.CollectionOrderGroupBox.Name = "CollectionOrderGroupBox";
			this.CollectionOrderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1022, 113, true);
			this.CollectionOrderGroupBox.TabIndex = 1;
			this.CollectionOrderGroupBox.TabStop = false;
			//
			// OrderSummaryGroupBox
			//
			this.OrderSummaryGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6F4DC327-B338-4515-AC49-A4EDC416B062", "Order Summary");
			this.OrderSummaryGroupBox.Controls.Add(this.AccountBoundGuidFindBox);
			this.OrderSummaryGroupBox.Controls.Add(this.AccountNameTextBox);
			this.OrderSummaryGroupBox.Controls.Add(this.AccountNumberTextBox);
			this.OrderSummaryGroupBox.Controls.Add(this.CurrencyTextBox);
			this.OrderSummaryGroupBox.Controls.Add(this.IBANNumberTextBox);
			this.OrderSummaryGroupBox.Controls.Add(this.BankNameTextBox);
			this.OrderSummaryGroupBox.Controls.Add(this.BankBrnCodeTextBox);
			this.OrderSummaryGroupBox.Controls.Add(this.SwiftCodeTextBox);
			this.OrderSummaryGroupBox.Controls.Add(this.CountryTextBox);
			this.OrderSummaryGroupBox.Controls.Add(this.CancelledCheckBox);
			this.OrderSummaryGroupBox.Controls.Add(this.RejectedReasonTextBox);
			this.OrderSummaryGroupBox.Controls.Add(this.BatchNumberTextBox);
			this.OrderSummaryGroupBox.Controls.Add(this.OrderNumberTextBox);
			this.OrderSummaryGroupBox.Controls.Add(this.CollectionDateDateEdit);
			this.OrderSummaryGroupBox.Controls.Add(this.BankIntoCodeFindBox);
			this.OrderSummaryGroupBox.Controls.Add(this.DepositedDateDateEdit);
			this.OrderSummaryGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrderSummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrderSummaryGroupBox.Name = "OrderSummaryGroupBox";
			this.OrderSummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1022, 215, true);
			this.OrderSummaryGroupBox.TabIndex = 1;
			this.OrderSummaryGroupBox.TabStop = false;
			//
			// AccountBoundGuidFindBox
			//
			this.AccountBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AccountBoundGuidFindBox, "ACO_OH_Debtor");
			this.AccountBoundGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("48E991DF-2CF1-445D-B282-D9CB043A113A", "Account");
			this.AccountBoundGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.AccountBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 19, true);
			this.AccountBoundGuidFindBox.Name = "AccountBoundGuidFindBox";
			this.AccountBoundGuidFindBox.PopupCaption = null;
			this.AccountBoundGuidFindBox.ShouldResize = true;
			this.AccountBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 20, true);
			this.AccountBoundGuidFindBox.TabIndex = 7;
			this.AccountBoundGuidFindBox.ReadOnly = true;
			//
			// AccountNameTextBox
			//
			this.BindingSource.SetBindingMember(this.AccountNameTextBox, "CollectionRequestAccountName");
			this.AccountNameTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DCA12DF9-690A-4DC6-B58C-88A826B20428", "Account Name");
			this.AccountNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 44, true);
			this.AccountNameTextBox.Name = "AccountNameTextBox";
			this.AccountNameTextBox.ReadOnly = true;
			this.AccountNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 20, true);
			this.AccountNameTextBox.TabIndex = 0;
			//
			// AccountNumberTextBox
			//
			this.BindingSource.SetBindingMember(this.AccountNumberTextBox, "CollectionRequestAccountNumber");
			this.AccountNumberTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("C393D590-2E49-4C50-A9D3-029A293E4784", "Account Number");
			this.AccountNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 71, true);
			this.AccountNumberTextBox.Name = "AccountNumberTextBox";
			this.AccountNumberTextBox.ReadOnly = true;
			this.AccountNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.AccountNumberTextBox.TabIndex = 0;
			//
			// CurrencyTextBox
			//
			this.BindingSource.SetBindingMember(this.CurrencyTextBox, "ACO_RX_NKCurrency");
			this.CurrencyTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("EB5DEAC3-B352-41B5-A3A9-DE38A4398FB6", "Currency");
			this.CurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(471, 71, true);
			this.CurrencyTextBox.Name = "CurrencyTextBox";
			this.CurrencyTextBox.ReadOnly = true;
			this.CurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 20, true);
			this.CurrencyTextBox.TabIndex = 0;
			//
			// IBANNumberTextBox
			//
			this.BindingSource.SetBindingMember(this.IBANNumberTextBox, "CollectionRequestIBANNumber");
			this.IBANNumberTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AB7128AC-7549-4D52-8658-B3CCFA49286E", "IBAN Number");
			this.IBANNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 94, true);
			this.IBANNumberTextBox.Name = "IBANNumberTextBox";
			this.IBANNumberTextBox.ReadOnly = true;
			this.IBANNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.IBANNumberTextBox.TabIndex = 0;
			//
			// BankNameTextBox
			//
			this.BindingSource.SetBindingMember(this.BankNameTextBox, "CollectionRequestBankName");
			this.BankNameTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("FD7B9590-3E93-4FA4-85EF-28281784AFF6", "Bank Name");
			this.BankNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 119, true);
			this.BankNameTextBox.Name = "BankNameTextBox";
			this.BankNameTextBox.ReadOnly = true;
			this.BankNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 20, true);
			this.BankNameTextBox.TabIndex = 0;
			//
			// BankBrnCodeTextBox
			//
			this.BindingSource.SetBindingMember(this.BankBrnCodeTextBox, "CollectionRequestBankBsb");
			this.BankBrnCodeTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("093F787F-883D-4C4C-B31C-0347D44058E5", "Bank/Brn. Code");
			this.BankBrnCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 146, true);
			this.BankBrnCodeTextBox.Name = "BankBrnCodeTextBox";
			this.BankBrnCodeTextBox.ReadOnly = true;
			this.BankBrnCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.BankBrnCodeTextBox.TabIndex = 0;
			//
			// SwiftCodeTextBox
			//
			this.BindingSource.SetBindingMember(this.SwiftCodeTextBox, "CollectionRequestBankSwift");
			this.SwiftCodeTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AB16E2D8-B5BB-443C-87E8-882F47245564", "Swift Code");
			this.SwiftCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(278, 146, true);
			this.SwiftCodeTextBox.Name = "SwiftCodeTextBox";
			this.SwiftCodeTextBox.ReadOnly = true;
			this.SwiftCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.SwiftCodeTextBox.TabIndex = 0;
			//
			// CountryTextBox
			//
			this.BindingSource.SetBindingMember(this.CountryTextBox, "CollectionRequestBankCountry");
			this.CountryTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("B6D118DF-0A91-4105-9538-012B489EABC1", "Ctry./Rgn.", "Country/Region");
			this.CountryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 146, true);
			this.CountryTextBox.Name = "CountryTextBox";
			this.CountryTextBox.ReadOnly = true;
			this.CountryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.CountryTextBox.TabIndex = 0;
			//
			// CancelledCheckBox
			//
			this.CancelledCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CancelledCheckBox, "ACO_IsCancelled");
			this.CancelledCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("{A3306C05-B512-4098-881F-6B852D6394D6}", "Is Rejected");
			this.CancelledCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CancelledCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.CancelledCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 174, true);
			this.CancelledCheckBox.Name = "CancelledCheckBox";
			this.CancelledCheckBox.ReadOnly = true;
			this.CancelledCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 17, true);
			this.CancelledCheckBox.TabIndex = 0;
			//
			// RejectedReasonTextBox
			//
			this.BindingSource.SetBindingMember(this.RejectedReasonTextBox, "ACO_CancelledReason");
			this.RejectedReasonTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5C586523-73E0-4D02-9612-9DE28A88EEB3", "Rejected Reason");
			this.RejectedReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 171, true);
			this.RejectedReasonTextBox.Name = "RejectedReasonTextBox";
			this.RejectedReasonTextBox.ReadOnly = true;
			this.RejectedReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.RejectedReasonTextBox.TabIndex = 0;
			//
			// BatchNumberTextBox
			//
			this.BatchNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BatchNumberTextBox, "ACO_BatchNumber");
			this.BatchNumberTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3DE3DF34-6683-4820-B4D9-7C4DD9D24C96", "Batch Number");
			this.BatchNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(862, 19, true);
			this.BatchNumberTextBox.Name = "BatchNumberTextBox";
			this.BatchNumberTextBox.ReadOnly = true;
			this.BatchNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.BatchNumberTextBox.TabIndex = 0;
			//
			// OrderNumberTextBox
			//
			this.OrderNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OrderNumberTextBox, "ACO_OrderNumber");
			this.OrderNumberTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("B253ADCC-8A89-4F40-B5EF-49B0DD97730F", "Order Number");
			this.OrderNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(862, 44, true);
			this.OrderNumberTextBox.Name = "OrderNumberTextBox";
			this.OrderNumberTextBox.ReadOnly = true;
			this.OrderNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.OrderNumberTextBox.TabIndex = 0;
			//
			// CollectionDateDateEdit
			//
			this.CollectionDateDateEdit.AllowDrop = true;
			this.CollectionDateDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CollectionDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.CollectionDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CollectionDateDateEdit, "ACO_CollectionDate");
			this.CollectionDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("42A115B2-C934-406E-A036-EE29329894DB", "Collection Date");
			this.CollectionDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(862, 71, true);
			this.CollectionDateDateEdit.Name = "CollectionDateDateEdit";
			this.CollectionDateDateEdit.TabIndex = 0;
			//
			// BankIntoCodeFindBox
			//
			this.BankIntoCodeFindBox.AllowDrop = true;
			this.BankIntoCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BankIntoCodeFindBox, "BankAccount");
			this.BankIntoCodeFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CEAF377F-A5A4-4F41-83E3-BCD406EAF450", "Bank Into");
			this.BankIntoCodeFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.BankIntoCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(862, 94, true);
			this.BankIntoCodeFindBox.Name = "BankIntoCodeFindBox";
			this.BankIntoCodeFindBox.ShouldResize = true;
			this.BankIntoCodeFindBox.ShowDescriptionBox = false;
			this.BankIntoCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.BankIntoCodeFindBox.TabIndex = 0;
			//
			// DepositedDateDateEdit
			//
			this.DepositedDateDateEdit.AllowDrop = true;
			this.DepositedDateDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DepositedDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.DepositedDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DepositedDateDateEdit, "ACO_DepositedDate");
			this.DepositedDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("16D4C3A4-3747-496B-B4D7-74C940B18C36", "Deposited Date");
			this.DepositedDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(862, 119, true);
			this.DepositedDateDateEdit.Name = "DepositedDateDateEdit";
			this.DepositedDateDateEdit.TabIndex = 0;
			this.DepositedDateDateEdit.ReadOnly = true;
			//
			// TotalAmountCalcFindBox
			//
			this.TotalAmountCalcFindBox.AllowDrop = true;
			this.TotalAmountCalcFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.TotalAmountCalcFindBox.BindToAmount = "ACO_Amount";
			this.TotalAmountCalcFindBox.BindToDecimalPlaces = "CurrencyDecimals";
			this.TotalAmountCalcFindBox.BindToUnit = "ACO_RX_NKCurrency";
			this.TotalAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("C063B453-659C-442F-9BC9-2DA773AD7B40", "Collection Amount");
			this.TotalAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.TotalAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(757, 8, true);
			this.TotalAmountCalcFindBox.Name = "TotalAmountCalcFindBox";
			this.TotalAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 20, true);
			this.TotalAmountCalcFindBox.TabIndex = 8;
			//
			// zPanel1
			//
			this.zPanel1.Controls.Add(this.TotalAmountCalcFindBox);
			this.zPanel1.Controls.Add(this.zCalcFindBox1);
			this.zPanel1.Controls.Add(this.zPostingButtonsUserControl1);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 328, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1022, 36, true);
			this.zPanel1.TabIndex = 3;
			//
			// zCalcFindBox1
			//
			this.zCalcFindBox1.AllowDrop = true;
			this.zCalcFindBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.zCalcFindBox1.BindToAmount = "ACO_Amount";
			this.zCalcFindBox1.BindToDecimalPlaces = "CurrencyDecimals";
			this.zCalcFindBox1.BindToUnit = "ACO_RX_NKCurrency";
			this.zCalcFindBox1.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.zCalcFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1695, 11, true);
			this.zCalcFindBox1.Name = "zCalcFindBox1";
			this.zCalcFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 20, true);
			this.zCalcFindBox1.TabIndex = 8;
			//
			// zPostingButtonsUserControl1
			//
			this.zPostingButtonsUserControl1.AllowDrop = true;
			this.zPostingButtonsUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zPostingButtonsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1652, -48, true);
			this.zPostingButtonsUserControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.zPostingButtonsUserControl1.Name = "zPostingButtonsUserControl1";
			this.zPostingButtonsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 27, true);
			this.zPostingButtonsUserControl1.TabIndex = 0;
			this.MainTabPage.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TransactionsGrid)).EndInit();
			this.TransactionsGrid.ResumeLayout(false);
			this.TransactionsGrid.PerformLayout();
			this.CollectionOrderGroupBox.ResumeLayout(false);
			this.CollectionOrderGroupBox.PerformLayout();
			this.OrderSummaryGroupBox.ResumeLayout(false);
			this.OrderSummaryGroupBox.PerformLayout();
			this.AccountBoundGuidFindBox.ResumeLayout(true);
			this.AccountBoundGuidFindBox.PerformLayout();
			this.CollectionDateDateEdit.ResumeLayout(true);
			this.CollectionDateDateEdit.PerformLayout();
			this.BankIntoCodeFindBox.ResumeLayout(true);
			this.BankIntoCodeFindBox.PerformLayout();
			this.DepositedDateDateEdit.ResumeLayout(true);
			this.DepositedDateDateEdit.PerformLayout();
			this.TotalAmountCalcFindBox.ResumeLayout(true);
			this.TotalAmountCalcFindBox.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.zCalcFindBox1.ResumeLayout(true);
			this.zCalcFindBox1.PerformLayout();
			this.zPostingButtonsUserControl1.ResumeLayout(true);
			this.zPostingButtonsUserControl1.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}

		private void WorkflowTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			this.WorkflowTabPage.SuspendLayout();
			this.WorkflowTabPage.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(true);
		}

		Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage MainTabPage;
		Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
		Enterprise.ZArchitecture.GUI.ZGroupBox CollectionOrderGroupBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox OrderSummaryGroupBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox AccountBoundGuidFindBox;
		Enterprise.ZArchitecture.ZTextBox AccountNameTextBox;
		Enterprise.ZArchitecture.ZTextBox AccountNumberTextBox;
		Enterprise.ZArchitecture.ZTextBox CurrencyTextBox;
		Enterprise.ZArchitecture.ZTextBox IBANNumberTextBox;
		Enterprise.ZArchitecture.ZTextBox BankNameTextBox;
		Enterprise.ZArchitecture.ZTextBox BankBrnCodeTextBox;
		Enterprise.ZArchitecture.ZTextBox SwiftCodeTextBox;
		Enterprise.ZArchitecture.ZTextBox CountryTextBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox CancelledCheckBox;
		Enterprise.ZArchitecture.ZTextBox RejectedReasonTextBox;
		Enterprise.ZArchitecture.ZTextBox BatchNumberTextBox;
		Enterprise.ZArchitecture.ZTextBox OrderNumberTextBox;
		Enterprise.ZArchitecture.GUI.ZDateEdit CollectionDateDateEdit;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox BankIntoCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZDateEdit DepositedDateDateEdit;
		Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		Enterprise.ZArchitecture.ZGrid TransactionsGrid;
		Enterprise.ZArchitecture.GUI.ZCalcFindBox TotalAmountCalcFindBox;
		private Enterprise.ZArchitecture.GUI.ZPanel zPanel1;
		private Enterprise.ZArchitecture.GUI.ZCalcFindBox zCalcFindBox1;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl zPostingButtonsUserControl1;
		Enterprise.Core.Forms.ZPostingButtonsUserControl PostingControl;
	}
}
