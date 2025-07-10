namespace Enterprise.LogShipping.Setup.GUI
{

	partial class PrimaryEDocsDatabaseControl
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
			this.titleLabel = new System.Windows.Forms.Label();
			this.eDocsDatabasesCheckBox = new System.Windows.Forms.CheckBox();
			this.settingsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// settingsGroupBox
			// 
			this.settingsGroupBox.Controls.Add(this.eDocsDatabasesCheckBox);
			this.settingsGroupBox.Controls.Add(this.titleLabel);
			this.settingsGroupBox.Text = "eDocs Databases";
			// 
			// titleLabel
			// 
			this.titleLabel.AutoSize = true;
			this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.titleLabel.Location = new System.Drawing.Point(17, 25);
			this.titleLabel.Name = "titleLabel";
			this.titleLabel.Size = new System.Drawing.Size(422, 26);
			this.titleLabel.TabIndex = 0;
			this.titleLabel.Text = "Setup has determined that the selected database has linked eDocs database/s.\r\nPle" +
				"ase tick the box below if you want to setup Log Shipping for those databases as " +
				"well.";
			// 
			// eDocsDatabasesCheckBox
			// 
			this.eDocsDatabasesCheckBox.AutoSize = true;
			this.eDocsDatabasesCheckBox.Location = new System.Drawing.Point(20, 64);
			this.eDocsDatabasesCheckBox.Name = "eDocsDatabasesCheckBox";
			this.eDocsDatabasesCheckBox.Size = new System.Drawing.Size(415, 17);
			this.eDocsDatabasesCheckBox.TabIndex = 1;
			this.eDocsDatabasesCheckBox.Text = "Yes, I want to have Log Shipping set up for linked eDocs Databases";
			this.eDocsDatabasesCheckBox.UseVisualStyleBackColor = true;
			// 
			// PrimaryEDocsDatabaseControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Name = "PrimaryEDocsDatabaseControl";
			this.settingsGroupBox.ResumeLayout(false);
			this.settingsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label titleLabel;
		private System.Windows.Forms.CheckBox eDocsDatabasesCheckBox;
	}
}