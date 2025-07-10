namespace Enterprise.Registry.GUI
{
	partial class PacklineWeightDistributionControl : RegistryBusinessObjectTemplateZUserControl
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
			this.OptionGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.NoRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.YesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.EnableActualWeightDistributionCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EnableVolumetricWeightDistributionCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OptionGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.PacklineWeightDistributionConfiguration);
			// 
			// OptionGroupBox
			// 
			this.OptionGroupBox.Controls.Add(this.NoRadioButton);
			this.OptionGroupBox.Controls.Add(this.YesRadioButton);
			this.OptionGroupBox.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
			this.OptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OptionGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 3, 3, true);
			this.OptionGroupBox.Name = "OptionGroupBox";
			this.OptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 37, true);
			this.OptionGroupBox.TabIndex = 1;
			this.OptionGroupBox.TabStop = false;
			// 
			// NoRadioButton
			// 
			this.NoRadioButton.AutoCheck = false;
			this.NoRadioButton.AutoSize = true;
			this.NoRadioButton.BackColor = System.Drawing.Color.Transparent;
			this.NoRadioButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("632B6072-1E1B-453B-8284-991F4A3CB55D", "No");
			this.NoRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 13, true);
			this.NoRadioButton.Name = "NoRadioButton";
			this.NoRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 15, true);
			this.NoRadioButton.TabIndex = 2;
			this.NoRadioButton.TabStop = true;
			this.NoRadioButton.UseVisualStyleBackColor = false;
			this.NoRadioButton.Click += new System.EventHandler(this.NoRadioButton_Checked);
			// 
			// YesRadioButton
			// 
			this.YesRadioButton.AutoCheck = false;
			this.YesRadioButton.AutoSize = true;
			this.YesRadioButton.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.YesRadioButton, "EnablePacklineWeightDistribution");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.PacklineWeightDistributionConfiguration)(null)).EnablePacklineWeightDistribution)));
			this.YesRadioButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ECDD2028-F46C-4823-9CA0-3C40891413FC", "Yes");
			this.YesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 13, true);
			this.YesRadioButton.Name = "YesRadioButton";
			this.YesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 15, true);
			this.YesRadioButton.TabIndex = 1;
			this.YesRadioButton.TabStop = true;
			this.YesRadioButton.UseVisualStyleBackColor = false;
			this.YesRadioButton.Click += new System.EventHandler(this.YesRadioButton_Checked);

			// 
			// EnableActualWeightDistributionCheckBox
			// 
			this.EnableActualWeightDistributionCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.EnableActualWeightDistributionCheckBox, "EnableActualWeightDistribution");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.PacklineWeightDistributionConfiguration)(null)).EnableActualWeightDistribution)));
			this.EnableActualWeightDistributionCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("86243F90-6B4C-4806-A4A2-C6FEB443D10A", "Actual Weight Distribution");
			this.EnableActualWeightDistributionCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EnableActualWeightDistributionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 80, true);
			this.EnableActualWeightDistributionCheckBox.Name = "EnableActualWeightDistributionCheckBox";
			this.EnableActualWeightDistributionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 17, true);
			this.EnableActualWeightDistributionCheckBox.TabIndex = 0;
			this.EnableActualWeightDistributionCheckBox.UseVisualStyleBackColor = true;
			this.EnableActualWeightDistributionCheckBox.CheckedChanged += new System.EventHandler(this.CheckBox_CheckedChanged);
			// 
			// EnableVolumetricWeightDistributionCheckBox
			// 
			this.EnableVolumetricWeightDistributionCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.EnableVolumetricWeightDistributionCheckBox, "EnableVolumetricWeightDistribution");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.PacklineWeightDistributionConfiguration)(null)).EnableVolumetricWeightDistribution)));
			this.EnableVolumetricWeightDistributionCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("560E5DA8-8536-466B-8AF9-53E22A997C27", "Volumetric Weight Distribution");
			this.EnableVolumetricWeightDistributionCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EnableVolumetricWeightDistributionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 100, true);
			this.EnableVolumetricWeightDistributionCheckBox.Name = "EnableVolumetricWeightDistributionCheckBox";
			this.EnableVolumetricWeightDistributionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 17, true);
			this.EnableVolumetricWeightDistributionCheckBox.TabIndex = 0;
			this.EnableVolumetricWeightDistributionCheckBox.UseVisualStyleBackColor = true;
			this.EnableVolumetricWeightDistributionCheckBox.CheckedChanged += new System.EventHandler(this.CheckBox_CheckedChanged);
			// 
			// EnableComplianceWiseRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OptionGroupBox);
			this.Controls.Add(this.EnableActualWeightDistributionCheckBox);
			this.Controls.Add(this.EnableVolumetricWeightDistributionCheckBox);
			this.Name = "PacklineWeightDistributionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 171, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OptionGroupBox.ResumeLayout(false);
			this.OptionGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal CargoWise.Windows.UI.KGroupBox OptionGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZRadioButton NoRadioButton;
		internal Enterprise.ZArchitecture.GUI.ZRadioButton YesRadioButton;
		internal ZArchitecture.GUI.ZCheckBox EnableActualWeightDistributionCheckBox;
		internal ZArchitecture.GUI.ZCheckBox EnableVolumetricWeightDistributionCheckBox;
	}
}
