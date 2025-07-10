namespace Enterprise.ZArchitecture.GUI.Tools.SpellCheck
{
	partial class PersonalDictionaryForm
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
			this.wordsListBox = new Enterprise.ZArchitecture.GUI.ZListBox();
			this.saveButtonUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.removeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.saveButtonUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 324, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 24, true);
			// 
			// wordsListBox
			// 
			this.wordsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.wordsListBox.FormattingEnabled = true;
			this.wordsListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.wordsListBox.Name = "wordsListBox";
			this.wordsListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 264, true);
			this.wordsListBox.TabIndex = 1;
			this.wordsListBox.SelectedIndexChanged += new System.EventHandler(this.wordsListBox_SelectedIndexChanged);
			// 
			// saveButtonUserControl
			// 
			this.saveButtonUserControl.AllowDrop = true;
			this.saveButtonUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 293, true);
			this.saveButtonUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.saveButtonUserControl.Name = "saveButtonUserControl";
			this.saveButtonUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.saveButtonUserControl.TabIndex = 11;
			// 
			// removeButton
			// 
			this.removeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.removeButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("2315d026-a436-42fc-bcb0-5e247e4e57b8", "Remove");
			this.removeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 12, true);
			this.removeButton.Name = "removeButton";
			this.removeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 44, true);
			this.removeButton.TabIndex = 12;
			this.removeButton.ToolTipCaption = null;
			this.removeButton.UseVisualStyleBackColor = true;
			this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
			// 
			// PersonalDictionaryForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("514aec84-8d72-43e6-a0f2-638f4fc12d3e", "My Dictionary");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 348, true);
			this.Controls.Add(this.removeButton);
			this.Controls.Add(this.wordsListBox);
			this.Controls.Add(this.saveButtonUserControl);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 387, true);
			this.Name = "PersonalDictionaryForm";
			this.Controls.SetChildIndex(this.saveButtonUserControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.wordsListBox, 0);
			this.Controls.SetChildIndex(this.removeButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.saveButtonUserControl.ResumeLayout(true);
			this.saveButtonUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZListBox wordsListBox;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl saveButtonUserControl;
		private ZButton removeButton;
	}
}
