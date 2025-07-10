using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	abstract class HVLVItemUsageByTransportModeTest : RefStlScriptWithDefaultsTest
	{
		protected abstract bool ShouldIncludeSeaShipments { get; }
		protected abstract bool ShouldIncludeAirShipments { get; }
		protected abstract bool ShouldIncludeRoadShipments { get; }
		protected abstract bool ShouldIncludeOtherTransportModeShipments { get; }

		int ExpectedMatches => (ShouldIncludeSeaShipments ? 1 : 0) +
			(ShouldIncludeAirShipments ? 1 : 0) +
			(ShouldIncludeRoadShipments ? 1 : 0) +
			(ShouldIncludeOtherTransportModeShipments ? 1 : 0);

		protected override void PrepareTestData()
		{
			var sqlText = @"
				DECLARE @ClusterKey int = 24;
				DECLARE @BookingHeaderPK UNIQUEIDENTIFIER = newid();
				DECLARE @ConsignmentPK UNIQUEIDENTIFIER = newid();
				DECLARE @SeaShipmentPK UNIQUEIDENTIFIER = newid();
				DECLARE @AirShipmentPK UNIQUEIDENTIFIER = newid();
				DECLARE @RoadShipmentPK UNIQUEIDENTIFIER = newid();
				DECLARE @OtherTransportModeShipmentPK UNIQUEIDENTIFIER = newid();

				INSERT dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_TransportMode) VALUES
				(@SeaShipmentPK, 'SEATEST001', 'SEA'),
				(@AirShipmentPK, 'AIRTEST001', 'AIR'),
				(@RoadShipmentPK, 'ROATEST001', 'ROA'),
				(@OtherTransportModeShipmentPK, 'OTHTEST001', 'OTH')
				INSERT dbo.HVLVBookingHeader (HVH_PK, HVH_BookingReference, HVH_ClusterKey, HVH_OA_BillToParty, HVH_SystemCreateTimeUtc, HVH_SystemLastEditTimeUtc, HVH_SystemCreateUser, HVH_SystemLastEditUser) VALUES (@BookingHeaderPK, 'TESTBOOKINGHEADER', @ClusterKey, (Select Top 1 OA_PK from dbo.OrgAddress), '2019-03-29', '2019-03-29', 'BP~', 'BP~')
				INSERT dbo.HVLVConsignment (HVC_PK, HVC_ConsignmentId, HVC_HVH_BookingHeader, HVC_ClusterKey, HVC_Status, HVC_SystemCreateTimeUtc, HVC_SystemLastEditTimeUtc, HVC_SystemCreateUser, HVC_SystemLastEditUser) VALUES (@ConsignmentPK, 'TESTHVC001', @BookingHeaderPK, @ClusterKey, 'CLR', '2019-03-29', '2019-03-29', 'BP~', 'BP~')
				INSERT dbo.HVLVItem (HVI_PK, HVI_ItemId, HVI_CurrentBarcode, HVI_ShipperReference, HVI_ClusterKey, HVI_HVC_Consignment, HVI_JS_LoadedOnShipment, HVI_DestinationFirstUsageTimeUtc, HVI_SystemCreateTimeUtc, HVI_SystemLastEditTimeUtc, HVI_SystemCreateUser, HVI_SystemLastEditUser) VALUES 
				(NEWID(), 'SEAHVI001', 'TestBarcode001', 'TestShipperRef001', @ClusterKey, @ConsignmentPK, @SeaShipmentPK, '2020-10-02', '2019-03-29', '2019-03-29', 'BP~', 'BP~'),
				(NEWID(), 'SEAHVI002', 'TestBarcode002', 'TestShipperRef002', @ClusterKey, @ConsignmentPK, @SeaShipmentPK, NULL, '2019-03-29', '2019-03-29', 'BP~', 'BP~'),
				(NEWID(), 'SEAHVI003', 'TestBarcode003', 'TestShipperRef003', @ClusterKey, @ConsignmentPK, @SeaShipmentPK, '2020-05-02', '2019-03-29', '2019-03-29', 'BP~', 'BP~'),
				(NEWID(), 'AIRHVI004', 'TestBarcode004', 'TestShipperRef004', @ClusterKey, @ConsignmentPK, @AirShipmentPK, '2020-10-02', '2019-03-29', '2019-03-29', 'BP~', 'BP~'),
				(NEWID(), 'AIRHVI005', 'TestBarcode005', 'TestShipperRef005', @ClusterKey, @ConsignmentPK, @AirShipmentPK, NULL, '2019-03-29', '2019-03-29', 'BP~', 'BP~'),
				(NEWID(), 'AIRHVI006', 'TestBarcode006', 'TestShipperRef006', @ClusterKey, @ConsignmentPK, @AirShipmentPK, '2020-05-02', '2019-03-29', '2019-03-29', 'BP~', 'BP~'),
				(NEWID(), 'ROAHVI007', 'TestBarcode007', 'TestShipperRef007', @ClusterKey, @ConsignmentPK, @RoadShipmentPK, '2020-10-02', '2019-03-29', '2019-03-29', 'BP~', 'BP~'),
				(NEWID(), 'ROAHVI008', 'TestBarcode008', 'TestShipperRef008', @ClusterKey, @ConsignmentPK, @RoadShipmentPK, NULL, '2019-03-29', '2019-03-29', 'BP~', 'BP~'),
				(NEWID(), 'ROAHVI009', 'TestBarcode009', 'TestShipperRef009', @ClusterKey, @ConsignmentPK, @RoadShipmentPK, '2020-05-02', '2019-03-29', '2019-03-29', 'BP~', 'BP~'),
				(NEWID(), 'OTHHVI010', 'TestBarcode010', 'TestShipperRef010', @ClusterKey, @ConsignmentPK, @OtherTransportModeShipmentPK, '2020-10-02', '2019-03-29', '2019-03-29', 'BP~', 'BP~'),
				(NEWID(), 'OTHHVI011', 'TestBarcode011', 'TestShipperRef011', @ClusterKey, @ConsignmentPK, @OtherTransportModeShipmentPK, NULL, '2019-03-29', '2019-03-29', 'BP~', 'BP~'),
				(NEWID(), 'OTHHVI012', 'TestBarcode012', 'TestShipperRef012', @ClusterKey, @ConsignmentPK, @OtherTransportModeShipmentPK, '2020-05-02', '2019-03-29', '2019-03-29', 'BP~', 'BP~')";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Transactions", ExpectedMatches, transactions.Count());
				if (ShouldIncludeSeaShipments)
				{
					var seaRow = transactions.Single(transaction => transaction.Reference1.StartsWith("SEA"));
					AssertRow(seaRow, "SEA", null, null, new DateTime(2020, 10, 02), "", 1, "SEAHVI001", "TestBarcode001", null, "TestShipperRef001");
				}
				if (ShouldIncludeAirShipments)
				{
					var seaRow = transactions.Single(transaction => transaction.Reference1.StartsWith("AIR"));
					AssertRow(seaRow, "AIR", null, null, new DateTime(2020, 10, 02), "", 1, "AIRHVI004", "TestBarcode004", null, "TestShipperRef004");
				}
				if (ShouldIncludeRoadShipments)
				{
					var seaRow = transactions.Single(transaction => transaction.Reference1.StartsWith("ROA"));
					AssertRow(seaRow, "ROA", null, null, new DateTime(2020, 10, 02), "", 1, "ROAHVI007", "TestBarcode007", null, "TestShipperRef007");
				}
				if (ShouldIncludeOtherTransportModeShipments)
				{
					var seaRow = transactions.Single(transaction => transaction.Reference1.StartsWith("OTH"));
					AssertRow(seaRow, "OTH", null, null, new DateTime(2020, 10, 02), "", 1, "OTHHVI010", "TestBarcode010", null, "TestShipperRef010");
				}
			});
		}

		protected override bool IsMandatoryForMilestones => false;

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2020, 10);
	}
}
