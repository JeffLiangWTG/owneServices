namespace Enterprise.RemotePrinting.Client
{
	partial class PrintClientForm
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
				if (components != null)
				{
					components.Dispose();
				}
				LogWriter.UnregisterAllLogTargets();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "CargoWise One Web  is a seperate entity regardless of product")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1014:EmbeddedIconRule", Justification = "Embedding a print icon, not a product icon")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrintClientForm));
			this.StartButton = new System.Windows.Forms.Button();
			this.StopButton = new System.Windows.Forms.Button();
			this.CloseButton = new System.Windows.Forms.Button();
			this.LocalComputerNameLabel = new System.Windows.Forms.Label();
			this.LocalComputerNameTextBox = new System.Windows.Forms.TextBox();
			this.FormMenu = new System.Windows.Forms.MainMenu(this.components);
			this.FileMenuItem = new System.Windows.Forms.MenuItem();
			this.ConfigurationMenuItem = new System.Windows.Forms.MenuItem();
			this.LogFilesMenuItem = new System.Windows.Forms.MenuItem();
			this.ThreadMonitorMenuItem = new System.Windows.Forms.MenuItem();
			this.ExitMenuItem = new System.Windows.Forms.MenuItem();
			this.ConfigurationNameLabel = new System.Windows.Forms.Label();
			this.ConfigurationsComboBox = new System.Windows.Forms.ComboBox();
			this.ScanForNewPrintersButton = new System.Windows.Forms.Button();
			this.OutputRichTextBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// StartButton
			// 
			this.StartButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.StartButton.Location = new System.Drawing.Point(413, 48);
			this.StartButton.Name = "StartButton";
			this.StartButton.Size = new System.Drawing.Size(75, 23);
			this.StartButton.TabIndex = 5;
			this.StartButton.Text = "Start";
			this.StartButton.UseVisualStyleBackColor = true;
			this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
			// 
			// StopButton
			// 
			this.StopButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.StopButton.Enabled = false;
			this.StopButton.Location = new System.Drawing.Point(494, 48);
			this.StopButton.Name = "StopButton";
			this.StopButton.Size = new System.Drawing.Size(75, 23);
			this.StopButton.TabIndex = 6;
			this.StopButton.Text = "Stop";
			this.StopButton.UseVisualStyleBackColor = true;
			this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Location = new System.Drawing.Point(575, 48);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = new System.Drawing.Size(75, 23);
			this.CloseButton.TabIndex = 7;
			this.CloseButton.Text = "Close";
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// LocalComputerNameLabel
			// 
			this.LocalComputerNameLabel.AutoSize = true;
			this.LocalComputerNameLabel.Location = new System.Drawing.Point(9, 17);
			this.LocalComputerNameLabel.Name = "LocalComputerNameLabel";
			this.LocalComputerNameLabel.Size = new System.Drawing.Size(86, 13);
			this.LocalComputerNameLabel.TabIndex = 1;
			this.LocalComputerNameLabel.Text = "Computer Name:";
			// 
			// LocalComputerNameTextBox
			// 
			this.LocalComputerNameTextBox.Location = new System.Drawing.Point(129, 14);
			this.LocalComputerNameTextBox.Name = "LocalComputerNameTextBox";
			this.LocalComputerNameTextBox.ReadOnly = true;
			this.LocalComputerNameTextBox.Size = new System.Drawing.Size(278, 20);
			this.LocalComputerNameTextBox.TabIndex = 2;
			// 
			// FormMenu
			// 
			this.FormMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.FileMenuItem});
			// 
			// FileMenuItem
			// 
			this.FileMenuItem.Index = 0;
			this.FileMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.ConfigurationMenuItem,
            this.LogFilesMenuItem,
            this.ThreadMonitorMenuItem,
            this.ExitMenuItem});
			this.FileMenuItem.Text = "&File";
			// 
			// ConfigurationMenuItem
			// 
			this.ConfigurationMenuItem.Index = 0;
			this.ConfigurationMenuItem.Text = "&Configuration";
			this.ConfigurationMenuItem.Click += new System.EventHandler(this.ConfigurationToolStripMenuItem_Click);
			// 
			// LogFilesMenuItem
			// 
			this.LogFilesMenuItem.Index = 1;
			this.LogFilesMenuItem.Text = "Log Files";
			this.LogFilesMenuItem.Click += new System.EventHandler(this.LogFilesToolStripMenuItem_Click);
			// 
			// ThreadMonitorMenuItem
			// 
			this.ThreadMonitorMenuItem.Index = 2;
			this.ThreadMonitorMenuItem.Text = "&Thread Monitor (Hang Debug)";
			this.ThreadMonitorMenuItem.Click += new System.EventHandler(this.ThreadMonitorToolStripMenuItem_Click);
			// 
			// ExitMenuItem
			// 
			this.ExitMenuItem.Index = 3;
			this.ExitMenuItem.Text = "&Exit";
			this.ExitMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
			// 
			// ConfigurationNameLabel
			// 
			this.ConfigurationNameLabel.AutoSize = true;
			this.ConfigurationNameLabel.Location = new System.Drawing.Point(9, 51);
			this.ConfigurationNameLabel.Name = "ConfigurationNameLabel";
			this.ConfigurationNameLabel.Size = new System.Drawing.Size(103, 13);
			this.ConfigurationNameLabel.TabIndex = 4;
			this.ConfigurationNameLabel.Text = "Configuration Name:";
			// 
			// ConfigurationsComboBox
			// 
			this.ConfigurationsComboBox.FormattingEnabled = true;
			this.ConfigurationsComboBox.Location = new System.Drawing.Point(129, 48);
			this.ConfigurationsComboBox.Name = "ConfigurationsComboBox";
			this.ConfigurationsComboBox.Size = new System.Drawing.Size(278, 21);
			this.ConfigurationsComboBox.TabIndex = 5;
			this.ConfigurationsComboBox.SelectedIndexChanged += new System.EventHandler(this.ConfigurationsComboBox_SelectedIndexChanged);
			// 
			// ScanForNewPrintersButton
			// 
			this.ScanForNewPrintersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ScanForNewPrintersButton.Enabled = false;
			this.ScanForNewPrintersButton.Location = new System.Drawing.Point(494, 14);
			this.ScanForNewPrintersButton.Name = "ScanForNewPrintersButton";
			this.ScanForNewPrintersButton.Size = new System.Drawing.Size(155, 23);
			this.ScanForNewPrintersButton.TabIndex = 9;
			this.ScanForNewPrintersButton.Text = "Scan for new printers";
			this.ScanForNewPrintersButton.UseVisualStyleBackColor = true;
			this.ScanForNewPrintersButton.Click += new System.EventHandler(this.ScanForNewPrintersButton_Click);
			// 
			// OutputRichTextBox
			// 
			this.OutputRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.OutputRichTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OutputRichTextBox.Location = new System.Drawing.Point(12, 83);
			this.OutputRichTextBox.MaxLength = 2147483647;
			this.OutputRichTextBox.Multiline = true;
			this.OutputRichTextBox.Name = "OutputRichTextBox";
			this.OutputRichTextBox.ReadOnly = true;
			this.OutputRichTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.OutputRichTextBox.Size = new System.Drawing.Size(637, 372);
			this.OutputRichTextBox.TabIndex = 10;
			this.OutputRichTextBox.WordWrap = false;
			// 
			// PrintClientForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(664, 468);
			this.Controls.Add(this.OutputRichTextBox);
			this.Controls.Add(this.ScanForNewPrintersButton);
			this.Controls.Add(this.ConfigurationsComboBox);
			this.Controls.Add(this.ConfigurationNameLabel);
			this.Controls.Add(this.LocalComputerNameLabel);
			this.Controls.Add(this.StartButton);
			this.Controls.Add(this.LocalComputerNameTextBox);
			this.Controls.Add(this.StopButton);
			this.Controls.Add(this.CloseButton);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Menu = this.FormMenu;
			this.MinimumSize = new System.Drawing.Size(680, 450);
			this.Name = "PrintClientForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "CargoWise One WebPrint Client";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PrintClientForm_FormClosing);
			this.Load += new System.EventHandler(this.PrintClientForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button StartButton;
		private System.Windows.Forms.Button StopButton;
		private System.Windows.Forms.Button CloseButton;
		private System.Windows.Forms.Label LocalComputerNameLabel;
		private System.Windows.Forms.TextBox LocalComputerNameTextBox;
		private System.Windows.Forms.MenuItem FileMenuItem;
		private System.Windows.Forms.MenuItem ConfigurationMenuItem;
		private System.Windows.Forms.MenuItem ExitMenuItem;
		private System.Windows.Forms.MainMenu FormMenu;
		private System.Windows.Forms.MenuItem LogFilesMenuItem;
		private System.Windows.Forms.MenuItem ThreadMonitorMenuItem;
		private System.Windows.Forms.Label ConfigurationNameLabel;
		private System.Windows.Forms.ComboBox ConfigurationsComboBox;
		private System.Windows.Forms.Button ScanForNewPrintersButton;
		private System.Windows.Forms.TextBox OutputRichTextBox;
	}
}
