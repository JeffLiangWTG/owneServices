namespace Enterprise.LogShipping.Setup.GUI
{

	partial class UserAreaControl
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
			this.settingsGroupBox = new System.Windows.Forms.GroupBox();
			this.outputTextBox = new System.Windows.Forms.TextBox();
			this.outputGroupBox = new System.Windows.Forms.GroupBox();
			this.outputGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// settingsGroupBox
			// 
			this.settingsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.settingsGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.settingsGroupBox.Location = new System.Drawing.Point(3, 3);
			this.settingsGroupBox.Name = "settingsGroupBox";
			this.settingsGroupBox.Size = new System.Drawing.Size(451, 147);
			this.settingsGroupBox.TabIndex = 33;
			this.settingsGroupBox.TabStop = false;
			// 
			// outputTextBox
			// 
			this.outputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.outputTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.outputTextBox.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.outputTextBox.ForeColor = System.Drawing.Color.Black;
			this.outputTextBox.Location = new System.Drawing.Point(6, 16);
			this.outputTextBox.Multiline = true;
			this.outputTextBox.Name = "outputTextBox";
			this.outputTextBox.ReadOnly = true;
			this.outputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.outputTextBox.Size = new System.Drawing.Size(439, 126);
			this.outputTextBox.TabIndex = 32;
			// 
			// outputGroupBox
			// 
			this.outputGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.outputGroupBox.Controls.Add(this.outputTextBox);
			this.outputGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.outputGroupBox.Location = new System.Drawing.Point(3, 156);
			this.outputGroupBox.Name = "outputGroupBox";
			this.outputGroupBox.Size = new System.Drawing.Size(451, 149);
			this.outputGroupBox.TabIndex = 34;
			this.outputGroupBox.TabStop = false;
			this.outputGroupBox.Text = "Output:";
			// 
			// UserAreaControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.Controls.Add(this.outputGroupBox);
			this.Controls.Add(this.settingsGroupBox);
			this.Name = "UserAreaControl";
			this.Size = new System.Drawing.Size(458, 308);
			this.outputGroupBox.ResumeLayout(false);
			this.outputGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		protected System.Windows.Forms.GroupBox settingsGroupBox;
		private System.Windows.Forms.GroupBox outputGroupBox;
		private System.Windows.Forms.TextBox outputTextBox;

	}
}