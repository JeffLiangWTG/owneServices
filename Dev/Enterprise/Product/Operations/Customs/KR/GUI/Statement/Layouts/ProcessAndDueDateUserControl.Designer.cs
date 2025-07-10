namespace Enterprise.Customs.KR.GUI
{
	partial class ProcessAndDueDateUserControl
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
			this.ProcessDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ProcessDateEdit.SuspendLayout();
			this.DueDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.CusStatementHeader);
			// 
			// ProcessDateEdit
			// 
			this.ProcessDateEdit.AllowDrop = true;
			this.ProcessDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ProcessDateEdit, "B2_ProcessDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).B2_ProcessDate)));
			this.ProcessDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProcessDateEdit.Name = "ProcessDateEdit";
			this.ProcessDateEdit.TabIndex = 0;
			// 
			// DueDateEdit
			// 
			this.DueDateEdit.AllowDrop = true;
			this.DueDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DueDateEdit, "B2_DueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).B2_DueDate)));
			this.DueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 0, true);
			this.DueDateEdit.Name = "DueDateEdit";
			this.DueDateEdit.TabIndex = 1;
			// 
			// ProcessAndDueDateUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DueDateEdit);
			this.Controls.Add(this.ProcessDateEdit);
			this.Name = "ProcessAndDueDateUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ProcessDateEdit.ResumeLayout(true);
			this.ProcessDateEdit.PerformLayout();
			this.DueDateEdit.ResumeLayout(true);
			this.DueDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDateEdit ProcessDateEdit;
		private ZArchitecture.GUI.ZDateEdit DueDateEdit;
	}
}
