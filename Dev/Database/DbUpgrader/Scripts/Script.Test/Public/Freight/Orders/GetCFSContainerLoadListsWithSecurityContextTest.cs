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
	[TestedType(typeof(GetCFSContainerLoadListsWithSecurityContext))]
	class GetCFSContainerLoadListsWithSecurityContextTest : DbCreateScriptTest
	{
		public void TestReturnsContainerLoadListColumns()
		{
			var fromContainerLoadList = GetColumnNames("SELECT * FROM dbo.ContainerLoadListHeader");
			var fromTvf = GetColumnNames("SELECT * FROM GetCFSContainerLoadListsWithSecurityContext(NEWID())");

			var missingColumns = fromContainerLoadList.Except(fromTvf.Append("CLH_LoadMode"));
			var errorMessage = $"The following columns are missing and should be added to the function: {string.Join(", ", missingColumns)}";

			Assert(errorMessage, !missingColumns.Any());
		}

		public void TestGetCFSContainerLoadLists_BuyerVisibility()
		{
			SetupOrganisations();
			AssertShouldSeeStatuses("Buyer", buyerPK, shouldSee: ContainerLoadListStatuses.Where(status => status != "INC"));
		}

		public void TestGetCFSContainerLoadLists_ControllingCustomerVisibility()
		{
			SetupOrganisations();
			AssertShouldSeeStatuses("Controlling Customer", controllingCustomerPK, shouldSee: ContainerLoadListStatuses);
		}

		public void TestGetCFSContainerLoadLists_SupplierVisibility()
		{
			SetupOrganisations();
			AssertShouldSeeStatuses("Supplier", supplierPK, shouldSee: Array.Empty<string>());
		}

		public void TestGetCFSContainerLoadLists_ManufacturerVisibility_AsLoadListParty()
		{
			SetupOrganisations();
			AssertShouldSeeStatuses("Manufacturer (+Load List Party)", manufacturerPK, shouldSee: Array.Empty<string>(), isLoadListParty: true);
		}

		public void TestGetCFSContainerLoadLists_ManufacturerVisibility_NotLoadListParty()
		{
			SetupOrganisations();
			AssertShouldSeeStatuses("Manufacturer (only)", manufacturerPK, shouldSee: Array.Empty<string>());
		}

		public void TestGetCFSContainerLoadLists_UnrelatedOrg()
		{
			SetupOrganisations();
			AssertShouldSeeStatuses("Unrelated Org", Guid.NewGuid(), shouldSee: Array.Empty<string>());
		}

		public void TestGetCFSContainerLoadLists_NoDuplicateRows()
		{
			SetupOrganisations();

			var orderPK = TestDataCreator.CreateJobOrderHeader("ORD01", buyerAddressPK, supplierAddress: supplierAddressPK, orderStatus: "PLC");
			var orderLine1PK = TestDataCreator.CreateJobOrderLine(orderPK, 1);
			var orderLine2PK = TestDataCreator.CreateJobOrderLine(orderPK, 2);

			var supplierBookingPK = TestDataCreator.CreateJobSupplierBooking("SBK01", supplierPK, "PLN");
			TestDataCreator.CreateDocAddress(supplierAddressPK, "", supplierBookingPK, "JSB", "SUD", 0);
			TestDataCreator.CreateDocAddress(supplierAddressPK, "", supplierBookingPK, "JSB", "SUD", 1);
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", supplierBookingPK, "JSB", "SCP", 0);
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", supplierBookingPK, "JSB", "SCP", 1);

			var supplierBookingLine1PK = TestDataCreator.CreateJobSupplierBookingLine(orderLine1PK, supplierBookingPK, "JSL100");
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", supplierBookingLine1PK, "JSL", "MAN");
			var supplierBookingLine2PK = TestDataCreator.CreateJobSupplierBookingLine(orderLine2PK, supplierBookingPK, "JSL200");
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", supplierBookingLine2PK, "JSL", "MAN");

			var containerLoadListPK = TestDataCreator.CreateCFSContainerLoadList("CLH01", supplierBookingPK, manufacturerPK, "PLC");
			var refContainer = TestDataCreator.CreateRefContainer("RC01");
			var containerPK = TestDataCreator.CreateJobContainer("JC01", refContainer);

			TestDataCreator.CreateCFSContainerLoadListLine(containerLoadListPK, supplierBookingLine1PK, containerPK, null);
			TestDataCreator.CreateCFSContainerLoadListLine(containerLoadListPK, supplierBookingLine1PK, Guid.Empty, null);

			TestDataCreator.CreateCFSContainerLoadListLine(containerLoadListPK, supplierBookingLine2PK, containerPK, null);
			TestDataCreator.CreateCFSContainerLoadListLine(containerLoadListPK, supplierBookingLine2PK, Guid.Empty, null);

			var results = ExecuteAs(buyerPK).Select();
			AssertEquals("Should only return 1 container load list with context attached", 1, results.Length);
		}

		public void TestGetCFSContainerLoadLists_DoesNotReturnCYRecords()
		{
			SetupOrganisations();

			var refContainer = TestDataCreator.CreateRefContainer("RC");
			var containerPK1 = TestDataCreator.CreateJobContainer("CONTAINER1", refContainer);
			var containerPK2 = TestDataCreator.CreateJobContainer("CONTAINER2", refContainer);

			var orderPK = TestDataCreator.CreateJobOrderHeader("ORDER", buyerAddressPK, supplierAddress: supplierAddressPK, orderStatus: "PLC");
			var orderLinePK = TestDataCreator.CreateJobOrderLine(orderPK);

			var supplierBookingPK = TestDataCreator.CreateJobSupplierBooking("SBK01", supplierPK, "PLN");
			var supplierBookingLinePK1 = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, supplierBookingPK, "JSL001");
			var supplierBookingLinePK2 = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, supplierBookingPK, "JSL002");

			var cyContainerLoadListPK = TestDataCreator.CreateCYContainerLoadList("CLH01", supplierBookingPK, supplierPK, "PLC");
			TestDataCreator.CreateCYContainerLoadListLine(cyContainerLoadListPK, supplierBookingLinePK1, containerPK1);

			var cfsContainerLoadListPK = TestDataCreator.CreateCFSContainerLoadList("CLH02", supplierBookingPK, supplierPK, "PLC");
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", cfsContainerLoadListPK, "CLH", "SCP", 0);
			TestDataCreator.CreateCFSContainerLoadListLine(cfsContainerLoadListPK, supplierBookingLinePK2, containerPK2, null);

			var results = ExecuteAs(controllingCustomerPK).Select();
			AssertEquals("It should only return CFS container load lists, not CY container load lists.", 1, results.Length);
		}

		public void TestGetCFSContainerLoadLists_ReturnOwnRecords()
		{
			SetupOrganisations();

			var controllingCustomerPK2 = TestDataCreator.CreateOrganisation("CTRLCUSYD2", "Controlling Customer2");
			var controllingCustomerAddressPK2 = TestDataCreator.CreateAddress(controllingCustomerPK2, "CTRLCUSYD2", "2 controllingCustomer st");

			var refContainer = TestDataCreator.CreateRefContainer("RC");
			var containerPK1 = TestDataCreator.CreateJobContainer("CONTAINER1", refContainer);
			var containerPK2 = TestDataCreator.CreateJobContainer("CONTAINER2", refContainer);

			var orderPK = TestDataCreator.CreateJobOrderHeader("ORDER", buyerAddressPK, supplierAddress: supplierAddressPK, orderStatus: "PLC");
			var orderLinePK = TestDataCreator.CreateJobOrderLine(orderPK);

			var supplierBookingPK = TestDataCreator.CreateJobSupplierBooking("SBK01", supplierPK, "PLN");
			var supplierBookingLinePK1 = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, supplierBookingPK, "JSL001");
			var supplierBookingLinePK2 = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, supplierBookingPK, "JSL002");

			var cfsContainerLoadListPK1 = TestDataCreator.CreateCFSContainerLoadList("CLH01", supplierBookingPK, supplierPK, "PLC");
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", cfsContainerLoadListPK1, "CLH", "SCP", 0);
			TestDataCreator.CreateCFSContainerLoadListLine(cfsContainerLoadListPK1, supplierBookingLinePK1, containerPK1, null);
			TestDataCreator.CreateCFSContainerLoadListLine(cfsContainerLoadListPK1, supplierBookingLinePK1, Guid.Empty, null);

			var cfsContainerLoadListPK2 = TestDataCreator.CreateCFSContainerLoadList("CLH02", supplierBookingPK, supplierPK, "PLC");
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK2, "", cfsContainerLoadListPK2, "CLH", "SCP", 0);
			TestDataCreator.CreateCFSContainerLoadListLine(cfsContainerLoadListPK2, supplierBookingLinePK2, containerPK2, null);
			TestDataCreator.CreateCFSContainerLoadListLine(cfsContainerLoadListPK2, supplierBookingLinePK2, Guid.Empty, null);

			AssertEquals(1, ExecuteAs(controllingCustomerPK).Select().Length);
			AssertEquals(1, ExecuteAs(controllingCustomerPK2).Select().Length);

			AssertEquals(2, ExecuteAs(buyerPK).Select().Length);
		}

		#region Implementation

		void SetupOrganisations()
		{
			buyerPK = TestDataCreator.CreateOrganisation("BUYERSYD", "Buyer");
			supplierPK = TestDataCreator.CreateOrganisation("SUPPLISYD", "Supplier");
			manufacturerPK = TestDataCreator.CreateOrganisation("MANUFASYD", "Manufacturer");
			controllingCustomerPK = TestDataCreator.CreateOrganisation("CTRLCUSYD", "Controlling Customer");

			buyerAddressPK = TestDataCreator.CreateAddress(buyerPK, "BUYERSYD", "1 buyer st");
			supplierAddressPK = TestDataCreator.CreateAddress(supplierPK, "SUPPLISYD", "1 supplier st");
			manufacturerAddressPK = TestDataCreator.CreateAddress(manufacturerPK, "MANUFASYD", "1 manufacturer st");
			controllingCustomerAddressPK = TestDataCreator.CreateAddress(controllingCustomerPK, "CTRLCUSYD", "1 controllingCustomer st");
		}

		void AssertShouldSeeStatuses(string partyType, Guid organisationPK, IEnumerable<string> shouldSee, bool isLoadListParty = false)
		{
			foreach (var item in ContainerLoadListStatuses.Select((status, index) => new { index, status }))
			{
				var status = item.status;
				var index = item.index;

				var orderPK = TestDataCreator.CreateJobOrderHeader($"ORDER-{index}", buyerAddressPK, supplierAddress: supplierAddressPK, orderStatus: "PLC");
				var orderLinePK = TestDataCreator.CreateJobOrderLine(orderPK);

				var supplierBookingPK = TestDataCreator.CreateJobSupplierBooking($"BOOKING-{index}", supplierPK, "PLN");
				TestDataCreator.CreateDocAddress(supplierAddressPK, "", supplierBookingPK, "JSB", "SUD");

				var supplierBookingLinePK = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, supplierBookingPK, "JSL00" + index);
				TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", supplierBookingLinePK, "JSL", "MAN");

				var loadListPartyPK = isLoadListParty ? organisationPK : supplierPK;
				var containerLoadListPK = TestDataCreator.CreateCFSContainerLoadList($"LOADLIST-{index}", supplierBookingPK, loadListPartyPK, status);
				TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", containerLoadListPK, "CLH", "SCP");

				var refContainer = TestDataCreator.CreateRefContainer($"RC{index}");
				var containerPK = TestDataCreator.CreateJobContainer($"CONTAINER-{index}", refContainer);

				TestDataCreator.CreateCFSContainerLoadListLine(containerLoadListPK, supplierBookingLinePK, containerPK, null);
				TestDataCreator.CreateCFSContainerLoadListLine(containerLoadListPK, supplierBookingLinePK, Guid.Empty, null);
			}

			var results = ExecuteAs(organisationPK);
			AssertEquals($"{partyType} (when related) should see headers based on status", shouldSee.Count(), results.Rows.Count);
			AssertCanEdit(partyType, results, false);
		}

		protected void AssertCanEdit(string partyType, DataTable results, bool canEdit)
		{
			foreach (var row in results.Rows.OfType<DataRow>())
			{
				var status = row["CLH_Status"].ToString();
				var actual = row["CanEdit"];
				var message = $"{partyType}: CanEdit: {status}";

				AssertEquals(message, false, actual);
			}
		}

		DataTable ExecuteAs(Guid loggedInContactOrganisation)
		{
			using (var command = TestConnection.Command("SELECT * FROM GetCFSContainerLoadListsWithSecurityContext(@loggedInContactOrganisation)"))
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

		Guid buyerPK;
		Guid supplierPK;
		Guid manufacturerPK;
		Guid controllingCustomerPK;

		Guid buyerAddressPK;
		Guid supplierAddressPK;
		Guid manufacturerAddressPK;
		Guid controllingCustomerAddressPK;

		#endregion
	}
}
