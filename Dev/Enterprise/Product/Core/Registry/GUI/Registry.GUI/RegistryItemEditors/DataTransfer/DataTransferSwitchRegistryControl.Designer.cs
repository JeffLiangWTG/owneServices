namespace Enterprise.Registry.GUI
{
	partial class DataTransferSwitchRegistryControl
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
			this.EnableInterfaceCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zGroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// DirectoryTextBox
			// 
			this.DirectoryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DirectoryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 50, true);
			this.DirectoryTextBox.TabIndex = 2;
			// 
			// DirectorySelectorButton
			// 
			this.DirectorySelectorButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DirectorySelectorButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(374, 47, true);
			this.DirectorySelectorButton.TabIndex = 3;
			// 
			// zGuidFindBox1
			// 
			this.zGuidFindBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 182, true);
			this.zGuidFindBox1.TabIndex = 13;
			// 
			// zTextBox1
			// 
			this.zTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 88, true);
			this.zTextBox1.TabIndex = 5;
			// 
			// NextRunDateTimeEdit
			// 
			this.NextRunDateTimeEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.NextRunDateTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 124, true);
			this.NextRunDateTimeEdit.TabIndex = 7;
			// 
			// IntervalCalcEdit
			// 
			this.IntervalCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.IntervalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 154, true);
			this.IntervalCalcEdit.TabIndex = 9;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(206, 154, true);
			this.zDropEdit1.TabIndex = 10;
			// 
			// zLabel5
			// 
			this.zLabel5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(329, 157, true);
			this.zLabel5.TabIndex = 11;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.zGroupBox1.Controls.Add(this.EnableInterfaceCheckBox);
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 213, true);
			this.zGroupBox1.Controls.SetChildIndex(this.DirectorySelectorButton, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.zTextBox1, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.IntervalCalcEdit, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.DirectoryTextBox, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.zDropEdit1, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.zLabel5, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.zGuidFindBox1, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.NextRunDateTimeEdit, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.EnableInterfaceCheckBox, 0);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.DataTransferSwitchRegistryBusinessObject);
			// 
			// EnableInterfaceCheckBox
			// 
			this.EnableInterfaceCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.EnableInterfaceCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.EnableInterfaceCheckBox, "EnableInterface");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.DataTransferSwitchRegistryBusinessObject)(null)).EnableInterface)));
			this.EnableInterfaceCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.EnableInterfaceCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("DataTransferSwitchRegistryControl|be8c927d-e9e7-4c22-8be6-122a2ce01d71", "Enable Interface:");
			this.EnableInterfaceCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EnableInterfaceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 19, true);
			this.EnableInterfaceCheckBox.Name = "EnableInterfaceCheckBox";
			this.EnableInterfaceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.EnableInterfaceCheckBox.TabIndex = 0;
			// 
			// DataTransferSwitchRegistryControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "DataTransferSwitchRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 216, true);
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion		

		protected Enterprise.ZArchitecture.GUI.ZCheckBox EnableInterfaceCheckBox;

	}
}
