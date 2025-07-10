using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUCMRSupplierHeaderUserControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AUCMRSupplierHeaderUserControl));
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			this.zA_GSTECodeFindBox = new ZCodeFindBox();
			this.ZA_VALB_HiddenDropEdit = new ZDropEdit();
			this.aQISInfoTabPage = new ZTabPage();
			this.aQISDetailsPanel = new ZPanel();
			this.aQISDetailsBottomPanel = new ZPanel();
			this.aQISDocumentGroupBox = new ZGroupBox();
			this.aQISDocumentGrid = new ZArchitecture.ZGrid();
			this.aQISPremisesIdAndPackagesGroupBox = new ZGroupBox();
			this.aQISPremisesIdProcessingTypeGrid = new ZArchitecture.ZGrid();
			this.aQISInformation = new ZGroupBox();
			this.aQISCommodityCodeControl = new AQISCommodityCodeControl();
			this.aQISPermitNumberControl = new AQISPermitNumberControl();
			this.aQISEntityIDControl = new AQISEntityIdControl();
			this.aQISProducerCodeControl = new AQISProducerCodeControl();
			this.AQISTabHiddenLabel = new ZArchitecture.ZLabel();
			this.preferenceGroupBox = new ZGroupBox();
			this.preferenceRuleTypeDropEdit = new ZDropEdit();
			this.preferenceSchemeTypeDropDown = new ZDropEdit();
			this.pOCCodeFindBox = new ZCodeFindBox();
			this.overrideFOBCheckBox = new ZCheckBox();
			this.zA_HeaderREL_HiddenDropEdit = new ZDropEdit();
			this.addInfoPermitNumberBoundTextBox = new ZArchitecture.ZTextBox();
			this.dutyDateDateEdit = new ZDateEdit();
			this.includeADJCheckBox = new ZCheckBox();
			this.JZ_AddInfoBoundAddInfoControl.SuspendLayout();
			this.InvoiceOriginCodeFindBox.SuspendLayout();
			this.JZ_IncoTermBoundDropDownEdit.SuspendLayout();
			this.GroupInvoiceDropEdit.SuspendLayout();
			this.NoOfPacksCalcDropEdit.SuspendLayout();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.JZ_InvoiceAmountBoundCurrencyControl.SuspendLayout();
			this.LeftBottomPanel.SuspendLayout();
			this.RightBottomPanel.SuspendLayout();
			this.InvoiceTabControl.SuspendLayout();
			this.ComInvoiceDetailsTabPage.SuspendLayout();
			this.ChargesGroupBox.SuspendLayout();
			this.ChargesTabControl.SuspendLayout();
			this.InvoiceChargesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).BeginInit();
			this.InvoiceChargesGrid.SuspendLayout();
			this.ApportionedTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).BeginInit();
			this.ApportionedChargesGrid.SuspendLayout();
			this.BaseGroupChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BaseGroupChargesGrid)).BeginInit();
			this.BaseGroupChargesGrid.SuspendLayout();
			this.JZ_FOBAmountBoundCurrencyControl.SuspendLayout();
			this.JZ_Calc_TNIBoundInvoiceCurrencyControl.SuspendLayout();
			this.JZ_CIFAmountBoundCurrencyControl.SuspendLayout();
			this.LineTotalBoundConvertToLocalCurrencyControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobComInvoiceHeadersBoundGrid.InnerGrid)).BeginInit();
			this.JobComInvoiceHeadersBoundGrid.SuspendLayout();
			this.InvCustomFieldsDisplayControl.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Splitter)).BeginInit();
			this.Splitter.Panel1.SuspendLayout();
			this.Splitter.Panel2.SuspendLayout();
			this.Splitter.SuspendLayout();
			this.InvDetailLeftPanel.SuspendLayout();
			this.InvDetailRightPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChargesGroupsSplitterContainer)).BeginInit();
			this.ChargesGroupsSplitterContainer.Panel1.SuspendLayout();
			this.ChargesGroupsSplitterContainer.Panel2.SuspendLayout();
			this.ChargesGroupsSplitterContainer.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zA_GSTECodeFindBox.SuspendLayout();
			this.ZA_VALB_HiddenDropEdit.SuspendLayout();
			this.aQISInfoTabPage.SuspendLayout();
			this.aQISDetailsPanel.SuspendLayout();
			this.aQISDetailsBottomPanel.SuspendLayout();
			this.aQISDocumentGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.aQISDocumentGrid)).BeginInit();
			this.aQISDocumentGrid.SuspendLayout();
			this.aQISPremisesIdAndPackagesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.aQISPremisesIdProcessingTypeGrid)).BeginInit();
			this.aQISPremisesIdProcessingTypeGrid.SuspendLayout();
			this.aQISInformation.SuspendLayout();
			this.aQISCommodityCodeControl.SuspendLayout();
			this.aQISPermitNumberControl.SuspendLayout();
			this.aQISEntityIDControl.SuspendLayout();
			this.aQISProducerCodeControl.SuspendLayout();
			this.preferenceGroupBox.SuspendLayout();
			this.preferenceRuleTypeDropEdit.SuspendLayout();
			this.preferenceSchemeTypeDropDown.SuspendLayout();
			this.pOCCodeFindBox.SuspendLayout();
			this.zA_HeaderREL_HiddenDropEdit.SuspendLayout();
			this.dutyDateDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// JZ_AddInfoBoundAddInfoControl
			// 
			this.JZ_AddInfoBoundAddInfoControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 198, true);
			this.JZ_AddInfoBoundAddInfoControl.ShowCMRAddInfo = true;
			this.JZ_AddInfoBoundAddInfoControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 20, true);
			this.JZ_AddInfoBoundAddInfoControl.TabIndex = 14;
			// 
			// ITOTIncoTermTextBox
			// 
			this.ITOTIncoTermTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 0, true);
			this.ITOTIncoTermTextBox.TabIndex = 4;
			// 
			// InvoiceOriginCodeFindBox
			// 
			this.InvoiceOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 151, true);
			this.InvoiceOriginCodeFindBox.ShowDescriptionBox = false;
			this.InvoiceOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.InvoiceOriginCodeFindBox.TabIndex = 11;
			// 
			// IncoTermExplainButton
			// 
			this.IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 79, true);
			// 
			// JZ_InvoiceCurrExRateCalcEdit
			// 
			this.JZ_InvoiceCurrExRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 55, true);
			this.JZ_InvoiceCurrExRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 20, true);
			// 
			// JZ_InvoiceNumberBoundTextBox
			// 
			this.JZ_InvoiceNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 10, true);
			this.JZ_InvoiceNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 20, true);
			// 
			// JZ_IncoTermBoundDropDownEdit
			// 
			this.JZ_IncoTermBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 79, true);
			// 
			// GroupInvoiceDropEdit
			// 
			this.GroupInvoiceDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRSupplierHeaderUserControl|ac57f9ee-5b96-422a-8d8b-4aca92f641cd", "Group Invoice", "Indicates the immediate Group Invoice to place this Commercial Invoice in for the purposes of grouping and distributing charges across multiple invoices.");
			this.GroupInvoiceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 33, true);
			// 
			// JZ_InvoiceCurrLandedCostExRateCalcEdit
			// 
			this.JZ_InvoiceCurrLandedCostExRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(278, 222, true);
			this.JZ_InvoiceCurrLandedCostExRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 20, true);
			this.JZ_InvoiceCurrLandedCostExRateCalcEdit.TabIndex = 16;
			// 
			// NoOfPacksCalcDropEdit
			// 
			this.NoOfPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 79, true);
			this.NoOfPacksCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 20, true);
			this.NoOfPacksCalcDropEdit.TabIndex = 6;
			// 
			// GrossWeightCalcDropEdit
			// 
			this.GrossWeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRSupplierHeaderUserControl|33fe7550-d2d5-424f-9fb1-e1b57a6c8df5", "Inv. Gross Weight", "Enter the Gross Weight and Unit of Gross Weight for this Commercial Invoice. It is mandatory for declarations and the amount should not exceed the Total Weight entered on the Declaration form in the Shipment Details.");
			this.GrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 103, true);
			this.GrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.GrossWeightCalcDropEdit.TabIndex = 7;
			// 
			// NetWeightCalcDropEdit
			// 
			this.NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(386, 103, true);
			this.NetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 20, true);
			this.NetWeightCalcDropEdit.TabIndex = 8;
			// 
			// JZ_InvoiceAmountBoundCurrencyControl
			// 
			this.JZ_InvoiceAmountBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 56, true);
			this.JZ_InvoiceAmountBoundCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			// 
			// JZ_IncoTermPlaceTextBox
			// 
			this.JZ_IncoTermPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 80, true);
			// 
			// LeftBottomPanel
			// 
			this.LeftBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(308, 32, true);
			// 
			// IncoTermTextBox
			// 
			this.IncoTermTextBox.Enabled = false;
			this.IncoTermTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 0, true);
			this.IncoTermTextBox.TabIndex = 2;
			this.IncoTermTextBox.Visible = false;
			// 
			// ApportionmentPendingLabel
			// 
			this.ApportionmentPendingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 32, true);
			this.ApportionmentPendingLabel.TabIndex = 3;
			// 
			// RightBottomPanel
			// 
			this.RightBottomPanel.Controls.Add(this.overrideFOBCheckBox);
			this.RightBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 0, true);
			this.RightBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 32, true);
			this.RightBottomPanel.Controls.SetChildIndex(this.ApportionmentPendingLabel, 0);
			this.RightBottomPanel.Controls.SetChildIndex(this.JZ_CIFAmountBoundCurrencyControl, 0);
			this.RightBottomPanel.Controls.SetChildIndex(this.JZ_Calc_TNIBoundInvoiceCurrencyControl, 0);
			this.RightBottomPanel.Controls.SetChildIndex(this.JZ_FOBAmountBoundCurrencyControl, 0);
			this.RightBottomPanel.Controls.SetChildIndex(this.overrideFOBCheckBox, 0);
			// 
			// InvoiceTabControl
			// 
			this.InvoiceTabControl.Controls.Add(this.aQISInfoTabPage);
			this.InvoiceTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 433, true);
			this.InvoiceTabControl.Controls.SetChildIndex(this.CustomFieldsTabPage, 0);
			this.InvoiceTabControl.Controls.SetChildIndex(this.aQISInfoTabPage, 0);
			this.InvoiceTabControl.Controls.SetChildIndex(this.ComInvoiceDetailsTabPage, 0);
			// 
			// ChargesGroupBox
			// 
			this.ChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(476, 145, true);
			this.ChargesGroupBox.TabIndex = 30;
			// 
			// ChargesTabControl
			// 
			this.ChargesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 126, true);
			// 
			// InvoiceChargesTabPage
			// 
			this.InvoiceChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(462, 99, true);
			// 
			// InvoiceChargesGrid
			// 
			this.InvoiceChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(462, 99, true);
			// 
			// BaseGroupChargesGroupBox
			// 
			this.BaseGroupChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(476, 128, true);
			this.BaseGroupChargesGroupBox.TabIndex = 31;
			// 
			// BaseGroupChargesGrid
			// 
			this.BaseGroupChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 109, true);
			// 
			// JZ_FOBAmountBoundCurrencyControl
			// 
			this.JZ_FOBAmountBoundCurrencyControl.BindToAmount = "Invoices.EffectiveFOBAmount";
			this.JZ_FOBAmountBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 3, true);
			this.JZ_FOBAmountBoundCurrencyControl.TabIndex = 6;
			// 
			// JZ_Calc_TNIBoundInvoiceCurrencyControl
			// 
			this.JZ_Calc_TNIBoundInvoiceCurrencyControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRSupplierHeaderUserControl|ac55d4fe-8469-4f9c-a2ce-ebac3587a6e2", "T & I", "Transport and Insurance amount (read-only). This T&I amount is calculated by the system based on data entered in fields such as overseas freight (OFT) and overseas insurance (ONS). Note that the T&I amount at the invoice header level MUST equal the T&I at the line level.");
			this.JZ_Calc_TNIBoundInvoiceCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(524, 3, true);
			this.JZ_Calc_TNIBoundInvoiceCurrencyControl.TabIndex = 2;
			// 
			// JZ_CIFAmountBoundCurrencyControl
			// 
			this.JZ_CIFAmountBoundCurrencyControl.BindToAmount = "Invoices.EffectiveCIFAmount";
			this.JZ_CIFAmountBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 3, true);
			this.JZ_CIFAmountBoundCurrencyControl.TabIndex = 0;
			// 
			// LineTotalBoundConvertToLocalCurrencyControl
			// 
			this.LineTotalBoundConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRSupplierHeaderUserControl|e16c2e76-05ea-496a-a810-ccf6ad722bac", "Exp. Inv. Line Tot", "Expected Invoice Line Total", "This is the calculated Expected Line Total for this commercial invoice after calculations and consideration of additions and deductions to the Invoice Total amount.");
			this.LineTotalBoundConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 0, true);
			// 
			// JobComInvoiceHeadersBoundGrid
			// 
			this.JobComInvoiceHeadersBoundGrid.GridId = null;
			// 
			// 
			// 
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.AllowNavigation = false;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.CaptionVisible = false;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.CheckDatabaseAfterFirstBinding = true;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = null;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.LayoutKey = "JobComInvoiceHeadersBoundGrid";
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Name = "Grid";
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 184, true);
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.TabIndex = 0;
			this.JobComInvoiceHeadersBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 222, true);
			// 
			// Splitter
			// 
			this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 696, true);
			this.Splitter.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(342);
			this.Splitter.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(222);
			// 
			// InvDetailLeftPanel
			// 
			this.InvDetailLeftPanel.Controls.Add(this.zA_GSTECodeFindBox);
			this.InvDetailLeftPanel.Controls.Add(this.ZA_VALB_HiddenDropEdit);
			this.InvDetailLeftPanel.Controls.Add(this.preferenceGroupBox);
			this.InvDetailLeftPanel.Controls.Add(this.addInfoPermitNumberBoundTextBox);
			this.InvDetailLeftPanel.Controls.Add(this.dutyDateDateEdit);
			this.InvDetailLeftPanel.Controls.Add(this.zA_HeaderREL_HiddenDropEdit);
			this.InvDetailLeftPanel.Controls.Add(this.includeADJCheckBox);
			this.InvDetailLeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 279, true);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.NoOfPacksCalcDropEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_InvoiceCurrLandedCostExRateCalcEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.NetWeightCalcDropEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.GrossWeightCalcDropEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_IncoTermPlaceTextBox, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_IncoTermBoundDropDownEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.IncoTermExplainButton, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_InvoiceCurrExRateCalcEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_InvoiceAmountBoundCurrencyControl, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.GroupInvoiceDropEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_InvoiceNumberBoundTextBox, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.InvoiceOriginCodeFindBox, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_AddInfoBoundAddInfoControl, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.includeADJCheckBox, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.zA_HeaderREL_HiddenDropEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.dutyDateDateEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.addInfoPermitNumberBoundTextBox, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.preferenceGroupBox, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.ZA_VALB_HiddenDropEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.zA_GSTECodeFindBox, 0);
			// 
			// InvDetailRightPanel
			// 
			this.InvDetailRightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(510, 0, true);
			this.InvDetailRightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(476, 279, true);
			// 
			// ChargesGroupsSplitterContainer
			// 
			this.ChargesGroupsSplitterContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(476, 279, true);
			this.ChargesGroupsSplitterContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(145);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 433, true);
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 32, true);
			// 
			// zA_GSTECodeFindBox
			// 
			this.zA_GSTECodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zA_GSTECodeFindBox, "Invoices.ZA_GSTE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).ZA_GSTE)));
			this.zA_GSTECodeFindBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRSupplierHeaderUserControl|e34c8440-a42b-4b45-a4a9-9d52ecab5ddc", "GSTE", "GST Exemption", "GST Exemption Code (GSTE)", "A code indicating that goods are exempt from the Goods and Services Tax (GST).");
			this.zA_GSTECodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(301, 151, true);
			this.zA_GSTECodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.CMRCodeLists;
			this.zA_GSTECodeFindBox.Name = "zA_GSTECodeFindBox";
			this.zA_GSTECodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.zA_GSTECodeFindBox.ParentType = null;
			this.zA_GSTECodeFindBox.PopupCaption = "GST Exemption Code";
			this.zA_GSTECodeFindBox.PreBoundMaxLength = 4;
			this.zA_GSTECodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 20, true);
			this.zA_GSTECodeFindBox.TabIndex = 12;
			// 
			// ZA_VALB_HiddenDropEdit
			// 
			this.ZA_VALB_HiddenDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ZA_VALB_HiddenDropEdit, "Invoices.AddInfo+ZA_VALB_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AddInfo.ZA_VALB_Hidden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AddInfo.Lookups.ValuationBasisListForCMR)));
			this.ZA_VALB_HiddenDropEdit.BindToList = "Invoices.AddInfo+Lookups+ValuationBasisListForCMR";
			this.ZA_VALB_HiddenDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 127, true);
			this.ZA_VALB_HiddenDropEdit.Name = "ZA_VALB_HiddenDropEdit";
			this.ZA_VALB_HiddenDropEdit.PreBoundMaxLength = 4;
			this.ZA_VALB_HiddenDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			this.ZA_VALB_HiddenDropEdit.TabIndex = 9;
			// 
			// aQISInfoTabPage
			// 
			this.aQISInfoTabPage.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRSupplierHeaderUserControl|0f7520a2-12ee-48cc-bb39-d9628d438e89", "Quarantine");
			this.aQISInfoTabPage.Controls.Add(this.aQISDetailsPanel);
			this.aQISInfoTabPage.Controls.Add(this.AQISTabHiddenLabel);
			this.aQISInfoTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.aQISInfoTabPage.Name = "aQISInfoTabPage";
			this.aQISInfoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 406, true);
			this.aQISInfoTabPage.TabIndex = 2;
			// 
			// aQISDetailsPanel
			// 
			this.aQISDetailsPanel.Controls.Add(this.aQISDetailsBottomPanel);
			this.aQISDetailsPanel.Controls.Add(this.aQISInformation);
			this.aQISDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.aQISDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.aQISDetailsPanel.Name = "aQISDetailsPanel";
			this.aQISDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 406, true);
			this.aQISDetailsPanel.TabIndex = 28;
			// 
			// aQISDetailsBottomPanel
			// 
			this.aQISDetailsBottomPanel.Controls.Add(this.aQISDocumentGroupBox);
			this.aQISDetailsBottomPanel.Controls.Add(this.aQISPremisesIdAndPackagesGroupBox);
			this.aQISDetailsBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.aQISDetailsBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 68, true);
			this.aQISDetailsBottomPanel.Name = "aQISDetailsBottomPanel";
			this.aQISDetailsBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 338, true);
			this.aQISDetailsBottomPanel.TabIndex = 28;
			// 
			// aQISDocumentGroupBox
			// 
			this.aQISDocumentGroupBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRSupplierHeaderUserControl|d15dae12-eda5-4f84-ab9e-5e01d827cd21", "Quarantine Document");
			this.aQISDocumentGroupBox.Controls.Add(this.aQISDocumentGrid);
			this.aQISDocumentGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.aQISDocumentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.aQISDocumentGroupBox.Name = "aQISDocumentGroupBox";
			this.aQISDocumentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 338, true);
			this.aQISDocumentGroupBox.TabIndex = 0;
			this.aQISDocumentGroupBox.TabStop = false;
			// 
			// aQISDocumentGrid
			// 
			this.aQISDocumentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.aQISDocumentGrid, "Invoices.AQISDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AQISDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AQISDocument)(((System.Collections.IList)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AQISDocuments)).SyncRoot)).Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((AQISDocument)(((System.Collections.IList)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AQISDocuments)).SyncRoot)).Lookups.AQISDocumentTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AQISDocument)(((System.Collections.IList)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AQISDocuments)).SyncRoot)).Number)));
			this.aQISDocumentGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Lookups+AQISDocumentTypeList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRSupplierHeaderUserControl|531b8295-b8ef-4acb-a903-1274d89b4a26", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "Type";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.ToolTip = resources.GetString("zDropEditColumnStyleInfo1.ToolTip");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRSupplierHeaderUserControl|10b09b05-0dbc-4c9d-8fe2-e432d1736ed5", "Number");
			zTextBoxColumnStyleInfo1.ColumnName = "Number";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.ToolTip = resources.GetString("zTextBoxColumnStyleInfo1.ToolTip");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.aQISDocumentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.aQISDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.aQISDocumentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.aQISDocumentGrid.GridId = "95ce5422-5e93-46dc-aa22-339f7faea8f8";
			this.aQISDocumentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.aQISDocumentGrid.LayoutKey = "AQISDocumentGrid";
			this.aQISDocumentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.aQISDocumentGrid.Name = "aQISDocumentGrid";
			this.aQISDocumentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(495, 319, true);
			this.aQISDocumentGrid.TabIndex = 0;
			// 
			// aQISPremisesIdAndPackagesGroupBox
			// 
			this.aQISPremisesIdAndPackagesGroupBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRSupplierHeaderUserControl|14a44ba2-8b48-49cd-92a5-172f05922629", "Quarantine Premises ID and AEP Processing Type");
			this.aQISPremisesIdAndPackagesGroupBox.Controls.Add(this.aQISPremisesIdProcessingTypeGrid);
			this.aQISPremisesIdAndPackagesGroupBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.aQISPremisesIdAndPackagesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(512, 0, true);
			this.aQISPremisesIdAndPackagesGroupBox.Name = "aQISPremisesIdAndPackagesGroupBox";
			this.aQISPremisesIdAndPackagesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 338, true);
			this.aQISPremisesIdAndPackagesGroupBox.TabIndex = 1;
			this.aQISPremisesIdAndPackagesGroupBox.TabStop = false;
			// 
			// aQISPremisesIdProcessingTypeGrid
			// 
			this.aQISPremisesIdProcessingTypeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.aQISPremisesIdProcessingTypeGrid, "Invoices.AQISPremisesIdAndProcessingTypes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AQISPremisesIdAndProcessingTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AQISPremisesIdAndProcessingType)(((System.Collections.IList)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AQISPremisesIdAndProcessingTypes)).SyncRoot)).PremisesId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((AQISPremisesIdAndProcessingType)(((System.Collections.IList)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AQISPremisesIdAndProcessingTypes)).SyncRoot)).Lookups.AQISPremisesIdList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AQISPremisesIdAndProcessingType)(((System.Collections.IList)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AQISPremisesIdAndProcessingTypes)).SyncRoot)).ProcessingType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((AQISPremisesIdAndProcessingType)(((System.Collections.IList)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AQISPremisesIdAndProcessingTypes)).SyncRoot)).Lookups.AQISProcessingTypeList)));
			this.aQISPremisesIdProcessingTypeGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups+AQISPremisesIdList";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRSupplierHeaderUserControl|945e81db-516a-4f4c-b9b7-595fb80379ab", "Premises Id");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "PremisesId";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.ToolTip = resources.GetString("zCodeFindBoxColumnStyleInfo1.ToolTip");
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.BindToList = "Lookups+AQISProcessingTypeList";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRSupplierHeaderUserControl|9219498b-aab7-49bd-b85f-228261726aa1", "AEP Processing Type");
			zDropEditColumnStyleInfo2.ColumnName = "ProcessingType";
			zDropEditColumnStyleInfo2.IsMandatory = true;
			zDropEditColumnStyleInfo2.ToolTip = resources.GetString("zDropEditColumnStyleInfo2.ToolTip");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			this.aQISPremisesIdProcessingTypeGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.aQISPremisesIdProcessingTypeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.aQISPremisesIdProcessingTypeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.aQISPremisesIdProcessingTypeGrid.GridId = "b00cc226-b45a-4f22-8fe6-dc021315d79f";
			this.aQISPremisesIdProcessingTypeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.aQISPremisesIdProcessingTypeGrid.LayoutKey = "AQISPremisesIdProcessingTypeGrid";
			this.aQISPremisesIdProcessingTypeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.aQISPremisesIdProcessingTypeGrid.Name = "aQISPremisesIdProcessingTypeGrid";
			this.aQISPremisesIdProcessingTypeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 319, true);
			this.aQISPremisesIdProcessingTypeGrid.TabIndex = 0;
			// 
			// aQISInformation
			// 
			this.aQISInformation.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRSupplierHeaderUserControl|9ba53d50-a9d2-4366-8d3b-9e0883b2cc2a", "Quarantine Information");
			this.aQISInformation.Controls.Add(this.aQISCommodityCodeControl);
			this.aQISInformation.Controls.Add(this.aQISPermitNumberControl);
			this.aQISInformation.Controls.Add(this.aQISEntityIDControl);
			this.aQISInformation.Controls.Add(this.aQISProducerCodeControl);
			this.aQISInformation.Dock = System.Windows.Forms.DockStyle.Top;
			this.aQISInformation.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.aQISInformation.Name = "aQISInformation";
			this.aQISInformation.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 68, true);
			this.aQISInformation.TabIndex = 0;
			this.aQISInformation.TabStop = false;
			// 
			// aQISCommodityCodeControl
			// 
			this.aQISCommodityCodeControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.aQISCommodityCodeControl, "Invoices.AddInfo.ZA_AQISCommCodes_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AddInfo.ZA_AQISCommCodes_Hidden)));
			this.aQISCommodityCodeControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRSupplierHeaderUserControl|6d3a8daf-87f4-4161-9a24-a16a1091c1d0", "Commodity Code", "Linked to the ACS tariff. Provides a more detailed breakdown of goods within a classification to enable commodities to be further identified for Quarantine profiling purposes.  They are stored against the Tariff Classification - Statistical Classification Combination.");
			this.aQISCommodityCodeControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(340, 40, true);
			this.aQISCommodityCodeControl.Name = "aQISCommodityCodeControl";
			this.aQISCommodityCodeControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.aQISCommodityCodeControl.TabIndex = 3;
			// 
			// aQISPermitNumberControl
			// 
			this.aQISPermitNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.aQISPermitNumberControl, "Invoices.AddInfo.ZA_AQISPermitIds_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AddInfo.ZA_AQISPermitIds_Hidden)));
			this.aQISPermitNumberControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRSupplierHeaderUserControl|52a774aa-7bc7-402e-a228-be0726560366", "Permit Number", "The number of a permit issued by Quarantine that authorities the importation of certain  commodities that are subject to Quarantine controls of restrictions.");
			this.aQISPermitNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 16, true);
			this.aQISPermitNumberControl.Name = "aQISPermitNumberControl";
			this.aQISPermitNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.aQISPermitNumberControl.TabIndex = 0;
			// 
			// aQISEntityIDControl
			// 
			this.aQISEntityIDControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.aQISEntityIDControl, "Invoices.AddInfo.ZA_AQISEntityIds_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AddInfo.ZA_AQISEntityIds_Hidden)));
			this.aQISEntityIDControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRSupplierHeaderUserControl|7369ca8b-62b9-4357-aef2-c8af12a251ad", "Entity ID", "An identifier of some business objects for significance within the Quarantine system. This is a catchall attribute that allows the importer to quote additional information that may interest Quarantine. Example: Overseas treatment Provider Number, Quarantine Vessel Identifier.");
			this.aQISEntityIDControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(340, 16, true);
			this.aQISEntityIDControl.Name = "aQISEntityIDControl";
			this.aQISEntityIDControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.aQISEntityIDControl.TabIndex = 1;
			// 
			// aQISProducerCodeControl
			// 
			this.aQISProducerCodeControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.aQISProducerCodeControl, "Invoices.AddInfo.ZA_AQISProducerCodes_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AddInfo.ZA_AQISProducerCodes_Hidden)));
			this.aQISProducerCodeControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRSupplierHeaderUserControl|9175ce23-1520-45c4-bbf8-04ff83209242", "Producer Code", "Relates to food shipments. A requirement of Quarantine IFP scheme, the producer code indicates who actually manufactured the product, not who supplied it. The broker nominates a producer code for a line of food when required by certain Quarantine profiles.");
			this.aQISProducerCodeControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 40, true);
			this.aQISProducerCodeControl.Name = "aQISProducerCodeControl";
			this.aQISProducerCodeControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.aQISProducerCodeControl.TabIndex = 2;
			// 
			// AQISTabHiddenLabel
			// 
			this.AQISTabHiddenLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AQISTabHiddenLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AQISTabHiddenLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AQISTabHiddenLabel.Name = "AQISTabHiddenLabel";
			this.AQISTabHiddenLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 406, true);
			this.AQISTabHiddenLabel.TabIndex = 29;
			this.AQISTabHiddenLabel.Text = "Quarantine Information is not required for a Nature 30 entry.";
			this.AQISTabHiddenLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// preferenceGroupBox
			// 
			this.preferenceGroupBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRSupplierHeaderUserControl|cbab2a26-e41b-4eb3-8306-19318a660274", "Preference");
			this.preferenceGroupBox.Controls.Add(this.preferenceRuleTypeDropEdit);
			this.preferenceGroupBox.Controls.Add(this.preferenceSchemeTypeDropDown);
			this.preferenceGroupBox.Controls.Add(this.pOCCodeFindBox);
			this.preferenceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 246, true);
			this.preferenceGroupBox.Name = "preferenceGroupBox";
			this.preferenceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 35, true);
			this.preferenceGroupBox.TabIndex = 29;
			this.preferenceGroupBox.TabStop = false;
			// 
			// preferenceRuleTypeDropEdit
			// 
			this.preferenceRuleTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.preferenceRuleTypeDropEdit, "Invoices.AddInfo+ZA_PRT");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AddInfo.ZA_PRT)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AddInfo.Lookups.ZA_PRT_List)));
			this.preferenceRuleTypeDropEdit.BindToList = "Invoices.AddInfo+Lookups+ZA_PRT_List";
			this.preferenceRuleTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 12, true);
			this.preferenceRuleTypeDropEdit.Name = "preferenceRuleTypeDropEdit";
			this.preferenceRuleTypeDropEdit.PreBoundMaxLength = 4;
			this.preferenceRuleTypeDropEdit.ShowDescriptionBox = false;
			this.preferenceRuleTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.preferenceRuleTypeDropEdit.TabIndex = 5;
			// 
			// preferenceSchemeTypeDropDown
			// 
			this.preferenceSchemeTypeDropDown.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.preferenceSchemeTypeDropDown, "Invoices.AddInfo+ZA_PST");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AddInfo.ZA_PST)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AddInfo.Lookups.ZA_PST_List)));
			this.preferenceSchemeTypeDropDown.BindToList = "Invoices.AddInfo+Lookups+ZA_PST_List";
			this.preferenceSchemeTypeDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(268, 12, true);
			this.preferenceSchemeTypeDropDown.Name = "preferenceSchemeTypeDropDown";
			this.preferenceSchemeTypeDropDown.PreBoundMaxLength = 4;
			this.preferenceSchemeTypeDropDown.ShowDescriptionBox = false;
			this.preferenceSchemeTypeDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.preferenceSchemeTypeDropDown.TabIndex = 3;
			// 
			// pOCCodeFindBox
			// 
			this.pOCCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.pOCCodeFindBox, "Invoices.AddInfo+ZA_POC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AddInfo.ZA_POC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AddInfo.Lookups.ZA_POC_List)));
			this.pOCCodeFindBox.BindToList = "Invoices.AddInfo+Lookups+ZA_POC_List";
			this.pOCCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 12, true);
			this.pOCCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.pOCCodeFindBox.Name = "pOCCodeFindBox";
			this.pOCCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.pOCCodeFindBox.ParentType = null;
			this.pOCCodeFindBox.PreBoundMaxLength = 4;
			this.pOCCodeFindBox.ShowDescriptionBox = false;
			this.pOCCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.pOCCodeFindBox.TabIndex = 1;
			// 
			// overrideFOBCheckBox
			// 
			this.BindingSource.SetBindingMember(this.overrideFOBCheckBox, "Invoices.JZ_OverrideFOB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).JZ_OverrideFOB)));
			this.overrideFOBCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.overrideFOBCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 6, true);
			this.overrideFOBCheckBox.Name = "overrideFOBCheckBox";
			this.overrideFOBCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 15, true);
			this.overrideFOBCheckBox.TabIndex = 4;
			this.overrideFOBCheckBox.Text = "Override FOB?";
			// 
			// zA_HeaderREL_HiddenDropEdit
			// 
			this.zA_HeaderREL_HiddenDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zA_HeaderREL_HiddenDropEdit, "Invoices.AddInfo+ZA_HeaderREL_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AddInfo.ZA_HeaderREL_Hidden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AddInfo.Lookups.ZA_HeaderREL_List)));
			this.zA_HeaderREL_HiddenDropEdit.BindToList = "Invoices.AddInfo+Lookups+ZA_HeaderREL_List";
			this.zA_HeaderREL_HiddenDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(471, 127, true);
			this.zA_HeaderREL_HiddenDropEdit.Name = "zA_HeaderREL_HiddenDropEdit";
			this.zA_HeaderREL_HiddenDropEdit.PreBoundMaxLength = 1;
			this.zA_HeaderREL_HiddenDropEdit.ShowDescriptionBox = false;
			this.zA_HeaderREL_HiddenDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.zA_HeaderREL_HiddenDropEdit.TabIndex = 10;
			// 
			// addInfoPermitNumberBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.addInfoPermitNumberBoundTextBox, "Invoices.AddInfo+ZA_PermitNumbers_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AddInfo.ZA_PermitNumbers_Hidden)));
			this.addInfoPermitNumberBoundTextBox.CaptionResourceString = null;
			this.addInfoPermitNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 174, true);
			this.addInfoPermitNumberBoundTextBox.Name = "addInfoPermitNumberBoundTextBox";
			this.addInfoPermitNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 20, true);
			this.addInfoPermitNumberBoundTextBox.TabIndex = 13;
			// 
			// dutyDateDateEdit
			// 
			this.dutyDateDateEdit.AllowDrop = true;
			this.dutyDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.dutyDateDateEdit, "Invoices.EffectiveDutyDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).EffectiveDutyDate)));
			this.dutyDateDateEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRSupplierHeaderUserControl|a07a0578-c952-40f1-a436-41fab0b5b85f", "Duty Date");
			this.dutyDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 222, true);
			this.dutyDateDateEdit.Name = "dutyDateDateEdit";
			this.dutyDateDateEdit.TabIndex = 15;
			// 
			// includeADJCheckBox
			// 
			this.includeADJCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.includeADJCheckBox, "Invoices.AddInfo.ZA_IncADJ_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((JobComInvoiceHeader)(((System.Collections.IList)(((IInvoicesProvider)(null)).Invoices)).SyncRoot)).AddInfo.ZA_IncADJ_Hidden)));
			this.includeADJCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.includeADJCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 224, true);
			this.includeADJCheckBox.Name = "includeADJCheckBox";
			this.includeADJCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 17, true);
			this.includeADJCheckBox.TabIndex = 17;
			this.includeADJCheckBox.Text = "Incl. Line ADJs for LC";
			this.includeADJCheckBox.UseVisualStyleBackColor = true;
			// 
			// AUCMRSupplierHeaderUserControl
			// 
			this.Name = "AUCMRSupplierHeaderUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 696, true);
			this.JZ_AddInfoBoundAddInfoControl.ResumeLayout(true);
			this.JZ_AddInfoBoundAddInfoControl.PerformLayout();
			this.InvoiceOriginCodeFindBox.ResumeLayout(true);
			this.InvoiceOriginCodeFindBox.PerformLayout();
			this.JZ_IncoTermBoundDropDownEdit.ResumeLayout(true);
			this.JZ_IncoTermBoundDropDownEdit.PerformLayout();
			this.GroupInvoiceDropEdit.ResumeLayout(true);
			this.GroupInvoiceDropEdit.PerformLayout();
			this.NoOfPacksCalcDropEdit.ResumeLayout(true);
			this.NoOfPacksCalcDropEdit.PerformLayout();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.JZ_InvoiceAmountBoundCurrencyControl.ResumeLayout(true);
			this.JZ_InvoiceAmountBoundCurrencyControl.PerformLayout();
			this.LeftBottomPanel.ResumeLayout(false);
			this.LeftBottomPanel.PerformLayout();
			this.RightBottomPanel.ResumeLayout(false);
			this.RightBottomPanel.PerformLayout();
			this.InvoiceTabControl.ResumeLayout(false);
			this.InvoiceTabControl.PerformLayout();
			this.ComInvoiceDetailsTabPage.ResumeLayout(false);
			this.ComInvoiceDetailsTabPage.PerformLayout();
			this.ChargesGroupBox.ResumeLayout(false);
			this.ChargesGroupBox.PerformLayout();
			this.ChargesTabControl.ResumeLayout(false);
			this.ChargesTabControl.PerformLayout();
			this.InvoiceChargesTabPage.ResumeLayout(false);
			this.InvoiceChargesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).EndInit();
			this.InvoiceChargesGrid.ResumeLayout(false);
			this.InvoiceChargesGrid.PerformLayout();
			this.ApportionedTabPage.ResumeLayout(false);
			this.ApportionedTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).EndInit();
			this.ApportionedChargesGrid.ResumeLayout(false);
			this.ApportionedChargesGrid.PerformLayout();
			this.BaseGroupChargesGroupBox.ResumeLayout(false);
			this.BaseGroupChargesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BaseGroupChargesGrid)).EndInit();
			this.BaseGroupChargesGrid.ResumeLayout(false);
			this.BaseGroupChargesGrid.PerformLayout();
			this.JZ_FOBAmountBoundCurrencyControl.ResumeLayout(true);
			this.JZ_FOBAmountBoundCurrencyControl.PerformLayout();
			this.JZ_Calc_TNIBoundInvoiceCurrencyControl.ResumeLayout(true);
			this.JZ_Calc_TNIBoundInvoiceCurrencyControl.PerformLayout();
			this.JZ_CIFAmountBoundCurrencyControl.ResumeLayout(true);
			this.JZ_CIFAmountBoundCurrencyControl.PerformLayout();
			this.LineTotalBoundConvertToLocalCurrencyControl.ResumeLayout(true);
			this.LineTotalBoundConvertToLocalCurrencyControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobComInvoiceHeadersBoundGrid.InnerGrid)).EndInit();
			this.JobComInvoiceHeadersBoundGrid.ResumeLayout(true);
			this.JobComInvoiceHeadersBoundGrid.PerformLayout();
			this.InvCustomFieldsDisplayControl.ResumeLayout(true);
			this.InvCustomFieldsDisplayControl.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.Splitter.Panel1.ResumeLayout(false);
			this.Splitter.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.Splitter)).EndInit();
			this.Splitter.ResumeLayout(false);
			this.Splitter.PerformLayout();
			this.InvDetailLeftPanel.ResumeLayout(false);
			this.InvDetailLeftPanel.PerformLayout();
			this.InvDetailRightPanel.ResumeLayout(false);
			this.InvDetailRightPanel.PerformLayout();
			this.ChargesGroupsSplitterContainer.Panel1.ResumeLayout(false);
			this.ChargesGroupsSplitterContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ChargesGroupsSplitterContainer)).EndInit();
			this.ChargesGroupsSplitterContainer.ResumeLayout(false);
			this.ChargesGroupsSplitterContainer.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zA_GSTECodeFindBox.ResumeLayout(true);
			this.zA_GSTECodeFindBox.PerformLayout();
			this.ZA_VALB_HiddenDropEdit.ResumeLayout(true);
			this.ZA_VALB_HiddenDropEdit.PerformLayout();
			this.aQISInfoTabPage.ResumeLayout(false);
			this.aQISInfoTabPage.PerformLayout();
			this.aQISDetailsPanel.ResumeLayout(false);
			this.aQISDetailsPanel.PerformLayout();
			this.aQISDetailsBottomPanel.ResumeLayout(false);
			this.aQISDetailsBottomPanel.PerformLayout();
			this.aQISDocumentGroupBox.ResumeLayout(false);
			this.aQISDocumentGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.aQISDocumentGrid)).EndInit();
			this.aQISDocumentGrid.ResumeLayout(false);
			this.aQISDocumentGrid.PerformLayout();
			this.aQISPremisesIdAndPackagesGroupBox.ResumeLayout(false);
			this.aQISPremisesIdAndPackagesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.aQISPremisesIdProcessingTypeGrid)).EndInit();
			this.aQISPremisesIdProcessingTypeGrid.ResumeLayout(false);
			this.aQISPremisesIdProcessingTypeGrid.PerformLayout();
			this.aQISInformation.ResumeLayout(false);
			this.aQISInformation.PerformLayout();
			this.aQISCommodityCodeControl.ResumeLayout(true);
			this.aQISCommodityCodeControl.PerformLayout();
			this.aQISPermitNumberControl.ResumeLayout(true);
			this.aQISPermitNumberControl.PerformLayout();
			this.aQISEntityIDControl.ResumeLayout(true);
			this.aQISEntityIDControl.PerformLayout();
			this.aQISProducerCodeControl.ResumeLayout(true);
			this.aQISProducerCodeControl.PerformLayout();
			this.preferenceGroupBox.ResumeLayout(false);
			this.preferenceGroupBox.PerformLayout();
			this.preferenceRuleTypeDropEdit.ResumeLayout(true);
			this.preferenceRuleTypeDropEdit.PerformLayout();
			this.preferenceSchemeTypeDropDown.ResumeLayout(true);
			this.preferenceSchemeTypeDropDown.PerformLayout();
			this.pOCCodeFindBox.ResumeLayout(true);
			this.pOCCodeFindBox.PerformLayout();
			this.zA_HeaderREL_HiddenDropEdit.ResumeLayout(true);
			this.zA_HeaderREL_HiddenDropEdit.PerformLayout();
			this.dutyDateDateEdit.ResumeLayout(true);
			this.dutyDateDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal ZCodeFindBox zA_GSTECodeFindBox;
		internal ZTabPage aQISInfoTabPage;
		ZGroupBox preferenceGroupBox;
		ZDropEdit preferenceRuleTypeDropEdit;
		ZDropEdit preferenceSchemeTypeDropDown;
		ZCodeFindBox pOCCodeFindBox;
		ZGroupBox aQISInformation;
		AQISCommodityCodeControl aQISCommodityCodeControl;
		AQISPermitNumberControl aQISPermitNumberControl;
		AQISEntityIdControl aQISEntityIDControl;
		AQISProducerCodeControl aQISProducerCodeControl;
		ZPanel aQISDetailsPanel;
		ZPanel aQISDetailsBottomPanel;
		ZGroupBox aQISDocumentGroupBox;
		ZArchitecture.ZGrid aQISDocumentGrid;
		ZGroupBox aQISPremisesIdAndPackagesGroupBox;
		internal ZArchitecture.ZGrid aQISPremisesIdProcessingTypeGrid;
		protected internal ZArchitecture.ZLabel AQISTabHiddenLabel;
		ZCheckBox overrideFOBCheckBox;
		internal ZArchitecture.ZTextBox addInfoPermitNumberBoundTextBox;
		ZDateEdit dutyDateDateEdit;
		internal ZDropEdit zA_HeaderREL_HiddenDropEdit;
		ZCheckBox includeADJCheckBox;
		protected internal ZDropEdit ZA_VALB_HiddenDropEdit;
	}
}
