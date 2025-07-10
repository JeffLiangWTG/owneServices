namespace Enterprise.Registry.GUI
{
	partial class AutoRatingRequiredFieldsWithIncotermControl : AutoRatingRequiredFieldsControl
	{
		Enterprise.ZArchitecture.GUI.ZCheckBox IncotermCheckBox;

		void InitializeComponent()
		{
			this.IncotermCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// IncotermCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IncotermCheckBox, "RequireIncoterm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AutoRatingRequiredFields)(null)).RequireIncoterm)));
			this.IncotermCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IncotermCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AutoRatingRequiredFieldsWithIncotermControl|11c7ce6a-66b0-d7a2-456c-169104f729d6", "Incoterm");
			this.IncotermCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncotermCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 128, true);
			this.IncotermCheckBox.Name = "IncotermCheckBox";
			this.IncotermCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 24, true);
			this.IncotermCheckBox.TabIndex = 4;
			// 
			// AutoRatingRequiredFieldsWithIncotermControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IncotermCheckBox);
			this.Name = "AutoRatingRequiredFieldsWithIncotermControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 152, true);
			this.Controls.SetChildIndex(this.IncotermCheckBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
