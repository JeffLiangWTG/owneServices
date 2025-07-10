namespace Enterprise.Customs.KR.GUI
{
	partial class MiscRequestRelatedEntriesUserControl
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
      this.EntryTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
      this.EntryDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
      this.EntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
      this.EntryTypeDropEdit.SuspendLayout();
      this.SuspendLayout();
      // 
      // BindingSource
      // 
      this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.CusMiscRequestHeader);
      // 
      // EntryTypeDropEdit
      // 
      this.EntryTypeDropEdit.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.EntryTypeDropEdit, "RequestLines.EntryType");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusMiscRequestLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusMiscRequestHeader)(null)).RequestLines)).SyncRoot)).EntryType)));
      this.EntryTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(27, 54, true);
      this.EntryTypeDropEdit.Name = "EntryTypeDropEdit";
      this.EntryTypeDropEdit.PreBoundMaxLength = 3;
      this.EntryTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 15, true);
      this.EntryTypeDropEdit.TabIndex = 1;
      // 
      // EntryDetailsTextBox
      // 
      this.BindingSource.SetBindingMember(this.EntryDetailsTextBox, "RequestLines.CML_Remarks");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusMiscRequestLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusMiscRequestHeader)(null)).RequestLines)).SyncRoot)).CML_Remarks)));
      this.EntryDetailsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
      this.EntryDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(27, 113, true);
      this.EntryDetailsTextBox.Multiline = true;
      this.EntryDetailsTextBox.Name = "EntryDetailsTextBox";
      this.EntryDetailsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.EntryDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 147, true);
      this.EntryDetailsTextBox.TabIndex = 3;
      // 
      // EntryNumberTextBox
      // 
      this.BindingSource.SetBindingMember(this.EntryNumberTextBox, "RequestLines.FormattedEntryNumber");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusMiscRequestLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusMiscRequestHeader)(null)).RequestLines)).SyncRoot)).FormattedEntryNumber)));
      this.EntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(27, 84, true);
      this.EntryNumberTextBox.Name = "EntryNumberTextBox";
      this.EntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 15, true);
      this.EntryNumberTextBox.TabIndex = 2;
      // 
      // MiscRequestRelatedEntriesUserControl
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.Controls.Add(this.EntryTypeDropEdit);
      this.Controls.Add(this.EntryNumberTextBox);
      this.Controls.Add(this.EntryDetailsTextBox);
      this.Name = "MiscRequestRelatedEntriesUserControl";
      this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 281, true);
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
      this.EntryTypeDropEdit.ResumeLayout(true);
      this.EntryTypeDropEdit.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		#endregion
		public ZArchitecture.GUI.ZDropEdit EntryTypeDropEdit;
		public ZArchitecture.ZTextBox EntryDetailsTextBox;
		public ZArchitecture.ZTextBox EntryNumberTextBox;
	}
}
