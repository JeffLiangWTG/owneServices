using System;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.ServiceTask;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.Modules.AirCargo
{
	public partial class DeveloperToolsForm : ZForm
	{
		ZButton uploadDownloadButton;
		Enterprise.ZArchitecture.ZTextBox outputBox;
		ZGroupBox zGroupBox1;
		ZButton queueProcessorButton;
		ZButton imageImportButton;
		Enterprise.Core.Forms.ZPostingButtonsUserControl oPostingButtonsUserControl;
		ZGroupBox ChildPackagesGroupBox;

		protected override void InitializeComponent()
		{
			this.uploadDownloadButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.outputBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ChildPackagesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.queueProcessorButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.imageImportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.oPostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ChildPackagesGroupBox.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 500, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(706, 24, true);
			// 
			// uploadDownloadButton
			// 
			this.uploadDownloadButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.uploadDownloadButton.Name = "uploadDownloadButton";
			this.uploadDownloadButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 22, true);
			this.uploadDownloadButton.TabIndex = 8;
			this.uploadDownloadButton.Text = "Run BISI Upload/Download";
			this.uploadDownloadButton.UseVisualStyleBackColor = true;
			this.uploadDownloadButton.Click += new System.EventHandler(this.uploadButton_Click);
			// 
			// outputBox
			// 
			this.outputBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.outputBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.outputBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.outputBox.Multiline = true;
			this.outputBox.Name = "outputBox";
			this.outputBox.ReadOnly = true;
			this.outputBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.outputBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 288, true);
			this.outputBox.TabIndex = 24;
			// 
			// ChildPackagesGroupBox
			// 
			this.ChildPackagesGroupBox.Controls.Add(this.queueProcessorButton);
			this.ChildPackagesGroupBox.Controls.Add(this.imageImportButton);
			this.ChildPackagesGroupBox.Controls.Add(this.uploadDownloadButton);
			this.ChildPackagesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.ChildPackagesGroupBox.Name = "ChildPackagesGroupBox";
			this.ChildPackagesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 116, true);
			this.ChildPackagesGroupBox.TabIndex = 2;
			this.ChildPackagesGroupBox.TabStop = false;
			this.ChildPackagesGroupBox.Text = "BISI";
			// 
			// queueProcessorButton
			// 
			this.queueProcessorButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 75, true);
			this.queueProcessorButton.Name = "queueProcessorButton";
			this.queueProcessorButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 22, true);
			this.queueProcessorButton.TabIndex = 10;
			this.queueProcessorButton.Text = "Run BISI QueueProcessor";
			this.queueProcessorButton.UseVisualStyleBackColor = true;
			this.queueProcessorButton.Click += new System.EventHandler(this.queueProcessorButton_Click);
			// 
			// imageImportButton
			// 
			this.imageImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 47, true);
			this.imageImportButton.Name = "imageImportButton";
			this.imageImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 22, true);
			this.imageImportButton.TabIndex = 9;
			this.imageImportButton.Text = "Run BISI Image Import";
			this.imageImportButton.UseVisualStyleBackColor = true;
			this.imageImportButton.Click += new System.EventHandler(this.imageImportButton_Click);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.Controls.Add(this.outputBox);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 134, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 307, true);
			this.zGroupBox1.TabIndex = 26;
			this.zGroupBox1.TabStop = false;
			this.zGroupBox1.Text = "Output:";
			// 
			// oPostingButtonsUserControl
			// 
			this.oPostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.oPostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(454, 458, true);
			this.oPostingButtonsUserControl.Name = "oPostingButtonsUserControl";
			this.oPostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 25, true);
			this.oPostingButtonsUserControl.TabIndex = 27;
			this.oPostingButtonsUserControl.Visible = false;
			// 
			// DeveloperToolsForm
			// 
			this.CaptionRenderingEnabled = false;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(706, 524, true);
			this.Controls.Add(this.oPostingButtonsUserControl);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.ChildPackagesGroupBox);
			this.Name = "DeveloperToolsForm";
			this.Text = "Developer Debug Tools";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ChildPackagesGroupBox, 0);
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			this.Controls.SetChildIndex(this.oPostingButtonsUserControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ChildPackagesGroupBox.ResumeLayout(false);
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}
