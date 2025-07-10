using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

partial class GoodsItemDifferencesDetailsColumnUserControl
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
        this.UnloadedCommodityCodeCodeFindBox = new Enterprise.Customs.CH.NCTS.GUI.NctsTariffFindBox();
        this.DeclaredCommodityCodeCodeFindBox = new Enterprise.Customs.CH.NCTS.GUI.NctsTariffFindBox();
        this.UnloadingRemarkCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.UnloadingRemarkTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.UnloadedCommodityCodeCodeFindBox.SuspendLayout();
        this.DeclaredCommodityCodeCodeFindBox.SuspendLayout();
        this.UnloadingRemarkCodeDropEdit.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.NCTS.Business.NctsArrivalCargoDesc);
        // 
        // UnloadedCommodityCodeCodeFindBox
        // 
        this.UnloadedCommodityCodeCodeFindBox.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.UnloadedCommodityCodeCodeFindBox, "UnloadedGoodsItem.BY_FormattedHarmonisedTariff");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.NctsArrivalCargoDesc)(null)).UnloadedGoodsItem.BY_FormattedHarmonisedTariff)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsArrivalCargoDesc)(null)).Lookups.Tariffs)));
        this.UnloadedCommodityCodeCodeFindBox.BindToList = "Lookups+Tariffs";
        this.UnloadedCommodityCodeCodeFindBox.ErrorForUnsupportedCountry = null;
        this.UnloadedCommodityCodeCodeFindBox.GetEffectiveDate = null;
        this.UnloadedCommodityCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 3, true);
        this.UnloadedCommodityCodeCodeFindBox.Name = "UnloadedCommodityCodeCodeFindBox";
        this.UnloadedCommodityCodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
        this.UnloadedCommodityCodeCodeFindBox.ParentType = null;
        this.UnloadedCommodityCodeCodeFindBox.PreBoundMaxLength = 8;
        this.UnloadedCommodityCodeCodeFindBox.SelectNomenclatureModes = null;
        this.UnloadedCommodityCodeCodeFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
        this.UnloadedCommodityCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 20, true);
        this.UnloadedCommodityCodeCodeFindBox.TabIndex = 1;
        this.UnloadedCommodityCodeCodeFindBox.TariffType = null;
        // 
        // DeclaredCommodityCodeCodeFindBox
        // 
        this.DeclaredCommodityCodeCodeFindBox.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.DeclaredCommodityCodeCodeFindBox, "BY_FormattedHarmonisedTariff");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.NctsArrivalCargoDesc)(null)).BY_FormattedHarmonisedTariff)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsArrivalCargoDesc)(null)).Lookups.Tariffs)));
        this.DeclaredCommodityCodeCodeFindBox.BindToList = "Lookups+Tariffs";
        this.DeclaredCommodityCodeCodeFindBox.ErrorForUnsupportedCountry = null;
        this.DeclaredCommodityCodeCodeFindBox.GetEffectiveDate = null;
        this.DeclaredCommodityCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 28, true);
        this.DeclaredCommodityCodeCodeFindBox.Name = "DeclaredCommodityCodeCodeFindBox";
        this.DeclaredCommodityCodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
        this.DeclaredCommodityCodeCodeFindBox.ParentType = null;
        this.DeclaredCommodityCodeCodeFindBox.PreBoundMaxLength = 8;
        this.DeclaredCommodityCodeCodeFindBox.SelectNomenclatureModes = null;
        this.DeclaredCommodityCodeCodeFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
        this.DeclaredCommodityCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 20, true);
        this.DeclaredCommodityCodeCodeFindBox.TabIndex = 0;
        this.DeclaredCommodityCodeCodeFindBox.TariffType = null;
        // 
        // UnloadingRemarkCodeDropEdit
        // 
        this.UnloadingRemarkCodeDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.UnloadingRemarkCodeDropEdit, "UnloadingRemarkCode");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.NCTS.Business.NctsArrivalCargoDesc)(null)).UnloadingRemarkCode)));
        this.UnloadingRemarkCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 54, true);
        this.UnloadingRemarkCodeDropEdit.Name = "UnloadingRemarkCodeDropEdit";
        this.UnloadingRemarkCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
        this.UnloadingRemarkCodeDropEdit.TabIndex = 2;
        // 
        // UnloadingRemarkTextTextBox
        // 
        this.BindingSource.SetBindingMember(this.UnloadingRemarkTextTextBox, "UnloadingRemarkText");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.NctsArrivalCargoDesc)(null)).UnloadingRemarkText)));
        this.UnloadingRemarkTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
        this.UnloadingRemarkTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 80, true);
        this.UnloadingRemarkTextTextBox.Multiline = true;
        this.UnloadingRemarkTextTextBox.Name = "UnloadingRemarkTextTextBox";
        this.UnloadingRemarkTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 45, true);
        this.UnloadingRemarkTextTextBox.TabIndex = 3;
        // 
        // GoodsItemDifferencesDetailsColumnUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.Controls.Add(this.DeclaredCommodityCodeCodeFindBox);
        this.Controls.Add(this.UnloadedCommodityCodeCodeFindBox);
        this.Controls.Add(this.UnloadingRemarkTextTextBox);
        this.Controls.Add(this.UnloadingRemarkCodeDropEdit);
        this.Name = "GoodsItemDifferencesDetailsColumnUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 135, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.UnloadedCommodityCodeCodeFindBox.ResumeLayout(true);
        this.UnloadedCommodityCodeCodeFindBox.PerformLayout();
        this.DeclaredCommodityCodeCodeFindBox.ResumeLayout(true);
        this.DeclaredCommodityCodeCodeFindBox.PerformLayout();
        this.UnloadingRemarkCodeDropEdit.ResumeLayout(true);
        this.UnloadingRemarkCodeDropEdit.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal NctsTariffFindBox DeclaredCommodityCodeCodeFindBox;
    internal NctsTariffFindBox UnloadedCommodityCodeCodeFindBox;
    internal ZDropEdit UnloadingRemarkCodeDropEdit;
    internal ZTextBox UnloadingRemarkTextTextBox;
}
