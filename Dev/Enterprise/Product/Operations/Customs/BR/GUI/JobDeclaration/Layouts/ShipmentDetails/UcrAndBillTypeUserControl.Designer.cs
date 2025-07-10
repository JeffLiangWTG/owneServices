namespace Enterprise.Customs.BR.GUI
{
	partial class UcrAndBillTypeUserControl
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
			this.BillTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JE_UCRTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BillTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
			// 
			// JE_UCRTextBox
			// 
			this.BindingSource.SetBindingMember(this.JE_UCRTextBox, "JE_UCR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).JE_UCR)));
			this.JE_UCRTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 0, true);
			this.JE_UCRTextBox.Name = "JE_UCRTextBox";
			this.JE_UCRTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 18, true);
			this.JE_UCRTextBox.TabIndex = 1;
			// 
			// BillTypeDropEdit
			// 
			this.BillTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BillTypeDropEdit, "BillType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).BillType)));
			this.BillTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BillTypeDropEdit.Name = "BillTypeDropEdit";
			this.BillTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.BillTypeDropEdit.TabIndex = 0;
			// 
			// UcrAndBillTypeUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BillTypeDropEdit);
			this.Controls.Add(this.JE_UCRTextBox);
			this.Name = "UcrAndBillTypeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BillTypeDropEdit.ResumeLayout(true);
			this.BillTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit BillTypeDropEdit;
		internal ZArchitecture.ZTextBox JE_UCRTextBox;
	}
}
