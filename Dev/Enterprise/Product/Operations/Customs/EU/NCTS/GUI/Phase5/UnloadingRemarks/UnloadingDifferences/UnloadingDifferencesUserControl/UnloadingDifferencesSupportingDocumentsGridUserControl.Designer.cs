namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class UnloadingDifferencesSupportingDocumentsGridUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.SupportingDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).BeginInit();
            this.SupportingDocumentsGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocumentCollection<Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument>);
            // 
            // SupportingDocumentsGrid
            // 
            this.SupportingDocumentsGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.SupportingDocumentsGrid, ".");
            this.SupportingDocumentsGrid.CaptionVisible = false;
            this.SupportingDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SupportingDocumentsGrid.GridId = "f58e23e6-b4e6-4480-83c3-a55ac9d98cb4";
            this.SupportingDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.SupportingDocumentsGrid.LayoutKey = "SupportingDocumentsGrid";
            this.SupportingDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.SupportingDocumentsGrid.Name = "SupportingDocumentsGrid";
            this.SupportingDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(531, 70, true);
            this.SupportingDocumentsGrid.TabIndex = 0;
            // 
            // UnloadingDifferencesSupportingDocumentsGridUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.SupportingDocumentsGrid);
            this.Name = "UnloadingDifferencesSupportingDocumentsGridUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(531, 106, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).EndInit();
            this.SupportingDocumentsGrid.ResumeLayout(false);
            this.SupportingDocumentsGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid SupportingDocumentsGrid;
	}
}
