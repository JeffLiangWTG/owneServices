namespace Enterprise.ExcelComparator
{
	partial class MainForm
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
			this.file1GroupBox = new System.Windows.Forms.GroupBox();
			this.file1ChangeButton = new System.Windows.Forms.Button();
			this.file1TextBox = new System.Windows.Forms.TextBox();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.file2GroupBox = new System.Windows.Forms.GroupBox();
			this.file2ChangeButton = new System.Windows.Forms.Button();
			this.file2TextBox = new System.Windows.Forms.TextBox();
			this.compareButton = new System.Windows.Forms.Button();
			this.closeButton = new System.Windows.Forms.Button();
			this.optionsBox = new System.Windows.Forms.GroupBox();
			this.compareSectionsCheckBox = new System.Windows.Forms.CheckBox();
			this.file1GroupBox.SuspendLayout();
			this.file2GroupBox.SuspendLayout();
			this.optionsBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// file1GroupBox
			// 
			this.file1GroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.file1GroupBox.Controls.Add(this.file1ChangeButton);
			this.file1GroupBox.Controls.Add(this.file1TextBox);
			this.file1GroupBox.Location = new System.Drawing.Point(12, 12);
			this.file1GroupBox.Name = "file1GroupBox";
			this.file1GroupBox.Size = new System.Drawing.Size(311, 45);
			this.file1GroupBox.TabIndex = 0;
			this.file1GroupBox.TabStop = false;
			this.file1GroupBox.Text = "Excel File 1";
			// 
			// file1ChangeButton
			// 
			this.file1ChangeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.file1ChangeButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.file1ChangeButton.Location = new System.Drawing.Point(281, 18);
			this.file1ChangeButton.Margin = new System.Windows.Forms.Padding(0);
			this.file1ChangeButton.Name = "file1ChangeButton";
			this.file1ChangeButton.Size = new System.Drawing.Size(25, 21);
			this.file1ChangeButton.TabIndex = 1;
			this.file1ChangeButton.Text = "...";
			this.file1ChangeButton.UseVisualStyleBackColor = true;
			this.file1ChangeButton.Click += new System.EventHandler(this.file1ChangeButton_Click);
			// 
			// file1TextBox
			// 
			this.file1TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.file1TextBox.Location = new System.Drawing.Point(6, 19);
			this.file1TextBox.Name = "file1TextBox";
			this.file1TextBox.Size = new System.Drawing.Size(272, 20);
			this.file1TextBox.TabIndex = 0;
			// 
			// openFileDialog
			// 
			this.openFileDialog.DefaultExt = "xls";
			this.openFileDialog.Filter = "Excel files (*.xls;*.xlsx)|*.xls;*.xlsx|All files|*.*";
			this.openFileDialog.Title = "Select Excel File";
			// 
			// file2GroupBox
			// 
			this.file2GroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.file2GroupBox.Controls.Add(this.file2ChangeButton);
			this.file2GroupBox.Controls.Add(this.file2TextBox);
			this.file2GroupBox.Location = new System.Drawing.Point(12, 63);
			this.file2GroupBox.Name = "file2GroupBox";
			this.file2GroupBox.Size = new System.Drawing.Size(311, 45);
			this.file2GroupBox.TabIndex = 1;
			this.file2GroupBox.TabStop = false;
			this.file2GroupBox.Text = "Excel File 2";
			// 
			// file2ChangeButton
			// 
			this.file2ChangeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.file2ChangeButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.file2ChangeButton.Location = new System.Drawing.Point(281, 18);
			this.file2ChangeButton.Margin = new System.Windows.Forms.Padding(0);
			this.file2ChangeButton.Name = "file2ChangeButton";
			this.file2ChangeButton.Size = new System.Drawing.Size(25, 21);
			this.file2ChangeButton.TabIndex = 1;
			this.file2ChangeButton.Text = "...";
			this.file2ChangeButton.UseVisualStyleBackColor = true;
			this.file2ChangeButton.Click += new System.EventHandler(this.file2ChangeButton_Click);
			// 
			// file2TextBox
			// 
			this.file2TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.file2TextBox.Location = new System.Drawing.Point(6, 19);
			this.file2TextBox.Name = "file2TextBox";
			this.file2TextBox.Size = new System.Drawing.Size(272, 20);
			this.file2TextBox.TabIndex = 0;
			// 
			// compareButton
			// 
			this.compareButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.compareButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.compareButton.Location = new System.Drawing.Point(167, 216);
			this.compareButton.Name = "compareButton";
			this.compareButton.Size = new System.Drawing.Size(75, 23);
			this.compareButton.TabIndex = 3;
			this.compareButton.Text = "&Compare";
			this.compareButton.UseVisualStyleBackColor = true;
			this.compareButton.Click += new System.EventHandler(this.compareButton_Click);
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(248, 216);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(75, 23);
			this.closeButton.TabIndex = 4;
			this.closeButton.Text = "Close";
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// optionsBox
			// 
			this.optionsBox.Controls.Add(this.compareSectionsCheckBox);
			this.optionsBox.Location = new System.Drawing.Point(12, 114);
			this.optionsBox.Name = "optionsBox";
			this.optionsBox.Size = new System.Drawing.Size(311, 96);
			this.optionsBox.TabIndex = 2;
			this.optionsBox.TabStop = false;
			this.optionsBox.Text = "Options";
			// 
			// compareSectionsCheckBox
			// 
			this.compareSectionsCheckBox.AutoSize = true;
			this.compareSectionsCheckBox.Location = new System.Drawing.Point(7, 20);
			this.compareSectionsCheckBox.Name = "compareSectionsCheckBox";
			this.compareSectionsCheckBox.Size = new System.Drawing.Size(112, 17);
			this.compareSectionsCheckBox.TabIndex = 0;
			this.compareSectionsCheckBox.Text = "Compare Sections";
			this.compareSectionsCheckBox.UseVisualStyleBackColor = true;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(334, 242);
			this.Controls.Add(this.optionsBox);
			this.Controls.Add(this.closeButton);
			this.Controls.Add(this.compareButton);
			this.Controls.Add(this.file2GroupBox);
			this.Controls.Add(this.file1GroupBox);
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(1200, 280);
			this.MinimumSize = new System.Drawing.Size(350, 280);
			this.Name = "MainForm";
			this.Text = "Excel Comparator";
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.centralKeyPressHandler);
			this.file1GroupBox.ResumeLayout(false);
			this.file1GroupBox.PerformLayout();
			this.file2GroupBox.ResumeLayout(false);
			this.file2GroupBox.PerformLayout();
			this.optionsBox.ResumeLayout(false);
			this.optionsBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox file1GroupBox;
		private System.Windows.Forms.TextBox file1TextBox;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.Button file1ChangeButton;
		private System.Windows.Forms.GroupBox file2GroupBox;
		private System.Windows.Forms.Button file2ChangeButton;
		private System.Windows.Forms.TextBox file2TextBox;
		private System.Windows.Forms.Button compareButton;
		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.GroupBox optionsBox;
		private System.Windows.Forms.CheckBox compareSectionsCheckBox;

	}
}

