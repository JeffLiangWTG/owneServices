namespace Enterprise.Loader
{
	partial class ConfigurationDialog
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
				components.Dispose();
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
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.headerLabel = new System.Windows.Forms.Label();
			this.serverNameLabel = new System.Windows.Forms.Label();
			this.serverNameTextBox = new System.Windows.Forms.TextBox();
			this.connectButton = new System.Windows.Forms.Button();
			this.comboBoxInstance = new System.Windows.Forms.ComboBox();
			this.tlpMainLayout = new System.Windows.Forms.TableLayoutPanel();
			this.databaseNameTextBox = new System.Windows.Forms.TextBox();
			this.databaseNameLabel = new System.Windows.Forms.Label();
			this.pnlConnect = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.tlpMainLayout.SuspendLayout();
			this.pnlConnect.SuspendLayout();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Location = new System.Drawing.Point(25, 25);
			this.pictureBox1.Margin = new System.Windows.Forms.Padding(15);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(140, 140);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureBox1.TabIndex = 1;
			this.pictureBox1.TabStop = false;
			// 
			// headerLabel
			// 
			this.headerLabel.Location = new System.Drawing.Point(-1, 57);
			this.headerLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.headerLabel.Name = "headerLabel";
			this.headerLabel.Size = new System.Drawing.Size(343, 37);
			this.headerLabel.TabIndex = 2;
			this.headerLabel.Text = "Connect to an instance of the product";
			this.headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// serverNameLabel
			// 
			this.serverNameLabel.AutoSize = true;
			this.serverNameLabel.Dock = System.Windows.Forms.DockStyle.Right;
			this.serverNameLabel.Location = new System.Drawing.Point(45, 180);
			this.serverNameLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.serverNameLabel.Name = "serverNameLabel";
			this.serverNameLabel.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.serverNameLabel.Size = new System.Drawing.Size(133, 33);
			this.serverNameLabel.TabIndex = 3;
			this.serverNameLabel.Text = "Server Name:";
			this.serverNameLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			this.serverNameLabel.Visible = false;
			// 
			// serverNameTextBox
			// 
			this.serverNameTextBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.serverNameTextBox.Location = new System.Drawing.Point(183, 182);
			this.serverNameTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 2, 2);
			this.serverNameTextBox.Name = "serverNameTextBox";
			this.serverNameTextBox.Size = new System.Drawing.Size(592, 29);
			this.serverNameTextBox.TabIndex = 0;
			this.serverNameTextBox.Visible = false;
			// 
			// connectButton
			// 
			this.connectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.connectButton.Location = new System.Drawing.Point(632, 266);
			this.connectButton.Margin = new System.Windows.Forms.Padding(10, 20, 0, 20);
			this.connectButton.Name = "connectButton";
			this.connectButton.Size = new System.Drawing.Size(145, 43);
			this.connectButton.TabIndex = 2;
			this.connectButton.Text = "Connect";
			this.connectButton.UseVisualStyleBackColor = true;
			this.connectButton.Click += new System.EventHandler(this.conntectButton_Click);
			// 
			// comboBoxInstance
			// 
			this.comboBoxInstance.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.comboBoxInstance.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxInstance.FormattingEnabled = true;
			this.comboBoxInstance.Location = new System.Drawing.Point(0, 96);
			this.comboBoxInstance.Margin = new System.Windows.Forms.Padding(2);
			this.comboBoxInstance.Name = "comboBoxInstance";
			this.comboBoxInstance.Size = new System.Drawing.Size(591, 32);
			this.comboBoxInstance.TabIndex = 0;
			this.comboBoxInstance.SelectedIndexChanged += new System.EventHandler(this.comboBoxInstance_SelectedIndexChanged);
			// 
			// tlpMainLayout
			// 
			this.tlpMainLayout.AutoSize = true;
			this.tlpMainLayout.ColumnCount = 2;
			this.tlpMainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tlpMainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tlpMainLayout.Controls.Add(this.databaseNameTextBox, 1, 2);
			this.tlpMainLayout.Controls.Add(this.connectButton, 1, 3);
			this.tlpMainLayout.Controls.Add(this.databaseNameLabel, 0, 2);
			this.tlpMainLayout.Controls.Add(this.serverNameTextBox, 1, 1);
			this.tlpMainLayout.Controls.Add(this.serverNameLabel, 0, 1);
			this.tlpMainLayout.Controls.Add(this.pictureBox1, 0, 0);
			this.tlpMainLayout.Controls.Add(this.pnlConnect, 1, 0);
			this.tlpMainLayout.Dock = System.Windows.Forms.DockStyle.Top;
			this.tlpMainLayout.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.AddColumns;
			this.tlpMainLayout.Location = new System.Drawing.Point(0, 0);
			this.tlpMainLayout.MinimumSize = new System.Drawing.Size(700, 200);
			this.tlpMainLayout.Name = "tlpMainLayout";
			this.tlpMainLayout.Padding = new System.Windows.Forms.Padding(10, 10, 20, 10);
			this.tlpMainLayout.RowCount = 4;
			this.tlpMainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tlpMainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tlpMainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tlpMainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tlpMainLayout.Size = new System.Drawing.Size(797, 339);
			this.tlpMainLayout.TabIndex = 9;
			// 
			// databaseNameTextBox
			// 
			this.databaseNameTextBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.databaseNameTextBox.Location = new System.Drawing.Point(183, 215);
			this.databaseNameTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 2, 2);
			this.databaseNameTextBox.Name = "databaseNameTextBox";
			this.databaseNameTextBox.Size = new System.Drawing.Size(592, 29);
			this.databaseNameTextBox.TabIndex = 1;
			this.databaseNameTextBox.Visible = false;
			// 
			// databaseNameLabel
			// 
			this.databaseNameLabel.AutoSize = true;
			this.databaseNameLabel.Dock = System.Windows.Forms.DockStyle.Right;
			this.databaseNameLabel.Location = new System.Drawing.Point(19, 213);
			this.databaseNameLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.databaseNameLabel.Name = "databaseNameLabel";
			this.databaseNameLabel.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.databaseNameLabel.Size = new System.Drawing.Size(159, 33);
			this.databaseNameLabel.TabIndex = 5;
			this.databaseNameLabel.Text = "Database Name:";
			this.databaseNameLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			this.databaseNameLabel.Visible = false;
			// 
			// pnlConnect
			// 
			this.pnlConnect.Controls.Add(this.comboBoxInstance);
			this.pnlConnect.Controls.Add(this.headerLabel);
			this.pnlConnect.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlConnect.Location = new System.Drawing.Point(183, 13);
			this.pnlConnect.Name = "pnlConnect";
			this.pnlConnect.Size = new System.Drawing.Size(591, 128);
			this.pnlConnect.TabIndex = 6;
			// 
			// ConfigurationDialog
			// 
			this.AcceptButton = this.connectButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ClientSize = new System.Drawing.Size(797, 334);
			this.Controls.Add(this.tlpMainLayout);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Margin = new System.Windows.Forms.Padding(2);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(820, 250);
			this.Name = "ConfigurationDialog";
			this.ShowIcon = false;
			this.Text = "Configuration";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.tlpMainLayout.ResumeLayout(false);
			this.tlpMainLayout.PerformLayout();
			this.pnlConnect.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal System.Windows.Forms.PictureBox pictureBox1;
		internal System.Windows.Forms.Label headerLabel;
		internal System.Windows.Forms.TextBox serverNameTextBox;
		internal System.Windows.Forms.Button connectButton;
		internal System.Windows.Forms.ComboBox comboBoxInstance;
		internal System.Windows.Forms.TableLayoutPanel tlpMainLayout;
		internal System.Windows.Forms.TextBox databaseNameTextBox;
		internal System.Windows.Forms.Panel pnlConnect;
		internal System.Windows.Forms.Label serverNameLabel;
		internal System.Windows.Forms.Label databaseNameLabel;
	}
}
