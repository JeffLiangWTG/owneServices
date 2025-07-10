using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Orders;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Orders.Testing
{
	[TestedType(typeof(GetJobOrderLinesWithSecurityContext))]
	class GetJobOrderLinesWithSecurityContextTest : DbCreateScriptTest
	{
		public void TestReturnsJobOrderLineColumns()
		{
			var fromJobOrderLine = GetColumnNames("SELECT * FROM dbo.JobOrderLine");
			var fromTvf = GetColumnNames("SELECT * FROM GetJobOrderLinesWithSecurityContext(NEWID())");

			var missingColumns = fromJobOrderLine.Except(fromTvf);
			var errorMessage = $"The following columns are missing and should be added to the function: {string.Join(", ", missingColumns)}";

			Assert(errorMessage, !missingColumns.Any());
		}

		public void TestGetJobOrderLines_UnrelatedOrganisation_ShouldReturnEmptyResults()
		{
			SetupTestData();

			var results = ExecuteAs(Guid.NewGuid()).Rows;
			AssertEquals("Unrelated organisation sees no lines", 0, results.Count);
		}

		// --------------------------------------------------------------------------------------------------------------------
		// Line  ||    Buyer    ||    Supplier    ||    Controlling Customer    ||    Manufacturer    ||  	Manufacturer (Line)
		// --------------------------------------------------------------------------------------------------------------------
		// 1		   TESTSYD        SUPPLISYD         CTRLCUSYD                     MANUFASYD             null
		// 2		   BUYERSYD       TESTSYD           CTRLCUSYD                     MANUFASYD             null
		// 3		   BUYERSYD       SUPPLISYD         TESTSYD                       MANUFASYD             null
		// 4		   BUYERSYD       SUPPLISYD         CTRLCUSYD                     TESTSYD               null
		// 5		   BUYERSYD       SUPPLISYD         null                          MANUFASYD             TESTSYD

		public void TestGetJobOrderLines()
		{
			SetupTestData();

			var results = ExecuteAs(testTargetPK).Rows;
			AssertEquals("TESTSYD - Related to ALL 5 lines, should see all:", 5, results.Count);
			AssertTVPRow("TESTSYD - Line 1", results[0], isBuyer: true, isSupplier: false, isControllingCustomer: false);
			AssertTVPRow("TESTSYD - Line 2", results[1], isBuyer: false, isSupplier: true, isControllingCustomer: false);
			AssertTVPRow("TESTSYD - Line 3", results[2], isBuyer: false, isSupplier: false, isControllingCustomer: true);
			AssertTVPRow("TESTSYD - Line 4", results[3], isBuyer: false, isSupplier: false, isControllingCustomer: false);
			AssertTVPRow("TESTSYD - Line 5", results[4], isBuyer: false, isSupplier: false, isControllingCustomer: false);

			results = ExecuteAs(buyerPK).Rows;
			AssertEquals("BUYERSYD - Should only see subset they are related to", 4, results.Count);
			AssertTVPRow("BUYERSYD - Line 2", results[0], isBuyer: true, isSupplier: false, isControllingCustomer: false);
			AssertTVPRow("BUYERSYD - Line 3", results[1], isBuyer: true, isSupplier: false, isControllingCustomer: false);
			AssertTVPRow("BUYERSYD - Line 4", results[2], isBuyer: true, isSupplier: false, isControllingCustomer: false);
			AssertTVPRow("BUYERSYD - Line 5", results[3], isBuyer: true, isSupplier: false, isControllingCustomer: false);

			results = ExecuteAs(supplierPK).Rows;
			AssertEquals("SUPPLISYD - Should only see subset they are related to", 4, results.Count);
			AssertTVPRow("SUPPLISYD - Line 1", results[0], isBuyer: false, isSupplier: true, isControllingCustomer: false);
			AssertTVPRow("SUPPLISYD - Line 3", results[1], isBuyer: false, isSupplier: true, isControllingCustomer: false);
			AssertTVPRow("SUPPLISYD - Line 4", results[2], isBuyer: false, isSupplier: true, isControllingCustomer: false);
			AssertTVPRow("SUPPLISYD - Line 5", results[3], isBuyer: false, isSupplier: true, isControllingCustomer: false);

			results = ExecuteAs(controllingCustomerPK).Rows;
			AssertEquals("CTRLCUSYD - Should only see subset they are related to", 3, results.Count);
			AssertTVPRow("CTRLCUSYD - Line 1", results[0], isBuyer: false, isSupplier: false, isControllingCustomer: true);
			AssertTVPRow("CTRLCUSYD - Line 2", results[1], isBuyer: false, isSupplier: false, isControllingCustomer: true);
			AssertTVPRow("CTRLCUSYD - Line 4", results[2], isBuyer: false, isSupplier: false, isControllingCustomer: true);

			results = ExecuteAs(manufacturerPK).Rows;
			AssertEquals("MANUFASYD - Should only see subset they are related to", 3, results.Count);
			AssertTVPRow("MANUFASYD - Line 1", results[0], isBuyer: false, isSupplier: false, isControllingCustomer: false);
			AssertTVPRow("MANUFASYD - Line 2", results[1], isBuyer: false, isSupplier: false, isControllingCustomer: false);
			AssertTVPRow("MANUFASYD - Line 3", results[2], isBuyer: false, isSupplier: false, isControllingCustomer: false);
		}

		public void TestGetJobOrderLines_BuyerSeesAllRelatedOrderLines()
		{
			SetupOrganisations();
			AssertShouldSeeAllRelatedOrders("Buyer", buyerPK);
		}

		public void TestGetJobOrderLines_ControllingCustomerSeesAllRelatedOrderLines()
		{
			SetupOrganisations();
			AssertShouldSeeAllRelatedOrders("Controlling Customer", controllingCustomerPK);
		}

		public void TestGetJobOrderLines_SupplierCanOnlySeeActiveAndReleasedOrderLines()
		{
			SetupOrganisations();
			AssertShouldOnlySeeActiveAndReleasedOrders("Supplier", supplierPK);
		}

		public void TestGetJobOrderLines_ManufacturerCanOnlySeeActiveAndReleasedOrderLines()
		{
			SetupOrganisations();
			AssertShouldOnlySeeActiveAndReleasedOrders("Manufacturer", manufacturerPK);
		}

		public void TestGetJobOrderLines_ManufacturerForLineCanOnlySeeActiveAndReleasedOrderLines()
		{
			SetupOrganisations();

			foreach (var status in OrderStatuses)
			{
				foreach (var isReleased in new[] { true, false })
				{
					foreach (var isCancelled in new[] { true, false })
					{
						var order = TestDataCreator.CreateJobOrderHeader(
							$"Order-{status}-{isReleased}-{isCancelled}",
							buyerAddressPK,
							supplierAddress: supplierAddressPK,
							orderStatus: status,
							isReleased: isReleased,
							isCancelled: isCancelled
						);
						var orderLine = TestDataCreator.CreateJobOrderLine(order, 1);
						TestDataCreator.CreateDocAddress(testTargetAddressPK, "", orderLine, "JO", "MAN");
					}
				}
			}

			var results = ExecuteAs(testTargetPK);
			AssertEquals("Manufacturer for line should see all order lines where the relevant order is active and released, regardless of status", OrderStatuses.Length, results.Rows.Count);
			Assert("Should not contain unreleased orders", !results.Select().Select(x => x["JD_IsReleased"]).Contains(false));
			Assert("Should not contain inactive orders", !results.Select().Select(x => x["JD_IsCancelled"]).Contains(true));
			AssertCanEdit("Manufacturer for line", results, isBuyerOrCC: false);
		}

		public void TestGetJobOrderLines_NoDuplicateRows()
		{
			SetupOrganisations();

			var order = TestDataCreator.CreateJobOrderHeader("Order 1", buyerAddressPK, supplierAddress: supplierAddressPK, isReleased: true, isCancelled: false);
			var orderLine = TestDataCreator.CreateJobOrderLine(order, 1);

			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", order, "JD", "SCP", 0);
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", order, "JD", "SCP", 1);

			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", order, "JD", "MAN", 0);
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", order, "JD", "MAN", 1);

			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", orderLine, "JO", "MAN", 0);
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", orderLine, "JO", "MAN", 1);

			var jobOrderLines = ExecuteAs(buyerPK).Select();

			AssertEquals("Should only return 1 order line with context attached", 1, jobOrderLines.Length);
		}

		#region Implementation

		Guid testTargetPK;
		Guid buyerPK;
		Guid supplierPK;
		Guid manufacturerPK;
		Guid controllingCustomerPK;

		Guid testTargetAddressPK;
		Guid buyerAddressPK;
		Guid supplierAddressPK;
		Guid manufacturerAddressPK;
		Guid controllingCustomerAddressPK;

		protected override void SetUp()
		{
			base.SetUp();
			ClearTable(JobOrderLineSchema.Constants.TableName);
		}

		void AssertShouldSeeAllRelatedOrders(string message, Guid organisationPK)
		{
			foreach (var status in OrderStatuses)
			{
				foreach (var isReleased in new[] { true, false })
				{
					foreach (var isCancelled in new[] { true, false })
					{
						SetupOrderWithOneLine(
							$"Order-{status}-{isReleased}-{isCancelled}",
							buyerAddressPK,
							supplierAddressPK,
							controllingCustomerAddressPK,
							manufacturerAddressPK,
							status,
							isReleased,
							isCancelled);
					}
				}
			}

			var results = ExecuteAs(organisationPK);
			AssertEquals($"{message} should see all orders lines, regardless of the parent order status or released/active state", OrderStatuses.Length * 2 * 2, results.Rows.Count);
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
						SetupOrderWithOneLine(
							$"Order-{status}-{isReleased}-{isCancelled}",
							buyerAddressPK,
							supplierAddressPK,
							controllingCustomerAddressPK,
							manufacturerAddressPK,
							status,
							isReleased,
							isCancelled);
					}
				}
			}

			var results = ExecuteAs(organisationPK);
			AssertEquals($"{message} should see all order lines where the relevant order is active and released, regardless of status", OrderStatuses.Length, results.Rows.Count);
			Assert("Should not contain unreleased orders", !results.Select().Select(x => x["JD_IsReleased"]).Contains(false));
			Assert("Should not contain inactive orders", !results.Select().Select(x => x["JD_IsCancelled"]).Contains(true));
			AssertCanEdit(message, results, isBuyerOrCC: false);
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

		void SetupOrganisations()
		{
			testTargetPK = TestDataCreator.CreateOrganisation("TESTSYD", "Test Organisation");
			buyerPK = TestDataCreator.CreateOrganisation("BUYERSYD", "Buyer");
			supplierPK = TestDataCreator.CreateOrganisation("SUPPLISYD", "Supplier");
			manufacturerPK = TestDataCreator.CreateOrganisation("MANUFASYD", "Manufacturer");
			controllingCustomerPK = TestDataCreator.CreateOrganisation("CTRLCUSYD", "Controlling Customer");

			testTargetAddressPK = TestDataCreator.CreateAddress(testTargetPK, "TESTSYD", "1 test st");
			buyerAddressPK = TestDataCreator.CreateAddress(buyerPK, "BUYERSYD", "1 buyer st");
			supplierAddressPK = TestDataCreator.CreateAddress(supplierPK, "SUPPLISYD", "1 supplier st");
			manufacturerAddressPK = TestDataCreator.CreateAddress(manufacturerPK, "MANUFASYD", "1 manufacturer st");
			controllingCustomerAddressPK = TestDataCreator.CreateAddress(controllingCustomerPK, "CTRLCUSYD", "1 controllingCustomer st");
		}

		void SetupTestData()
		{
			SetupOrganisations();

			SetupOrderWithOneLine("Order 1", testTargetAddressPK, supplierAddressPK, controllingCustomerAddressPK, manufacturerAddressPK, isReleased: true, isCancelled: false);
			SetupOrderWithOneLine("Order 2", buyerAddressPK, testTargetAddressPK, controllingCustomerAddressPK, manufacturerAddressPK, isReleased: true, isCancelled: false);
			SetupOrderWithOneLine("Order 3", buyerAddressPK, supplierAddressPK, testTargetAddressPK, manufacturerAddressPK, isReleased: true, isCancelled: false);
			SetupOrderWithOneLine("Order 4", buyerAddressPK, supplierAddressPK, controllingCustomerAddressPK, testTargetAddressPK, isReleased: true, isCancelled: false);

			var order5PK = TestDataCreator.CreateJobOrderHeader("Order 5", buyerAddressPK, supplierAddress: supplierAddressPK, isReleased: true, isCancelled: false);
			var order5Line1PK = TestDataCreator.CreateJobOrderLine(order5PK, 1);
			TestDataCreator.CreateDocAddress(testTargetAddressPK, "", order5Line1PK, "JO", "MAN");
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", order5PK, "JD", "MAN");
		}

		Guid SetupOrderWithOneLine(
			string orderNum,
			Guid buyerAddress,
			Guid supplierAddress,
			Guid controllingCustomAddress,
			Guid manufacturerAddress,
			string orderStatus = "PLC",
			bool isReleased = false,
			bool isCancelled = false
		)
		{
			var orderPK = TestDataCreator.CreateJobOrderHeader(
				orderNum,
				buyerAddress,
				supplierAddress: supplierAddress,
				orderStatus: orderStatus,
				isReleased: isReleased,
				isCancelled: isCancelled
			);
			TestDataCreator.CreateDocAddress(manufacturerAddress, "", orderPK, "JD", "MAN");
			TestDataCreator.CreateDocAddress(controllingCustomAddress, "", orderPK, "JD", "SCP");
			TestDataCreator.CreateJobOrderLine(orderPK, 1);

			return orderPK;
		}

		static void ClearTable(string tableName)
		{
			var query = string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0}", tableName);
			using var command = Db.Connection.Command(query);
			command.ExecuteNonQuery();
		}

		DataTable ExecuteAs(Guid loggedInContactOrganisation)
		{
			using var command = TestConnection.Command("SELECT * FROM GetJobOrderLinesWithSecurityContext(@loggedInContactOrganisation) JOIN dbo.JobOrderHeader ON JO_JD = JD_PK");
			command.AddParameter("@loggedInContactOrganisation", SqlDbType.UniqueIdentifier, loggedInContactOrganisation);

			return DataUtils.GetDataTableFromCommand(command);
		}

		IEnumerable<string> GetColumnNames(string commandText)
		{
			using var command = TestConnection.Command(commandText);
			using var reader = command.ExecuteReader(CommandBehavior.SchemaOnly);
			return reader.GetSchemaTable()
				.Select()
				.Select(x => (string)x["ColumnName"]);
		}

		protected void AssertTVPRow(string testMessage, DataRow row, bool isBuyer, bool isSupplier, bool isControllingCustomer)
		{
			CombineAssertions(testMessage, () =>
			{
				AssertEquals("IsBuyer", isBuyer, row["IsBuyer"]);
				AssertEquals("IsSupplier", isSupplier, row["IsSupplier"]);
				AssertEquals("IsControllingCustomer", isControllingCustomer, row["IsControllingCustomer"]);
			});
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

		#endregion
	}
}
