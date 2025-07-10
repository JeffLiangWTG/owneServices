namespace Enterprise.Accounting.Module
{
	public partial class DepositBatchFilterControl
	{
		#region Component Designer generated code

		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// FilteredGrid
			//
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("DepositBatchFilterControl|940ce6e5-d1d4-40fe-954c-72e14e64b866", "Deposit Date");
			zDateEditColumnStyleInfo1.ColumnName = "AH_InvoiceDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("DepositBatchFilterControl|8b483ae1-fad7-449d-a158-b18c0c413e09", "Deposit Post Date");
			zDateEditColumnStyleInfo2.ColumnName = "AH_PostDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("DepositBatchFilterControl|46ef01ae-de19-424c-861e-a89a2c523b06", "Batch Num.", "Batch Number.");
			zTextBoxColumnStyleInfo1.ColumnName = "AH_TransactionNum";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AH_AB";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("DepositBatchFilterControl|e5e7f3f8-7cb8-476e-8766-67ecc07dd4cd", "Currency");
			zTextBoxColumnStyleInfo2.ColumnName = "CurrencyCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("DepositBatchFilterControl|0a852af9-b93b-40d2-a1fe-152ae93bdf37", "Batch Total");
			zCalcEditColumnStyleInfo1.ColumnName = "AH_OSTotal";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("DepositBatchFilterControl|5c268f7a-44a5-4486-9cd6-8a61570ee21c", "Canceled", "The deposit batch was canceled.");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsCancelledCalc";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("DepositBatchFilterControl|2b7d6808-554b-4b6b-8731-f0daec9de121", "Created By");
			zTextBoxColumnStyleInfo3.ColumnName = "CreatingUser";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("DepositBatchFilterControl|e3610f6c-a2e9-47dd-b973-ac8d6c254969", "Created Date");
			zDateEditColumnStyleInfo3.ColumnName = "CreatedDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 57, true);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 351, true);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.CashBook.DepositBatch.DepositBatch);
			//
			// DepositBatchFilterControl
			//
			this.CaptionRenderingEnabled = true;
			this.Name = "DepositBatchFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 408, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
