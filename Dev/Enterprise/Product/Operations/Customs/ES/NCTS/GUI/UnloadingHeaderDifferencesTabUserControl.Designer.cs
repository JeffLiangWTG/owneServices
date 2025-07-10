namespace Enterprise.Customs.ES.NCTS.GUI
{
	partial class UnloadingHeaderDifferencesTabUserControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.UnloadingDifferencesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SealsGrid)).BeginInit();
			this.SealsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.NCTS.Business.NctsHeader);
			// 
			// 
			// SealsGrid
			//
			this.BindingSource.SetBindingMember(this.SealsGrid, "Seals");
			zCheckBoxColumnStyleInfo.ColumnName = "IsBroken";
			zCheckBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.SealsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo);
			// 
			// UnloadingHeaderDifferencesTabUserControl
			// 
			this.Name = "UnloadingHeaderDifferencesTabUserControl";
			this.UnloadingDifferencesGroupBox.ResumeLayout(false);
			this.UnloadingDifferencesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SealsGrid)).EndInit();
			this.SealsGrid.ResumeLayout(false);
			this.SealsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
