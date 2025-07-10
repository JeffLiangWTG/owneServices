using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmNoteCollection))]
	sealed class StmNoteCollectionTestCase : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmNoteCollection(BizO, Factory);
		}

		public void TestDeleteElementWithNoAccess()
		{
			StmNoteCollection noteList = new StmNoteCollection(BizO, BizO.Factory);
			StmNote testNote = noteList.AddNew();
			Factory.Save();
			AssertEquals("PRE: There should be 1 notes.", 1, noteList.Count);
			Assert("PRE: note should be saved.", noteList[0].IsInDatabase);

			bool oldValue = EnvProxy.Instance.Security.NotesDelete.IsAllowed;
			bool noteSecurityPreventedDelete = false;
			noteList.DeleteNoteNotAllowed += new EventHandler<EventArgs>(delegate
			{ noteSecurityPreventedDelete = true; });

			try
			{
				EnvProxy.Instance.Security.NotesDelete.IsAllowed = false;
				AssertEquals("PRE: Delete dbo.alert should not have been hit", false, noteSecurityPreventedDelete);
				noteList.RemoveAndDeleteAll();
				AssertEquals("Object should not have been removed from the collection", 1, noteList.Count);
				AssertEquals("Object should not have been deleted", false, testNote.IsDeleted);
				AssertEquals("Delete dbo.alert should have been hit", true, noteSecurityPreventedDelete);

				EnvProxy.Instance.Security.NotesDelete.IsAllowed = true;
				noteList.RemoveAndDeleteAll();
				AssertEquals("Object should have been removed from the collection", 0, noteList.Count);
				AssertEquals("Object should have been deleted", true, testNote.IsDeleted);
			}
			finally
			{
				EnvProxy.Instance.Security.NotesDelete.IsAllowed = oldValue;
			}
		}

		public void TestConstructor()
		{
			AssertNotNull(new StmNoteCollection(BizO, Factory));
		}

		public void TestMasterIsSetOnCreateAndLoad()
		{
			DummyEnterpriseBusinessObject dummyBusinessObject = Factory.New<DummyEnterpriseBusinessObject>();
			StmNoteCollection collection = new StmNoteCollection(dummyBusinessObject, Factory);
			StmNote note = collection.AddNew();
			AssertEquals("Master after create", dummyBusinessObject, note.Master);

			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyEnterpriseBusinessObject dummyBusinessObject2 = factory2.Load<DummyEnterpriseBusinessObject>(dummyBusinessObject.PK);
			StmNoteCollection collection2 = new StmNoteCollection(dummyBusinessObject2, factory2);
			collection2.Load();
			AssertEquals("Master after load", dummyBusinessObject2, collection2[0].Master);
		}

		public void TestCollectionLoadsCorrectNoteTypes()
		{
			StmNote notePub = CreateNote(BizO, StmNoteVisibility.PUB);
			StmNote notePrv = CreateNote(BizO, StmNoteVisibility.PRV);
			StmNote noteInt = CreateNote(BizO, StmNoteVisibility.INT);
			StmNote noteDoc = CreateNote(BizO, StmNoteVisibility.DOC);
			StmNote noteAgv = CreateNote(BizO, StmNoteVisibility.AGV);

			Factory.Save();
			BusinessObjectFactory retrievingFactory = new BusinessObjectFactory(); // ensure results are from the db (not cached)
			StmNoteCollection notes = new StmNoteCollection(BizO, retrievingFactory, true);
			notes.Load();

			AssertEquals("There should be 4 notes.", 4, notes.Count);

			foreach (StmNote note in notes)
			{
				Assert(note.ST_NoteType == nameof(StmNoteVisibility.PUB) ||
						note.ST_NoteType == nameof(StmNoteVisibility.PRV) ||
						note.ST_NoteType == nameof(StmNoteVisibility.INT) ||
						note.ST_NoteType == nameof(StmNoteVisibility.AGV));
			}
		}

		public void TestViewDoesNotIncludePrivateNotes()
		{
			var notes = BizO.Notes;
			notes.Add(CreateNote(StmNoteVisibility.PUB));
			notes.Add(CreateNote(StmNoteVisibility.PRV));
			notes.Add(CreateNote(StmNoteVisibility.INT));
			notes.Add(CreateNote(StmNoteVisibility.AGV));

			var nonPrivateNotesView = notes.NonPrivateNotesWithContext(StmNoteContextUtils.StmNoteContextsAll, ZGuid.Empty);

			AssertEquals("There should be 4 notes.", 4, notes.ElementsInternal.Count);
			AssertEquals("There should be 3 notes.", 3, nonPrivateNotesView.Count);
			Assert("Should not have added 'PRV' note.", nonPrivateNotesView[0].ST_NoteType != nameof(StmNoteVisibility.PRV));
			Assert("Should not have added 'PRV' note.", nonPrivateNotesView[1].ST_NoteType != nameof(StmNoteVisibility.PRV));
			Assert("Should not have added 'PRV' note.", nonPrivateNotesView[2].ST_NoteType != nameof(StmNoteVisibility.PRV));
		}

		public void TestViewWithNoteContext()
		{
			var notePub = CreateNote(StmNoteVisibility.PUB);
			var noteInt = CreateNote(StmNoteVisibility.INT);

			var notes = BizO.Notes;
			notes.Add(notePub);
			notes.Add(noteInt);

			var viewWithNullContext = notes.NonPrivateNotesWithContext(StmNoteContextUtils.StmNoteContextsAll, ZGuid.Empty);
			AssertEquals("'AAA' (all) notes are always visible", 2, viewWithNullContext.Count);

			var airContext = new StmNoteContexts();
			airContext.Module = StmNoteContextModule.A;
			airContext.Direction = StmNoteContextDirection.A;
			airContext.FreightMode = StmNoteContextFreightMode.I;

			var viewWithAirContext = notes.NonPrivateNotesWithContext(airContext, ZGuid.Empty);
			AssertEquals("StmNoteCollectionViewNonPrivateNotesWithContext.Count", 2, viewWithAirContext.Count);

			noteInt.ST_NoteContext = nameof(StmNoteContextModule.D) + nameof(StmNoteContextDirection.A) + nameof(StmNoteContextFreightMode.A);
			notePub.ST_NoteContext = nameof(StmNoteContextModule.F) + nameof(StmNoteContextDirection.E) + nameof(StmNoteContextFreightMode.I);

			AssertEquals("StmNoteCollectionViewNonPrivateNotesWithContext.Count", 0, viewWithAirContext.Count);

			noteInt.ST_NoteContext = nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.A) + nameof(StmNoteContextFreightMode.I);

			AssertEquals("StmNoteCollectionViewNonPrivateNotesWithContext.Count", 1, viewWithAirContext.Count);
			AssertEquals(nameof(StmNoteContextFreightMode.I), viewWithAirContext[0].ST_NoteContext.Substring(2, 1));
		}

		public void TestViewWithNoteContext_RebuildingAfterContextChange()
		{
			var note = BizO.Notes.AddNew();
			note.ST_NoteContextDirection = nameof(StmNoteContextDirection.I);

			var importContext = StmNoteContexts.Default;
			importContext.Direction = StmNoteContextDirection.I;
			var nonPrivateNotesView = BizO.Notes.NonPrivateNotesWithContext(importContext, ZGuid.Empty);

			AssertEquals(1, nonPrivateNotesView.Count);

			var exportContext = StmNoteContexts.Default;
			exportContext.Direction = StmNoteContextDirection.E;

			nonPrivateNotesView = BizO.Notes.NonPrivateNotesWithContext(exportContext, ZGuid.Empty);

			AssertEquals("View has been re-builded with new context", 0, nonPrivateNotesView.Count);
		}

		#region Implementation

		DummyBizOWithRelatedNotes BizO
		{
			get { return bizO ?? (bizO = Factory.New<DummyBizOWithRelatedNotes>()); }
		}

		DummyBizOWithRelatedNotes bizO;

		StmNote CreateNote(StmNoteVisibility visibility)
		{
			StmNote note = Factory.New<StmNote>();
			note.ST_NoteType = visibility.ToString();

			return note;
		}

		StmNote CreateNote(BusinessObject parent, StmNoteVisibility visibility)
		{
			StmNote note = Factory.New<StmNote>();
			note.ST_NoteType = visibility.ToString();
			note.ST_ParentID = parent.PK;
			note.ST_Table = parent.TableName;

			return note;
		}

		#endregion
	}
}
