using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class RadioButtonControl
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
			this.optionGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.noRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.yesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.optionGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.GUI.RadioButtonWrapper);
			// 
			// OptionGroupBox
			// 
			this.optionGroupBox.Controls.Add(this.noRadioButton);
			this.optionGroupBox.Controls.Add(this.yesRadioButton);
			this.optionGroupBox.Font = new System.Drawing.Font(OFont.NormalFontName, 8F, System.Drawing.FontStyle.Bold);
			this.optionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.optionGroupBox.Name = "optionGroupBox";
			this.optionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 57, true);
			this.optionGroupBox.TabIndex = 0;
			this.optionGroupBox.TabStop = false;
			this.optionGroupBox.Text = "Option";
			// 
			// NoRadioButton
			// 
			this.noRadioButton.AutoCheck = false;
			this.noRadioButton.AutoSize = true;
			this.noRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.noRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 24, true);
			this.noRadioButton.Name = "noRadioButton";
			this.noRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.noRadioButton.TabIndex = 3;
			this.noRadioButton.TabStop = true;
			this.noRadioButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("RegistryForm|e02cb7ac-2068-40d7-aede-bdba1e38271e", "No");
			this.noRadioButton.BackColor = System.Drawing.Color.Transparent;
			// 
			// YesRadioButton
			// 
			this.yesRadioButton.AutoCheck = false;
			this.yesRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.yesRadioButton, "Value");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.GUI.RadioButtonWrapper)(null)).Value)));
			this.yesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.yesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 24, true);
			this.yesRadioButton.Name = "yesRadioButton";
			this.yesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.yesRadioButton.TabIndex = 2;
			this.yesRadioButton.TabStop = true;
			this.yesRadioButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("RegistryForm|DE9F4DFC-1006-4378-8926-7028D672E44D", "Yes");
			this.yesRadioButton.BackColor = System.Drawing.Color.Transparent;
			// 
			// RadioButtonControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.optionGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 57, true);
			this.Name = "RadioButtonControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 57, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.optionGroupBox.ResumeLayout(false);
			this.optionGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZRadioButton noRadioButton;
		protected CargoWise.Windows.UI.KGroupBox optionGroupBox;
		protected ZArchitecture.GUI.ZRadioButton yesRadioButton;
	}
}
