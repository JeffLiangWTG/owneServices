namespace Enterprise.DataTools.DbBackupAndRestore.GUI
{
	partial class TaskPageControl
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
			this.CloseButton = new CargoWise.Windows.UI.KButton();
			this.OuputGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.OutputTextBox = new CargoWise.Windows.UI.KRichTextBox();
			this.OuputGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 583, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 32, true);
			this.CloseButton.TabIndex = 12;
			this.CloseButton.Text = "Close";
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// OuputGroupBox
			// 
			this.OuputGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.OuputGroupBox.Controls.Add(this.OutputTextBox);
			this.OuputGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 355, true);
			this.OuputGroupBox.Name = "OuputGroupBox";
			this.OuputGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(657, 222, true);
			this.OuputGroupBox.TabIndex = 11;
			this.OuputGroupBox.TabStop = false;
			this.OuputGroupBox.Text = "Output:";
			// 
			// OutputTextBox
			// 
			this.OutputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.OutputTextBox.BackColor = System.Drawing.SystemColors.Info;
			this.OutputTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OutputTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 19, true);
			this.OutputTextBox.Name = "OutputTextBox";
			this.OutputTextBox.ReadOnly = true;
			this.OutputTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(629, 187, true);
			this.OutputTextBox.TabIndex = 0;
			this.OutputTextBox.TabStop = false;
			this.OutputTextBox.Text = "";
			// 
			// TaskPageControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.OuputGroupBox);
			this.Name = "TaskPageControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 622, true);
			this.OuputGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KButton CloseButton;
		private CargoWise.Windows.UI.KGroupBox OuputGroupBox;
		protected CargoWise.Windows.UI.KRichTextBox OutputTextBox;
	}
}
