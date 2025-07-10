namespace Enterprise.Accounting.GUI.PayableOrder
{
	partial class AccPayableOrderLinesControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.orderTotalsControl = new Enterprise.Accounting.GUI.PayableOrder.OrderTotalsControl();
			this.OrderLinesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.orderTotalsControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrderLinesGrid)).BeginInit();
			this.OrderLinesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader);
			// 
			// orderTotalsControl
			// 
			this.orderTotalsControl.AllowDrop = true;
			this.orderTotalsControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.orderTotalsControl, ".");
			this.orderTotalsControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("69a1aec1-df17-48dd-a141-5983ba8d7f68", "Totals");
			this.orderTotalsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 44, true);
			this.orderTotalsControl.Name = "orderTotalsControl";
			this.orderTotalsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 26, true);
			this.orderTotalsControl.TabIndex = 6;
			// 
			// OrderLinesGrid
			// 
			this.OrderLinesGrid.AllowNavigation = false;
			this.OrderLinesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OrderLinesGrid, "OrderLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).OrderLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).OrderLines)).SyncRoot)).APL_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).OrderLines)).SyncRoot)).GenericCharge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).OrderLines)).SyncRoot)).APL_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).OrderLines)).SyncRoot)).APL_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).OrderLines)).SyncRoot)).APL_PartNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).OrderLines)).SyncRoot)).APL_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).OrderLines)).SyncRoot)).APL_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).OrderLines)).SyncRoot)).APL_InnerPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).OrderLines)).SyncRoot)).APL_InnerPacksUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).OrderLines)).SyncRoot)).APL_OuterPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).OrderLines)).SyncRoot)).APL_OuterPacksUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).OrderLines)).SyncRoot)).APL_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).OrderLines)).SyncRoot)).APL_F3_NKPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).OrderLines)).SyncRoot)).APL_ItemPrice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).OrderLines)).SyncRoot)).APL_LinePrice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).OrderLines)).SyncRoot)).APL_QtyReceived)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).OrderLines)).SyncRoot)).APL_QtyInvoiced)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).OrderLines)).SyncRoot)).APL_QuantityRemaining)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).OrderLines)).SyncRoot)).APL_GoodsReceivedNote)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).OrderLines)).SyncRoot)).APL_DeliveryDate)));
			this.OrderLinesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "APL_LineNo";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "GenericCharge";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "APL_GB";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "APL_GE";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "APL_PartNo";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "APL_Desc";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "APL_Status";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "APL_InnerPacks";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "APL_InnerPacksUQ";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "APL_OuterPacks";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.ColumnName = "APL_OuterPacksUQ";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "APL_Quantity";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.ColumnName = "APL_F3_NKPackType";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "APL_ItemPrice";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "APL_LinePrice";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "APL_QtyReceived";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "APL_QtyInvoiced";
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.ColumnName = "APL_QuantityRemaining";
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo1.ColumnName = "APL_GoodsReceivedNote";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "APL_DeliveryDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.OrderLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.OrderLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.OrderLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.OrderLinesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.OrderLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrderLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.OrderLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.OrderLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.OrderLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.OrderLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.OrderLinesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.OrderLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.OrderLinesGrid.CopySelectedRowsAllowed = true;
			this.OrderLinesGrid.GridId = "C650C6FC-20D8-4D66-ADCF-8ECD965AAD15";
			this.OrderLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrderLinesGrid.LayoutKey = "TransactionsGrid";
			this.OrderLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.OrderLinesGrid.Name = "OrderLinesGrid";
			this.OrderLinesGrid.ShouldSetErrorsOnTabPage = false;
			this.OrderLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(985, 37, true);
			this.OrderLinesGrid.TabIndex = 7;
			// 
			// AccPayableOrderLinesControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OrderLinesGrid);
			this.Controls.Add(this.orderTotalsControl);
			this.Name = "AccPayableOrderLinesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 69, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.orderTotalsControl.ResumeLayout(true);
			this.orderTotalsControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrderLinesGrid)).EndInit();
			this.OrderLinesGrid.ResumeLayout(false);
			this.OrderLinesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OrderTotalsControl orderTotalsControl;
		private ZArchitecture.ZGrid OrderLinesGrid;
	}
}
