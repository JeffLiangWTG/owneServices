using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AE.Module
{
	public partial class JobDeclarationFilterControl
	{
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			this.DataSourceAssemblyName = "Enterprise.Customs.AE.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AE.Business.JobDeclarationCollection";
			this.Name = "AEJobDeclarationFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		readonly System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
