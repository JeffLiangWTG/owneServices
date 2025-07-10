namespace Enterprise.Customs.CA.GUI
{
	partial class CellsTissuesAndOrgansUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SuspendLayout();
			this.BindingSource.SetBindingMember(this.ExceptProcessing1CheckBox, "CA_CTO_LCO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).CA_CTO_LCO)));
			this.ExceptProcessing1CheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("DA590364-1741-49B8-9EC1-A8920B7535D4", "Lymphohematopoietic Cells and Organs");
			this.ExceptProcessing1CheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(493, 40, true);
			this.ExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 66, true);
			this.ExpiryDateEdit.TabIndex = 3;
			this.GTINNumberTextBox.TabIndex = 4;
			this.ExceptProcessing1CheckBox.TabIndex = 5;
			// 
			// IntendedUseCodeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.IntendedUseCodeDropEdit, "CA_IntendedUseCodeCTO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).CA_IntendedUseCodeCTO)));
			// 
			// CategoryDropEdit
			// 
			this.BindingSource.SetBindingMember(this.CategoryDropEdit, "CA_CategoryCTO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).CA_CategoryCTO)));
			// 
			// ActivePHIngredientsUserControl
			// 
			this.Name = "CellsTissuesAndOrgansUserControl";
			this.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
