namespace Enterprise.Customs.IE.GUI
{
	partial class AISDocumentsUploadAddInfoGridUserControl
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
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.AddInfosGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AddInfosGrid)).BeginInit();
			this.AddInfosGrid.SuspendLayout();
			this.AddInfosIM483Grid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.AddInfosIM483Grid)).BeginInit();
			this.AddInfosIM483Grid.SuspendLayout();
			this.SuspendLayout();

			// 
			// AddInfosGrid
			// 
			this.AddInfosGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AddInfosGrid, "AddInfoCollection");
			this.AddInfosGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "DocumentType";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zMultiLineTextBoxColumnInfo1.ColumnName = "DocumentInformation";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.AddInfosGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.AddInfosGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.AddInfosGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddInfosGrid.GridId = "680C7814-8F5E-4587-AC67-91E36F42DB59";
			this.AddInfosGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AddInfosGrid.LayoutKey = "AddInfosGrid";
			this.AddInfosGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AddInfosGrid.Name = "AddInfosGrid";
			this.AddInfosGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(519, 413, true);
			this.AddInfosGrid.TabIndex = 4;
			// 
			// AddInfosIM483Grid
			// 
			this.AddInfosIM483Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AddInfosIM483Grid, "AddInfoCollection");
			this.AddInfosIM483Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.ColumnName = "ReferenceNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "CCQualifier";
			zCodeFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.AddInfosIM483Grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.AddInfosIM483Grid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.AddInfosIM483Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.AddInfosIM483Grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.AddInfosIM483Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddInfosIM483Grid.GridId = "30B5C482-55ED-45FA-8E5C-7319220AF4DA";
			this.AddInfosIM483Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AddInfosIM483Grid.LayoutKey = "AddInfosIM483Grid";
			this.AddInfosIM483Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AddInfosIM483Grid.Name = "AddInfosIM483Grid";
			this.AddInfosIM483Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(519, 413, true);
			this.AddInfosIM483Grid.TabIndex = 4;
			// 
			// AISDocumentsUploadAddInfoGridUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AddInfosGrid);
			this.Controls.Add(this.AddInfosIM483Grid);
			this.Name = "AISDocumentsUploadAddInfoGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(519, 413, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AddInfosGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AddInfosIM483Grid)).EndInit();
			this.AddInfosGrid.ResumeLayout(false);
			this.AddInfosGrid.PerformLayout();
			this.AddInfosIM483Grid.ResumeLayout(false);
			this.AddInfosIM483Grid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZArchitecture.ZGrid AddInfosGrid;
		internal ZArchitecture.ZGrid AddInfosIM483Grid;
	}
}
