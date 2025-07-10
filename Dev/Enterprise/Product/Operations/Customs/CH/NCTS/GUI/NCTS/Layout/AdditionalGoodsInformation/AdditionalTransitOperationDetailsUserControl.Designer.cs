using System;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

partial class AdditionalTransitOperationDetailsUserControl
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
        this.AdditionalTransitOperationDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
        this.LineNoCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
        this.TypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.ReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.GrossMassDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
        this.PackagesCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
        this.StateOfSealsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.AdditionalTransitOperationDetailsPanel.SuspendLayout();
        this.TypeDropEdit.SuspendLayout();
        this.GrossMassDropEdit.SuspendLayout();
        this.PackagesCalcDropEdit.SuspendLayout();
        this.StateOfSealsDropEdit.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.NCTS.Business.AdditionalTransitOperation);
        // 
        // AdditionalTransitOperationDetailsPanel
        // 
        this.AdditionalTransitOperationDetailsPanel.Controls.Add(this.LineNoCalcEdit);
        this.AdditionalTransitOperationDetailsPanel.Controls.Add(this.TypeDropEdit);
        this.AdditionalTransitOperationDetailsPanel.Controls.Add(this.ReferenceTextBox);
        this.AdditionalTransitOperationDetailsPanel.Controls.Add(this.DescriptionTextBox);
        this.AdditionalTransitOperationDetailsPanel.Controls.Add(this.GrossMassDropEdit);
        this.AdditionalTransitOperationDetailsPanel.Controls.Add(this.PackagesCalcDropEdit);
        this.AdditionalTransitOperationDetailsPanel.Controls.Add(this.StateOfSealsDropEdit);
        this.AdditionalTransitOperationDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.AdditionalTransitOperationDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.AdditionalTransitOperationDetailsPanel.Name = "AdditionalTransitOperationDetailsPanel";
        this.AdditionalTransitOperationDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(511, 395, true);
        this.AdditionalTransitOperationDetailsPanel.TabIndex = 0;
        this.AdditionalTransitOperationDetailsPanel.TabStop = false;
        // 
        // LineNoCalcEdit
        // 
        this.BindingSource.SetBindingMember(this.LineNoCalcEdit, "CSI_LineNo");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.NCTS.Business.AdditionalTransitOperation)(null)).CSI_LineNo)));
        this.LineNoCalcEdit.DecimalPlaces = 2;
        this.LineNoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 16, true);
        this.LineNoCalcEdit.Name = "LineNoCalcEdit";
        this.LineNoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
        this.LineNoCalcEdit.TabIndex = 0;
        this.LineNoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.LineNoCalcEdit.TrackDisposedAccess = true;
        // 
        // TypeDropEdit
        // 
        this.TypeDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.TypeDropEdit, "CSI_IssuerType");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.NCTS.Business.AdditionalTransitOperation)(null)).CSI_IssuerType)));
        this.TypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 38, true);
        this.TypeDropEdit.Name = "TypeDropEdit";
        this.TypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
        this.TypeDropEdit.TabIndex = 1;
        // 
        // ReferenceTextBox
        // 
        this.BindingSource.SetBindingMember(this.ReferenceTextBox, "CSI_ReferenceNumber");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.AdditionalTransitOperation)(null)).CSI_ReferenceNumber)));
        this.ReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 60, true);
        this.ReferenceTextBox.Name = "ReferenceTextBox";
        this.ReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 17, true);
        this.ReferenceTextBox.TabIndex = 2;
        // 
        // DescriptionTextBox
        // 
        this.BindingSource.SetBindingMember(this.DescriptionTextBox, "CSI_Description");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.AdditionalTransitOperation)(null)).CSI_Description)));
        this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 82, true);
        this.DescriptionTextBox.Multiline = true;
        this.DescriptionTextBox.Name = "DescriptionTextBox";
        this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 90, true);
        this.DescriptionTextBox.TabIndex = 3;
        // 
        // GrossMassDropEdit
        // 
        this.GrossMassDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.GrossMassDropEdit, ".");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.NCTS.Business.AdditionalTransitOperation)(null)).CSI_Quantity)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.AdditionalTransitOperation)(null)).CSI_UnitOfQuantity)));
        this.GrossMassDropEdit.BindToAmount = "CSI_Quantity";
        this.GrossMassDropEdit.BindToUnit = "CSI_UnitOfQuantity";
        this.GrossMassDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 177, true);
        this.GrossMassDropEdit.Name = "GrossMassDropEdit";
        this.GrossMassDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 17, true);
        this.GrossMassDropEdit.TabIndex = 4;
        // 
        // PackagesCalcDropEdit
        // 
        this.PackagesCalcDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.PackagesCalcDropEdit, ".");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.NCTS.Business.AdditionalTransitOperation)(null)).CSI_PackQty)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.AdditionalTransitOperation)(null)).CSI_PackType)));
        this.PackagesCalcDropEdit.BindToAmount = "CSI_PackQty";
        this.PackagesCalcDropEdit.BindToUnit = "CSI_PackType";
        this.PackagesCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 199, true);
        this.PackagesCalcDropEdit.Name = "PackagesCalcDropEdit";
        this.PackagesCalcDropEdit.ShowDescriptionBox = true;
        this.PackagesCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 17, true);
        this.PackagesCalcDropEdit.TabIndex = 5;
        this.PackagesCalcDropEdit.UnitPreBoundMaxLength = 2;
        // 
        // StateOfSealsDropEdit
        // 
        this.StateOfSealsDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.StateOfSealsDropEdit, "CSI_Status");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.NCTS.Business.AdditionalTransitOperation)(null)).CSI_Status)));
        this.StateOfSealsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 221, true);
        this.StateOfSealsDropEdit.Name = "StateOfSealsDropEdit";
        this.StateOfSealsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 17, true);
        this.StateOfSealsDropEdit.TabIndex = 6;
        // 
        // AdditionalTransitOperationDetailsUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.AdditionalTransitOperationDetailsPanel);
        this.Name = "AdditionalTransitOperationDetailsUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(511, 395, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.AdditionalTransitOperationDetailsPanel.ResumeLayout(false);
        this.AdditionalTransitOperationDetailsPanel.PerformLayout();
        this.TypeDropEdit.ResumeLayout(true);
        this.TypeDropEdit.PerformLayout();
        this.GrossMassDropEdit.ResumeLayout(true);
        this.GrossMassDropEdit.PerformLayout();
        this.PackagesCalcDropEdit.ResumeLayout(true);
        this.PackagesCalcDropEdit.PerformLayout();
        this.StateOfSealsDropEdit.ResumeLayout(true);
        this.StateOfSealsDropEdit.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZPanel AdditionalTransitOperationDetailsPanel;
    internal ZCalcEdit LineNoCalcEdit;
    internal ZDropEdit TypeDropEdit;
    internal ZTextBox ReferenceTextBox;
    internal ZTextBox DescriptionTextBox;
    internal ZCalcDropEdit GrossMassDropEdit;
    internal ZCalcDropEdit PackagesCalcDropEdit;
    internal ZDropEdit StateOfSealsDropEdit;
}
