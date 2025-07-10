namespace Enterprise.Customs.ES.GUI
{
	public partial class DateOfIssueAndExpiryUserControl
	{
		private void InitializeComponent()
		{
			this.DateOfIssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DateOfExpiryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DateOfIssueDateEdit.SuspendLayout();
			this.DateOfExpiryDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.JobDeclaration);
			// 
			// DateOfIssueDateEdit
			// 
			this.DateOfIssueDateEdit.AllowDrop = true;
			this.DateOfIssueDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateOfIssueDateEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DateOfIssueDateEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_DateOfIssue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_DateOfIssue)));
			this.DateOfIssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DateOfIssueDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.DateOfIssueDateEdit.Name = "DateOfIssueDateEdit";
			this.DateOfIssueDateEdit.TabIndex = 0;
			// 
			// DateOfExpiryDateEdit
			// 
			this.DateOfExpiryDateEdit.AllowDrop = true;
			this.DateOfExpiryDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateOfExpiryDateEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DateOfExpiryDateEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_DateOfExpiry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_DateOfExpiry)));
			this.DateOfExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 0, true);
			this.DateOfExpiryDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.DateOfExpiryDateEdit.Name = "DateOfExpiryDateEdit";
			this.DateOfExpiryDateEdit.TabIndex = 1;
			// 
			// DateOfIssueAndExpiryUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DateOfExpiryDateEdit);
			this.Controls.Add(this.DateOfIssueDateEdit);
			this.Name = "DateOfIssueAndExpiryUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(335, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DateOfIssueDateEdit.ResumeLayout(true);
			this.DateOfIssueDateEdit.PerformLayout();
			this.DateOfExpiryDateEdit.ResumeLayout(true);
			this.DateOfExpiryDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.GUI.ZDateEdit DateOfIssueDateEdit;
		internal ZArchitecture.GUI.ZDateEdit DateOfExpiryDateEdit;
	}
}
