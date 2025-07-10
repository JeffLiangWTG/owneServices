using CargoWise.Common.Testing;

namespace Enterprise.DataTools.DbBackupAndRestore.GUI
{
	partial class DbServerBrowseForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
				components.Dispose();
				fDirectoryBrowser?.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DbServerBrowseForm));
			this.DirectoryTreeView = new CargoWise.Windows.UI.KTreeView();
			this.DirectoryTreeImageList = new System.Windows.Forms.ImageList(this.components);
			this.SelectedPathTextBox = new CargoWise.Windows.UI.KTextBox();
			this.SelectedPathLabel = new CargoWise.Windows.UI.KLabel();
			this.CancelAndCloseButton = new CargoWise.Windows.UI.KButton();
			this.OkButton = new CargoWise.Windows.UI.KButton();
			this.FilesOfTypeComboBox = new CargoWise.Windows.UI.KComboBox();
			this.FilesOfTypeLabel = new CargoWise.Windows.UI.KLabel();
			this.SuspendLayout();
			// 
			// DirectoryTreeView
			// 
			this.DirectoryTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.DirectoryTreeView.ImageIndex = 0;
			this.DirectoryTreeView.ImageList = this.DirectoryTreeImageList;
			this.DirectoryTreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.DirectoryTreeView.Name = "DirectoryTreeView";
			this.DirectoryTreeView.SelectedImageIndex = 0;
			this.DirectoryTreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 465, true);
			this.DirectoryTreeView.TabIndex = 0;
			// 
			// DirectoryTreeImageList
			// 
			this.DirectoryTreeImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("DirectoryTreeImageList.ImageStream")));
			this.DirectoryTreeImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.DirectoryTreeImageList.Images.SetKeyName(0, "Folder.ico");
			this.DirectoryTreeImageList.Images.SetKeyName(1, "File.ico");
			// 
			// SelectedPathTextBox
			// 
			this.SelectedPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.SelectedPathTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 483, true);
			this.SelectedPathTextBox.Name = "SelectedPathTextBox";
			this.SelectedPathTextBox.ReadOnly = true;
			this.SelectedPathTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(373, 20, true);
			this.SelectedPathTextBox.TabIndex = 5;
			// 
			// SelectedPathLabel
			// 
			this.SelectedPathLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SelectedPathLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 482, true);
			this.SelectedPathLabel.Name = "SelectedPathLabel";
			this.SelectedPathLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 20, true);
			this.SelectedPathLabel.TabIndex = 6;
			this.SelectedPathLabel.Text = "Selected Path:";
			this.SelectedPathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// CancelAndCloseButton
			// 
			this.CancelAndCloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelAndCloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelAndCloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 536, true);
			this.CancelAndCloseButton.Name = "CancelAndCloseButton";
			this.CancelAndCloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 28, true);
			this.CancelAndCloseButton.TabIndex = 8;
			this.CancelAndCloseButton.Text = "Cancel";
			// 
			// OkButton
			// 
			this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(278, 536, true);
			this.OkButton.Name = "OkButton";
			this.OkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 28, true);
			this.OkButton.TabIndex = 7;
			this.OkButton.Text = "OK";
			// 
			// FilesOfTypeComboBox
			// 
			this.FilesOfTypeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.FilesOfTypeComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
			this.FilesOfTypeComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this.FilesOfTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.FilesOfTypeComboBox.FormattingEnabled = true;
			this.FilesOfTypeComboBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 509, true);
			this.FilesOfTypeComboBox.Name = "FilesOfTypeComboBox";
			this.FilesOfTypeComboBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(373, 21, true);
			this.FilesOfTypeComboBox.TabIndex = 9;
			this.FilesOfTypeComboBox.Visible = false;
			// 
			// FilesOfTypeLabel
			// 
			this.FilesOfTypeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.FilesOfTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 510, true);
			this.FilesOfTypeLabel.Name = "FilesOfTypeLabel";
			this.FilesOfTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 20, true);
			this.FilesOfTypeLabel.TabIndex = 10;
			this.FilesOfTypeLabel.Text = "Files of Type:";
			this.FilesOfTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.FilesOfTypeLabel.Visible = false;
			// 
			// DbServerBrowseForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(494, 576, true);
			this.Controls.Add(this.FilesOfTypeLabel);
			this.Controls.Add(this.FilesOfTypeComboBox);
			this.Controls.Add(this.CancelAndCloseButton);
			this.Controls.Add(this.OkButton);
			this.Controls.Add(this.SelectedPathTextBox);
			this.Controls.Add(this.SelectedPathLabel);
			this.Controls.Add(this.DirectoryTreeView);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 500, true);
			this.Name = "DbServerBrowseForm";
			this.Text = "DB Server File/Folder Browser";
			this.Load += new System.EventHandler(this.DbServerBrowseForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTreeView DirectoryTreeView;
		protected CargoWise.Windows.UI.KTextBox SelectedPathTextBox;
		private CargoWise.Windows.UI.KLabel SelectedPathLabel;
		private CargoWise.Windows.UI.KButton CancelAndCloseButton;
		private CargoWise.Windows.UI.KButton OkButton;
		private System.Windows.Forms.ImageList DirectoryTreeImageList;
		private CargoWise.Windows.UI.KComboBox FilesOfTypeComboBox;
		private CargoWise.Windows.UI.KLabel FilesOfTypeLabel;
	}
}
