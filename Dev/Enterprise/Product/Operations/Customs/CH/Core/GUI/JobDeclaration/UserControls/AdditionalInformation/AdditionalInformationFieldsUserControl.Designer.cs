
namespace Enterprise.Customs.CH.GUI;

partial class AdditionalInformationFieldsUserControl
{
    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.CodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.ReferenceNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.CodeDropEdit.SuspendLayout();
        this.ReferenceNumberDropEdit.SuspendLayout();
        this.DescriptionTextBox.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.AdditionalInformation);
        // 
        // CodeDropEdit
        // 
        this.CodeDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.CodeDropEdit, "CSI_Code");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.AdditionalInformation)(null)).CSI_Code)));
        this.CodeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
        this.CodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 18, true);
        this.CodeDropEdit.Name = "CodeDropEdit";
        this.CodeDropEdit.PreBoundMaxLength = 3;
        this.CodeDropEdit.TabIndex = 0;
        // 
        // ReferenceNumberDropEdit
        // 
        this.ReferenceNumberDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.ReferenceNumberDropEdit, "CSI_ReferenceNumber");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.AdditionalInformation)(null)).CSI_ReferenceNumber)));
        this.ReferenceNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 40, true);
        this.ReferenceNumberDropEdit.Name = "ReferenceNumberDropEdit";
        this.ReferenceNumberDropEdit.PreBoundMaxLength = 50;
        this.ReferenceNumberDropEdit.TabIndex = 1;
        // 
        // DescriptionTextBox
        //
        this.BindingSource.SetBindingMember(this.DescriptionTextBox, "CSI_Description");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.AdditionalInformation)(null)).CSI_Description)));
        this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
        this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 40, true);
        this.DescriptionTextBox.Multiline = true;
        this.DescriptionTextBox.Name = "DescriptionTextBox";
        this.DescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(675, 89, true);
        this.DescriptionTextBox.TabIndex = 2;
        // 
        // AdditionalInformationDetailsUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.CodeDropEdit);
        this.Controls.Add(this.ReferenceNumberDropEdit);
        this.Controls.Add(this.DescriptionTextBox);
        this.Dock = System.Windows.Forms.DockStyle.Fill;
        this.Name = "AdditionalInformationDetailsUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 72, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.CodeDropEdit.ResumeLayout(true);
        this.CodeDropEdit.PerformLayout();
        this.ReferenceNumberDropEdit.ResumeLayout(true);
        this.ReferenceNumberDropEdit.PerformLayout();
        this.DescriptionTextBox.ResumeLayout(true);
        this.DescriptionTextBox.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.GUI.ZDropEdit CodeDropEdit;
    internal ZArchitecture.GUI.ZDropEdit ReferenceNumberDropEdit;
    internal ZArchitecture.ZTextBox DescriptionTextBox;
}
