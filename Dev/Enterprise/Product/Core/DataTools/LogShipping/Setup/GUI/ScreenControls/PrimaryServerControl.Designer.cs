namespace Enterprise.LogShipping.Setup.GUI
{

	partial class PrimaryServerControl
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
			this.instanceLabel = new System.Windows.Forms.Label();
			this.serverNameTextBox = new System.Windows.Forms.TextBox();
			this.instanceComboBox = new System.Windows.Forms.ComboBox();
			this.refreshButton = new System.Windows.Forms.Button();
			this.settingsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// settingsGroupBox
			// 
			this.settingsGroupBox.Controls.Add(this.refreshButton);
			this.settingsGroupBox.Controls.Add(this.instanceComboBox);
			this.settingsGroupBox.Controls.Add(this.serverNameTextBox);
			this.settingsGroupBox.Controls.Add(this.instanceLabel);
			this.settingsGroupBox.Controls.Add(this.titleLabel);
			this.settingsGroupBox.Text = "Primary Server Name";
			// 
			// titleLabel
			// 
			this.titleLabel.AutoSize = true;
			this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.titleLabel.Location = new System.Drawing.Point(18, 19);
			this.titleLabel.Name = "titleLabel";
			this.titleLabel.Size = new System.Drawing.Size(72, 13);
			this.titleLabel.TabIndex = 0;
			this.titleLabel.Text = "Server Name:";
			// 
			// instanceLabel
			// 
			this.instanceLabel.AutoSize = true;
			this.instanceLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.instanceLabel.Location = new System.Drawing.Point(18, 58);
			this.instanceLabel.Name = "instanceLabel";
			this.instanceLabel.Size = new System.Drawing.Size(51, 13);
			this.instanceLabel.TabIndex = 0;
			this.instanceLabel.Text = "Instance:";
			// 
			// serverNameTextBox
			// 
			this.serverNameTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.serverNameTextBox.Location = new System.Drawing.Point(21, 35);
			this.serverNameTextBox.Name = "serverNameTextBox";
			this.serverNameTextBox.Size = new System.Drawing.Size(306, 20);
			this.serverNameTextBox.TabIndex = 1;
			this.serverNameTextBox.TextChanged += new System.EventHandler(this.serverNameTextBox_TextChanged);
			// 
			// instanceComboBox
			// 
			this.instanceComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
			this.instanceComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this.instanceComboBox.Enabled = false;
			this.instanceComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.instanceComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.instanceComboBox.FormattingEnabled = true;
			this.instanceComboBox.Location = new System.Drawing.Point(21, 74);
			this.instanceComboBox.Name = "instanceComboBox";
			this.instanceComboBox.Size = new System.Drawing.Size(413, 21);
			this.instanceComboBox.TabIndex = 2;
			// 
			// refreshButton
			// 
			this.refreshButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.refreshButton.Location = new System.Drawing.Point(333, 33);
			this.refreshButton.Name = "refreshButton";
			this.refreshButton.Size = new System.Drawing.Size(101, 23);
			this.refreshButton.TabIndex = 3;
			this.refreshButton.Text = "Refresh Instances";
			this.refreshButton.UseVisualStyleBackColor = true;
			this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
			// 
			// PrimaryServerControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Name = "PrimaryServerControl";
			this.settingsGroupBox.ResumeLayout(false);
			this.settingsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label titleLabel;
		private System.Windows.Forms.TextBox serverNameTextBox;
		private System.Windows.Forms.Label instanceLabel;
		private System.Windows.Forms.ComboBox instanceComboBox;
		private System.Windows.Forms.Button refreshButton;
	}
}