using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Imaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	public partial class DocumentFullScreenPreviewForm : ZChildForm
	{
		protected DocumentPictureBox DocumentPreviewPictureBox;
		protected Enterprise.ZArchitecture.GUI.ZPanel ImagePanel;
		protected Enterprise.ZArchitecture.GUI.ZButton CloseUnboundButton;
		protected Enterprise.ZArchitecture.GUI.ZMenuItem FitToHeightMenuItem;
		protected Enterprise.ZArchitecture.GUI.ZMenuItem FitToWidthMenuItem;
		private System.Windows.Forms.ContextMenu ImageFullScreenMenu;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DocumentFullScreenPreviewForm));
			this.ImagePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DocumentPreviewPictureBox = new Enterprise.DocumentScanning.GUI.DocumentFullScreenPreviewForm.DocumentPictureBox();
			this.ImageFullScreenMenu = new System.Windows.Forms.ContextMenu();
			this.FitToHeightMenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
			this.FitToWidthMenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
			this.CloseUnboundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ImagePanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocumentPreviewPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 602, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 23, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(408);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(408);
			// 
			// ImagePanel
			// 
			this.ImagePanel.AutoScroll = true;
			this.ImagePanel.Controls.Add(this.DocumentPreviewPictureBox);
			this.ImagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImagePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ImagePanel.Name = "ImagePanel";
			this.ImagePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 625, true);
			this.ImagePanel.TabIndex = 3;
			this.ImagePanel.ClientSizeChanged += new System.EventHandler(this.HandleClientSizeChanged);
			// 
			// DocumentPreviewPictureBox
			// 
			this.DocumentPreviewPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.DocumentPreviewPictureBox.ContextMenu = this.ImageFullScreenMenu;
			this.DocumentPreviewPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocumentPreviewPictureBox.Name = "DocumentPreviewPictureBox";
			this.DocumentPreviewPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 480, true);
			this.DocumentPreviewPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.DocumentPreviewPictureBox.TabIndex = 3;
			this.DocumentPreviewPictureBox.TabStop = false;
			// 
			// ImageFullScreenMenu
			// 
			this.ImageFullScreenMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.FitToHeightMenuItem,
			this.FitToWidthMenuItem });
			// 
			// FitToHeightMenuItem
			// 
			this.FitToHeightMenuItem.Index = 0;
			this.FitToHeightMenuItem.RadioCheck = true;
			this.FitToHeightMenuItem.Click += new System.EventHandler(this.FitToHeightMenuItem_Click);
			// 
			// FitToWidthMenuItem
			// 
			this.FitToWidthMenuItem.Index = 1;
			this.FitToWidthMenuItem.RadioCheck = true;
			this.FitToWidthMenuItem.Click += new System.EventHandler(this.FitToWidthMenuItem_Click);
			// 
			// CloseUnboundButton
			// 
			this.CloseUnboundButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseUnboundButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseUnboundButton.Image = ((System.Drawing.Image)(resources.GetObject("CloseUnboundButton.Image")));
			this.CloseUnboundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(769, 9, true);
			this.CloseUnboundButton.Name = "CloseUnboundButton";
			this.CloseUnboundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 21, true);
			this.CloseUnboundButton.TabIndex = 5;
			this.CloseUnboundButton.TabStop = false;
			this.CloseUnboundButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.CloseUnboundButton.Click += new System.EventHandler(this.CloseUnboundButton_Click);
			// 
			// DocumentFullScreenPreviewForm
			// 

			this.CancelButton = this.CloseUnboundButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("18a13b9f-ba6c-475f-800a-9bca94ac81bd", "Document Preview");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 625, true);
			this.Controls.Add(this.CloseUnboundButton);
			this.Controls.Add(this.ImagePanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MinimizeBox = false;
			this.Name = "DocumentFullScreenPreviewForm";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.Controls.SetChildIndex(this.ImagePanel, 0);
			this.Controls.SetChildIndex(this.CloseUnboundButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ImagePanel.ResumeLayout(false);
			this.ImagePanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocumentPreviewPictureBox)).EndInit();
			this.ResumeLayout(false);
		}

		private System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.DocumentPreviewPictureBox.Image = null;

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
