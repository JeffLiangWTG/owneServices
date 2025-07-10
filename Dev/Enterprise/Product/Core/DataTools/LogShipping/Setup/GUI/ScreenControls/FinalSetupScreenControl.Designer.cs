namespace Enterprise.LogShipping.Setup.GUI
{

	partial class FinalSetupScreenControl
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
			this.label = new System.Windows.Forms.Label();
			this.settingsTextBox = new System.Windows.Forms.TextBox();
			this.setupButton = new System.Windows.Forms.Button();
			this.dropDbCheckBox = new System.Windows.Forms.CheckBox();
			this.settingsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// settingsGroupBox
			// 
			this.settingsGroupBox.Controls.Add(this.dropDbCheckBox);
			this.settingsGroupBox.Controls.Add(this.setupButton);
			this.settingsGroupBox.Controls.Add(this.settingsTextBox);
			this.settingsGroupBox.Controls.Add(this.label);
			this.settingsGroupBox.Text = "Set Up Log Shipping";
			// 
			// label
			// 
			this.label.AutoSize = true;
			this.label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label.Location = new System.Drawing.Point(18, 16);
			this.label.Name = "label";
			this.label.Size = new System.Drawing.Size(321, 13);
			this.label.TabIndex = 0;
			this.label.Text = "Review Log Shipping settings and click setup to start configuration";
			// 
			// settingsTextBox
			// 
			this.settingsTextBox.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.settingsTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.settingsTextBox.Location = new System.Drawing.Point(21, 32);
			this.settingsTextBox.Multiline = true;
			this.settingsTextBox.Name = "settingsTextBox";
			this.settingsTextBox.ReadOnly = true;
			this.settingsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.settingsTextBox.Size = new System.Drawing.Size(400, 87);
			this.settingsTextBox.TabIndex = 1;
			// 
			// setupButton
			// 
			this.setupButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.setupButton.Location = new System.Drawing.Point(347, 120);
			this.setupButton.Name = "setupButton";
			this.setupButton.Size = new System.Drawing.Size(75, 23);
			this.setupButton.TabIndex = 2;
			this.setupButton.Text = "Setup";
			this.setupButton.UseVisualStyleBackColor = true;
			this.setupButton.Click += new System.EventHandler(this.setupButton_Click);
			// 
			// dropDbCheckBox
			// 
			this.dropDbCheckBox.AutoSize = true;
			this.dropDbCheckBox.Checked = true;
			this.dropDbCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.dropDbCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dropDbCheckBox.Location = new System.Drawing.Point(21, 124);
			this.dropDbCheckBox.Name = "dropDbCheckBox";
			this.dropDbCheckBox.Size = new System.Drawing.Size(218, 17);
			this.dropDbCheckBox.TabIndex = 3;
			this.dropDbCheckBox.Text = "Drop secondary database after removing";
			this.dropDbCheckBox.UseVisualStyleBackColor = true;
			this.dropDbCheckBox.Visible = false;
			// 
			// FinalSetupScreenControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Name = "FinalSetupScreenControl";
			this.VisibleChanged += new System.EventHandler(this.FinalSetupScreenControl_VisibleChanged);
			this.settingsGroupBox.ResumeLayout(false);
			this.settingsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label label;
		private System.Windows.Forms.Button setupButton;
		private System.Windows.Forms.TextBox settingsTextBox;
		private System.Windows.Forms.CheckBox dropDbCheckBox;
	}
}