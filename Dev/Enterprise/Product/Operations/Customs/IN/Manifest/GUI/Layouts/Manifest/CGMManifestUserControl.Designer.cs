namespace Enterprise.Customs.IN.Manifest.GUI;

partial class CGMManifestUserControl
{
	void InitializeComponent()
	{
		this.ImportGeneralManifestDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
		this.ImportGeneralManifestNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.GrossWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
		this.ManifestQtyCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
		this.MessageAndCustomsStatusWithOverrideUserControl = new Enterprise.Customs.IN.Manifest.GUI.MessageAndCustomsStatusWithOverrideUserControl();
		this.ActionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.ImportGeneralManifestDateEdit.SuspendLayout();
		this.GrossWeightCalcDropEdit.SuspendLayout();
		this.ManifestQtyCalcDropEdit.SuspendLayout();
		this.MessageAndCustomsStatusWithOverrideUserControl.SuspendLayout();
		this.ActionDropEdit.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Manifest.Business.CGMAsycudaManifestHeader);
		// 
		// ImportGeneralManifestDateEdit
		// 
		this.ImportGeneralManifestDateEdit.AllowDrop = true;
		this.ImportGeneralManifestDateEdit.AutoCompleteMonthThreshold = 1;
		this.BindingSource.SetBindingMember(this.ImportGeneralManifestDateEdit, "ImportGeneralManifestDate");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Manifest.Business.CGMAsycudaManifestHeader)(null)).ImportGeneralManifestDate)));
		this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.ImportGeneralManifestDateEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
		this.ImportGeneralManifestDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 0, true);
		this.ImportGeneralManifestDateEdit.Name = "ImportGeneralManifestDateEdit";
		this.ImportGeneralManifestDateEdit.TabIndex = 30;
		// 
		// ImportGeneralManifestNumberTextBox
		// 
		this.BindingSource.SetBindingMember(this.ImportGeneralManifestNumberTextBox, "ImportGeneralManifestNumber");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Manifest.Business.CGMAsycudaManifestHeader)(null)).ImportGeneralManifestNumber)));
		this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.ImportGeneralManifestNumberTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
		this.ImportGeneralManifestNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 32, true);
		this.ImportGeneralManifestNumberTextBox.Name = "ImportGeneralManifestNumberTextBox";
		this.ImportGeneralManifestNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 20, true);
		this.ImportGeneralManifestNumberTextBox.TabIndex = 29;
		// 
		// GrossWeightCalcDropEdit
		// 
		this.GrossWeightCalcDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.GrossWeightCalcDropEdit, ".");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IN.Manifest.Business.CGMAsycudaManifestHeader)(null)).GrossWeight)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Manifest.Business.CGMAsycudaManifestHeader)(null)).GrossWeightUQ)));
		this.GrossWeightCalcDropEdit.BindToAmount = "GrossWeight";
		this.GrossWeightCalcDropEdit.BindToUnit = "GrossWeightUQ";
		this.GrossWeightCalcDropEdit.Decimals = 3;
		this.GrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 75, true);
		this.GrossWeightCalcDropEdit.Name = "GrossWeightCalcDropEdit";
		this.GrossWeightCalcDropEdit.ShowDescriptionBox = true;
		this.GrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
		this.GrossWeightCalcDropEdit.TabIndex = 49;
		this.GrossWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
		// 
		// ManifestQtyCalcDropEdit
		// 
		this.ManifestQtyCalcDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.ManifestQtyCalcDropEdit, ".");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IN.Manifest.Business.CGMAsycudaManifestHeader)(null)).ManifestQty)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Manifest.Business.CGMAsycudaManifestHeader)(null)).ManifestUQ)));
		this.ManifestQtyCalcDropEdit.BindToAmount = "ManifestQty";
		this.ManifestQtyCalcDropEdit.BindToUnit = "ManifestUQ";
		this.ManifestQtyCalcDropEdit.Decimals = 0;
		this.ManifestQtyCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 123, true);
		this.ManifestQtyCalcDropEdit.MaxValue = 99999999m;
		this.ManifestQtyCalcDropEdit.Name = "ManifestQtyCalcDropEdit";
		this.ManifestQtyCalcDropEdit.ShowDescriptionBox = true;
		this.ManifestQtyCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
		this.ManifestQtyCalcDropEdit.TabIndex = 50;
		this.ManifestQtyCalcDropEdit.UnitPreBoundMaxLength = 2;
		// 
		// MessageAndCustomsStatusWithOverrideUserControl
		// 
		this.MessageAndCustomsStatusWithOverrideUserControl.AllowDrop = true;
		this.MessageAndCustomsStatusWithOverrideUserControl.AutoSize = true;
		this.MessageAndCustomsStatusWithOverrideUserControl.BackColor = System.Drawing.Color.Transparent;
		this.BindingSource.SetBindingMember(this.MessageAndCustomsStatusWithOverrideUserControl, ".");
		this.MessageAndCustomsStatusWithOverrideUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 159, true);
		this.MessageAndCustomsStatusWithOverrideUserControl.Name = "MessageAndCustomsStatusWithOverrideUserControl";
		this.MessageAndCustomsStatusWithOverrideUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 77, true);
		this.MessageAndCustomsStatusWithOverrideUserControl.TabIndex = 51;
		// 
		// ActionDropEdit
		// 
		this.ActionDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.ActionDropEdit, "Action");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Manifest.Business.CGMAsycudaManifestHeader)(null)).Action)));
		this.ActionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 292, true);
		this.ActionDropEdit.Name = "ActionDropEdit";
		this.ActionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
		// 
		// CGMManifestUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.BackColor = System.Drawing.SystemColors.Control;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.MessageAndCustomsStatusWithOverrideUserControl);
		this.Controls.Add(this.ImportGeneralManifestDateEdit);
		this.Controls.Add(this.ImportGeneralManifestNumberTextBox);
		this.Controls.Add(this.GrossWeightCalcDropEdit);
		this.Controls.Add(this.ManifestQtyCalcDropEdit);
		this.Controls.Add(this.ActionDropEdit);
		this.Name = "CGMManifestUserControl";
		this.ShouldSerializeTabPageMethods = false;
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 253, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.ImportGeneralManifestDateEdit.ResumeLayout(true);
		this.ImportGeneralManifestDateEdit.PerformLayout();
		this.GrossWeightCalcDropEdit.ResumeLayout(true);
		this.GrossWeightCalcDropEdit.PerformLayout();
		this.ManifestQtyCalcDropEdit.ResumeLayout(true);
		this.ManifestQtyCalcDropEdit.PerformLayout();
		this.MessageAndCustomsStatusWithOverrideUserControl.ResumeLayout(true);
		this.MessageAndCustomsStatusWithOverrideUserControl.PerformLayout();
		this.ActionDropEdit.ResumeLayout(true);
		this.ActionDropEdit.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	internal ZArchitecture.GUI.ZDateEdit ImportGeneralManifestDateEdit;
	internal ZArchitecture.ZTextBox ImportGeneralManifestNumberTextBox;
	internal ZArchitecture.GUI.ZCalcDropEdit GrossWeightCalcDropEdit;
	internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit ManifestQtyCalcDropEdit;
	internal Enterprise.Customs.IN.Manifest.GUI.MessageAndCustomsStatusWithOverrideUserControl MessageAndCustomsStatusWithOverrideUserControl;
	internal Enterprise.ZArchitecture.GUI.ZDropEdit ActionDropEdit;
}
