using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class BulkDSBJobCloseBatchApprovalUserControl
	{


		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo13 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo14 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo15 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo16 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo17 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			this.JobGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(JobGrid)).BeginInit();
			BindingSource.DataSourceType = typeof(Business.JobInvoicing.DsbJobCloseBatch);
			BindingSource.SetBindingMember(this.JobGrid, "Jobs");
			((System.ComponentModel.ISupportInitialize)(JobGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(BindingSource)).EndInit();
			this.JobGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// JobGrid
			// 
			this.JobGrid.AllowNavigation = false;
			this.JobGrid.CaptionVisible = false;
			this.JobGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JobGrid.GridId = "1fb73e1f-df6f-4c8e-a081-3e7efb975a60";
			this.JobGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobGrid.LayoutKey = "JobGrid";
			this.JobGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobGrid.Name = "JobGrid";
			this.JobGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 150, true);
			this.JobGrid.TabIndex = 0;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalUserControl|5ebb22c9-e503-4abc-84f1-a35d0c233a39", "Revenue");
			zCalcEditColumnStyleInfo1.ColumnName = "TotalRevenue";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalUserControl|6433edc1-e5c6-497b-b402-952f3549e9eb", "WIP");
			zCalcEditColumnStyleInfo2.ColumnName = "TotalWIP";
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalUserControl|71986a81-1642-4d50-9e12-915f13237992", "Cost");
			zCalcEditColumnStyleInfo3.ColumnName = "TotalCost";
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalUserControl|8c82147b-2543-4ce3-9dde-6c52307c3d46", "Accrual");
			zCalcEditColumnStyleInfo4.ColumnName = "TotalAccrual";
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalUserControl|99fb30c8-5d19-45ac-a6fb-9a915104173a", "Profit/Loss");
			zCalcEditColumnStyleInfo5.ColumnName = "TotalLineAmount";
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalUserControl|1c1a868a-8a10-421f-878f-2905a5cfcbdd", "Margin%");
			zCalcEditColumnStyleInfo6.ColumnName = "TotalMargin";
			zTextBoxColumnStyleInfo1.ColumnName = "JH_ProfitLossReasonCode";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalUserControl|27ecc277-9b66-4938-902d-d85bb7125439", "REV Recognized");
			zCalcEditColumnStyleInfo7.ColumnName = "RevRecognized";
			zCalcEditColumnStyleInfo7.IsVisible = false;
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalUserControl|31092121-a01c-4e44-8b89-41e539a9333d", "REV Not Recognized");
			zCalcEditColumnStyleInfo8.ColumnName = "RevNotRecognized";
			zCalcEditColumnStyleInfo8.IsVisible = false;
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalUserControl|4d507aa5-2dd2-4b87-8eb1-094c69d5d3bb", "CST Recognized");
			zCalcEditColumnStyleInfo9.ColumnName = "CstRecognized";
			zCalcEditColumnStyleInfo9.IsVisible = false;
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalUserControl|57108a01-4305-491e-af90-9511d383276b", "CST Not Recognized");
			zCalcEditColumnStyleInfo10.ColumnName = "CstNotRecognized";
			zCalcEditColumnStyleInfo10.IsVisible = false;
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalUserControl|64c1ad1c-a610-4f63-84b2-797b8bf3d0df", "WIP Recognized");
			zCalcEditColumnStyleInfo11.ColumnName = "WipRecognized";
			zCalcEditColumnStyleInfo11.IsVisible = false;
			zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalUserControl|70e1249a-fddf-44c3-b282-161147ddbda6", "WIP Not Recognized");
			zCalcEditColumnStyleInfo12.ColumnName = "WipNotRecognized";
			zCalcEditColumnStyleInfo12.IsVisible = false;
			zCalcEditColumnStyleInfo13.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalUserControl|715f6563-6d99-4dc1-b4f4-b60c1adc2afd", "ACR Recognized");
			zCalcEditColumnStyleInfo13.ColumnName = "AcrRecognized";
			zCalcEditColumnStyleInfo13.IsVisible = false;
			zCalcEditColumnStyleInfo14.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalUserControl|893bf083-c52f-4bd3-8e43-6aec11ad6a25", "ACR Not Recognized");
			zCalcEditColumnStyleInfo14.ColumnName = "AcrNotRecognized";
			zCalcEditColumnStyleInfo14.IsVisible = false;
			zCalcEditColumnStyleInfo15.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo15.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalUserControl|9a027032-502d-49bf-9b04-10bf95c9a8a0", "Profit/Loss Recognized");
			zCalcEditColumnStyleInfo15.ColumnName = "ProfitLossRecognized";
			zCalcEditColumnStyleInfo15.IsVisible = false;
			zCalcEditColumnStyleInfo16.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo16.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalUserControl|10147276-ecd3-426d-a536-24b5fba4d1be", "Profit/Loss Not Recognized");
			zCalcEditColumnStyleInfo16.ColumnName = "ProfitLossNotRecognized";
			zCalcEditColumnStyleInfo16.IsVisible = false;
			zTextBoxColumnStyleInfo2.ColumnName = "JH_JobNum";
			zTextBoxColumnStyleInfo3.ColumnName = "JH_Status";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JH_GB";
			zGuidFindBoxColumnStyleInfo2.ColumnName = "JH_GE";
			zDateEditColumnStyleInfo1.ColumnName = "JH_A_JOP";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.ColumnName = "JH_A_JCL";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JH_GS_NKRepOps";
			zCodeFindBoxColumnStyleInfo2.ColumnName = "JH_GS_NKRepSales";
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalUserControl|15520eb3-7c6f-4f64-a5b7-6d4dd2c6a3d0", "Local Charges");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "LocalChargesPK";
			zGuidFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalUserControl|1bbbf931-a73f-4668-a57c-4009dd580ba0", "Overseas Agent");
			zGuidFindBoxColumnStyleInfo4.ColumnName = "AgentCollectPK";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalUserControl|9fe3b4ed-4394-4d2c-824c-dc3a15505130", "Job Revenue Recognition Date", "Job Revenue Recognition Date", "");
			zTextBoxColumnStyleInfo4.ColumnName = "RevenueRecognitionDates";
			zTextBoxColumnStyleInfo5.ColumnName = "JH_JobLocalReference";
			zCalcEditColumnStyleInfo17.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo17.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BulkDSBJobCloseBatchApprovalUserControl|9c3c1e91-6a83-45c0-b786-8e4fc557ba24", "DSB Surplus/Shortfall");
			zCalcEditColumnStyleInfo17.ColumnName = "DSBSurplusAndShortfallAmount";
			this.JobGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.JobGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.JobGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.JobGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.JobGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.JobGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.JobGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.JobGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.JobGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.JobGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.JobGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.JobGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.JobGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.JobGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.JobGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.JobGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.JobGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.JobGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.JobGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.JobGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo17);
			this.JobGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.JobGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.JobGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.JobGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.JobGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.JobGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			this.JobGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo13);
			this.JobGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo14);
			this.JobGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo15);
			this.JobGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo16);
			// 
			// BulkDSBJobCloseBatchApprovalUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.JobGrid);
			this.Name = "BulkDSBJobCloseBatchApprovalUserControl";
			((System.ComponentModel.ISupportInitialize)(this.JobGrid)).EndInit();
			this.JobGrid.ResumeLayout(false);
			this.JobGrid.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

	}
}