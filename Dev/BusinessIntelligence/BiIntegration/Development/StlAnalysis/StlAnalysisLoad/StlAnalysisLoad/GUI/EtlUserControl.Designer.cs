namespace Enterprise.StlAnalysis.Load
{
	partial class EtlUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.txbOutputTextBox = new System.Windows.Forms.TextBox();
			this.txbErrorTextBox = new System.Windows.Forms.TextBox();
			this.btnCollectDataButton = new System.Windows.Forms.Button();
			this.serverTextBox = new System.Windows.Forms.TextBox();
			this.serverLabel = new System.Windows.Forms.Label();
			this.databaseLabel = new System.Windows.Forms.Label();
			this.databaseTextBox = new System.Windows.Forms.TextBox();
			this.lastLoadMonthLabel = new System.Windows.Forms.Label();
			this.lastLoadMonthTextBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// txbOutputTextBox
			// 
			this.txbOutputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txbOutputTextBox.BackColor = System.Drawing.Color.Snow;
			this.txbOutputTextBox.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txbOutputTextBox.Location = new System.Drawing.Point(3, 72);
			this.txbOutputTextBox.MaxLength = 65536;
			this.txbOutputTextBox.Multiline = true;
			this.txbOutputTextBox.Name = "txbOutputTextBox";
			this.txbOutputTextBox.ReadOnly = true;
			this.txbOutputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txbOutputTextBox.Size = new System.Drawing.Size(1022, 400);
			this.txbOutputTextBox.TabIndex = 7;
			this.txbOutputTextBox.Tag = "";
			// 
			// txbErrorTextBox
			// 
			this.txbErrorTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txbErrorTextBox.BackColor = System.Drawing.Color.Snow;
			this.txbErrorTextBox.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txbErrorTextBox.Location = new System.Drawing.Point(3, 478);
			this.txbErrorTextBox.MaxLength = 65536;
			this.txbErrorTextBox.Multiline = true;
			this.txbErrorTextBox.Name = "txbErrorTextBox";
			this.txbErrorTextBox.ReadOnly = true;
			this.txbErrorTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txbErrorTextBox.Size = new System.Drawing.Size(1022, 182);
			this.txbErrorTextBox.TabIndex = 8;
			this.txbErrorTextBox.Tag = "";
			// 
			// btnCollectDataButton
			// 
			this.btnCollectDataButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnCollectDataButton.FlatAppearance.BorderSize = 0;
			this.btnCollectDataButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnCollectDataButton.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnCollectDataButton.Image = global::Enterprise.StlAnalysis.Load.Properties.Resources.Start_24p;
			this.btnCollectDataButton.Location = new System.Drawing.Point(3, 3);
			this.btnCollectDataButton.Name = "btnCollectDataButton";
			this.btnCollectDataButton.Size = new System.Drawing.Size(132, 63);
			this.btnCollectDataButton.TabIndex = 6;
			this.btnCollectDataButton.Text = "Load Data";
			this.btnCollectDataButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.btnCollectDataButton.UseVisualStyleBackColor = false;
			this.btnCollectDataButton.Click += new System.EventHandler(this.btnCollectDataButton_Click);
			// 
			// serverTextBox
			// 
			this.serverTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.serverTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.serverTextBox.Location = new System.Drawing.Point(230, 8);
			this.serverTextBox.Name = "serverTextBox";
			this.serverTextBox.Size = new System.Drawing.Size(498, 26);
			this.serverTextBox.TabIndex = 10;
			// 
			// serverLabel
			// 
			this.serverLabel.AutoSize = true;
			this.serverLabel.Location = new System.Drawing.Point(141, 11);
			this.serverLabel.Name = "serverLabel";
			this.serverLabel.Size = new System.Drawing.Size(59, 20);
			this.serverLabel.TabIndex = 11;
			this.serverLabel.Text = "Server:";
			// 
			// databaseLabel
			// 
			this.databaseLabel.AutoSize = true;
			this.databaseLabel.Location = new System.Drawing.Point(141, 43);
			this.databaseLabel.Name = "databaseLabel";
			this.databaseLabel.Size = new System.Drawing.Size(83, 20);
			this.databaseLabel.TabIndex = 12;
			this.databaseLabel.Text = "Database:";
			// 
			// databaseTextBox
			// 
			this.databaseTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.databaseTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.databaseTextBox.Location = new System.Drawing.Point(230, 40);
			this.databaseTextBox.Name = "databaseTextBox";
			this.databaseTextBox.Size = new System.Drawing.Size(498, 26);
			this.databaseTextBox.TabIndex = 13;
			// 
			// lastLoadMonthLabel
			// 
			this.lastLoadMonthLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lastLoadMonthLabel.AutoSize = true;
			this.lastLoadMonthLabel.Location = new System.Drawing.Point(734, 11);
			this.lastLoadMonthLabel.Name = "lastLoadMonthLabel";
			this.lastLoadMonthLabel.Size = new System.Drawing.Size(133, 20);
			this.lastLoadMonthLabel.TabIndex = 15;
			this.lastLoadMonthLabel.Text = "Last Load Month:";
			// 
			// lastLoadMonthTextBox
			// 
			this.lastLoadMonthTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lastLoadMonthTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.lastLoadMonthTextBox.Location = new System.Drawing.Point(873, 8);
			this.lastLoadMonthTextBox.Name = "lastLoadMonthTextBox";
			this.lastLoadMonthTextBox.Size = new System.Drawing.Size(152, 26);
			this.lastLoadMonthTextBox.TabIndex = 14;
			// 
			// EtlUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(225)))), ((int)(((byte)(220)))));
			this.Controls.Add(this.lastLoadMonthLabel);
			this.Controls.Add(this.lastLoadMonthTextBox);
			this.Controls.Add(this.databaseTextBox);
			this.Controls.Add(this.databaseLabel);
			this.Controls.Add(this.serverLabel);
			this.Controls.Add(this.serverTextBox);
			this.Controls.Add(this.txbErrorTextBox);
			this.Controls.Add(this.txbOutputTextBox);
			this.Controls.Add(this.btnCollectDataButton);
			this.Name = "EtlUserControl";
			this.Size = new System.Drawing.Size(1028, 663);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txbOutputTextBox;
		private System.Windows.Forms.Button btnCollectDataButton;
		private System.Windows.Forms.TextBox txbErrorTextBox;
		private System.Windows.Forms.TextBox serverTextBox;
		private System.Windows.Forms.Label serverLabel;
		private System.Windows.Forms.Label databaseLabel;
		private System.Windows.Forms.TextBox databaseTextBox;
		private System.Windows.Forms.Label lastLoadMonthLabel;
		private System.Windows.Forms.TextBox lastLoadMonthTextBox;
	}
}
