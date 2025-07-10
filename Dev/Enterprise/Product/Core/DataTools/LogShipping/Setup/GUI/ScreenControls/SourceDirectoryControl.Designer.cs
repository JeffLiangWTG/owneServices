namespace Enterprise.LogShipping.Setup.GUI
{

	partial class SourceDirectoryControl
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
			this.directoryTextBox = new System.Windows.Forms.TextBox();
			this.settingsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// settingsGroupBox
			// 
			this.settingsGroupBox.Controls.Add(this.directoryTextBox);
			this.settingsGroupBox.Controls.Add(this.label);
			this.settingsGroupBox.Text = "Source Directory";
			// 
			// label
			// 
			this.label.AutoSize = true;
			this.label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label.Location = new System.Drawing.Point(18, 25);
			this.label.Name = "label";
			this.label.Size = new System.Drawing.Size(339, 26);
			this.label.TabIndex = 0;
			this.label.Text = "Full path of the source directory in universal naming convention (UNC).\r\nFor exam" +
				"ple: \"\\\\Server\\SharedFolder\".";
			// 
			// directoryTextBox
			// 
			this.directoryTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.directoryTextBox.Location = new System.Drawing.Point(21, 54);
			this.directoryTextBox.Name = "directoryTextBox";
			this.directoryTextBox.Size = new System.Drawing.Size(411, 20);
			this.directoryTextBox.TabIndex = 1;
			// 
			// SourceDirectoryControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Name = "SourceDirectoryControl";
			this.VisibleChanged += new System.EventHandler(this.SourceDirectoryControl_VisibleChanged);
			this.settingsGroupBox.ResumeLayout(false);
			this.settingsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label label;
		private System.Windows.Forms.TextBox directoryTextBox;
	}
}