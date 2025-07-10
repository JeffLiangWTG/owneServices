using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.MDM.Subscribers;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.MDM.Testing
{
	[TestedType(typeof(GlbStaffCertificateSubscriber))]
	class GlbStaffCertificateSubscriberTest : CertificateSubscriberTest<GlbStaffCertificateSubscriber>
	{
		protected override string ParentTablePrefix => GlbStaffSchema.Constants.Prefix;

		protected override ZGuid PersonPK => glbStaff.GS_PER;

		GlbStaff glbStaff;

		protected override GlbPerson GetPersonFromCertificate(BusinessObjectFactory factory, GenRegCertAccredMaintList bizO) => factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, bizO.XZ_ParentID)).Person;

		protected override GenRegCertAccredMaintList CreateParentRecords(BusinessObjectFactory factory)
		{
			glbStaff = SubscriberTestUtilities.CreateTestGlbStaff(factory);

			var certificate = factory.New<GenRegCertAccredMaintList>();
			certificate.XZ_ParentID = glbStaff.PK;
			certificate.XZ_ParentTableCode = ParentTablePrefix;
			certificate.XZ_RefNumber = parentNumber;
			certificate.XZ_Type = CertificateTypePairList.Codes.CA1;

			factory.Save();

			return certificate;
		}

		protected override GenRegCertAccredMaintList CreatePlaceholderRecords(BusinessObjectFactory factory)
		{
			glbStaff = SubscriberTestUtilities.CreateTestGlbStaff(factory);

			var certificate = factory.New<GenRegCertAccredMaintList>();
			certificate.XZ_ParentID = glbStaff.PK;
			certificate.XZ_ParentTableCode = ParentTablePrefix;
			certificate.XZ_RefNumber = placeHolderNumber;
			certificate.XZ_Type = CertificateTypePairList.Codes.PA1;

			factory.Save();

			return certificate;
		}

		protected GenRegCertAccredMaintList CreateUnlinkedStaffRecords(BusinessObjectFactory factory)
		{
			glbStaff = factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_IsSystemAccount = true;

			var certificate = factory.New<GenRegCertAccredMaintList>();
			certificate.XZ_ParentID = glbStaff.PK;
			certificate.XZ_ParentTableCode = ParentTablePrefix;
			certificate.XZ_RefNumber = parentNumber;
			certificate.XZ_Type = CertificateTypePairList.Codes.PA1;

			factory.Save();

			Db.Connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "UPDATE dbo.GlbStaff SET GS_PER = NULL, GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate() WHERE GS_PK = '{0}'", glbStaff.PK));

			return certificate;
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSubscriberIgnoresChangesForUnlinkedStaff()
		{
			var factory = new BusinessObjectFactory();

			var bizO = CreateUnlinkedStaffRecords(factory);

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				AssertPatternMatchingNewRecordsPrecondition(factory, bizO);

				var insertIntoCDCText = GetNewRecordCDCInsertQuery(bizO);
				auditConnection.ExecuteNonQuery(insertIntoCDCText);

				var insertLsnMappingText = GetLsnMappingInsertQuery();
				auditConnection.ExecuteNonQuery(insertLsnMappingText);

				auditConnection.ExecuteNonQuery(GetCdcMappingInsertQuery());

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "None Queuing Deduplication in CertificateSubscriber. Row: {0} - {1} - {2}_{3}", "Added", bizO.PK, bizO.XZ_Type, bizO.XZ_RefNumber))
				);

				AssertNoPatternMatchingForPlaceholderRecords(factory, bizO); // Can also be used to test unlinked records
			}
		}

		protected override void AssertQUEResultRecordExists(BusinessObjectFactory factory, GenRegCertAccredMaintList bizO)
		{
			var personPK = factory.Load<GlbStaff>(bizO.XZ_ParentID).GS_PER;
			AssertQueuedRecordExists(factory, personPK, GlbPersonSchema.Constants.Prefix);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new GlbStaffCertificateSubscriber();

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
				row[GenRegCertAccredMaintListSchema.XZ_Type.Name] = DBNull.Value;
				row[GenRegCertAccredMaintListSchema.XZ_RefNumber.Name] = DBNull.Value;
				row[GenRegCertAccredMaintListSchema.XZ_ParentTableCode.Name] = "BAD";
			}
			else
			{
				row[GenRegCertAccredMaintListSchema.XZ_Type.Name] = "Type";
				row[GenRegCertAccredMaintListSchema.XZ_RefNumber.Name] = "Ref";
				row[GenRegCertAccredMaintListSchema.XZ_ParentTableCode.Name] = GlbStaffSchema.Constants.Prefix;
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { GenRegCertAccredMaintListSchema.XZ_Type, GenRegCertAccredMaintListSchema.XZ_RefNumber, GenRegCertAccredMaintListSchema.XZ_ParentTableCode };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
