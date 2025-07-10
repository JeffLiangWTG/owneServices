using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Orders;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Freight.Orders
{
	[TestedType(typeof(GetJobSupplierBookingsWithSecurityContext))]
	class GetJobSupplierBookingsWithSecurityContextTest : BaseSupplierBookingSecurityContextTest
	{
		public void TestAllColumnsAreContainedInScript()
		{
			var expectedColumns = DbObjectCreator.GetTableColumns(Db.Connection, "JobSupplierBooking");
			var results = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.GetJobSupplierBookingsWithSecurityContext(NEWID())");

			CombineAssertions(() =>
			{
				foreach (var expectedColumn in expectedColumns)
				{
					Assert($"Column {expectedColumn} isn't returned by the SQL function {nameof(GetJobSupplierBookingsWithSecurityContext)}. Please add them to the function", results.Columns.Contains(expectedColumn));
				}
			});
		}

		public void TestRestrictedVisibilty_Buyer()
		{
			CombineAssertions("Buyer Visibility", () =>
			{
				foreach (var status in Statuses)
				{
					SetUpBookingWithOneLine(status);

					var results = GetResults(buyerPK);
					AssertEquals($"count for one booking", 1, results.Rows.Count);
					AssertEquals($"CanEdit value for {status} bookings", EditableStatus.Contains(status), results.Rows[0]["CanEdit"]);
					ClearBookingsAndLines();
				}
			});
		}

		public void TestRestrictedVisibilty_ControllingCustomer()
		{
			CombineAssertions("Controlling Customer Visibility", () =>
			{
				foreach (var status in Statuses)
				{
					SetUpBookingWithOneLine(status);
					var results = GetResults(controllingCustomerPK);
					AssertEquals($"count for one booking", 1, results.Rows.Count);
					AssertEquals($"CanEdit value for {status} bookings", EditableStatus.Contains(status), results.Rows[0]["CanEdit"]);
					ClearBookingsAndLines();
				}
			});
		}

		public void TestRestrictedVisibilty_Supplier()
		{
			CombineAssertions("Supplier Visibility", () =>
			{
				foreach (var status in Statuses)
				{
					SetUpBookingWithOneLine(status);
					var results = GetResults(supplierPK);
					AssertEquals($"count for {status} bookings", 1, results.Rows.Count);
					AssertEquals($"CanEdit value for {status} bookings", EditableStatuses.Contains(status), results.Rows[0]["CanEdit"]);
					ClearBookingsAndLines();
				}
			});
		}

		public void TestRestrictedVisibilty_Manufacturer()
		{
			CombineAssertions("Manufacturer Visibility", () =>
			{
				foreach (var status in Statuses)
				{
					SetUpBookingWithOneLine(status);
					var results = GetResults(manufacturerPK);
					AssertEquals($"count for {status} bookings", 1, results.Rows.Count);
					AssertEquals($"CanEdit value for {status} bookings", false, results.Rows[0]["CanEdit"]);
					ClearBookingsAndLines();
				}
			});
		}

		public void TestRestrictedVisibilty_Unrelated()
		{
			CombineAssertions("Unrelated Org Visibility", () =>
			{
				foreach (var status in Statuses)
				{
					SetUpBookingWithOneLine(status);
					var results = GetResults(miscOrgPK);
					AssertEquals($"count for {status} bookings", 0, results.Rows.Count);
					ClearBookingsAndLines();
				}
			});
		}

		public void TestGetJobSupplierBooking_NoDuplicateRows()
		{
			SetUpOrganisations();

			var orderPK = TestDataCreator.CreateJobOrderHeader("ORD01", buyerAddressPK, supplierAddress: supplierAddressPK, orderStatus: "PLC");
			var orderLinePK = TestDataCreator.CreateJobOrderLine(orderPK);

			var supplierBookingPK = TestDataCreator.CreateJobSupplierBooking("SBK01", supplierPK, "PLN");
			TestDataCreator.CreateDocAddress(supplierAddressPK, "", supplierBookingPK, "JSB", "SUD", 0);
			TestDataCreator.CreateDocAddress(supplierAddressPK, "", supplierBookingPK, "JSB", "SUD", 1);
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", supplierBookingPK, "JSB", "SCP", 0);
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", supplierBookingPK, "JSB", "SCP", 1);

			var supplierBookingLinePK = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, supplierBookingPK, "JSL001");
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", supplierBookingLinePK, "JSL", "MAN", 0);
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", supplierBookingLinePK, "JSL", "MAN", 1);

			var results = GetResults(buyerPK).Select();
			AssertEquals("Should only return 1 supplier booking with context attached", 1, results.Length);
		}

		DataTable GetResults(Guid orgPK)
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM dbo.GetJobSupplierBookingsWithSecurityContext('{orgPK}') ORDER BY JSB_BookingID");
		}
	}
}
