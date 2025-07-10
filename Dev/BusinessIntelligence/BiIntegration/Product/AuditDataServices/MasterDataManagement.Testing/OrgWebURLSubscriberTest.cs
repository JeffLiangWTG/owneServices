
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
	[TestedType(typeof(OrgWebURLSubscriber))]
	class OrgWebURLSubscriberTest : PatternMatchingBaseSubscriberTest<OrgWebURL, OrgWebURLSubscriber>
	{
		protected override string GetGenericQuery
		{
			get { return @"INSERT [dbo].[OrgWebURL] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PU_PK, PU_OH, PU_URL, PU_Description) VALUES 
					(0x10, 0x01, {0}, 0x0, 1803, 1, '{1}', '{2}', '{3}','{4}');"; }
		}

		protected override string GetLsnMappingInsertQuery() => "INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15');";

		protected override string GetCdcMappingInsertQuery() => "INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgWebURL', 1, 1803);";

		protected override string GetLog
		{
			get { return "Queued Deduplication in OrgWebURLSubscriber for {0}. Row: {1} - {2} - {3}"; }
		}

		protected override OrgWebURL CreateParentRecords(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);

			var orgWebURL = orgHeader.OrgWebURLs.AddNew();
			orgWebURL.PU_URL = "www.HouseArryn.com.au";
			orgWebURL.PU_OH = orgHeader.PK;
			orgWebURL.PU_Description = "As High As Honour";

			return orgWebURL;
		}

		protected override OrgWebURL CreatePlaceholderRecords(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);

			var orgWebURL = orgHeader.OrgWebURLs.AddNew();
			orgWebURL.PU_URL = "test";
			orgWebURL.PU_OH = orgHeader.PK;
			orgWebURL.PU_Description = "Description";

			return orgWebURL;
		}

		protected override void CreatePatternMatchingRecords(BusinessObjectFactory factory, OrgWebURL bizO)
		{
			SubscriberTestUtilities.CreatePatternMatchingBusinessObject<PatternMatchingDomain>(factory, bizO.Header.PK, SubscriberTestUtilities.GetHash("HOUSEARRYN.COM.AU"), OrgWebURLSchema.Constants.Prefix, bizO.PK, bizO.Header.CountryCode);
		}

		protected override string GetNewRecordCDCInsertQuery(OrgWebURL bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, 2, bizO.PK, bizO.Header.PK, bizO.PU_URL, bizO.PU_Description);
		}

		protected override string GetOriginalRecordCDCInsertQuery(OrgWebURL bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, 3, bizO.PK, bizO.Header.PK, bizO.PU_URL, bizO.PU_Description);
		}

		protected override string GetUpdateRecordCDCInsertQuery(OrgWebURL bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, 4, bizO.PK, bizO.Header.PK, "www.HaveAtThee.com.au", bizO.PU_Description);
		}

		protected override string GetNonSubscribedColumnsCDCInsertQuery(OrgWebURL bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, 4, bizO.PK, bizO.Header.PK, bizO.PU_URL, "A New Description");
		}

		protected override string GetUpdateRecordToPlaceHolderCDCInsertQuery(OrgWebURL bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, 4, bizO.PK, bizO.Header.PK, "test", bizO.PU_Description);
		}

		protected override BetterLogForTest GetLogForNewRecords(OrgWebURL bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Added", bizO.PK, bizO.PU_URL));
		}

		protected override BetterLogForTest GetLogForExistingRecords(OrgWebURL bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "www.HaveAtThee.com.au"));
		}

		protected override BetterLogForTest GetLogForDeletedRecords(OrgWebURL bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "test"));
		}

		protected override string GetPatternMasters(OrgWebURL bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, "[OH_PK:{0}]", bizO.Header.PK);
		}

		protected override void AssertPatternMatchingNewRecordsPrecondition(BusinessObjectFactory factory, OrgWebURL bizO)
		{
			var patternMatchingDomain = factory.Load<PatternMatchingDomain>(new ZQuery(PatternMatchingDomainSchema.PMD_ParentId, bizO.PK)).ToList();

			AssertEquals(0, patternMatchingDomain.Count);
		}

		void AssertPatternMatchingRecords(BusinessObjectFactory factory, OrgWebURL bizO, string orgWebURL)
		{
			var domain = TextStandardizerHelper.ExtractEmailDomain(orgWebURL);
			var domainHashedValue = TextStandardizerHelper.ComputeStringHashFast(domain);

			var patternMatchingDomain = factory.Load<PatternMatchingDomain>(new ZQuery(PatternMatchingDomainSchema.PMD_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingDomain = patternMatchingDomain.Single();

			AssertEquals(1, patternMatchingDomain.Count);
			AssertEquals(domainHashedValue, firstPatternMatchingDomain.PMD_HashedValue);
			AssertEquals(bizO.Header.PK, firstPatternMatchingDomain.PMD_OH);
			AssertEquals(OrgWebURLSchema.Constants.Prefix, firstPatternMatchingDomain.PMD_ParentTableCode);
			AssertEquals(bizO.PK, firstPatternMatchingDomain.PMD_ParentId);
			AssertEquals(bizO.Header.CountryCode, firstPatternMatchingDomain.PMD_RN_NKCountryCode);
		}

		protected override void AssertPatternMatchingForNewRecords(BusinessObjectFactory factory, OrgWebURL bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, bizO.PU_URL);
		}

		protected override void AssertPatternMatchingForExistingRecords(BusinessObjectFactory factory, OrgWebURL bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, "www.HaveAtThee.com.au");
		}

		protected override void AssertNoPatternMatchingForPlaceholderRecords(BusinessObjectFactory factory, OrgWebURL bizO)
		{
			var patternMatchingDomain = factory.Load<PatternMatchingDomain>(new ZQuery(PatternMatchingDomainSchema.PMD_ParentId, bizO.PK)).ToList();
			AssertEquals(0, patternMatchingDomain.Count);
		}

		protected override void AssertPatternMatchingForNonSubscribedColumns(BusinessObjectFactory factory, OrgWebURL bizO)
		{
			var patternMatchingDomain = factory.Load<PatternMatchingDomain>(new ZQuery(PatternMatchingDomainSchema.PMD_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingDomain = patternMatchingDomain.Single();

			AssertEquals(1, patternMatchingDomain.Count);
			AssertEquals(SubscriberTestUtilities.GetHash("HOUSEARRYN.COM.AU"), firstPatternMatchingDomain.PMD_HashedValue);
		}

		protected override void AssertQUEResultRecordExists(BusinessObjectFactory factory, OrgWebURL bizO)
		{
			AssertQueuedRecordExists(factory, bizO.PU_OH, OrgHeaderSchema.Constants.Prefix);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new OrgWebURLSubscriber();

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
				row[OrgWebURLSchema.PU_URL.Name] = DBNull.Value;
			}
			else
			{
				row[OrgWebURLSchema.PU_URL.Name] = "URL";
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { OrgWebURLSchema.PU_URL };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
