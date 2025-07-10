namespace Enterprise.ResourceStrings.Module
{
	partial class ResourceStringsFilterControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ResourceStrings.Module.Res.GetData("ResourceStringsFilterControl|HD_Code", "Key");
			zTextBoxColumnStyleInfo1.ColumnName = "HD_Code";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.ResourceStrings.Module.Res.GetData("ResourceStringsFilterControl|HD_ShortCaption", "Short Caption");
			zTextBoxColumnStyleInfo2.ColumnName = "HD_ShortCaption";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.ResourceStrings.Module.Res.GetData("ResourceStringsFilterControl|HD_MidCaption", "Medium Caption");
			zTextBoxColumnStyleInfo3.ColumnName = "HD_MidCaption";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.ResourceStrings.Module.Res.GetData("ResourceStringsFilterControl|HD_Caption", "Caption");
			zTextBoxColumnStyleInfo4.ColumnName = "HD_Caption";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.ResourceStrings.Module.Res.GetData("ResourceStringsFilterControl|HD_Language", "Language");
			zTextBoxColumnStyleInfo5.ColumnName = "HD_Language";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.ResourceStrings.Module.Res.GetData("ResourceStringsFilterControl|HD_FullDescription", "Full Description");
			zTextBoxColumnStyleInfo6.ColumnName = "HD_FullDescription";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ResourceStrings.Module.Res.GetData("ResourceStringsFilterControl|HD_IsCheckedOut", "Checked Out");
			zCheckBoxColumnStyleInfo1.ColumnName = "HD_IsCheckedOut";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.ResourceStrings.Module.Res.GetData("242a1c8c-a52f-4c64-9ebe-6b93089d4c4a", "Edit Reason");
			zTextBoxColumnStyleInfo7.ColumnName = "HD_EditReason";
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 174, true);
			this.grid.TabIndex = 11;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ResourceStrings.Business.HelpDataString);
			// 
			// ResourceStringsFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "ResourceStringsFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

	}
}