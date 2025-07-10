namespace Enterprise.Accounting.Module
{
	public partial class AccCollectionBatchFilterControl
	{
		#region Component Designer generated code

		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			this.CollectionOrdersDisplayGrid = new ZArchitecture.ZGrid();
			this.GridSplitter = new CargoWise.Windows.UI.KSplitter();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CollectionOrdersDisplayGrid)).BeginInit();
			this.CollectionOrdersDisplayGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.grid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			zTextBoxColumnStyleInfo1.ColumnName = "ACB_BatchNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "ACB_Calc_RXDecimals";
			zCalcEditColumnStyleInfo1.ColumnName = "ACB_TotalAmount";
			zTextBoxColumnStyleInfo2.ColumnName = "ACB_RX_NKCurrency";
			zCheckBoxColumnStyleInfo1.ColumnName = "ACB_IsCancelled";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ACB_AB";
			zTextBoxColumnStyleInfo15.ColumnName = "ACB_Type";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 54, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 474, true);
			this.grid.TabIndex = 3;
			this.grid.AfterBind += new System.EventHandler(this.FilteredGrid_AfterBind);
			this.grid.CurrentCellChanged += new System.EventHandler(this.FilteredGrid_CurrentCellChanged);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(AccCollectionBatchFilterBusinessObject);
			// 
			// CollectionOrdersDisplayGrid
			// 
			this.CollectionOrdersDisplayGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CollectionOrdersDisplayGrid, "BatchCollectionOrders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((AccCollectionBatchFilterBusinessObject)(null)).BatchCollectionOrders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((AccCollectionBatchFilterBusinessObject)(null)).BatchCollectionOrders)).SyncRoot)).ACO_OrderNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((AccCollectionBatchFilterBusinessObject)(null)).BatchCollectionOrders)).SyncRoot)).ACO_OH_Debtor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((AccCollectionBatchFilterBusinessObject)(null)).BatchCollectionOrders)).SyncRoot)).ACO_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((AccCollectionBatchFilterBusinessObject)(null)).BatchCollectionOrders)).SyncRoot)).ACO_CollectionDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((AccCollectionBatchFilterBusinessObject)(null)).BatchCollectionOrders)).SyncRoot)).ACO_DepositedDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((AccCollectionBatchFilterBusinessObject)(null)).BatchCollectionOrders)).SyncRoot)).ACO_IsCancelled)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((AccCollectionBatchFilterBusinessObject)(null)).BatchCollectionOrders)).SyncRoot)).ACO_CancelledReason)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((AccCollectionBatchFilterBusinessObject)(null)).BatchCollectionOrders)).SyncRoot)).CollectionRequestBankName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((AccCollectionBatchFilterBusinessObject)(null)).BatchCollectionOrders)).SyncRoot)).CollectionRequestBankBsb)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((AccCollectionBatchFilterBusinessObject)(null)).BatchCollectionOrders)).SyncRoot)).CollectionRequestBankCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((AccCollectionBatchFilterBusinessObject)(null)).BatchCollectionOrders)).SyncRoot)).CollectionRequestBankSwift)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((AccCollectionBatchFilterBusinessObject)(null)).BatchCollectionOrders)).SyncRoot)).CollectionRequestAccountName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((AccCollectionBatchFilterBusinessObject)(null)).BatchCollectionOrders)).SyncRoot)).CollectionRequestAccountNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((AccCollectionBatchFilterBusinessObject)(null)).BatchCollectionOrders)).SyncRoot)).CollectionRequestAccountCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((AccCollectionBatchFilterBusinessObject)(null)).BatchCollectionOrders)).SyncRoot)).CollectionRequestIBANNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Riba.AccCollectionOrder)(((System.Collections.IList)(((AccCollectionBatchFilterBusinessObject)(null)).BatchCollectionOrders)).SyncRoot)).ACO_Amount)));
			this.CollectionOrdersDisplayGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.ColumnName = "ACO_OrderNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "ACO_OH_Debtor";
			zTextBoxColumnStyleInfo4.ColumnName = "ACO_RX_NKCurrency";
			zTextBoxColumnStyleInfo5.ColumnName = "ACO_CollectionDate";
			zTextBoxColumnStyleInfo16.ColumnName = "ACO_DepositedDate";
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("3e5641e3-c1f6-42f8-9863-eda74abbd2b7", "Is Rejected");
			zCheckBoxColumnStyleInfo2.ColumnName = "ACO_IsCancelled";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("80d9b223-002f-4536-8f51-2ea6ad823410", "Rejected Reason");
			zTextBoxColumnStyleInfo6.ColumnName = "ACO_CancelledReason";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("4c447611-96a7-4e1c-80ea-46c2f6a1b373", "Bank Name");
			zTextBoxColumnStyleInfo7.ColumnName = "CollectionRequestBankName";
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("8730c27e-2b45-4871-92b1-d1399a970755", "Bank/Branch Code");
			zTextBoxColumnStyleInfo8.ColumnName = "CollectionRequestBankBsb";
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("7194acfa-c836-4fd1-bbee-39d5ac4ad295", "Bank Country/Region");
			zTextBoxColumnStyleInfo9.ColumnName = "CollectionRequestBankCountry";
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ccb6a63c-b048-417b-8f49-cdd51bb96d6a", "Bank Swift");
			zTextBoxColumnStyleInfo10.ColumnName = "CollectionRequestBankSwift";
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ce55b8ff-94e3-40c8-a765-f566b44ca2fa", "Account Name");
			zTextBoxColumnStyleInfo11.ColumnName = "CollectionRequestAccountName";
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("d2814f80-5f8f-4a6d-b312-8ccee091b57a", "Account Num.");
			zTextBoxColumnStyleInfo12.ColumnName = "CollectionRequestAccountNumber";
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("f1054b94-f4e6-4a85-8f2d-b925e96e1b1d", "Account Currency");
			zTextBoxColumnStyleInfo13.ColumnName = "CollectionRequestAccountCurrency";
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("4119a9df-5568-4384-9b64-b7885be3ad28", "IBAN Num.");
			zTextBoxColumnStyleInfo14.ColumnName = "CollectionRequestIBANNumber";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "ACO_Amount";
			this.CollectionOrdersDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CollectionOrdersDisplayGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.CollectionOrdersDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.CollectionOrdersDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.CollectionOrdersDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.CollectionOrdersDisplayGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.CollectionOrdersDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.CollectionOrdersDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.CollectionOrdersDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.CollectionOrdersDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.CollectionOrdersDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.CollectionOrdersDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.CollectionOrdersDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.CollectionOrdersDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.CollectionOrdersDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.CollectionOrdersDisplayGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.CollectionOrdersDisplayGrid.CopySelectedRowsAllowed = true;
			this.CollectionOrdersDisplayGrid.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.CollectionOrdersDisplayGrid.GridId = "6ac807a3-1d27-4ca3-bbdc-0e0238d2d1e3";
			this.CollectionOrdersDisplayGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CollectionOrdersDisplayGrid.LayoutKey = "CollectionOrdersDisplayGrid";
			this.CollectionOrdersDisplayGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 333, true);
			this.CollectionOrdersDisplayGrid.Name = "CollectionOrdersDisplayGrid";
			this.CollectionOrdersDisplayGrid.ReadOnly = true;
			this.CollectionOrdersDisplayGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 195, true);
			this.CollectionOrdersDisplayGrid.TabIndex = 17;
			// 
			// GridSplitter
			// 
			this.GridSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.GridSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 325, true);
			this.GridSplitter.Name = "GridSplitter";
			this.GridSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 8, true);
			this.GridSplitter.TabIndex = 16;
			this.GridSplitter.TabStop = false;
			this.GridSplitter.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.GridSplitter_SplitterMoved);
			// 
			// AccCollectionBatchFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GridSplitter);
			this.Controls.Add(this.CollectionOrdersDisplayGrid);
			this.Name = "AccCollectionBatchFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 528, true);
			this.Controls.SetChildIndex(this.FilterStripsPanel, 0);
			this.Controls.SetChildIndex(this.AddStripButton, 0);
			this.Controls.SetChildIndex(this.ToolStripRecordsFoundLabel, 0);
			this.Controls.SetChildIndex(this.grid, 0);
			this.Controls.SetChildIndex(this.CollectionOrdersDisplayGrid, 0);
			this.Controls.SetChildIndex(this.GridSplitter, 0);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CollectionOrdersDisplayGrid)).EndInit();
			this.CollectionOrdersDisplayGrid.ResumeLayout(false);
			this.CollectionOrdersDisplayGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal CargoWise.Windows.UI.KSplitter GridSplitter;
		private ZArchitecture.ZGrid CollectionOrdersDisplayGrid;
	}
}
