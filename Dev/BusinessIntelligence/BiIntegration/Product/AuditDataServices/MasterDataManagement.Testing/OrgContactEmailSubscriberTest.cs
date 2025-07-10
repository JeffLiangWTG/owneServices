
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.AuditDataServices.MDM.Subscribers;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.MDM.Testing
{
	[TestedType(typeof(OrgContactEmailSubscriber))]
	class OrgContactEmailSubscriberTest : PatternMatchingBaseSubscriberTest<OrgContact, OrgContactEmailSubscriber>
	{
		protected override string GetGenericQuery => @"INSERT [dbo].[OrgContact] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OC_PK, OC_OH, OC_Email, OC_Gender) VALUES 
					(0x10, 0x01,  {0}, 0x0, 1803, 1, '{1}', '{2}', '{3}','{4}')";

		protected override string GetLsnMappingInsertQuery() => "INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15');";

		protected override string GetCdcMappingInsertQuery() => "INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgContact', 1, 1803);";

		protected override string GetLog => "Queued Deduplication in OrgContactEmailSubscriber for {0}. Row: {1} - {2} - {3}";

		protected override OrgContact CreateParentRecords(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_OH = orgHeader.PK;
			orgContact.OC_PER = glbPerson.PK;
			orgContact.OC_Email = "mywebsiteyay@newdomain.com";

			return orgContact;
		}

		protected override OrgContact CreatePlaceholderRecords(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_OH = orgHeader.PK;
			orgContact.OC_PER = glbPerson.PK;
			orgContact.OC_Email = "test@test.com";

			return orgContact;
		}

		protected override void CreatePatternMatchingRecords(BusinessObjectFactory factory, OrgContact bizO)
		{
			SubscriberTestUtilities.CreatePatternMatchingBusinessObject<PatternMatchingDomain>(factory, bizO.Header.PK, SubscriberTestUtilities.GetHash("NEWDOMAIN.COM"), OrgContactSchema.Constants.Prefix, bizO.PK, bizO.Header.CountryCode);
			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForOrgAndPerson<PatternMatchingEmail>(factory, bizO.Header.PK, bizO.Person.PK, SubscriberTestUtilities.GetHash("MYWEBSITEYAY@NEWDOMAIN.COM"), OrgContactSchema.Constants.Prefix, bizO.PK, bizO.Header.CountryCode);
		}

		protected override string GetNewRecordCDCInsertQuery(OrgContact bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, 2, bizO.PK, bizO.Header.PK, bizO.OC_Email, "M");
		}

		protected override string GetOriginalRecordCDCInsertQuery(OrgContact bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, 3, bizO.PK, bizO.Header.PK, bizO.OC_Email, "M");
		}

		protected override string GetUpdateRecordCDCInsertQuery(OrgContact bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, 4, bizO.PK, bizO.Header.PK, "Jon.Snow@differentdomain.com", "M");
		}

		protected override string GetNonSubscribedColumnsCDCInsertQuery(OrgContact bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, 4, bizO.PK, bizO.Header.PK, bizO.OC_Email, "M");
		}

		protected override string GetUpdateRecordToPlaceHolderCDCInsertQuery(OrgContact bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, 4, bizO.PK, bizO.Header.PK, "test@test.com", "M");
		}

		protected override BetterLogForTest GetLogForNewRecords(OrgContact bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Added", bizO.PK, bizO.OC_Email));
		}

		protected override BetterLogForTest GetLogForExistingRecords(OrgContact bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "Jon.Snow@differentdomain.com"));
		}

		protected override BetterLogForTest GetLogForDeletedRecords(OrgContact bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "test@test.com"));
		}

		protected override string GetPatternMasters(OrgContact bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, "[OH_PK:{0}], [PER_PK:{1}]", bizO.Header.PK, bizO.Person.PK);
		}

		protected override void AssertPatternMatchingNewRecordsPrecondition(BusinessObjectFactory factory, OrgContact bizO)
		{
			var patternMatchingDomain = factory.Load<PatternMatchingDomain>(new ZQuery(PatternMatchingDomainSchema.PMD_ParentId, bizO.PK));
			var patternMatchingEmail = factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, bizO.PK));

			AssertEquals(0, patternMatchingDomain.Length);
			AssertEquals(0, patternMatchingEmail.Length);
		}

		void AssertPatternMatchingRecords(BusinessObjectFactory factory, OrgContact bizO, string orgContactEmail)
		{
			var domain = TextStandardizerHelper.ExtractEmailDomain(orgContactEmail);
			var domainHashedValue = TextStandardizerHelper.ComputeStringHashFast(domain);
			var standardisedEmail = TextStandardizerHelper.StandardizeEmail(orgContactEmail);
			var emailHashedValue = TextStandardizerHelper.ComputeStringHashFast(standardisedEmail);

			var patternMatchingDomain = factory.Load<PatternMatchingDomain>(new ZQuery(PatternMatchingDomainSchema.PMD_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingDomain = patternMatchingDomain.Single();

			AssertEquals(1, patternMatchingDomain.Count);
			AssertEquals(domainHashedValue, firstPatternMatchingDomain.PMD_HashedValue);
			AssertEquals(bizO.Header.PK, firstPatternMatchingDomain.PMD_OH);
			AssertEquals(ZGuid.Empty, firstPatternMatchingDomain.PMD_PER);
			AssertEquals(OrgContactSchema.Constants.Prefix, firstPatternMatchingDomain.PMD_ParentTableCode);
			AssertEquals(bizO.PK, firstPatternMatchingDomain.PMD_ParentId);
			AssertEquals(bizO.Header.CountryCode, firstPatternMatchingDomain.PMD_RN_NKCountryCode);

			var patternMatchingEmail = factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingEmail = patternMatchingEmail.Single();

			AssertEquals(1, patternMatchingEmail.Count);
			AssertEquals(emailHashedValue, firstPatternMatchingEmail.PME_HashedValue);
			AssertEquals(bizO.Header.PK, firstPatternMatchingEmail.PME_OH);
			AssertEquals(bizO.Person.PK, firstPatternMatchingEmail.PME_PER);
			AssertEquals(OrgContactSchema.Constants.Prefix, firstPatternMatchingEmail.PME_ParentTableCode);
			AssertEquals(bizO.PK, firstPatternMatchingEmail.PME_ParentId);
			AssertEquals(bizO.Header.CountryCode, firstPatternMatchingEmail.PME_RN_NKCountryCode);
		}

		protected override void AssertPatternMatchingForNewRecords(BusinessObjectFactory factory, OrgContact bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, bizO.OC_Email);
		}

		protected override void AssertPatternMatchingForExistingRecords(BusinessObjectFactory factory, OrgContact bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, "Jon.Snow@differentdomain.com");
		}

		protected override void AssertNoPatternMatchingForPlaceholderRecords(BusinessObjectFactory factory, OrgContact bizO)
		{
			var patternMatchingDomain = factory.Load<PatternMatchingDomain>(new ZQuery(PatternMatchingDomainSchema.PMD_ParentId, bizO.PK)).ToList();
			AssertEquals(0, patternMatchingDomain.Count);

			var patternMatchingEmail = factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, bizO.PK)).ToList();
			AssertEquals(0, patternMatchingEmail.Count);
		}

		protected override void AssertPatternMatchingForNonSubscribedColumns(BusinessObjectFactory factory, OrgContact bizO)
		{
			var patternMatchingDomain = factory.Load<PatternMatchingDomain>(new ZQuery(PatternMatchingDomainSchema.PMD_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingDomain = patternMatchingDomain.Single();

			AssertEquals(1, patternMatchingDomain.Count);
			AssertEquals(SubscriberTestUtilities.GetHash("NEWDOMAIN.COM"), firstPatternMatchingDomain.PMD_HashedValue);

			var patternMatchingEmail = factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingEmail = patternMatchingEmail.Single();

			AssertEquals(1, patternMatchingEmail.Count);
			AssertEquals(SubscriberTestUtilities.GetHash("MYWEBSITEYAY@NEWDOMAIN.COM"), firstPatternMatchingEmail.PME_HashedValue);
		}

		protected override void AssertQUEResultRecordExists(BusinessObjectFactory factory, OrgContact bizO)
		{
			AssertQueuedRecordExists(factory, bizO.OC_OH, OrgHeaderSchema.Constants.Prefix);
			AssertQueuedRecordExists(factory, bizO.OC_PER, GlbPersonSchema.Constants.Prefix);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new OrgContactEmailSubscriber();

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
				row[OrgContactSchema.OC_Email.Name] = DBNull.Value;
			}
			else
			{
				row[OrgContactSchema.OC_Email.Name] = "Email";
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { OrgContactSchema.OC_Email };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
