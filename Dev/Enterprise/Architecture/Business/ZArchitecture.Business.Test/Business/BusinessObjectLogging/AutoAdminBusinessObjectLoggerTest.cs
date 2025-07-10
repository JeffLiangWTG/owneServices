using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class AutoAdminBusinessObjectLoggerTest : TestCaseWithFactory
	{
		public void TestIsNotDeferred()
		{
			var autoAdminLogger = new AutoAdminBusinessObjectLogger();
			AssertEquals(false, autoAdminLogger.RunAfterOnSavingForAllBizos);
		}

		public void TestDoesntCreateExtraEmptyLogs()
		{
			var dummyAutoLog = Factory.New<DummyAutoLogged>();

			dummyAutoLog.ZL2_Description = "This is description";

			var logTarget = (IAutoAdminLogTarget)dummyAutoLog;
			logTarget.Logs.AutoCreatedLogDefaultSL_Reference = "";

			Factory.Save();

			//add another EDT log that isn't empty
			dummyAutoLog.ZL2_Description = "This is description 2";
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			logTarget.Logs.AddNew(Events.EditedARecord, "Foo Bar Baz");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();
			AssertEquals(1, logTarget.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());
			Assert(logTarget.Logs.AutoCreatedLog == null || logTarget.Logs.AutoCreatedLog.SL_SE_NKEvent == "ADD");

			dummyAutoLog.ZL2_Description = "This is description 3";

			Factory.Save();
			AssertEquals(2, logTarget.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());
			Assert(logTarget.Logs.AutoCreatedLog.SL_SE_NKEvent == "EDT");
		}

		public void TestDoesCreateExtraNonEmptyLogs()
		{
			var dummyAutoLog = Factory.New<DummyAutoLogged>();

			dummyAutoLog.ZL2_Description = "This is description";

			var logTarget = (IAutoAdminLogTarget)dummyAutoLog;
			logTarget.Logs.AutoCreatedLogDefaultSL_Reference = "The reference";

			Factory.Save();

			//add another EDT log that isn't empty
			dummyAutoLog.ZL2_Description = "This is description 2";
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			logTarget.Logs.AddNew(Events.EditedARecord, "Foo Bar Baz");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();
			AssertEquals(2, logTarget.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());
			Assert(logTarget.Logs.AutoCreatedLog.SL_SE_NKEvent == "EDT");

			dummyAutoLog.ZL2_Description = "This is description 3";

			Factory.Save();
			AssertEquals(3, logTarget.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());
			Assert(logTarget.Logs.AutoCreatedLog.SL_SE_NKEvent == "EDT");
		}

		public void TestCanCreateAndRemoveSaveLog()
		{
			var dummyAutoLog = Factory.New<DummyAutoLogged>();

			dummyAutoLog.ZL2_Description = "This is description";

			var logTarget = (IAutoAdminLogTarget)dummyAutoLog;
			logTarget.Logs.AutoCreatedLogDefaultSL_Reference = "The reference";

			var autoAdminLogger = new AutoAdminBusinessObjectLogger();

			var autoLog = autoAdminLogger.CreateSaveLog(dummyAutoLog) as StmALog;

			AssertNotNull(autoLog);
			AssertEquals("ADD", autoLog.Event.SE_Code);
			AssertEquals(false, autoLog.IsDeleted);
			AssertEquals("The reference", autoLog.SL_Reference);

			autoAdminLogger.RemoveLog(dummyAutoLog);
			AssertEquals(true, autoLog.IsDeleted);
		}

		public void TestCanCreateAndRemoveDeleteLog()
		{
			var dummyAutoLog = Factory.New<DummyAutoLogged>();

			dummyAutoLog.ZL2_Description = "This is description";

			var logTarget = (IAutoAdminLogTarget)dummyAutoLog;
			logTarget.Logs.AutoCreatedLogDefaultSL_Reference = "The reference";

			Factory.Save();

			dummyAutoLog.Delete();

			var autoAdminLogger = new AutoAdminBusinessObjectLogger();

			var autoLog = autoAdminLogger.CreateDeleteLog(dummyAutoLog) as StmALog;

			AssertNotNull(autoLog);
			AssertEquals("DEL", autoLog.Event.SE_Code);
			AssertEquals(false, autoLog.IsDeleted);
			AssertEquals(string.Empty, autoLog.SL_Reference);

			autoAdminLogger.RemoveLog(dummyAutoLog);
			AssertEquals(true, autoLog.IsDeleted);
		}

		public void TestAddedLogWIthCustomSuffixFiresWorkflowOnce()
		{
			using (ObjectFactory.Substitute(LogsTest.SetupMockAuditDecider(true, true, true)))
			{
				var dummyAutoLog = Factory.New<IDummyWithWorkflow>();
				dummyAutoLog.SetCustomLogReferenceSuffix("dummy log suffix");
				var trigger = dummyAutoLog.AddNewTrigger();
				((IBaseTrigger)trigger).TriggerEventCode = Events.AddedARecordToTheSystemCode;
				var fireTriggersCallCount = 0;
				ProcessTaskHandler.SetOnFireHookForTest((log, method) =>
				{
					if (method == "FireTriggers" && log.SL_SE_NKEvent == Events.AddedARecordToTheSystemCode)
					{
						fireTriggersCallCount++;
					}
				});
				Factory.Save();
				AssertEquals(1, fireTriggersCallCount);
			}
		}
	}
}
