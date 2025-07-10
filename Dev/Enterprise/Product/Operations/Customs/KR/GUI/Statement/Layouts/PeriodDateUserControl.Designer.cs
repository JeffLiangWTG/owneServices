namespace Enterprise.Customs.KR.GUI
{
	partial class PeriodDateUserControl
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
			this.PeriodStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PeriodEndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PeriodStartDateEdit.SuspendLayout();
			this.PeriodEndDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.CusStatementHeader);
			// 
			// PeriodStartDateEdit
			// 
			this.PeriodStartDateEdit.AllowDrop = true;
			this.PeriodStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.PeriodStartDateEdit, "B2_PeriodStartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).B2_PeriodStartDate)));
			this.PeriodStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PeriodStartDateEdit.Name = "PeriodStartDateEdit";
			this.PeriodStartDateEdit.TabIndex = 0;
			// 
			// PeriodEndDateEdit
			// 
			this.PeriodEndDateEdit.AllowDrop = true;
			this.PeriodEndDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.PeriodEndDateEdit, "B2_PeriodEndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).B2_PeriodEndDate)));
			this.PeriodEndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 0, true);
			this.PeriodEndDateEdit.Name = "PeriodEndDateEdit";
			this.PeriodEndDateEdit.TabIndex = 1;
			// 
			// PeriodDateUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PeriodEndDateEdit);
			this.Controls.Add(this.PeriodStartDateEdit);
			this.Name = "PeriodDateUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PeriodStartDateEdit.ResumeLayout(true);
			this.PeriodStartDateEdit.PerformLayout();
			this.PeriodEndDateEdit.ResumeLayout(true);
			this.PeriodEndDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDateEdit PeriodStartDateEdit;
		private ZArchitecture.GUI.ZDateEdit PeriodEndDateEdit;
	}
}
