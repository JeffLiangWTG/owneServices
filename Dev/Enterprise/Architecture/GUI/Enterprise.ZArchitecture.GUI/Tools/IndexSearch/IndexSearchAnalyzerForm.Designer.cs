namespace Enterprise.ZArchitecture.DevTools
{
	partial class IndexSearchAnalyzerForm
	{
		private void InitializeComponent()
		{
			this.IndexSearchUriTextBox = new CargoWise.Windows.UI.KTextBox();
			this.ButtonPanel = new CargoWise.Windows.UI.KPanel();
			this.QueryStatusTextBox = new CargoWise.Windows.UI.KTextBox();
			this.FormatUriCheckBox = new CargoWise.Windows.UI.KCheckBox();
			this.RunQueryButton = new CargoWise.Windows.UI.KButton();
			this.OpenUrlButton = new CargoWise.Windows.UI.KButton();
			this.ResultPanel = new CargoWise.Windows.UI.KPanel();
			this.QueryResultSplitPanel = new CargoWise.Windows.UI.KSplitContainer();
			this.ResultsSplitPanel = new CargoWise.Windows.UI.KSplitContainer();
			this.ResultsTextBox = new CargoWise.Windows.UI.KTextBox();
			this.ButtonPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.QueryResultSplitPanel)).BeginInit();
			this.QueryResultSplitPanel.Panel1.SuspendLayout();
			this.QueryResultSplitPanel.Panel2.SuspendLayout();
			this.QueryResultSplitPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ResultsSplitPanel)).BeginInit();
			this.ResultsSplitPanel.Panel1.SuspendLayout();
			this.ResultsSplitPanel.Panel2.SuspendLayout();
			this.ResultsSplitPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// IndexSearchUriTextBox
			// 
			this.IndexSearchUriTextBox.AcceptsReturn = true;
			this.IndexSearchUriTextBox.AcceptsTab = true;
			this.IndexSearchUriTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IndexSearchUriTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.IndexSearchUriTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IndexSearchUriTextBox.MaxLength = 99999999;
			this.IndexSearchUriTextBox.Multiline = true;
			this.IndexSearchUriTextBox.Name = "IndexSearchUriTextBox";
			this.IndexSearchUriTextBox.ReadOnly = true;
			this.IndexSearchUriTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.IndexSearchUriTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 338, true);
			this.IndexSearchUriTextBox.TabIndex = 0;
			this.IndexSearchUriTextBox.TabStop = false;
			this.IndexSearchUriTextBox.WordWrap = false;
			// 
			// ButtonPanel
			// 
			this.ButtonPanel.Controls.Add(this.QueryStatusTextBox);
			this.ButtonPanel.Controls.Add(this.FormatUriCheckBox);
			this.ButtonPanel.Controls.Add(this.RunQueryButton);
			this.ButtonPanel.Controls.Add(this.OpenUrlButton);
			this.ButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 338, true);
			this.ButtonPanel.Name = "ButtonPanel";
			this.ButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 40, true);
			this.ButtonPanel.TabIndex = 1;
			// 
			// QueryStatusTextBox
			// 
			this.QueryStatusTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.QueryStatusTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
			this.QueryStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(353, 11, true);
			this.QueryStatusTextBox.Name = "QueryStatusTextBox";
			this.QueryStatusTextBox.ReadOnly = true;
			this.QueryStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 18, true);
			this.QueryStatusTextBox.TabIndex = 0;
			// 
			// FormatUriCheckBox
			// 
			this.FormatUriCheckBox.Checked = true;
			this.FormatUriCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.FormatUriCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FormatUriCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 8, true);
			this.FormatUriCheckBox.Name = "FormatUriCheckBox";
			this.FormatUriCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 24, true);
			this.FormatUriCheckBox.TabIndex = 1;
			this.FormatUriCheckBox.Text = "Format URI";
			this.FormatUriCheckBox.Click += new System.EventHandler(this.FormatUriCheckBox_Click);
			// 
			// RunQueryButton
			// 
			this.RunQueryButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RunQueryButton.IsCaptionOverridden = true;
			this.RunQueryButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.RunQueryButton.Name = "RunQueryButton";
			this.RunQueryButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 24, true);
			this.RunQueryButton.TabIndex = 2;
			this.RunQueryButton.Text = "&Run Query";
			this.RunQueryButton.ToolTipCaption = null;
			this.RunQueryButton.Click += new System.EventHandler(this.RunQueryButton_Click);
			// 
			// OpenUrlButton
			// 
			this.OpenUrlButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OpenUrlButton.IsCaptionOverridden = true;
			this.OpenUrlButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 8, true);
			this.OpenUrlButton.Name = "OpenUrlButton";
			this.OpenUrlButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 24, true);
			this.OpenUrlButton.TabIndex = 3;
			this.OpenUrlButton.Text = "Open in Browser";
			this.OpenUrlButton.ToolTipCaption = null;
			this.OpenUrlButton.Click += new System.EventHandler(this.OpenUrlButton_Click);
			// 
			// ResultPanel
			// 
			this.ResultPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.ResultPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ResultPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ResultPanel.Name = "ResultPanel";
			this.ResultPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 231, true);
			this.ResultPanel.TabIndex = 0;
			// 
			// QueryResultSplitPanel
			// 
			this.QueryResultSplitPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QueryResultSplitPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.QueryResultSplitPanel.Name = "QueryResultSplitPanel";
			this.QueryResultSplitPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// QueryResultSplitPanel.Panel1
			// 
			this.QueryResultSplitPanel.Panel1.Controls.Add(this.IndexSearchUriTextBox);
			this.QueryResultSplitPanel.Panel1.Controls.Add(this.ButtonPanel);
			this.QueryResultSplitPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 647, true);
			this.QueryResultSplitPanel.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(50);
			// 
			// QueryResultSplitPanel.Panel2
			// 
			this.QueryResultSplitPanel.Panel2.Controls.Add(this.ResultsSplitPanel);
			this.QueryResultSplitPanel.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(50);
			this.QueryResultSplitPanel.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(378);
			this.QueryResultSplitPanel.SplitterWidth = 6;
			this.QueryResultSplitPanel.TabIndex = 0;
			// 
			// ResultsSplitPanel
			// 
			this.ResultsSplitPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ResultsSplitPanel.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.ResultsSplitPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ResultsSplitPanel.Name = "ResultsSplitPanel";
			this.ResultsSplitPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// ResultsSplitPanel.Panel1
			// 
			this.ResultsSplitPanel.Panel1.Controls.Add(this.ResultPanel);
			// 
			// ResultsSplitPanel.Panel2
			// 
			this.ResultsSplitPanel.Panel2.Controls.Add(this.ResultsTextBox);
			this.ResultsSplitPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 264, true);
			this.ResultsSplitPanel.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(231);
			this.ResultsSplitPanel.SplitterWidth = 6;
			this.ResultsSplitPanel.TabIndex = 0;
			// 
			// ResultsTextBox
			// 
			this.ResultsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ResultsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ResultsTextBox.Multiline = true;
			this.ResultsTextBox.Name = "ResultsTextBox";
			this.ResultsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 28, true);
			this.ResultsTextBox.TabIndex = 0;
			// 
			// IndexSearchAnalyzerForm
			// 
			this.ClientSize = new System.Drawing.Size(962, 809);
			this.Controls.Add(this.QueryResultSplitPanel);
			this.KeyPreview = true;
			this.MinimumSize = new System.Drawing.Size(400, 300);
			this.Name = "IndexSearchAnalyzerForm";
			this.Text = "Index Search Analyzer";
			this.ButtonPanel.ResumeLayout(false);
			this.ButtonPanel.PerformLayout();
			this.QueryResultSplitPanel.Panel1.ResumeLayout(false);
			this.QueryResultSplitPanel.Panel1.PerformLayout();
			this.QueryResultSplitPanel.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.QueryResultSplitPanel)).EndInit();
			this.QueryResultSplitPanel.ResumeLayout(false);
			this.QueryResultSplitPanel.PerformLayout();
			this.ResultsSplitPanel.Panel1.ResumeLayout(false);
			this.ResultsSplitPanel.Panel2.ResumeLayout(false);
			this.ResultsSplitPanel.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ResultsSplitPanel)).EndInit();
			this.ResultsSplitPanel.ResumeLayout(false);
			this.ResultsSplitPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		private CargoWise.Windows.UI.KPanel ResultPanel;
		private CargoWise.Windows.UI.KPanel ButtonPanel;
		private CargoWise.Windows.UI.KTextBox IndexSearchUriTextBox;
		private CargoWise.Windows.UI.KButton RunQueryButton;
		private CargoWise.Windows.UI.KButton OpenUrlButton;
		private CargoWise.Windows.UI.KTextBox QueryStatusTextBox;
		private CargoWise.Windows.UI.KSplitContainer QueryResultSplitPanel;
		private CargoWise.Windows.UI.KSplitContainer ResultsSplitPanel;
		private CargoWise.Windows.UI.KTextBox ResultsTextBox;
		private CargoWise.Windows.UI.KCheckBox FormatUriCheckBox;
	}
}
