namespace Enterprise.Customs.CH.GUI;

partial class ArrivalCustomerReferenceFormatConfigControl
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

    #region Component Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
        this.UseSystemDefinedFormatLabel = new Enterprise.ZArchitecture.ZLabel();
        this.UseSystemDefinedFormatGroupBox = new CargoWise.Windows.UI.KGroupBox();
        this.UseSystemDefinedFormatYesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
        this.UseSystemDefinedFormatNoRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
        this.CustomFormatsGrid = new Enterprise.ZArchitecture.ZGrid();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.UseSystemDefinedFormatGroupBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.CustomFormatsGrid)).BeginInit();
        this.CustomFormatsGrid.SuspendLayout();
        this.SuspendLayout();
        //
        // BindingSource
        //
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.ArrivalCustomerReferenceFormat);
        //
        // UseSystemDefinedFormatLabel
        //
        this.UseSystemDefinedFormatLabel.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("1dc247ff-fba0-4aed-9223-b85ef08f63db", "Use system-defined format:");
        this.UseSystemDefinedFormatLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 17, true);
        this.UseSystemDefinedFormatLabel.Name = "UseSystemDefinedFormatLabel";
        this.UseSystemDefinedFormatLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 15, true);
        this.UseSystemDefinedFormatLabel.TabIndex = 0;
        this.UseSystemDefinedFormatLabel.UseMnemonic = false;
        //
        // UseSystemDefinedFormatGroupBox
        //
        this.UseSystemDefinedFormatGroupBox.AutoSize = true;
        this.UseSystemDefinedFormatGroupBox.Controls.Add(this.UseSystemDefinedFormatLabel);
        this.UseSystemDefinedFormatGroupBox.Controls.Add(this.UseSystemDefinedFormatYesRadioButton);
        this.UseSystemDefinedFormatGroupBox.Controls.Add(this.UseSystemDefinedFormatNoRadioButton);
        this.UseSystemDefinedFormatGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.UseSystemDefinedFormatGroupBox.Name = "UseSystemDefinedFormatGroupBox";
        this.UseSystemDefinedFormatGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 53, true);
        this.UseSystemDefinedFormatGroupBox.TabIndex = 0;
        this.UseSystemDefinedFormatGroupBox.TabStop = false;
        //
        // UseSystemDefinedFormatYesRadioButton
        //
        this.UseSystemDefinedFormatYesRadioButton.AutoCheck = false;
        this.UseSystemDefinedFormatYesRadioButton.AutoSize = true;
        this.BindingSource.SetBindingMember(this.UseSystemDefinedFormatYesRadioButton, "UseSystemDefinedFormat");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.ArrivalCustomerReferenceFormat)(null)).UseSystemDefinedFormat)));
        this.UseSystemDefinedFormatYesRadioButton.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("80d6b68f-fae6-44c0-bb40-28a0b11e91eb", "Yes");
        this.UseSystemDefinedFormatYesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 16, true);
        this.UseSystemDefinedFormatYesRadioButton.Name = "UseSystemDefinedFormatYesRadioButton";
        this.UseSystemDefinedFormatYesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 17, true);
        this.UseSystemDefinedFormatYesRadioButton.TabIndex = 0;
        //
        // UseSystemDefinedFormatNoRadioButton
        //
        this.UseSystemDefinedFormatNoRadioButton.AutoCheck = false;
        this.UseSystemDefinedFormatNoRadioButton.AutoSize = true;
        this.BindingSource.SetBindingMember(this.UseSystemDefinedFormatNoRadioButton, "NotUseSystemDefinedFormat");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.ArrivalCustomerReferenceFormat)(null)).NotUseSystemDefinedFormat)));
        this.UseSystemDefinedFormatNoRadioButton.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("6211bde4-7647-44ac-a7ea-0dce78e19dbb", "No");
        this.UseSystemDefinedFormatNoRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 16, true);
        this.UseSystemDefinedFormatNoRadioButton.Name = "UseSystemDefinedFormatNoRadioButton";
        this.UseSystemDefinedFormatNoRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 17, true);
        this.UseSystemDefinedFormatNoRadioButton.TabIndex = 0;
        //
        // CustomFormatsGrid
        //
        this.CustomFormatsGrid.AllowNavigation = false;
        this.CustomFormatsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.BindingSource.SetBindingMember(this.CustomFormatsGrid, "CustomFormats");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.Business.ArrivalCustomerReferenceFormat)(null)).CustomFormats)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CustomArrivalCustomerReferenceFormat)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.ArrivalCustomerReferenceFormat)(null)).CustomFormats)).SyncRoot)).AuthorizationLocationCode)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CustomArrivalCustomerReferenceFormat)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.ArrivalCustomerReferenceFormat)(null)).CustomFormats)).SyncRoot)).Prefix)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CustomArrivalCustomerReferenceFormat)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.ArrivalCustomerReferenceFormat)(null)).CustomFormats)).SyncRoot)).Suffix)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CustomArrivalCustomerReferenceFormat)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.ArrivalCustomerReferenceFormat)(null)).CustomFormats)).SyncRoot)).YearOption)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.CustomArrivalCustomerReferenceFormat)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.ArrivalCustomerReferenceFormat)(null)).CustomFormats)).SyncRoot)).SequenceNumberLength)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.CustomArrivalCustomerReferenceFormat)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.ArrivalCustomerReferenceFormat)(null)).CustomFormats)).SyncRoot)).IsRemoveLeadingZeros)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.CustomArrivalCustomerReferenceFormat)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.ArrivalCustomerReferenceFormat)(null)).CustomFormats)).SyncRoot)).IsRestartOnNewYear)));
        this.CustomFormatsGrid.CaptionVisible = false;
        zDropEditColumnStyleInfo1.ColumnName = "AuthorizationLocationCode";
        zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
        zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
        zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(118);
        zTextBoxColumnStyleInfo1.ColumnName = "Prefix";
        zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
        zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47);
        zTextBoxColumnStyleInfo2.ColumnName = "Suffix";
        zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
        zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47);
        zDropEditColumnStyleInfo2.ColumnName = "YearOption";
        zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
        zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(69);
        zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo1.ColumnName = "SequenceNumberLength";
        zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
        zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(143);
        zCheckBoxColumnStyleInfo1.ColumnName = "IsRemoveLeadingZeros";
        zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
        zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
        zCheckBoxColumnStyleInfo2.ColumnName = "IsRestartOnNewYear";
        zCheckBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
        zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
        this.CustomFormatsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
        this.CustomFormatsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
        this.CustomFormatsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
        this.CustomFormatsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
        this.CustomFormatsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
        this.CustomFormatsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
        this.CustomFormatsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
        this.CustomFormatsGrid.GridId = "9762494b-d5ca-45b2-bf4f-8fb1ed6a553e";
        this.CustomFormatsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.CustomFormatsGrid.LayoutKey = "CustomFormatsGrid";
        this.CustomFormatsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 57, true);
        this.CustomFormatsGrid.Name = "CustomFormatsGrid";
        this.CustomFormatsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 90, true);
        this.CustomFormatsGrid.TabIndex = 1;
        //
        // ArrivalCustomerReferenceFormatConfigControl
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.UseSystemDefinedFormatGroupBox);
        this.Controls.Add(this.CustomFormatsGrid);
        this.Name = "ArrivalCustomerReferenceFormatConfigControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(668, 155, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.UseSystemDefinedFormatGroupBox.ResumeLayout(false);
        this.UseSystemDefinedFormatGroupBox.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.CustomFormatsGrid)).EndInit();
        this.CustomFormatsGrid.ResumeLayout(false);
        this.CustomFormatsGrid.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal CargoWise.Windows.UI.KGroupBox UseSystemDefinedFormatGroupBox;
    internal ZArchitecture.ZLabel UseSystemDefinedFormatLabel;
    internal ZArchitecture.GUI.ZRadioButton UseSystemDefinedFormatYesRadioButton;
    internal ZArchitecture.GUI.ZRadioButton UseSystemDefinedFormatNoRadioButton;
    internal ZArchitecture.ZGrid CustomFormatsGrid;
}
