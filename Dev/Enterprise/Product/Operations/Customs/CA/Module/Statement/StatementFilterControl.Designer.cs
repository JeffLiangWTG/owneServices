using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CA.Module
{
	public partial class StatementFilterControl
	{
		/// <summary>
		/// Required method for Designer support - do not modifyB
		/// the contents of this method with the code editor.
		/// </summary>
		protected virtual void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new ZArchitecture.ZCalcEditColumnStyleInfo();

			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementHeader)(null)).ARLMessageType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementHeader)(null)).B2_StatementNumber);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementHeader)(null)).B2_ImporterCustomsID);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementHeader)(null)).B2_OH_Importer);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementHeader)(null)).B2_EntryFilerCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementHeader)(null)).B2_StatementType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementHeader)(null)).B2_ProcessDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementHeader)(null)).B2_DueDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementHeader)(null)).B2_PrintDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementHeader)(null)).B2_PaidAmount);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusStatementHeader)(null)).B2_RefundAmount);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("5b009909-db7a-490d-a79e-e508033eba72", "Message Type");
			zTextBoxColumnStyleInfo1.ColumnName = "ARLMessageType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("5939ed01-d313-433e-a7d1-726a9d4d5a02", "Statement Number");
			zTextBoxColumnStyleInfo2.ColumnName = "B2_StatementNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("61ad3880-d2e5-45e1-9f4e-8e5cb83d962e", "Importer");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "B2_OH_Importer";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("09177329-ce2f-4be6-9e66-adbc4d11b541", "Importer Business Number");
			zTextBoxColumnStyleInfo3.ColumnName = "B2_ImporterCustomsID";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("e2f55b31-82e9-4a6e-8503-bc1226d7dcad", "Account Security Code");
			zTextBoxColumnStyleInfo4.ColumnName = "B2_EntryFilerCode";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("ea2a9ed8-57ce-44ef-aa0e-5d0156a65475", "Statement Type");
			zTextBoxColumnStyleInfo5.ColumnName = "B2_StatementType";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("db1582e8-c417-48c6-90d7-86d5e76f5462", "Accounting Date");
			zDateEditColumnStyleInfo1.ColumnName = "B2_ProcessDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("71adf188-305c-4816-b921-b73093ab06f9", "Payment Due Date");
			zDateEditColumnStyleInfo2.ColumnName = "B2_DueDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("ffd8a156-6cd3-4905-a960-59b2b11864d9", "Statement Date");
			zDateEditColumnStyleInfo3.ColumnName = "B2_PrintDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("06feddf1-5c10-4a7d-baee-cc2311009110", "Total Payment Received");
			zCalcEditColumnStyleInfo1.ColumnName = "B2_PaidAmount";
			zCalcEditColumnStyleInfo1.Decimals = 2;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("791563df-e1ed-4718-b3b5-b4229550f028", "Disbursements");
			zCalcEditColumnStyleInfo2.ColumnName = "B2_RefundAmount";
			zCalcEditColumnStyleInfo2.Decimals = 2;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("105210f8-5023-429e-853e-5c971edd3c54", "Total Due");
			zCalcEditColumnStyleInfo3.ColumnName = "B2_StatementAmount";
			zCalcEditColumnStyleInfo3.Decimals = 2;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("17bc0f0d-2c6f-4220-95f4-9953c38b5187", "Duties");
			zCalcEditColumnStyleInfo4.ColumnName = "B2_TotalCustomsDuties";
			zCalcEditColumnStyleInfo4.Decimals = 2;
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("82b19532-93a5-497c-b3b9-e6c695fe2390", "SIMA");
			zCalcEditColumnStyleInfo5.ColumnName = "B2_TotalSIMA";
			zCalcEditColumnStyleInfo5.Decimals = 2;
			zCalcEditColumnStyleInfo5.IsVisible = false;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("39e4cfd6-15fc-4beb-863d-49f103f37339", "Excise Tax");
			zCalcEditColumnStyleInfo6.ColumnName = "B2_TotalExciseTax";
			zCalcEditColumnStyleInfo6.Decimals = 2;
			zCalcEditColumnStyleInfo6.IsVisible = false;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("15175b33-eb19-427b-83a4-b1b9eb94b47d", "GST/PST/HST");
			zCalcEditColumnStyleInfo7.ColumnName = "B2_TotalGST";
			zCalcEditColumnStyleInfo7.Decimals = 2;
			zCalcEditColumnStyleInfo7.IsVisible = false;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("72084f3a-974f-49ef-8a60-a4cb44087034", "Others");
			zCalcEditColumnStyleInfo8.ColumnName = "B2_TotalOthers";
			zCalcEditColumnStyleInfo8.Decimals = 2;
			zCalcEditColumnStyleInfo8.IsVisible = false;
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("F408741A-D183-4674-90AE-FDF6C6D0D1B3", "Interests");
			zCalcEditColumnStyleInfo9.ColumnName = "B2_TotalInterests";
			zCalcEditColumnStyleInfo9.Decimals = 2;
			zCalcEditColumnStyleInfo9.IsVisible = false;
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CusStatementHeader);
			// 
			// StatementFilterControl
			//
			this.CaptionRenderingEnabled = true;
			this.Name = "StatementFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
