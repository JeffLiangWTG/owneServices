namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class ImportABMForm
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
		protected new void InitializeComponent()
		{
			this.PeriodStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.FilePathTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SelectFileButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ImportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MessageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeleteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PeriodStartDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 196, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 5, true);
			this.MainStatusBar.Visible = false;
			// 
			// PeriodStartDateEdit
			// 
			this.PeriodStartDateEdit.AllowDrop = true;
			this.PeriodStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.PeriodStartDateEdit.AutoCompleteYear = true;
			this.PeriodStartDateEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("614de477-3f5c-42c4-9234-6f782f15815a", "Date From");
			this.PeriodStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 19, true);
			this.PeriodStartDateEdit.Name = "PeriodStartDateEdit";
			this.PeriodStartDateEdit.TabIndex = 0;
			// 
			// FilePathTextBox
			// 
			this.FilePathTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("acdd499a-6eb8-41a7-a132-d3049f829533", "File Path");
			this.FilePathTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 47, true);
			this.FilePathTextBox.Name = "FilePathTextBox";
			this.FilePathTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(274, 20, true);
			this.FilePathTextBox.TabIndex = 2;
			// 
			// SelectFileButton
			// 
			this.SelectFileButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("9a788a94-9e5b-4013-9f6c-b92c102ff6b9", "Browse");
			this.SelectFileButton.IsCaptionOverridden = false;
			this.SelectFileButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 45, true);
			this.SelectFileButton.Name = "SelectFileButton";
			this.SelectFileButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SelectFileButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.SelectFileButton.TabIndex = 3;
			this.SelectFileButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SelectFileButton.ToolTipCaption = null;
			this.SelectFileButton.UseVisualStyleBackColor = true;
			this.SelectFileButton.Click += new System.EventHandler(this.FileSelectButton_Click);
			// 
			// ImportButton
			// 
			this.ImportButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("2f8f14e2-9b52-4077-abc4-570c316e99eb", "Import");
			this.ImportButton.IsCaptionOverridden = false;
			this.ImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 158, true);
			this.ImportButton.Name = "ImportButton";
			this.ImportButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 23, true);
			this.ImportButton.TabIndex = 5;
			this.ImportButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ImportButton.ToolTipCaption = null;
			this.ImportButton.UseVisualStyleBackColor = true;
			this.ImportButton.Click += new System.EventHandler(this.ImportButton_Click);
			// 
			// MessageTextBox
			// 
			this.MessageTextBox.CaptionResourceString = null;
			this.MessageTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageTextBox, false);
			this.MessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 74, true);
			this.MessageTextBox.Multiline = true;
			this.MessageTextBox.Name = "MessageTextBox";
			this.MessageTextBox.ReadOnly = true;
			this.MessageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 71, true);
			this.MessageTextBox.TabIndex = 4;
			// 
			// DeleteButton
			// 
			this.DeleteButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("7144cfee-2c98-4047-b4a5-63a800e91efc", "Delete");
			this.DeleteButton.IsCaptionOverridden = false;
			this.DeleteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 17, true);
			this.DeleteButton.Name = "DeleteButton";
			this.DeleteButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.DeleteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 23, true);
			this.DeleteButton.TabIndex = 1;
			this.DeleteButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.DeleteButton.ToolTipCaption = null;
			this.DeleteButton.UseVisualStyleBackColor = true;
			this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
			// 
			// ImportABMForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 201, true);
			this.Controls.Add(this.DeleteButton);
			this.Controls.Add(this.MessageTextBox);
			this.Controls.Add(this.ImportButton);
			this.Controls.Add(this.SelectFileButton);
			this.Controls.Add(this.FilePathTextBox);
			this.Controls.Add(this.PeriodStartDateEdit);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 240, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 240, true);
			this.Name = "ImportABMForm";
			this.Text = "Import ABM Customs Data";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PeriodStartDateEdit, 0);
			this.Controls.SetChildIndex(this.FilePathTextBox, 0);
			this.Controls.SetChildIndex(this.SelectFileButton, 0);
			this.Controls.SetChildIndex(this.ImportButton, 0);
			this.Controls.SetChildIndex(this.MessageTextBox, 0);
			this.Controls.SetChildIndex(this.DeleteButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PeriodStartDateEdit.ResumeLayout(true);
			this.PeriodStartDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDateEdit PeriodStartDateEdit;
		private ZArchitecture.ZTextBox FilePathTextBox;
		private ZArchitecture.GUI.ZButton SelectFileButton;
		private ZArchitecture.GUI.ZButton ImportButton;
		private ZArchitecture.ZTextBox MessageTextBox;
		private ZArchitecture.GUI.ZButton DeleteButton;
	}
}