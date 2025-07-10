namespace Enterprise.Registry.GUI
{
	internal partial class SystemDefinedOrganisationControl : RegistryBusinessObjectTemplateZUserControl
	{
		Enterprise.ZArchitecture.GUI.ZCheckBox EnableCheckBox;

		void InitializeComponent()
		{
			this.EnableCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.UnmatchedOrganisation);
			// 
			// EnableCheckBox
			// 
			this.BindingSource.SetBindingMember(this.EnableCheckBox, "IsEnabled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.UnmatchedOrganisation)(null)).IsEnabled)));
			this.EnableCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("SystemDefinedOrganisationControl|a30a3241-d4ff-477b-bfdc-9449f0a21c0d", "Enable Matching to Unmatched Organization");
			this.EnableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EnableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EnableCheckBox.Name = "EnableCheckBox";
			this.EnableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 24, true);
			this.EnableCheckBox.TabIndex = 1;
			// 
			// SystemDefinedOrganisationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EnableCheckBox);
			this.Name = "SystemDefinedOrganisationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 48, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
