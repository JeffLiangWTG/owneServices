using System;
using System.Collections.Generic;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class JobChargeQuickCalculateForm
	{


		#region Windows Form Designer generated code

		ZDropEdit MeasurementBasisDropEdit;
		internal ZArchitecture.ZGrid containersGrid;
		internal ZArchitecture.ZCalcEdit QuantityEdit;
		internal ZArchitecture.ZCalcEdit RateCalcEditSell;
		internal ZArchitecture.ZCalcEdit RateCalcEditCost;
		internal ZArchitecture.ZCalcEdit CostTotalCalcEdit;
		internal ZArchitecture.ZCalcEdit SellTotalCalcEdit;
		internal ZArchitecture.ZCalcEdit CostMinimumCalcEdit;
		internal ZArchitecture.ZCalcEdit SellMinimumCalcEdit;
		internal ZCheckBox IsMinimumCheckBox;
		ZArchitecture.ZLabel zLabel1;
		ZButton zButton1;
		ZButton zButton2;
		ZCheckBox UpdateSellCheckBox;
		ZCheckBox UpdateCostCheckBox;
		ZGuidFindBox ChargeCodeFindBox;
		System.ComponentModel.Container components = null;

		ZArchitecture.ZTextBoxColumnStyleInfo ContainerTypeTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
		ZArchitecture.ZCalcEditColumnStyleInfo SellCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
		ZArchitecture.ZCalcEditColumnStyleInfo CostCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
		ZArchitecture.ZCalcEditColumnStyleInfo QuantitySelectedContainerCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
		ZMultiLineTextBoxColumnInfo ContainerNumberMultiLineTextBoxColumnInfo = new ZMultiLineTextBoxColumnInfo();

		protected override void InitializeComponent()
		{
			ContainerTypeTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			SellCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			CostCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			QuantitySelectedContainerCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ContainerNumberMultiLineTextBoxColumnInfo = new ZMultiLineTextBoxColumnInfo();
			this.MeasurementBasisDropEdit = new ZDropEdit();
			this.containersGrid = new ZArchitecture.ZGrid();
			this.QuantityEdit = new ZArchitecture.ZCalcEdit();
			this.RateCalcEditSell = new ZArchitecture.ZCalcEdit();
			this.RateCalcEditCost = new ZArchitecture.ZCalcEdit();
			this.CostTotalCalcEdit = new ZArchitecture.ZCalcEdit();
			this.SellTotalCalcEdit = new ZArchitecture.ZCalcEdit();
			this.CostMinimumCalcEdit = new ZArchitecture.ZCalcEdit();
			this.SellMinimumCalcEdit = new ZArchitecture.ZCalcEdit();
			this.zLabel1 = new ZArchitecture.ZLabel();
			this.zButton1 = new ZButton();
			this.zButton2 = new ZButton();
			this.UpdateSellCheckBox = new ZCheckBox();
			this.UpdateCostCheckBox = new ZCheckBox();
			this.ChargeCodeFindBox = new ZGuidFindBox();
			this.IsMinimumCheckBox = new ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MeasurementBasisDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.containersGrid)).BeginInit();
			this.containersGrid.SuspendLayout();
			this.ChargeCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 153, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(461, 10, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 12;
			this.MainStatusBar.Visible = false;
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
			this.BindingSource.DataSourceType = typeof(JobChargeQuickCalculateBusinessObject);
			// 
			// MeasurementBasisDropEdit
			// 
			this.MeasurementBasisDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MeasurementBasisDropEdit, "QuantityDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobChargeQuickCalculateBusinessObject)(null)).QuantityDescription)));
			this.MeasurementBasisDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeQuickCalculateForm|1dc5ed8d-4ef3-4149-bf09-84e6a9df9361", "Measurement", "Measurement Basis", "Specifies the measurement basis for this quick calculation.");
			this.MeasurementBasisDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MeasurementBasisDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 12, true);
			this.MeasurementBasisDropEdit.Name = "MeasurementBasisDropEdit";
			this.MeasurementBasisDropEdit.ShowDescriptionBox = false;
			this.MeasurementBasisDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.MeasurementBasisDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.MeasurementBasisDropEdit.TabIndex = 0;
			// 
			// containersGrid
			// 
			this.containersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.containersGrid, "Containers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobChargeQuickCalculateBusinessObject)(null)).Containers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ContainerCalculationData)(((System.Collections.IList)(((JobChargeQuickCalculateBusinessObject)(null)).Containers)).SyncRoot)).ContainerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ContainerCalculationData)(((System.Collections.IList)(((JobChargeQuickCalculateBusinessObject)(null)).Containers)).SyncRoot)).Cost)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ContainerCalculationData)(((System.Collections.IList)(((JobChargeQuickCalculateBusinessObject)(null)).Containers)).SyncRoot)).Sell)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ContainerCalculationData)(((System.Collections.IList)(((JobChargeQuickCalculateBusinessObject)(null)).Containers)).SyncRoot)).QuantitySelectedContainers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ContainerCalculationData)(((System.Collections.IList)(((JobChargeQuickCalculateBusinessObject)(null)).Containers)).SyncRoot)).ContainerNumbers)));
			this.containersGrid.CaptionVisible = false;
			this.ContainerTypeTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("62e21a33-c8da-4f9d-a479-72e1d04619b4", "Cont/ULD", "Container Type", "");
			this.ContainerTypeTextBoxColumnStyleInfo.ColumnName = "ContainerType";
			this.ContainerTypeTextBoxColumnStyleInfo.IsReadOnly = true;
			this.ContainerTypeTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.SellCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			this.SellCalcEditColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("394d76a1-25e9-48d8-b613-9f5d5858f3b4", "Cost", "Cost Rate", "");
			this.SellCalcEditColumnStyleInfo.ColumnName = "Cost";
			this.SellCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.CostCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			this.CostCalcEditColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e2ba1348-fc8d-4626-9c60-9f7362af75ee", "Sell", "Sell Rate", "");
			this.CostCalcEditColumnStyleInfo.ColumnName = "Sell";
			this.CostCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.QuantitySelectedContainerCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			this.QuantitySelectedContainerCalcEditColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f50503a3-8212-41d3-9a38-4466e3d067e4", "Count");
			this.QuantitySelectedContainerCalcEditColumnStyleInfo.ColumnName = "QuantitySelectedContainers";
			this.QuantitySelectedContainerCalcEditColumnStyleInfo.Decimals = 0;
			this.QuantitySelectedContainerCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			this.ContainerNumberMultiLineTextBoxColumnInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8d22ff9d-2a7c-4e46-bf19-f1c5a6e777fc", "Cont. #", "Container Numbers", "The Container Numbers of the Containers to be counted");
			this.ContainerNumberMultiLineTextBoxColumnInfo.ColumnName = "ContainerNumbers";
			this.ContainerNumberMultiLineTextBoxColumnInfo.IsReadOnly = true;
			this.ContainerNumberMultiLineTextBoxColumnInfo.MinimumEditControlWidth = 300;
			this.ContainerNumberMultiLineTextBoxColumnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.containersGrid.ColumnStyles.Add(this.ContainerTypeTextBoxColumnStyleInfo);
			this.containersGrid.ColumnStyles.Add(this.SellCalcEditColumnStyleInfo);
			this.containersGrid.ColumnStyles.Add(this.CostCalcEditColumnStyleInfo);
			this.containersGrid.ColumnStyles.Add(this.QuantitySelectedContainerCalcEditColumnStyleInfo);
			this.containersGrid.ColumnStyles.Add(this.ContainerNumberMultiLineTextBoxColumnInfo);
			this.containersGrid.GridId = "40b95fcb-bc4d-4e93-a339-01b272b83ca1";
			this.containersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.containersGrid.LayoutKey = "containersGrid";
			this.containersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 38, true);
			this.containersGrid.Name = "containersGrid";
			this.containersGrid.RowHeadersVisible = false;
			this.containersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 120, true);
			this.containersGrid.TabIndex = 13;
			this.containersGrid.Visible = false;
			// 
			// QuantityEdit
			// 
			this.BindingSource.SetBindingMember(this.QuantityEdit, "Quantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JobChargeQuickCalculateBusinessObject)(null)).Quantity)));
			this.QuantityEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("281dafa5-cc94-469d-8667-6824c147ed84", "Quantity");
			this.QuantityEdit.DecimalPlaces = 3;
			this.QuantityEdit.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QuantityEdit, false);
			this.QuantityEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 12, true);
			this.QuantityEdit.Name = "QuantityEdit";
			this.QuantityEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.QuantityEdit.TabIndex = 1;
			this.QuantityEdit.Text = "0.000";
			this.QuantityEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RateCalcEditSell
			// 
			this.BindingSource.SetBindingMember(this.RateCalcEditSell, "SellRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JobChargeQuickCalculateBusinessObject)(null)).SellRate)));
			this.RateCalcEditSell.CaptionResourceString = null;
			this.RateCalcEditSell.DecimalPlaces = 3;
			this.RateCalcEditSell.Decimals = 3;
			this.RateCalcEditSell.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 61, true);
			this.RateCalcEditSell.Name = "RateCalcEditSell";
			this.RateCalcEditSell.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 17, true);
			this.RateCalcEditSell.TabIndex = 7;
			this.RateCalcEditSell.Text = "0.000";
			this.RateCalcEditSell.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RateCalcEditCost
			// 
			this.BindingSource.SetBindingMember(this.RateCalcEditCost, "CostRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JobChargeQuickCalculateBusinessObject)(null)).CostRate)));
			this.RateCalcEditCost.CaptionResourceString = null;
			this.RateCalcEditCost.DecimalPlaces = 3;
			this.RateCalcEditCost.Decimals = 3;
			this.RateCalcEditCost.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 38, true);
			this.RateCalcEditCost.Name = "RateCalcEditCost";
			this.RateCalcEditCost.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 17, true);
			this.RateCalcEditCost.TabIndex = 3;
			this.RateCalcEditCost.Text = "0.000";
			this.RateCalcEditCost.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CostTotalCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CostTotalCalcEdit, "CostTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JobChargeQuickCalculateBusinessObject)(null)).CostTotal)));
			this.CostTotalCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeQuickCalculateForm|37f33956-4a52-4ad7-87ef-923c9f530804", "Total", "Total Cost", "The Total Cost amount calculated.");
			this.CostTotalCalcEdit.DecimalPlaces = 3;
			this.CostTotalCalcEdit.Decimals = 3;
			this.CostTotalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 38, true);
			this.CostTotalCalcEdit.Name = "CostTotalCalcEdit";
			this.CostTotalCalcEdit.ReadOnly = true;
			this.CostTotalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 17, true);
			this.CostTotalCalcEdit.TabIndex = 5;
			this.CostTotalCalcEdit.Text = "0.000";
			this.CostTotalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SellTotalCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SellTotalCalcEdit, "SellTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JobChargeQuickCalculateBusinessObject)(null)).SellTotal)));
			this.SellTotalCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeQuickCalculateForm|66f8303d-52fa-4e3d-a3b2-f996db349f36", "Total", "Total Sell", "The Total Sell amount calculated.");
			this.SellTotalCalcEdit.DecimalPlaces = 3;
			this.SellTotalCalcEdit.Decimals = 3;
			this.SellTotalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 61, true);
			this.SellTotalCalcEdit.Name = "SellTotalCalcEdit";
			this.SellTotalCalcEdit.ReadOnly = true;
			this.SellTotalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 17, true);
			this.SellTotalCalcEdit.TabIndex = 9;
			this.SellTotalCalcEdit.Text = "0.000";
			this.SellTotalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CostMinimumCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CostMinimumCalcEdit, "CostMinimum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JobChargeQuickCalculateBusinessObject)(null)).CostMinimum)));
			this.CostMinimumCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeQuickCalculateForm|5C4E6F8A-B36B-4AF0-83B5-5252538E9288", "Minimum", "Minimum", "The Minimum Cost amount calculated.");
			this.CostMinimumCalcEdit.DecimalPlaces = 3;
			this.CostMinimumCalcEdit.Decimals = 3;
			this.CostMinimumCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 38, true);
			this.CostMinimumCalcEdit.Name = "CostMinimumCalcEdit";
			this.CostMinimumCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 17, true);
			this.CostMinimumCalcEdit.TabIndex = 4;
			this.CostMinimumCalcEdit.Text = "0.000";
			this.CostMinimumCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SellMinimumCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SellMinimumCalcEdit, "SellMinimum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JobChargeQuickCalculateBusinessObject)(null)).SellMinimum)));
			this.SellMinimumCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeQuickCalculateForm|475DF9FF-4985-42F6-B335-27F291B8676E", "Minimum", "Minimum", "The Minimum Sell amount calculated.");
			this.SellMinimumCalcEdit.DecimalPlaces = 3;
			this.SellMinimumCalcEdit.Decimals = 3;
			this.SellMinimumCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 61, true);
			this.SellMinimumCalcEdit.Name = "SellMinimumCalcEdit";
			this.SellMinimumCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 17, true);
			this.SellMinimumCalcEdit.TabIndex = 8;
			this.SellMinimumCalcEdit.Text = "0.000";
			this.SellMinimumCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zLabel1, "QuantityUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobChargeQuickCalculateBusinessObject)(null)).QuantityUnit)));
			this.zLabel1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeQuickCalculateForm|0fbeb8ff-571b-4015-89b6-e2de7e84d0cb", "<Unit>");
			this.zLabel1.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(440, 15, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 13, true);
			// 
			// zButton1
			// 
			this.zButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zButton1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeQuickCalculateForm|5ccf7efe-64a9-472f-a9d5-7e7da2247076", "OK");
			this.zButton1.IsCaptionOverridden = false;
			this.zButton1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 134, true);
			this.zButton1.Name = "zButton1";
			this.zButton1.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.zButton1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zButton1.TabIndex = 14;
			this.zButton1.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.zButton1.ToolTipCaption = null;
			this.zButton1.Click += new EventHandler(this.OKButton_Click);
			// 
			// zButton2
			// 
			this.zButton2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zButton2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeQuickCalculateForm|8EBA9021-4BF6-44C8-9859-A9109E898881", "Cancel");
			this.zButton2.IsCaptionOverridden = false;
			this.zButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 134, true);
			this.zButton2.Name = "zButton2";
			this.zButton2.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.zButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zButton2.TabIndex = 15;
			this.zButton2.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.zButton2.ToolTipCaption = null;
			this.zButton2.Click += new EventHandler(this.CancelButton_Click);
			// 
			// UpdateSellCheckBox
			// 
			this.UpdateSellCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UpdateSellCheckBox, "UpdateSell");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((JobChargeQuickCalculateBusinessObject)(null)).UpdateSell)));
			this.UpdateSellCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeQuickCalculateForm|6fe7b612-0a56-487b-9cf3-46fd7423262f", "Update Sell", "Specifies whether the Sell Amount should be updated by the Quick Calculator.");
			this.UpdateSellCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UpdateSellCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 63, true);
			this.UpdateSellCheckBox.Name = "UpdateSellCheckBox";
			this.UpdateSellCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 16, true);
			this.UpdateSellCheckBox.TabIndex = 6;
			this.UpdateSellCheckBox.UseVisualStyleBackColor = true;
			// 
			// UpdateCostCheckBox
			// 
			this.UpdateCostCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UpdateCostCheckBox, "UpdateCost");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((JobChargeQuickCalculateBusinessObject)(null)).UpdateCost)));
			this.UpdateCostCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeQuickCalculateForm|b693c60b-9a76-436c-8ef2-1337c45d7a6a", "Update Cost", "Specifies whether the Cost Amount should be updated by the Quick Calculator.");
			this.UpdateCostCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UpdateCostCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 40, true);
			this.UpdateCostCheckBox.Name = "UpdateCostCheckBox";
			this.UpdateCostCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 16, true);
			this.UpdateCostCheckBox.TabIndex = 2;
			this.UpdateCostCheckBox.UseVisualStyleBackColor = true;
			// 
			// ChargeCodeFindBox
			// 
			this.ChargeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChargeCodeFindBox, "ChargeCodePK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((JobChargeQuickCalculateBusinessObject)(null)).ChargeCodePK)));
			this.ChargeCodeFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeQuickCalculateForm|65e2ca83-76f8-4d0e-98b0-ed3cbda3b828", "Charge Code", "Existing Charge", "Existing Charge Code", "When specifying a charge as a percentage of another, the existing charge code should be specified here.");
			this.ChargeCodeFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.ChargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 118, true);
			this.ChargeCodeFindBox.Name = "ChargeCodeFindBox";
			this.ChargeCodeFindBox.PopupCaption = null;
			this.ChargeCodeFindBox.ShouldResize = true;
			this.ChargeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 17, true);
			this.ChargeCodeFindBox.TabIndex = 11;
			// 
			// IsMinimumCheckBox
			// 
			this.IsMinimumCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsMinimumCheckBox, "IsMinimum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((JobChargeQuickCalculateBusinessObject)(null)).IsMinimum)));
			this.IsMinimumCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeQuickCalculateForm|3a8e073c-77ff-42b6-a268-fe8ece08ec70", "Is Minimum");
			this.IsMinimumCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsMinimumCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 85, true);
			this.IsMinimumCheckBox.Name = "IsMinimumCheckBox";
			this.IsMinimumCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 16, true);
			this.IsMinimumCheckBox.TabIndex = 10;
			this.IsMinimumCheckBox.UseVisualStyleBackColor = true;
			// 
			// JobChargeQuickCalculateForm
			// 
			this.AcceptButton = this.zButton1;
			this.CancelButton = this.zButton2;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeQuickCalculateForm|97c2a7a5-033b-4260-b256-004455b432dc", "Quick Calculate");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(461, 163, true);
			this.Controls.Add(this.IsMinimumCheckBox);
			this.Controls.Add(this.ChargeCodeFindBox);
			this.Controls.Add(this.UpdateCostCheckBox);
			this.Controls.Add(this.UpdateSellCheckBox);
			this.Controls.Add(this.zButton2);
			this.Controls.Add(this.zButton1);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.SellMinimumCalcEdit);
			this.Controls.Add(this.CostMinimumCalcEdit);
			this.Controls.Add(this.CostTotalCalcEdit);
			this.Controls.Add(this.SellTotalCalcEdit);
			this.Controls.Add(this.RateCalcEditCost);
			this.Controls.Add(this.RateCalcEditSell);
			this.Controls.Add(this.MeasurementBasisDropEdit);
			this.Controls.Add(this.QuantityEdit);
			this.Controls.Add(this.containersGrid);
			this.DataSourceAssemblyName = "Enterprise.Accounting.GUI";
			this.DataSourceType = typeof(JobChargeQuickCalculateBusinessObject);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.JobInvoicing.JobChargeQuickCalculateBusinessObject";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "JobChargeQuickCalculateForm";
			this.Controls.SetChildIndex(this.containersGrid, 0);
			this.Controls.SetChildIndex(this.QuantityEdit, 0);
			this.Controls.SetChildIndex(this.MeasurementBasisDropEdit, 0);
			this.Controls.SetChildIndex(this.RateCalcEditSell, 0);
			this.Controls.SetChildIndex(this.RateCalcEditCost, 0);
			this.Controls.SetChildIndex(this.SellTotalCalcEdit, 0);
			this.Controls.SetChildIndex(this.CostTotalCalcEdit, 0);
			this.Controls.SetChildIndex(this.CostMinimumCalcEdit, 0);
			this.Controls.SetChildIndex(this.SellMinimumCalcEdit, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.zButton1, 0);
			this.Controls.SetChildIndex(this.zButton2, 0);
			this.Controls.SetChildIndex(this.UpdateSellCheckBox, 0);
			this.Controls.SetChildIndex(this.UpdateCostCheckBox, 0);
			this.Controls.SetChildIndex(this.ChargeCodeFindBox, 0);
			this.Controls.SetChildIndex(this.IsMinimumCheckBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MeasurementBasisDropEdit.ResumeLayout(true);
			this.MeasurementBasisDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.containersGrid)).EndInit();
			this.containersGrid.ResumeLayout(false);
			this.containersGrid.PerformLayout();
			this.ChargeCodeFindBox.ResumeLayout(true);
			this.ChargeCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}