namespace Enterprise.Customs.CA.GUI
{
	public partial class DataImportForm
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
			if (disposing)
			{
				if (this.dataImporter != null)
				{
					this.dataImporter.OnImportStart -= DataImporter_OnImportStart;
					this.dataImporter.OnShowNotification -= DataImporter_OnShowMessage;
					this.dataImporter.OnProgress -= DataImporter_OnProgress;
					this.dataImporter.OnImportFinished -= DataImporter_OnImportFinished;
				}

				if (components != null)
				{
					components.Dispose();
				}
				if (OpenFileDialog != null)
				{
					OpenFileDialog.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.OpenFileDialog = new Enterprise.ZArchitecture.GUI.ZOpenFileDialog();
			this.FileNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BrowseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OutputTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OutputGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ImportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ProgressBar = new CargoWise.Windows.UI.KProgressBar();
			this.ProgressCaptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ProgressLabel = new Enterprise.ZArchitecture.ZLabel();
			this.LinesProcessedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.LinesCountLabel = new Enterprise.ZArchitecture.ZLabel();
			this.LinesInvalidLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ProgressPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OutputGroupBox.SuspendLayout();
			this.ProgressPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 381, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 24, true);
			this.MainStatusBar.TabIndex = 8;
			// 
			// OpenFileDialog
			// 
			this.OpenFileDialog.AddExtension = true;
			this.OpenFileDialog.CheckFileExists = true;
			this.OpenFileDialog.CheckPathExists = true;
			this.OpenFileDialog.DefaultExt = "";
			this.OpenFileDialog.DereferenceLinks = true;
			this.OpenFileDialog.Filter = "CSV Files|*.csv";
			this.OpenFileDialog.FilterIndex = 1;
			this.OpenFileDialog.InitialDirectory = "";
			this.OpenFileDialog.Multiselect = false;
			this.OpenFileDialog.ReadOnlyChecked = false;
			this.OpenFileDialog.RestoreDirectory = false;
			this.OpenFileDialog.ShowHelp = false;
			this.OpenFileDialog.SupportMultiDottedExtensions = false;
			this.OpenFileDialog.Title = "";
			this.OpenFileDialog.ValidateNames = true;
			// 
			// FileNameTextBox
			// 
			this.FileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FileNameTextBox, false);
			this.FileNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 12, true);
			this.FileNameTextBox.Name = "FileNameTextBox";
			this.FileNameTextBox.ReadOnly = true;
			this.FileNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 20, true);
			this.FileNameTextBox.TabIndex = 1;
			// 
			// BrowseButton
			// 
			this.BrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BrowseButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("DataImportForm|f99f04f2-0014-41c9-9217-386224bf328a", "Browse");
			this.BrowseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(474, 10, true);
			this.BrowseButton.Name = "BrowseButton";
			this.BrowseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.BrowseButton.TabIndex = 2;
			this.BrowseButton.UseVisualStyleBackColor = true;
			this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
			// 
			// OutputTextBox
			// 
			this.OutputTextBox.BackColor = System.Drawing.SystemColors.Info;
			this.OutputTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OutputTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OutputTextBox.IsDynamicMultiline = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OutputTextBox, false);
			this.OutputTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OutputTextBox.Multiline = true;
			this.OutputTextBox.Name = "OutputTextBox";
			this.OutputTextBox.ReadOnly = true;
			this.OutputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.OutputTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(531, 230, true);
			this.OutputTextBox.TabIndex = 0;
			// 
			// OutputGroupBox
			// 
			this.OutputGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.OutputGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("DataImportForm|8e3a4ec4-fa28-4360-b5ce-3ed8d69c297d", "Output");
			this.OutputGroupBox.Controls.Add(this.OutputTextBox);
			this.OutputGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 97, true);
			this.OutputGroupBox.Name = "OutputGroupBox";
			this.OutputGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(537, 249, true);
			this.OutputGroupBox.TabIndex = 5;
			this.OutputGroupBox.TabStop = false;
			// 
			// ImportButton
			// 
			this.ImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ImportButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("DataImportForm|83d5f5f4-1fbe-4c56-b9fa-70049f34bc7f", "Import");
			this.ImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(391, 352, true);
			this.ImportButton.Name = "ImportButton";
			this.ImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 23, true);
			this.ImportButton.TabIndex = 6;
			this.ImportButton.UseVisualStyleBackColor = true;
			this.ImportButton.Click += new System.EventHandler(this.ImportButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(474, 352, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 7;
			this.CloseButton.UseVisualStyleBackColor = true;
			// 
			// ProgressBar
			// 
			this.ProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 24, true);
			this.ProgressBar.Name = "ProgressBar";
			this.ProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 23, true);
			this.ProgressBar.TabIndex = 4;
			// 
			// ProgressCaptionLabel
			// 
			this.ProgressCaptionLabel.AutoSize = true;
			this.ProgressCaptionLabel.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("DataImportForm|a612baec-17b7-45e0-91b3-60510b2027e6", "Progress:");
			this.ProgressCaptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 3, true);
			this.ProgressCaptionLabel.Name = "ProgressCaptionLabel";
			this.ProgressCaptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.ProgressCaptionLabel.TabIndex = 0;
			// 
			// ProgressLabel
			// 
			this.ProgressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(492, 29, true);
			this.ProgressLabel.Name = "ProgressLabel";
			this.ProgressLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 13, true);
			this.ProgressLabel.TabIndex = 5;
			// 
			// LinesProcessedLabel
			// 
			this.LinesProcessedLabel.AutoSize = true;
			this.LinesProcessedLabel.ForeColor = System.Drawing.Color.Green;
			this.LinesProcessedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 3, true);
			this.LinesProcessedLabel.Name = "LinesProcessedLabel";
			this.LinesProcessedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.LinesProcessedLabel.TabIndex = 1;
			// 
			// LinesCountLabel
			// 
			this.LinesCountLabel.AutoSize = true;
			this.LinesCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(426, 3, true);
			this.LinesCountLabel.Name = "LinesCountLabel";
			this.LinesCountLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.LinesCountLabel.TabIndex = 3;
			this.LinesCountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// LinesInvalidLabel
			// 
			this.LinesInvalidLabel.AutoSize = true;
			this.LinesInvalidLabel.ForeColor = System.Drawing.Color.Red;
			this.LinesInvalidLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 3, true);
			this.LinesInvalidLabel.Name = "LinesInvalidLabel";
			this.LinesInvalidLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.LinesInvalidLabel.TabIndex = 2;
			this.LinesInvalidLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ProgressPanel
			// 
			this.ProgressPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressPanel.Controls.Add(this.LinesCountLabel);
			this.ProgressPanel.Controls.Add(this.ProgressCaptionLabel);
			this.ProgressPanel.Controls.Add(this.LinesProcessedLabel);
			this.ProgressPanel.Controls.Add(this.ProgressBar);
			this.ProgressPanel.Controls.Add(this.ProgressLabel);
			this.ProgressPanel.Controls.Add(this.LinesInvalidLabel);
			this.ProgressPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 38, true);
			this.ProgressPanel.Name = "ProgressPanel";
			this.ProgressPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 53, true);
			this.ProgressPanel.TabIndex = 4;
			// 
			// DataImportForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 405, true);
			this.Controls.Add(this.FileNameTextBox);
			this.Controls.Add(this.BrowseButton);
			this.Controls.Add(this.OutputGroupBox);
			this.Controls.Add(this.ImportButton);
			this.Controls.Add(this.ProgressPanel);
			this.Controls.Add(this.CloseButton);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(577, 443, true);
			this.Name = "DataImportForm";
			this.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b5a8e0f5-969d-4f5c-b82e-66351f7fc15b", "Data Import");
			this.ShowInTaskbar = false;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DataImportForm_FormClosing);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.ProgressPanel, 0);
			this.Controls.SetChildIndex(this.ImportButton, 0);
			this.Controls.SetChildIndex(this.OutputGroupBox, 0);
			this.Controls.SetChildIndex(this.BrowseButton, 0);
			this.Controls.SetChildIndex(this.FileNameTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OutputGroupBox.ResumeLayout(false);
			this.OutputGroupBox.PerformLayout();
			this.ProgressPanel.ResumeLayout(false);
			this.ProgressPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZOpenFileDialog OpenFileDialog;
		protected Enterprise.ZArchitecture.ZTextBox FileNameTextBox;
		protected Enterprise.ZArchitecture.GUI.ZButton BrowseButton;
		protected Enterprise.ZArchitecture.ZTextBox OutputTextBox;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox OutputGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZButton ImportButton;
		protected Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		protected CargoWise.Windows.UI.KProgressBar ProgressBar;
		protected Enterprise.ZArchitecture.ZLabel ProgressCaptionLabel;
		protected Enterprise.ZArchitecture.ZLabel ProgressLabel;
		protected Enterprise.ZArchitecture.ZLabel LinesProcessedLabel;
		protected Enterprise.ZArchitecture.ZLabel LinesCountLabel;
		protected Enterprise.ZArchitecture.ZLabel LinesInvalidLabel;
		protected ZArchitecture.GUI.ZPanel ProgressPanel;
	}
}
