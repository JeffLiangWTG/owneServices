using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(HVLVOriginLoadListConversion))]
	sealed class HVLVOriginLoadListConversionTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2021, 7);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Shipments converted from Load List", 1, transactions.Count());

			var transaction1 = transactions.First();
			AssertNull("BranchCode", transaction1.GetBranchCode());
			AssertEquals("CompanyCode", null, transaction1.GetCompanyCode());
			AssertEquals("UserCode", "", transaction1.ClientStaffCode);
			AssertEquals("TransactionDateUtc", new DateTime(2021, 7, 29), transaction1.ServiceOccuredUTC);
			AssertEquals("TransactionReference01", "HCH000000839102", transaction1.Reference1);
			AssertEquals("TransactionReference02", "Consign2", transaction1.Reference2);
		}

		protected override void PrepareTestData()
		{
			var sqlText = @"
			DECLARE @HCH_PK1 UNIQUEIDENTIFIER = newid();
			DECLARE @JS_Shipment1 UNIQUEIDENTIFIER = newid();
			DECLARE @SL_PK1 UNIQUEIDENTIFIER = newid();
			DECLARE @HCH_PK2 UNIQUEIDENTIFIER = newid();
			DECLARE @JS_Shipment2 UNIQUEIDENTIFIER = newid();
			DECLARE @SL_PK2 UNIQUEIDENTIFIER = newid();

			INSERT dbo.JobShipment (JS_PK, JS_ShipmentType, JS_TransportMode, JS_RL_NKDestination, JS_IsForwardRegistered, JS_UniqueConsignRef) VALUES (@JS_Shipment1, 'HVL', 'AIR', 'AUSYD', 1, 'Consign1');
			INSERT dbo.HVLVConsignmentHeader (HCH_PK, HCH_AutoVersion, HCH_ClusterKey, HCH_JobNumber, HCH_IsArchived, HCH_JS_Shipment, HCH_UsageType, HCH_SystemCreateTimeUtc, HCH_SystemCreateUser, HCH_SystemLastEditTimeUtc, HCH_SystemLastEditUser) VALUES (@HCH_PK1, '0', '1456', 'HCH000000483291', '0', @JS_Shipment1, 'S', '2021-06-29', 'E', '2021-07-12 05:30:00', 'E')
			INSERT dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_IsCancelled, SL_Reference, SL_PostedTimeUtc, SL_EventTime, SL_SE_NKEvent, SL_GB_NKBranch, SL_FireWorkflow) VALUES (@SL_PK1, 'RefDocType', @HCH_PK1, 'N', 'N', '2021-07-06', '2021-07-06', 'ADD', 'GB2', '0')

			INSERT dbo.JobShipment (JS_PK, JS_ShipmentType, JS_TransportMode, JS_RL_NKDestination, JS_IsForwardRegistered, JS_UniqueConsignRef) VALUES (@JS_Shipment2, 'HVL', 'AIR', 'AUSYD', 1, 'Consign2');
			INSERT dbo.HVLVConsignmentHeader (HCH_PK, HCH_AutoVersion, HCH_ClusterKey, HCH_JobNumber, HCH_IsArchived, HCH_JS_Shipment, HCH_UsageType, HCH_SystemCreateTimeUtc, HCH_SystemCreateUser, HCH_SystemLastEditTimeUtc, HCH_SystemLastEditUser) VALUES (@HCH_PK2, '0', '2234', 'HCH000000839102', '0', @JS_Shipment2, 'S', '2021-07-29', 'E', '2021-07-12 05:30:00', 'E')
			INSERT dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_IsCancelled, SL_Reference, SL_PostedTimeUtc, SL_EventTime, SL_SE_NKEvent, SL_GB_NKBranch, SL_FireWorkflow) VALUES (@SL_PK2, 'RefDocType', @HCH_PK2, 'N', 'N', '2021-07-06', '2021-07-06', 'ELC', 'GB2', '0')";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
