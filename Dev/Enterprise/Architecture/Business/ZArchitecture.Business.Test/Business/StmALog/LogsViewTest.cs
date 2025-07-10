using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(LogsView))]
	sealed class LogsViewTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCollection()
		{
			StmALog log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Parent = Dummy.PK;
			}
			AssertEquals("With log manually created", 1, LogsView.Collection.Count);

			Dummy.Logs.RemoveAndDeleteAll();
			AssertEquals("With log removed", 0, LogsView.Collection.Count);
		}

		public void TestCollection_WhenReloadedFromOtherFactory()
		{
			Dummy.Factory.Save();
			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			otherFactory.ImportFromAnotherFactory(Dummy);
			StmALog log = otherFactory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Parent = Dummy.PK;
				log.SL_Table = DummyBizoSchema.Constants.TableName;
			}
			AssertEquals("No logs initially", 0, LogsView.Collection.Count);
			otherFactory.Save();
			((ILogsInternals)Dummy.Logs).ReloadFromDB();
			AssertEquals("When log is saved is other factory", 1, LogsView.Collection.Count);
		}

		public void TestLogsToShow()
		{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			StmALog changeLog = Dummy.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			StmALog operationsLog = Dummy.Logs.AddNew(Events.Arrival);

			LogsView.LogsToShow = LogsToShow.ChangeLogs;
			AssertEquals(1, LogsView.Collection.Count);
			AssertEquals(changeLog, LogsView.Collection[0]);

			LogsView.LogsToShow = LogsToShow.Operations;
			AssertEquals(1, LogsView.Collection.Count);
			AssertEquals(operationsLog, LogsView.Collection[0]);
		}

		public void TestBizObjNameToShowEventsFor()
		{
			Dummy.CreateNoteInDB();
			AssertEquals("Pre-Condition: Should not create Add Log", 0, Dummy.Note.Logs.GetAllLogs().Count);

			_ = Dummy.Logs.AddNew();
			_ = Dummy.Note.Logs.AddNew();

			AssertEquals("Expect 'All Events', 'This DummyBizo' and a note in list", 3, LogsView.BizObjNameToShowEventsFor_List.Count);
			LogsView.BizObjNameToShowEventsFor = "All";
			AssertEquals("0 add log, 2 related log", 2, LogsView.Collection.Count);

			LogsView.BizObjNameToShowEventsFor = LogsView.BizObjNameToShowEventsFor_List[1].Code;
			AssertEquals("1 related log only", 1, LogsView.Collection.Count);

			ErrorReporter.Clear();
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new LogsView(Dummy, LogsToShow.All);
		}

		LogsView LogsView
		{
			get
			{
				if (logsView == null)
				{
					logsView = (LogsView)GetNewBusinessObject();
				}
				return logsView;
			}
		}
		LogsView logsView;

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

		#endregion
	}
}
