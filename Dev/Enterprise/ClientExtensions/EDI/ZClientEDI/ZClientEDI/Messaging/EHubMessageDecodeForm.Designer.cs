namespace Enterprise.Client.EDI.Messaging
{
	partial class EHubMessageDecodeForm
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
		new void InitializeComponent()
		{
			this.CopyButton = new CargoWise.Windows.UI.KButton();
			this.OutputGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.OutputTextBox = new CargoWise.Windows.UI.KTextBox();
			this.CustomerServiceNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CustomerServiceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OriginalContentsGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.OriginalContentsTextBox = new CargoWise.Windows.UI.KTextBox();
			this.splitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OutputGroupBox.SuspendLayout();
			this.OriginalContentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 487, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(934, 24, true);
			// 
			// CopyButton
			// 
			this.CopyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CopyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(693, 10, true);
			this.CopyButton.Name = "CopyButton";
			this.CopyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 23, true);
			this.CopyButton.TabIndex = 6;
			this.CopyButton.Text = "Copy Decoded Message to Clipboard";
			this.CopyButton.UseVisualStyleBackColor = true;
			this.CopyButton.Click += new System.EventHandler(this.CopyButton_Click);
			// 
			// OutputGroupBox
			// 
			this.OutputGroupBox.Controls.Add(this.OutputTextBox);
			this.OutputGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OutputGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OutputGroupBox.Name = "OutputGroupBox";
			this.OutputGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 442, true);
			this.OutputGroupBox.TabIndex = 7;
			this.OutputGroupBox.TabStop = false;
			this.OutputGroupBox.Text = "Decoded Message";
			// 
			// OutputTextBox
			// 
			this.OutputTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OutputTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OutputTextBox.MaxLength = 3000000;
			this.OutputTextBox.Multiline = true;
			this.OutputTextBox.Name = "OutputTextBox";
			this.OutputTextBox.ReadOnly = true;
			this.OutputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.OutputTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 423, true);
			this.OutputTextBox.TabIndex = 3;
			// 
			// CustomerServiceNumberLabel
			// 
			this.CustomerServiceNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 10, true);
			this.CustomerServiceNumberLabel.Name = "CustomerServiceNumberLabel";
			this.CustomerServiceNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 23, true);
			this.CustomerServiceNumberLabel.TabIndex = 8;
			this.CustomerServiceNumberLabel.Text = "ZLabel";
			// 
			// CustomerServiceNumberTextBox
			// 
			this.CustomerServiceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 12, true);
			this.CustomerServiceNumberTextBox.Name = "CustomerServiceNumberTextBox";
			this.CustomerServiceNumberTextBox.ReadOnly = true;
			this.CustomerServiceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 20, true);
			this.CustomerServiceNumberTextBox.TabIndex = 9;
			// 
			// OriginalContentsGroupBox
			// 
			this.OriginalContentsGroupBox.Controls.Add(this.OriginalContentsTextBox);
			this.OriginalContentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OriginalContentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OriginalContentsGroupBox.Name = "OriginalContentsGroupBox";
			this.OriginalContentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 442, true);
			this.OriginalContentsGroupBox.TabIndex = 10;
			this.OriginalContentsGroupBox.TabStop = false;
			this.OriginalContentsGroupBox.Text = "Original Contents";
			// 
			// OriginalContentsTextBox
			// 
			this.OriginalContentsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OriginalContentsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OriginalContentsTextBox.MaxLength = 3000000;
			this.OriginalContentsTextBox.Multiline = true;
			this.OriginalContentsTextBox.Name = "OriginalContentsTextBox";
			this.OriginalContentsTextBox.ReadOnly = true;
			this.OriginalContentsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.OriginalContentsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 423, true);
			this.OriginalContentsTextBox.TabIndex = 3;
			// 
			// splitContainer
			// 
			this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 39, true);
			this.splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.OriginalContentsGroupBox);
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.OutputGroupBox);
			this.splitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(904, 442, true);
			this.splitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(452);
			this.splitContainer.TabIndex = 11;
			// 
			// EHubMessageDecodeForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(934, 511, true);
			this.Controls.Add(this.splitContainer);
			this.Controls.Add(this.CustomerServiceNumberTextBox);
			this.Controls.Add(this.CustomerServiceNumberLabel);
			this.Controls.Add(this.CopyButton);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 550, true);
			this.Name = "EHubMessageDecodeForm";
			this.Text = "Decoded EHub Customer Service Message";
			this.Controls.SetChildIndex(this.CopyButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CustomerServiceNumberLabel, 0);
			this.Controls.SetChildIndex(this.CustomerServiceNumberTextBox, 0);
			this.Controls.SetChildIndex(this.splitContainer, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OutputGroupBox.ResumeLayout(false);
			this.OutputGroupBox.PerformLayout();
			this.OriginalContentsGroupBox.ResumeLayout(false);
			this.OriginalContentsGroupBox.PerformLayout();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KButton CopyButton;
		private CargoWise.Windows.UI.KGroupBox OutputGroupBox;
		private CargoWise.Windows.UI.KTextBox OutputTextBox;
		private ZArchitecture.ZLabel CustomerServiceNumberLabel;
		private ZArchitecture.ZTextBox CustomerServiceNumberTextBox;
		private CargoWise.Windows.UI.KGroupBox OriginalContentsGroupBox;
		private CargoWise.Windows.UI.KTextBox OriginalContentsTextBox;
		private CargoWise.Windows.UI.KSplitContainer splitContainer;
	}
}