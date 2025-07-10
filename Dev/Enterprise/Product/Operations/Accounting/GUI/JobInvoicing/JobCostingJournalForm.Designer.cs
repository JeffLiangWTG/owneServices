using System;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class JobCostingJournalForm
	{


		#region Windows Form Designer generated code

		private ZArchitecture.ZTextBox zTextBox1;
		private ZDateEdit zDateEdit1;
		private ZExchangeRateControl zExchangeRateControl1;
		private ZGuidFindBox zGuidFindBox3;
		private ZGuidFindBox zGuidFindBox2;
		private ZArchitecture.ZGrid zGrid1;
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private ZArchitecture.ZCalcEdit AH_NumberOfSupportingDocumentsCalcEdit;
		private System.ComponentModel.Container components = null;

		new void InitializeComponent()
		{
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo6 = new ZGuidFindBoxColumnStyleInfo();
			this.AH_NumberOfSupportingDocumentsCalcEdit = new ZArchitecture.ZCalcEdit();
			this.zTextBox1 = new ZArchitecture.ZTextBox();
			this.zDateEdit1 = new ZDateEdit();
			this.zExchangeRateControl1 = new ZExchangeRateControl();
			this.zGuidFindBox3 = new ZGuidFindBox();
			this.zGuidFindBox2 = new ZGuidFindBox();
			this.zGrid1 = new ZArchitecture.ZGrid();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 444, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 26, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 7;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(265);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(265);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JCJournalHeader);
			// 
			// AH_NumberOfSupportingDocumentsCalcEdit
			// 
			this.AH_NumberOfSupportingDocumentsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AH_NumberOfSupportingDocumentsCalcEdit, "AH_NumberOfSupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JCJournalHeader)(null)).AH_NumberOfSupportingDocuments)));
			this.AH_NumberOfSupportingDocumentsCalcEdit.DecimalPlaces = 2;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 32, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.Name = "AH_NumberOfSupportingDocumentsCalcEdit";
			this.AH_NumberOfSupportingDocumentsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.TabIndex = 24;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Text = "0";
			this.AH_NumberOfSupportingDocumentsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "AH_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JCJournalHeader)(null)).AH_Desc)));
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 56, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 20, true);
			this.zTextBox1.TabIndex = 4;
			// 
			// zDateEdit1
			// 
			this.zDateEdit1.AllowDrop = true;
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.zDateEdit1.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit1, "AH_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JCJournalHeader)(null)).AH_InvoiceDate)));
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 8, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 0;
			// 
			// zExchangeRateControl1
			// 
			this.zExchangeRateControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zExchangeRateControl1, "ExchangeRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.ZExchangeRate)(((JCJournalHeader)(null)).ExchangeRate)));
			this.zExchangeRateControl1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobCostingJournalForm|9580e4d8-d463-486b-a1ec-969dc46ac1a6", "Currency");
			this.zExchangeRateControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 32, true);
			this.zExchangeRateControl1.Name = "zExchangeRateControl1";
			this.zExchangeRateControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 22, true);
			this.zExchangeRateControl1.TabIndex = 1;
			// 
			// zGuidFindBox3
			// 
			this.zGuidFindBox3.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox3, "AH_GE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((JCJournalHeader)(null)).AH_GE)));
			this.zGuidFindBox3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobCostingJournalForm|6058e83d-ae06-4533-85ec-2825da9c881a", "Dept");
			this.zGuidFindBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 34, true);
			this.zGuidFindBox3.Name = "zGuidFindBox3";
			this.zGuidFindBox3.ShowDescriptionBox = false;
			this.zGuidFindBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.zGuidFindBox3.TabIndex = 3;
			// 
			// zGuidFindBox2
			// 
			this.zGuidFindBox2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox2, "AH_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((JCJournalHeader)(null)).AH_GB)));
			this.zGuidFindBox2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobCostingJournalForm|c011e98d-a154-4854-9cc4-d2f937417a4f", "Brn.", "Branch");
			this.zGuidFindBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 9, true);
			this.zGuidFindBox2.Name = "zGuidFindBox2";
			this.zGuidFindBox2.ShowDescriptionBox = false;
			this.zGuidFindBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.zGuidFindBox2.TabIndex = 2;
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zGrid1, "Lines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JCJournalHeader)(null)).Lines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((JCJournalLine)(((System.Collections.IList)(((JCJournalHeader)(null)).Lines)).SyncRoot)).AL_JH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((JCJournalLine)(((System.Collections.IList)(((JCJournalHeader)(null)).Lines)).SyncRoot)).AL_AC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((JCJournalLine)(((System.Collections.IList)(((JCJournalHeader)(null)).Lines)).SyncRoot)).AL_AG)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JCJournalLine)(((System.Collections.IList)(((JCJournalHeader)(null)).Lines)).SyncRoot)).AL_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JCJournalLine)(((System.Collections.IList)(((JCJournalHeader)(null)).Lines)).SyncRoot)).AL_LocalExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((JCJournalLine)(((System.Collections.IList)(((JCJournalHeader)(null)).Lines)).SyncRoot)).AL_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((JCJournalLine)(((System.Collections.IList)(((JCJournalHeader)(null)).Lines)).SyncRoot)).AL_ReverseDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((JCJournalLine)(((System.Collections.IList)(((JCJournalHeader)(null)).Lines)).SyncRoot)).AL_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((JCJournalLine)(((System.Collections.IList)(((JCJournalHeader)(null)).Lines)).SyncRoot)).AL_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((JCJournalLine)(((System.Collections.IList)(((JCJournalHeader)(null)).Lines)).SyncRoot)).AL_CFXControlAccount)));
			this.zGrid1.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobCostingJournalForm|64ab51bd-eb89-44ca-ae67-7c688db7eaf6", "Job");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AL_JH";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobCostingJournalForm|73793a19-7da4-44cb-8016-a11ba319856e", "Charge Code");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "AL_AC";
			zGuidFindBoxColumnStyleInfo3.ColumnName = "AL_AG";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobCostingJournalForm|5af7de0b-2cdb-4408-ba0c-ebb051863f36", "Desc.", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "AL_Desc";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobCostingJournalForm|1c8dcb80-4a92-4e49-b7cb-3c2bfde60e40", "Local Amt");
			zCalcEditColumnStyleInfo1.ColumnName = "AL_LocalExTaxAmount";
			zDateEditColumnStyleInfo1.ColumnName = "AL_PostDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobCostingJournalForm|2c061ab0-428e-46e8-a9da-1b6b205797dd", "Recognition Date");
			zDateEditColumnStyleInfo2.ColumnName = "AL_ReverseDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zGuidFindBoxColumnStyleInfo4.ColumnName = "AL_GB";
			zGuidFindBoxColumnStyleInfo5.ColumnName = "AL_GE";
			zGuidFindBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobCostingJournalForm|b20f33a7-388c-4c74-9ad3-14e852df6bfb", "CFX Control Account");
			zGuidFindBoxColumnStyleInfo6.ColumnName = "AL_CFXControlAccount";
			this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo6);
			this.zGrid1.CopySelectedRowsAllowed = true;
			this.zGrid1.GridId = "fba068c6-834d-4aa5-ae09-0f23b2d2bf18";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 88, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(516, 316, true);
			this.zGrid1.TabIndex = 5;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 410, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.PostingButtonsUserControl.TabIndex = 6;
			// 
			// JobCostingJournalForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobCostingJournalForm|a11bd1fe-6731-40a9-8305-b1117d4ff287", "Job Costing Journal");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 470, true);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.zGrid1);
			this.Controls.Add(this.zExchangeRateControl1);
			this.Controls.Add(this.zDateEdit1);
			this.Controls.Add(this.zTextBox1);
			this.Controls.Add(this.zGuidFindBox3);
			this.Controls.Add(this.zGuidFindBox2);
			this.Controls.Add(this.AH_NumberOfSupportingDocumentsCalcEdit);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(JCJournalHeader);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.JobInvoicing.JCJournalHeader";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(535, 517, true);
			this.Name = "JobCostingJournalForm";
			this.Controls.SetChildIndex(this.AH_NumberOfSupportingDocumentsCalcEdit, 0);
			this.Controls.SetChildIndex(this.zGuidFindBox2, 0);
			this.Controls.SetChildIndex(this.zGuidFindBox3, 0);
			this.Controls.SetChildIndex(this.zTextBox1, 0);
			this.Controls.SetChildIndex(this.zDateEdit1, 0);
			this.Controls.SetChildIndex(this.zExchangeRateControl1, 0);
			this.Controls.SetChildIndex(this.zGrid1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}