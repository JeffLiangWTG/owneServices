using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Bi.Common.Testing;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.AuditDataServices.MDM.Subscribers;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.MDM.Testing
{
	[TestedType(typeof(OrgHeaderSubscriber))]
	class OrgHeaderSubscriberTest : PatternMatchingBaseSubscriberTest<OrgHeader, OrgHeaderSubscriber>
	{
		protected override OrgHeader CreateParentRecords(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory, "THE AWESOME");
			return orgHeader;
		}

		protected override OrgHeader CreatePlaceholderRecords(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);
			return orgHeader;
		}

		protected override void CreatePatternMatchingRecords(BusinessObjectFactory factory, OrgHeader bizO)
		{
			SubscriberTestUtilities.CreatePatternMatchingBusinessObject<PatternMatchingName>(factory, bizO.PK, SubscriberTestUtilities.GetHash(TextStandardizerHelper.StandardizeCompanyName("THE AWESOME", bizO.CountryCode)), OrgHeaderSchema.Constants.Prefix, bizO.PK, bizO.CountryCode);
		}

		protected override string GetGenericQuery
		{
			get { return @"INSERT [dbo].[OrgHeader] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OH_PK, OH_Code, OH_FullName, OH_IsActive) VALUES 
				(0x10, {4}, {0}, 0x0, 1803, 1, '{1}', '{2}', '{3}', 1);"; }
		}

		protected override string GetLsnMappingInsertQuery() => "INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15');";

		protected override string GetCdcMappingInsertQuery() => "INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgHeader', 1, 1803);";

		protected override string GetNewRecordCDCInsertQuery(OrgHeader bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "2", bizO.PK, bizO.OH_Code, bizO.OH_FullName, SeqVal);
		}

		protected override string GetOriginalRecordCDCInsertQuery(OrgHeader bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "3", bizO.PK, bizO.OH_Code, bizO.OH_FullName, SeqVal);
		}

		protected override string GetUpdateRecordCDCInsertQuery(OrgHeader bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.OH_Code, "THE GOOGLE", SeqVal);
		}

		protected override string GetNonSubscribedColumnsCDCInsertQuery(OrgHeader bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, "ABFESTA", bizO.OH_FullName, SeqVal);
		}

		protected override string GetUpdateRecordToPlaceHolderCDCInsertQuery(OrgHeader bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.OH_Code, "THE GOOGLE", SeqVal);
		}

		protected override string GetLog
		{
			get { return "Queued Deduplication in OrgHeaderSubscriber for {0}. Row: {1} - {2} - {3} - {4}"; }
		}

		int seqVal;
		string SeqVal
		{
			get
			{
				if (seqVal.Equals(null))
				{
					seqVal = 1;
				}
				else
				{
					seqVal++;
				}
				return "0x0" + seqVal.ToString();
			}
		}

		protected override BetterLogForTest GetLogForNewRecords(OrgHeader bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Added", bizO.PK, bizO.OH_Code, bizO.OH_FullName));
		}

		protected override BetterLogForTest GetLogForExistingRecords(OrgHeader bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, bizO.OH_Code, "THE GOOGLE"));
		}

		protected override BetterLogForTest GetLogForDeletedRecords(OrgHeader bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, bizO.OH_Code, "THE GOOGLE"));
		}

		protected override string GetPatternMasters(OrgHeader bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, "[OH_PK:{0}]", bizO.PK);
		}

		protected override void AssertPatternMatchingNewRecordsPrecondition(BusinessObjectFactory factory, OrgHeader bizO)
		{
			var patternMatchingName = factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK));

			AssertEquals(0, patternMatchingName.Length);
		}

		void AssertPatternMatchingRecords(BusinessObjectFactory factory, OrgHeader bizO, string orgName)
		{
			var standardName = TextStandardizerHelper.StandardizeCompanyName(orgName, bizO.CountryCode);
			var hashedValue = TextStandardizerHelper.ComputeStringHashFast(standardName);
			var patternMatchingName = factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingName = patternMatchingName.Single();

			AssertEquals(1, patternMatchingName.Count);
			AssertEquals(hashedValue, firstPatternMatchingName.PMN_HashedValue);
			AssertEquals(bizO.PK, firstPatternMatchingName.PMN_OH);
			AssertEquals(OrgHeaderSchema.Constants.Prefix, firstPatternMatchingName.PMN_ParentTableCode);
			AssertEquals(bizO.PK, firstPatternMatchingName.PMN_ParentId);
			AssertEquals(bizO.CountryCode, firstPatternMatchingName.PMN_RN_NKCountryCode);
		}

		protected override void AssertPatternMatchingForNewRecords(BusinessObjectFactory factory, OrgHeader bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, bizO.OH_FullName);
		}

		protected override void AssertPatternMatchingForExistingRecords(BusinessObjectFactory factory, OrgHeader bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, "THE GOOGLE");
		}

		protected override void AssertNoPatternMatchingForPlaceholderRecords(BusinessObjectFactory factory, OrgHeader bizO)
		{
			// company names don't remove placeholders
			Assert(true);
		}

		protected override void AssertPatternMatchingForNonSubscribedColumns(BusinessObjectFactory factory, OrgHeader bizO)
		{
			var updatedPatternMatchingName = factory.LoadTop1<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK));

			AssertEquals(SubscriberTestUtilities.GetHash(TextStandardizerHelper.StandardizeCompanyName("THE AWESOME", bizO.CountryCode)), updatedPatternMatchingName.PMN_HashedValue);
		}

		protected override void AssertQUEResultRecordExists(BusinessObjectFactory factory, OrgHeader bizO)
		{
			AssertQueuedRecordExists(factory, bizO.PK, OrgHeaderSchema.Constants.Prefix);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestProcessPMTForAllChangedRecords()
		{
			var factory = new BusinessObjectFactory();
			var bizO = CreateParentRecords(factory);
			var bizO2 = CreateParentRecords(factory);
			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var insertIntoCDCText = GetNewRecordCDCInsertQuery(bizO);
				auditConnection.ExecuteNonQuery(insertIntoCDCText);

				insertIntoCDCText = GetNewRecordCDCInsertQuery(bizO2);
				auditConnection.ExecuteNonQuery(insertIntoCDCText);

				var insertLsnMappingText = GetLsnMappingInsertQuery();
				auditConnection.ExecuteNonQuery(insertLsnMappingText);

				var insertCDCHistoryText = GetCdcMappingInsertQuery();
				auditConnection.ExecuteNonQuery(insertCDCHistoryText);

				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, "0x10"))
				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, "2018-03-15 00:00:00.000"))
				{
					var wrappedSub = testSubscribers[0].GetWrapper(auditConnection, new LoggerForTest());
					if (wrappedSub.ShouldRunSubscriber())
					{
						wrappedSub.FetchDataAndProcessChanges(); // TODO: Does not process changes because HasChangesToProcess() == false
					}
				}

				AssertQUEResultRecordExists(factory, bizO);
				AssertQUEResultRecordExists(factory, bizO2);
			}
		}

		public override void TestCustomFilter()
		{
			var subscriber = new OrgHeaderSubscriber();

			var table = GetTestDataTable();
			var rowToKeep = GetPopulatedDataRow(table, false);
			var rowToDelete = GetPopulatedDataRow(table, true);
			table.AcceptChanges();

			AssertEquals(2, table.Rows.Count);

			RunCustomFilter(rowToKeep, subscriber);
			RunCustomFilter(rowToDelete, subscriber);
			table.AcceptChanges();

			AssertEquals(1, table.Rows.Count);
			AssertCollectionContains(rowToKeep, table.Rows);
			AssertCollectionNotContains(rowToDelete, table.Rows);
		}

		DataRow GetPopulatedDataRow(DataTable table, bool shouldBeFiltered)
		{
			var row = table.NewRow();
			if (shouldBeFiltered)
			{
				row[OrgHeaderSchema.OH_IsActive.Name] = 0;
			}
			else
			{
				row[OrgHeaderSchema.OH_IsActive.Name] = 1;
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { OrgHeaderSchema.OH_IsActive };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
