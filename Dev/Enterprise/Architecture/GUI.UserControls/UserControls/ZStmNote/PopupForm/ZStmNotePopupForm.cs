using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.ZArchitecture.GUI.UserControls.Res;

namespace Enterprise.ZArchitecture.GUI
{
	[TestExcludeZWinFormHasTypedConstructor] // The master type is unknown and must be BusinessObject.
	public partial class ZStmNotePopupForm : ZStmNotePopupViewForm
	{
		#region Construction

		public ZStmNotePopupForm(StmNote note, bool resetHasChangesOnCancelEdit, bool deleteNoteOnCancelEdit, Form parentForm = null)
			: this(note, note.Master, resetHasChangesOnCancelEdit, deleteNoteOnCancelEdit, parentForm)
		{
		}

		public ZStmNotePopupForm(StmNote note, IStmNoteParent noteParent, bool resetHasChangesOnCancelEdit, bool deleteNoteOnCancelEdit, Form parentForm = null)
			: base(note.GetCloneForPopup())
		{
			Icon = Icons.GetIcon(IconTypes.StmNote);
			bizO = noteParent;
			if (parentForm != null && parentForm is ZForm parentZForm)
			{
				NoteUserControl.NoteTextBox.NoteRichTextBox.ParentZForm = parentZForm;
			}
			originalNote = note;
			this.resetHasChangesOnCancelEdit = resetHasChangesOnCancelEdit;
			this.deleteNoteOnCancelEdit = deleteNoteOnCancelEdit;
			cancelNoteOnClose = true;
		}

		#endregion

		public delegate void NoteHasChangesEventHandler();
		public event NoteHasChangesEventHandler NoteHasChangesChanged;

		public bool ConfirmCancelChanges { get; set; }

		internal void SetCaretLocationAfterBindingHasFinished(int location) => NoteUserControl.NoteTextBox.SetCaretLocationAfterBindingHasFinished(location);

#if DEBUG
		internal
#endif
		protected StmNote ClonedNote => BusinessEntity;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Ok / Cancel Buttons

		void OKButton_Click(object sender, System.EventArgs e) => HandleOkButton();

		protected void HandleOkButton()
		{
			var clonedNote = ClonedNote;
			clonedNote.RunPreSaveValidation();

			if (!clonedNote.HasErrors && !originalNote.IsDeleted)
			{
				if (clonedNote.HasChanges)
				{
					originalNote.CopyChangesFromPopup(clonedNote);
					((BusinessObject)bizO).HasChanges = true;
					NoteHasChangesChanged?.Invoke();
				}

				cancelNoteOnClose = false;

				Close(); // - must close before delete as losing focus on the text box will validate the note
						 // - we don't want to validate after deleting (would blow up)

				clonedNote.Delete();
			}
			else
			{
				_ = ShowErrorsDialog();
			}
		}

		protected override DialogResult ShowErrorsDialogCore(bool includeIgnoreOption)
		{
			if (originalNote.IsDeleted)
			{
				Globals.Message.ShowError(Res.GetString("6b0df84f-59cd-46f8-9a79-2931592e6514", "The original note has been deleted from the database. Your changes have not been saved.\r\n Please make a new note."), Res.GetString("1d0203c7-5cc6-427c-bcbd-1f5d82d2d8d0", "Error - Deleted Note"));
				return DialogResult.Abort;
			}
			else
			{
				return base.ShowErrorsDialogCore(includeIgnoreOption);
			}
		}

		#endregion

		#region Cancelling a Note on close

		protected override void OnClosing(CancelEventArgs e)
		{
			if (cancelNoteOnClose)
			{
				if (ConfirmCancelChanges && ClonedNote.HasChanges)
				{
					var saveOption = Globals.Message.Show(
						Res.GetString("e554bd73-bf18-458f-8322-26ae92c0e06f", "This note has been modified.\r\nWould you like to save the changes?"),
						Res.GetString("8400942d-4f60-4167-abd9-6feeed0875fc", "Warning"),
						MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

					if (saveOption == DialogResult.Yes)
					{
						HandleOkButton();
						return;
					}
					else if (saveOption == DialogResult.Cancel)
					{
						e.Cancel = true;
						return;
					}
				}

				CancelNote();
			}

			base.OnClosing(e);
		}

		void CancelNote()
		{
			ClonedNote.Delete();

			if (deleteNoteOnCancelEdit)
			{
				originalNote.Delete();
			}
			if (resetHasChangesOnCancelEdit && bizO != null)
			{
				((BusinessObject)bizO).HasChanges = false;
			}
		}

		#endregion

		readonly IStmNoteParent bizO;
		readonly StmNote originalNote;
		bool cancelNoteOnClose;
		readonly bool resetHasChangesOnCancelEdit;
		readonly bool deleteNoteOnCancelEdit;

		#region IDisposable Members

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

		#endregion
	}
}
