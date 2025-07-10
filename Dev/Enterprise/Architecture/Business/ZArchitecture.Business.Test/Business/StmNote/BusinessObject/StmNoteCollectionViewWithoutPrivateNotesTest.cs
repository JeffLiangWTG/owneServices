using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class StmNoteCollectionViewWithoutPrivateNotesTest : TestCaseWithFactory
	{
		public void TestRebuildWithNoteContextIsIncorrect()
		{
			var collection = new StmNoteCollectionViewNonPrivateNotesWithContext(Dummy.Notes, StmNoteContextUtils.StmNoteContextsAll, ZGuid.Empty);
			AssertEquals("Collection count should be 0", 0, collection.Count);

			var note1 = Dummy.Notes.AddNew();
			AssertEquals("Collection count should be 1", 1, collection.Count);

			note1.ST_NoteContext = "AA";

			AssertNoExceptionThrown(() => collection.Rebuild());
			AssertEquals("Collection count should be 0", 0, collection.Count);
			ErrorReporter.Clear();
		}

		public void TestConstructor()
		{
			StmNoteCollectionViewNonPrivateNotesWithContext view = new StmNoteCollectionViewNonPrivateNotesWithContext(Dummy.Notes, StmNoteContextUtils.StmNoteContextsAll, ZGuid.Empty);
			AssertNotNull(view);
		}

		public void TestRebuildingView()
		{
			StmNoteCollectionViewNonPrivateNotesWithContext view = new StmNoteCollectionViewNonPrivateNotesWithContext(Dummy.Notes, StmNoteContextUtils.StmNoteContextsAll, ZGuid.Empty);
			AssertEquals("StmNoteCollectionViewNonPrivateNotesWithContext.Count", 0, view.Count);

			StmNote prvNote = Dummy.Notes.AddNew();
			prvNote.ST_NoteType = nameof(StmNoteVisibility.PRV);
			AssertEquals("StmNoteCollectionViewNonPrivateNotesWithContext.Count", 0, view.Count);

			StmNote intNote = Dummy.Notes.AddNew();
			intNote.ST_NoteType = nameof(StmNoteVisibility.INT);
			AssertEquals("StmNoteCollectionViewNonPrivateNotesWithContext.Count", 1, view.Count);

			StmNote pubNote = Dummy.Notes.AddNew();
			pubNote.ST_NoteType = nameof(StmNoteVisibility.PUB);
			AssertEquals("StmNoteCollectionViewNonPrivateNotesWithContext.Count", 2, view.Count);

			StmNote docNote = Dummy.Notes.AddNew();
			docNote.ST_NoteType = nameof(StmNoteVisibility.DOC);
			AssertEquals("StmNoteCollectionViewNonPrivateNotesWithContext.Count", 2, view.Count);

			view.ValidNoteContexts = StmNoteContextUtils.StmNoteContextsAll; // "ALL" notes are always visible
			AssertEquals("StmNoteCollectionViewNonPrivateNotesWithContext.Count", 2, view.Count);

			StmNoteContexts stmNoteContexts = new StmNoteContexts();
			stmNoteContexts.Module = StmNoteContextModule.A;
			stmNoteContexts.Direction = StmNoteContextDirection.A;
			stmNoteContexts.FreightMode = StmNoteContextFreightMode.I;
			view.ValidNoteContexts = stmNoteContexts;
			AssertEquals("StmNoteCollectionViewNonPrivateNotesWithContext.Count", 2, view.Count);

			intNote.ST_NoteContext = nameof(StmNoteContextModule.D) + nameof(StmNoteContextDirection.A) + nameof(StmNoteContextFreightMode.A);
			pubNote.ST_NoteContext = nameof(StmNoteContextModule.S) + nameof(StmNoteContextDirection.E) + nameof(StmNoteContextFreightMode.I);
			AssertEquals("StmNoteCollectionViewNonPrivateNotesWithContext.Count", 0, view.Count);

			string airStmNoteContextString = nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.A) + nameof(StmNoteContextFreightMode.I);
			intNote.ST_NoteContext = airStmNoteContextString;
			AssertEquals("StmNoteCollectionViewNonPrivateNotesWithContext.Count", 1, view.Count);
			AssertEquals(airStmNoteContextString, view[0].ST_NoteContext);

			stmNoteContexts = new StmNoteContexts();
			stmNoteContexts.Module = StmNoteContextModule.W | StmNoteContextModule.A;
			stmNoteContexts.Direction = StmNoteContextDirection.O | StmNoteContextDirection.A;
			stmNoteContexts.FreightMode = StmNoteContextFreightMode.A | StmNoteContextFreightMode.I;

			view.ValidNoteContexts = stmNoteContexts;
			Dummy.Notes.RemoveAndDeleteAll();
			StmNote note1 = Dummy.Notes.AddNew();
			StmNote note2 = Dummy.Notes.AddNew();
			StmNote note3 = Dummy.Notes.AddNew();
			StmNote note4 = Dummy.Notes.AddNew();
			note1.ST_NoteContext = nameof(StmNoteContextModule.W) + nameof(StmNoteContextDirection.A) + nameof(StmNoteContextFreightMode.A);
			note2.ST_NoteContext = nameof(StmNoteContextModule.W) + nameof(StmNoteContextDirection.O) + nameof(StmNoteContextFreightMode.A);
			note3.ST_NoteContext = nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.O) + nameof(StmNoteContextFreightMode.A);
			note4.ST_NoteContext = nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.A) + nameof(StmNoteContextFreightMode.A);
			AssertEquals("StmNoteCollectionViewNonPrivateNotesWithContext.Count", 3, view.Count);
			AssertEquals("Must contain this note", true, view.Contains(note1));
			AssertEquals("Must contain this note", true, view.Contains(note2));
			AssertEquals("Must not contain this note", false, view.Contains(note3));
			AssertEquals("Must contain this note", true, view.Contains(note4));
		}

		public void TestMaster_ShouldBeMasterOfCollectionTofilter_WhenCollectionToFilterIsDependentBusinessObjectCollection()
		{
			var view = new StmNoteCollectionViewNonPrivateNotesWithContext(Dummy.Notes, StmNoteContextUtils.StmNoteContextsAll, ZGuid.Empty);
			AssertNotNull(view.Master);
			var dependentCollection = Dummy.Notes.ElementsInternal as IDependentBusinessObjectCollection;
			AssertNotNull(dependentCollection);
			AssertEquals(view.Master, dependentCollection.Master);
		}

		public void TestMaster_ShouldBeMasterOfCollectionTofilter_WhenCollectionToFilterIsIBusinessObjectCollectionWithMaster()
		{
			var dummyNotes = new DummyNotesWithIBusinessObjectCollectionWithMaster(Dummy);
			var view = new StmNoteCollectionViewNonPrivateNotesWithContext(dummyNotes, StmNoteContextUtils.StmNoteContextsAll, ZGuid.Empty);
			AssertNotNull(view.Master);
			var dependentCollection = dummyNotes.ElementsInternal as IBusinessObjectCollectionWithMaster;
			AssertNotNull(dependentCollection);
			AssertEquals(view.Master, dependentCollection.Master);
		}

		DummyBizOWithRelatedNotes Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyBizOWithRelatedNotes>()); }
		}

		DummyBizOWithRelatedNotes dummy;
	}
}
