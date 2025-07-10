namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class RegistrationCodeUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.CountryFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CustomTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CountryFindBox.SuspendLayout();
			this.CustomTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.RuntimeOptions.RegistrationCodeField);
			// 
			// CountryFindBox
			// 
			this.CountryFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryFindBox, "CodeCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.RuntimeOptions.RegistrationCodeField)(null)).CodeCountry)));
			this.CountryFindBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("RegistrationCodeUserControl|8d6d3b40-2138-4902-b9a8-b9c4b7278238", "Code Country/Region");
			this.CountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 0, true);
			this.CountryFindBox.Name = "CountryFindBox";
			this.CountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 21, true);
			this.CountryFindBox.TabIndex = 1;
			// 
			// CustomTypeDropEdit
			// 
			this.CustomTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomTypeDropEdit, "CustomType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.RuntimeOptions.RegistrationCodeField)(null)).CustomType)));
			this.CustomTypeDropEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("RegistrationCodeUserControl|7dd27df4-b4fd-497d-9976-0b6b5426427a", "Code Type");
			this.CustomTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 26, true);
			this.CustomTypeDropEdit.Name = "CustomTypeDropEdit";
			this.CustomTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 20, true);
			this.CustomTypeDropEdit.TabIndex = 1;
			// 
			// RegistrationCodeUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CountryFindBox);
			this.Controls.Add(this.CustomTypeDropEdit);
			this.Name = "RegistrationCodeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(497, 52, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CountryFindBox.ResumeLayout(true);
			this.CountryFindBox.PerformLayout();
			this.CustomTypeDropEdit.ResumeLayout(true);
			this.CustomTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox CountryFindBox;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit CustomTypeDropEdit;
	}
}
