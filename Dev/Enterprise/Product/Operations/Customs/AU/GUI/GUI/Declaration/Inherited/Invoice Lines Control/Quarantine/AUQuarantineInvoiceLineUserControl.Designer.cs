using Enterprise.Customs.AU.GUI;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUQuarantineInvoiceLineUserControl
	{
		#region Component Designer generated code

		private void InitializeComponent()
		{
			this.RFPPackagesUserControl = new RFPPackagesUserControl();
			this.RFPCertificatesUserControl = new Enterprise.Customs.AU.Declaration.GUI.RFPCertificatesUserControl();
			this.DangerousGoodsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DGGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DGLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.FlashPointDescLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FlashPointCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.UNDGContactGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.RFPDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RFPDetailsUserControl = new Enterprise.Customs.AU.Declaration.GUI.InvoiceLineRFPDetailsUserControl();
			this.RFPPackagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RFPProcessTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RFPProcessUserControl = new Enterprise.Customs.AU.Declaration.GUI.RFPProcessUserControl();
			this.RFPAnalysisTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RFPAnalysisUserControl = new Enterprise.Customs.AU.GUI.RFPAnalysisUserControl();
			this.RFPMeatTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RFPMeatUserControl = new Enterprise.Customs.AU.Declaration.GUI.RFPMeatUserControl();
			this.RFPCertificatesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RFPStatementsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RFPStatementsUserControl = new Enterprise.Customs.AU.Declaration.GUI.RFPStatementsUserControl();
			this.JI_AUStateBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JI_AUStateBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.JI_TempImportNumBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AssayCodeBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.JI_TempImportDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JI_TariffFindBoxAHECC = new Enterprise.Customs.AU.Declaration.GUI.AHECCFindBox();
			this.JI_TariffFindBox = new Enterprise.Customs.AU.Declaration.GUI.UniversalTariffExportFindBox();
			this.RFPNumbersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RFPNumbersUserControl = new Enterprise.Customs.AU.Declaration.GUI.RFPNumbersUserControl();
			this.REXProductAttachmentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.REXProductAttachmentsUserControl = new Enterprise.Customs.AU.Declaration.GUI.REXProductAttachmentsUserControl();
			this.zCalcDropEditAqisCustomsWeight = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.NoPermitRequiredCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.InvoiceLinesSummaryGroupBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.LineDetailTabControl.SuspendLayout();
			this.InvoiceDetailsGroupBox.SuspendLayout();
			this.JI_CountryOfOriginBoundFindBox.SuspendLayout();
			this.JI_RH_NKCommodity_CodeBoundFindBox.SuspendLayout();
			this.JI_LinePriceBoundCurrencyControl.SuspendLayout();
			this.ClassificationDetailsGroupBox.SuspendLayout();
			this.LineChargesTabPage.SuspendLayout();
			this.CurrentInvoicePanel.SuspendLayout();
			this.LineSummaryPanel.SuspendLayout();
			this.ContainersTabPage.SuspendLayout();
			this.ContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).BeginInit();
			this.CusContainerInvoiceLineGrid.SuspendLayout();
			this.LineDetailsTabPage.SuspendLayout();
			this.NewLineDetailsTabPage.SuspendLayout();
			this.InvoiceLineDetailsUserControl.SuspendLayout();
			this.ClassificationPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).BeginInit();
			this.CustomsInvoiceLinesBoundGrid.SuspendLayout();
			this.JI_Calc_CIFConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_FreightConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_FOBConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_GSTConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_DutyConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.SuspendLayout();
			this.CustomsQuantityCalcDropEdit.SuspendLayout();
			this.VolumeCalcDropEdit.SuspendLayout();
			this.JI_WeightCalcDropEdit.SuspendLayout();
			this.InvoiceQuantityCalcDropEdit.SuspendLayout();
			this.JI_DescriptionBoundTextBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DangerousGoodsTabPage.SuspendLayout();
			this.DGGuidFindBox.SuspendLayout();
			this.UNDGContactGuidFindBox.SuspendLayout();
			this.RFPDetailsTabPage.SuspendLayout();
			this.RFPDetailsUserControl.SuspendLayout();
			this.RFPPackagesTabPage.SuspendLayout();
			this.RFPPackagesUserControl.SuspendLayout();
			this.RFPProcessTabPage.SuspendLayout();
			this.RFPProcessUserControl.SuspendLayout();
			this.RFPAnalysisTabPage.SuspendLayout();
			this.RFPAnalysisUserControl.SuspendLayout();
			this.RFPMeatTabPage.SuspendLayout();
			this.RFPMeatUserControl.SuspendLayout();
			this.RFPCertificatesTabPage.SuspendLayout();
			this.RFPCertificatesUserControl.SuspendLayout();
			this.RFPStatementsTabPage.SuspendLayout();
			this.RFPStatementsUserControl.SuspendLayout();
			this.JI_TempImportDateDateEdit.SuspendLayout();
			this.JI_TariffFindBoxAHECC.SuspendLayout();
			this.JI_TariffFindBox.SuspendLayout();
			this.RFPNumbersTabPage.SuspendLayout();
			this.RFPNumbersUserControl.SuspendLayout();
			this.REXProductAttachmentsTabPage.SuspendLayout();
			this.REXProductAttachmentsUserControl.SuspendLayout();
			this.zCalcDropEditAqisCustomsWeight.SuspendLayout();
			this.SuspendLayout();
			// 
			// InvoiceLinesSummaryGroupBox
			// 
			this.InvoiceLinesSummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(759, 0, true);
			this.InvoiceLinesSummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 292, true);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 236, true);
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1015, 292, true);
			// 
			// TopPanel
			// 
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 50, true);
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1015, 478, true);
			// 
			// LineDetailTabControl
			// 
			this.LineDetailTabControl.Controls.Add(this.RFPPackagesTabPage);
			this.LineDetailTabControl.Controls.Add(this.DangerousGoodsTabPage);
			this.LineDetailTabControl.Controls.Add(this.RFPDetailsTabPage);
			this.LineDetailTabControl.Controls.Add(this.RFPProcessTabPage);
			this.LineDetailTabControl.Controls.Add(this.RFPCertificatesTabPage);
			this.LineDetailTabControl.Controls.Add(this.RFPAnalysisTabPage);
			this.LineDetailTabControl.Controls.Add(this.RFPMeatTabPage);
			this.LineDetailTabControl.Controls.Add(this.RFPStatementsTabPage);
			this.LineDetailTabControl.Controls.Add(this.RFPNumbersTabPage);
			this.LineDetailTabControl.Controls.Add(this.REXProductAttachmentsTabPage);
			this.LineDetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 292, true);
			this.LineDetailTabControl.Controls.SetChildIndex(this.NewLineDetailsTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.REXProductAttachmentsTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.RFPNumbersTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.RFPStatementsTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.RFPMeatTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.RFPAnalysisTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.RFPCertificatesTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.RFPProcessTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.RFPDetailsTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.DangerousGoodsTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.RFPPackagesTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.LineChargesTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.ContainersTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.LineDetailsTabPage, 0);
			// 
			// InvoiceDetailsGroupBox
			// 
			this.InvoiceDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 174, true);
			this.InvoiceDetailsGroupBox.TabIndex = 1;
			// 
			// JI_CountryOfOriginBoundFindBox
			// 
			this.JI_CountryOfOriginBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 40, true);
			// 
			// ClassificationDetailsGroupBox
			// 
			this.ClassificationDetailsGroupBox.Controls.Add(this.NoPermitRequiredCheckBox);
			this.ClassificationDetailsGroupBox.Controls.Add(this.zCalcDropEditAqisCustomsWeight);
			this.ClassificationDetailsGroupBox.Controls.Add(this.JI_TariffFindBoxAHECC);
			this.ClassificationDetailsGroupBox.Controls.Add(this.JI_TariffFindBox);
			this.ClassificationDetailsGroupBox.Controls.Add(this.JI_AUStateBoundButton);
			this.ClassificationDetailsGroupBox.Controls.Add(this.JI_AUStateBoundTextBox);
			this.ClassificationDetailsGroupBox.Controls.Add(this.AssayCodeBoundButton);
			this.ClassificationDetailsGroupBox.Controls.Add(this.JI_TempImportDateDateEdit);
			this.ClassificationDetailsGroupBox.Controls.Add(this.JI_TempImportNumBoundTextBox);
			this.ClassificationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 174, true);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.JI_TempImportNumBoundTextBox, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.JI_TempImportDateDateEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.AssayCodeBoundButton, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.JI_AUStateBoundTextBox, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.JI_AUStateBoundButton, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.JI_TariffFindBox, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.JI_TariffFindBoxAHECC, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zCalcDropEditAqisCustomsWeight, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.CustomsQuantityCalcDropEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.NoPermitRequiredCheckBox, 0);
			// 
			// LineChargesTabPage
			// 
			this.LineChargesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LineChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 270, true);
			// 
			// LineSummaryPanel
			// 
			this.LineSummaryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 162, true);
			// 
			// PendingApportionmentLabel
			// 
			this.PendingApportionmentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 316, true);
			// 
			// ContainersTabPage
			// 
			this.ContainersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.ContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 270, true);
			// 
			// ContainersGroupBox
			// 
			this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 270, true);
			// 
			// CusContainerInvoiceLineGrid
			// 
			this.CusContainerInvoiceLineGrid.Dock = System.Windows.Forms.DockStyle.Top;
			this.CusContainerInvoiceLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 169, true);
			// 
			// CantCreateInvoiceLinesLabel
			// 
			this.CantCreateInvoiceLinesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 50, true);
			this.CantCreateInvoiceLinesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1015, 478, true);
			// 
			// LineDetailsTabPage
			// 
			this.LineDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 270, true);
			// 
			// NewLineDetailsTabPage
			// 
			this.NewLineDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NewLineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 270, true);
			// 
			// InvoiceLineDetailsUserControl
			// 
			this.InvoiceLineDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 270, true);
			// 
			// ClassificationPanel
			// 
			this.ClassificationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClassificationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 174, true);
			// 
			// CustomsInvoiceLinesBoundGrid
			// 
			this.CustomsInvoiceLinesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1015, 478, true);
			// 
			// JI_Calc_CIFConvertToLocalCurrencyControl
			// 
			this.JI_Calc_CIFConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 88, true);
			// 
			// JI_Calc_InsuranceConvertToLocalCurrencyControl
			// 
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 64, true);
			// 
			// JI_Calc_FreightConvertToLocalCurrencyControl
			// 
			this.JI_Calc_FreightConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 40, true);
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
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 62, true);
			// 
			// JI_Calc_LinesEnteredConvertToLocalCurrencyControl
			// 
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 40, true);
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
			this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 17, true);
			// 
			// JI_WeightCalcDropEdit
			// 
			this.JI_WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 64, true);
			// 
			// JI_DescriptionBoundTextBox
			// 
			this.JI_DescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 23, true);
			// 
			// Splitter
			// 
			this.Splitter.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.Splitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.Splitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 226, true);
			this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1015, 10, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobDeclaration);
			// 
			// DangerousGoodsTabPage
			// 
			this.DangerousGoodsTabPage.Controls.Add(this.DGGuidFindBox);
			this.DangerousGoodsTabPage.Controls.Add(this.DGLinkLabel);
			this.DangerousGoodsTabPage.Controls.Add(this.FlashPointDescLabel);
			this.DangerousGoodsTabPage.Controls.Add(this.FlashPointCalcEdit);
			this.DangerousGoodsTabPage.Controls.Add(this.UNDGContactGuidFindBox);
			this.DangerousGoodsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.DangerousGoodsTabPage.Name = "DangerousGoodsTabPage";
			this.DangerousGoodsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 270, true);
			this.DangerousGoodsTabPage.TabIndex = 2;
			this.DangerousGoodsTabPage.Text = "Dangerous Goods";
			// 
			// DGGuidFindBox
			// 
			this.DGGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DGGuidFindBox, "FilteredInvoiceLines.UNDGs+FirstItemForBinding.DI_DG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).UNDGs.FirstItemForBinding)).SyncRoot)).DI_DG)));
			this.DGGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 9, true);
			this.DGGuidFindBox.Name = "DGGuidFindBox";
			this.DGGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DGGuidFindBox.ParentType = null;
			this.DGGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 17, true);
			this.DGGuidFindBox.TabIndex = 1;
			// 
			// DGLinkLabel
			// 
			this.DGLinkLabel.AutoSize = true;
			this.DGLinkLabel.IsFontBold = false;
			this.DGLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 13, true);
			this.DGLinkLabel.Name = "DGLinkLabel";
			this.DGLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 13, true);
			this.DGLinkLabel.TabIndex = 23;
			this.DGLinkLabel.Text = "Dangerous Goods Details";
			// 
			// FlashPointDescLabel
			// 
			this.FlashPointDescLabel.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUQuarantineInvoiceLineUserControl|7de832b0-464b-4809-9e06-1fe145539233", "(Manufacturer Specified)");
			this.FlashPointDescLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FlashPointDescLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 29, true);
			this.FlashPointDescLabel.Name = "FlashPointDescLabel";
			this.FlashPointDescLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 23, true);
			this.FlashPointDescLabel.TabIndex = 20;
			// 
			// FlashPointCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.FlashPointCalcEdit, "FilteredInvoiceLines.UNDGs+FirstItemForBinding.DI_DGFlashPoint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).UNDGs.FirstItemForBinding)).SyncRoot)).DI_DGFlashPoint)));
			this.FlashPointCalcEdit.CaptionResourceString = null;
			this.FlashPointCalcEdit.DecimalPlaces = 2;
			this.FlashPointCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 32, true);
			this.FlashPointCalcEdit.Name = "FlashPointCalcEdit";
			this.FlashPointCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 17, true);
			this.FlashPointCalcEdit.TabIndex = 2;
			this.FlashPointCalcEdit.Text = "0.0";
			this.FlashPointCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// UNDGContactGuidFindBox
			// 
			this.UNDGContactGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UNDGContactGuidFindBox, "FilteredInvoiceLines.UNDGs+FirstItemForBinding.DI_OC_DGContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).UNDGs.FirstItemForBinding)).SyncRoot)).DI_OC_DGContact)));
			this.UNDGContactGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 56, true);
			this.UNDGContactGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrgContacts;
			this.UNDGContactGuidFindBox.Name = "UNDGContactGuidFindBox";
			this.UNDGContactGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.UNDGContactGuidFindBox.ParentType = null;
			this.UNDGContactGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 17, true);
			this.UNDGContactGuidFindBox.TabIndex = 3;
			// 
			// RFPDetailsTabPage
			// 
			this.RFPDetailsTabPage.Controls.Add(this.RFPDetailsUserControl);
			this.RFPDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.RFPDetailsTabPage.Name = "RFPDetailsTabPage";
			this.RFPDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RFPDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(885, 270, true);
			this.RFPDetailsTabPage.TabIndex = 4;
			this.RFPDetailsTabPage.Text = "RFP Details";
			// 
			// RFPDetailsUserControl
			// 
			this.RFPDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RFPDetailsUserControl, "FilteredInvoiceLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)))));
			this.RFPDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RFPDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RFPDetailsUserControl.Name = "RFPDetailsUserControl";
			this.RFPDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(885, 265, true);
			this.RFPDetailsUserControl.TabIndex = 0;
			// 
			// RFPPackagesTabPage
			// 
			this.RFPPackagesTabPage.Controls.Add(this.RFPPackagesUserControl);
			this.RFPPackagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.RFPPackagesTabPage.Name = "RFPPackagesTabPage";
			this.RFPPackagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RFPPackagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 270, true);
			this.RFPPackagesTabPage.TabIndex = 5;
			this.RFPPackagesTabPage.Text = "RFP Packages";
			// 
			// RFPPackagesUserControl
			// 
			this.RFPPackagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RFPPackagesUserControl, "FilteredInvoiceLines");
			this.RFPPackagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RFPPackagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.RFPPackagesUserControl.Name = "RFPPackagesUserControl";
			this.RFPPackagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 265, true);
			this.RFPPackagesUserControl.TabIndex = 0;
			// 
			// RFPProcessTabPage
			//
			this.RFPProcessTabPage.Controls.Add(this.RFPProcessUserControl);
			this.RFPProcessTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.RFPProcessTabPage.Name = "RFPProcessTabPage";
			this.RFPProcessTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RFPProcessTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 270, true);
			this.RFPProcessTabPage.TabIndex = 6;
			this.RFPProcessTabPage.Text = "RFP Process";
			// 
			// RFPProcessUserControl
			// 
			this.RFPProcessUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RFPProcessUserControl, ".");
			this.RFPProcessUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RFPProcessUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.RFPProcessUserControl.Name = "RFPProcessUserControl";
			this.RFPProcessUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 265, true);
			this.RFPProcessUserControl.TabIndex = 0;
			// 
			// RFPAnalysisTabPage
			// 
			this.RFPAnalysisTabPage.Controls.Add(this.RFPAnalysisUserControl);
			this.RFPAnalysisTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.RFPAnalysisTabPage.Name = "RFPAnalysisTabPage";
			this.RFPAnalysisTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RFPAnalysisTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 270, true);
			this.RFPAnalysisTabPage.TabIndex = 7;
			this.RFPAnalysisTabPage.Text = "RFP Analysis";
			// 
			// RFPAnalysisUserControl
			// 
			this.RFPAnalysisUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RFPAnalysisUserControl, "FilteredInvoiceLines");
			this.RFPAnalysisUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RFPAnalysisUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.RFPAnalysisUserControl.Name = "RFPAnalysisUserControl";
			this.RFPAnalysisUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 265, true);
			this.RFPAnalysisUserControl.TabIndex = 0;
			// 
			// RFPMeatTabPage
			// 
			this.RFPMeatTabPage.Controls.Add(this.RFPMeatUserControl);
			this.RFPMeatTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.RFPMeatTabPage.Name = "RFPMeatTabPage";
			this.RFPMeatTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RFPMeatTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 270, true);
			this.RFPMeatTabPage.TabIndex = 8;
			this.RFPMeatTabPage.Text = "RFP Meat";
			//
			// RFPMeatUserControl
			// 
			this.RFPMeatUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RFPMeatUserControl, "FilteredInvoiceLines");
			this.RFPMeatUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RFPMeatUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.RFPMeatUserControl.Name = "RFPMeatUserControl";
			this.RFPMeatUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 265, true);
			this.RFPMeatUserControl.TabIndex = 0;
			// 
			// RFPCertificatesTabPage
			// 
			this.RFPCertificatesTabPage.Controls.Add(this.RFPCertificatesUserControl);
			this.RFPCertificatesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.RFPCertificatesTabPage.Name = "RFPCertificatesTabPage";
			this.RFPCertificatesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RFPCertificatesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 270, true);
			this.RFPCertificatesTabPage.TabIndex = 9;
			this.RFPCertificatesTabPage.Text = "RFP Certificates";
			// 
			// RFPCertificatesUserControl
			// 
			this.RFPCertificatesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RFPCertificatesUserControl, "FilteredInvoiceLines");
			this.RFPCertificatesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RFPCertificatesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.RFPCertificatesUserControl.Name = "RFPCertificatesUserControl";
			this.RFPCertificatesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 265, true);
			this.RFPCertificatesUserControl.TabIndex = 0;
			// 
			// RFPStatementsTabPage
			// 
			this.RFPStatementsTabPage.Controls.Add(this.RFPStatementsUserControl);
			this.RFPStatementsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.RFPStatementsTabPage.Name = "RFPStatementsTabPage";
			this.RFPStatementsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RFPStatementsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 270, true);
			this.RFPStatementsTabPage.TabIndex = 9;
			this.RFPStatementsTabPage.Text = "RFP Statements";
			//
			// RFPStatementsUserControl
			// 
			this.RFPStatementsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RFPStatementsUserControl, "FilteredInvoiceLines");
			this.RFPStatementsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RFPStatementsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.RFPStatementsUserControl.Name = "RFPStatementsUserControl";
			this.RFPStatementsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 265, true);
			this.RFPStatementsUserControl.TabIndex = 0;
			// 
			// JI_AUStateBoundTextBox
			// JI_AUStateBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.JI_AUStateBoundTextBox, "FilteredInvoiceLines.JI_AUState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_AUState)));
			this.JI_AUStateBoundTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUQuarantineInvoiceLineUserControl|e0e31d88-7cf3-455d-aa4c-260c31e23a2c", "AU State");
			this.JI_AUStateBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 63, true);
			this.JI_AUStateBoundTextBox.Name = "JI_AUStateBoundTextBox";
			this.JI_AUStateBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 17, true);
			this.JI_AUStateBoundTextBox.TabIndex = 4;
			// 
			// JI_AUStateBoundButton
			// 
			this.JI_AUStateBoundButton.IsCaptionOverridden = true;
			this.JI_AUStateBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 63, true);
			this.JI_AUStateBoundButton.Name = "JI_AUStateBoundButton";
			this.JI_AUStateBoundButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.JI_AUStateBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.JI_AUStateBoundButton.TabIndex = 5;
			this.JI_AUStateBoundButton.Text = "More...";
			this.JI_AUStateBoundButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.JI_AUStateBoundButton.ToolTipCaption = null;
			this.JI_AUStateBoundButton.Click += new System.EventHandler(this.JI_AUStateBoundButton_Click);
			// 
			// JI_TempImportNumBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.JI_TempImportNumBoundTextBox, "FilteredInvoiceLines.JI_TempImportNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_TempImportNum)));
			this.JI_TempImportNumBoundTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUQuarantineInvoiceLineUserControl|c218f7de-f357-4318-9e94-e9ed26ecd24a", "Temporary Import Number");
			this.JI_TempImportNumBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 40, true);
			this.JI_TempImportNumBoundTextBox.Name = "JI_TempImportNumBoundTextBox";
			this.JI_TempImportNumBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 17, true);
			this.JI_TempImportNumBoundTextBox.TabIndex = 3;
			// 
			// AssayCodeBoundButton
			// 
			this.AssayCodeBoundButton.IsCaptionOverridden = true;
			this.AssayCodeBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(645, 85, true);
			this.AssayCodeBoundButton.Name = "AssayCodeBoundButton";
			this.AssayCodeBoundButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AssayCodeBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 35, true);
			this.AssayCodeBoundButton.TabIndex = 10;
			this.AssayCodeBoundButton.Text = "Assay Codes";
			this.AssayCodeBoundButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.AssayCodeBoundButton.ToolTipCaption = null;
			this.AssayCodeBoundButton.Click += new System.EventHandler(this.AssayCodeBoundButton_Click);
			// 
			// JI_TempImportDateDateEdit
			// 
			this.JI_TempImportDateDateEdit.AllowDrop = true;
			this.JI_TempImportDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.JI_TempImportDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JI_TempImportDateDateEdit, "FilteredInvoiceLines.JI_TempImportDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_TempImportDate)));
			this.JI_TempImportDateDateEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUQuarantineInvoiceLineUserControl|39a36399-c6e1-4611-980e-81bc95978936", "Temporary Import Date");
			this.JI_TempImportDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 64, true);
			this.JI_TempImportDateDateEdit.Name = "JI_TempImportDateDateEdit";
			this.JI_TempImportDateDateEdit.TabIndex = 6;
			// 
			// JI_TariffFindBoxAHECC
			// 
			this.JI_TariffFindBoxAHECC.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JI_TariffFindBoxAHECC, "FilteredInvoiceLines.JI_Tariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Tariff)));
			this.JI_TariffFindBoxAHECC.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 17, true);
			this.JI_TariffFindBoxAHECC.Name = "JI_TariffFindBoxAHECC";
			this.JI_TariffFindBoxAHECC.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JI_TariffFindBoxAHECC.ParentType = null;
			this.JI_TariffFindBoxAHECC.PreBoundMaxLength = 10;
			this.JI_TariffFindBoxAHECC.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(597, 20, true);
			this.JI_TariffFindBoxAHECC.TabIndex = 1;
			this.JI_TariffFindBoxAHECC.Visible = false;
			// 
			// JI_TariffFindBox
			// 
			this.JI_TariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JI_TariffFindBox, "FilteredInvoiceLines.JI_Tariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Tariff)));
			this.JI_TariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 17, true);
			this.JI_TariffFindBox.Name = "JI_TariffFindBox";
			this.JI_TariffFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JI_TariffFindBox.ParentType = null;
			this.JI_TariffFindBox.PreBoundMaxLength = 10;
			this.JI_TariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(597, 20, true);
			this.JI_TariffFindBox.TabIndex = 1;
			this.JI_TariffFindBox.Visible = false;
			// 
			// RFPNumbersTabPage
			// 
			this.RFPNumbersTabPage.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUQuarantineInvoiceLineUserControl|838a3fa7-fb97-4b30-b35a-d26fc6e2c8a3", "RFP Numbers", "RFP Numbers is required for Certificate Request message only.");
			this.RFPNumbersTabPage.Controls.Add(this.RFPNumbersUserControl);
			this.RFPNumbersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.RFPNumbersTabPage.Name = "RFPNumbersTabPage";
			this.RFPNumbersTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RFPNumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 270, true);
			this.RFPNumbersTabPage.TabIndex = 10;
			// 
			// RFPNumbersUserControl
			// 
			this.RFPNumbersUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RFPNumbersUserControl, ".");
			this.RFPNumbersUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RFPNumbersUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.RFPNumbersUserControl.Name = "RFPNumbersUserControl";
			this.RFPNumbersUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 265, true);
			this.RFPNumbersUserControl.TabIndex = 0;
			// 
			// REXProductAttachmentsTabPage
			// 
			this.REXProductAttachmentsTabPage.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUQuarantineInvoiceLineUserControl|DC73221B-20C1-4E22-AF4F-201DD55AB4A7", "REX Product Attachments");
			this.REXProductAttachmentsTabPage.Controls.Add(this.REXProductAttachmentsUserControl);
			this.REXProductAttachmentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.REXProductAttachmentsTabPage.Name = "REXProductAttachmentsTabPage";
			this.REXProductAttachmentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.REXProductAttachmentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 270, true);
			this.REXProductAttachmentsTabPage.TabIndex = 11;
			this.REXProductAttachmentsTabPage.Text = "REX Product Attachments";
			// 
			// REXProductAttachmentsUserControl
			// 
			this.REXProductAttachmentsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.REXProductAttachmentsUserControl, "FilteredInvoiceLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.AU.Declaration.Business.ICusStorageDocPivotParent)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)))));
			this.REXProductAttachmentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.REXProductAttachmentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.REXProductAttachmentsUserControl.Name = "REXProductAttachmentsUserControl";
			this.REXProductAttachmentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 265, true);
			this.REXProductAttachmentsUserControl.TabIndex = 2;
			// 
			// zCalcDropEditAqisCustomsWeight
			// 
			this.zCalcDropEditAqisCustomsWeight.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCalcDropEditAqisCustomsWeight, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.QL_AqisCustomsWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.QL_AqisCustomsWeightUQ)));
			this.zCalcDropEditAqisCustomsWeight.BindToAmount = "FilteredInvoiceLines.QuarantineExDocLine+QL_AqisCustomsWeight";
			this.zCalcDropEditAqisCustomsWeight.BindToUnit = "FilteredInvoiceLines.QuarantineExDocLine+QL_AqisCustomsWeightUQ";
			this.zCalcDropEditAqisCustomsWeight.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUQuarantineInvoiceLineUserControl|44af482b-938e-46dd-b7f7-3a7998ca3f05", "Customs Weight");
			this.zCalcDropEditAqisCustomsWeight.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 90, true);
			this.zCalcDropEditAqisCustomsWeight.Name = "zCalcDropEditAqisCustomsWeight";
			this.zCalcDropEditAqisCustomsWeight.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 17, true);
			this.zCalcDropEditAqisCustomsWeight.TabIndex = 7;
			// 
			// NoPermitRequiredCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NoPermitRequiredCheckBox, "FilteredInvoiceLines.JI_NoPermitRequired");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_NoPermitRequired)));
			this.NoPermitRequiredCheckBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("afd206d6-fb5b-42f3-b470-2816de2d0821", "No Permit Required");
			this.NoPermitRequiredCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.NoPermitRequiredCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NoPermitRequiredCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 90, true);
			this.NoPermitRequiredCheckBox.Name = "NoPermitRequiredCheckBox";
			this.NoPermitRequiredCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.NoPermitRequiredCheckBox.TabIndex = 9;
			this.NoPermitRequiredCheckBox.Text = "No Permit Required";
			this.NoPermitRequiredCheckBox.UseVisualStyleBackColor = true;
			// 
			// AUQuarantineInvoiceLineUserControl
			// 
			this.Name = "AUQuarantineInvoiceLineUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1015, 528, true);
			this.InvoiceLinesSummaryGroupBox.ResumeLayout(false);
			this.InvoiceLinesSummaryGroupBox.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.LineDetailTabControl.ResumeLayout(false);
			this.LineDetailTabControl.PerformLayout();
			this.InvoiceDetailsGroupBox.ResumeLayout(false);
			this.InvoiceDetailsGroupBox.PerformLayout();
			this.JI_CountryOfOriginBoundFindBox.ResumeLayout(true);
			this.JI_CountryOfOriginBoundFindBox.PerformLayout();
			this.JI_RH_NKCommodity_CodeBoundFindBox.ResumeLayout(true);
			this.JI_RH_NKCommodity_CodeBoundFindBox.PerformLayout();
			this.JI_LinePriceBoundCurrencyControl.ResumeLayout(true);
			this.JI_LinePriceBoundCurrencyControl.PerformLayout();
			this.ClassificationDetailsGroupBox.ResumeLayout(false);
			this.ClassificationDetailsGroupBox.PerformLayout();
			this.LineChargesTabPage.ResumeLayout(false);
			this.LineChargesTabPage.PerformLayout();
			this.CurrentInvoicePanel.ResumeLayout(false);
			this.CurrentInvoicePanel.PerformLayout();
			this.LineSummaryPanel.ResumeLayout(false);
			this.LineSummaryPanel.PerformLayout();
			this.ContainersTabPage.ResumeLayout(false);
			this.ContainersTabPage.PerformLayout();
			this.ContainersGroupBox.ResumeLayout(false);
			this.ContainersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).EndInit();
			this.CusContainerInvoiceLineGrid.ResumeLayout(false);
			this.CusContainerInvoiceLineGrid.PerformLayout();
			this.LineDetailsTabPage.ResumeLayout(false);
			this.LineDetailsTabPage.PerformLayout();
			this.NewLineDetailsTabPage.ResumeLayout(false);
			this.NewLineDetailsTabPage.PerformLayout();
			this.InvoiceLineDetailsUserControl.ResumeLayout(true);
			this.InvoiceLineDetailsUserControl.PerformLayout();
			this.ClassificationPanel.ResumeLayout(false);
			this.ClassificationPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).EndInit();
			this.CustomsInvoiceLinesBoundGrid.ResumeLayout(false);
			this.CustomsInvoiceLinesBoundGrid.PerformLayout();
			this.JI_Calc_CIFConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_CIFConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_FreightConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_FreightConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_FOBConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_FOBConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_GSTConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_GSTConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_DutyConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_DutyConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.PerformLayout();
			this.CustomsQuantityCalcDropEdit.ResumeLayout(true);
			this.CustomsQuantityCalcDropEdit.PerformLayout();
			this.VolumeCalcDropEdit.ResumeLayout(true);
			this.VolumeCalcDropEdit.PerformLayout();
			this.JI_WeightCalcDropEdit.ResumeLayout(true);
			this.JI_WeightCalcDropEdit.PerformLayout();
			this.InvoiceQuantityCalcDropEdit.ResumeLayout(true);
			this.InvoiceQuantityCalcDropEdit.PerformLayout();
			this.JI_DescriptionBoundTextBox.ResumeLayout(true);
			this.JI_DescriptionBoundTextBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DangerousGoodsTabPage.ResumeLayout(false);
			this.DangerousGoodsTabPage.PerformLayout();
			this.DGGuidFindBox.ResumeLayout(true);
			this.DGGuidFindBox.PerformLayout();
			this.UNDGContactGuidFindBox.ResumeLayout(true);
			this.UNDGContactGuidFindBox.PerformLayout();
			this.RFPDetailsTabPage.ResumeLayout(false);
			this.RFPDetailsTabPage.PerformLayout();
			this.RFPDetailsUserControl.ResumeLayout(true);
			this.RFPDetailsUserControl.PerformLayout();
			this.RFPPackagesTabPage.ResumeLayout(false);
			this.RFPPackagesTabPage.PerformLayout();
			this.RFPPackagesUserControl.ResumeLayout(false);
			this.RFPPackagesUserControl.PerformLayout();
			this.RFPProcessTabPage.ResumeLayout(false);
			this.RFPProcessTabPage.PerformLayout();
			this.RFPProcessUserControl.ResumeLayout(true);
			this.RFPProcessUserControl.PerformLayout();
			this.RFPAnalysisTabPage.ResumeLayout(false);
			this.RFPAnalysisTabPage.PerformLayout();
			this.RFPAnalysisUserControl.ResumeLayout(false);
			this.RFPAnalysisUserControl.PerformLayout();
			this.RFPMeatTabPage.ResumeLayout(false);
			this.RFPMeatTabPage.PerformLayout();
			this.RFPMeatUserControl.ResumeLayout(false);
			this.RFPMeatUserControl.PerformLayout();
			this.RFPCertificatesTabPage.ResumeLayout(false);
			this.RFPCertificatesTabPage.PerformLayout();
			this.RFPCertificatesUserControl.ResumeLayout(false);
			this.RFPCertificatesUserControl.PerformLayout();
			this.RFPStatementsTabPage.ResumeLayout(false);
			this.RFPStatementsTabPage.PerformLayout();
			this.RFPStatementsUserControl.ResumeLayout(false);
			this.RFPStatementsUserControl.PerformLayout();
			this.JI_TempImportDateDateEdit.ResumeLayout(true);
			this.JI_TempImportDateDateEdit.PerformLayout();
			this.JI_TariffFindBoxAHECC.ResumeLayout(true);
			this.JI_TariffFindBoxAHECC.PerformLayout();
			this.JI_TariffFindBox.ResumeLayout(true);
			this.JI_TariffFindBox.PerformLayout();
			this.RFPNumbersTabPage.ResumeLayout(false);
			this.RFPNumbersTabPage.PerformLayout();
			this.RFPNumbersUserControl.ResumeLayout(true);
			this.RFPNumbersUserControl.PerformLayout();
			this.REXProductAttachmentsTabPage.ResumeLayout(false);
			this.REXProductAttachmentsTabPage.PerformLayout();
			this.REXProductAttachmentsUserControl.ResumeLayout(true);
			this.REXProductAttachmentsUserControl.PerformLayout();
			this.zCalcDropEditAqisCustomsWeight.ResumeLayout(true);
			this.zCalcDropEditAqisCustomsWeight.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private ZTextBox JI_AUStateBoundTextBox;
		private ZButton JI_AUStateBoundButton;
		private ZTextBox JI_TempImportNumBoundTextBox;
		protected internal Enterprise.Customs.AU.Declaration.GUI.AHECCFindBox JI_TariffFindBoxAHECC;
		protected internal Enterprise.Customs.AU.Declaration.GUI.UniversalTariffExportFindBox JI_TariffFindBox;
		protected internal Enterprise.ZArchitecture.GUI.ZButton AssayCodeBoundButton;
		private Enterprise.ZArchitecture.GUI.ZTabPage DangerousGoodsTabPage;
		internal ZTabPage RFPDetailsTabPage;
		private ZTabPage RFPPackagesTabPage;
		private ZTabPage RFPProcessTabPage;
		private ZTabPage RFPAnalysisTabPage;
		private ZTabPage RFPMeatTabPage;
		private ZTabPage RFPCertificatesTabPage;
		private ZTabPage RFPStatementsTabPage;
		private ZDateEdit JI_TempImportDateDateEdit;
		private ZGroupBox GroupBoxShippingMarks;
		private ZTextBox TextBoxQL_ShippingMarks;
		private ZGroupBox PackagesGroupBox;
		private ZGroupBox InnerPackGroupBox;
		private ZCalcDropEdit QL_InnerPackWeightCalcDropEdit;
		private ZDropEdit QL_InnerPackAccuracyDropEdit;
		private ZDropEdit QL_InnerPackTypeDropEdit;
		private ZCalcEdit QL_InnerPackCountCalcEdit;
		private ZGroupBox IntermediatePackGroupBox;
		private ZCalcDropEdit QL_IntermediatePackWeightCalcDropEdit;
		private ZDropEdit QL_IntermediatePackAccuracyDropEdit;
		private ZDropEdit QL_IntermediatePackTypeDropEdit;
		private ZCalcEdit QL_IntermediatePackCountCalcEdit;
		private ZGroupBox OuterPackGroupBox;
		private ZCalcDropEdit QL_OuterPackWeightCalcDropEdit;
		private ZDropEdit QL_OuterPackAccuracyDropEdit;
		private ZDropEdit QL_OuterPackTypeDropEdit;
		private ZCalcEdit QL_OuterPackCountCalcEdit;
		private ZLabel PackMeasurementLabel;
		private ZLabel AccuracyLabel;
		private ZLabel EXDOCPackTypeLabel;
		private ZLabel PackCountLabel;
		private ZGroupBox WeightGroupBox;
		private ZCalcDropEdit QL_GrossMetricWeightCalcDropEdit;
		private ZCalcDropEdit QL_NetQuantityCalcDropEdit;
		private ZCalcDropEdit QL_ImperialNetWeightCalcDropEdit;
		private ZGuidFindBox DGGuidFindBox;
		private ZLinkLabel DGLinkLabel;
		private ZLabel FlashPointDescLabel;
		private ZCalcEdit FlashPointCalcEdit;
		private ZGuidFindBox UNDGContactGuidFindBox;
		private ZCheckBox LabelApprovalIndicatorCheckBox;
		private ZTextBox QL_LabelApprovalNumberTextBox;
		private ZCheckBox UngradedProductCheckBox;
		private ZTextBox TextBoxQL_BatchCode;
		private ZTabPage RFPNumbersTabPage;
		private ZTabPage REXProductAttachmentsTabPage;
		private RFPNumbersUserControl RFPNumbersUserControl;
		private RFPProcessUserControl RFPProcessUserControl;
		private ZCalcDropEdit zCalcDropEditAqisCustomsWeight;
		private ZCheckBox NoPermitRequiredCheckBox;
		private REXProductAttachmentsUserControl REXProductAttachmentsUserControl;
		internal InvoiceLineRFPDetailsUserControl RFPDetailsUserControl;
		internal RFPAnalysisUserControl RFPAnalysisUserControl;
		private RFPPackagesUserControl RFPPackagesUserControl;
		private RFPCertificatesUserControl RFPCertificatesUserControl;
		private RFPStatementsUserControl RFPStatementsUserControl;
		private RFPMeatUserControl RFPMeatUserControl;
	}
}
