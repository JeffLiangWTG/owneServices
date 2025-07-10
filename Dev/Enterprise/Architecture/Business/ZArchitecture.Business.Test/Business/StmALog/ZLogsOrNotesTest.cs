using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ZLogsOrNotesTest : TestCaseWithFactory
	{
		public void TestCallingElementsInternalWhileCreatingElements()
		{
			Dummy = (DummyWithRelatedLogs)Factory.New(typeof(DummyWithRelatedLogsHitsElementsDuringReadOnly));
			Assert("ElementsInternal should not be created", !Dummy.Logs.IsElementsLoaded);

			Dummy.Logs.AddNew();

			Assert("ElementsInternal should be created before the assertion on the next line", Dummy.Logs.IsElementsLoaded);
			AssertEquals("Log correctly added to the ElementsInternal collection", 1, Dummy.Logs.ElementsInternal.Count);
		}

		class DummyWithRelatedLogsHitsElementsDuringReadOnly : DummyWithRelatedLogs
		{
			public DummyWithRelatedLogsHitsElementsDuringReadOnly(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void UpdateChildReadOnlyWhenRegistering(IBusiness child)
			{
				object x = Logs.ElementsInternal; // this simulates, for example, a LogsForNominatedEvent being lazy-created to calculate the readonly value
				base.UpdateChildReadOnlyWhenRegistering(child);
			}
		}

		public void TestExtendsFromNonPersistentBusinessObjectForBinding()
		{
			AssertEquals(
				"Must extend " + nameof(NonPersistentBusinessObject) + " so that binding to properties on ZLogsOrNotes works properly",
				true, Dummy.Logs.GetType().IsSubclassOf(typeof(NonPersistentBusinessObject)));
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestPassingNullParentThrowsArgumentException()
		{
			new Logs(null);
		}

		public void TestDatabaseCount()
		{
			AssertEquals("Dummy.Logs.DatabaseCount", 0, Dummy.Logs.DatabaseCount);

			Dummy.Logs.AddNew(Events.AvailableTo);
			Factory.Save();
			AssertEquals("Dummy.Logs.DatabaseCount", 1, Dummy.Logs.DatabaseCount);
		}

		public void TestLogsDoesNotIncludeRelatedLogs()
		{
			AssertEquals("Dummy.Logs.ElementsInternal.Count", 0, Dummy.LogsCount);

			Dummy.CreateNoteInDB();
			AssertEquals("Dummy.Logs.ElementsInternal.Count", 0, Dummy.LogsCount);
		}

		public void TestAddNewBeforeAccessingAllLogs()
		{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Dummy.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			AssertEquals("Dummy.Logs.ElementsInternal.Count", 1, Dummy.LogsCount);
			AssertEquals("Dummy.Logs.AllElements.Count", 1, Dummy.Logs.AllElements.Count);
		}

		public void TestAddNewAfterAccessingAllLogs()
		{
			AssertEquals("Dummy.Logs.ElementsInternal.Count", 0, Dummy.LogsCount);
			AssertEquals("Dummy.Logs.AllElements.Count", 0, Dummy.Logs.AllElements.Count);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Dummy.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			AssertEquals("Dummy.Logs.ElementsInternal.Count", 1, Dummy.LogsCount);
			AssertEquals("Dummy.Logs.AllElements.Count", 1, Dummy.Logs.AllElements.Count);
		}

		public void TestAddBeforeAccessingAllLogs()
		{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Dummy.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			AssertEquals("Dummy.Logs.ElementsInternal.Count", 1, Dummy.LogsCount);
			AssertEquals("Dummy.Logs.AllElements.Count", 1, Dummy.Logs.AllElements.Count);
		}

		public void TestAddAfterAccessingAllLogs()
		{
			AssertEquals("Dummy.Logs.ElementsInternal.Count", 0, Dummy.LogsCount);
			AssertEquals("Dummy.Logs.AllElements.Count", 0, Dummy.Logs.AllElements.Count);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Dummy.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			AssertEquals("Dummy.Logs.ElementsInternal.Count", 1, Dummy.LogsCount);
			AssertEquals("Dummy.Logs.AllElements.Count", 1, Dummy.Logs.AllElements.Count);
		}

		public void TestAddLogDoesNotLoadAllLogs()
		{
			Assert("Dummy.Logs.IsElementsLoaded", !Dummy.IsElementsLoaded);
			Assert("Dummy.Logs.IsAllElementsLoaded", !Dummy.IsAllElementsLoaded);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Dummy.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Assert("Dummy.Logs.IsElementsLoaded", !Dummy.IsElementsLoaded);
			Assert("Dummy.Logs.IsAllElementsLoaded", !Dummy.IsAllElementsLoaded);
		}

		public void TestAddNewLogDoesNotLoadAllLogs()
		{
			Assert("Dummy.Logs.IsElementsLoaded", !Dummy.IsElementsLoaded);
			Assert("Dummy.Logs.IsAllElementsLoaded", !Dummy.IsAllElementsLoaded);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Dummy.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Assert("Dummy.Logs.IsElementsLoaded", !Dummy.IsElementsLoaded);
			Assert("Dummy.Logs.IsAllElementsLoaded", !Dummy.IsAllElementsLoaded);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Dummy.Logs.AddNew(Events.EditedARecord, "INRI");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Assert("Dummy.Logs.IsElementsLoaded", !Dummy.IsElementsLoaded);
			Assert("Dummy.Logs.IsAllElementsLoaded", !Dummy.IsAllElementsLoaded);
		}

		public void TestRebuildLoadsParentBusinessObjectElements()
		{
			AssertEquals("Dummy.Logs.ElementsInternal.Count", 0, Dummy.LogsCount);

			Dummy.Logs.AddNew(Events.Delivered);
			AssertEquals("Dummy.Logs.ElementsInternal.Count", 1, Dummy.LogsCount);

			Dummy.CreateNoteInDB(); // this will cause a rebuild as the RelatedBusinessEntitiesWithLogs array has changed
			AssertEquals("Dummy.Logs.ElementsInternal.Count", 1, Dummy.LogsCount);
		}

		public void TestRebuildLoadsRelatedBusinessObjectElements()
		{
			AssertEquals("Dummy.Logs.ElementsInternal.Count", 0, Dummy.LogsCount);
			AssertEquals("Dummy.Logs.AllElements.Count", 0, Dummy.Logs.AllElements.Count);

			Dummy.CreateNoteInDB(); // no "ADD" log now exists for the note (the note is in the RelatedBusinessEntitiesWithLogs array)
			AssertEquals("Dummy.Logs.ElementsInternal.Count", 0, Dummy.LogsCount);
			AssertEquals("Dummy.Logs.AllElements.Count", 0, Dummy.Logs.AllElements.Count);

			Dummy.Note.Logs.AddNew(Events.Booked);
			AssertEquals("Dummy.Logs.ElementsInternal.Count", 0, Dummy.LogsCount);
			AssertEquals("Dummy.Logs.AllElements.Count", 1, Dummy.Logs.AllElements.Count);

			Dummy.CreateStaffInDB(); // an "ADD" log now exists for the staff (the staff is in the RelatedBusinessEntitiesWithLogs array)
			AssertEquals("Dummy.Logs.ElementsInternal.Count", 0, Dummy.LogsCount);
			AssertEquals("Dummy.Logs.AllElements.Count", 3, Dummy.Logs.AllElements.Count);
		}

		public void TestReloadFromDBLoadsParentBusinessObjectElements()
		{
			AssertEquals("Dummy.Notes.VisibleNotes.Count", 0, Dummy.Notes.VisibleNotes.Count);

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var notePostedViaOtherFactory = otherFactory.New<StmNote>();
			notePostedViaOtherFactory.ST_ParentID = Dummy.PK;
			notePostedViaOtherFactory.ST_Table = Dummy.TableName;
			Factory.Save();
			otherFactory.Save();
			AssertEquals("Dummy.Notes.VisibleNotes.Count", 0, Dummy.Notes.VisibleNotes.Count);

			Dummy.Notes.GetType().InvokeMember("ReloadFromDB", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, Dummy.Notes, null);
			AssertEquals("Dummy.Notes.VisibleNotes.Count", 1, Dummy.Notes.VisibleNotes.Count);
		}

		public void TestReloadFromDBLoadsRelatedBusinessObjectElements()
		{
			AssertLogCountBeforeAndAfterReloadFromDB(0, 0);

			Dummy.CreateNoteInDB(); // no "ADD" log now exists for the note (the note is in the RelatedBusinessEntitiesWithLogs array)
			AssertLogCountBeforeAndAfterReloadFromDB(0, 0);

			Dummy.Note.Logs.AddNew(Events.Booked);
			AssertLogCountBeforeAndAfterReloadFromDB(0, 1);

			Dummy.CreateStaffInDB(); // an "ADD" log now exists for the staff (the staff is in the RelatedBusinessEntitiesWithLogs array)
			AssertLogCountBeforeAndAfterReloadFromDB(0, 3);

			((ILogsInternals)Dummy.Logs).ReloadFromDB();
			AssertLogCountBeforeAndAfterReloadFromDB(0, 3);
		}

		public void TestForceReloadRelatedElementsOnNextAccess()
		{
			var dummy = Factory.New<DummyBizOWithRelatedNotes>();
			var relatedNotes = dummy.Notes.FindByDescription("NOTE TYPE", true);
			AssertEquals(0, relatedNotes.Length);

			var note = dummy.RelatedDummy.Notes.AddNew();
			note.ST_Description = "NOTE TYPE";
			note.ST_NoteDataAsText = "NOTE ON RELATED DUMMY BIZ OBJ";
			relatedNotes = dummy.Notes.FindByDescription("NOTE TYPE", true);
			AssertEquals(0, relatedNotes.Length);

			dummy.Notes.ForceReloadRelatedElementsOnNextAccess = true;
			relatedNotes = dummy.Notes.FindByDescription("NOTE TYPE", true);
			AssertEquals(1, relatedNotes.Length);
			AssertEquals("NOTE ON RELATED DUMMY BIZ OBJ", relatedNotes[0].ST_NoteDataAsText);
		}

		[ExpectNoExceptions]
		public void TestReloadFromDBWhenNoLogsLoaded()
		{
			Dummy.Logs.GetAllLogs();
			((ILogsInternals)Dummy.Logs).ReloadFromDB();
		}

		void AssertLogCountBeforeAndAfterReloadFromDB(int expectedNonRelatedElements, int expectedRelatedElements)
		{
			AssertEquals("Dummy.Logs.ElementsInternal.Count", expectedNonRelatedElements, Dummy.LogsCount);
			AssertEquals("Dummy.Logs.AllElements.Count", expectedRelatedElements, Dummy.Logs.AllElements.Count);

			((ILogsInternals)Dummy.Logs).ReloadFromDB();
			AssertEquals("Dummy.Logs.ElementsInternal.Count", expectedNonRelatedElements, Dummy.LogsCount);
			AssertEquals("Dummy.Logs.AllElements.Count", expectedRelatedElements, Dummy.Logs.AllElements.Count);
		}

		public void TestAllElementsIsUpdatedWhenRelatedElementCollectionCountChanges()
		{
			Dummy.CreateNoteInDB();
			AssertEquals("Dummy.Logs.AllElements.Count", 0, Dummy.Logs.AllElements.Count);

			StmALog log = Dummy.Note.Logs.AddNew(Events.Booked);
			AssertEquals("Dummy.Logs.AllElements.Count", 1, Dummy.Logs.AllElements.Count);

			Dummy.Note.Logs.ElementsInternal.Remove(log);
			AssertEquals("Dummy.Logs.AllElements.Count", 0, Dummy.Logs.AllElements.Count);
		}

		public void TestElementsInternalFilter()
		{
			ZQuery elementsInternalFilter = (ZQuery)Dummy.Logs.GetType().GetProperty("ElementsInternalFilter", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Dummy.Logs, null);
			AssertEquals(((IBusinessObjectCollection)Dummy.Logs.ElementsInternal).CompleteFilter.LiteralTextADO, elementsInternalFilter.LiteralTextADO);
		}

		public void TestRelatedBusinessObjectsHaveChanged()
		{
			var note = Factory.New<StmNote>();
			var log = Factory.New<StmALog>();

			BusinessObjectCollection temp;
			var dummyWhoseRelatedLogsChange = Factory.New<DummyWithChangingRelatedLogs>();

			// null -> object
			temp = dummyWhoseRelatedLogsChange.Logs.AllElements; // load all logs (includes related)
			dummyWhoseRelatedLogsChange.SetBusinessObjectsWithRelatedEvents(note);
			Assert(RelatedBusinessObjectsHaveChanged(dummyWhoseRelatedLogsChange.Logs));

			temp = dummyWhoseRelatedLogsChange.Logs.AllElements; // resets flag
			Assert(!RelatedBusinessObjectsHaveChanged(dummyWhoseRelatedLogsChange.Logs));

			// object1 -> object2
			temp = dummyWhoseRelatedLogsChange.Logs.AllElements; // load all logs (includes related)
			dummyWhoseRelatedLogsChange.SetBusinessObjectsWithRelatedEvents(log);
			Assert(RelatedBusinessObjectsHaveChanged(dummyWhoseRelatedLogsChange.Logs));

			temp = dummyWhoseRelatedLogsChange.Logs.AllElements; // resets flag
			Assert(!RelatedBusinessObjectsHaveChanged(dummyWhoseRelatedLogsChange.Logs));

			// object -> multiple objects (ie array length changed)
			dummyWhoseRelatedLogsChange.SetBusinessObjectsWithRelatedEvents(log, note);
			Assert(RelatedBusinessObjectsHaveChanged(dummyWhoseRelatedLogsChange.Logs));

			temp = dummyWhoseRelatedLogsChange.Logs.AllElements; // resets flag
			Assert(!RelatedBusinessObjectsHaveChanged(dummyWhoseRelatedLogsChange.Logs));

			// object -> null
			dummyWhoseRelatedLogsChange.SetBusinessObjectsWithRelatedEvents(null, null);
			Assert(RelatedBusinessObjectsHaveChanged(dummyWhoseRelatedLogsChange.Logs));

			temp = dummyWhoseRelatedLogsChange.Logs.AllElements; // resets flag
			Assert(!RelatedBusinessObjectsHaveChanged(dummyWhoseRelatedLogsChange.Logs));
		}

		public void TestMovingNewElementsToElementsInternalDoesNotAlterNewElementsCount()
		{
			Dummy.Notes.AddNew();
			StmNoteCollection newNotes = (StmNoteCollection)NewElements(Dummy.Notes);
			AssertEquals("NewElements.Count should be 1", 1, newNotes.Count);

			// accessing entire collection results in all NewElements being added to ElementsInternal
			StmNoteCollection allNotes = (StmNoteCollection)Dummy.Notes.ElementsInternal;
			AssertEquals("NewElements.Count should be 1", 1, newNotes.Count);
			AssertEquals("ElementsInternal.Count should be 1", 1, allNotes.Count);
		}

		public void TestRemoveAndDeleteAllWithoutLoadingElements()
		{
			Dummy.Notes.AddNew();
			Dummy.Notes.AddNew();
			Dummy.Notes.AddNew();
			Factory.Save();
			AssertEquals("Should have 3 notes in DB", 3, Dummy.Notes.DatabaseCount);

			var otherFactory = new BusinessObjectFactory();
			var dummyFromDB = otherFactory.Load<DummyWithRelatedLogs>(Dummy.PK);
			Assert("Notes should not be loaded", !dummyFromDB.IsElementsLoaded);
			AssertEquals("Should have 3 notes in DB", 3, dummyFromDB.Notes.DatabaseCount);

			dummyFromDB.Notes.RemoveAndDeleteAll();
			otherFactory.Save();
			AssertEquals("Should have 0 notes in DB", 0, dummyFromDB.Notes.DatabaseCount);
		}

		public void TestRemoveAndDeleteAllWithNewElements()
		{
			Assert("Should not have notes", !Dummy.Notes.HasNotes);

			Dummy.Notes.AddNew();
			Assert("Should have notes", Dummy.Notes.HasNotes);

			Dummy.Notes.RemoveAndDeleteAll();
			Assert("All note should be deleted", !Dummy.Notes.HasNotes);
		}

		public void TestRemoveAndDeleteAllWithLoadedElements()
		{
			Assert("Should not have notes", !Dummy.Notes.HasNotes);

			Dummy.Notes.AddNew();
			Factory.Save();
			Assert("Should have notes", Dummy.Notes.HasNotes);
			AssertEquals("Should have 1 note in DB", 1, Dummy.Notes.DatabaseCount);

			Dummy.Notes.RemoveAndDeleteAll();
			Factory.Save();
			Assert("All note should be deleted", !Dummy.Notes.HasNotes);
			AssertEquals("Should have 0 notes in DB", 0, Dummy.Notes.DatabaseCount);
		}

		public void TestRemoveAndDelete()
		{
			MethodInfo removeAndDeleteMethod = Dummy.Notes.GetType().GetMethod("RemoveAndDeleteWithoutLoading", BindingFlags.NonPublic | BindingFlags.Instance);

			StmNote note = Dummy.Notes.AddNew();
			AssertEquals("Precondition - should have 1 note in collection.", 1, Dummy.Notes.GetAllNotes().Count);

			removeAndDeleteMethod.Invoke(Dummy.Notes, new object[] { note });
			AssertEquals("Precondition - should have 0 notes in collection.", 0, Dummy.Notes.GetAllNotes().Count);
		}

		[ExpectNoExceptions]
		public void TestRemoveAndDeleteDoesNotBlowUpIfElementDoesNotExist()
		{
			MethodInfo removeAndDeleteMethod = typeof(ZLogsOrNotes).GetMethod("RemoveAndDeleteWithoutLoading", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			StmNote note1 = Dummy.Notes.AddNew();
			AssertEquals("Precondition - IsElementsLoaded must be false if we are to test NewElements.RemoveAndDelete().", false, Dummy.Notes.IsElementsLoaded);

			removeAndDeleteMethod.Invoke(Dummy.Notes, new object[] { note1 });
			removeAndDeleteMethod.Invoke(Dummy.Notes, new object[] { note1 }); // should not blow up

			StmNote note2 = Dummy.Notes.AddNew();
			object loadAllNotes = Dummy.Notes.GetAllNotes();
			AssertEquals("Precondition - IsElementsLoaded must be true if we are to test ElementsInternal.RemoveAndDelete().", true, Dummy.Notes.IsElementsLoaded);

			removeAndDeleteMethod.Invoke(Dummy.Notes, new object[] { note2 });
			removeAndDeleteMethod.Invoke(Dummy.Notes, new object[] { note2 }); // should not blow up
		}

		public void TestNoDetachedLogsOrNotesAfterFactoryRollback()
		{
			var factory = new BusinessObjectFactory();
			var bizO = factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			factory.Save();

			bizO.Logs.GetAllLogs();
			bizO.Logs.AddNew();

			bizO.Notes.GetAllNotes();
			bizO.Notes.AddNew();

			((IBusinessObjectFactoryInternals)factory).Rollback();

			Assert(!bizO.Logs.HasLogWith(log => ((INeedRow)log).Row.RowState == DataRowState.Detached));
			Assert(!bizO.Notes.GetAllNotes().Any(log => ((INeedRow)log).Row.RowState == DataRowState.Detached));
		}

		public void TestElementsNotInDB()
		{
			PropertyInfo elementsNotInDBProperty = Dummy.Logs.GetType().GetProperty("ElementsNotInDB", BindingFlags.NonPublic | BindingFlags.Instance);
			ArrayList elementsNotInDB = (ArrayList)elementsNotInDBProperty.GetValue(Dummy.Logs, null);
			AssertEquals("Precondition - ElementsNotInDB should have 0 logs.", 0, elementsNotInDB.Count);

			StmALog log1 = Dummy.Logs.AddNew();
			elementsNotInDB = (ArrayList)elementsNotInDBProperty.GetValue(Dummy.Logs, null);
			AssertEquals("ElementsNotInDB should have 1 log.", 1, elementsNotInDB.Count);
			AssertEquals("ElementsNotInDB should contain the new log.", log1, elementsNotInDB[0]);

			Dummy.Factory.Save();
			elementsNotInDB = (ArrayList)elementsNotInDBProperty.GetValue(Dummy.Logs, null);
			AssertEquals("ElementsNotInDB should have 0 logs again.", 0, elementsNotInDB.Count);

			StmALog log2 = Dummy.Logs.AddNew();
			elementsNotInDB = (ArrayList)elementsNotInDBProperty.GetValue(Dummy.Logs, null);
			AssertEquals("ElementsNotInDB should have 1 log.", 1, elementsNotInDB.Count);
			AssertEquals("ElementsNotInDB should contain the new log.", log2, elementsNotInDB[0]);
		}

		public void TestElementsNotInDBDoesNotLoadAllElements()
		{
			Dummy.Logs.AddNew(); // ensure there is a new log so that we attempt to access NewElements
			Assert("Precondition - Dummy.Logs.IsElementsLoaded should be false", !Dummy.IsElementsLoaded);
			Assert("Precondition - Dummy.Logs.IsAllElementsLoaded should be false", !Dummy.IsAllElementsLoaded);

			PropertyInfo elementsNotInDBProperty = Dummy.Logs.GetType().GetProperty("ElementsNotInDB", BindingFlags.NonPublic | BindingFlags.Instance);
			ArrayList accessElementsNotInDB = (ArrayList)elementsNotInDBProperty.GetValue(Dummy.Logs, null);

			Assert("Dummy.Logs.IsElementsLoaded should still be false", !Dummy.IsElementsLoaded);
			Assert("Dummy.Logs.IsAllElementsLoaded should still be false", !Dummy.IsAllElementsLoaded);
		}

		#region Test Classes

		class DummyWithChangingRelatedLogs : DummyEnterpriseBusinessObject
		{
			public DummyWithChangingRelatedLogs(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				fBusinessObjectsWithRelatedEvents = Array.Empty<EnterpriseBusinessObject>();
			}

			protected override BusinessObject[] BusinessObjectsWithRelatedEvents
			{
				get { return fBusinessObjectsWithRelatedEvents; }
			}

			BusinessObject[] fBusinessObjectsWithRelatedEvents;

			public void SetBusinessObjectsWithRelatedEvents(params EnterpriseBusinessObject[] relatedBusinessObject)
			{
				fBusinessObjectsWithRelatedEvents = relatedBusinessObject;
			}
		}

		#endregion

		#region Implementation

		bool RelatedBusinessObjectsHaveChanged(ZLogsOrNotes zLogsOrNotes)
		{
			return (bool)zLogsOrNotes.GetType().GetProperty("RelatedBusinessObjectsHaveChanged", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(zLogsOrNotes, null);
		}

		IDependentBusinessObjectCollection NewElements(ZLogsOrNotes zLogsOrNotes)
		{
			return (IDependentBusinessObjectCollection)zLogsOrNotes.GetType().GetProperty("NewElements", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(zLogsOrNotes, null);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Dummy = Factory.New<DummyWithRelatedLogs>();
		}

		DummyWithRelatedLogs Dummy;

		#endregion
	}
}
