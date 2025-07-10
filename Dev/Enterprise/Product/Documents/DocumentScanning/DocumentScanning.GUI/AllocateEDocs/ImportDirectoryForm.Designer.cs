using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	public partial class ImportDirectoryForm : ZChildForm
	{
		Enterprise.ZArchitecture.GUI.ZFolderBrowserDialog ImportDirectoryDialog;
		Enterprise.ZArchitecture.GUI.ZButton DirectoryBrowseButton;
		protected Enterprise.ZArchitecture.GUI.ZButton ACancelButton;
		Enterprise.ZArchitecture.ZTextBox FilePathTextBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox DeleteSourceFilesAfterImportCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox IncludeSubdirectoriesCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZButton OKButton;
		Enterprise.ZArchitecture.ZLabel zLabel1;

		new void InitializeComponent()
		{
			this.ImportDirectoryDialog = new Enterprise.ZArchitecture.GUI.ZFolderBrowserDialog();
			this.DirectoryBrowseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ACancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FilePathTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeleteSourceFilesAfterImportCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IncludeSubdirectoriesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 126, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 23, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(223);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(223);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentScanning.Business.FileImporter);
			// 
			// ImportDirectoryDialog
			// 
			this.ImportDirectoryDialog.CreateDirectory = false;
			this.ImportDirectoryDialog.Description = "";
			this.ImportDirectoryDialog.RequireMappablePath = false;
			this.ImportDirectoryDialog.RootFolder = System.Environment.SpecialFolder.Desktop;
			this.ImportDirectoryDialog.ShowNewFolderButton = true;
			// 
			// DirectoryBrowseButton
			// 
			this.DirectoryBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DirectoryBrowseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(411, 45, true);
			this.DirectoryBrowseButton.Name = "DirectoryBrowseButton";
			this.DirectoryBrowseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.DirectoryBrowseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 23, true);
			this.DirectoryBrowseButton.TabIndex = 2;
			this.DirectoryBrowseButton.Text = "...";
			this.DirectoryBrowseButton.ToolTipCaption = null;
			this.DirectoryBrowseButton.Click += new System.EventHandler(this.DirectoryBrowseButton_Click);
			// 
			// ACancelButton
			// 
			this.ACancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ACancelButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ImportDirectoryForm|d716bb29-79a7-42f8-a598-594610187619", "Cancel");
			this.ACancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ACancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 96, true);
			this.ACancelButton.Name = "ACancelButton";
			this.ACancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ACancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ACancelButton.TabIndex = 7;
			this.ACancelButton.ToolTipCaption = null;
			// 
			// FilePathTextBox
			// 
			this.FilePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FilePathTextBox, "DefaultImportDirectory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.DocumentScanning.Business.FileImporter)(null)).DefaultImportDirectory)));
			this.FilePathTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FilePathTextBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ImportDirectoryForm|527bb8c1-bad3-419d-b950-938a868e8e12", "Import Directory");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.FilePathTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.FilePathTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 48, true);
			this.FilePathTextBox.Name = "FilePathTextBox";
			this.FilePathTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 20, true);
			this.FilePathTextBox.TabIndex = 1;
			// 
			// DeleteSourceFilesAfterImportCheckBox
			// 
			this.DeleteSourceFilesAfterImportCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DeleteSourceFilesAfterImportCheckBox, "DeleteSourceFilesAfterImport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.DocumentScanning.Business.FileImporter)(null)).DeleteSourceFilesAfterImport)));
			this.DeleteSourceFilesAfterImportCheckBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ImportDirectoryForm|57e577d4-06aa-4c28-8557-7fa91592d0e8", "Delete Source Files After Import");
			this.DeleteSourceFilesAfterImportCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DeleteSourceFilesAfterImportCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 75, true);
			this.DeleteSourceFilesAfterImportCheckBox.Name = "DeleteSourceFilesAfterImportCheckBox";
			this.DeleteSourceFilesAfterImportCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 17, true);
			this.DeleteSourceFilesAfterImportCheckBox.TabIndex = 3;
			// 
			// IncludeSubdirectoriesCheckBox
			// 
			this.IncludeSubdirectoriesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IncludeSubdirectoriesCheckBox, "IncludeSubdirectories");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.DocumentScanning.Business.FileImporter)(null)).IncludeSubdirectories)));
			this.IncludeSubdirectoriesCheckBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ImportDirectoryForm|a946ee5f-b40c-48ad-8e6a-261e49330fc3", "Include Sub-Folders");
			this.IncludeSubdirectoriesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludeSubdirectoriesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 75, true);
			this.IncludeSubdirectoriesCheckBox.Name = "IncludeSubdirectoriesCheckBox";
			this.IncludeSubdirectoriesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 17, true);
			this.IncludeSubdirectoriesCheckBox.TabIndex = 4;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 96, true);
			this.OKButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ImportDirectoryForm|64786d9d-6838-4916-8eb9-0e8a7a992b53", "OK");
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 6;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ImportDirectoryForm|2ca23fe5-e117-4bac-ae96-b4ed1e4e24ef", "All BMP, GIF, JPG, PNG, TIF, and PDF files in the directory will be imported.");
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 7, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 23, true);
			this.zLabel1.TabIndex = 10;
			// 
			// ImportDirectoryForm
			// 
			this.CancelButton = this.ACancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ImportDirectoryForm|adf0bd3f-298e-448d-9974-86a63e78aadd", "Select Import Directory");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 149, true);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.IncludeSubdirectoriesCheckBox);
			this.Controls.Add(this.DeleteSourceFilesAfterImportCheckBox);
			this.Controls.Add(this.FilePathTextBox);
			this.Controls.Add(this.ACancelButton);
			this.Controls.Add(this.DirectoryBrowseButton);
			this.DataSourceAssemblyName = "DocumentScanning";
			this.DataSourceType = typeof(Enterprise.DocumentScanning.Business.FileImporter);
			this.DataSourceTypeName = "Enterprise.DocumentScanning.Business.FileImporter";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Name = "ImportDirectoryForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.DirectoryBrowseButton, 0);
			this.Controls.SetChildIndex(this.ACancelButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.FilePathTextBox, 0);
			this.Controls.SetChildIndex(this.DeleteSourceFilesAfterImportCheckBox, 0);
			this.Controls.SetChildIndex(this.IncludeSubdirectoriesCheckBox, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
