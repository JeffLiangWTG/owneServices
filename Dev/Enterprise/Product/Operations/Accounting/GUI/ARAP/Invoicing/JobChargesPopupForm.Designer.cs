using System;
using System.Linq;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class JobChargesPopupForm
	{


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new ZGuidFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.JobChargesGrid = new ZDisplayGrid();
			this.SelectJobChargesLabel = new ZArchitecture.ZLabel();
			this.ImportButton = new ZButton();
			this.CancelImportButton = new ZButton();
			this.OptionsGroupBox = new ZGroupBox();
			this.IncludeChargesForCreditorsWithTheSameAPSettlementGroupCheckBox = new ZCheckBox();
			this.IncludeChargesForAllOtherCreditorsCheckBox = new ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.JobChargesGrid)).BeginInit();
			this.JobChargesGrid.SuspendLayout();
			this.OptionsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 239, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 25, true);
			this.MainStatusBar.TabIndex = 5;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JobChargesImporter);
			// 
			// JobChargesGrid
			// 
			this.JobChargesGrid.AllowNavigation = false;
			this.JobChargesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JobChargesGrid, "JobChargesCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobChargesImporter)(null)).JobChargesCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Charge)(((System.Collections.IList)(((JobChargesImporter)(null)).JobChargesCollection)).SyncRoot)).JR_AC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Charge)(((System.Collections.IList)(((JobChargesImporter)(null)).JobChargesCollection)).SyncRoot)).JR_JH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Charge)(((System.Collections.IList)(((JobChargesImporter)(null)).JobChargesCollection)).SyncRoot)).JR_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Charge)(((System.Collections.IList)(((JobChargesImporter)(null)).JobChargesCollection)).SyncRoot)).JR_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Charge)(((System.Collections.IList)(((JobChargesImporter)(null)).JobChargesCollection)).SyncRoot)).JR_RX_NKCostCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Charge)(((System.Collections.IList)(((JobChargesImporter)(null)).JobChargesCollection)).SyncRoot)).JR_OSCostExRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Charge)(((System.Collections.IList)(((JobChargesImporter)(null)).JobChargesCollection)).SyncRoot)).JR_OSCostAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Charge)(((System.Collections.IList)(((JobChargesImporter)(null)).JobChargesCollection)).SyncRoot)).JR_LocalCostAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Charge)(((System.Collections.IList)(((JobChargesImporter)(null)).JobChargesCollection)).SyncRoot)).JR_OH_CostAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Charge)(((System.Collections.IList)(((JobChargesImporter)(null)).JobChargesCollection)).SyncRoot)).JR_JobLocalRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Charge)(((System.Collections.IList)(((JobChargesImporter)(null)).JobChargesCollection)).SyncRoot)).JR_Calc_RelatedJobNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Charge)(((System.Collections.IList)(((JobChargesImporter)(null)).JobChargesCollection)).SyncRoot)).JR_GB_CostTaxBranch)));
			this.JobChargesGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JR_AC";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "JR_JH";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "JR_GB";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargesPopupForm|cce0c893-2ad3-47fd-a8c1-64ab65c24042", "Dept");
			zGuidFindBoxColumnStyleInfo4.ColumnName = "JR_GE";
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JR_RX_NKCostCurrency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargesPopupForm|c24d9402-ef89-4229-a9b4-36ef11be7937", "Ex Rate", "Cost Exchange Rate.");
			zCalcEditColumnStyleInfo1.ColumnName = "JR_OSCostExRate";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargesPopupForm|e96de502-15da-44e0-b12b-6b640fdbea8c", "OS Amt");
			zCalcEditColumnStyleInfo2.ColumnName = "JR_OSCostAmt";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargesPopupForm|70a20415-62af-4edd-bc82-7717849914a1", "Local Amt");
			zCalcEditColumnStyleInfo3.ColumnName = "JR_LocalCostAmt";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "JR_OH_CostAccount";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "JR_JobLocalRef";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e04e8386-3f71-49f8-b364-9119b4777b42", "Related Job Number");
			zTextBoxColumnStyleInfo2.ColumnName = "JR_Calc_RelatedJobNumber";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargesPopupForm|8D354F77-4BDC-4F82-91A3-167F1F978AE3", "Cost Tax Branch");
			zGuidFindBoxColumnStyleInfo5.ColumnName = "JR_GB_CostTaxBranch";
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.JobChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.JobChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.JobChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.JobChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.JobChargesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.JobChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.JobChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.JobChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.JobChargesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.JobChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.JobChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.JobChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.JobChargesGrid.GridId = "b8073657-afe7-4570-977f-7524cb02fc2a";
			this.JobChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobChargesGrid.IsWholeRowSelectedOnClick = true;
			this.JobChargesGrid.LayoutKey = "AccrualsGrid";
			this.JobChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 30, true);
			this.JobChargesGrid.Name = "JobChargesGrid";
			this.JobChargesGrid.ReadOnly = true;
			this.JobChargesGrid.ShouldSetErrorsOnTabPage = false;
			this.JobChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 133, true);
			this.JobChargesGrid.TabIndex = 1;
			// 
			// SelectJobChargesLabel
			// 
			this.SelectJobChargesLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargesPopupForm|74f557e1-c7b3-4779-bb45-eac95c15f434", "Please select charges to import into invoice from the list below");
			this.SelectJobChargesLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SelectJobChargesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 4, true);
			this.SelectJobChargesLabel.Name = "SelectJobChargesLabel";
			this.SelectJobChargesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 21, true);
			this.SelectJobChargesLabel.TabIndex = 0;
			// 
			// ImportButton
			// 
			this.ImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ImportButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargesPopupForm|638a3fe3-cd9f-4b54-a56a-33172994aabe", "&Import");
			this.ImportButton.IsCaptionOverridden = false;
			this.ImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 211, true);
			this.ImportButton.Name = "ImportButton";
			this.ImportButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.ImportButton.TabIndex = 3;
			this.ImportButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ImportButton.ToolTipCaption = null;
			this.ImportButton.Click += new EventHandler(this.ImportButton_Click);
			// 
			// CancelImportButton
			// 
			this.CancelImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelImportButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargesPopupForm|4256b02a-9e2a-4a0a-aba4-802bd648d732", "&Cancel");
			this.CancelImportButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelImportButton.IsCaptionOverridden = false;
			this.CancelImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(497, 211, true);
			this.CancelImportButton.Name = "CancelImportButton";
			this.CancelImportButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.CancelImportButton.TabIndex = 4;
			this.CancelImportButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CancelImportButton.ToolTipCaption = null;
			this.CancelImportButton.Click += new EventHandler(this.CancelImportButton_Click);
			// 
			// OptionsGroupBox
			// 
			this.OptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.OptionsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargesPopupForm|e1546955-8801-486e-8ab3-05e8a6fa2678", "Options");
			this.OptionsGroupBox.Controls.Add(this.IncludeChargesForCreditorsWithTheSameAPSettlementGroupCheckBox);
			this.OptionsGroupBox.Controls.Add(this.IncludeChargesForAllOtherCreditorsCheckBox);
			this.OptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 169, true);
			this.OptionsGroupBox.Name = "OptionsGroupBox";
			this.OptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 64, true);
			this.OptionsGroupBox.TabIndex = 2;
			this.OptionsGroupBox.TabStop = false;
			// 
			// IncludeChargesForCreditorsWithTheSameAPSettlementGroupCheckBox
			// 
			this.IncludeChargesForCreditorsWithTheSameAPSettlementGroupCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IncludeChargesForCreditorsWithTheSameAPSettlementGroupCheckBox, "IncludeChargesForCreditorsWithTheSameAPSettlementGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((JobChargesImporter)(null)).IncludeChargesForCreditorsWithTheSameAPSettlementGroup)));
			this.IncludeChargesForCreditorsWithTheSameAPSettlementGroupCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargesPopupForm|ae3d1e3c-c654-4e0c-84dd-39d3d1b594c0", "Include Charges for Creditors with the Same AP Settlement Group");
			this.IncludeChargesForCreditorsWithTheSameAPSettlementGroupCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludeChargesForCreditorsWithTheSameAPSettlementGroupCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 42, true);
			this.IncludeChargesForCreditorsWithTheSameAPSettlementGroupCheckBox.Name = "IncludeChargesForCreditorsWithTheSameAPSettlementGroupCheckBox";
			this.IncludeChargesForCreditorsWithTheSameAPSettlementGroupCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IncludeChargesForCreditorsWithTheSameAPSettlementGroupCheckBox.TabIndex = 1;
			this.IncludeChargesForCreditorsWithTheSameAPSettlementGroupCheckBox.UseVisualStyleBackColor = true;
			// 
			// IncludeChargesForAllOtherCreditorsCheckBox
			// 
			this.IncludeChargesForAllOtherCreditorsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IncludeChargesForAllOtherCreditorsCheckBox, "IncludeChargesForAllOtherCreditors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((JobChargesImporter)(null)).IncludeChargesForAllOtherCreditors)));
			this.IncludeChargesForAllOtherCreditorsCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargesPopupForm|bf38dfb8-80e7-4f5e-98f6-d09f77e1bf58", "Include Charges for all other Creditors");
			this.IncludeChargesForAllOtherCreditorsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludeChargesForAllOtherCreditorsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.IncludeChargesForAllOtherCreditorsCheckBox.Name = "IncludeChargesForAllOtherCreditorsCheckBox";
			this.IncludeChargesForAllOtherCreditorsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IncludeChargesForAllOtherCreditorsCheckBox.TabIndex = 0;
			this.IncludeChargesForAllOtherCreditorsCheckBox.UseVisualStyleBackColor = true;
			// 
			// JobChargesPopupForm
			// 
			this.CancelButton = this.CancelImportButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargesPopupForm|55562fc2-6fd3-47fb-ba4d-1cafae77bb15", "Import Charges");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 264, true);
			this.Controls.Add(this.OptionsGroupBox);
			this.Controls.Add(this.ImportButton);
			this.Controls.Add(this.SelectJobChargesLabel);
			this.Controls.Add(this.JobChargesGrid);
			this.Controls.Add(this.CancelImportButton);
			this.DataSourceType = typeof(JobChargesImporter);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 300, true);
			this.Name = "JobChargesPopupForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelImportButton, 0);
			this.Controls.SetChildIndex(this.JobChargesGrid, 0);
			this.Controls.SetChildIndex(this.SelectJobChargesLabel, 0);
			this.Controls.SetChildIndex(this.ImportButton, 0);
			this.Controls.SetChildIndex(this.OptionsGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.JobChargesGrid)).EndInit();
			this.JobChargesGrid.ResumeLayout(false);
			this.JobChargesGrid.PerformLayout();
			this.OptionsGroupBox.ResumeLayout(false);
			this.OptionsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}