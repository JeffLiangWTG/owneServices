using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

partial class NC123MessageSendingDetailsUserControl
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
        this.IdentificationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.ContactNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.PhoneNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.EmailAddressTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.ApprovedLocationOfGoodsUserControl = new Enterprise.Customs.EU.GUI.LocationOfGoodsUserControl();
        this.TransportDepartureDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
        this.CommunicationLanguageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.ApprovedLocationOfGoodsUserControl.SuspendLayout();
        this.CommunicationLanguageDropEdit.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.NCTS.Business.NctsHeaderDepartureMessageSendingObject);
        // 
        // IdentificationNumberTextBox
        // 
        this.BindingSource.SetBindingMember(this.IdentificationNumberTextBox, "IdentificationNumber");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.NctsHeaderDepartureMessageSendingObject)(null)).IdentificationNumber)));
        this.IdentificationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 11, true);
        this.IdentificationNumberTextBox.Name = "IdentificationNumberTextBox";
        this.IdentificationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 17, true);
        this.IdentificationNumberTextBox.TabIndex = 2;
        // 
        // ContactNameTextBox
        // 
        this.BindingSource.SetBindingMember(this.ContactNameTextBox, "ContactName");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.NctsHeaderDepartureMessageSendingObject)(null)).ContactName)));
        this.ContactNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 37, true);
        this.ContactNameTextBox.Name = "ContactNameTextBox";
        this.ContactNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 17, true);
        this.ContactNameTextBox.TabIndex = 3;
        // 
        // PhoneNumberTextBox
        // 
        this.BindingSource.SetBindingMember(this.PhoneNumberTextBox, "PhoneNumber");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.NctsHeaderDepartureMessageSendingObject)(null)).PhoneNumber)));
        this.PhoneNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 63, true);
        this.PhoneNumberTextBox.Name = "PhoneNumberTextBox";
        this.PhoneNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 17, true);
        this.PhoneNumberTextBox.TabIndex = 4;
        // 
        // EmailAddressTextBox
        // 
        this.BindingSource.SetBindingMember(this.EmailAddressTextBox, "EmailAddress");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.NctsHeaderDepartureMessageSendingObject)(null)).EmailAddress)));
        this.EmailAddressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 89, true);
        this.EmailAddressTextBox.Name = "EmailAddressTextBox";
        this.EmailAddressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 17, true);
        this.EmailAddressTextBox.TabIndex = 5;
        // 
        // ApprovedLocationOfGoodsUserControl
        // 
        this.ApprovedLocationOfGoodsUserControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.ApprovedLocationOfGoodsUserControl, ".");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.ICusGoodsLocationProvider)(((Enterprise.Customs.CH.NCTS.Business.NctsHeaderDepartureMessageSendingObject)(null)))));
        this.ApprovedLocationOfGoodsUserControl.CusGoodsLocationProviderType = typeof(Enterprise.Customs.EU.Business.ICusGoodsLocationProvider);
        this.ApprovedLocationOfGoodsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(428, 11, true);
        this.ApprovedLocationOfGoodsUserControl.Name = "ApprovedLocationOfGoodsUserControl";
        this.ApprovedLocationOfGoodsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 20, true);
        this.ApprovedLocationOfGoodsUserControl.TabIndex = 6;
        // 
        // TransportDepartureDynamicLayoutPanel
        // 
        this.TransportDepartureDynamicLayoutPanel.AllowDrop = true;
        this.TransportDepartureDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(322, 37, true);
        this.TransportDepartureDynamicLayoutPanel.Name = "TransportDepartureDynamicLayoutPanel";
        this.TransportDepartureDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(497, 115, true);
        this.TransportDepartureDynamicLayoutPanel.TabIndex = 0;
        // 
        // CommunicationLanguageDropEdit
        // 
        this.CommunicationLanguageDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.CommunicationLanguageDropEdit, "CommunicationLanguage");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.NCTS.Business.NctsHeaderDepartureMessageSendingObject)(null)).CommunicationLanguage)));
        this.CommunicationLanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 117, true);
        this.CommunicationLanguageDropEdit.Name = "CommunicationLanguageDropEdit";
        this.CommunicationLanguageDropEdit.PreBoundMaxLength = 2;
        this.CommunicationLanguageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 17, true);
        this.CommunicationLanguageDropEdit.TabIndex = 11;
        // 
        // NC123MessageSendingDetailsUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.CaptionResourceString = Enterprise.Customs.CH.NCTS.GUI.Res.GetData("db153ea9-2e83-4852-a018-1bf5e4a3d50e", "Sending Departure Activation");
        this.Controls.Add(this.IdentificationNumberTextBox);
        this.Controls.Add(this.ContactNameTextBox);
        this.Controls.Add(this.PhoneNumberTextBox);
        this.Controls.Add(this.EmailAddressTextBox);
        this.Controls.Add(this.ApprovedLocationOfGoodsUserControl);
        this.Controls.Add(this.TransportDepartureDynamicLayoutPanel);
        this.Controls.Add(this.CommunicationLanguageDropEdit);
        this.Name = "NC123MessageSendingDetailsUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 205, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.ApprovedLocationOfGoodsUserControl.ResumeLayout(true);
        this.ApprovedLocationOfGoodsUserControl.PerformLayout();
        this.CommunicationLanguageDropEdit.ResumeLayout(true);
        this.CommunicationLanguageDropEdit.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.ZTextBox IdentificationNumberTextBox;
    internal ZArchitecture.ZTextBox ContactNameTextBox;
    internal ZArchitecture.ZTextBox PhoneNumberTextBox;
    internal ZArchitecture.ZTextBox EmailAddressTextBox;
    internal LocationOfGoodsUserControl ApprovedLocationOfGoodsUserControl;
    internal ZArchitecture.GUI.DynamicLayoutPanel TransportDepartureDynamicLayoutPanel;
    internal ZArchitecture.GUI.ZDropEdit CommunicationLanguageDropEdit;
}
