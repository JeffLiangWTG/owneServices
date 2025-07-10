namespace Enterprise.StlAnalysis.Load
{
	partial class LoadFromCsvControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoadFromCsvControl));
			this.lblImportFolderLabel = new System.Windows.Forms.Label();
			this.txbImportFolderTextBox = new System.Windows.Forms.TextBox();
			this.btnBrowseOutputFolderButton = new System.Windows.Forms.Button();
			this.btnStopImportButton = new System.Windows.Forms.Button();
			this.btnStartImportButton = new System.Windows.Forms.Button();
			this.txbErrorTextBox = new System.Windows.Forms.TextBox();
			this.txbOutputTextBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// lblImportFolderLabel
			// 
			this.lblImportFolderLabel.AutoSize = true;
			this.lblImportFolderLabel.Location = new System.Drawing.Point(3, 6);
			this.lblImportFolderLabel.Name = "lblImportFolderLabel";
			this.lblImportFolderLabel.Size = new System.Drawing.Size(71, 13);
			this.lblImportFolderLabel.TabIndex = 11;
			this.lblImportFolderLabel.Text = "Import Folder:";
			// 
			// txbImportFolderTextBox
			// 
			this.txbImportFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txbImportFolderTextBox.Location = new System.Drawing.Point(80, 3);
			this.txbImportFolderTextBox.Name = "txbImportFolderTextBox";
			this.txbImportFolderTextBox.Size = new System.Drawing.Size(718, 20);
			this.txbImportFolderTextBox.TabIndex = 12;
			this.txbImportFolderTextBox.Text = "<import_folder>";
			// 
			// btnBrowseOutputFolderButton
			// 
			this.btnBrowseOutputFolderButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnBrowseOutputFolderButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnBrowseOutputFolderButton.FlatAppearance.BorderSize = 0;
			this.btnBrowseOutputFolderButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnBrowseOutputFolderButton.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnBrowseOutputFolderButton.Image = ((System.Drawing.Image)(resources.GetObject("btnBrowseOutputFolderButton.Image")));
			this.btnBrowseOutputFolderButton.Location = new System.Drawing.Point(804, 2);
			this.btnBrowseOutputFolderButton.Name = "btnBrowseOutputFolderButton";
			this.btnBrowseOutputFolderButton.Size = new System.Drawing.Size(20, 20);
			this.btnBrowseOutputFolderButton.TabIndex = 13;
			this.btnBrowseOutputFolderButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.btnBrowseOutputFolderButton.UseVisualStyleBackColor = false;
			this.btnBrowseOutputFolderButton.Click += new System.EventHandler(this.btnBrowseOutputFolderButton_Click);
			// 
			// btnStopImportButton
			// 
			this.btnStopImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnStopImportButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnStopImportButton.Enabled = false;
			this.btnStopImportButton.FlatAppearance.BorderSize = 0;
			this.btnStopImportButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnStopImportButton.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnStopImportButton.Image = global::Enterprise.StlAnalysis.Load.Properties.Resources.Stop_24p;
			this.btnStopImportButton.Location = new System.Drawing.Point(715, 29);
			this.btnStopImportButton.Name = "btnStopImportButton";
			this.btnStopImportButton.Size = new System.Drawing.Size(109, 41);
			this.btnStopImportButton.TabIndex = 20;
			this.btnStopImportButton.Text = "Stop";
			this.btnStopImportButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.btnStopImportButton.UseVisualStyleBackColor = false;
			this.btnStopImportButton.Click += new System.EventHandler(this.btnStopImportButton_Click);
			// 
			// btnStartImportButton
			// 
			this.btnStartImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnStartImportButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnStartImportButton.FlatAppearance.BorderSize = 0;
			this.btnStartImportButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnStartImportButton.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnStartImportButton.Image = global::Enterprise.StlAnalysis.Load.Properties.Resources.Start_24p;
			this.btnStartImportButton.Location = new System.Drawing.Point(600, 29);
			this.btnStartImportButton.Name = "btnStartImportButton";
			this.btnStartImportButton.Size = new System.Drawing.Size(109, 41);
			this.btnStartImportButton.TabIndex = 19;
			this.btnStartImportButton.Text = "Start";
			this.btnStartImportButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.btnStartImportButton.UseVisualStyleBackColor = false;
			this.btnStartImportButton.Click += new System.EventHandler(this.btnStartImportButton_Click);
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
			this.txbErrorTextBox.Size = new System.Drawing.Size(821, 182);
			this.txbErrorTextBox.TabIndex = 22;
			this.txbErrorTextBox.Tag = "";
			// 
			// txbOutputTextBox
			// 
			this.txbOutputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txbOutputTextBox.BackColor = System.Drawing.Color.Snow;
			this.txbOutputTextBox.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txbOutputTextBox.Location = new System.Drawing.Point(3, 76);
			this.txbOutputTextBox.MaxLength = 65536;
			this.txbOutputTextBox.Multiline = true;
			this.txbOutputTextBox.Name = "txbOutputTextBox";
			this.txbOutputTextBox.ReadOnly = true;
			this.txbOutputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txbOutputTextBox.Size = new System.Drawing.Size(821, 396);
			this.txbOutputTextBox.TabIndex = 21;
			this.txbOutputTextBox.Tag = "";
			// 
			// LoadFromCsvControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(225)))), ((int)(((byte)(220)))));
			this.Controls.Add(this.txbErrorTextBox);
			this.Controls.Add(this.txbOutputTextBox);
			this.Controls.Add(this.btnStopImportButton);
			this.Controls.Add(this.btnStartImportButton);
			this.Controls.Add(this.btnBrowseOutputFolderButton);
			this.Controls.Add(this.lblImportFolderLabel);
			this.Controls.Add(this.txbImportFolderTextBox);
			this.Name = "LoadFromCsvControl";
			this.Size = new System.Drawing.Size(827, 663);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnBrowseOutputFolderButton;
		private System.Windows.Forms.Label lblImportFolderLabel;
		private System.Windows.Forms.TextBox txbImportFolderTextBox;
		private System.Windows.Forms.Button btnStopImportButton;
		private System.Windows.Forms.Button btnStartImportButton;
		private System.Windows.Forms.TextBox txbErrorTextBox;
		private System.Windows.Forms.TextBox txbOutputTextBox;
	}
}
