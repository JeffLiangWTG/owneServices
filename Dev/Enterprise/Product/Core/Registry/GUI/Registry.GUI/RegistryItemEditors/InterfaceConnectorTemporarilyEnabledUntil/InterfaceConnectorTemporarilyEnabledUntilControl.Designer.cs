namespace Enterprise.Registry.GUI
{
	partial class InterfaceConnectorTemporarilyEnabledUntilControl : RegistryBusinessObjectTemplateZUserControl
	{
		private ZArchitecture.GUI.ZDateEdit EnabledUntil;

		void InitializeComponent()
		{
			this.EnabledUntil = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EnabledUntil.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.InterfaceConnectorTemporarilyEnabledUntil);
			// 
			// EnabledUntil
			// 
			this.EnabledUntil.AllowDrop = true;
			this.EnabledUntil.AutoCompleteMonthThreshold = 1;
			this.EnabledUntil.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EnabledUntil, "EnabledUntil");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.InterfaceConnectorTemporarilyEnabledUntil)(null)).EnabledUntil)));
			this.EnabledUntil.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("40795fc7-1deb-4852-a25b-956be07064e1", "Enable Until");
			this.EnabledUntil.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 9, true);
			this.EnabledUntil.Name = "EnabledUntil";
			this.EnabledUntil.TabIndex = 0;
			// 
			// InterfaceConnectorTemporarilyEnabledUntilControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EnabledUntil);
			this.Name = "InterfaceConnectorTemporarilyEnabledUntilControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 69, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EnabledUntil.ResumeLayout(true);
			this.EnabledUntil.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
