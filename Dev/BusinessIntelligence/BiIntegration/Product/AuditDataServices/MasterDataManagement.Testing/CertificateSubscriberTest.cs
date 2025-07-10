using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.AuditDataServices.MDM.Subscribers;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.MDM.Testing
{
	abstract class CertificateSubscriberTest<T> : PatternMatchingBaseSubscriberTest<GenRegCertAccredMaintList, T> where T : CertificateSubscriber
	{
		protected const string parentNumber = "31415926";
		protected const string placeHolderNumber = "1234";
		protected const string updateNumber = "161803399";

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestNoExceptionThrowWhenCertificateParentNotExistInDatabase()
		{
			var factory = new BusinessObjectFactory();

			var certificate = factory.New<GenRegCertAccredMaintList>();
			certificate.XZ_ParentID = ZGuid.NewZGuid();
			certificate.XZ_ParentTableCode = ParentTablePrefix;
			certificate.XZ_RefNumber = parentNumber;
			certificate.XZ_Type = CertificateTypePairList.Codes.CA1;

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				AssertPatternMatchingNewRecordsPrecondition(factory, certificate);

				var insertIntoCDCText = GetNewRecordCDCInsertQuery(certificate);
				auditConnection.ExecuteNonQuery(insertIntoCDCText);

				var insertLsnMappingText = GetLsnMappingInsertQuery();
				auditConnection.ExecuteNonQuery(insertLsnMappingText);

				var insertCdcMappingText = GetCdcMappingInsertQuery();
				auditConnection.ExecuteNonQuery(insertCdcMappingText);

				AssertNoExceptionThrown(() =>
				{
					AuditTestHelper.RunNotificationCycleAndAssert(
						auditConnection,
						testSubscribers,
						"0x10",
						"2018-03-14",
						expectedLog: new BetterLogForTest(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "None Queuing Deduplication in CertificateSubscriber. Row: {0} - {1} - {2}_{3}", "Added", certificate.PK, certificate.XZ_Type, certificate.XZ_RefNumber))
					);
				});

				var patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, certificate.PK)).ToList();
				AssertEquals("No record generated", 0, patternMatchingRegCode.Count);
			}
		}

		protected abstract string ParentTablePrefix { get; }

		protected abstract GlbPerson GetPersonFromCertificate(BusinessObjectFactory factory, GenRegCertAccredMaintList bizO);

		protected override string GetGenericQuery => @"INSERT [dbo].[GenRegCertAccredMaintList] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], XZ_PK, XZ_ParentTableCode, XZ_ParentID, XZ_RefNumber, XZ_Type, XZ_Comment) VALUES (0x10, 0x01, {0}, 0x0, 1803, 1, '{1}', '{2}', '{3}', '{4}', '{5}', '{6}');";
		// NOTE: Codes for [__$operation] are as follows
		// 1 - Delete
		// 2 - Insert
		// 3 - Update(Before Values)
		// 4 - Update(After Values)
		// See https://blogs.technet.microsoft.com/sql_server_isv/2010/12/02/change-data-capture-what-is-it-and-how-do-i-use-it/ for more details

		protected override string GetLog => "Queued Deduplication in CertificateSubscriber for {0}. Row: {1} - {2} - {3}_{4}";

		protected override string GetLsnMappingInsertQuery() => "INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-14');";

		protected override string GetCdcMappingInsertQuery() => "INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GenRegCertAccredMaintList', 1, 1803);";

		protected abstract ZGuid PersonPK { get; }

		protected override void CreatePatternMatchingRecords(BusinessObjectFactory factory, GenRegCertAccredMaintList bizO)
		{
			var person = GetPersonFromCertificate(factory, bizO);
			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingRegCode>(factory, person.PK, SubscriberTestUtilities.GetHash(string.Concat(bizO.XZ_Type, bizO.XZ_RefNumber)), GenRegCertAccredMaintListSchema.Constants.Prefix, bizO.PK, person.PER_RN_NKCountry);
		}

		protected override string GetNewRecordCDCInsertQuery(GenRegCertAccredMaintList bizO) => string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "2", bizO.PK, bizO.XZ_ParentTableCode, bizO.XZ_ParentID, bizO.XZ_RefNumber, bizO.XZ_Type, "0");

		protected override string GetOriginalRecordCDCInsertQuery(GenRegCertAccredMaintList bizO) => string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "3", bizO.PK, bizO.XZ_ParentTableCode, bizO.XZ_ParentID, bizO.XZ_RefNumber, bizO.XZ_Type, "0");

		protected override string GetUpdateRecordCDCInsertQuery(GenRegCertAccredMaintList bizO) => string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.XZ_ParentTableCode, bizO.XZ_ParentID, updateNumber, bizO.XZ_Type, "0");

		protected override string GetNonSubscribedColumnsCDCInsertQuery(GenRegCertAccredMaintList bizO) => string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.XZ_ParentTableCode, bizO.XZ_ParentID, bizO.XZ_RefNumber, bizO.XZ_Type, "1");

		protected override string GetUpdateRecordToPlaceHolderCDCInsertQuery(GenRegCertAccredMaintList bizO) => string.Format(CultureInfo.InvariantCulture, GetGenericQuery, "4", bizO.PK, bizO.XZ_ParentTableCode, bizO.XZ_ParentID, placeHolderNumber, bizO.XZ_Type, "0");

		protected override BetterLogForTest GetLogForNewRecords(GenRegCertAccredMaintList bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Added", bizO.PK, bizO.XZ_Type, bizO.XZ_RefNumber));
		}

		protected override BetterLogForTest GetLogForExistingRecords(GenRegCertAccredMaintList bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, bizO.XZ_Type, updateNumber));
		}

		protected override BetterLogForTest GetLogForDeletedRecords(GenRegCertAccredMaintList bizO)
		{
			return new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, GetLog, GetPatternMasters(bizO), "Modified", bizO.PK, bizO.XZ_Type, placeHolderNumber));
		}

		protected override string GetPatternMasters(GenRegCertAccredMaintList bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, "[PER_PK:{0}]", PersonPK);
		}

		protected override void AssertPatternMatchingNewRecordsPrecondition(BusinessObjectFactory factory, GenRegCertAccredMaintList bizO)
		{
			var patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK)); //TODO: Confirm if this should be bizO.ParentID

			AssertEquals(0, patternMatchingRegCode.Length);
		}

		protected override void AssertPatternMatchingForNewRecords(BusinessObjectFactory factory, GenRegCertAccredMaintList bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, bizO.XZ_Type, bizO.XZ_RefNumber);
		}

		protected override void AssertPatternMatchingForExistingRecords(BusinessObjectFactory factory, GenRegCertAccredMaintList bizO)
		{
			AssertPatternMatchingRecords(factory, bizO, bizO.XZ_Type, updateNumber);
		}

		protected override void AssertNoPatternMatchingForPlaceholderRecords(BusinessObjectFactory factory, GenRegCertAccredMaintList bizO)
		{
			var patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK)).ToList();

			AssertEquals(0, patternMatchingRegCode.Count);
		}

		protected override void AssertPatternMatchingForNonSubscribedColumns(BusinessObjectFactory factory, GenRegCertAccredMaintList bizO)
		{
			var patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingRegCode = patternMatchingRegCode.Single();

			AssertEquals(1, patternMatchingRegCode.Count);
			AssertEquals(SubscriberTestUtilities.GetHash(string.Concat(bizO.XZ_Type, bizO.XZ_RefNumber)), firstPatternMatchingRegCode.PMR_HashedValue);
		}

		protected void AssertPatternMatchingRecords(BusinessObjectFactory factory, GenRegCertAccredMaintList bizO, string certificateType, string certificateRefNumber)
		{
			var standardizedRegCode = TextStandardizerHelper.StandardizeCertificate(certificateRefNumber, certificateType);
			var regCodeHashedValue = TextStandardizerHelper.ComputeStringHashFast(standardizedRegCode);

			var patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK)).ToList();
			var firstPatternMatchingRegCode = patternMatchingRegCode.Single();

			var person = GetPersonFromCertificate(factory, bizO);
			AssertEquals(1, patternMatchingRegCode.Count);
			AssertEquals(regCodeHashedValue, firstPatternMatchingRegCode.PMR_HashedValue);
			AssertEquals(person.PK, firstPatternMatchingRegCode.PMR_PER);
			AssertEquals(GenRegCertAccredMaintListSchema.Constants.Prefix, firstPatternMatchingRegCode.PMR_ParentTableCode);
			AssertEquals(bizO.PK, firstPatternMatchingRegCode.PMR_ParentId);
			AssertEquals(person.PER_RN_NKCountry, firstPatternMatchingRegCode.PMR_RN_NKCountryCode);
		}
	}
}
