namespace Enterprise.Registry.GUI
{
	public partial class ShipmentImportBranchRuleRegistryControl
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		void InitializeComponent()
		{
			this.useBranchFromXMLCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.branchSettingsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.fallbackRuleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.fallbackDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.defaultToDestinationDischargePortCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.defaultToOriginLoadPortCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.defaultToDestinationDischargePortLabel = new Enterprise.ZArchitecture.ZLabel();
			this.defaultToOriginLoadPortLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.branchSettingsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.ImportBranchRule);
			// 
			// useBranchFromXMLCheckBox
			// 
			this.BindingSource.SetBindingMember(this.useBranchFromXMLCheckBox, "UseBranchFromXml");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.ImportBranchRule)(null)).UseBranchFromXml)));
			this.useBranchFromXMLCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ShipmentImportBranchRuleRegistryControl|a2874be8-e634-45ce-84de-f555d5ee15e8", "Use Branch From XML");
			this.useBranchFromXMLCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.useBranchFromXMLCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 12, true);
			this.useBranchFromXMLCheckBox.Name = "useBranchFromXMLCheckBox";
			this.useBranchFromXMLCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 24, true);
			this.useBranchFromXMLCheckBox.TabIndex = 0;
			this.useBranchFromXMLCheckBox.UseVisualStyleBackColor = true;
			// 
			// branchSettingsGroupBox
			// 
			this.branchSettingsGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ShipmentImportBranchRuleRegistryControl|01d1b27a-9f6b-4d83-bc20-d12602ecd8ea", "Branch Default Order Rule when Branch in XML is not found or is to be discarded");
			this.branchSettingsGroupBox.Controls.Add(this.fallbackRuleLabel);
			this.branchSettingsGroupBox.Controls.Add(this.fallbackDropEdit);
			this.branchSettingsGroupBox.Controls.Add(this.defaultToDestinationDischargePortCalcEdit);
			this.branchSettingsGroupBox.Controls.Add(this.defaultToOriginLoadPortCalcEdit);
			this.branchSettingsGroupBox.Controls.Add(this.defaultToDestinationDischargePortLabel);
			this.branchSettingsGroupBox.Controls.Add(this.defaultToOriginLoadPortLabel);
			this.branchSettingsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 42, true);
			this.branchSettingsGroupBox.Name = "branchSettingsGroupBox";
			this.branchSettingsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 126, true);
			this.branchSettingsGroupBox.TabIndex = 1;
			this.branchSettingsGroupBox.TabStop = false;
			// 
			// fallbackRuleLabel
			// 
			this.fallbackRuleLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ShipmentImportBranchRuleRegistryControl|c46e3c37-daa7-45e3-a275-7e74949610bd", "Fallback Rule");
			this.fallbackRuleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 93, true);
			this.fallbackRuleLabel.Name = "fallbackRuleLabel";
			this.fallbackRuleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 23, true);
			this.fallbackRuleLabel.TabIndex = 8;
			// 
			// fallbackDropEdit
			// 
			this.fallbackDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.fallbackDropEdit, "FallbackRule");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.ImportBranchRule)(null)).FallbackRule)));
			this.fallbackDropEdit.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.fallbackDropEdit, false);
			this.fallbackDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 95, true);
			this.fallbackDropEdit.Name = "fallbackDropEdit";
			this.fallbackDropEdit.PreBoundMaxLength = 3;
			this.fallbackDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 20, true);
			this.fallbackDropEdit.TabIndex = 7;
			// 
			// defaultToDestinationDischargePortCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.defaultToDestinationDischargePortCalcEdit, "DefaultToBranchRelatedToDestinationDischargePort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.ImportBranchRule)(null)).DefaultToBranchRelatedToDestinationDischargePort)));
			this.defaultToDestinationDischargePortCalcEdit.CaptionResourceString = null;
			this.defaultToDestinationDischargePortCalcEdit.DecimalPlaces = 0;
			this.defaultToDestinationDischargePortCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.defaultToDestinationDischargePortCalcEdit, false);
			this.defaultToDestinationDischargePortCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 69, true);
			this.defaultToDestinationDischargePortCalcEdit.Name = "defaultToDestinationDischargePortCalcEdit";
			this.defaultToDestinationDischargePortCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.defaultToDestinationDischargePortCalcEdit.TabIndex = 5;
			this.defaultToDestinationDischargePortCalcEdit.Text = "0";
			this.defaultToDestinationDischargePortCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// defaultToOriginLoadPortCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.defaultToOriginLoadPortCalcEdit, "DefaultToBranchRelatedToOriginLoadPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.ImportBranchRule)(null)).DefaultToBranchRelatedToOriginLoadPort)));
			this.defaultToOriginLoadPortCalcEdit.CaptionResourceString = null;
			this.defaultToOriginLoadPortCalcEdit.DecimalPlaces = 0;
			this.defaultToOriginLoadPortCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.defaultToOriginLoadPortCalcEdit, false);
			this.defaultToOriginLoadPortCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 42, true);
			this.defaultToOriginLoadPortCalcEdit.Name = "defaultToOriginLoadPortCalcEdit";
			this.defaultToOriginLoadPortCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.defaultToOriginLoadPortCalcEdit.TabIndex = 4;
			this.defaultToOriginLoadPortCalcEdit.Text = "0";
			this.defaultToOriginLoadPortCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// defaultToDestinationDischargePortLabel
			// 
			this.defaultToDestinationDischargePortLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ShipmentImportBranchRuleRegistryControl|ff1d827b-d5e7-4f3f-a69b-627ab03071fa", "Default to Branch Related to Destination/Discharge Port");
			this.defaultToDestinationDischargePortLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 67, true);
			this.defaultToDestinationDischargePortLabel.Name = "defaultToDestinationDischargePortLabel";
			this.defaultToDestinationDischargePortLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 23, true);
			this.defaultToDestinationDischargePortLabel.TabIndex = 1;
			// 
			// defaultToOriginLoadPortLabel
			// 
			this.defaultToOriginLoadPortLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ShipmentImportBranchRuleRegistryControl|92ff2bcb-1721-4360-9295-7e2e487b63a8", "Default to Branch Related to Origin/Load Port");
			this.defaultToOriginLoadPortLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 40, true);
			this.defaultToOriginLoadPortLabel.Name = "defaultToOriginLoadPortLabel";
			this.defaultToOriginLoadPortLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 23, true);
			this.defaultToOriginLoadPortLabel.TabIndex = 0;
			// 
			// ShipmentImportBranchRuleRegistryControl
			// 
			this.Controls.Add(this.branchSettingsGroupBox);
			this.Controls.Add(this.useBranchFromXMLCheckBox);
			this.Name = "ShipmentImportBranchRuleRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 176, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.branchSettingsGroupBox.ResumeLayout(false);
			this.branchSettingsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		private ZArchitecture.GUI.ZCheckBox useBranchFromXMLCheckBox;
		private ZArchitecture.GUI.ZGroupBox branchSettingsGroupBox;
		private ZArchitecture.ZCalcEdit defaultToDestinationDischargePortCalcEdit;
		private ZArchitecture.ZCalcEdit defaultToOriginLoadPortCalcEdit;
		protected ZArchitecture.ZLabel defaultToDestinationDischargePortLabel;
		protected ZArchitecture.ZLabel defaultToOriginLoadPortLabel;
		private ZArchitecture.GUI.ZDropEdit fallbackDropEdit;
		private ZArchitecture.ZLabel fallbackRuleLabel;
	}
}
