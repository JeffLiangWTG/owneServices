namespace Enterprise.Customs.CH.GUI;

partial class EntryInstructionDetailPanelUserControl
{
    #region Component Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.DeclarationReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.PartialDeliveryCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
        this.TransportChargesMethodOfPaymentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.ProcedureCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.NextProcedureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.DeclarationReasonDropEdit.SuspendLayout();
        this.PartialDeliveryCheckBox.SuspendLayout();
        this.TransportChargesMethodOfPaymentDropEdit.SuspendLayout();
        this.ProcedureCodeDropEdit.SuspendLayout();
        this.NextProcedureDropEdit.SuspendLayout();
        this.SuspendLayout();
        //
        // BindingSource
        //
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.CusEntryInstruction);
        //
        // CEI_DeclarationReasonDropEdit
        //
        this.DeclarationReasonDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.DeclarationReasonDropEdit, "CEI_DeclarationReason");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_DeclarationReason)));
        this.DeclarationReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 86, true);
        this.DeclarationReasonDropEdit.Name = "DeclarationReasonDropEdit";
        this.DeclarationReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
        this.DeclarationReasonDropEdit.TabIndex = 2;
        //
        // ProcedureCodeDropEdit
        //
        this.ProcedureCodeDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.ProcedureCodeDropEdit, "CEI_Procedure");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_Procedure)));
        this.ProcedureCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 86, true);
        this.ProcedureCodeDropEdit.Name = "ProcedureCodeDropEdit";
        this.ProcedureCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
        this.ProcedureCodeDropEdit.TabIndex = 3;
        //
        // CEI_PartialDeliveryCheckBox
        //
        this.BindingSource.SetBindingMember(this.PartialDeliveryCheckBox, "CEI_PartialDelivery");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_PartialDelivery)));
        this.PartialDeliveryCheckBox.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("5A4A4055-10A4-4D29-986B-0D9EF65F9231", "Partial Delivery");
        this.PartialDeliveryCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 86, true);
        this.PartialDeliveryCheckBox.Name = "PartialDeliveryCheckBox";
        this.PartialDeliveryCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 24, true);
        this.PartialDeliveryCheckBox.TabIndex = 4;
        //
        // TransportChargesMethodOfPaymentDropEdit
        //
        this.TransportChargesMethodOfPaymentDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.TransportChargesMethodOfPaymentDropEdit, "CEI_TransportChargesMethodOfPayment");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_TransportChargesMethodOfPayment)));
        this.TransportChargesMethodOfPaymentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 86, true);
        this.TransportChargesMethodOfPaymentDropEdit.Name = "TransportChargesMethodOfPaymentDropEdit";
        this.TransportChargesMethodOfPaymentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
        this.TransportChargesMethodOfPaymentDropEdit.TabIndex = 5;
        //
        // NextProcedureDropEdit
        //
        this.NextProcedureDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.NextProcedureDropEdit, "CEI_NextProcedure");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_NextProcedure)));
        this.NextProcedureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 86, true);
        this.NextProcedureDropEdit.Name = "NextProcedureDropEdit";
        this.NextProcedureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
        this.NextProcedureDropEdit.TabIndex = 4;
        //
        // EntryInstructionDetailsUserControl
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.DeclarationReasonDropEdit);
        this.Controls.Add(this.PartialDeliveryCheckBox);
        this.Controls.Add(this.TransportChargesMethodOfPaymentDropEdit);
        this.Controls.Add(this.ProcedureCodeDropEdit);
        this.Controls.Add(this.NextProcedureDropEdit);
        this.Name = "EntryInstructionDetailPanelUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 385, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.DeclarationReasonDropEdit.ResumeLayout(true);
        this.DeclarationReasonDropEdit.PerformLayout();
        this.PartialDeliveryCheckBox.ResumeLayout(true);
        this.PartialDeliveryCheckBox.PerformLayout();
        this.TransportChargesMethodOfPaymentDropEdit.ResumeLayout(true);
        this.TransportChargesMethodOfPaymentDropEdit.PerformLayout();
        this.ProcedureCodeDropEdit.ResumeLayout(true);
        this.ProcedureCodeDropEdit.PerformLayout();
        this.NextProcedureDropEdit.ResumeLayout(true);
        this.NextProcedureDropEdit.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }
    #endregion

    internal ZArchitecture.GUI.ZDropEdit DeclarationReasonDropEdit;
    internal ZArchitecture.GUI.ZCheckBox PartialDeliveryCheckBox;
    internal ZArchitecture.GUI.ZDropEdit TransportChargesMethodOfPaymentDropEdit;
    internal ZArchitecture.GUI.ZDropEdit ProcedureCodeDropEdit;
    internal ZArchitecture.GUI.ZDropEdit NextProcedureDropEdit;

}
