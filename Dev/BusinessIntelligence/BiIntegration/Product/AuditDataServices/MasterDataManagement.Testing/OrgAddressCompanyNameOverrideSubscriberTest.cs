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
	[TestedType(typeof(OrgAddressCompanyNameOverrideSubscriber))]
	class OrgAddressCompanyNameOverrideSubscriberTest : PatternMatchingBaseSubscriberTest<OrgAddress, OrgAddressCompanyNameOverrideSubscriber>
	{
		const string OverrideCompanyName = "Override Company Name";

		protected override OrgAddress CreateParentRecords(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);

			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "1 test avenue";
			orgAddress.OA_CompanyNameOverride = OverrideCompanyName;
			orgAddress.OA_Language = Enterprise.Core.SharedConstants.Languages.English;

			return orgAddress;
		}

		protected override OrgAddress CreatePlaceholderRecords(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);

			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "1 test avenue";
			orgAddress.OA_CompanyNameOverride = OverrideCompanyName;
			orgAddress.OA_Language = Enterprise.Core.SharedConstants.Languages.English;

			return orgAddress;
		}

		protected override void CreatePatternMatchingRecords(BusinessObjectFactory factory, OrgAddress bizO)
		{
			SubscriberTestUtilities.CreatePatternMatchingBusinessObject<PatternMatchingName>(factory, bizO.Header.PK, SubscriberTestUtilities.GetHash(TextStandardizerHelper.StandardizeCompanyName(OverrideCompanyName, bizO.Header.CountryCode)), OrgAddressSchema.Constants.Prefix, bizO.PK, bizO.Header.CountryCode);
		}

		protected override string GetGenericQuery
		{
			get { return @"INSERT [dbo].[OrgAddress] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OA_PK, OA_OH, OA_Address1, OA_CompanyNameOverride, OA_Language) VALUES 
				(0x10, 0x01, {0}, 0x0, 1803, 1, '{1}', '{2}', '{3}', '{4}', '{5}');"; }
		}

		protected override string GetLsnMappingInsertQuery() => "INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15');";

		protected override string GetCdcMappingInsertQuery() => "INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgAddress', 1, 1803);";

		protected override string GetNewRecordCDCInsertQuery(OrgAddress bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "2", bizO.PK, bizO.Header.PK, bizO.OA_Address1, bizO.OA_CompanyNameOverride, bizO.OA_Language);
		}

		protected override string GetOriginalRecordCDCInsertQuery(OrgAddress bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "3", bizO.PK, bizO.Header.PK, bizO.OA_Address1, bizO.OA_CompanyNameOverride, bizO.OA_Language);
		}

		protected override string GetUpdateRecordCDCInsertQuery(OrgAddress bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.Header.PK, bizO.OA_Address1, "Different Company Name", bizO.OA_Language);
		}

		protected override string GetNonSubscribedColumnsCDCInsertQuery(OrgAddress bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.Header.PK, bizO.OA_Address1, bizO.OA_CompanyNameOverride, "ABC");
		}

		protected override string GetUpdateRecordToPlaceHolderCDCInsertQuery(OrgAddress bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.Header.PK, bizO.OA_Address1, "Different Company Name", bizO.OA_Language);
		}

		protected override string GetLog
		{
			get { return "Queued Deduplication in OrgAddressCompanyNameOverrideSubscriber for {0}. Row: {1} - {2} - {3}"; }
		}

		protected override BetterLogForTest GetLogForNewRecords(OrgAddress bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Added", bizO.PK, bizO.OA_CompanyNameOverride));
		}

		protected override BetterLogForTest GetLogForExistingRecords(OrgAddress bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "Different Company Name"));
		}

		protected override BetterLogForTest GetLogForDeletedRecords(OrgAddress bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "Different Company Name"));
		}

		protected override string GetPatternMasters(OrgAddress bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, "[OH_PK:{0}]", bizO.Header.PK);
		}

		protected override void AssertPatternMatchingNewRecordsPrecondition(BusinessObjectFactory factory, OrgAddress bizO)
		{
			var patternMatchingName = factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK)).ToList();

			AssertEquals(0, patternMatchingName.Count);
		}

		void AssertPatternMatchingRecords(BusinessObjectFactory factory, OrgAddress bizO, string orgCompanyNameOverride)
		{
			var patternMatchingName = factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingName = patternMatchingName.Single();

			var standardisedStringToHash = TextStandardizerHelper.StandardizeCompanyName(orgCompanyNameOverride, bizO.Header.CountryCode);
			var hashedValue = TextStandardizerHelper.ComputeStringHashFast(standardisedStringToHash);

			AssertEquals(1, patternMatchingName.Count);
			AssertEquals(hashedValue, firstPatternMatchingName.PMN_HashedValue);
			AssertEquals(bizO.Header.PK, firstPatternMatchingName.PMN_OH);
			AssertEquals(OrgAddressSchema.Constants.Prefix, firstPatternMatchingName.PMN_ParentTableCode);
			AssertEquals(bizO.PK, firstPatternMatchingName.PMN_ParentId);
			AssertEquals(bizO.Header.CountryCode, firstPatternMatchingName.PMN_RN_NKCountryCode);
		}

		protected override void AssertPatternMatchingForNewRecords(BusinessObjectFactory factory, OrgAddress bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, bizO.OA_CompanyNameOverride);
		}

		protected override void AssertPatternMatchingForExistingRecords(BusinessObjectFactory factory, OrgAddress bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, "Different Company Name");
		}

		protected override void AssertNoPatternMatchingForPlaceholderRecords(BusinessObjectFactory factory, OrgAddress bizO)
		{
			// company names don't remove placeholders
			Assert(true);
		}

		protected override void AssertPatternMatchingForNonSubscribedColumns(BusinessObjectFactory factory, OrgAddress bizO)
		{
			var patternMatchingName = factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingName = patternMatchingName.Single();

			AssertEquals(1, patternMatchingName.Count);
			AssertEquals(SubscriberTestUtilities.GetHash(TextStandardizerHelper.StandardizeCompanyName(OverrideCompanyName, bizO.Header.CountryCode)), firstPatternMatchingName.PMN_HashedValue);
		}

		protected override void AssertQUEResultRecordExists(BusinessObjectFactory factory, OrgAddress bizO)
		{
			AssertQueuedRecordExists(factory, bizO.OA_OH, OrgHeaderSchema.Constants.Prefix);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new OrgAddressCompanyNameOverrideSubscriber();

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
				row[OrgAddressSchema.OA_CompanyNameOverride.Name] = DBNull.Value;
			}
			else
			{
				row[OrgAddressSchema.OA_CompanyNameOverride.Name] = "Name";
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { OrgAddressSchema.OA_CompanyNameOverride };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
