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
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.MDM.Testing
{
	[TestedType(typeof(HRJobApplicantEmailSubscriber))]
	class HRJobApplicantEmailSubscriberTest : PatternMatchingBaseSubscriberTest<HRJobApplicant, HRJobApplicantEmailSubscriber>
	{
		protected override string GetGenericQuery => @"INSERT [dbo].[HRJobApplicant] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], HA_PK, HA_PER, HA_EmailAddress) VALUES (0x10, 0x01, {0}, 0x0, 1803, 1, '{1}', '{2}', '{3}');";

		protected override string GetLsnMappingInsertQuery() => "INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08');";

		protected override string GetCdcMappingInsertQuery() => "INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'HRJobApplicant', 1, 1803);";

		protected override string GetLog => "Queued Deduplication in HRJobApplicantEmailSubscriber for {0}. Row: {1} - {2} - {3}";

		protected override HRJobApplicant CreateParentRecords(BusinessObjectFactory factory)
		{
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			var jobApplicant = factory.New<HRJobApplicant>();
			jobApplicant.HA_PER = glbPerson.PK;
			jobApplicant.HA_EmailAddress = "Davos.Seaworth@wisetechglobal.com";
			jobApplicant.HA_RN_NKCountry = "AU";

			return jobApplicant;
		}

		protected override HRJobApplicant CreatePlaceholderRecords(BusinessObjectFactory factory)
		{
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			var jobApplicant = factory.New<HRJobApplicant>();
			jobApplicant.HA_PER = glbPerson.PK;
			jobApplicant.HA_EmailAddress = "test@test.com";

			return jobApplicant;
		}

		protected override void CreatePatternMatchingRecords(BusinessObjectFactory factory, HRJobApplicant bizO)
		{
			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingEmail>(factory, bizO.HA_PER, GetUpperValueToHash(bizO.HA_EmailAddress), HRJobApplicantSchema.Constants.Prefix, bizO.PK, bizO.Person.PER_RN_NKCountry);
		}

		protected override string GetNewRecordCDCInsertQuery(HRJobApplicant bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "2", bizO.PK, bizO.HA_PER, bizO.HA_EmailAddress);
		}

		protected override string GetOriginalRecordCDCInsertQuery(HRJobApplicant bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "3", bizO.PK, bizO.HA_PER, bizO.HA_EmailAddress);
		}

		protected override string GetUpdateRecordCDCInsertQuery(HRJobApplicant bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.HA_PER, "diiopnail@domawapppppop.com");
		}

		protected override string GetNonSubscribedColumnsCDCInsertQuery(HRJobApplicant bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.HA_PER, bizO.HA_EmailAddress);
		}

		protected override string GetUpdateRecordToPlaceHolderCDCInsertQuery(HRJobApplicant bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.HA_PER, "test@test.com");
		}

		protected override BetterLogForTest GetLogForNewRecords(HRJobApplicant bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Added", bizO.PK, bizO.HA_EmailAddress));
		}

		protected override BetterLogForTest GetLogForExistingRecords(HRJobApplicant bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "diiopnail@domawapppppop.com"));
		}

		protected override BetterLogForTest GetLogForDeletedRecords(HRJobApplicant bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, "test@test.com"));
		}

		protected override string GetPatternMasters(HRJobApplicant bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, "[PER_PK:{0}]", bizO.Person.PK);
		}

		protected override void AssertPatternMatchingNewRecordsPrecondition(BusinessObjectFactory factory, HRJobApplicant bizO)
		{
			var patternMatchingEmail = factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, bizO.PK));
			AssertEquals(0, patternMatchingEmail.Length);
		}

		protected override void AssertPatternMatchingForNewRecords(BusinessObjectFactory factory, HRJobApplicant bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, bizO.HA_EmailAddress);
		}

		protected override void AssertPatternMatchingForExistingRecords(BusinessObjectFactory factory, HRJobApplicant bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, "diiopnail@domawapppppop.com");
		}

		protected override void AssertNoPatternMatchingForPlaceholderRecords(BusinessObjectFactory factory, HRJobApplicant bizO)
		{
			var patternMatchingEmail = factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, bizO.PK)).ToList();
			AssertEquals(0, patternMatchingEmail.Count);
		}

		protected override void AssertPatternMatchingForNonSubscribedColumns(BusinessObjectFactory factory, HRJobApplicant bizO)
		{
			var patternMatchingEmail = factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingEmail = patternMatchingEmail.Single();

			AssertEquals(1, patternMatchingEmail.Count);
			AssertEquals(GetUpperValueToHash(bizO.HA_EmailAddress), firstPatternMatchingEmail.PME_HashedValue);
		}

		void AssertPatternMatchingRecords(BusinessObjectFactory factory, HRJobApplicant bizO, string email)
		{
			var standardisedEmail = TextStandardizerHelper.StandardizeEmail(email);
			var emailHashedValue = TextStandardizerHelper.ComputeStringHashFast(standardisedEmail);
			var patternMatchingEmail = factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingEmail = patternMatchingEmail.Single();

			AssertEquals(1, patternMatchingEmail.Count);
			AssertEquals(emailHashedValue, firstPatternMatchingEmail.PME_HashedValue);
			AssertEquals(bizO.Person.PK, firstPatternMatchingEmail.PME_PER);
			AssertEquals(HRJobApplicantSchema.Constants.Prefix, firstPatternMatchingEmail.PME_ParentTableCode);
			AssertEquals(bizO.PK, firstPatternMatchingEmail.PME_ParentId);
			AssertEquals(bizO.Person.PER_RN_NKCountry, firstPatternMatchingEmail.PME_RN_NKCountryCode);
		}

		protected override void AssertQUEResultRecordExists(BusinessObjectFactory factory, HRJobApplicant bizO)
		{
			AssertQueuedRecordExists(factory, bizO.HA_PER, GlbPersonSchema.Constants.Prefix);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new HRJobApplicantEmailSubscriber();

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
				row[HRJobApplicantSchema.HA_EmailAddress.Name] = DBNull.Value;
			}
			else
			{
				row[HRJobApplicantSchema.HA_EmailAddress.Name] = "Name";
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { HRJobApplicantSchema.HA_EmailAddress };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
