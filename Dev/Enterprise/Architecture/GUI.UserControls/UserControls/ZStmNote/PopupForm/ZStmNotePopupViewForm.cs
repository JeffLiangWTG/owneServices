using System;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZStmNotePopupViewForm : ZChildForm
	{
		public ZStmNotePopupViewForm()
		{
		}

		public ZStmNotePopupViewForm(StmNote businessEntity)
			: base(businessEntity)
		{
		}

		public override sealed string FormHeading
			=> BusinessEntity.IsDeleted
				? string.Empty
				: (!FormHeadingPrefix.IsEmpty ? FormHeadingPrefix + " - " : string.Empty) + Enterprise.ZArchitecture.GUI.UserControls.Res.GetString("ae1f8681-5670-4613-aad7-801b24012666", "{0} Note", BusinessEntity.ST_Description);

		#region Form Heading Prefix

		public ZString FormHeadingPrefix { get; set; }

		#endregion

		public new StmNote BusinessEntity => (StmNote)base.BusinessEntity;

		public ZString LeadingComment
		{
			[System.Diagnostics.DebuggerStepThrough]
			get => leadingCommentLabel.Text;
			set
			{
				if (value.IsEmpty)
				{
					leadingCommentLabel.Text = string.Empty;
					leadingCommentLabel.Visible = false;
					ControlDpiScalingHelper.SetHeight(ref leadingCommentLabel, 0, true);

					ControlDpiScalingHelper.SetHeight(ref NoteUserControl, NoteUserControl.Bottom - ControlDpiScalingHelper.ScaleToCurrentDpiY(4), false);
					ControlDpiScalingHelper.SetTop(ref NoteUserControl, 4, true);
				}
				else
				{
					var g = leadingCommentLabel.CreateGraphics();
					var commentTextWidth = leadingCommentLabel.Width - leadingCommentLabel.Padding.Left - leadingCommentLabel.Padding.Right;
					var size = g.MeasureString(value, leadingCommentLabel.Font, commentTextWidth);
					var newCommentHeight = (int)size.Height + leadingCommentLabel.Padding.Top + leadingCommentLabel.Padding.Bottom;

					ControlDpiScalingHelper.SetHeight(ref NoteUserControl, NoteUserControl.Bottom - ControlDpiScalingHelper.ScaleToCurrentDpiY(4) - newCommentHeight, false);
					ControlDpiScalingHelper.SetTop(ref NoteUserControl, ControlDpiScalingHelper.ScaleToCurrentDpiY(4) + newCommentHeight, false);

					leadingCommentLabel.Text = value;
					ControlDpiScalingHelper.SetHeight(ref leadingCommentLabel, newCommentHeight, false);
					leadingCommentLabel.Visible = true;
				}
			}
		}

		void CloseButton_Click(object sender, EventArgs e) => Close();

		public void DisableEdit()
		{
			NoteUserControl.NoteTextBox.SetReadOnly(true);
			NoteUserControl.NoteTextBox.NoteRichTextBox.IsToolBarVisible = false;
		}
	}
}
