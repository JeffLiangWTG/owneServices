using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

partial class RestrictionsUserControl
{
    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
        this.ItemNumberIntEdit = new Enterprise.ZArchitecture.GUI.ZIntEdit();
        this.RestrictionsGrid = new Enterprise.ZArchitecture.ZGrid();
        this.RestrictionAdditionalInformationGrid = new Enterprise.ZArchitecture.ZGrid();
        this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
        this.RestrictionDetailTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
        this.ItemDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
        this.RestrictionsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
        this.OverriddenIdentificationTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.PermitOwnerIdentificationTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.PermitOwnerOverrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
        this.CodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.PermitNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.PermitExceptionReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.PermitOwnerDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
        this.AdditionalInformationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.RestrictionsGrid)).BeginInit();
        this.RestrictionsGrid.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.RestrictionAdditionalInformationGrid)).BeginInit();
        this.RestrictionAdditionalInformationGrid.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
        this.SplitContainer.Panel1.SuspendLayout();
        this.SplitContainer.Panel2.SuspendLayout();
        this.SplitContainer.SuspendLayout();
        this.RestrictionDetailTabControl.SuspendLayout();
        this.ItemDetailsTabPage.SuspendLayout();
        this.RestrictionsPanel.SuspendLayout();
        this.CodeDropEdit.SuspendLayout();
        this.PermitExceptionReasonDropEdit.SuspendLayout();
        this.PermitOwnerDocAddressControl.SuspendLayout();
        this.AdditionalInformationTabPage.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.Restriction);
        // 
        // ItemNumberIntEdit
        // 
        this.BindingSource.SetBindingMember(this.ItemNumberIntEdit, "CSI_LineNo");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.CH.Business.Restriction)(null)).CSI_LineNo)));
        this.ItemNumberIntEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 8, true);
        this.ItemNumberIntEdit.Name = "ItemNumberIntEdit";
        this.ItemNumberIntEdit.ReadOnly = true;
        this.ItemNumberIntEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
        this.ItemNumberIntEdit.TabIndex = 0;
        // 
        // RestrictionsGrid
        // 
        this.RestrictionsGrid.AllowNavigation = false;
        this.BindingSource.SetBindingMember(this.RestrictionsGrid, ".");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.Business.Restriction)(null)))));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.Restriction)(null)).CSI_LineNo)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.Restriction)(null)).CSI_Code)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.Restriction)(null)).CSI_ReferenceNumber)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.Restriction)(null)).CSI_Description)));
        this.RestrictionsGrid.CaptionVisible = false;
        zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo1.ColumnName = "CSI_LineNo";
        zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
        zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zDropEditColumnStyleInfo1.ColumnName = "CSI_Code";
        zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
        zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
        zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
        zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
        zDropEditColumnStyleInfo2.ColumnName = "CSI_Description";
        zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
        zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        this.RestrictionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
        this.RestrictionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
        this.RestrictionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
        this.RestrictionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
        this.RestrictionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.RestrictionsGrid.GridId = "81a104a2-3c99-4e5c-bbec-7519acf0ad28";
        this.RestrictionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.RestrictionsGrid.LayoutKey = "RestrictionsGrid";
        this.RestrictionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.RestrictionsGrid.Name = "RestrictionsGrid";
        this.RestrictionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 180, true);
        this.RestrictionsGrid.TabIndex = 0;
        // 
        // RestrictionAdditionalInformationGrid
        // 
        this.RestrictionAdditionalInformationGrid.AllowNavigation = false;
        this.BindingSource.SetBindingMember(this.RestrictionAdditionalInformationGrid, "AdditionalInformations");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.Business.Restriction)(null)).AdditionalInformations)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.RestrictionAdditionalInformation)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.Restriction)(null)).AdditionalInformations)).SyncRoot)).CY_Order)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.RestrictionAdditionalInformation)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.Restriction)(null)).AdditionalInformations)).SyncRoot)).CY_Code)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.RestrictionAdditionalInformation)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.Restriction)(null)).AdditionalInformations)).SyncRoot)).CY_CodeDescription)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.RestrictionAdditionalInformation)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.Restriction)(null)).AdditionalInformations)).SyncRoot)).CY_Data)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.RestrictionAdditionalInformation)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.Restriction)(null)).AdditionalInformations)).SyncRoot)).CY_DataFieldType)));
        this.RestrictionAdditionalInformationGrid.CaptionVisible = false;
        zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo2.ColumnName = "CY_Order";
        zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
        zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zDropEditColumnStyleInfo3.ColumnName = "CY_Code";
        zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
        zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zTextBoxColumnStyleInfo2.ColumnName = "CY_CodeDescription";
        zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
        zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
        zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
        zMultiControlColumnStyleInfo1.ColumnName = "CY_Data";
        zMultiControlColumnStyleInfo1.DefaultCollectionIndex = 0;
        zMultiControlColumnStyleInfo1.FieldTypeColumnName = "CY_DataFieldType";
        zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
        this.RestrictionAdditionalInformationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
        this.RestrictionAdditionalInformationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
        this.RestrictionAdditionalInformationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
        this.RestrictionAdditionalInformationGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
        this.RestrictionAdditionalInformationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.RestrictionAdditionalInformationGrid.GridId = "300AA756-6193-47F2-9FB6-A7B83DE61B3E";
        this.RestrictionAdditionalInformationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.RestrictionAdditionalInformationGrid.LayoutKey = "RestrictionAdditionalInformationGrid";
        this.RestrictionAdditionalInformationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
        this.RestrictionAdditionalInformationGrid.Name = "RestrictionAdditionalInformationGrid";
        this.RestrictionAdditionalInformationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 198, true);
        this.RestrictionAdditionalInformationGrid.TabIndex = 0;
        // 
        // SplitContainer
        // 
        this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
        this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.SplitContainer.Name = "SplitContainer";
        this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
        // 
        // SplitContainer.Panel1
        // 
        this.SplitContainer.Panel1.Controls.Add(this.RestrictionsGrid);
        this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 449, true);
        this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(80);
        // 
        // SplitContainer.Panel2
        // 
        this.SplitContainer.Panel2.AutoScroll = true;
        this.SplitContainer.Panel2.Controls.Add(this.RestrictionDetailTabControl);
        this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(180);
        this.SplitContainer.SplitterWidth = 7;
        this.SplitContainer.TabIndex = 0;
        // 
        // RestrictionDetailTabControl
        // 
        this.RestrictionDetailTabControl.Controls.Add(this.ItemDetailsTabPage);
        this.RestrictionDetailTabControl.Controls.Add(this.AdditionalInformationTabPage);
        this.RestrictionDetailTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.RestrictionDetailTabControl.Name = "RestrictionDetailTabControl";
        this.RestrictionDetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 262, true);
        this.RestrictionDetailTabControl.TabIndex = 0;
        // 
        // ItemDetailsTabPage
        // 
        this.ItemDetailsTabPage.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("13E3EDFC-2CC6-4CE9-B90E-699104661471", "Item Details");
        this.ItemDetailsTabPage.Controls.Add(this.RestrictionsPanel);
        this.ItemDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
        this.ItemDetailsTabPage.Name = "ItemDetailsTabPage";
        this.ItemDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
        this.ItemDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(906, 235, true);
        this.ItemDetailsTabPage.TabIndex = 0;
        this.ItemDetailsTabPage.UseVisualStyleBackColor = true;
        // 
        // RestrictionsPanel
        // 
        this.RestrictionsPanel.Controls.Add(this.OverriddenIdentificationTextBox);
        this.RestrictionsPanel.Controls.Add(this.PermitOwnerIdentificationTextBox);
        this.RestrictionsPanel.Controls.Add(this.PermitOwnerOverrideCheckBox);
        this.RestrictionsPanel.Controls.Add(this.ItemNumberIntEdit);
        this.RestrictionsPanel.Controls.Add(this.CodeDropEdit);
        this.RestrictionsPanel.Controls.Add(this.PermitNumberTextBox);
        this.RestrictionsPanel.Controls.Add(this.PermitExceptionReasonDropEdit);
        this.RestrictionsPanel.Controls.Add(this.PermitOwnerDocAddressControl);
        this.RestrictionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.RestrictionsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
        this.RestrictionsPanel.Name = "RestrictionsPanel";
        this.RestrictionsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 229, true);
        this.RestrictionsPanel.TabIndex = 0;
        // 
        // OverriddenIdentificationTextBox
        // 
        this.OverriddenIdentificationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.BindingSource.SetBindingMember(this.OverriddenIdentificationTextBox, "CSI_ReferenceNumber2");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.Restriction)(null)).CSI_ReferenceNumber2)));
        this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OverriddenIdentificationTextBox, false);
        this.OverriddenIdentificationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(768, 98, true);
        this.OverriddenIdentificationTextBox.Name = "OverriddenIdentificationTextBox";
        this.OverriddenIdentificationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 20, true);
        this.OverriddenIdentificationTextBox.TabIndex = 8;
        // 
        // PermitOwnerIdentificationTextBox
        // 
        this.BindingSource.SetBindingMember(this.PermitOwnerIdentificationTextBox, "PermitOwnerIdentification");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.Restriction)(null)).PermitOwnerIdentification)));
        this.PermitOwnerIdentificationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(540, 98, true);
        this.PermitOwnerIdentificationTextBox.Name = "PermitOwnerIdentificationTextBox";
        this.PermitOwnerIdentificationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
        this.PermitOwnerIdentificationTextBox.TabIndex = 6;
        // 
        // PermitOwnerOverrideCheckBox
        // 
        this.BindingSource.SetBindingMember(this.PermitOwnerOverrideCheckBox, "OverrideIdentification");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.Restriction)(null)).OverrideIdentification)));
        this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PermitOwnerOverrideCheckBox, false);
        this.PermitOwnerOverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(674, 99, true);
        this.PermitOwnerOverrideCheckBox.Name = "PermitOwnerOverrideCheckBox";
        this.PermitOwnerOverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
        this.PermitOwnerOverrideCheckBox.TabIndex = 7;
        this.PermitOwnerOverrideCheckBox.UseVisualStyleBackColor = true;
        // 
        // CodeDropEdit
        // 
        this.CodeDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.CodeDropEdit, "CSI_Code");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.Restriction)(null)).CSI_Code)));
        this.CodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 30, true);
        this.CodeDropEdit.Name = "CodeDropEdit";
        this.CodeDropEdit.PreBoundMaxLength = 3;
        this.CodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
        this.CodeDropEdit.TabIndex = 1;
        // 
        // PermitNumberTextBox
        // 
        this.BindingSource.SetBindingMember(this.PermitNumberTextBox, "CSI_ReferenceNumber");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.Restriction)(null)).CSI_ReferenceNumber)));
        this.PermitNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
        this.PermitNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 52, true);
        this.PermitNumberTextBox.Name = "PermitNumberTextBox";
        this.PermitNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
        this.PermitNumberTextBox.TabIndex = 2;
        // 
        // PermitExceptionReasonDropEdit
        // 
        this.PermitExceptionReasonDropEdit.AllowDrop = true;
        this.PermitExceptionReasonDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.BindingSource.SetBindingMember(this.PermitExceptionReasonDropEdit, "CSI_Description");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.Restriction)(null)).CSI_Description)));
        this.PermitExceptionReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 74, true);
        this.PermitExceptionReasonDropEdit.Name = "PermitExceptionReasonDropEdit";
        this.PermitExceptionReasonDropEdit.PreBoundMaxLength = 12;
        this.PermitExceptionReasonDropEdit.ShouldResizeByMaxLength = false;
        this.PermitExceptionReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 20, true);
        this.PermitExceptionReasonDropEdit.TabIndex = 3;
        // 
        // PermitOwnerDocAddressControl
        // 
        this.PermitOwnerDocAddressControl.AddressValidationProcessCmdKey = null;
        this.PermitOwnerDocAddressControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.PermitOwnerDocAddressControl, "PermitOwnerDocAddress");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.CH.Business.Restriction)(null)).PermitOwnerDocAddress)));
        this.PermitOwnerDocAddressControl.BindToOrganisations = "FilteredInvoiceLines.Restrictions.Lookups.PermitOwnerList";
        this.PermitOwnerDocAddressControl.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("7256a72c-9e22-45ad-8d2c-c9f3ba21cffe", "Permit Owner");
        this.PermitOwnerDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
        this.PermitOwnerDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 98, true);
        this.PermitOwnerDocAddressControl.Name = "PermitOwnerDocAddressControl";
        this.PermitOwnerDocAddressControl.ReadOnly = false;
        this.PermitOwnerDocAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
        this.PermitOwnerDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
        this.PermitOwnerDocAddressControl.TabIndex = 5;
        this.PermitOwnerDocAddressControl.ValidationJustForced = false;
        // 
        // AdditionalInformationTabPage
        // 
        this.AdditionalInformationTabPage.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("131E5519-12A2-472A-AD18-1F65FE7A61FC", "Additional Information");
        this.AdditionalInformationTabPage.Controls.Add(this.RestrictionAdditionalInformationGrid);
        this.AdditionalInformationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
        this.AdditionalInformationTabPage.Name = "AdditionalInformationTabPage";
        this.AdditionalInformationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
        this.AdditionalInformationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(906, 204, true);
        this.AdditionalInformationTabPage.TabIndex = 1;
        this.AdditionalInformationTabPage.UseVisualStyleBackColor = true;
        // 
        // RestrictionsUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.SplitContainer);
        this.Name = "RestrictionsUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 449, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.RestrictionsGrid)).EndInit();
        this.RestrictionsGrid.ResumeLayout(false);
        this.RestrictionsGrid.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.RestrictionAdditionalInformationGrid)).EndInit();
        this.RestrictionAdditionalInformationGrid.ResumeLayout(false);
        this.RestrictionAdditionalInformationGrid.PerformLayout();
        this.SplitContainer.Panel1.ResumeLayout(false);
        this.SplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
        this.SplitContainer.ResumeLayout(false);
        this.SplitContainer.PerformLayout();
        this.RestrictionDetailTabControl.ResumeLayout(false);
        this.RestrictionDetailTabControl.PerformLayout();
        this.ItemDetailsTabPage.ResumeLayout(false);
        this.ItemDetailsTabPage.PerformLayout();
        this.RestrictionsPanel.ResumeLayout(false);
        this.RestrictionsPanel.PerformLayout();
        this.CodeDropEdit.ResumeLayout(true);
        this.CodeDropEdit.PerformLayout();
        this.PermitExceptionReasonDropEdit.ResumeLayout(true);
        this.PermitExceptionReasonDropEdit.PerformLayout();
        this.PermitOwnerDocAddressControl.ResumeLayout(true);
        this.PermitOwnerDocAddressControl.PerformLayout();
        this.AdditionalInformationTabPage.ResumeLayout(false);
        this.AdditionalInformationTabPage.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal CargoWise.Windows.UI.KSplitContainer SplitContainer;
    internal ZTabControl RestrictionDetailTabControl;
    internal ZTabPage ItemDetailsTabPage;
    internal ZTabPage AdditionalInformationTabPage;
    internal Enterprise.ZArchitecture.ZGrid RestrictionsGrid;
    internal Enterprise.ZArchitecture.ZGrid RestrictionAdditionalInformationGrid;
    internal ZArchitecture.GUI.ZPanel RestrictionsPanel;
    internal ZArchitecture.GUI.ZIntEdit ItemNumberIntEdit;
    internal ZArchitecture.GUI.ZDropEdit CodeDropEdit;
    internal ZArchitecture.ZTextBox PermitNumberTextBox;
    internal ZArchitecture.GUI.ZDropEdit PermitExceptionReasonDropEdit;
    public ZDocAddressControl PermitOwnerDocAddressControl;
    internal ZCheckBox PermitOwnerOverrideCheckBox;
    internal ZTextBox PermitOwnerIdentificationTextBox;
    internal ZTextBox OverriddenIdentificationTextBox;
}
