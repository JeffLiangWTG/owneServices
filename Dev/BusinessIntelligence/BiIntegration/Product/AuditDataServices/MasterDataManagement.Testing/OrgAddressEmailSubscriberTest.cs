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
	[TestedType(typeof(OrgAddressEmailSubscriber))]
	class OrgAddressEmailSubscriberTest : PatternMatchingBaseSubscriberTest<OrgAddress, OrgAddressEmailSubscriber>
	{
		protected override OrgAddress CreateParentRecords(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);

			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "1 olriordan avenue";
			orgAddress.OA_Email = "Davos.Seaworth@wisetechglobal.com";
			orgAddress.OA_Language = Enterprise.Core.SharedConstants.Languages.English;

			return orgAddress;
		}

		protected override OrgAddress CreatePlaceholderRecords(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);

			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "test";
			orgAddress.OA_Email = "test@test.com";
			orgAddress.OA_Language = Enterprise.Core.SharedConstants.Languages.English;

			return orgAddress;
		}

		protected override void CreatePatternMatchingRecords(BusinessObjectFactory factory, OrgAddress bizO)
		{
			SubscriberTestUtilities.CreatePatternMatchingBusinessObject<PatternMatchingDomain>(factory, bizO.Header.PK, SubscriberTestUtilities.GetHash("WISETECHGLOBAL.COM"), OrgAddressSchema.Constants.Prefix, bizO.PK, bizO.Header.CountryCode);
			SubscriberTestUtilities.CreatePatternMatchingBusinessObject<PatternMatchingEmail>(factory, bizO.Header.PK, SubscriberTestUtilities.GetHash("DAVOS.SEAWORTH@WISETECHGLOBAL.COM"), OrgAddressSchema.Constants.Prefix, bizO.PK, bizO.Header.CountryCode);
		}

		protected override string GetGenericQuery
		{
			get { return @"INSERT [dbo].[OrgAddress] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OA_PK, OA_OH, OA_Address1, OA_Email, OA_Language) VALUES 
				(0x10, 0x01, {0}, 0x0, 1803, 1, '{1}', '{2}', '{3}', '{4}', '{5}');"; }
		}

		protected override string GetLsnMappingInsertQuery() => "INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15');";

		protected override string GetCdcMappingInsertQuery() => "INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgAddress', 1, 1803);";

		protected override string GetNewRecordCDCInsertQuery(OrgAddress bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "2", bizO.PK, bizO.Header.PK, bizO.OA_Address1, bizO.OA_Email, bizO.OA_Language);
		}

		protected override string GetOriginalRecordCDCInsertQuery(OrgAddress bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "3", bizO.PK, bizO.Header.PK, bizO.OA_Address1, bizO.OA_Email, bizO.OA_Language);
		}

		protected override string GetUpdateRecordCDCInsertQuery(OrgAddress bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.Header.PK, bizO.OA_Address1, "diiopnail@domawapppppop.com", bizO.OA_Language);
		}

		protected override string GetNonSubscribedColumnsCDCInsertQuery(OrgAddress bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.Header.PK, bizO.OA_Address1, bizO.OA_Email, "ABC");
		}

		protected override string GetUpdateRecordToPlaceHolderCDCInsertQuery(OrgAddress bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.Header.PK, bizO.OA_Address1, "test@test.com", bizO.OA_Language);
		}

		protected override string GetLog
		{
			get { return "Queued Deduplication in OrgAddressEmailSubscriber for {0}. Row: {1} - {2} - {3}"; }
		}

		protected override BetterLogForTest GetLogForNewRecords(OrgAddress bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Added", bizO.PK, bizO.OA_Email));
		}

		protected override BetterLogForTest GetLogForExistingRecords(OrgAddress bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "diiopnail@domawapppppop.com"));
		}

		protected override BetterLogForTest GetLogForDeletedRecords(OrgAddress bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "test@test.com"));
		}

		protected override string GetPatternMasters(OrgAddress bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, "[OH_PK:{0}]", bizO.Header.PK);
		}

		protected override void AssertPatternMatchingNewRecordsPrecondition(BusinessObjectFactory factory, OrgAddress bizO)
		{
			var patternMatchingDomain = factory.Load<PatternMatchingDomain>(new ZQuery(PatternMatchingDomainSchema.PMD_ParentId, bizO.PK)).ToList();
			var patternMatchingEmail = factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, bizO.PK)).ToList();

			AssertEquals(0, patternMatchingDomain.Count);
			AssertEquals(0, patternMatchingEmail.Count);
		}

		void AssertPatternMatchingRecords(BusinessObjectFactory factory, OrgAddress bizO, string orgEmail)
		{
			var domain = TextStandardizerHelper.ExtractEmailDomain(orgEmail);
			var domainHashedValue = TextStandardizerHelper.ComputeStringHashFast(domain);
			var standardisedEmail = TextStandardizerHelper.StandardizeEmail(orgEmail);
			var emailHashedValue = TextStandardizerHelper.ComputeStringHashFast(standardisedEmail);

			var patternMatchingDomain = factory.Load<PatternMatchingDomain>(new ZQuery(PatternMatchingDomainSchema.PMD_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingDomain = patternMatchingDomain.Single();

			AssertEquals(1, patternMatchingDomain.Count);
			AssertEquals(domainHashedValue, firstPatternMatchingDomain.PMD_HashedValue);
			AssertEquals(bizO.Header.PK, firstPatternMatchingDomain.PMD_OH);
			AssertEquals(OrgAddressSchema.Constants.Prefix, firstPatternMatchingDomain.PMD_ParentTableCode);
			AssertEquals(bizO.PK, firstPatternMatchingDomain.PMD_ParentId);
			AssertEquals(bizO.Header.CountryCode, firstPatternMatchingDomain.PMD_RN_NKCountryCode);

			var patternMatchingEmail = factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingEmail = patternMatchingEmail.Single();

			AssertEquals(1, patternMatchingEmail.Count);
			AssertEquals(emailHashedValue, firstPatternMatchingEmail.PME_HashedValue);
			AssertEquals(bizO.Header.PK, firstPatternMatchingEmail.PME_OH);
			AssertEquals(OrgAddressSchema.Constants.Prefix, firstPatternMatchingEmail.PME_ParentTableCode);
			AssertEquals(bizO.PK, firstPatternMatchingEmail.PME_ParentId);
			AssertEquals(bizO.Header.CountryCode, firstPatternMatchingEmail.PME_RN_NKCountryCode);
		}

		protected override void AssertPatternMatchingForNewRecords(BusinessObjectFactory factory, OrgAddress bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, bizO.OA_Email);
		}

		protected override void AssertPatternMatchingForExistingRecords(BusinessObjectFactory factory, OrgAddress bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, "diiopnail@domawapppppop.com");
		}

		protected override void AssertNoPatternMatchingForPlaceholderRecords(BusinessObjectFactory factory, OrgAddress bizO)
		{
			var patternMatchingDomain = factory.Load<PatternMatchingDomain>(new ZQuery(PatternMatchingDomainSchema.PMD_ParentId, bizO.PK)).ToList();
			AssertEquals(0, patternMatchingDomain.Count);

			var patternMatchingEmail = factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, bizO.PK)).ToList();
			AssertEquals(0, patternMatchingEmail.Count);
		}

		protected override void AssertPatternMatchingForNonSubscribedColumns(BusinessObjectFactory factory, OrgAddress bizO)
		{
			var patternMatchingDomain = factory.Load<PatternMatchingDomain>(new ZQuery(PatternMatchingDomainSchema.PMD_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingDomain = patternMatchingDomain.Single();

			AssertEquals(1, patternMatchingDomain.Count);
			AssertEquals(SubscriberTestUtilities.GetHash("WISETECHGLOBAL.COM"), firstPatternMatchingDomain.PMD_HashedValue);

			var patternMatchingEmail = factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, bizO.PK)).ToList();
			var firstpatternMatchingEmail = patternMatchingEmail.Single();

			AssertEquals(1, patternMatchingEmail.Count);
			AssertEquals(SubscriberTestUtilities.GetHash("DAVOS.SEAWORTH@WISETECHGLOBAL.COM"), firstpatternMatchingEmail.PME_HashedValue);
		}

		protected override void AssertQUEResultRecordExists(BusinessObjectFactory factory, OrgAddress bizO)
		{
			AssertQueuedRecordExists(factory, bizO.OA_OH, OrgHeaderSchema.Constants.Prefix);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new OrgAddressEmailSubscriber();

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
				row[OrgAddressSchema.OA_Email.Name] = DBNull.Value;
			}
			else
			{
				row[OrgAddressSchema.OA_Email.Name] = "Name";
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { OrgAddressSchema.OA_Email };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
