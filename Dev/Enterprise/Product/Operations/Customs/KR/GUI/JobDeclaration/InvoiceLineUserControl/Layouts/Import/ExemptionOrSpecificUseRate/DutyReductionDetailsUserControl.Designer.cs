using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	partial class DutyReductionDetailsUserControl
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
      this.GroupNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
      this.SeqNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
      this.ItemNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
      this.GroupNumberDropEdit.SuspendLayout();
      this.SuspendLayout();
      // 
      // BindingSource
      // 
      this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobComInvoiceLine);
      // 
      // GroupNumberDropEdit
      // 
      this.GroupNumberDropEdit.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.GroupNumberDropEdit, "DutyReductionGroupNumber");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).DutyReductionGroupNumber)));
      this.GroupNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 5, true);
      this.GroupNumberDropEdit.Name = "GroupNumberDropEdit";
      this.GroupNumberDropEdit.PreBoundMaxLength = 3;
      this.GroupNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 17, true);
      this.GroupNumberDropEdit.TabIndex = 0;
      // 
      // SeqNumberTextBox
      // 
      this.BindingSource.SetBindingMember(this.SeqNumberTextBox, "DutyReductionSeqNumber");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).DutyReductionSeqNumber)));
      this.SeqNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 27, true);
      this.SeqNumberTextBox.Name = "SeqNumberTextBox";
      this.SeqNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 17, true);
      this.SeqNumberTextBox.TabIndex = 1;
      // 
      // ItemNumberTextBox
      // 
      this.BindingSource.SetBindingMember(this.ItemNumberTextBox, "DutyReductionItemNumber");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).DutyReductionItemNumber)));
      this.ItemNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 26, true);
      this.ItemNumberTextBox.Name = "ItemNumberTextBox";
      this.ItemNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 17, true);
      this.ItemNumberTextBox.TabIndex = 2;
      // 
      // DutyReductionDetailsUserControl
      // 
      this.CaptionRenderingEnabled = true;
      this.Controls.Add(this.ItemNumberTextBox);
      this.Controls.Add(this.SeqNumberTextBox);
      this.Controls.Add(this.GroupNumberDropEdit);
      this.Name = "DutyReductionDetailsUserControl";
      this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(439, 75, true);
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
      this.GroupNumberDropEdit.ResumeLayout(true);
      this.GroupNumberDropEdit.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		#endregion

		internal ZDropEdit GroupNumberDropEdit;
		internal ZArchitecture.ZTextBox SeqNumberTextBox;
		internal ZArchitecture.ZTextBox ItemNumberTextBox;
	}
}
