namespace Enterprise.Customs.CA.GUI
{
	partial class NaturalHealthProductsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SuspendLayout();
			this.ManufactureDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 66, true);
			this.ManufactureDateEdit.TabIndex = 3;
			this.GTINNumberTextBox.TabIndex = 4;
			this.BrandNameTextBox.TabIndex = 5;
			this.BatchLotNumberTextBox.TabIndex = 6;
			// 
			// IntendedUseCodeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.IntendedUseCodeDropEdit, "CA_IntendedUseCodeNHP");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).CA_IntendedUseCodeNHP)));
			// 
			// CategoryDropEdit
			// 
			this.BindingSource.SetBindingMember(this.CategoryDropEdit, "CA_CategoryNHP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).CA_CategoryNHP)));
			// 
			// NaturalHealthProductsUserControl
			// 
			this.Name = "NaturalHealthProductsUserControl";
			this.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
