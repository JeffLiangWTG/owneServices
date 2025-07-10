using Enterprise.Customs.CA.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class CAOrgSupplierPartFormCustomsControl
	{
		void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			TariffColumnStyleInfo tariffColumnStyleInfo1 = new TariffColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new ZMultiLineTextBoxColumnInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGridGuidFindBoxForManufacturer = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfoForManufacturer = new ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new ZCodeFindBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new ZDropEditColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZCheckBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo10 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo5 = new ZCodeFindBoxColumnStyleInfo();
			this.detailsPanel = new ZPanel();
			this.importPanel = new ZPanel();
			this.detailTabControl = new ZTabControl();
			this.detailsTabPage = new ZTabPage();
			this.productOverridesGroupBox = new ZGroupBox();
			this.manufacturerAddressControl = new ZAddressControl();
			this.eTExemptionDropEdit = new ZDropEdit();
			this.eTRateCodeDropEdit = new ZDropEdit();
			this.gSTStatusCodeDropEdit = new ZDropEdit();
			this.classificationNumberFindBox = new TariffFindBox();
			this.cCA_RN_NKOriginzCodeFindBox2 = new ZCodeFindBox();
			this.cCA_ProvinceOfOriginDropEdit2 = new ZDropEdit();
			this.treatmentCodeDropEdit = new ZDropEdit();
			this.tRSNumberTextBox = new ZTextBox();
			this.tariff99CodeFindBox = new TariffFindBox();
			this.authorityNumberCodeFindBox = new ZCodeFindBox();
			this.valueForDutyCodeDropEdit = new ZDropEdit();
			this.classificationDescriptionTextBox = new LongTextControl();
			this.classDetailsGroupBox = new ZGroupBox();
			this.cI_CC_CA_99TariffCodeTextBox = new ZCodeFindBox();
			this.cI_CC_CA_ValueForDutyCodeTextBox = new ZTextBox();
			this.cI_CC_FormattedTariffIMPCodeFindBox = new ZCodeFindBox();
			this.cI_CC_CA_TreatmentCodeTextBox = new ZTextBox();
			this.cI_CC_CA_TRSNumberTextBox = new ZTextBox();
			this.cI_CC_CA_AuthorityNumberTextBox = new ZTextBox();
			this.cI_CC_CA_GSTStatusCodeTextBox = new ZTextBox();
			this.cI_CC_CA_ETRateCodeTextBox = new ZTextBox();
			this.cI_CC_CA_ETExemptionTextBox = new ZTextBox();
			this.cFIATabPage = new ZTabPage();
			this.cFIARegNumbersGroupBox = new ZGroupBox();
			this.cFIARegNumbersGrid = new ZGrid();
			this.cFIAUSStateOfOriginDropEdit = new ZDropEdit();
			this.nKCFIAOriginCodeFindBox = new ZCodeFindBox();
			this.miscIDCodeFindBox = new ZCodeFindBox();
			this.endUseCodeFindBox = new ZCodeFindBox();
			this.destinationProvinceDropEdit = new ZDropEdit();
			this.airsCodeTextBox = new ZTextBox();
			this.requirementVerTextBox = new ZTextBox();
			this.requirementIDTextBox = new ZTextBox();
			this.sITTTab = new ZTabPage();
			this.numbersGroupBox = new ZGroupBox();
			this.sITTRegistrationNumbersGrid = new ZGrid();
			this.sITTBrandNameTextBox = new ZTextBox();
			this.sITTModelNumberTextBox = new ZTextBox();
			this.sITTModelTextBox = new ZTextBox();
			this.sITTImportReasonCodeDropEdit = new ZDropEdit();
			this.nRCANTabPage = new ZTabPage();
			this.nRCANTypeSizeTextBox = new ZTextBox();
			this.nRCANBrandNameTextBox = new ZTextBox();
			this.nRCANModelNumberTextBox = new ZTextBox();
			this.nRCANModelTextBox = new ZTextBox();
			this.nRCANImportReasonCodeDropEdit = new ZDropEdit();
			this.tiresTabPage = new ZTabPage();
			this.tiresTypeSizeTextBox = new ZTextBox();
			this.tiresBrandNameTextBox = new ZTextBox();
			this.compliantImportDateCheckBox = new ZCheckBox();
			this.compliantCompletionCheckBox = new ZCheckBox();
			this.tIINTextBox = new ZTextBox();
			this.tiresImportReasonCodeDropEdit = new ZDropEdit();
			this.sIMATabPage = new ZTabPage();
			this.sIMAGroupBox = new ZGroupBox();
			this.sIMAFeeGrid = new ZGrid();
			this.sIMATopPanel = new ZPanel();
			this.sIMAMeasureDescriptionTextBox = new ZTextBox();
			this.sIMAForCCTabPage = new ZTabPage();
			this.sIMAForCCGroupBox = new ZGroupBox();
			this.sIMAFeeForCCGrid = new ZGrid();
			this.sIMAForCCTopPanel = new ZPanel();
			this.sIMAMeasureDescriptionForCCTextBox = new ZTextBox();
			this.AttributesTabPage = new ZTabPage();
			this.attributesLeftSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.attributes1GroupBox = new ZGroupBox();
			this.attributes1Grid = new ZGrid();
			this.attributesRightSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.attributes2GroupBox = new ZGroupBox();
			this.attributes2Grid = new ZGrid();
			this.attributes3GroupBox = new ZGroupBox();
			this.attributes3Grid = new ZGrid();
			this.pGATabPage = new ZTabPage();
			this.pGARequirementsControl = new PGARequirementsControl();
			this.exportPanel = new ZPanel();
			this.exportClassDetailsGroupBox = new ZGroupBox();
			this.cI_CC_FormattedTariffEXPCodeFindBox = new ZCodeFindBox();
			this.exportProductOverridesGroupBox = new ZGroupBox();
			this.exportTariffCodeFindBox = new TariffFindBox();
			this.cCA_ProvinceOfOriginDropEdit = new ZDropEdit();
			this.cCA_RN_NKOriginCodeFindBox = new ZCodeFindBox();
			this.exportClassificationDescriptionTextBox = new LongTextControl();
			this.cAOrgSupplierPartPanel = new ZPanel();
			this.topPanel = new ZPanel();
			this.aMMVGroupBox = new ZGroupBox();
			this.ammvUnitCalcFindBox = new ZCalcFindBox();
			this.ammvPercentageCalcEdit = new ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.PivotGrid)).BeginInit();
			this.PivotGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.detailsPanel.SuspendLayout();
			this.importPanel.SuspendLayout();
			this.detailTabControl.SuspendLayout();
			this.detailsTabPage.SuspendLayout();
			this.productOverridesGroupBox.SuspendLayout();
			this.manufacturerAddressControl.SuspendLayout();
			this.eTExemptionDropEdit.SuspendLayout();
			this.eTRateCodeDropEdit.SuspendLayout();
			this.gSTStatusCodeDropEdit.SuspendLayout();
			this.classificationNumberFindBox.SuspendLayout();
			this.cCA_RN_NKOriginzCodeFindBox2.SuspendLayout();
			this.cCA_ProvinceOfOriginDropEdit2.SuspendLayout();
			this.treatmentCodeDropEdit.SuspendLayout();
			this.tariff99CodeFindBox.SuspendLayout();
			this.authorityNumberCodeFindBox.SuspendLayout();
			this.valueForDutyCodeDropEdit.SuspendLayout();
			this.classificationDescriptionTextBox.SuspendLayout();
			this.classDetailsGroupBox.SuspendLayout();
			this.cI_CC_CA_99TariffCodeTextBox.SuspendLayout();
			this.cI_CC_FormattedTariffIMPCodeFindBox.SuspendLayout();
			this.cFIATabPage.SuspendLayout();
			this.cFIARegNumbersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.cFIARegNumbersGrid)).BeginInit();
			this.cFIARegNumbersGrid.SuspendLayout();
			this.cFIAUSStateOfOriginDropEdit.SuspendLayout();
			this.nKCFIAOriginCodeFindBox.SuspendLayout();
			this.miscIDCodeFindBox.SuspendLayout();
			this.endUseCodeFindBox.SuspendLayout();
			this.destinationProvinceDropEdit.SuspendLayout();
			this.sITTTab.SuspendLayout();
			this.numbersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.sITTRegistrationNumbersGrid)).BeginInit();
			this.sITTRegistrationNumbersGrid.SuspendLayout();
			this.sITTImportReasonCodeDropEdit.SuspendLayout();
			this.nRCANTabPage.SuspendLayout();
			this.nRCANImportReasonCodeDropEdit.SuspendLayout();
			this.tiresTabPage.SuspendLayout();
			this.tiresImportReasonCodeDropEdit.SuspendLayout();
			this.sIMATabPage.SuspendLayout();
			this.sIMAGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.sIMAFeeGrid)).BeginInit();
			this.sIMAFeeGrid.SuspendLayout();
			this.sIMATopPanel.SuspendLayout();
			this.sIMAForCCTabPage.SuspendLayout();
			this.sIMAForCCGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.sIMAFeeForCCGrid)).BeginInit();
			this.sIMAFeeForCCGrid.SuspendLayout();
			this.sIMAForCCTopPanel.SuspendLayout();
			this.AttributesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.attributesLeftSplitContainer)).BeginInit();
			this.attributesLeftSplitContainer.Panel1.SuspendLayout();
			this.attributesLeftSplitContainer.Panel2.SuspendLayout();
			this.attributesLeftSplitContainer.SuspendLayout();
			this.attributes1GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.attributes1Grid)).BeginInit();
			this.attributes1Grid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.attributesRightSplitContainer)).BeginInit();
			this.attributesRightSplitContainer.Panel1.SuspendLayout();
			this.attributesRightSplitContainer.Panel2.SuspendLayout();
			this.attributesRightSplitContainer.SuspendLayout();
			this.attributes2GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.attributes2Grid)).BeginInit();
			this.attributes2Grid.SuspendLayout();
			this.attributes3GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.attributes3Grid)).BeginInit();
			this.attributes3Grid.SuspendLayout();
			this.pGATabPage.SuspendLayout();
			this.pGARequirementsControl.SuspendLayout();
			this.exportPanel.SuspendLayout();
			this.exportClassDetailsGroupBox.SuspendLayout();
			this.cI_CC_FormattedTariffEXPCodeFindBox.SuspendLayout();
			this.exportProductOverridesGroupBox.SuspendLayout();
			this.exportTariffCodeFindBox.SuspendLayout();
			this.cCA_ProvinceOfOriginDropEdit.SuspendLayout();
			this.cCA_RN_NKOriginCodeFindBox.SuspendLayout();
			this.exportClassificationDescriptionTextBox.SuspendLayout();
			this.aMMVGroupBox.SuspendLayout();
			this.cAOrgSupplierPartPanel.SuspendLayout();
			this.ammvUnitCalcFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// PivotGrid
			// 
			this.PivotGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PivotGrid, "PivotsForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((OrgSupplierPart)(null)).PivotsForBinding);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_ChildType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_ChildTypeDescription);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_OH);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_TariffNumTariffInfo);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_FormattedTariffNum);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_CC);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_UsageComment);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_DateStart);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_DateEnd);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_LastAuditedDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).LastAuditedUserFullName);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).PGARequirements);
			this.PivotGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo11.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo11.ColumnName = "CCA_AuthorityNumber";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|a54075b7-e8b8-4784-bfb7-5ac586c2813e", "Authority Number");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|4b459ee3-9b7f-4b1c-b078-c91ef8100419", "Type Desc.");
			zTextBoxColumnStyleInfo1.ColumnName = "CI_ChildTypeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|04c71f23-4251-4a27-b882-910720d8553f", "Organization");
			zGuidDropEditColumnStyleInfo1.ColumnName = "CI_OH";
			tariffColumnStyleInfo1.BindToTariffPropertyInfo = "CI_TariffNumTariffInfo";
			tariffColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|6c7dd3f3-5140-4b22-ad55-a5832f162f81", "HS", "Class. Tariff #", "Classification Tariff Code", "Must be a 10-digit Canadian National Customs Tariff code.");
			tariffColumnStyleInfo1.ColumnName = "CI_FormattedTariffNum";
			tariffColumnStyleInfo1.TariffCode = null;
			tariffColumnStyleInfo1.TariffType = Enterprise.Customs.Common.TariffType.Import;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|a8cb36ca-1eb3-418d-aea3-2fc749dca28d", "Lookup", "Class. Lookup", "Classification Lookup", "");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "CI_CC";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiLineTextBoxColumnInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiLineTextBoxColumnInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|38e49079-4782-44fa-bfb1-a4a937ad0fa3", "Comment", "Usage Comment", "");
			zMultiLineTextBoxColumnInfo1.ColumnName = "CI_UsageComment";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDateEditColumnStyleInfo1.Caption = "Start Date";
			zDateEditColumnStyleInfo1.ColumnName = "CI_DateStart";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Caption = "End Date";
			zDateEditColumnStyleInfo2.ColumnName = "CI_DateEnd";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zGridGuidFindBoxForManufacturer.BindToList = "Lookups+Manufacturers";
			zGridGuidFindBoxForManufacturer.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|B4A8A6E4-7EDA-490F-8E61-8688538E409C", "Manufacturer");
			zGridGuidFindBoxForManufacturer.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("9E538C49-98A8-4348-B464-DF8B5998E905", "Manufacturer");
			zGridGuidFindBoxForManufacturer.ColumnName = "CCA_OA_Manufacturer_ZAddress+OrgPK";
			zGridGuidFindBoxForManufacturer.ModuleID = ((ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			zGridGuidFindBoxForManufacturer.ToolTip = "Manufacturer";
			zGridGuidFindBoxForManufacturer.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zAddressDropEditColumnStyleInfoForManufacturer.Caption = "Manufacturer Address";
			zAddressDropEditColumnStyleInfoForManufacturer.ColumnName = "CCA_OA_Manufacturer";
			zAddressDropEditColumnStyleInfoForManufacturer.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zAddressDropEditColumnStyleInfoForManufacturer.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("9E538C49-98A8-4348-B464-DF8B5998E905", "Manufacturer");
			this.PivotGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PivotGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.PivotGrid.ColumnStyles.Add(tariffColumnStyleInfo1);
			this.PivotGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.PivotGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.PivotGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.PivotGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.PivotGrid.ColumnStyles.Add(zGridGuidFindBoxForManufacturer);
			this.PivotGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfoForManufacturer);
			this.PivotGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.PivotGrid.CopySelectedRowsAllowed = true;
			this.PivotGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PivotGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PivotGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PivotGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1123, 66, true);
			this.PivotGrid.TabIndex = 9;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(OrgSupplierPart);
			// 
			// detailsPanel
			// 
			this.detailsPanel.Controls.Add(this.importPanel);
			this.detailsPanel.Controls.Add(this.exportPanel);
			this.detailsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.detailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 140, true);
			this.detailsPanel.Name = "detailsPanel";
			this.detailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1123, 345, true);
			this.detailsPanel.TabIndex = 11;
			this.detailsPanel.AutoScroll = true;
			// 
			// importPanel
			//
			this.importPanel.Controls.Add(this.detailTabControl);
			this.importPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.importPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 243, true);
			this.importPanel.Name = "importPanel";
			this.importPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1123, 345, true);
			this.importPanel.TabIndex = 8;
			this.importPanel.AutoScroll = true;
			// 
			// DetailTabControl
			// 
			this.detailTabControl.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left);
			this.detailTabControl.Controls.Add(this.detailsTabPage);
			this.detailTabControl.Controls.Add(this.cFIATabPage);
			this.detailTabControl.Controls.Add(this.sITTTab);
			this.detailTabControl.Controls.Add(this.nRCANTabPage);
			this.detailTabControl.Controls.Add(this.tiresTabPage);
			this.detailTabControl.Controls.Add(this.sIMATabPage);
			this.detailTabControl.Controls.Add(this.sIMAForCCTabPage);
			this.detailTabControl.Controls.Add(this.AttributesTabPage);
			this.detailTabControl.Controls.Add(this.pGATabPage);
			this.detailTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.detailTabControl.Name = "DetailTabControl";
			this.detailTabControl.SelectedIndex = 0;
			this.detailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1123, 345, true);
			this.detailTabControl.TabIndex = 2;
			// 
			// DetailsTabPage
			// 
			this.detailsTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|edb610e2-e530-492f-af2e-9c6947e97d1c", "Details");
			this.detailsTabPage.Controls.Add(this.productOverridesGroupBox);
			this.detailsTabPage.Controls.Add(this.classDetailsGroupBox);
			this.detailsTabPage.Controls.Add(this.aMMVGroupBox);
			this.detailsTabPage.ForeColor = System.Drawing.SystemColors.ControlText;
			this.detailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.detailsTabPage.Name = "DetailsTabPage";
			this.detailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.detailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 345, true);
			this.detailsTabPage.TabIndex = 0;
			// 
			// ProductOverridesGroupBox
			// 
			this.productOverridesGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|bad274d2-df25-444a-83d6-e7cc5f3dcd4e", "Product Overrides");
			this.productOverridesGroupBox.Controls.Add(this.manufacturerAddressControl);
			this.productOverridesGroupBox.Controls.Add(this.eTExemptionDropEdit);
			this.productOverridesGroupBox.Controls.Add(this.eTRateCodeDropEdit);
			this.productOverridesGroupBox.Controls.Add(this.gSTStatusCodeDropEdit);
			this.productOverridesGroupBox.Controls.Add(this.classificationNumberFindBox);
			this.productOverridesGroupBox.Controls.Add(this.cCA_RN_NKOriginzCodeFindBox2);
			this.productOverridesGroupBox.Controls.Add(this.cCA_ProvinceOfOriginDropEdit2);
			this.productOverridesGroupBox.Controls.Add(this.treatmentCodeDropEdit);
			this.productOverridesGroupBox.Controls.Add(this.tRSNumberTextBox);
			this.productOverridesGroupBox.Controls.Add(this.tariff99CodeFindBox);
			this.productOverridesGroupBox.Controls.Add(this.authorityNumberCodeFindBox);
			this.productOverridesGroupBox.Controls.Add(this.valueForDutyCodeDropEdit);
			this.productOverridesGroupBox.Controls.Add(this.classificationDescriptionTextBox);
			this.productOverridesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 103, true);
			this.productOverridesGroupBox.Name = "ProductOverridesGroupBox";
			this.productOverridesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(805, 166, true);
			this.productOverridesGroupBox.TabIndex = 8;
			this.productOverridesGroupBox.TabStop = false;
			//
			// AMMVGroupBox
			//
			this.aMMVGroupBox.Controls.Add(this.ammvUnitCalcFindBox);
			this.aMMVGroupBox.Controls.Add(this.ammvPercentageCalcEdit);
			this.aMMVGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 275, true);
			this.aMMVGroupBox.Name = "AMMVGroupBox";
			this.aMMVGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(805, 40, true);
			this.aMMVGroupBox.TabIndex = 9;
			this.aMMVGroupBox.TabStop = false;
			// 
			// AmmvUnitCalcFindBox
			//
			this.ammvUnitCalcFindBox.AllowDrop = true;
			this.ammvUnitCalcFindBox.BindToAmount = "PivotsForBinding.CCA_AMMVPerUnit";
			this.ammvUnitCalcFindBox.BindToUnit = "PivotsForBinding.CCA_AMMVPerUnitCurrency";
			this.ammvUnitCalcFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|D797C5FA-A582-4AE4-AD6F-67CE6C343A04", "Assist/Unit", "Assist/AMMV/Unit", "");
			this.ammvUnitCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.ammvUnitCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 20, true);
			this.ammvUnitCalcFindBox.Name = "AmmvUnitCalcFindBox";
			this.ammvUnitCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 17, true);
			this.ammvUnitCalcFindBox.TabIndex = 1;
			// 
			// AmmvPercentageCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ammvPercentageCalcEdit, "PivotsForBinding.CCA_AMMVPercentage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_AMMVPercentage);
			this.ammvPercentageCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|B375B7EF-ED19-4FC0-8E94-C1241B702A9D", "Assist Percentage", "Assist/AMMV Percentage", "");
			this.ammvPercentageCalcEdit.DecimalPlaces = 4;
			this.ammvPercentageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 20, true);
			this.ammvPercentageCalcEdit.Name = "AmmvPercentageCalcEdit";
			this.ammvPercentageCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 17, true);
			this.ammvPercentageCalcEdit.TabIndex = 2;
			this.ammvPercentageCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ManufacturerAddressControl
			// 
			this.manufacturerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.manufacturerAddressControl, "PivotsForBinding.CCA_OA_Manufacturer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_OA_Manufacturer);
			this.manufacturerAddressControl.BindToOrgList = "PivotsForBinding.Lookups+Manufacturers";
			this.manufacturerAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("291ddf65-0257-4bd3-bbe7-8dd79ca6cb1a", "Manufacturer");
			this.manufacturerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(506, 40, true);
			this.manufacturerAddressControl.Name = "ManufacturerAddressControl";
			this.manufacturerAddressControl.PopupCaption = "";
			this.manufacturerAddressControl.ReadOnly = false;
			this.manufacturerAddressControl.ShowAddress = false;
			this.manufacturerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 18, true);
			this.manufacturerAddressControl.TabIndex = 13;
			// 
			// ETExemptionDropEdit
			// 
			this.eTExemptionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.eTExemptionDropEdit, "PivotsForBinding.CCA_ETExemption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_ETExemption);
			this.eTExemptionDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|64A7207F-7E0D-4DFB-A0F9-599B46CCA9E2", "", "", "E/T Ex.", "Excise Tax Exemption Code.");
			this.eTExemptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(640, 113, true);
			this.eTExemptionDropEdit.Name = "ETExemptionDropEdit";
			this.eTExemptionDropEdit.ShowHorizontalScrollBar = true;
			this.eTExemptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 20, true);
			this.eTExemptionDropEdit.TabIndex = 21;
			// 
			// ETRateCodeDropEdit
			// 
			this.eTRateCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.eTRateCodeDropEdit, "PivotsForBinding.CCA_ETRateCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_ETRateCode);
			this.eTRateCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|F18C7FE5-A86F-4981-AFF9-FD114732783D", "", "", "E/T Rt.", "Excise Tax Rate Code.");
			this.eTRateCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(401, 113, true);
			this.eTRateCodeDropEdit.Name = "ETRateCodeDropEdit";
			this.eTRateCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 20, true);
			this.eTRateCodeDropEdit.TabIndex = 20;
			// 
			// GSTStatusCodeDropEdit
			// 
			this.gSTStatusCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.gSTStatusCodeDropEdit, "PivotsForBinding.CCA_GSTStatusCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_GSTStatusCode);
			this.gSTStatusCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|B7338110-34BC-42C3-BD93-BF32D2766EF7", "", "", "GST Code", "GST Status Code.");
			this.gSTStatusCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 113, true);
			this.gSTStatusCodeDropEdit.Name = "GSTStatusCodeDropEdit";
			this.gSTStatusCodeDropEdit.ShowHorizontalScrollBar = true;
			this.gSTStatusCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 20, true);
			this.gSTStatusCodeDropEdit.TabIndex = 19;
			// 
			// ClassificationNumberFindBox
			// 
			this.classificationNumberFindBox.AllowDrop = true;
			this.classificationNumberFindBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.classificationNumberFindBox, "PivotsForBinding.CI_FormattedTariffNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_FormattedTariffNum);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_TariffNumTariffInfo);
			this.classificationNumberFindBox.BindToTariffPropertyInfo = "PivotsForBinding.CI_TariffNumTariffInfo";
			this.classificationNumberFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|c8af2f7f-014b-4c8f-99fb-e664841f6ef9", "HS", "Class. Tariff #", "Classification Tariff Code", "Must be a 10-digit Canadian National Customs Tariff code.");
			this.classificationNumberFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 16, true);
			this.classificationNumberFindBox.Name = "ClassificationNumberFindBox";
			this.classificationNumberFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 20, true);
			this.classificationNumberFindBox.TabIndex = 10;
			// 
			// CCA_RN_NKOriginzCodeFindBox2
			// 
			this.cCA_RN_NKOriginzCodeFindBox2.AllowDrop = true;
			this.cCA_RN_NKOriginzCodeFindBox2.AutoSize = true;
			this.BindingSource.SetBindingMember(this.cCA_RN_NKOriginzCodeFindBox2, "PivotsForBinding.CCA_RN_NKOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_RN_NKOrigin);
			this.cCA_RN_NKOriginzCodeFindBox2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|f86248e0-c37d-449c-990c-7b163c619f57", "Org.", "Origin", "Country/Region of Origin", "");
			this.cCA_RN_NKOriginzCodeFindBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 40, true);
			this.cCA_RN_NKOriginzCodeFindBox2.Name = "CCA_RN_NKOriginzCodeFindBox2";
			this.cCA_RN_NKOriginzCodeFindBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.cCA_RN_NKOriginzCodeFindBox2.TabIndex = 11;
			// 
			// CCA_ProvinceOfOriginDropEdit2
			// 
			this.cCA_ProvinceOfOriginDropEdit2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cCA_ProvinceOfOriginDropEdit2, "PivotsForBinding.CCA_ProvinceOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_ProvinceOfOrigin);
			this.cCA_ProvinceOfOriginDropEdit2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|69ec62b2-11bd-4fa5-a384-99f3b10031ab", "State", "State of Origin", "");
			this.cCA_ProvinceOfOriginDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(322, 40, true);
			this.cCA_ProvinceOfOriginDropEdit2.Name = "CCA_ProvinceOfOriginDropEdit2";
			this.cCA_ProvinceOfOriginDropEdit2.PreBoundMaxLength = 2;
			this.cCA_ProvinceOfOriginDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 20, true);
			this.cCA_ProvinceOfOriginDropEdit2.TabIndex = 12;
			// 
			// TreatmentCodeDropEdit
			// 
			this.treatmentCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.treatmentCodeDropEdit, "PivotsForBinding.CCA_TreatmentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_TreatmentCode);
			this.treatmentCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|F294A240-D220-4FE0-ACF2-5364C17A8C07", "TT", "", "Treatment Code", "A means by which normal rates of duty may be modified according to the Customs Tariff. Refer to the Customs Tariff for information on the applicability of these tariff treatments.");
			this.treatmentCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 88, true);
			this.treatmentCodeDropEdit.Name = "TreatmentCodeDropEdit";
			this.treatmentCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 20, true);
			this.treatmentCodeDropEdit.TabIndex = 16;
			// 
			// TRSNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.tRSNumberTextBox, "PivotsForBinding.CCA_TRSNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_TRSNumber);
			this.tRSNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|09e83b4c-2f8b-40c5-a4c3-43a3a3076a10", "TRS #", "TRS Number", "Technical Reference System ruling number.");
			this.tRSNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(640, 88, true);
			this.tRSNumberTextBox.Name = "TRSNumberTextBox";
			this.tRSNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 20, true);
			this.tRSNumberTextBox.TabIndex = 18;
			// 
			// Tariff99CodeFindBox
			// 
			this.tariff99CodeFindBox.AllowDrop = true;
			this.tariff99CodeFindBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.tariff99CodeFindBox, "PivotsForBinding.CCA_99TariffCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_99TariffCode);
			this.tariff99CodeFindBox.BindToTariffPropertyInfo = null;
			this.tariff99CodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|80f76a15-7382-4521-a8e9-b35e86dbebbd", "Tariff", "Tariff Code", "Applicable if the conditions specified in the Chapter 99 (special classification provisions) tariff item apply.");
			this.tariff99CodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(486, 64, true);
			this.tariff99CodeFindBox.Name = "Tariff99CodeFindBox";
			this.tariff99CodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 20, true);
			this.tariff99CodeFindBox.TabIndex = 15;
			// 
			// AuthorityNumberCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.authorityNumberCodeFindBox, "PivotsForBinding.CCA_AuthorityNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_AuthorityNumber);
			this.authorityNumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(401, 88, true);
			this.authorityNumberCodeFindBox.Name = "AuthorityNumberCodeFindBox";
			this.authorityNumberCodeFindBox.ShowDescriptionBox = false;
			this.authorityNumberCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.authorityNumberCodeFindBox.TabIndex = 17;
			// 
			// ValueForDutyCodeDropEdit
			// 
			this.valueForDutyCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.valueForDutyCodeDropEdit, "PivotsForBinding.CCA_ValueForDutyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_ValueForDutyCode);
			this.valueForDutyCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|a5e2e5ce-50e1-4d77-baa8-f1304c510add", "VFD Cd", "VFD Code", "Value for Duty Code", "The basis on which the value for duty was determined.");
			this.valueForDutyCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 64, true);
			this.valueForDutyCodeDropEdit.Name = "ValueForDutyCodeDropEdit";
			this.valueForDutyCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(308, 20, true);
			this.valueForDutyCodeDropEdit.TabIndex = 14;
			// 
			// ClassificationDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.classificationDescriptionTextBox, "PivotsForBinding.CI_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_Description);
			this.classificationDescriptionTextBox.Name = "classificationDescriptionTextBox";
			this.classificationDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 138, true);
			this.classificationDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 20, true);
			this.classificationDescriptionTextBox.TabIndex = 22;
			// 
			// ClassDetailsGroupBox
			// 
			this.classDetailsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|316187c4-a6a6-4aaf-bfd4-f064c9bc9287", "Classification Lookup Details");
			this.classDetailsGroupBox.Controls.Add(this.cI_CC_CA_99TariffCodeTextBox);
			this.classDetailsGroupBox.Controls.Add(this.cI_CC_CA_ValueForDutyCodeTextBox);
			this.classDetailsGroupBox.Controls.Add(this.cI_CC_FormattedTariffIMPCodeFindBox);
			this.classDetailsGroupBox.Controls.Add(this.cI_CC_CA_TreatmentCodeTextBox);
			this.classDetailsGroupBox.Controls.Add(this.cI_CC_CA_TRSNumberTextBox);
			this.classDetailsGroupBox.Controls.Add(this.cI_CC_CA_AuthorityNumberTextBox);
			this.classDetailsGroupBox.Controls.Add(this.cI_CC_CA_GSTStatusCodeTextBox);
			this.classDetailsGroupBox.Controls.Add(this.cI_CC_CA_ETRateCodeTextBox);
			this.classDetailsGroupBox.Controls.Add(this.cI_CC_CA_ETExemptionTextBox);
			this.classDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.classDetailsGroupBox.Name = "ClassDetailsGroupBox";
			this.classDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(805, 96, true);
			this.classDetailsGroupBox.TabIndex = 7;
			this.classDetailsGroupBox.TabStop = false;
			// 
			// CI_CC_CA_99TariffCodeTextBox
			// 
			this.cI_CC_CA_99TariffCodeTextBox.AllowDrop = true;
			this.cI_CC_CA_99TariffCodeTextBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.cI_CC_CA_99TariffCodeTextBox, "PivotsForBinding.CI_CC_CA_99TariffCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_CC_CA_99TariffCode);
			this.cI_CC_CA_99TariffCodeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|83518f0b-a459-4a6a-8978-3e65c9295b4f", "Tariff", "Tariff Code", "Applicable if the conditions specified in the Chapter 99 (special classification provisions) tariff item apply.");
			this.cI_CC_CA_99TariffCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(486, 19, true);
			this.cI_CC_CA_99TariffCodeTextBox.Name = "CI_CC_CA_99TariffCodeTextBox";
			this.cI_CC_CA_99TariffCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 20, true);
			this.cI_CC_CA_99TariffCodeTextBox.TabIndex = 2;
			// 
			// CI_CC_CA_ValueForDutyCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.cI_CC_CA_ValueForDutyCodeTextBox, "PivotsForBinding.CI_CC_CA_ValueForDutyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_CC_CA_ValueForDutyCode);
			this.cI_CC_CA_ValueForDutyCodeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|225bab81-23af-4214-8f79-afb68af1b8c4", "VFD Cd", "VFD Code", "Value for Duty Code", "The basis on which the value for duty was determined.");
			this.cI_CC_CA_ValueForDutyCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 43, true);
			this.cI_CC_CA_ValueForDutyCodeTextBox.Name = "CI_CC_CA_ValueForDutyCodeTextBox";
			this.cI_CC_CA_ValueForDutyCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 20, true);
			this.cI_CC_CA_ValueForDutyCodeTextBox.TabIndex = 3;
			// 
			// CI_CC_FormattedTariffIMPCodeFindBox
			// 
			this.cI_CC_FormattedTariffIMPCodeFindBox.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
			this.cI_CC_FormattedTariffIMPCodeFindBox.AllowDrop = true;
			this.cI_CC_FormattedTariffIMPCodeFindBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.cI_CC_FormattedTariffIMPCodeFindBox, "PivotsForBinding.CI_CC_FormattedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_CC_FormattedTariff);
			this.cI_CC_FormattedTariffIMPCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|b228dc06-9273-4132-ae0c-163a5248049b", "HS", "Class. Tariff #", "Classification Tariff Code", "Customs Classification Tariff (HS) Code.");
			this.cI_CC_FormattedTariffIMPCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 19, true);
			this.cI_CC_FormattedTariffIMPCodeFindBox.Name = "CI_CC_FormattedTariffIMPCodeFindBox";
			this.cI_CC_FormattedTariffIMPCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(326, 20, true);
			this.cI_CC_FormattedTariffIMPCodeFindBox.TabIndex = 1;
			// 
			// CI_CC_CA_TreatmentCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.cI_CC_CA_TreatmentCodeTextBox, "PivotsForBinding.CI_CC_CA_TreatmentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_CC_CA_TreatmentCode);
			this.cI_CC_CA_TreatmentCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 43, true);
			this.cI_CC_CA_TreatmentCodeTextBox.Name = "CI_CC_CA_TreatmentCodeTextBox";
			this.cI_CC_CA_TreatmentCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 20, true);
			this.cI_CC_CA_TreatmentCodeTextBox.TabIndex = 4;
			// 
			// CI_CC_CA_TRSNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.cI_CC_CA_TRSNumberTextBox, "PivotsForBinding.CI_CC_CA_TRSNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_CC_CA_TRSNumber);
			this.cI_CC_CA_TRSNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|6e2b60b2-098e-4f6b-9209-321a2bae11e5", "TRS #", "TRS Number", "Technical Reference System ruling number.");
			this.cI_CC_CA_TRSNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(640, 43, true);
			this.cI_CC_CA_TRSNumberTextBox.Name = "CI_CC_CA_TRSNumberTextBox";
			this.cI_CC_CA_TRSNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 20, true);
			this.cI_CC_CA_TRSNumberTextBox.TabIndex = 6;
			// 
			// CI_CC_CA_AuthorityNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.cI_CC_CA_AuthorityNumberTextBox, "PivotsForBinding.CI_CC_CA_AuthorityNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_CC_CA_AuthorityNumber);
			this.cI_CC_CA_AuthorityNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|dd3da52e-0ec4-4b29-b0a5-e9df1ba8fdd1", "Auth/Pmt", "Special Auth/Pmt", "Special Authority/Permit", "A permit number or Order in Council (OIC) authorization number to import goods under special conditions.");
			this.cI_CC_CA_AuthorityNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(401, 43, true);
			this.cI_CC_CA_AuthorityNumberTextBox.Name = "CI_CC_CA_AuthorityNumberTextBox";
			this.cI_CC_CA_AuthorityNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.cI_CC_CA_AuthorityNumberTextBox.TabIndex = 5;
			// 
			// CI_CC_CA_GSTStatusCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.cI_CC_CA_GSTStatusCodeTextBox, "PivotsForBinding.CI_CC_CA_GSTStatusCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_CC_CA_GSTStatusCode);
			this.cI_CC_CA_GSTStatusCodeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|B7338110-34BC-42C3-BD93-BF32D2766EF7", "", "", "GST Code", "GST Status Code.");
			this.cI_CC_CA_GSTStatusCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 68, true);
			this.cI_CC_CA_GSTStatusCodeTextBox.Name = "CI_CC_CA_GSTStatusCodeTextBox";
			this.cI_CC_CA_GSTStatusCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
			this.cI_CC_CA_GSTStatusCodeTextBox.TabIndex = 7;
			// 
			// CI_CC_CA_ETRateCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.cI_CC_CA_ETRateCodeTextBox, "PivotsForBinding.CI_CC_CA_ETRateCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_CC_CA_ETRateCode);
			this.cI_CC_CA_ETRateCodeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|F18C7FE5-A86F-4981-AFF9-FD114732783D", "", "", "E/T Rt.", "Excise Tax Rate Code.");
			this.cI_CC_CA_ETRateCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(401, 68, true);
			this.cI_CC_CA_ETRateCodeTextBox.Name = "CI_CC_CA_ETRateCodeTextBox";
			this.cI_CC_CA_ETRateCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.cI_CC_CA_ETRateCodeTextBox.TabIndex = 8;
			// 
			// CI_CC_CA_ETExemptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.cI_CC_CA_ETExemptionTextBox, "PivotsForBinding.CI_CC_CA_ETExemption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_CC_CA_ETExemption);
			this.cI_CC_CA_ETExemptionTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|64A7207F-7E0D-4DFB-A0F9-599B46CCA9E2", "", "", "E/T Ex.", "Excise Tax Exemption Code.");
			this.cI_CC_CA_ETExemptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(640, 68, true);
			this.cI_CC_CA_ETExemptionTextBox.Name = "CI_CC_CA_ETExemptionTextBox";
			this.cI_CC_CA_ETExemptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.cI_CC_CA_ETExemptionTextBox.TabIndex = 9;
			// 
			// CFIATabPage
			// 
			this.cFIATabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|06cb0b5b-02f6-49e7-b93b-8d7d38390d55", "CFIA");
			this.cFIATabPage.Controls.Add(this.cFIARegNumbersGroupBox);
			this.cFIATabPage.Controls.Add(this.cFIAUSStateOfOriginDropEdit);
			this.cFIATabPage.Controls.Add(this.nKCFIAOriginCodeFindBox);
			this.cFIATabPage.Controls.Add(this.miscIDCodeFindBox);
			this.cFIATabPage.Controls.Add(this.endUseCodeFindBox);
			this.cFIATabPage.Controls.Add(this.destinationProvinceDropEdit);
			this.cFIATabPage.Controls.Add(this.airsCodeTextBox);
			this.cFIATabPage.Controls.Add(this.requirementVerTextBox);
			this.cFIATabPage.Controls.Add(this.requirementIDTextBox);
			this.cFIATabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.cFIATabPage.Name = "CFIATabPage";
			this.cFIATabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.cFIATabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 323, true);
			this.cFIATabPage.TabIndex = 1;
			// 
			// CFIARegNumbersGroupBox
			// 
			this.cFIARegNumbersGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|788b4b1f-d83f-4c42-8797-86f49fd93c1d", "Registration/Permit Numbers");
			this.cFIARegNumbersGroupBox.Controls.Add(this.cFIARegNumbersGrid);
			this.cFIARegNumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(527, 6, true);
			this.cFIARegNumbersGroupBox.Name = "CFIARegNumbersGroupBox";
			this.cFIARegNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 176, true);
			this.cFIARegNumbersGroupBox.TabIndex = 9;
			this.cFIARegNumbersGroupBox.TabStop = false;
			// 
			// CFIARegNumbersGrid
			// 
			this.cFIARegNumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.cFIARegNumbersGrid, "PivotsForBinding.CFIARegistrationNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CFIARegistrationNumbers);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CFIARegistrationNumber)(((System.Collections.IList)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CFIARegistrationNumbers)).SyncRoot)).CY_Code);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CFIARegistrationNumber)(((System.Collections.IList)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CFIARegistrationNumbers)).SyncRoot)).CY_Data);
			this.cFIARegNumbersGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CY_Code";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|bef94485-0bc6-403e-b276-42f1236bf54d", "Reg/Permit Number");
			zTextBoxColumnStyleInfo3.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.cFIARegNumbersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.cFIARegNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.cFIARegNumbersGrid.GridId = "359cfa60-a75c-4695-a8d9-dc4fa87f4cea";
			this.cFIARegNumbersGrid.CopySelectedRowsAllowed = true;
			this.cFIARegNumbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cFIARegNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.cFIARegNumbersGrid.LayoutKey = "CFIARegNumbersGrid";
			this.cFIARegNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.cFIARegNumbersGrid.Name = "CFIARegNumbersGrid";
			this.cFIARegNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 157, true);
			this.cFIARegNumbersGrid.TabIndex = 1;
			// 
			// CFIAUSStateOfOriginDropEdit
			// 
			this.cFIAUSStateOfOriginDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cFIAUSStateOfOriginDropEdit, "PivotsForBinding.CCA_CFIAUSStateOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_CFIAUSStateOfOrigin);
			this.cFIAUSStateOfOriginDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|857ebfd8-0d56-4a22-b519-2b4173f9f8b9", "State");
			this.cFIAUSStateOfOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 162, true);
			this.cFIAUSStateOfOriginDropEdit.Name = "CFIAUSStateOfOriginDropEdit";
			this.cFIAUSStateOfOriginDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 20, true);
			this.cFIAUSStateOfOriginDropEdit.TabIndex = 8;
			// 
			// NKCFIAOriginCodeFindBox
			// 
			this.nKCFIAOriginCodeFindBox.AllowDrop = true;
			this.nKCFIAOriginCodeFindBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.nKCFIAOriginCodeFindBox, "PivotsForBinding.CCA_RN_NKCFIAOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_RN_NKCFIAOrigin);
			this.nKCFIAOriginCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|6f5be225-7815-4670-b19a-5e90cb73906d", "CFIA Org.", "Country/Region of Origin (CFIA)", "Country/Region of Origin, as per CFIA origin rules.");
			this.nKCFIAOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 136, true);
			this.nKCFIAOriginCodeFindBox.Name = "NKCFIAOriginCodeFindBox";
			this.nKCFIAOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 20, true);
			this.nKCFIAOriginCodeFindBox.TabIndex = 7;
			// 
			// MiscIDCodeFindBox
			// 
			this.miscIDCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.miscIDCodeFindBox, "PivotsForBinding.CCA_MiscID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_MiscID);
			this.miscIDCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|9252df3c-189d-483a-be01-175ff977908b", "Misc. ID", "Miscellaneous ID", "Miscellaneous information related to the commodity which may be needed to identify the import requirements.");
			this.miscIDCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(358, 110, true);
			this.miscIDCodeFindBox.Name = "MiscIDCodeFindBox";
			this.miscIDCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.miscIDCodeFindBox.TabIndex = 6;
			// 
			// EndUseCodeFindBox
			// 
			this.endUseCodeFindBox.AllowDrop = true;
			this.endUseCodeFindBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.endUseCodeFindBox, "PivotsForBinding.CCA_EndUse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_EndUse);
			this.endUseCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|1ab8d937-12cb-4f84-b709-5145402c81af", "End Use", "A code to identify how the a product will be used within Canada. i.e. consumption, animal feed, or processing.");
			this.endUseCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 110, true);
			this.endUseCodeFindBox.Name = "EndUseCodeFindBox";
			this.endUseCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 20, true);
			this.endUseCodeFindBox.TabIndex = 5;
			// 
			// DestinationProvinceDropEdit
			// 
			this.destinationProvinceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.destinationProvinceDropEdit, "PivotsForBinding.CCA_DestinationProvince");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_DestinationProvince);
			this.destinationProvinceDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|326f1fc8-a9c5-4243-a7af-cd5bf293b657", "Dest. Province", "Destination Province", "Province that represents the shipments final destination within Canada.");
			this.destinationProvinceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 84, true);
			this.destinationProvinceDropEdit.Name = "DestinationProvinceDropEdit";
			this.destinationProvinceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 20, true);
			this.destinationProvinceDropEdit.TabIndex = 4;
			// 
			// AirsCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.airsCodeTextBox, "PivotsForBinding.CCA_AirsCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_AirsCode);
			this.airsCodeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|9605bdcb-b857-4ac0-a243-781fea2c07c8", "Airs", "Airs Code", "");
			this.airsCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 58, true);
			this.airsCodeTextBox.Name = "AirsCodeTextBox";
			this.airsCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 20, true);
			this.airsCodeTextBox.TabIndex = 3;
			// 
			// RequirementVerTextBox
			// 
			this.requirementVerTextBox.AcceptsTab = true;
			this.BindingSource.SetBindingMember(this.requirementVerTextBox, "PivotsForBinding.CCA_RequirementVersion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_RequirementVersion);
			this.requirementVerTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|8e23cb21-d2f7-49d2-9850-45dffbf17f8d", "Req. Ver.", "Requirement Version", "A number associated to the Requirement ID as defined within the AIRS system. Sequential number starting at 1 and incremented each time a requirement is updated.");
			this.requirementVerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 32, true);
			this.requirementVerTextBox.Name = "RequirementVerTextBox";
			this.requirementVerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 20, true);
			this.requirementVerTextBox.TabIndex = 2;
			// 
			// RequirementIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.requirementIDTextBox, "PivotsForBinding.CCA_RequirementID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_RequirementID);
			this.requirementIDTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|03550c69-e4cd-415c-9a7e-63cb129b12d7", "Req. Id.", "Requirement ID", "An Agriculture Import Reference System (AIRS) system generated number that uniquely identifies the import requirements.");
			this.requirementIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 6, true);
			this.requirementIDTextBox.Name = "RequirementIDTextBox";
			this.requirementIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 20, true);
			this.requirementIDTextBox.TabIndex = 1;
			// 
			// SITTTab
			// 
			this.sITTTab.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|64dfc225-b307-4959-9838-156b5f36a280", "SITT");
			this.sITTTab.Controls.Add(this.numbersGroupBox);
			this.sITTTab.Controls.Add(this.sITTBrandNameTextBox);
			this.sITTTab.Controls.Add(this.sITTModelNumberTextBox);
			this.sITTTab.Controls.Add(this.sITTModelTextBox);
			this.sITTTab.Controls.Add(this.sITTImportReasonCodeDropEdit);
			this.sITTTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.sITTTab.Name = "SITTTab";
			this.sITTTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.sITTTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 323, true);
			this.sITTTab.TabIndex = 2;
			// 
			// NumbersGroupBox
			// 
			this.numbersGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|b25def2d-eaa8-45e4-a130-85c2ea45a39c", "Certificate Numbers");
			this.numbersGroupBox.Controls.Add(this.sITTRegistrationNumbersGrid);
			this.numbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(527, 6, true);
			this.numbersGroupBox.Name = "NumbersGroupBox";
			this.numbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 185, true);
			this.numbersGroupBox.TabIndex = 8;
			this.numbersGroupBox.TabStop = false;
			// 
			// SITTRegistrationNumbersGrid
			// 
			this.sITTRegistrationNumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.sITTRegistrationNumbersGrid, "PivotsForBinding.SITTCertificationNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).SITTCertificationNumbers);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((SITTCertificationNumber)(((System.Collections.IList)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).SITTCertificationNumbers)).SyncRoot)).CY_Data);
			this.sITTRegistrationNumbersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|fbfd53b2-dc95-4c73-b110-5403521442b2", "Number");
			zTextBoxColumnStyleInfo4.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.sITTRegistrationNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.sITTRegistrationNumbersGrid.GridId = "b63fb2fe-9743-4152-bc5c-b9e723c6ac16";
			this.sITTRegistrationNumbersGrid.CopySelectedRowsAllowed = true;
			this.sITTRegistrationNumbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sITTRegistrationNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.sITTRegistrationNumbersGrid.LayoutKey = "SITTRegistrationNumbersGrid";
			this.sITTRegistrationNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.sITTRegistrationNumbersGrid.Name = "SITTRegistrationNumbersGrid";
			this.sITTRegistrationNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 166, true);
			this.sITTRegistrationNumbersGrid.TabIndex = 1;
			// 
			// SITTBrandNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.sITTBrandNameTextBox, "PivotsForBinding.CCA_BrandName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_BrandName);
			this.sITTBrandNameTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|ab7f4139-af7b-4294-8d92-3ca9f2af5308", "Name", "Brand Name", "Product Brand Name", "The name of a product of a particular make or trademark.");
			this.sITTBrandNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 86, true);
			this.sITTBrandNameTextBox.Name = "SITTBrandNameTextBox";
			this.sITTBrandNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 20, true);
			this.sITTBrandNameTextBox.TabIndex = 7;
			// 
			// SITTModelNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.sITTModelNumberTextBox, "PivotsForBinding.CCA_ModelNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_ModelNumber);
			this.sITTModelNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|537e73bd-ca24-41df-8ca9-0d27455795d2", "Product Number", "The identification number of a product model.");
			this.sITTModelNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 60, true);
			this.sITTModelNumberTextBox.Name = "SITTModelNumberTextBox";
			this.sITTModelNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 20, true);
			this.sITTModelNumberTextBox.TabIndex = 6;
			// 
			// SITTModelTextBox
			//  
			this.BindingSource.SetBindingMember(this.sITTModelTextBox, "PivotsForBinding.CCA_Model");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_Model);
			this.sITTModelTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|558395be-51a7-4228-a4bc-e3f881551ce6", "Product Model", "The design, or style, or structure of a product.");
			this.sITTModelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 34, true);
			this.sITTModelTextBox.Name = "SITTModelTextBox";
			this.sITTModelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 20, true);
			this.sITTModelTextBox.TabIndex = 5;
			// 
			// SITTImportReasonCodeDropEdit
			// 
			this.sITTImportReasonCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.sITTImportReasonCodeDropEdit, "PivotsForBinding.CCA_ImportReasonCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_ImportReasonCode);
			this.sITTImportReasonCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|ba26a631-9b38-4086-9716-b76f19fa897f", "Reason Cd", "Import Reason", "Import Reason Code", "The reason a product is being imported.");
			this.sITTImportReasonCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 8, true);
			this.sITTImportReasonCodeDropEdit.Name = "SITTImportReasonCodeDropEdit";
			this.sITTImportReasonCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 20, true);
			this.sITTImportReasonCodeDropEdit.TabIndex = 4;
			// 
			// NRCANTabPage
			// 
			this.nRCANTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|a5541c25-eadf-4eab-8724-052a295a5ef4", "NRCAN");
			this.nRCANTabPage.Controls.Add(this.nRCANTypeSizeTextBox);
			this.nRCANTabPage.Controls.Add(this.nRCANBrandNameTextBox);
			this.nRCANTabPage.Controls.Add(this.nRCANModelNumberTextBox);
			this.nRCANTabPage.Controls.Add(this.nRCANModelTextBox);
			this.nRCANTabPage.Controls.Add(this.nRCANImportReasonCodeDropEdit);
			this.nRCANTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.nRCANTabPage.Name = "NRCANTabPage";
			this.nRCANTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.nRCANTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 323, true);
			this.nRCANTabPage.TabIndex = 3;
			// 
			// NRCANTypeSizeTextBox
			// 
			this.nRCANTypeSizeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.nRCANTypeSizeTextBox, "PivotsForBinding.CCA_TypeSize");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_TypeSize);
			this.nRCANTypeSizeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|d8b6c96e-5e67-4748-bc54-774a7d11a822", "Motor HP/RPM", "The type and size for NRCAN commodities. HP/RPM - if motors.");
			this.nRCANTypeSizeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 112, true);
			this.nRCANTypeSizeTextBox.Name = "NRCANTypeSizeTextBox";
			this.nRCANTypeSizeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 20, true);
			this.nRCANTypeSizeTextBox.TabIndex = 9;
			// 
			// NRCANBrandNameTextBox
			// 
			this.nRCANBrandNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.nRCANBrandNameTextBox, "PivotsForBinding.CCA_BrandName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_BrandName);
			this.nRCANBrandNameTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|f2037634-6748-4256-931f-8296209dd1f2", "Name", "Brand Name", "Product Brand Name", "The name of a product of a particular make or trademark.");
			this.nRCANBrandNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 86, true);
			this.nRCANBrandNameTextBox.Name = "NRCANBrandNameTextBox";
			this.nRCANBrandNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 20, true);
			this.nRCANBrandNameTextBox.TabIndex = 8;
			// 
			// NRCANModelNumberTextBox
			// 
			this.nRCANModelNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.nRCANModelNumberTextBox, "PivotsForBinding.CCA_ModelNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_ModelNumber);
			this.nRCANModelNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|cc4ce6e5-ea63-4b62-9eee-81cc6da39148", "Product Number", "The identification number of a product model.");
			this.nRCANModelNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 60, true);
			this.nRCANModelNumberTextBox.Name = "NRCANModelNumberTextBox";
			this.nRCANModelNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 20, true);
			this.nRCANModelNumberTextBox.TabIndex = 7;
			// 
			// NRCANModelTextBox
			// 
			this.nRCANModelTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.nRCANModelTextBox, "PivotsForBinding.CCA_Model");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_Model);
			this.nRCANModelTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|7f60aca4-ef20-4be3-94e1-9162c3ed710e", "Product Model", "The design, or style, or structure of a product.");
			this.nRCANModelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 34, true);
			this.nRCANModelTextBox.Name = "NRCANModelTextBox";
			this.nRCANModelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 20, true);
			this.nRCANModelTextBox.TabIndex = 6;
			// 
			// NRCANImportReasonCodeDropEdit
			// 
			this.nRCANImportReasonCodeDropEdit.AllowDrop = true;
			this.nRCANImportReasonCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.nRCANImportReasonCodeDropEdit, "PivotsForBinding.CCA_ImportReasonCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_ImportReasonCode);
			this.nRCANImportReasonCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|30641ec9-98d1-4584-a162-d39fd8ee8859", "Reason Cd", "Import Reason", "Import Reason Code", "The reason a product is being imported.");
			this.nRCANImportReasonCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 8, true);
			this.nRCANImportReasonCodeDropEdit.Name = "NRCANImportReasonCodeDropEdit";
			this.nRCANImportReasonCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 20, true);
			this.nRCANImportReasonCodeDropEdit.TabIndex = 5;
			// 
			// TiresTabPage
			// 
			this.tiresTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|4a8f19e4-528b-4500-9984-c8b2c6365727", "Tires");
			this.tiresTabPage.Controls.Add(this.tiresTypeSizeTextBox);
			this.tiresTabPage.Controls.Add(this.tiresBrandNameTextBox);
			this.tiresTabPage.Controls.Add(this.compliantImportDateCheckBox);
			this.tiresTabPage.Controls.Add(this.compliantCompletionCheckBox);
			this.tiresTabPage.Controls.Add(this.tIINTextBox);
			this.tiresTabPage.Controls.Add(this.tiresImportReasonCodeDropEdit);
			this.tiresTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.tiresTabPage.Name = "TiresTabPage";
			this.tiresTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.tiresTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 323, true);
			this.tiresTabPage.TabIndex = 4;
			// 
			// TiresTypeSizeTextBox
			// 
			this.BindingSource.SetBindingMember(this.tiresTypeSizeTextBox, "PivotsForBinding.CCA_TypeSize");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_TypeSize);
			this.tiresTypeSizeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|519269d4-18b6-4654-995a-87a362d3c1af", "Tire Kind/Class/Dimension", "The kind/class and dimensions of the tire.");
			this.tiresTypeSizeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 126, true);
			this.tiresTypeSizeTextBox.Name = "TiresTypeSizeTextBox";
			this.tiresTypeSizeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			this.tiresTypeSizeTextBox.TabIndex = 10;
			// 
			// TiresBrandNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.tiresBrandNameTextBox, "PivotsForBinding.CCA_BrandName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_BrandName);
			this.tiresBrandNameTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|BDA66AEF-C48D-4673-A351-7DBA7B83095B", "Name", "Brand Name", "Product Brand Name", "The name of a product of a particular make or trademark.");
			this.tiresBrandNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 100, true);
			this.tiresBrandNameTextBox.Name = "TiresBrandNameTextBox";
			this.tiresBrandNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			this.tiresBrandNameTextBox.TabIndex = 9;
			// 
			// CompliantImportDateCheckBox
			// 
			this.compliantImportDateCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.compliantImportDateCheckBox, "PivotsForBinding.CCA_CompliantImportDateIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_CompliantImportDateIndicator);
			this.compliantImportDateCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|d599bb5e-0d92-4212-872d-741395091a91", "Import Date Compliant", "Indicates the tire complied with Transport Canada regulations on the date of its completion.");
			this.compliantImportDateCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.compliantImportDateCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 80, true);
			this.compliantImportDateCheckBox.Name = "CompliantImportDateCheckBox";
			this.compliantImportDateCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 17, true);
			this.compliantImportDateCheckBox.TabIndex = 8;
			this.compliantImportDateCheckBox.UseVisualStyleBackColor = true;
			// 
			// CompliantCompletionCheckBox
			// 
			this.compliantCompletionCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.compliantCompletionCheckBox, "PivotsForBinding.CCA_CompliantCompletion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_CompliantCompletion);
			this.compliantCompletionCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|ac870c36-30d4-4690-a783-6d2aa1b47232", "Compliant Completion", "Indicates that tire complied with Transport Canada regulations on the date of its completion.");
			this.compliantCompletionCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.compliantCompletionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 59, true);
			this.compliantCompletionCheckBox.Name = "CompliantCompletionCheckBox";
			this.compliantCompletionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 17, true);
			this.compliantCompletionCheckBox.TabIndex = 7;
			this.compliantCompletionCheckBox.UseVisualStyleBackColor = true;
			// 
			// TIINTextBox
			// 
			this.BindingSource.SetBindingMember(this.tIINTextBox, "PivotsForBinding.CCA_TIIN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_TIIN);
			this.tIINTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|14ffa96e-8c22-4310-92ce-f08bee0f3535", "TIIN", "Tire Importer ID", "The tire importer identification number assigned by Transport Canada to the importer for the import of tires for retread.");
			this.tIINTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 32, true);
			this.tIINTextBox.Name = "TIINTextBox";
			this.tIINTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			this.tIINTextBox.TabIndex = 6;
			// 
			// TiresImportReasonCodeDropEdit
			// 
			this.tiresImportReasonCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.tiresImportReasonCodeDropEdit, "PivotsForBinding.CCA_ImportReasonCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_ImportReasonCode);
			this.tiresImportReasonCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|06b31700-30e9-43ed-91c5-fc6a387e011c", "Reason Cd", "Import Reason", "Import Reason Code", "The reason a product is being imported.");
			this.tiresImportReasonCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 6, true);
			this.tiresImportReasonCodeDropEdit.Name = "TiresImportReasonCodeDropEdit";
			this.tiresImportReasonCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			this.tiresImportReasonCodeDropEdit.TabIndex = 5;
			// 
			// SIMATabPage
			// 
			this.sIMATabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2ea2f43a-e931-4bdc-ace6-a9792699cf9d", "SIMA");
			this.sIMATabPage.Controls.Add(this.sIMAGroupBox);
			this.sIMATabPage.Controls.Add(this.sIMATopPanel);
			this.sIMATabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.sIMATabPage.Name = "SIMATabPage";
			this.sIMATabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.sIMATabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 323, true);
			this.sIMATabPage.TabIndex = 6;
			this.sIMATabPage.Text = "SIMA";
			// 
			// SIMAGroupBox
			// 
			this.sIMAGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|1ebf87c4-b079-41d1-8732-406b4ab61581", "Duties and Taxes");
			this.sIMAGroupBox.Controls.Add(this.sIMAFeeGrid);
			this.sIMAGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sIMAGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 90, true);
			this.sIMAGroupBox.Name = "SIMAGroupBox";
			this.sIMAGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 287, true);
			this.sIMAGroupBox.TabIndex = 2;
			this.sIMAGroupBox.TabStop = false;
			// 
			// SIMAFeeGrid
			// 
			this.sIMAFeeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.sIMAFeeGrid, "PivotsForBinding.DutiesAndTaxes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).DutiesAndTaxes);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)(((System.Collections.IList)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).C1_TaxType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)(((System.Collections.IList)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).C1_Code);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)(((System.Collections.IList)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).Description);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)(((System.Collections.IList)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).C1_RateType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)(((System.Collections.IList)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).C1_Rate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)(((System.Collections.IList)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).C1_UnitOfMeasure);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)(((System.Collections.IList)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).C1_NormalValuePerUnit);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)(((System.Collections.IList)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).C1_NormalValueCurrency);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)(((System.Collections.IList)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).C1_ForeignRate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)(((System.Collections.IList)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).DutiesAndTaxes)).SyncRoot)).C1_ForeignCurrency);
			this.sIMAFeeGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.ColumnName = "C1_TaxType";
			zTextBoxColumnStyleInfo9.ColumnName = "Description";
			zDropEditColumnStyleInfo3.ColumnName = "C1_ExemptCode";
			zCheckBoxColumnStyleInfo1.ColumnName = "C1_Override";
			zDropEditColumnStyleInfo4.ColumnName = "C1_RateType";
			zCalcEditColumnStyleInfo1.ColumnName = "C1_Rate";
			zDropEditColumnStyleInfo6.ColumnName = "C1_UnitOfMeasure";
			zCalcEditColumnStyleInfo2.ColumnName = "C1_NormalValuePerUnit";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("DutyAndTaxGrid|6e57d040-b852-4f9b-85f1-3a1677102842", "Normal Values");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "C1_NormalValueCurrency";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCodeFindBoxColumnStyleInfo2.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("DutyAndTaxGrid|6e57d040-b852-4f9b-85f1-3a1677102842", "Normal Values");
			zCalcEditColumnStyleInfo3.ColumnName = "C1_ForeignRate";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("DutyAndTaxGrid|fc8684ad-9bd1-44da-8292-fcfa29219397", "Foreign Rate");
			zCodeFindBoxColumnStyleInfo3.ColumnName = "C1_ForeignCurrency";
			zCodeFindBoxColumnStyleInfo3.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("DutyAndTaxGrid|fc8684ad-9bd1-44da-8292-fcfa29219397", "Foreign Rate");
			this.sIMAFeeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.sIMAFeeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.sIMAFeeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.sIMAFeeGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.sIMAFeeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.sIMAFeeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.sIMAFeeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.sIMAFeeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.sIMAFeeGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.sIMAFeeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.sIMAFeeGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.sIMAFeeGrid.GridId = "9b9dfb89-6368-44cb-a4b6-6a9da8f32704";
			this.sIMAFeeGrid.CopySelectedRowsAllowed = true;
			this.sIMAFeeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sIMAFeeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.sIMAFeeGrid.LayoutKey = "SIMAFeeGrid";
			this.sIMAFeeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.sIMAFeeGrid.Name = "SIMAFeeGrid";
			this.sIMAFeeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 268, true);
			this.sIMAFeeGrid.TabIndex = 1;
			// 
			// SIMATopPanel
			// 
			this.sIMATopPanel.Controls.Add(this.sIMAMeasureDescriptionTextBox);
			this.sIMATopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.sIMATopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.sIMATopPanel.Name = "SIMATopPanel";
			this.sIMATopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 30, true);
			this.sIMATopPanel.TabIndex = 1;
			// 
			// SIMAMeasureDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.sIMAMeasureDescriptionTextBox, "PivotsForBinding.CCA_SIMADumpingDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_SIMADumpingDesc);
			this.sIMAMeasureDescriptionTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|069FB932-02B5-4C88-9497-B948A6679326", "SIMA Measure");
			this.sIMAMeasureDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 5, true);
			this.sIMAMeasureDescriptionTextBox.Name = "SIMAMeasureDescriptionTextBox";
			this.sIMAMeasureDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.sIMAMeasureDescriptionTextBox.TabIndex = 1;
			// 
			// SIMAForCCTabPage
			// 
			this.sIMAForCCTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3ec2f43a-e931-4bdc-ace6-a97926992fdd", "SIMA");
			this.sIMAForCCTabPage.Controls.Add(this.sIMAForCCGroupBox);
			this.sIMAForCCTabPage.Controls.Add(this.sIMAForCCTopPanel);
			this.sIMAForCCTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.sIMAForCCTabPage.Name = "SIMAForCCTabPage";
			this.sIMAForCCTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.sIMAForCCTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 323, true);
			this.sIMAForCCTabPage.TabIndex = 7;
			this.sIMAForCCTabPage.Text = "SIMA";
			// 
			// SIMAForCCGroupBox
			// 
			this.sIMAForCCGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|cebf87c4-b079-41d1-8732-406b4abe15a1", "Duties and Taxes");
			this.sIMAForCCGroupBox.Controls.Add(this.sIMAFeeForCCGrid);
			this.sIMAForCCGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sIMAForCCGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 90, true);
			this.sIMAForCCGroupBox.Name = "SIMAForCCGroupBox";
			this.sIMAForCCGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 287, true);
			this.sIMAForCCGroupBox.TabIndex = 2;
			this.sIMAForCCGroupBox.TabStop = false;
			// 
			// SIMAFeeForCCGrid
			// 
			this.sIMAFeeForCCGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.sIMAFeeForCCGrid, "PivotsForBinding.DutiesAndTaxesForCC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).DutiesAndTaxes);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)(((System.Collections.IList)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).DutiesAndTaxesForCC)).SyncRoot)).C1_TaxType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)(((System.Collections.IList)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).DutiesAndTaxesForCC)).SyncRoot)).C1_Code);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)(((System.Collections.IList)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).DutiesAndTaxesForCC)).SyncRoot)).Description);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)(((System.Collections.IList)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).DutiesAndTaxesForCC)).SyncRoot)).C1_RateType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)(((System.Collections.IList)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).DutiesAndTaxesForCC)).SyncRoot)).C1_Rate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)(((System.Collections.IList)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).DutiesAndTaxesForCC)).SyncRoot)).C1_UnitOfMeasure);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)(((System.Collections.IList)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).DutiesAndTaxesForCC)).SyncRoot)).C1_NormalValuePerUnit);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)(((System.Collections.IList)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).DutiesAndTaxesForCC)).SyncRoot)).C1_NormalValueCurrency);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)(((System.Collections.IList)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).DutiesAndTaxesForCC)).SyncRoot)).C1_ForeignRate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)(((System.Collections.IList)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).DutiesAndTaxesForCC)).SyncRoot)).C1_ForeignCurrency);
			this.sIMAFeeForCCGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo7.ColumnName = "C1_TaxType";
			zTextBoxColumnStyleInfo10.ColumnName = "Description";
			zDropEditColumnStyleInfo8.ColumnName = "C1_ExemptCode";
			zCheckBoxColumnStyleInfo2.ColumnName = "C1_Override";
			zDropEditColumnStyleInfo9.ColumnName = "C1_RateType";
			zCalcEditColumnStyleInfo4.ColumnName = "C1_Rate";
			zDropEditColumnStyleInfo10.ColumnName = "C1_UnitOfMeasure";
			zCalcEditColumnStyleInfo5.ColumnName = "C1_NormalValuePerUnit";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zCalcEditColumnStyleInfo5.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("DutyAndTaxGrid|1e57d040-b852-4f9b-85f1-3a167710a832", "Normal Values");
			zCodeFindBoxColumnStyleInfo4.ColumnName = "C1_NormalValueCurrency";
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCodeFindBoxColumnStyleInfo4.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("DutyAndTaxGrid|1e57d040-b852-4f9b-85f1-3a167710a832", "Normal Values");
			zCalcEditColumnStyleInfo6.ColumnName = "C1_ForeignRate";
			zCalcEditColumnStyleInfo6.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("DutyAndTaxGrid|1c8684ad-9bd1-44da-8292-fcfa2921a337", "Foreign Rate");
			zCodeFindBoxColumnStyleInfo5.ColumnName = "C1_ForeignCurrency";
			zCodeFindBoxColumnStyleInfo5.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("DutyAndTaxGrid|1c8684ad-9bd1-44da-8292-fcfa2921a337", "Foreign Rate");
			this.sIMAFeeForCCGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.sIMAFeeForCCGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.sIMAFeeForCCGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.sIMAFeeForCCGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.sIMAFeeForCCGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.sIMAFeeForCCGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.sIMAFeeForCCGrid.ColumnStyles.Add(zDropEditColumnStyleInfo10);
			this.sIMAFeeForCCGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.sIMAFeeForCCGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.sIMAFeeForCCGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.sIMAFeeForCCGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo5);
			this.sIMAFeeForCCGrid.GridId = "cb9dfb89-6368-44cb-a4b6-6a9da8f3270a";
			this.sIMAFeeForCCGrid.CopySelectedRowsAllowed = true;
			this.sIMAFeeForCCGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sIMAFeeForCCGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.sIMAFeeForCCGrid.LayoutKey = "SIMAFeeForCCGrid";
			this.sIMAFeeForCCGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.sIMAFeeForCCGrid.Name = "SIMAFeeForCCGrid";
			this.sIMAFeeForCCGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 268, true);
			this.sIMAFeeForCCGrid.TabIndex = 1;
			// 
			// SIMAForCCTopPanel
			// 
			this.sIMAForCCTopPanel.Controls.Add(this.sIMAMeasureDescriptionForCCTextBox);
			this.sIMAForCCTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.sIMAForCCTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.sIMAForCCTopPanel.Name = "SIMAForCCTopPanel";
			this.sIMAForCCTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 30, true);
			this.sIMAForCCTopPanel.TabIndex = 1;
			// 
			// SIMAMeasureDescriptionForCCTextBox
			// 
			this.BindingSource.SetBindingMember(this.sIMAMeasureDescriptionForCCTextBox, "PivotsForBinding.CCA_SIMADumpingDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_SIMADumpingDesc);
			this.sIMAMeasureDescriptionForCCTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|A69FB932-02B5-4C88-9497-B948A66793C6", "SIMA Measure");
			this.sIMAMeasureDescriptionForCCTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 5, true);
			this.sIMAMeasureDescriptionForCCTextBox.Name = "SIMAMeasureDescriptionForCCTextBox";
			this.sIMAMeasureDescriptionForCCTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.sIMAMeasureDescriptionForCCTextBox.TabIndex = 1;
			// 
			// AttributesTabPage
			// 
			this.AttributesTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|16510d11-f7b4-4a94-a6e1-30b70e1f549a", "Applies to");
			this.AttributesTabPage.Controls.Add(this.attributesLeftSplitContainer);
			this.AttributesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AttributesTabPage.Name = "AttributesTabPage";
			this.AttributesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AttributesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 323, true);
			this.AttributesTabPage.TabIndex = 8;
			// 
			// AttributesLeftSplitContainer
			// 
			this.attributesLeftSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.attributesLeftSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.attributesLeftSplitContainer.Name = "AttributesLeftSplitContainer";
			// 
			// AttributesLeftSplitContainer.Panel1
			// 
			this.attributesLeftSplitContainer.Panel1.Controls.Add(this.attributes1GroupBox);
			this.attributesLeftSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			// 
			// AttributesLeftSplitContainer.Panel2
			// 
			this.attributesLeftSplitContainer.Panel2.Controls.Add(this.attributesRightSplitContainer);
			this.attributesLeftSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 183, true);
			this.attributesLeftSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(260);
			this.attributesLeftSplitContainer.TabIndex = 3;
			// 
			// Attributes1GroupBox
			// 
			this.attributes1GroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|a8243a24-ad67-46aa-a486-072886946490", "Attribute 1");
			this.attributes1GroupBox.Controls.Add(this.attributes1Grid);
			this.attributes1GroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.attributes1GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.attributes1GroupBox.Name = "Attributes1GroupBox";
			this.attributes1GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 317, true);
			this.attributes1GroupBox.TabIndex = 1;
			this.attributes1GroupBox.TabStop = false;
			// 
			// Attributes1Grid
			// 
			this.attributes1Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.attributes1Grid, "PivotsForBinding.Attributes1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).Attributes1);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Customs.Business.CusAttributeFilter)(((System.Collections.IList)((((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).Attributes1))).SyncRoot)).BG_AttributeValue1);
			this.attributes1Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "BG_AttributeValue1";
			zTextBoxColumnStyleInfo5.IsMandatory = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.attributes1Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.attributes1Grid.CopySelectedRowsAllowed = true;
			this.attributes1Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.attributes1Grid.GridId = "0b5e80de-100c-4750-8a3b-5eab98711af2";
			this.attributes1Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.attributes1Grid.LayoutKey = "zGrid1";
			this.attributes1Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.attributes1Grid.Name = "Attributes1Grid";
			this.attributes1Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 298, true);
			this.attributes1Grid.TabIndex = 4;
			// 
			// AttributesRightSplitContainer
			// 
			this.attributesRightSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.attributesRightSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.attributesRightSplitContainer.Name = "AttributesRightSplitContainer";
			// 
			// AttributesRightSplitContainer.Panel1
			// 
			this.attributesRightSplitContainer.Panel1.Controls.Add(this.attributes2GroupBox);
			this.attributesRightSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			// 
			// AttributesRightSplitContainer.Panel2
			// 
			this.attributesRightSplitContainer.Panel2.Controls.Add(this.attributes3GroupBox);
			this.attributesRightSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(548, 183, true);
			this.attributesRightSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(257);
			this.attributesRightSplitContainer.TabIndex = 0;
			// 
			// Attributes2GroupBox
			// 
			this.attributes2GroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|79545c4f-eeb8-439e-b72a-c2258031d47c", "Attribute 2");
			this.attributes2GroupBox.Controls.Add(this.attributes2Grid);
			this.attributes2GroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.attributes2GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.attributes2GroupBox.Name = "Attributes2GroupBox";
			this.attributes2GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 317, true);
			this.attributes2GroupBox.TabIndex = 2;
			this.attributes2GroupBox.TabStop = false;
			// 
			// Attributes2Grid
			// 
			this.attributes2Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.attributes2Grid, "PivotsForBinding.Attributes2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).Attributes2);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Customs.Business.CusAttributeFilter)(((System.Collections.IList)((((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).Attributes2))).SyncRoot)).BG_AttributeValue1);
			this.attributes2Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "BG_AttributeValue1";
			zTextBoxColumnStyleInfo6.IsMandatory = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.attributes2Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.attributes2Grid.CopySelectedRowsAllowed = true;
			this.attributes2Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.attributes2Grid.GridId = "0e811a3c-495e-4e44-940c-767033f21fd0";
			this.attributes2Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.attributes2Grid.LayoutKey = "zGrid1";
			this.attributes2Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.attributes2Grid.Name = "Attributes2Grid";
			this.attributes2Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 298, true);
			this.attributes2Grid.TabIndex = 4;
			// 
			// Attributes3GroupBox
			// 
			this.attributes3GroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|151249a5-57db-48ca-bb97-2793bc487ae0", "Attribute 3");
			this.attributes3GroupBox.Controls.Add(this.attributes3Grid);
			this.attributes3GroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.attributes3GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.attributes3GroupBox.Name = "Attributes3GroupBox";
			this.attributes3GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 317, true);
			this.attributes3GroupBox.TabIndex = 3;
			this.attributes3GroupBox.TabStop = false;
			// 
			// Attributes3Grid
			// 
			this.attributes3Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.attributes3Grid, "PivotsForBinding.Attributes3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).Attributes3);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Customs.Business.CusAttributeFilter)(((System.Collections.IList)((((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).Attributes3))).SyncRoot)).BG_AttributeValue1);
			this.attributes3Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "BG_AttributeValue1";
			zTextBoxColumnStyleInfo7.IsMandatory = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.attributes3Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.attributes3Grid.CopySelectedRowsAllowed = true;
			this.attributes3Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.attributes3Grid.GridId = "5eabfd61-ee80-48bb-bf10-952b4661be8c";
			this.attributes3Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.attributes3Grid.LayoutKey = "zGrid1";
			this.attributes3Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.attributes3Grid.Name = "Attributes3Grid";
			this.attributes3Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(722, 298, true);
			this.attributes3Grid.TabIndex = 4;
			// 
			// PGATabPage
			// 
			this.pGATabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|38a46758-177b-43c3-8f69-dbfa8fb08eb6", "PGA Requirements");
			this.pGATabPage.Controls.Add(this.pGARequirementsControl);
			this.pGATabPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pGATabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.pGATabPage.Name = "PGATabPage";
			this.pGATabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.pGATabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1115, 323, true);
			this.pGATabPage.TabIndex = 11;
			// 
			// PGARequirementsControl
			// 
			this.pGARequirementsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.pGARequirementsControl, "PivotsForBinding.PGARequirements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).PGARequirements);
			this.pGARequirementsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pGARequirementsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.pGARequirementsControl.Name = "PGARequirementsControl";
			this.pGARequirementsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1109, 317, true);
			this.pGARequirementsControl.TabIndex = 0;
			// 
			// exportPanel
			// 
			this.exportPanel.Controls.Add(this.exportClassDetailsGroupBox);
			this.exportPanel.Controls.Add(this.exportProductOverridesGroupBox);
			this.exportPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.exportPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.exportPanel.Name = "exportPanel";
			this.exportPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1123, 243, true);
			this.exportPanel.TabIndex = 8;
			// 
			// ExportClassDetailsGroupBox
			// 
			this.exportClassDetailsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|d6861682-1a60-4be3-9ec1-c212d9f0d680", "Classification Lookup Details");
			this.exportClassDetailsGroupBox.Controls.Add(this.cI_CC_FormattedTariffEXPCodeFindBox);
			this.exportClassDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.exportClassDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.exportClassDetailsGroupBox.Name = "ExportClassDetailsGroupBox";
			this.exportClassDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1123, 69, true);
			this.exportClassDetailsGroupBox.TabIndex = 3;
			this.exportClassDetailsGroupBox.TabStop = false;
			// 
			// CI_CC_FormattedTariffEXPCodeFindBox
			// 
			this.cI_CC_FormattedTariffEXPCodeFindBox.AllowDrop = true;
			this.cI_CC_FormattedTariffEXPCodeFindBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.cI_CC_FormattedTariffEXPCodeFindBox, "PivotsForBinding.CI_CC_FormattedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_CC_FormattedTariff);
			this.cI_CC_FormattedTariffEXPCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|6da94942-0474-418e-99ee-8fad03d48ca1", "Lookup Tariff", "Lookup Class. Tariff", "Lookup Classification Tariff Code", "Customs Classification Tariff Code set on Classification lookup.");
			this.cI_CC_FormattedTariffEXPCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 29, true);
			this.cI_CC_FormattedTariffEXPCodeFindBox.Name = "CI_CC_FormattedTariffEXPCodeFindBox";
			this.cI_CC_FormattedTariffEXPCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 20, true);
			this.cI_CC_FormattedTariffEXPCodeFindBox.TabIndex = 2;
			// 
			// ExportProductOverridesGroupBox
			// 
			this.exportProductOverridesGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|865cf592-f5d2-4d69-b88c-647b19777c47", "Product Overrides");
			this.exportProductOverridesGroupBox.Controls.Add(this.exportTariffCodeFindBox);
			this.exportProductOverridesGroupBox.Controls.Add(this.cCA_ProvinceOfOriginDropEdit);
			this.exportProductOverridesGroupBox.Controls.Add(this.cCA_RN_NKOriginCodeFindBox);
			this.exportProductOverridesGroupBox.Controls.Add(this.exportClassificationDescriptionTextBox);
			this.exportProductOverridesGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.exportProductOverridesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 90, true);
			this.exportProductOverridesGroupBox.Name = "ExportProductOverridesGroupBox";
			this.exportProductOverridesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1123, 153, true);
			this.exportProductOverridesGroupBox.TabIndex = 4;
			this.exportProductOverridesGroupBox.TabStop = false;
			// 
			// ExportTariffCodeFindBox
			// 
			this.exportTariffCodeFindBox.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
			this.exportTariffCodeFindBox.AllowDrop = true;
			this.exportTariffCodeFindBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.exportTariffCodeFindBox, "PivotsForBinding.CI_FormattedTariffNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_FormattedTariffNum);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_TariffNumTariffInfo);
			this.exportTariffCodeFindBox.BindToTariffPropertyInfo = "PivotsForBinding.CI_TariffNumTariffInfo";
			this.exportTariffCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|02aa95fb-6202-4221-81fe-f571be0ac311", "HS", "Class. Tariff #", "Classification Tariff Code", "Must be a 10-digit Canadian National Customs Tariff code.");
			this.exportTariffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 19, true);
			this.exportTariffCodeFindBox.Name = "ExportTariffCodeFindBox";
			this.exportTariffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 20, true);
			this.exportTariffCodeFindBox.TabIndex = 11;
			// 
			// CCA_ProvinceOfOriginDropEdit
			// 
			this.cCA_ProvinceOfOriginDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cCA_ProvinceOfOriginDropEdit, "PivotsForBinding.CCA_ProvinceOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_ProvinceOfOrigin);
			this.cCA_ProvinceOfOriginDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|df6982c1-7c12-449e-91fa-dc9f1b348310", "P/Origin", "Province Of Origin/Shipment", "Province of origin/Shipment", "The Region in which the goods have been produced or manufactured.  If the goods were originally imported into Canada and are being exported in the same condition, provide the province the goods were shipped from.");
			this.cCA_ProvinceOfOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 70, true);
			this.cCA_ProvinceOfOriginDropEdit.Name = "CCA_ProvinceOfOriginDropEdit";
			this.cCA_ProvinceOfOriginDropEdit.PreBoundMaxLength = 2;
			this.cCA_ProvinceOfOriginDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.cCA_ProvinceOfOriginDropEdit.TabIndex = 4;
			// 
			// CCA_RN_NKOriginCodeFindBox
			// 
			this.cCA_RN_NKOriginCodeFindBox.AllowDrop = true;
			this.cCA_RN_NKOriginCodeFindBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.cCA_RN_NKOriginCodeFindBox, "PivotsForBinding.CCA_RN_NKOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CCA_RN_NKOrigin);
			this.cCA_RN_NKOriginCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAOrgSupplierPartFormCustomsControl|8a7c557f-cffa-4fd4-b026-f53d65fcdff2", "Org.", "Origin", "Country/Region of Origin", "");
			this.cCA_RN_NKOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 45, true);
			this.cCA_RN_NKOriginCodeFindBox.Name = "CCA_RN_NKOriginCodeFindBox";
			this.cCA_RN_NKOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.cCA_RN_NKOriginCodeFindBox.TabIndex = 3;
			// 
			// exportClassificationDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.exportClassificationDescriptionTextBox, "PivotsForBinding.CI_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_Description);
			this.exportClassificationDescriptionTextBox.Name = "exportClassificationDescriptionTextBox";
			this.exportClassificationDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 95, true);
			this.exportClassificationDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 20, true);
			this.exportClassificationDescriptionTextBox.TabIndex = 4;
			// 
			// CAOrgSupplierPartPanel
			//
			this.cAOrgSupplierPartPanel.Controls.Add(this.topPanel);
			this.cAOrgSupplierPartPanel.Controls.Add(this.detailsPanel);
			this.cAOrgSupplierPartPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cAOrgSupplierPartPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.cAOrgSupplierPartPanel.Name = "CAOrgSupplierPartPanel";
			this.cAOrgSupplierPartPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1123, 500, true);
			this.cAOrgSupplierPartPanel.TabIndex = 9;
			// 
			// TopPanel
			//
			this.topPanel.Controls.Add(this.PivotGrid);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 69, true);
			this.topPanel.Name = "TopPanel";
			this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1123, 66, true);
			this.topPanel.TabIndex = 9;
			this.topPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1123, 66, true);
			this.topPanel.AutoScroll = true;
			// 
			// CAOrgSupplierPartFormCustomsControl
			// 
			this.Controls.Add(this.cAOrgSupplierPartPanel);
			this.Name = "CAOrgSupplierPartFormCustomsControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1123, 500, true);
			this.Controls.SetChildIndex(this.cAOrgSupplierPartPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.PivotGrid)).EndInit();
			this.PivotGrid.ResumeLayout(false);
			this.PivotGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.detailsPanel.ResumeLayout(false);
			this.detailsPanel.PerformLayout();
			this.importPanel.ResumeLayout(false);
			this.importPanel.PerformLayout();
			this.detailTabControl.ResumeLayout(false);
			this.detailTabControl.PerformLayout();
			this.detailsTabPage.ResumeLayout(false);
			this.detailsTabPage.PerformLayout();
			this.productOverridesGroupBox.ResumeLayout(false);
			this.productOverridesGroupBox.PerformLayout();
			this.manufacturerAddressControl.ResumeLayout(true);
			this.manufacturerAddressControl.PerformLayout();
			this.eTExemptionDropEdit.ResumeLayout(true);
			this.eTExemptionDropEdit.PerformLayout();
			this.eTRateCodeDropEdit.ResumeLayout(true);
			this.eTRateCodeDropEdit.PerformLayout();
			this.gSTStatusCodeDropEdit.ResumeLayout(true);
			this.gSTStatusCodeDropEdit.PerformLayout();
			this.classificationNumberFindBox.ResumeLayout(true);
			this.classificationNumberFindBox.PerformLayout();
			this.cCA_RN_NKOriginzCodeFindBox2.ResumeLayout(true);
			this.cCA_RN_NKOriginzCodeFindBox2.PerformLayout();
			this.cCA_ProvinceOfOriginDropEdit2.ResumeLayout(true);
			this.cCA_ProvinceOfOriginDropEdit2.PerformLayout();
			this.treatmentCodeDropEdit.ResumeLayout(true);
			this.treatmentCodeDropEdit.PerformLayout();
			this.tariff99CodeFindBox.ResumeLayout(true);
			this.tariff99CodeFindBox.PerformLayout();
			this.authorityNumberCodeFindBox.ResumeLayout(true);
			this.authorityNumberCodeFindBox.PerformLayout();
			this.valueForDutyCodeDropEdit.ResumeLayout(true);
			this.valueForDutyCodeDropEdit.PerformLayout();
			this.classificationDescriptionTextBox.ResumeLayout(true);
			this.classificationDescriptionTextBox.PerformLayout();
			this.classDetailsGroupBox.ResumeLayout(false);
			this.classDetailsGroupBox.PerformLayout();
			this.cI_CC_CA_99TariffCodeTextBox.ResumeLayout(true);
			this.cI_CC_CA_99TariffCodeTextBox.PerformLayout();
			this.cI_CC_FormattedTariffIMPCodeFindBox.ResumeLayout(true);
			this.cI_CC_FormattedTariffIMPCodeFindBox.PerformLayout();
			this.cFIATabPage.ResumeLayout(false);
			this.cFIATabPage.PerformLayout();
			this.cFIARegNumbersGroupBox.ResumeLayout(false);
			this.cFIARegNumbersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.cFIARegNumbersGrid)).EndInit();
			this.cFIARegNumbersGrid.ResumeLayout(false);
			this.cFIARegNumbersGrid.PerformLayout();
			this.cFIAUSStateOfOriginDropEdit.ResumeLayout(true);
			this.cFIAUSStateOfOriginDropEdit.PerformLayout();
			this.nKCFIAOriginCodeFindBox.ResumeLayout(true);
			this.nKCFIAOriginCodeFindBox.PerformLayout();
			this.miscIDCodeFindBox.ResumeLayout(true);
			this.miscIDCodeFindBox.PerformLayout();
			this.endUseCodeFindBox.ResumeLayout(true);
			this.endUseCodeFindBox.PerformLayout();
			this.destinationProvinceDropEdit.ResumeLayout(true);
			this.destinationProvinceDropEdit.PerformLayout();
			this.sITTTab.ResumeLayout(false);
			this.sITTTab.PerformLayout();
			this.numbersGroupBox.ResumeLayout(false);
			this.numbersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.sITTRegistrationNumbersGrid)).EndInit();
			this.sITTRegistrationNumbersGrid.ResumeLayout(false);
			this.sITTRegistrationNumbersGrid.PerformLayout();
			this.sITTImportReasonCodeDropEdit.ResumeLayout(true);
			this.sITTImportReasonCodeDropEdit.PerformLayout();
			this.nRCANTabPage.ResumeLayout(false);
			this.nRCANTabPage.PerformLayout();
			this.nRCANImportReasonCodeDropEdit.ResumeLayout(true);
			this.nRCANImportReasonCodeDropEdit.PerformLayout();
			this.tiresTabPage.ResumeLayout(false);
			this.tiresTabPage.PerformLayout();
			this.tiresImportReasonCodeDropEdit.ResumeLayout(true);
			this.tiresImportReasonCodeDropEdit.PerformLayout();
			this.sIMATabPage.ResumeLayout(false);
			this.sIMATabPage.PerformLayout();
			this.sIMAGroupBox.ResumeLayout(false);
			this.sIMAGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.sIMAFeeGrid)).EndInit();
			this.sIMAFeeGrid.ResumeLayout(false);
			this.sIMAFeeGrid.PerformLayout();
			this.sIMATopPanel.ResumeLayout(false);
			this.sIMATopPanel.PerformLayout();
			this.sIMAForCCTabPage.ResumeLayout(false);
			this.sIMAForCCTabPage.PerformLayout();
			this.sIMAForCCGroupBox.ResumeLayout(false);
			this.sIMAForCCGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.sIMAFeeForCCGrid)).EndInit();
			this.sIMAFeeForCCGrid.ResumeLayout(false);
			this.sIMAFeeForCCGrid.PerformLayout();
			this.sIMAForCCTopPanel.ResumeLayout(false);
			this.sIMAForCCTopPanel.PerformLayout();
			this.AttributesTabPage.ResumeLayout(false);
			this.AttributesTabPage.PerformLayout();
			this.attributesLeftSplitContainer.Panel1.ResumeLayout(false);
			this.attributesLeftSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.attributesLeftSplitContainer)).EndInit();
			this.attributesLeftSplitContainer.ResumeLayout(false);
			this.attributesLeftSplitContainer.PerformLayout();
			this.attributes1GroupBox.ResumeLayout(false);
			this.attributes1GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.attributes1Grid)).EndInit();
			this.attributes1Grid.ResumeLayout(false);
			this.attributes1Grid.PerformLayout();
			this.attributesRightSplitContainer.Panel1.ResumeLayout(false);
			this.attributesRightSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.attributesRightSplitContainer)).EndInit();
			this.attributesRightSplitContainer.ResumeLayout(false);
			this.attributesRightSplitContainer.PerformLayout();
			this.attributes2GroupBox.ResumeLayout(false);
			this.attributes2GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.attributes2Grid)).EndInit();
			this.attributes2Grid.ResumeLayout(false);
			this.attributes2Grid.PerformLayout();
			this.attributes3GroupBox.ResumeLayout(false);
			this.attributes3GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.attributes3Grid)).EndInit();
			this.attributes3Grid.ResumeLayout(false);
			this.attributes3Grid.PerformLayout();
			this.pGATabPage.ResumeLayout(false);
			this.pGATabPage.PerformLayout();
			this.pGARequirementsControl.ResumeLayout(true);
			this.pGARequirementsControl.PerformLayout();
			this.exportPanel.ResumeLayout(false);
			this.exportPanel.PerformLayout();
			this.exportClassDetailsGroupBox.ResumeLayout(false);
			this.exportClassDetailsGroupBox.PerformLayout();
			this.cI_CC_FormattedTariffEXPCodeFindBox.ResumeLayout(true);
			this.cI_CC_FormattedTariffEXPCodeFindBox.PerformLayout();
			this.exportProductOverridesGroupBox.ResumeLayout(false);
			this.exportProductOverridesGroupBox.PerformLayout();
			this.exportTariffCodeFindBox.ResumeLayout(true);
			this.exportTariffCodeFindBox.PerformLayout();
			this.cCA_ProvinceOfOriginDropEdit.ResumeLayout(true);
			this.cCA_ProvinceOfOriginDropEdit.PerformLayout();
			this.cCA_RN_NKOriginCodeFindBox.ResumeLayout(true);
			this.cCA_RN_NKOriginCodeFindBox.PerformLayout();
			this.exportClassificationDescriptionTextBox.ResumeLayout(true);
			this.exportClassificationDescriptionTextBox.PerformLayout();
			this.cAOrgSupplierPartPanel.ResumeLayout(false);
			this.cAOrgSupplierPartPanel.PerformLayout();
			this.aMMVGroupBox.ResumeLayout(false);
			this.aMMVGroupBox.PerformLayout();
			this.ammvUnitCalcFindBox.ResumeLayout(false);
			this.ammvUnitCalcFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal ZTabPage AttributesTabPage;
		ZPanel cAOrgSupplierPartPanel;
		ZPanel topPanel;
		ZPanel detailsPanel;
		ZPanel importPanel;
		ZPanel exportPanel;
		ZTabControl detailTabControl;
		ZTabPage detailsTabPage;
		ZTabPage cFIATabPage;
		ZGroupBox classDetailsGroupBox;
		ZCodeFindBox cI_CC_CA_99TariffCodeTextBox;
		ZTextBox cI_CC_CA_ValueForDutyCodeTextBox;
		ZCodeFindBox cI_CC_FormattedTariffIMPCodeFindBox;
		ZTextBox cI_CC_CA_TRSNumberTextBox;
		ZTextBox cI_CC_CA_AuthorityNumberTextBox;
		ZTextBox cI_CC_CA_GSTStatusCodeTextBox;
		ZTextBox cI_CC_CA_ETRateCodeTextBox;
		ZTextBox cI_CC_CA_ETExemptionTextBox;
		ZGroupBox productOverridesGroupBox;
		ZTextBox tRSNumberTextBox;
		ZCodeFindBox authorityNumberCodeFindBox;
		ZDropEdit valueForDutyCodeDropEdit;
		ZTextBox requirementIDTextBox;
		ZDropEdit destinationProvinceDropEdit;
		ZTextBox airsCodeTextBox;
		ZTextBox requirementVerTextBox;
		ZCodeFindBox nKCFIAOriginCodeFindBox;
		ZCodeFindBox miscIDCodeFindBox;
		ZCodeFindBox endUseCodeFindBox;
		ZDropEdit cFIAUSStateOfOriginDropEdit;
		ZCodeFindBox cCA_RN_NKOriginzCodeFindBox2;
		ZDropEdit cCA_ProvinceOfOriginDropEdit2;
		ZGroupBox cFIARegNumbersGroupBox;
		ZGrid cFIARegNumbersGrid;
		ZTabPage sITTTab;
		ZTextBox sITTBrandNameTextBox;
		ZTextBox sITTModelNumberTextBox;
		ZTextBox sITTModelTextBox;
		ZDropEdit sITTImportReasonCodeDropEdit;
		ZGroupBox numbersGroupBox;
		ZGrid sITTRegistrationNumbersGrid;
		ZTabPage nRCANTabPage;
		ZTextBox nRCANTypeSizeTextBox;
		ZTextBox nRCANBrandNameTextBox;
		ZTextBox nRCANModelNumberTextBox;
		ZTextBox nRCANModelTextBox;
		ZDropEdit nRCANImportReasonCodeDropEdit;
		ZTabPage tiresTabPage;
		ZTextBox tiresTypeSizeTextBox;
		ZTextBox tiresBrandNameTextBox;
		ZCheckBox compliantImportDateCheckBox;
		ZCheckBox compliantCompletionCheckBox;
		ZTextBox tIINTextBox;
		ZDropEdit tiresImportReasonCodeDropEdit;
		CargoWise.Windows.UI.KSplitContainer attributesLeftSplitContainer;
		ZGroupBox attributes1GroupBox;
		ZGrid attributes1Grid;
		CargoWise.Windows.UI.KSplitContainer attributesRightSplitContainer;
		ZGroupBox attributes2GroupBox;
		ZGrid attributes2Grid;
		ZGroupBox attributes3GroupBox;
		ZGrid attributes3Grid;
		CusClassPartPivot currentPivot;
		ZGroupBox exportClassDetailsGroupBox;
		ZCodeFindBox cI_CC_FormattedTariffEXPCodeFindBox;
		ZGroupBox exportProductOverridesGroupBox;
		ZDropEdit cCA_ProvinceOfOriginDropEdit;
		ZCodeFindBox cCA_RN_NKOriginCodeFindBox;
		ZTextBox cI_CC_CA_TreatmentCodeTextBox;
		ZDropEdit treatmentCodeDropEdit;
		ZDropEdit gSTStatusCodeDropEdit;
		ZDropEdit eTExemptionDropEdit;
		ZDropEdit eTRateCodeDropEdit;
		TariffFindBox classificationNumberFindBox;
		TariffFindBox tariff99CodeFindBox;
		TariffFindBox exportTariffCodeFindBox;
		ZTabPage sIMATabPage;
		ZTabPage sIMAForCCTabPage;
		ZAddressControl manufacturerAddressControl;
		ZTabPage pGATabPage;
		PGARequirementsControl pGARequirementsControl;
		ZPanel sIMATopPanel;
		ZPanel sIMAForCCTopPanel;
		ZTextBox sIMAMeasureDescriptionTextBox;
		ZTextBox sIMAMeasureDescriptionForCCTextBox;
		ZGroupBox sIMAGroupBox;
		ZGroupBox sIMAForCCGroupBox;
		LongTextControl classificationDescriptionTextBox;
		LongTextControl exportClassificationDescriptionTextBox;
		ZGrid sIMAFeeGrid;
		ZGrid sIMAFeeForCCGrid;
		ZGroupBox aMMVGroupBox;
		ZCalcFindBox ammvUnitCalcFindBox;
		ZCalcEdit ammvPercentageCalcEdit;
	}
}
