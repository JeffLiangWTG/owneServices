using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

partial class NT141MessageSendingDetailsUserControl
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
        this.ActualDestinationCustomsOfficeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
        this.DoubleEntryMRNTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.TC11DeliveryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
        this.ActualConsigneeAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.ActualDestinationCustomsOfficeFindBox.SuspendLayout();
        this.TC11DeliveryDateEdit.SuspendLayout();
        this.ActualConsigneeAddressControl.SuspendLayout();
        this.SuspendLayout();
        //
        // BindingSource
        //
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.NCTS.Business.NctsHeaderDepartureMessageSendingObjectParent);
        //
        // ActualDestinationCustomsOfficeFindBox
        //
        this.ActualDestinationCustomsOfficeFindBox.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.ActualDestinationCustomsOfficeFindBox, "SendingObjectsCollection.ActualDestinationCustomsOffice");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.NctsHeaderDepartureMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeaderDepartureMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ActualDestinationCustomsOffice)));
        this.ActualDestinationCustomsOfficeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 31, true);
        this.ActualDestinationCustomsOfficeFindBox.Name = "ActualDestinationCustomsOfficeFindBox";
        this.ActualDestinationCustomsOfficeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
        this.ActualDestinationCustomsOfficeFindBox.ParentType = null;
        this.ActualDestinationCustomsOfficeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 20, true);
        this.ActualDestinationCustomsOfficeFindBox.TabIndex = 3;
        //
        // DoubleEntryMRNTextBox
        //
        this.BindingSource.SetBindingMember(this.DoubleEntryMRNTextBox, "SendingObjectsCollection.DoubleEntryMRN");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.NctsHeaderDepartureMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeaderDepartureMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).DoubleEntryMRN)));
        this.DoubleEntryMRNTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
        this.DoubleEntryMRNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 70, true);
        this.DoubleEntryMRNTextBox.Name = "DoubleEntryMRNTextBox";
        this.DoubleEntryMRNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
        this.DoubleEntryMRNTextBox.TabIndex = 4;
        //
        // TC11DeliveryDateEdit
        //
        this.TC11DeliveryDateEdit.AllowDrop = true;
        this.TC11DeliveryDateEdit.AutoCompleteMonthThreshold = 1;
        this.BindingSource.SetBindingMember(this.TC11DeliveryDateEdit, "SendingObjectsCollection.TC11DeliveryDate");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.NCTS.Business.NctsHeaderDepartureMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeaderDepartureMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).TC11DeliveryDate)));
        this.TC11DeliveryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 109, true);
        this.TC11DeliveryDateEdit.Name = "TC11DeliveryDateEdit";
        this.TC11DeliveryDateEdit.TabIndex = 5;
        //
        // ActualConsigneeAddressControl
        //
        this.ActualConsigneeAddressControl.AddressValidationProcessCmdKey = null;
        this.ActualConsigneeAddressControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.ActualConsigneeAddressControl, "SendingObjectsCollection.ActualConsignee");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.CH.NCTS.Business.NctsHeaderDepartureMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeaderDepartureMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ActualConsignee)));
        this.ActualConsigneeAddressControl.BindToOrganisations = "SendingObjectsCollection.Lookups.Consignees";
        this.ActualConsigneeAddressControl.CaptionResourceString = Enterprise.Customs.CH.NCTS.GUI.Res.GetData("B43015E1-08E7-49F1-AE18-F6697538686B", "Actual Consignee");
        this.ActualConsigneeAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.CompactWithOverride;
        this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ActualConsigneeAddressControl, false);
        this.ActualConsigneeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(425, 18, true);
        this.ActualConsigneeAddressControl.Name = "ActualConsigneeAddressControl";
        this.ActualConsigneeAddressControl.ReadOnly = false;
        this.ActualConsigneeAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
        this.ActualConsigneeAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 130, true);
        this.ActualConsigneeAddressControl.TabIndex = 6;
        this.ActualConsigneeAddressControl.ValidationJustForced = false;
        //
        // NT141MessageSendingDetailsUserControl
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.BackColor = System.Drawing.SystemColors.Control;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.ActualConsigneeAddressControl);
        this.Controls.Add(this.TC11DeliveryDateEdit);
        this.Controls.Add(this.DoubleEntryMRNTextBox);
        this.Controls.Add(this.ActualDestinationCustomsOfficeFindBox);
        this.Name = "NT141MessageSendingDetailsUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 211, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.ActualDestinationCustomsOfficeFindBox.ResumeLayout(true);
        this.ActualDestinationCustomsOfficeFindBox.PerformLayout();
        this.TC11DeliveryDateEdit.ResumeLayout(true);
        this.TC11DeliveryDateEdit.PerformLayout();
        this.ActualConsigneeAddressControl.ResumeLayout(true);
        this.ActualConsigneeAddressControl.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.ZTextBox DoubleEntryMRNTextBox;
    internal ZArchitecture.GUI.ZDateEdit TC11DeliveryDateEdit;
    internal ZCodeFindBox ActualDestinationCustomsOfficeFindBox;
    internal ZDocAddressControl ActualConsigneeAddressControl;
}
