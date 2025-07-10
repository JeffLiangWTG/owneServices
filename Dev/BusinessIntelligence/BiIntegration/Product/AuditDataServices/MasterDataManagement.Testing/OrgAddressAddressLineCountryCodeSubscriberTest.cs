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
	[TestedType(typeof(OrgAddressAddressLineSubscriber))]
	class OrgAddressAddressLineCountryCodeSubscriberTest : PatternMatchingCountryCodeSubscriberTest<OrgAddress, OrgAddressAddressLineSubscriber>
	{
		protected override void AssertPatternMatchingCountryCodePrecondition(BusinessObjectFactory factory, OrgAddress bizO)
		{
			var patternMatchingAddress = factory.LoadTop1<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, bizO.PK));
			var patternMatchingName = factory.LoadTop1<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.Header.Contacts[0].PK));

			AssertNotNull(patternMatchingAddress);
			AssertNotNull(patternMatchingName);
			AssertEquals("NZ", patternMatchingAddress.PMA_RN_NKCountryCode);
			AssertEquals("NZ", patternMatchingName.PMN_RN_NKCountryCode);
		}

		protected override void AssertPatternMatchingForExistingRecords(BusinessObjectFactory factory, OrgAddress bizO)
		{
			var patternMatchingAddress = factory.LoadTop1<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, bizO.PK));
			var patternMatchingName = factory.LoadTop1<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.Header.Contacts[0].PK));

			AssertNotNull(patternMatchingAddress);
			AssertNotNull(patternMatchingName);
			AssertEquals("AU", patternMatchingAddress.PMA_RN_NKCountryCode);
			AssertEquals("AU", patternMatchingName.PMN_RN_NKCountryCode);
		}

		protected override OrgAddress CreateParentRecords(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);
			var orgAddress = orgHeader.Addresses.AddNew();
			var capability = factory.NewWithValidTestData<OrgAddressCapability>();
			var contact = orgHeader.Contacts.AddNew();

			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "1 thisisnotaplaceholder avenue";
			orgAddress.OA_Language = Enterprise.Core.SharedConstants.Languages.English;
			orgAddress.OA_RL_NKRelatedPortCode = "NZAKA";

			capability.PZ_AddressType = "OFC";
			capability.PZ_IsMainAddress = true;
			capability.PZ_OA = orgAddress.PK;

			contact.OC_ContactName = "Eddy";

			return orgAddress;
		}

		protected override void CreatePatternMatchingRecords(BusinessObjectFactory factory, OrgAddress bizO)
		{
			SubscriberTestUtilities.CreatePatternMatchingBusinessObject<PatternMatchingAddress>(factory, bizO.Header.PK, 0, OrgAddressSchema.Constants.Prefix, bizO.PK, "NZ");
			SubscriberTestUtilities.CreatePatternMatchingBusinessObject<PatternMatchingName>(factory, bizO.Header.PK, 0, OrgContactSchema.Constants.Prefix, bizO.Header.Contacts[0].PK, "NZ");
		}

		protected override string GetLog
		{
			get { return "Queued Deduplication in OrgAddressAddressLineSubscriber for {0}. Row: {1} - {2} - {3} -  -  -  - "; }
		}

		protected override string GetGenericQuery
		{
			get { return @"INSERT [dbo].[OrgAddress] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OA_PK, OA_OH, OA_Address1, OA_Language, OA_RL_NKRelatedPortCode) VALUES 
				(0x10, 0x01, {0}, 0x0, 1803, 1, '{1}', '{2}', '{3}', '{4}', '{5}');"; }
		}

		protected override string GetLsnMappingInsertQuery() => "INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15');";

		protected override string GetCDCHistoryInsertQuery() => "INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgAddress', 1, 1803);";

		protected override BetterLogForTest GetLogForExistingRecords(OrgAddress bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "1 thisisnotaplaceholder avenue"));
		}

		protected override string GetPatternMasters(OrgAddress bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, "[OH_PK:{0}]", bizO.Header.PK);
		}

		protected override string GetOriginalRecordCDCInsertQuery(OrgAddress bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "3", bizO.PK, bizO.Header.PK, bizO.OA_Address1, bizO.OA_Language, bizO.OA_RL_NKRelatedPortCode);
		}

		protected override string GetUpdateRecordCDCInsertQuery(OrgAddress bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.Header.PK, bizO.OA_Address1, bizO.OA_Language, "AUSYD");
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
				row[OrgAddressSchema.OA_Address1.Name] = "Address";
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
