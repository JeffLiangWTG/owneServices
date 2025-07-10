namespace Enterprise.Customs.CA.GUI
{
	public partial class BulkConsolidationProgressForm
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
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.ProgressTextBox = new CargoWise.Windows.UI.KRichTextBox();
			this.OutputGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.StartButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ProgressBar = new CargoWise.Windows.UI.KProgressBar();
			this.ProgressPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OutputGroupBox.SuspendLayout();
			this.ProgressPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 381, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 24, true);
			this.MainStatusBar.TabIndex = 8;
			// 
			// ProgressTextBox
			// 
			this.ProgressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ProgressTextBox, false);
			this.ProgressTextBox.Name = "ProgressTextBox";
			this.ProgressTextBox.ReadOnly = true;
			this.ProgressTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.ProgressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(531, 278, true);
			this.ProgressTextBox.TabIndex = 0;
			// 
			// OutputGroupBox
			// 
			this.OutputGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.OutputGroupBox.Controls.Add(this.ProgressTextBox);
			this.OutputGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 44, true);
			this.OutputGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("D85900D6-A583-4FD1-A0CC-B3F8B51A9867", "Output");
			this.OutputGroupBox.Name = "OutputGroupBox";
			this.OutputGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(537, 297, true);
			this.OutputGroupBox.TabIndex = 5;
			this.OutputGroupBox.TabStop = false;
			// 
			// StartButton
			// 
			this.StartButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.StartButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("20252022-6C4A-456F-B399-F1F2F2674F01", "Start");
			this.StartButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(391, 352, true);
			this.StartButton.Name = "StartButton";
			this.StartButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.StartButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 23, true);
			this.StartButton.TabIndex = 6;
			this.StartButton.ToolTipCaption = null;
			this.StartButton.UseVisualStyleBackColor = true;
			this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(474, 352, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 7;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			// 
			// ProgressBar
			// 
			this.ProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ProgressBar.Name = "ProgressBar";
			this.ProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 23, true);
			this.ProgressBar.TabIndex = 4;
			// 
			// ProgressPanel
			// 
			this.ProgressPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressPanel.Controls.Add(this.ProgressBar);
			this.ProgressPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 12, true);
			this.ProgressPanel.Name = "ProgressPanel";
			this.ProgressPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 31, true);
			this.ProgressPanel.TabIndex = 4;
			// 
			// BulkConsolidationProgressForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 405, true);
			this.Controls.Add(this.OutputGroupBox);
			this.Controls.Add(this.StartButton);
			this.Controls.Add(this.ProgressPanel);
			this.Controls.Add(this.CloseButton);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(577, 443, true);
			this.Name = "BulkConsolidationProgressForm";
			this.ShowInTaskbar = false;
			this.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("F66375CE-C552-4792-865D-C32BF9A645AB", "Bulk Consolidate");
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DataImportForm_FormClosing);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.ProgressPanel, 0);
			this.Controls.SetChildIndex(this.StartButton, 0);
			this.Controls.SetChildIndex(this.OutputGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OutputGroupBox.ResumeLayout(false);
			this.OutputGroupBox.PerformLayout();
			this.ProgressPanel.ResumeLayout(false);
			this.ProgressPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		protected CargoWise.Windows.UI.KRichTextBox ProgressTextBox;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox OutputGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZButton StartButton;
		protected Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		protected CargoWise.Windows.UI.KProgressBar ProgressBar;
		protected ZArchitecture.GUI.ZPanel ProgressPanel;
	}
}
