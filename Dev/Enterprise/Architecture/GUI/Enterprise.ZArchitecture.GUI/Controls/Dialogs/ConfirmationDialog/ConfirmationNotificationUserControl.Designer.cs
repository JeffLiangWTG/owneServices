namespace Enterprise.ZArchitecture.GUI
{
	partial class ConfirmationNotificationUserControl
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
			this.IgnoredCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HintTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Core.ConfirmationNotification);
			// 
			// IgnoredCheckBox
			// 
			this.IgnoredCheckBox.AutoSize = true;
			this.IgnoredCheckBox.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.IgnoredCheckBox, "IsIgnored");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ZArchitecture.Core.ConfirmationNotification)(null)).IsIgnored)));
			this.IgnoredCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IgnoredCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 18, true);
			this.IgnoredCheckBox.Name = "IgnoredCheckBox";
			this.IgnoredCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 17, true);
			this.IgnoredCheckBox.TabIndex = 0;
			this.IgnoredCheckBox.UseVisualStyleBackColor = false;
			// 
			// HintTextBox
			// 
			this.HintTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.HintTextBox.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
			this.BindingSource.SetBindingMember(this.HintTextBox, "Message");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Core.ConfirmationNotification)(null)).Message)));
			this.HintTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.HintTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.HintTextBox, false);
			this.HintTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 3, true);
			this.HintTextBox.Multiline = true;
			this.HintTextBox.Name = "HintTextBox";
			this.HintTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 48, true);
			this.HintTextBox.TabIndex = 1;
			// 
			// ConfirmationNotificationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.HintTextBox);
			this.Controls.Add(this.IgnoredCheckBox);
			this.Name = "ConfirmationNotificationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 56, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZCheckBox IgnoredCheckBox;
		private ZArchitecture.ZTextBox HintTextBox;
	}
}
