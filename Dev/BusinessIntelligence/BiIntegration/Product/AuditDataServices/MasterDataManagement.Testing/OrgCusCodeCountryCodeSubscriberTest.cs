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
	[TestedType(typeof(OrgCusCodeSubscriber))]
	class OrgCusCodeCountryCodeSubscriberTest : PatternMatchingCountryCodeSubscriberTest<OrgCusCode, OrgCusCodeSubscriber>
	{
		protected override string GetLog
		{
			get { return "Queued Deduplication in OrgCusCodeSubscriber for {0}. Row: {1} - {2} - {3}"; }
		}

		protected override string GetGenericQuery
		{
			get { return @"INSERT [dbo].[OrgCusCode] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OK_PK, OK_OH, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry) VALUES 
					(0x10, 0x01, {0}, 0x0, 1803, 1, '{1}', '{2}', '{3}', '{4}', '{5}');"; }
		}

		protected override string GetLsnMappingInsertQuery() => "INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15');";

		protected override string GetCDCHistoryInsertQuery() => "INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgCusCode', 1, 1803);";

		protected override void AssertPatternMatchingCountryCodePrecondition(BusinessObjectFactory factory, OrgCusCode bizO)
		{
			var patternMatchingAddress = factory.LoadTop1<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK));

			AssertNotNull(patternMatchingAddress);
			AssertEquals("NZ", patternMatchingAddress.PMR_RN_NKCountryCode);
		}

		protected override void AssertPatternMatchingForExistingRecords(BusinessObjectFactory factory, OrgCusCode bizO)
		{
			var patternMatchingAddress = factory.LoadTop1<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK));

			AssertNotNull(patternMatchingAddress);
			AssertEquals("AU", patternMatchingAddress.PMR_RN_NKCountryCode);
		}

		protected override OrgCusCode CreateParentRecords(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);
			var orgCusCode = orgHeader.CustomsCodes.AddNew();

			orgCusCode.OK_OH = orgHeader.PK;
			orgCusCode.OK_CustomsRegNo = "22343";
			orgCusCode.OK_RN_NKCodeCountry = "NZ";

			return orgCusCode;
		}

		protected override void CreatePatternMatchingRecords(BusinessObjectFactory factory, OrgCusCode bizO)
		{
			SubscriberTestUtilities.CreatePatternMatchingBusinessObject<PatternMatchingRegCode>(factory, bizO.Header.PK, 0, OrgCusCodeSchema.Constants.Prefix, bizO.PK, "NZ");
		}

		protected override BetterLogForTest GetLogForExistingRecords(OrgCusCode bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "13345"));
		}

		protected override string GetPatternMasters(OrgCusCode bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, "[OH_PK:{0}]", bizO.Header.PK);
		}

		protected override string GetOriginalRecordCDCInsertQuery(OrgCusCode bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, 3, bizO.PK, bizO.Header.PK, bizO.OK_CustomsRegNo, bizO.OK_CodeType, "NZ");
		}

		protected override string GetUpdateRecordCDCInsertQuery(OrgCusCode bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, 4, bizO.PK, bizO.Header.PK, "13345", bizO.OK_CodeType, "AU");
		}

		public override void TestCustomFilter()
		{
			var subscriber = new OrgCusCodeSubscriber();

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
				row[OrgCusCodeSchema.OK_CustomsRegNo.Name] = DBNull.Value;
			}
			else
			{
				row[OrgCusCodeSchema.OK_CustomsRegNo.Name] = "RegNo";
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { OrgCusCodeSchema.OK_CustomsRegNo };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
