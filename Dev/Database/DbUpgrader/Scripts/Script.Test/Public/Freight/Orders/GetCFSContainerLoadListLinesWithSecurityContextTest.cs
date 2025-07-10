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
	[TestedType(typeof(GetCFSContainerLoadListLinesWithSecurityContext))]
	class GetCFSContainerLoadListLinesWithSecurityContextTest : DbCreateScriptTest
	{
		public void TestReturnsContainerLoadListLinesColumns()
		{
			var fromContainerLoadListLine = GetColumnNames("SELECT * FROM dbo.ContainerLoadListLine");
			var fromTvf = GetColumnNames("SELECT * FROM GetCFSContainerLoadListLinesWithSecurityContext(NEWID())");

			var missingColumns = fromContainerLoadListLine.Except(fromTvf.Append("CLL_LoadMode"));
			var errorMessage = $"The following columns are missing and should be added to the function: {string.Join(", ", missingColumns)}";

			Assert(errorMessage, !missingColumns.Any());
		}
		public void TestGetCFSContainerLoadListLines_BuyerVisibility()
		{
			SetupOrganisations();
			AssertShouldSeeStatuses("Buyer1", buyerPK1, ContainerLoadListStatuses.Count() - 1);
		}

		public void TestGetCFSContainerLoadListLines_ControllingCustomerVisibility()
		{
			SetupOrganisations();
			AssertShouldSeeStatuses("Controlling Customer", controllingCustomerPK, ContainerLoadListStatuses.Count() * 2);
		}

		public void TestGetCFSContainerLoadListLines_SupplierVisibility()
		{
			SetupOrganisations();
			AssertShouldSeeStatuses("Supplier1", supplierPK1, 0);
		}

		public void TestGetCFSContainerLoadListLines_ManufacturerVisibility_AsLoadListParty()
		{
			SetupOrganisations();
			AssertShouldSeeStatuses("Manufacturer (+Load List Party)", manufacturerPK, 0, isLoadListParty: true);
		}

		public void TestGetCFSContainerLoadListLines_ManufacturerVisibility_NotLoadListParty()
		{
			SetupOrganisations();
			AssertShouldSeeStatuses("Manufacturer (only)", manufacturerPK, 0);
		}

		public void TestGetCFSContainerLoadListLines_UnrelatedOrg()
		{
			SetupOrganisations();
			AssertShouldSeeStatuses("Unrelated Org", Guid.NewGuid(), 0);
		}

		public void TestGetCFSContainerLoadListLines_DoesNotReturnCYRecords()
		{
			SetupOrganisations();

			var refContainer = TestDataCreator.CreateRefContainer("RC");
			var containerPK1 = TestDataCreator.CreateJobContainer("CONTAINER1", refContainer);
			var containerPK2 = TestDataCreator.CreateJobContainer("CONTAINER2", refContainer);

			var orderPK = TestDataCreator.CreateJobOrderHeader("ORDER", buyerAddressPK1, supplierAddress: supplierAddressPK1, orderStatus: "PLC");
			var orderLinePK = TestDataCreator.CreateJobOrderLine(orderPK);

			var supplierBookingPK = TestDataCreator.CreateJobSupplierBooking("SBK01", supplierPK1, "PLN");
			var supplierBookingLinePK1 = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, supplierBookingPK, "JSL001");
			var supplierBookingLinePK2 = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, supplierBookingPK, "JSL002");

			var cyContainerLoadListPK = TestDataCreator.CreateCYContainerLoadList("CLH01", supplierBookingPK, supplierPK1, "PLC");
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", cyContainerLoadListPK, "CLH", "SCP", 0);
			TestDataCreator.CreateCYContainerLoadListLine(cyContainerLoadListPK, supplierBookingLinePK1, containerPK1);

			var cfsContainerLoadListPK1 = TestDataCreator.CreateCFSContainerLoadList("CLH02", supplierBookingPK, supplierPK1, "PLC");
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", cfsContainerLoadListPK1, "CLH", "SCP", 0);
			TestDataCreator.CreateCFSContainerLoadListLine(cfsContainerLoadListPK1, supplierBookingLinePK2, containerPK2, null);

			AssertEquals("It should only return CFS container load lists, not CY container load lists.", 1, ExecuteAs(controllingCustomerPK).Select().Length);
		}

		public void TestGetCFSContainerLoadListLines_ReturnOwnRecords()
		{
			SetupOrganisations();

			var controllingCustomerPK2 = TestDataCreator.CreateOrganisation("CTRLCUSYD2", "Controlling Customer2");
			var controllingCustomerAddressPK2 = TestDataCreator.CreateAddress(controllingCustomerPK2, "CTRLCUSYD2", "2 controllingCustomer st");

			var refContainer = TestDataCreator.CreateRefContainer("RC");
			var containerPK1 = TestDataCreator.CreateJobContainer("CONTAINER1", refContainer);
			var containerPK2 = TestDataCreator.CreateJobContainer("CONTAINER2", refContainer);

			var orderPK = TestDataCreator.CreateJobOrderHeader("ORDER", buyerAddressPK1, supplierAddress: supplierAddressPK1, orderStatus: "PLC");
			var orderLinePK = TestDataCreator.CreateJobOrderLine(orderPK);

			var supplierBookingPK = TestDataCreator.CreateJobSupplierBooking("SBK01", supplierPK1, "PLN");
			var supplierBookingLinePK1 = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, supplierBookingPK, "JSL001");
			var supplierBookingLinePK2 = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, supplierBookingPK, "JSL002");

			var cfsContainerLoadListPK1 = TestDataCreator.CreateCFSContainerLoadList("CLH01", supplierBookingPK, supplierPK1, "PLC");
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", cfsContainerLoadListPK1, "CLH", "SCP", 0);
			TestDataCreator.CreateCFSContainerLoadListLine(cfsContainerLoadListPK1, supplierBookingLinePK1, containerPK1, null);

			var cfsContainerLoadListPK2 = TestDataCreator.CreateCFSContainerLoadList("CLH02", supplierBookingPK, supplierPK1, "PLC");
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK2, "", cfsContainerLoadListPK2, "CLH", "SCP", 0);
			TestDataCreator.CreateCFSContainerLoadListLine(cfsContainerLoadListPK2, supplierBookingLinePK2, containerPK2, null);

			AssertEquals(1, ExecuteAs(controllingCustomerPK).Select().Length);
			AssertEquals(1, ExecuteAs(controllingCustomerPK2).Select().Length);
		}

		#region Implementation

		void SetupOrganisations()
		{
			buyerPK1 = TestDataCreator.CreateOrganisation("BUYERSYD1", "Buyer1");
			supplierPK1 = TestDataCreator.CreateOrganisation("SUPPLISYD1", "Supplier1");
			buyerPK2 = TestDataCreator.CreateOrganisation("BUYERSYD2", "Buyer2");
			supplierPK2 = TestDataCreator.CreateOrganisation("SUPPLISYD2", "Supplier2");
			manufacturerPK = TestDataCreator.CreateOrganisation("MANUFASYD", "Manufacturer");
			controllingCustomerPK = TestDataCreator.CreateOrganisation("CTRLCUSYD", "Controlling Customer");

			buyerAddressPK1 = TestDataCreator.CreateAddress(buyerPK1, "BUYERSYD1", "1 buyer st");
			buyerAddressPK2 = TestDataCreator.CreateAddress(buyerPK2, "BUYERSYD2", "2 buyer st");
			supplierAddressPK1 = TestDataCreator.CreateAddress(supplierPK1, "SUPPLISYD1", "1 supplier st");
			supplierAddressPK2 = TestDataCreator.CreateAddress(supplierPK2, "SUPPLISYD2", "2 supplier st");
			manufacturerAddressPK = TestDataCreator.CreateAddress(manufacturerPK, "MANUFASYD", "1 manufacturer st");
			controllingCustomerAddressPK = TestDataCreator.CreateAddress(controllingCustomerPK, "CTRLCUSYD", "1 controllingCustomer st");
		}

		void AssertShouldSeeStatuses(string partyType, Guid organisationPK, int numberOfShouldSee, bool isLoadListParty = false)
		{
			foreach (var item in ContainerLoadListStatuses.Select((status, index) => new { index, status }))
			{
				var status = item.status;
				var index = item.index;

				var orderPK1 = TestDataCreator.CreateJobOrderHeader($"ORDER-1#{index}", buyerAddressPK1, supplierAddress: supplierAddressPK1, orderStatus: "PLC");
				var orderLinePK1 = TestDataCreator.CreateJobOrderLine(orderPK1);

				var orderPK2 = TestDataCreator.CreateJobOrderHeader($"ORDER-2#{index}", buyerAddressPK2, supplierAddress: supplierAddressPK2, orderStatus: "PLC");
				var orderLinePK2 = TestDataCreator.CreateJobOrderLine(orderPK2);

				var supplierBookingPK = TestDataCreator.CreateJobSupplierBooking($"BOOKING-{index}", supplierPK1, "PLN");
				TestDataCreator.CreateDocAddress(supplierAddressPK1, "", supplierBookingPK, "JSB", "SUD");

				var supplierBookingLinePK1 = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK1, supplierBookingPK, "JSL001" + index);
				TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", supplierBookingLinePK1, "JSL", "MAN");

				var supplierBookingLinePK2 = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK2, supplierBookingPK, "JSL002" + index);
				TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", supplierBookingLinePK2, "JSL", "MAN");

				var loadListPartyPK = isLoadListParty ? organisationPK : supplierPK1;
				var containerLoadListPK = TestDataCreator.CreateCFSContainerLoadList($"LOADLIST-{index}", supplierBookingPK, loadListPartyPK, status);
				TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", containerLoadListPK, "CLH", "SCP");

				var refContainer = TestDataCreator.CreateRefContainer($"RC{index}");
				var containerPK = TestDataCreator.CreateJobContainer($"CONTAINER-{index}", refContainer);

				TestDataCreator.CreateCFSContainerLoadListLine(containerLoadListPK, supplierBookingLinePK1, containerPK, null);
				TestDataCreator.CreateCFSContainerLoadListLine(containerLoadListPK, supplierBookingLinePK2, containerPK, null);
			}

			var results = ExecuteAs(organisationPK);
			AssertEquals($"{partyType} (when related) should see headers based on status", numberOfShouldSee, results.Rows.Count);
		}

		DataTable ExecuteAs(Guid loggedInContactOrganisation)
		{
			using (var command = TestConnection.Command("SELECT * FROM GetCFSContainerLoadListLinesWithSecurityContext(@loggedInContactOrganisation)"))
			{
				command.AddParameter("@loggedInContactOrganisation", SqlDbType.UniqueIdentifier, loggedInContactOrganisation);

				return DataUtils.GetDataTableFromCommand(command);
			}
		}

		IEnumerable<string> GetColumnNames(string commandText)
		{
			using (var command = TestConnection.Command(commandText))
			using (var reader = command.ExecuteReader(CommandBehavior.SchemaOnly))
			{
				return reader
					.GetSchemaTable()
					.Select()
					.Select(x => (string)x["ColumnName"]);
			}
		}

		protected IEnumerable<string> ContainerLoadListStatuses
		{
			get
			{
				yield return "INC";
				yield return "REJ";
				yield return "PLC";
				yield return "APP";
				yield return "SHP";
				yield return "CAN";
				yield return "CNV";
			}
		}

		Guid buyerPK1;
		Guid supplierPK1;
		Guid buyerPK2;
		Guid supplierPK2;
		Guid manufacturerPK;
		Guid controllingCustomerPK;

		Guid buyerAddressPK1;
		Guid supplierAddressPK1;
		Guid buyerAddressPK2;
		Guid supplierAddressPK2;
		Guid manufacturerAddressPK;
		Guid controllingCustomerAddressPK;

		#endregion
	}
}
