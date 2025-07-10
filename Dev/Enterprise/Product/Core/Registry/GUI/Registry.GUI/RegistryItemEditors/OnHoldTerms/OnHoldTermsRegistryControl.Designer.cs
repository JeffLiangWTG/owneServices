namespace Enterprise.Registry.GUI
{
	public partial class OnHoldTermsRegistryControl : RegistryBusinessObjectTemplateZUserControl
	{
		ZArchitecture.GUI.ZGroupBox OnHoldTermsGroupBox;
		ZArchitecture.GUI.ZDropEdit TermsDropDown;
		ZArchitecture.ZCalcEdit TermDaysTextBox;

		void InitializeComponent()
		{
			this.OnHoldTermsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TermsDropDown = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TermDaysTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OnHoldTermsGroupBox.SuspendLayout();
			this.TermsDropDown.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OnHoldTerms);
			// 
			// OnHoldTermsGroupBox
			// 
			this.OnHoldTermsGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("a84f4ec3-9d29-4213-8b19-e1f99a93b3e6", "On Hold Terms and Days");
			this.OnHoldTermsGroupBox.Controls.Add(this.TermsDropDown);
			this.OnHoldTermsGroupBox.Controls.Add(this.TermDaysTextBox);
			this.OnHoldTermsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OnHoldTermsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OnHoldTermsGroupBox.Name = "OnHoldTermsGroupBox";
			this.OnHoldTermsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 100, true);
			this.OnHoldTermsGroupBox.TabIndex = 1;
			this.OnHoldTermsGroupBox.TabStop = false;
			// 
			// TermsDropDown
			// 
			this.TermsDropDown.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TermsDropDown, "Terms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OnHoldTerms)(null)).Terms)));
			this.TermsDropDown.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("9f53e708-e040-4f9a-af76-29e8c795b854", "Terms");
			this.TermsDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 21, true);
			this.TermsDropDown.Name = "TermsDropDown";
			this.TermsDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 17, true);
			this.TermsDropDown.TabIndex = 0;
			// 
			// TermDaysTextBox
			// 
			this.BindingSource.SetBindingMember(this.TermDaysTextBox, "TermDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OnHoldTerms)(null)).TermDays)));
			this.TermDaysTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("17d6f965-55b8-4642-a352-f87feffed633", "Term Days");
			this.TermDaysTextBox.DecimalPlaces = 0;
			this.TermDaysTextBox.Decimals = 0;
			this.TermDaysTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 56, true);
			this.TermDaysTextBox.Name = "TermDaysTextBox";
			this.TermDaysTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 17, true);
			this.TermDaysTextBox.TabIndex = 1;
			this.TermDaysTextBox.Text = "0";
			this.TermDaysTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OnHoldTermsRegistryControl
			// 
			this.Controls.Add(this.OnHoldTermsGroupBox);
			this.Name = "OnHoldTermsRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 100, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OnHoldTermsGroupBox.ResumeLayout(false);
			this.OnHoldTermsGroupBox.PerformLayout();
			this.TermsDropDown.ResumeLayout(true);
			this.TermsDropDown.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
