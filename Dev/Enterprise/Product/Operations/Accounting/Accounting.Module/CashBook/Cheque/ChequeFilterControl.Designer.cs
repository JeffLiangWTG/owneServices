namespace Enterprise.Accounting.Module
{
	public partial class ChequeFilterControl
	{
		#region Component Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBox1 = new ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBox2 = new ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBox3 = new ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeFilterControl|f729552b-677a-4928-adeb-063fc79256d9", "Cheque Number");
			zTextBoxColumnStyleInfo1.ColumnName = "RCH_ChequeNumber";
			zTextBoxColumnStyleInfo1.IsVisible = true;
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeFilterControl|e3d67cf3-df37-43e8-9b24-e028b63ded06", "Cheque Portfolio #");
			zTextBoxColumnStyleInfo12.ColumnName = "RCH_ChequeReference";
			zTextBoxColumnStyleInfo12.IsVisible = true;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeFilterControl|cf695a31-282b-4ba7-b537-ac9973a1022c", "Due Date");
			zDateEditColumnStyleInfo1.ColumnName = "RCH_DueDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsVisible = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeFilterControl|7ba456ef-1703-460e-9f09-bc4b2202c8fa", "Amount");
			zTextBoxColumnStyleInfo2.ColumnName = "RCH_Amount";
			zTextBoxColumnStyleInfo2.IsVisible = true;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeFilterControl|5eabef97-6e05-4bb4-9b46-a71ea5be9e68", "Currency");
			zTextBoxColumnStyleInfo3.ColumnName = "RCH_RX_NKChequeCurrency";
			zTextBoxColumnStyleInfo3.IsVisible = true;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeFilterControl|169233e0-7693-4a71-88ea-98382e85d0ff", "Status");
			zTextBoxColumnStyleInfo4.ColumnName = "RCH_Status";
			zTextBoxColumnStyleInfo4.IsVisible = true;
			zGuidFindBox1.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeFilterControl|e0040ba1-04cb-4d30-9faa-309378057b41", "Cheque Drawer");
			zGuidFindBox1.ColumnName = "RCH_OH_Drawer";
			zGuidFindBox1.IsVisible = true;
			zGuidFindBox2.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeFilterControl|0ba3df41-9041-4fad-b863-880a294d1a80", "Received Organization");
			zGuidFindBox2.ColumnName = "RCH_OH_ReceivedFrom";
			zGuidFindBox2.IsVisible = true;
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeFilterControl|98c0cad8-7f63-46c0-a6d2-9bda1fb2b85d", "Received Organization Full Name");
			zTextBoxColumnStyleInfo7.ColumnName = "ReceivedFrom+OH_FullName";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zGuidFindBox3.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeFilterControl|4dcc9c0c-75dd-4b9f-8821-b3ebff0e56b8", "Given Organization");
			zGuidFindBox3.ColumnName = "RCH_OH_GivenTo";
			zGuidFindBox3.IsVisible = true;
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeFilterControl|32f1156c-b2e7-45a9-b9c2-2e961e2bb1da", "Given Organization Full Name");
			zTextBoxColumnStyleInfo9.ColumnName = "GivenTo+OH_FullName";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeFilterControl|ec509d47-2df6-4e5a-b4e7-11a9ba08ccd6", "Bank/Branch Name of Cheque");
			zTextBoxColumnStyleInfo10.ColumnName = "RCH_BankName";
			zTextBoxColumnStyleInfo10.IsVisible = true;
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeFilterControl|dfb9d45a-5b9d-49b9-b506-258821d285fa", "Bank/Branch Name of Cheque");
			zTextBoxColumnStyleInfo10.ColumnName = "RCH_BankName";
			zTextBoxColumnStyleInfo10.IsVisible = true;
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeFilterControl|ab86487e-1207-4775-a998-28f6c3caf4ca", "Given Bank Account");
			zTextBoxColumnStyleInfo11.ColumnName = "GivenBankAccount+AB_BankAccountName";
			zTextBoxColumnStyleInfo11.IsVisible = true;

			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBox1);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBox2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBox3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 57, true);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 351, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(MasterFiles.Business.AccReceivedCheque);
			// 
			// ChequeFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "ChequeFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 408, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
