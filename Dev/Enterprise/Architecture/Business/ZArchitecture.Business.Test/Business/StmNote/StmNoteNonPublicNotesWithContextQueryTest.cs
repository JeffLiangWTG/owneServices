using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class StmNoteNonPublicNotesWithContextQueryTest : TestCaseWithDummy
	{
		public void TestQueryWithContextAll()
		{
			StmNoteNonPublicNotesWithContextQuery query = new StmNoteNonPublicNotesWithContextQuery(Dummy, StmNoteContextUtils.StmNoteContextsAll);
			//Query.FetchOnlyFromLocalCache = !Dummy.IsInDatabase;	//was it needed?
			StmNoteNonDependentCollection notes = new StmNoteNonDependentCollection(Factory, query);

			StmNote prvNote = Factory.New<StmNote>();
			prvNote.ST_Table = Dummy.TableName;
			StmNote intNote = Factory.New<StmNote>();
			intNote.ST_Table = Dummy.TableName;
			StmNote pubNote = Factory.New<StmNote>();
			pubNote.ST_Table = Dummy.TableName;
			StmNote agvNote = Factory.New<StmNote>();
			agvNote.ST_Table = Dummy.TableName;
			Factory.Save();

			notes.Load();
			AssertEquals("StmNoteNonDependentCollection.Count", 0, notes.Count);

			prvNote.ST_ParentID = Dummy.PK;
			intNote.ST_ParentID = Dummy.PK;
			pubNote.ST_ParentID = Dummy.PK;
			agvNote.ST_ParentID = Dummy.PK;
			prvNote.ST_NoteType = nameof(StmNoteVisibility.PRV);
			intNote.ST_NoteType = nameof(StmNoteVisibility.INT);
			pubNote.ST_NoteType = nameof(StmNoteVisibility.PUB);
			agvNote.ST_NoteType = nameof(StmNoteVisibility.AGV);
			Factory.Save();

			notes.Load();
			AssertEquals("StmNoteNonDependentCollection.Count", 3, notes.Count);
			Assert("Note Type should not be PRV", notes[0].ST_NoteType != nameof(StmNoteVisibility.PRV));
			Assert("Note Type should not be PRV", notes[1].ST_NoteType != nameof(StmNoteVisibility.PRV));
			Assert("Note Type should not be PRV", notes[2].ST_NoteType != nameof(StmNoteVisibility.PRV));

			intNote.ST_NoteContext = nameof(StmNoteContextModule.F) + nameof(StmNoteContextDirection.E) + nameof(StmNoteContextFreightMode.I);
			agvNote.ST_NoteContext = nameof(StmNoteContextModule.F) + nameof(StmNoteContextDirection.E) + nameof(StmNoteContextFreightMode.I);
			Factory.Save();
			notes.Load();
			AssertEquals("StmNoteNonDependentCollection.Count", 1, notes.Count);
			Assert("Note Context should be ALL", notes[0].ST_NoteContext == StmNoteContextUtils.StmNoteContextsAllToString);
		}

		public void TestQueryAlwaysIncludesAllContext()
		{
			StmNoteContexts stmNoteContext = new StmNoteContexts();
			stmNoteContext.Module = StmNoteContextModule.S;
			stmNoteContext.Direction = StmNoteContextDirection.E;
			stmNoteContext.FreightMode = StmNoteContextFreightMode.I;
			StmNoteNonPublicNotesWithContextQuery query = new StmNoteNonPublicNotesWithContextQuery(Dummy, stmNoteContext);
			//Query.FetchOnlyFromLocalCache = !Dummy.IsInDatabase;	//was it needed?
			StmNoteNonDependentCollection notes = new StmNoteNonDependentCollection(Factory, query);

			notes.Load();
			AssertEquals("StmNoteNonDependentCollection.Count", 0, notes.Count);

			StmNote pubNote = Factory.New<StmNote>();
			pubNote.ST_ParentID = Dummy.PK;
			pubNote.ST_Table = Dummy.TableName;
			pubNote.ST_NoteContext = StmNoteContextUtils.StmNoteContextsAllToString;
			Factory.Save();

			notes.Load();
			AssertEquals("StmNoteNonDependentCollection.Count", 1, notes.Count);
			Assert("Note Context should be AAA", notes[0].ST_NoteContext == StmNoteContextUtils.StmNoteContextsAllToString);
		}

		public void TestQueryWithMixedContext()
		{
			StmNoteContexts stmNoteContexts = new StmNoteContexts();
			stmNoteContexts.Module |= StmNoteContextModule.D;
			stmNoteContexts.Module |= StmNoteContextModule.F;
			stmNoteContexts.Direction |= StmNoteContextDirection.I;
			stmNoteContexts.Direction |= StmNoteContextDirection.E;
			stmNoteContexts.FreightMode |= StmNoteContextFreightMode.I;
			StmNoteNonPublicNotesWithContextQuery query = new StmNoteNonPublicNotesWithContextQuery(Dummy, stmNoteContexts);
			//Query.FetchOnlyFromLocalCache = !Dummy.IsInDatabase;	//was it needed?
			StmNoteNonDependentCollection notes = new StmNoteNonDependentCollection(Factory, query);

			StmNote noteDEC = Factory.New<StmNote>();
			StmNote noteEAS = Factory.New<StmNote>();
			StmNote noteIAS = Factory.New<StmNote>();
			StmNote noteSEA = Factory.New<StmNote>();

			noteDEC.ST_ParentID = Dummy.PK;
			noteDEC.ST_Table = Dummy.TableName;
			noteEAS.ST_ParentID = Dummy.PK;
			noteEAS.ST_Table = Dummy.TableName;
			noteIAS.ST_ParentID = Dummy.PK;
			noteIAS.ST_Table = Dummy.TableName;
			noteSEA.ST_ParentID = Dummy.PK;
			noteSEA.ST_Table = Dummy.TableName;

			noteDEC.ST_NoteContext = nameof(StmNoteContextModule.D) + nameof(StmNoteContextDirection.A) + nameof(StmNoteContextFreightMode.A);
			noteEAS.ST_NoteContext = nameof(StmNoteContextModule.F) + nameof(StmNoteContextDirection.E) + nameof(StmNoteContextFreightMode.I);
			noteIAS.ST_NoteContext = nameof(StmNoteContextModule.F) + nameof(StmNoteContextDirection.I) + nameof(StmNoteContextFreightMode.I);
			noteSEA.ST_NoteContext = nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.A) + nameof(StmNoteContextFreightMode.S);

			Factory.Save();

			notes.Load();
			AssertEquals("StmNoteNonDependentCollection.Count", 3, notes.Count);
			Assert("Note Context should not be SEA", notes[0].ST_NoteContext.Substring(2, 1) != nameof(StmNoteContextFreightMode.S));
			Assert("Note Context should not be SEA", notes[1].ST_NoteContext.Substring(2, 1) != nameof(StmNoteContextFreightMode.S));
			Assert("Note Context should not be SEA", notes[2].ST_NoteContext.Substring(2, 1) != nameof(StmNoteContextFreightMode.S));
		}
	}
}
