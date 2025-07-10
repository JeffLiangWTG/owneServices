using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(ZStmNotePopupForm))]
	sealed class ZStmNotePopupFormTest : ZStmNotePopupViewFormTest
	{
		public void TestDeletedNote()
		{
			Note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			Note.ST_NoteDataAsText = "Oompa Loompa";
			Note.Validation.ValidateAll();

			AssertEquals("Precondition - the Note must have no errors to test the delete on close.", false, Note.HasErrors);

			using (var form = new ZStmNotePopupFormForTest(Note, Dummy, null))
			{
				form.Show();

				Factory.Load(typeof(StmNote), Note.PK).Delete();
				Factory.Save();

				form.ClonedNote.HasChanges = true;
				AssertNoExceptionThrown(form.HandleOkButton);
			}
		}

		public void TestReadOnlyNoteIsReadonlyOnForm()
		{
			Note.ReadOnly = true;

			using (var form = GetFormToBash())
			{
				AssertEquals("Form's bound business entity is readonly.", true, form.BusinessEntity.ReadOnly);
			}

			Note.ReadOnly = false;

			using (var form = GetFormToBash())
			{
				AssertEquals("Form's bound business entity is not readonly.", false, form.BusinessEntity.ReadOnly);
			}
		}

		public void TestClosingFormBeforeLosingTextBoxFocusDoesNotBlowUp()
		{
			Note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			Note.ST_NoteDataAsText = "Oompa Loompa";
			Note.Validation.ValidateAll();

			AssertEquals("Precondition - the Note must have no errors to test the delete on close.", false, Note.HasErrors);

			using (var form = new ZStmNotePopupFormForTest(Note, Dummy, null))
			{
				form.Show();
				form.HandleOkButton(); // Ensures no DataRowNotInTableException is thrown.
			}
		}

		public void TestStmNotePopupFormWillUseOwnerParentAsZParentForm()
		{
			using (var parentForm = new ZForm())
			{
				parentForm.Text = "I am a Parent.";
				using (var form = new ZStmNotePopupFormForTest(Note, Dummy, parentForm))
				{
					AssertEquals(form.NoteUserControl.NoteTextBox.NoteRichTextBox.ParentZForm, parentForm);
				}
			}
		}

		public void TestConfirmCancelChanges()
		{
			Note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			Note.ST_NoteDataAsText = "Xyz";
			Note.HasChanges = false;

			using (var form = new ZStmNotePopupFormForTest(Note, Dummy, null))
			{
				Assert(!form.ConfirmCancelChanges);
				form.Show();
				form.ClonedNote.ST_NoteDataAsText = "Abracadabra";
				Assert(form.ClonedNote.HasChanges);
				form.Close();
				Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
				AssertEquals("Xyz", Note.ST_NoteDataAsText);
			}

			using (var form = new ZStmNotePopupFormForTest(Note, Dummy, null))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				form.ConfirmCancelChanges = true;
				Assert(form.ConfirmCancelChanges);
				form.Show();
				form.ClonedNote.ST_NoteDataAsText = "Abracadabra";
				Assert(form.ClonedNote.HasChanges);
				form.Close();
				AssertEquals("This note has been modified.\r\nWould you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Xyz", Note.ST_NoteDataAsText);
			}

			using (var form = new ZStmNotePopupFormForTest(Note, Dummy, null))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.ConfirmCancelChanges = true;
				Assert(form.ConfirmCancelChanges);
				form.Show();
				form.ClonedNote.ST_NoteDataAsText = "Abracadabra";
				Assert(form.ClonedNote.HasChanges);
				form.Close();
				AssertEquals("This note has been modified.\r\nWould you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Abracadabra", Note.ST_NoteDataAsText);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestStmNotePopupFormWillUseNonZFormAsZParentForm()
		{
			using (var parentForm = new ZMainForm())
			{
				using (var form = new ZStmNotePopupFormForTest(Note, Dummy, parentForm))
				{
					AssertNotEquals(form.NoteUserControl.NoteTextBox.NoteRichTextBox.ParentZForm, parentForm);
				}
			}
		}

		#region class ZStmNotePopupFormForTest

		class ZStmNotePopupFormForTest : ZStmNotePopupForm
		{
			public ZStmNotePopupFormForTest(StmNote note, IStmNoteParent parent, Form parentForm)
				: base(note, parent, false, false, parentForm)
			{
			}

			public new void HandleOkButton() => base.HandleOkButton();
		}

		#endregion

		#region Implementation

		new ZStmNotePopupForm GetFormToBash() => (ZStmNotePopupForm)base.GetFormToBash();

		protected override Form GetFormToBashCore() => new ZStmNotePopupForm(Note, Dummy, false, false, null);

		protected override void AddHasChangesIsTrueException(ZForm formWithChanges)
		{
			// Editing a note via this form should set the parent form's HasChanges to true.
		}

		DummyEnterpriseBusinessObject Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = DummyEnterpriseBusinessObject.New(Factory);
				}
				return dummy;
			}
		}

		StmNote Note
		{
			get
			{
				if (note == null)
				{
					note = Factory.New<StmNote>();
					note.ST_Table = "DummyBizo";
					note.ST_ParentID = Factory.NewWithValidTestData<DummyBusinessObject>().PK;
					Factory.Save();
				}
				return note;
			}
		}

		DummyEnterpriseBusinessObject dummy;
		StmNote note;

		#endregion
	}
}
