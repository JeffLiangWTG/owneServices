using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class AUExportInvoiceLineUserControl
	{
		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.JI_AUStateBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JI_AUStateBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.JI_PermitNumberBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JI_TempImportNumBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AssayCodeBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.dangerousGoodsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.dGGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.dGLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.flashPointDescLabel = new Enterprise.ZArchitecture.ZLabel();
			this.flashPointCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.uNDGContactGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.JI_TariffFindBoxAHECC = new Enterprise.Customs.AU.Declaration.GUI.AHECCFindBox();
			this.JI_TariffFindBox = new Enterprise.Customs.AU.Declaration.GUI.UniversalTariffExportFindBox();
			this.InvoiceLinesSummaryGroupBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.LineDetailTabControl.SuspendLayout();
			this.InvoiceDetailsGroupBox.SuspendLayout();
			this.ClassificationDetailsGroupBox.SuspendLayout();
			this.CurrentInvoicePanel.SuspendLayout();
			this.LineSummaryPanel.SuspendLayout();
			this.ContainersTabPage.SuspendLayout();
			this.ContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).BeginInit();
			this.LineDetailsTabPage.SuspendLayout();
			this.ClassificationPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.dangerousGoodsTabPage.SuspendLayout();
			this.SuspendLayout();
			//
			// InvoiceLinesSummaryGroupBox
			//
			this.InvoiceLinesSummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 248, true);
			this.InvoiceLinesSummaryGroupBox.TabIndex = 0;
			//
			// BottomPanel
			//
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 328, true);
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 248, true);
			this.BottomPanel.TabIndex = 0;
			//
			// TopPanel
			//
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 576, true);
			//
			// LineDetailTabControl
			//
			this.LineDetailTabControl.Controls.Add(this.dangerousGoodsTabPage);
			this.LineDetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(712, 248, true);
			this.LineDetailTabControl.TabIndex = 1;
			this.LineDetailTabControl.Controls.SetChildIndex(this.ContainersTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.dangerousGoodsTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.LineChargesTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.LineDetailsTabPage, 0);
			//
			// InvoiceDetailsGroupBox
			//
			this.InvoiceDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.InvoiceDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.InvoiceDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 125, true);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_LinePriceBoundCurrencyControl, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_DescriptionBoundTextBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.InvoiceQuantityCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_WeightCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_CountryOfOriginBoundFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.VolumeCalcDropEdit, 0);
			//
			// JI_CountryOfOriginBoundFindBox
			//
			this.JI_CountryOfOriginBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 40, true);
			//
			// ClassificationDetailsGroupBox
			//
			this.ClassificationDetailsGroupBox.Controls.Add(this.JI_AUStateBoundTextBox);
			this.ClassificationDetailsGroupBox.Controls.Add(this.JI_AUStateBoundButton);
			this.ClassificationDetailsGroupBox.Controls.Add(this.JI_PermitNumberBoundTextBox);
			this.ClassificationDetailsGroupBox.Controls.Add(this.JI_TempImportNumBoundTextBox);
			this.ClassificationDetailsGroupBox.Controls.Add(this.AssayCodeBoundButton);
			this.ClassificationDetailsGroupBox.Controls.Add(this.JI_TariffFindBoxAHECC);
			this.ClassificationDetailsGroupBox.Controls.Add(this.JI_TariffFindBox);
			this.ClassificationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 125, true);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.JI_TariffFindBox, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.JI_TariffFindBoxAHECC, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.AssayCodeBoundButton, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.JI_TempImportNumBoundTextBox, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.JI_PermitNumberBoundTextBox, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.JI_AUStateBoundTextBox, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.JI_AUStateBoundButton, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.CustomsQuantityCalcDropEdit, 0);
			//
			// LineChargesTabPage
			//
			this.LineChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 221, true);
			//
			// LineSummaryPanel
			//
			this.LineSummaryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 145, true);
			//
			// ContainersTabPage
			//
			this.ContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 221, true);
			//
			// ContainersGroupBox
			//
			this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 221, true);
			//
			// CusContainerInvoiceLineGrid
			//
			this.CusContainerInvoiceLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(698, 202, true);
			//
			// CantCreateInvoiceLinesLabel
			//
			this.CantCreateInvoiceLinesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 576, true);
			//
			// LineDetailsTabPage
			//
			this.LineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 221, true);
			//
			// ClassificationPanel
			//
			this.ClassificationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClassificationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 125, true);
			//
			// CustomsInvoiceLinesBoundGrid
			//
			zTextBoxColumnStyleInfo1.Caption = "Merged Ln #";
			zTextBoxColumnStyleInfo1.ColumnName = "MergedLineNumber";
			zTextBoxColumnStyleInfo1.ToolTip = "Merged Line No";
			zDropEditColumnStyleInfo1.BindToList = "JI_AUStatesList";
			zDropEditColumnStyleInfo1.Caption = "AU State";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUExportInvoiceLineUserControl|5ca6fa2e-adb9-48de-aedd-f0928b676c3f", "AU State", "State of Origin for this line");
			zDropEditColumnStyleInfo1.ColumnName = "JI_AUState";
			zCheckBoxColumnStyleInfo1.Caption = "Drawback";
			zCheckBoxColumnStyleInfo1.ColumnName = "JI_Drawback";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo2.Caption = "Texco";
			zCheckBoxColumnStyleInfo2.ColumnName = "JI_Texco";
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo3.Caption = "Motor Vehicle Plan";
			zCheckBoxColumnStyleInfo3.ColumnName = "JI_MotorVehiclePlan";
			zCheckBoxColumnStyleInfo3.IsVisible = false;
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.CustomsInvoiceLinesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 576, true);
			//
			// JI_Calc_CIFConvertToLocalCurrencyControl
			//
			this.JI_Calc_CIFConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 78, true);
			//
			// JI_Calc_InsuranceConvertToLocalCurrencyControl
			//
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 57, true);
			//
			// JI_Calc_FreightConvertToLocalCurrencyControl
			//
			this.JI_Calc_FreightConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 37, true);
			//
			// JI_Calc_FOBConvertToLocalCurrencyControl
			//
			this.JI_Calc_FOBConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 16, true);
			//
			// JI_Calc_GSTConvertToLocalCurrencyControl
			//
			this.JI_Calc_GSTConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 248, true);
			//
			// JI_Calc_DutyConvertToLocalCurrencyControl
			//
			this.JI_Calc_DutyConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 224, true);
			//
			// JI_Calc_BalanceConvertToLocalCurrencyControl
			//
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 57, true);
			//
			// JI_Calc_LinesEnteredConvertToLocalCurrencyControl
			//
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 37, true);
			//
			// JI_Calc_LinesTotalConvertToLocalCurrencyControl
			//
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 16, true);
			//
			// CustomsQuantityCalcDropEdit
			//
			this.CustomsQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 40, true);
			this.CustomsQuantityCalcDropEdit.TabIndex = 3;
			//
			// VolumeCalcDropEdit
			//
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(572, 40, true);
			//
			// JI_WeightCalcDropEdit
			//
			this.JI_WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 64, true);
			//
			// JI_DescriptionBoundTextBox
			//
			this.JI_DescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(596, 23, true);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobDeclaration);
			//
			// JI_AUStateBoundTextBox
			//
			this.BindingSource.SetBindingMember(this.JI_AUStateBoundTextBox, "FilteredInvoiceLines.JI_AUState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_AUState)));
			this.JI_AUStateBoundTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUQuarantineInvoiceLineUserControl|e0e31d88-7cf3-455d-aa4c-260c31e23a2c", "AU State");
			this.JI_AUStateBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(472, 40, true);
			this.JI_AUStateBoundTextBox.Name = "JI_AUStateBoundTextBox";
			this.JI_AUStateBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.JI_AUStateBoundTextBox.TabIndex = 5;
			//
			// JI_AUStateBoundButton
			//
			this.JI_AUStateBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(632, 40, true);
			this.JI_AUStateBoundButton.Name = "JI_AUStateBoundButton";
			this.JI_AUStateBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.JI_AUStateBoundButton.TabIndex = 14;
			this.JI_AUStateBoundButton.Text = "More...";
			this.JI_AUStateBoundButton.Click += new System.EventHandler(this.JI_AUStateBoundButton_Click);
			//
			// JI_PermitNumberBoundTextBox
			//
			this.BindingSource.SetBindingMember(this.JI_PermitNumberBoundTextBox, "FilteredInvoiceLines.AddInfo+ZA_PermitNumbers_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfo.ZA_PermitNumbers_Hidden)));
			this.JI_PermitNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 64, true);
			this.JI_PermitNumberBoundTextBox.Name = "JI_PermitNumberBoundTextBox";
			this.JI_PermitNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.JI_PermitNumberBoundTextBox.TabIndex = 7;
			//
			// JI_TempImportNumBoundTextBox
			//
			this.BindingSource.SetBindingMember(this.JI_TempImportNumBoundTextBox, "FilteredInvoiceLines.JI_TempImportNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_TempImportNum)));
			this.JI_TempImportNumBoundTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUExportInvoiceLineUserControl|27d02ca5-0ca8-4933-8f10-7a0c025ced43", "Temporary Import Number");
			this.JI_TempImportNumBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 64, true);
			this.JI_TempImportNumBoundTextBox.Name = "JI_TempImportNumBoundTextBox";
			this.JI_TempImportNumBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.JI_TempImportNumBoundTextBox.TabIndex = 9;
			//
			// AssayCodeBoundButton
			//
			this.AssayCodeBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(608, 86, true);
			this.AssayCodeBoundButton.Name = "AssayCodeBoundButton";
			this.AssayCodeBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 36, true);
			this.AssayCodeBoundButton.TabIndex = 10;
			this.AssayCodeBoundButton.Text = "Assay Codes";
			this.AssayCodeBoundButton.Click += new System.EventHandler(this.AssayCodeBoundButton_Click);
			//
			// DangerousGoodsTabPage
			//
			this.dangerousGoodsTabPage.Controls.Add(this.dGGuidFindBox);
			this.dangerousGoodsTabPage.Controls.Add(this.dGLinkLabel);
			this.dangerousGoodsTabPage.Controls.Add(this.flashPointDescLabel);
			this.dangerousGoodsTabPage.Controls.Add(this.flashPointCalcEdit);
			this.dangerousGoodsTabPage.Controls.Add(this.uNDGContactGuidFindBox);
			this.dangerousGoodsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.dangerousGoodsTabPage.Name = "DangerousGoodsTabPage";
			this.dangerousGoodsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 221, true);
			this.dangerousGoodsTabPage.TabIndex = 2;
			this.dangerousGoodsTabPage.Text = "Dangerous Goods";
			//
			// DGGuidFindBox
			//
			this.BindingSource.SetBindingMember(this.dGGuidFindBox, "FilteredInvoiceLines.UNDGs+FirstItemForBinding.DI_DG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).UNDGs.FirstItemForBinding)).SyncRoot)).DI_DG)));
			this.dGGuidFindBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUExportInvoiceLineUserControl|250490f9-4713-4cad-a8e3-5a72402ff7f7", "UNDG Number");
			this.dGGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 17, true);
			this.dGGuidFindBox.Name = "DGGuidFindBox";
			this.dGGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.dGGuidFindBox.TabIndex = 1;
			//
			// DGLinkLabel
			//
			this.dGLinkLabel.AutoSize = true;
			this.dGLinkLabel.IsFontBold = false;
			this.dGLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 21, true);
			this.dGLinkLabel.Name = "DGLinkLabel";
			this.dGLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 13, true);
			this.dGLinkLabel.TabIndex = 15;
			this.dGLinkLabel.Text = "Dangerous Goods Details";
			//
			// FlashPointDescLabel
			//
			this.flashPointDescLabel.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUExportInvoiceLineUserControl|7de832b0-464b-4809-9e06-1fe145539233", "(Manufacturer Specified)");
			this.flashPointDescLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 40, true);
			this.flashPointDescLabel.Name = "FlashPointDescLabel";
			this.flashPointDescLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 23, true);
			this.flashPointDescLabel.TabIndex = 10;
			//
			// FlashPointCalcEdit
			//
			this.BindingSource.SetBindingMember(this.flashPointCalcEdit, "FilteredInvoiceLines.UNDGs+FirstItemForBinding.DI_DGFlashPoint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).UNDGs.FirstItemForBinding)).SyncRoot)).DI_DGFlashPoint)));
			this.flashPointCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 40, true);
			this.flashPointCalcEdit.Name = "FlashPointCalcEdit";
			this.flashPointCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.flashPointCalcEdit.TabIndex = 9;
			this.flashPointCalcEdit.Text = "0.0";
			this.flashPointCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// UNDGContactGuidFindBox
			//
			this.BindingSource.SetBindingMember(this.uNDGContactGuidFindBox, "FilteredInvoiceLines.UNDGs+FirstItemForBinding.DI_OC_DGContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).UNDGs.FirstItemForBinding)).SyncRoot)).DI_OC_DGContact)));
			this.uNDGContactGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 64, true);
			this.uNDGContactGuidFindBox.Name = "UNDGContactGuidFindBox";
			this.uNDGContactGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 21, true);
			this.uNDGContactGuidFindBox.TabIndex = 14;
			//
			// JI_TariffFindBoxAHECC
			//
			this.BindingSource.SetBindingMember(this.JI_TariffFindBoxAHECC, "FilteredInvoiceLines.JI_Tariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Tariff)));
			this.JI_TariffFindBoxAHECC.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 17, true);
			this.JI_TariffFindBoxAHECC.Name = "JI_TariffFindBoxAHECC";
			this.JI_TariffFindBoxAHECC.PreBoundMaxLength = 10;
			this.JI_TariffFindBoxAHECC.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 21, true);
			this.JI_TariffFindBoxAHECC.TabIndex = 1;
			this.JI_TariffFindBoxAHECC.Visible = false;
			// 
			// JI_TariffFindBox
			// 
			this.BindingSource.SetBindingMember(this.JI_TariffFindBox, "FilteredInvoiceLines.JI_Tariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Tariff)));
			this.JI_TariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 17, true);
			this.JI_TariffFindBox.Name = "JI_TariffFindBox";
			this.JI_TariffFindBox.PreBoundMaxLength = 10;
			this.JI_TariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 21, true);
			this.JI_TariffFindBox.TabIndex = 1;
			this.JI_TariffFindBox.Visible = false;
			//
			// AUExportInvoiceLineUserControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Name = "AUExportInvoiceLineUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 576, true);
			this.InvoiceLinesSummaryGroupBox.ResumeLayout(false);
			this.BottomPanel.ResumeLayout(false);
			this.TopPanel.ResumeLayout(false);
			this.LineDetailTabControl.ResumeLayout(false);
			this.InvoiceDetailsGroupBox.ResumeLayout(false);
			this.ClassificationDetailsGroupBox.ResumeLayout(false);
			this.ClassificationDetailsGroupBox.PerformLayout();
			this.CurrentInvoicePanel.ResumeLayout(false);
			this.LineSummaryPanel.ResumeLayout(false);
			this.ContainersTabPage.ResumeLayout(false);
			this.ContainersGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).EndInit();
			this.LineDetailsTabPage.ResumeLayout(false);
			this.ClassificationPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.dangerousGoodsTabPage.ResumeLayout(false);
			this.dangerousGoodsTabPage.PerformLayout();
			this.ResumeLayout(false);
		}
		#endregion

		protected internal ZTextBox JI_AUStateBoundTextBox;
		protected internal ZButton JI_AUStateBoundButton;
		protected internal Enterprise.ZArchitecture.ZTextBox JI_PermitNumberBoundTextBox;
		protected internal Enterprise.ZArchitecture.ZTextBox JI_TempImportNumBoundTextBox;
		protected internal Enterprise.Customs.AU.Declaration.GUI.AHECCFindBox JI_TariffFindBoxAHECC;
		protected internal Enterprise.Customs.AU.Declaration.GUI.UniversalTariffExportFindBox JI_TariffFindBox;
		protected internal Enterprise.ZArchitecture.GUI.ZButton AssayCodeBoundButton;
		private Enterprise.ZArchitecture.GUI.ZTabPage dangerousGoodsTabPage;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox uNDGContactGuidFindBox;
		private Enterprise.ZArchitecture.ZCalcEdit flashPointCalcEdit;
		private ZGuidFindBox dGGuidFindBox;
		private ZLinkLabel dGLinkLabel;
		private Enterprise.ZArchitecture.ZLabel flashPointDescLabel;
	}
}
