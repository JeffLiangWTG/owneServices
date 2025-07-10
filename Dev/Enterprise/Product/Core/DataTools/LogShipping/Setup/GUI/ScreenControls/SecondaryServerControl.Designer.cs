namespace Enterprise.LogShipping.Setup.GUI
{

	partial class SecondaryServerControl
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
			this.instancesComboBox = new System.Windows.Forms.ComboBox();
			this.settingsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// settingsGroupBox
			// 
			this.settingsGroupBox.Controls.Add(this.instancesComboBox);
			this.settingsGroupBox.Controls.Add(this.titleLabel);
			this.settingsGroupBox.Text = "Secondary Server";
			// 
			// titleLabel
			// 
			this.titleLabel.AutoSize = true;
			this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.titleLabel.Location = new System.Drawing.Point(14, 24);
			this.titleLabel.Name = "titleLabel";
			this.titleLabel.Size = new System.Drawing.Size(48, 13);
			this.titleLabel.TabIndex = 0;
			this.titleLabel.Text = "Instance";
			// 
			// instancesComboBox
			// 
			this.instancesComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
			this.instancesComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this.instancesComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.instancesComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.instancesComboBox.FormattingEnabled = true;
			this.instancesComboBox.Location = new System.Drawing.Point(17, 40);
			this.instancesComboBox.Name = "instancesComboBox";
			this.instancesComboBox.Size = new System.Drawing.Size(414, 21);
			this.instancesComboBox.TabIndex = 1;
			this.instancesComboBox.DropDown += new System.EventHandler(this.instancesComboBox_DropDown);
			// 
			// SecondaryServerControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Name = "SecondaryServerControl";
			this.settingsGroupBox.ResumeLayout(false);
			this.settingsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label titleLabel;
		private System.Windows.Forms.ComboBox instancesComboBox;
	}
}