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
	[TestedType(typeof(GetJobSupplierBookingLinesWithSecurityContext))]
	class GetJobSupplierBookingLinesWithSecurityContextTest : BaseSupplierBookingSecurityContextTest
	{
		public void TestAllColumnsAreContainedInScript()
		{
			var expectedColumns = DbObjectCreator.GetTableColumns(Db.Connection, "JobSupplierBookingLine");
			var results = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.GetJobSupplierBookingLinesWithSecurityContext(NEWID())");

			CombineAssertions(() =>
			{
				foreach (var expectedColumn in expectedColumns)
				{
					Assert($"Column {expectedColumn} isn't returned by the SQL function {nameof(GetJobSupplierBookingLinesWithSecurityContext)}. Please add them to the function", results.Columns.Contains(expectedColumn));
				}
			});
		}

		public void TestBookingLineVisibilityFromUnrelatedOrg()
		{
			SetUpTestData();
			var unrelatedOrg = TestDataCreator.CreateOrganisation("UR", "Unrelated Org");
			AssertContainsExactElementsInAnyOrder(
				"None of the booking lines should be visible to a completely unrelated org",
				Array.Empty<string>(),
				GetSupplierBookingLinePKs(unrelatedOrg)
			);
		}

		public void TestBookingLineVisibilityFromBuyer()
		{
			SetUpTestData();
			AssertContainsExactElementsInAnyOrder(
				"The \"Buyer\" org is the buyer of orders 1, 2, 3, and 5a, " +
				"which correspond to booking lines 1, 2, 3, and 5a",
				new[] { "bookingLine1PK", "bookingLine2PK", "bookingLine3PK", "bookingLine5aPK" },
				GetSupplierBookingLinePKs(buyerPK)
			);
		}

		public void TestBookingLineVisibilityFromSupplier()
		{
			SetUpTestData();
			AssertContainsExactElementsInAnyOrder(
				"The \"Supplier\" org is the supplier for orders 1, 2, 5a, and 5b, bookings SB01 and SB05. " +
				"These related to booking lines 1, 2, 5a and 5b",
				new[] { "bookingLine1PK", "bookingLine2PK", "bookingLine5aPK", "bookingLine5bPK" },
				GetSupplierBookingLinePKs(supplierPK)
			);
		}

		public void TestBookingLineVisibilityFromManufacturer()
		{
			SetUpTestData();
			AssertContainsExactElementsInAnyOrder(
				"The \"Manufacturer\" org is assigned as the manufacturer for booking lines 1, 2, 5a and 5b, and is the booking party for SB02",
				new[] { "bookingLine1PK", "bookingLine2PK", "bookingLine5aPK", "bookingLine5bPK" },
				GetSupplierBookingLinePKs(manufacturerPK)
			);
		}

		public void TestBookingLineVisibilityFromControllingCustomer()
		{
			SetUpTestData();
			AssertContainsExactElementsInAnyOrder(
				"The \"Controlling Customer\" org is assigned as the controlling customer for bookings SB01, SB02, SB03, and SB05. " +
				"All are visible based on their booking status (INC, CAN, PLC and REJ).",
				new[] { "bookingLine1PK", "bookingLine2PK", "bookingLine3PK", "bookingLine5aPK", "bookingLine5bPK" },
				GetSupplierBookingLinePKs(controllingCustomerPK)
			);
		}

		public void TestBookingLineVisibilityFromMultiRoleOrg()
		{
			SetUpTestData();
			AssertContainsExactElementsInAnyOrder(
				"The \"Misc org\" org is the manufactuerer of booking lines 3 and 4; " +
				"it is also the buyers of orders 4 and 5b; " +
				"it is also the suppliers for orders 3 and 4; " +
				"it is also the booking party of bookings SB03 and SB04; " +
				"it is also the controlling customer of booking SB04...",
				new[] { "bookingLine3PK", "bookingLine4PK", "bookingLine5bPK", "bookingLine6PK" },
				GetSupplierBookingLinePKs(miscOrgPK)
			);
		}

		public void TestCanEdit()
		{
			SetUpTestData();
			AssertContainsExactElementsInAnyOrder(
				"The \"Supplier\" org as a supplier can edit booking lines 1 (INC), 5a (REJ), and 5b (REJ)",
				new[]
				{
					("bookingLine1PK", true),
					("bookingLine2PK", false),
					("bookingLine5aPK", true),
					("bookingLine5bPK", true),
				},
				GetSupplierBookingLinePKsAndCanEditFlags(supplierPK)
			);

			AssertContainsExactElementsInAnyOrder(
				"The \"Misc org\" org as a supplier and booking party can't edit booking line 3 or 4 because SB03 and SB04 are PLC and PLN respectively",
				new[]
				{
					("bookingLine3PK", false),
					("bookingLine4PK", false),
					("bookingLine5bPK", false),
					("bookingLine6PK", false),
				},
				GetSupplierBookingLinePKsAndCanEditFlags(miscOrgPK)
			);

			AssertContainsExactElementsInAnyOrder(
				"The \"Manufacturer\" org cannot edit booking line 2 as the booking party because the status for the booking line is CAN",
				new[]
				{
					("bookingLine1PK", false),
					("bookingLine2PK", false),
					("bookingLine5aPK", false),
					("bookingLine5bPK", false),
				},
				GetSupplierBookingLinePKsAndCanEditFlags(manufacturerPK)
			);
		}

		public void TestGetJobSupplierBookingLines_NoDuplicateRows()
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
			AssertEquals("Should only return 1 supplier booking line with context attached", 1, results.Length);
		}

		DataTable GetResults(Guid orgPK)
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT results.* FROM dbo.GetJobSupplierBookingLinesWithSecurityContext('{orgPK}') results JOIN dbo.JobOrderLine ON JO_PK = JSL_JO_OrderLine ORDER BY JO_LineNo");
		}

		string[] GetSupplierBookingLinePKs(Guid orgPK) =>
			GetResults(orgPK)
			.Rows
			.Cast<DataRow>()
			.Select(row => pkName.TryGetValue(row.Field<Guid>("JSL_PK"), out var name) ? name : "?")
			.ToArray();

		(string, bool)[] GetSupplierBookingLinePKsAndCanEditFlags(Guid orgPK) =>
			GetResults(orgPK)
			.Rows
			.Cast<DataRow>()
			.Select(row => (
				pkName.TryGetValue(row.Field<Guid>("JSL_PK"), out var name) ? name : "?",
				row.Field<bool>("CanEdit")
			))
			.ToArray();
	}
}

