namespace Enterprise.Customs.EU.GUI
{
	partial class InvoiceLineAuthorizationsComputedUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.AuthorisationsGrid)).BeginInit();
			this.AuthorisationsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// AuthorisationsGrid
			// 
			this.BindingSource.SetBindingMember(this.AuthorisationsGrid, "FilteredInvoiceLines.CusAuthorizationUsages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CusAuthorizationUsages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusAuthorizationUsage)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CusAuthorizationUsages)).SyncRoot)).AGC_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusAuthorizationUsage)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CusAuthorizationUsages)).SyncRoot)).CustomsCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusAuthorizationUsage)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CusAuthorizationUsages)).SyncRoot)).EffectiveReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.CusAuthorizationUsage)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CusAuthorizationUsages)).SyncRoot)).AGC_OH_Owner)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusAuthorizationUsage)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CusAuthorizationUsages)).SyncRoot)).Lookups.Owners)));
			this.AuthorisationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AuthorisationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.AuthorisationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 269, true);
			// 
			// InvoiceLineAuthorizationsComputedUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "InvoiceLineAuthorizationsComputedUserControl";
			((System.ComponentModel.ISupportInitialize)(this.AuthorisationsGrid)).EndInit();
			this.AuthorisationsGrid.ResumeLayout(false);
			this.AuthorisationsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
