using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ImageSelectionControl
	{
		#region Component Designer generated code
		void InitializeComponent()
		{
			this.BrowseButton = new ZButton();
			this.fPictureBox = new ZPictureBox();
			this.ClearImageButton = new ZButton();
			this.SaveImageButton = new ZButton();
			this.FileDialog = new ZOpenFileDialog();
			this.SaveFileDialog = new ZSaveFileDialog();
			this.ViewModeButton = new ZButton();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			((ISupportInitialize)(this.fPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// BrowseButton
			// 
			this.BrowseButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.BrowseButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ImageSelectionControl|9c59ffc8-545c-4023-b612-efa212685f1c", "Choose &File");
			this.BrowseButton.IsCaptionOverridden = false;
			this.BrowseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 155, true);
			this.BrowseButton.Name = "BrowseButton";
			this.BrowseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.BrowseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.BrowseButton.TabIndex = 5;
			this.BrowseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.BrowseButton.ToolTipCaption = null;
			this.BrowseButton.Click += new EventHandler(this.BrowseButton_Click);
			// 
			// fPictureBox
			// 
			this.fPictureBox.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.fPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.fPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.fPictureBox.Name = "fPictureBox";
			this.fPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 147, true);
			this.fPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.fPictureBox.TabIndex = 4;
			this.fPictureBox.TabStop = false;
			// 
			// ClearImageButton
			// 
			this.ClearImageButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.ClearImageButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ImageSelectionControl|7f62ccde-b21d-4bd6-8bb4-876fd1581ae5", "Clea&r");
			this.ClearImageButton.IsCaptionOverridden = false;
			this.ClearImageButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 155, true);
			this.ClearImageButton.Name = "ClearImageButton";
			this.ClearImageButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ClearImageButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ClearImageButton.TabIndex = 6;
			this.ClearImageButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ClearImageButton.ToolTipCaption = null;
			this.ClearImageButton.Click += new EventHandler(this.ClearImageButton_Click);
			// 
			// SaveImageButton
			// 
			this.SaveImageButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.SaveImageButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ImageSelectionControl|eeed5f55-7724-4e40-84f8-78d1da6b00e6", "Save");
			this.SaveImageButton.IsCaptionOverridden = false;
			this.SaveImageButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 155, true);
			this.SaveImageButton.Name = "SaveImageButton";
			this.SaveImageButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SaveImageButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SaveImageButton.TabIndex = 7;
			this.SaveImageButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SaveImageButton.ToolTipCaption = null;
			this.SaveImageButton.Click += new EventHandler(this.SaveImageButton_Click);
			// 
			// FileDialog
			// 
			this.FileDialog.AddExtension = true;
			this.FileDialog.CheckFileExists = true;
			this.FileDialog.CheckPathExists = true;
			this.FileDialog.DefaultExt = "";
			this.FileDialog.DereferenceLinks = true;
			this.FileDialog.Filter = "";
			this.FileDialog.FilterIndex = 1;
			this.FileDialog.InitialDirectory = "";
			this.FileDialog.Multiselect = false;
			this.FileDialog.ReadOnlyChecked = false;
			this.FileDialog.RestoreDirectory = false;
			this.FileDialog.ShowHelp = false;
			this.FileDialog.SupportMultiDottedExtensions = false;
			this.FileDialog.Title = "";
			this.FileDialog.ValidateNames = true;
			this.FileDialog.FileOk += new CancelEventHandler(this.FileDialog_FileOk);
			// 
			// SaveFileDialog
			// 
			this.SaveFileDialog.AddExtension = true;
			this.SaveFileDialog.CheckFileExists = false;
			this.SaveFileDialog.CheckPathExists = true;
			this.SaveFileDialog.CreatePrompt = false;
			this.SaveFileDialog.DefaultExt = "";
			this.SaveFileDialog.DereferenceLinks = true;
			this.SaveFileDialog.Filter = "";
			this.SaveFileDialog.FilterIndex = 1;
			this.SaveFileDialog.InitialDirectory = "";
			this.SaveFileDialog.OverwritePrompt = true;
			this.SaveFileDialog.RestoreDirectory = false;
			this.SaveFileDialog.ShowHelp = false;
			this.SaveFileDialog.SupportMultiDottedExtensions = false;
			this.SaveFileDialog.Title = "";
			this.SaveFileDialog.ValidateNames = true;
			this.SaveFileDialog.FileOk += new CancelEventHandler(this.SaveFileDialog_FileOk);
			// 
			// ViewModeButton
			// 
			this.ViewModeButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.ViewModeButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("22344758-b3c4-4a2c-9c35-ea6211f479b9", "Stretch View");
			this.ViewModeButton.IsCaptionOverridden = false;
			this.ViewModeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 155, true);
			this.ViewModeButton.Name = "ViewModeButton";
			this.ViewModeButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ViewModeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ViewModeButton.TabIndex = 8;
			this.ViewModeButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ViewModeButton.ToolTipCaption = null;
			this.ViewModeButton.Click += new EventHandler(this.ViewModeButton_Click);
			// 
			// ImageSelectionControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ViewModeButton);
			this.Controls.Add(this.BrowseButton);
			this.Controls.Add(this.fPictureBox);
			this.Controls.Add(this.ClearImageButton);
			this.Controls.Add(this.SaveImageButton);
			this.Name = "ImageSelectionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 179, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			((ISupportInitialize)(this.fPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion
	}
}
