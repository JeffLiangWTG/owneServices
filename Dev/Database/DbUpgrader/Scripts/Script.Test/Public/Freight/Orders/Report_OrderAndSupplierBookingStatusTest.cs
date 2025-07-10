using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Orders;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Orders
{
	[TestedType(typeof(Report_OrderAndSupplierBookingStatus))]
	class Report_OrderAndSupplierBookingStatusTest : DbCreateScriptTest
	{
		protected override bool RequiresSchemaBinding => false;

		public void TestJobSupplierBookingStatusForBuyer()
		{
			SetupTestData();

			var result = GetReportByClient("2abd4a70-bf65-4830-8167-01e3020656d4", "Buyer/Controlling Customer");
			TestJobSupplierBookingStatusForClient(result, 2, 0, "CFS-JD101", "CFS-BUY1", "CFS-SUP1", "CFS-MAN1", "CFS-JSB101", "CFS-CFS1", "CFS-JSL101", "", "", "", "");
			TestJobSupplierBookingStatusForClient(result, 2, 1, "CFS-JD101", "CFS-BUY1", "CFS-SUP1", "CFS-MAN1", "CFS-JSB101", "CFS-CFS1", "CFS-JSL102", "", "", "", "");

			result = GetReportByClient("6a14aacb-ff03-4432-b9a1-6bdf0adf966f", "Buyer/Controlling Customer");
			TestJobSupplierBookingStatusForClient(result, 2, 0, "CFS-JD201", "CFS-BUY2", "CFS-SUP2", "CFS-MAN2", "CFS-JSB201", "CFS-CFS2", "CFS-JSL201", "CFS-JC201", "", "", "");
			TestJobSupplierBookingStatusForClient(result, 2, 1, "CFS-JD201", "CFS-BUY2", "CFS-SUP2", "CFS-MAN2", "CFS-JSB201", "CFS-CFS2", "CFS-JSL202", "CFS-JC201", "", "", "");

			result = GetReportByClient("d5d3e0a4-5094-457d-ab17-cb17c018bb2c", "Buyer/Controlling Customer");
			TestJobSupplierBookingStatusForClient(result, 2, 0, "CFS-JD301", "CFS-BUY3", "CFS-SUP3", "CFS-MAN3", "CFS-JSB301", "CFS-CFS3", "CFS-JSL301", "CFS-JC301", "CFS-JS301", "CFS_JK301", "CFS-JL301");
			TestJobSupplierBookingStatusForClient(result, 2, 1, "CFS-JD301", "CFS-BUY3", "CFS-SUP3", "CFS-MAN3", "CFS-JSB301", "CFS-CFS3", "CFS-JSL302", "CFS-JC301", "CFS-JS301", "CFS_JK301", "CFS-JL302");

			result = GetReportByClient("3bf69f75-7e43-4ccd-8c39-5c1578873f7b", "Buyer/Controlling Customer");
			TestJobSupplierBookingStatusForClient(result, 1, 0, "CY-JD101", "CY-BUY1", "CY-SUP1", "CY-MAN1", "CY-JSB101", "", "CY-JSL101", "", "", "", "");

			result = GetReportByClient("84404f49-4af0-4829-9221-5bda4e1b0404", "Buyer/Controlling Customer");
			TestJobSupplierBookingStatusForClient(result, 2, 0, "CY-JD201", "CY-BUY2", "CY-SUP2", "CY-MAN2", "CY-JSB201", "", "CY-JSL201", "CY-JC201", "", "", "");
			TestJobSupplierBookingStatusForClient(result, 2, 1, "CY-JD201", "CY-BUY2", "CY-SUP2", "CY-MAN2", "CY-JSB201", "", "CY-JSL202", "CY-JC201", "", "", "");

			result = GetReportByClient("bb892828-0c34-4fcc-99a0-54cef886cff1", "Buyer/Controlling Customer");
			TestJobSupplierBookingStatusForClient(result, 2, 0, "CY-JD301", "CY-BUY3", "CY-SUP3", "CY-MAN3", "CY-JSB301", "", "CY-JSL301", "CY-JC301", "CY-JS301", "CY-JK301", "CY-JL301");
			TestJobSupplierBookingStatusForClient(result, 2, 1, "CY-JD301", "CY-BUY3", "CY-SUP3", "CY-MAN3", "CY-JSB301", "", "CY-JSL302", "CY-JC301", "CY-JS301", "CY-JK301", "CY-JL302");
		}

		public void TestJobSupplierBookingStatusForSupplier()
		{
			SetupTestData();

			var result = GetReportByClient("d01169d6-9b0f-42e9-b764-1ccb03f0ab28", "Supplier/Controlling Customer");
			TestJobSupplierBookingStatusForClient(result, 2, 0, "CFS-JD101", "CFS-BUY1", "CFS-SUP1", "CFS-MAN1", "CFS-JSB101", "CFS-CFS1", "CFS-JSL101", "", "", "", "");
			TestJobSupplierBookingStatusForClient(result, 2, 1, "CFS-JD101", "CFS-BUY1", "CFS-SUP1", "CFS-MAN1", "CFS-JSB101", "CFS-CFS1", "CFS-JSL102", "", "", "", "");

			result = GetReportByClient("955c0c32-ec7c-460c-b804-713ada3c752a", "Supplier/Controlling Customer");
			TestJobSupplierBookingStatusForClient(result, 2, 0, "CFS-JD201", "CFS-BUY2", "CFS-SUP2", "CFS-MAN2", "CFS-JSB201", "CFS-CFS2", "CFS-JSL201", "CFS-JC201", "", "", "");
			TestJobSupplierBookingStatusForClient(result, 2, 1, "CFS-JD201", "CFS-BUY2", "CFS-SUP2", "CFS-MAN2", "CFS-JSB201", "CFS-CFS2", "CFS-JSL202", "CFS-JC201", "", "", "");

			result = GetReportByClient("83261c6c-d316-43fe-9bf2-8917976343df", "Supplier/Controlling Customer");
			TestJobSupplierBookingStatusForClient(result, 2, 0, "CFS-JD301", "CFS-BUY3", "CFS-SUP3", "CFS-MAN3", "CFS-JSB301", "CFS-CFS3", "CFS-JSL301", "CFS-JC301", "CFS-JS301", "CFS_JK301", "CFS-JL301");
			TestJobSupplierBookingStatusForClient(result, 2, 1, "CFS-JD301", "CFS-BUY3", "CFS-SUP3", "CFS-MAN3", "CFS-JSB301", "CFS-CFS3", "CFS-JSL302", "CFS-JC301", "CFS-JS301", "CFS_JK301", "CFS-JL302");

			result = GetReportByClient("2e8ab22c-d8f0-4652-9e13-b08c25d2eabf", "Supplier/Controlling Customer");
			TestJobSupplierBookingStatusForClient(result, 1, 0, "CY-JD101", "CY-BUY1", "CY-SUP1", "CY-MAN1", "CY-JSB101", "", "CY-JSL101", "", "", "", "");

			result = GetReportByClient("d5395962-c429-48a8-9975-a6643e220445", "Supplier/Controlling Customer");
			TestJobSupplierBookingStatusForClient(result, 2, 0, "CY-JD201", "CY-BUY2", "CY-SUP2", "CY-MAN2", "CY-JSB201", "", "CY-JSL201", "CY-JC201", "", "", "");
			TestJobSupplierBookingStatusForClient(result, 2, 1, "CY-JD201", "CY-BUY2", "CY-SUP2", "CY-MAN2", "CY-JSB201", "", "CY-JSL202", "CY-JC201", "", "", "");

			result = GetReportByClient("e121790c-2594-4b45-b539-516cb90fbef8", "Supplier/Controlling Customer");
			TestJobSupplierBookingStatusForClient(result, 2, 0, "CY-JD301", "CY-BUY3", "CY-SUP3", "CY-MAN3", "CY-JSB301", "", "CY-JSL301", "CY-JC301", "CY-JS301", "CY-JK301", "CY-JL301");
			TestJobSupplierBookingStatusForClient(result, 2, 1, "CY-JD301", "CY-BUY3", "CY-SUP3", "CY-MAN3", "CY-JSB301", "", "CY-JSL302", "CY-JC301", "CY-JS301", "CY-JK301", "CY-JL302");
		}

		public void TestJobSupplierBookingStatusForOrderNumber()
		{
			SetupTestData();

			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_OrderAndSupplierBookingStatus (NULL, NULL, NULL, '', '', 'CFS', null, null) ORDER BY OrderNumber,BookingId,BookingLineId");
			TestJobSupplierBookingStatusForClient(result, 6, 0, "CFS-JD101", "CFS-BUY1", "CFS-SUP1", "CFS-MAN1", "CFS-JSB101", "CFS-CFS1", "CFS-JSL101", "", "", "", "");
			TestJobSupplierBookingStatusForClient(result, 6, 1, "CFS-JD101", "CFS-BUY1", "CFS-SUP1", "CFS-MAN1", "CFS-JSB101", "CFS-CFS1", "CFS-JSL102", "", "", "", "");
			TestJobSupplierBookingStatusForClient(result, 6, 2, "CFS-JD201", "CFS-BUY2", "CFS-SUP2", "CFS-MAN2", "CFS-JSB201", "CFS-CFS2", "CFS-JSL201", "CFS-JC201", "", "", "");
			TestJobSupplierBookingStatusForClient(result, 6, 3, "CFS-JD201", "CFS-BUY2", "CFS-SUP2", "CFS-MAN2", "CFS-JSB201", "CFS-CFS2", "CFS-JSL202", "CFS-JC201", "", "", "");
			TestJobSupplierBookingStatusForClient(result, 6, 4, "CFS-JD301", "CFS-BUY3", "CFS-SUP3", "CFS-MAN3", "CFS-JSB301", "CFS-CFS3", "CFS-JSL301", "CFS-JC301", "CFS-JS301", "CFS_JK301", "CFS-JL301");
			TestJobSupplierBookingStatusForClient(result, 6, 5, "CFS-JD301", "CFS-BUY3", "CFS-SUP3", "CFS-MAN3", "CFS-JSB301", "CFS-CFS3", "CFS-JSL302", "CFS-JC301", "CFS-JS301", "CFS_JK301", "CFS-JL302");

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_OrderAndSupplierBookingStatus (NULL, NULL, NULL, '', '', 'XXXX', null, null) ORDER BY OrderNumber,BookingId,BookingLineId");
			AssertEquals(0, result.Rows.Count);
		}

		public void TestJobSupplierBookingStatusForVessel()
		{
			SetupTestData();

			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_OrderAndSupplierBookingStatus ('MSC CLAUDIA', NULL, NULL, '', '', '', null, null) ORDER BY OrderNumber,BookingId,BookingLineId");
			TestJobSupplierBookingStatusForClient(result, 2, 0, "CFS-JD301", "CFS-BUY3", "CFS-SUP3", "CFS-MAN3", "CFS-JSB301", "CFS-CFS3", "CFS-JSL301", "CFS-JC301", "CFS-JS301", "CFS_JK301", "CFS-JL301");
			TestJobSupplierBookingStatusForClient(result, 2, 1, "CFS-JD301", "CFS-BUY3", "CFS-SUP3", "CFS-MAN3", "CFS-JSB301", "CFS-CFS3", "CFS-JSL302", "CFS-JC301", "CFS-JS301", "CFS_JK301", "CFS-JL302");

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_OrderAndSupplierBookingStatus ('XXX', NULL, NULL, '', '', '', null, null) ORDER BY OrderNumber,BookingId,BookingLineId");
			AssertEquals(0, result.Rows.Count);
		}

		public void TestJobSupplierBookingStatusForVoyage()
		{
			SetupTestData();

			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_OrderAndSupplierBookingStatus (NULL, 'CFS301', NULL, '', '', '', null, null) ORDER BY OrderNumber,BookingId,BookingLineId");
			TestJobSupplierBookingStatusForClient(result, 2, 0, "CFS-JD301", "CFS-BUY3", "CFS-SUP3", "CFS-MAN3", "CFS-JSB301", "CFS-CFS3", "CFS-JSL301", "CFS-JC301", "CFS-JS301", "CFS_JK301", "CFS-JL301");
			TestJobSupplierBookingStatusForClient(result, 2, 1, "CFS-JD301", "CFS-BUY3", "CFS-SUP3", "CFS-MAN3", "CFS-JSB301", "CFS-CFS3", "CFS-JSL302", "CFS-JC301", "CFS-JS301", "CFS_JK301", "CFS-JL302");

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_OrderAndSupplierBookingStatus (NULL, 'XXX', NULL, '', '', '', null, null) ORDER BY OrderNumber,BookingId,BookingLineId");
			AssertEquals(0, result.Rows.Count);
		}

		void TestJobSupplierBookingStatusForClient(
			DataTable table,
			int resultCount,
			int index,
			string orderNumber,
			string buyerCode,
			string supplierCode,
			string manufactureCode,
			string bookingId,
			string cfsReceiptCode,
			string bookingLineId,
			string containerNumber,
			string shipmentNumber,
			string consolNumber,
			string packLineId)
		{
			var result = table;
			AssertEquals(resultCount, result.Rows.Count);
			if (resultCount == 0)
			{
				return;
			}

			var row = result.Rows[index];
			AssertEquals(orderNumber, row["OrderNumber"].ToString());
			AssertEquals(buyerCode, row["BuyerCode"].ToString());
			AssertEquals(supplierCode, row["SupplierCode"].ToString());
			AssertEquals(manufactureCode, row["ManufacturerCode"].ToString());
			AssertEquals(bookingId, row["BookingId"].ToString());
			AssertEquals(bookingLineId, row["BookingLineId"].ToString());
			AssertEquals(shipmentNumber, row["ShipmentNumber"].ToString());
			AssertEquals(consolNumber, row["consolNumber"].ToString());
			AssertEquals(packLineId, row["PackLineId"].ToString());
			AssertField(containerNumber, row["ContainerNumber"]);
			AssertField(cfsReceiptCode, row["BookingCFSReceiptCode"]);
		}

		DataTable GetReportByClient(string clientPK, string clientType)
		{
			if (clientType == "Buyer/Controlling Customer")
			{
				return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_OrderAndSupplierBookingStatus (NULL, NULL, NULL, '', '', '', null, null) WHERE BuyerPK = '{clientPK}' ORDER BY OrderNumber,BookingId,BookingLineId");
			}
			else
			{
				return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_OrderAndSupplierBookingStatus (NULL, NULL, NULL, '', '', '', null, null) WHERE SupplierPK = '{clientPK}' ORDER BY OrderNumber,BookingId,BookingLineId");
			}
		}

		static void AssertField(string expectedValue, object cellValue)
		{
			if (string.IsNullOrEmpty(expectedValue))
			{
				Assert(cellValue == null || string.IsNullOrEmpty(cellValue.ToString()));
			}
			else
			{
				AssertEquals(expectedValue, cellValue.ToString());
			}
		}

		static void SetupTestData()
		{
			SetupTestDataForCFSBooking();
			SetupTestDataForCFS();
			SetupTestDataForConvertedCFS();

			SetupTestDataForCYBooking();
			SetupTestDataForCY();
			SetupTestDataForConvertedCY();
		}

		static void SetupTestDataForCFSBooking()
		{
			var buyerPK = TestDataCreator.CreateOrganisation("CFS-BUY1", "Buyer", orgHeaderPK: new Guid("2abd4a70-bf65-4830-8167-01e3020656d4"));
			var supplierPK = TestDataCreator.CreateOrganisation("CFS-SUP1", "Supplier", orgHeaderPK: new Guid("d01169d6-9b0f-42e9-b764-1ccb03f0ab28"));
			var manufacturerPK = TestDataCreator.CreateOrganisation("CFS-MAN1", "Manufacturer", orgHeaderPK: new Guid("c3a0e7bf-99e2-4c0d-ad7d-7867ce888d89"));
			var receiptPK = TestDataCreator.CreateOrganisation("CFS-CFS1", "Receipt", orgHeaderPK: new Guid("3fed55b9-ad1a-459c-918e-5e8a119dae3b"));

			var buyerAddressPK = TestDataCreator.CreateAddress(buyerPK, "CFS-BUY1", "buyer st");
			var supplierAddressPK = TestDataCreator.CreateAddress(supplierPK, "CFS-SUP1", "supplier st");
			var manufacturerAddressPK = TestDataCreator.CreateAddress(manufacturerPK, "CFS-MAN1", "manufacturer st");
			var receiptAddressPK = TestDataCreator.CreateAddress(receiptPK, "CFS-CFS1", "receipt st");

			var orderPK = TestDataCreator.CreateJobOrderHeader("CFS-JD101", buyerAddressPK, supplierAddress: supplierAddressPK, orderStatus: "PLC");
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", orderPK, "JD", "MAN");
			var orderLinePK = TestDataCreator.CreateJobOrderLine(orderPK);

			var supplierBookingPK = TestDataCreator.CreateJobSupplierBooking("CFS-JSB101", supplierPK, "PLN", cfdAddressPK: receiptAddressPK);
			TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, supplierBookingPK, "CFS-JSL101");
			TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, supplierBookingPK, "CFS-JSL102");
		}

		static void SetupTestDataForCFS()
		{
			var buyerPK = TestDataCreator.CreateOrganisation("CFS-BUY2", "Buyer", orgHeaderPK: new Guid("6a14aacb-ff03-4432-b9a1-6bdf0adf966f"));
			var supplierPK = TestDataCreator.CreateOrganisation("CFS-SUP2", "Supplier", orgHeaderPK: new Guid("955c0c32-ec7c-460c-b804-713ada3c752a"));
			var manufacturerPK = TestDataCreator.CreateOrganisation("CFS-MAN2", "Manufacturer", orgHeaderPK: new Guid("eeb5ff43-85c4-49c8-9168-ce5b2c93689c"));
			var controllingCustomerPK = TestDataCreator.CreateOrganisation("CFS-CCP2", "Controlling Customer", orgHeaderPK: new Guid("68942281-520f-4fad-bc76-986a7ff63a8a"));
			var loadListBookingPartyPK = TestDataCreator.CreateOrganisation("CFS-BKD2", "Booking Party", orgHeaderPK: new Guid("65bd420c-e13a-40d2-b553-73fba41314c4"));
			var receiptPK = TestDataCreator.CreateOrganisation("CFS-CFS2", "Receipt", orgHeaderPK: new Guid("7bddcb21-cb6b-4332-9221-152b95d772ec"));

			var buyerAddressPK = TestDataCreator.CreateAddress(buyerPK, "CFS-BUY2", "buyer st");
			var supplierAddressPK = TestDataCreator.CreateAddress(supplierPK, "CFS-SUP2", "supplier st");
			var manufacturerAddressPK = TestDataCreator.CreateAddress(manufacturerPK, "CFS-MAN2", "manufacturer st");
			var controllingCustomerAddressPK = TestDataCreator.CreateAddress(controllingCustomerPK, "CFS-SCP2", "controllingCustomer st");
			var receiptAddressPK = TestDataCreator.CreateAddress(receiptPK, "CFS-CFS2", "receipt st");

			var orderPK = TestDataCreator.CreateJobOrderHeader("CFS-JD201", buyerAddressPK, supplierAddress: supplierAddressPK, orderStatus: "PLC");
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", orderPK, "JD", "MAN");
			var orderLinePK = TestDataCreator.CreateJobOrderLine(orderPK);
			var supplierBookingPK = TestDataCreator.CreateJobSupplierBooking("CFS-JSB201", supplierPK, "PLN", cfdAddressPK: receiptAddressPK);

			var supplierBookingLinePK1 = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, supplierBookingPK, "CFS-JSL201");
			var supplierBookingLinePK2 = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, supplierBookingPK, "CFS-JSL202");

			var containerLoadListPK = TestDataCreator.CreateCFSContainerLoadList("CFS-CLH201", supplierBookingPK, loadListBookingPartyPK, "PLN");
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", containerLoadListPK, "CLH", "SCP");

			var consolPK = TestDataCreator.CreateJobConsol();
			TestDataCreator.CreateJobConsolTransport(consolPK);

			var refContainerPK = TestDataCreator.CreateRefContainer("CFS-RC201");
			var containerPK = TestDataCreator.CreateJobContainer("CFS-JC201", refContainerPK, consolPK);

			TestDataCreator.CreateCFSContainerLoadListLine(containerLoadListPK, supplierBookingLinePK1, containerPK, null);
			TestDataCreator.CreateCFSContainerLoadListLine(containerLoadListPK, supplierBookingLinePK2, containerPK, null);
		}

		static void SetupTestDataForConvertedCFS()
		{
			var buyerPK = TestDataCreator.CreateOrganisation("CFS-BUY3", "Buyer", orgHeaderPK: new Guid("d5d3e0a4-5094-457d-ab17-cb17c018bb2c"));
			var supplierPK = TestDataCreator.CreateOrganisation("CFS-SUP3", "Supplier", orgHeaderPK: new Guid("83261c6c-d316-43fe-9bf2-8917976343df"));
			var manufacturerPK = TestDataCreator.CreateOrganisation("CFS-MAN3", "Manufacturer", orgHeaderPK: new Guid("68feb2c7-6533-40de-82c1-abc62725a5c5"));
			var controllingCustomerPK = TestDataCreator.CreateOrganisation("CFS-SCP3", "Controlling Customer", orgHeaderPK: new Guid("cb9e3b51-d5f8-41db-929f-392670c9fc82"));
			var loadListBookingPartyPK = TestDataCreator.CreateOrganisation("CFS-BKD3", "Booking Party", orgHeaderPK: new Guid("3c57e9a4-a6d9-416e-bae8-46e275e99e97"));
			var receiptPK = TestDataCreator.CreateOrganisation("CFS-CFS3", "Receipt", orgHeaderPK: new Guid("edb00421-360d-4211-9f88-1b90c8ca7924"));

			var buyerAddressPK1 = TestDataCreator.CreateAddress(buyerPK, "CFS-BUY3", "buyer st");
			var supplierAddressPK1 = TestDataCreator.CreateAddress(supplierPK, "CFS-SUP3", "supplier st");
			var manufacturerAddressPK = TestDataCreator.CreateAddress(manufacturerPK, "CFS-MAN3", "manufacturer st");
			var controllingCustomerAddressPK = TestDataCreator.CreateAddress(controllingCustomerPK, "CFS-CTRLCUSYD3", "controllingCustomer st");
			var receiptAddressPK = TestDataCreator.CreateAddress(receiptPK, "CFS-CFS3", "receipt st");

			var orderPK = TestDataCreator.CreateJobOrderHeader("CFS-JD301", buyerAddressPK1, supplierAddress: supplierAddressPK1, orderStatus: "PLC");
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", orderPK, "JD", "MAN");
			var orderLinePK = TestDataCreator.CreateJobOrderLine(orderPK);
			var supplierBookingPK = TestDataCreator.CreateJobSupplierBooking("CFS-JSB301", supplierPK, "PLN", cfdAddressPK: receiptAddressPK);
			TestDataCreator.CreateDocAddress(supplierAddressPK1, "", supplierBookingPK, "JSB", "SUD");

			var supplierBookingLinePK1 = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, supplierBookingPK, "CFS-JSL301");
			var supplierBookingLinePK2 = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, supplierBookingPK, "CFS-JSL302");
			var containerLoadListPK = TestDataCreator.CreateCFSContainerLoadList("CFS-CLH301", supplierBookingPK, loadListBookingPartyPK, "PLN");
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", containerLoadListPK, "CLH", "SCP");

			var consolPK = TestDataCreator.CreateJobConsol("CFS_JK301");
			TestDataCreator.CreateJobConsolTransport(consolPK, "MSC CLAUDIA", "CFS301");

			var refContainerPK = TestDataCreator.CreateRefContainer("CFS-RC301");
			var containerPK = TestDataCreator.CreateJobContainer("CFS-JC301", refContainerPK, consolPK);

			var shipmentPK = TestDataCreator.CreateShipment("CFS-JS301");
			TestDataCreator.CreateJobConShipLink(shipmentPK, consolPK);
			var packLinePK1 = TestDataCreator.CreateJobPackLines(shipmentPK, "CFS-JL301");
			var packLinePK2 = TestDataCreator.CreateJobPackLines(shipmentPK, "CFS-JL302");

			TestDataCreator.CreateCFSContainerLoadListLine(containerLoadListPK, supplierBookingLinePK1, containerPK, packLinePK1);
			TestDataCreator.CreateCFSContainerLoadListLine(containerLoadListPK, supplierBookingLinePK2, containerPK, packLinePK2);
		}

		static void SetupTestDataForCYBooking()
		{
			var buyerPK = TestDataCreator.CreateOrganisation("CY-BUY1", "Buyer", orgHeaderPK: new Guid("3bf69f75-7e43-4ccd-8c39-5c1578873f7b"));
			var supplierPK = TestDataCreator.CreateOrganisation("CY-SUP1", "Supplier", orgHeaderPK: new Guid("2e8ab22c-d8f0-4652-9e13-b08c25d2eabf"));
			var manufacturerPK = TestDataCreator.CreateOrganisation("CY-MAN1", "Manufacturer", orgHeaderPK: new Guid("35729a70-e8c3-4e45-84b5-f37ddbaff5c8"));
			var controllingCustomerPK = TestDataCreator.CreateOrganisation("CY-SCP1", "Controlling Customer", orgHeaderPK: new Guid("adfb2a42-e71e-4770-bbfd-1f5d775a49d6"));

			var buyerAddressPK = TestDataCreator.CreateAddress(buyerPK, "CY-BUY1", "buyer st");
			var supplierAddressPK = TestDataCreator.CreateAddress(supplierPK, "CY-SUP1", "supplier st");
			var manufacturerAddressPK = TestDataCreator.CreateAddress(manufacturerPK, "CY-MAN1", "manufacturer st");
			var controllingCustomerAddressPK = TestDataCreator.CreateAddress(controllingCustomerPK, "CY-SCP1", "controllingCustomer st");

			var orderPK = TestDataCreator.CreateJobOrderHeader("CY-JD101", buyerAddressPK, supplierAddress: supplierAddressPK);
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", orderPK, "JD", "MAN");
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", orderPK, "JD", "SCP");
			var orderLinePK = TestDataCreator.CreateJobOrderLine(orderPK, 1);
			var bookingHeaderPK = TestDataCreator.CreateJobSupplierBooking("CY-JSB101", supplierPK, status: "INC");
			TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, bookingHeaderPK, "CY-JSL101");
		}

		static void SetupTestDataForCY()
		{
			var buyerPK = TestDataCreator.CreateOrganisation("CY-BUY2", "Buyer", orgHeaderPK: new Guid("84404f49-4af0-4829-9221-5bda4e1b0404"));
			var supplierPK = TestDataCreator.CreateOrganisation("CY-SUP2", "Supplier", orgHeaderPK: new Guid("d5395962-c429-48a8-9975-a6643e220445"));
			var manufacturerPK = TestDataCreator.CreateOrganisation("CY-MAN2", "Manufacturer", orgHeaderPK: new Guid("1141193f-5c9f-4dc6-93f2-34073cb835f6"));
			var controllingCustomerPK = TestDataCreator.CreateOrganisation("CY-SCP2", "Controlling Customer", orgHeaderPK: new Guid("5410bdc3-3a2d-497b-bc1f-0f3b025d74d1"));

			var buyerAddressPK = TestDataCreator.CreateAddress(buyerPK, "CY-BUY2", "buyer st");
			var supplierAddressPK = TestDataCreator.CreateAddress(supplierPK, "CY-SUP2", "supplier st");
			var manufacturerAddressPK = TestDataCreator.CreateAddress(manufacturerPK, "CY-MAN2", "manufacturer st");
			var controllingCustomerAddressPK = TestDataCreator.CreateAddress(controllingCustomerPK, "CY-SCP2", "1 controllingCustomer st");

			var consolPK = TestDataCreator.CreateJobConsol("CY-JK201");
			TestDataCreator.CreateJobConsolTransport(consolPK);
			var refContainerPK = TestDataCreator.CreateRefContainer("CY-RC201");
			var container1PK = TestDataCreator.CreateJobContainer("CY-JC201", refContainerPK, consolPK);

			var orderPK = TestDataCreator.CreateJobOrderHeader("CY-JD201", buyerAddressPK, supplierAddress: supplierAddressPK);
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", orderPK, "JD", "MAN");
			var orderLinePK = TestDataCreator.CreateJobOrderLine(orderPK, 1);
			var bookingHeaderPK = TestDataCreator.CreateJobSupplierBooking("CY-JSB201", supplierPK, status: "INC");
			var bookingLine1PK = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, bookingHeaderPK, "CY-JSL201");
			var bookingLine2PK = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, bookingHeaderPK, "CY-JSL202");
			var loadListHeaderPK = TestDataCreator.CreateCYContainerLoadList("CY-CLH201", bookingHeaderPK, manufacturerPK, "CNV");
			TestDataCreator.CreateCYContainerLoadListLine(loadListHeaderPK, bookingLine1PK, container1PK, null);
			TestDataCreator.CreateCYContainerLoadListLine(loadListHeaderPK, bookingLine2PK, container1PK, null);

			TestDataCreator.CreateDocAddress(supplierAddressPK, "", bookingHeaderPK, "JSB", "SUD");
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", bookingHeaderPK, "JSB", "SCP");
		}

		static void SetupTestDataForConvertedCY()
		{
			var buyerPK = TestDataCreator.CreateOrganisation("CY-BUY3", "Buyer", orgHeaderPK: new Guid("bb892828-0c34-4fcc-99a0-54cef886cff1"));
			var supplierPK = TestDataCreator.CreateOrganisation("CY-SUP3", "Supplier", orgHeaderPK: new Guid("e121790c-2594-4b45-b539-516cb90fbef8"));
			var manufacturerPK = TestDataCreator.CreateOrganisation("CY-MAN3", "Manufacturer", orgHeaderPK: new Guid("819567f8-b1ab-4a69-bc24-ce1bd015a578"));
			var controllingCustomerPK = TestDataCreator.CreateOrganisation("CY-SCP", "Controlling Customer", orgHeaderPK: new Guid("3327cf98-b5d1-4528-8304-3f316624a5c5"));

			var buyerAddressPK = TestDataCreator.CreateAddress(buyerPK, "CY-BUY3", "buyer st");
			var supplierAddressPK = TestDataCreator.CreateAddress(supplierPK, "CY-SUP3", "supplier st");
			var manufacturerAddressPK = TestDataCreator.CreateAddress(manufacturerPK, "CY-MAN3", "manufacturer st");
			var controllingCustomerAddressPK = TestDataCreator.CreateAddress(controllingCustomerPK, "CY-SCP3", "controllingCustomer st");

			var consolPK = TestDataCreator.CreateJobConsol("CY-JK301");
			TestDataCreator.CreateJobConsolTransport(consolPK, "OOCL BEIJING", "CF301");
			var refContainerPK = TestDataCreator.CreateRefContainer("CY-RC301");
			var containerPK = TestDataCreator.CreateJobContainer("CY-JC301", refContainerPK, consolPK);
			var shipmentPK = TestDataCreator.CreateShipment("CY-JS301");
			TestDataCreator.CreateJobConShipLink(shipmentPK, consolPK);
			var packLinePK1 = TestDataCreator.CreateJobPackLines(shipmentPK, "CY-JL301");
			var packLinePK2 = TestDataCreator.CreateJobPackLines(shipmentPK, "CY-JL302");

			var orderPK = TestDataCreator.CreateJobOrderHeader("CY-JD301", buyerAddressPK, supplierAddress: supplierAddressPK);
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", orderPK, "JD", "MAN");
			var orderLinePK = TestDataCreator.CreateJobOrderLine(orderPK, 1);
			var bookingHeaderPK = TestDataCreator.CreateJobSupplierBooking("CY-JSB301", supplierPK, status: "INC");
			var bookingLinePK1 = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, bookingHeaderPK, "CY-JSL301");
			var bookingLinePK2 = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, bookingHeaderPK, "CY-JSL302");

			var loadListHeaderPK = TestDataCreator.CreateCYContainerLoadList("CLH107", bookingHeaderPK, manufacturerPK, "CNV");
			TestDataCreator.CreateCYContainerLoadListLine(loadListHeaderPK, bookingLinePK1, containerPK, packLinePK1);
			TestDataCreator.CreateCYContainerLoadListLine(loadListHeaderPK, bookingLinePK2, containerPK, packLinePK2);

			TestDataCreator.CreateDocAddress(supplierAddressPK, "", bookingHeaderPK, "JSB", "SUD");
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", bookingHeaderPK, "JSB", "SCP");
		}
	}
}
