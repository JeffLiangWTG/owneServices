namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class DeclarationAdditionalDocumentsGridUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.AdditionalDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalDocumentsGrid)).BeginInit();
			this.AdditionalDocumentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// AdditionalDocumentsGrid
			// 
			this.AdditionalDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalDocumentsGrid, "AdditionalDocuments");
			this.AdditionalDocumentsGrid.CaptionVisible = false;
			this.AdditionalDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalDocumentsGrid.GridId = "194e9e62-ec30-46c6-aba3-c3f90fd567b3";
			this.AdditionalDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalDocumentsGrid.LayoutKey = "AdditionalDocumentsGrid";
			this.AdditionalDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalDocumentsGrid.Name = "AdditionalDocumentsGrid";
			this.AdditionalDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 135, true);
			this.AdditionalDocumentsGrid.TabIndex = 0;
			// 
			// DeclarationAdditionalDocumentsGridUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalDocumentsGrid);
			this.Name = "DeclarationAdditionalDocumentsGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 135, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalDocumentsGrid)).EndInit();
			this.AdditionalDocumentsGrid.ResumeLayout(false);
			this.AdditionalDocumentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected internal ZArchitecture.ZGrid AdditionalDocumentsGrid;
	}
}
