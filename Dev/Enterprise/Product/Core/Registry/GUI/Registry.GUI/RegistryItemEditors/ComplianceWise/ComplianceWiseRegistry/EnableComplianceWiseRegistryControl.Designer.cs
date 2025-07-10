namespace Enterprise.Registry.GUI
{
	partial class EnableComplianceWiseRegistryControl
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
			this.EnableComplianceWiseCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.EnableComplianceWiseRegistryBusinessObject);
			// 
			// EnableComplianceWiseCheckBox
			// 
			this.EnableComplianceWiseCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.EnableComplianceWiseCheckBox, "EnableComplianceWise");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.EnableComplianceWiseRegistryBusinessObject)(null)).EnableComplianceWise)));
			this.EnableComplianceWiseCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("D72AEBD9-235B-4BA4-8F9E-BF8EF4651002", "Enable ComplianceWise");
			this.EnableComplianceWiseCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EnableComplianceWiseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EnableComplianceWiseCheckBox.Name = "EnableComplianceWiseCheckBox";
			this.EnableComplianceWiseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 17, true);
			this.EnableComplianceWiseCheckBox.TabIndex = 0;
			this.EnableComplianceWiseCheckBox.UseVisualStyleBackColor = true;
			// 
			// EnableComplianceWiseRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EnableComplianceWiseCheckBox);
			this.Name = "EnableComplianceWiseRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 171, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZArchitecture.GUI.ZCheckBox EnableComplianceWiseCheckBox;
	}
}
