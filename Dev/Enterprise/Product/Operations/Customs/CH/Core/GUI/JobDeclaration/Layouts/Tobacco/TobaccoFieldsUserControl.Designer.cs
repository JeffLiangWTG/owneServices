namespace Enterprise.Customs.CH.GUI;

partial class TobaccoFieldsUserControl
{
    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.RetailPriceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
        this.TobaccoBrandDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.SequentialNumberIntEdit = new Enterprise.ZArchitecture.GUI.ZIntEdit();
        this.DesignationTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.SubGroupDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.MainGroupDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.ReverseNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.SpecialUnitOfMeasureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.TobaccoBrandDropEdit.SuspendLayout();
        this.SubGroupDropEdit.SuspendLayout();
        this.MainGroupDropEdit.SuspendLayout();
        this.SpecialUnitOfMeasureDropEdit.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.Tobacco);
        // 
        // RetailPriceCalcEdit
        // 
        this.BindingSource.SetBindingMember(this.RetailPriceCalcEdit, "CSI_Value");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.Tobacco)(null)).CSI_Value)));
        this.RetailPriceCalcEdit.DecimalPlaces = 2;
        this.RetailPriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 122, true);
        this.RetailPriceCalcEdit.Name = "RetailPriceCalcEdit";
        this.RetailPriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 17, true);
        this.RetailPriceCalcEdit.TabIndex = 4;
        this.RetailPriceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.RetailPriceCalcEdit.TrackDisposedAccess = true;
        // 
        // TobaccoBrandDropEdit
        // 
        this.TobaccoBrandDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.TobaccoBrandDropEdit, "CSI_AdditionalDescription");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.Tobacco)(null)).CSI_AdditionalDescription)));
        this.TobaccoBrandDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 148, true);
        this.TobaccoBrandDropEdit.Name = "TobaccoBrandDropEdit";
        this.TobaccoBrandDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(519, 17, true);
        this.TobaccoBrandDropEdit.TabIndex = 5;
        // 
        // SequentialNumberIntEdit
        // 
        this.BindingSource.SetBindingMember(this.SequentialNumberIntEdit, "CSI_ItemNumber");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.CH.Business.Tobacco)(null)).CSI_ItemNumber)));
        this.SequentialNumberIntEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 96, true);
        this.SequentialNumberIntEdit.Name = "SequentialNumberIntEdit";
        this.SequentialNumberIntEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 17, true);
        this.SequentialNumberIntEdit.TabIndex = 3;
        // 
        // DesignationTextBox
        // 
        this.DesignationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.BindingSource.SetBindingMember(this.DesignationTextBox, "CSI_Description");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.Tobacco)(null)).CSI_Description)));
        this.DesignationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 70, true);
        this.DesignationTextBox.Name = "DesignationTextBox";
        this.DesignationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(519, 17, true);
        this.DesignationTextBox.TabIndex = 2;
        // 
        // SubGroupDropEdit
        // 
        this.SubGroupDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.SubGroupDropEdit, "CSI_SubType");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.Tobacco)(null)).CSI_SubType)));
        this.SubGroupDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 44, true);
        this.SubGroupDropEdit.Name = "SubGroupDropEdit";
        this.SubGroupDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(519, 17, true);
        this.SubGroupDropEdit.TabIndex = 1;
        // 
        // MainGroupDropEdit
        // 
        this.MainGroupDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.MainGroupDropEdit, "CSI_Code");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.Tobacco)(null)).CSI_Code)));
        this.MainGroupDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 18, true);
        this.MainGroupDropEdit.Name = "MainGroupDropEdit";
        this.MainGroupDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(519, 17, true);
        this.MainGroupDropEdit.TabIndex = 0;
        // 
        // ReverseNumberTextBox
        // 
        this.BindingSource.SetBindingMember(this.ReverseNumberTextBox, "CSI_ReferenceNumber");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.Tobacco)(null)).CSI_ReferenceNumber)));
        this.ReverseNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 173, true);
        this.ReverseNumberTextBox.Name = "ReverseNumberTextBox";
        this.ReverseNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(519, 17, true);
        this.ReverseNumberTextBox.TabIndex = 6;
        // 
        // SpecialUnitOfMeasureDropEdit
        // 
        this.SpecialUnitOfMeasureDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.SpecialUnitOfMeasureDropEdit, "CSI_UnitOfQuantity");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.Tobacco)(null)).CSI_UnitOfQuantity)));
        this.SpecialUnitOfMeasureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 194, true);
        this.SpecialUnitOfMeasureDropEdit.Name = "SpecialUnitOfMeasureDropEdit";
        this.SpecialUnitOfMeasureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(519, 17, true);
        this.SpecialUnitOfMeasureDropEdit.TabIndex = 7;
        // 
        // TobaccoFieldsUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.RetailPriceCalcEdit);
        this.Controls.Add(this.TobaccoBrandDropEdit);
        this.Controls.Add(this.SequentialNumberIntEdit);
        this.Controls.Add(this.DesignationTextBox);
        this.Controls.Add(this.SubGroupDropEdit);
        this.Controls.Add(this.MainGroupDropEdit);
        this.Controls.Add(this.ReverseNumberTextBox);
        this.Controls.Add(this.SpecialUnitOfMeasureDropEdit);
        this.Name = "TobaccoFieldsUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1997, 785, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.TobaccoBrandDropEdit.ResumeLayout(true);
        this.TobaccoBrandDropEdit.PerformLayout();
        this.SubGroupDropEdit.ResumeLayout(true);
        this.SubGroupDropEdit.PerformLayout();
        this.MainGroupDropEdit.ResumeLayout(true);
        this.MainGroupDropEdit.PerformLayout();
        this.SpecialUnitOfMeasureDropEdit.ResumeLayout(true);
        this.SpecialUnitOfMeasureDropEdit.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.GUI.ZDropEdit MainGroupDropEdit;
    internal ZArchitecture.GUI.ZDropEdit SubGroupDropEdit;
    internal ZArchitecture.ZTextBox DesignationTextBox;
    internal ZArchitecture.GUI.ZIntEdit SequentialNumberIntEdit;
    internal ZArchitecture.GUI.ZDropEdit TobaccoBrandDropEdit;
    internal ZArchitecture.ZCalcEdit RetailPriceCalcEdit;
    internal ZArchitecture.ZTextBox ReverseNumberTextBox;
    internal ZArchitecture.GUI.ZDropEdit SpecialUnitOfMeasureDropEdit;
}
