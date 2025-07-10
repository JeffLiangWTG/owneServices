using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.CH.GUI;

partial class OrganisationsUserControl
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
        this.ConsignorDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.ConsignorDocAddressControl.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.JobDeclaration);
        // 
        // ConsignorDcoAddressControl
        // 
        this.ConsignorDocAddressControl.AddressValidationProcessCmdKey = null;
        this.ConsignorDocAddressControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.ConsignorDocAddressControl, "ConsignorDocAddress");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).ConsignorDocAddress)));
        this.ConsignorDocAddressControl.BindToOrganisations = "Lookups+ConsignorList";
        this.ConsignorDocAddressControl.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("OrganisationsUserControl|db2963ba-fb7f-41fd-ac24-f445256c6eff", "Consignor");
        this.ConsignorDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
        this.ConsignorDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 18, true);
        this.ConsignorDocAddressControl.Name = "ConsignorDocAddressControl";
        this.ConsignorDocAddressControl.ReadOnly = false;
        this.ConsignorDocAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
        this.ConsignorDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
        this.ConsignorDocAddressControl.TabIndex = 5;
        this.ConsignorDocAddressControl.ValidationJustForced = false;
        // 
        // OrganisationsUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.Controls.Add(this.ConsignorDocAddressControl);
        this.Name = "OrganisationsUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 94, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.ConsignorDocAddressControl.ResumeLayout(true);
        this.ConsignorDocAddressControl.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZDocAddressControl ConsignorDocAddressControl;
}
