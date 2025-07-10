using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Ecommerce;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Ecommerce
{
	[TestedType(typeof(SplitHVC_ReleaseStatusByImportAndExport))]
	class SplitHVC_ReleaseStatusByImportAndExportTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			CombineAssertions("HVC_ReleaseStatus is correctly split into HVC_ImportReleaseStatus and HVC_ExportReleaseStatus", () =>
			{
				AssertReleaseStatuses("Case 1: Should copy release status to export", consignment0110, "~BP", "NON", "CLR");
				AssertReleaseStatuses("Case 2: Should keep import release status NON", consignment0111, "~BP", "NON", "HLD");
				AssertReleaseStatuses("Case 3: Should copy release status to import and export", consignment1110, "~BP", "CLR", "CLR");
				AssertReleaseStatuses("Case 4: Skipped by transform", consignment0000, "B", "NON", "NON");
				AssertReleaseStatuses("Case 5: Should copy release status to import", consignment1010, "~BP", "CLR", "NON");
				AssertReleaseStatuses("Case 6: Skipped by transform", consignment0101, "B", "NON", "HLD");
				AssertReleaseStatuses("Case 7: Skipped by transform", consignment1100, "B", "NON", "NON");
				AssertReleaseStatuses("Case 8: Should copy release status to import", consignment1111, "~BP", "CLR", "HLD");
			});
		}

		void AssertReleaseStatuses(string messageText, Guid consignmentPK, string expectedLastEditUser, string expectedReleaseStatus, string expectedExportReleaseStatus)
		{
			var getResultSQL = $@"
SELECT HVC_SystemLastEditUser, HVC_ImportReleaseStatus, HVC_ExportReleaseStatus, HVI_ImportReleaseStatus, HVI_ExportReleaseStatus
FROM dbo.HVLVConsignment
	INNER JOIN dbo.HVLVItem
	ON HVC_PK = HVI_HVC_Consignment
WHERE HVC_PK = '{consignmentPK}'";
			using (var cmd = Db.Connection.Command(getResultSQL))
			using (var reader = cmd.ExecuteReader())
			{
				Assert("precondition: there should be one record", reader.Read());
				AssertEquals($"{messageText} HVC_SystemLastEditUser", expectedLastEditUser, reader["HVC_SystemLastEditUser"]);
				AssertEquals($"{messageText} HVC_ImportReleaseStatus", expectedReleaseStatus, reader["HVC_ImportReleaseStatus"]);
				AssertEquals($"{messageText} HVC_ExportReleaseStatus", expectedExportReleaseStatus, reader["HVC_ExportReleaseStatus"]);
				AssertEquals($"{messageText} HVI_ImportReleaseStatus", expectedReleaseStatus.Substring(0,1), reader["HVI_ImportReleaseStatus"]);
				AssertEquals($"{messageText} HVI_ExportReleaseStatus", expectedExportReleaseStatus.Substring(0, 1), reader["HVI_ExportReleaseStatus"]);
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new SplitHVC_ReleaseStatusByImportAndExport();
		}

		public void TestSyncTriggerIsCreated()
		{
			var triggerName = "TG_HVLVConsignment_KeepReleaseStatusesInSync";
			Assert("Trigger should not exist", !DbObjectCreator.TriggerExists(TestConnection, HVLVConsignmentSchema.Constants.TableName, triggerName));

			var transform = GetNewTestTransformationInstance();

			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			Assert("Trigger should be created.", DbObjectCreator.TriggerExists(TestConnection, HVLVConsignmentSchema.Constants.TableName, triggerName));
		}

		public void TestTriggerWorksProperly()
		{
			consignmentNumber = 0;
			// all begin default.
			var consignment1 = CreateConsignment(setImportClearanceStatus: false, setExportClearanceStatus: false, setReleaseStatus: false, setExportReleaseStatus: false);
			var consignment2 = CreateConsignment(setImportClearanceStatus: false, setExportClearanceStatus: false, setReleaseStatus: false, setExportReleaseStatus: false);
			var consignment3 = CreateConsignment(setImportClearanceStatus: false, setExportClearanceStatus: false, setReleaseStatus: false, setExportReleaseStatus: false);

			var mockManager = new Mock<IUpgradeManager>();
			mockManager.Setup(m => m.ShowInfoMessage(It.IsAny<string>()));
			mockManager.Setup(m => m.StartSubtask(It.IsAny<string>()));

			var transform = GetNewTestTransformationInstance();
			transform.Initialise(manager: mockManager.Object);
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			CombineAssertions(() =>
			{
				// standard usage: Update HVC_ExportCustomsClearanceStatus and HVC_ReleaseStatus
				UpdateConsignmentAndItem(consignment1, "HVC_ExportCustomsClearanceStatus = 'A', HVC_ReleaseStatus = 'CLR'", "HVI_ReleaseStatus = 'C'");
				AssertReleaseStatuses("Trigger should sync to export consignment1", consignment1, "~BP", "NON", "CLR");

				// standard usage 2: Update HVC_ExportCustomsClearanceStatus updates both HVC_ReleaseStatus and HVC_ExportReleaseStatus
				UpdateConsignmentAndItem(consignment2, "HVC_ExportCustomsClearanceStatus = 'A', HVC_ReleaseStatus = 'CLR', HVC_ExportReleaseStatus = 'CLR'", "HVI_ReleaseStatus = 'C', HVI_ExportReleaseStatus = 'C'");
				AssertReleaseStatuses("Trigger sync consignment2 for export", consignment2, "~BP", "NON", "CLR");

				// standard usage 3: update HVC_ImportCustomsClearanceStatus and HVC_ReleaseStatus
				UpdateConsignmentAndItem(consignment3, "HVC_ImportCustomsClearanceStatus = 'A', HVC_ReleaseStatus = 'CLR'", "HVI_ReleaseStatus = 'C'");
				AssertReleaseStatuses("Trigger should sync import release status consignment3", consignment3, "~BP", "CLR", "NON");
			});
		}

		public void TestCreateColumnsIfNecessary()
		{
			DBTransformationTestHelper.DropFunctionIfExists("Report_Client_eTailShipmentProfile");
			DBTransformationTestHelper.DropFunctionIfExists("Report_eTailShipmentProfile");
			DBTransformationTestHelper.DropViewIfExists("HVLVShipmentItemsCountView");
			DBTransformationTestHelper.DropConstraintIfExists(HVLVConsignmentSchema.Constants.TableName, $"Constraint_{HVLVConsignmentSchema.Constants.HVC_ImportReleaseStatus}");
			DBTransformationTestHelper.DropConstraintIfExists(HVLVConsignmentSchema.Constants.TableName, $"Constraint_{HVLVConsignmentSchema.Constants.HVC_ExportReleaseStatus}");
			DBTransformationTestHelper.DropConstraintIfExists(HVLVItemSchema.Constants.TableName, $"Constraint_{HVLVItemSchema.Constants.HVI_ImportReleaseStatus}");
			DBTransformationTestHelper.DropConstraintIfExists(HVLVItemSchema.Constants.TableName, $"Constraint_{HVLVItemSchema.Constants.HVI_ExportReleaseStatus}");

			DBTransformationTestHelper.DropDefaultConstraintFor(HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.Constants.HVC_ImportReleaseStatus);
			DBTransformationTestHelper.DropDefaultConstraintFor(HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.Constants.HVC_ExportReleaseStatus);
			DBTransformationTestHelper.DropDefaultConstraintFor(HVLVItemSchema.Constants.TableName, HVLVItemSchema.Constants.HVI_ImportReleaseStatus);
			DBTransformationTestHelper.DropDefaultConstraintFor(HVLVItemSchema.Constants.TableName, HVLVItemSchema.Constants.HVI_ExportReleaseStatus);

			DBTransformationTestHelper.DropIndexIfExists(HVLVItemSchema.Constants.TableName, "FK_RX__HVI_JS_LoadedOnShipment");

			DBTransformationTestHelper.DropColumnIfExists(HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.Constants.HVC_ImportReleaseStatus);
			DBTransformationTestHelper.DropColumnIfExists(HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.Constants.HVC_ExportReleaseStatus);
			DBTransformationTestHelper.DropColumnIfExists(HVLVItemSchema.Constants.TableName, HVLVItemSchema.Constants.HVI_ImportReleaseStatus);
			DBTransformationTestHelper.DropColumnIfExists(HVLVItemSchema.Constants.TableName, HVLVItemSchema.Constants.HVI_ExportReleaseStatus);

			AssertNoExceptionThrown(RunTransformation);
		}

		protected override void PrepareTestData()
		{
			consignment0110 = CreateConsignment(setImportClearanceStatus: false, setExportClearanceStatus: true, setReleaseStatus: true, setExportReleaseStatus: false);
			consignment0111 = CreateConsignment(setImportClearanceStatus: false, setExportClearanceStatus: true, setReleaseStatus: true, setExportReleaseStatus: true);
			consignment1110 = CreateConsignment(setImportClearanceStatus: true, setExportClearanceStatus: true, setReleaseStatus: true, setExportReleaseStatus: false);

			consignment0000 = CreateConsignment(setImportClearanceStatus: false, setExportClearanceStatus: false, setReleaseStatus: false, setExportReleaseStatus: false);
			consignment1010 = CreateConsignment(setImportClearanceStatus: true, setExportClearanceStatus: false, setReleaseStatus: true, setExportReleaseStatus: false);
			consignment0101 = CreateConsignment(setImportClearanceStatus: false, setExportClearanceStatus: true, setReleaseStatus: false, setExportReleaseStatus: true);
			consignment1100 = CreateConsignment(setImportClearanceStatus: true, setExportClearanceStatus: true, setReleaseStatus: false, setExportReleaseStatus: false);
			consignment1111 = CreateConsignment(setImportClearanceStatus: true, setExportClearanceStatus: true, setReleaseStatus: true, setExportReleaseStatus: true);
		}

		Guid CreateConsignment(bool setImportClearanceStatus, bool setExportClearanceStatus, bool setReleaseStatus, bool setExportReleaseStatus)
		{
			var testDataCreator = new TransformationTestDataCreator();

			if (bookingHeaderPK == default)
			{
				var orgAddressPK = (Guid)Db.Connection.ExecuteScalar(@"SELECT TOP 1 OA_PK FROM dbo.OrgAddress");
				bookingHeaderPK = testDataCreator.CreateHVLVBookingHeader(orgAddressPK, "M00000112", "B", 1);
			}
			consignmentNumber++;
			var consignmentPK = testDataCreator.CreateHVLVConsignment(bookingHeaderPK, $"HVC0000{consignmentNumber}", $"HVC0000{consignmentNumber}", "B", 1, consignmentNumber);
			testDataCreator.CreateHVLVItem(consignmentPK, "", $"HVI0000{consignmentNumber}", consignmentNumber);
			UpdateCustomsClearanceStatusAndReleaseStatus(consignmentPK, setImportClearanceStatus ? "A" : "", setExportClearanceStatus ? "A" : "", setReleaseStatus ? "CLR" : "NON", setExportReleaseStatus ? "HLD" : "NON");
			return consignmentPK;
		}

		void UpdateCustomsClearanceStatusAndReleaseStatus(Guid pk, string importClearanceStatus, string exportClearanceStatus, string releaseStatus, string exportReleaseStatus)
		{
			var consignmentSetClause = @"HVC_ImportCustomsClearanceStatus = @importCustomsClearanceStatus,
										HVC_ExportCustomsClearanceStatus = @exportCustomsClearanceStatus,
										HVC_ReleaseStatus = @releaseStatus,
										HVC_ExportReleaseStatus = @exportReleaseStatus";
			var itemSetClause = @"HVI_ReleaseStatus = LEFT(@releaseStatus, 1),
								HVI_ExportReleaseStatus = LEFT(@exportReleaseStatus, 1)";
			var updateSQL = GetUpdateConsignmentAndItemSQL(consignmentSetClause, itemSetClause);
			using (DbCommand command = Db.Connection.Command(updateSQL))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@importCustomsClearanceStatus", SqlDbType.VarChar, HVLVConsignmentSchema.HVC_ImportCustomsClearanceStatus.MaxLength, importClearanceStatus);
				command.AddParameter("@exportCustomsClearanceStatus", SqlDbType.VarChar, HVLVConsignmentSchema.HVC_ExportCustomsClearanceStatus.MaxLength, exportClearanceStatus);
				command.AddParameter("@releaseStatus", SqlDbType.VarChar, HVLVConsignmentSchema.HVC_ImportReleaseStatus.MaxLength, releaseStatus);
				command.AddParameter("@exportReleaseStatus", SqlDbType.VarChar, HVLVConsignmentSchema.HVC_ExportReleaseStatus.MaxLength, exportReleaseStatus);
				command.ExecuteNonQuery();
			}
		}

		void UpdateConsignmentAndItem(Guid pk, string consignmentSetClause, string itemSetClause)
		{
			var updateSQL = GetUpdateConsignmentAndItemSQL(consignmentSetClause, itemSetClause);
			using (DbCommand command = Db.Connection.Command(updateSQL))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.ExecuteNonQuery();
			}
		}

		string GetUpdateConsignmentAndItemSQL(string consignmentSetClause, string itemSetClause) => @$"
UPDATE dbo.HVLVConsignment
SET {consignmentSetClause},
	HVC_SystemLastEditTimeUtc = HVC_SystemLastEditTimeUtc,
	HVC_SystemLastEditUser = 'B'
WHERE HVC_PK = @pk

UPDATE dbo.HVLVItem
SET {itemSetClause},
	HVI_SystemLastEditTimeUtc = HVI_SystemLastEditTimeUtc,
	HVI_SystemLastEditUser = 'B'
WHERE HVI_HVC_Consignment = @pk
";

		// Labelling Key: consignmentXXXX : when x = 0, value is default, when x=1 value is non-default
		// x position meaning:
		// 1: HVC_ImportCustomsClearanceStatus
		// 2: HVC_ExportCustomsClearanceStatus
		// 3: HVC_ReleaseStatus
		// 4: HVC_ExportReleaseStatus

		// these should update: HVC_ExportCustomsClearanceStatus and HVC_ReleaseStatus filled, at least one of the other 2 fields is default.
		Guid consignment0110;
		Guid consignment0111;
		Guid consignment1110;

		// common cases that shouldn't update at all by transform
		Guid consignment0000; // default
		Guid consignment0101; // export-only
		Guid consignment1100; // nothing to copy

		// common cases that transform will process but shouldn't change values:
		Guid consignment1010; // import-only
		Guid consignment1111; // already filled

		int consignmentNumber;
		Guid bookingHeaderPK;
	}
}
