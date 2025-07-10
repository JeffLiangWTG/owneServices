namespace Enterprise.Customs.IN.Manifest.GUI;

partial class CGMTransshipmentDetailsUserControl
{
	void InitializeComponent()
	{
		this.InlandTransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		this.TransshipmentDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
		this.LocalTransportCarrierOrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
		this.CarrierCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.InlandTransportModeDropEdit.SuspendLayout();
		this.TransshipmentDetailsGroupBox.SuspendLayout();
		this.LocalTransportCarrierOrganisationFindBox.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Manifest.Business.CGMAsycudaBill);
		// 
		// InlandTransportModeDropEdit
		// 
		this.InlandTransportModeDropEdit.AllowDrop = true;
		this.InlandTransportModeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
		this.BindingSource.SetBindingMember(this.InlandTransportModeDropEdit, "ABL_InlandTransportMode");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Manifest.Business.CGMAsycudaBill)(null)).ABL_InlandTransportMode)));
		this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.InlandTransportModeDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
		this.InlandTransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 15, true);
		this.InlandTransportModeDropEdit.Name = "InlandTransportModeDropEdit";
		this.InlandTransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 21, true);
		this.InlandTransportModeDropEdit.TabIndex = 2;
		// 
		// TransshipmentDetailsGroupBox
		// 
		this.TransshipmentDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
		this.TransshipmentDetailsGroupBox.CaptionResourceString = Enterprise.Customs.IN.Manifest.GUI.Res.GetData("f5c31ea1-db0f-4be8-85fc-80b421cb738b", "Transshipment Details");
		this.TransshipmentDetailsGroupBox.Controls.Add(this.LocalTransportCarrierOrganisationFindBox);
		this.TransshipmentDetailsGroupBox.Controls.Add(this.CarrierCodeTextBox);
		this.TransshipmentDetailsGroupBox.Controls.Add(this.InlandTransportModeDropEdit);
		this.TransshipmentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 2, true);
		this.TransshipmentDetailsGroupBox.Name = "TransshipmentDetailsGroupBox";
		this.TransshipmentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 92, true);
		this.TransshipmentDetailsGroupBox.TabIndex = 30;
		this.TransshipmentDetailsGroupBox.TabStop = false;
		// 
		// LocalTransportCarrierOrganisationFindBox
		// 
		this.LocalTransportCarrierOrganisationFindBox.AllowDrop = true;
		this.LocalTransportCarrierOrganisationFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
		this.BindingSource.SetBindingMember(this.LocalTransportCarrierOrganisationFindBox, "ABL_OH_LocalTransportCarrier");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.IN.Manifest.Business.CGMAsycudaBill)(null)).ABL_OH_LocalTransportCarrier)));
		this.LocalTransportCarrierOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 40, true);
		this.LocalTransportCarrierOrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
		this.LocalTransportCarrierOrganisationFindBox.Name = "LocalTransportCarrierOrganisationFindBox";
		this.LocalTransportCarrierOrganisationFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
		this.LocalTransportCarrierOrganisationFindBox.ParentType = null;
		this.LocalTransportCarrierOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 21, true);
		this.LocalTransportCarrierOrganisationFindBox.TabIndex = 0;
		// 
		// CarrierCodeTextBox
		// 
		this.CarrierCodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
		this.BindingSource.SetBindingMember(this.CarrierCodeTextBox, "CarrierCode");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Manifest.Business.CGMAsycudaBill)(null)).CarrierCode)));
		this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.CarrierCodeTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
		this.CarrierCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 65, true);
		this.CarrierCodeTextBox.Name = "CarrierCodeTextBox";
		this.CarrierCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 21, true);
		this.CarrierCodeTextBox.TabIndex = 1;
		// 
		// CGMTransshipmentDetailsUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.BackColor = System.Drawing.SystemColors.Control;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.TransshipmentDetailsGroupBox);
		this.Name = "CGMTransshipmentDetailsUserControl";
		this.ShouldSerializeTabPageMethods = false;
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 96, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.InlandTransportModeDropEdit.ResumeLayout(true);
		this.InlandTransportModeDropEdit.PerformLayout();
		this.TransshipmentDetailsGroupBox.ResumeLayout(false);
		this.TransshipmentDetailsGroupBox.PerformLayout();
		this.LocalTransportCarrierOrganisationFindBox.ResumeLayout(true);
		this.LocalTransportCarrierOrganisationFindBox.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	internal ZArchitecture.GUI.ZDropEdit InlandTransportModeDropEdit;
	private ZArchitecture.GUI.ZGroupBox TransshipmentDetailsGroupBox;
	internal ZArchitecture.ZTextBox CarrierCodeTextBox;
	internal MasterFiles.GUI.ZOrganisationFindBox LocalTransportCarrierOrganisationFindBox;
}
