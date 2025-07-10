namespace Enterprise.BufferManagement.GUI
{
	partial class TaskStatusControl
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
			this.ActualTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ProcessTask);
			// 
			// ActualTimeEdit
			// 
			this.ActualTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ActualTimeEdit, "P9_ActualDuration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_ActualDuration)));
			this.ActualTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 2, true);
			this.ActualTimeEdit.Name = "ActualTimeEdit";
			this.ActualTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.ActualTimeEdit.TabIndex = 35;
			this.ActualTimeEdit.Visible = false;
			// 
			// TaskStatusControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ActualTimeEdit);
			this.Controls.Add(this.ClosedStatusButton);
			this.Controls.Add(this.SuspendedStatusButton);
			this.Controls.Add(this.WorkingStatusButton);
			this.Name = "TaskStatusControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 26, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		public ZArchitecture.GUI.ZTimeEdit ActualTimeEdit;
	}
}
