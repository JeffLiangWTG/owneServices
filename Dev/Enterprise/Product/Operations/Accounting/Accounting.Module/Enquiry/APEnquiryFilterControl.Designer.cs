namespace Enterprise.Accounting.Module
{
	public partial class APEnquiryFilterControl
	{
		#region Windows Form Designer generated code

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("bc2ef86a-24ef-468a-a07b-da3358e47654", "Related Claim Status");
			zTextBoxColumnStyleInfo9.ColumnName = "RelatedClaimStatus";

			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("3418a637-fd3d-4e9c-a088-e80d2d83a8e2", "Query Number");
			zTextBoxColumnStyleInfo10.ColumnName = "QueryNumber";

			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(null)).AH_NotionalWHTTax)));
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfoNWHT = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			zCalcEditColumnStyleInfoNWHT.ColumnName = "AH_NotionalWHTTax";
			zCalcEditColumnStyleInfoNWHT.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(null)).AH_RealizedWHTTax)));
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfoRWHT = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			zCalcEditColumnStyleInfoRWHT.ColumnName = "AH_RealizedWHTTax";
			zCalcEditColumnStyleInfoRWHT.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			//
			// grid
			//
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfoNWHT);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfoRWHT);

			this.InvoiceAgeingGroupBox.SuspendLayout();
			this.ReadOnlyDetailsPanel.SuspendLayout();
			this.AverageDaysToFullyPayGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// ReadOnlyDetailsPanel
			//
			this.ReadOnlyDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 200, true);
			this.ReadOnlyDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 216, true);
			//
			// StdDaysFromDueCalcEdit
			//
			this.StdDaysFromDueCalcEdit.Visible = false;
			//
			// DisbursDaysFromDueCalcEdit
			//
			this.DisbursDaysFromDueCalcEdit.Visible = false;
			//
			// TermDaysLabel
			//

			//
			// LYRSalesLabel
			//
			this.LVRSalesCalcEdit.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("APEnquiryFilterControl|bdf4a448-b7da-4028-bb71-4c8adf71db99", "LYR Purchases");
			//
			// YTDPurchaseSalesLabel
			//
			this.YTDPurchaseSalesCalcEdit.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("APEnquiryFilterControl|f3c7a657-db02-459f-b847-7dafdf7735d1", "YTD Purchases");
			//
			// MTD_PTDSalesLabel
			//
			this.MTD_PTDSalesCalcEdit.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("APEnquiryFilterControl|fe8551f1-d429-4114-a8e0-7c47bd02d0b5", "MTD Purchases");
			//
			// FilteredGrid
			//
			this.FilteredGrid.ColorContextKey = "APEnquiryFilterControl";
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 58, true);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 358, true);
			//
			// APEnquiryFilterControl
			//
			this.Name = "APEnquiryFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 416, true);
			this.InvoiceAgeingGroupBox.ResumeLayout(false);
			this.InvoiceAgeingGroupBox.PerformLayout();
			this.ReadOnlyDetailsPanel.ResumeLayout(false);
			this.AverageDaysToFullyPayGroupBox.ResumeLayout(false);
			this.AverageDaysToFullyPayGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
