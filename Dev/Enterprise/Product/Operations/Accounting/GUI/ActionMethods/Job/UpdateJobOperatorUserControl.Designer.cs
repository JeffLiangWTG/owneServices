namespace Enterprise.Accounting.GUI
{
	partial class UpdateJobOperatorUserControl
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
			this.OperatorFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(UpdateJobStatusActionMethodApplicator);
			// 
			// DepartmentFindBox
			// 
			this.OperatorFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OperatorFindBox, "Operator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((UpdateJobOperatorActionMethodApplicator)(null)).Operator)));
			this.OperatorFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 2, true);
			this.OperatorFindBox.Name = "OperatorFindBox";
			this.OperatorFindBox.PreBoundMaxLength = 5;
			this.OperatorFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.OperatorFindBox.TabIndex = 0;
			this.OperatorFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("UpdateJobOperatorUserControl|99bea76a-5cec-4086-83a3-c0c8430b92ec", "Operator");
			// 
			// UpdateJobOperatorUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.OperatorFindBox);
			this.Name = "UpdateJobOperatorUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 187, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		ZArchitecture.GUI.ZCodeFindBox OperatorFindBox;
	}
}
