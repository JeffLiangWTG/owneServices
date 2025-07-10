namespace Enterprise.ZArchitecture
{
	partial class TextFindForm
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
		protected override void InitializeComponent()
		{
			this.FindTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FindLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FindNextButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FindPreviousButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MatchesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MatchCaseCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 81, true);
			this.MainStatusBar.ShowPanels = false;
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(466, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 6;
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(456);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(int);
			// 
			// FindTextBox
			// 
			this.FindTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FindTextBox, false);
			this.FindTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 9, true);
			this.FindTextBox.Name = "FindTextBox";
			this.FindTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 18, true);
			this.FindTextBox.TabIndex = 1;
			this.FindTextBox.TextChanged += new System.EventHandler(this.FindTextBox_TextChanged);
			// 
			// FindLabel
			// 
			this.FindLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("1779B097-9799-4B6D-B834-9FC7AABEE5A1", "Find what");
			this.FindLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FindLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 7, true);
			this.FindLabel.Name = "FindLabel";
			this.FindLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 22, true);
			this.FindLabel.TabIndex = 0;
			// 
			// FindNextButton
			// 
			this.FindNextButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("C0A4BD71-6F97-42DA-B961-004844A6FD56", "Find Next");
			this.FindNextButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(373, 31, true);
			this.FindNextButton.Name = "FindNextButton";
			this.FindNextButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 21, true);
			this.FindNextButton.TabIndex = 4;
			this.FindNextButton.ToolTipCaption = null;
			this.FindNextButton.Click += new System.EventHandler(this.FindNextButton_Click);
			// 
			// FindPreviousButton
			// 
			this.FindPreviousButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("C7D5817C-167B-4FF5-BDBD-9403FF372E33", "Find Previous");
			this.FindPreviousButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(373, 7, true);
			this.FindPreviousButton.Name = "FindPreviousButton";
			this.FindPreviousButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 22, true);
			this.FindPreviousButton.TabIndex = 3;
			this.FindPreviousButton.ToolTipCaption = null;
			this.FindPreviousButton.Click += new System.EventHandler(this.FindPreviousButton_Click);
			// 
			// MatchesLabel
			// 
			this.MatchesLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("EC11DD23-40FC-4990-A474-B18228981B50", "No Results");
			this.MatchesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MatchesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 81, true);
			this.MatchesLabel.Name = "MatchesLabel";
			this.MatchesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 17, true);
			this.MatchesLabel.TabIndex = 6;
			this.MatchesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// CloseButton
			// 
			this.CloseButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("3D2035A8-8076-4AB0-AC0D-FE386FCB8645", "Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(373, 55, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 21, true);
			this.CloseButton.TabIndex = 5;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// MatchCaseCheckBox
			// 
			this.MatchCaseCheckBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("40D06EED-81F9-4288-825F-DEBCDFF3F282", "Match Case");
			this.MatchCaseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 38, true);
			this.MatchCaseCheckBox.Name = "MatchCaseCheckBox";
			this.MatchCaseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 16, true);
			this.MatchCaseCheckBox.TabIndex = 2;
			// 
			// TextFindForm
			// 
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("TreeViewFindForm|6dd369b7-e419-4a86-b5bd-65cf59ca2d4f", "Find");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(466, 105, true);
			this.Controls.Add(this.MatchCaseCheckBox);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.MatchesLabel);
			this.Controls.Add(this.FindPreviousButton);
			this.Controls.Add(this.FindNextButton);
			this.Controls.Add(this.FindLabel);
			this.Controls.Add(this.FindTextBox);
			this.DataSourceAssemblyName = "mscorlib";
			this.DataSourceType = typeof(int);
			this.DataSourceTypeName = "System.Int32";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 119, true);
			this.Name = "TextFindForm";
			this.TopMost = true;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.FindTextBox, 0);
			this.Controls.SetChildIndex(this.FindLabel, 0);
			this.Controls.SetChildIndex(this.FindNextButton, 0);
			this.Controls.SetChildIndex(this.FindPreviousButton, 0);
			this.Controls.SetChildIndex(this.MatchesLabel, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.MatchCaseCheckBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZTextBox FindTextBox;
		Enterprise.ZArchitecture.ZLabel FindLabel;
		Enterprise.ZArchitecture.GUI.ZButton FindNextButton;
		Enterprise.ZArchitecture.GUI.ZButton FindPreviousButton;
		Enterprise.ZArchitecture.ZLabel MatchesLabel;
		Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		Enterprise.ZArchitecture.GUI.ZCheckBox MatchCaseCheckBox;
	}
}
