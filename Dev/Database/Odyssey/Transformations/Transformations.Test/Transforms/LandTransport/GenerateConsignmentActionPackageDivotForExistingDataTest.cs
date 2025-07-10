using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.LandTransport;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.LandTransport
{
	[TestedType(typeof(GenerateConsignmentActionPackageDivotForExistingData))]
	class GenerateConsignmentActionPackageDivotForExistingDataTest : DataTransformationTestCase
	{
		public void TestOnlinePostUpgradeTransform_CorrectActionPackageDivotShouldBeCreated()
		{
			TestRunAndAssertResultsTwice();
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			var transformation = new GenerateConsignmentActionPackageDivotForExistingData();
			var manager = new DummyUpgradeManager();
			transformation.Initialise(manager: manager);

			return transformation;
		}

		protected override void PrepareTestData()
		{
			var insertUnitTestDataSql = $@"
DECLARE @SYDBranchPk UNIQUEIDENTIFIER
SELECT @SYDBranchPk = GB_PK from GlbBranch WHERE GB_CODE = 'SYD'

DECLARE @address1PK UNIQUEIDENTIFIER = NEWID()
DECLARE @address2PK UNIQUEIDENTIFIER = NEWID()

DECLARE @address3PK UNIQUEIDENTIFIER = NEWID()
DECLARE @address4PK UNIQUEIDENTIFIER = NEWID()

DECLARE @address5PK UNIQUEIDENTIFIER = NEWID()
DECLARE @address6PK UNIQUEIDENTIFIER = NEWID()
DECLARE @address7PK UNIQUEIDENTIFIER = NEWID()

DECLARE @address8PK UNIQUEIDENTIFIER = NEWID()
DECLARE @address9PK UNIQUEIDENTIFIER = NEWID()
DECLARE @address10PK UNIQUEIDENTIFIER = NEWID()

DECLARE @packageJob1PK UNIQUEIDENTIFIER = NEWID()
DECLARE @packageJob2PK UNIQUEIDENTIFIER = NEWID()
DECLARE @packageJob3PK UNIQUEIDENTIFIER = NEWID()
DECLARE @packageJob4PK UNIQUEIDENTIFIER = NEWID()

INSERT INTO dbo.DtbConsignment (LTC_PK, LTC_ConsignmentType, LTC_JobID, LTC_ConnoteNumber, LTC_JobType, LTC_Status, LTC_Direction, LTC_GB_Branch, LTC_KM_Booking, LTC_IsRouteOverridden, LTC_SystemCreateTimeUtc, LTC_SystemCreateUser, LTC_SystemLastEditTimeUtc, LTC_SystemLastEditUser)
	VALUES
	('{consignment1Pk}', 'LTC', 'CN0001', 'CNNote001', 'LTL', 'CMP', 'LOC', @SYDBranchPk, NULL, 1, '2023-04-14 02:23:00', 'TST', GETUTCDATE(), 'TST'),
	('{consignment2Pk}', 'LTC', 'CN0002', 'CNNote002', 'LTL', 'CMP', 'LOC', @SYDBranchPk, NULL, 1, '2023-04-17 02:23:00', 'TST', GETUTCDATE(), 'TST'),
	('{consignment3Pk}', 'LTC', 'CN0003', 'CNNote003', 'LTL', 'CMP', 'LOC', @SYDBranchPk, NULL, 1, '2023-04-14 02:23:00', 'TST', GETUTCDATE(), 'TST'),
	('{consignment4Pk}', 'LTC', 'CN0004', 'CNNote004', 'LTL', 'CMP', 'LOC', @SYDBranchPk, NULL, 1, '2023-04-17 02:23:00', 'TST', GETUTCDATE(), 'TST');

INSERT INTO dbo.DtbConsignmentAddress (LTS_PK, LTS_LTC_Consignment, LTS_InstructionType, LTS_Sequence, LTS_Status, LTS_SystemCreateTimeUtc, LTS_SystemCreateUser, LTS_SystemLastEditTimeUtc, LTS_SystemLastEditUser)
	VALUES
	(@address1PK, '{consignment1Pk}', 'PIC', 1, 'INC', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@address2PK, '{consignment1Pk}', 'DLV', 2, 'INC', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),

	(@address3PK, '{consignment2Pk}', 'PIC', 1, 'INC', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@address4PK, '{consignment2Pk}', 'DLV', 2, 'INC', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),

	(@address5PK, '{consignment3Pk}', 'PIC', 1, 'INC', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@address6PK, '{consignment3Pk}', 'MLT', 2, 'INC', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@address7PK, '{consignment3Pk}', 'DLV', 3, 'INC', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),

	(@address8PK, '{consignment4Pk}', 'PIC', 1, 'INC', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@address9PK, '{consignment4Pk}', 'MLT', 2, 'INC', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@address10PK, '{consignment4Pk}', 'DLV', 3, 'INC', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST');

INSERT INTO dbo.DtbConsignmentAction (LTA_PK, LTA_ActionType, LTA_ReferenceNumber, LTA_SignedBy, LTA_SignedBySignature, LTA_LTS_ConsignmentAddress, LTA_K1_RunSheetInstruction, LTA_LHM_Manifest, LTA_ActualTime, LTA_EstimatedTime, LTA_RequiredFrom, LTA_RequiredTo, LTA_Slot, LTA_AutoVersion, LTA_FailureReason, LTA_FailureNotes, LTA_SystemCreateTimeUtc, LTA_SystemCreateUser, LTA_SystemLastEditTimeUtc, LTA_SystemLastEditUser, LTA_ActionID)
	VALUES
	-- Consignment without divot
	('{action1Pk}', N'PIC', N'0040155', N'', NULL, @address1PK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, N'', N'', DATEADD(DAY, 1, GETUTCDATE()), 'TST', GETUTCDATE(), 'TST', 'LTA00100001'),
	('{action2Pk}', N'DLV', N'0040156', N'', NULL, @address2PK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, N'', N'', DATEADD(DAY, 2, GETUTCDATE()), 'TST', GETUTCDATE(), 'TST', 'LTA00100002'),

	-- Consignment with divots
	('{action3Pk}', N'PIC', N'0040157', N'', NULL, @address3PK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, N'', N'', DATEADD(DAY, 3, GETUTCDATE()), 'TST', GETUTCDATE(), 'TST', 'LTA00100003'),
	('{action4Pk}', N'DLV', N'0040158', N'', NULL, @address4PK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, N'', N'', DATEADD(DAY, 4, GETUTCDATE()), 'TST', GETUTCDATE(), 'TST', 'LTA00100004'),

	-- Consignment with depot, with all divots
	('{action5Pk}', N'PIC', N'0040159', N'', NULL, @address5PK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, N'', N'', DATEADD(DAY, 5, GETUTCDATE()), 'TST', GETUTCDATE(), 'TST', 'LTA00100005'),
	('{action6Pk}', N'DLV', N'', N'', NULL, @address6PK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, N'', N'', DATEADD(DAY, 6, GETUTCDATE()), 'TST', GETUTCDATE(), 'TST', 'LTA00100006'),
	('{action7Pk}', N'PIC', N'', N'', NULL, @address6PK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, N'', N'', DATEADD(DAY, 7, GETUTCDATE()), 'TST', GETUTCDATE(), 'TST', 'LTA00100007'),
	('{action8Pk}', N'DLV', N'0040160', N'', NULL, @address7PK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, N'', N'', DATEADD(DAY, 8, GETUTCDATE()), 'TST', GETUTCDATE(), 'TST', 'LTA00100008'),

	-- Consignment with depot, partial divots
	('{action9Pk}', N'PIC', N'0040161', N'', NULL, @address8PK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, N'', N'', DATEADD(DAY, 9, GETUTCDATE()), 'TST', GETUTCDATE(), 'TST', 'LTA00100009'),
	('{action10Pk}', N'PIC', N'0040162', N'', NULL, @address8PK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, N'', N'', DATEADD(DAY, 9, GETUTCDATE()), 'TST', GETUTCDATE(), 'TST', 'LTA001000010'),
	('{action11Pk}', N'DLV', N'', N'', NULL, @address9PK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, N'', N'', DATEADD(DAY, 10, GETUTCDATE()), 'TST', GETUTCDATE(), 'TST', 'LTA001000011'),
	('{action12Pk}', N'DLV', N'', N'', NULL, @address9PK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, N'', N'', DATEADD(DAY, 10, GETUTCDATE()), 'TST', GETUTCDATE(), 'TST', 'LTA001000012'),
	('{action13Pk}', N'PIC', N'', N'', NULL, @address9PK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, N'', N'', DATEADD(DAY, 11, GETUTCDATE()), 'TST', GETUTCDATE(), 'TST', 'LTA001000013'),
	('{action14Pk}', N'DLV', N'0040163', N'', NULL, @address10PK, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, N'', N'', DATEADD(DAY, 12, GETUTCDATE()), 'TST', GETUTCDATE(), 'TST', 'LTA001000014');

INSERT INTO dbo.PkgPackageJob (KJ_PK, KJ_JobID, KJ_ParentID, KJ_ParentTableCode, KJ_IsReleased, KJ_IsFinalized, KJ_AutoVersion, KJ_SystemLastEditTimeUtc, KJ_SystemLastEditUser, KJ_SystemCreateTimeUtc, KJ_SystemCreateUser, KJ_CriticalChangesVersionID, KJ_GS_NKReleasedBy, KJ_ReleasedTimeUtc)
	VALUES
	(@packageJob1PK, 'P00001111', '{consignment1Pk}', 'LTC', 0, 0, 0, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST', NULL, N'', NULL),
	(@packageJob2PK, 'P00001112', '{consignment2Pk}', 'LTC', 0, 0, 0, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST', NULL, N'', NULL),
	(@packageJob3PK, 'P00001113', '{consignment3Pk}', 'LTC', 0, 0, 0, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST', NULL, N'', NULL),
	(@packageJob4PK, 'P00001114', '{consignment4Pk}', 'LTC', 0, 0, 0, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST', NULL, N'', NULL);

INSERT INTO dbo.PkgPackage (KP_PK, KP_KJ_ParentPackageJob, KP_PackageQty, KP_F3_NKPackType, KP_Sequence, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser)
	VALUES
	('{Package1Pk}', @packageJob1PK, {package1Quantity}, 'PLT', 1, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	('{Package2Pk}', @packageJob1PK, {package2Quantity}, 'BOX', 1, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	('{Package3Pk}', @packageJob2PK, {package3Quantity}, 'PLT', 1, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	('{Package4Pk}', @packageJob2PK, {package4Quantity}, 'BOX', 1, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	('{Package5Pk}', @packageJob3PK, {package5Quantity}, 'PLT', 1, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	('{Package6Pk}', @packageJob3PK, {package6Quantity}, 'BOX', 1, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	('{Package7Pk}', @packageJob4PK, {package7Quantity}, 'PLT', 1, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	('{Package8Pk}', @packageJob4PK, {package8Quantity}, 'BOX', 1, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST');

INSERT INTO dbo.DtbConsignmentActionPackageDivot (LTP_PK, LTP_AutoVersion, LTP_LTA_ConsignmentAction, LTP_KP_Package, LTP_PackageQuantity, LTP_SystemCreateTimeUtc, LTP_SystemCreateUser, LTP_SystemLastEditTimeUtc, LTP_SystemLastEditUser)
	VALUES             
	(NEWID(), 0, '{action3Pk}', '{Package3Pk}', {package3Quantity}, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(NEWID(), 0, '{action3Pk}', '{Package4Pk}', {package4Quantity}, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(NEWID(), 0, '{action4Pk}', '{Package3Pk}', {package3Quantity}, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(NEWID(), 0, '{action4Pk}', '{Package4Pk}', {package4Quantity}, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),

	(NEWID(), 0, '{action5Pk}', '{Package5Pk}', {package5Quantity}, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(NEWID(), 0, '{action5Pk}', '{Package6Pk}', {package6Quantity}, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(NEWID(), 0, '{action6Pk}', '{Package5Pk}', {package5Quantity}, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(NEWID(), 0, '{action6Pk}', '{Package6Pk}', {package6Quantity}, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(NEWID(), 0, '{action7Pk}', '{Package5Pk}', {package5Quantity}, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(NEWID(), 0, '{action7Pk}', '{Package6Pk}', {package6Quantity}, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(NEWID(), 0, '{action8Pk}', '{Package5Pk}', {package5Quantity}, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(NEWID(), 0, '{action8Pk}', '{Package6Pk}', {package6Quantity}, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),

	(NEWID(), 0, '{action9Pk}', '{Package7Pk}', {package7Quantity}, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(NEWID(), 0, '{action10Pk}', '{Package8Pk}', {package8Quantity}, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(NEWID(), 0, '{action11Pk}', '{Package7Pk}', {package7Quantity}, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(NEWID(), 0, '{action12Pk}', '{Package8Pk}', {package8Quantity}, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST');
	";
			TestConnection.ExecuteNonQuery(insertUnitTestDataSql);
		}

		protected override void AssertTransformationResults()
		{
			AssertActionPackageDivotsCreatedForExistingActionsWithoutDivot();
			AssertActionPackageDivotsWillNotBeCreatedForExistingActionsWithDivots_NoSplittingLeg();
			AssertActionPackageDivotsWillNotBeCreatedForExistingActionsWithDivots_SplittingLegs();
			AssertActionPackageDivotsWillBeCreatedForPartOfExistingActionsWithDivots_SplittingLegs();
		}

		void AssertActionPackageDivotsCreatedForExistingActionsWithoutDivot()
		{
			var expectedResult = new List<(Guid, Guid, int)>
			{
				(action1Pk, Package1Pk, package1Quantity),
				(action1Pk, Package2Pk, package2Quantity),
				(action2Pk, Package1Pk, package1Quantity),
				(action2Pk, Package2Pk, package2Quantity)
			};

			AssertActionPackageDivot(consignment1Pk, expectedResult);
		}

		void AssertActionPackageDivotsWillNotBeCreatedForExistingActionsWithDivots_NoSplittingLeg()
		{
			var expectedResult = new List<(Guid, Guid, int)>
			{
				(action3Pk, Package3Pk, package3Quantity),
				(action3Pk, Package4Pk, package4Quantity),
				(action4Pk, Package3Pk, package3Quantity),
				(action4Pk, Package4Pk, package4Quantity)
			};

			AssertActionPackageDivot(consignment2Pk, expectedResult);
		}

		void AssertActionPackageDivotsWillNotBeCreatedForExistingActionsWithDivots_SplittingLegs()
		{
			var expectedResult = new List<(Guid, Guid, int)>
			{
				(action5Pk, Package5Pk, package5Quantity),
				(action5Pk, Package6Pk, package6Quantity),
				(action6Pk, Package5Pk, package5Quantity),
				(action6Pk, Package6Pk, package6Quantity),
				(action7Pk, Package5Pk, package5Quantity),
				(action7Pk, Package6Pk, package6Quantity),
				(action8Pk, Package5Pk, package5Quantity),
				(action8Pk, Package6Pk, package6Quantity)
			};

			AssertActionPackageDivot(consignment3Pk, expectedResult);
		}

		void AssertActionPackageDivotsWillBeCreatedForPartOfExistingActionsWithDivots_SplittingLegs()
		{
			var expectedResult = new List<(Guid, Guid, int)>
			{
				(action9Pk, Package7Pk, package7Quantity),
				(action10Pk, Package8Pk, package8Quantity),
				(action11Pk, Package7Pk, package7Quantity),
				(action12Pk, Package8Pk, package8Quantity),
				(action13Pk, Package7Pk, package7Quantity),
				(action13Pk, Package8Pk, package8Quantity),
				(action14Pk, Package7Pk, package7Quantity),
				(action14Pk, Package8Pk, package8Quantity)
			};

			AssertActionPackageDivot(consignment4Pk, expectedResult);
		}

		void AssertActionPackageDivot(Guid consignmentPk, List<(Guid, Guid, int)> expectedResult)
		{
			var actualResult = new List<(Guid ActionPK, Guid PackagePK, int PackageQty)>();

			using (var cmd = Db.Connection.Command(string.Format(actionPackageDivotSelectionSql, consignmentPk)))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					actualResult.Add((
						reader.GetGuid(0),
						reader.GetGuid(1),
						reader.GetInt32(2)
					));
				}
			}
			AssertContainsExactElementsInAnyOrder("Expected action package divots are not created.", expectedResult, actualResult);
		}

		readonly Guid consignment1Pk = Guid.NewGuid();
		readonly Guid consignment2Pk = Guid.NewGuid();
		readonly Guid consignment3Pk = Guid.NewGuid();
		readonly Guid consignment4Pk = Guid.NewGuid();

		readonly Guid action1Pk = Guid.NewGuid();
		readonly Guid action2Pk = Guid.NewGuid();
		readonly Guid action3Pk = Guid.NewGuid();
		readonly Guid action4Pk = Guid.NewGuid();
		readonly Guid action5Pk = Guid.NewGuid();
		readonly Guid action6Pk = Guid.NewGuid();
		readonly Guid action7Pk = Guid.NewGuid();
		readonly Guid action8Pk = Guid.NewGuid();
		readonly Guid action9Pk = Guid.NewGuid();
		readonly Guid action10Pk = Guid.NewGuid();
		readonly Guid action11Pk = Guid.NewGuid();
		readonly Guid action12Pk = Guid.NewGuid();
		readonly Guid action13Pk = Guid.NewGuid();
		readonly Guid action14Pk = Guid.NewGuid();

		readonly Guid Package1Pk = Guid.NewGuid();
		readonly Guid Package2Pk = Guid.NewGuid();
		readonly Guid Package3Pk = Guid.NewGuid();
		readonly Guid Package4Pk = Guid.NewGuid();
		readonly Guid Package5Pk = Guid.NewGuid();
		readonly Guid Package6Pk = Guid.NewGuid();
		readonly Guid Package7Pk = Guid.NewGuid();
		readonly Guid Package8Pk = Guid.NewGuid();

		const int package1Quantity = 1;
		const int package2Quantity = 2;
		const int package3Quantity = 3;
		const int package4Quantity = 4;
		const int package5Quantity = 5;
		const int package6Quantity = 6;
		const int package7Quantity = 7;
		const int package8Quantity = 8;

		const string actionPackageDivotSelectionSql = @"
SELECT 
	LTP_LTA_ConsignmentAction, LTP_KP_Package, LTP_PackageQuantity
FROM dbo.DtbConsignmentActionPackageDivot Divot 
JOIN dbo.DtbConsignmentAction Action ON Divot.LTP_LTA_ConsignmentAction = Action.LTA_PK
JOIN dbo.DtbConsignmentAddress Address ON Action.LTA_LTS_ConsignmentAddress = Address.LTS_PK
AND Address.LTS_LTC_Consignment = '{0}'
";
	}
}
