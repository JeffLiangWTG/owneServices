namespace Enterprise.Registry.GUI
{
	partial class CaptionAndHintControl : RegistryBusinessObjectTemplateZUserControl
	{
		private Enterprise.ZArchitecture.ZTextBox HintTextBox;
		private Enterprise.ZArchitecture.ZTextBox CaptionTextBox;

		void InitializeComponent()
		{
			this.HintTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CaptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CaptionAndHint);
			// 
			// HintTextBox
			// 
			this.HintTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.HintTextBox, "Hint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CaptionAndHint)(null)).Hint)));
			this.HintTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.HintTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CaptionAndHintControl|64aa3018-8a1e-4e92-ac50-54732bf84388", "Hint");
			this.HintTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 32, true);
			this.HintTextBox.Name = "HintTextBox";
			this.HintTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.HintTextBox.TabIndex = 3;
			// 
			// CaptionTextBox
			// 
			this.CaptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CaptionTextBox, "Caption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CaptionAndHint)(null)).Caption)));
			this.CaptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CaptionTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CaptionAndHintControl|a980a806-34de-4081-815f-2d6451340349", "Caption");
			this.CaptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 0, true);
			this.CaptionTextBox.Name = "CaptionTextBox";
			this.CaptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.CaptionTextBox.TabIndex = 1;
			// 
			// CaptionAndHintControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.HintTextBox);
			this.Controls.Add(this.CaptionTextBox);
			this.Name = "CaptionAndHintControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 56, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
