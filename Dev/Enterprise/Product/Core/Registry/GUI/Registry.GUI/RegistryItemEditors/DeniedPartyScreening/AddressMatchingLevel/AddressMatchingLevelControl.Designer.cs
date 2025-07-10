namespace Enterprise.Registry.GUI
{
	partial class AddressMatchingLevelControl
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
		void InitializeComponent()
		{
			this.StrictRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.BalancedRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.ComprehensiveRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.AddressMatchingLevelBusinessObject);
			// 
			// StrictRadioButton
			// 
			this.StrictRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.StrictRadioButton.AutoCheck = false;
			this.StrictRadioButton.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.StrictRadioButton, "StrictRadioButtonSelection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AddressMatchingLevelBusinessObject)(null)).StrictRadioButtonSelection)));
			this.StrictRadioButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("db201ab7-4b98-4dbe-997b-6915d8a74633", "Strict");
			this.StrictRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.StrictRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 17, true);
			this.StrictRadioButton.Name = "StrictRadioButton";
			this.StrictRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(548, 34, true);
			this.StrictRadioButton.TabIndex = 0;
			this.StrictRadioButton.UseVisualStyleBackColor = false;
			// 
			// BalancedRadioButton
			// 
			this.BalancedRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BalancedRadioButton.AutoCheck = false;
			this.BalancedRadioButton.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.BalancedRadioButton, "BalancedRadioButtonSelection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AddressMatchingLevelBusinessObject)(null)).BalancedRadioButtonSelection)));
			this.BalancedRadioButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("6a34abf2-622f-44d1-9dfb-08fe2e1272b4", "Balanced");
			this.BalancedRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BalancedRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 67, true);
			this.BalancedRadioButton.Name = "BalancedRadioButton";
			this.BalancedRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(548, 34, true);
			this.BalancedRadioButton.TabIndex = 1;
			this.BalancedRadioButton.UseVisualStyleBackColor = false;
			// 
			// ComprehensiveRadioButton
			// 
			this.ComprehensiveRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ComprehensiveRadioButton.AutoCheck = false;
			this.ComprehensiveRadioButton.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.ComprehensiveRadioButton, "ComprehensiveRadioButtonSelection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AddressMatchingLevelBusinessObject)(null)).ComprehensiveRadioButtonSelection)));
			this.ComprehensiveRadioButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("1412a988-cf5a-4100-a0d7-5c1cb30a386f", "Comprehensive");
			this.ComprehensiveRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ComprehensiveRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 117, true);
			this.ComprehensiveRadioButton.Name = "ComprehensiveRadioButton";
			this.ComprehensiveRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(548, 34, true);
			this.ComprehensiveRadioButton.TabIndex = 2;
			this.ComprehensiveRadioButton.UseVisualStyleBackColor = false;
			// 
			// DpsMatchingConfigurationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.StrictRadioButton);
			this.Controls.Add(this.ComprehensiveRadioButton);
			this.Controls.Add(this.BalancedRadioButton);
			this.Name = "AddressMatchingLevelControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 171, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZRadioButton StrictRadioButton;
		protected ZArchitecture.GUI.ZRadioButton BalancedRadioButton;
		protected ZArchitecture.GUI.ZRadioButton ComprehensiveRadioButton;
	}
}
