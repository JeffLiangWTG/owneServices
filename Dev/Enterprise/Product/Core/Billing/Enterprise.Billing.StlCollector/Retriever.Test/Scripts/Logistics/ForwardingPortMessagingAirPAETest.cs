using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ForwardingPortMessagingAirPAE))]
	sealed class ForwardingPortMessagingAirPAETest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2023, 1);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals(5, transactions.Count());

			var row1 = transactions.Single(x => x.Reference2.StartsWith("CN001"));
			var row2 = transactions.Single(x => x.Reference2.StartsWith("CN002"));
			var row3 = transactions.Single(x => x.Reference2.StartsWith("CN003"));
			var row4 = transactions.Single(x => x.Reference2.StartsWith("CN004"));
			var row5 = transactions.Single(x => x.Reference2.StartsWith("CN005"));
			AssertNotNull(row1);
			AssertNotNull(row2);
			AssertNotNull(row3);
			AssertNotNull(row4);
			AssertNotNull(row5);

			CombineAssertions(() =>
			{
				AssertEquals("[Row-1] CompanyCode", "001", row1.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "GB1", row1.GetBranchCode());
				AssertEquals("[Row-1] TransactionDateUtc", new DateTime(2023, 1, 10, 00, 00, 01), row1.ServiceOccuredUTC);
				AssertEquals("[Row-1] ItemCount", 1, row1.BillableCount);
				AssertEquals("[Row-1] TransactionReference01", "JK_CON01", row1.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "CN001 - Export Notification (755)", row1.Reference2);
				AssertNull("[Row-1] TransactionReference03", row1.Reference3);
				AssertEquals("[Row-1] TransactionReference04", "JK_MasterBillNum_01", row1.Reference4);

				AssertEquals("[Row-2] CompanyCode", "001", row2.GetCompanyCode());
				AssertEquals("[Row-2] BranchCode", "GB1", row2.GetBranchCode());
				AssertEquals("[Row-2] TransactionDateUtc", new DateTime(2023, 1, 10, 00, 00, 02), row2.ServiceOccuredUTC);
				AssertEquals("[Row-2] ItemCount", 1, row2.BillableCount);
				AssertEquals("[Row-2] TransactionReference01", "JS_CON01", row2.Reference1);
				AssertEquals("[Row-2] TransactionReference02", "CN002 - Export Notification (755)", row2.Reference2);
				AssertEquals("[Row-2] TransactionReference03", "JS_HouseBill_01", row2.Reference3);
				AssertEquals("[Row-2] TransactionReference04", "JK_MasterBillNum_01", row2.Reference4);

				AssertEquals("[Row-3] CompanyCode", "001", row3.GetCompanyCode());
				AssertEquals("[Row-3] BranchCode", "GB2", row3.GetBranchCode());
				AssertEquals("[Row-3] TransactionDateUtc", new DateTime(2023, 1, 10, 00, 00, 03), row3.ServiceOccuredUTC);
				AssertEquals("[Row-3] ItemCount", 1, row3.BillableCount);
				AssertEquals("[Row-3] TransactionReference01", "JK_CON02", row3.Reference1);
				AssertEquals("[Row-3] TransactionReference02", "CN003 - Export Notification (755)", row3.Reference2);
				AssertNull("[Row-3] TransactionReference03", row3.Reference3);
				AssertEquals("[Row-3] TransactionReference04", "JK_MasterBillNum_02", row3.Reference4);

				AssertEquals("[Row-4] CompanyCode", "001", row4.GetCompanyCode());
				AssertEquals("[Row-4] BranchCode", "GB2", row4.GetBranchCode());
				AssertEquals("[Row-4] TransactionDateUtc", new DateTime(2023, 1, 10, 00, 00, 04), row4.ServiceOccuredUTC);
				AssertEquals("[Row-4] ItemCount", 1, row4.BillableCount);
				AssertEquals("[Row-4] TransactionReference01", "JS_CON02", row4.Reference1);
				AssertEquals("[Row-4] TransactionReference02", "CN004 - Export Notification (755)", row4.Reference2);
				AssertEquals("[Row-4] TransactionReference03", "JS_HouseBill_02", row4.Reference3);
				AssertEquals("[Row-4] TransactionReference04", "JK_MasterBillNum_02", row4.Reference4);

				AssertEquals("[Row-5] CompanyCode", "002", row5.GetCompanyCode());
				AssertEquals("[Row-5] BranchCode", "GB3", row5.GetBranchCode());
				AssertEquals("[Row-5] TransactionDateUtc", new DateTime(2023, 1, 10, 00, 00, 05), row5.ServiceOccuredUTC);
				AssertEquals("[Row-5] ItemCount", 1, row5.BillableCount);
				AssertEquals("[Row-5] TransactionReference01", "JS_CON02", row5.Reference1);
				AssertEquals("[Row-5] TransactionReference02", "CN005 - Export Notification (755)", row5.Reference2);
				AssertEquals("[Row-5] TransactionReference03", "JS_HouseBill_02", row5.Reference3);
				AssertEquals("[Row-5] TransactionReference04", "JK_MasterBillNum_02", row5.Reference4);
			});
		}

		protected override void PrepareTestData()
		{
			string sqlText = $@"
-- JobConsol
DECLARE @JkPK01 UNIQUEIDENTIFIER = 'EE9515D2-E1BC-4FF1-92DD-61FA27E18750';
DECLARE @JkPK02 UNIQUEIDENTIFIER = '64D6BE18-2D67-4DF9-9981-4E676C6892FF';
INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_MasterBillNum, JK_RL_NKLoadPort, JK_TransportMode) VALUES 
(@JkPK01, 'JK_CON01', 'JK_MasterBillNum_01', 'FRMR1', 'AIR'),
(@JkPK02, 'JK_CON02', 'JK_MasterBillNum_02', 'FRMR2', 'AIR');

-- JobShipment
DECLARE @JsPK01 UNIQUEIDENTIFIER = '1E42CD4C-A693-4513-8687-9692DD13BA70';
DECLARE @JsPK02 UNIQUEIDENTIFIER = 'C880D1B6-A0DC-49AE-A80D-80A1DE210DB8';
INSERT dbo.JobShipment (JS_PK, JS_ShipmentType, JS_RL_NKOrigin, JS_UniqueConsignRef, JS_HouseBill) VALUES 
(@JsPK01, 'HVL', 'FRMR1', 'JS_CON01', 'JS_HouseBill_01'),
(@JsPK02, 'HVL', 'FRMR2', 'JS_CON02', 'JS_HouseBill_02');

-- JobConShipLink
DECLARE @LinkPK01 UNIQUEIDENTIFIER = 'A727F16C-49D5-4DB6-9614-B2FB82E1A922';
DECLARE @LinkPK02 UNIQUEIDENTIFIER = 'F8700A6B-B5BF-4C5B-A3F6-A5FB982A5150';
INSERT dbo.JobConShipLink (JN_PK, JN_JK, JN_JS) VALUES 
(@LinkPK01, @JkPk01, @JsPk01),
(@LinkPK02, @JkPk02, @JsPk02);
				
-- JobDocumentData
DECLARE @JddPk01 UNIQUEIDENTIFIER = '9A0E0109-0B45-4208-B273-B49336D45004';
DECLARE @JddPk02 UNIQUEIDENTIFIER = '1C62E436-42DB-4E64-AA11-CF1C918F6F8B';
DECLARE @JddPk03 UNIQUEIDENTIFIER = '7E8FC675-EBD9-4398-A09A-96F0FEB87355';
DECLARE @JddPk04 UNIQUEIDENTIFIER = '373FAB3A-582A-4A98-91A8-87FEC88B5017';
INSERT dbo.JobDocumentData (JDD_PK, JDD_ParentID, JDD_ParentTableCode, JDD_SystemCreateTimeUtc, JDD_SystemLastEditTimeUtc, JDD_SystemCreateUser, JDD_SystemLastEditUser) VALUES
(@JddPk01, @JkPk01, 'JK', '2023-01-10 00:00:00', '2023-01-10 00:00:00', 'E', 'E'),
(@JddPk02, @JsPK01, 'JS', '2023-01-10 00:00:00', '2023-01-10 00:00:00', 'E', 'E'),
(@JddPk03, @JkPK02, 'JK', '2023-01-10 00:00:00', '2023-01-10 00:00:00', 'E', 'E'),
(@JddPk04, @JsPK02, 'JS', '2023-01-10 00:00:00', '2023-01-10 00:00:00', 'E', 'E');

-- StmALog ISN & MSN
DECLARE @LogPK01 UNIQUEIDENTIFIER = 'E2991151-2C0E-47F4-91AA-32CEC3B9F50D';
DECLARE @LogPK02 UNIQUEIDENTIFIER = '89F3757D-EB97-4ECF-8BDC-BA319C831588';
DECLARE @LogPK03 UNIQUEIDENTIFIER = 'D255D247-52CE-4FBC-AC20-0C6AD90DD5BA';
DECLARE @LogPK04 UNIQUEIDENTIFIER = '8BA2A900-4326-4EF5-AFB4-9AC5EDA6F027';
DECLARE @LogPK05 UNIQUEIDENTIFIER = '9B4F110C-AE55-4888-9D35-22CFB8B07705';
DECLARE @LogPK06 UNIQUEIDENTIFIER = '069FEC00-1411-4DD4-B93D-9333BE6C8F4B';
DECLARE @LogPK07 UNIQUEIDENTIFIER = '2B1EDB65-FB4F-4ED8-93FD-C35B9B5EE849';
DECLARE @LogPK08 UNIQUEIDENTIFIER = '1B620CD7-B74C-4F3D-B293-FFCC90046BFB';
DECLARE @LogPK09 UNIQUEIDENTIFIER = 'C297DD28-2C6F-4D90-A2C9-9F522D09D3C9';
INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsCancelled) VALUES
(@LogPK01, 'JobDocumentData', getdate(), '2023-01-10 00:00:01', @JddPk01, 'ISN', '|MST=Export Notification (755)|STA=ORG|LOC=CN001|EQN=eqn01', 'GB1', 'N'), -- SL_GB_NKBranch condition[GB_RL_NKHomePort = JK_RL_NKLoadPort]        
(@LogPK02, 'JobDocumentData', getdate(), '2023-01-10 00:00:02', @JddPk02, 'ISN', '|MST=Export Notification (755)|STA=ORG|LOC=CN002|EQN=eqn01', 'GB1', 'N'), -- SL_GB_NKBranch condition[GB_RL_NKHomePort = JS_RL_NKOrigin] 
(@LogPK03, 'JobDocumentData', getdate(), '2023-01-10 00:00:03', @JddPk03, 'ISN', '|MST=Export Notification (755)|STA=ORG|LOC=CN003|EQN=eqn01', 'GB2', 'N'), -- SL_GB_NKBranch condition[GY_RL_NKAdditionalBranchRelatedPort = JK_RL_NKLoadPort]
(@LogPK04, 'JobDocumentData', getdate(), '2023-01-10 00:00:04', @JddPk04, 'ISN', '|MST=Export Notification (755)|STA=ORG|LOC=CN004|EQN=eqn01', 'GB2', 'N'), -- SL_GB_NKBranch condition[GY_RL_NKAdditionalBranchRelatedPort = JS_RL_NKOrigin]
(@LogPK05, 'JobDocumentData', getdate(), '2023-01-10 00:00:05', @JddPk04, 'ISN', '|MST=Export Notification (755)|STA=ORG|LOC=CN005|EQN=eqn01', 'XXX', 'N'), -- SL_GB_NKBranch query by MSN log
(@LogPK06, 'JobDocumentData', getdate(), '2023-01-09 00:00:05', @JddPk04, 'MSN', '|MST=Export Notification (755)|STA=ORG|LOC=CN005|EQN=eqn01', 'GB3', 'N'), -- MSN log
(@LogPK07, 'JobDocumentData', getdate(), '2023-01-09 00:00:07', @JddPk01, 'ISN', '|MST=XXX1|STA=ORG|LOC=CN007|EQN=eqn01', 'GB1', 'N'), -- not MST=Export Notification (755)
(@LogPK08, 'JobDocumentData', getdate(), '2023-01-09 00:00:08', @JddPk01, 'ISN', '|MST=Export Notification (755)|STA=XXX|LOC=CN008|EQN=eqn01', 'GB1', 'N'), -- not STA=XXX
(@LogPK09, 'JobDocumentData', getdate(), '2023-01-09 00:00:09', @JddPk01, 'IRA', '|MST=Export Notification (755)|STA=XXX|LOC=CN009|EQN=eqn01', 'GB1', 'N'); -- not ISN

-- GlbCompany
DECLARE @GcPk01 UNIQUEIDENTIFIER = 'BAE2EABE-56E9-4AB4-9847-B2B4E6FEA229';
DECLARE @GcPk02 UNIQUEIDENTIFIER = '2B85555A-986A-453E-9A60-E4BF85E0E2E2';
INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES 
(@GcPk01, '001', 'FR company', 'EUR', 'FR'),
(@GcPk02, '002', 'AU company', 'AUD', 'AU');

-- GlbBranch
DECLARE @GbPk01 UNIQUEIDENTIFIER = 'F54C4638-E5E6-41AC-A9AE-398746A78800';
DECLARE @GbPk02 UNIQUEIDENTIFIER = '56AADE61-7FB2-4190-8460-898B5370CD8C';
DECLARE @GbPk03 UNIQUEIDENTIFIER = 'B276336D-15F7-4B96-A174-F917F9CF80D8';
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_RL_NKHomePort) VALUES 
(@GbPk01, 'GB1', @GcPk01, 'FRMR1'),
(@GbPk02, 'GB2', @GcPk01, 'FRMRX'),
(@GbPk03, 'GB3', @GcPk02, 'AUSYD');

-- GlbBranchExtraPorts
DECLARE @EPGBPk01 UNIQUEIDENTIFIER = '97276236-BC43-4E5F-93CA-DF4771002164';
INSERT dbo.GlbBranchExtraPorts(GY_PK, GY_RL_NKAdditionalBranchRelatedPort, GY_GB) VALUES
(@EPGBPk01, 'FRMR2', @GbPk02);
";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
