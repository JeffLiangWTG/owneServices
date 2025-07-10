using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmALogCollectionView))]
	sealed class StmALogCollectionViewTest : BusinessObjectCollectionViewTestCase<StmALogCollectionView>
	{
		public void TestRebuildViewWithLogsToShow()
		{
			AssertEquals("StmALogCollectionView.Count", 0, LogView.Count);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			StmALog adminLog = Dummy.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			LogView.LogsToShow = LogsToShow.Operations;
			AssertEquals("StmALogCollectionView.Count", 0, LogView.Count); // won't show change logs

			LogView.LogsToShow = LogsToShow.ChangeLogs;
			AssertEquals("StmALogCollectionView.Count", 1, LogView.Count);

			adminLog.Cancel();

			AssertEquals("StmALogCollectionView.Count", 0, LogView.Count);

			LogView.IncludeCancelled = true;
			AssertEquals("StmALogCollectionView.Count", 1, LogView.Count);

			using (adminLog.LockForUpdatingKeyFieldsForTesting())
			{
				adminLog.SL_IsEstimate = true;
			}
			AssertEquals("StmALogCollectionView.Count", 0, LogView.Count);

			LogView.IncludeEstimates = true;
			AssertEquals("StmALogCollectionView.Count", 1, LogView.Count);

			StmALog operationsLog = Dummy.Logs.AddNew(Events.DeclarationQueued);
			AssertEquals("StmALogCollectionView.Count", 1, LogView.Count); // won't show milestone logs

			LogView.IncludeCancelled = false;
			LogView.IncludeEstimates = false;
			LogView.LogsToShow = LogsToShow.Operations;
			AssertEquals("StmALogCollectionView.Count", 1, LogView.Count);

			LogView.IncludeCancelled = true;
			LogView.IncludeEstimates = true;
			LogView.LogsToShow = LogsToShow.All;
			AssertEquals("StmALogCollectionView.Count", 2, LogView.Count);
		}

		public void TestBizObjNameToShowEventsFor_List()
		{
			Dummy.CreateNoteInDB();
			AssertEquals("Pre-Condition: Should not create Add Log", 0, Dummy.Note.Logs.GetAllLogs().Count);

			_ = Dummy.Logs.AddNew();
			_ = Dummy.Note.Logs.AddNew();

			AssertEquals("Expect 'All Events', 'This DummyBizo' and a note in list", 3, LogView.BizObjNameToShowEventsFor_List.Count);
			LogView.BizObjNameToShowEventsFor = "All";
			AssertEquals("0 log, 2 related log", 2, LogView.Count);

			LogView.BizObjNameToShowEventsFor = LogView.BizObjNameToShowEventsFor_List[1].Code;
			AssertEquals("1 related log only", 1, LogView.Count);

			ErrorReporter.Clear();
		}

		public void TestBizObjNameToShowEventsFor_List_UsesEnglishNameInRelatedList()
		{
			Dummy.CreateNoteInDB();
			AssertEquals("Precondition - Note.HumanReadableName should be customised (and not == tablename).", "Paradigm Studio 100 V3", Dummy.Note.HumanReadableName);
			AssertEquals("Paradigm Studio 100 V3", LogView.BizObjNameToShowEventsFor_List.GetCodeFromDescription(Dummy.Note.PK.ToStringKey()));
		}

		#region Implementation

		StmALogCollectionView LogView
		{
			get
			{
				if (logView == null)
				{
					logView = new StmALogCollectionView(Dummy);
				}
				return logView;
			}
		}
		StmALogCollectionView logView;

		DummyWithRelatedLogs Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.New<DummyWithRelatedLogs>();
				}
				return dummy;
			}
		}
		DummyWithRelatedLogs dummy;

		protected override StmALogCollectionView GetCollectionToTest()
		{
			return new StmALogCollectionView(Dummy);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(StmALog));
		}

		#endregion
	}
}
