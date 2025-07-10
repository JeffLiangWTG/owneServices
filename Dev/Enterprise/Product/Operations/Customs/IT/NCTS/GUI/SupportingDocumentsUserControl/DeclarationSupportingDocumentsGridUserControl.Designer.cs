namespace Enterprise.Customs.IT.NCTS.GUI
{
	partial class DeclarationSupportingDocumentsGridUserControl
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
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.NCTS.Business.NctsHeader);
			// 
			// SupportingDocumentsGrid
			// 
			this.SupportingDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SupportingDocumentsGrid, "MovementHeader.SupportingDocuments");
			this.SupportingDocumentsGrid.CaptionVisible = false;
			this.SupportingDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsGrid.GridId = "7099b8a7-6791-4aee-813c-0cb1e2766fd5";
			this.SupportingDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SupportingDocumentsGrid.LayoutKey = "SupportingDocumentsGrid";
			this.SupportingDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentsGrid.Name = "SupportingDocumentsGrid";
			this.SupportingDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(596, 85, true);
			this.SupportingDocumentsGrid.TabIndex = 0;
			// 
			// DeclarationSupportingDocumentsGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SupportingDocumentsGrid);
			this.Name = "DeclarationSupportingDocumentsGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(596, 85, true);
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
