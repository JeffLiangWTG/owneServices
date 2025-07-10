using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.LandTransport;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test
{
	[TestedType(typeof(DeduplicateJobDocAddressForDtbConsignmentAddresses))]
	class DeduplicateJobDocAddressForDtbConsignmentAddressesTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			PrepareDataForTest();
		}

		static void PrepareDataForTest()
		{
			DBTransformationTestHelper.DropIndexIfExists("dbo.JobDocAddress", "NR_UX__E2_ParentID", Db.Connection);

			const string sqlText = @"
DECLARE @consignment1PK UNIQUEIDENTIFIER = '6b581b3f-1b20-4f77-aa2f-8dae1b466614'
DECLARE @consignment2PK UNIQUEIDENTIFIER = 'c512c955-4f8b-419a-b57c-59b5e8e00195'
DECLARE @consignment3PK UNIQUEIDENTIFIER = '76be4af2-7133-4d73-bb7f-f45cd11dbba4'
DECLARE @consignment4PK UNIQUEIDENTIFIER = '7e111088-cb5f-49e1-af77-53e2710e255f'
DECLARE @consignmentAddress1PK UNIQUEIDENTIFIER = '8d3ffe41-a97f-4d48-aa22-e38f416b4826'
DECLARE @consignmentAddress2PK UNIQUEIDENTIFIER = 'aa6fa00b-44dc-472d-b02a-e0204981fad6'
DECLARE @consignmentAddress3PK UNIQUEIDENTIFIER = '6f14c606-66e2-4d49-ba97-976957813da5'
DECLARE @consignmentAddress4PK UNIQUEIDENTIFIER = '08e6c6fd-4c97-4237-9bfb-38b75b475465'
DECLARE @shipment1PK UNIQUEIDENTIFIER = '40ee8bee-e15d-43b1-8b41-8526a2cfa68c'
DECLARE @shipment2PK UNIQUEIDENTIFIER = '6c69d2ab-4d8b-4857-bc05-ea850d9db820'
DECLARE @jda6PK UNIQUEIDENTIFIER = 'b02312a8-15f2-48aa-8d7b-3db87de16a59'
DECLARE @jda7PK UNIQUEIDENTIFIER = 'c2cbde74-82d4-461b-9b4e-33002697dc4e'
DECLARE @quarantineExDocLinePK UNIQUEIDENTIFIER = '0aef494a-7352-4d69-9160-7c1c4dffc6df'
DECLARE @invoiceLinePK UNIQUEIDENTIFIER = '7ea83f2b-75a7-4a7a-96c3-6a9e6f823ca4'
DECLARE @invoiceHeaderPK UNIQUEIDENTIFIER = '59c66292-38da-4f17-a312-f510bf10b36a'
DECLARE @declarationPK UNIQUEIDENTIFIER = '26c1593b-0fc9-4c3c-9a7a-549df3d6f8a6'
DECLARE @companyPK UNIQUEIDENTIFIER = 'cd160756-f9f1-4849-98ff-adc1b037817c'
DECLARE @branchPK UNIQUEIDENTIFIER = 'c67b0ca5-9acd-402b-b78a-89b187ee98fc'
DECLARE @now DATETIME = GETUTCDATE()

-- Land Transport relevant objects: This is realistically all that the customer will have in their system, if they even have any duplicates (which is unlikely):

INSERT INTO dbo.DtbConsignment (LTC_PK, LTC_ConsignmentType, LTC_JobID, LTC_ConnoteNumber, LTC_JobType, LTC_Status, LTC_Direction, LTC_KM_Booking, LTC_IsRouteOverridden, LTC_SystemCreateTimeUtc, LTC_SystemCreateUser, LTC_SystemLastEditTimeUtc, LTC_SystemLastEditUser)
VALUES
(@consignment1PK, 'LTC', 'CN0001', 'CNNote001', 'LTL', 'CMP', 'LOC', NULL, 1, @now, '~BP', @now, '~BP'),
(@consignment2PK, 'LTC', 'CN0002', 'CNNote002', 'LTL', 'CMP', 'LOC', NULL, 1, @now, '~BP', @now, '~BP'),
(@consignment3PK, 'LTC', 'CN0003', 'CNNote003', 'LTL', 'CMP', 'LOC', NULL, 1, @now, '~BP', @now, '~BP'),
(@consignment4PK, 'LTC', 'CN0004', 'CNNote004', 'LTL', 'CMP', 'LOC', NULL, 1, @now, '~BP', @now, '~BP');

INSERT INTO dbo.DtbConsignmentAddress (LTS_PK, LTS_LTC_Consignment, LTS_InstructionType, LTS_Sequence, LTS_Status, LTS_SystemCreateTimeUtc, LTS_SystemCreateUser, LTS_SystemLastEditTimeUtc, LTS_SystemLastEditUser)
VALUES
(@consignmentAddress1PK, @consignment1PK, 'PIC', 1, 'INC', @now, '~BP', @now, '~BP'),
(@consignmentAddress2PK, @consignment2PK, 'DLV', 1, 'INC', @now, '~BP', @now, '~BP'),
(@consignmentAddress3PK, @consignment3PK, 'PIC', 1, 'INC', @now, '~BP', @now, '~BP'),
(@consignmentAddress4PK, @consignment4PK, 'MLT', 1, 'INC', @now, '~BP', @now, '~BP');

INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_IsForwardRegistered, JS_IsBooking, JS_TransportMode, JS_PackingMode, JS_ActualWeight, JS_TH_OneTimeQuote, JS_SystemCreateTimeUtc, JS_SystemCreateUser, JS_SystemLastEditTimeUtc, JS_SystemLastEditUser)
VALUES
(@shipment1PK, '1234', 0, 0, 'ROA', 'FCL', 1, null, @now, '~BP', @now, '~BP'),
(@shipment2PK, '5678', 0, 0, 'ROA', 'FCL', 1, null, @now, '~BP', @now, '~BP');

INSERT INTO dbo.JobDocAddress (E2_PK, E2_ParentID, E2_ParentTableCode, E2_AddressType, E2_Contact, E2_City, E2_State, E2_RN_NKCountryCode, E2_GeoLocation, E2_OA_Address, E2_AddressOverride ,E2_ValidationStatus, E2_CompanyName, E2_SystemCreateTimeUtc, E2_SystemCreateUser, E2_SystemLastEditTimeUtc, E2_SystemLastEditUser)
VALUES
(newid(), @consignmentAddress1PK, 'LTS', '1', 'Address 1 - not the most recent creation date',	'Alexandria', 'NSW', 'AU', geography::Point(47.616, -122.360, 4326), NULL, 0, 'NRQ', '', @now - 2, '~BP', @now - 1, '~BP'),
(newid(), @consignmentAddress1PK, 'LTS', '2', 'Address 2 - most recent creation date',			'Alexandria', 'NSW', 'AU', geography::Point(47.616, -122.360, 4326), NULL, 0, 'NRQ', '', @now - 1, '~BP', @now - 1, '~BP'),
(newid(), @consignmentAddress1PK, 'LTS', '3', 'Address 3 - not the most recent creation date',	'Alexandria', 'NSW', 'AU', geography::Point(47.616, -122.360, 4326), NULL, 0, 'NRQ', '', @now - 3, '~BP', @now - 1, '~BP'),
(newid(), @consignmentAddress2PK, 'LTS', '1', 'Address 4 - not the most recent edit time',		'Alexandria', 'NSW', 'AU', geography::Point(47.616, -122.360, 4326), NULL, 0, 'NRQ', '', @now - 2, '~BP', @now - 2, '~BP'),
(newid(), @consignmentAddress2PK, 'LTS', '2', 'Address 5 - most recent edit time',				'Alexandria', 'NSW', 'AU', geography::Point(47.616, -122.360, 4326), NULL, 0, 'NRQ', '', @now - 2, '~BP', @now - 1, '~BP'),
(@jda6PK, @consignmentAddress3PK, 'LTS', '1', 'Address 6 - not the first PK',					'Alexandria', 'NSW', 'AU', geography::Point(47.616, -122.360, 4326), NULL, 0, 'NRQ', '', @now - 2, '~BP', @now - 1, '~BP'),
(@jda7PK, @consignmentAddress3PK, 'LTS', '2', 'Address 7 - first PK',							'Alexandria', 'NSW', 'AU', geography::Point(47.616, -122.360, 4326), NULL, 0, 'NRQ', '', @now - 2, '~BP', @now - 1, '~BP'),
(newid(), @consignmentAddress4PK, 'LTS', '1', 'Address 8 - not a duplicate',					'Alexandria', 'NSW', 'AU', geography::Point(47.616, -122.360, 4326), NULL, 0, 'NRQ', '', @now - 1, '~BP', @now - 1, '~BP'),
(newid(), @shipment1PK, 'JS', '1', 'Address 9 - not duplicate, not consignment address parent',	'Alexandria', 'NSW', 'AU', geography::Point(47.616, -122.360, 4326), NULL, 0, 'NRQ', '', @now - 1, '~BP', @now - 1, '~BP'),
(newid(), @shipment2PK, 'JS', '1', 'Address 10 - duplicate but not consignment address parent',	'Alexandria', 'NSW', 'AU', geography::Point(47.616, -122.360, 4326), NULL, 0, 'NRQ', '', @now - 1, '~BP', @now - 1, '~BP'),
(newid(), @shipment2PK, 'JS', '2', 'Address 11 - duplicate but not consignment address parent',	'Alexandria', 'NSW', 'AU', geography::Point(47.616, -122.360, 4326), NULL, 0, 'NRQ', '', @now - 1, '~BP', @now - 1, '~BP');

-- Everything below this line is extra objects to test the cascading delete.

INSERT INTO dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser)
VALUES
(@companyPK, 'AU', 'AUD', 'ZYX', 'AU company', @now, '~BP', @now, '~BP')

INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser)
VALUES
(@branchPK, @companyPK, 'XYZ', @now, '~BP', @now, '~BP')

INSERT INTO dbo.JobDocAddressNumber (E2N_PK, E2N_E2, E2N_NumberType, E2N_Number, E2N_RN_NKCountryCode, E2N_SystemCreateTimeUtc, E2N_SystemCreateUser, E2N_SystemLastEditTimeUtc, E2N_SystemLastEditUser)
VALUES
(newid(), @jda6PK, 'IDK', '123456', 'AU', @now, '~BP', @now, '~BP');

INSERT INTO dbo.JobDocumentExclusion (JDE_PK, JDE_ParentID, JDE_ParentTableCode, JDE_E2_Address)
VALUES
(newid(), newid(), 'IDK', @jda6PK);

INSERT INTO dbo.JobDeclaration (JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DataModel, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
VALUES
(@declarationPK, @branchPK, @companyPK, 1, 'AI', @now, '~BP', @now, '~BP');

INSERT INTO dbo.JobComInvoiceHeader (JZ_PK, JZ_JE, JZ_ClusterKey, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser)
VALUES
(@invoiceHeaderPK, @declarationPK, 1, 'AI', @now, '~BP', @now, '~BP');

INSERT INTO dbo.JobComInvoiceLine (JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_Weight, JI_Tariff, JI_AddInfo, JI_ClusterKey, JI_SystemCreateTimeUtc, JI_SystemCreateUser, JI_SystemLastEditTimeUtc, JI_SystemLastEditUser)
VALUES
(@invoiceLinePK, 'US', @invoiceHeaderPK, 1, 100, 2, '', 'GrossWeight=1', 1, @now, '~BP', @now, '~BP');

INSERT INTO dbo.QuarantineExDocLine (QL_PK, QL_JI, QL_ClusterKey, QL_SystemCreateTimeUtc, QL_SystemCreateUser, QL_SystemLastEditTimeUtc, QL_SystemLastEditUser)
VALUES
(@quarantineExDocLinePK, @invoiceLinePK, 1234567, @now, '~BP', @now, '~BP');

INSERT INTO dbo.QuarantineExDocEstablishmentAndTime (EE_PK, EE_QL, EE_E2_Address, EE_ClusterKey, EE_SystemCreateTimeUtc, EE_SystemCreateUser, EE_SystemLastEditTimeUtc, EE_SystemLastEditUser)
VALUES
(newid(), @quarantineExDocLinePK, @jda6PK, 12345, @now, '~BP', @now, '~BP');
";

			Db.Connection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertTransformationResults()
		{
			base.AssertTransformationResults();

			AssertDuplicatesDeleted();

			CombineAssertions(() =>
			{
				AssertTableIsEmpty("dbo.JobDocAddressNumber");
				AssertTableIsEmpty("dbo.JobDocumentExclusion");
				AssertTableIsEmpty("dbo.QuarantineExDocEstablishmentAndTime");
			});
		}

		static void AssertDuplicatesDeleted()
		{
			var remainingJobDocAddressDescriptions = new List<string>(7);

			Db.Connection.ExecuteReader("SELECT E2_Contact FROM dbo.JobDocAddress", record => remainingJobDocAddressDescriptions.Add(record.GetString(0)));

			AssertContainsExactElementsInAnyOrder("We expect the transformation to remove duplicate JobDocAddresses with DtbConsignmentAddress parents according to specific rules.",
				new[]
				{
					"Address 2 - most recent creation date",
					"Address 5 - most recent edit time",
					"Address 7 - first PK",
					"Address 8 - not a duplicate",
					"Address 9 - not duplicate, not consignment address parent",
					"Address 10 - duplicate but not consignment address parent",
					"Address 11 - duplicate but not consignment address parent",
				}, remainingJobDocAddressDescriptions);
		}

		static void AssertTableIsEmpty(string tableName)
		{
			var numberOfRows = Db.Connection.ExecuteScalar<int>($"SELECT COUNT(*) FROM {tableName}");
			AssertEquals($"{tableName} should be empty now that we've deleted the record related to the address that was deleted.", 0, numberOfRows);
		}

		public void TestTransformationShouldNotRun_WhenNoConsignmentAddressesExist()
		{
			var consignmentAddressCount = Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.DtbConsignmentAddress");
			AssertEquals("There should be no consignment addresses in the database by default.", 0, consignmentAddressCount);

			RunTransformationAndAssertDeleteCommandNotExecuted(@"
JobDocAddress doesn't have an index on E2_ParentTableCode, so this transformation involves an unavoidable clustered index scan.
The best we can do is to skip that query if the customer is not using Land Transport (specifically the DtbConsignmentAddress table, which is the vast majority of our customers).
In this test there are no DtbConsignmentAddress rows, so the main transformation query should be skipped.");
		}

		public void TestTransformationShouldNotRun_WhenDtbConsignmentAddressTableDoesNotExist()
		{
			DBTransformationTestHelper.DropFunctionIfExists("Report_LandTransportJobProfitCharge");
			DBTransformationTestHelper.DropFunctionIfExists("Report_LandTransportJobProfitSummary");
			DBTransformationTestHelper.DropFunctionIfExists("Report_TransportJobProfitSummary");
			DBTransformationTestHelper.DropFunctionIfExists("Report_MAWBJobProfitSummary");
			DBTransformationTestHelper.DropFunctionIfExists("Report_AllJobProfitSummaryWithCCBAndCAGInfo");
			DBTransformationTestHelper.DropFunctionIfExists("Report_AllJobProfitSummary");
			DBTransformationTestHelper.DropTableIfExists("DtbConsignmentActionPackageDivot");
			DBTransformationTestHelper.DropTableIfExists("DtbConsignmentLeg");
			DBTransformationTestHelper.DropTableIfExists("DtbConsignmentAction");
			DBTransformationTestHelper.DropTableIfExists("DtbConsignmentAddress");

			RunTransformationAndAssertDeleteCommandNotExecuted("The transformation should be skipped if the DtbConsignmentAddress table doesn't exist.");
		}

		void RunTransformationAndAssertDeleteCommandNotExecuted(string assertionMessage)
		{
			var transformation = GetNewTestTransformationInstance();
			AssertDeleteCommandNotExecuted(assertionMessage, transformation.Run);
		}

		static void AssertDeleteCommandNotExecuted(string assertionMessage, AnonymousMethod codeToRun)
		{
			using (Db.Connection.TrackExecutedCommands())
			{
				AssertNoExceptionThrown(codeToRun);

				var allQueries = string.Join(Environment.NewLine, Db.Connection.ExecutedCommands);

				AssertNotContains(assertionMessage, "DELETE", allQueries);
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new DeduplicateJobDocAddressForDtbConsignmentAddresses();
		}

		class DeduplicateJobDocAddressForDtbConsignmentAddressesNonTransactionedTest : TestCase
		{
			[UseSnapshotProtection]
			public void TestTrigger()
			{
				Db.Connection.BeginTransaction();
				PrepareDataForTest();
				Db.Connection.CommitTransaction();

				new DeduplicateJobDocAddressForDtbConsignmentAddresses_ForTest().Run();
			}

			class DeduplicateJobDocAddressForDtbConsignmentAddresses_ForTest : DeduplicateJobDocAddressForDtbConsignmentAddresses
			{
				protected override void OnlinePreUpgradeTransform()
				{
					base.OnlinePreUpgradeTransform();

					AssertDuplicatesDeleted();
					TryToInsertDuplicateAndAssertFailureByTrigger();
					AssertTriggerExistence("The trigger should be in place when the initial delete is run because this work is being done in the online phase, as we cannot have duplicates added before the new unique index is created.", true);
					InsertNonDuplicateAddressAndAssertSuccess("200");
				}

				protected override void OfflinePreUpgradeTransform()
				{
					AssertTriggerExistence("The trigger should exist throughout the entire offline process because removing it can be done online after the upgrade.", true);
					AssertDeleteCommandNotExecuted("The duplicate deletion query should not run offline because it is slow for the small handful of customers who actually use this table.", base.OfflinePreUpgradeTransform);
					AssertTriggerExistence("The trigger should exist throughout the entire offline process because removing it can be done online after the upgrade.", true);
				}

				protected override void OfflinePostUpgradeTransform()
				{
					AssertTriggerExistence("The trigger should exist throughout the entire offline process because removing it can be done online after the upgrade.", true);
					AssertDeleteCommandNotExecuted("The duplicate deletion query should not run offline because it is slow for the small handful of customers who actually use this table.", base.OfflinePostUpgradeTransform);
					AssertTriggerExistence("The trigger should exist throughout the entire offline process because removing it can be done online after the upgrade.", true);
				}

				protected override void OnlinePostUpgradeTransform(CancellationToken token)
				{
					AssertTriggerExistence("The trigger should exist throughout the entire offline process because removing it can be done online after the upgrade.", true);
					TryToInsertDuplicateAndAssertFailureByTrigger();
					InsertNonDuplicateAddressAndAssertSuccess("300");

					base.OnlinePostUpgradeTransform(token);

					AssertTriggerExistence("The trigger should be dropped after the schema change is done, during the online post-upgrade phase.", false);
				}
			}
		}

		static void TryToInsertDuplicateAndAssertFailureByTrigger()
		{
			var consignmentAddressPk = Db.Connection.ExecuteScalar<Guid>("SELECT TOP 1 E2_ParentId FROM dbo.JobDocAddress WHERE E2_ParentTableCode = 'LTS'");
			var insertDuplicateAddressSql = $@"
INSERT INTO dbo.JobDocAddress (E2_PK, E2_ParentID, E2_ParentTableCode, E2_AddressType, E2_Contact, E2_City, E2_State, E2_RN_NKCountryCode, E2_GeoLocation, E2_OA_Address, E2_AddressOverride ,E2_ValidationStatus, E2_CompanyName, E2_SystemCreateTimeUtc, E2_SystemCreateUser, E2_SystemLastEditTimeUtc, E2_SystemLastEditUser)
VALUES (newid(), '{consignmentAddressPk}', 'LTS', '2', 'Inserted address for consignment', 'Alexandria', 'NSW', 'AU', geography::Point(47.616, -122.360, 4326), NULL, 0, 'NRQ', '', GETUTCDATE(), 'BP', GETUTCDATE(), 'BP');";

			AssertExceptionThrown<SqlException>(
				"After the online pre-upgrade finishes there should be a trigger to prevent users from adding any new duplicates before the schema change.",
				"Unable to add more than one JobDocAddress for a DtbConsignmentAddress.",
				() => Db.Connection.ExecuteNonQuery(insertDuplicateAddressSql));
		}

		static void InsertNonDuplicateAddressAndAssertSuccess(string addressType)
		{
			var shipmentPk = Db.Connection.ExecuteScalar<Guid>("SELECT TOP 1 E2_ParentId FROM dbo.JobDocAddress WHERE E2_ParentTableCode = 'JS'");
			var insertDuplicateAddressSql = $@"
INSERT INTO dbo.JobDocAddress (E2_PK, E2_ParentID, E2_ParentTableCode, E2_AddressType, E2_Contact, E2_City, E2_State, E2_RN_NKCountryCode, E2_GeoLocation, E2_OA_Address, E2_AddressOverride ,E2_ValidationStatus, E2_CompanyName, E2_SystemCreateTimeUtc, E2_SystemCreateUser, E2_SystemLastEditTimeUtc, E2_SystemLastEditUser)
VALUES (newid(), '{shipmentPk}', 'JS', '{addressType}', 'Inserted address for shipment', 'Alexandria', 'NSW', 'AU', geography::Point(47.616, -122.360, 4326), NULL, 0, 'NRQ', '', GETUTCDATE(), 'BP', GETUTCDATE(), 'BP');";

			AssertNoExceptionThrown(
				"Inserting duplicate addresses for non-consignment addresses should continue to work as normal.",
				() => Db.Connection.ExecuteNonQuery(insertDuplicateAddressSql));
		}

		static void AssertTriggerExistence(string message, bool shouldTriggerExist)
		{
			var doesTriggerExist = Db.Connection.Exists("FROM sys.triggers WHERE type = 'TR' AND name = 'PreventDuplicateJobDocAddressesForDtbConsignmentAddress'");
			AssertEquals(message, shouldTriggerExist, doesTriggerExist);
		}
	}
}
