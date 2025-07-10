using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(DddStlBillingCollector))]
	sealed class DddStlBillingCollectorTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			string sqlText = @"
			IF NOT EXISTS (SELECT 1 FROM dbo.GlbCompany WHERE GC_Code = 'TST')
			BEGIN
				INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) 
				VALUES (NEWID(), 'TST', 'US company', 'USD', 'US')
			END
			IF NOT EXISTS (SELECT 1 FROM dbo.GlbBranch WHERE GB_Code = 'TST')
			BEGIN
				INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy, GB_RL_NKHomePort) 
				VALUES (NEWID(), 'TST', (SELECT TOP 1 GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'TST'), NULL, 'USLAX')
			END

			DECLARE @JobShipmentPk UNIQUEIDENTIFIER = NEWID();
			DECLARE @BranchCode NVARCHAR(3) = 'TST';
			DECLARE @GbPk UNIQUEIDENTIFIER;

			SELECT @GbPk = GB_PK FROM dbo.GlbBranch WHERE GB_Code = @BranchCode;

			INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_HouseBill, JS_HBLContainerPackModeOverride, JS_TransportMode, JS_RS_NKServiceLevel, JS_DeliveryDueDate, JS_RevisedDeliveryDueDate, JS_ShipmentType)
			VALUES (@JobShipmentPk, 'V00001001', 'V00001001', 'DOOR/DOOR', 'AIR', 'EXP', '2024-04-10', '2024-04-11', 'STD');

			INSERT INTO dbo.StmALog (SL_PK, SL_Parent, SL_Table, SL_SE_NKEvent, SL_PostedTimeUtc, SL_EventTime, SL_GB_NKBranch, SL_GS_NKUser, SL_Reference)
			VALUES (NEWID(), @JobShipmentPk, 'JobShipment', 'DDE', '2024-04-10 04:00:00', '2024-04-10 04:10:00', @BranchCode, 'MAW', '|ACT=Manual|TYP=Revised');

			INSERT INTO dbo.StmALog (SL_PK, SL_Parent, SL_Table, SL_SE_NKEvent, SL_PostedTimeUtc, SL_EventTime, SL_GB_NKBranch, SL_GS_NKUser, SL_Reference)
			VALUES (NEWID(), @JobShipmentPk, 'JobShipment', 'DDE', '2024-04-10 05:30:00', '2024-04-10 05:40:00', @BranchCode, 'MEW', '|ACT=Manual|TYP=Original');

			INSERT INTO dbo.StmALog (SL_PK, SL_Parent, SL_Table, SL_SE_NKEvent, SL_PostedTimeUtc, SL_EventTime, SL_GB_NKBranch, SL_GS_NKUser, SL_Reference)
			VALUES (NEWID(), @JobShipmentPk, 'JobShipment', 'DDE', '2024-04-10 03:00:00', '2024-04-10 03:10:00', @BranchCode, 'MOW', '|ACT=Manual|TYP=Original');

			INSERT INTO dbo.StmALog (SL_PK, SL_Parent, SL_Table, SL_SE_NKEvent, SL_PostedTimeUtc, SL_EventTime, SL_GB_NKBranch, SL_GS_NKUser, SL_Reference)
			VALUES (NEWID(), @JobShipmentPk, 'JobShipment', 'EDT', '2024-04-10 06:00:00', '2024-04-10 06:10:00', @BranchCode, 'MIW', 'ACT=Manual');
		";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());
			var transaction = transactions.First();

			AssertEquals("CompanyCode", "TST", transaction.GetCompanyCode());
			AssertEquals("BranchCode", "TST", transaction.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2024, 4, 10, 3, 0, 0), transaction.ServiceOccuredUTC);
			AssertEquals("UserCode", "MOW", transaction.ClientStaffCode);
			AssertEquals("BillableCount (TransactionCount)", 1, transaction.BillableCount);

			AssertEquals("BillingReference1 (JS_UniqueConsignRef)", "V00001001", transaction.Reference1);
			AssertEquals("BillingReference2 (JS_HouseBill)", "V00001001", transaction.Reference2);
			AssertEquals("BillingReference3 (HBLContainerPackModeOverride [TransportMode])", "DOOR/DOOR [AIR]", transaction.Reference3);
			AssertEquals("BillingReference4 (ServiceLevel)", "EXP", transaction.Reference4);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return new RecurringRange(
					new DateTime(2024, 4, 10, 2, 0, 0),
					new DateTime(2024, 4, 10, 6, 0, 0));
			}
		}
	}
}
