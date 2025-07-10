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
	[TestedType(typeof(OrgCusCodeSubscriber))]
	class OrgCusCodeSubscriberTest : PatternMatchingBaseSubscriberTest<OrgCusCode, OrgCusCodeSubscriber>
	{
		protected override string GetGenericQuery
		{
			get { return @"INSERT [dbo].[OrgCusCode] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OK_PK, OK_OH, OK_CustomsRegNo, OK_CodeType) VALUES 
					(0x10, 0x01, {0}, 0x0, 1803, 1, '{1}', '{2}', '{3}', '{4}');"; }
		}

		protected override string GetLsnMappingInsertQuery() => "INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15');";

		protected override string GetCdcMappingInsertQuery() => "INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgCusCode', 1, 1803);";

		protected override string GetLog
		{
			get { return "Queued Deduplication in OrgCusCodeSubscriber for {0}. Row: {1} - {2} - {3}"; }
		}

		protected override OrgCusCode CreateParentRecords(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);
			var orgCusCode = orgHeader.CustomsCodes.AddNew();

			orgCusCode.OK_OH = orgHeader.PK;
			orgCusCode.OK_CustomsRegNo = "22 34 3";

			return orgCusCode;
		}

		protected override OrgCusCode CreatePlaceholderRecords(BusinessObjectFactory factory)
		{
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);
			var orgCusCode = orgHeader.CustomsCodes.AddNew();

			orgCusCode.OK_OH = orgHeader.PK;
			orgCusCode.OK_CustomsRegNo = "111";

			return orgCusCode;
		}

		protected override void CreatePatternMatchingRecords(BusinessObjectFactory factory, OrgCusCode bizO)
		{
			SubscriberTestUtilities.CreatePatternMatchingBusinessObject<PatternMatchingRegCode>(
				factory,
				bizO.Header.PK,
				SubscriberTestUtilities.GetHash(Utils.RemoveAllWhiteSpaceCharacters(bizO.OK_CustomsRegNo)),
				OrgCusCodeSchema.Constants.Prefix,
				bizO.PK,
				bizO.Header.CountryCode);
		}

		protected override string GetNewRecordCDCInsertQuery(OrgCusCode bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, 2, bizO.PK, bizO.Header.PK, bizO.OK_CustomsRegNo, bizO.OK_CodeType);
		}

		protected override string GetOriginalRecordCDCInsertQuery(OrgCusCode bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, 3, bizO.PK, bizO.Header.PK, bizO.OK_CustomsRegNo, bizO.OK_CodeType);
		}

		protected override string GetUpdateRecordCDCInsertQuery(OrgCusCode bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, 4, bizO.PK, bizO.Header.PK, "13345", bizO.OK_CodeType);
		}

		protected override string GetNonSubscribedColumnsCDCInsertQuery(OrgCusCode bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, 4, bizO.PK, bizO.Header.PK, bizO.OK_CustomsRegNo, "ABC");
		}

		protected override string GetUpdateRecordToPlaceHolderCDCInsertQuery(OrgCusCode bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, 4, bizO.PK, bizO.Header.PK, "111", bizO.OK_CodeType);
		}

		protected override BetterLogForTest GetLogForNewRecords(OrgCusCode bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Added", bizO.PK, bizO.OK_CustomsRegNo));
		}

		protected override BetterLogForTest GetLogForExistingRecords(OrgCusCode bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "13345"));
		}

		protected override BetterLogForTest GetLogForDeletedRecords(OrgCusCode bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "111"));
		}

		protected override string GetPatternMasters(OrgCusCode bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, "[OH_PK:{0}]", bizO.Header.PK);
		}

		protected override void AssertPatternMatchingNewRecordsPrecondition(BusinessObjectFactory factory, OrgCusCode bizO)
		{
			var patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK));
			AssertEquals(0, patternMatchingRegCode.Length);
		}

		void AssertPatternMatchingRecords(BusinessObjectFactory factory, OrgCusCode bizO, string orgCusCodeCustomsRegNo)
		{
			var patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingRegCode = patternMatchingRegCode.Single();
			var hashedValue = TextStandardizerHelper.ComputeStringHashFast(Utils.RemoveAllWhiteSpaceCharacters(orgCusCodeCustomsRegNo));

			AssertEquals(1, patternMatchingRegCode.Count);
			AssertEquals(hashedValue, firstPatternMatchingRegCode.PMR_HashedValue);
			AssertEquals(bizO.Header.PK, firstPatternMatchingRegCode.PMR_OH);
			AssertEquals(OrgCusCodeSchema.Constants.Prefix, firstPatternMatchingRegCode.PMR_ParentTableCode);
			AssertEquals(bizO.PK, firstPatternMatchingRegCode.PMR_ParentId);
			AssertEquals(bizO.Header.CountryCode, firstPatternMatchingRegCode.PMR_RN_NKCountryCode);
		}

		protected override void AssertPatternMatchingForNewRecords(BusinessObjectFactory factory, OrgCusCode bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, bizO.OK_CustomsRegNo);
		}

		protected override void AssertPatternMatchingForExistingRecords(BusinessObjectFactory factory, OrgCusCode bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, "13345");
		}

		protected override void AssertNoPatternMatchingForPlaceholderRecords(BusinessObjectFactory factory, OrgCusCode bizO)
		{
			var patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK)).ToList();
			AssertEquals(0, patternMatchingRegCode.Count);
		}

		protected override void AssertPatternMatchingForNonSubscribedColumns(BusinessObjectFactory factory, OrgCusCode bizO)
		{
			var patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingRegCode = patternMatchingRegCode.Single();

			AssertEquals(1, patternMatchingRegCode.Count);
			AssertEquals(SubscriberTestUtilities.GetHash(Utils.RemoveAllWhiteSpaceCharacters(bizO.OK_CustomsRegNo)), firstPatternMatchingRegCode.PMR_HashedValue);
		}

		protected override void AssertQUEResultRecordExists(BusinessObjectFactory factory, OrgCusCode bizO)
		{
			AssertQueuedRecordExists(factory, bizO.OK_OH, OrgHeaderSchema.Constants.Prefix);
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
				row[OrgCusCodeSchema.OK_CustomsRegNo.Name] = "Email";
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
