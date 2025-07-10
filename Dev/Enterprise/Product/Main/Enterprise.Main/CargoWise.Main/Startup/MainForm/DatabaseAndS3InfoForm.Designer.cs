namespace Enterprise.Startup
{
	partial class DatabaseAndS3InfoForm
	{
		System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			this.ConnectionInfoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DiskUsageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.S3BucketLabel = new Enterprise.ZArchitecture.ZLabel();
			this.S3BucketInfoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.S3BucketSizeErrorLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DiskUsageListView = new CargoWise.Windows.UI.KListView();
			this.Database = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.UsedMb = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.ReservedMb = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.TotalSizeMb = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.TotalSizeGb = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.MainConnectionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FormLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.DiskUsageListViewPanel = new CargoWise.Windows.UI.KPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FormLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 426, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// ConnectionInfoLabel
			// 
			this.ConnectionInfoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ConnectionInfoLabel.CaptionResourceString = CargoWise.Main.Res.GetData("974EF6CF-886F-49D8-8CD3-31AB6B27DDAF", "Server Alias");
			this.ConnectionInfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 31, true);
			this.ConnectionInfoLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(28, 0, 1, 0, true);
			this.ConnectionInfoLabel.Name = "ConnectionInfoLabel";
			this.ConnectionInfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 96, true);
			this.ConnectionInfoLabel.TabIndex = 3;
			this.ConnectionInfoLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// S3BucketLabel
			// 
			this.S3BucketLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.S3BucketLabel.CaptionResourceString = CargoWise.Main.Res.GetData("1B848692-3509-4214-9865-CBE42EBBC868", "S3 Bucket Info:");
			this.S3BucketLabel.IsFontBold = true;
			this.S3BucketLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 127, true);
			this.S3BucketLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(12, 0, 1, 0, true);
			this.S3BucketLabel.Name = "S3BucketLabel";
			this.S3BucketLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 31, true);
			// 
			// S3BucketInfoLabel
			// 
			this.S3BucketInfoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.S3BucketInfoLabel.CaptionResourceString = CargoWise.Main.Res.GetData("B6CFB609-EFA4-4DD9-98DB-528440C9780C", "Bucket Name");
			this.S3BucketInfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 158, true);
			this.S3BucketInfoLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(28, 0, 1, 0, true);
			this.S3BucketInfoLabel.Name = "S3BucketInfoLabel";
			this.S3BucketInfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 72, true);
			this.S3BucketInfoLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// S3BucketSizeErrorLabel
			// 
			this.S3BucketSizeErrorLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.S3BucketSizeErrorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 260, true);
			this.S3BucketSizeErrorLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(28, 0, 1, 0, true);
			this.S3BucketSizeErrorLabel.Name = "S3BucketSizeErrorLabel";
			this.S3BucketSizeErrorLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 31, true);
			this.S3BucketSizeErrorLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.S3BucketSizeErrorLabel, false);
			// 
			// DiskUsageLabel
			// 
			this.DiskUsageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DiskUsageLabel.CaptionResourceString = CargoWise.Main.Res.GetData("B7E05C44-642C-49C9-8B35-9CB27D851C75", "Storage Usage:");
			this.DiskUsageLabel.IsFontBold = true;
			this.DiskUsageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 291, true);
			this.DiskUsageLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(12, 0, 1, 0, true);
			this.DiskUsageLabel.Name = "DiskUsageLabel";
			this.DiskUsageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 31, true);
			this.DiskUsageLabel.TabIndex = 4;
			// 
			// DiskUsageListView
			// 
			this.DiskUsageListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DiskUsageListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.DiskUsageListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.Database,
			this.UsedMb,
			this.ReservedMb,
			this.TotalSizeMb,
			this.TotalSizeGb});
			this.DiskUsageListView.GridLines = true;
			this.DiskUsageListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.DiskUsageListView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DiskUsageListView.MultiSelect = false;
			this.DiskUsageListView.Name = "DiskUsageListView";
			this.DiskUsageListView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(591, 140, true);
			this.DiskUsageListView.TabIndex = 6;
			this.DiskUsageListView.UseCompatibleStateImageBehavior = false;
			this.DiskUsageListView.View = System.Windows.Forms.View.Details;
			// 
			// Database
			// 
			this.Database.Text = "Database";
			this.Database.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			// 
			// UsedMb
			// 
			this.UsedMb.Text = "Data (MB)";
			this.UsedMb.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.UsedMb.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			// 
			// ReservedMb
			// 
			this.ReservedMb.Text = "Reserved (MB)";
			this.ReservedMb.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ReservedMb.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			// 
			// TotalSizeMb
			// 
			this.TotalSizeMb.Text = "Total (MB)";
			this.TotalSizeMb.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalSizeMb.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			// 
			// TotalSizeGb
			// 
			this.TotalSizeGb.Text = "Total (GB)";
			this.TotalSizeGb.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalSizeGb.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			// 
			// MainConnectionLabel
			// 
			this.MainConnectionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.MainConnectionLabel.CaptionResourceString = CargoWise.Main.Res.GetData("3B71F6D0-2681-4E38-8B30-68553C66247D", "Database Info:");
			this.MainConnectionLabel.IsFontBold = true;
			this.MainConnectionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainConnectionLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(12, 0, 1, 0, true);
			this.MainConnectionLabel.Name = "MainConnectionLabel";
			this.MainConnectionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 31, true);
			this.MainConnectionLabel.TabIndex = 7;
			// 
			// flowLayoutPanel1
			// 
			this.FormLayoutPanel.Controls.Add(this.MainConnectionLabel);
			this.FormLayoutPanel.Controls.Add(this.ConnectionInfoLabel);
			this.FormLayoutPanel.Controls.Add(this.S3BucketLabel);
			this.FormLayoutPanel.Controls.Add(this.S3BucketInfoLabel);
			this.FormLayoutPanel.Controls.Add(this.S3BucketSizeErrorLabel);
			this.FormLayoutPanel.Controls.Add(this.DiskUsageLabel);
			this.FormLayoutPanel.Controls.Add(this.DiskUsageListViewPanel);
			this.FormLayoutPanel.Location = new System.Drawing.Point(0, 0);
			this.FormLayoutPanel.Name = "FormLayoutPanel";
			this.FormLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 470, true);
			this.FormLayoutPanel.TabIndex = 11;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FormLayoutPanel, false);
			// 
			// panel1
			// 
			this.DiskUsageListViewPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 322, true);
			this.DiskUsageListViewPanel.Name = "DiskUsageListViewPanel";
			this.DiskUsageListViewPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(591, 140, true);
			this.DiskUsageListViewPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(31, 0, 1, 50, true);
			this.DiskUsageListViewPanel.TabIndex = 11;
			this.DiskUsageListViewPanel.Controls.Add(this.DiskUsageListView);
			// 
			// DatabaseInfoForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = CargoWise.Main.Res.GetData("0C7C348E-667E-4714-9F9B-8FEEED1573A1", "Database and S3 Storage Info");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 470, true);
			this.Controls.Add(this.FormLayoutPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "DatabaseInfoForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Database Info";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.FormLayoutPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FormLayoutPanel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel ConnectionInfoLabel;
		private ZArchitecture.ZLabel DiskUsageLabel;
		private ZArchitecture.ZLabel S3BucketLabel;
		private ZArchitecture.ZLabel S3BucketInfoLabel;
		private ZArchitecture.ZLabel S3BucketSizeErrorLabel;
		private CargoWise.Windows.UI.KListView DiskUsageListView;
		private System.Windows.Forms.ColumnHeader Database;
		private System.Windows.Forms.ColumnHeader UsedMb;
		private System.Windows.Forms.ColumnHeader ReservedMb;
		private System.Windows.Forms.ColumnHeader TotalSizeMb;
		private System.Windows.Forms.ColumnHeader TotalSizeGb;
		private ZArchitecture.ZLabel MainConnectionLabel;
		private CargoWise.Windows.UI.KFlowLayoutPanel FormLayoutPanel;
		private CargoWise.Windows.UI.KPanel DiskUsageListViewPanel;
	}
}
