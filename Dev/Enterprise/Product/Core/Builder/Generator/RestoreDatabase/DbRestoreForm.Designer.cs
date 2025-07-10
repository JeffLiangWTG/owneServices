namespace Enterprise.Builder.Generator
{
	public partial class DbRestoreForm
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1014:EmbeddedIconRule")]
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DbRestoreForm));
			this.CloseButton = new CargoWise.Windows.UI.KButton();
			this.ProgressTextBox = new CargoWise.Windows.UI.KTextBox();
			this.SuspendLayout();
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Enabled = false;
			this.CloseButton.Location = new System.Drawing.Point(232, 164);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = new System.Drawing.Size(104, 23);
			this.CloseButton.TabIndex = 8;
			this.CloseButton.Text = "Close";
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// ProgressTextBox
			// 
			this.ProgressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressTextBox.AutoSize = false;
			this.ProgressTextBox.BackColor = System.Drawing.Color.White;
			this.ProgressTextBox.Location = new System.Drawing.Point(8, 8);
			this.ProgressTextBox.Multiline = true;
			this.ProgressTextBox.Name = "ProgressTextBox";
			this.ProgressTextBox.ReadOnly = true;
			this.ProgressTextBox.Size = new System.Drawing.Size(328, 140);
			this.ProgressTextBox.TabIndex = 12;
			this.ProgressTextBox.Text = "";
			// 
			// DbRestoreForm
			// 
			this.ClientSize = new System.Drawing.Size(344, 198);
			this.ControlBox = false;
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.ProgressTextBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "DbRestoreForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Restoring Database...";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.DbRestoreForm_Closing);
			this.Load += new System.EventHandler(this.DbRestoreForm_Load);
			this.ResumeLayout(false);
		}
		#endregion

		private CargoWise.Windows.UI.KButton CloseButton;
		private CargoWise.Windows.UI.KTextBox ProgressTextBox;
	}
}
