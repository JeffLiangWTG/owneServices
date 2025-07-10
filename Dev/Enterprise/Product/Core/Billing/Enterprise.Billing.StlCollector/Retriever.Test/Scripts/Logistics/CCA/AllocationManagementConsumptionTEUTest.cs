using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(AllocationManagementConsumptionTEU))]
	sealed class AllocationManagementConsumptionTEUTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => true;

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2024, 2);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions("CON1 - Consol with main leg departed should count all containers", () =>
			{
				var con1 = FindRowByOccured(transactions, new DateTime(2024, 02, 01));
				AssertEquals("BranchCode", "GB1", con1.GetBranchCode());
				AssertEquals("BranchCode", "USR", con1.ClientStaffCode);
				AssertEquals("ItemCount", 3, con1.BillableCount);
				AssertEquals("TransactionReference01", "CON", con1.Reference1);
				AssertEquals("TransactionReference02", "CON1", con1.Reference2);
				AssertEquals("TransactionReference03", "CCA1", con1.Reference3);
				AssertEquals("TransactionReference04", "OH1", con1.Reference4);
			});

			CombineAssertions("CON2: Consol's load port matches DEP branch's slibling on home port", () =>
			{
				var con2 = FindRowByOccured(transactions, new DateTime(2024, 02, 02));
				AssertEquals("BranchCode", "GB2", con2.GetBranchCode());
				AssertEquals("BranchCode", "USR", con2.ClientStaffCode);
				AssertEquals("ItemCount", 1, con2.BillableCount);
				AssertEquals("TransactionReference01", "CON", con2.Reference1);
				AssertEquals("TransactionReference02", "CON2", con2.Reference2);
				AssertEquals("TransactionReference03", "CCA2", con2.Reference3);
				AssertEquals("TransactionReference04", "OH1", con2.Reference4);
			});

			CombineAssertions("CON3: Consol's load port matches DEP branch's slibling on additional related port", () =>
			{
				var con3 = FindRowByOccured(transactions, new DateTime(2024, 02, 03));
				AssertEquals("BranchCode", "GB4", con3.GetBranchCode());
				AssertEquals("BranchCode", "USR", con3.ClientStaffCode);
				AssertEquals("ItemCount", 1, con3.BillableCount);
				AssertEquals("TransactionReference01", "CON", con3.Reference1);
				AssertEquals("TransactionReference02", "CON3", con3.Reference2);
				AssertEquals("TransactionReference03", "CCA3", con3.Reference3);
				AssertEquals("TransactionReference04", "OH1", con3.Reference4);
			});

			CombineAssertions("CON5: Consol with Container Mode = 'OTH' should only count containers with a non-empty container number.", () =>
			{
				var con3 = FindRowByOccured(transactions, new DateTime(2024, 02, 05));
				AssertEquals("BranchCode", "GB1", con3.GetBranchCode());
				AssertEquals("BranchCode", "USR", con3.ClientStaffCode);
				AssertEquals("ItemCount", 2, con3.BillableCount);
				AssertEquals("TransactionReference01", "CON", con3.Reference1);
				AssertEquals("TransactionReference02", "CON5", con3.Reference2);
				AssertEquals("TransactionReference03", "CCA5", con3.Reference3);
				AssertEquals("TransactionReference04", "OH1", con3.Reference4);
			});

			AssertEquals("Number of Transactions", 4, transactions.Count());
		}

		protected override void PrepareTestData()
		{
			var sql = @"
-- Data Setup
DECLARE @OhPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk01 UNIQUEIDENTIFIER = NEWID();

DECLARE @RcPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @RcPk02 UNIQUEIDENTIFIER = NEWID();

DECLARE @GcPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk02 UNIQUEIDENTIFIER = NEWID();

DECLARE @GbPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk04 UNIQUEIDENTIFIER = NEWID();

DECLARE @GyPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @GyPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @GyPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @GyPk04 UNIQUEIDENTIFIER = NEWID();
DECLARE @GyPk05 UNIQUEIDENTIFIER = NEWID();
DECLARE @GyPk06 UNIQUEIDENTIFIER = NEWID();
DECLARE @GyPk07 UNIQUEIDENTIFIER = NEWID();
DECLARE @GyPk08 UNIQUEIDENTIFIER = NEWID();

INSERT dbo.OrgHeader (OH_PK, OH_IsActive, OH_Code, OH_IsShippingLine) VALUES
	(@OhPk01, 1, 'OH1', 1);

INSERT dbo.OrgAddress (OA_PK, OA_OH, OA_Address1, OA_Address2, OA_City, OA_PostCode, OA_RN_NKCountryCode, OA_IsActive) VALUES
	(@OaPk01, @OhPk01, '10', 'George St', 'SYD', '2000', 'AU', 1);

INSERT dbo.RefContainer (RC_PK, RC_Code, RC_ISOType, RC_TEU, RC_SystemCreateTimeUtc, RC_SystemCreateUser) VALUES
	(@RcPk01, 'rc01', '20GP', 1, '2024-03-01', 'DS2'),
	(@RcPk02, 'rc02', '40GP', 2, '2024-03-01', 'DS2');
	
INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_IsActive, GC_SystemCreateTimeUtc, GC_SystemCreateUser) VALUES
	(@GcPk01, 'CO1', 'CO company', 1, '2024-03-01', 'DS2'),
	(@GcPk02, 'CO2', 'CO company', 1, '2024-03-01', 'DS2');

INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_RL_NKHomePort, GB_GC, GB_IsActive, GB_SystemCreateTimeUtc, GB_SystemCreateUser) VALUES
	(@GbPk01, 'GB1', 'AAAAA', @GcPk01, 1, '2024-03-01', 'DS2'),
	(@GbPk02, 'GB2', 'BBBBB', @GcPk01, 1, '2024-03-01', 'DS2'),
	(@GbPk03, 'GB3', 'CCCCC', @GcPk02, 1, '2024-03-01', 'DS2'),
	(@GbPk04, 'GB4', 'AUSYD', @GcPk02, 1, '2024-03-01', 'DS2');

INSERT dbo.GlbBranchExtraPorts(GY_PK, GY_GB, GY_RL_NKAdditionalBranchRelatedPort) VALUES 
	(@GyPk01, @GbPk01, 'AAAAB'),
	(@GyPk02, @GbPk01, 'AAAAC'),
	(@GyPk03, @GbPk02, 'BBBBC'),
	(@GyPk04, @GbPk02, 'BBBBD'),
	(@GyPk05, @GbPk03, 'CCCCD'),
	(@GyPk06, @GbPk03, 'CCCCE'),
	(@GyPk07, @GbPk04, 'DDDDE'),
	(@GyPk08, @GbPk04, 'DDDDF');

--- CON1: Consol with main leg departed should count all containers

DECLARE @JkPk11 UNIQUEIDENTIFIER = NEWID();
DECLARE @JwPk11 UNIQUEIDENTIFIER = NEWID();

INSERT dbo.JobConsol (JK_PK, JK_RL_NKLoadPort, JK_UniqueConsignRef, JK_CarrierContractNumber, JK_RCA_AllocationLine, JK_IsForwarding, JK_TransportMode, JK_ConsolMode, JK_OA_ShippingLineAddress, JK_SystemCreateTimeUtc, JK_SystemCreateUser) VALUES
	(@JkPk11, 'AUSYD', 'CON1', 'CCA1', null, 1, 'SEA', 'FCL', @OaPk01, '2024-03-01', 'USR');

INSERT dbo.JobContainer (JC_PK, JC_JK, JC_RCA_AllocationLine, JC_RC, JC_ContainerNum, JC_ContainerCount, JC_SystemCreateTimeUtc, JC_SystemCreateUser) VALUES
	(NEWID(), @JkPk11, null, @RcPk01, 'CONTAINER11', 1, '2024-03-01', 'DS2');

INSERT dbo.JobContainer (JC_PK, JC_JK, JC_RCA_AllocationLine, JC_RC, JC_ContainerNum, JC_ContainerCount, JC_SystemCreateTimeUtc, JC_SystemCreateUser) VALUES
	(NEWID(), @JkPk11, null, @RcPk02, 'CONTAINER12', 1, '2024-03-01', 'DS2');

INSERT dbo.JobConsolTransport (JW_PK, JW_ParentGUID, JW_ParentType, JW_JX, JW_RL_NKLoadPort, JW_TransportType, JW_TransportMode) VALUES
	(@JwPk11, @JkPk11, 'CON', null, 'AUSYD', 'MAI', 'SEA');
	
INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsEstimate, SL_IsCancelled) VALUES
	(NEWID(), 'JobConsolTransport', '2024-02-01', '2024-02-01', @JwPk11, 'DEP', 'LOC=AUSYD', 'GB1', 'N', 'N');

INSERT dbo.RatingContract (RCT_PK, RCT_ContractNumber, RCT_ContractType, RCT_OH, RCT_StartDate, RCT_EndDate, RCT_GS_NKContractOwner, RCT_SystemCreateTimeUtc, RCT_SystemLastEditTimeUtc, RCT_SystemCreateUser, RCT_SystemLastEditUser) VALUES
	(NEWID(), 'CCA1', 'PRO', @OhPk01, '2024-01-01', '2024-04-01', 'DS2', '2024-01-01', '2024-01-01', 'DS2', 'DS2');

--- CON2: Consol's load port matches DEP branch's slibling on home port

DECLARE @JkPk21 UNIQUEIDENTIFIER = NEWID();
DECLARE @JwPk21 UNIQUEIDENTIFIER = NEWID();

INSERT dbo.JobConsol (JK_PK, JK_RL_NKLoadPort, JK_UniqueConsignRef, JK_CarrierContractNumber, JK_RCA_AllocationLine, JK_IsForwarding, JK_TransportMode, JK_ConsolMode, JK_OA_ShippingLineAddress, JK_SystemCreateTimeUtc, JK_SystemCreateUser) VALUES
	(@JkPk21, 'BBBBB', 'CON2', 'CCA2', null, 1, 'SEA', 'FCL', @OaPk01, '2024-03-01', 'USR');

INSERT dbo.JobContainer (JC_PK, JC_JK, JC_RCA_AllocationLine, JC_RC, JC_ContainerNum, JC_ContainerCount, JC_SystemCreateTimeUtc, JC_SystemCreateUser) VALUES
	(NEWID(), @JkPk21, null, @RcPk01, 'CONTAINER21', 1, '2024-03-01', 'DS2');

INSERT dbo.JobConsolTransport (JW_PK, JW_ParentGUID, JW_ParentType, JW_JX, JW_RL_NKLoadPort, JW_TransportType, JW_TransportMode) VALUES
	(@JwPk21, @JkPk21, 'CON', null, 'BBBBB', 'MAI', 'SEA');
	
INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsEstimate, SL_IsCancelled) VALUES
	(NEWID(), 'JobConsolTransport', '2024-02-01', '2024-02-02', @JwPk21, 'DEP', 'LOC=BBBBB', 'GB1', 'N', 'N');

INSERT dbo.RatingContract (RCT_PK, RCT_ContractNumber, RCT_ContractType, RCT_OH, RCT_StartDate, RCT_EndDate, RCT_GS_NKContractOwner, RCT_SystemCreateTimeUtc, RCT_SystemLastEditTimeUtc, RCT_SystemCreateUser, RCT_SystemLastEditUser) VALUES
	(NEWID(), 'CCA2', 'PRO', @OhPk01, '2024-01-01', '2024-04-01', 'DS2', '2024-01-01', '2024-01-01', 'DS2', 'DS2');

--- CON3: Consol's load port matches DEP branch's slibling on additional related port

DECLARE @JkPk31 UNIQUEIDENTIFIER = NEWID();
DECLARE @JwPk31 UNIQUEIDENTIFIER = NEWID();

INSERT dbo.JobConsol (JK_PK, JK_RL_NKLoadPort, JK_UniqueConsignRef, JK_CarrierContractNumber, JK_RCA_AllocationLine, JK_IsForwarding, JK_TransportMode, JK_ConsolMode, JK_OA_ShippingLineAddress, JK_SystemCreateTimeUtc, JK_SystemCreateUser) VALUES
	(@JkPk31, 'DDDDE', 'CON3', 'CCA3', null, 1, 'SEA', 'FCL', @OaPk01, '2024-03-01', 'USR');

INSERT dbo.JobContainer (JC_PK, JC_JK, JC_RCA_AllocationLine, JC_RC, JC_ContainerNum, JC_ContainerCount, JC_SystemCreateTimeUtc, JC_SystemCreateUser) VALUES
	(NEWID(), @JkPk31, null, @RcPk01, 'CONTAINER31', 1, '2024-03-01', 'DS2');

INSERT dbo.JobConsolTransport (JW_PK, JW_ParentGUID, JW_ParentType, JW_JX, JW_RL_NKLoadPort, JW_TransportType, JW_TransportMode) VALUES
	(@JwPk31, @JkPk31, 'CON', null, 'DDDDE', 'MAI', 'SEA');
	
INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsEstimate, SL_IsCancelled) VALUES
	(NEWID(), 'JobConsolTransport', '2024-02-01', '2024-02-03', @JwPk31, 'DEP', 'LOC=BBBBB', 'GB3', 'N', 'N');

INSERT dbo.RatingContract (RCT_PK, RCT_ContractNumber, RCT_ContractType, RCT_OH, RCT_StartDate, RCT_EndDate, RCT_GS_NKContractOwner, RCT_SystemCreateTimeUtc, RCT_SystemLastEditTimeUtc, RCT_SystemCreateUser, RCT_SystemLastEditUser) VALUES
	(NEWID(), 'CCA3', 'PRO', @OhPk01, '2024-01-01', '2024-04-01', 'DS2', '2024-01-01', '2024-01-01', 'DS2', 'DS2');

--- CON4: Consol that departed, but with no containers

DECLARE @JkPk41 UNIQUEIDENTIFIER = NEWID();
DECLARE @JwPk41 UNIQUEIDENTIFIER = NEWID();

INSERT dbo.JobConsol (JK_PK, JK_RL_NKLoadPort, JK_UniqueConsignRef, JK_CarrierContractNumber, JK_RCA_AllocationLine, JK_IsForwarding, JK_TransportMode, JK_ConsolMode, JK_OA_ShippingLineAddress, JK_SystemCreateTimeUtc, JK_SystemCreateUser) VALUES
	(@JkPk41, 'DDDDE', 'CON4', 'CCA4', null, 1, 'SEA', 'FCL', @OaPk01, '2024-03-01', 'USR');

INSERT dbo.JobConsolTransport (JW_PK, JW_ParentGUID, JW_ParentType, JW_JX, JW_RL_NKLoadPort, JW_TransportType, JW_TransportMode) VALUES
	(@JwPk41, @JkPk41, 'CON', null, 'AUSYD', 'MAI', 'SEA');

INSERT dbo.RatingContract (RCT_PK, RCT_ContractNumber, RCT_ContractType, RCT_OH, RCT_StartDate, RCT_EndDate, RCT_GS_NKContractOwner, RCT_SystemCreateTimeUtc, RCT_SystemLastEditTimeUtc, RCT_SystemCreateUser, RCT_SystemLastEditUser) VALUES
	(NEWID(), 'CCA4', 'PRO', @OhPk01, '2024-01-01', '2024-04-01', 'DS2', '2024-01-01', '2024-01-01', 'DS2', 'DS2');

INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsEstimate, SL_IsCancelled) VALUES
	(NEWID(), 'JobConsolTransport', '2024-02-01', '2024-02-04', @JwPk41, 'DEP', 'LOC=BBBBB', 'GB1', 'N', 'N');

--- CON5: Consol with Container Mode = 'OTH' should only count containers with a non-empty container number.

DECLARE @JkPk51 UNIQUEIDENTIFIER = NEWID();
DECLARE @JwPk51 UNIQUEIDENTIFIER = NEWID();

INSERT dbo.JobConsol (JK_PK, JK_RL_NKLoadPort, JK_UniqueConsignRef, JK_CarrierContractNumber, JK_RCA_AllocationLine, JK_IsForwarding, JK_TransportMode, JK_ConsolMode, JK_OA_ShippingLineAddress, JK_SystemCreateTimeUtc, JK_SystemCreateUser) VALUES
	(@JkPk51, 'DDDDE', 'CON5', 'CCA5', null, 1, 'SEA', 'OTH', @OaPk01, '2024-03-01', 'USR');

INSERT dbo.JobContainer (JC_PK, JC_JK, JC_RCA_AllocationLine, JC_RC, JC_ContainerNum, JC_ContainerCount, JC_SystemCreateTimeUtc, JC_SystemCreateUser) VALUES
	(NEWID(), @JkPk51, null, @RcPk01, 'CONTAINER51', 1, '2024-03-01', 'DS2');

INSERT dbo.JobContainer (JC_PK, JC_JK, JC_RCA_AllocationLine, JC_RC, JC_ContainerNum, JC_ContainerCount, JC_SystemCreateTimeUtc, JC_SystemCreateUser) VALUES
	(NEWID(), @JkPk51, null, @RcPk01, 'CONTAINER52', 1, '2024-03-01', 'DS2');

INSERT dbo.JobContainer (JC_PK, JC_JK, JC_RCA_AllocationLine, JC_RC, JC_ContainerNum, JC_ContainerCount, JC_SystemCreateTimeUtc, JC_SystemCreateUser) VALUES
	(NEWID(), @JkPk51, null, @RcPk01, '', 2, '2024-03-01', 'DS2');

INSERT dbo.JobConsolTransport (JW_PK, JW_ParentGUID, JW_ParentType, JW_JX, JW_RL_NKLoadPort, JW_TransportType, JW_TransportMode) VALUES
	(@JwPk51, @JkPk51, 'CON', null, 'AUSYD', 'MAI', 'SEA');

INSERT dbo.RatingContract (RCT_PK, RCT_ContractNumber, RCT_ContractType, RCT_OH, RCT_StartDate, RCT_EndDate, RCT_GS_NKContractOwner, RCT_SystemCreateTimeUtc, RCT_SystemLastEditTimeUtc, RCT_SystemCreateUser, RCT_SystemLastEditUser) VALUES
	(NEWID(), 'CCA5', 'PRO', @OhPk01, '2024-01-01', '2024-04-01', 'DS2', '2024-01-01', '2024-01-01', 'DS2', 'DS2');

INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsEstimate, SL_IsCancelled) VALUES
	(NEWID(), 'JobConsolTransport', '2024-02-05', '2024-02-05', @JwPk51, 'DEP', 'LOC=BBBBB', 'GB1', 'N', 'N');

";

			TestConnection.ExecuteNonQuery(sql);
		}
	}
}
