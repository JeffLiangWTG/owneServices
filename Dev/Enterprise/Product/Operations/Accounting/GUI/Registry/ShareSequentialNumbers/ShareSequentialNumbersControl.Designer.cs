namespace Enterprise.Accounting.Registry.GUI
{
	partial class ShareSequentialNumbersControl
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
			this.NoShareSequentialNumbers = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.YesShareSequentialNumbers = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OptionGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.ShareSequentialTransactionNumbers);
			// 
			// OptionGroupBox
			// 
			this.OptionGroupBox.Controls.Add(this.NoShareSequentialNumbers);
			this.OptionGroupBox.Controls.Add(this.YesShareSequentialNumbers);
			this.OptionGroupBox.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
			this.OptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OptionGroupBox.Name = "OptionGroupBox";
			this.OptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 57, true);
			this.OptionGroupBox.TabIndex = 0;
			this.OptionGroupBox.TabStop = false;
			this.OptionGroupBox.Text = "Option";
			// 
			// NoShareSequentialNumbers
			// 
			this.NoShareSequentialNumbers.AutoCheck = false;
			this.NoShareSequentialNumbers.AutoSize = true;
			this.NoShareSequentialNumbers.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NoShareSequentialNumbers.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1e4c0ac6-3ee2-485f-81f4-74ce55292811", "No");
			this.NoShareSequentialNumbers.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 24, true);
			this.NoShareSequentialNumbers.Name = "NoShareSequentialNumbers";
			this.NoShareSequentialNumbers.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 17, true);
			this.NoShareSequentialNumbers.TabIndex = 3;
			this.NoShareSequentialNumbers.TabStop = true;
			this.NoShareSequentialNumbers.UseVisualStyleBackColor = true;
			// 
			// YesShareSequentialNumbers
			// 
			this.YesShareSequentialNumbers.AutoCheck = false;
			this.YesShareSequentialNumbers.AutoSize = true;
			this.BindingSource.SetBindingMember(this.YesShareSequentialNumbers, "Value");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Registry.Business.ShareSequentialTransactionNumbers)(null)).Value)));
			this.YesShareSequentialNumbers.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.YesShareSequentialNumbers.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8bdebfc5-7012-4e2c-8fe5-c3a6228c2a06", "Yes");
			this.YesShareSequentialNumbers.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 24, true);
			this.YesShareSequentialNumbers.Name = "YesShareSequentialNumbers";
			this.YesShareSequentialNumbers.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 17, true);
			this.YesShareSequentialNumbers.TabIndex = 2;
			this.YesShareSequentialNumbers.TabStop = true;
			this.YesShareSequentialNumbers.UseVisualStyleBackColor = true;
			// 
			// ShareSequentialNumbersControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OptionGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 57, true);
			this.Name = "ShareSequentialNumbersControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 57, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OptionGroupBox.ResumeLayout(false);
			this.OptionGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZRadioButton NoShareSequentialNumbers;
		protected Enterprise.ZArchitecture.GUI.ZRadioButton YesShareSequentialNumbers;
		protected CargoWise.Windows.UI.KGroupBox OptionGroupBox;

	}
}
