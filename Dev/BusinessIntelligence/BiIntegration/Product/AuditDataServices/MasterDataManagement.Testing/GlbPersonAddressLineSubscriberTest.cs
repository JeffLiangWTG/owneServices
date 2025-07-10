using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.AuditDataServices.MDM.Subscribers;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.MDM.Testing
{
	[TestedType(typeof(GlbPersonAddressLineSubscriber))]
	class GlbPersonAddressLineSubscriberTest : PatternMatchingBaseSubscriberTest<GlbPerson, GlbPersonAddressLineSubscriber>
	{
		protected override string GetGenericQuery => @"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_HomeAddress1, PER_Gender) VALUES (0x10, 0x01, {0}, 0x0, 1803, 1, '{1}', '{2}', '{3}', '{4}');";

		protected override string GetLsnMappingInsertQuery() => "INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-20');";

		protected override string GetCdcMappingInsertQuery() => "INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbPerson', 1, 1803);";

		protected override string GetLog => "Queued Deduplication in GlbPersonAddressLineSubscriber for {0}. Row: {1} - {2} - {3} -  -  -  - ";

		protected override GlbPerson CreateParentRecords(BusinessObjectFactory factory)
		{
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			glbPerson.PER_HomeAddress1 = "1 thisisnotaplaceholder avenue";

			return glbPerson;
		}

		protected override GlbPerson CreatePlaceholderRecords(BusinessObjectFactory factory)
		{
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			glbPerson.PER_HomeAddress1 = "test";

			return glbPerson;
		}

		protected override void CreatePatternMatchingRecords(BusinessObjectFactory factory, GlbPerson bizO)
		{
			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingAddress>(factory, bizO.PK, GetUpperValueToHash(bizO.PER_HomeAddress1), GlbPersonSchema.Constants.Prefix, bizO.PK, bizO.PER_RN_NKCountry);
		}

		protected override string GetNewRecordCDCInsertQuery(GlbPerson bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "2", bizO.PK, bizO.PER_FullName, bizO.PER_HomeAddress1, "M");
		}

		protected override string GetOriginalRecordCDCInsertQuery(GlbPerson bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "3", bizO.PK, bizO.PER_FullName, bizO.PER_HomeAddress1, "M");
		}

		protected override string GetUpdateRecordCDCInsertQuery(GlbPerson bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.PER_FullName, "2 thisisnotaplaceholder avenue", "M");
		}

		protected override string GetNonSubscribedColumnsCDCInsertQuery(GlbPerson bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.PER_FullName, bizO.PER_HomeAddress1, "F");
		}

		protected override string GetUpdateRecordToPlaceHolderCDCInsertQuery(GlbPerson bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.PER_FullName, "test", "M");
		}

		protected override BetterLogForTest GetLogForNewRecords(GlbPerson bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Added", bizO.PK, bizO.PER_HomeAddress1));
		}

		protected override BetterLogForTest GetLogForExistingRecords(GlbPerson bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "2 thisisnotaplaceholder avenue"));
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
			var patternMatchingAddress = factory.Load<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, bizO.PK));
			AssertEquals(0, patternMatchingAddress.Length);
		}

		protected override void AssertPatternMatchingForNewRecords(BusinessObjectFactory factory, GlbPerson bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, bizO.PER_HomeAddress1);
		}

		protected override void AssertPatternMatchingForExistingRecords(BusinessObjectFactory factory, GlbPerson bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, "2 thisisnotaplaceholder avenue");
		}

		protected override void AssertNoPatternMatchingForPlaceholderRecords(BusinessObjectFactory factory, GlbPerson bizO)
		{
			var patternMatchingAddress = factory.Load<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, bizO.PK)).ToList();
			AssertEquals(0, patternMatchingAddress.Count);
		}

		protected override void AssertPatternMatchingForNonSubscribedColumns(BusinessObjectFactory factory, GlbPerson bizO)
		{
			var patternMatchingAddress = factory.Load<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingAddress = patternMatchingAddress.Single();

			AssertEquals(1, patternMatchingAddress.Count);
			AssertEquals(GetUpperValueToHash(bizO.PER_HomeAddress1), firstPatternMatchingAddress.PMA_HashedValue);
		}

		void AssertPatternMatchingRecords(BusinessObjectFactory factory, GlbPerson bizO, string address1)
		{
			var patternMatchingAddress = factory.Load<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingAddress = patternMatchingAddress.Single();
			var stringToHash = address1 + bizO.PER_HomeAddress2 + bizO.PER_City + bizO.PER_Postcode + bizO.PER_State;
			var hashedValue = GetUpperValueToHash(stringToHash);

			AssertEquals(1, patternMatchingAddress.Count);
			AssertEquals(hashedValue, firstPatternMatchingAddress.PMA_HashedValue);
			AssertEquals(bizO.PK, firstPatternMatchingAddress.PMA_PER);
			AssertEquals(GlbPersonSchema.Constants.Prefix, firstPatternMatchingAddress.PMA_ParentTableCode);
			AssertEquals(bizO.PK, firstPatternMatchingAddress.PMA_ParentId);
			AssertEquals(bizO.PER_RN_NKCountry, firstPatternMatchingAddress.PMA_RN_NKCountryCode);
		}

		protected override void AssertQUEResultRecordExists(BusinessObjectFactory factory, GlbPerson bizO)
		{
			AssertQueuedRecordExists(factory, bizO.PK, GlbPersonSchema.Constants.Prefix);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new GlbPersonAddressLineSubscriber();

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
				row[GlbPersonSchema.PER_HomeAddress1.Name] = DBNull.Value;
			}
			else
			{
				row[GlbPersonSchema.PER_HomeAddress1.Name] = "Address";
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { GlbPersonSchema.PER_HomeAddress1 };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
