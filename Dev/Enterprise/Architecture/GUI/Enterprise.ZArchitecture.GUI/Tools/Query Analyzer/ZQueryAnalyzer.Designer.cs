using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.DevTools
{
	partial class ZQueryAnalyzerForm
	{
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.SQLTextBox = new CargoWise.Windows.UI.KTextBox();
			this.ButtonPanel = new CargoWise.Windows.UI.KPanel();
			this.StopButton = new CargoWise.Windows.UI.KButton();
			this.TransactionButton = new CargoWise.Windows.UI.KButton();
			this.CommandTextBox = new CargoWise.Windows.UI.KTextBox();
			this.RollbackButton = new CargoWise.Windows.UI.KButton();
			this.RunQueryButton = new CargoWise.Windows.UI.KButton();
			this.ClearButton = new CargoWise.Windows.UI.KButton();
			this.ResultPanel = new CargoWise.Windows.UI.KPanel();
			this.QueryResultSplitPanel = new CargoWise.Windows.UI.KSplitContainer();
			this.ResultsSplitPanel = new CargoWise.Windows.UI.KSplitContainer();
			this.ResultsTextBox = new CargoWise.Windows.UI.KTextBox();
			this.QueryAnalyzerToolTip = new System.Windows.Forms.ToolTip(this.components);
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
			// SQLTextBox
			// 
			this.SQLTextBox.AcceptsReturn = true;
			this.SQLTextBox.AcceptsTab = true;
			this.SQLTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SQLTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SQLTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SQLTextBox.MaxLength = 99999999;
			this.SQLTextBox.Multiline = true;
			this.SQLTextBox.Name = "SQLTextBox";
			this.SQLTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.SQLTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 236, true);
			this.SQLTextBox.TabIndex = 0;
			this.SQLTextBox.WordWrap = false;
			this.SQLTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.SQLTextBox_KeyUp);
			// 
			// ButtonPanel
			// 
			this.ButtonPanel.Controls.Add(this.StopButton);
			this.ButtonPanel.Controls.Add(this.TransactionButton);
			this.ButtonPanel.Controls.Add(this.CommandTextBox);
			this.ButtonPanel.Controls.Add(this.RollbackButton);
			this.ButtonPanel.Controls.Add(this.RunQueryButton);
			this.ButtonPanel.Controls.Add(this.ClearButton);
			this.ButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 236, true);
			this.ButtonPanel.Name = "ButtonPanel";
			this.ButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 40, true);
			this.ButtonPanel.TabIndex = 0;
			// 
			// StopButton
			// 
			this.StopButton.Enabled = false;
			this.StopButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.StopButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 8, true);
			this.StopButton.Name = "StopButton";
			this.StopButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 24, true);
			this.StopButton.TabIndex = 5;
			this.StopButton.Text = "&Stop";
			this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
			// 
			// TransactionButton
			// 
			this.TransactionButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.TransactionButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 8, true);
			this.TransactionButton.Name = "TransactionButton";
			this.TransactionButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 24, true);
			this.TransactionButton.TabIndex = 4;
			this.TransactionButton.Text = "&Begin Tran";
			this.TransactionButton.Click += new System.EventHandler(this.TransactionButton_Click);
			// 
			// CommandTextBox
			// 
			this.CommandTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CommandTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CommandTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(353, 11, true);
			this.CommandTextBox.Name = "CommandTextBox";
			this.CommandTextBox.ReadOnly = true;
			this.CommandTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 20, true);
			this.CommandTextBox.TabIndex = 3;
			this.CommandTextBox.TabStop = false;
			// 
			// RollbackButton
			// 
			this.RollbackButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RollbackButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 8, true);
			this.RollbackButton.Name = "RollbackButton";
			this.RollbackButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 24, true);
			this.RollbackButton.TabIndex = 3;
			this.RollbackButton.Text = "Rollback";
			this.RollbackButton.Click += new System.EventHandler(this.RollbackButton_Click);
			// 
			// RunQueryButton
			// 
			this.RunQueryButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RunQueryButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.RunQueryButton.Name = "RunQueryButton";
			this.RunQueryButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 24, true);
			this.RunQueryButton.TabIndex = 0;
			this.RunQueryButton.Text = "&Run Query";
			this.RunQueryButton.Click += new System.EventHandler(this.RunQueryButton_Click);
			// 
			// ClearButton
			// 
			this.ClearButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(234, 8, true);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 24, true);
			this.ClearButton.TabIndex = 2;
			this.ClearButton.Text = "&Clear";
			this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
			// 
			// ResultPanel
			// 
			this.ResultPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.ResultPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ResultPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ResultPanel.Name = "ResultPanel";
			this.ResultPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 161, true);
			this.ResultPanel.TabIndex = 2;
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
			this.QueryResultSplitPanel.Panel1.Controls.Add(this.SQLTextBox);
			this.QueryResultSplitPanel.Panel1.Controls.Add(this.ButtonPanel);
			this.QueryResultSplitPanel.Panel1MinSize = 50;
			// 
			// QueryResultSplitPanel.Panel2
			// 
			this.QueryResultSplitPanel.Panel2.Controls.Add(this.ResultsSplitPanel);
			this.QueryResultSplitPanel.Panel2MinSize = 50;
			this.QueryResultSplitPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 473, true);
			this.QueryResultSplitPanel.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(276);
			this.QueryResultSplitPanel.TabIndex = 1;
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
			this.ResultsSplitPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 193, true);
			this.ResultsSplitPanel.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(161);
			this.ResultsSplitPanel.TabIndex = 0;
			// 
			// ResultsTextBox
			// 
			this.ResultsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ResultsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ResultsTextBox.Multiline = true;
			this.ResultsTextBox.Name = "ResultsTextBox";
			this.ResultsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 28, true);
			this.ResultsTextBox.TabIndex = 2;
			// 
			// ZQueryAnalyzerForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 473, true);
			this.Controls.Add(this.QueryResultSplitPanel);
			this.KeyPreview = true;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			this.Name = "ZQueryAnalyzerForm";
			this.Text = "Z Query Analyzer";
			this.ButtonPanel.ResumeLayout(false);
			this.ButtonPanel.PerformLayout();
			this.QueryResultSplitPanel.Panel1.ResumeLayout(false);
			this.QueryResultSplitPanel.Panel1.PerformLayout();
			this.QueryResultSplitPanel.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.QueryResultSplitPanel)).EndInit();
			this.QueryResultSplitPanel.ResumeLayout(false);
			this.ResultsSplitPanel.Panel1.ResumeLayout(false);
			this.ResultsSplitPanel.Panel2.ResumeLayout(false);
			this.ResultsSplitPanel.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ResultsSplitPanel)).EndInit();
			this.ResultsSplitPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		private CargoWise.Windows.UI.KPanel ResultPanel;
		private CargoWise.Windows.UI.KPanel ButtonPanel;
		private CargoWise.Windows.UI.KTextBox SQLTextBox;		// SuppressCodeSmell Reason=ZQueryAnalyzerForm form is not part of Enterprise.				// SuppressCodeSmell Reason=ZQueryAnalyzerForm form is not part of Enterprise.				// SuppressCodeSmell Reason=ZQueryAnalyzerForm form is not part of Enterprise.
		private CargoWise.Windows.UI.KButton ClearButton;
		private CargoWise.Windows.UI.KButton RunQueryButton;
		private CargoWise.Windows.UI.KButton RollbackButton;
		private CargoWise.Windows.UI.KTextBox CommandTextBox;
		private System.Windows.Forms.ToolTip QueryAnalyzerToolTip;
		private CargoWise.Windows.UI.KButton TransactionButton;
		private CargoWise.Windows.UI.KButton StopButton;
		private CargoWise.Windows.UI.KSplitContainer QueryResultSplitPanel;
		private CargoWise.Windows.UI.KSplitContainer ResultsSplitPanel;
		private CargoWise.Windows.UI.KTextBox ResultsTextBox;
		private System.ComponentModel.IContainer components;
	}
}
