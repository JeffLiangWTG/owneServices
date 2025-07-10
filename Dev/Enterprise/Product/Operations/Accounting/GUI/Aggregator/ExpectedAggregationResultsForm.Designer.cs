using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI.Aggregator
{
	partial class ExpectedAggregationResultsForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.zPeriodEdit = new Enterprise.ZArchitecture.GUI.ZPeriodEdit();
			this.CompanyFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ExecuteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zButton2 = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NumberOfPeriodsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.HeaderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LineChargesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.HeaderGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LineChargesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 117, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 24, true);
			this.MainStatusBar.TabIndex = 6;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.Aggregator.AggregationDiscrepanciesCalculator);
			// 
			// zPeriodEdit
			// 
			this.BindingSource.SetBindingMember(this.zPeriodEdit, "PeriodToReaggregate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Accounting.Business.Aggregator.AggregationDiscrepanciesCalculator)(null)).PeriodToReaggregate)));
			this.zPeriodEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExpectedAggregationResultsForm|f8719678-7c62-40fe-a0ec-9e70206920a8", "Period");
			this.zPeriodEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(362, 19, true);
			this.zPeriodEdit.Name = "zPeriodEdit";
			this.zPeriodEdit.TabIndex = 3;
			// 
			// CompanyFindBox
			// 
			this.BindingSource.SetBindingMember(this.CompanyFindBox, "Company");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Aggregator.AggregationDiscrepanciesCalculator)(null)).Company)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.Aggregator.AggregationDiscrepanciesCalculator)(null)).Companies)));
			this.CompanyFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExpectedAggregationResultsForm|7bb88f95-df39-459a-909e-2ad9b5695fa4", "Company");
			this.CompanyFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(66, 19, true);
			this.CompanyFindBox.Name = "CompanyFindBox";
			this.CompanyFindBox.PopupCaption = null;
			this.CompanyFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CompanyFindBox.TabIndex = 1;
			// 
			// ExecuteButton
			// 
			this.ExecuteButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExpectedAggregationResultsForm|bc15fa68-98fb-4946-87f3-e2fbfc0f8834", "Execute");
			this.ExecuteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(707, 17, true);
			this.ExecuteButton.Name = "ExecuteButton";
			this.ExecuteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ExecuteButton.TabIndex = 4;
			this.ExecuteButton.UseVisualStyleBackColor = true;
			this.ExecuteButton.Click += new System.EventHandler(this.ExecuteButton_Click);
			// 
			// zButton2
			// 
			this.zButton2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExpectedAggregationResultsForm|4557e540-3112-42ce-a622-9e371ee46301", "Close");
			this.zButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(788, 17, true);
			this.zButton2.Name = "zButton2";
			this.zButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zButton2.TabIndex = 5;
			this.zButton2.UseVisualStyleBackColor = true;
			this.zButton2.Click += new System.EventHandler(this.zButton2_Click);
			// 
			// NumberOfPeriodsCalcEdit
			// 
			this.NumberOfPeriodsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.NumberOfPeriodsCalcEdit, "NumberOfPeriods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Aggregator.AggregationDiscrepanciesCalculator)(null)).NumberOfPeriods)));
			this.NumberOfPeriodsCalcEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.NumberOfPeriodsCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExpectedAggregationResultsForm|b03a714d-618b-45f5-8add-4524f4e984c5", "Number of Periods");
			this.NumberOfPeriodsCalcEdit.Decimals = 0;
			this.NumberOfPeriodsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(582, 19, true);
			this.NumberOfPeriodsCalcEdit.Name = "NumberOfPeriodsCalcEdit";
			this.NumberOfPeriodsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 20, true);
			this.NumberOfPeriodsCalcEdit.TabIndex = 20;
			this.NumberOfPeriodsCalcEdit.Text = "0";
			this.NumberOfPeriodsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// HeaderGroupBox
			// 
			this.HeaderGroupBox.Controls.Add(this.zButton2);
			this.HeaderGroupBox.Controls.Add(this.ExecuteButton);
			this.HeaderGroupBox.Controls.Add(this.NumberOfPeriodsCalcEdit);
			this.HeaderGroupBox.Controls.Add(this.CompanyFindBox);
			this.HeaderGroupBox.Controls.Add(this.zPeriodEdit);
			this.HeaderGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.HeaderGroupBox, false);
			this.HeaderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderGroupBox.Name = "HeaderGroupBox";
			this.HeaderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 55, true);
			this.HeaderGroupBox.TabIndex = 21;
			this.HeaderGroupBox.TabStop = false;
			// 
			// LineChargesGrid
			// 
			this.LineChargesGrid.AllowNavigation = false;
			this.LineChargesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LineChargesGrid, "Lines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.Aggregator.AggregationDiscrepanciesCalculator)(null)).Lines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.AggregationDiscrepanciesCalculatorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Aggregator.AggregationDiscrepanciesCalculator)(null)).Lines)).SyncRoot)).LineType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.AggregationDiscrepanciesCalculatorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Aggregator.AggregationDiscrepanciesCalculator)(null)).Lines)).SyncRoot)).Period1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.AggregationDiscrepanciesCalculatorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Aggregator.AggregationDiscrepanciesCalculator)(null)).Lines)).SyncRoot)).Period2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.AggregationDiscrepanciesCalculatorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Aggregator.AggregationDiscrepanciesCalculator)(null)).Lines)).SyncRoot)).Period3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.AggregationDiscrepanciesCalculatorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Aggregator.AggregationDiscrepanciesCalculator)(null)).Lines)).SyncRoot)).Period4)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.AggregationDiscrepanciesCalculatorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Aggregator.AggregationDiscrepanciesCalculator)(null)).Lines)).SyncRoot)).Period5)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.AggregationDiscrepanciesCalculatorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Aggregator.AggregationDiscrepanciesCalculator)(null)).Lines)).SyncRoot)).Period6)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.AggregationDiscrepanciesCalculatorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Aggregator.AggregationDiscrepanciesCalculator)(null)).Lines)).SyncRoot)).Period7)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.AggregationDiscrepanciesCalculatorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Aggregator.AggregationDiscrepanciesCalculator)(null)).Lines)).SyncRoot)).Period8)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.AggregationDiscrepanciesCalculatorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Aggregator.AggregationDiscrepanciesCalculator)(null)).Lines)).SyncRoot)).Period9)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.AggregationDiscrepanciesCalculatorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Aggregator.AggregationDiscrepanciesCalculator)(null)).Lines)).SyncRoot)).Period10)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.AggregationDiscrepanciesCalculatorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Aggregator.AggregationDiscrepanciesCalculator)(null)).Lines)).SyncRoot)).Period11)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.AggregationDiscrepanciesCalculatorLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.Aggregator.AggregationDiscrepanciesCalculator)(null)).Lines)).SyncRoot)).Period12)));
			this.LineChargesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExpectedAggregationResultsForm|4da2502c-ae51-42d1-a784-ef18a90b4303", "Line Type");
			zTextBoxColumnStyleInfo1.ColumnName = "LineType";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExpectedAggregationResultsForm|6e6492e5-f5f3-4893-aae1-d15b3b44d79a", "Period 1");
			zCalcEditColumnStyleInfo1.ColumnName = "Period1";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExpectedAggregationResultsForm|381b481d-67f6-49b2-bce4-d8ec0ef57c05", "Period 2");
			zCalcEditColumnStyleInfo2.ColumnName = "Period2";
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExpectedAggregationResultsForm|c86ae800-2372-47a5-9f66-d5db312c9127", "Period 3");
			zCalcEditColumnStyleInfo3.ColumnName = "Period3";
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExpectedAggregationResultsForm|ab116cee-e25a-450f-8c0c-547625375c29", "Period 4");
			zCalcEditColumnStyleInfo4.ColumnName = "Period4";
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExpectedAggregationResultsForm|674cdd6f-c48b-4c90-9cdc-a3b1c187487c", "Period 5");
			zCalcEditColumnStyleInfo5.ColumnName = "Period5";
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExpectedAggregationResultsForm|58a86f74-a012-4ead-a00e-3657c11d1888", "Period 6");
			zCalcEditColumnStyleInfo6.ColumnName = "Period6";
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExpectedAggregationResultsForm|981fa255-18e1-46f9-9d06-f6832f9bd334", "Period 7");
			zCalcEditColumnStyleInfo7.ColumnName = "Period7";
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExpectedAggregationResultsForm|0c6106e8-d5c7-41b3-a5d1-502c78ceca15", "Period 8");
			zCalcEditColumnStyleInfo8.ColumnName = "Period8";
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExpectedAggregationResultsForm|a9439f9a-ebe3-4e5e-a570-6789b2ba24b8", "Period 9");
			zCalcEditColumnStyleInfo9.ColumnName = "Period9";
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExpectedAggregationResultsForm|f122e280-03c1-475a-88f7-d829c6f5ab7e", "Period 10");
			zCalcEditColumnStyleInfo10.ColumnName = "Period10";
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExpectedAggregationResultsForm|687db38b-ed4f-473d-b09b-06d4f5dad489", "Period 11");
			zCalcEditColumnStyleInfo11.ColumnName = "Period11";
			zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExpectedAggregationResultsForm|9869a09e-8dd4-4194-b5a2-430a879d929a", "Period 12");
			zCalcEditColumnStyleInfo12.ColumnName = "Period12";
			this.LineChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LineChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LineChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.LineChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.LineChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.LineChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.LineChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.LineChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.LineChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.LineChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.LineChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.LineChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.LineChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			this.LineChargesGrid.GridId = "43FFFF38-8822-4B8C-90BD-B3E318A46E7E";
			this.LineChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LineChargesGrid.LayoutKey = "LineChargesGrid";
			this.LineChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 61, true);
			this.LineChargesGrid.Name = "LineChargesGrid";
			this.LineChargesGrid.ReadOnly = true;
			this.LineChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 50, true);
			this.LineChargesGrid.TabIndex = 7;
			// 
			// ExpectedAggregationResultsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 141, true);
			this.Controls.Add(this.HeaderGroupBox);
			this.Controls.Add(this.LineChargesGrid);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Enterprise.Accounting.Business.Aggregator.AggregationDiscrepanciesCalculator);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.Aggregator.AggregationDiscrepanciesCalculator";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 177, true);
			this.Name = "ExpectedAggregationResultsForm";
			this.Controls.SetChildIndex(this.LineChargesGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.HeaderGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.HeaderGroupBox.ResumeLayout(false);
			this.HeaderGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LineChargesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPeriodEdit zPeriodEdit;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox CompanyFindBox;
		private Enterprise.ZArchitecture.GUI.ZButton ExecuteButton;
		private Enterprise.ZArchitecture.GUI.ZButton zButton2;
		public Enterprise.ZArchitecture.ZGrid LineChargesGrid;
		public Enterprise.ZArchitecture.ZCalcEdit NumberOfPeriodsCalcEdit;
		public Enterprise.ZArchitecture.GUI.ZGroupBox HeaderGroupBox;
	}
}
