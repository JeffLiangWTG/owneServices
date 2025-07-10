using System;
using System.Data;
using System.Globalization;
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
	class GlbPersonAddressLineCountryCodeSubscriberTest : PatternMatchingCountryCodeSubscriberTest<GlbPerson, GlbPersonAddressLineSubscriber>
	{
		protected override string GetLog => "Queued Deduplication in GlbPersonAddressLineSubscriber for {0}. Row: {1} - {2} - {3} -  -  -  - ";

		protected override string GetGenericQuery => @"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_HomeAddress1, PER_RN_NKCountry) VALUES (0x10, 0x01, {0}, 0x0, 1803, 1, '{1}', '{2}', '{3}', '{4}');";

		protected override void AssertPatternMatchingCountryCodePrecondition(BusinessObjectFactory factory, GlbPerson bizO)
		{
			var patternMatchingAddress = factory.LoadTop1<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, bizO.PK));
			var patternMatchingName = factory.LoadTop1<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK));

			AssertNotNull(patternMatchingAddress);
			AssertNotNull(patternMatchingName);
			AssertEquals("NZ", patternMatchingAddress.PMA_RN_NKCountryCode);
			AssertEquals("NZ", patternMatchingName.PMN_RN_NKCountryCode);
		}

		protected override void AssertPatternMatchingForExistingRecords(BusinessObjectFactory factory, GlbPerson bizO)
		{
			var patternMatchingAddress = factory.LoadTop1<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, bizO.PK));
			var patternMatchingName = factory.LoadTop1<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK));

			AssertNotNull(patternMatchingAddress);
			AssertNotNull(patternMatchingName);
			AssertEquals("US", patternMatchingAddress.PMA_RN_NKCountryCode);
			AssertEquals("US", patternMatchingName.PMN_RN_NKCountryCode);
		}

		protected override GlbPerson CreateParentRecords(BusinessObjectFactory factory)
		{
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			glbPerson.PER_RN_NKCountry = "NZ";
			glbPerson.PER_HomeAddress1 = "3 thisisnotaplaceholder avenue";
			return glbPerson;
		}

		protected override void CreatePatternMatchingRecords(BusinessObjectFactory factory, GlbPerson bizO)
		{
			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingAddress>(factory, bizO.PK, 0, GlbPersonSchema.Constants.Prefix, bizO.PK, "NZ");
			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingName>(factory, bizO.PK, 0, GlbPersonSchema.Constants.Prefix, bizO.PK, "NZ");
		}

		protected override BetterLogForTest GetLogForExistingRecords(GlbPerson bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "3 thisisnotaplaceholder avenue"));
		}

		protected override string GetPatternMasters(GlbPerson bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, "[PER_PK:{0}]", bizO.PK);
		}

		protected override string GetLsnMappingInsertQuery() => "INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08');";

		protected override string GetCDCHistoryInsertQuery() => "INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbPerson', 1, 1803);";

		protected override string GetOriginalRecordCDCInsertQuery(GlbPerson bizO) => string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "3", bizO.PK, bizO.PER_FullName, bizO.PER_HomeAddress1, bizO.PER_RN_NKCountry);

		protected override string GetUpdateRecordCDCInsertQuery(GlbPerson bizO) => string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.PER_FullName, bizO.PER_HomeAddress1, "US");

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
