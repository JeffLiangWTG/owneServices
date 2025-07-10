using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;

namespace Enterprise.Build.Database.Script.Testing.Public.Freight.Orders
{
	abstract class BaseSupplierBookingSecurityContextTest : DbCreateScriptTest
	{
		protected void SetUpTestData()
		{
			/// ---------------------------------------------------------------------------------
			/// Test Case	| Buyer		| Supplier	| Customer	| Manufacturer	| BookingParty	|
			/// ---------------------------------------------------------------------------------
			/// 1			| ORG_BUY	| ORG_SUP	| ORG_CC	| ORG_MAN		| ORG_SUP		|	(all parties different, supplier is booking party)
			/// 1			| ORG_BUY	| ORG_SUP	| ORG_CC	| ORG_MAN		| ORG_MAN		|	(all parties different, man is booking party)
			/// 3			| ORG_BUY	| ORG_MISC	| ORG_CC	| ORG_MISC		| ORG_MISC		|	(supplier + manufacuter are same)
			/// 4			| ORG_MISC	| ORG_MISC	| ORG_MISC	| ORG_MISC		| ORG_MISC		|	(all parties same, very rare edge case)
			///
			///	  Test Case 5 is two supplier booking lines attached to the same supplier booking
			/// 5a			| ORG_BUY	| ORG_SUP	| ORG_CC	| ORG_MAN		| ORG_SUP		|	(two orders/order lines, with separate buyers, one booking)
			/// 5b			| ORG_MISC	| ORG_SUP	| ORG_CC	| ORG_MAN		| ORG_SUP		|	(two orders/order lines, with separate buyers, one booking)

			pkName.Clear();
			pkName[Guid.Empty] = "<empty>";
			SetUpOrganisations();

			var order1PK = TestDataCreator.CreateJobOrderHeader("Order1", buyerAddressPK, supplierAddress: supplierAddressPK);
			var order2PK = TestDataCreator.CreateJobOrderHeader("Order2", buyerAddressPK, supplierAddress: supplierAddressPK);
			var order3PK = TestDataCreator.CreateJobOrderHeader("Order3", buyerAddressPK, supplierAddress: miscOrgAddressPK);
			var order4PK = TestDataCreator.CreateJobOrderHeader("Order4", miscOrgAddressPK, supplierAddress: miscOrgAddressPK);
			var order5aPK = TestDataCreator.CreateJobOrderHeader("Order5a", buyerAddressPK, supplierAddress: supplierAddressPK);
			var order5bPK = TestDataCreator.CreateJobOrderHeader("Order5b", miscOrgAddressPK, supplierAddress: supplierAddressPK);
			var order6PK = TestDataCreator.CreateJobOrderHeader("Order6", miscOrgAddressPK, supplierAddress: miscOrgAddressPK);

			var orderLine1PK = TestDataCreator.CreateJobOrderLine(order1PK, 1);
			var orderLine2PK = TestDataCreator.CreateJobOrderLine(order2PK, 2);
			var orderLine3PK = TestDataCreator.CreateJobOrderLine(order3PK, 3);
			var orderLine4PK = TestDataCreator.CreateJobOrderLine(order4PK, 4);
			var orderLine5aPK = TestDataCreator.CreateJobOrderLine(order5aPK, 5);
			var orderLine5bPK = TestDataCreator.CreateJobOrderLine(order5bPK, 6);
			var orderLine6PK = TestDataCreator.CreateJobOrderLine(order6PK, 7);

			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", order1PK, "JD", "MAN");
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", order2PK, "JD", "MAN");
			TestDataCreator.CreateDocAddress(miscOrgAddressPK, "", order3PK, "JD", "MAN");
			TestDataCreator.CreateDocAddress(miscOrgAddressPK, "", order4PK, "JD", "MAN");
			TestDataCreator.CreateDocAddress(miscOrgAddressPK, "", order4PK, "JD", "MAN", 1);
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", order5aPK, "JD", "MAN");
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", order5bPK, "JD", "MAN");
			TestDataCreator.CreateDocAddress(miscOrgAddressPK, "", order6PK, "JD", "MAN");
			TestDataCreator.CreateDocAddress(miscOrgAddressPK, "", order6PK, "JD", "MAN", 1);

			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", order1PK, "JD", "SCP");
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", order2PK, "JD", "SCP");
			TestDataCreator.CreateDocAddress(miscOrgAddressPK, "", order3PK, "JD", "SCP");
			TestDataCreator.CreateDocAddress(miscOrgAddressPK, "", order4PK, "JD", "SCP");
			TestDataCreator.CreateDocAddress(miscOrgAddressPK, "", order4PK, "JD", "SCP", 1);
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", order5aPK, "JD", "SCP");
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", order5bPK, "JD", "SCP");
			TestDataCreator.CreateDocAddress(miscOrgAddressPK, "", order6PK, "JD", "SCP");
			TestDataCreator.CreateDocAddress(miscOrgAddressPK, "", order6PK, "JD", "SCP", 1);

			const string Cancelled = "CAN";
			const string Incomplete = "INC";
			const string Placed = "PLC";
			const string Planned = "PLN";
			const string Rejected = "REJ";
			const string Converted = "CNV";

			var bookingHeader1PK = TestDataCreator.CreateJobSupplierBooking("SB01", supplierPK, status: Incomplete);
			var bookingHeader2PK = TestDataCreator.CreateJobSupplierBooking("SB02", manufacturerPK, status: Cancelled);
			var bookingHeader3PK = TestDataCreator.CreateJobSupplierBooking("SB03", miscOrgPK, status: Placed);
			var bookingHeader4PK = TestDataCreator.CreateJobSupplierBooking("SB04", miscOrgPK, status: Planned);
			var bookingHeader5PK = TestDataCreator.CreateJobSupplierBooking("SB05", supplierPK, status: Rejected);
			var bookingHeader6PK = TestDataCreator.CreateJobSupplierBooking("SB06", miscOrgPK, status: Converted);

			var bookingLine1PK = TestDataCreator.CreateJobSupplierBookingLine(orderLine1PK, bookingHeader1PK, "JSL0010");
			var bookingLine2PK = TestDataCreator.CreateJobSupplierBookingLine(orderLine2PK, bookingHeader2PK, "JSL0020");
			var bookingLine3PK = TestDataCreator.CreateJobSupplierBookingLine(orderLine3PK, bookingHeader3PK, "JSL0030");
			var bookingLine4PK = TestDataCreator.CreateJobSupplierBookingLine(orderLine4PK, bookingHeader4PK, "JSL0040");
			var bookingLine5aPK = TestDataCreator.CreateJobSupplierBookingLine(orderLine5aPK, bookingHeader5PK, "JSL0051");
			var bookingLine5bPK = TestDataCreator.CreateJobSupplierBookingLine(orderLine5bPK, bookingHeader5PK, "JSL0052");
			var bookingLine6PK = TestDataCreator.CreateJobSupplierBookingLine(orderLine6PK, bookingHeader6PK, "JSL0060");

			TestDataCreator.CreateDocAddress(supplierAddressPK, "", bookingHeader1PK, "JSB", "SUD");
			TestDataCreator.CreateDocAddress(supplierAddressPK, "", bookingHeader2PK, "JSB", "SUD");
			TestDataCreator.CreateDocAddress(miscOrgAddressPK, "", bookingHeader3PK, "JSB", "SUD");
			TestDataCreator.CreateDocAddress(miscOrgAddressPK, "", bookingHeader4PK, "JSB", "SUD");
			TestDataCreator.CreateDocAddress(miscOrgAddressPK, "", bookingHeader4PK, "JSB", "SUD", 1);
			TestDataCreator.CreateDocAddress(supplierAddressPK, "", bookingHeader5PK, "JSB", "SUD");
			TestDataCreator.CreateDocAddress(miscOrgAddressPK, "", bookingHeader6PK, "JSB", "SUD");
			TestDataCreator.CreateDocAddress(miscOrgAddressPK, "", bookingHeader6PK, "JSB", "SUD", 1);

			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", bookingHeader1PK, "JSB", "SCP");
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", bookingHeader2PK, "JSB", "SCP");
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", bookingHeader3PK, "JSB", "SCP");
			TestDataCreator.CreateDocAddress(miscOrgAddressPK, "", bookingHeader4PK, "JSB", "SCP");
			TestDataCreator.CreateDocAddress(miscOrgAddressPK, "", bookingHeader4PK, "JSB", "SCP", 1);
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", bookingHeader5PK, "JSB", "SCP");
			TestDataCreator.CreateDocAddress(miscOrgAddressPK, "", bookingHeader6PK, "JSB", "SCP");
			TestDataCreator.CreateDocAddress(miscOrgAddressPK, "", bookingHeader6PK, "JSB", "SCP", 1);

			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", bookingLine1PK, "JSL", "MAN");
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", bookingLine2PK, "JSL", "MAN");
			TestDataCreator.CreateDocAddress(miscOrgAddressPK, "", bookingLine3PK, "JSL", "MAN");
			TestDataCreator.CreateDocAddress(miscOrgAddressPK, "", bookingLine4PK, "JSL", "MAN");
			TestDataCreator.CreateDocAddress(miscOrgAddressPK, "", bookingLine4PK, "JSL", "MAN", 1);
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", bookingLine5aPK, "JSL", "MAN");
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", bookingLine5bPK, "JSL", "MAN");
			TestDataCreator.CreateDocAddress(miscOrgAddressPK, "", bookingLine6PK, "JSL", "MAN");
			TestDataCreator.CreateDocAddress(miscOrgAddressPK, "", bookingLine6PK, "JSL", "MAN", 1);

			pkName[order1PK] = nameof(order1PK);
			pkName[order2PK] = nameof(order2PK);
			pkName[order3PK] = nameof(order3PK);
			pkName[order4PK] = nameof(order4PK);
			pkName[order5aPK] = nameof(order5aPK);
			pkName[order5bPK] = nameof(order5bPK);
			pkName[order6PK] = nameof(order6PK);
			pkName[orderLine1PK] = nameof(orderLine1PK);
			pkName[orderLine2PK] = nameof(orderLine2PK);
			pkName[orderLine3PK] = nameof(orderLine3PK);
			pkName[orderLine4PK] = nameof(orderLine4PK);
			pkName[orderLine5aPK] = nameof(orderLine5aPK);
			pkName[orderLine5bPK] = nameof(orderLine5bPK);
			pkName[orderLine6PK] = nameof(orderLine6PK);
			pkName[bookingLine1PK] = nameof(bookingLine1PK);
			pkName[bookingLine2PK] = nameof(bookingLine2PK);
			pkName[bookingLine3PK] = nameof(bookingLine3PK);
			pkName[bookingLine4PK] = nameof(bookingLine4PK);
			pkName[bookingLine5aPK] = nameof(bookingLine5aPK);
			pkName[bookingLine5bPK] = nameof(bookingLine5bPK);
			pkName[bookingLine6PK] = nameof(bookingLine6PK);
			pkName[bookingHeader1PK] = nameof(bookingHeader1PK);
			pkName[bookingHeader2PK] = nameof(bookingHeader2PK);
			pkName[bookingHeader3PK] = nameof(bookingHeader3PK);
			pkName[bookingHeader4PK] = nameof(bookingHeader4PK);
			pkName[bookingHeader5PK] = nameof(bookingHeader5PK);
			pkName[bookingHeader6PK] = nameof(bookingHeader6PK);
		}

		protected void SetUpBookingWithOneLine(string bookingStatus)
		{
			/// ---------------------------------------------------------------------------------
			/// Test Case	| Buyer		| Supplier	| Customer	| Manufacturer	| BookingParty	|
			/// ---------------------------------------------------------------------------------
			/// 1			| ORG_BUY	| ORG_SUP	| ORG_CC	| ORG_MAN		| ORG_SUP		|	(all parties different, supplier is booking party)

			SetUpOrganisations();
			var orderPK = TestDataCreator.CreateJobOrderHeader($"TestOrder-{bookingStatus}", buyerAddressPK, supplierAddress: supplierAddressPK);
			var orderLinePK = TestDataCreator.CreateJobOrderLine(orderPK, 1);
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", orderPK, "JD", "MAN");
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", orderPK, "JD", "SCP");
			var bookingHeaderPK = TestDataCreator.CreateJobSupplierBooking($"TestBooking-{bookingStatus}", supplierPK, bookingStatus);
			var bookingLinePK = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, bookingHeaderPK, "JSL001");
			TestDataCreator.CreateDocAddress(supplierAddressPK, "", bookingHeaderPK, "JSB", "SUD");
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", bookingHeaderPK, "JSB", "SCP");
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", bookingLinePK, "JSL", "MAN");
		}

		protected void SetUpOrganisations()
		{
			if (!hasSetUpOrgs)
			{
				buyerPK = TestDataCreator.CreateOrganisation("ORG_BUY", "Buyer");
				supplierPK = TestDataCreator.CreateOrganisation("ORG_SUP", "Supplier");
				manufacturerPK = TestDataCreator.CreateOrganisation("ORG_MAN", "Manufacturer");
				controllingCustomerPK = TestDataCreator.CreateOrganisation("ORG_CC", "Controlling Customer");
				miscOrgPK = TestDataCreator.CreateOrganisation("ORG_MISC", "Misc Org");

				pkName[buyerPK] = nameof(buyerPK);
				pkName[supplierPK] = nameof(supplierPK);
				pkName[manufacturerPK] = nameof(manufacturerPK);
				pkName[controllingCustomerPK] = nameof(controllingCustomerPK);
				pkName[miscOrgPK] = nameof(miscOrgPK);

				buyerAddressPK = TestDataCreator.CreateAddress(buyerPK, "ORG_BUY", "1 buyer st");
				supplierAddressPK = TestDataCreator.CreateAddress(supplierPK, "ORG_SUP", "1 supplier st");
				manufacturerAddressPK = TestDataCreator.CreateAddress(manufacturerPK, "ORG_MAN", "1 manufacturer st");
				controllingCustomerAddressPK = TestDataCreator.CreateAddress(controllingCustomerPK, "ORG_CC", "1 controllingCustomer st");
				miscOrgAddressPK = TestDataCreator.CreateAddress(miscOrgPK, "ORG_MISC", "1 miscOrg st");

				pkName[buyerAddressPK] = nameof(buyerAddressPK);
				pkName[supplierAddressPK] = nameof(supplierAddressPK);
				pkName[manufacturerAddressPK] = nameof(manufacturerAddressPK);
				pkName[controllingCustomerAddressPK] = nameof(controllingCustomerAddressPK);
				pkName[miscOrgAddressPK] = nameof(miscOrgAddressPK);
				hasSetUpOrgs = true;
			}
		}

		protected IEnumerable<string> Statuses
		{
			get
			{
				yield return "APP";
				yield return "CAN";
				yield return "INC";
				yield return "PLC";
				yield return "PLN";
				yield return "REJ";
				yield return "CNV";
			}
		}

		protected IEnumerable<string> EditableStatuses
		{
			get
			{
				yield return "INC";
				yield return "REJ";
			}
		}

		protected IEnumerable<string> EditableStatus
		{
			get
			{
				yield return "PLC";
			}
		}

		protected void AssertTVFRow(string testMessage, DataRow row, bool isBuyer, bool isSupplier, bool isManufacturer, bool isControllingCustomer, bool isBookingParty)
		{
			CombineAssertions(testMessage, () =>
			{
				AssertEquals("IsBuyer", isBuyer, row["IsBuyer"]);
				AssertEquals("IsSupplier", isSupplier, row["IsSupplier"]);
				AssertEquals("IsManufacturer", isManufacturer, row["IsManufacturer"]);
				AssertEquals("IsControllingCustomer", isControllingCustomer, row["IsControllingCustomer"]);
				AssertEquals("IsBookingParty", isBookingParty, row["IsBookingParty"]);
			});
		}

		protected void AssertTVFRow(string testMessage, DataRow row, bool canEdit)
		{
			AssertEquals($"{testMessage}: CanEdit", canEdit, row["CanEdit"]);
		}

		protected void ClearBookingsAndLines()
		{
			var sql = "DELETE FROM dbo.JobSupplierBookingLine; DELETE FROM dbo.JobSupplierBooking;";

			using (var command = Db.Connection.Command(sql))
			{
				command.ExecuteNonQuery();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			hasSetUpOrgs = false;
		}

		protected Dictionary<Guid, string> pkName = new Dictionary<Guid, string>(32);
		protected Guid buyerPK;
		protected Guid supplierPK;
		protected Guid manufacturerPK;
		protected Guid controllingCustomerPK;
		protected Guid miscOrgPK;
		protected Guid buyerAddressPK;
		protected Guid supplierAddressPK;
		protected Guid manufacturerAddressPK;
		protected Guid controllingCustomerAddressPK;
		protected Guid miscOrgAddressPK;

		bool hasSetUpOrgs;
	}
}
