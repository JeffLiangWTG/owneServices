using Enterprise.ZArchitecture;

namespace Enterprise.Customs.CA.GUI
{
	partial class RadiationEmittingDevicesUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.FDANumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SuspendLayout();
			this.CategoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 14, true);
			this.DetailsGroupBox.Controls.Add(this.FDANumberTextBox);
			// 
			// FDANumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.FDANumberTextBox, "CA_FDANumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).CA_FDANumber)));
			this.FDANumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6BF6B00C-4F67-4D28-922A-FE6C134C1EB2", "FDA", "FDA No.", "FDA Number");
			this.FDANumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 40, true);
			this.FDANumberTextBox.Name = "FDANumberTextBox";
			this.FDANumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.FDANumberTextBox.TabIndex = 15;
			this.ModelNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 66, true);
			// 
			// CategoryDropEdit
			// 
			this.BindingSource.SetBindingMember(this.CategoryDropEdit, "CA_CategoryRED");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).CA_CategoryRED)));
			// 
			// RadiationEmittingDevicesUserControl
			// 
			this.Name = "RadiationEmittingDevicesUserControl";
			this.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		internal ZTextBox FDANumberTextBox;
	}
}
