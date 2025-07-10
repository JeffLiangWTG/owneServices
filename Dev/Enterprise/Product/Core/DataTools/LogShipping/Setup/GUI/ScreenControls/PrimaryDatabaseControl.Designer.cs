namespace Enterprise.LogShipping.Setup.GUI
{

	partial class PrimaryDatabaseControl
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
			this.databasesComboBox = new System.Windows.Forms.ComboBox();
			this.settingsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// settingsGroupBox
			// 
			this.settingsGroupBox.Controls.Add(this.databasesComboBox);
			this.settingsGroupBox.Controls.Add(this.titleLabel);
			this.settingsGroupBox.Text = "Primary Database";
			// 
			// titleLabel
			// 
			this.titleLabel.AutoSize = true;
			this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.titleLabel.Location = new System.Drawing.Point(17, 25);
			this.titleLabel.Name = "titleLabel";
			this.titleLabel.Size = new System.Drawing.Size(144, 13);
			this.titleLabel.TabIndex = 0;
			this.titleLabel.Text = "Select the Primary Database:";
			// 
			// databasesComboBox
			// 
			this.databasesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.databasesComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.databasesComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.databasesComboBox.FormattingEnabled = true;
			this.databasesComboBox.Location = new System.Drawing.Point(20, 41);
			this.databasesComboBox.Name = "databasesComboBox";
			this.databasesComboBox.Size = new System.Drawing.Size(410, 21);
			this.databasesComboBox.TabIndex = 1;
			this.databasesComboBox.VisibleChanged += new System.EventHandler(this.databasesComboBox_VisibleChanged);
			// 
			// PrimaryDatabaseControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Name = "PrimaryDatabaseControl";
			this.settingsGroupBox.ResumeLayout(false);
			this.settingsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label titleLabel;
		private System.Windows.Forms.ComboBox databasesComboBox;
	}
}