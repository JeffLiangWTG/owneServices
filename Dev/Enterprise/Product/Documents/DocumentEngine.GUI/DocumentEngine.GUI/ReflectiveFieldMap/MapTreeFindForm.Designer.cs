using System.Windows.Forms;

namespace Enterprise.DocumentEngine.GUI.ReflectiveFieldMap
{
	partial class MapTreeFindForm
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
			this.FindNextButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ResetButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FindPreviousButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Label = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 75, true);
			this.MainStatusBar.ShowPanels = false;
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 8, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 7;
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(365);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(int);
			// 
			// FindTextBox
			// 
			this.FindTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("751dfbd4-f4ba-42a0-bc0e-bfcd25fc1d52", "Find What", "Keyword to find");
			this.FindTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 18, true);
			this.FindTextBox.Name = "FindTextBox";
			this.FindTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 18, true);
			this.FindTextBox.TabIndex = 1;
			this.FindTextBox.TextChanged += new System.EventHandler(this.FindTextBox_TextChanged);
			// 
			// FindNextButton
			// 
			this.FindNextButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("TreeViewFindForm|42E70132-6A37-42CC-9A77-1266C25FBCE2", "Find Next");
			this.FindNextButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 18, true);
			this.FindNextButton.Name = "FindNextButton";
			this.FindNextButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.FindNextButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 21, true);
			this.FindNextButton.TabIndex = 3;
			this.FindNextButton.Click += new System.EventHandler(this.FindNextButton_Click);
			// 
			// ResetButton
			// 
			this.ResetButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("TreeViewFindForm|4F1BE424-189E-494B-9B42-3D65264D691E", "Reset Search");
			this.ResetButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 48, true);
			this.ResetButton.Name = "ResetButton";
			this.ResetButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ResetButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 22, true);
			this.ResetButton.TabIndex = 5;
			this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
			// 
			// FindPreviousButton
			// 
			this.FindPreviousButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("TreeViewFindForm|2415F19D-0630-4E49-A347-93757B11A46D", "Find Previous");
			this.FindPreviousButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 40, true);
			this.FindPreviousButton.Name = "FindPreviousButton";
			this.FindPreviousButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.FindPreviousButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 22, true);
			this.FindPreviousButton.TabIndex = 4;
			this.FindPreviousButton.Click += new System.EventHandler(this.FindPreviousButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("TreeViewFindForm|488CFED2-FF50-4BFD-AE07-B800C770D964", "Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 48, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 22, true);
			this.CloseButton.TabIndex = 6;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			//
			// Label
			//
			this.Label.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("TreeViewFindForm|53e475a5-e85b-41c1-bca9-2eee27b27354", "No Results");
			this.Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(194, 48, true);
			this.Label.Name = "Label";
			this.Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 22, true);
			// 
			// MapTreeFindForm
			// 
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("TreeViewFindForm|97410F17-312D-48EF-A972-CBC59DD0C2D4", "Find");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 83, true);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.FindPreviousButton);
			this.Controls.Add(this.FindNextButton);
			this.Controls.Add(this.ResetButton);
			this.Controls.Add(this.FindTextBox);
			this.Controls.Add(this.Label);
			this.DataSourceAssemblyName = "mscorlib";
			this.DataSourceType = typeof(int);
			this.DataSourceTypeName = "System.Int32";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 119, true);
			this.Name = "MapTreeFindForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.FindTextBox, 0);
			this.Controls.SetChildIndex(this.ResetButton, 0);
			this.Controls.SetChildIndex(this.FindNextButton, 0);
			this.Controls.SetChildIndex(this.FindPreviousButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.Label, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		#region Controls

		protected Enterprise.ZArchitecture.ZTextBox FindTextBox;
		protected Enterprise.ZArchitecture.GUI.ZButton FindNextButton;
		protected Enterprise.ZArchitecture.GUI.ZButton ResetButton;
		protected Enterprise.ZArchitecture.GUI.ZButton FindPreviousButton;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.ZLabel Label;

		#endregion
	}
}
