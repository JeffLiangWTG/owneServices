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
	[TestedType(typeof(OrgContactItemSubscriber))]
	class OrgContactItemPhoneNumberSubscriberTest : PatternMatchingBaseSubscriberTest<OrgContactItem, OrgContactItemSubscriber>
	{
		protected override string GetGenericQuery => @"INSERT [dbo].[OrgContactItem] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OI_PK, OI_OC, OI_ContactItemType, OI_Address, OI_Description) VALUES (0x10, 0x01,  {0}, 0x0, 1803, 1, '{1}', '{2}', '{3}', '{4}', '{5}')";

		protected override string GetCdcMappingInsertQuery() => "INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgContactItem', 1, 1803);";

		protected override string GetLog => "Queued Deduplication in OrgContactItemSubscriber for {0}. Row: {1} - {2} - {3} - {4}";

		protected override void AssertNoPatternMatchingForPlaceholderRecords(BusinessObjectFactory factory, OrgContactItem bizO)
		{
			var patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK)).ToList();
			AssertEquals(0, patternMatchingPhone.Count);
		}

		protected override void AssertPatternMatchingForExistingRecords(BusinessObjectFactory factory, OrgContactItem bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, "15512341234");
		}

		void AssertPatternMatchingRecords(BusinessObjectFactory factory, OrgContactItem bizO, string phone)
		{
			var patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingPhone = patternMatchingPhone.Single();

			AssertEquals(1, patternMatchingPhone.Count);
			AssertEquals(GetUpperValueToHash(phone), firstPatternMatchingPhone.PMP_HashedValue);
			AssertEquals(bizO.Contact.Person.PK, firstPatternMatchingPhone.PMP_PER);
			AssertEquals(OrgContactItemSchema.Constants.Prefix, firstPatternMatchingPhone.PMP_ParentTableCode);
			AssertEquals(bizO.PK, firstPatternMatchingPhone.PMP_ParentId);
			AssertEquals(bizO.Contact.Person.PER_RN_NKCountry, firstPatternMatchingPhone.PMP_RN_NKCountryCode);
		}

		protected override void AssertPatternMatchingForNewRecords(BusinessObjectFactory factory, OrgContactItem bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, "23355551234");
		}

		protected override void AssertPatternMatchingForNonSubscribedColumns(BusinessObjectFactory factory, OrgContactItem bizO)
		{
			var patternMatchingPhoneFirst = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK)).ToList().Single();
			AssertEquals(GetUpperValueToHash("23355551234"), patternMatchingPhoneFirst.PMP_HashedValue);
		}

		protected override void AssertPatternMatchingNewRecordsPrecondition(BusinessObjectFactory factory, OrgContactItem bizO)
		{
			var patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK)).ToList();
			AssertEquals(0, patternMatchingPhone.Count);
		}

		protected override OrgContactItem CreateParentRecords(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_OH = orgHeader.PK;
			orgContact.OC_PER = glbPerson.PK;

			var contactItem = orgContact.ContactItems.AddNew();
			contactItem.OI_Address = "23355551234";
			contactItem.OI_ContactItemType = OrgContactItemTypes.Codes.Phone;

			return contactItem;
		}

		protected override void CreatePatternMatchingRecords(BusinessObjectFactory factory, OrgContactItem bizO)
		{
			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingPhone>(factory, bizO.Contact.Person.PK, GetUpperValueToHash(bizO.OI_Address), OrgContactItemSchema.Constants.Prefix, bizO.PK, bizO.Contact.Person.PER_RN_NKCountry);
		}

		protected override OrgContactItem CreatePlaceholderRecords(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_OH = orgHeader.PK;
			orgContact.OC_PER = glbPerson.PK;

			var contactItem = orgContact.ContactItems.AddNew();
			contactItem.OI_Address = "111";
			contactItem.OI_ContactItemType = OrgContactItemTypes.Codes.Phone;

			return contactItem;
		}

		protected override BetterLogForTest GetLogForDeletedRecords(OrgContactItem bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, OrgContactItemTypes.Codes.Phone, "111"));
		}

		protected override BetterLogForTest GetLogForExistingRecords(OrgContactItem bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, OrgContactItemTypes.Codes.Phone, "15512341234"));
		}

		protected override BetterLogForTest GetLogForNewRecords(OrgContactItem bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Added", bizO.PK, OrgContactItemTypes.Codes.Phone, bizO.OI_Address));
		}

		protected override string GetPatternMasters(OrgContactItem bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, "[PER_PK:{0}]", bizO.Contact.Person.PK);
		}

		protected override string GetLsnMappingInsertQuery() => "INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-15');";

		protected override string GetNewRecordCDCInsertQuery(OrgContactItem bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "2", bizO.PK, bizO.Contact.PK, OrgContactItemTypes.Codes.Phone, bizO.OI_Address, bizO.OI_Description);
		}

		protected override string GetNonSubscribedColumnsCDCInsertQuery(OrgContactItem bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.Contact.PK, OrgContactItemTypes.Codes.Phone, bizO.OI_Address, "ABC");
		}

		protected override string GetOriginalRecordCDCInsertQuery(OrgContactItem bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "3", bizO.PK, bizO.Contact.PK, OrgContactItemTypes.Codes.Phone, bizO.OI_Address, bizO.OI_Description);
		}

		protected override string GetUpdateRecordCDCInsertQuery(OrgContactItem bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.Contact.PK, OrgContactItemTypes.Codes.Phone, "15512341234", bizO.OI_Description);
		}

		protected override string GetUpdateRecordToPlaceHolderCDCInsertQuery(OrgContactItem bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.Contact.PK, OrgContactItemTypes.Codes.Phone, "111", bizO.OI_Description);
		}

		protected override void AssertQUEResultRecordExists(BusinessObjectFactory factory, OrgContactItem bizO)
		{
			AssertQueuedRecordExists(factory, bizO.Contact.OC_PER, GlbPersonSchema.Constants.Prefix);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new OrgContactItemSubscriber();

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
				row[OrgContactItemSchema.OI_Address.Name] = DBNull.Value;
			}
			else
			{
				row[OrgContactItemSchema.OI_Address.Name] = "Email";
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { OrgContactItemSchema.OI_Address };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
