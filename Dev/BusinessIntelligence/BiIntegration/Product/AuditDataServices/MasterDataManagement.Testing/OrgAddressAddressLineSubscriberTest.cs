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
	[TestedType(typeof(OrgAddressAddressLineSubscriber))]
	class OrgAddressAddressLineSubscriberTest : PatternMatchingBaseSubscriberTest<OrgAddress, OrgAddressAddressLineSubscriber>
	{
		protected override OrgAddress CreateParentRecords(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);

			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "1 thisisnotaplaceholder avenue";
			orgAddress.OA_Language = Enterprise.Core.SharedConstants.Languages.English;

			return orgAddress;
		}

		protected override OrgAddress CreatePlaceholderRecords(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);

			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "test";
			orgAddress.OA_Language = Enterprise.Core.SharedConstants.Languages.English;

			return orgAddress;
		}

		protected override void CreatePatternMatchingRecords(BusinessObjectFactory factory, OrgAddress bizO)
		{
			SubscriberTestUtilities.CreatePatternMatchingBusinessObject<PatternMatchingAddress>(factory, bizO.Header.PK, GetUpperValueToHash(bizO.OA_Address1), OrgAddressSchema.Constants.Prefix, bizO.PK, bizO.Header.CountryCode);
		}

		protected override string GetGenericQuery
		{
			get { return @"INSERT [dbo].[OrgAddress] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OA_PK, OA_OH, OA_Address1, OA_Language) VALUES 
				(0x10, 0x01, {0}, 0x0, 1803, 1, '{1}', '{2}', '{3}', '{4}');"; }
		}

		protected override string GetLsnMappingInsertQuery() => "INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15');";

		protected override string GetCdcMappingInsertQuery() => "INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgAddress', 1, 1803);";

		protected override string GetNewRecordCDCInsertQuery(OrgAddress bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "2", bizO.PK, bizO.Header.PK, bizO.OA_Address1, bizO.OA_Language);
		}

		protected override string GetOriginalRecordCDCInsertQuery(OrgAddress bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "3", bizO.PK, bizO.Header.PK, bizO.OA_Address1, bizO.OA_Language);
		}

		protected override string GetUpdateRecordCDCInsertQuery(OrgAddress bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.Header.PK, "2 thisisnotaplaceholder avenue", bizO.OA_Language);
		}

		protected override string GetNonSubscribedColumnsCDCInsertQuery(OrgAddress bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.Header.PK, bizO.OA_Address1, "ABC");
		}

		protected override string GetUpdateRecordToPlaceHolderCDCInsertQuery(OrgAddress bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.Header.PK, "test", bizO.OA_Language);
		}

		protected override string GetLog
		{
			get { return "Queued Deduplication in OrgAddressAddressLineSubscriber for {0}. Row: {1} - {2} - {3} -  -  -  - "; }
		}

		protected override BetterLogForTest GetLogForNewRecords(OrgAddress bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Added", bizO.PK, bizO.OA_Address1));
		}

		protected override BetterLogForTest GetLogForExistingRecords(OrgAddress bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "2 thisisnotaplaceholder avenue"));
		}

		protected override BetterLogForTest GetLogForDeletedRecords(OrgAddress bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "test"));
		}

		protected override string GetPatternMasters(OrgAddress bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, "[OH_PK:{0}]", bizO.Header.PK);
		}

		protected override void AssertPatternMatchingNewRecordsPrecondition(BusinessObjectFactory factory, OrgAddress bizO)
		{
			var patternMatchingAddress = factory.Load<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, bizO.PK)).ToList();

			AssertEquals(0, patternMatchingAddress.Count);
		}

		void AssertPatternMatchingRecords(BusinessObjectFactory factory, OrgAddress bizO, string orgAddress1)
		{
			var patternMatchingAddress = factory.Load<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingAddress = patternMatchingAddress.Single();
			var stringToHash = orgAddress1 + bizO.OA_Address2 + bizO.OA_City + bizO.OA_PostCode + bizO.OA_State;

			var hashedValue = GetUpperValueToHash(stringToHash);

			AssertEquals(1, patternMatchingAddress.Count);
			AssertEquals(hashedValue, firstPatternMatchingAddress.PMA_HashedValue);
			AssertEquals(bizO.Header.PK, firstPatternMatchingAddress.PMA_OH);
			AssertEquals(OrgAddressSchema.Constants.Prefix, firstPatternMatchingAddress.PMA_ParentTableCode);
			AssertEquals(bizO.PK, firstPatternMatchingAddress.PMA_ParentId);
			AssertEquals(bizO.Header.CountryCode, firstPatternMatchingAddress.PMA_RN_NKCountryCode);
		}

		protected override void AssertPatternMatchingForNewRecords(BusinessObjectFactory factory, OrgAddress bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, bizO.OA_Address1);
		}

		protected override void AssertPatternMatchingForExistingRecords(BusinessObjectFactory factory, OrgAddress bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, "2 thisisnotaplaceholder avenue");
		}

		protected override void AssertNoPatternMatchingForPlaceholderRecords(BusinessObjectFactory factory, OrgAddress bizO)
		{
			var patternMatchingAddress = factory.Load<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, bizO.PK)).ToList();
			AssertEquals(0, patternMatchingAddress.Count);
		}

		protected override void AssertPatternMatchingForNonSubscribedColumns(BusinessObjectFactory factory, OrgAddress bizO)
		{
			var patternMatchingAddress = factory.Load<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingAddress = patternMatchingAddress.Single();

			AssertEquals(1, patternMatchingAddress.Count);
			AssertEquals(GetUpperValueToHash(bizO.OA_Address1), firstPatternMatchingAddress.PMA_HashedValue);
		}

		protected override void AssertQUEResultRecordExists(BusinessObjectFactory factory, OrgAddress bizO)
		{
			AssertQueuedRecordExists(factory, bizO.OA_OH, OrgHeaderSchema.Constants.Prefix);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new OrgAddressAddressLineSubscriber();

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
				row[OrgAddressSchema.OA_Address1.Name] = DBNull.Value;
			}
			else
			{
				row[OrgAddressSchema.OA_Address1.Name] = "Name";
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { OrgAddressSchema.OA_Address1 };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
