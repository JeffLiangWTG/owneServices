using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.AuditDataService.MDM.Testing;
using Enterprise.AuditDataServices.MDM.Subscribers;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.MDM.Testing
{
	[TestedType(typeof(OrgContactNameSubscriber))]
	class OrgContactNameSubscriberTest : PatternMatchingPersonNameSubscriberTest<OrgContact, OrgContactNameSubscriber>
	{
		protected override string GetGenericQuery =>
			@"INSERT [dbo].[OrgContact] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OC_PK, OC_PER, OC_ContactName, OC_Gender) VALUES (0x10, 0x01,  {0}, 0x0, 1803, 1, '{1}', '{2}', '{3}','{4}')";

		protected override string GetCdcMappingInsertQuery() => "INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgContact', 1, 1803);";

		protected override string GetLog => "Queued Deduplication in OrgContactNameSubscriber for {0}. Row: {1} - {2} - {3}";

		protected override void AssertNoPatternMatchingForPlaceholderRecords(BusinessObjectFactory factory, OrgContact bizO)
		{
			var patternMatchingName = factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK)).ToList();
			AssertEquals(0, patternMatchingName.Count);
		}

		protected override void AssertPatternMatchingForExistingRecords(BusinessObjectFactory factory, OrgContact bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, "Different Name");
		}

		protected override void AssertPatternMatchingForNewRecords(BusinessObjectFactory factory, OrgContact bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, bizO.OC_ContactName);
		}

		protected override void AssertPatternMatchingForNonSubscribedColumns(BusinessObjectFactory factory, OrgContact bizO)
		{
			var patternMatchingName = factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingName = patternMatchingName.Single();

			AssertEquals(1, patternMatchingName.Count);
			AssertEquals(GetUpperValueToHash(bizO.OC_ContactName), firstPatternMatchingName.PMN_HashedValue);
		}

		protected override void AssertPatternMatchingNewRecordsPrecondition(BusinessObjectFactory factory, OrgContact bizO)
		{
			var patternMatchingName = factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK));
			AssertEquals(0, patternMatchingName.Length);
		}

		protected override OrgContact CreateParentRecords(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_OH = orgHeader.PK;
			orgContact.OC_ContactName = "Albert Cheng";
			orgContact.OC_PER = glbPerson.PK;
			glbPerson.UpdateFromContact(orgContact);

			return orgContact;
		}

		protected override void CreatePatternMatchingRecords(BusinessObjectFactory factory, OrgContact bizO)
		{
			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingName>(factory, bizO.Person.PK, GetUpperValueToHash(bizO.OC_ContactName), OrgContactSchema.Constants.Prefix, bizO.PK, bizO.Person.PER_RN_NKCountry);
		}

		protected override OrgContact CreatePlaceholderRecords(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_OH = orgHeader.PK;
			orgContact.OC_ContactName = "test test";
			orgContact.OC_PER = glbPerson.PK;

			return orgContact;
		}

		protected override BetterLogForTest GetLogForDeletedRecords(OrgContact bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "test test"));
		}

		protected override BetterLogForTest GetLogForExistingRecords(OrgContact bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "Different name"));
		}

		protected override BetterLogForTest GetLogForNewRecords(OrgContact bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Added", bizO.PK, bizO.OC_ContactName));
		}

		protected override string GetPatternMasters(OrgContact bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, "[OH_PK:{0}], [PER_PK:{1}]", bizO.Header.PK, bizO.Person.PK);
		}

		protected override string GetLsnMappingInsertQuery() => "INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-15');";

		protected override string GetNewRecordCDCInsertQuery(OrgContact bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "2", bizO.PK, bizO.OC_PER, bizO.OC_ContactName, "M");
		}

		protected override string GetNonSubscribedColumnsCDCInsertQuery(OrgContact bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.OC_PER, bizO.OC_ContactName, "F");
		}

		protected override string GetOriginalRecordCDCInsertQuery(OrgContact bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "3", bizO.PK, bizO.OC_PER, bizO.OC_ContactName, "M");
		}

		protected override string GetUpdateRecordCDCInsertQuery(OrgContact bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.OC_PER, "Different name", "M");
		}

		protected override string GetUpdateRecordToPlaceHolderCDCInsertQuery(OrgContact bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.OC_PER, "test test", "1");
		}

		void AssertPatternMatchingRecords(BusinessObjectFactory factory, OrgContact bizO, string contactName)
		{
			var patternMatchingName = factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingName = patternMatchingName.Single();

			var standardName = TextStandardizerHelper.StandardizePersonName(contactName);
			var hashedValue = TextStandardizerHelper.ComputeStringHashFast(standardName);

			AssertEquals(1, patternMatchingName.Count);
			AssertEquals(hashedValue, firstPatternMatchingName.PMN_HashedValue);
			AssertEquals(bizO.Person.PK, firstPatternMatchingName.PMN_PER);
			AssertEquals(OrgContactSchema.Constants.Prefix, firstPatternMatchingName.PMN_ParentTableCode);
			AssertEquals(bizO.PK, firstPatternMatchingName.PMN_ParentId);
			AssertEquals(bizO.Person.PER_RN_NKCountry, firstPatternMatchingName.PMN_RN_NKCountryCode);
		}

		protected override void SetNameToSingleWord(OrgContact bizO)
		{
			bizO.OC_ContactName = "Single";
		}

		protected override void SetNameToMultiWords(OrgContact bizO)
		{
			bizO.OC_ContactName = "Albert Cheng";
		}

		protected override string GetUpdateRecordNameToSingleWordCDCInsertQuery(OrgContact bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.OC_PER, "Single", "M");
		}

		protected override BetterLogForTest GetLogForUpdateNameToSingleWordRecords(OrgContact bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "Single"));
		}

		protected override void AssertQUEResultRecordExists(BusinessObjectFactory factory, OrgContact bizO)
		{
			AssertQueuedRecordExists(factory, bizO.OC_OH, OrgHeaderSchema.Constants.Prefix);
			AssertQueuedRecordExists(factory, bizO.OC_PER, GlbPersonSchema.Constants.Prefix);
		}

		protected override OrgContact CreateDummyContact(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_OH = orgHeader.PK;
			orgContact.OC_ContactName = "DUMMY CONTACT TO SUPPRESS DOCS";
			orgContact.OC_PER = glbPerson.PK;
			glbPerson.UpdateFromContact(orgContact);

			factory.Save();
			return orgContact;
		}

		public override void TestCustomFilter()
		{
			var subscriber = new OrgContactNameSubscriber();

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
				row[OrgContactSchema.OC_ContactName.Name] = "DUMMY CONTACT TO SUPPRESS DOCS";
			}
			else
			{
				row[OrgContactSchema.OC_ContactName.Name] = "Email";
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { OrgContactSchema.OC_ContactName };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
