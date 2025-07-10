namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	partial class RelatedDeclarationsUserControl
	{
		private System.ComponentModel.IContainer components = null;

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
			((System.ComponentModel.ISupportInitialize)(this.RelatedDeclarationsGrid)).BeginInit();
			this.RelatedDeclarationsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ParentButton
			// 
			this.ParentButton.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("516347fc-820e-4801-bf98-e4bf0e05d5d7", "Parent");
			this.ParentButton.IsCaptionOverridden = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AsycudaCustoms.Business.JobDeclaration);
			// 
			// RelatedDeclarationsUserControl
			// 
			this.Name = "RelatedDeclarationsUserControl";
			((System.ComponentModel.ISupportInitialize)(this.RelatedDeclarationsGrid)).EndInit();
			this.RelatedDeclarationsGrid.ResumeLayout(false);
			this.RelatedDeclarationsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
