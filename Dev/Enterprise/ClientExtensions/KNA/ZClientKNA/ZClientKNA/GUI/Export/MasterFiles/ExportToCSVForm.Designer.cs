using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public abstract partial class ExportToCSVForm : ZChildForm
	{
		protected Enterprise.ZArchitecture.GUI.ZButton SelectFileButton;
		protected Enterprise.ZArchitecture.ZTextBox FileNameTextBox;
		protected CargoWise.Windows.UI.KProgressBar ProgressBar;
		protected Enterprise.ZArchitecture.GUI.ZButton StartButton;
		protected Enterprise.ZArchitecture.ZLabel label2;
		protected CargoWise.Windows.UI.KListBox OutputListBox;
		protected Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		protected ZButton CopyLogToClipboardButton;
		protected Enterprise.ZArchitecture.ZLabel zLabel1;

		new void InitializeComponent()
		{
			this.SelectFileButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FileNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StartButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ProgressBar = new CargoWise.Windows.UI.KProgressBar();
			this.label2 = new Enterprise.ZArchitecture.ZLabel();
			this.OutputListBox = new CargoWise.Windows.UI.KListBox();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CopyLogToClipboardButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 523, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 28, true);
			// 
			// SelectFileButton
			// 
			this.SelectFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectFileButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(548, 36, true);
			this.SelectFileButton.Name = "SelectFileButton";
			this.SelectFileButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 22, true);
			this.SelectFileButton.TabIndex = 2;
			this.SelectFileButton.Text = Res.GetString("ExportToCSVForm|SelectFileButton", "Browse...");
			// 
			// FileNameTextBox
			// 
			this.FileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.FileNameTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("ExportToCSVForm|5270371f-d9e2-45a8-9a27-6c31dc843952", "Export To CSV File");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FileNameTextBox, false);
			this.FileNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 36, true);
			this.FileNameTextBox.Name = "FileNameTextBox";
			this.FileNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 20, true);
			this.FileNameTextBox.TabIndex = 1;
			// 
			// StartButton
			// 
			this.StartButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.StartButton.BackColor = System.Drawing.Color.MediumSeaGreen;
			this.StartButton.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			this.StartButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(548, 488, true);
			this.StartButton.Name = "StartButton";
			this.StartButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 22, true);
			this.StartButton.TabIndex = 6;
			this.StartButton.Text = Res.GetString("ExportToCSVForm|StartButton", "Start Export");
			this.StartButton.UseVisualStyleBackColor = false;
			// 
			// ProgressBar
			// 
			this.ProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 488, true);
			this.ProgressBar.Name = "ProgressBar";
			this.ProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 20, true);
			this.ProgressBar.TabIndex = 5;
			// 
			// label2
			// 
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 64, true);
			this.label2.Name = "label2";
			this.label2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 16, true);
			this.label2.TabIndex = 3;
			this.label2.Text = Res.GetString("ExportToCSVForm|label2", "Export Log:");
			// 
			// OutputListBox
			// 
			this.OutputListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.OutputListBox.Font = new System.Drawing.Font(OFont.NormalFontName, 8F);
			this.OutputListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 84, true);
			this.OutputListBox.Name = "OutputListBox";
			this.OutputListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 394, true);
			this.OutputListBox.TabIndex = 4;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Enabled = false;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(548, 527, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 22, true);
			this.CloseButton.TabIndex = 9;
			this.CloseButton.Text = Res.GetString("ExportToCSVForm|CloseButton", "Close");
			this.CloseButton.Visible = false;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// CopyLogToClipboardButton
			// 
			this.CopyLogToClipboardButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CopyLogToClipboardButton.Enabled = false;
			this.CopyLogToClipboardButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 526, true);
			this.CopyLogToClipboardButton.Name = "CopyLogToClipboardButton";
			this.CopyLogToClipboardButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 23, true);
			this.CopyLogToClipboardButton.TabIndex = 8;
			this.CopyLogToClipboardButton.Text = Res.GetString("ExportToCSVForm|CopyLogToClipboardButton", "Copy Log to Clipboard");
			this.CopyLogToClipboardButton.Visible = false;
			this.CopyLogToClipboardButton.Click += new System.EventHandler(this.CopyLogToClipboardButton_Click);
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 19, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 14, true);
			this.zLabel1.TabIndex = 10;
			this.zLabel1.Text = Res.GetString("ExportToCSVForm|zLabel1", "Location of export file:");
			// 
			// ExportToCSVForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 551, true);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.CopyLogToClipboardButton);
			this.Controls.Add(this.OutputListBox);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.StartButton);
			this.Controls.Add(this.SelectFileButton);
			this.Controls.Add(this.FileNameTextBox);
			this.Controls.Add(this.ProgressBar);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(668, 580, true);
			this.Name = "ExportToCSVForm";
			this.Text = Res.GetString("ExportToCSVForm|ExportToCSVForm", "Export To CSV File");
			this.Controls.SetChildIndex(this.ProgressBar, 0);
			this.Controls.SetChildIndex(this.FileNameTextBox, 0);
			this.Controls.SetChildIndex(this.SelectFileButton, 0);
			this.Controls.SetChildIndex(this.StartButton, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.OutputListBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CopyLogToClipboardButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
