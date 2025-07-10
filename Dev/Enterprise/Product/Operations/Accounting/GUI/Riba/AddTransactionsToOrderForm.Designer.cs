namespace Enterprise.Accounting.GUI.Riba
{
	public partial class AddTransactionsToOrderForm
	{


		#region Component Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.FilterPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.NotificationGridContainer = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.InvoicesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.NotificationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.NotificationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FilterGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BottomButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FilterPanel.SuspendLayout();
			this.NotificationGridContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoicesGrid)).BeginInit();
			this.NotificationPanel.SuspendLayout();
			this.FilterGroupBox.SuspendLayout();
			this.BottomButtonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 572, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 27, true);
			this.MainStatusBar.TabIndex = 6;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = 436;
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = 437;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.Riba.OrderTransactionsFilterHolder);
			// 
			// FilterPanel
			// 
			this.FilterPanel.Controls.Add(this.NotificationGridContainer);
			this.FilterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.FilterPanel.Name = "FilterPanel";
			this.FilterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 498, true);
			this.FilterPanel.TabIndex = 0;
			// 
			// NotificationGridContainer
			// 
			this.NotificationGridContainer.Controls.Add(this.InvoicesGrid);
			this.NotificationGridContainer.Controls.Add(this.NotificationPanel);
			this.NotificationGridContainer.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.NotificationGridContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 139, true);
			this.NotificationGridContainer.Name = "NotificationGridContainer";
			this.NotificationGridContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 359, true);
			this.NotificationGridContainer.TabIndex = 0;
			// 
			// InvoicesGrid
			// 
			this.InvoicesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.InvoicesGrid, "Transactions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.OrderTransactionsFilterHolder)(null)).Transactions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.OrderTransactionsFilterHolder)(null)).Transactions)).SyncRoot)).AH_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.OrderTransactionsFilterHolder)(null)).Transactions)).SyncRoot)).AH_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.OrderTransactionsFilterHolder)(null)).Transactions)).SyncRoot)).AH_ConsolidatedInvoiceRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.OrderTransactionsFilterHolder)(null)).Transactions)).SyncRoot)).AH_TransactionCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.OrderTransactionsFilterHolder)(null)).Transactions)).SyncRoot)).AH_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.OrderTransactionsFilterHolder)(null)).Transactions)).SyncRoot)).AH_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.OrderTransactionsFilterHolder)(null)).Transactions)).SyncRoot)).AH_DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.OrderTransactionsFilterHolder)(null)).Transactions)).SyncRoot)).AH_RX_NKTransactionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.OrderTransactionsFilterHolder)(null)).Transactions)).SyncRoot)).AH_OSTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.OrderTransactionsFilterHolder)(null)).Transactions)).SyncRoot)).AH_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.OrderTransactionsFilterHolder)(null)).Transactions)).SyncRoot)).AH_InvoiceAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.OrderTransactionsFilterHolder)(null)).Transactions)).SyncRoot)).AH_GSTAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.OrderTransactionsFilterHolder)(null)).Transactions)).SyncRoot)).AH_OutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Riba.OrderTransactionsFilterHolder)(null)).Transactions)).SyncRoot)).AH_ExchangeRate)));
			this.InvoicesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AddTransactionToOrderForm|b7863873-0f39-4e4d-914a-6385f9171f1c", "Type");
			zTextBoxColumnStyleInfo1.ColumnName = "AH_TransactionType";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AddTransactionToOrderForm|85a100f6-61e0-4bd2-af42-f463a2f00f98", "Transaction No.");
			zTextBoxColumnStyleInfo2.ColumnName = "AH_TransactionNum";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.ColumnName = "AH_ConsolidatedInvoiceRef";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.ColumnName = "AH_TransactionCategory";
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AddTransactionToOrderForm|6afe8b65-7b1b-43fd-a294-766153c7fc42", "Post Date");
			zDateEditColumnStyleInfo1.ColumnName = "AH_PostDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo2.ColumnName = "AH_InvoiceDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo3.ColumnName = "AH_DueDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "AH_RX_NKTransactionCurrency";
			zCodeFindBoxColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AddTransactionToOrderForm|23bdd48c-27ff-455c-98fe-32f0565cc1ed", "Trans. Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "AH_OSTotal";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AddTransactionToOrderForm|45356f9d-244d-4a89-8b67-7e9a024871e4", "Debtor");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "AH_OH";
			zOrganisationFindBoxColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AddTransactionToOrderForm|5ac9a4c6-2782-4b02-9419-1a7f109315d6", "Local Amount");
			zCalcEditColumnStyleInfo2.ColumnName = "AH_InvoiceAmount";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AddTransactionToOrderForm|0c0336c9-633e-4e05-9a2d-96a42e6b02c7", "Tax Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "AH_GSTAmount";
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "AH_OutstandingAmount";
			zCalcEditColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "AH_ExchangeRate";
			zCalcEditColumnStyleInfo5.IsReadOnly = true;
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.InvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.InvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.InvoicesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.InvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.InvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.InvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.InvoicesGrid.CopySelectedRowsAllowed = true;
			this.InvoicesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoicesGrid.GridId = "8566fc19-4cae-48aa-a846-343505daa277";
			this.InvoicesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InvoicesGrid.LayoutKey = "InvoiceGrid";
			this.InvoicesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			this.InvoicesGrid.Name = "InvoicesGrid";
			this.InvoicesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.InvoicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 327, true);
			this.InvoicesGrid.TabIndex = 6;
			// 
			// NotificationPanel
			// 
			this.NotificationPanel.Controls.Add(this.NotificationLabel);
			this.NotificationPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.NotificationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NotificationPanel.Name = "NotificationPanel";
			this.NotificationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 32, true);
			this.NotificationPanel.TabIndex = 5;
			// 
			// NotificationLabel
			// 
			this.NotificationLabel.AutoSize = true;
			this.NotificationLabel.ForeColor = System.Drawing.SystemColors.InfoText;
			this.NotificationLabel.IsFontBold = true;
			this.NotificationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.NotificationLabel.Name = "NotificationLabel";
			this.NotificationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.NotificationLabel.TabIndex = 0;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AddTransactionToOrderForm|2216edf5-dbfd-4861-a9bb-2403953a4c6b", "Cancel");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(773, 19, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 2;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// AddButton
			// 
			this.AddButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AddButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AddTransactionToOrderForm|0090dd51-63d5-4c42-8de6-5513d6f69044", "Add");
			this.AddButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 19, true);
			this.AddButton.Name = "AddButton";
			this.AddButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.AddButton.TabIndex = 1;
			this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
			// 
			// FilterGroupBox
			// 
			this.FilterGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AddTransactionToOrderForm|a6f1778a-4ca3-4699-a302-b7c5b0fe1d5d", "Selection Filters");
			this.FilterGroupBox.Controls.Add(this.FilterPanel);
			this.FilterGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FilterGroupBox.Name = "FilterGroupBox";
			this.FilterGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 517, true);
			this.FilterGroupBox.TabIndex = 1;
			this.FilterGroupBox.TabStop = false;
			// 
			// BottomButtonPanel
			// 
			this.BottomButtonPanel.Controls.Add(this.CloseButton);
			this.BottomButtonPanel.Controls.Add(this.AddButton);
			this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 517, true);
			this.BottomButtonPanel.Name = "BottomButtonPanel";
			this.BottomButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 55, true);
			this.BottomButtonPanel.TabIndex = 5;
			// 
			// AddTransactionsToOrderForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AddTransactionToOrderForm|5877969d-7e6f-44bc-b7fa-d2fc23649f1b", "Search Transactions");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 599, true);
			this.Controls.Add(this.FilterGroupBox);
			this.Controls.Add(this.BottomButtonPanel);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Enterprise.Accounting.Business.Riba.OrderTransactionsFilterHolder);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.Riba.OrderTransactionsFilter";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 638, true);
			this.Name = "AddTransactionsToOrderForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomButtonPanel, 0);
			this.Controls.SetChildIndex(this.FilterGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FilterPanel.ResumeLayout(false);
			this.NotificationGridContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.InvoicesGrid)).EndInit();
			this.NotificationPanel.ResumeLayout(false);
			this.NotificationPanel.PerformLayout();
			this.FilterGroupBox.ResumeLayout(false);
			this.BottomButtonPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

		protected AddTransactionsToOrderOnFormFilterControl FilterControl;
		protected Enterprise.Accounting.Business.Riba.AddTransactionsToOrderFilterBusinessObject FilterBuisnessObject;
		protected Enterprise.ZArchitecture.GUI.ZPanel FilterPanel;
		protected Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		protected internal Enterprise.ZArchitecture.GUI.ZButton AddButton;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox FilterGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZPanel BottomButtonPanel;
		protected Enterprise.ZArchitecture.GUI.ZPanel NotificationGridContainer;
		protected internal Enterprise.ZArchitecture.ZGrid InvoicesGrid;
		protected Enterprise.ZArchitecture.GUI.ZPanel NotificationPanel;
		protected Enterprise.ZArchitecture.ZLabel NotificationLabel;

	}
}
