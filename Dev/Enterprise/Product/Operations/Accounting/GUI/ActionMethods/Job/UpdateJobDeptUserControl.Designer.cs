namespace Enterprise.Accounting.GUI
{
	partial class UpdateJobDeptUserControl
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
			this.DepartmentFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(UpdateJobStatusActionMethodApplicator);
			// 
			// DepartmentFindBox
			// 
			this.DepartmentFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DepartmentFindBox, "Department");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((UpdateJobDeptActionMethodApplicator)(null)).Department)));
			this.DepartmentFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("UpdateJobDeptUserControl|3ea9b7f5-1a68-4d09-9b30-8a0086dac2fd", "Department");
			this.DepartmentFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 2, true);
			this.DepartmentFindBox.Name = "DepartmentFindBox";
			this.DepartmentFindBox.PreBoundMaxLength = 5;
			this.DepartmentFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.DepartmentFindBox.TabIndex = 0;
			// 
			// UpdateJobDeptUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.DepartmentFindBox);
			this.Name = "UpdateJobDeptUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 187, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		ZArchitecture.GUI.ZGuidFindBox DepartmentFindBox;
	}
}
