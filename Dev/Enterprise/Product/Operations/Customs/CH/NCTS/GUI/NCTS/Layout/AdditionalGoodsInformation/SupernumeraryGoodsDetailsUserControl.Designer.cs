using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

partial class SupernumeraryGoodsDetailsUserControl
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
        this.LineNoCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
        this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.GrossMassDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
        this.TariffFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
        this.PackagesCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
        this.SupernumeraryGoodsDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.GrossMassDropEdit.SuspendLayout();
        this.TariffFindBox.SuspendLayout();
        this.PackagesCalcDropEdit.SuspendLayout();
        this.SupernumeraryGoodsDetailsPanel.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.NCTS.Business.SupernumeraryGoods);
        // 
        // LineNoCalcEdit
        // 
        this.BindingSource.SetBindingMember(this.LineNoCalcEdit, "CSI_LineNo");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.NCTS.Business.SupernumeraryGoods)(null)).CSI_LineNo)));
        this.LineNoCalcEdit.DecimalPlaces = 2;
        this.LineNoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 16, true);
        this.LineNoCalcEdit.Name = "LineNoCalcEdit";
        this.LineNoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 16, true);
        this.LineNoCalcEdit.TabIndex = 0;
        this.LineNoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.LineNoCalcEdit.TrackDisposedAccess = true;
        // 
        // DescriptionTextBox
        // 
        this.BindingSource.SetBindingMember(this.DescriptionTextBox, "CSI_Description");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.SupernumeraryGoods)(null)).CSI_Description)));
        this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 38, true);
        this.DescriptionTextBox.Multiline = true;
        this.DescriptionTextBox.Name = "DescriptionTextBox";
        this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 90, true);
        this.DescriptionTextBox.TabIndex = 1;
        // 
        // GrossMassDropEdit
        // 
        this.GrossMassDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.GrossMassDropEdit, ".");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.NCTS.Business.SupernumeraryGoods)(null)).CSI_Quantity)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.SupernumeraryGoods)(null)).CSI_UnitOfQuantity)));
        this.GrossMassDropEdit.BindToAmount = "CSI_Quantity";
        this.GrossMassDropEdit.BindToUnit = "CSI_UnitOfQuantity";
        this.GrossMassDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 133, true);
        this.GrossMassDropEdit.Name = "GrossMassDropEdit";
        this.GrossMassDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 16, true);
        this.GrossMassDropEdit.TabIndex = 2;
        // 
        // TariffFindBox
        // 
        this.TariffFindBox.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.TariffFindBox, "CSI_Tariff");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.SupernumeraryGoods)(null)).CSI_Tariff)));
        this.TariffFindBox.ErrorForUnsupportedCountry = null;
        this.TariffFindBox.GetDataGrouping = null;
        this.TariffFindBox.GetEffectiveDate = null;
        this.TariffFindBox.GetTariffType = null;
        this.TariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 155, true);
        this.TariffFindBox.Name = "TariffFindBox";
        this.TariffFindBox.NeedLoadParentDataGroup = true;
        this.TariffFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
        this.TariffFindBox.ParentType = null;
        this.TariffFindBox.SelectNomenclatureModes = null;
        this.TariffFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
        this.TariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 16, true);
        this.TariffFindBox.TabIndex = 3;
        this.TariffFindBox.TariffType = null;
        // 
        // PackagesCalcDropEdit
        // 
        this.PackagesCalcDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.PackagesCalcDropEdit, ".");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.NCTS.Business.SupernumeraryGoods)(null)).CSI_PackQty)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.SupernumeraryGoods)(null)).CSI_PackType)));
        this.PackagesCalcDropEdit.BindToAmount = "CSI_PackQty";
        this.PackagesCalcDropEdit.BindToUnit = "CSI_PackType";
        this.PackagesCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 177, true);
        this.PackagesCalcDropEdit.Name = "PackagesCalcDropEdit";
        this.PackagesCalcDropEdit.ShowDescriptionBox = true;
        this.PackagesCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 16, true);
        this.PackagesCalcDropEdit.TabIndex = 4;
        this.PackagesCalcDropEdit.UnitPreBoundMaxLength = 2;
        // 
        // SupernumeraryGoodsDetailsGroupBox
        // 
        this.SupernumeraryGoodsDetailsPanel.Controls.Add(this.LineNoCalcEdit);
        this.SupernumeraryGoodsDetailsPanel.Controls.Add(this.DescriptionTextBox);
        this.SupernumeraryGoodsDetailsPanel.Controls.Add(this.GrossMassDropEdit);
        this.SupernumeraryGoodsDetailsPanel.Controls.Add(this.TariffFindBox);
        this.SupernumeraryGoodsDetailsPanel.Controls.Add(this.PackagesCalcDropEdit);
        this.SupernumeraryGoodsDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.SupernumeraryGoodsDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.SupernumeraryGoodsDetailsPanel.Name = "SupernumeraryGoodsDetailsPanel";
        this.SupernumeraryGoodsDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 303, true);
        this.SupernumeraryGoodsDetailsPanel.TabIndex = 5;
        this.SupernumeraryGoodsDetailsPanel.TabStop = false;
        // 
        // SupernumeraryGoodsDetailsUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.SupernumeraryGoodsDetailsPanel);
        this.Name = "SupernumeraryGoodsDetailsUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 303, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.GrossMassDropEdit.ResumeLayout(true);
        this.GrossMassDropEdit.PerformLayout();
        this.TariffFindBox.ResumeLayout(true);
        this.TariffFindBox.PerformLayout();
        this.PackagesCalcDropEdit.ResumeLayout(true);
        this.PackagesCalcDropEdit.PerformLayout();
        this.SupernumeraryGoodsDetailsPanel.ResumeLayout(false);
        this.SupernumeraryGoodsDetailsPanel.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    Enterprise.ZArchitecture.GUI.ZPanel SupernumeraryGoodsDetailsPanel;
    internal Enterprise.ZArchitecture.ZCalcEdit LineNoCalcEdit;
    internal Enterprise.ZArchitecture.ZTextBox DescriptionTextBox;
    internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit GrossMassDropEdit;
    internal Enterprise.Customs.Universal.GUI.TariffFindBox TariffFindBox;
    internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit PackagesCalcDropEdit;
}
