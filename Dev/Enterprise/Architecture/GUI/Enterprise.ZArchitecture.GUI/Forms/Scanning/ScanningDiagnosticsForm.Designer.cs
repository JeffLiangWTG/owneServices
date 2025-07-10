namespace Enterprise.ZArchitecture.GUI.Scanning
{
	partial class ScanningDiagnosticsForm
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
		void InitializeComponent()
		{
			this.ClearButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InstructionsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OutputTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CopyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SuspendLayout();
			// 
			// ClearButton
			// 
			this.ClearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 281, true);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ClearButton.TabIndex = 3;
			this.ClearButton.Text = "C&lear";
			this.ClearButton.UseVisualStyleBackColor = true;
			this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
			// 
			// InstructionsLabel
			// 
			this.InstructionsLabel.AutoSize = true;
			this.InstructionsLabel.IsFontBold = true;
			this.InstructionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 13, true);
			this.InstructionsLabel.Name = "InstructionsLabel";
			this.InstructionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 13, true);
			this.InstructionsLabel.TabIndex = 0;
			this.InstructionsLabel.Text = "Scan a barcode while this form has focus.";
			// 
			// OutputTextBox
			// 
			this.OutputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.OutputTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.OutputTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 30, true);
			this.OutputTextBox.Multiline = true;
			this.OutputTextBox.Name = "OutputTextBox";
			this.OutputTextBox.ReadOnly = true;
			this.OutputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.OutputTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 245, true);
			this.OutputTextBox.TabIndex = 1;
			// 
			// CopyButton
			// 
			this.CopyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CopyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 281, true);
			this.CopyButton.Name = "CopyButton";
			this.CopyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CopyButton.TabIndex = 2;
			this.CopyButton.Text = "&Copy Text";
			this.CopyButton.UseVisualStyleBackColor = true;
			this.CopyButton.Click += new System.EventHandler(this.CopyButton_Click);
			// 
			// ScanningDiagnosticsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 312, true);
			this.Controls.Add(this.ClearButton);
			this.Controls.Add(this.CopyButton);
			this.Controls.Add(this.OutputTextBox);
			this.Controls.Add(this.InstructionsLabel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(274, 350, true);
			this.Name = "ScanningDiagnosticsForm";
			this.ShowIcon = false;
			this.Text = "Scanning Diagnostics";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZButton ClearButton;
		private ZLabel InstructionsLabel;
		private ZTextBox OutputTextBox;
		private ZButton CopyButton;

	}
}