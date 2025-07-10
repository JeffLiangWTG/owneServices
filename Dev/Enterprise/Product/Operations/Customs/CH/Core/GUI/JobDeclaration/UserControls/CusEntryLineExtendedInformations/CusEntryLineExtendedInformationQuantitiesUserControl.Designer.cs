
namespace Enterprise.Customs.CH.GUI;

partial class CusEntryLineExtendedInformationQuantitiesUserControl
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
        this.CalcCustomsNetWeightDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
        this.CalcAdditionalQuantityDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
        this.CalcNetWeightDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
        this.CalcGrossWeightDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.CalcCustomsNetWeightDropEdit.SuspendLayout();
        this.CalcAdditionalQuantityDropEdit.SuspendLayout();
        this.CalcNetWeightDropEdit.SuspendLayout();
        this.CalcGrossWeightDropEdit.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.CusEntryLine);
        // 
        // CalcCustomsNetWeightDropEdit
        // 
        this.CalcCustomsNetWeightDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.CalcCustomsNetWeightDropEdit, ".");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.CusEntryLine)(null)).CalcCustomsNetWeight)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusEntryLine)(null)).CalcCustomsNetWeightUQ)));
        this.CalcCustomsNetWeightDropEdit.BindToAmount = "CalcCustomsNetWeight";
        this.CalcCustomsNetWeightDropEdit.BindToUnit = "CalcCustomsNetWeightUQ";
        this.CalcCustomsNetWeightDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 70, true);
        this.CalcCustomsNetWeightDropEdit.Name = "CalcCustomsNetWeightDropEdit";
        this.CalcCustomsNetWeightDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 18, true);
        this.CalcCustomsNetWeightDropEdit.TabIndex = 3;
        // 
        // CalcAdditionalQuantityDropEdit
        // 
        this.CalcAdditionalQuantityDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.CalcAdditionalQuantityDropEdit, ".");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.CusEntryLine)(null)).CalcAdditionalQty)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusEntryLine)(null)).CalcAdditionalQtyUQ)));
        this.CalcAdditionalQuantityDropEdit.BindToAmount = "CalcAdditionalQty";
        this.CalcAdditionalQuantityDropEdit.BindToUnit = "CalcAdditionalQtyUQ";
        this.CalcAdditionalQuantityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 47, true);
        this.CalcAdditionalQuantityDropEdit.Name = "CalcAdditionalQuantityDropEdit";
        this.CalcAdditionalQuantityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 18, true);
        this.CalcAdditionalQuantityDropEdit.TabIndex = 2;
        // 
        // CalcNetWeightDropEdit
        // 
        this.CalcNetWeightDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.CalcNetWeightDropEdit, ".");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.CusEntryLine)(null)).CalcNetWeight)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusEntryLine)(null)).CalcNetWeightUQ)));
        this.CalcNetWeightDropEdit.BindToAmount = "CalcNetWeight";
        this.CalcNetWeightDropEdit.BindToUnit = "CalcNetWeightUQ";
        this.CalcNetWeightDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 24, true);
        this.CalcNetWeightDropEdit.Name = "CalcNetWeightDropEdit";
        this.CalcNetWeightDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 18, true);
        this.CalcNetWeightDropEdit.TabIndex = 1;
        // 
        // CalcGrossWeightDropEdit
        // 
        this.CalcGrossWeightDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.CalcGrossWeightDropEdit, ".");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.CusEntryLine)(null)).CalcGrossWeight)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusEntryLine)(null)).CalcGrossWeightUQ)));
        this.CalcGrossWeightDropEdit.BindToAmount = "CalcGrossWeight";
        this.CalcGrossWeightDropEdit.BindToUnit = "CalcGrossWeightUQ";
        this.CalcGrossWeightDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 2, true);
        this.CalcGrossWeightDropEdit.Name = "CalcGrossWeightDropEdit";
        this.CalcGrossWeightDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 18, true);
        this.CalcGrossWeightDropEdit.TabIndex = 0;
        // 
        // CusEntryLineExtendedInformationQuantitiesUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.CalcCustomsNetWeightDropEdit);
        this.Controls.Add(this.CalcAdditionalQuantityDropEdit);
        this.Controls.Add(this.CalcNetWeightDropEdit);
        this.Controls.Add(this.CalcGrossWeightDropEdit);
        this.Name = "CusEntryLineExtendedInformationQuantitiesUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(354, 113, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.CalcCustomsNetWeightDropEdit.ResumeLayout(true);
        this.CalcCustomsNetWeightDropEdit.PerformLayout();
        this.CalcAdditionalQuantityDropEdit.ResumeLayout(true);
        this.CalcAdditionalQuantityDropEdit.PerformLayout();
        this.CalcNetWeightDropEdit.ResumeLayout(true);
        this.CalcNetWeightDropEdit.PerformLayout();
        this.CalcGrossWeightDropEdit.ResumeLayout(true);
        this.CalcGrossWeightDropEdit.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.GUI.ZCalcDropEdit CalcCustomsNetWeightDropEdit;
    internal ZArchitecture.GUI.ZCalcDropEdit CalcAdditionalQuantityDropEdit;
    internal ZArchitecture.GUI.ZCalcDropEdit CalcNetWeightDropEdit;
    internal ZArchitecture.GUI.ZCalcDropEdit CalcGrossWeightDropEdit;
}
