using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(HVLVConsignmentPreScreeningUsage))]
	sealed class HVLVConsignmentPreScreeningUsageTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2021, 8);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Transactions", 3, transactions.Count());

				AssertRowMatchingOccured(transactions, "1", null, null, new DateTime(2021, 8, 10), string.Empty, 10, "Consign12", "JobShipment");
				AssertRowMatchingOccured(transactions, "2", null, null, new DateTime(2021, 8, 11), string.Empty, 1, "Consign12", "JobShipment");
				AssertRowMatchingOccured(transactions, "3", null, null, new DateTime(2021, 8, 21), string.Empty, 150, "TESTBOOKINGHEADER", "HVLVBookingHeader");
			});
		}

		protected override void PrepareTestData()
		{
			var sqlText = @"
				DECLARE @ClusterKey int = 2824;
				DECLARE @BookingHeaderPK UNIQUEIDENTIFIER = newid();
				DECLARE @ShipmentPK UNIQUEIDENTIFIER = newid();

				INSERT dbo.HVLVBookingHeader (HVH_PK, HVH_BookingReference, HVH_ClusterKey, HVH_OA_BillToParty, HVH_SystemCreateTimeUtc, HVH_SystemLastEditTimeUtc, HVH_SystemCreateUser, HVH_SystemLastEditUser) VALUES (@BookingHeaderPK, 'TESTBOOKINGHEADER', @ClusterKey, (Select Top 1 OA_PK from dbo.OrgAddress), '2019-03-29', '2019-03-29', '~BP', '~BP');

				INSERT dbo.JobShipment (JS_PK, JS_ShipmentType, JS_TransportMode, JS_RL_NKDestination, JS_IsForwardRegistered, JS_UniqueConsignRef) VALUES (@ShipmentPK, 'HVL', 'AIR', 'AUSYD', 1, 'Consign12');

				INSERT dbo.StmALog (SL_PK, SL_Parent, SL_Table, SL_Reference, SL_PostedTimeUtc, SL_EventTime, SL_GB_NKBranch) VALUES
					(NEWID(), @ShipmentPK, 'JobShipment', '|RES=Pre-Screened|TTL=10', '2021-7-10', '2021-7-10', 'GB1'),
					(NEWID(), @ShipmentPK, 'JobShipment', '|RES=Pre-Screened|TTL=10', '2021-9-10', '2021-9-10', 'GB2'),
					(NEWID(), @ShipmentPK, 'JobShipment', '|RES=Pre-Screened|TTL=10', '2021-8-10', '2021-8-10', 'GB1'),
					(NEWID(), @ShipmentPK, 'JobShipment', '|RES=Pre-Screened|TTL=1', '2021-8-11', '2021-8-11', 'GB2'),
					(NEWID(), @BookingHeaderPK, 'HVLVBookingHeader', '|RES=Pre-Screened|TTL=150', '2021-8-21', '2021-8-21', 'HN1');";

			TestConnection.Command(sqlText).ExecuteNonQuery();
		}
	}
}
