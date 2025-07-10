using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class HouseConsignmentAdditionalDocumentGridUserControl
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
			this.AdditionalDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalDocumentsGrid)).BeginInit();
			this.AdditionalDocumentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>);
			// 
			// AdditionalDocumentsGrid
			// 
			this.AdditionalDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalDocumentsGrid, ".");
			this.AdditionalDocumentsGrid.CaptionVisible = false;
			this.AdditionalDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalDocumentsGrid.GridId = "B72DF376-2994-422A-ABEA-CC17EA2DEDC9";
			this.AdditionalDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalDocumentsGrid.LayoutKey = "AdditionalDocumentsGrid";
			this.AdditionalDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalDocumentsGrid.Name = "AdditionalDocumentsGrid";
			this.AdditionalDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 139, true);
			this.AdditionalDocumentsGrid.TabIndex = 0;
			// 
			// HouseConsignmentAdditionalDocumentGridUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalDocumentsGrid);
			this.Name = "HouseConsignmentAdditionalDocumentGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 139, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalDocumentsGrid)).EndInit();
			this.AdditionalDocumentsGrid.ResumeLayout(false);
			this.AdditionalDocumentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid AdditionalDocumentsGrid;
	}
}
