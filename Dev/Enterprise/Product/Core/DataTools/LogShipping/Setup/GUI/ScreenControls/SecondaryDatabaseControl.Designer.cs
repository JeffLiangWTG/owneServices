namespace Enterprise.LogShipping.Setup.GUI.ScreenControls
{

	partial class SecondaryDatabaseControl
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
			this.changeRadioButton = new System.Windows.Forms.RadioButton();
			this.secondaryDatabasesComboBox = new System.Windows.Forms.ComboBox();
			this.setupRadioButton = new System.Windows.Forms.RadioButton();
			this.removeRadioButton = new System.Windows.Forms.RadioButton();
			this.settingsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// settingsGroupBox
			// 
			this.settingsGroupBox.Controls.Add(this.removeRadioButton);
			this.settingsGroupBox.Controls.Add(this.changeRadioButton);
			this.settingsGroupBox.Controls.Add(this.secondaryDatabasesComboBox);
			this.settingsGroupBox.Controls.Add(this.setupRadioButton);
			this.settingsGroupBox.Text = "Secondary Database";
			// 
			// changeRadioButton
			// 
			this.changeRadioButton.AutoSize = true;
			this.changeRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.changeRadioButton.Location = new System.Drawing.Point(20, 53);
			this.changeRadioButton.Name = "changeRadioButton";
			this.changeRadioButton.Size = new System.Drawing.Size(271, 17);
			this.changeRadioButton.TabIndex = 0;
			this.changeRadioButton.Text = "Change settings of an existing Log Shipping process";
			this.changeRadioButton.UseVisualStyleBackColor = true;
			this.changeRadioButton.CheckedChanged += new System.EventHandler(this.changeRadioButton_CheckedChanged);
			// 
			// secondaryDatabasesComboBox
			// 
			this.secondaryDatabasesComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
			this.secondaryDatabasesComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this.secondaryDatabasesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.secondaryDatabasesComboBox.Enabled = false;
			this.secondaryDatabasesComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.secondaryDatabasesComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.secondaryDatabasesComboBox.FormattingEnabled = true;
			this.secondaryDatabasesComboBox.Location = new System.Drawing.Point(38, 99);
			this.secondaryDatabasesComboBox.Name = "secondaryDatabasesComboBox";
			this.secondaryDatabasesComboBox.Size = new System.Drawing.Size(383, 21);
			this.secondaryDatabasesComboBox.Sorted = true;
			this.secondaryDatabasesComboBox.TabIndex = 1;
			// 
			// setupRadioButton
			// 
			this.setupRadioButton.AutoSize = true;
			this.setupRadioButton.Checked = true;
			this.setupRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.setupRadioButton.Location = new System.Drawing.Point(20, 30);
			this.setupRadioButton.Name = "setupRadioButton";
			this.setupRadioButton.Size = new System.Drawing.Size(190, 17);
			this.setupRadioButton.TabIndex = 0;
			this.setupRadioButton.TabStop = true;
			this.setupRadioButton.Text = "Setup a new Log Shipping process";
			this.setupRadioButton.UseVisualStyleBackColor = true;
			this.setupRadioButton.CheckedChanged += new System.EventHandler(this.setupRadioButton_CheckedChanged);
			// 
			// removeRadioButton
			// 
			this.removeRadioButton.AutoSize = true;
			this.removeRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.removeRadioButton.Location = new System.Drawing.Point(20, 76);
			this.removeRadioButton.Name = "removeRadioButton";
			this.removeRadioButton.Size = new System.Drawing.Size(223, 17);
			this.removeRadioButton.TabIndex = 0;
			this.removeRadioButton.Text = "Remove an existing Log Shipping process";
			this.removeRadioButton.UseVisualStyleBackColor = true;
			this.removeRadioButton.CheckedChanged += new System.EventHandler(this.removeRadioButton_CheckedChanged);
			// 
			// SecondaryDatabaseControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Name = "SecondaryDatabaseControl";
			this.VisibleChanged += new System.EventHandler(this.SecondaryDatabaseControl_VisibleChanged);
			this.settingsGroupBox.ResumeLayout(false);
			this.settingsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RadioButton changeRadioButton;
		private System.Windows.Forms.ComboBox secondaryDatabasesComboBox;
		private System.Windows.Forms.RadioButton setupRadioButton;
		private System.Windows.Forms.RadioButton removeRadioButton;
	}
}