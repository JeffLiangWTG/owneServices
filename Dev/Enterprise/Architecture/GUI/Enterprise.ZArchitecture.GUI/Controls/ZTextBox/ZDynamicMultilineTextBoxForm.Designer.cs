namespace Enterprise.ZArchitecture.GUI
{
	partial class ZDynamicMultilineTextBoxForm
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
			if (contextMenuManager != null)
			{
				contextMenuManager.Dispose();
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
			this.multilineTextBox = new CargoWise.Windows.UI.KTextBox();
			this.resizeLabel = new CargoWise.Windows.UI.KLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// multilineTextBox
			// 
			this.multilineTextBox.AcceptsReturn = true;
			this.multilineTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.multilineTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.multilineTextBox.Multiline = true;
			this.multilineTextBox.Name = "multilineTextBox";
			this.multilineTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.multilineTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 264, true);
			this.multilineTextBox.TabIndex = 0;
			this.multilineTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.multilineTextBox_KeyUp);
			// 
			// resizeLabel
			// 
			this.resizeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.resizeLabel.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
			this.resizeLabel.Font = new System.Drawing.Font("Marlett", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
			this.resizeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(266, 245, true);
			this.resizeLabel.Name = "resizeLabel";
			this.resizeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(17, 16, true);
			this.resizeLabel.TabIndex = 1;
			this.resizeLabel.Text = "o";
			this.resizeLabel.TextAlign = System.Drawing.ContentAlignment.BottomRight;
			this.resizeLabel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.resizeLabel_MouseMove);
			this.resizeLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.resizeLabel_MouseDown);
			this.resizeLabel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.resizeLabel_MouseUp);
			// 
			// ZMultilineTextBoxForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 264, true);
			this.Controls.Add(this.resizeLabel);
			this.Controls.Add(this.multilineTextBox);
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "ZMultilineTextBoxForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "ZMultilineTextBoxForm";			
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTextBox multilineTextBox;
		private CargoWise.Windows.UI.KLabel resizeLabel;
	}
}