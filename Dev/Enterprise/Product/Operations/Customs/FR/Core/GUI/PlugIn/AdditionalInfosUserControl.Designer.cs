namespace Enterprise.Customs.FR.GUI.PlugIn
{
	partial class AdditionalInfosUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.DateOfIssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DateOfIssueDateEdit.SuspendLayout();
			this.AdditionalInfosGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Business.Declaration.JobDeclaration);
			// 
			// AdditionalInfosGroupBox
			// 
			this.AdditionalInfosGroupBox.Controls.Add(this.DateOfIssueDateEdit);
			this.AdditionalInfosGroupBox.Controls.SetChildIndex(this.DateOfIssueDateEdit, 0);
			// 
			// AddInfoTypeCodeDropEdit
			//
			this.AddInfoTypeCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 14, true);
			// 
			// AddiInfoDescriptionTextBox
			// 
			this.AddiInfoDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 38, true);
			this.AddiInfoDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 46, true);
			// 
			// DateOfIssueDateEdit
			// 
			this.DateOfIssueDateEdit.AllowDrop = true;
			this.DateOfIssueDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateOfIssueDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateOfIssueDateEdit, "FilteredInvoiceLines.AdditionalInfos.CSI_DateOfIssue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.Declaration.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AdditionalInfos)).SyncRoot)).CSI_DateOfIssue)));
			this.DateOfIssueDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Custom;
			this.DateOfIssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 87, true);
			this.DateOfIssueDateEdit.Name = "DateOfIssueDateEdit";
			this.DateOfIssueDateEdit.TabIndex = 4;
			// 
			// AdditionalInfosUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.DateOfIssueDateEdit.ResumeLayout(true);
			this.DateOfIssueDateEdit.PerformLayout();
			this.AdditionalInfosGroupBox.ResumeLayout(true);
			this.AdditionalInfosGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected ZArchitecture.GUI.ZDateEdit DateOfIssueDateEdit;
	}
}
