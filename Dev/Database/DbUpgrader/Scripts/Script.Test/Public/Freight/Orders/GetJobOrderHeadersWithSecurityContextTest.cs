using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Orders;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Orders.Testing
{
	[TestedType(typeof(GetJobOrderHeadersWithSecurityContext))]
	class GetJobOrderHeadersWithSecurityContextTest : DbCreateScriptTest
	{
		public void TestReturnsJobOrderHeaderColumns()
		{
			var fromJobOrderHeader = GetColumnNames("SELECT * FROM dbo.JobOrderHeader");
			var fromTvf = GetColumnNames("SELECT * FROM GetJobOrderHeadersWithSecurityContext(NEWID())");

			var missingColumns = fromJobOrderHeader.Except(fromTvf);
			var errorMessage = $"The following columns are missing and should be added to the function: {string.Join(", ", missingColumns)}";

			Assert(errorMessage, !missingColumns.Any());
		}

		public void TestGetJobOrderHeaders_BuyerSeesAllRelatedOrders()
		{
			SetupOrganisations();
			AssertShouldSeeAllRelatedOrders("Buyer", buyerPK);
		}

		public void TestGetJobOrderHeaders_ControllingCustomerSeesAllRelatedOrders()
		{
			SetupOrganisations();
			AssertShouldSeeAllRelatedOrders("Controlling Customer", controllingCustomerPK);
		}

		public void TestGetJobOrderHeaders_SupplierCanOnlySeeActiveAndReleasedOrders()
		{
			SetupOrganisations();
			AssertShouldOnlySeeActiveAndReleasedOrders("Supplier", supplierPK);
		}

		public void TestGetJobOrderHeaders_ManufacturerCanOnlySeeActiveAndReleasedOrders()
		{
			SetupOrganisations();
			AssertShouldOnlySeeActiveAndReleasedOrders("Manufacturer", manufacturerPK);
		}

		public void TestGetJobOrderHeaders_ManufacturerForLineCanOnlySeeActiveAndReleasedOrders()
		{
			SetupOrganisations();
			AssertShouldOnlySeeActiveAndReleasedOrders("Manufacturer For Line", manufacturerForLinePK);
		}

		public void TestGetJobOrderHeaders_UnrelatedShouldSeeNothing()
		{
			SetupOrganisations();
			AssertShouldSeeNothing("Unrelated Org", miscOrgAddressPK);
		}

		public void TestGetJobOrderHeaders_NoDuplicateRows()
		{
			SetupOrganisations();

			var order = TestDataCreator.CreateJobOrderHeader("ORD1", buyerAddressPK, supplierAddress: supplierAddressPK, isReleased: true, isCancelled: false);
			var orderLine = TestDataCreator.CreateJobOrderLine(order, 1);

			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", order, "JD", "SCP", 0);
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", order, "JD", "SCP", 1);

			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", order, "JD", "MAN", 0);
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", order, "JD", "MAN", 1);

			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", orderLine, "JO", "MAN", 0);
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", orderLine, "JO", "MAN", 1);

			var results = ExecuteAs(buyerPK).Select();
			AssertEquals("Should only return 1 order with context attached", 1, results.Length);
		}

		#region Implementation

		void SetupOrganisations()
		{
			miscOrgPK = TestDataCreator.CreateOrganisation("MISCSYD", "Misc Organisation");
			buyerPK = TestDataCreator.CreateOrganisation("BUYERSYD", "Buyer");
			supplierPK = TestDataCreator.CreateOrganisation("SUPPLISYD", "Supplier");
			manufacturerPK = TestDataCreator.CreateOrganisation("MANUFASYD", "Manufacturer");
			manufacturerForLinePK = TestDataCreator.CreateOrganisation("MALINESYD", "Manufacturer For Line");
			controllingCustomerPK = TestDataCreator.CreateOrganisation("CTRLCUSYD", "Controlling Customer");

			miscOrgAddressPK = TestDataCreator.CreateAddress(miscOrgPK, "MISCSYD", "1 misc st");
			buyerAddressPK = TestDataCreator.CreateAddress(buyerPK, "BUYERSYD", "1 buyer st");
			supplierAddressPK = TestDataCreator.CreateAddress(supplierPK, "SUPPLISYD", "1 supplier st");
			manufacturerAddressPK = TestDataCreator.CreateAddress(manufacturerPK, "MANUFASYD", "1 manufacturer st");
			manufacturerForLineAddressPK = TestDataCreator.CreateAddress(manufacturerForLinePK, "MALINESYD", "1 manufacturer for line st");
			controllingCustomerAddressPK = TestDataCreator.CreateAddress(controllingCustomerPK, "CTRLCUSYD", "1 controllingCustomer st");
		}

		Guid CreateOrderWithAddresses(string orderNum, string orderStatus, bool isReleased, bool isCancelled)
		{
			var orderPK = TestDataCreator.CreateJobOrderHeader(
				orderNum,
				buyerAddressPK,
				supplierAddress: supplierAddressPK,
				orderStatus: orderStatus,
				isReleased: isReleased,
				isCancelled: isCancelled
			);

			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", orderPK, "JD", "MAN");
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", orderPK, "JD", "SCP");

			var orderLinePK = TestDataCreator.CreateJobOrderLine(orderPK);
			TestDataCreator.CreateDocAddress(manufacturerForLineAddressPK, "", orderLinePK, "JO", "MAN");

			return orderPK;
		}

		void AssertShouldSeeAllRelatedOrders(string message, Guid organisationPK)
		{
			foreach (var status in OrderStatuses)
			{
				foreach (var isReleased in new[] { true, false })
				{
					foreach (var isCancelled in new[] { true, false })
					{
						CreateOrderWithAddresses($"Order-{status}-{isReleased}-{isCancelled}", status, isReleased, isCancelled);
					}
				}
			}

			var results = ExecuteAs(organisationPK);
			AssertEquals($"{message} should see all orders, regardless of status or released/active state", OrderStatuses.Length * 2 * 2, results.Rows.Count);
			AssertCanEdit(message, results, isBuyerOrCC: true);
		}

		void AssertShouldOnlySeeActiveAndReleasedOrders(string message, Guid organisationPK)
		{
			foreach (var status in OrderStatuses)
			{
				foreach (var isReleased in new[] { true, false })
				{
					foreach (var isCancelled in new[] { true, false })
					{
						CreateOrderWithAddresses($"Order-{status}-{isReleased}-{isCancelled}", status, isReleased, isCancelled);
					}
				}
			}

			var results = ExecuteAs(organisationPK);
			AssertEquals($"{message} should see all orders which are active and released, regardless of status", OrderStatuses.Length, results.Rows.Count);
			Assert("Should not contain unreleased orders", !results.Select().Select(x => x["JD_IsReleased"]).Contains(false));
			Assert("Should not contain inactive orders", !results.Select().Select(x => x["JD_IsCancelled"]).Contains(true));
			AssertCanEdit(message, results, isBuyerOrCC: false);
		}

		void AssertShouldSeeNothing(string message, Guid organisationPK)
		{
			foreach (var status in OrderStatuses)
			{
				foreach (var isReleased in new[] { true, false })
				{
					foreach (var isCancelled in new[] { true, false })
					{
						CreateOrderWithAddresses($"Order-{status}-{isReleased}-{isCancelled}", status, isReleased, isCancelled);
					}
				}
			}

			var results = ExecuteAs(organisationPK);
			AssertEquals(message, 0, results.Rows.Count);
		}

		protected void AssertCanEdit(string testMessage, DataTable results, bool isBuyerOrCC)
		{
			foreach (var row in results.Rows.OfType<DataRow>())
			{
				var status = row["JD_OrderStatus"].ToString();
				var isReleased = bool.Parse(row["JD_IsReleased"].ToString());
				var isCancelled = bool.Parse(row["JD_IsCancelled"].ToString());
				var message = $"{testMessage}: Status: {status}, IsReleased: {isReleased}, IsCancelled: {isCancelled}";

				AssertEquals(message, !isCancelled && isBuyerOrCC, row["CanEdit"]);
			}
		}

		DataTable ExecuteAs(Guid loggedInContactOrganisation)
		{
			using var command = TestConnection.Command("SELECT * FROM GetJobOrderHeadersWithSecurityContext(@loggedInContactOrganisation)");
			command.AddParameter("@loggedInContactOrganisation", SqlDbType.UniqueIdentifier, loggedInContactOrganisation);
			return DataUtils.GetDataTableFromCommand(command);
		}

		IEnumerable<string> GetColumnNames(string commandText)
		{
			using var command = TestConnection.Command(commandText);
			using var reader = command.ExecuteReader(CommandBehavior.SchemaOnly);
			return reader
				.GetSchemaTable()
				.Select()
				.Select(x => (string)x["ColumnName"]);
		}

		readonly string[] OrderStatuses =
		[
			"INC",
			"PLC",
			"CNF",
			"SHP",
			"PRT",
			"DLV",
			"CAN",
			"BKD"
		];

		Guid miscOrgPK;
		Guid buyerPK;
		Guid supplierPK;
		Guid manufacturerPK;
		Guid manufacturerForLinePK;
		Guid controllingCustomerPK;

		Guid miscOrgAddressPK;
		Guid buyerAddressPK;
		Guid supplierAddressPK;
		Guid manufacturerAddressPK;
		Guid manufacturerForLineAddressPK;
		Guid controllingCustomerAddressPK;

		#endregion
	}
}
