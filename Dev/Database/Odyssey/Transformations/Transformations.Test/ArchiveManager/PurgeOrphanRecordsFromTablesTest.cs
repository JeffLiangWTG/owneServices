using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.Abstractions;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ArchiveManager;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ArchiveManager
{
	[TestedType(typeof(PurgeOrphanRecordsFromTablesSecondVersion))]
	[UseSnapshotProtection]
	internal class PurgeOrphanRecordsFromTablesTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new PurgeOrphanRecordsFromTablesSecondVersion();

		protected override void PrepareTestData()
		{
			var insertSql = @"
				DECLARE @TestDate datetime = '2000-01-01 00:00:00.000';
				DECLARE @PreviousTestDate datetime = DATEADD(DAY, -1, @TestDate);
				DECLARE @CompanyPK uniqueidentifier = (SELECT TOP 1 GC_PK FROM dbo.GlbCompany);

				--Delete data before 2000-01-01 and insert test data before this time point
				DELETE FROM dbo.JobShipment WHERE JS_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.CusCAeMHItem WHERE BX_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.CusCAeMHHouse WHERE BW_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.CusCAeMHMaster WHERE BP_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.AccBillingItem WHERE ABI_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.AccBillingHeader WHERE ABH_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.AccChargeSupplyTypeOverride WHERE ACS_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.AccChargeTaxOverride WHERE AO_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.AccCommissionLine WHERE CL0_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.OrgSalesValueAssociationPivot WHERE SVP_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.GenCustomAddOnValue WHERE XV_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.JobComInvHeaderCharge WHERE J7_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.CusInvPack WHERE B5_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.ExportAWBHeader WHERE EH_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.LandCostInput WHERE LI_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.LandedCostHeader WHERE LT_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.GenAddOnColumn WHERE XA_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.GenPivot WHERE XX_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.BMBoardSectionChannel WHERE MSC_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.GlbStaff WHERE GS_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM JobConversationMessage WHERE JCM_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM JobConversationParticipant WHERE JCP_SystemCreateTimeUtc <= @TestDate;

				/*
					TEST DATA THAT SHOULD BE ERASED BY THIS TRANSFORMATION
				*/

				--Orphan AccBillingHeader with a child (both [2] should be eraseed)
				DECLARE @PK1 uniqueidentifier = NEWID();
				INSERT INTO AccBillingHeader(ABH_PK, ABH_ParentId, ABH_ParentTableCode, ABH_BillingCode, ABH_BillingCounter, ABH_EventType, ABH_ParentReferenceNumber, ABH_EventTimeUtc, ABH_GC_Company, ABH_SystemCreateTimeUtc, ABH_SystemCreateUser, ABH_SystemLastEditTimeUtc, ABH_SystemLastEditUser)
				VALUES(@PK1, NewID(), 'JK', 'GSH', 1, 'PST', 'TestReferID', @PreviousTestDate, @CompanyPK, @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				INSERT INTO AccBillingItem(ABI_PK, ABI_ParentId, ABI_ParentTableCode, ABI_ABH, ABI_ParentReferenceNumber, ABI_SystemCreateTimeUtc, ABI_SystemCreateUser, ABI_SystemLastEditTimeUtc, ABI_SystemLastEditUser)
				VALUES(NewID(), NewID(), 'JS',  @PK1, 'TestReferID', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');

				--Orphan LandedCostHeader with a child (both [2] should be eraseed)
				DECLARE @PK2 uniqueidentifier = NEWID();
				INSERT INTO LandedCostHeader(LT_PK, LT_ParentID, LT_ParentTableCode, LT_GC, LT_SystemCreateTimeUtc, LT_SystemCreateUser, LT_SystemLastEditTimeUtc, LT_SystemLastEditUser, LT_ClusterKey)
				VALUES(@PK2, NewID(), 'JE', @CompanyPK, @PreviousTestDate, '~BP', @PreviousTestDate, '~BP', 1);
				INSERT INTO LandCostInput(LI_PK, LI_ParentID, LI_ParentTableCode, LI_LT, LI_SystemCreateTimeUtc, LI_SystemCreateUser, LI_SystemLastEditTimeUtc, LI_SystemLastEditUser, LI_ClusterKey)
				VALUES(NewID(), NewID(), 'JZ', @PK2, @PreviousTestDate, '~BP', @PreviousTestDate, '~BP', 1);
								
				--Simple/assorted orphans (Count: 10)
				INSERT INTO AccChargeSupplyTypeOverride(ACS_PK, ACS_ParentTableCode, ACS_ParentID, ACS_JobType, ACS_TransportMode, ACS_Direction, ACS_IncoTerm, ACS_SupplyType, ACS_SystemCreateTimeUtc, ACS_SystemCreateUser, ACS_SystemLastEditTimeUtc, ACS_SystemLastEditUser)
				VALUES(NewID(), 'AC', NewID(), 'STO', 'COU', 'OTH', 'DPU', 'DSB', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				INSERT INTO AccChargeTaxOverride(AO_PK, AO_ParentTableCode, AO_ParentID, AO_Direction, AO_IncoTerm, AO_JobType, AO_CostSellAll, AO_Origin, AO_Destination, AO_TaxRegCntryOrGroup, AO_SystemCreateTimeUtc, AO_SystemCreateUser, AO_SystemLastEditTimeUtc, AO_SystemLastEditUser)
				VALUES(NewID(), 'AX', NewID(), 'ALL', 'ALL', 'ALL', 'ALL', 'ALL', 'ALL', 'ALL', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				INSERT INTO AccCommissionLine(CL0_PK, CL0_ParentID, CL0_ParentTableCode, CL0_SystemCreateTimeUtc, CL0_SystemCreateUser, CL0_SystemLastEditTimeUtc, CL0_SystemLastEditUser)
				VALUES(NewID(), NewID(), 'CH0', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				INSERT INTO OrgSalesValueAssociationPivot(SVP_PK, SVP_TradeId, SVP_TradeTableCode, SVP_ActivityId, SVP_ActivityTableCode, SVP_SystemCreateTimeUtc, SVP_SystemCreateUser, SVP_SystemLastEditTimeUtc, SVP_SystemLastEditUser)
				VALUES(NewID(), NewID(), 'PA', NewID(), 'TI', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				INSERT INTO GenCustomAddOnValue(XV_PK, XV_ParentTableCode, XV_ParentID, XV_Type, XV_Data, XV_SystemCreateTimeUtc, XV_SystemCreateUser, XV_SystemLastEditTimeUtc, XV_SystemLastEditUser)
				VALUES(NewID(), 'BQ', NewID(), 'BOO', 'Y', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				INSERT INTO JobComInvHeaderCharge(J7_PK, J7_ParentID, J7_ParentTableCode, J7_SystemCreateTimeUtc, J7_SystemCreateUser, J7_SystemLastEditTimeUtc, J7_SystemLastEditUser)
				VALUES(NewID(), NewID(), 'JZ', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				INSERT INTO ExportAWBHeader(EH_PK, EH_Table, EH_ParentID, EH_SystemCreateTimeUtc, EH_SystemCreateUser, EH_SystemLastEditTimeUtc, EH_SystemLastEditUser)
				VALUES(NewID(), 'JobShipment', NewID(), @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				INSERT INTO CusInvPack(B5_PK, B5_ParentID, B5_ParentTableCode, B5_SystemCreateTimeUtc, B5_SystemCreateUser, B5_SystemLastEditTimeUtc, B5_SystemLastEditUser)
				VALUES(NewID(), NewID(), 'JI', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				INSERT INTO GenAddOnColumn(XA_PK, XA_ParentTableCode, XA_ParentID, XA_SystemCreateTimeUtc, XA_SystemCreateUser, XA_SystemLastEditTimeUtc, XA_SystemLastEditUser)
				VALUES(NewID(), 'JE', NewID(), @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				INSERT INTO GenPivot(XX_PK, XX_Relation1ID, XX_Relation1TableCode, XX_Relation2ID, XX_Relation2TableCode, XX_SystemCreateTimeUtc, XX_SystemCreateUser, XX_SystemLastEditTimeUtc, XX_SystemLastEditUser)
				VALUES(NewID(), NewID(), 'LA', NewID(), 'B7', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				INSERT INTO OrgCommissionAgreementItem(CAI_PK, CAI_ParentID, CAI_ParentTableCode, CAI_Code, CAI_Type, CAI_SystemCreateTimeUtc, CAI_SystemCreateUser, CAI_SystemLastEditTimeUtc, CAI_SystemLastEditUser)
				VALUES(NewID(), NewID(), 'CAI', 'ALL', 'MDL', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');

				--Test cases under multi-layer foreign key constraints
				DECLARE @PK3 uniqueidentifier = NEWID();
				DECLARE @PK4 uniqueidentifier = NEWID();
				INSERT INTO JobConversation(JCC_PK, JCC_ParentTableCode, JCC_ParentID)
				VALUES(@PK3, 'JS', NEWID());
				INSERT INTO JobConversationParticipant(JCP_PK, JCP_ParticipantID, JCP_ParticipantTableCode, JCP_JCC_Conversation, JCP_SystemCreateTimeUtc, JCP_SystemCreateUser, JCP_SystemLastEditTimeUtc, JCP_SystemLastEditUser)
				VALUES(@PK4, NEWID(), 'OC', @PK3, @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				INSERT INTO JobConversationMessage(JCM_PK, JCM_JCC_Conversation, JCM_JCP_Participant, JCM_PostedTimeUtc, JCM_SystemCreateTimeUtc, JCM_SystemCreateUser, JCM_SystemLastEditTimeUtc, JCM_SystemLastEditUser, JCM_Body, JCM_Language)
				VALUES(NEWID(), @PK3, @PK4, @PreviousTestDate, @PreviousTestDate, '~BP', @PreviousTestDate, '~BP', 'TEST', 'TEST');

				DECLARE @PK5 uniqueidentifier = NEWID();
				DECLARE @PK6 uniqueidentifier = NEWID();
				INSERT INTO dbo.CusCAeMHMaster(BP_PK, BP_ParentID, BP_ParentTableCode, BP_ModeOfTransport, BP_MasterBill, BP_GB_Branch, BP_SystemCreateTimeUtc, BP_SystemCreateUser, BP_SystemLastEditTimeUtc, BP_SystemLastEditUser)
				VALUES(@PK5, NewID(), 'JK', 'SEA', 'TestBill0001', (SELECT TOP 1 GB_PK FROM dbo.GlbBranch), @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				INSERT INTO CusCAeMHHouse(BW_PK, BW_HouseBill, BW_BP_Master, BW_MessageReference, BW_SystemCreateTimeUtc, BW_SystemCreateUser, BW_SystemLastEditTimeUtc, BW_SystemLastEditUser)
				VALUES(@PK6, 'TESTBILL00001', @PK5, 'TestS00001', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				INSERT INTO CusCAeMHItem(BX_PK, BX_BW_House, BX_Quantity, BX_LineNumber, BX_SystemCreateTimeUtc, BX_SystemCreateUser, BX_SystemLastEditTimeUtc, BX_SystemLastEditUser)
				VALUES(NewID(), @PK6, 1, 1, @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');

				--Test cases for CusInvPack under self associated foreign key constraints(should be erased)
				DECLARE @PK8 uniqueidentifier = NEWID();
				DECLARE @PK9 uniqueidentifier = NEWID();
				INSERT INTO CusInvPack(B5_PK, B5_ParentID, B5_ParentTableCode, B5_SystemCreateTimeUtc, B5_SystemCreateUser, B5_SystemLastEditTimeUtc, B5_SystemLastEditUser)
				VALUES(@PK8, NewID(), 'JI', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				INSERT INTO CusInvPack(B5_PK, B5_ParentID, B5_ParentTableCode, B5_B5_ParentPackage, B5_SystemCreateTimeUtc, B5_SystemCreateUser, B5_SystemLastEditTimeUtc, B5_SystemLastEditUser)
				VALUES(@PK9, NewID(), 'JI', @PK8, @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				INSERT INTO CusInvPack(B5_PK, B5_ParentID, B5_ParentTableCode, B5_B5_ParentPackage, B5_SystemCreateTimeUtc, B5_SystemCreateUser, B5_SystemLastEditTimeUtc, B5_SystemLastEditUser)
				VALUES(NEWID(), NewID(), 'JI', @PK9, @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');

				--Setup for BMBoardSectionChannel Test Cases
				DROP TABLE IF EXISTS dbo.ClientGlbStaff;
				CREATE TABLE [dbo].[ClientGlbStaff] ([GS_PK] [uniqueidentifier] NOT NULL,[GS_LoginName] [nvarchar](104) NOT NULL, [GS_Code] [varchar](3) NOT NULL, [GS_SystemCreateTimeUtc] [datetime] NULL, [GS_SystemCreateUser] [varchar](3) NOT NULL, [GS_SystemLastEditTimeUtc] [datetime] NULL,[GS_SystemLastEditUser] [varchar](3) NOT NULL)
				
				--Test Cases for tables which have a same PK Column and they are orphan For BMBoardSectionChannel
				DECLARE @myidForDummyOrphanBMBoardSection uniqueidentifier
				SET @myidForDummyOrphanBMBoardSection = NEWID()
				INSERT INTO dbo.BMBoardSection(MS_PK, MS_SystemCreateTimeUtc, MS_SystemCreateUser, MS_SystemLastEditTimeUtc, MS_SystemLastEditUser)
				VALUES(@myidForDummyOrphanBMBoardSection, @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');

				INSERT INTO dbo.BMBoardSectionChannel(MSC_PK, MSC_ParentID, MSC_ParentTableCode, MSC_MS_Section, MSC_ChannelType, MSC_SystemCreateTimeUtc, MSC_SystemCreateUser, MSC_SystemLastEditTimeUtc, MSC_SystemLastEditUser)
				VALUES(NewID(), NEWID(), 'GS', @myidForDummyOrphanBMBoardSection, 'RES', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');

				--Orphan PkgPackageJob should delete
				DECLARE @myidForParentDummyBizo uniqueidentifier  
				SET @myidForParentDummyBizo = NEWID()
				INSERT INTO DummyBizo (Z0_PK)
				VALUES (@myidForParentDummyBizo);

				DECLARE @myidForValidPkgPackageJob uniqueidentifier
				SET @myidForValidPkgPackageJob = NEWID()
				INSERT INTO dbo.PkgPackageJob(KJ_PK, KJ_ParentID, KJ_ParentTableCode, KJ_JobID, KJ_SystemCreateTimeUtc, KJ_SystemCreateUser, KJ_SystemLastEditTimeUtc, KJ_SystemLastEditUser)
				VALUES(@myidForValidPkgPackageJob, @myidForParentDummyBizo, 'Z0', 'P00000001', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				
				DECLARE @myidForOrphanPkgPackageJob uniqueidentifier
				SET @myidForOrphanPkgPackageJob = NEWID()
				INSERT INTO dbo.PkgPackageJob(KJ_PK, KJ_ParentID, KJ_ParentTableCode, KJ_JobID, KJ_SystemCreateTimeUtc, KJ_SystemCreateUser, KJ_SystemLastEditTimeUtc, KJ_SystemLastEditUser)
				VALUES(@myidForOrphanPkgPackageJob, NEWID(), 'KB', 'P00000002', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');

				DECLARE @myidForOrphanPkgPackage uniqueidentifier
				SET @myidForOrphanPkgPackage = NEWID()
				INSERT INTO dbo.PkgPackage(KP_PK, KP_KJ_ParentPackageJob, KP_F3_NKPackType, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser)
				VALUES(@myidForOrphanPkgPackage, @myidForOrphanPkgPackageJob, 'PKG', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');

				DECLARE @myidForValidPkgPackage uniqueidentifier
				SET @myidForValidPkgPackage = NEWID()
				INSERT INTO dbo.PkgPackage(KP_PK, KP_KJ_ParentPackageJob, KP_KP_ParentPackage, KP_F3_NKPackType, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser)
				VALUES(@myidForValidPkgPackage, @myidForValidPkgPackageJob, @myidForOrphanPkgPackage, 'PKG', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');

				INSERT INTO dbo.PkgPackageHandlingUnitDivot(KPD_PK, KPD_KP_Package, KPD_KP_HandlingUnit, KPD_SystemCreateTimeUtc, KPD_SystemCreateUser, KPD_SystemLastEditTimeUtc, KPD_SystemLastEditUser)
				VALUES(NEWID(), @myidForValidPkgPackage, @myidForValidPkgPackage, @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');

				--Test records that do not have ITableSchema are still purged
				DROP TRIGGER TG_AccJobConfig_InsertUpdate;
				INSERT INTO AccJobConfig(JCF_PK, JCF_ParentId, JCF_ParentTableCode, JCF_GC, JCF_ServiceDirection, JCF_TransportMode, JCF_ConfigType, JCF_SystemCreateTimeUtc, JCF_SystemCreateUser, JCF_SystemLastEditTimeUtc, JCF_SystemLastEditUser)
				VALUES(NEWID(), NEWID(), 'OH', @CompanyPK, 'ALL', 'ALL', 'CFX', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');

				--Test records under orphan with cascade delete do not throw reference constraint error
				DECLARE @myidForOrphanPkgPackageJob2 uniqueidentifier
				SET @myidForOrphanPkgPackageJob2 = NEWID()
				INSERT INTO dbo.PkgPackageJob(KJ_PK, KJ_ParentID, KJ_ParentTableCode, KJ_JobID, KJ_SystemCreateTimeUtc, KJ_SystemCreateUser, KJ_SystemLastEditTimeUtc, KJ_SystemLastEditUser)
				VALUES(@myidForOrphanPkgPackageJob2, NEWID(), 'KB', 'P00000003', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');

				DECLARE @myidForOrphanPkgPackage2 uniqueidentifier
				SET @myidForOrphanPkgPackage2 = NEWID()
				INSERT INTO dbo.PkgPackage(KP_PK, KP_KJ_ParentPackageJob, KP_F3_NKPackType, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser)
				VALUES(@myidForOrphanPkgPackage2, @myidForOrphanPkgPackageJob2, 'PKG', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');

				ALTER TABLE DtbBookingInstructionPkgDivot DROP CONSTRAINT DtbBookingInstructionPkgDivot_KD_KN_BookingInstruction_FK2_DtbBookingInstruction_CRR_120N ;
				DECLARE @myidForDtbBookingInstructionPkgDivot uniqueidentifier
				SET @myidForDtbBookingInstructionPkgDivot = NEWID()
				INSERT INTO dbo.DtbBookingInstructionPkgDivot(KD_PK, KD_KP_Package, KD_KN_BookingInstruction, KD_SystemCreateTimeUtc, KD_SystemCreateUser, KD_SystemLastEditTimeUtc, KD_SystemLastEditUser)
				VALUES(@myidForDtbBookingInstructionPkgDivot, @myidForOrphanPkgPackage2, NewID(), @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');

				ALTER TABLE DtbBookingConfirmation DROP CONSTRAINT DtbBookingConfirmation_KK_KN_BookingInstruction_FK2_DtbBookingInstruction_RRR_120N ;
				INSERT INTO dbo.DtbBookingConfirmation(KK_PK, KK_KD_BookingInstructionPkgDivot, KK_KN_BookingInstruction, KK_SystemCreateTimeUtc, KK_SystemCreateUser, KK_SystemLastEditTimeUtc, KK_SystemLastEditUser)
				VALUES(NEWID(), @myidForDtbBookingInstructionPkgDivot, NewID(), @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');

				/*
					TEST DATA THAT SHOULD *NOT* BE ERASED BY THIS TRANSFORMATION (VALID DATA)
				*/

				--Valid record that should not be erased because ExportAWBHeader has valid JobShipment parent
				DECLARE @PK7 uniqueidentifier = NEWID();
				INSERT dbo.JobShipment(JS_PK, JS_UniqueConsignRef, JS_IsShipping, JS_IsForwardRegistered, JS_IsCFSRegistered, JS_SystemCreateTimeUtc, JS_SystemCreateUser, JS_SystemLastEditTimeUtc, JS_SystemLastEditUser)
				VALUES(@PK7, 'SHPTest99901', 1, 0, 0, @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				INSERT INTO ExportAWBHeader(EH_PK, EH_Table, EH_ParentID, EH_SystemCreateTimeUtc, EH_SystemCreateUser, EH_SystemLastEditTimeUtc, EH_SystemLastEditUser)
				VALUES(NewID(), 'JobShipment', @PK7, @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');

				--Valid OrgCommissionAgreementItem should not be deleted because parent OrgCommissionAgreementItem is valid
				ALTER TABLE OrgCommissionAgreement DROP CONSTRAINT OrgCommissionAgreement_CA0_OH_Customer_FK2_OrgHeader_RRR_120N;
				ALTER TABLE OrgCommissionAgreement DROP CONSTRAINT OrgCommissionAgreement_CA0_P8_FK2_OrgOpportunity_CRR_120N;
				DECLARE @validOrgCommissionAgreement uniqueidentifier = NEWID();
				INSERT INTO OrgCommissionAgreement(CA0_PK, CA0_P8, CA0_OH_Customer, CA0_CommissionTriggerType, CA0_CommissionBasis, CA0_Name, CA0_SystemCreateTimeUtc, CA0_SystemCreateUser, CA0_SystemLastEditTimeUtc, CA0_SystemLastEditUser)
				VALUES(@ValidOrgCommissionAgreement, NewID(), NewID(), 'CCD', 'ABC', 'ABC', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				
				DECLARE @validOrgCommissionAgreementItem uniqueidentifier = NEWID();
				INSERT INTO OrgCommissionAgreementItem(CAI_PK, CAI_ParentID, CAI_ParentTableCode, CAI_Code, CAI_Type, CAI_SystemCreateTimeUtc, CAI_SystemCreateUser, CAI_SystemLastEditTimeUtc, CAI_SystemLastEditUser)
				VALUES(@validOrgCommissionAgreementItem, @validOrgCommissionAgreement, 'CA0', 'ALL', 'MDL', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				INSERT INTO OrgCommissionAgreementItem(CAI_PK, CAI_ParentID, CAI_ParentTableCode, CAI_Code, CAI_Type, CAI_SystemCreateTimeUtc, CAI_SystemCreateUser, CAI_SystemLastEditTimeUtc, CAI_SystemLastEditUser)
				VALUES(NewID(), @validOrgCommissionAgreementItem, 'CAI', 'ALL', 'MDL', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');

				--Test Cases for tables which have a same PK Column and they are not orphan For BMBoardSectionChannel
				DECLARE @myidForDummyBMBoardSection uniqueidentifier
				SET @myidForDummyBMBoardSection = NEWID()
				INSERT INTO dbo.BMBoardSection(MS_PK, MS_SystemCreateTimeUtc, MS_SystemCreateUser, MS_SystemLastEditTimeUtc, MS_SystemLastEditUser)
				VALUES(@myidForDummyBMBoardSection, @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');

				DECLARE @myidForValidGlbStaff uniqueidentifier  
				SET @myidForValidGlbStaff = NEWID()
				INSERT INTO
					GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				VALUES
					(@myidForValidGlbStaff, 'user002_', 'UUX', @PreviousTestDate, 'UUX', GETUTCDATE(), 'UUX');
				INSERT INTO dbo.BMBoardSectionChannel(MSC_PK, MSC_ParentID, MSC_ParentTableCode, MSC_MS_Section, MSC_ChannelType, MSC_SystemCreateTimeUtc, MSC_SystemCreateUser, MSC_SystemLastEditTimeUtc, MSC_SystemLastEditUser)
				VALUES(NewID(), @myidForValidGlbStaff, 'GS', @myidForDummyBMBoardSection, 'RES', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');

				INSERT INTO
					ClientGlbStaff (GS_PK, GS_LoginName, GS_Code, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				VALUES
					(@myidForValidGlbStaff, 'user003_', 'UUX',@PreviousTestDate, 'UUX', GETUTCDATE(), 'UUX');
				INSERT INTO dbo.BMBoardSectionChannel(MSC_PK, MSC_ParentID, MSC_ParentTableCode, MSC_MS_Section, MSC_ChannelType, MSC_SystemCreateTimeUtc, MSC_SystemCreateUser, MSC_SystemLastEditTimeUtc, MSC_SystemLastEditUser)
				VALUES(NewID(), @myidForValidGlbStaff, 'GS', @myidForDummyBMBoardSection, 'RES', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
			";

			TestConnection.ExecuteNonQuery(insertSql);

			AssertEquals(3, FindTableCount("BMBoardSectionChannel", "MSC_SystemCreateTimeUtc"));
			AssertEquals(3, FindTableCount("OrgCommissionAgreementItem", "CAI_SystemCreateTimeUtc"));
			AssertEquals(2, FindTableCount("ExportAWBHeader", "EH_SystemCreateTimeUtc"));
			AssertEquals(1, FindTableCount("PkgPackageHandlingUnitDivot", "KPD_SystemCreateTimeUtc"));
		}

		class TableAndTimeColumn
		{
			public string TableName { get; set; }
			public string TimeColumnName { get; set; }
		}

		List<TableAndTimeColumn> GetAssertTableList()
		{
			var purgeTable = new PurgeOrphanRecordsFromTablesSecondVersion();
			var tableList = purgeTable.GetOrphanDataTablesForDeletion();
			var result = new List<TableAndTimeColumn>();
			foreach (var table in tableList)
			{
				var schema = GlobalServiceProvider.Instance.GetRequiredService<IApplicationSchemaResolver>().GetTableSchema(table.TableName);
				if (schema != null)
				{
					var allColumns = schema.All;
					var timeColumn = allColumns.Select(c => c.Name)
						.FirstOrDefault(x => x.EndsWith("SystemCreateTimeUtc"));
					if (timeColumn != null)
					{
						result.Add(new TableAndTimeColumn()
						{
							TableName = table.TableName,
							TimeColumnName = timeColumn,
						});
					}
				}
			}
			return result;
		}

		int FindTableCount(string tableName, string timeColumnName)
		{
			string assertSql = $@"
					SELECT COUNT(*) FROM {tableName} WHERE {timeColumnName} <= '2000-01-01 00:00:00.000';";
			var count = 0;
			using (var reader = Db.Connection.Command(assertSql).ExecuteReader())
			{
				if (reader.Read())
				{
					count = reader.GetInt32(0);
				}
			}
			return count;
		}

		protected override void AssertTransformationResults()
		{
			var tables = GetAssertTableList();
			foreach (var table in tables)
			{
				var count = FindTableCount(table.TableName.QuoteName(), table.TimeColumnName.QuoteName());

				var expected = 0;
				switch(table.TableName)
				{
					case "ExportAWBHeader":
					case "PkgPackageJob":
						expected = 1; break;
					case "BMBoardSectionChannel":
					case "OrgCommissionAgreementItem":
						expected = 2; break;
				}

				AssertEquals($"Expected {expected} of {table.TableName}(s) but was {count}", expected, count);
			}
			AssertEquals(0, FindTableCount("PkgPackageHandlingUnitDivot", "KPD_SystemCreateTimeUtc"));
		}
	}
}
