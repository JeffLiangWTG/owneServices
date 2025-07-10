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
	[TestedType(typeof(OrgContactBirthdaySubscriber))]
	class OrgContactBirthdaySubscriberTest : PatternMatchingBaseSubscriberTest<OrgContact, OrgContactBirthdaySubscriber>
	{
		protected override string GetGenericQuery => @"INSERT [dbo].[OrgContact] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OC_PK, OC_PER, OC_Birthday, OC_Gender) VALUES (0x10, 0x01,  {0}, 0x0, 1803, 1, '{1}', '{2}', {3},'{4}')";

		protected override string GetLog => "Queued Deduplication in OrgContactBirthdaySubscriber for {0}. Row: {1} - {2} - {3}";

		protected override string GetCdcMappingInsertQuery() => "INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgContact', 1, 1803);";

		protected override void AssertNoPatternMatchingForPlaceholderRecords(BusinessObjectFactory factory, OrgContact bizO)
		{
			// birthday don't have placeholders
			Assert(true);
		}

		protected override void AssertPatternMatchingForExistingRecords(BusinessObjectFactory factory, OrgContact bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, new ZDateTime(1993, 10, 1));
		}

		protected override void AssertPatternMatchingForNewRecords(BusinessObjectFactory factory, OrgContact bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, bizO.OC_Birthday);
		}

		protected override void AssertPatternMatchingForNonSubscribedColumns(BusinessObjectFactory factory, OrgContact bizO)
		{
			var patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingRegCode = patternMatchingRegCode.Single();

			AssertEquals(1, patternMatchingRegCode.Count);
			AssertEquals(GetBirthdayHashedValue(bizO.OC_Birthday), firstPatternMatchingRegCode.PMR_HashedValue);
		}

		protected override void AssertPatternMatchingNewRecordsPrecondition(BusinessObjectFactory factory, OrgContact bizO)
		{
			var patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK));
			AssertEquals(0, patternMatchingRegCode.Length);
		}

		protected override OrgContact CreateParentRecords(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_OH = orgHeader.PK;
			orgContact.OC_PER = glbPerson.PK;
			orgContact.OC_Birthday = new ZDateTime(1992, 1, 2);
			glbPerson.UpdateFromContact(orgContact);

			return orgContact;
		}

		protected override void CreatePatternMatchingRecords(BusinessObjectFactory factory, OrgContact bizO)
		{
			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingRegCode>(factory, bizO.Person.PK, GetBirthdayHashedValue(bizO.OC_Birthday), OrgContactSchema.Constants.Prefix, bizO.PK, bizO.Person.PER_RN_NKCountry);
		}

		protected override OrgContact CreatePlaceholderRecords(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_OH = orgHeader.PK;
			orgContact.OC_PER = glbPerson.PK;
			orgContact.OC_Birthday = new ZDateTime(1900, 1, 1);
			glbPerson.UpdateFromContact(orgContact);

			return orgContact;
		}

		protected override BetterLogForTest GetLogForDeletedRecords(OrgContact bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "01/01/1900"));
		}

		protected override BetterLogForTest GetLogForExistingRecords(OrgContact bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "10/01/1993"));
		}

		protected override BetterLogForTest GetLogForNewRecords(OrgContact bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Added", bizO.PK, bizO.OC_Birthday.IsEmpty ? "null" : bizO.OC_Birthday.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)));
		}

		protected override string GetPatternMasters(OrgContact bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, "[PER_PK:{0}]", bizO.Person.PK);
		}

		protected override string GetLsnMappingInsertQuery() => "INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-15');";

		protected override string GetNewRecordCDCInsertQuery(OrgContact bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "2", bizO.PK, bizO.OC_PER, bizO.OC_Birthday.IsEmpty ? "null" : "'" + bizO.OC_Birthday.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "'", "M");
		}

		protected override string GetNonSubscribedColumnsCDCInsertQuery(OrgContact bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.OC_PER, "'1992-1-2'", "F");
		}

		protected override string GetOriginalRecordCDCInsertQuery(OrgContact bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "3", bizO.PK, bizO.OC_PER, "'1992-1-2'", "M");
		}

		protected override string GetUpdateRecordCDCInsertQuery(OrgContact bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.OC_PER, "'1993-10-1'", "M");
		}

		protected override string GetUpdateRecordToPlaceHolderCDCInsertQuery(OrgContact bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.OC_PER, "'1900-01-01'", "M");
		}

		void AssertPatternMatchingRecords(BusinessObjectFactory factory, OrgContact bizO, ZDateTime birthday)
		{
			var patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingRegCode = patternMatchingRegCode.Single();

			AssertEquals(1, patternMatchingRegCode.Count);
			AssertEquals(GetBirthdayHashedValue(birthday), firstPatternMatchingRegCode.PMR_HashedValue);
			AssertEquals(bizO.Person.PK, firstPatternMatchingRegCode.PMR_PER);
			AssertEquals(OrgContactSchema.Constants.Prefix, firstPatternMatchingRegCode.PMR_ParentTableCode);
			AssertEquals(bizO.PK, firstPatternMatchingRegCode.PMR_ParentId);
			AssertEquals(bizO.Person.PER_RN_NKCountry, firstPatternMatchingRegCode.PMR_RN_NKCountryCode);
		}

		readonly DateTime beginDate = new DateTime(1753, 1, 1);

		int GetBirthdayHashedValue(ZDateTime birthday)
		{
			return TextStandardizerHelper.ComputeStringHashFast((birthday - beginDate).Days.ToString());
		}

		protected override void AssertQUEResultRecordExists(BusinessObjectFactory factory, OrgContact bizO)
		{
			AssertQueuedRecordExists(factory, bizO.OC_PER, GlbPersonSchema.Constants.Prefix);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new OrgContactBirthdaySubscriber();

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
				row[OrgContactSchema.OC_Birthday.Name] = DBNull.Value;
			}
			else
			{
				row[OrgContactSchema.OC_Birthday.Name] = DateTime.Now;
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { OrgContactSchema.OC_Birthday };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
