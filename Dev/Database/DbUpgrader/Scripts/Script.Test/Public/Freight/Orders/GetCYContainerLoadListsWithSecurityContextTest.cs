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
	[TestedType(typeof(GetCYContainerLoadListsWithSecurityContext))]
	class GetCYContainerLoadListsWithSecurityContextTest : DbCreateScriptTest
	{
		public void TestReturnsContainerLoadListColumns()
		{
			var fromContainerLoadList = GetColumnNames("SELECT * FROM dbo.ContainerLoadListHeader");
			var fromTvf = GetColumnNames("SELECT * FROM GetCYContainerLoadListsWithSecurityContext(NEWID())");

			var missingColumns = fromContainerLoadList.Except(fromTvf.Append("CLH_LoadMode"));
			var errorMessage = $"The following columns are missing and should be added to the function: {string.Join(", ", missingColumns)}";

			Assert(errorMessage, !missingColumns.Any());
		}

		public void TestGetCYContainerLoadLists_BuyerVisibility()
		{
			SetupOrganisations();
			AssertShouldSeeStatuses("Buyer", buyerPK, shouldSee: ContainerLoadListStatuses);
		}

		public void TestGetCYContainerLoadLists_SupplierVisibility()
		{
			SetupOrganisations();
			AssertShouldSeeStatuses("Supplier", supplierPK, shouldSee: ContainerLoadListStatuses);
		}

		public void TestGetCYContainerLoadLists_ManufacturerVisibility_AsLoadListParty()
		{
			SetupOrganisations();
			AssertShouldSeeStatuses("Manufacturer (+Load List Party)", manufacturerPK, shouldSee: ContainerLoadListStatuses, isLoadListParty: true);
		}

		public void TestGetCYContainerLoadLists_ManufacturerVisibility_NotLoadListParty()
		{
			SetupOrganisations();
			AssertShouldSeeStatuses("Manufacturer (only)", manufacturerPK, shouldSee: Array.Empty<string>());
		}

		public void TestGetCYContainerLoadLists_ControllingCustomerVisibility()
		{
			SetupOrganisations();
			AssertShouldSeeStatuses("Controlling Customer", controllingCustomerPK, shouldSee: ContainerLoadListStatuses);
		}

		public void TestGetCYContainerLoadLists_UnrelatedOrg()
		{
			SetupOrganisations();
			AssertShouldSeeStatuses("Unrelated Org", Guid.NewGuid(), shouldSee: Array.Empty<string>());
		}

		public void TestGetCYContainerLoadLists_NoDuplicateRows()
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

			var supplierBookingLine1PK = TestDataCreator.CreateJobSupplierBookingLine(orderLine1PK, supplierBookingPK, "JSL001");
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", supplierBookingLine1PK, "JSL", "MAN");
			var supplierBookingLine2PK = TestDataCreator.CreateJobSupplierBookingLine(orderLine2PK, supplierBookingPK, "JSL002");
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", supplierBookingLine2PK, "JSL", "MAN");

			var containerLoadListPK = TestDataCreator.CreateCYContainerLoadList("CLH01", supplierBookingPK, manufacturerPK, "PLC");
			var refContainer = TestDataCreator.CreateRefContainer("RC01");
			var containerPK = TestDataCreator.CreateJobContainer("JC01", refContainer);

			TestDataCreator.CreateCYContainerLoadListLine(containerLoadListPK, supplierBookingLine1PK, containerPK);
			TestDataCreator.CreateCYContainerLoadListLine(containerLoadListPK, supplierBookingLine2PK, containerPK);

			var results = ExecuteAs(buyerPK).Select();
			AssertEquals("Should only return 1 container load list with context attached", 1, results.Length);
		}

		public void TestGetCYContainerLoadLists_DoesNotReturnCFSRecords()
		{
			SetupOrganisations();

			var supplierBookingPK = TestDataCreator.CreateJobSupplierBooking("SBK01", supplierPK, "PLN");
			TestDataCreator.CreateCYContainerLoadList("CLH01", supplierBookingPK, supplierPK, "PLC");
			TestDataCreator.CreateCFSContainerLoadList("CLH02", supplierBookingPK, supplierPK, "PLC");

			var results = ExecuteAs(supplierPK).Select();
			AssertEquals("It should only return CY container load lists, not CFS container load lists.", 1, results.Length);
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
				var orderLine1PK = TestDataCreator.CreateJobOrderLine(orderPK, 1 * 10000 + index);
				var orderLine2PK = TestDataCreator.CreateJobOrderLine(orderPK, 2 * 10000 + index);

				var supplierBookingPK = TestDataCreator.CreateJobSupplierBooking($"BOOKING-{index}", supplierPK, "PLN");
				TestDataCreator.CreateDocAddress(supplierAddressPK, "", supplierBookingPK, "JSB", "SUD");
				TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", supplierBookingPK, "JSB", "SCP");

				var supplierBookingLine1PK = TestDataCreator.CreateJobSupplierBookingLine(orderLine1PK, supplierBookingPK, "JSL00" + index);
				TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", supplierBookingLine1PK, "JSL", "MAN");

				var supplierBookingLine2PK = TestDataCreator.CreateJobSupplierBookingLine(orderLine1PK, supplierBookingPK, "JSL01" + index);
				TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", supplierBookingLine2PK, "JSL", "MAN");

				var loadListPartyPK = isLoadListParty ? organisationPK : supplierPK;
				var containerLoadListPK = TestDataCreator.CreateCYContainerLoadList($"LOADLIST-{index}", supplierBookingPK, loadListPartyPK, status);

				var refContainer = TestDataCreator.CreateRefContainer($"RC{index}");
				var containerPK = TestDataCreator.CreateJobContainer($"CONTAINER-{index}", refContainer);

				TestDataCreator.CreateCYContainerLoadListLine(containerLoadListPK, supplierBookingLine1PK, containerPK);
				TestDataCreator.CreateCYContainerLoadListLine(containerLoadListPK, supplierBookingLine2PK, containerPK);
			}

			var results = ExecuteAs(organisationPK);
			AssertEquals($"{partyType} (when related) should see headers based on status", shouldSee.Count(), results.Rows.Count);
			AssertCanEdit(partyType, results, organisationPK != buyerPK && organisationPK != controllingCustomerPK);
		}

		protected void AssertCanEdit(string partyType, DataTable results, bool isSupplierOrRelatedManufacturer)
		{
			foreach (var row in results.Rows.OfType<DataRow>())
			{
				var status = row["CLH_Status"].ToString();
				var actual = row["CanEdit"];
				var message = $"{partyType}: CanEdit: {status}";

				if ((status == "INC" || status == "REJ") && isSupplierOrRelatedManufacturer)
				{
					AssertEquals(message, true, actual);
				}
				else
				{
					AssertEquals(message, false, actual);
				}
			}
		}

		DataTable ExecuteAs(Guid loggedInContactOrganisation)
		{
			using (var command = TestConnection.Command("SELECT * FROM GetCYContainerLoadListsWithSecurityContext(@loggedInContactOrganisation)"))
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
