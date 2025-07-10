using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.AuditDataServices.MDM.Subscribers;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.MDM.Testing
{
	[TestedType(typeof(OrgBrandOrRelatedNameSubscriber))]
	class OrgBrandOrRelatedNameSubscriberTest : PatternMatchingBaseSubscriberTest<OrgBrandOrRelatedName, OrgBrandOrRelatedNameSubscriber>
	{
		protected override string GetGenericQuery
		{
			get { return @"INSERT[dbo].[OrgBrandOrRelatedName]([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id],
						 P1_PK, P1_OH, P1_RelatedName, P1_IsValid) VALUES (0x10, 0x01, {0}, 0x0, 1803, 1, '{1}', '{2}','{3}',{4});"; }
		}

		protected override string GetLsnMappingInsertQuery() => "INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15');";

		protected override string GetCdcMappingInsertQuery() => "INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgBrandOrRelatedName', 1, 1803);";

		protected override string GetLog => "Queued Deduplication in OrgBrandOrRelatedNameSubscriber for {0}. Row: {1} - {2} - {3}";

		protected override OrgBrandOrRelatedName CreateParentRecords(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);

			var orgBrandOrRelatedName = orgHeader.BrandsOrRelatedNames.AddNew();
			orgBrandOrRelatedName.P1_OH = orgHeader.PK;
			orgBrandOrRelatedName.P1_RelatedName = "Test name";

			return orgBrandOrRelatedName;
		}

		protected override OrgBrandOrRelatedName CreatePlaceholderRecords(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);

			var orgBrandOrRelatedName = orgHeader.BrandsOrRelatedNames.AddNew();
			orgBrandOrRelatedName.P1_OH = orgHeader.PK;
			orgBrandOrRelatedName.P1_RelatedName = "test";

			return orgBrandOrRelatedName;
		}

		protected override void CreatePatternMatchingRecords(BusinessObjectFactory factory, OrgBrandOrRelatedName bizO)
		{
			SubscriberTestUtilities.CreatePatternMatchingBusinessObject<PatternMatchingName>(factory, bizO.P1_OH, SubscriberTestUtilities.GetHash("TEST NAME"), OrgBrandOrRelatedNameSchema.Constants.Prefix, bizO.PK, bizO.Header.CountryCode);
		}

		protected override string GetNewRecordCDCInsertQuery(OrgBrandOrRelatedName bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "2", bizO.PK, bizO.P1_OH, bizO.P1_RelatedName, 1);
		}

		protected override string GetOriginalRecordCDCInsertQuery(OrgBrandOrRelatedName bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "3", bizO.PK, bizO.P1_OH, bizO.P1_RelatedName, 1);
		}

		protected override string GetUpdateRecordCDCInsertQuery(OrgBrandOrRelatedName bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.P1_OH, "Different name", 1);
		}

		protected override string GetNonSubscribedColumnsCDCInsertQuery(OrgBrandOrRelatedName bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.P1_OH, bizO.P1_RelatedName, 0);
		}

		protected override string GetUpdateRecordToPlaceHolderCDCInsertQuery(OrgBrandOrRelatedName bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.P1_OH, "test", 1);
		}

		protected override BetterLogForTest GetLogForNewRecords(OrgBrandOrRelatedName bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO),"Added",bizO.PK, bizO.P1_RelatedName));
		}

		protected override BetterLogForTest GetLogForExistingRecords(OrgBrandOrRelatedName bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO),"Modified", bizO.PK, "Different name"));
		}

		protected override BetterLogForTest GetLogForDeletedRecords(OrgBrandOrRelatedName bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "test"));
		}

		protected override string GetPatternMasters(OrgBrandOrRelatedName bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, "[OH_PK:{0}]", bizO.Header.PK);
		}

		protected override void AssertPatternMatchingNewRecordsPrecondition(BusinessObjectFactory factory, OrgBrandOrRelatedName bizO)
		{
			var patternMatchingName = factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK));
			AssertEquals(0, patternMatchingName.Length);
		}

		void AssertPatternMatchingRecords(BusinessObjectFactory factory, OrgBrandOrRelatedName bizO, string orgBrandOrRelatedNameRelatedName)
		{
			var patternMatchingName = factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingName = patternMatchingName.Single();

			var standardName = TextStandardizerHelper.StandardizeCompanyName(orgBrandOrRelatedNameRelatedName, bizO.Header.CountryCode);
			var hashedValue = TextStandardizerHelper.ComputeStringHashFast(standardName);

			AssertEquals(1, patternMatchingName.Count);
			AssertEquals(hashedValue, firstPatternMatchingName.PMN_HashedValue);
			AssertEquals(bizO.Header.PK, firstPatternMatchingName.PMN_OH);
			AssertEquals(OrgBrandOrRelatedNameSchema.Constants.Prefix, firstPatternMatchingName.PMN_ParentTableCode);
			AssertEquals(bizO.PK, firstPatternMatchingName.PMN_ParentId);
			AssertEquals(bizO.Header.CountryCode, firstPatternMatchingName.PMN_RN_NKCountryCode);
		}

		protected override void AssertPatternMatchingForNewRecords(BusinessObjectFactory factory, OrgBrandOrRelatedName bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, bizO.P1_RelatedName);
		}

		protected override void AssertPatternMatchingForExistingRecords(BusinessObjectFactory factory, OrgBrandOrRelatedName bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, "Different name");
		}

		protected override void AssertNoPatternMatchingForPlaceholderRecords(BusinessObjectFactory factory, OrgBrandOrRelatedName bizO)
		{
			// company names don't remove placeholders
			Assert(true);
		}

		protected override void AssertPatternMatchingForNonSubscribedColumns(BusinessObjectFactory factory, OrgBrandOrRelatedName bizO)
		{
			var patternMatchingName = factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingName = patternMatchingName.Single();

			AssertEquals(1, patternMatchingName.Count);
			AssertEquals(SubscriberTestUtilities.GetHash("TEST NAME"), firstPatternMatchingName.PMN_HashedValue);
		}

		protected override void AssertQUEResultRecordExists(BusinessObjectFactory factory, OrgBrandOrRelatedName bizO)
		{
			AssertQueuedRecordExists(factory, bizO.P1_OH, OrgHeaderSchema.Constants.Prefix);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new OrgBrandOrRelatedNameSubscriber();

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
				row[OrgBrandOrRelatedNameSchema.P1_RelatedName.Name] = DBNull.Value;
			}
			else
			{
				row[OrgBrandOrRelatedNameSchema.P1_RelatedName.Name] = "Name";
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { OrgBrandOrRelatedNameSchema.P1_RelatedName };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
