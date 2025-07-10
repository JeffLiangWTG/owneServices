namespace Enterprise.Customs.CH.GUI;

partial class InAndOutwardProcessingFieldsUserControl
{
    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.SubTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.CodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.ProcedureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.IssuerTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.StatusCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
        this.CustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.SubTypeDropEdit.SuspendLayout();
        this.CodeDropEdit.SuspendLayout();
        this.ProcedureDropEdit.SuspendLayout();
        this.IssuerTypeDropEdit.SuspendLayout();
        this.CustomsOfficeCodeFindBox.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.JobComInvoiceLine);
        // 
        // SubTypeDropEdit
        // 
        this.SubTypeDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.SubTypeDropEdit, "InAndOutwardProcessing+CSI_SubType");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).InAndOutwardProcessing.CSI_SubType)));
        this.SubTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 3, true);
        this.SubTypeDropEdit.Name = "SubTypeDropEdit";
        this.SubTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 17, true);
        this.SubTypeDropEdit.TabIndex = 0;
        // 
        // CodeDropEdit
        // 
        this.CodeDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.CodeDropEdit, "InAndOutwardProcessing+CSI_Code");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).InAndOutwardProcessing.CSI_Code)));
        this.CodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 29, true);
        this.CodeDropEdit.Name = "CodeDropEdit";
        this.CodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 17, true);
        this.CodeDropEdit.TabIndex = 1;
        // 
        // ProcedureDropEdit
        // 
        this.ProcedureDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.ProcedureDropEdit, "InAndOutwardProcessing+CSI_Procedure");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).InAndOutwardProcessing.CSI_Procedure)));
        this.ProcedureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 55, true);
        this.ProcedureDropEdit.Name = "ProcedureDropEdit";
        this.ProcedureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 17, true);
        this.ProcedureDropEdit.TabIndex = 2;
        // 
        // IssuerTypeDropEdit
        // 
        this.IssuerTypeDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.IssuerTypeDropEdit, "InAndOutwardProcessing+CSI_IssuerType");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).InAndOutwardProcessing.CSI_IssuerType)));
        this.IssuerTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 81, true);
        this.IssuerTypeDropEdit.Name = "IssuerTypeDropEdit";
        this.IssuerTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 17, true);
        this.IssuerTypeDropEdit.TabIndex = 3;
        // 
        // DescriptionTextBox
        // 
        this.BindingSource.SetBindingMember(this.DescriptionTextBox, "InAndOutwardProcessing+CSI_Description");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).InAndOutwardProcessing.CSI_Description)));
        this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 137, true);
        this.DescriptionTextBox.Multiline = true;
        this.DescriptionTextBox.Name = "DescriptionTextBox";
        this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 61, true);
        this.DescriptionTextBox.TabIndex = 5;
        // 
        // StatusCheckBox
        // 
        this.BindingSource.SetBindingMember(this.StatusCheckBox, "InAndOutwardProcessing+Repair");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).InAndOutwardProcessing.Repair)));
        this.StatusCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 107, true);
        this.StatusCheckBox.Name = "StatusCheckBox";
        this.StatusCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 24, true);
        this.StatusCheckBox.TabIndex = 4;
        // 
        // CustomsOfficeCodeFindBox
        // 
        this.CustomsOfficeCodeFindBox.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.CustomsOfficeCodeFindBox, "InAndOutwardProcessing+CSI_CustomsOffice");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).InAndOutwardProcessing.CSI_CustomsOffice)));
        this.CustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 209, true);
        this.CustomsOfficeCodeFindBox.Name = "CustomsOfficeCodeFindBox";
        this.CustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
        this.CustomsOfficeCodeFindBox.ParentType = null;
        this.CustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 17, true);
        this.CustomsOfficeCodeFindBox.TabIndex = 22;
        // 
        // InAndOutwardProcessingFieldsUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.StatusCheckBox);
        this.Controls.Add(this.DescriptionTextBox);
        this.Controls.Add(this.IssuerTypeDropEdit);
        this.Controls.Add(this.ProcedureDropEdit);
        this.Controls.Add(this.CodeDropEdit);
        this.Controls.Add(this.SubTypeDropEdit);
        this.Controls.Add(this.CustomsOfficeCodeFindBox);
        this.Name = "InAndOutwardProcessingFieldsUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(619, 257, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.SubTypeDropEdit.ResumeLayout(true);
        this.SubTypeDropEdit.PerformLayout();
        this.CodeDropEdit.ResumeLayout(true);
        this.CodeDropEdit.PerformLayout();
        this.ProcedureDropEdit.ResumeLayout(true);
        this.ProcedureDropEdit.PerformLayout();
        this.IssuerTypeDropEdit.ResumeLayout(true);
        this.IssuerTypeDropEdit.PerformLayout();
        this.CustomsOfficeCodeFindBox.ResumeLayout(true);
        this.CustomsOfficeCodeFindBox.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.GUI.ZDropEdit SubTypeDropEdit;
    internal ZArchitecture.GUI.ZDropEdit CodeDropEdit;
    internal ZArchitecture.GUI.ZDropEdit ProcedureDropEdit;
    internal ZArchitecture.GUI.ZDropEdit IssuerTypeDropEdit;
    internal ZArchitecture.ZTextBox DescriptionTextBox;
    internal ZArchitecture.GUI.ZCheckBox StatusCheckBox;
    internal ZArchitecture.GUI.ZCodeFindBox CustomsOfficeCodeFindBox;
}
