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
	[TestedType(typeof(GlbStaffAddressLineSubscriber))]
	class GlbStaffAddressLineCountryCodeSubscriberTest : PatternMatchingCountryCodeSubscriberTest<GlbStaff, GlbStaffAddressLineSubscriber>
	{
		protected override string GetLog => "Queued Deduplication in GlbStaffAddressLineSubscriber for {0}. Row: {1} - {2} - {3} -  -  -  - ";

		protected override string GetGenericQuery => @"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_UserAddress1, GS_RN_NKCountryCode) VALUES (0x10, 0x01, {0}, 0x0, 1803, 1, '{1}', '{2}', '{3}', '{4}', '{5}');";

		protected override void AssertPatternMatchingCountryCodePrecondition(BusinessObjectFactory factory, GlbStaff bizO)
		{
			var patternMatchingAddress = factory.LoadTop1<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, bizO.PK));
			var patternMatchingName = factory.LoadTop1<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK));

			AssertNotNull(patternMatchingAddress);
			AssertNotNull(patternMatchingName);
			AssertEquals("GB", patternMatchingAddress.PMA_RN_NKCountryCode);
			AssertEquals("GB", patternMatchingName.PMN_RN_NKCountryCode);
		}

		protected override void AssertPatternMatchingForExistingRecords(BusinessObjectFactory factory, GlbStaff bizO)
		{
			var patternMatchingAddress = factory.LoadTop1<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, bizO.PK));
			var patternMatchingName = factory.LoadTop1<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK));

			AssertNotNull(patternMatchingAddress);
			AssertNotNull(patternMatchingName);
			AssertEquals("NZ", patternMatchingAddress.PMA_RN_NKCountryCode);
			AssertEquals("NZ", patternMatchingName.PMN_RN_NKCountryCode);
		}

		protected override GlbStaff CreateParentRecords(BusinessObjectFactory factory)
		{
			var person = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_Code = "VQU";
			staff.GS_PER = person.PK;
			staff.GS_UserAddress1 = "4 Thompson Street";
			staff.GS_RN_NKCountryCode = "GB";
			person.UpdateFromStaff(staff);

			return staff;
		}

		protected override void CreatePatternMatchingRecords(BusinessObjectFactory factory, GlbStaff bizO)
		{
			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingAddress>(factory, bizO.Person.PK, 0, HRJobApplicantSchema.Constants.Prefix, bizO.PK, "GB");
			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingName>(factory, bizO.Person.PK, 0, HRJobApplicantSchema.Constants.Prefix, bizO.PK, "GB");
		}

		protected override BetterLogForTest GetLogForExistingRecords(GlbStaff bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "4 Thompson Street"));
		}

		protected override string GetPatternMasters(GlbStaff bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, "[PER_PK:{0}]", bizO.Person.PK);
		}

		protected override string GetLsnMappingInsertQuery()
		{
			return "INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08');";
		}

		protected override string GetCDCHistoryInsertQuery() => "INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbStaff', 1, 1803);";

		protected override string GetOriginalRecordCDCInsertQuery(GlbStaff bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "3", bizO.PK, bizO.GS_Code, bizO.GS_PER, bizO.GS_UserAddress1, bizO.GS_RN_NKCountryCode);
		}

		protected override string GetUpdateRecordCDCInsertQuery(GlbStaff bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.GS_Code, bizO.GS_PER, bizO.GS_UserAddress1, "NZ");
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
				row[GlbStaffSchema.GS_UserAddress1.Name] = "Address";
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
