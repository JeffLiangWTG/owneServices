using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	sealed class BISIUploadRecordListTest : TransactionedTestCase
	{
		public void TestTotalLineCount()
		{
			AssertEquals("No data has been added yet, should be 0", 0, RecordList.TotalLineCount);
			RecordList.AddFromShipment(CreateShipmentDataForTest("101"));
			RecordList.AddFromShipment(CreateShipmentDataForTest("102"));
			RecordList.AddFromShipmentStatus(CreateShipmentStatusDataForTest("103"));
			RecordList.AddFromShipmentStatus(CreateShipmentStatusDataForTest("102"));
			RecordList.AddFromShipmentStatus(CreateShipmentStatusDataForTest("104"));
			RecordList.AddFromShipment(CreateShipmentDataForTest("103"));
			AssertEquals(6, RecordList.TotalLineCount);
			RecordList.AddFromShipment(CreateShipmentDataForTest("105", 5, 17));
			AssertEquals(29, RecordList.TotalLineCount);
		}

		public void TestIsEmpty()
		{
			Assert("No data has been added yet, should be empty", RecordList.IsEmpty);
			RecordList.AddFromShipment(CreateShipmentDataForTest("101"));
			Assert("Should not be empty", !RecordList.IsEmpty);
		}

		public void TestRecords()
		{
			AssertEquals("No data has been added yet, should be empty", 0, RecordList.Records.Count);
			RecordList.AddFromShipment(CreateShipmentDataForTest("101"));
			RecordList.AddFromShipment(CreateShipmentDataForTest("102"));
			RecordList.AddFromShipmentStatus(CreateShipmentStatusDataForTest("103"));
			RecordList.AddFromShipmentStatus(CreateShipmentStatusDataForTest("102"));
			RecordList.AddFromShipmentStatus(CreateShipmentStatusDataForTest("104"));
			RecordList.AddFromShipment(CreateShipmentDataForTest("103"));
			AssertEquals(4, RecordList.Records.Count);
			RecordList.AddFromShipment(CreateShipmentDataForTest("104"));
			AssertEquals("Should not add a new one", 4, RecordList.Records.Count);
		}

		public void TestContainsShipmentDetailRecord()
		{
			IShipmentData shipmentData = CreateShipmentDataForTest("ShipmentRef");
			AssertEquals("Should not have shipment data yet", false, RecordList.ContainsShipmentDetailRecord(shipmentData.ShipmentRef));
			RecordList.AddFromShipment(shipmentData);
			AssertEquals("Should have shipment data after adding it", true, RecordList.ContainsShipmentDetailRecord(shipmentData.ShipmentRef));
		}

		public void TestContainShipmentDetailsRecord_NotCreatingNewRecord()
		{
			AssertEquals(false, RecordList.ContainsShipmentDetailRecord("ASDF"));
			AssertEquals("Should still be empty", true, RecordList.IsEmpty);
			RecordList.AddFromShipment(CreateShipmentDataForTest("ASDF"));
			AssertEquals("Should find it now", true, RecordList.ContainsShipmentDetailRecord("ASDF"));
		}

		[TestDate(2005, 12, 7)]
		public void TestGetHeaderLine()
		{
			RecordList.AddFromShipment(CreateShipmentDataForTest("101", 20, 77));
			RecordList.AddFromShipmentStatus(CreateShipmentStatusDataForTest("102"));
			RecordList.AddFromShipmentStatus(CreateShipmentStatusDataForTest("101"));
			RecordHeaderLine generatedHeaderLine = RecordList.GetHeaderLine(78);
			RecordHeaderLine expectedHeaderLine = new RecordHeaderLine(78, 2, 100);
			AssertEquals(expectedHeaderLine.LineAsString, generatedHeaderLine.LineAsString);
		}

		#region Implementation

		ShipmentDataForTest CreateShipmentDataForTest(string shipmentRef, int numberOfCommodities, int numberOfCharges)
		{
			var result = new ShipmentDataForTest();
			result.ShipmentRef = shipmentRef;
			var commoditiesData = new CommodityDetailData[numberOfCommodities];
			for (var i = 0; i < numberOfCommodities; i++)
			{
				commoditiesData[i] = new CommodityDetailData("Goods", "Tariff", Core.Constants.CountryCodes.Australia, 320.00m);
			}
			result.CommoditiesData = commoditiesData;

			var chargesData = new ShipmentChargeData[numberOfCharges];
			for (var i = 0; i < numberOfCharges; i++)
			{
				chargesData[i] = new ShipmentChargeData(ShipmentChargeTypeCode.Duty, 22, Core.Constants.CurrencyCodes.Australia);
			}
			result.ChargesData = chargesData;
			result.ReceiptsData = System.Array.Empty<ShipmentReceiptData>();
			return result;
		}

		ShipmentDataForTest CreateShipmentDataForTest(string shipmentRef)
		{
			return CreateShipmentDataForTest(shipmentRef, 0, 0);
		}

		ShipmentStatusDataForTest CreateShipmentStatusDataForTest(string shipmentRef)
		{
			ShipmentStatusDataForTest result = new ShipmentStatusDataForTest();
			result.ShipmentRef = shipmentRef;
			return result;
		}

		BISIUploadRecordList RecordList
		{
			get
			{
				if (fRecordList == null)
				{
					fRecordList = new BISIUploadRecordList();
				}

				return fRecordList;
			}
		}

		BISIUploadRecordList fRecordList;
		#endregion
	}
}
