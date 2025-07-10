using System.Diagnostics;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class EnterpriseBusinessObjectFetchStrategyTest : TestCaseWithFactory
	{
		[DeveloperOnlyTest]
		public void TestFetchForFactorySaveBeforeTransactionCorePerformance()
		{
			var note = Factory.New<StmNote>();
			note.ST_Table = "DummyBizo";
			Factory.Save();
			var stopWatch = new Stopwatch();
			var strategy = new EnterpriseBusinessObjectFetchStrategyForTest(note);
			stopWatch.Start();
			for (var i = 0; i < 2000000; i++)
			{
				strategy.FetchForFactorySaveBeforeTransaction();
			}
			AssertLessThan(stopWatch.ElapsedMilliseconds, 400);
		}

		public void TestFetchForDeleteDoesNotGenerateHintForNotSupportsNotesDelete()
		{
			StmNote note = Factory.New<StmNote>();
			note.ST_Table = "DummyBizo";
			Factory.Save();
			AssertEquals("Precondition", 0, Factory.ActiveFetchHintsForTable(StmNoteSchema.Constants.TableName));
			note.Delete();
			AssertEquals(0, Factory.ActiveFetchHintsForTable(StmNoteSchema.Constants.TableName));
		}

		public void TestFetchForDeleteGeneratesStmNoteHintOnSavedObject()
		{
			EnterpriseBusinessObjectFetchStrategyForTest test = new EnterpriseBusinessObjectFetchStrategyForTest(Factory.New<StmData>());
			Factory.Save();
			test.FetchForDelete();
			AssertEquals(1, Factory.ActiveFetchHintsForTable(StmNoteSchema.Constants.TableName));
		}

		public void TestFetchForDeleteGeneratesNoHintForStmNoteHintOnUnsavedObject()
		{
			EnterpriseBusinessObjectFetchStrategyForTest test = new EnterpriseBusinessObjectFetchStrategyForTest(Factory.New<StmData>());
			test.FetchForDelete();
			AssertEquals(0, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
		}
	}
}
