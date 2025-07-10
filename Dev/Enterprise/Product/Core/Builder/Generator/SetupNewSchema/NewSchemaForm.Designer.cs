namespace Enterprise.Builder.Generator
{
	partial class NewSchemaForm
	{
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1014:EmbeddedIconRule")]
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewSchemaForm));
			this.CloseButton = new CargoWise.Windows.UI.KButton();
			this.SkippedFilesTextBox = new CargoWise.Windows.UI.KTextBox();
			this.SkippedFilesLabel = new CargoWise.Windows.UI.KLabel();
			this.ProgressTextBox = new CargoWise.Windows.UI.KTextBox();
			this.SaveToClipboardButton = new CargoWise.Windows.UI.KButton();
			this.StatusBar = new CargoWise.Windows.UI.KStatusBar();
			this.OutputPanel = new System.Windows.Forms.StatusBarPanel();
			((System.ComponentModel.ISupportInitialize)(this.OutputPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Location = new System.Drawing.Point(675, 541);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = new System.Drawing.Size(104, 23);
			this.CloseButton.TabIndex = 7;
			this.CloseButton.Text = "Close";
			this.CloseButton.Visible = false;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// SkippedFilesTextBox
			// 
			this.SkippedFilesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.SkippedFilesTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.SkippedFilesTextBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.SkippedFilesTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SkippedFilesTextBox.Location = new System.Drawing.Point(4, 416);
			this.SkippedFilesTextBox.Multiline = true;
			this.SkippedFilesTextBox.Name = "SkippedFilesTextBox";
			this.SkippedFilesTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.SkippedFilesTextBox.Size = new System.Drawing.Size(794, 120);
			this.SkippedFilesTextBox.TabIndex = 8;
			this.SkippedFilesTextBox.TabStop = false;
			// 
			// SkippedFilesLabel
			// 
			this.SkippedFilesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SkippedFilesLabel.Location = new System.Drawing.Point(8, 400);
			this.SkippedFilesLabel.Name = "SkippedFilesLabel";
			this.SkippedFilesLabel.Size = new System.Drawing.Size(144, 16);
			this.SkippedFilesLabel.TabIndex = 9;
			this.SkippedFilesLabel.Text = "Skipped Files";
			// 
			// ProgressTextBox
			// 
			this.ProgressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressTextBox.BackColor = System.Drawing.Color.White;
			this.ProgressTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ProgressTextBox.Location = new System.Drawing.Point(4, 8);
			this.ProgressTextBox.Multiline = true;
			this.ProgressTextBox.Name = "ProgressTextBox";
			this.ProgressTextBox.ReadOnly = true;
			this.ProgressTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.ProgressTextBox.Size = new System.Drawing.Size(794, 340);
			this.ProgressTextBox.TabIndex = 10;
			// 
			// SaveToClipboardButton
			// 
			this.SaveToClipboardButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveToClipboardButton.Location = new System.Drawing.Point(675, 355);
			this.SaveToClipboardButton.Name = "SaveToClipboardButton";
			this.SaveToClipboardButton.Size = new System.Drawing.Size(104, 23);
			this.SaveToClipboardButton.TabIndex = 11;
			this.SaveToClipboardButton.Text = "Save to Clipboard";
			this.SaveToClipboardButton.Visible = false;
			this.SaveToClipboardButton.Click += new System.EventHandler(this.SaveToClipboardButton_Click);
			// 
			// StatusBar
			// 
			this.StatusBar.Location = new System.Drawing.Point(0, 574);
			this.StatusBar.Name = "StatusBar";
			this.StatusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
						this.OutputPanel });
			this.StatusBar.ShowPanels = true;
			this.StatusBar.Size = new System.Drawing.Size(802, 24);
			this.StatusBar.TabIndex = 13;
			// 
			// OutputPanel
			// 
			this.OutputPanel.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
			this.OutputPanel.Name = "OutputPanel";
			this.OutputPanel.Width = 785;
			// 
			// NewSchemaForm
			// 
			this.ClientSize = new System.Drawing.Size(802, 598);
			this.Controls.Add(this.StatusBar);
			this.Controls.Add(this.SaveToClipboardButton);
			this.Controls.Add(this.ProgressTextBox);
			this.Controls.Add(this.SkippedFilesTextBox);
			this.Controls.Add(this.SkippedFilesLabel);
			this.Controls.Add(this.CloseButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Location = new System.Drawing.Point(400, 200);
			this.Name = "NewSchemaForm";
			this.Text = "Setup New Schema";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.NewSchemaForm_Closing);
			this.Load += new System.EventHandler(this.NewSchemaForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.OutputPanel)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private System.ComponentModel.Container components = null;
		private CargoWise.Windows.UI.KButton CloseButton;
		private CargoWise.Windows.UI.KButton SaveToClipboardButton;
		private CargoWise.Windows.UI.KTextBox SkippedFilesTextBox;
		private CargoWise.Windows.UI.KLabel SkippedFilesLabel;
		private CargoWise.Windows.UI.KStatusBar StatusBar;
		private System.Windows.Forms.StatusBarPanel OutputPanel;
		private CargoWise.Windows.UI.KTextBox ProgressTextBox;
	}
}
