using System;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.OCR;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	public sealed partial class MagnifyForm : ZChildForm
	{
		Enterprise.ZArchitecture.GUI.ZPanel ImagePanel;
		Enterprise.ZArchitecture.GUI.ZToolBar MagnifyToolBar;
		System.Windows.Forms.ToolBarButton LeftButton;
		System.Windows.Forms.ToolBarButton UpButton;
		System.Windows.Forms.ToolBarButton DownButton;
		System.Windows.Forms.ToolBarButton RightButton;
		System.Windows.Forms.ToolBarButton Separator1Button;
		System.Windows.Forms.ToolBarButton OCRButton;
		System.Windows.Forms.ToolBarButton Separator2Button;
		System.Windows.Forms.ToolBarButton ClearSelectionButton;
		System.Windows.Forms.ToolBarButton SelectTextButton;
		System.Windows.Forms.ImageList ImageList;
		internal Enterprise.ZArchitecture.GUI.ZPictureBox PictureBox;
		System.Windows.Forms.ImageList ExpandCollapseImageList;
		CargoWise.Windows.UI.KPanel ToolbarPanel;
		internal CargoWise.Windows.UI.KButton ExpandCollapseButton;
		Enterprise.ZArchitecture.GUI.ZDropEdit ZoomDropEdit;
		Enterprise.ZArchitecture.ZLabel PercentLabel;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MagnifyForm));
			this.ImagePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.MagnifyToolBar = new Enterprise.ZArchitecture.GUI.ZToolBar();
			this.LeftButton = new System.Windows.Forms.ToolBarButton();
			this.UpButton = new System.Windows.Forms.ToolBarButton();
			this.DownButton = new System.Windows.Forms.ToolBarButton();
			this.RightButton = new System.Windows.Forms.ToolBarButton();
			this.Separator1Button = new System.Windows.Forms.ToolBarButton();
			this.SelectTextButton = new System.Windows.Forms.ToolBarButton();
			this.OCRButton = new System.Windows.Forms.ToolBarButton();
			this.ClearSelectionButton = new System.Windows.Forms.ToolBarButton();
			this.Separator2Button = new System.Windows.Forms.ToolBarButton();
			this.ImageList = new System.Windows.Forms.ImageList(this.components);
			this.ExpandCollapseButton = new CargoWise.Windows.UI.KButton();
			this.ExpandCollapseImageList = new System.Windows.Forms.ImageList(this.components);
			this.ToolbarPanel = new CargoWise.Windows.UI.KPanel();
			this.PercentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ZoomDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ImagePanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
			this.ToolbarPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 311, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 25, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentScanning.Business.MagnifyManager);
			// 
			// ImagePanel
			// 
			this.ImagePanel.BackColor = System.Drawing.SystemColors.Control;
			this.ImagePanel.Controls.Add(this.PictureBox);
			this.ImagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImagePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 30, true);
			this.ImagePanel.Name = "ImagePanel";
			this.ImagePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 306, true);
			this.ImagePanel.TabIndex = 0;
			// 
			// PictureBox
			// 
			this.PictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PictureBox.Name = "PictureBox";
			this.PictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 306, true);
			this.PictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.PictureBox.TabIndex = 1;
			this.PictureBox.TabStop = false;
			this.PictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureBox_MouseMove);
			this.PictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PictureBox_MouseDown);
			this.PictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBox_Paint);
			this.PictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PictureBox_MouseUp);
			// 
			// MagnifyToolBar
			// 
			this.MagnifyToolBar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
			this.LeftButton,
			this.UpButton,
			this.DownButton,
			this.RightButton,
			this.Separator1Button,
			this.SelectTextButton,
			this.OCRButton,
			this.ClearSelectionButton,
			this.Separator2Button });
			this.MagnifyToolBar.ButtonSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 16, true);
			this.MagnifyToolBar.Divider = false;
			this.MagnifyToolBar.Dock = System.Windows.Forms.DockStyle.None;
			this.MagnifyToolBar.DropDownArrows = true;
			this.MagnifyToolBar.ImageList = this.ImageList;
			this.MagnifyToolBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.MagnifyToolBar.Name = "MagnifyToolBar";
			this.MagnifyToolBar.ShowToolTips = true;
			this.MagnifyToolBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 26, true);
			this.MagnifyToolBar.TabIndex = 1;
			this.MagnifyToolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.MagnifyToolBar_ButtonClick);
			// 
			// LeftButton
			// 
			this.LeftButton.ImageIndex = 2;
			this.LeftButton.Name = "LeftButton";
			this.LeftButton.Tag = "LeftButton";
			// 
			// UpButton
			// 
			this.UpButton.ImageIndex = 4;
			this.UpButton.Name = "UpButton";
			this.UpButton.Tag = "UpButton";
			// 
			// DownButton
			// 
			this.DownButton.ImageIndex = 1;
			this.DownButton.Name = "DownButton";
			this.DownButton.Tag = "DownButton";
			// 
			// RightButton
			// 
			this.RightButton.ImageIndex = 3;
			this.RightButton.Name = "RightButton";
			this.RightButton.Tag = "RightButton";
			// 
			// Separator1Button
			// 
			this.Separator1Button.Name = "Separator1Button";
			this.Separator1Button.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// SelectTextButton
			// 
			this.SelectTextButton.ImageIndex = 9;
			this.SelectTextButton.Name = "SelectTextButton";
			this.SelectTextButton.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			this.SelectTextButton.Tag = "SelectTextButton";
			// 
			// OCRButton
			// 
			this.OCRButton.ImageIndex = 10;
			this.OCRButton.Name = "OCRButton";
			this.OCRButton.Tag = "OCRButton";
			// 
			// ClearSelectionButton
			// 
			this.ClearSelectionButton.ImageIndex = 11;
			this.ClearSelectionButton.Name = "ClearSelectionButton";
			this.ClearSelectionButton.Tag = "ClearSelectionButton";
			// 
			// Separator2Button
			// 
			this.Separator2Button.Name = "Separator2Button";
			this.Separator2Button.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// ImageList
			// 
			ZImageListDpiScalingHelper.SetScaledImagesFromImageListStreamer(this.ImageList, (System.Windows.Forms.ImageListStreamer)resources.GetObject("ImageList.ImageStream"));
			this.ImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.ImageList.Images.SetKeyName(0, "");
			this.ImageList.Images.SetKeyName(1, "");
			this.ImageList.Images.SetKeyName(2, "");
			this.ImageList.Images.SetKeyName(3, "");
			this.ImageList.Images.SetKeyName(4, "");
			this.ImageList.Images.SetKeyName(5, "");
			this.ImageList.Images.SetKeyName(6, "");
			this.ImageList.Images.SetKeyName(7, "");
			this.ImageList.Images.SetKeyName(8, "");
			this.ImageList.Images.SetKeyName(9, "");
			this.ImageList.Images.SetKeyName(10, "");
			this.ImageList.Images.SetKeyName(11, "");
			// 
			// ExpandCollapseButton
			// 
			this.ExpandCollapseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ExpandCollapseButton.ImageIndex = 1;
			this.ExpandCollapseButton.ImageList = this.ExpandCollapseImageList;
			this.ExpandCollapseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(528, 4, true);
			this.ExpandCollapseButton.Name = "ExpandCollapseButton";
			this.ExpandCollapseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.ExpandCollapseButton.TabIndex = 5;
			this.ExpandCollapseButton.Click += new System.EventHandler(this.ExpandCollapseButton_Click);
			// 
			// ExpandCollapseImageList
			// 
			ZImageListDpiScalingHelper.SetScaledImagesFromImageListStreamer(this.ExpandCollapseImageList, (System.Windows.Forms.ImageListStreamer)(resources.GetObject("ExpandCollapseImageList.ImageStream")));
			this.ExpandCollapseImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.ExpandCollapseImageList.Images.SetKeyName(0, "");
			this.ExpandCollapseImageList.Images.SetKeyName(1, "");
			// 
			// ToolbarPanel
			// 
			this.ToolbarPanel.Controls.Add(this.PercentLabel);
			this.ToolbarPanel.Controls.Add(this.ZoomDropEdit);
			this.ToolbarPanel.Controls.Add(this.MagnifyToolBar);
			this.ToolbarPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ToolbarPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ToolbarPanel.Name = "ToolbarPanel";
			this.ToolbarPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 30, true);
			this.ToolbarPanel.TabIndex = 2;
			// 
			// PercentLabel
			// 
			this.PercentLabel.AutoSize = true;
			this.PercentLabel.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("MagnifyForm|ee3b1efd-d79b-4010-91ff-028663e5bee4", "%");
			this.PercentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 7, true);
			this.PercentLabel.Name = "PercentLabel";
			this.PercentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.PercentLabel.TabIndex = 5;
			// 
			// ZoomDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ZoomDropEdit, "Zoom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentScanning.Business.MagnifyManager)(null)).Zoom)));
			this.ZoomDropEdit.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("MagnifyForm|bda00e3f-aeba-46e1-9005-c91fe736550b", "Zoom");
			this.ZoomDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 4, true);
			this.ZoomDropEdit.Name = "ZoomDropEdit";
			this.ZoomDropEdit.ShowDescriptionBox = false;
			this.ZoomDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ZoomDropEdit.TabIndex = 4;
			this.ZoomDropEdit.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ZoomDropEdit_KeyDown);
			// 
			// MagnifyForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 336, true);
			this.Controls.Add(this.ExpandCollapseButton);
			this.Controls.Add(this.ImagePanel);
			this.Controls.Add(this.ToolbarPanel);
			this.DataSourceAssemblyName = "DocumentScanning";
			this.DataSourceType = typeof(Enterprise.DocumentScanning.Business.MagnifyManager);
			this.DataSourceTypeName = "Enterprise.DocumentScanning.Business.MagnifyManager";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 93, true);
			this.Name = "MagnifyForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Load += new System.EventHandler(this.MagnifyForm_Load);
			this.Resize += new System.EventHandler(this.MagnifyForm_Resize);
			this.Controls.SetChildIndex(this.ToolbarPanel, 0);
			this.Controls.SetChildIndex(this.ImagePanel, 0);
			this.Controls.SetChildIndex(this.ExpandCollapseButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ImagePanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
			this.ToolbarPanel.ResumeLayout(false);
			this.ToolbarPanel.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}
