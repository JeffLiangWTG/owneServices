using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class AdditionalCodeDetailControl : ZUserControl
	{
		private ZArchitecture.GUI.ZRadioButton additionalCodeRadioButton;
		private ZArchitecture.GUI.ZCodeFindBox additionalCodeCodeFindBox;

		private void InitializeComponent()
		{
			this.additionalCodeRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.additionalCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.additionalCodeRadioButton.SuspendLayout();
			this.additionalCodeCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.GuidedDecisionMakingAdditionalCode);
			// 
			// additionalCodeRadioButton
			// 
			this.additionalCodeRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.additionalCodeRadioButton, "IsTicked");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.GuidedDecisionMakingAdditionalCode)(null)).IsTicked)));
			this.additionalCodeRadioButton.Controls.Add(this.additionalCodeCodeFindBox);
			this.additionalCodeRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.additionalCodeRadioButton.Name = "additionalCodeRadioButton";
			this.additionalCodeRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(917, 19, true);
			this.additionalCodeRadioButton.TabIndex = 0;
			this.additionalCodeRadioButton.TabStop = true;
			this.additionalCodeRadioButton.UseVisualStyleBackColor = true;
			// 
			// additionalCodeCodeFindBox
			// 
			this.additionalCodeCodeFindBox.AllowDrop = true;
			this.additionalCodeCodeFindBox.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
			this.BindingSource.SetBindingMember(this.additionalCodeCodeFindBox, "AdditionalCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.GuidedDecisionMakingAdditionalCode)(null)).AdditionalCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.GuidedDecisionMakingAdditionalCode)(null)).AdditionalCodeDescription)));
			this.additionalCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 1, true);
			this.additionalCodeCodeFindBox.Name = "additionalCodeCodeFindBox";
			this.additionalCodeCodeFindBox.BindToForDescription = "AdditionalCodeDescription";
			this.additionalCodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.additionalCodeCodeFindBox.ParentType = null;
			this.additionalCodeCodeFindBox.PreBoundMaxLength = 5;
			this.additionalCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(893, 17, true);
			this.additionalCodeCodeFindBox.TabIndex = 1;
			// 
			// AdditionalCodeDetailControl
			// 
			this.Controls.Add(this.additionalCodeRadioButton);
			this.Name = "AdditionalCodeDetailControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(930, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.additionalCodeRadioButton.ResumeLayout(false);
			this.additionalCodeRadioButton.PerformLayout();
			this.additionalCodeCodeFindBox.ResumeLayout(true);
			this.additionalCodeCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
