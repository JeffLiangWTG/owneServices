using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class DataTransferForm
	{
		protected ZButton BrowseButton;
		protected ZGroupBox LogGroupBox;
		protected ZTextBox FileNameTextBox;
		protected ZLabel RowsProcessedTitle;
		protected ZLabel RowsProcessedLabel;
		protected ZLabel RowsExcludedLabel;
		protected ZButton ProcessButton;
		protected CargoWise.Windows.UI.KProgressBar ProgressBar;
		protected ZLabel RowsIncludedLabel;
		protected ZLabel RowsIncludedTitle;
		protected CargoWise.Windows.UI.KPanel RowStatisticsPanel;
		protected CargoWise.Windows.UI.KListBox LogListBox;
		protected ZButton CloseButton;
		protected ZLabel ProgressLabel;
		protected ZLabel RowsExcludedTitle;

		new void InitializeComponent()
		{
			this.BrowseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FileNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LogGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LogListBox = new CargoWise.Windows.UI.KListBox();
			this.RowStatisticsPanel = new CargoWise.Windows.UI.KPanel();
			this.RowsProcessedTitle = new Enterprise.ZArchitecture.ZLabel();
			this.RowsProcessedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RowsIncludedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RowsIncludedTitle = new Enterprise.ZArchitecture.ZLabel();
			this.RowsExcludedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RowsExcludedTitle = new Enterprise.ZArchitecture.ZLabel();
			this.ProcessButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ProgressBar = new CargoWise.Windows.UI.KProgressBar();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ProgressLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LogGroupBox.SuspendLayout();
			this.RowStatisticsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 448, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 24, true);
			this.MainStatusBar.TabIndex = 8;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(360);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(361);
			// 
			// BrowseButton
			// 
			this.BrowseButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.BrowseButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataTransferForm|6e489286-847e-40e4-9eca-8dfae88d6ff5", "Browse");
			this.BrowseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 30, true);
			this.BrowseButton.Name = "BrowseButton";
			this.BrowseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.BrowseButton.TabIndex = 1;
			this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
			// 
			// FileNameTextBox
			// 
			this.FileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.FileNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FileNameTextBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataTransferForm|7b4997dc-65d5-460c-9575-c491c0fbe1b1", "Location of the file to import from");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.FileNameTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.FileNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 30, true);
			this.FileNameTextBox.Name = "FileNameTextBox";
			this.FileNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 20, true);
			this.FileNameTextBox.TabIndex = 2;
			// 
			// LogGroupBox
			// 
			this.LogGroupBox.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.LogGroupBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataTransferForm|53717808-8b1a-4fe0-82a3-437cdabf45e1", "Import Log");
			this.LogGroupBox.Controls.Add(this.LogListBox);
			this.LogGroupBox.Controls.Add(this.RowStatisticsPanel);
			this.LogGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 97, true);
			this.LogGroupBox.Name = "LogGroupBox";
			this.LogGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 312, true);
			this.LogGroupBox.TabIndex = 5;
			this.LogGroupBox.TabStop = false;
			// 
			// LogListBox
			// 
			this.LogListBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LogListBox.HorizontalScrollbar = true;
			this.LogListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LogListBox.Name = "LogListBox";
			this.LogListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 290, true);
			this.LogListBox.TabIndex = 0;
			// 
			// RowStatisticsPanel
			// 
			this.RowStatisticsPanel.Controls.Add(this.RowsProcessedTitle);
			this.RowStatisticsPanel.Controls.Add(this.RowsProcessedLabel);
			this.RowStatisticsPanel.Controls.Add(this.RowsIncludedLabel);
			this.RowStatisticsPanel.Controls.Add(this.RowsIncludedTitle);
			this.RowStatisticsPanel.Controls.Add(this.RowsExcludedLabel);
			this.RowStatisticsPanel.Controls.Add(this.RowsExcludedTitle);
			this.RowStatisticsPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.RowStatisticsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(549, 16, true);
			this.RowStatisticsPanel.Name = "RowStatisticsPanel";
			this.RowStatisticsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 293, true);
			this.RowStatisticsPanel.TabIndex = 1;
			// 
			// RowsProcessedTitle
			// 
			this.RowsProcessedTitle.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.RowsProcessedTitle.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataTransferForm|8320d532-660b-4055-84af-04fe237e3118", "Rows");
			this.RowsProcessedTitle.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.RowsProcessedTitle.Name = "RowsProcessedTitle";
			this.RowsProcessedTitle.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 22, true);
			this.RowsProcessedTitle.TabIndex = 0;
			// 
			// RowsProcessedLabel
			// 
			this.RowsProcessedLabel.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.RowsProcessedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 7, true);
			this.RowsProcessedLabel.Name = "RowsProcessedLabel";
			this.RowsProcessedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 22, true);
			this.RowsProcessedLabel.TabIndex = 1;
			this.RowsProcessedLabel.Text = "0";
			// 
			// RowsIncludedLabel
			// 
			this.RowsIncludedLabel.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.RowsIncludedLabel.ForeColor = System.Drawing.Color.Green;
			this.RowsIncludedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 30, true);
			this.RowsIncludedLabel.Name = "RowsIncludedLabel";
			this.RowsIncludedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 21, true);
			this.RowsIncludedLabel.TabIndex = 3;
			this.RowsIncludedLabel.Text = "0";
			// 
			// RowsIncludedTitle
			// 
			this.RowsIncludedTitle.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.RowsIncludedTitle.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataTransferForm|b49d00a3-3f56-429f-a1f4-9de1d561ee93", "Rows Imported");
			this.RowsIncludedTitle.ForeColor = System.Drawing.Color.Green;
			this.RowsIncludedTitle.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 30, true);
			this.RowsIncludedTitle.Name = "RowsIncludedTitle";
			this.RowsIncludedTitle.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.RowsIncludedTitle.TabIndex = 2;
			// 
			// RowsExcludedLabel
			// 
			this.RowsExcludedLabel.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.RowsExcludedLabel.ForeColor = System.Drawing.Color.Red;
			this.RowsExcludedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 52, true);
			this.RowsExcludedLabel.Name = "RowsExcludedLabel";
			this.RowsExcludedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 21, true);
			this.RowsExcludedLabel.TabIndex = 5;
			this.RowsExcludedLabel.Text = "0";
			// 
			// RowsExcludedTitle
			// 
			this.RowsExcludedTitle.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.RowsExcludedTitle.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataTransferForm|7d390e43-9a34-4a79-a51b-a3ba3fce0235", "Rows Excluded");
			this.RowsExcludedTitle.ForeColor = System.Drawing.Color.Red;
			this.RowsExcludedTitle.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 52, true);
			this.RowsExcludedTitle.Name = "RowsExcludedTitle";
			this.RowsExcludedTitle.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.RowsExcludedTitle.TabIndex = 4;
			// 
			// ProcessButton
			// 
			this.ProcessButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.ProcessButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataTransferForm|a46eb202-204d-4e5b-b130-094885744920", "Import");
			this.ProcessButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(572, 415, true);
			this.ProcessButton.Name = "ProcessButton";
			this.ProcessButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.ProcessButton.TabIndex = 6;
			this.ProcessButton.Click += new System.EventHandler(this.ProcessButton_Click);
			// 
			// ProgressBar
			// 
			this.ProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.ProgressBar.Enabled = false;
			this.ProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 74, true);
			this.ProgressBar.Name = "ProgressBar";
			this.ProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 15, true);
			this.ProgressBar.TabIndex = 4;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.CloseButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataTransferForm|25ceb208-76d5-4df4-a749-ef920ed3c1ee", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(652, 415, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.CloseButton.TabIndex = 7;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// ProgressLabel
			// 
			this.ProgressLabel.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.ProgressLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataTransferForm|a552e6c5-1f38-41e3-9b83-1d34d6204c94", "Progress");
			this.ProgressLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 53, true);
			this.ProgressLabel.Name = "ProgressLabel";
			this.ProgressLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 18, true);
			this.ProgressLabel.TabIndex = 9;
			// 
			// DataTransferForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 472, true);
			this.ControlBox = false;
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataTransferForm|57b772bc-7a7e-401e-8628-63f8fd7a4968", "Data Transfer");
			this.Controls.Add(this.ProgressLabel);
			this.Controls.Add(this.ProgressBar);
			this.Controls.Add(this.LogGroupBox);
			this.Controls.Add(this.FileNameTextBox);
			this.Controls.Add(this.BrowseButton);
			this.Controls.Add(this.ProcessButton);
			this.Controls.Add(this.CloseButton);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 453, true);
			this.Name = "DataTransferForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.ProcessButton, 0);
			this.Controls.SetChildIndex(this.BrowseButton, 0);
			this.Controls.SetChildIndex(this.FileNameTextBox, 0);
			this.Controls.SetChildIndex(this.LogGroupBox, 0);
			this.Controls.SetChildIndex(this.ProgressBar, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ProgressLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LogGroupBox.ResumeLayout(false);
			this.RowStatisticsPanel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
