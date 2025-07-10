using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(Logs))]
	sealed class LogsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLoadLastEditInfoShouldUseAuditColumns()
		{
			Note.ST_Description = "New Desc";
			Factory.Save();

			var reloadedNote = new BusinessObjectFactory().Load<StmNote>(Note.PK);
			AssertNotNull(reloadedNote);
			AssertNull(reloadedNote.Logs.AddedLog);
			AssertNotNullOrEmpty(reloadedNote.ST_LastModifiedByUserName);
			AssertNotNull(reloadedNote.ST_LastModifiedDate);
		}

		public void TestTemplateRecordShouldNotCreateAddedLogToStmNote()
		{
			var templateRecordProvider = Factory.New<DummyBizoWithITemplateRecordProvider>();
			templateRecordProvider.IsTemplateRecord = true;
			var stmNote = templateRecordProvider.Notes.AddNew();

			AssertNull(stmNote.Logs.AddedLog);
			AssertNull(templateRecordProvider.Logs.AddedLog);
		}

		public void TestLogsShouldNotBeRegisteredEditable()
		{
			AssertEquals("Logs should be registered editable.", true, Dummy.IsRegisteredEditableChildObject(Dummy.Logs.ElementsInternal));
		}

		public void TestLastRecordedLog()
		{
			AssertNull(Dummy.Logs.MostRecentLog);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			StmALog log = Dummy.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			AssertEquals(log.SL_SE_NKEvent, Dummy.Logs.MostRecentLog.SL_SE_NKEvent);

			Factory.Save();
			DummyEnterpriseBusinessObject dummy2 = Factory.Load<DummyEnterpriseBusinessObject>(Dummy.PK);
			AssertEquals(log.SL_SE_NKEvent, dummy2.Logs.MostRecentLog.SL_SE_NKEvent);
		}

		public void TestMostRecentLogShouldNotContainSortByEventTimeUtc()
		{
			AssertNull(Dummy.Logs.MostRecentLog);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			StmALog log = Dummy.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();
			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				AssertEquals(log.SL_SE_NKEvent, Dummy.Logs.MostRecentLog.SL_SE_NKEvent);
				var plan = new QueryPlanalyzer(TestConnection.ExecutedCommandsAndQueryPlans.First().Item2[0]);
				AssertEquals(1, plan.Sorts.Count);
				AssertEquals(1, plan.Sorts[0].SortColumns.Count);
				AssertEquals("SL_EventTime", plan.Sorts[0].SortColumns[0].ColumnName);
			}
		}

		public void TestMostRecentLogByEventTimeShouldNotContainSort()
		{
			AssertNull(Dummy.Logs.MostRecentLog);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Dummy.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();
			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var log = Dummy.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem, x => true);
				var queryPlan = TestConnection.ExecutedCommandsAndQueryPlans.First(tuple => tuple.Item1.StartsWith("SELECT ") && tuple.Item1.Contains("FROM dbo.StmALog")).Item2[0];
				var plan = new QueryPlanalyzer(queryPlan);
				AssertEquals(0, plan.Sorts.Count);
			}
		}

		[TestDate]
		public void TestIsLastLogAnInactiveLog()
		{
			AssertNull(Dummy.Logs.MostRecentLog);

			StmALog futureLog = Dummy.Logs.AddNew(Events.Arrival, ZDateTimeOffset.Now.AddDays(1));
			Factory.Save();
			Assert(!new BusinessObjectFactory { RefreshEnabled = false }.Load<DummyWithRelatedLogs>(Dummy.PK).Logs.IsLastLogAnInactiveLog);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			Dummy.SetIsCancelled(true);
			Dummy.Logs.CreateAutoAdminLog();
			Factory.Save();
			Assert(new BusinessObjectFactory { RefreshEnabled = false }.Load<DummyWithRelatedLogs>(Dummy.PK).Logs.IsLastLogAnInactiveLog);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			Dummy.SetIsCancelled(false);
			Dummy.Logs.CreateAutoAdminLog();
			Factory.Save();
			Assert(!new BusinessObjectFactory { RefreshEnabled = false }.Load<DummyWithRelatedLogs>(Dummy.PK).Logs.IsLastLogAnInactiveLog);
			AssertEquals(futureLog.SL_SE_NKEvent, Dummy.Logs.MostRecentLog.SL_SE_NKEvent);
		}

		public void TestIsLastLogAnInactiveLog_WithMultipleLogsAddedWhenTurnInactive()
		{
			AssertNull(Dummy.Logs.MostRecentLog);
			Factory.Save(); // CreateAddLog
			AssertEquals(false, Dummy.Logs.IsLastLogAnInactiveLog);

			Dummy.SetIsCancelled(true);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Dummy.Logs.AddNew(Events.EditedARecord, string.Format("Deleted Job Record - {0}", Dummy.Z0_Description)); // add additional event while Inactivating
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Dummy.Logs.CreateAutoAdminLog();
			Factory.Save();
			AssertEquals("IsLastLogAnInactiveLog should be true.", true, new BusinessObjectFactory() { RefreshEnabled = false }.Load<DummyWithRelatedLogs>(Dummy.PK).Logs.IsLastLogAnInactiveLog);

			Dummy.SetIsCancelled(false);
			Dummy.Logs.CreateAutoAdminLog();
			Factory.Save();
			AssertEquals("IsLastLogAnInactiveLog should be false.", false, new BusinessObjectFactory() { RefreshEnabled = false }.Load<DummyWithRelatedLogs>(Dummy.PK).Logs.IsLastLogAnInactiveLog);

			System.Threading.Thread.Sleep(10); // short delay to ensure the active after inactive is log on different time
			Dummy.SetIsCancelled(true);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Dummy.Logs.AddNew(Events.EditedARecord, string.Format("Deleted Job Record - {0}", Dummy.Z0_Description));   // add additional event while Inactivating
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Dummy.Logs.CreateAutoAdminLog();
			Factory.Save();
			AssertEquals("IsLastLogAnInactiveLog should be true.", true, new BusinessObjectFactory() { RefreshEnabled = false }.Load<DummyWithRelatedLogs>(Dummy.PK).Logs.IsLastLogAnInactiveLog);
		}

		public void TestDeferFiringWorkflow()
		{
			var event1 = new EventValue(Events.CustomisableEvent01, isEstimate: false, deferFiringWorkflow: false);
			var event2 = new EventValue(Events.CustomisableEvent02, isEstimate: false, deferFiringWorkflow: false);

			Dummy.Logs.AddNew(event1);

			using (Logs.DeferFiringWorkflow(true))
			{
				Dummy.Logs.AddNew(event2);
			}

			AssertEquals("Event1 not deferred", false, Dummy.Logs.Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent01Code).First().SL_FireWorkflow);
			AssertEquals("Event2 deferred", true, Dummy.Logs.Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent02Code).First().SL_FireWorkflow);
		}

		public void TestST_CreatedByUserInitials()
		{
			DummyBizOWithAutoLogs dummy = Factory.New<DummyBizOWithAutoLogs>();
			AssertEquals("CreatedByUserInitials should be set to current user before saving", StaticCurrentFetcher.Instance.CurrentUser.GS_Code, dummy.Logs.CreatedByUserInitials);
			Factory.Save();
			AssertEquals("CreatedByUserInitials should be set to current user after saving", StaticCurrentFetcher.Instance.CurrentUser.GS_Code, dummy.Logs.CreatedByUserInitials);
		}

		public void TestCreatedByUserName()
		{
			DummyBizOWithAutoLogs dummy = Factory.New<DummyBizOWithAutoLogs>();
			AssertEquals("CreatedByUserName should be set to current user before saving", StaticCurrentFetcher.Instance.CurrentUser.GS_FullName, dummy.Logs.CreatedByUserName);
			Factory.Save();
			AssertEquals("CreatedByUserName should be set to current user after saving", StaticCurrentFetcher.Instance.CurrentUser.GS_FullName, dummy.Logs.CreatedByUserName);
		}

		public void TestST_CreatedDateUtc()
		{
			ZDateTime dbStartDate = EnvProxy.Instance.Time.CurrentUtcDateTime;
			System.Threading.Thread.Sleep(50);

			AssertEquals("New record should have no CreatedDate", ZDateTime.Empty, Note.ST_CreatedDateUtc);
			Factory.Save();

			System.Threading.Thread.Sleep(50);
			ZDateTime dbEndDate = EnvProxy.Instance.Time.CurrentUtcDateTime;

			AssertAlmostNow("CreatedDate should be set after saving, and be set to the time of posting", Note.ST_CreatedDateUtc, dbStartDate, dbEndDate);
		}

		public void TestST_LastModifiedByUserName()
		{
			AssertEquals("Pre-condition: New record should have no LastModifiedByUserName", "", Note.Logs.LastModifiedByUserName);
			AssertEquals("Pre-condition: New record should have no LastModifiedByUserFullName", "", Note.Logs.LastModifiedByUserFullName);
			Factory.Save();

			AssertEquals("Pre-condition: Added record should have no LastModifiedByUserName", "", Note.Logs.LastModifiedByUserName);
			AssertEquals("Pre-condition: Added record should have no LastModifiedByUserFullName", "", Note.Logs.LastModifiedByUserFullName);

			Note.ST_Description = "New Desc";
			Factory.Save();

			AssertEquals("LastModifiedByUserName should not be set to current user after editing and saving", "", Note.Logs.LastModifiedByUserName);
			AssertEquals("LastModifiedByUserFullName should not be set to current user after editing and saving", "", Note.Logs.LastModifiedByUserFullName);
		}

		public void TestST_SystemCreateTimeUtc()
		{
			ZDateTime dbStartDate = EnvProxy.Instance.Time.CurrentUtcDateTime;
			System.Threading.Thread.Sleep(50);

			AssertEquals("New record should have no CreatedDate", ZDateTime.Empty, Note.Logs.CreatedDateUtc);
			AssertEquals("New record should have no ST_SystemCreateTimeUtc", ZDateTime.Empty, Note.ST_SystemCreateTimeUtc);
			Factory.Save();

			System.Threading.Thread.Sleep(50);
			ZDateTime dbEndDate = EnvProxy.Instance.Time.CurrentUtcDateTime;

			AssertEquals("CreatedDate should not be set after saving, and be set to the time of posting", ZDateTime.Empty, Note.Logs.CreatedDateUtc);
			AssertAlmostNow("ST_SystemCreateTimeUtc should be set after saving, and be set to the time of posting", Note.ST_SystemCreateTimeUtc, dbStartDate, dbEndDate);
		}

		public void TestST_SystemLastEditUser()
		{
			AssertEquals("Pre-condition: New record should have no ST_SystemLastEditUser", "", Note.ST_SystemLastEditUser);
			Factory.Save();

			AssertEquals("Pre-condition: Added record should have ST_SystemLastEditUser", StaticCurrentFetcher.Instance.CurrentUser.GS_Code, Note.ST_SystemLastEditUser);

			Note.ST_Description = "New Desc";
			Factory.Save();

			AssertEquals("ST_SystemLastEditUser should be set to current user after editing and saving", StaticCurrentFetcher.Instance.CurrentUser.GS_Code, Note.ST_SystemLastEditUser);
		}

		public void TestST_SystemLastEditTimeUtc()
		{
			ZDateTime dbStartDate = EnvProxy.Instance.Time.CurrentUtcDateTime;
			System.Threading.Thread.Sleep(50);

			Note.ST_Description = "New Desc";
			Factory.Save();

			System.Threading.Thread.Sleep(50);
			ZDateTime dbEndDate = EnvProxy.Instance.Time.CurrentUtcDateTime;

			AssertAlmostNow("ST_SystemLastEditTimeUtc should be set after editing and saving", Note.ST_SystemLastEditTimeUtc, dbStartDate, dbEndDate);

			dbStartDate = EnvProxy.Instance.Time.CurrentUtcDateTime;
			System.Threading.Thread.Sleep(50);

			Note.ST_Description = "New Desc 2";
			Factory.Save();

			System.Threading.Thread.Sleep(50);
			dbEndDate = EnvProxy.Instance.Time.CurrentUtcDateTime;

			AssertAlmostNow("ST_SystemLastEditTimeUtc should be set after editing and saving", Note.ST_SystemLastEditTimeUtc, dbStartDate, dbEndDate);
		}

		public void TestST_LastModifiedDate()
		{
			AssertEquals("New record should have no LastModifiedDate", ZDateTime.Empty, Note.ST_LastModifiedDate);
			Factory.Save();

			AssertNotEquals("Added record should have no LastModifiedDate", ZDateTime.Empty, Note.ST_LastModifiedDate);

			ZDateTime dbStartDate = EnvProxy.Instance.Time.CurrentUtcDateTime;
			System.Threading.Thread.Sleep(50);

			Note.ST_Description = "New Desc";
			Factory.Save();

			System.Threading.Thread.Sleep(50);
			ZDateTime dbEndDate = EnvProxy.Instance.Time.CurrentUtcDateTime;

			AssertAlmostNow("LastModifiedDate should be set after editing and saving", Note.ST_LastModifiedDate, dbStartDate, dbEndDate);

			dbStartDate = EnvProxy.Instance.Time.CurrentUtcDateTime;
			System.Threading.Thread.Sleep(50);

			Note.ST_Description = "New Desc 2";
			Factory.Save();

			System.Threading.Thread.Sleep(50);
			dbEndDate = EnvProxy.Instance.Time.CurrentUtcDateTime;

			AssertAlmostNow("LastModifiedDate should be set after editing and saving", Note.ST_LastModifiedDate, dbStartDate, dbEndDate);
		}

		public void TestST_LastModifiedDateLocal()
		{
			AssertEquals("New record should have no LastModifiedDateLocal", ZDateTime.Empty, Note.ST_LastModifiedDateLocal);
			Factory.Save();

			AssertNotEquals("Added record should have no LastModifiedDateLocal", ZDateTime.Empty, Note.ST_LastModifiedDateLocal);

			ZDateTime dbStartDate = EnvProxy.Instance.Time.CurrentLocalDateTime;
			System.Threading.Thread.Sleep(50);

			Note.ST_Description = "New Desc";
			Factory.Save();

			System.Threading.Thread.Sleep(50);
			ZDateTime dbEndDate = EnvProxy.Instance.Time.CurrentLocalDateTime;

			AssertAlmostNow("LastModifiedDateLocal should be set after editing and saving", Note.ST_LastModifiedDateLocal, dbStartDate, dbEndDate);

			dbStartDate = EnvProxy.Instance.Time.CurrentLocalDateTime;
			System.Threading.Thread.Sleep(50);

			Note.ST_Description = "New Desc 2";
			Factory.Save();

			System.Threading.Thread.Sleep(50);
			dbEndDate = EnvProxy.Instance.Time.CurrentLocalDateTime;

			AssertAlmostNow("LastModifiedDateLocal should be set after editing and saving", Note.ST_LastModifiedDateLocal, dbStartDate, dbEndDate);
		}

		public void TestAddedLog()
		{
			IGlbStaff sampleRecord = Factory.New<IGlbStaff>();

			sampleRecord.GS_Code = "BOO";
			ZDateTime beforeSave = ZDateTime.Now;
			Factory.Save(); // get the log in the DB
			ZDateTime afterSave = ZDateTime.Now;

			StmALog logRecord = ((IStmALogParent)sampleRecord).Logs.AddedLog;

			AssertNotNull("Should have found added log", logRecord);
			AssertEquals("SL_Parent", sampleRecord.PK, logRecord.SL_Parent);
			AssertEquals("SL_Table", sampleRecord.TableName, logRecord.SL_Table);
		}

		public static IAuditStmALogDecider SetupMockAuditDecider(bool isAdd, bool isEdt, bool isDel)
		{
			var mockAuditLogDecider = new Mock<IAuditStmALogDecider>();

			mockAuditLogDecider.Setup(x =>
					x.AuditLogConfigNonPersistedForEventCode(It.IsAny<DummyBaseBusinessObject>(), It.IsAny<string>()))
				.Returns((IBusiness bizo, string arg) =>
				{
					if (arg == Events.AddedARecordToTheSystemCode)
					{
						return !isAdd;
					}
					if (arg == Events.EditedARecordCode)
					{
						return !isEdt;
					}
					if (arg == Events.DeletedARecordInTheSystemCode)
					{
						return !isDel;
					}
					return false;
				});
			mockAuditLogDecider.Setup(x => x.IsAutoAdminBusinessObjectLoggerEnabled(It.IsAny<DummyBaseBusinessObject>(),
					It.IsAny<EnterpriseBusinessObject.AutologState>()))
				.Returns(true);
			return mockAuditLogDecider.Object;
		}

		public void TestAddNewWithLongReference()
		{
			var sampleRecord = Factory.New<IGlbStaff>();
			sampleRecord.GS_Code = "BOO";
			Factory.Save(); // get the log in the DB
			ExceptionReporterTestListener.Instance.Clear();

			// insert a description that is longer than the maxlength of the field
			var description = ZString.Replicate('a', StmALogSchema.SL_Reference.MaxLength + 1);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			AssertNoExceptionThrown(() => ((IStmALogParent)sampleRecord).Logs.AddNew(Events.EditedARecord, description));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			AssertEquals("No exception should be thrown.", 0, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		[TestTimeZoneUNLOCO("TWTPE")]
		public void TestAddNew_ParametersWithoutLocation()
		{
			TestDateAttribute.UseUNLOCO = true;

			var parent = Factory.New<DummyBizOWithAutoLogs>();

			var offset = new ZDateTimeOffset(new DateTime(2014, 4, 6, 1, 30, 0), DateTimeKind.Local);

			var log = parent.Logs.AddNew(Events.Arrival, offset);

			AssertEquals("Time of the event", TimeSpan.FromHours(8), log.SL_EventTimeOffset.Offset);
		}

		[TestTimeZoneUNLOCO("TWTPE")]
		public void TestAddNew_ParametersWithLocation_TimezoneWithDST()
		{
			TestDateAttribute.UseUNLOCO = true;

			var parent = Factory.New<DummyBizOWithAutoLogs>();

			//Local time immediately before DST ending in 2014
			var offset0 = new ZDateTimeOffset(new DateTime(2014, 4, 6, 1, 30, 0), DateTimeKind.Local);

			//UTCNOW immediately before DST ending in 2014 (going from UTC+11h to UTC+10h)
			var offset1 = new ZDateTimeOffset(new DateTime(2014, 4, 6, 2, 30, 0), DateTimeKind.Local);

			var logWithDST = parent.Logs.AddNew(Events.Arrival, offset0, "LOC".AsKeyFor("AUSYD"));
			var logWithoutDST = parent.Logs.AddNew(Events.Arrival, offset1, "LOC".AsKeyFor("AUSYD"));

			AssertEquals("Time of the event", TimeSpan.FromHours(11), logWithDST.SL_EventTimeOffset.Offset);
			AssertEquals("Time of the event", TimeSpan.FromHours(10), logWithoutDST.SL_EventTimeOffset.Offset);
		}

		[TestTimeZoneUNLOCO("TWTPE")]
		public void TestAddNew_ParametersWithLocation_InvalidLOC_DefaultsToLocal()
		{
			TestDateAttribute.UseUNLOCO = true;

			var parent = Factory.New<DummyBizOWithAutoLogs>();

			var offset = new ZDateTimeOffset(new DateTime(2014, 4, 6, 1, 30, 0), DateTimeKind.Local);
			var logWithInvalidUnloco = parent.Logs.AddNew(Events.Arrival, offset, "LOC".AsKeyFor("ZZINV"));
			var logWithEmptyUnloco = parent.Logs.AddNew(Events.Arrival, offset, "LOC".AsKeyFor(""));

			AssertEquals("Time of the event", TimeSpan.FromHours(8), logWithInvalidUnloco.SL_EventTimeOffset.Offset);
			AssertEquals("Time of the event", TimeSpan.FromHours(8), logWithEmptyUnloco.SL_EventTimeOffset.Offset);
		}

		public void TestAddNew_ParametersAreSpecified_AddThemToLog()
		{
			var parent = Factory.New<DummyBizOWithAutoLogs>();

			var log = parent.Logs.AddNew(Events.Arrival, "LOC".AsKeyFor("UAIEV"));
			AssertEquals("Value of custom parameter", "UAIEV", log.Parameters["LOC"]);

			log = parent.Logs.AddNew(Events.Arrival, ZDateTimeOffset.Now, "LOC".AsKeyFor("UAIEV"));
			AssertEquals("Value of custom parameter", "UAIEV", log.Parameters["LOC"]);

			log = parent.Logs.AddNew(Events.Arrival, "MAIDAN", ZDateTimeOffset.Now, "LOC".AsKeyFor("UAIEV"));
			AssertEquals("Value of custom parameter", "UAIEV", log.Parameters["LOC"]);

			log = parent.Logs.AddNew(Events.Arrival, ZDateTimeOffset.Now, true, "LOC".AsKeyFor("UAIEV"));
			AssertEquals("Value of custom parameter", "UAIEV", log.Parameters["LOC"]);

			log = parent.Logs.AddNew(Events.Arrival, "MAIDAN", ZDateTimeOffset.Now, true, "LOC".AsKeyFor("UAIEV"));
			AssertEquals("Value of custom parameter", "UAIEV", log.Parameters["LOC"]);

			log = parent.Logs.AddNew(Events.Arrival, "MAIDAN", "LOC".AsKeyFor("UAIEV"));
			AssertEquals("Value of custom parameter", "UAIEV", log.Parameters["LOC"]);

			log = parent.Logs.AddNew(Events.Arrival, "MAIDAN|CMP=V1", "LOC".AsKeyFor("UAIEV"));
			AssertEquals("Value of custom parameter", "MAIDAN", log.ReferenceFreeText);
			AssertEquals("Value of custom parameter", "UAIEV", log.Parameters["LOC"]);
			AssertEquals("Value of custom parameter", "V1", log.Parameters["CMP"]);
		}

		public void TestAddNew_ParentDefersFiringWorkflow()
		{
			var parent = Factory.New<DummyBizOWithAutoLogs>();
			var log = parent.Logs.AddNew(Events.Departure, "LOC".AsKeyFor("GBLHR"));
			Assert("Fire workflow is not deferred", !log.SL_FireWorkflow);

			var deferredParent = Factory.New<DummyBizOWithAutoLogsAndDeferredWorkflowFiring>();
			log = deferredParent.Logs.AddNew(Events.Departure, "LOC".AsKeyFor("GBLHR"));
			Assert("Fire workflow is deferred", log.SL_FireWorkflow);
		}

		public void TestTwoBusinessObjectsAroundSameRowCreatesOnlyOneLogItem()
		{
			var bizO1 = Factory.New<DummyBizOWithAutoLogs>();
			var instantiationTime = bizO1.InstantiationTime;
			System.Threading.Thread.Sleep(1000);
			var bizO2 = Factory.Load<SubClassedDummyBusinessObject>(bizO1.PK);
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var bizOInSecondFactory = factory2.Load<DummyBizOWithAutoLogs>(bizO2.PK);
			AssertEquals("Log count", 1, bizOInSecondFactory.Logs.AllElements.Count);
			AssertEquals(instantiationTime, bizOInSecondFactory.Logs.AddedLog.SL_EventTime);
		}

		public void TestDatabaseHasLogs()
		{
			ZQuery emptyFilter = new ZQuery();
			Assert("Dummy.Logs.DatabaseHasLogs(Filter)", !Dummy.Logs.DatabaseHasLogs(emptyFilter));

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.Login.Code;
				log.SL_Reference = "INRI";
			}
			Dummy.Logs.Add(log);
			Factory.Save();
			Assert("Dummy.Logs.DatabaseHasLogs(Filter)", Dummy.Logs.DatabaseHasLogs(emptyFilter));

			ZQuery badFilter = new ZQuery(StmALogSchema.SL_Reference, "...");
			Factory.Save();
			Assert("Dummy.Logs.DatabaseHasLogs(Filter)", !Dummy.Logs.DatabaseHasLogs(badFilter));

			ZQuery goodFilter = new ZQuery(StmALogSchema.SL_Reference, "INRI");
			Factory.Save();
			Assert("Dummy.Logs.DatabaseHasLogs(Filter)", Dummy.Logs.DatabaseHasLogs(goodFilter));
		}

		public void TestFind()
		{
			var logs = new StmALogCollection(Dummy.Factory);

			var filter = new ZQuery();
			AssertEquals("Logs.Find()", 0, logs.Find(filter).Length);
			Assert("Logs.Find() should return the same results as BusinessObjectCollection.Find()", logs.Find(filter).Length == Dummy.Logs.Find(filter).Length);

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.Login.Code;
				log.SL_Reference = "INRI";
			}
			Dummy.Logs.Add(log);
			logs.Add(log);
			Factory.Save();
			AssertEquals("Logs.Find()", 1, logs.Find(filter).Length);
			Assert("Logs.Find() should return the same results as BusinessObjectCollection.Find()", logs.Find(filter).Length == Dummy.Logs.Find(filter).Length);
		}

		public void TestCancelAll()
		{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			StmALog log1 = Dummy.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			StmALog log2 = Dummy.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Assert("Logs.CancelAll()", !log1.SL_IsCancelled);
			Assert("Logs.CancelAll()", !log2.SL_IsCancelled);

			Dummy.Logs.CancelAll();
			Assert("Logs.CancelAll()", log1.SL_IsCancelled);
			Assert("Logs.CancelAll()", log2.SL_IsCancelled);
		}

		public void TestMostRecentLogByEventTime()
		{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			var log1 = Dummy.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			var logReallyNewButCancelled = Dummy.Logs.AddNew(Events.EditedARecord, "EDT1");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			using (logReallyNewButCancelled.LockForUpdatingKeyFieldsForTesting())
			{
				logReallyNewButCancelled.SL_EventTime = new ZDateTime(2005, 1, 1);
			}
			logReallyNewButCancelled.Cancel();

			var eventNew = new EventValue(Events.EditedARecord, eventTime: new ZDateTimeOffset(2004, 12, 12));
			var logNew = Dummy.Logs.AddNew(eventNew);

			var eventOld1 = new EventValue(Events.EditedARecord, eventTime: new ZDateTimeOffset(2004, 2, 2), reference: "EDT2");
			var logOld1 = Dummy.Logs.AddNew(eventOld1);

			var eventOld2 = new EventValue(Events.EditedARecord, eventTime: new ZDateTimeOffset(2004, 1, 1), reference: "EDT3");
			var logOld2 = Dummy.Logs.AddNew(eventOld2);

			var decoyDummy = Factory.New<DummyEnterpriseBusinessObject>();
			var decoyEvent = new EventValue(Events.EditedARecord, eventTime: new ZDateTimeOffset(2005, 1, 1), reference: "EDT4");
			var decoyLog = decoyDummy.Logs.AddNew(decoyEvent);

			AssertEquals("MostRecentLogByEventTime without Reference parameter", logNew.SL_EventTime, Dummy.Logs.MostRecentLogByEventTime(Events.EditedARecord).SL_EventTime);
			AssertEquals("MostRecentLogByEventTime with ''", logNew.SL_EventTime, Dummy.Logs.MostRecentLogByEventTime(Events.EditedARecord, "").SL_EventTime);
			AssertEquals("MostRecentLogByEventTime with 'EDT2'", logOld1.SL_EventTime, Dummy.Logs.MostRecentLogByEventTime(Events.EditedARecord, "EDT2").SL_EventTime);
			AssertEquals("MostRecentLogByEventTime with ZQuery(StmALogSchema.SL_Reference, 'EDT3')", logOld2.SL_EventTime, Dummy.Logs.MostRecentLogByEventTime(Events.EditedARecord, new ZQuery(StmALogSchema.SL_Reference, "EDT3")).SL_EventTime);
			AssertEquals("MostRecentLogByEventTime with Func(StmALogSchema.SL_Reference, 'EDT3')", logOld2.SL_EventTime, Dummy.Logs.MostRecentLogByEventTime(Events.EditedARecord, x => x.SL_Reference == "EDT3").SL_EventTime);
		}

		[TestDate(2021, 6, 1)]
		public void TestMostRecentLogByPostedTime()
		{
			var log1 = Dummy.Logs.AddNew(Events.MessageSent, new[] { new KeyValuePair<string, string>(Params.Codes.Department, "ONE") });
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			var log2 = Dummy.Logs.AddNew(Events.MessageSent, new[] { new KeyValuePair<string, string>(Params.Codes.Department, "TWO") });
			Factory.Save();

			Func<StmALog, bool> queryFunc = x => x.Parameters[Params.Codes.Department].Equals("ONE");

			var latestLog = Dummy.Logs.MostRecentLogByPostedTime(Events.MessageSent, queryFunc);
			AssertEquals("MostRecentLogByPostedTime should return based on query function", "ONE", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department]);
		}

		public void TestCreateRecreateOrUpdateEventLog_ForEstimate()
		{
			TestCreateRecreateOrUpdateEventLog(EstimateActual.Estimate);
		}

		public void TestCreateRecreateOrUpdateEventLog_ForActual()
		{
			TestCreateRecreateOrUpdateEventLog(EstimateActual.Actual);
		}

		void TestCreateRecreateOrUpdateEventLog(EstimateActual estimateActual)
		{
			var isEstimate = estimateActual == EstimateActual.Estimate;
			var decoyParent = Factory.New<DummyEnterpriseBusinessObject>();
			var decoyLog = decoyParent.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.Arrival, estimateActual, new ZDateTimeOffset(2000, 1, 1));
			var decoyLog2 = Dummy.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.Arrival, isEstimate ? EstimateActual.Actual : EstimateActual.Estimate, new ZDateTimeOffset(2000, 1, 1));

			var createdLog = Dummy.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.Arrival, estimateActual, new ZDateTimeOffset(2000, 1, 1));
			AssertEquals("SL_SE_NKEvent of new log", Events.Arrival.Code, createdLog.SL_SE_NKEvent);
			AssertEquals("SL_EventTime of new log", new ZDateTime(2000, 1, 1), createdLog.SL_EventTime);
			AssertEquals("SL_IsEstimate of new log", isEstimate, createdLog.SL_IsEstimate);

			var updatedLog = Dummy.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.Arrival, estimateActual, new ZDateTimeOffset(2000, 2, 2));
			AssertEquals("Uncommitted in-memory log should be updated", createdLog.PK, updatedLog.PK);
			AssertEquals("SL_EventTime of updated log", new ZDateTime(2000, 2, 2), updatedLog.SL_EventTime);

			Factory.Save();
			var secondCreatedLog = Dummy.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.Arrival, estimateActual, new ZDateTimeOffset(2000, 3, 3));
			AssertEquals("The saved log should be cancelled", ZBool.True, createdLog.SL_IsCancelled);
			AssertNotEquals("A new log should be created", createdLog.PK, secondCreatedLog.PK);
		}

		public void TestCreateRecreateOrUpdateEventLog_EmptyEventTimeWhenInMemory()
		{
			StmALog createdLog = Dummy.Logs.AddNew(Events.Arrival);
			Dummy.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.Arrival, EstimateActual.Actual, ZDateTimeOffset.Empty);
			AssertEquals("Unsaved in-memory log should be deleted", true, createdLog.IsDeleted);
		}

		public void TestCreateRecreateOrUpdateEventLog_EmptyEventTimeWhenSavedToDatabase()
		{
			StmALog createdLog = Dummy.Logs.AddNew(Events.Arrival);
			Factory.Save();

			Dummy.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.Arrival, EstimateActual.Actual, ZDateTimeOffset.Empty);
			AssertEquals("Saved log should be cancelled", true, createdLog.SL_IsCancelled);
		}

		public void TestCreateRecreateOrUpdateEventLog_NewLogAddedAfterOldLogHasCancelledInMemory()
		{
			var createdLog = Dummy.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.Arrival, EstimateActual.Actual, new ZDateTimeOffset(2016, 5, 21));
			Factory.Save();

			createdLog.Cancel();

			var newFactory = Factory.CreateNewFactory();
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(StmALog));
			query.AddToFilter(StmALogSchema.SL_Parent, createdLog.SL_Parent);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, createdLog.SL_SE_NKEvent);
			query.AddToFilter(StmALogSchema.SL_IsEstimate, createdLog.SL_IsEstimate);
			query.OrderBy = "SL_PostedTimeUtc desc";// this is a column name

			var createdLogInDB = newFactory.LoadTop1<StmALog>(query);
			AssertEquals("Created log in memory should be cancelled", true, createdLog.SL_IsCancelled);
			AssertEquals("Created log in DB should not be cancelled", false, createdLogInDB.SL_IsCancelled);

			var newLog = Dummy.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.Arrival, EstimateActual.Actual, new ZDateTimeOffset(2016, 5, 21));
			AssertNotNull("A new log should not null", newLog);
			AssertNotEquals("A new log should be created", createdLog.PK, newLog.PK);
		}

		public void TestCreateOrRecreateEventLog()
		{
			StmALog createdLog = Dummy.Logs.AddNew(Events.Arrival);
			Dummy.Logs.CreateOrRecreateEventLog(AutoEvents.Arrival, EstimateActual.Actual, ZDateTimeOffset.Empty);
			AssertEquals("Unsaved in-memory log should be deleted", true, createdLog.IsDeleted);
		}

		public void TestCreateOrRecreateEventLog_WithTableName()
		{
			var dummyWithLogs = Factory.New<DummyWithRelatedLogs>();
			var log = dummyWithLogs.Logs.AddNew(Events.Arrival);
			dummyWithLogs.Factory.Save();

			dummyWithLogs.AlternateLogsParentTableName = "DummyWithCoolTableName";

			dummyWithLogs.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.Arrival, EstimateActual.Actual, ZDateTimeOffset.Now);
			Assert("First log should not be cancelled as second log has different ParentTable", !log.IsCancelled);
		}

		public void TestCreateOrRecreateEventLog_LogReference()
		{
			StmALog createdLog = Dummy.Logs.AddNew(Events.Arrival, "AAA", new ZDateTimeOffset(2012, 2, 1));
			Dummy.Logs.CreateOrRecreateEventLog(AutoEvents.Arrival, EstimateActual.Actual, new ZDateTimeOffset(2012, 3, 2), "BBB");
			AssertEquals("createdLog shouldn't be updated as the reference doesn't match", "AAA", createdLog.SL_Reference);
			AssertEquals("createdLog shouldn't be updated as the reference doesn't match", new ZDateTime(2012, 2, 1), createdLog.SL_EventTime);

			var query = new ZQuery(StmALogSchema.SL_Reference, "BBB");
			query.AddToFilter(StmALogSchema.SL_EventTime, new ZDateTime(2012, 3, 2));

			var logWithReferenceBBB = Dummy.Logs.MostRecentLogByEventTime(AutoEvents.Arrival, query);

			AssertNotNull("A new log is added with reference 'BBB'", Dummy.Logs.MostRecentLogByEventTime(AutoEvents.Arrival, query));
			Dummy.Logs.CreateOrRecreateEventLog(AutoEvents.Arrival, EstimateActual.Actual, new ZDateTimeOffset(2012, 4, 1), "BBB");

			var logAfterProcess = Factory.Load<StmALog>(logWithReferenceBBB.PK);
			AssertEquals(" log with reference 'BBB' should have updated", new ZDateTime(2012, 4, 1), logAfterProcess.SL_EventTime);

			Factory.Save();
			Dummy.Logs.CreateOrRecreateEventLog(AutoEvents.Arrival, EstimateActual.Actual, new ZDateTimeOffset(2013, 4, 1), "BBB");

			logAfterProcess = Factory.Load<StmALog>(logWithReferenceBBB.PK);
			AssertEquals(" log with reference 'BBB' shouldn't be updated", new ZDateTime(2012, 4, 1), logAfterProcess.SL_EventTime);

			query = new ZQuery(StmALogSchema.SL_Reference, "BBB");
			query.AddToFilter(StmALogSchema.SL_EventTime, new ZDateTime(2013, 4, 1));
			query.AddToFilter(StmALogSchema.SL_IsEstimate, false);

			logWithReferenceBBB = Dummy.Logs.MostRecentLogByEventTime(AutoEvents.Arrival, query);

			AssertNotEquals("A new log is added", logAfterProcess.PK, logWithReferenceBBB.PK);
		}

		public void TestCreateRecreateOrUpdateEventLog_LogReference()
		{
			int second = 0;
			StmALog log1 = Dummy.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.Arrival, EstimateActual.Actual, ZDateTimeOffset.Now.AddSeconds(second++));
			Factory.Save();

			StmALog log2 = Dummy.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.Arrival, EstimateActual.Actual, ZDateTimeOffset.Now.AddSeconds(second++));
			AssertEquals("Saved log should be cancelled", true, log1.SL_IsCancelled);
			Factory.Save();

			Event e = AutoEvents.Arrival;
			StmALog log3 = Dummy.Logs.CreateRecreateOrUpdateEventLog(e, EstimateActual.Actual, ZDateTimeOffset.Now.AddSeconds(second++), new ZString("ref 1"));
			AssertEquals("not cancelled because references are not equal", false, log2.SL_IsCancelled);
			Factory.Save();

			StmALog log4 = Dummy.Logs.CreateRecreateOrUpdateEventLog(e, EstimateActual.Actual, ZDateTimeOffset.Now.AddSeconds(second++), new ZString("ref 2"));
			AssertEquals("not cancelled because references are not equal", false, log2.SL_IsCancelled);
			AssertEquals("not cancelled because references are not equal", false, log3.SL_IsCancelled);
			Factory.Save();

			StmALog log5 = Dummy.Logs.CreateRecreateOrUpdateEventLog(e, EstimateActual.Actual, ZDateTimeOffset.Now.AddSeconds(second++), new ZString("ref 1"));
			AssertEquals("not cancelled because references are not equal", false, log2.SL_IsCancelled);
			AssertEquals("cancelled because references are equal", true, log3.SL_IsCancelled);
			AssertEquals("not cancelled because references are not equal", false, log4.SL_IsCancelled);
		}

		public void TestCreateRecreateOrUpdateEventLog_LogReference_NotInDb()
		{
			var time = ZDateTimeOffset.Now;
			StmALog log1 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, time, new ZString("ref 1"));
			StmALog log2 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, time, new ZString("ref 2"));
			StmALog log3 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, time, new ZString("ref 3"));

			AssertNotEquals("Should create separate logs", log1.PK, log2.PK);
			AssertNotEquals("Should create separate logs", log1.PK, log3.PK);
			AssertNotEquals("Should create separate logs", log2.PK, log3.PK);

			AssertEquals("Uses reference to search logs not in db", log1.PK, Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, time, new ZString("ref 1")).PK);
			AssertEquals("Uses reference to search logs not in db", log2.PK, Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, time, new ZString("ref 2")).PK);
			AssertEquals("Uses reference to search logs not in db", log3.PK, Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, time, new ZString("ref 3")).PK);
		}

		[ExpectNoExceptions()]
		public void TestCreateRecreateOrUpdateEventLog_Concurrency()
		{
			Factory.RefreshEnabled = false; //simulating different users / appdomains etc
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			var second = 0;
			var log1 = dummy.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.Arrival, EstimateActual.Actual, ZDateTimeOffset.Now.AddSeconds(second++));
			Factory.Save();

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_Parent, dummy.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.Arrival.Code);
			query.AddToFilter(StmALogSchema.SL_IsEstimate, false);

			var log1ReloadedInAnotherFactory = new BusinessObjectFactory().LoadTop1<StmALog>(query);
			log1ReloadedInAnotherFactory.Factory.Load<DummyEnterpriseBusinessObject>(dummy.PK); // Loading to avoid error on save.
			log1ReloadedInAnotherFactory.Cancel();
			log1ReloadedInAnotherFactory.Factory.Save();

			var log3 = dummy.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.Arrival, EstimateActual.Actual, ZDateTimeOffset.Now.AddSeconds(second++));
			Factory.Save();
		}

		public void TestAddNew_SL_ReferenceByParameterTooLong()
		{
			var tooLongString = new string('A', StmALogSchema.SL_Reference.MaxLength * 2);
			var parameters = new KeyValuePair<string, string>("SL_Reference", tooLongString);
			var log = Dummy.Logs.AddNew(Events.Arrival, parameters);

			AssertEquals(string.Format("SL_Reference did not truncate to {0} characters", StmALogSchema.SL_Reference.MaxLength), StmALogSchema.SL_Reference.MaxLength, log.SL_Reference.Length);
			AssertStartsWith("GenerateEventReferenceToFitInReferenceMaxLength did not format properly", "|SL_Reference=", log.SL_Reference);
		}

		public void TestCreateRecreateOrUpdateEventLog_ReferenceContainsParameters_JoinParametersWithParametersSpecifiedExplicitly()
		{
			var log = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, ZDateTimeOffset.Now, "McLaren|LOC=UAIEV", Params.Codes.Facility.AsKeyFor("Port"));

			AssertEquals("Fire workflow must be false.", false, log.SL_FireWorkflow);
			AssertEquals("Reference free text", "McLaren", log.ReferenceFreeText);
			AssertEquals("Location", "UAIEV", log.Parameters["LOC"]);
			AssertEquals("Facility", "Port", log.Parameters[Params.Codes.Facility]);
		}

		public void TestCreateRecreateOrUpdateEventLog_DeferFiringWorkflow()
		{
			var log = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, ZDateTimeOffset.Now, "GALLE|LOC=SLCMB", true, Params.Codes.Facility.AsKeyFor("Port")).newLog;

			AssertEquals("Fire workflow must be true.", true, log.SL_FireWorkflow);
			AssertEquals("Reference free text", "GALLE", log.ReferenceFreeText);
			AssertEquals("Location", "SLCMB", log.Parameters["LOC"]);
			AssertEquals("Facility", "Port", log.Parameters[Params.Codes.Facility]);
		}

		public void TestCreateRecreateOrUpdateEventLog_LogWithSameReferenceButWithoutParametersExistInMemory_AddParametersToLog()
		{
			var oldLog1 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, ZDateTimeOffset.Now, "McLaren");
			var newLog1 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, ZDateTimeOffset.Now.AddHours(-1), "McLaren", Params.Codes.Location.AsKeyFor("UAIEV"));

			var oldLog2 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Departure, EstimateActual.Actual, ZDateTimeOffset.Now, "McLaren");
			var newLog2 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Departure, EstimateActual.Actual, ZDateTimeOffset.Now.AddHours(-1), "McLaren|LOC=UAIEV");

			AssertEquals("Should match existing log", oldLog1, newLog1);
			AssertEquals("Should match existing log", oldLog2, newLog2);
			AssertEquals("Location on log", "UAIEV", newLog1.Parameters[Params.Codes.Location]);
			AssertEquals("Location on log", "UAIEV", newLog2.Parameters[Params.Codes.Location]);
			AssertEquals("Reference not changed", "McLaren", newLog1.ReferenceFreeText);
			AssertEquals("Reference not changed", "McLaren", newLog2.ReferenceFreeText);
		}

		public void TestCreateRecreateOrUpdateEventLog_LogWithSameReferenceButDifferentParametersExistInMemory_CreateNewLog()
		{
			var oldLog1 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, ZDateTimeOffset.Now, "McLaren", Params.Codes.Location.AsKeyFor("AUSYD"));
			var newLog1 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, ZDateTimeOffset.Now.AddHours(-1), "McLaren", Params.Codes.Location.AsKeyFor("UAIEV"));

			var oldLog2 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Departure, EstimateActual.Actual, ZDateTimeOffset.Now, "McLaren|LOC=AUSYD");
			var newLog2 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Departure, EstimateActual.Actual, ZDateTimeOffset.Now.AddHours(-1), "McLaren|LOC=UAIEV");

			AssertNotEquals("Should match existing log", oldLog1, newLog1);
			AssertNotEquals("Should match existing log", oldLog2, newLog2);
			AssertEquals("Location on log", "UAIEV", newLog1.Parameters[Params.Codes.Location]);
			AssertEquals("Location on log", "UAIEV", newLog2.Parameters[Params.Codes.Location]);
			AssertEquals("Reference not changed", "McLaren", newLog1.ReferenceFreeText);
			AssertEquals("Reference not changed", "McLaren", newLog2.ReferenceFreeText);
		}

		public void TestCreateRecreateOrUpdateEventLog_LogWithDifferentReferenceExistInMemory_CreateNewLog()
		{
			var oldLog1 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, ZDateTimeOffset.Now, "McLaren");
			var newLog1 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, ZDateTimeOffset.Now.AddHours(-1), "ManUtd", Params.Codes.Location.AsKeyFor("UAIEV"));

			var oldLog2 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Departure, EstimateActual.Actual, ZDateTimeOffset.Now, "McLaren");
			var newLog2 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Departure, EstimateActual.Actual, ZDateTimeOffset.Now.AddHours(-1), "ManUtd|LOC=UAIEV");

			AssertNotEquals("Should match existing log", oldLog1, newLog1);
			AssertNotEquals("Should match existing log", oldLog2, newLog2);
			AssertEquals("Location on log", "UAIEV", newLog1.Parameters[Params.Codes.Location]);
			AssertEquals("Location on log", "UAIEV", newLog2.Parameters[Params.Codes.Location]);
			AssertEquals("Reference not changed", "ManUtd", newLog1.ReferenceFreeText);
			AssertEquals("Reference not changed", "ManUtd", newLog2.ReferenceFreeText);
		}

		public void TestCreateRecreateOrUpdateEventLog_TheSameLogButWithDifferentTimeExistInDatabase_RecreateLog()
		{
			var oldLog1 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, ZDateTimeOffset.Now, "McLaren", Params.Codes.Location.AsKeyFor("UAIEV"));
			var oldLog2 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Departure, EstimateActual.Actual, ZDateTimeOffset.Now, "McLaren|LOC=UAIEV");

			Factory.Save();

			var newLog1 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, ZDateTimeOffset.Now.AddHours(-1), "McLaren", Params.Codes.Location.AsKeyFor("UAIEV"));
			var newLog2 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Departure, EstimateActual.Actual, ZDateTimeOffset.Now.AddHours(-1), "McLaren|LOC=UAIEV");

			AssertEquals("Original log is canceled", true, oldLog1.IsCancelled);
			AssertEquals("Original log is canceled", true, oldLog2.IsCancelled);
			AssertEquals("Location on log", "UAIEV", newLog1.Parameters[Params.Codes.Location]);
			AssertEquals("Location on log", "UAIEV", newLog2.Parameters[Params.Codes.Location]);
			AssertEquals("Reference not changed", "McLaren", newLog1.ReferenceFreeText);
			AssertEquals("Reference not changed", "McLaren", newLog2.ReferenceFreeText);
		}

		public void TestCreateRecreateOrUpdateEventLog_TheSameLogExistInDatabase_DoNotRecreateLog()
		{
			var time = ZDateTimeOffset.Now;
			var oldLog1 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, time, "McLaren", Params.Codes.Location.AsKeyFor("UAIEV"));
			var oldLog2 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Departure, EstimateActual.Actual, time, "McLaren|LOC=UAIEV");

			Factory.Save();

			var newLog1 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, time, "McLaren", Params.Codes.Location.AsKeyFor("UAIEV"));
			var newLog2 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Departure, EstimateActual.Actual, time, "McLaren|LOC=UAIEV");

			AssertEquals("Original log is canceled", false, oldLog1.IsCancelled);
			AssertEquals("Original log is canceled", false, oldLog2.IsCancelled);
			AssertNull(newLog1);
			AssertNull(newLog2);
		}

		public void TestCreateRecreateOrUpdateEventLog_LogWithSameReferenceExistInDatabase_CreateNewLog()
		{
			var time = ZDateTimeOffset.Now;
			var oldLog1 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, time, "McLaren");
			var oldLog2 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Departure, EstimateActual.Actual, time, "McLaren");

			Factory.Save();

			var newLog1 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, time, "McLaren", Params.Codes.Location.AsKeyFor("UAIEV"));
			var newLog2 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Departure, EstimateActual.Actual, time, "McLaren|LOC=UAIEV");

			AssertEquals("Original log is canceled", false, oldLog1.IsCancelled);
			AssertEquals("Original log is canceled", false, oldLog2.IsCancelled);
			AssertNotEquals("Should match existing log", oldLog1, newLog1);
			AssertNotEquals("Should match existing log", oldLog2, newLog2);
			AssertEquals("Location on log", "UAIEV", newLog1.Parameters[Params.Codes.Location]);
			AssertEquals("Location on log", "UAIEV", newLog2.Parameters[Params.Codes.Location]);
			AssertEquals("Reference not changed", "McLaren", newLog1.ReferenceFreeText);
			AssertEquals("Reference not changed", "McLaren", newLog2.ReferenceFreeText);
		}

		public void TestCreateRecreateOrUpdateEventLog_LogWithSameParametersExistInDatabase_CreateNewLog()
		{
			var time = ZDateTimeOffset.Now;
			var oldLog1 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, time, ZString.Empty, Params.Codes.Location.AsKeyFor("UAIEV"));
			var oldLog2 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Departure, EstimateActual.Actual, time, "|LOC=UAIEV");

			Factory.Save();

			var newLog1 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, time, "McLaren", Params.Codes.Location.AsKeyFor("UAIEV"));
			var newLog2 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Departure, EstimateActual.Actual, time, "McLaren|LOC=UAIEV");

			AssertEquals("Original log is canceled", false, oldLog1.IsCancelled);
			AssertEquals("Original log is canceled", false, oldLog2.IsCancelled);
			AssertNotEquals("Should match existing log", oldLog1, newLog1);
			AssertNotEquals("Should match existing log", oldLog2, newLog2);
			AssertEquals("Location on log", "UAIEV", newLog1.Parameters[Params.Codes.Location]);
			AssertEquals("Location on log", "UAIEV", newLog2.Parameters[Params.Codes.Location]);
			AssertEquals("Reference not changed", "McLaren", newLog1.ReferenceFreeText);
			AssertEquals("Reference not changed", "McLaren", newLog2.ReferenceFreeText);
		}

		public void TestCreateRecreateOrUpdateEventLog_LogWithEmptyReferenceExistInDatabase_CreateNewLog()
		{
			var time = ZDateTimeOffset.Now;
			var oldLog1 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, time, ZString.Empty);
			var oldLog2 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Departure, EstimateActual.Actual, time, ZString.Empty);

			Factory.Save();

			var newLog1 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, time, "McLaren", Params.Codes.Location.AsKeyFor("UAIEV"));
			var newLog2 = Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.Departure, EstimateActual.Actual, time, "McLaren|LOC=UAIEV");

			AssertEquals("Original log is canceled", false, oldLog1.IsCancelled);
			AssertEquals("Original log is canceled", false, oldLog2.IsCancelled);
			AssertNotEquals("Should match existing log", oldLog1, newLog1);
			AssertNotEquals("Should match existing log", oldLog2, newLog2);
			AssertEquals("Location on log", "UAIEV", newLog1.Parameters[Params.Codes.Location]);
			AssertEquals("Location on log", "UAIEV", newLog2.Parameters[Params.Codes.Location]);
			AssertEquals("Reference not changed", "McLaren", newLog1.ReferenceFreeText);
			AssertEquals("Reference not changed", "McLaren", newLog2.ReferenceFreeText);
		}

		public void TestNotesReadStatusIsPerUser()
		{
			IGlbStaff user1 = Factory.New<IGlbStaff>();
			IGlbStaff user2 = Factory.New<IGlbStaff>();

			user1.GS_LoginName = "Dummy 1";
			user1.GS_Code = "D1";

			user2.GS_LoginName = "Dummy 2";
			user2.GS_Code = "D2";

			Factory.Save();

			AssertIsFalse(LogParent.Logs.HasUserReadNotes(user1));
			AssertIsFalse(LogParent.Logs.HasUserReadNotes(user2));

			LogParent.Logs.MarkNotesAsRead(user1);
			AssertIsTrue(LogParent.Logs.HasUserReadNotes(user1));
			AssertIsFalse(LogParent.Logs.HasUserReadNotes(user2));

			LogParent.Logs.MarkNotesAsRead(user2);
			AssertIsTrue(LogParent.Logs.HasUserReadNotes(user1));
			AssertIsTrue(LogParent.Logs.HasUserReadNotes(user2));
		}

		public void TestNotesRead_IGuessItNeedsToHandleConcurrencyToo()
		{
			var user1 = Factory.New<IGlbStaff>();
			user1.GS_LoginName = "Dummy 1";
			user1.GS_Code = "D1";
			Factory.Save();

			var branch = Factory.Load<IGlbBranch>(new ZQuery()).FirstOrDefault();
			var bizO = branch as BusinessObject;

			bizO["GB_Code"] = "MOG";
			Factory.Save();
			bizO["GB_Code"] = "MIG";
			(bizO as IStmALogParent).Logs.MarkNotesAsRead(user1);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var freshlyLoadedBranch = newFactory.Load<IGlbBranch>(branch.PK);
			AssertEquals("MOG", (freshlyLoadedBranch as BusinessObject)["GB_Code"]);
		}

		public void TestMarkNotesAsReadWhenParentInDb()
		{
			AssertIsFalse(LogParent.Logs.HasUserReadNotes());

			Factory.Save(); // we want the dummy in the DB

			LogParent.Logs.MarkNotesAsRead();
			AssertIsTrue(LogParent.Logs.HasUserReadNotes());

			// ensure log entry was not written to DB
			AssertEquals("Should have found RRN log in the DB", 1, GetReadNotesLogs(LogParent as BusinessObject).Length);
		}

		public void TestMarkNotesAsRead_ButWhatAboutQuotedBooking()
		{
			var bizo = new QuotedBookingIsTerrible(Factory);
			bizo.Logs.MarkNotesAsRead();
			AssertIsTrue(bizo.Logs.HasUserReadNotes());
			AssertEquals(1, LoadLogs(bizo, Events.ReadRelatedNotes, new BusinessObjectFactory()).Length);
			ErrorReporter.Clear();
		}

		class QuotedBookingIsTerrible : NonPersistentBusinessObject, IStmALogParent
		{
			public QuotedBookingIsTerrible(BusinessObjectFactory factory)
			{
				LogsFactory = factory;
			}
			public override bool IsInDatabase => true;
			public ZGuid LogsParentPK => PK;
			public string LogsParentTableName => "QuotedBooking";
			public BusinessObject[] BusinessObjectsWithRelatedEvents => Array.Empty<BusinessObject>();
			public bool DeferFiringWorkflow => true;
			public Logs Logs => logs ?? (logs = new Logs(this));
			Logs logs;
			public BusinessObjectFactory LogsFactory { get; }

			public void ProcessLog(IStmALog log)
			{
			}
		}

		public void TestMarkNotesAsReadWhenParentNotInDb()
		{
			AssertIsFalse(Dummy.Logs.HasUserReadNotes());

			Dummy.Logs.MarkNotesAsRead();
			AssertIsTrue(Dummy.Logs.HasUserReadNotes());

			// ensure log entry was not written to DB
			AssertEquals("Shouldn't have found RRN log in the DB", 0, GetReadNotesLogs().Length);
		}

		public void TestGetAllLogs()
		{
			StmALogDependentCollection allLogs = Dummy.Logs.GetAllLogs();
			AssertEquals("AllLogs.Count", 0, allLogs.Count);

			StmALog log = Dummy.Logs.AddNew();
			allLogs = Dummy.Logs.GetAllLogs();
			AssertEquals("AllLogs.Count", 1, allLogs.Count);
			AssertEquals("AllLogs[0]", log, allLogs[0]);
		}

		public void TestEventsThatCannotBeCancelled()
		{
			AssertEquals("Precondition", 0, Dummy.Logs.EventsThatCannotBeCancelled.Count);

			Dummy.Logs.EventsThatCannotBeCancelled.AddRange(Events.Booked, Events.CargoCheckin);
			AssertEquals(2, Dummy.Logs.EventsThatCannotBeCancelled.Count);
			Assert(Dummy.Logs.EventsThatCannotBeCancelled.Contains(Events.Booked));
			Assert(Dummy.Logs.EventsThatCannotBeCancelled.Contains(Events.CargoCheckin));
		}

		public void TestEventsThatCannotBeAdded()
		{
			AssertEquals("Precondition", 0, Dummy.Logs.EventsThatCannotBeAdded.Count);

			Dummy.Logs.EventsThatCannotBeAdded.AddRange(Events.Booked, Events.CargoCheckin);
			AssertEquals(2, Dummy.Logs.EventsThatCannotBeAdded.Count);
			Assert(Dummy.Logs.EventsThatCannotBeAdded.Contains(Events.Booked));
			Assert(Dummy.Logs.EventsThatCannotBeAdded.Contains(Events.CargoCheckin));
		}

		public void TestCreateAutoAdminLog()
		{
			DummyWithAutoLogs.Logs.CreateAutoAdminLog();
			AssertNotNull("Logs.AutoCreatedLog should be not be null", DummyWithAutoLogs.Logs.AutoCreatedLog);
			AssertEquals("There should be 1 log in Dummy.Logs.", 1, DummyWithAutoLogs.Logs.GetAllLogs().Count);
			AssertEquals("AutoCreatedLog.SL_SE_NKEvent", Events.AddedARecordToTheSystem.Code, DummyWithAutoLogs.Logs.AutoCreatedLog.SL_SE_NKEvent);
		}

		public void TestCreateAutoAdminLogWithAuditStmALogDecider_AddAndEdtLogsOn()
		{
			using (ObjectFactory.Substitute(SetupMockAuditDecider(true, true, false)))
			{
				var dummy = Factory.New<DummyEnterpriseBusinessObject>();
				Factory.Save();
				AssertEquals("A new ADD log should be created.", 1, dummy.Logs.GetAllLogs().Count);
				dummy.Logs.CreateAutoAdminLog(true);
				AssertEquals("A new EDT log should be created.", 2, dummy.Logs.GetAllLogs().Count);
			}
		}

		public void TestCreateAutoDeleteWithAuditStmALogDecider_DelLogOn()
		{
			using (ObjectFactory.Substitute(SetupMockAuditDecider(false, false, true)))
			{
				var dummy = Factory.New<DummyEnterpriseBusinessObject>();
				Factory.Save();
				dummy.Logs.CreateAutoDeleteLog();
				Factory.Save();
				AssertEquals("A new DEL log should be created.", 1, dummy.Logs.GetAllLogs().Count(l => ((StmALog)l).SL_SE_NKEvent == Events.DeletedARecordInTheSystemCode && l.IsInDatabase));
			}
		}

		public void TestActiveAndInactive()
		{
			Dummy.SetIsCancelled(true);
			Dummy.SetIsInDatabase(true);
			Dummy.Logs.CreateAutoAdminLog();
			AssertEquals(1, Dummy.Logs.GetAllLogs().Count);
			AssertEquals(Events.SetToInactive.Code, Dummy.Logs.AutoCreatedLog.SL_SE_NKEvent);
			Factory.Save();

			Dummy.SetIsCancelled(false);
			Dummy.SetIsInDatabase(true);
			Dummy.Logs.CreateAutoAdminLog();
			AssertEquals(2, Dummy.Logs.GetAllLogs().Count);
			AssertEquals(Events.SetToActive.Code, Dummy.Logs.AutoCreatedLog.SL_SE_NKEvent);
		}

		public void TestRemoveAutoAdminLog()
		{
			Dummy.Logs.GetType().GetMethod("CreateAutoAdminLog", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(DummyWithAutoLogs.Logs, new object[] { false });
			AssertEquals("Precondition - There should be 1 logs in Dummy.Logs.", 1, DummyWithAutoLogs.Logs.GetAllLogs().Count);
			AssertEquals("Precondition - AutoCreatedLog.IsDeleted should be false", false, DummyWithAutoLogs.Logs.AutoCreatedLog.IsDeleted);

			DummyWithAutoLogs.Logs.RemoveAutoAdminLog();
			AssertNull("Accessing a deleted AutoCreatedLog should return null.", DummyWithAutoLogs.Logs.AutoCreatedLog);
			AssertEquals("There should be 0 logs in Dummy.Logs.", 0, DummyWithAutoLogs.Logs.GetAllLogs().Count);
		}

		public void TestRemoveAutoAdminLogUnfiresTriggers()
		{
			using (ObjectFactory.Substitute(SetupMockAuditDecider(true, true, true)))
			{
				var dummy = Factory.New<IDummyWithWorkflow>();
				var trigger = dummy.AddNewTrigger() as ITriggerConditions;
				trigger.TriggerEventCode = Events.AddedARecordToTheSystemCode;
				trigger.TriggerFiredCountdown = 10;
				var logs = ((IStmALogParent)dummy).Logs;
				logs.CreateAutoAdminLog();

				AssertEquals((ZShort)9, trigger.TriggerFiredCountdown);

				logs.RemoveAutoAdminLog();

				AssertEquals("Removing the ADD event should unfire ADD trigger", (ZShort)10, trigger.TriggerFiredCountdown);
			}
		}

		public void TestRemoveAutoAdminLog_IgnoreIfAlreadyInDatabase()
		{
			DummyWithAutoLogs.Logs.CreateAutoAdminLog();
			AssertEquals("Precondition - There should be 1 logs in Dummy.Logs.", 1, DummyWithAutoLogs.Logs.GetAllLogs().Count);
			AssertEquals("Precondition - AutoCreatedLog.IsDeleted should be false", false, DummyWithAutoLogs.Logs.AutoCreatedLog.IsDeleted);
			StmALog previousLog = DummyWithAutoLogs.Logs.AutoCreatedLog;

			Factory.Save();
			DummyWithAutoLogs.Logs.RemoveAutoAdminLog();
			AssertEquals(previousLog, DummyWithAutoLogs.Logs.AutoCreatedLog);
			AssertEquals(1, DummyWithAutoLogs.Logs.GetAllLogs().Count);
		}

		[ExpectNoExceptions]
		public void TestRemoveAutoAdminLogOnSaved_ShouldNotThrowException()
		{
			Dummy.Logs.CreateAutoAdminLog();
			Dummy.Factory.Save();

			EventHandler savingBlowUp = delegate
			{
				throw new ApplicationException("Saving Blow Up");
			};
			Dummy.FactorySaving += savingBlowUp;

			try
			{
				Dummy.Factory.Save();
			}
			catch
			{
			}

			Dummy.FactorySaving -= savingBlowUp;
			Dummy.Factory.Save();
		}

		public void TestCreateAutoDeleteLogDoesNotCreateLogIfParentNotInDb()
		{
			AssertEquals("Precondition - Dummy.IsInDatabase should be false", false, DummyWithAutoLogs.IsInDatabase);
			AssertNull("Precondition - Logs.AutoCreatedLog should be null", DummyWithAutoLogs.Logs.AutoCreatedLog);
			AssertEquals("Precondition - There should be 0 logs in Dummy.Logs.", 0, DummyWithAutoLogs.Logs.GetAllLogs().Count);

			Dummy.Logs.GetType().GetMethod("CreateAutoDeleteLog", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(DummyWithAutoLogs.Logs, null);
			AssertNull("Logs.AutoCreatedLog should still be null", DummyWithAutoLogs.Logs.AutoCreatedLog);
			AssertEquals("There should be 0 logs in Dummy.Logs.", 0, DummyWithAutoLogs.Logs.GetAllLogs().Count);
		}

		public void TestCreateAutoDeleteLog()
		{
			using (ObjectFactory.Substitute(SetupMockAuditDecider(false, false, true)))
			{
				Factory.Save();
				Assert("Precondition - Logs.AutoCreatedLog should not be saved to db", !Dummy.Logs.AutoCreatedLog.IsInDatabase);
				AssertEquals("Precondition - There should be 0 logs in the db in Dummy.Logs.", 0, Dummy.Logs.GetAllLogs().Count(log => log.IsInDatabase));

				Dummy.Logs.GetType().GetMethod("CreateAutoDeleteLog", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Dummy.Logs, null);
				AssertNotNull("Logs.AutoCreatedLog should be not be null", Dummy.Logs.AutoCreatedLog);
				AssertEquals("There should be 1 log in Dummy.Logs.", 1, Dummy.Logs.GetAllLogs().Count(log => ((StmALog)log).SL_SE_NKEvent == Events.DeletedARecordInTheSystemCode));
				AssertEquals("AutoCreatedLog.SL_SE_NKEvent", Events.DeletedARecordInTheSystem.Code, Dummy.Logs.AutoCreatedLog.SL_SE_NKEvent);
			}
		}

		public void TestRemoveAutoDeleteLog()
		{
			using (ObjectFactory.Substitute(SetupMockAuditDecider(false, false, true)))
			{
				Factory.Save();
				Dummy.Logs.CreateAutoDeleteLog();
				AssertEquals("Precondition - There should be 1 logs in Dummy.Logs.", 1, Dummy.Logs.GetAllLogs().Count(l => ((StmALog)l).SL_SE_NKEvent == Events.DeletedARecordInTheSystemCode));
				Assert("Precondition - AutoCreatedLog.IsDeleted should be false", !Dummy.Logs.AutoCreatedLog.IsDeleted);

				Dummy.Logs.RemoveAutoDeleteLog();
				AssertNull("Accessing a deleted AutoCreatedLog should return null.", Dummy.Logs.AutoCreatedLog);
				AssertEquals("There should be 0 logs in Dummy.Logs.", 0, Dummy.Logs.GetAllLogs().Count(l => ((StmALog)l).SL_SE_NKEvent == Events.DeletedARecordInTheSystemCode));
			}
		}

		public void TestAddedLogCreatesBeforeSaveOnAutoLog()
		{
			AssertNotNull(DummyWithAutoLogs.Logs.AddedLog);
		}

		public void TestAddedLogDoesNotCreateBeforeSaveOnManualLog()
		{
			AssertNull(Dummy.Logs.AddedLog);
		}

		public void TestAddedLogPostedTime()
		{
			DummyBizOWithAutoLogs dummy = Factory.New<DummyBizOWithAutoLogs>();
			AssertEquals("SL_PostedTimeUtc.IsValid before save", false, dummy.Logs.AddedLog.SL_PostedTimeUtc.IsValid);
			Factory.Save();
			AssertEquals("SL_PostedTimeUtc.IsValid after save", true, dummy.Logs.AddedLog.SL_PostedTimeUtc.IsValid);
		}

		public void TestAddedLogIsLazyLoadedWhenLogIsDeleted()
		{
			DummyWithAutoLogsThatCantSave dummy = Factory.New<DummyWithAutoLogsThatCantSave>();
			AssertNotNull(dummy.Logs.AddedLog);
			try
			{
				Factory.Save();
			}
			catch (ZException)
			{
			}
			finally
			{
				Assert("AddedLog should not be deleted", !dummy.Logs.AddedLog.IsDeleted);
			}
		}

		public void TestLogsNotInDB()
		{
			AssertEquals("Precondition - LogsNotInDB should have 0 logs.", 0, Dummy.Logs.LogsNotInDB.Length);

			StmALog log1 = Dummy.Logs.AddNew();
			AssertEquals("LogsNotInDB should have 1 log.", 1, Dummy.Logs.LogsNotInDB.Length);
			AssertEquals("LogsNotInDB should contain the new log.", log1, Dummy.Logs.LogsNotInDB[0]);

			Dummy.Factory.Save();
			AssertEquals("LogsNotInDB should have 0 logs again.", 0, Dummy.Logs.LogsNotInDB.Length);

			StmALog log2 = Dummy.Logs.AddNew();
			AssertEquals("LogsNotInDB should have 1 log.", 1, Dummy.Logs.LogsNotInDB.Length);
			AssertEquals("LogsNotInDB should contain the new log.", log2, Dummy.Logs.LogsNotInDB[0]);
		}

		public void TestLogsNotInDBDoesNotLoadAllLogs()
		{
			Dummy.Logs.AddNew(); // ensure there is a new log so that we attempt to access NewElements
			Assert("Precondition - Dummy.IsElementsLoaded should be false", !Dummy.IsElementsLoaded);
			Assert("Precondition - Dummy.IsAllElementsLoaded should be false", !Dummy.IsAllElementsLoaded);

			object accessLogsNotInDB = Dummy.Logs.LogsNotInDB;
			Assert("Dummy.Logs.IsElementsLoaded should still be false", !Dummy.IsElementsLoaded);
			Assert("Dummy.Logs.IsAllElementsLoaded should still be false", !Dummy.IsAllElementsLoaded);
		}

		public void TestAutoCreateLogDefaultSL_ReferenceIsEmptyByDefault()
		{
			DummyBizOWithAutoLogs dummyWithAutoLogs = Factory.New(typeof(DummyBizOWithAutoLogs)) as DummyBizOWithAutoLogs;
			Factory.Save();
			AssertEquals("AutoCreatedLogDefaultSL_Reference", "", dummyWithAutoLogs.Logs.AutoCreatedLog.SL_Reference);
		}

		public void TestAutoCreateLogDefaultSL_ReferenceAdd()
		{
			DummyBizOWithAutoLogs dummyWithAutoLogs = Factory.New(typeof(DummyBizOWithAutoLogs)) as DummyBizOWithAutoLogs;
			dummyWithAutoLogs.Logs.AutoCreatedLogDefaultSL_Reference = "Test Reference for Dummy";
			Factory.Save();
			AssertEquals("AutoCreatedLogDefaultSL_Reference", "Test Reference for Dummy", dummyWithAutoLogs.Logs.AutoCreatedLog.SL_Reference);
		}

		public void TestAutoCreateLogDefaultSL_ReferenceEdit()
		{
			DummyBizOWithAutoLogs dummyWithAutoLogs = Factory.New(typeof(DummyBizOWithAutoLogs)) as DummyBizOWithAutoLogs;
			Factory.Save();
			AssertEquals("AutoCreatedLogDefaultSL_Reference", "", dummyWithAutoLogs.Logs.AutoCreatedLog.SL_Reference);

			dummyWithAutoLogs.Z0_Code = "ABC";
			dummyWithAutoLogs.Logs.AutoCreatedLogDefaultSL_Reference = "Test Reference for Dummy";
			Factory.Save();
			AssertEquals("AutoCreatedLogDefaultSL_Reference", "Test Reference for Dummy", dummyWithAutoLogs.Logs.AutoCreatedLog.SL_Reference);
		}

		public void TestAutoCreateLogDefaultSL_ReferenceDelete()
		{
			DummyBizOWithAutoLogs dummyWithAutoLogs = Factory.New(typeof(DummyBizOWithAutoLogs)) as DummyBizOWithAutoLogs;
			Factory.Save();
			AssertEquals("AutoCreatedLogDefaultSL_Reference", "", dummyWithAutoLogs.Logs.AutoCreatedLog.SL_Reference);

			dummyWithAutoLogs.Logs.AutoCreatedLogDefaultSL_Reference = "Test Reference for Dummy";
			dummyWithAutoLogs.Delete();
			AssertEquals("AutoCreatedLogDefaultSL_Reference", "Test Reference for Dummy", dummyWithAutoLogs.Logs.AutoCreatedLog.SL_Reference);
		}

		public void TestDeletedAutoCreatedLogCannotBeAccessed()
		{
			ErrorReporter.Clear();

			DummyBizOWithAutoLogs dummyWithAutoLogs = Factory.New(typeof(DummyBizOWithAutoLogs)) as DummyBizOWithAutoLogs;
			Factory.Save();
			AssertNotNull("Precondition DummyWithAutoLogs.Logs.AutoCreatedLog exists.", dummyWithAutoLogs.Logs.AutoCreatedLog);

			try
			{
				dummyWithAutoLogs.Logs.AutoCreatedLog.Delete();
				Fail("Should Throw Exception");
			}
			catch (InvalidOperationException ex)
			{
				Assert("Exception caught (below) is not the one expected.\r\n" + ex.Message, ex.Message.StartsWith("Disallowed attempt to delete an StmALog object already in database"));
			}

			AssertNotNull("Deleted DummyWithAutoLogs.Logs.AutoCreatedLog should NOT return null.", dummyWithAutoLogs.Logs.AutoCreatedLog);
		}

		public void TestSuspendAndResumeCreateAutoAdminLog()
		{
			using (ObjectFactory.Substitute(SetupMockAuditDecider(true, true, true)))
			{
				int initialLogCount = Dummy.Logs.GetAllLogs().Count;
				using (Dummy.Logs.SuspendCreateAutoAdminLog())
				{
					Dummy.Logs.CreateAutoAdminLog();
					AssertEquals("No logs should be created.", initialLogCount, Dummy.Logs.GetAllLogs().Count);
				}
				Dummy.Logs.CreateAutoAdminLog();
				AssertEquals("A log should be created.", initialLogCount + 1, Dummy.Logs.GetAllLogs().Count);
			}
		}

		public void TestGetAllLogsByEventOrderByPostedTimeUtcDESC()
		{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			var log1 = Dummy.Logs.AddNew(new EventValue(Events.EditedARecord, reference: "EDT1"));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			var log2 = Dummy.Logs.AddNew(new EventValue(Events.EditedARecord, reference: "EDT2"));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			var log3 = Dummy.Logs.AddNew(new EventValue(Events.EditedARecord, reference: "EDT3"));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			var log4 = Dummy.Logs.AddNew(new EventValue(Events.EditedARecord, reference: "EDT4"));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Dummy.Logs.AddNew(new EventValue(Events.AddressValidationStatus, reference: "OTH1"));

			((INeedRow)log2).Row[StmALogSchema.Constants.SL_PostedTimeUtc] = new DateTime(2020, 2, 5);
			((INeedRow)log3).Row[StmALogSchema.Constants.SL_PostedTimeUtc] = new DateTime(2020, 2, 16);
			((INeedRow)log4).Row[StmALogSchema.Constants.SL_PostedTimeUtc] = new DateTime(2020, 2, 10);
			log1.Cancel();

			var logs = Dummy.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.EditedARecord).ToList();
			AssertEquals(3, logs.Count);
			AssertEquals("EDT3", logs[0].SL_Reference);
			AssertEquals("EDT4", logs[1].SL_Reference);
			AssertEquals("EDT2", logs[2].SL_Reference);
		}

		public void TestHasLogWith()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			AssertEquals("Precondition : Should not contain any logs in newly create business object", 0, dummy.Logs.GetAllLogs().Count);

			dummy.Logs.AddNew(Events.Departure, ZDateTimeOffset.Empty, true);
			Assert("Should have Departure Log for business object", dummy.Logs.HasLogWith(StmALogSchema.SL_SE_NKEvent, Events.DepartureCode));
		}

		public void TestHasLogWithFilter()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			AssertEquals("Precondition : Should not contain any logs in newly create business object", 0, dummy.Logs.GetAllLogs().Count);

			dummy.Logs.AddNew(Events.Departure, ZDateTimeOffset.Empty, false);

			var filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DepartureCode);

			filter.AddToFilter(StmALogSchema.SL_IsEstimate, true);
			Assert("Should not have Departure Log for business object", !dummy.Logs.HasLogWith(filter));
		}

		public void TestEventsTimeOutOfRangeOfSmallDateTime()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			AssertEquals("Precondition : Should not contain any logs in newly create business object", 0, dummy.Logs.GetAllLogs().Count);

			var date = new ZDateTimeOffset(1899, 8, 27, 10, 30, 35);
			dummy.Logs.AddNew(Events.Departure, date, true);
			Assert("Should NOT report out of range", ErrorReporter.TotalErrorCount == 0);
		}

		public void TestMostRecentLogByEventTime_CollectionNotLoaded_ShouldNotOnlyAccessElementsInternal()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			dummy.Logs.AddNew(Events.Departure, ZDateTimeOffset.Now);

			Factory.Save();

			var newFactory = new LoadCountingFactory();
			var loadedDummy = newFactory.Load<DummyEnterpriseBusinessObject>(dummy.PK);

			AssertFactoryLoads(loadedDummy, shouldLoadsIncrease: true);
			AssertFactoryLoads(loadedDummy, shouldLoadsIncrease: true);

			AssertEquals("None of these methods should have used a direct factory query instead of accessing ElementsInternal because it wasn't loaded, and yet...", false, loadedDummy.Logs.IsElementsLoaded);

			var elements = loadedDummy.Logs.ElementsInternal; // will Load() collection
			AssertEquals(true, loadedDummy.Logs.IsElementsLoaded);

			AssertFactoryLoads(loadedDummy, shouldLoadsIncrease: false);
		}

		public void TestMostRecentLogByEventTime_ChildNotInDB_ShouldLoadChildren()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			dummy.Logs.AddNew(Events.MessageSent, ZDateTimeOffset.Now);
			Factory.Save();

			var newFactory = new LoadCountingFactory();
			var loadedDummy = newFactory.Load<DummyEnterpriseBusinessObject>(dummy.PK);
			loadedDummy.RegisterEditableChildObject(Factory.New<DummyEnterpriseBusinessObject>());
			var mostRecentEvent = loadedDummy.GetLogs().MostRecentLogByEventTime(Events.MessageSent, (x) => true);

			AssertNotNull("Most recent log event should not be null", mostRecentEvent);
		}

		[TestDate(2017, 02, 02)]
		public void TestEventPropertiesDoNotRaiseTooManyEvents_ViaSetDateProperty()
		{
			var dummy = Factory.New<DummyWithRecursiveEventBadness>();

			dummy.Z0_Date = ZDateTime.UtcNow; // Z00 Logs.AddNew
			dummy.Z0_AnotherDate = ZDateTime.UtcNow; // Z01 Logs.CreateOrRecreate
			dummy.Z0_SmallDateTime = ZDateTime.UtcNow; // Z02 Logs.CreateUpdateOrRecreate

			var customEvents = dummy.Logs.Find(l => l.SL_SE_NKEvent.StartsWith("Z"));
			AssertLogsExist("One of each please", customEvents, "Z00", "Z01", "Z02");

			dummy.Z0_Date = ZDateTime.UtcNow;
			dummy.Z0_AnotherDate = ZDateTime.UtcNow;
			dummy.Z0_SmallDateTime = ZDateTime.UtcNow;

			customEvents = dummy.Logs.Find(l => l.SL_SE_NKEvent.StartsWith("Z"));
			AssertLogsExist("Extra Z00, because not updating", customEvents, "Z00", "Z00", "Z01", "Z02");

			Factory.Save();
			TestDateAttribute.Date = ZDateTime.UtcNow.AddMinutes(30).ToDateTime();

			dummy.Z0_Date = ZDateTime.UtcNow;
			dummy.Z0_AnotherDate = ZDateTime.UtcNow;
			dummy.Z0_SmallDateTime = ZDateTime.UtcNow;

			customEvents = dummy.Logs.Find(l => l.SL_SE_NKEvent.StartsWith("Z") && !l.IsCancelled);
			var cancelledEvents = dummy.Logs.Find(l => l.SL_SE_NKEvent.StartsWith("Z") && l.IsCancelled);
			AssertLogsExist("Save means bonus events for CreateRecreate but not update", customEvents, "Z00", "Z00", "Z00", "Z01", "Z01", "Z02");
			AssertLogsExist("Update event was cancelled", cancelledEvents, "Z02");
		}

		[TestDate(2017, 02, 02)]
		public void TestEventPropertiesDoNotRaiseTooManyEvents_ViaAddNew()
		{
			var dummy = Factory.New<DummyWithRecursiveEventBadness>();

			dummy.Logs.AddNew(Events.CustomisableEvent00);
			dummy.Logs.AddNew(Events.CustomisableEvent01);
			dummy.Logs.AddNew(Events.CustomisableEvent02);

			var customEvents = dummy.Logs.Find(l => l.SL_SE_NKEvent.StartsWith("Z"));
			AssertLogsExist("One of each please", customEvents, "Z00", "Z01", "Z02");

			dummy.Logs.AddNew(Events.CustomisableEvent00);
			dummy.Logs.AddNew(Events.CustomisableEvent01);
			dummy.Logs.AddNew(Events.CustomisableEvent02);

			customEvents = dummy.Logs.Find(l => l.SL_SE_NKEvent.StartsWith("Z"));
			AssertLogsExist("Two of each please.", customEvents, "Z00", "Z01", "Z02", "Z00", "Z01", "Z02");

			Factory.Save();
			TestDateAttribute.Date = ZDateTime.UtcNow.AddMinutes(30).ToDateTime();

			dummy.Logs.AddNew(Events.CustomisableEvent00);
			dummy.Logs.AddNew(Events.CustomisableEvent01);
			dummy.Logs.AddNew(Events.CustomisableEvent02);

			customEvents = dummy.Logs.Find(l => l.SL_SE_NKEvent.StartsWith("Z") && !l.IsCancelled);
			var cancelledEvents = dummy.Logs.Find(l => l.SL_SE_NKEvent.StartsWith("Z") && l.IsCancelled);
			AssertLogsExist("All of the events.", customEvents, "Z00", "Z01", "Z02", "Z00", "Z01", "Z02", "Z00", "Z01", "Z02");
			AssertLogsExist("No events", cancelledEvents);
		}

		[TestDate(2017, 02, 02)]
		public void TestEventPropertiesDoNotRaiseTooManyEvents_ViaCreateOrRecreate()
		{
			var dummy = Factory.New<DummyWithRecursiveEventBadness>();

			dummy.Logs.CreateOrRecreateEventLog(Events.CustomisableEvent00, EstimateActual.Actual, ZDateTimeOffset.UtcNow);
			dummy.Logs.CreateOrRecreateEventLog(Events.CustomisableEvent01, EstimateActual.Actual, ZDateTimeOffset.UtcNow);
			dummy.Logs.CreateOrRecreateEventLog(Events.CustomisableEvent02, EstimateActual.Actual, ZDateTimeOffset.UtcNow);

			var customEvents = dummy.Logs.Find(l => l.SL_SE_NKEvent.StartsWith("Z"));
			AssertLogsExist("One of each please", customEvents, "Z00", "Z01", "Z02");

			dummy.Logs.CreateOrRecreateEventLog(Events.CustomisableEvent00, EstimateActual.Actual, ZDateTimeOffset.UtcNow);
			dummy.Logs.CreateOrRecreateEventLog(Events.CustomisableEvent01, EstimateActual.Actual, ZDateTimeOffset.UtcNow);
			dummy.Logs.CreateOrRecreateEventLog(Events.CustomisableEvent02, EstimateActual.Actual, ZDateTimeOffset.UtcNow);

			customEvents = dummy.Logs.Find(l => l.SL_SE_NKEvent.StartsWith("Z"));
			AssertLogsExist("One of each please", customEvents, "Z00", "Z01", "Z02");

			Factory.Save();
			TestDateAttribute.Date = ZDateTime.UtcNow.AddMinutes(30).ToDateTime();

			dummy.Logs.CreateOrRecreateEventLog(Events.CustomisableEvent00, EstimateActual.Actual, ZDateTimeOffset.UtcNow);
			dummy.Logs.CreateOrRecreateEventLog(Events.CustomisableEvent01, EstimateActual.Actual, ZDateTimeOffset.UtcNow);
			dummy.Logs.CreateOrRecreateEventLog(Events.CustomisableEvent02, EstimateActual.Actual, ZDateTimeOffset.UtcNow);

			customEvents = dummy.Logs.Find(l => l.SL_SE_NKEvent.StartsWith("Z") && !l.IsCancelled);
			var cancelledEvents = dummy.Logs.Find(l => l.SL_SE_NKEvent.StartsWith("Z") && l.IsCancelled);
			AssertLogsExist("All the new events get created.", customEvents, "Z00", "Z01", "Z02", "Z00", "Z01", "Z02");
			AssertLogsExist("No cancelled events.", cancelledEvents);
		}

		[TestDate(2017, 02, 02)]
		public void TestEventPropertiesDoNotRaiseTooManyEvents_ViaCreateRecreateOrUpdate()
		{
			var dummy = Factory.New<DummyWithRecursiveEventBadness>();

			dummy.Logs.CreateRecreateOrUpdateEventLog(Events.CustomisableEvent00, EstimateActual.Actual, ZDateTimeOffset.UtcNow);
			dummy.Logs.CreateRecreateOrUpdateEventLog(Events.CustomisableEvent01, EstimateActual.Actual, ZDateTimeOffset.UtcNow);
			dummy.Logs.CreateRecreateOrUpdateEventLog(Events.CustomisableEvent02, EstimateActual.Actual, ZDateTimeOffset.UtcNow);

			var customEvents = dummy.Logs.Find(l => l.SL_SE_NKEvent.StartsWith("Z"));
			AssertLogsExist("One of each please", customEvents, "Z00", "Z01", "Z02");

			dummy.Logs.CreateRecreateOrUpdateEventLog(Events.CustomisableEvent00, EstimateActual.Actual, ZDateTimeOffset.UtcNow);
			dummy.Logs.CreateRecreateOrUpdateEventLog(Events.CustomisableEvent01, EstimateActual.Actual, ZDateTimeOffset.UtcNow);
			dummy.Logs.CreateRecreateOrUpdateEventLog(Events.CustomisableEvent02, EstimateActual.Actual, ZDateTimeOffset.UtcNow);

			customEvents = dummy.Logs.Find(l => l.SL_SE_NKEvent.StartsWith("Z"));
			AssertLogsExist("One of each please", customEvents, "Z00", "Z01", "Z02");

			Factory.Save();
			TestDateAttribute.Date = ZDateTime.UtcNow.AddMinutes(30).ToDateTime();

			dummy.Logs.CreateRecreateOrUpdateEventLog(Events.CustomisableEvent00, EstimateActual.Actual, ZDateTimeOffset.UtcNow);
			dummy.Logs.CreateRecreateOrUpdateEventLog(Events.CustomisableEvent01, EstimateActual.Actual, ZDateTimeOffset.UtcNow);
			dummy.Logs.CreateRecreateOrUpdateEventLog(Events.CustomisableEvent02, EstimateActual.Actual, ZDateTimeOffset.UtcNow);

			customEvents = dummy.Logs.Find(l => l.SL_SE_NKEvent.StartsWith("Z") && !l.IsCancelled);
			var cancelledEvents = dummy.Logs.Find(l => l.SL_SE_NKEvent.StartsWith("Z") && l.IsCancelled);
			AssertLogsExist("The add new event behaves a little oddly for this case.", customEvents, "Z00", "Z01", "Z02");
			AssertLogsExist("One of each please", cancelledEvents, "Z00", "Z01", "Z02");
		}

		public void TestAutoEditEventLogsShouldBeAddedOnceOnly()
		{
			var autoLoggedDummy = Factory.New<DummyAutoLogged>();
			AssertEquals("Logs should be empty.", 0, autoLoggedDummy.Logs.GetAllLogs().Count);
			autoLoggedDummy.IsTopLevel = true;
			autoLoggedDummy.RegisterEditableChildObject(autoLoggedDummy.Collection);
			var dummyChild = autoLoggedDummy.Collection.AddNew();
			Factory.Save();
			AssertEquals("Total logs should be 1, 'ADD' event log added.", 1, autoLoggedDummy.Logs.GetAllLogs().Count);
			AssertEquals("1 'ADD' event log added.", 1, autoLoggedDummy.Logs.GetAllLogs().Where(log => log.SL_SE_NKEvent == "ADD").Count());

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			autoLoggedDummy.Logs.AddNew(Events.EditedARecord, "Merged Lines");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			dummyChild.Z0_Description = "Test";
			Factory.Save();
			AssertEquals("Only 1 'EDT' event logs should be added, 1 with reference.", 2, autoLoggedDummy.Logs.GetAllLogs().Count);
			AssertEquals("1 'ADD' event log.", 1, autoLoggedDummy.Logs.GetAllLogs().Where(log => log.SL_SE_NKEvent == "ADD").Count());
			AssertEquals("1 'EDT' event logs.", 1, autoLoggedDummy.Logs.GetAllLogs().Where(log => log.SL_SE_NKEvent == "EDT").Count());
		}

		void AssertLogsExist(string message, IEnumerable<StmALog> logs, params string[] expected)
		{
			var actual = logs.Select(s => s.SL_SE_NKEvent.ToString()).ToArray();
			AssertContainsExactElementsInAnyOrder(message, expected, actual);
		}

		public void TestEventsInTheProcessOfBeingAdded_OnlyCreatesNewEventsOfUniqueEventType()
		{
			var today = ZDateTimeOffset.Today;
			var dummy = Factory.New<DummyForEventDatePropertyTesting>();
			AssertEquals("Precondition: There should be no events on the log", 0, dummy.Logs.GetAllLogs().Count);

			dummy.Logs.AddNew(Events.GateIn, today);
			var expectedEvents = new[] { AutoEvents.GateInCode, AutoEvents.GateOutCode, AutoEvents.DehireCode };
			AssertContainsExactElementsInAnyOrder(expectedEvents, dummy.Logs.GetAllLogs().Cast<StmALog>().Select(log => log.SL_SE_NKEvent));
		}

		void AssertFactoryLoads(DummyEnterpriseBusinessObject dummy, bool shouldLoadsIncrease)
		{
			var factory = (LoadCountingFactory)dummy.Factory;
			var tableHits = factory.LoadCount;
			var log = dummy.Logs.MostRecentLogByEventTime(Events.Departure);
			AssertNotNull(log);
			AssertEquals("The collection isn't loaded, so a factory hit should have ocurred, and yet...", shouldLoadsIncrease ? ++tableHits : tableHits, factory.LoadCount);

			log = dummy.Logs.MostRecentLogByEventTime(Events.Departure, l => l.SL_SE_NKEvent == Events.Departure.Code);
			AssertNotNull(log);
			AssertEquals("The collection isn't loaded, so a factory hit should have ocurred, and yet...", shouldLoadsIncrease ? ++tableHits : tableHits, factory.LoadCount);

			log = dummy.Logs.MostRecentLogByPostedDate;
			AssertNotNull(log);
			AssertEquals("The collection isn't loaded, so a factory hit should have ocurred, and yet...", shouldLoadsIncrease ? ++tableHits : tableHits, factory.LoadCount);

			log = dummy.Logs.MostRecentLog;
			AssertNotNull(log);
			AssertEquals("The collection isn't loaded, so a factory hit should have ocurred, and yet...", shouldLoadsIncrease ? ++tableHits : tableHits, factory.LoadCount);
		}

		#region ILogsInternals

		public void TestAddNew_WithType()
		{
			StmALog log = ((ILogsInternals)Dummy.Logs).AddNew(typeof(SubClassedStmALog));
			AssertEquals(typeof(SubClassedStmALog), log.GetType());
		}

		public void TestReloadFromDB()
		{
			AssertEquals("Dummy.Logs.VisibleLogs.Count", 0, LogCollectionView.Count);

			Dummy.Factory.Save();
			var otherFactory = new BusinessObjectFactory();
			var logPostedViaOtherFactory = otherFactory.New<StmALog>();
			otherFactory.ImportFromAnotherFactory(Dummy);
			using (logPostedViaOtherFactory.LockForUpdatingKeyFieldsForTesting())
			{
				logPostedViaOtherFactory.SL_Parent = Dummy.PK;
				logPostedViaOtherFactory.SL_Table = Dummy.TableName;
			}
			otherFactory.Save();
			AssertEquals("Dummy.Logs.VisibleLogs.Count", 0, LogCollectionView.Count);
			Factory.Save();
			((ILogsInternals)Dummy.Logs).ReloadFromDB();
			AssertEquals("Dummy.Logs.VisibleLogs.Count", 1, LogCollectionView.Count);
		}

		#endregion

		#region Test Classes

		class LoadCountingFactory : BusinessObjectFactory
		{
			public int LoadCount { get; set; }

			protected override BusinessObject[] LoadCore(string tableOrViewName, Type bizOType, ZQuery effectiveFilter)
			{
				LoadCount++;
				return base.LoadCore(tableOrViewName, bizOType, effectiveFilter);
			}
		}

		class SubClassedDummyBusinessObject : DummyBizOWithAutoLogs
		{
			public SubClassedDummyBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		class SubClassedStmALog : StmALog
		{
			public SubClassedStmALog(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		class DummyWithAutoLogsThatCantSave : DummyBizOWithAutoLogs
		{
			public DummyWithAutoLogsThatCantSave(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override void OnSaving()
			{
				base.OnSaving();
				throw new ZException("cant save");
			}
		}

		#endregion

		#region Implementation

		StmALogCollectionView LogCollectionView
		{
			get
			{
				if (logCollectionView == null)
				{
					logCollectionView = new StmALogCollectionView(Dummy);
				}
				return logCollectionView;
			}
		}
		StmALogCollectionView logCollectionView;

		DummyBizOWithAutoLogs DummyWithAutoLogs
		{
			get
			{
				if (dummyWithAutoLogs == null)
				{
					dummyWithAutoLogs = Factory.New<DummyBizOWithAutoLogs>();
					AssertNull("Precondition - Logs.AutoCreatedLog should be null", DummyWithAutoLogs.Logs.AutoCreatedLog);
					AssertEquals("Precondition - There should be 0 logs in Dummy.Logs.", 0, DummyWithAutoLogs.Logs.GetAllLogs().Count);
				}
				return dummyWithAutoLogs;
			}
		}
		DummyBizOWithAutoLogs dummyWithAutoLogs;

		void AssertAlmostNow(string message, ZDateTime date, ZDateTime startDate, ZDateTime endDate)
		{
			string dateTimeFormat = "dd-MMM-yyyy HH:mm:ss:fff";
			bool isBeforeEnd = date <= endDate;
			bool isAfterStart = date >= startDate;
			string failMessage = !isBeforeEnd ? "after " + endDate.ToString(dateTimeFormat) : (!isAfterStart ? "before " + startDate.ToString(dateTimeFormat) : "");

			Assert(message + " DateTime was: <" + date.ToString(dateTimeFormat) + "> " + failMessage, isAfterStart && isBeforeEnd);
		}

		StmALog[] GetReadNotesLogs(BusinessObject bizO = null)
		{
			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();
			return LoadLogs(bizO ?? Dummy, Events.ReadRelatedNotes, loadingFactory);
		}

		StmALog[] LoadLogs(BusinessObject parent, Event @event, BusinessObjectFactory logsFactory)
		{
			ZQuery query = new ZQuery(StmALogSchema.SL_Parent, parent.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, @event.Code);

			return (StmALog[])logsFactory.Load(typeof(StmALog), query);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Dummy = Factory.New<DummyWithRelatedLogs>();
			Note = Factory.New<StmNote>();
			Note.ST_Table = AutoDummyBizo.Schema.TableName;

			// savable log parent
			var branch = Factory.Load<IGlbBranch>(new ZQuery()).FirstOrDefault();
			LogParent = branch as IStmALogParent;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Dummy.Logs;
		}

		DummyWithRelatedLogs Dummy;
		StmNote Note;
		IStmALogParent LogParent;

		#endregion

		#region Assertion Helpers

		public void AssertIsFalse(bool condition)
		{
			AssertIsTrue(!condition);
		}

		public void AssertIsTrue(bool condition)
		{
			Assert(condition);
		}

		#endregion
	}
}
