using CargoWise.Types;

namespace Enterprise.Customs.CH.GUI;

partial class TransportDetailsUserControl
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
        this.VehicleTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.DispatchCountryUserControl = new Enterprise.Customs.CH.GUI.DispatchCountryUserControl();
        this.SpecificCircumstanceIndicatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.TransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.VehicleTypeDropEdit.SuspendLayout();
        this.DispatchCountryUserControl.SuspendLayout();
        this.SpecificCircumstanceIndicatorDropEdit.SuspendLayout();
        this.TransportModeDropEdit.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.IMessageSendingDeclaration);
        // 
        // VehicleTypeDropEdit
        // 
        this.VehicleTypeDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.VehicleTypeDropEdit, "JE_VehicleType");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.IMessageSendingDeclaration)(null)).JE_VehicleType)));
        this.VehicleTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 3, true);
        this.VehicleTypeDropEdit.Name = "VehicleTypeDropEdit";
        this.VehicleTypeDropEdit.PreBoundMaxLength = 2;
        this.VehicleTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 17, true);
        this.VehicleTypeDropEdit.TabIndex = 6;
        // 
        // DispatchCountryUserControl
        // 
        this.DispatchCountryUserControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.DispatchCountryUserControl, ".");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CH.Business.JobDeclaration)(((Enterprise.Customs.CH.Business.IMessageSendingDeclaration)(null)))));
        this.DispatchCountryUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 29, true);
        this.DispatchCountryUserControl.Name = "DispatchCountryUserControl";
        this.DispatchCountryUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
        this.DispatchCountryUserControl.TabIndex = 7;
        // 
        // SpecificCircumstanceIndicatorDropEdit
        // 
        this.SpecificCircumstanceIndicatorDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.SpecificCircumstanceIndicatorDropEdit, "JE_SpecificCircumstanceIndicator");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.IMessageSendingDeclaration)(null)).JE_SpecificCircumstanceIndicator)));
        this.SpecificCircumstanceIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 108, true);
        this.SpecificCircumstanceIndicatorDropEdit.Name = "SpecificCircumstanceIndicatorDropEdit";
        this.SpecificCircumstanceIndicatorDropEdit.PreBoundMaxLength = 2;
        this.SpecificCircumstanceIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 17, true);
        this.SpecificCircumstanceIndicatorDropEdit.TabIndex = 0;
        // 
        // TransportModeDropEdit
        // 
        this.TransportModeDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.TransportModeDropEdit, "JE_TransportMode");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.IMessageSendingDeclaration)(null)).JE_TransportMode)));
        this.TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 53, true);
        this.TransportModeDropEdit.Name = "TransportModeDropEdit";
        this.TransportModeDropEdit.PreBoundMaxLength = 3;
        this.TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
        this.TransportModeDropEdit.TabIndex = 13;
        // 
        // TransportDetailsUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.Controls.Add(this.DispatchCountryUserControl);
        this.Controls.Add(this.VehicleTypeDropEdit);
        this.Controls.Add(this.SpecificCircumstanceIndicatorDropEdit);
        this.Controls.Add(this.TransportModeDropEdit);
        this.Name = "TransportDetailsUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 185, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.VehicleTypeDropEdit.ResumeLayout(true);
        this.VehicleTypeDropEdit.PerformLayout();
        this.DispatchCountryUserControl.ResumeLayout(true);
        this.DispatchCountryUserControl.PerformLayout();
        this.SpecificCircumstanceIndicatorDropEdit.ResumeLayout(true);
        this.SpecificCircumstanceIndicatorDropEdit.PerformLayout();
        this.TransportModeDropEdit.ResumeLayout(true);
        this.TransportModeDropEdit.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal Enterprise.ZArchitecture.GUI.ZDropEdit VehicleTypeDropEdit;
    internal DispatchCountryUserControl DispatchCountryUserControl;
    internal Enterprise.ZArchitecture.GUI.ZDropEdit SpecificCircumstanceIndicatorDropEdit;
    internal ZArchitecture.GUI.ZDropEdit TransportModeDropEdit;
}
