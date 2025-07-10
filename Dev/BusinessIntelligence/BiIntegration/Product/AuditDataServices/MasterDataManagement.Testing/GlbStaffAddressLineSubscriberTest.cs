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
	[TestedType(typeof(GlbStaffAddressLineSubscriber))]
	class GlbStaffAddressLineSubscriberTest : GlbStaffSubscriberTest<GlbStaffAddressLineSubscriber>
	{
		protected override string GetGenericQuery => @"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_UserAddress1, GS_ResourceType) VALUES (0x10, 0x01, {0}, 0x0, 1803, 1, '{1}', '{2}', '{3}', '{4}', '{5}');";

		protected override string GetCdcMappingInsertQuery() => "INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbStaff', 1, 1803);";

		protected override string GetLog => "Queued Deduplication in GlbStaffAddressLineSubscriber for {0}. Row: {1} - {2} - {3} -  -  -  - ";

		protected override void AssertQUEResultRecordExists(BusinessObjectFactory factory, GlbStaff bizO)
		{
			AssertQueuedRecordExists(factory, bizO.GS_PER, GlbPersonSchema.Constants.Prefix);
		}

		protected override void AssertNoPatternMatchingForPlaceholderRecords(BusinessObjectFactory factory, GlbStaff bizO)
		{
			var patternMatchingAddress = factory.Load<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, bizO.PK)).ToList();
			AssertEquals(0, patternMatchingAddress.Count);
		}

		protected override void AssertPatternMatchingForExistingRecords(BusinessObjectFactory factory, GlbStaff bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, "84 Thompson Street");
		}

		protected override void AssertPatternMatchingForNewRecords(BusinessObjectFactory factory, GlbStaff bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, bizO.GS_UserAddress1);
		}

		protected override void AssertPatternMatchingForNonSubscribedColumns(BusinessObjectFactory factory, GlbStaff bizO)
		{
			var patternMatchingAddress = factory.Load<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingAddress = patternMatchingAddress.Single();

			AssertEquals(1, patternMatchingAddress.Count);
			AssertEquals(GetUpperValueToHash(bizO.GS_UserAddress1), firstPatternMatchingAddress.PMA_HashedValue);
		}

		protected override void AssertPatternMatchingNewRecordsPrecondition(BusinessObjectFactory factory, GlbStaff bizO)
		{
			var patternMatchingAddress = factory.Load<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, bizO.PK));
			AssertEquals(0, patternMatchingAddress.Length);
		}

		protected override GlbStaff CreateParentRecords(BusinessObjectFactory factory)
		{
			var person = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_Code = "QAA";
			staff.GS_PER = person.PK;
			staff.GS_UserAddress1 = "4 Thompson Street";
			person.UpdateFromStaff(staff);

			return staff;
		}

		protected override void CreatePatternMatchingRecords(BusinessObjectFactory factory, GlbStaff bizO)
		{
			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingAddress>(factory, bizO.GS_PER, GetUpperValueToHash(bizO.GS_UserAddress1), GlbStaffSchema.Constants.Prefix, bizO.PK, bizO.Person.PER_RN_NKCountry);
		}

		protected override GlbStaff CreatePlaceholderRecords(BusinessObjectFactory factory)
		{
			var person = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "QQW";
			staff.GS_PER = person.PK;
			staff.GS_UserAddress1 = "test";
			person.UpdateFromStaff(staff);

			return staff;
		}

		protected override BetterLogForTest GetLogForDeletedRecords(GlbStaff bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "test"));
		}

		protected override BetterLogForTest GetLogForExistingRecords(GlbStaff bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "84 Thompson Street"));
		}

		protected override BetterLogForTest GetLogForNewRecords(GlbStaff bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Added", bizO.PK, bizO.GS_UserAddress1));
		}

		protected override string GetPatternMasters(GlbStaff bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, "[PER_PK:{0}]", bizO.Person.PK);
		}

		protected override string GetLsnMappingInsertQuery()
		{
			return "INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08');";
		}

		protected override string GetNewRecordCDCInsertQuery(GlbStaff bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "2", bizO.PK, bizO.GS_Code, bizO.GS_PER, bizO.GS_UserAddress1, bizO.GS_ResourceType);
		}

		protected override string GetNonSubscribedColumnsCDCInsertQuery(GlbStaff bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.GS_Code, bizO.GS_PER, bizO.GS_UserAddress1, bizO.GS_ResourceType);
		}

		protected override string GetOriginalRecordCDCInsertQuery(GlbStaff bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "3", bizO.PK, bizO.GS_Code, bizO.GS_PER, bizO.GS_UserAddress1, bizO.GS_ResourceType);
		}

		protected override string GetUpdateRecordCDCInsertQuery(GlbStaff bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.GS_Code, bizO.GS_PER, "84 Thompson Street", bizO.GS_ResourceType);
		}

		protected override string GetUpdateRecordToPlaceHolderCDCInsertQuery(GlbStaff bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.GS_Code, bizO.GS_PER, "test", bizO.GS_ResourceType);
		}

		void AssertPatternMatchingRecords(BusinessObjectFactory factory, GlbStaff bizO, string address1)
		{
			var patternMatchingAddress = factory.Load<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingAddress = patternMatchingAddress.Single();
			var stringToHash = address1 + bizO.GS_UserAddress2 + bizO.GS_City + bizO.GS_Postcode + bizO.GS_State;
			var hashedValue = GetUpperValueToHash(stringToHash);

			AssertEquals(1, patternMatchingAddress.Count);
			AssertEquals(hashedValue, firstPatternMatchingAddress.PMA_HashedValue);
			AssertEquals(bizO.Person.PK, firstPatternMatchingAddress.PMA_PER);
			AssertEquals(GlbStaffSchema.Constants.Prefix, firstPatternMatchingAddress.PMA_ParentTableCode);
			AssertEquals(bizO.PK, firstPatternMatchingAddress.PMA_ParentId);
			AssertEquals(bizO.Person.PER_RN_NKCountry, firstPatternMatchingAddress.PMA_RN_NKCountryCode);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new GlbStaffAddressLineSubscriber();

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
				row[GlbStaffSchema.GS_UserAddress1.Name] = DBNull.Value;
			}
			else
			{
				row[GlbStaffSchema.GS_UserAddress1.Name] = "Name";
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { GlbStaffSchema.GS_UserAddress1 };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
