namespace Enterprise.Customs.EU.ExitControl.GUI
{
	partial class ReportAdditionalDocumentsGridUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AdditionalDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalDocumentsGrid)).BeginInit();
			this.AdditionalDocumentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.ExitControl.Business.IAdditionalInfoCollection<Enterprise.Customs.EU.ExitControl.Business.AdditionalInfo>);
			// 
			// AdditionalDocumentsGrid
			// 
			this.AdditionalDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalDocumentsGrid, ".");
			this.AdditionalDocumentsGrid.CaptionVisible = false;
			this.AdditionalDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalDocumentsGrid.GridId = "2a403264-f338-4c0e-b426-2c0261ca4701";
			this.AdditionalDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalDocumentsGrid.LayoutKey = "AdditionalDocumentsGrid";
			this.AdditionalDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalDocumentsGrid.Name = "AdditionalDocumentsGrid";
			this.AdditionalDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 90, true);
			this.AdditionalDocumentsGrid.TabIndex = 0;
			// 
			// ReportAdditionalDocumentsGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalDocumentsGrid);
			this.Name = "ReportAdditionalDocumentsGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 90, true);
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

