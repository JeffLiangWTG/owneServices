namespace Enterprise.Customs.CH.NCTS.GUI;

partial class GoodsItemDetailsUserControl
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
        this.HarmonisedTariffFindBox = new Enterprise.Customs.CH.NCTS.GUI.NctsTariffFindBox();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.HarmonisedTariffFindBox.SuspendLayout();
        this.SuspendLayout();
        //
        // BindingSource
        //
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.NCTS.Business.NctsDepartureCargoDesc);
        //
        // HarmonisedTariffFindBox
        //
        this.HarmonisedTariffFindBox.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.HarmonisedTariffFindBox, "BY_FormattedHarmonisedTariff");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_FormattedHarmonisedTariff)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsDepartureCargoDesc)(null)).Lookups.Tariffs)));
        this.HarmonisedTariffFindBox.BindToList = "Lookups+Tariffs";
        this.HarmonisedTariffFindBox.ErrorForUnsupportedCountry = null;
        this.HarmonisedTariffFindBox.GetEffectiveDate = null;
        this.HarmonisedTariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 28, true);
        this.HarmonisedTariffFindBox.Name = "HarmonisedTariffFindBox";
        this.HarmonisedTariffFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
        this.HarmonisedTariffFindBox.ParentType = null;
        this.HarmonisedTariffFindBox.PreBoundMaxLength = 8;
        this.HarmonisedTariffFindBox.SelectNomenclatureModes = null;
        this.HarmonisedTariffFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
        this.HarmonisedTariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 20, true);
        this.HarmonisedTariffFindBox.TabIndex = 1;
        this.HarmonisedTariffFindBox.TariffType = null;
        //
        // GoodsItemDetailsUserControl
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.HarmonisedTariffFindBox);
        this.Name = "GoodsItemDetailsUserControl";
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.HarmonisedTariffFindBox.ResumeLayout(false);
        this.HarmonisedTariffFindBox.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal NctsTariffFindBox HarmonisedTariffFindBox;
}
