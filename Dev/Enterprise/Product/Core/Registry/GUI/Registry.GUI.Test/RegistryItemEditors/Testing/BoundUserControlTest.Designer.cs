namespace Enterprise.Registry.GUI.Testing
{
	sealed partial class BoundUserControlTest : RegistryZUserControl
	{
		public Enterprise.ZArchitecture.ZTextBox TextBox;

		void InitializeComponent()
		{
            this.TextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Registry.GUI.Testing.DummyBusinessObject);
            // 
            // TextBox
            // 
            this.BindingSource.SetBindingMember(this.TextBox, "Property");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Registry.GUI.Testing.DummyBusinessObject)(null)).Property)));
            this.TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
            this.TextBox.Name = "TextBox";
            this.TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
            this.TextBox.TabIndex = 0;
            // 
            // BoundUserControlTest
            // 
            this.Controls.Add(this.TextBox);
            this.Name = "BoundUserControlTest";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 27, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
	}
}
