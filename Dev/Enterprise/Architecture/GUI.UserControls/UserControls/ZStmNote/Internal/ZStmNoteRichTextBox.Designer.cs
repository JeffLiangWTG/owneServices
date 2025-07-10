namespace Enterprise.ZArchitecture.GUI.Internal
{
	partial class ZStmNoteRichTextBox
	{
		internal InternalZRichTextBox NoteRichTextBox;
		internal Enterprise.ZArchitecture.GUI.Internal.ZStmNoteRichTextBox.InternalZTextBox NoteTextBox;
		Enterprise.ZArchitecture.ZLabel TextOnlyLabel;
		internal Enterprise.ZArchitecture.GUI.ZButton PopupButton;
		internal Enterprise.ZArchitecture.GUI.ZButton ModifyButton;
		internal Enterprise.ZArchitecture.GUI.ZButton OverrideValidationButton;

		void InitializeComponent()
		{
			this.NoteRichTextBox = new Enterprise.ZArchitecture.GUI.Internal.ZStmNoteRichTextBox.InternalZRichTextBox();
			this.NoteTextBox = new Enterprise.ZArchitecture.GUI.Internal.ZStmNoteRichTextBox.InternalZTextBox();
			this.TextOnlyLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PopupButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ModifyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OverrideValidationButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// NoteRichTextBox
			// 
			this.NoteRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.NoteRichTextBox.BindTo = "ST_NoteData";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NoteRichTextBox, false);
			this.NoteRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NoteRichTextBox.MaxLength = 10000000;
			this.NoteRichTextBox.Name = "NoteRichTextBox";
			this.NoteRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 236, true);
			this.NoteRichTextBox.TabIndex = 0;
			// 
			// NoteTextBox
			// 
			this.NoteTextBox.AcceptsReturn = true;
			this.NoteTextBox.AcceptsTab = true;
			this.NoteTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.NoteTextBox.BindTo = "ST_NoteText";
			this.NoteTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NoteTextBox, false);
			this.NoteTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NoteTextBox.Multiline = true;
			this.NoteTextBox.Name = "NoteTextBox";
			this.NoteTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.NoteTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 236, true);
			this.NoteTextBox.TabIndex = 5;
			this.NoteTextBox.Visible = false;
			// 
			// TextOnlyLabel
			// 
			this.TextOnlyLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNoteRichTextBox|4f5a5ade-263e-4182-a8b2-09204281e973", "This is a text-only note and does not contain images or formatting (e.g. Bold text)");
			this.TextOnlyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TextOnlyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 0, true);
			this.TextOnlyLabel.Name = "TextOnlyLabel";
			this.TextOnlyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(602, 23, true);
			this.TextOnlyLabel.TabIndex = 1;
			this.TextOnlyLabel.Visible = false;
			// 
			// OverrideValidationButton
			//
			this.OverrideValidationButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNoteRichTextBox|985caf75-b9bf-4125-9c91-3c08c9ddf109", "Override");
			this.OverrideValidationButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OverrideValidationButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverrideValidationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(436, 0, true);
			this.OverrideValidationButton.Name = "OverrideValidationButton";
			this.OverrideValidationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.OverrideValidationButton.TabIndex = 2;
			this.OverrideValidationButton.Click += new System.EventHandler(this.OverrideValidationButton_Click);
			// 
			// PopupButton
			//
			this.PopupButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNoteRichTextBox|ADD74614-E2B5-4ac9-87F5-50C7E04DB564", "Popup");
			this.PopupButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PopupButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PopupButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(502, 0, true);
			this.PopupButton.Name = "PopupButton";
			this.PopupButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.PopupButton.TabIndex = 4;
			this.PopupButton.Click += new System.EventHandler(this.PopupButton_Click);
			// 
			// ModifyButton
			// 
			this.ModifyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ModifyButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNoteRichTextBox|84a34c8c-366d-4ddc-a38f-5e4e13a0ef80", "Modify");
			this.ModifyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(432, 0, true);
			this.ModifyButton.Name = "ModifyButton";
			this.ModifyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.ModifyButton.TabIndex = 3;
			this.ModifyButton.Visible = false;
			this.ModifyButton.Click += new System.EventHandler(this.ModifyButton_Click);
			// 
			// ZStmNoteRichTextBox
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OverrideValidationButton);
			this.Controls.Add(this.ModifyButton);
			this.Controls.Add(this.PopupButton);
			this.Controls.Add(this.TextOnlyLabel);
			this.Controls.Add(this.NoteRichTextBox);
			this.Controls.Add(this.NoteTextBox);
			this.Name = "ZStmNoteRichTextBox";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 236, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
