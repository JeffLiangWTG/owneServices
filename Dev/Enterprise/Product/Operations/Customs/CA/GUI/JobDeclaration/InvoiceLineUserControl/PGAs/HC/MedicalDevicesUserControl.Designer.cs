using Enterprise.ZArchitecture;

namespace Enterprise.Customs.CA.GUI
{
	partial class MedicalDevicesUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.UniqueDeviceIDNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SuspendLayout();
			this.DetailsGroupBox.Controls.Add(this.UniqueDeviceIDNumberTextBox);
			this.ManufactureDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 66, true);
			this.BindingSource.SetBindingMember(this.ExceptProcessing1CheckBox, "CA_MDE_LEX");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).CA_MDE_LEX)));
			this.ExceptProcessing1CheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("74C36761-BD00-4B7F-9FBF-3E1E36A1D1B9", "Medical Device Establishment License Exemption");
			this.ModelNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(493, 92, true);
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1070, 132, true);
			// 
			// IntendedUseCodeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.IntendedUseCodeDropEdit, "CA_IntendedUseCodeMDE");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).CA_IntendedUseCodeMDE)));
			// 
			// CategoryDropEdit
			// 
			this.BindingSource.SetBindingMember(this.CategoryDropEdit, "CA_CategoryMDE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).CA_CategoryMDE)));
			// 
			// UniqueDeviceIDNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.UniqueDeviceIDNumberTextBox, "CA_UniqueDeviceIDNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).CA_UniqueDeviceIDNumber)));
			this.UniqueDeviceIDNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("EB65A029-4081-4EE4-B163-2542386E2143", "Unique Device", "Unique Device ID", "Unique device ID number");
			this.UniqueDeviceIDNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 66, true);
			this.UniqueDeviceIDNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 20, true);
			this.UniqueDeviceIDNumberTextBox.Name = "UniqueDeviceIDNumberTextBox";
			this.ManufactureDateEdit.TabIndex = 3;
			this.UniqueDeviceIDNumberTextBox.TabIndex = 4;
			this.ExceptProcessing1CheckBox.TabIndex = 5;
			this.GTINNumberTextBox.TabIndex = 6;
			this.BrandNameTextBox.TabIndex = 7;
			this.BatchLotNumberTextBox.TabIndex = 8;
			this.ModelNameTextBox.TabIndex = 9;
			// 
			// MedicalDevicesUserControl
			// 
			this.Name = "MedicalDevicesUserControl";
			this.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		internal ZTextBox UniqueDeviceIDNumberTextBox;
	}
}
