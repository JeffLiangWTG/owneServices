namespace Enterprise.Customs.CH.GUI;

partial class PermitsUserControl
{
    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
        Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
        this.PermitsGrid = new Enterprise.ZArchitecture.ZGrid();
        this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
        this.PermitGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
        this.PermitItemDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
        this.PermitItemDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
        this.IssuerTypeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
        this.DateOfIssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
        this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.PermitCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
        this.ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.Splitter = new CargoWise.Windows.UI.KSplitter();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.TopPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.PermitsGrid)).BeginInit();
        this.PermitsGrid.SuspendLayout();
        this.BottomPanel.SuspendLayout();
        this.PermitGroupBox.SuspendLayout();
        this.PermitItemDetailsGroupBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.PermitItemDetailsGrid)).BeginInit();
        this.PermitItemDetailsGrid.SuspendLayout();
        this.IssuerTypeFindBox.SuspendLayout();
        this.DateOfIssueDateEdit.SuspendLayout();
        this.PermitCodeFindBox.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.Permit);
        // 
        // TopPanel
        // 
        this.TopPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.TopPanel.Controls.Add(this.PermitsGrid);
        this.TopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.TopPanel.Name = "TopPanel";
        this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 153, true);
        this.TopPanel.TabIndex = 0;
        // 
        // PermitsGrid
        // 
        this.PermitsGrid.AllowNavigation = false;
        this.BindingSource.SetBindingMember(this.PermitsGrid, ".");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.Business.Permit)(null)))));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.Permit)(null)).CSI_Code)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.Permit)(null)).CSI_ReferenceNumber)));
        this.PermitsGrid.CaptionVisible = false;
        zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_Code";
        zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
        zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
        zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
        this.PermitsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
        this.PermitsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
        this.PermitsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PermitsGrid.GridId = "82ef5be5-70ef-487d-bcf7-d1af6bd48be2";
        this.PermitsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.PermitsGrid.LayoutKey = "PermitsGrid";
        this.PermitsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.PermitsGrid.Name = "PermitsGrid";
        this.PermitsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 153, true);
        this.PermitsGrid.TabIndex = 0;
        // 
        // BottomPanel
        // 
        this.BottomPanel.Controls.Add(this.PermitGroupBox);
        this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 155, true);
        this.BottomPanel.Name = "BottomPanel";
        this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 252, true);
        this.BottomPanel.TabIndex = 1;
        // 
        // PermitGroupBox
        // 
        this.PermitGroupBox.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("535054e2-bdcb-4f9b-9223-8bcfe5948999", "Permits");
        this.PermitGroupBox.Controls.Add(this.PermitItemDetailsGroupBox);
        this.PermitGroupBox.Controls.Add(this.IssuerTypeFindBox);
        this.PermitGroupBox.Controls.Add(this.DateOfIssueDateEdit);
        this.PermitGroupBox.Controls.Add(this.DescriptionTextBox);
        this.PermitGroupBox.Controls.Add(this.PermitCodeFindBox);
        this.PermitGroupBox.Controls.Add(this.ReferenceNumberTextBox);
        this.PermitGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PermitGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.PermitGroupBox.Name = "PermitGroupBox";
        this.PermitGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 252, true);
        this.PermitGroupBox.TabIndex = 0;
        this.PermitGroupBox.TabStop = false;
        // 
        // PermitItemDetailsGroupBox
        // 
        this.PermitItemDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.PermitItemDetailsGroupBox.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("e9ff8af8-ee02-4c67-bfca-3037764c21ff", "Permit Item Details (for e-Permits)");
        this.PermitItemDetailsGroupBox.Controls.Add(this.PermitItemDetailsGrid);
        this.PermitItemDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 140, true);
        this.PermitItemDetailsGroupBox.Name = "PermitItemDetailsGroupBox";
        this.PermitItemDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(673, 107, true);
        this.PermitItemDetailsGroupBox.TabIndex = 6;
        this.PermitItemDetailsGroupBox.TabStop = false;
        // 
        // PermitItemDetailsGrid
        // 
        this.PermitItemDetailsGrid.AllowNavigation = false;
        this.BindingSource.SetBindingMember(this.PermitItemDetailsGrid, "PermitItemDetails");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.Business.Permit)(null)).PermitItemDetails)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.PermitItemDetail)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.Permit)(null)).PermitItemDetails)).SyncRoot)).CY_Code)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.PermitItemDetail)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.Permit)(null)).PermitItemDetails)).SyncRoot)).CY_DataDecimalPlaces)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.PermitItemDetail)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.Permit)(null)).PermitItemDetails)).SyncRoot)).CY_Data)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.PermitItemDetail)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.Permit)(null)).PermitItemDetails)).SyncRoot)).CY_DataFieldType)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.PermitItemDetail)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.Permit)(null)).PermitItemDetails)).SyncRoot)).DataDescription)));
        this.PermitItemDetailsGrid.CaptionVisible = false;
        zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
        zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zMultiControlColumnStyleInfo1.BindToDecimalPlaces = "CY_DataDecimalPlaces";
        zMultiControlColumnStyleInfo1.ColumnName = "CY_Data";
        zMultiControlColumnStyleInfo1.FieldTypeColumnName = "CY_DataFieldType";
        zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zTextBoxColumnStyleInfo2.ColumnName = "DataDescription";
        zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        this.PermitItemDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
        this.PermitItemDetailsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
        this.PermitItemDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
        this.PermitItemDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PermitItemDetailsGrid.GridId = "786c773e-a704-4e9b-b372-152174ce84c0";
        this.PermitItemDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.PermitItemDetailsGrid.LayoutKey = "zGrid1";
        this.PermitItemDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
        this.PermitItemDetailsGrid.Name = "PermitItemDetailsGrid";
        this.PermitItemDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 88, true);
        this.PermitItemDetailsGrid.TabIndex = 5;
        // 
        // IssuerTypeFindBox
        // 
        this.IssuerTypeFindBox.AllowDrop = true;
        this.IssuerTypeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.BindingSource.SetBindingMember(this.IssuerTypeFindBox, "CSI_IssuerType");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.Permit)(null)).CSI_IssuerType)));
        this.IssuerTypeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 40, true);
        this.IssuerTypeFindBox.Name = "IssuerTypeFindBox";
        this.IssuerTypeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
        this.IssuerTypeFindBox.ParentType = null;
        this.IssuerTypeFindBox.PreBoundMaxLength = 2;
        this.IssuerTypeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 20, true);
        this.IssuerTypeFindBox.TabIndex = 1;
        // 
        // DateOfIssueDateEdit
        // 
        this.DateOfIssueDateEdit.AllowDrop = true;
        this.DateOfIssueDateEdit.AutoCompleteMonthThreshold = 1;
        this.DateOfIssueDateEdit.AutoCompleteYear = true;
        this.BindingSource.SetBindingMember(this.DateOfIssueDateEdit, "CSI_DateOfIssue");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.Permit)(null)).CSI_DateOfIssue)));
        this.DateOfIssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 85, true);
        this.DateOfIssueDateEdit.Name = "DateOfIssueDateEdit";
        this.DateOfIssueDateEdit.TabIndex = 3;
        // 
        // DescriptionTextBox
        // 
        this.DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.BindingSource.SetBindingMember(this.DescriptionTextBox, "CSI_Description");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.Permit)(null)).CSI_Description)));
        this.DescriptionTextBox.CaptionResourceString = null;
        this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 107, true);
        this.DescriptionTextBox.Name = "DescriptionTextBox";
        this.DescriptionTextBox.ShouldEscapeAllSpecialCharacters = false;
        this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 20, true);
        this.DescriptionTextBox.TabIndex = 4;
        // 
        // PermitCodeFindBox
        // 
        this.PermitCodeFindBox.AllowDrop = true;
        this.PermitCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.BindingSource.SetBindingMember(this.PermitCodeFindBox, "CSI_Code");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.Permit)(null)).CSI_Code)));
        this.PermitCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 18, true);
        this.PermitCodeFindBox.Name = "PermitCodeFindBox";
        this.PermitCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
        this.PermitCodeFindBox.ParentType = null;
        this.PermitCodeFindBox.PreBoundMaxLength = 2;
        this.PermitCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 20, true);
        this.PermitCodeFindBox.TabIndex = 0;
        // 
        // ReferenceNumberTextBox
        // 
        this.ReferenceNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ReferenceNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
        this.BindingSource.SetBindingMember(this.ReferenceNumberTextBox, "CSI_ReferenceNumber");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.Permit)(null)).CSI_ReferenceNumber)));
        this.ReferenceNumberTextBox.CaptionResourceString = null;
        this.ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 62, true);
        this.ReferenceNumberTextBox.Name = "ReferenceNumberTextBox";
        this.ReferenceNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
        this.ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 20, true);
        this.ReferenceNumberTextBox.TabIndex = 2;
        // 
        // Splitter
        // 
        this.Splitter.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.Splitter.DoNotSaveSplitterLayout = false;
        this.Splitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 153, true);
        this.Splitter.Name = "Splitter";
        this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 2, true);
        this.Splitter.TabIndex = 1;
        this.Splitter.TabStop = false;
        // 
        // PermitsUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.TopPanel);
        this.Controls.Add(this.Splitter);
        this.Controls.Add(this.BottomPanel);
        this.Name = "PermitsUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 407, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.TopPanel.ResumeLayout(false);
        this.TopPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.PermitsGrid)).EndInit();
        this.PermitsGrid.ResumeLayout(false);
        this.PermitsGrid.PerformLayout();
        this.BottomPanel.ResumeLayout(false);
        this.BottomPanel.PerformLayout();
        this.PermitGroupBox.ResumeLayout(false);
        this.PermitGroupBox.PerformLayout();
        this.PermitItemDetailsGroupBox.ResumeLayout(false);
        this.PermitItemDetailsGroupBox.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.PermitItemDetailsGrid)).EndInit();
        this.PermitItemDetailsGrid.ResumeLayout(false);
        this.PermitItemDetailsGrid.PerformLayout();
        this.IssuerTypeFindBox.ResumeLayout(true);
        this.IssuerTypeFindBox.PerformLayout();
        this.DateOfIssueDateEdit.ResumeLayout(true);
        this.DateOfIssueDateEdit.PerformLayout();
        this.PermitCodeFindBox.ResumeLayout(true);
        this.PermitCodeFindBox.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal Enterprise.ZArchitecture.GUI.ZGroupBox PermitGroupBox;
    internal Enterprise.ZArchitecture.ZTextBox ReferenceNumberTextBox;
    internal Enterprise.ZArchitecture.ZGrid PermitsGrid;
    internal Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
    internal Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
    internal ZArchitecture.GUI.ZCodeFindBox PermitCodeFindBox;
    internal ZArchitecture.ZTextBox DescriptionTextBox;
    internal ZArchitecture.GUI.ZDateEdit DateOfIssueDateEdit;
    internal ZArchitecture.GUI.ZCodeFindBox IssuerTypeFindBox;
    internal ZArchitecture.GUI.ZGroupBox PermitItemDetailsGroupBox;
    internal ZArchitecture.ZGrid PermitItemDetailsGrid;
    internal CargoWise.Windows.UI.KSplitter Splitter;
}
