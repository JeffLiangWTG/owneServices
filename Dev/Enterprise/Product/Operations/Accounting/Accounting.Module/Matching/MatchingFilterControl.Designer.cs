namespace Enterprise.Accounting.Module
{
	public partial class MatchingFilterControl
	{
		CargoWise.Windows.UI.KSplitter GridSplitter;
		internal ZArchitecture.ZGrid MatchTransactionsDisplayGrid;

		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.MatchTransactionsDisplayGrid = new ZArchitecture.ZGrid();
			this.GridSplitter = new CargoWise.Windows.UI.KSplitter();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MatchTransactionsDisplayGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.FilteredGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("MatchingFilterControl|90a05a05-5351-4ffe-ad68-d57ac889f07f", "Match Number");
			zTextBoxColumnStyleInfo1.ColumnName = "MatchGroupNum";
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("MatchingFilterControl|b38a3810-dabd-4239-9c98-97393f971e14", "Match Date");
			zDateEditColumnStyleInfo1.ColumnName = "MatchDate";
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FilteredGrid.IgnoreParentFilterControl = true;
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 109, true);
			this.FilteredGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 419, true);
			this.FilteredGrid.TabIndex = 3;
			this.FilteredGrid.AfterBind += new System.EventHandler(this.FilteredGrid_AfterBind);
			this.FilteredGrid.CurrentCellChanged += new System.EventHandler(this.FilteredGrid_CurrentCellChanged);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ARMatchingFilterBusinessObject);
			// 
			// MatchTransactionsDisplayGrid
			// 
			this.MatchTransactionsDisplayGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MatchTransactionsDisplayGrid, "TransactionHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((ARMatchingFilterBusinessObject)(null)).TransactionHeaders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((ARMatchingFilterBusinessObject)(null)).TransactionHeaders)).SyncRoot)).AH_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((ARMatchingFilterBusinessObject)(null)).TransactionHeaders)).SyncRoot)).AH_Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((ARMatchingFilterBusinessObject)(null)).TransactionHeaders)).SyncRoot)).AH_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((ARMatchingFilterBusinessObject)(null)).TransactionHeaders)).SyncRoot)).AH_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((ARMatchingFilterBusinessObject)(null)).TransactionHeaders)).SyncRoot)).AH_ChequeOrReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((ARMatchingFilterBusinessObject)(null)).TransactionHeaders)).SyncRoot)).AH_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((ARMatchingFilterBusinessObject)(null)).TransactionHeaders)).SyncRoot)).AH_DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((ARMatchingFilterBusinessObject)(null)).TransactionHeaders)).SyncRoot)).MatchedDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((ARMatchingFilterBusinessObject)(null)).TransactionHeaders)).SyncRoot)).AH_RX_NKTransactionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((ARMatchingFilterBusinessObject)(null)).TransactionHeaders)).SyncRoot)).MatchedAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((ARMatchingFilterBusinessObject)(null)).TransactionHeaders)).SyncRoot)).LocalMatchedAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((ARMatchingFilterBusinessObject)(null)).TransactionHeaders)).SyncRoot)).AH_ExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((ARMatchingFilterBusinessObject)(null)).TransactionHeaders)).SyncRoot)).AH_ConsolidatedInvoiceRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((ARMatchingFilterBusinessObject)(null)).TransactionHeaders)).SyncRoot)).AH_TransactionReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((ARMatchingFilterBusinessObject)(null)).TransactionHeaders)).SyncRoot)).AH_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((ARMatchingFilterBusinessObject)(null)).TransactionHeaders)).SyncRoot)).AH_LocalExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((ARMatchingFilterBusinessObject)(null)).TransactionHeaders)).SyncRoot)).AH_OSTotalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((ARMatchingFilterBusinessObject)(null)).TransactionHeaders)).SyncRoot)).AH_Calc_OSOutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((ARMatchingFilterBusinessObject)(null)).TransactionHeaders)).SyncRoot)).AH_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((ARMatchingFilterBusinessObject)(null)).TransactionHeaders)).SyncRoot)).AH_Calc_LocalRXCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((ARMatchingFilterBusinessObject)(null)).TransactionHeaders)).SyncRoot)).AH_GB)));
			this.MatchTransactionsDisplayGrid.CaptionVisible = false;
			this.MatchTransactionsDisplayGrid.ColorContextKey = "MatchTranactions";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("MatchingFilterControl|22dece08-580d-490d-a3a8-b497c7849aef", "Organization");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AH_OH";
			zTextBoxColumnStyleInfo2.ColumnName = "AH_Ledger";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("MatchingFilterControl|19d7d635-8548-4aa2-8827-253d793c9e9b", "Type");
			zTextBoxColumnStyleInfo3.ColumnName = "AH_TransactionType";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("MatchingFilterControl|c534c6f5-de28-448f-918c-a3505a42a947", "Trans. No.");
			zTextBoxColumnStyleInfo4.ColumnName = "AH_TransactionNum";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("MatchingFilterControl|0cd5c84e-9fd1-4e24-834f-3e8909dd28cf", "Check Or Reference");
			zTextBoxColumnStyleInfo5.ColumnName = "AH_ChequeOrReference";
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("MatchingFilterControl|80e5d238-a471-49c9-9f0e-9dfcabb37464", "Trans. Date");
			zDateEditColumnStyleInfo2.ColumnName = "AH_InvoiceDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.ColumnName = "AH_DueDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("MatchingFilterControl|b7fe277b-dfee-4ec0-9d6c-b1bbc853d120", "Matched Date");
			zDateEditColumnStyleInfo4.ColumnName = "MatchedDate";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "AH_RX_NKTransactionCurrency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("MatchingFilterControl|93f45b01-027f-4769-b1ec-4cc74ef6b60d", "Match Amt");
			zCalcEditColumnStyleInfo1.ColumnName = "MatchedAmount";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("MatchingFilterControl|3f77b078-7987-467f-9475-3d89e13fa4a6", "Local Match Amt");
			zCalcEditColumnStyleInfo2.ColumnName = "LocalMatchedAmount";
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("MatchingFilterControl|a8dadaae-dd6c-4ff0-9f7f-56e3d430f295", "Exch. Rate", "Exchange Rate");
			zCalcEditColumnStyleInfo3.ColumnName = "AH_ExchangeRate";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("MatchingFilterControl|f1f85ddd-2a8c-4c4f-9649-f85cb9dfa702", "Job Inv. No.", "Job Invoice Number.");
			zTextBoxColumnStyleInfo6.ColumnName = "AH_ConsolidatedInvoiceRef";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("MatchingFilterControl|8cddb0d7-0501-4127-818c-0fff3b483030", "Govt Tax Invoice No.");
			zTextBoxColumnStyleInfo7.ColumnName = "AH_TransactionReference";
			zDateEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("MatchingFilterControl|a076b38d-8807-4a44-9933-cb655f3995ff", "Post Date");
			zDateEditColumnStyleInfo5.ColumnName = "AH_PostDate";
			zDateEditColumnStyleInfo5.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("MatchingFilterControl|5359a6ea-fbf6-44ab-bece-fc63381d8b6d", "Local Inv. Amt", "Local Invoice Amount.");
			zCalcEditColumnStyleInfo4.ColumnName = "AH_LocalExTaxAmount";
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("MatchingFilterControl|8cb61c21-63a8-4f4b-aafc-d3be0b8ddf56", "Inv. Amt", "Invoice Amount.");
			zCalcEditColumnStyleInfo5.ColumnName = "AH_OSTotalAmount";
			zCalcEditColumnStyleInfo5.IsVisible = false;
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("MatchingFilterControl|076e8055-dc88-4703-b39c-07a7f17fabe4", "Outstanding Amt");
			zCalcEditColumnStyleInfo6.ColumnName = "AH_Calc_OSOutstandingAmount";
			zCalcEditColumnStyleInfo6.IsVisible = false;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("MatchingFilterControl|8fa870d5-33ac-4de3-9108-f2aa9045006e", "Desc.", "Description");
			zTextBoxColumnStyleInfo8.ColumnName = "AH_Desc";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zCodeFindBoxColumnStyleInfo2.ColumnName = "AH_Calc_LocalRXCode";
			zCodeFindBoxColumnStyleInfo2.IsVisible = false;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("MatchingFilterControl|f973b0d0-6c77-43a1-b858-5e563c1cd30c", "Local Currency");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "AH_GB";
			zGuidFindBoxColumnStyleInfo2.IsVisible = false;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.MatchTransactionsDisplayGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.MatchTransactionsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MatchTransactionsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MatchTransactionsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MatchTransactionsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MatchTransactionsDisplayGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.MatchTransactionsDisplayGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.MatchTransactionsDisplayGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.MatchTransactionsDisplayGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.MatchTransactionsDisplayGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.MatchTransactionsDisplayGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.MatchTransactionsDisplayGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.MatchTransactionsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.MatchTransactionsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.MatchTransactionsDisplayGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.MatchTransactionsDisplayGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.MatchTransactionsDisplayGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.MatchTransactionsDisplayGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.MatchTransactionsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.MatchTransactionsDisplayGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.MatchTransactionsDisplayGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.MatchTransactionsDisplayGrid.GridId = "efce3ae4-f317-4f14-b3f8-aecd261c723a";
			this.MatchTransactionsDisplayGrid.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.MatchTransactionsDisplayGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MatchTransactionsDisplayGrid.IgnoreParentFilterControl = true;
			this.MatchTransactionsDisplayGrid.IsWholeRowSelectedOnClick = true;
			this.MatchTransactionsDisplayGrid.LayoutKey = "MatchTransactionsDisplayGrid";
			this.MatchTransactionsDisplayGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 336, true);
			this.MatchTransactionsDisplayGrid.Name = "MatchTransactionsDisplayGrid";
			this.MatchTransactionsDisplayGrid.ShouldSetErrorsOnTabPage = false;
			this.MatchTransactionsDisplayGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 192, true);
			this.MatchTransactionsDisplayGrid.TabIndex = 50;
			// 
			// GridSplitter
			// 
			this.GridSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.GridSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 328, true);
			this.GridSplitter.Name = "GridSplitter";
			this.GridSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 8, true);
			this.GridSplitter.TabIndex = 16;
			this.GridSplitter.TabStop = false;
			this.GridSplitter.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.GridSplitter_SplitterMoved);
			// 
			// MatchingFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GridSplitter);
			this.Controls.Add(this.MatchTransactionsDisplayGrid);
			this.Name = "MatchingFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 528, true);
			this.Controls.SetChildIndex(this.FilterStripsPanel, 0);
			this.Controls.SetChildIndex(this.AddStripButton, 0);
			this.Controls.SetChildIndex(this.ToolStripRecordsFoundLabel, 0);
			this.Controls.SetChildIndex(this.FilteredGrid, 0);
			this.Controls.SetChildIndex(this.MatchTransactionsDisplayGrid, 0);
			this.Controls.SetChildIndex(this.GridSplitter, 0);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MatchTransactionsDisplayGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

	}
}
