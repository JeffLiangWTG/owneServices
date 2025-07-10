using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

partial class CusEntryLineExtendedInformationsUserControl
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
        this.CL_AdValoremTariffTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.CL_DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.Quantities = new Enterprise.ZArchitecture.GUI.ZGroupBox();
        this.DynamicQuantitiesPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.Quantities.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.CusEntryLine);
        // 
        // CL_AdValoremTariffTextBox
        // 
        this.BindingSource.SetBindingMember(this.CL_AdValoremTariffTextBox, "CL_AdValoremTariff");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusEntryLine)(null)).CL_AdValoremTariff)));
        this.CL_AdValoremTariffTextBox.CaptionResourceString = null;
        this.CL_AdValoremTariffTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 3, true);
        this.CL_AdValoremTariffTextBox.Name = "CL_AdValoremTariffTextBox";
        this.CL_AdValoremTariffTextBox.ReadOnly = true;
        this.CL_AdValoremTariffTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
        this.CL_AdValoremTariffTextBox.TabIndex = 0;
        // 
        // CL_DescriptionTextBox
        // 
        this.BindingSource.SetBindingMember(this.CL_DescriptionTextBox, "CL_Description");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusEntryLine)(null)).CL_Description)));
        this.CL_DescriptionTextBox.CaptionResourceString = null;
        this.CL_DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 29, true);
        this.CL_DescriptionTextBox.Multiline = true;
        this.CL_DescriptionTextBox.Name = "CL_DescriptionTextBox";
        this.CL_DescriptionTextBox.ReadOnly = true;
        this.CL_DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 100, true);
        this.CL_DescriptionTextBox.TabIndex = 1;
        // 
        // Quantities
        // 
        this.Quantities.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("2e3625d9-4a86-4499-bf60-0fe487a77579", "Quantities");
        this.Quantities.Controls.Add(this.DynamicQuantitiesPanel);
        this.Quantities.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 3, true);
        this.Quantities.Name = "Quantities";
        this.Quantities.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(373, 141, true);
        this.Quantities.TabIndex = 2;
        this.Quantities.TabStop = false;
        // 
        // DynamicQuantitiesPanel
        // 
        this.DynamicQuantitiesPanel.AllowDrop = true;
        this.DynamicQuantitiesPanel.AutoScroll = true;
        this.DynamicQuantitiesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.DynamicQuantitiesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
        this.DynamicQuantitiesPanel.Name = "DynamicQuantitiesPanel";
        this.DynamicQuantitiesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 124, true);
        this.DynamicQuantitiesPanel.TabIndex = 0;
        // 
        // CusEntryLineExtendedInformationsUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.Quantities);
        this.Controls.Add(this.CL_DescriptionTextBox);
        this.Controls.Add(this.CL_AdValoremTariffTextBox);
        this.Name = "CusEntryLineExtendedInformationsUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(607, 148, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.Quantities.ResumeLayout(false);
        this.Quantities.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    internal ZArchitecture.ZTextBox CL_AdValoremTariffTextBox;
    internal ZArchitecture.ZTextBox CL_DescriptionTextBox;
    private ZArchitecture.GUI.ZGroupBox Quantities;
    internal DynamicLayoutPanel DynamicQuantitiesPanel;

    #endregion
}
