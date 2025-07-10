using System;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(Notes))]
	sealed class NotesTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFindByVisibilityWithLargeAmountNotes()
		{
			for (var i = 0; i < 2101; i++)
			{
				var note = Factory.NewWithValidTestData<StmNote>();
				note.ST_ParentID = Dummy.PK;
				note.ST_Table = AutoDummyBizo.Schema.TableName;
				note.ST_NoteType = nameof(StmNoteVisibility.PUB);
			}
			Factory.Save();

			var notes = new Notes(Dummy);
			try
			{
				BusinessObjectFactory.StartLogging();
				AssertNoExceptionThrown(() => notes.FindByVisibility(StmNoteVisibility.PUB));
				AssertContains("Should use one filter for all notes.", @"LOAD Enterprise.ZArchitecture.Business.StmNote Filter = ST_NoteType = 'PUB' and (ST_ParentID = CONVERT(", BusinessObjectFactory.DebugLog);
			}
			finally
			{
				BusinessObjectFactory.StopLogging();
			}
		}

		public void TestNotesShouldBeRegisteredEditable()
		{
			AssertEquals("Notes should be registered editable.", true, Dummy.IsRegisteredEditableChildObject(Dummy.Notes.ElementsInternal));
		}

		public void TestHasSecurityToDeleteNotes()
		{
			bool oldNotesValue = EnvProxy.Instance.Security.Notes.IsAllowed;
			bool oldNotesDeleteValue = EnvProxy.Instance.Security.NotesDelete.IsAllowed;
			bool oldNotesEditValue = EnvProxy.Instance.Security.NotesEdit.IsAllowed;
			bool oldNotesNewValue = EnvProxy.Instance.Security.NotesNew.IsAllowed;
			bool oldNotesNewCustomNoteValue = EnvProxy.Instance.Security.NotesNewCustomNote.IsAllowed;

			try
			{
				EnvProxy.Instance.Security.Notes.IsAllowed = false;
				EnvProxy.Instance.Security.NotesDelete.IsAllowed = false;
				EnvProxy.Instance.Security.NotesEdit.IsAllowed = false;
				EnvProxy.Instance.Security.NotesNew.IsAllowed = false;
				EnvProxy.Instance.Security.NotesNewCustomNote.IsAllowed = false;

				EnvProxy.Instance.Security.NotesNewCustomNote.IsAllowed = true;
				Assert(!Dummy.Notes.HasAllNoteSecurity);

				EnvProxy.Instance.Security.NotesNew.IsAllowed = true;
				Assert(!Dummy.Notes.HasAllNoteSecurity);

				EnvProxy.Instance.Security.NotesEdit.IsAllowed = true;
				Assert(!Dummy.Notes.HasAllNoteSecurity);

				EnvProxy.Instance.Security.NotesDelete.IsAllowed = true;
				Assert(!Dummy.Notes.HasAllNoteSecurity);

				EnvProxy.Instance.Security.Notes.IsAllowed = true;
				Assert(Dummy.Notes.HasAllNoteSecurity);
			}
			finally
			{
				EnvProxy.Instance.Security.Notes.IsAllowed = oldNotesValue;
				EnvProxy.Instance.Security.NotesDelete.IsAllowed = oldNotesDeleteValue;
				EnvProxy.Instance.Security.NotesEdit.IsAllowed = oldNotesEditValue;
				EnvProxy.Instance.Security.NotesNew.IsAllowed = oldNotesNewValue;
				EnvProxy.Instance.Security.NotesNewCustomNote.IsAllowed = oldNotesNewCustomNoteValue;
			}
		}

		public void TestHasAllNoteSecurity()
		{
			bool oldValue = EnvProxy.Instance.Security.NotesDelete.IsAllowed;

			try
			{
				EnvProxy.Instance.Security.NotesDelete.IsAllowed = true;
				Assert(Dummy.Notes.HasSecurityToDeleteNotes);

				EnvProxy.Instance.Security.NotesDelete.IsAllowed = false;
				Assert(!Dummy.Notes.HasSecurityToDeleteNotes);
			}
			finally
			{
				EnvProxy.Instance.Security.NotesDelete.IsAllowed = oldValue;
			}
		}

		public void TestHasNotesWhenNotesNotLoaded()
		{
			Assert("HasNotes", !Dummy.Notes.HasNotes);

			var docNote = Factory.New<StmNote>();
			docNote.ST_ParentID = Dummy.PK;
			docNote.ST_Table = AutoDummyBizo.Schema.TableName;
			docNote.ST_NoteType = nameof(StmNoteVisibility.DOC); // this note type should *not* be retrieved
			Factory.Save();
			Assert("HasNotes", !Dummy.Notes.HasNotes);

			var note = Factory.New<StmNote>();
			note.ST_ParentID = Dummy.PK;
			note.ST_Table = AutoDummyBizo.Schema.TableName;
			Factory.Save();
			Assert("HasNotes", Dummy.Notes.HasNotes);
		}

		public void TestHasNotesWhenNotesLoaded()
		{
			Assert("HasNotes", !Dummy.Notes.HasNotes);

			Dummy.Notes.Add(Factory.New(typeof(StmNote)));
			Assert("HasNotes", Dummy.Notes.HasNotes);
		}

		public void TestShowRelatedNotes()
		{
			var dummyWithNotes = Factory.New<DummyBizOWithRelatedNotes>();

			dummyWithNotes.Notes.ShowRelatedNotes = false;
			AssertEquals("Note count should be 0.", 0, dummyWithNotes.Notes.VisibleNotes.Count);

			dummyWithNotes.Notes.ShowRelatedNotes = true; // there are no related notes yet
			AssertEquals("Note count should be 0.", 0, dummyWithNotes.Notes.VisibleNotes.Count);

			StmNote relatedPrivateNote = dummyWithNotes.RelatedDummy.Notes.AddNew();
			relatedPrivateNote.ST_NoteType = nameof(StmNoteVisibility.PRV); // this is private and should not be added to visible notes
			AssertEquals("Note count should be 0.", 0, dummyWithNotes.Notes.VisibleNotes.Count);

			StmNote relatedInternalNote = dummyWithNotes.RelatedDummy.Notes.AddNew();
			relatedInternalNote.ST_NoteType = nameof(StmNoteVisibility.INT);
			AssertEquals("Note count should be 1.", 1, dummyWithNotes.Notes.VisibleNotes.Count);

			dummyWithNotes.Notes.ShowRelatedNotes = false;
			AssertEquals("Note count should be 0.", 0, dummyWithNotes.Notes.VisibleNotes.Count);

			Factory.Save();
			dummyWithNotes.Notes.ShowRelatedNotes = true;
			dummyWithNotes.Notes.ShowNotesForAllCompanies = false;
			relatedInternalNote.ST_GC_RelatedCompany = Guid.NewGuid();
			AssertEquals("Related note is hiden when specific to another company", 0, dummyWithNotes.Notes.VisibleNotes.Count);

			relatedInternalNote.ST_GC_RelatedCompany = EnvProxy.Instance.CurrentCompany.PK;
			AssertEquals("Related note is visible when specific to current company", 1, dummyWithNotes.Notes.VisibleNotes.Count);
		}

		public void TestCheckRelatedElementState()
		{
			ErrorReporter.Clear();
			var bizO = Factory.New<DummyBizOWithRelatedNotes>();

			var relatedBizO = bizO.RelatedDummy;
			var relatedNote = relatedBizO.Notes.AddNew();
			relatedNote.ST_NoteType = nameof(StmNoteVisibility.INT);
			relatedNote.ST_Table = "DummyBizo";

			var relatedElements = bizO.GetNotes().RelatedElements;
			AssertEquals(1, relatedElements.Count);

			((IBusinessObjectInternals)relatedNote).Row.Table.Rows.Remove(((IBusinessObjectInternals)relatedNote).Row);

			AssertEquals(DataRowState.Detached, ((IBusinessObjectInternals)relatedNote).Row.RowState);
			AssertEquals(1, relatedElements.Count);

			 _ = bizO.GetNotes().HasVisibleRelatedNotes;

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("RelatedElement has been deleted", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestRelatedNotesDoesntContainNotesBelongingToSelf()
		{
			var bizOHasRelatedNotesBelongingToSelf = Factory.New<DummyBizOWithRelatedNotesWithRelatedBizOReturningNotesBelongingToSelf>();
			var relatedInternalNote = bizOHasRelatedNotesBelongingToSelf.Notes.AddNew();
			relatedInternalNote.ST_NoteType = nameof(StmNoteVisibility.INT);

			var relatedInternalNote2 = Factory.NewWithValidTestData<StmNote>();
			relatedInternalNote2.ST_NoteType = nameof(StmNoteVisibility.INT);
			relatedInternalNote2.ST_ParentID = bizOHasRelatedNotesBelongingToSelf.BadDummyBizO.PK;
			relatedInternalNote2.ST_Table = "DummyBizo";
			Factory.Save();

			AssertEquals(0, bizOHasRelatedNotesBelongingToSelf.GetNotes().RelatedElements.Count);
			Assert(!bizOHasRelatedNotesBelongingToSelf.GetNotes().HasRelatedNotes);
		}

		public void TestShowNotesForAllCompanies()
		{
			var dummyWithNotes = Factory.New<DummyBizOWithRelatedNotes>();

			dummyWithNotes.Notes.ShowNotesForAllCompanies = false;
			AssertEquals("Note count should be 0.", 0, dummyWithNotes.Notes.VisibleNotes.Count);
			dummyWithNotes.Notes.ShowNotesForAllCompanies = true;
			AssertEquals("Note count should be 0.", 0, dummyWithNotes.Notes.VisibleNotes.Count);

			dummyWithNotes.Notes.ShowNotesForAllCompanies = false;
			var note = dummyWithNotes.Notes.AddNew();
			note.ST_NoteType = nameof(StmNoteVisibility.INT);
			note.ST_GC_RelatedCompany = Guid.Empty;
			AssertEquals("Note count should be 1.", 1, dummyWithNotes.Notes.VisibleNotes.Count);
			Factory.Save();

			note.ST_GC_RelatedCompany = Guid.NewGuid();
			AssertEquals("Note count should be 0.", 0, dummyWithNotes.Notes.VisibleNotes.Count);

			note.ST_GC_RelatedCompany = EnvProxy.Instance.CurrentCompany.PK;
			AssertEquals("Note count should be 1.", 1, dummyWithNotes.Notes.VisibleNotes.Count);
		}

		public void TestFindByPKWhenNotesNotLoaded()
		{
			StmNote fredNote = CreateNoteInDB(Dummy, "Fred");
			StmNote barneyNote = CreateNoteInDB(Dummy, "Barney");

			StmNote fredNoteFound = Dummy.Notes.FindByPK(fredNote.PK);
			Assert("Notes found.", fredNoteFound != null);
			AssertEquals("Notes found.", fredNote.PK, fredNoteFound.PK);
			AssertEquals("Notes found.", Dummy.PK, fredNoteFound.Master.NotesParentPK);

			StmNote barneyNoteFound = Dummy.Notes.FindByPK(barneyNote.PK);
			Assert("Notes found.", barneyNoteFound != null);
			AssertEquals("Notes found.", barneyNote.PK, barneyNoteFound.PK);
			AssertEquals("Notes found.", Dummy.PK, barneyNoteFound.Master.NotesParentPK);

			StmNote emptyNote = Dummy.Notes.FindByPK(ZGuid.Empty);
			Assert("Notes found.", emptyNote == null);
		}

		public void TestFindByPKWhenNotesLoaded()
		{
			StmNote fredNote = Dummy.Notes.AddNew();
			StmNote barneyNote = Dummy.Notes.AddNew();

			StmNote fredNoteFound = Dummy.Notes.FindByPK(fredNote.PK);
			Assert("Notes found.", fredNote != null);
			AssertEquals("Notes found.", fredNote.PK, fredNoteFound.PK);
			AssertEquals("Notes found.", Dummy.PK, fredNoteFound.Master.NotesParentPK);

			StmNote barneyNoteFound = Dummy.Notes.FindByPK(barneyNote.PK);
			Assert("Notes found.", barneyNote != null);
			AssertEquals("Notes found.", barneyNote.PK, barneyNoteFound.PK);
			AssertEquals("Notes found.", Dummy.PK, barneyNoteFound.Master.NotesParentPK);

			StmNote emptyNote = Dummy.Notes.FindByPK(ZGuid.Empty);
			Assert("Notes found.", emptyNote == null);
		}

		public void TestFindByDescriptionWhenNotesNotLoaded()
		{
			CreateNoteInDB(Dummy, "Fred");
			CreateNoteInDB(Dummy, "Barney");

			StmNote[] fredNotes = Dummy.Notes.FindByDescription("Fred");
			AssertEquals("Notes found.", 1, fredNotes.Length);
			AssertEquals("Notes found.", "Fred", fredNotes[0].ST_Description);
			AssertEquals("Notes found.", Dummy.PK, fredNotes[0].Master.NotesParentPK);

			StmNote[] barneyNotes = Dummy.Notes.FindByDescription("Barney");
			AssertEquals("Notes found.", 1, barneyNotes.Length);
			AssertEquals("Notes found.", "Barney", barneyNotes[0].ST_Description);
			AssertEquals("Notes found.", Dummy.PK, barneyNotes[0].Master.NotesParentPK);

			StmNote[] emptyNotes = Dummy.Notes.FindByDescription("This note does not exist.");
			AssertEquals("Notes found.", 0, emptyNotes.Length);
		}

		public void TestFindByDescription_RelatedNotes()
		{
			DummyWithDependentsWithRelatedNotes dummy = Factory.New<DummyWithDependentsWithRelatedNotes>();
			DummyEnterpriseBusinessObject related = Factory.New<DummyEnterpriseBusinessObject>();
			dummy.SetBusinessObjectsWithRelatedNotes(new BusinessObject[] { related });

			StmNote parentNote = CreateNoteInDB(dummy, "Fred");
			StmNote childNote = CreateNoteInDB(related, "Fred");

			StmNote[] result = dummy.Notes.FindByDescription("Fred");
			AssertEquals(parentNote.PK, result[0].PK);

			result = dummy.Notes.FindByDescription("Fred", true);
			AssertEquals(parentNote.PK, result[0].PK);

			dummy.Notes.RemoveAndDeleteAll();
			result = dummy.Notes.FindByDescription("Fred");
			AssertEquals(0, result.Length);

			result = dummy.Notes.FindByDescription("Fred", false);
			AssertEquals(0, result.Length);

			result = dummy.Notes.FindByDescription("Fred", true);
			AssertEquals(childNote.PK, result[0].PK);
		}

		class DummyWithDependentsWithRelatedNotes : DummyWithDependentsEnterpriseBusinessObject
		{
			public DummyWithDependentsWithRelatedNotes(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public void SetBusinessObjectsWithRelatedNotes(BusinessObject[] relatedBusinessObjects)
			{
				this.relatedBusinessObjects = relatedBusinessObjects;
			}

			public override BusinessObject[] BusinessObjectsWithRelatedNotes
			{
				get { return relatedBusinessObjects; }
			}
			BusinessObject[] relatedBusinessObjects;
		}

		public void TestFindByDescriptionWhenNotesLoaded()
		{
			StmNote fredNote = Dummy.Notes.AddNew();
			fredNote.ST_Description = "Fred";

			StmNote barneyNote = Dummy.Notes.AddNew();
			barneyNote.ST_Description = "Barney";

			StmNote[] fredNotes = Dummy.Notes.FindByDescription("Fred");
			AssertEquals("Notes found.", 1, fredNotes.Length);
			AssertEquals("Notes found.", "Fred", fredNotes[0].ST_Description);
			AssertEquals("Notes found.", Dummy.PK, fredNotes[0].Master.NotesParentPK);

			StmNote[] barneyNotes = Dummy.Notes.FindByDescription("Barney");
			AssertEquals("Notes found.", 1, barneyNotes.Length);
			AssertEquals("Notes found.", "Barney", barneyNotes[0].ST_Description);
			AssertEquals("Notes found.", Dummy.PK, barneyNotes[0].Master.NotesParentPK);

			StmNote[] emptyNotes = Dummy.Notes.FindByDescription("This note does not exist.");
			AssertEquals("Notes found.", 0, emptyNotes.Length);
		}

		public void TestFindByDescriptionDoesNotLoadNotes()
		{
			Assert("Notes should not be loaded", !Dummy.Notes.IsElementsLoaded);
			CreateNoteInDB(Dummy, "Fred");

			StmNote[] notes = Dummy.Notes.FindByDescription("Fred");
			AssertEquals("Notes found.", 1, notes.Length);
			AssertEquals("Notes found.", "Fred", notes[0].ST_Description);
			AssertEquals("Notes found.", Dummy.PK, notes[0].Master.NotesParentPK);

			Assert("Notes should not be loaded", !Dummy.Notes.IsElementsLoaded);
		}

		public void TestFindByDescription_ComparisonOperator_Loaded()
		{
			StmNote note1 = Dummy.Notes.AddNew();
			note1.ST_Description = "Note1";

			StmNote note2 = Dummy.Notes.AddNew();
			note2.ST_Description = "Note2";

			StmNote[] notes = Dummy.Notes.FindByDescription("Note", false, SQLComparisonOperator.StartsWith);
			AssertEquals("Notes found.", 2, notes.Length);
			Assert("2 descriptions", notes[0].ST_Description != notes[1].ST_Description);

			notes = Dummy.Notes.FindByDescription("Note1", false, SQLComparisonOperator.StartsWith);
			AssertEquals("Notes found.", 1, notes.Length);
			AssertEquals("Notes found.", "Note1", notes[0].ST_Description);
			AssertEquals("Notes found.", Dummy.PK, notes[0].Master.NotesParentPK);

			notes = Dummy.Notes.FindByDescription("Note2", false, SQLComparisonOperator.StartsWith);
			AssertEquals("Notes found.", 1, notes.Length);
			AssertEquals("Notes found.", "Note2", notes[0].ST_Description);
			AssertEquals("Notes found.", Dummy.PK, notes[0].Master.NotesParentPK);

			notes = Dummy.Notes.FindByDescription("Nite", false, SQLComparisonOperator.StartsWith);
			AssertEquals("Notes found.", 0, notes.Length);

			notes = Dummy.Notes.FindByDescription("te2", false, SQLComparisonOperator.EndsWith);
			AssertEquals("Notes found.", 1, notes.Length);
			AssertEquals("Notes found.", "Note2", notes[0].ST_Description);
			AssertEquals("Notes found.", Dummy.PK, notes[0].Master.NotesParentPK);
		}

		public void TestFindByDescription_ComparisonOperator_NotLoaded()
		{
			CreateNoteInDB(Dummy, "Note1");
			CreateNoteInDB(Dummy, "Note2");

			StmNote[] notes = Dummy.Notes.FindByDescription("Note", false, SQLComparisonOperator.StartsWith);
			AssertEquals("Notes found.", 2, notes.Length);
			Assert("2 descriptions", notes[0].ST_Description != notes[1].ST_Description);
			AssertEquals("Notes found.", Dummy.PK, notes[0].Master.NotesParentPK);

			notes = Dummy.Notes.FindByDescription("Note2", false, SQLComparisonOperator.StartsWith);
			AssertEquals("Notes found.", 1, notes.Length);
			AssertEquals("Notes found.", "Note2", notes[0].ST_Description);
			AssertEquals("Notes found.", Dummy.PK, notes[0].Master.NotesParentPK);

			notes = Dummy.Notes.FindByDescription("Nite", false, SQLComparisonOperator.StartsWith);
			AssertEquals("Notes found.", 0, notes.Length);
		}

		public void TestGetAllNotes()
		{
			StmNoteCollection allNotes = (StmNoteCollection)Dummy.Notes.GetAllNotes();
			AssertEquals("AllNotes.Count", 0, allNotes.Count);

			StmNote note = Dummy.Notes.AddNew();
			allNotes = (StmNoteCollection)Dummy.Notes.GetAllNotes();
			AssertEquals("AllNotes.Count", 1, allNotes.Count);
			AssertEquals("AllNotes[0]", note, allNotes[0]);
		}

		public void TestFetchHintsForNotes()
		{
			Assert("Pre-Condition: No Fetch Hints", !Dummy.Factory.GetAllFetchHintedTableNames().Contains(StmNoteSchema.Constants.TableName));

			var dummyWithRelatedNotes = Factory.New<DummyWithDependentsWithRelatedNotes>();

			var related1 = Factory.New<DummyEnterpriseBusinessObject>();
			var related2 = Factory.New<DummyEnterpriseBusinessObject>();
			var related3 = Factory.New<DummyEnterpriseBusinessObject>();

			var relatedObjects = new BusinessObject[] { related1, related2, related3 };
			dummyWithRelatedNotes.SetBusinessObjectsWithRelatedNotes(relatedObjects);

			Factory.Save();

			AssertNotNull("We should have some Notes", dummyWithRelatedNotes.Notes);
			Assert("Table should be fetch hinted", dummyWithRelatedNotes.Factory.GetAllFetchHintedTableNames().Contains(StmNoteSchema.Constants.TableName));
			AssertEquals("Table has 1 fetch hint", 1, dummyWithRelatedNotes.Factory.ActiveFetchHintsForTable(StmNoteSchema.Constants.TableName));

			AssertEquals("Pre-Condition: StmNote table not hit yet", 0, Factory.GetTableHitCount(StmNoteSchema.Constants.TableName));
			Assert("Accessing RelatedNotes to load them", !dummyWithRelatedNotes.Notes.HasRelatedNotes);
			AssertEquals("StmNote table was only hit once thanks to fetch hints", 1, Factory.GetTableHitCount(StmNoteSchema.Constants.TableName));
		}

		public void TestNoteAddedEventFired()
		{
			StmNote note = Dummy.Notes.AddNew();
			AssertEquals("Note Added event not fired as it was not hooked up", false, NoteAddedEventFired);

			Dummy.Notes.NoteAdded += new NoteAddedEventHandler(Notes_NoteAdded);
			StmNote note2 = Dummy.Notes.AddNew();

			AssertEquals("Note Added event fired as it was hooked up", true, NoteAddedEventFired);
			AssertEquals("Note exists in eventargs", note2, NoteAddedEventNote);

			NoteAddedEventFired = false;
			NoteAddedEventNote = null;
		}

		public void TestAddNew_IsCustomDescriptionFalse()
		{
			Assert("HasNotes", !Dummy.Notes.HasNotes);

			Dummy.Notes.AddNew(false, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, "STC: Personal Effects.");
			Assert("Expecting new note to have been created.", Dummy.Notes.HasNotes);

			StmNote newNote = Dummy.Notes.FindByDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description)[0];
			AssertNotNull(newNote);
			AssertEquals("Expecting notes data set", "STC: Personal Effects.", newNote.ST_NoteDataAsText);
			AssertEquals("Expecting notes text set", "STC: Personal Effects.", newNote.ST_NoteText);
			Assert("Expecting notes data set", newNote.ST_NoteData.Length > 0);
		}

		public void TestAddNew_IsCustomDescriptionTrue()
		{
			Assert("HasNotes", !Dummy.Notes.HasNotes);

			Dummy.Notes.AddNew(true, "Test Description", "Test Text");
			Assert("Expecting new note to have been created.", Dummy.Notes.HasNotes);

			StmNote newNote = Dummy.Notes.FindByDescription("Test Description")[0];
			AssertNotNull(newNote);
			AssertEquals("Expecting notes data set", "Test Text", newNote.ST_NoteDataAsText);
			Assert("Expecting notes data set", newNote.ST_NoteData.Length > 0);
			Assert("Expecting notes data set", newNote.ST_NoteText.Length > 0);
		}

		public void TestFindByVisibility()
		{
			StmNote fredNote = CreateNoteInDB(Dummy, "Fred", StmNoteVisibility.PUB);
			StmNote barneyNote = CreateNoteInDB(Dummy, "Barney", StmNoteVisibility.INT);
			StmNote dinoNote = CreateNoteInDB(Dummy, "Dino", StmNoteVisibility.PRV);

			StmNote[] clientVisibleNotes = Dummy.Notes.FindByVisibility(StmNoteVisibility.PUB);
			AssertEquals("Notes.FindByVisibility() should have returned 1 note.", 1, clientVisibleNotes.Length);
			AssertEquals("Note found.", fredNote.PK, clientVisibleNotes[0].PK);

			StmNote[] internalNotes = Dummy.Notes.FindByVisibility(StmNoteVisibility.INT);
			AssertEquals("Notes.FindByVisibility() should have returned 1 note.", 1, internalNotes.Length);
			AssertEquals("Notes found.", barneyNote.PK, internalNotes[0].PK);

			StmNote[] privateNotes = Dummy.Notes.FindByVisibility(StmNoteVisibility.PRV);
			AssertEquals("Notes.FindByVisibility() should have returned 1 note.", 1, privateNotes.Length);
			AssertEquals("Notes found.", dinoNote.PK, privateNotes[0].PK);
		}

		public void TestFindLocatesNotesNotYetInDB()
		{
			StmNote note = Dummy.Notes.AddNew();
			note.ST_NoteType = nameof(StmNoteVisibility.PUB);

			StmNote[] clientVisibleNotes = Dummy.Notes.FindByVisibility(StmNoteVisibility.PUB);
			AssertEquals("Notes.FindByVisibility() should have returned 1 note.", 1, clientVisibleNotes.Length);
			AssertEquals("Note found.", note.PK, clientVisibleNotes[0].PK);
		}

		public void TestFindByVisibilityDoesNotLoadNotes()
		{
			Assert("Notes should not be loaded", !Dummy.Notes.IsElementsLoaded);
			CreateNoteInDB(Dummy, "Fred", StmNoteVisibility.PUB);

			StmNote[] notes = Dummy.Notes.FindByVisibility(StmNoteVisibility.PUB);
			AssertEquals("Notes found.", 1, notes.Length);
			AssertEquals("Notes found.", "Fred", notes[0].ST_Description);

			Assert("Notes should not be loaded", !Dummy.Notes.IsElementsLoaded);
		}

		[TestDateIncremental(0, 0, 0, 1)]
		public void TestSystemCreateTimeUtcShouldBeRecorded()
		{
			StmNote fredNote = CreateNoteInDB(Dummy, "Fred", StmNoteVisibility.PUB);
			StmNote barneyNote = CreateNoteInDB(Dummy, "Barney", StmNoteVisibility.PUB);
			StmNote dinoNote = CreateNoteInDB(Dummy, "Dino", StmNoteVisibility.PUB);

			AssertEquals("ST_SystemCreateTimeUtc should be recorded.", true, dinoNote.ST_SystemCreateTimeUtc > barneyNote.ST_SystemCreateTimeUtc);
			AssertEquals("ST_SystemCreateTimeUtc should be recorded.", true, barneyNote.ST_SystemCreateTimeUtc > fredNote.ST_SystemCreateTimeUtc);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestFindByVisibilityDocThrowsArgumentException()
		{
			Dummy.Notes.FindByVisibility(StmNoteVisibility.DOC);
		}

		[TestDateIncremental(0, 0, 0, 1)]
		public void TestSortBySystemCreateTimeDescending()
		{
			var fredNote = CreateNoteInDB(Dummy, "Fred", StmNoteVisibility.PUB);
			var barneyNote = CreateNoteInDB(Dummy, "Barney", StmNoteVisibility.PUB);
			var dinoNote = CreateNoteInDB(Dummy, "Dino", StmNoteVisibility.PUB);
			var unsortedNotes = new[] { barneyNote, dinoNote, fredNote };

			var sortMethod = Dummy.Notes.GetType().GetMethod("SortBySystemCreateTimeDescending", BindingFlags.NonPublic | BindingFlags.Instance);
			sortMethod.Invoke(Dummy.Notes, new object[] { unsortedNotes });

			AssertEquals("Notes should be returned ordered by the SystemCreateTime.", dinoNote.ST_Description, unsortedNotes[0].ST_Description);
			AssertEquals("Notes should be returned ordered by the SystemCreateTime.", barneyNote.ST_Description, unsortedNotes[1].ST_Description);
			AssertEquals("Notes should be returned ordered by the SystemCreateTime.", fredNote.ST_Description, unsortedNotes[2].ST_Description);
		}

		public void TestNoAuditLogsGeneratedForNotes()
		{
			var fredNote = CreateNoteInDB(Dummy, "Fred", StmNoteVisibility.PUB);
			var barneyNote = CreateNoteInDB(Dummy, "Barney", StmNoteVisibility.PUB);
			_ = new[] { fredNote, barneyNote };

			var logsFilter = new ZQuery();
			logsFilter.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Parent, SQLComparisonOperator.Equal, fredNote.PK);
			logsFilter.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Parent, SQLComparisonOperator.Equal, barneyNote.PK);

			var otherFactory = new BusinessObjectFactory();
			var logs = (StmALog[])otherFactory.Load(typeof(StmALog), logsFilter);
			AssertEquals("should have loaded 0 logs.", 0, logs.Length);

			fredNote.ST_Description = "changed";
			fredNote.Factory.Save();
			barneyNote.ST_Description = "changed";
			barneyNote.Factory.Save();

			logs = (StmALog[])otherFactory.Load(typeof(StmALog), logsFilter);
			AssertEquals("should have loaded 0 logs.", 0, logs.Length);

			fredNote.Delete();
			barneyNote.Delete();
			logs = (StmALog[])otherFactory.Load(typeof(StmALog), logsFilter);
			AssertEquals("should have loaded 0 logs.", 0, logs.Length);
		}

		public void TestClientVisibleNotes()
		{
			_ = CreateNoteInDB(Dummy, "Fred", StmNoteVisibility.DOC);
			_ = CreateNoteInDB(Dummy, "Barney", StmNoteVisibility.PRV);
			_ = CreateNoteInDB(Dummy, "Burns", StmNoteVisibility.INT);
			var dinoNote = CreateNoteInDB(Dummy, "Dino", StmNoteVisibility.PUB);

			AssertEquals("Number of notes returned is incorrect", 1, Dummy.Notes.ClientVisibleNotes.Length);
			AssertEquals("Notes should be returned ordered by the Add log date.", dinoNote.ST_Description, Dummy.Notes.ClientVisibleNotes[0].ST_Description);
		}

		public void TestFindByDescriptionMultilingual()
		{
			var bizo1 = Factory.New<DummyBizOWithRelatedNotes>();
			CreateNoteInDB(bizo1, "green is unique", StmNoteVisibility.PUB);
			CreateNoteInDB(bizo1, "white is *not* unique", StmNoteVisibility.PUB);

			var bizo2 = Factory.New<DummyBizOWithRelatedNotes>();
			var note = bizo2.Notes.AddNew();
			note.ST_Description = "green is unique";
			note = bizo2.Notes.AddNew();
			note.ST_Description = "white is *not* unique";

			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockCHS = Res.UseMockData())
			{
				mockCHS.Put("green", new ResourceStringData("green", "绿色是唯一的"));
				mockCHS.Put("white", new ResourceStringData("white", "白色是*不*独特"));

				var notes = bizo1.Notes.FindByDescription("绿色是唯一的");
				AssertEquals(1, notes.Length);
				AssertEquals("绿色是唯一的", notes[0].ST_Description);
				AssertEquals("green is unique", notes[0].ST_DescriptionInDatabase);

				notes = bizo1.Notes.FindByDescription("白色是*不*独特");
				AssertEquals(1, notes.Length);
				AssertEquals("白色是*不*独特", notes[0].ST_Description);
				AssertEquals("white is *not* unique", notes[0].ST_DescriptionInDatabase);

				notes = bizo1.Notes.FindByDescription("green is unique");
				AssertEquals(1, notes.Length);
				AssertEquals("绿色是唯一的", notes[0].ST_Description);
				AssertEquals("green is unique", notes[0].ST_DescriptionInDatabase);

				notes = bizo2.Notes.FindByDescription("绿色是唯一的");
				AssertEquals(1, notes.Length);
				AssertEquals("绿色是唯一的", notes[0].ST_Description);
				AssertEquals("green is unique", notes[0].ST_DescriptionInDatabase);

				notes = bizo2.Notes.FindByDescription("白色是*不*独特");
				AssertEquals(1, notes.Length);
				AssertEquals("白色是*不*独特", notes[0].ST_Description);
				AssertEquals("white is *not* unique", notes[0].ST_DescriptionInDatabase);

				notes = bizo2.Notes.FindByDescription("green is unique");
				AssertEquals(1, notes.Length);
				AssertEquals("绿色是唯一的", notes[0].ST_Description);
				AssertEquals("green is unique", notes[0].ST_DescriptionInDatabase);
			}
		}

		public void TestFindByDescription_AllNotes()
		{
			DummyWithDependentsWithRelatedNotes dummy = Factory.New<DummyWithDependentsWithRelatedNotes>();
			DummyEnterpriseBusinessObject related = Factory.New<DummyEnterpriseBusinessObject>();
			DummyEnterpriseBusinessObject related2 = Factory.New<DummyEnterpriseBusinessObject>();
			dummy.SetBusinessObjectsWithRelatedNotes(new BusinessObject[] { related, related2 });

			StmNote parentNote = CreateNoteInDB(dummy, "Sango");

			StmNote relatedNote = CreateNoteInDB(related, "Sango");
			StmNote relatedNote2 = CreateNoteInDB(related2, "Sango");

			StmNote[] result = dummy.Notes.FindByDescription("Sango");
			AssertEquals("Note", 1, result.Length);

			result = dummy.Notes.FindByDescription("Sango", true, false);
			AssertEquals("All Note : ", 3, result.Length);
		}

		public void TestAllElementsIsUpdatedByDataRefresh()
		{
			// Create parent and related bizo with a Note each
			var bizoWithNotes = Factory.New<DummyWithDependentsWithRelatedNotes>();
			var relatedBizoWithNotes = Factory.New<DummyEnterpriseBusinessObject>();
			bizoWithNotes.SetBusinessObjectsWithRelatedNotes(new BusinessObject[] { relatedBizoWithNotes });
			var parentNote = bizoWithNotes.Notes.AddNew();
			parentNote.ST_Description = "ParentNote1";

			var relatedNote = relatedBizoWithNotes.Notes.AddNew();
			relatedNote.ST_Description = "RelatedNote1";

			Factory.Save();

			AssertEquals("AllElements are loaded", 2, bizoWithNotes.Notes.AllElements.Count);

			// Load notes.AllElements in another factory
			var factory2 = new BusinessObjectFactory();
			var factory2bizoWithNotes = factory2.Load<DummyWithDependentsWithRelatedNotes>(bizoWithNotes.PK);
			var factory2relatedBizoWithNotes = factory2.Load<DummyEnterpriseBusinessObject>(relatedBizoWithNotes.PK);
			factory2bizoWithNotes.SetBusinessObjectsWithRelatedNotes(new BusinessObject[] { factory2relatedBizoWithNotes });
			AssertEquals("factory2 AllElements are loaded", 2, factory2bizoWithNotes.Notes.AllElements.Count);

			// Create and save a note in the main factory
			var parentNote2 = bizoWithNotes.Notes.AddNew();
			parentNote2.ST_Description = "ParentNote2";
			Factory.Save();

			AssertEquals("factory2 AllElements is updated by DataRefresh", 3, factory2bizoWithNotes.Notes.AllElements.Count);
		}

		#region Implementation

		StmNote CreateNoteInDB(BusinessObject parent, string description)
		{
			return CreateNoteInDB(parent, description, StmNoteVisibility.INT);
		}

		StmNote CreateNoteInDB(BusinessObject parent, string description, StmNoteVisibility visibility)
		{
			if (!parent.IsInDatabase)
			{
				parent.Factory.Save();
			}

			var newFactory = new BusinessObjectFactory();

			var note = newFactory.New<StmNote>();
			note.ST_ParentID = parent.PK;
			note.ST_Description = description;
			note.ST_Table = AutoDummyBizo.Schema.TableName;
			note.ST_NoteType = visibility.ToString();

			newFactory.Save();
			return note;
		}

		void Notes_NoteAdded(NoteAddedEventArgs args)
		{
			NoteAddedEventFired = true;
			NoteAddedEventNote = args.NoteAdded;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Dummy.Notes;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Dummy = Factory.New<DummyEnterpriseBusinessObject>();
		}

		DummyEnterpriseBusinessObject Dummy;
		bool NoteAddedEventFired;
		StmNote NoteAddedEventNote;

		public class DummyBizOWithRelatedNotesReturningNotesBelongingToOtherGuys : DummyBizOWithRelatedNotes
		{
			public DummyBizOWithRelatedNotesReturningNotesBelongingToOtherGuys(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override Notes Notes
			{
				get { return NoteParentBizO.Notes; }
			}

			internal DummyBizOWithRelatedNotes NoteParentBizO { get; set; }
		}

		public class DummyBizOWithRelatedNotesWithRelatedBizOReturningNotesBelongingToSelf : DummyBizOWithRelatedNotes
		{
			public DummyBizOWithRelatedNotesWithRelatedBizOReturningNotesBelongingToSelf(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			internal DummyBizOWithRelatedNotesReturningNotesBelongingToOtherGuys BadDummyBizO
			{
				get
				{
					if (badDummyBizO == null)
					{
						badDummyBizO = Factory.New<DummyBizOWithRelatedNotesReturningNotesBelongingToOtherGuys>();
						badDummyBizO.NoteParentBizO = this;
					}
					return badDummyBizO;
				}
			}
			DummyBizOWithRelatedNotesReturningNotesBelongingToOtherGuys badDummyBizO;

			protected override BusinessObject[] GetBusinessObjectsWithRelatedNotes()
			{
				return new BusinessObject[] { BadDummyBizO };
			}
		}

		#endregion
	}
}
