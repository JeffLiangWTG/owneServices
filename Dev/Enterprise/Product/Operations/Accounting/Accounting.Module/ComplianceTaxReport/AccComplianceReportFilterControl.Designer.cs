namespace Enterprise.Accounting.Module
{
	public partial class AccComplianceReportFilterControl
	{
		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxACR_ReportType_ColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxACR_Description_ColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxACR_GB_Branch_ColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxACR_Status_ColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxACR_Periodicity_ColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditACR_DateFrom_ColumnStyleInfo = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditACR_DateTo_ColumnStyleInfo = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditACR_PageNumberFrom_ColumnStyleInfo = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditACR_PageNumberTo_ColumnStyleInfo = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxACR_IsFinalised_ColumnStyleInfo = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxReportTypeDescription_ColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditAccountingPeriod_ColumnStyleInfo = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxReportStatusDescription_ColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxPeriodicityDescriptionColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxACR_StatusMessage_ColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// grid
			//
			zTextBoxACR_ReportType_ColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("c07111ef-78bc-4b59-83ac-0b7797ab0153", "Type", "Type", "Report Type");
			zTextBoxACR_ReportType_ColumnStyleInfo.ColumnName = "ACR_ReportType";
			zTextBoxACR_ReportType_ColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxACR_Description_ColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("82a584c9-5e1d-42e8-9904-0c849f6abf82", "Desc.", "Description", "Report Description");
			zTextBoxACR_Description_ColumnStyleInfo.ColumnName = "ACR_Description";
			zTextBoxACR_Description_ColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zGuidFindBoxACR_GB_Branch_ColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("19920bfc-6529-4ade-ad50-78152de8a2e8", "Branch");
			zGuidFindBoxACR_GB_Branch_ColumnStyleInfo.ColumnName = "ACR_GB_Branch";
			zGuidFindBoxACR_GB_Branch_ColumnStyleInfo.IsVisible = false;
			zGuidFindBoxACR_GB_Branch_ColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxACR_Status_ColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("3bd7ac66-b814-4cfa-aa16-1595182c86c0", "Status", "Report Status Code");
			zTextBoxACR_Status_ColumnStyleInfo.ColumnName = "ACR_Status";
			zTextBoxACR_Status_ColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxACR_Periodicity_ColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("6176006d-adae-4cf9-bb21-13d7a87e6712", "Periodicity", "Periodicity", "Periodicity Code");
			zTextBoxACR_Periodicity_ColumnStyleInfo.ColumnName = "ACR_Periodicity";
			zTextBoxACR_Periodicity_ColumnStyleInfo.IsVisible = false;
			zTextBoxACR_Periodicity_ColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxACR_StatusMessage_ColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("76ef2db7-9200-45ff-a33f-9a7611ab81bc", "Status Message", "Status Message", "");
			zTextBoxACR_StatusMessage_ColumnStyleInfo.ColumnName = "ACR_StatusMessage";
			zTextBoxACR_StatusMessage_ColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditACR_DateFrom_ColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("37e3c616-2b6c-4549-bd32-db34de2c750d", "Post From", "Post From", "Post From Date");
			zDateEditACR_DateFrom_ColumnStyleInfo.ColumnName = "ACR_DateFrom";
			zDateEditACR_DateFrom_ColumnStyleInfo.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditACR_DateFrom_ColumnStyleInfo.GroupName = Enterprise.Accounting.Module.Res.GetData("ba5d9ad6-f22d-4abf-9664-84db712ec639", "Report Dates");
			zDateEditACR_DateFrom_ColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditACR_DateTo_ColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("5a946bd2-8919-4726-89fc-ce14380ae266", "Post To", "Post To", "Post To Date");
			zDateEditACR_DateTo_ColumnStyleInfo.ColumnName = "ACR_DateTo";
			zDateEditACR_DateTo_ColumnStyleInfo.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditACR_DateTo_ColumnStyleInfo.GroupName = Enterprise.Accounting.Module.Res.GetData("ba5d9ad6-f22d-4abf-9664-84db712ec639", "Report Dates");
			zDateEditACR_DateTo_ColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditACR_PageNumberFrom_ColumnStyleInfo.BindToDecimalPlaces = null;
			zCalcEditACR_PageNumberFrom_ColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("4a23e564-60c5-4c72-97a4-cb30cd341469", "Page # From");
			zCalcEditACR_PageNumberFrom_ColumnStyleInfo.ColumnName = "ACR_PageNumberFrom";
			zCalcEditACR_PageNumberFrom_ColumnStyleInfo.Decimals = 0;
			zCalcEditACR_PageNumberFrom_ColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditACR_PageNumberFrom_ColumnStyleInfo.IsVisible = false;
			zCalcEditACR_PageNumberTo_ColumnStyleInfo.BindToDecimalPlaces = null;
			zCalcEditACR_PageNumberTo_ColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("63f451a1-c54a-4b10-b509-5804699cc38d", "Page # To");
			zCalcEditACR_PageNumberTo_ColumnStyleInfo.ColumnName = "ACR_PageNumberTo";
			zCalcEditACR_PageNumberTo_ColumnStyleInfo.Decimals = 0;
			zCalcEditACR_PageNumberTo_ColumnStyleInfo.IsVisible = false;
			zCalcEditACR_PageNumberTo_ColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxACR_IsFinalised_ColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("378ca063-9064-4db7-8eb4-9d781a6d7e8c", "Finalized");
			zCheckBoxACR_IsFinalised_ColumnStyleInfo.ColumnName = "ACR_IsFinalised";
			zCheckBoxACR_IsFinalised_ColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(64);
			zTextBoxReportTypeDescription_ColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("fcd9db35-e9d4-49d6-8fee-036e6ff5be9f", "Type Desc.", "Type Description", "Report Type Description");
			zTextBoxReportTypeDescription_ColumnStyleInfo.ColumnName = "ReportTypeDescription";
			zTextBoxReportTypeDescription_ColumnStyleInfo.IsVisible = false;
			zTextBoxReportTypeDescription_ColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditAccountingPeriod_ColumnStyleInfo.BindToDecimalPlaces = null;
			zCalcEditAccountingPeriod_ColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("5a7492db-443e-42fd-838a-734b288b2ec9", "Period");
			zCalcEditAccountingPeriod_ColumnStyleInfo.ColumnName = "AccountingPeriod";
			zCalcEditAccountingPeriod_ColumnStyleInfo.Decimals = 0;
			zCalcEditAccountingPeriod_ColumnStyleInfo.IsVisible = false;
			zCalcEditAccountingPeriod_ColumnStyleInfo.ShowGroupSeparators = false;
			zCalcEditAccountingPeriod_ColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxReportStatusDescription_ColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("0b5a7ee3-2c02-4aa5-a8ed-0d37b37cd52b", "Status Desc.", "Status Description", "Report Status Description");
			zTextBoxReportStatusDescription_ColumnStyleInfo.ColumnName = "ReportStatusDescription";
			zTextBoxReportStatusDescription_ColumnStyleInfo.IsVisible = false;
			zTextBoxReportStatusDescription_ColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxPeriodicityDescriptionColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("9feab3b9-1ee5-459e-8f66-e004e8a5d41b", "Periodicity Desc.", "Periodicity Description", "");
			zTextBoxPeriodicityDescriptionColumnStyleInfo.ColumnName = "PeriodicityDescription";
			zTextBoxPeriodicityDescriptionColumnStyleInfo.IsVisible = false;
			zTextBoxPeriodicityDescriptionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.grid.ColumnStyles.Add(zTextBoxACR_ReportType_ColumnStyleInfo);
			this.grid.ColumnStyles.Add(zTextBoxACR_Description_ColumnStyleInfo);
			this.grid.ColumnStyles.Add(zGuidFindBoxACR_GB_Branch_ColumnStyleInfo);
			this.grid.ColumnStyles.Add(zTextBoxACR_Status_ColumnStyleInfo);
			this.grid.ColumnStyles.Add(zTextBoxACR_Periodicity_ColumnStyleInfo);
			this.grid.ColumnStyles.Add(zDateEditACR_DateFrom_ColumnStyleInfo);
			this.grid.ColumnStyles.Add(zDateEditACR_DateTo_ColumnStyleInfo);
			this.grid.ColumnStyles.Add(zCalcEditACR_PageNumberFrom_ColumnStyleInfo);
			this.grid.ColumnStyles.Add(zCalcEditACR_PageNumberTo_ColumnStyleInfo);
			this.grid.ColumnStyles.Add(zCheckBoxACR_IsFinalised_ColumnStyleInfo);
			this.grid.ColumnStyles.Add(zTextBoxReportTypeDescription_ColumnStyleInfo);
			this.grid.ColumnStyles.Add(zCalcEditAccountingPeriod_ColumnStyleInfo);
			this.grid.ColumnStyles.Add(zTextBoxReportStatusDescription_ColumnStyleInfo);
			this.grid.ColumnStyles.Add(zTextBoxPeriodicityDescriptionColumnStyleInfo);
			this.grid.ColumnStyles.Add(zTextBoxACR_StatusMessage_ColumnStyleInfo);
			this.grid.DataSource = this.BindingSource;
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 169, true);
			this.grid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(516, 303, true);
			this.grid.TabIndex = 13;
			//
			// AddStripButton
			//
			this.AddStripButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 28, true);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport);
			//
			// AccComplianceReportFilterControl
			//
			this.CaptionRenderingEnabled = true;
			this.Name = "AccComplianceReportFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(516, 472, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
