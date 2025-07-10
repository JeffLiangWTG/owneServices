using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.MDM.Subscribers;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.MDM.Testing
{
	public class PatternMatchingSubscriberTest : AuditSubscriberTest
	{
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestPatternMatchingSubscriberDoesNotLog_ProcessNothing()
		{
			var changeTable = GetNewChangeTable();
			var testLogger = new BetterLoggerForTest();

			fakeSubscriber.PatternMasterDetailsForTesting = Array.Empty<PatternMasterDetail>();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				fakeSubscriber.ProcessChanges(testLogger, changeTable);
				AuditTestHelper.AssertLog(testLogger.Logs, new BetterLogForTest(LogType.Debug, "None Queuing Deduplication in FakePatternMatchingSubscriber. Row: [Fake Row Detail]"));
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestPatternMatchingSubscriberDoesNotHaveErrorReportWhenAccessCurrentBranch()
		{
			ErrorReporter.Clear();
			var changeTable = GetNewChangeTable();
			var testLogger = new BetterLoggerForTest();

			fakeSubscriber.PatternMasterDetailsForTesting = Array.Empty<PatternMasterDetail>();
			using (EnvProxy.Instance.TemporaryServiceTaskContext("TEST_SERVICE", true))
			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				fakeSubscriber.ProcessChanges(testLogger, changeTable);
				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestPatternMatchingSubscriberLog_Queued()
		{
			var changeTable = GetNewChangeTable();
			var testLogger = new BetterLoggerForTest();

			fakeSubscriber.PatternMasterDetailsForTesting = new PatternMasterDetail[]
			{
				new PatternMasterDetailForTest(queued: true, index: 1),
				new PatternMasterDetailForTest(queued: true, index: 2)
			};

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				fakeSubscriber.ProcessChanges(testLogger, changeTable);
				AuditTestHelper.AssertLog(testLogger.Logs, new BetterLogForTest(LogType.Information, "Queued Deduplication in FakePatternMatchingSubscriber for [FakeMasterDetail:1], [FakeMasterDetail:2]. Row: [Fake Row Detail]"));
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestPatternMatchingSubscriberLog_SkipQueuing()
		{
			var changeTable = GetNewChangeTable();
			var testLogger = new BetterLoggerForTest();

			fakeSubscriber.PatternMasterDetailsForTesting = new PatternMasterDetail[]
			{
				new PatternMasterDetailForTest(queued: false, index: 1),
				new PatternMasterDetailForTest(queued: false, index: 2)
			};

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				fakeSubscriber.ProcessChanges(testLogger, changeTable);
				AuditTestHelper.AssertLog(testLogger.Logs, new BetterLogForTest(LogType.Debug, "Skip Queuing Deduplication in FakePatternMatchingSubscriber for [FakeMasterDetail:1], [FakeMasterDetail:2]. Row: [Fake Row Detail]"));
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestPatternMatchingSubscriberLog_PartQueued()
		{
			var changeTable = GetNewChangeTable();
			var testLogger = new BetterLoggerForTest();

			fakeSubscriber.PatternMasterDetailsForTesting = new PatternMasterDetail[]
			{
				new PatternMasterDetailForTest(queued: false, index: 1),
				new PatternMasterDetailForTest(queued: true, index: 2)
			};

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				fakeSubscriber.ProcessChanges(testLogger, changeTable);
				AuditTestHelper.AssertLog(testLogger.Logs,
					new BetterLogForTest(LogType.Information, "Queued Deduplication in FakePatternMatchingSubscriber for [FakeMasterDetail:2]. Row: [Fake Row Detail]"),
					new BetterLogForTest(LogType.Debug, "Skip Queuing Deduplication in FakePatternMatchingSubscriber for [FakeMasterDetail:1]. Row: [Fake Row Detail]"));
			}
		}

		protected override IAuditSubscriber TestSubscriber => fakeSubscriber;
		FakePatternMatchingSubscriber fakeSubscriber { get; } = new FakePatternMatchingSubscriber();

		DataTable GetNewChangeTable()
		{
			var factory = new BusinessObjectFactory();
			var org = SubscriberTestUtilities.CreateTestOrgHeader(factory, "THE AWESOME");
			factory.Save();

			var changeTable = new DataTable();
			changeTable.Columns.Add(OrgHeaderSchema.Constants.PK, typeof(Guid));
			var changeRow = changeTable.NewRow();
			changeRow[OrgHeaderSchema.Constants.PK] = org.PK.ToGuid();
			changeTable.Rows.Add(changeRow);

			return changeTable;
		}
	}

	class FakePatternMatchingSubscriber : PatternMatchingSubscriber<OrgHeader>
	{
		public override string Code => "FPM";
		public override ITableSchema Table => OrgHeaderSchema.Instance;
		protected override string PKColumn => OrgHeaderSchema.Constants.PK;
		protected override ICollection<string> ColumnsToHash => Array.Empty<string>();

		public override string Description => throw new NotImplementedException();
		public override IEnumerable<SchemaColumn> SpecificColumns => throw new NotImplementedException();
		public override Action<DataRow> CustomFilter => throw new NotImplementedException();
		protected override List<string> GetValuesToHash(DataRow changeRow, string changeRowField, DataRowVersion rowVersion) => throw new NotImplementedException();
		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue) => throw new NotImplementedException();
		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue) => throw new NotImplementedException();
		protected override bool DeleteExistingRecords(BusinessObjectFactory factory, int index, int hashedValue) => throw new NotImplementedException();

		protected override string GetRowDetail(DataRow changeRow) => "[Fake Row Detail]";
		protected override string GetSubscriberName() => nameof(FakePatternMatchingSubscriber);

		protected override IEnumerable<PatternMasterDetail> ProcessPatternMatchingResultsCore(BusinessObjectFactory factory)
		{
			FetchEnvCurrentBranch();
			return PatternMasterDetailsForTesting;
		}

		public IEnumerable<PatternMasterDetail> PatternMasterDetailsForTesting { private get; set; }

		IBranch FetchEnvCurrentBranch() => Env.CurrentBranch;
	}

	class PatternMasterDetailForTest : PatternMasterDetail
	{
		readonly int index;

		public PatternMasterDetailForTest(bool queued, int index)
			: base(PatternMasterType.OrgHeader, ZGuid.Empty, queued)
		{
			this.index = index;
		}

		public override string ToString() => $"[FakeMasterDetail:{index}]";
	}
}
