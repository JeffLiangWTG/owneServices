namespace Enterprise.Customs.IN.Manifest.GUI;

partial class MessageAndCustomsStatusWithOverrideUserControl
{
    void InitializeComponent()
    {
        this.StatusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
        this.CustomsStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.MessageStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.OverrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.StatusGroupBox.SuspendLayout();
        this.CustomsStatusDropEdit.SuspendLayout();
        this.MessageStatusDropEdit.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Manifest.Business.CGMAsycudaManifestHeader);
        // 
        // StatusGroupBox
        // 
        this.StatusGroupBox.BackColor = System.Drawing.Color.Transparent;
        this.StatusGroupBox.Controls.Add(this.CustomsStatusDropEdit);
        this.StatusGroupBox.Controls.Add(this.MessageStatusDropEdit);
        this.StatusGroupBox.Controls.Add(this.OverrideCheckBox);
        this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.StatusGroupBox, false);
        this.StatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 1, true);
        this.StatusGroupBox.Name = "StatusGroupBox";
        this.StatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 74, true);
        this.StatusGroupBox.TabIndex = 0;
        this.StatusGroupBox.TabStop = false;
        // 
        // CustomsStatusDropEdit
        // 
        this.CustomsStatusDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.CustomsStatusDropEdit, "RegistrationStatus");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Manifest.Business.CGMAsycudaManifestHeader)(null)).RegistrationStatus)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Manifest.Business.CGMAsycudaManifestHeader)(null)).RegistrationStatusDescription)));
        this.CustomsStatusDropEdit.BindToForDescription = "RegistrationStatusDescription";
        this.CustomsStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 45, true);
        this.CustomsStatusDropEdit.Name = "CustomsStatusDropEdit";
        this.CustomsStatusDropEdit.PreBoundMaxLength = 3;
        this.CustomsStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
        this.CustomsStatusDropEdit.TabIndex = 3;
        // 
        // MessageStatusDropEdit
        // 
        this.MessageStatusDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.MessageStatusDropEdit, "AMA_MessageStatus");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Manifest.Business.CGMAsycudaManifestHeader)(null)).AMA_MessageStatus)));
        this.MessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 19, true);
        this.MessageStatusDropEdit.Name = "MessageStatusDropEdit";
        this.MessageStatusDropEdit.PreBoundMaxLength = 3;
        this.MessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
        this.MessageStatusDropEdit.TabIndex = 2;
        // 
        // OverrideCheckBox
        // 
        this.OverrideCheckBox.AutoSize = true;
        this.OverrideCheckBox.BackColor = System.Drawing.SystemColors.Control;
        this.BindingSource.SetBindingMember(this.OverrideCheckBox, "StatusOverride");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IN.Manifest.Business.CGMAsycudaManifestHeader)(null)).StatusOverride)));
        this.OverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 0, true);
        this.OverrideCheckBox.Name = "OverrideCheckBox";
        this.OverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 17, true);
        this.OverrideCheckBox.TabIndex = 1;
        this.OverrideCheckBox.UseVisualStyleBackColor = false;
        // 
        // MessageAndCustomsStatusWithOverrideUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.BackColor = System.Drawing.SystemColors.Control;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.StatusGroupBox);
        this.Name = "MessageAndCustomsStatusWithOverrideUserControl";
        this.ShouldSerializeTabPageMethods = false;
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 77, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.StatusGroupBox.ResumeLayout(false);
        this.StatusGroupBox.PerformLayout();
        this.CustomsStatusDropEdit.ResumeLayout(true);
        this.CustomsStatusDropEdit.PerformLayout();
        this.MessageStatusDropEdit.ResumeLayout(true);
        this.MessageStatusDropEdit.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    private ZArchitecture.GUI.ZGroupBox StatusGroupBox;
    private ZArchitecture.GUI.ZCheckBox OverrideCheckBox;
    private ZArchitecture.GUI.ZDropEdit CustomsStatusDropEdit;
    private ZArchitecture.GUI.ZDropEdit MessageStatusDropEdit;
}
