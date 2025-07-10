namespace Enterprise.Customs.CH.GUI;

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
        this.DeclarationLanguageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.TransportDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
        this.LocationOfGoodsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.NextProcedureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.DeclarationLanguageDropEdit.SuspendLayout();
        this.LocationOfGoodsDropEdit.SuspendLayout();
        this.NextProcedureDropEdit.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.ExportDeclarationMessageSendingObjectParent);
        // 
        // IdentificationNumberTextBox
        // 
        this.BindingSource.SetBindingMember(this.IdentificationNumberTextBox, "IdentificationNumber");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.ExportDeclarationMessageSendingObjectParent)(null)).IdentificationNumber)));
        this.IdentificationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 11, true);
        this.IdentificationNumberTextBox.Name = "IdentificationNumberTextBox";
        this.IdentificationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 17, true);
        this.IdentificationNumberTextBox.TabIndex = 2;
        // 
        // ContactNameTextBox
        // 
        this.BindingSource.SetBindingMember(this.ContactNameTextBox, "ContactName");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.ExportDeclarationMessageSendingObjectParent)(null)).ContactName)));
        this.ContactNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 37, true);
        this.ContactNameTextBox.Name = "ContactNameTextBox";
        this.ContactNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 17, true);
        this.ContactNameTextBox.TabIndex = 3;
        // 
        // PhoneNumberTextBox
        // 
        this.BindingSource.SetBindingMember(this.PhoneNumberTextBox, "PhoneNumber");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.ExportDeclarationMessageSendingObjectParent)(null)).PhoneNumber)));
        this.PhoneNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 63, true);
        this.PhoneNumberTextBox.Name = "PhoneNumberTextBox";
        this.PhoneNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 17, true);
        this.PhoneNumberTextBox.TabIndex = 4;
        // 
        // EmailAddressTextBox
        // 
        this.BindingSource.SetBindingMember(this.EmailAddressTextBox, "EmailAddress");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.ExportDeclarationMessageSendingObjectParent)(null)).EmailAddress)));
        this.EmailAddressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 89, true);
        this.EmailAddressTextBox.Name = "EmailAddressTextBox";
        this.EmailAddressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 17, true);
        this.EmailAddressTextBox.TabIndex = 5;
        // 
        // DeclarationLanguageDropEdit
        // 
        this.DeclarationLanguageDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.DeclarationLanguageDropEdit, "SendingDeclaration.JE_DeclarationLanguage");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.ExportDeclarationMessageSendingObjectParent)(null)).SendingDeclaration.JE_DeclarationLanguage)));
        this.DeclarationLanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 115, true);
        this.DeclarationLanguageDropEdit.Name = "DeclarationLanguageDropEdit";
        this.DeclarationLanguageDropEdit.PreBoundMaxLength = 3;
        this.DeclarationLanguageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 17, true);
        this.DeclarationLanguageDropEdit.TabIndex = 13;
        // 
        // TransportDynamicLayoutPanel
        // 
        this.TransportDynamicLayoutPanel.AllowDrop = true;
        this.TransportDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(322, 63, true);
        this.TransportDynamicLayoutPanel.Name = "TransportDynamicLayoutPanel";
        this.TransportDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(497, 115, true);
        this.TransportDynamicLayoutPanel.TabIndex = 0;
        // 
        // LocationOfGoodsDropEdit
        // 
        this.LocationOfGoodsDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.LocationOfGoodsDropEdit, "SendingDeclaration.JE_LocationOfGoods");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.ExportDeclarationMessageSendingObjectParent)(null)).SendingDeclaration.JE_LocationOfGoods)));
        this.LocationOfGoodsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(428, 11, true);
        this.LocationOfGoodsDropEdit.Name = "LocationOfGoodsDropEdit";
        this.LocationOfGoodsDropEdit.ShouldResizeByMaxLength = false;
        this.LocationOfGoodsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 17, true);
        this.LocationOfGoodsDropEdit.TabIndex = 8;
        // 
        // NextProcedureDropEdit
        // 
        this.NextProcedureDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.NextProcedureDropEdit, "SendingObjectsCollection.NextProcedure");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.ExportDeclarationMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.ExportDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).NextProcedure)));
        this.NextProcedureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(428, 37, true);
        this.NextProcedureDropEdit.Name = "NextProcedureDropEdit";
        this.NextProcedureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 17, true);
        this.NextProcedureDropEdit.TabIndex = 14;
        // 
        // NC123MessageSendingDetailsUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.NextProcedureDropEdit);
        this.Controls.Add(this.IdentificationNumberTextBox);
        this.Controls.Add(this.ContactNameTextBox);
        this.Controls.Add(this.PhoneNumberTextBox);
        this.Controls.Add(this.EmailAddressTextBox);
        this.Controls.Add(this.DeclarationLanguageDropEdit);
        this.Controls.Add(this.LocationOfGoodsDropEdit);
        this.Controls.Add(this.TransportDynamicLayoutPanel);
        this.Name = "NC123MessageSendingDetailsUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 173, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.DeclarationLanguageDropEdit.ResumeLayout(true);
        this.DeclarationLanguageDropEdit.PerformLayout();
        this.LocationOfGoodsDropEdit.ResumeLayout(true);
        this.LocationOfGoodsDropEdit.PerformLayout();
        this.NextProcedureDropEdit.ResumeLayout(true);
        this.NextProcedureDropEdit.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.ZTextBox IdentificationNumberTextBox;
    internal ZArchitecture.ZTextBox ContactNameTextBox;
    internal ZArchitecture.ZTextBox PhoneNumberTextBox;
    internal ZArchitecture.ZTextBox EmailAddressTextBox;
    internal ZArchitecture.GUI.ZDropEdit DeclarationLanguageDropEdit;
    internal ZArchitecture.GUI.ZDropEdit LocationOfGoodsDropEdit;
    internal ZArchitecture.GUI.DynamicLayoutPanel TransportDynamicLayoutPanel;
    internal ZArchitecture.GUI.ZDropEdit NextProcedureDropEdit;
}
