using CargoWise.Windows.UI;

namespace Enterprise.Registry.GUI
{
	partial class WebServicesRegistryGridControl
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MainPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.WebServicesConfigItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.WebServicesConfigItemsGrid)).BeginInit();
			this.WebServicesConfigItemsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.WebServicesConfigCollection);
			// 
			// MainPanel
			// 
			this.MainPanel.ColumnCount = 1;
			this.MainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 90F));
			this.MainPanel.Controls.Add(this.WebServicesConfigItemsGrid, 0, 0);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.RowCount = 1;
			this.MainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 419, true);
			this.MainPanel.TabIndex = 0;
			// 
			// WebServicesConfigItemsGrid
			// 
			this.WebServicesConfigItemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.WebServicesConfigItemsGrid, ".");
			this.WebServicesConfigItemsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("A4101939-0042-40F1-9052-D99D710B277D", "Name");
			zTextBoxColumnStyleInfo4.ColumnName = "Name";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("34C381D2-46E7-4F19-9B75-08759F9E011B", "Is Enabled");
			zCheckBoxColumnStyleInfo4.ColumnName = "IsEnabled";
			zCheckBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo4.IsSortable = false;
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("A340D607-A1EB-4C95-AECF-AD4F8FD65F61", "Is Custom URL");
			zCheckBoxColumnStyleInfo5.ColumnName = "IsCustomURL";
			zCheckBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo5.IsSortable = false;
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("4D4F8CDC-C1EC-4255-805F-F0D5B8DA69E5", "URL");
			zTextBoxColumnStyleInfo5.ColumnName = "URL";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.IsSortable = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCheckBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("03A49002-E662-49DC-8AEB-F55112493E58", "Is Auto Managed");
			zCheckBoxColumnStyleInfo6.ColumnName = "IsAutoManaged";
			zCheckBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo6.IsSortable = false;
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("0E662672-1BD5-4EA4-A89F-D5FF6108457A", "Number of Server Clusters");
			zTextBoxColumnStyleInfo6.ColumnName = "NumberOfServerClusters";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.IsSortable = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(147);
			this.WebServicesConfigItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.WebServicesConfigItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.WebServicesConfigItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.WebServicesConfigItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.WebServicesConfigItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.WebServicesConfigItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.WebServicesConfigItemsGrid.GridId = "40F410DF-88AC-4508-A88F-793D594AE53C";
			this.WebServicesConfigItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.WebServicesConfigItemsGrid.LayoutKey = "WebServicesConfigItemsGrid";
			this.WebServicesConfigItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.WebServicesConfigItemsGrid.Name = "WebServicesConfigItemsGrid";
			this.WebServicesConfigItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 415, true);
			this.WebServicesConfigItemsGrid.TabIndex = 0;
			// 
			// WebServicesRegistryGridControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainPanel);
			this.Name = "WebServicesRegistryGridControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 419, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.WebServicesConfigItemsGrid)).EndInit();
			this.WebServicesConfigItemsGrid.ResumeLayout(false);
			this.WebServicesConfigItemsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private KTableLayoutPanel MainPanel;
		internal ZArchitecture.ZGrid WebServicesConfigItemsGrid;
	}
}
