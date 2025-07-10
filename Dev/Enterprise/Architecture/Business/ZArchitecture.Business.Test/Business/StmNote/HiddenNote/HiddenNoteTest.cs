using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class HiddenNoteTest : TestCaseWithDummy
	{
		public void TestNoteProperty()
		{
			AssertNull("Precondition - HiddenNote.note should be null", fNoteField.GetValue(HiddenNote));

			AssertNotNull("Note should have been either loaded or newed.", HiddenNote.Note);
			AssertEquals("Note.ST_ParentID should be set to Dummy.PK", Dummy.PK, HiddenNote.Note.ST_ParentID);
			AssertEquals("Note.ST_Table should be set to Dummy.TableName", Dummy.TableName, HiddenNote.Note.ST_Table);
			AssertEquals("Note.ST_Description should be set to overridden Description", "Dummy Description", HiddenNote.Note.ST_Description);

			// ensure note is reloaded from factory
			fNoteField.SetValue(HiddenNote, null);
			AssertNull("Precondition - HiddenNote.note should be null", fNoteField.GetValue(HiddenNote));

			AssertNotNull(HiddenNote.Note);
			AssertEquals("Note.ST_ParentID should be set to Dummy.PK", Dummy.PK, HiddenNote.Note.ST_ParentID);
			AssertEquals("Note.ST_Table should be set to Dummy.TableName", Dummy.TableName, HiddenNote.Note.ST_Table);
			AssertEquals("Note.ST_Description should be set to overridden Description", "Dummy Description", HiddenNote.Note.ST_Description);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestHiddenNoteDescriptionMustNotBeEmpty()
		{
			HiddenStmNote note = HiddenNoteWithEmptyDescription.Note;
		}

		public void TestEmptyNoteIsDeletedOnSave()
		{
			AssertNotNull("Precondition - Note should have been either loaded or newed.", HiddenNote.Note);

			HiddenNote.SetIsEmpty(false);
			HiddenNote.Note.Factory.Save();
			AssertEquals("Note is not empty and should not have been deleted", false, HiddenNote.Note.IsDeleted);

			HiddenNote.SetIsEmpty(true);
			StmNote deletedNote = (StmNote)fNoteField.GetValue(HiddenNote);
			HiddenNote.Note.Factory.Save();
			AssertEquals("Note is empty and should have been deleted", true, deletedNote.IsDeleted);
		}

		public void TestDelete()
		{
			AssertNotNull("Precondition - Note should have been either loaded or newed.", HiddenNote.Note);
			StmNote noteToBeDeleted = (StmNote)fNoteField.GetValue(HiddenNote);
			AssertEquals("Precondition - Note.IsDeleted should be false", false, noteToBeDeleted.IsDeleted);

			HiddenNote.Delete();
			AssertEquals("Note should have been deleted", true, noteToBeDeleted.IsDeleted);
			AssertEquals("Note should have been either loaded or newed .IsDeleted == false.", false, HiddenNote.Note.IsDeleted);
		}

		public void TestTouchingNoteDoesNotIncreaseDBLoadCount()
		{
			AssertEquals("Precondition", 0, Factory.DatabaseLoadCount);
			HiddenStmNote note = HiddenNote.Note;
			AssertEquals(0, Factory.DatabaseLoadCount);
		}

		public void TestFetchHintDecreasesDBLoadCount()
		{
			const int elementsToTest = 10;
			DummyEnterpriseBusinessObject[] bizOs = new DummyEnterpriseBusinessObject[elementsToTest];
			for (int i = 0; i < elementsToTest; i++)
			{
				bizOs[i] = Factory.New<DummyEnterpriseBusinessObject>();
				new DummyHiddenNote(bizOs[i]);
			}
			Factory.Save();

			DummyHiddenNote[] hiddenNotes = new DummyHiddenNote[elementsToTest];
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			for (int i = 0; i < elementsToTest; i++)
			{
				DummyEnterpriseBusinessObject bizOInFactory2 = factory2.Load<DummyEnterpriseBusinessObject>(bizOs[i].PK);
				hiddenNotes[i] = new DummyHiddenNote(bizOInFactory2);
			}

			AssertEquals(elementsToTest, factory2.DatabaseLoadCount);

			foreach (DummyHiddenNote hiddenNote in hiddenNotes)
			{
				object dummyCall = hiddenNote.Note;
			}
			AssertEquals(elementsToTest + 1, factory2.DatabaseLoadCount);
		}

		[ExpectNoExceptions("The HiddenNoteFetchHint requires the Description property, if your Description property requires an external input (i.e. from your constructor), the FetchHint should be disabled")]
		public void TestDisableFetchHint()
		{
			DummyEnterpriseBusinessObject dummy = Factory.New<DummyEnterpriseBusinessObject>();
			DummyHiddenNoteWithFetchHintDisabled note = new DummyHiddenNoteWithFetchHintDisabled(dummy, () => "Description Overrides");
		}

		public void TestDeleteInnerNoteWithMultipleInstances()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			var note1 = new DummyHiddenNoteWithCalculableIsEmpty(dummy);
			var note2 = new DummyHiddenNoteWithCalculableIsEmpty(dummy);

			AssertNotNull(note1.Note);
			AssertNotNull(note2.Note);
			AssertSame(note1.Note, note2.Note);

			AssertNoExceptionThrown("Double Factory_Saving invoking should not access deleted row", Factory.Save);
		}

		public void TestOnFactorySaving_WhenNoteIsDeleted_ShouldNotCheckIsEmpty()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			var hiddenNote = new DummyHiddenNoteWithExceptionOnIsEmptyAccessWhenDeleted(dummy);
			AssertNotNull("Note should be loaded", hiddenNote.Note);
			hiddenNote.Note.Delete();

			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestDoesNotUseValidation()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			var hiddenNote = new DummyHiddenNote(dummy);
			var note = hiddenNote.Note;
			AssertEquals(true, note.IsValidationSuspended);
		}

		#region Implementation

		DummyHiddenNote HiddenNote;
		DummyHiddenNoteWithEmptyDescription HiddenNoteWithEmptyDescription;
		FieldInfo fNoteField;

		protected override Type TypeOfDummy
		{
			get { return typeof(DummyEnterpriseBusinessObject); }
		}

		protected override void SetUp()
		{
			// We want to ensure the ProcessFieldChangeRules are pre-loaded in the UberCache so that HitCounts are accurate
			new BusinessObjectFactory().New<DummyBusinessObject>();
			base.SetUp();
			HiddenNote = new DummyHiddenNote((IStmNoteParent)Dummy);
			HiddenNoteWithEmptyDescription = new DummyHiddenNoteWithEmptyDescription((IStmNoteParent)Dummy);
			fNoteField = typeof(HiddenNote).GetField("note", BindingFlags.Instance | BindingFlags.NonPublic);
		}

		#endregion

		#region Test Classes

		class DummyHiddenNoteWithExceptionOnIsEmptyAccessWhenDeleted : HiddenNote
		{
			internal DummyHiddenNoteWithExceptionOnIsEmptyAccessWhenDeleted(IStmNoteParent parent)
				: base(parent)
			{
			}

			protected internal override ZString Description
			{
				get
				{
					return description;
				}
			}
			internal ZString description = "Description";

			protected override bool IsEmpty
			{
				get
				{
					EnsureNotAccessingWhenDeleted();
					return isEmpty;
				}
			}
			internal bool isEmpty;

			void EnsureNotAccessingWhenDeleted()
			{
				if (Note.IsDeleted)
				{
					throw new Exception("Property accessed when deleted");
				}
			}

			internal new HiddenStmNote Note
			{
				get { return base.Note; }
			}
		}

		class DummyHiddenNoteWithFetchHintDisabled : HiddenNote
		{
			public DummyHiddenNoteWithFetchHintDisabled(IStmNoteParent parent, Func<string> descriptionFunc)
				: base(parent, false)
			{
				this.descriptionFunc = descriptionFunc;
			}

			protected internal override ZString Description
			{
				get { return descriptionFunc(); }
			}

			protected override bool IsEmpty { get { return false; } }

			readonly Func<string> descriptionFunc;
		}

		class DummyHiddenNote : HiddenNote
		{
			public DummyHiddenNote(IStmNoteParent parent)
				: base(parent)
			{
			}

			protected internal override ZString Description
			{
				get { return "Dummy Description"; }
			}

			public new HiddenStmNote Note
			{
				get { return base.Note; }
			}

			public void SetIsEmpty(bool value)
			{
				fIsEmpty = value;
			}

			protected override bool IsEmpty
			{
				get { return fIsEmpty; }
			}

			bool fIsEmpty;
		}

		class DummyHiddenNoteWithEmptyDescription : DummyHiddenNote
		{
			public DummyHiddenNoteWithEmptyDescription(IStmNoteParent parent)
				: base(parent)
			{
			}

			protected internal override ZString Description
			{
				get { return ""; }
			}
		}

		class DummyHiddenNoteWithCalculableIsEmpty : DummyHiddenNote
		{
			public DummyHiddenNoteWithCalculableIsEmpty(IStmNoteParent parent) : base(parent) { }

			protected override bool IsEmpty
			{
				get { return !HasNote || Note.ST_NoteData.IsEmpty; }
			}
		}

		#endregion
	}
}
