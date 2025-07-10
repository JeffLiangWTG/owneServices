using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.AuditDataService.MDM.Testing;
using Enterprise.AuditDataServices.MDM.Subscribers;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.MDM.Testing
{
	[TestedType(typeof(GlbPersonNameSubscriber))]
	class GlbPersonNameSubscriberTest : PatternMatchingPersonNameSubscriberTest<GlbPerson, GlbPersonNameSubscriber>
	{
		protected override string GetGenericQuery => @"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_Gender) VALUES (0x10, 0x01, {0}, 0x0, 1803, 1, '{1}', '{2}', '{3}');";

		protected override string GetLsnMappingInsertQuery() => "INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08');";

		protected override string GetCdcMappingInsertQuery() => "INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbPerson', 1, 1803);";

		protected override string GetLog => "Queued Deduplication in GlbPersonNameSubscriber for {0}. Row: {1} - {2} - {3}";

		protected override GlbPerson CreateParentRecords(BusinessObjectFactory factory)
		{
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			glbPerson.PER_FullName = "Albert Cheng";

			return glbPerson;
		}

		protected override GlbPerson CreatePlaceholderRecords(BusinessObjectFactory factory)
		{
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			glbPerson.PER_FullName = "test test";

			return glbPerson;
		}

		protected override void CreatePatternMatchingRecords(BusinessObjectFactory factory, GlbPerson bizO)
		{
			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingName>(factory, bizO.PK, GetUpperValueToHash(bizO.PER_FullName), GlbPersonSchema.Constants.Prefix, bizO.PK, bizO.PER_RN_NKCountry);
		}

		protected override string GetNewRecordCDCInsertQuery(GlbPerson bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "2", bizO.PK, bizO.PER_FullName, "M");
		}

		protected override string GetOriginalRecordCDCInsertQuery(GlbPerson bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "3", bizO.PK, bizO.PER_FullName, "M");
		}

		protected override string GetUpdateRecordCDCInsertQuery(GlbPerson bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, "Different name", "M");
		}

		protected override string GetNonSubscribedColumnsCDCInsertQuery(GlbPerson bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.PER_FullName, "F");
		}

		protected override string GetUpdateRecordToPlaceHolderCDCInsertQuery(GlbPerson bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, "test", "M");
		}

		protected override BetterLogForTest GetLogForNewRecords(GlbPerson bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Added", bizO.PK, bizO.PER_FullName));
		}

		protected override BetterLogForTest GetLogForExistingRecords(GlbPerson bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "Different name"));
		}

		protected override BetterLogForTest GetLogForDeletedRecords(GlbPerson bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "test"));
		}

		protected override string GetPatternMasters(GlbPerson bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, "[PER_PK:{0}]", bizO.PK);
		}

		protected override void AssertPatternMatchingNewRecordsPrecondition(BusinessObjectFactory factory, GlbPerson bizO)
		{
			var patternMatchingName = factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK));
			AssertEquals(0, patternMatchingName.Length);
		}

		protected override void AssertPatternMatchingForNewRecords(BusinessObjectFactory factory, GlbPerson bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, bizO.PER_FullName);
		}

		protected override void AssertPatternMatchingForExistingRecords(BusinessObjectFactory factory, GlbPerson bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, "Different name");
		}

		protected override void AssertNoPatternMatchingForPlaceholderRecords(BusinessObjectFactory factory, GlbPerson bizO)
		{
			var patternMatchingName = factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK)).ToList();
			AssertEquals(0, patternMatchingName.Count);
		}

		protected override void AssertPatternMatchingForNonSubscribedColumns(BusinessObjectFactory factory, GlbPerson bizO)
		{
			var patternMatchingName = factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingName = patternMatchingName.Single();

			AssertEquals(1, patternMatchingName.Count);
			AssertEquals(SubscriberTestUtilities.GetHash("ALBERT CHENG"), firstPatternMatchingName.PMN_HashedValue);
		}

		void AssertPatternMatchingRecords(BusinessObjectFactory factory, GlbPerson bizO, string fullName)
		{
			var patternMatchingName = factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingName = patternMatchingName.Single();

			var standardName = TextStandardizerHelper.StandardizePersonName(fullName);
			var hashedValue = TextStandardizerHelper.ComputeStringHashFast(standardName);

			AssertEquals(1, patternMatchingName.Count);
			AssertEquals(hashedValue, firstPatternMatchingName.PMN_HashedValue);
			AssertEquals(bizO.PK, firstPatternMatchingName.PMN_PER);
			AssertEquals(GlbPersonSchema.Constants.Prefix, firstPatternMatchingName.PMN_ParentTableCode);
			AssertEquals(bizO.PK, firstPatternMatchingName.PMN_ParentId);
			AssertEquals(bizO.PER_RN_NKCountry, firstPatternMatchingName.PMN_RN_NKCountryCode);
		}

		protected override BetterLogForTest GetLogForUpdateNameToSingleWordRecords(GlbPerson bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "Single"));
		}

		protected override string GetUpdateRecordNameToSingleWordCDCInsertQuery(GlbPerson bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, "Single", "M");
		}

		protected override void SetNameToSingleWord(GlbPerson bizO)
		{
			bizO.PER_FullName = "Single";
		}

		protected override void SetNameToMultiWords(GlbPerson bizO)
		{
			bizO.PER_FullName = "Albert Cheng";
		}

		protected override void AssertQUEResultRecordExists(BusinessObjectFactory factory, GlbPerson bizO)
		{
			AssertQueuedRecordExists(factory, bizO.PK, GlbPersonSchema.Constants.Prefix);
		}

		protected override GlbPerson CreateDummyContact(BusinessObjectFactory factory)
		{
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			glbPerson.PER_FullName = "DUMMY CONTACT TO SUPPRESS DOCS";

			factory.Save();
			return glbPerson;
		}

		public override void TestCustomFilter()
		{
			var subscriber = new GlbPersonNameSubscriber();

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
				row[GlbPersonSchema.PER_FullName.Name] = "DUMMY CONTACT TO SUPPRESS DOCS";
			}
			else
			{
				row[GlbPersonSchema.PER_FullName.Name] = "Name";
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { GlbPersonSchema.PER_FullName };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}

