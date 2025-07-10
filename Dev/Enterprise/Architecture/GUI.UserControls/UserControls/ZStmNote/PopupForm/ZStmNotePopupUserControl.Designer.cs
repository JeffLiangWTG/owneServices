
namespace Enterprise.ZArchitecture.GUI
{
	using CargoWise.Windows.UI.Testing;

	[SuppressFormDesignerAnalysis]
	[SuppressBindingMemberBashingTest]

	internal partial class ZStmNotePopupUserControl
	{
		#region Auto

		internal Enterprise.ZArchitecture.GUI.Internal.ZStmNoteRichTextBox NoteTextBox;
		Enterprise.ZArchitecture.ZLabel NoteDescriptionLabel;

		void InitializeComponent()
		{
			this.NoteDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NoteTextBox = new Enterprise.ZArchitecture.GUI.Internal.ZStmNoteRichTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Business.StmNote);
			// 
			// NoteDescriptionLabel
			// 
			this.NoteDescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.NoteDescriptionLabel, "ST_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.ZArchitecture.Business.StmNote)(null)).ST_Description)));
			this.NoteDescriptionLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNotePopupUserControl|efec4625-033f-4379-9ad9-3c77510e9df8", "Marks & Numbers Note");
			this.NoteDescriptionLabel.IsFontBold = true;
			this.NoteDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.NoteDescriptionLabel.Name = "NoteDescriptionLabel";
			this.NoteDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 20, true);
			this.NoteDescriptionLabel.TabIndex = 12;
			this.NoteDescriptionLabel.UseMnemonic = false;
			// 
			// NoteTextBox
			// 
			this.NoteTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.NoteTextBox.BindToRtfNote = "ST_NoteData";
			this.NoteTextBox.BindToTextNote = "ST_NoteText";
			this.NoteTextBox.IsDescriptionVisibleInTextMode = false;
			this.NoteTextBox.IsModifyButtonVisible = false;
			this.NoteTextBox.IsPopupButtonVisible = false;
			this.NoteTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 28, true);
			this.NoteTextBox.Name = "NoteTextBox";
			this.NoteTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 144, true);
			this.NoteTextBox.TabIndex = 0;
			this.BindingSource.SetBindingMember(NoteTextBox, ".");
			// 
			// ZStmNotePopupUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.NoteTextBox);
			this.Controls.Add(this.NoteDescriptionLabel);
			this.Name = "ZStmNotePopupUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 176, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion
	}
}
