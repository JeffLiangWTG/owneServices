using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.UPE.Module
{
	public partial class UPEOrganisationFilterControl : OrganisationFilterControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.Caption = "Registration Number";
			zTextBoxColumnStyleInfo1.ColumnName = "AccountNumber";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			// 
			// UPEOrganisationFilterControl
			// 
			this.Name = "UPEOrganisationFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
