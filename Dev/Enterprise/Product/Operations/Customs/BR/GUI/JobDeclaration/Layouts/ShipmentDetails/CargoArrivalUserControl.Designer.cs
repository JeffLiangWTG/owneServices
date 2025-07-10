namespace Enterprise.Customs.BR.GUI
{
	partial class CargoArrivalUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
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
			this.BR_CargoArrivalDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BR_CargoArrivalDocNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BR_CargoArrivalDropDownEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
			// 
			// BR_CargoArrivalDropDownEdit
			// 
			this.BR_CargoArrivalDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BR_CargoArrivalDropDownEdit, "JE_CargoArrivalDocumentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).JE_CargoArrivalDocumentType)));
			this.BR_CargoArrivalDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BR_CargoArrivalDropDownEdit.Name = "BR_CargoArrivalDropDownEdit";
			this.BR_CargoArrivalDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 20, true);
			this.BR_CargoArrivalDropDownEdit.TabIndex = 0;
			// 
			// BR_CargoArrivalDocNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.BR_CargoArrivalDocNumberTextBox, "JE_CargoArrivalDocumentNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).JE_CargoArrivalDocumentNumber)));
			this.BR_CargoArrivalDocNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(239, 0, true);
			this.BR_CargoArrivalDocNumberTextBox.Name = "BR_CargoArrivalDocNumberTextBox";
			this.BR_CargoArrivalDocNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
			this.BR_CargoArrivalDocNumberTextBox.TabIndex = 1;
			// 
			// CargoArrivalUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BR_CargoArrivalDocNumberTextBox);
			this.Controls.Add(this.BR_CargoArrivalDropDownEdit);
			this.Name = "CargoArrivalUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BR_CargoArrivalDropDownEdit.ResumeLayout(true);
			this.BR_CargoArrivalDropDownEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit BR_CargoArrivalDropDownEdit;
		private ZArchitecture.ZTextBox BR_CargoArrivalDocNumberTextBox;
	}
}
