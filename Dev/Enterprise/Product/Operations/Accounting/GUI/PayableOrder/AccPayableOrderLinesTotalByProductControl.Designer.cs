namespace Enterprise.Accounting.GUI.PayableOrder
{
	partial class AccPayableOrderLinesTotalByProductControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.orderTotalsControl = new Enterprise.Accounting.GUI.PayableOrder.OrderTotalsControl();
			this.ProductSummaryLinesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.orderTotalsControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProductSummaryLinesGrid)).BeginInit();
			this.ProductSummaryLinesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader);
			// 
			// orderTotalsControl
			// 
			this.orderTotalsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.orderTotalsControl, ".");
			this.orderTotalsControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("69a1aec1-df17-48dd-a141-5983ba8d7f68", "Totals");
			this.orderTotalsControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.orderTotalsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 223, true);
			this.orderTotalsControl.Name = "orderTotalsControl";
			this.orderTotalsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1047, 26, true);
			this.orderTotalsControl.TabIndex = 6;
			// 
			// ProductSummaryLinesGrid
			// 
			this.ProductSummaryLinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ProductSummaryLinesGrid, "ProductQuantitySummary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).ProductQuantitySummary)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLinesTotalByProduct)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).ProductQuantitySummary)).SyncRoot)).Product)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLinesTotalByProduct)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).ProductQuantitySummary)).SyncRoot)).ProductDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLinesTotalByProduct)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).ProductQuantitySummary)).SyncRoot)).Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLinesTotalByProduct)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).ProductQuantitySummary)).SyncRoot)).QuantityInvoiced)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLinesTotalByProduct)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).ProductQuantitySummary)).SyncRoot)).QuantityReceived)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLinesTotalByProduct)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).ProductQuantitySummary)).SyncRoot)).QuantityRemaining)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLinesTotalByProduct)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).ProductQuantitySummary)).SyncRoot)).LinePrice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLinesTotalByProduct)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).ProductQuantitySummary)).SyncRoot)).InnerPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderLinesTotalByProduct)(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).ProductQuantitySummary)).SyncRoot)).OuterPacks)));
			this.ProductSummaryLinesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Product";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "ProductDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Quantity";
			zCalcEditColumnStyleInfo1.Decimals = 5;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "QuantityInvoiced";
			zCalcEditColumnStyleInfo2.Decimals = 5;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "QuantityReceived";
			zCalcEditColumnStyleInfo3.Decimals = 5;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "QuantityRemaining";
			zCalcEditColumnStyleInfo4.Decimals = 5;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "LinePrice";
			zCalcEditColumnStyleInfo5.Decimals = 5;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3740c7e1-ed57-46e6-8d71-dae667ac7c11", "Inner Packs");
			zCalcEditColumnStyleInfo6.ColumnName = "InnerPacks";
			zCalcEditColumnStyleInfo6.IsVisible = false;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2df77f7a-812d-432c-b82b-776e16da509a", "Outer Packs");
			zCalcEditColumnStyleInfo7.ColumnName = "OuterPacks";
			zCalcEditColumnStyleInfo7.IsVisible = false;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ProductSummaryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ProductSummaryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ProductSummaryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ProductSummaryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ProductSummaryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ProductSummaryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ProductSummaryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.ProductSummaryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.ProductSummaryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.ProductSummaryLinesGrid.CopySelectedRowsAllowed = false;
			this.ProductSummaryLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProductSummaryLinesGrid.GridId = "13728054-BC4A-4117-A685-93395EBE1A6B";
			this.ProductSummaryLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProductSummaryLinesGrid.LayoutKey = "TransactionsGrid";
			this.ProductSummaryLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProductSummaryLinesGrid.Name = "ProductSummaryLinesGrid";
			this.ProductSummaryLinesGrid.ShouldSetErrorsOnTabPage = false;
			this.ProductSummaryLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1047, 223, true);
			this.ProductSummaryLinesGrid.TabIndex = 7;
			// 
			// AccPayableOrderLinesTotalByProductControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ProductSummaryLinesGrid);
			this.Controls.Add(this.orderTotalsControl);
			this.Name = "AccPayableOrderLinesTotalByProductControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1047, 249, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.orderTotalsControl.ResumeLayout(true);
			this.orderTotalsControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProductSummaryLinesGrid)).EndInit();
			this.ProductSummaryLinesGrid.ResumeLayout(false);
			this.ProductSummaryLinesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OrderTotalsControl orderTotalsControl;
		private ZArchitecture.ZGrid ProductSummaryLinesGrid;
	}
}
