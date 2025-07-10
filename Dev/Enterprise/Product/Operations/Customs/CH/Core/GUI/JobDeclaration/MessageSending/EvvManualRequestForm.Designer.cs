using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.CH.GUI;

partial class EvvManualRequestForm
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
    private new void InitializeComponent()
    {
        this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
        this.ZCancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
        this.CheckBoxGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
        this.ReimbursementVatCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
        this.ReimbursementCustomsDutiesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
        this.VatCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
        this.CustomsDutiesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
        this.MrnTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.VersionTextBox = new Enterprise.ZArchitecture.ZTextBox();
        ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.CheckBoxGroupBox.SuspendLayout();
        this.SuspendLayout();
        // 
        // MainStatusBar
        // 
        this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 235, true);
        this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 26, true);
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.EvvRequestSendingObjectParent);
        // 
        // SendButton
        // 
        this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.SendButton.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("DDC9C0EF-C183-4614-A6AD-D071CD70AB7E", "Send");
        this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 206, true);
        this.SendButton.Name = "SendButton";
        this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
        this.SendButton.TabIndex = 7;
        this.SendButton.ToolTipCaption = null;
        this.SendButton.UseVisualStyleBackColor = true;
        this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
        // 
        // ZCancelButton
        // 
        this.ZCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.ZCancelButton.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("A187F391-B1E6-4F07-B095-AB64FA7F393B", "Cancel");
        this.ZCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.ZCancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(276, 206, true);
        this.ZCancelButton.Name = "ZCancelButton";
        this.ZCancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
        this.ZCancelButton.TabIndex = 8;
        this.ZCancelButton.ToolTipCaption = null;
        this.ZCancelButton.UseVisualStyleBackColor = true;
        // 
        // CheckBoxGroupBox
        // 
        this.CheckBoxGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.CheckBoxGroupBox.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("07028179-4DD1-4CF6-A84A-3215F8ED2E92", "Document Types");
        this.CheckBoxGroupBox.Controls.Add(this.ReimbursementVatCheckBox);
        this.CheckBoxGroupBox.Controls.Add(this.ReimbursementCustomsDutiesCheckBox);
        this.CheckBoxGroupBox.Controls.Add(this.VatCheckBox);
        this.CheckBoxGroupBox.Controls.Add(this.CustomsDutiesCheckBox);
        this.CheckBoxGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(51, 60, true);
        this.CheckBoxGroupBox.Name = "CheckBoxGroupBox";
        this.CheckBoxGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 140, true);
        this.CheckBoxGroupBox.TabIndex = 1;
        this.CheckBoxGroupBox.TabStop = false;
        // 
        // ReimbursementVatCheckBox
        // 
        this.BindingSource.SetBindingMember(this.ReimbursementVatCheckBox, "SendReimbursementVat");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.EvvRequestSendingObjectParent)(null)).SendReimbursementVat)));
        this.ReimbursementVatCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 106, true);
        this.ReimbursementVatCheckBox.Name = "ReimbursementVatCheckBox";
        this.ReimbursementVatCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 24, true);
        this.ReimbursementVatCheckBox.TabIndex = 6;
        this.ReimbursementVatCheckBox.CheckedChanged += new System.EventHandler(this.ReimbursementVatCheckBox_CheckedChanged);
        // 
        // ReimbursementCustomsDutiesCheckBox
        // 
        this.BindingSource.SetBindingMember(this.ReimbursementCustomsDutiesCheckBox, "SendReimbursementCustomsDuties");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.EvvRequestSendingObjectParent)(null)).SendReimbursementCustomsDuties)));
        this.ReimbursementCustomsDutiesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 76, true);
        this.ReimbursementCustomsDutiesCheckBox.Name = "ReimbursementCustomsDutiesCheckBox";
        this.ReimbursementCustomsDutiesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 24, true);
        this.ReimbursementCustomsDutiesCheckBox.TabIndex = 5;
        this.ReimbursementCustomsDutiesCheckBox.CheckedChanged += new System.EventHandler(this.ReimbursementCustomsDutiesCheckBox_CheckedChanged);
        // 
        // VatCheckBox
        // 
        this.BindingSource.SetBindingMember(this.VatCheckBox, "SendVat");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.EvvRequestSendingObjectParent)(null)).SendVat)));
        this.VatCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 46, true);
        this.VatCheckBox.Name = "VatCheckBox";
        this.VatCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 24, true);
        this.VatCheckBox.TabIndex = 4;
        this.VatCheckBox.CheckedChanged += new System.EventHandler(this.VatCheckBox_CheckedChanged);
        // 
        // CustomsDutiesCheckBox
        // 
        this.BindingSource.SetBindingMember(this.CustomsDutiesCheckBox, "SendCustomsDuties");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.EvvRequestSendingObjectParent)(null)).SendCustomsDuties)));
        this.CustomsDutiesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
        this.CustomsDutiesCheckBox.Name = "CustomsDutiesCheckBox";
        this.CustomsDutiesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 24, true);
        this.CustomsDutiesCheckBox.TabIndex = 3;
        this.CustomsDutiesCheckBox.CheckedChanged += new System.EventHandler(this.CustomsDutiesCheckBox_CheckedChanged);
        // 
        // MrnTextBox
        // 
        this.BindingSource.SetBindingMember(this.MrnTextBox, "Mrn");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.EvvRequestSendingObjectParent)(null)).Mrn)));
        this.MrnTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(51, 24, true);
        this.MrnTextBox.Name = "MrnTextBox";
        this.MrnTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
        this.MrnTextBox.TabIndex = 1;
        // 
        // VersionTextBox
        // 
        this.BindingSource.SetBindingMember(this.VersionTextBox, "MrnVersion");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.CH.Business.EvvRequestSendingObjectParent)(null)).MrnVersion)));
        this.VersionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 24, true);
        this.VersionTextBox.Name = "VersionTextBox";
        this.VersionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(29, 20, true);
        this.VersionTextBox.TabIndex = 2;
        this.VersionTextBox.Validated += new System.EventHandler(this.VersionTextBox_TextChanged);
        // 
        // EvvManualRequestForm
        // 
        this.CaptionRenderingEnabled = true;
        this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 261, true);
        this.Controls.Add(this.CheckBoxGroupBox);
        this.Controls.Add(this.VersionTextBox);
        this.Controls.Add(this.MrnTextBox);
        this.Controls.Add(this.ZCancelButton);
        this.Controls.Add(this.SendButton);
        this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Business";
        this.DataSourceType = typeof(Enterprise.Customs.CH.Business.EvvRequestSendingObjectParent);
        this.DataSourceTypeName = "Enterprise.ZArchitecture.Business.Design.DataSourceTypeRequiredInstructionsType";
        this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 340, true);
        this.MinimizeBox = false;
        this.Name = "EvvManualRequestForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Controls.SetChildIndex(this.SendButton, 0);
        this.Controls.SetChildIndex(this.ZCancelButton, 0);
        this.Controls.SetChildIndex(this.MrnTextBox, 0);
        this.Controls.SetChildIndex(this.VersionTextBox, 0);
        this.Controls.SetChildIndex(this.CheckBoxGroupBox, 0);
        this.Controls.SetChildIndex(this.MainStatusBar, 0);
        ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.CheckBoxGroupBox.ResumeLayout(false);
        this.CheckBoxGroupBox.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZTextBox MrnTextBox;
    internal ZTextBox VersionTextBox;
    internal ZCheckBox CustomsDutiesCheckBox;
    internal ZCheckBox VatCheckBox;
    internal ZCheckBox ReimbursementCustomsDutiesCheckBox;
    internal ZCheckBox ReimbursementVatCheckBox;

    internal ZGroupBox CheckBoxGroupBox;
    internal Enterprise.ZArchitecture.GUI.ZButton SendButton;
    internal Enterprise.ZArchitecture.GUI.ZButton ZCancelButton;
}
