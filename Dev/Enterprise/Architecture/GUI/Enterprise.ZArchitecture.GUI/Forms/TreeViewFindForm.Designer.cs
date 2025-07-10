using System.ComponentModel;

namespace Enterprise.ZArchitecture.GUI
{
	partial class TreeViewFindForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.FindTextBox = new CargoWise.Windows.UI.KTextBox();
			this.FindLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FindNextButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FindPreviousButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MatchesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MatchCaseCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SearchFromSelectedNodeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 76, true);
			this.MainStatusBar.ShowPanels = false;
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 22, true);
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
			this.FindTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 7, true);
			this.FindTextBox.Name = "FindTextBox";
			this.FindTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 18, true);
			this.FindTextBox.TabIndex = 1;
			this.FindTextBox.TextChanged += new System.EventHandler(this.FindTextBox_TextChanged);
			// 
			// FindLabel
			// 
			this.FindLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("TreeViewFindForm|2ad7bfa8-5c2b-4a96-990d-893376713a6e", "Find What");
			this.FindLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FindLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.FindLabel.Name = "FindLabel";
			this.FindLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 22, true);
			this.FindLabel.TabIndex = 0;
			// 
			// FindNextButton
			// 
			this.FindNextButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("TreeViewFindForm|9d8c4148-a0c0-447d-9b6c-6d89c5938da1", "Find Next");
			this.FindNextButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(368, 30, true);
			this.FindNextButton.Name = "FindNextButton";
			this.FindNextButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 21, true);
			this.FindNextButton.TabIndex = 5;
			this.FindNextButton.ToolTipCaption = null;
			this.FindNextButton.Click += new System.EventHandler(this.FindNextButton_Click);
			// 
			// FindPreviousButton
			// 
			this.FindPreviousButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("TreeViewFindForm|de5a4a97-199f-40c3-a5f5-cedfe1b06db6", "Find Previous");
			this.FindPreviousButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(368, 7, true);
			this.FindPreviousButton.Name = "FindPreviousButton";
			this.FindPreviousButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 22, true);
			this.FindPreviousButton.TabIndex = 4;
			this.FindPreviousButton.ToolTipCaption = null;
			this.FindPreviousButton.Click += new System.EventHandler(this.FindPreviousButton_Click);
			// 
			// MatchesLabel
			// 
			this.MatchesLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("TreeViewFindForm|0cf728a9-8119-4b17-b4d8-d23b5f3f1f32", "No Results");
			this.MatchesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MatchesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 79, true);
			this.MatchesLabel.Name = "MatchesLabel";
			this.MatchesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 17, true);
			this.MatchesLabel.TabIndex = 6;
			this.MatchesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// CloseButton
			// 
			this.CloseButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("TreeViewFindForm|d62f0505-6768-4b51-86a6-991c29d9bfad", "Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(368, 52, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 21, true);
			this.CloseButton.TabIndex = 7;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// MatchCaseCheckBox
			// 
			this.MatchCaseCheckBox.AutoSize = true;
			this.MatchCaseCheckBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("TreeViewFindForm|057fdc6c-515c-4aeb-a401-0c47c2a522a9", "Match Case");
			this.MatchCaseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 34, true);
			this.MatchCaseCheckBox.Name = "MatchCaseCheckBox";
			this.MatchCaseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 16, true);
			this.MatchCaseCheckBox.TabIndex = 2;
			// 
			// SearchFromSelectedNodeCheckBox
			// 
			this.SearchFromSelectedNodeCheckBox.AutoSize = true;
			this.SearchFromSelectedNodeCheckBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("TreeViewFindForm|32d0dbd3-7f42-4179-bd56-1df4d9057ae5", "Search From Selected Node");
			this.SearchFromSelectedNodeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 54, true);
			this.SearchFromSelectedNodeCheckBox.Name = "SearchFromSelectedNodeCheckBox";
			this.SearchFromSelectedNodeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 16, true);
			this.SearchFromSelectedNodeCheckBox.TabIndex = 3;
			// 
			// TreeViewFindForm
			// 
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("TreeViewFindForm|6dd369b7-e419-4a86-b5bd-65cf59ca2d4f", "Find");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 98, true);
			this.Controls.Add(this.SearchFromSelectedNodeCheckBox);
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
			this.Name = "TreeViewFindForm";
			this.TopMost = true;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.FindTextBox, 0);
			this.Controls.SetChildIndex(this.FindLabel, 0);
			this.Controls.SetChildIndex(this.FindNextButton, 0);
			this.Controls.SetChildIndex(this.FindPreviousButton, 0);
			this.Controls.SetChildIndex(this.MatchesLabel, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.MatchCaseCheckBox, 0);
			this.Controls.SetChildIndex(this.SearchFromSelectedNodeCheckBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		#region Controls

		protected CargoWise.Windows.UI.KTextBox FindTextBox;
		private Enterprise.ZArchitecture.ZLabel FindLabel;
		protected Enterprise.ZArchitecture.GUI.ZButton FindNextButton;
		protected Enterprise.ZArchitecture.GUI.ZButton FindPreviousButton;
		protected Enterprise.ZArchitecture.ZLabel MatchesLabel;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox MatchCaseCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox SearchFromSelectedNodeCheckBox;

		#endregion
	}
}
