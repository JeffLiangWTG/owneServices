using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Container;
using Enterprise.Build.Database.Script.Public.Freight.Cartage.Testing;
using Enterprise.Build.Database.Script.Public.Freight.Shipment.Testing;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Container.Testing
{
	[TestedType(typeof(GetJobContainersWithSecurityContext))]
	class GetJobContainersWithSecurityContextTest : DbCreateScriptTest
	{
		public void TestReturnsJobContainerColumns()
		{
			var fromJobContainer = GetColumnNames("SELECT * FROM dbo.JobContainer");
			var fromTvf = GetColumnNames("SELECT * FROM GetJobContainersWithSecurityContext('', 0, 0, 0, 0, '', '')");

			var missingColumns = fromJobContainer.Except(fromTvf);
			var errorMessage = $"The following columns are missing and should be added to the function: {string.Join(", ", missingColumns)}";

			Assert(errorMessage, !missingColumns.Any());
		}

		public void TestContainersFromShipments()
		{
			var companyPK = TestDataCreator.CreateCompany("CMP", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "BRN", "USORD");
			var departmentPK = TestDataCreator.CreateDepartment("DPT");

			var org1PK = TestDataCreator.CreateOrganisation("ORG1", "Test Org1");
			var address1PK = TestDataCreator.CreateAddress(org1PK, "Address1", "Test Address1");

			var org2PK = TestDataCreator.CreateOrganisation("ORG2", "Test Org2");
			var address2PK = TestDataCreator.CreateAddress(org2PK, "Address2", "Test Address2");

			var allowedShipments = shipmentsWithSecurity.CreateAllowedShipments(org1PK, address1PK, companyPK, branchPK, departmentPK);
			var restrictedShipments = shipmentsWithSecurity.CreateAllowedShipments(org2PK, address2PK, companyPK, branchPK, departmentPK);

			var allowedContainers = allowedShipments.Select(s => CreateShipmentContainer(s)).ToList();
			restrictedShipments.Select(s => CreateShipmentContainer(s)).ToList();
			AssertEquals(14, allowedContainers.Count);

			var relatedOrgsList = new[] { org1PK };
			var relatedAddressesList = new[] { address1PK };

			var containers = GetJobContainersWithSecurityContext(loggedInOrganisation: Guid.NewGuid(), relatedOrgsList: relatedOrgsList, relatedAddressesList: relatedAddressesList);
			AssertEquals(allowedContainers.Count, containers.Count());
			AssertContainsExactElementsInAnyOrder(allowedContainers, containers);
		}

		public void TestContainersFromPackLines()
		{
			var companyPK = TestDataCreator.CreateCompany("CMP", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "BRN", "USORD");
			var departmentPK = TestDataCreator.CreateDepartment("DPT");

			var org1PK = TestDataCreator.CreateOrganisation("ORG1", "Test Org1");
			var address1PK = TestDataCreator.CreateAddress(org1PK, "Address1", "Test Address1");

			var org2PK = TestDataCreator.CreateOrganisation("ORG2", "Test Org2");
			var address2PK = TestDataCreator.CreateAddress(org2PK, "Address2", "Test Address2");

			var allowedShipments = shipmentsWithSecurity.CreateAllowedShipments(org1PK, address1PK, companyPK, branchPK, departmentPK);
			var restrictedShipments = shipmentsWithSecurity.CreateAllowedShipments(org2PK, address2PK, companyPK, branchPK, departmentPK);

			var allowedContainers = allowedShipments.Select(s => CreatePackLineContainer(s)).ToList();
			restrictedShipments.Select(s => CreatePackLineContainer(s)).ToList();
			AssertEquals(14, allowedContainers.Count);

			var relatedOrgsList = new[] { org1PK };
			var relatedAddressesList = new[] { address1PK };

			var containers = GetJobContainersWithSecurityContext(loggedInOrganisation: Guid.NewGuid(), relatedOrgsList: relatedOrgsList, relatedAddressesList: relatedAddressesList);
			AssertEquals(allowedContainers.Count, containers.Count());
			AssertContainsExactElementsInAnyOrder(allowedContainers, containers);
		}

		public void TestContainersFromCartages()
		{
			var companyPK = TestDataCreator.CreateCompany("CMP", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "BRN", "USORD");
			var departmentPK = TestDataCreator.CreateDepartment("DPT");

			var org1PK = TestDataCreator.CreateOrganisation("ORG1", "Test Org1");
			var address1PK = TestDataCreator.CreateAddress(org1PK, "Address1", "Test Address1");

			var org2PK = TestDataCreator.CreateOrganisation("ORG2", "Test Org2");
			var address2PK = TestDataCreator.CreateAddress(org2PK, "Address2", "Test Address2");

			var allowedCartages = cartagesWithSecurity.CreateAllowedCartages(org1PK, address1PK, companyPK, branchPK, departmentPK);
			var restrictedCartages = cartagesWithSecurity.CreateAllowedCartages(org2PK, address2PK, companyPK, branchPK, departmentPK);

			var allowedContainers = allowedCartages.Select(c => CreateCartageContainer(c)).ToList();
			restrictedCartages.Select(c => CreateCartageContainer(c)).ToList();
			AssertEquals(4, allowedContainers.Count);

			var relatedOrgsList = new[] { org1PK };
			var relatedAddressesList = new[] { address1PK };

			var containers = GetJobContainersWithSecurityContext(loggedInOrganisation: Guid.NewGuid(), relatedOrgsList: relatedOrgsList, relatedAddressesList: relatedAddressesList);
			AssertEquals(allowedContainers.Count, containers.Count());
			AssertContainsExactElementsInAnyOrder(allowedContainers, containers);
		}

		public void TestContainersFromCYContainerLoadList()
		{
			var org1PK = TestDataCreator.CreateOrganisation("ORG1", "Test Org1");
			var address1PK = TestDataCreator.CreateAddress(org1PK, "Address1", "Test Address1");

			var org2PK = TestDataCreator.CreateOrganisation("ORG2", "Test Org2");
			var address2PK = TestDataCreator.CreateAddress(org2PK, "Address2", "Test Address2");

			var supplierPK = TestDataCreator.CreateOrganisation("SUPORG", "Supplier");

			var allowedContainerLoadLists = CreateAllowedCYContainerLoadLists("Allowed", org1PK, address1PK, supplierPK);
			var restrictedContainerLoadLists = CreateAllowedCYContainerLoadLists("Restricted", org2PK, address2PK, supplierPK);

			var allowedContainers = allowedContainerLoadLists.Select(c => CreateLoadListContainer(c)).ToList();
			restrictedContainerLoadLists.Select(c => CreateLoadListContainer(c)).ToList();
			AssertEquals(4, allowedContainers.Count);

			var relatedOrgsList = new[] { org1PK };
			var relatedAddressesList = new[] { address1PK };

			var containers = GetJobContainersWithSecurityContext(loggedInOrganisation: org1PK, relatedOrgsList: relatedOrgsList, relatedAddressesList: relatedAddressesList);
			AssertEquals(allowedContainers.Count, containers.Count());
			AssertContainsExactElementsInAnyOrder(allowedContainers, containers);
		}

		public void TestContainersFromCFSContainerLoadList()
		{
			var org1PK = TestDataCreator.CreateOrganisation("ORG1", "Test Org1");
			var address1PK = TestDataCreator.CreateAddress(org1PK, "Address1", "Test Address1");

			var org2PK = TestDataCreator.CreateOrganisation("ORG2", "Test Org2");
			var address2PK = TestDataCreator.CreateAddress(org2PK, "Address2", "Test Address2");

			var supplierPK = TestDataCreator.CreateOrganisation("SUPORG", "Supplier");

			var allowedContainerLoadLists = CreateAllowedCFSContainerLoadLists("Allowed", org1PK, address1PK, supplierPK);
			var restrictedContainerLoadLists = CreateAllowedCFSContainerLoadLists("Restricted", org2PK, address2PK, supplierPK);

			var allowedContainers = allowedContainerLoadLists.Select(c => CreateLoadListContainer(c)).ToList();
			restrictedContainerLoadLists.Select(c => CreateLoadListContainer(c)).ToList();
			AssertEquals(2, allowedContainers.Count);

			var relatedOrgsList = new[] { org1PK };
			var relatedAddressesList = new[] { address1PK };

			var containers = GetJobContainersWithSecurityContext(loggedInOrganisation: org1PK, relatedOrgsList: relatedOrgsList, relatedAddressesList: relatedAddressesList);
			AssertEquals(allowedContainers.Count, containers.Count());
			AssertContainsExactElementsInAnyOrder(allowedContainers, containers);
		}

		public void TestQueryPlan()
		{
			var companyPK = TestDataCreator.CreateCompany("CMP", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "BRN", "USORD");
			var departmentPK = TestDataCreator.CreateDepartment("DPT");

			var org1PK = TestDataCreator.CreateOrganisation("ORG1", "Test Org1");
			var address1PK = TestDataCreator.CreateAddress(org1PK, "Address1", "Test Address1");

			var org2PK = TestDataCreator.CreateOrganisation("ORG2", "Test Org2");
			var address2PK = TestDataCreator.CreateAddress(org2PK, "Address2", "Test Address2");

			var relatedOrgsList = new[] { org1PK };
			var relatedAddressesList = new[] { address1PK };

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				TestConnection.ExecuteReader($"SELECT * FROM GetJobContainersWithSecurityContext('{org1PK}', 1, 1, 1, 1, N'{address1PK}', N'{org1PK}')", _ => { });
				var queryPlan = TestConnection.ExecutedCommandsAndQueryPlans.First();
				var queryPlanAnalyzer = new QueryPlanalyzer(queryPlan.Item2.First());

				AssertEquals("There should only be 1 index scan on JobConsol", 1, queryPlanAnalyzer.IndexScans.Count(x => x.TableName == "JobConsol"));
			}
		}

		public IEnumerable<Guid> CreateAllowedCYContainerLoadLists(string uniquePrefix, Guid organisationPK, Guid addressPK, Guid supplierPK)
		{
			var supplierBookingWithSUDPK = TestDataCreator.CreateJobSupplierBooking($"{uniquePrefix}SUD", supplierPK);
			TestDataCreator.CreateDocAddress(addressPK, "", supplierBookingWithSUDPK, "JSB", "SUD");
			var containerLoadListWithSUDPK = TestDataCreator.CreateCYContainerLoadList($"{uniquePrefix}SUD", supplierBookingWithSUDPK, supplierPK);

			var supplierBookingWithSCPPK = TestDataCreator.CreateJobSupplierBooking($"{uniquePrefix}SCP", supplierPK);
			TestDataCreator.CreateDocAddress(addressPK, "", supplierBookingWithSCPPK, "JSB", "SCP");
			var containerLoadListWithSCPPK = TestDataCreator.CreateCYContainerLoadList($"{uniquePrefix}SCP", supplierBookingWithSCPPK, supplierPK);

			var orderHeaderPK = TestDataCreator.CreateJobOrderHeader(uniquePrefix, addressPK);
			var orderLinePK = TestDataCreator.CreateJobOrderLine(orderHeaderPK);
			var supplierBookingWithOrderPK = TestDataCreator.CreateJobSupplierBooking($"{uniquePrefix}Order", supplierPK);
			var supplierBookingLineWithOrderPK = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, supplierBookingWithOrderPK, uniquePrefix);
			var containerLoadListWithOrderPK = TestDataCreator.CreateCYContainerLoadList($"{uniquePrefix}Order", supplierBookingWithOrderPK, supplierPK);
			var containerWithOrderPK = TestDataCreator.CreateJobContainer(uniquePrefix, null);
			TestDataCreator.CreateCYContainerLoadListLine(containerLoadListWithOrderPK, supplierBookingLineWithOrderPK, containerWithOrderPK);

			var supplierBookingPK = TestDataCreator.CreateJobSupplierBooking($"{uniquePrefix}Booking", supplierPK);
			var containerLoadListPK = TestDataCreator.CreateCYContainerLoadList($"{uniquePrefix}List", supplierBookingPK, organisationPK);

			return new[]
			{
				containerLoadListWithSUDPK,
				containerLoadListWithSCPPK,
				containerLoadListWithOrderPK,
				containerLoadListPK
			};
		}

		public IEnumerable<Guid> CreateAllowedCFSContainerLoadLists(string uniquePrefix, Guid organisationPK, Guid addressPK, Guid supplierPK)
		{
			var supplierBookingWithSCPPK = TestDataCreator.CreateJobSupplierBooking($"{uniquePrefix}SCP", supplierPK);
			var containerLoadListWithSCPPK = TestDataCreator.CreateCFSContainerLoadList($"{uniquePrefix}SCP", supplierBookingWithSCPPK, supplierPK, "APP");
			TestDataCreator.CreateDocAddress(addressPK, "", containerLoadListWithSCPPK, "CLH", "SCP");

			var orderHeaderPK = TestDataCreator.CreateJobOrderHeader(uniquePrefix, addressPK);
			var orderLinePK = TestDataCreator.CreateJobOrderLine(orderHeaderPK);
			var supplierBookingWithOrderPK = TestDataCreator.CreateJobSupplierBooking($"{uniquePrefix}Order", supplierPK);
			var supplierBookingLineWithOrderPK = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, supplierBookingWithOrderPK, uniquePrefix);
			var containerLoadListWithOrderPK = TestDataCreator.CreateCYContainerLoadList($"{uniquePrefix}Order", supplierBookingWithOrderPK, supplierPK);
			var containerWithOrderPK = TestDataCreator.CreateJobContainer(uniquePrefix, null);
			TestDataCreator.CreateCFSContainerLoadListLine(containerLoadListWithOrderPK, supplierBookingLineWithOrderPK, containerWithOrderPK, null);

			return new[]
			{
				containerLoadListWithSCPPK,
				containerLoadListWithOrderPK,
			};
		}

		Guid CreateLoadListContainer(Guid loadListPK)
		{
			var containerPK = TestDataCreator.CreateJobContainer($"C{containerCounter}", null);
			LinkContainerToLoadList(containerPK, loadListPK);

			return containerPK;
		}

		Guid CreateCartageContainer(Guid cartagePK)
		{
			var containerPK = TestDataCreator.CreateJobContainer($"C{containerCounter}", null);
			TestDataCreator.CreateJobBookedCtgMove(cartagePK, containerPK);

			return containerPK;
		}

		public void TestAllAllowedContainers()
		{
			var companyPK = TestDataCreator.CreateCompany("CMP", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "BRN", "USORD");
			var departmentPK = TestDataCreator.CreateDepartment("DPT");

			var org1PK = TestDataCreator.CreateOrganisation("ORG1", "Test Org1");
			var address1PK = TestDataCreator.CreateAddress(org1PK, "Address1", "Test Address1");

			var org2PK = TestDataCreator.CreateOrganisation("ORG2", "Test Org2");
			var address2PK = TestDataCreator.CreateAddress(org2PK, "Address2", "Test Address2");

			var supplierPK = TestDataCreator.CreateOrganisation("SUPORG", "Supplier");

			var allowedShipments = shipmentsWithSecurity.CreateAllowedShipments(org1PK, address1PK, companyPK, branchPK, departmentPK);
			var restrictedShipments = shipmentsWithSecurity.CreateAllowedShipments(org2PK, address2PK, companyPK, branchPK, departmentPK);

			var allowedShipmentContainers = allowedShipments.Select(s => CreateShipmentContainer(s)).ToList();
			restrictedShipments.Select(s => CreateShipmentContainer(s)).ToList();

			allowedShipments = shipmentsWithSecurity.CreateAllowedShipments(org1PK, address1PK, companyPK, branchPK, departmentPK);
			restrictedShipments = shipmentsWithSecurity.CreateAllowedShipments(org2PK, address2PK, companyPK, branchPK, departmentPK);

			var allowedPackLinesContainers = allowedShipments.Select(s => CreatePackLineContainer(s)).ToList();
			restrictedShipments.Select(s => CreatePackLineContainer(s)).ToList();

			var allowedCartages = cartagesWithSecurity.CreateAllowedCartages(org1PK, address1PK, companyPK, branchPK, departmentPK);
			var restrictedCartages = cartagesWithSecurity.CreateAllowedCartages(org2PK, address2PK, companyPK, branchPK, departmentPK);

			var allowedCartageContainers = allowedCartages.Select(c => CreateCartageContainer(c)).ToList();
			restrictedCartages.Select(c => CreateCartageContainer(c)).ToList();

			var allowedContainerLoadLists = CreateAllowedCYContainerLoadLists("AllowedCY", org1PK, address1PK, supplierPK);
			var restrictedContainerLoadLists = CreateAllowedCYContainerLoadLists("RestrictedCY", org2PK, address2PK, supplierPK);

			var allowedCYLoadListContainers = allowedContainerLoadLists.Select(c => CreateLoadListContainer(c)).ToList();
			restrictedContainerLoadLists.Select(c => CreateLoadListContainer(c)).ToList();

			allowedContainerLoadLists = CreateAllowedCFSContainerLoadLists("AllowedCFS", org1PK, address1PK, supplierPK);
			restrictedContainerLoadLists = CreateAllowedCFSContainerLoadLists("RestrictedCFS", org2PK, address2PK, supplierPK);

			var allowedCFSLoadListContainers = allowedContainerLoadLists.Select(c => CreateLoadListContainer(c)).ToList();
			restrictedContainerLoadLists.Select(c => CreateLoadListContainer(c)).ToList();

			var allowedContainers = allowedShipmentContainers
				.Concat(allowedPackLinesContainers)
				.Concat(allowedCartageContainers)
				.Concat(allowedCYLoadListContainers)
				.Concat(allowedCFSLoadListContainers);

			var relatedOrgsList = new[] { org1PK };
			var relatedAddressesList = new[] { address1PK };

			var containers = GetJobContainersWithSecurityContext(loggedInOrganisation: org1PK, relatedOrgsList: relatedOrgsList, relatedAddressesList: relatedAddressesList);
			AssertEquals(allowedContainers.Count(), containers.Count());
			AssertContainsExactElementsInAnyOrder(allowedContainers, containers);
		}

		Guid CreateShipmentContainer(Guid shipmentPK)
		{
			var containerPK = TestDataCreator.CreateJobContainer($"C{containerCounter}", null);
			LinkContainerToShipment(containerPK, shipmentPK);

			return containerPK;
		}

		Guid CreatePackLineContainer(Guid shipmentPK)
		{
			var packLinePK = TestDataCreator.CreateJobPackLines(shipmentPK);
			var containerPK = TestDataCreator.CreateJobContainer($"C{containerCounter}", null);
			TestDataCreator.CreateJobContainerPackPivot(containerPK, packLinePK);

			return containerPK;
		}

		void LinkContainerToLoadList(Guid containerPK, Guid loadListPK)
		{
			TestConnection.ExecuteNonQuery($"UPDATE dbo.JobContainer SET JC_CLH_LoadListPlan='{loadListPK}',JC_SystemLastEditTimeUtc=GETUTCDATE(),JC_SystemLastEditUser='~BP' where JC_PK='{containerPK}'");
		}

		void LinkContainerToShipment(Guid containerPK, Guid shipmentPK)
		{
			TestConnection.ExecuteNonQuery($"UPDATE dbo.JobContainer SET JC_JS_FCLBookingOnlyLink='{shipmentPK}',JC_SystemLastEditTimeUtc=GETUTCDATE(),JC_SystemLastEditUser='~BP' where JC_PK='{containerPK}'");
		}

		IEnumerable<Guid> GetJobContainersWithSecurityContext(
			Guid loggedInOrganisation,
			IEnumerable<Guid> relatedAddressesList = null,
			IEnumerable<Guid> relatedOrgsList = null)
		{
			var relatedAddresses = relatedAddressesList?.Select(pk => $"{pk}") ?? Enumerable.Empty<string>();
			var relatedOrgs = relatedOrgsList?.Select(pk => $"{pk}") ?? Enumerable.Empty<string>();

			using (var command = TestConnection.Command("SELECT * FROM GetJobContainersWithSecurityContext(@loggedInOrganisationForContainers, @hasShipmentsRightForContainers, @hasBookingsRightForContainers, @hasCFSShipmentsRightForContainers, @hasLinerAndAgencyBookingForContainers, @addressListForContainers, @orgListForContainers)"))
			{
				command.AddParameter("@loggedInOrganisationForContainers", SqlDbType.UniqueIdentifier, loggedInOrganisation);

				command.AddParameter("@hasShipmentsRightForContainers", SqlDbType.Bit, true);
				command.AddParameter("@hasBookingsRightForContainers", SqlDbType.Bit, true);
				command.AddParameter("@hasCFSShipmentsRightForContainers", SqlDbType.Bit, true);
				command.AddParameter("@hasLinerAndAgencyBookingForContainers", SqlDbType.Bit, true);

				command.AddParameter("@addressListForContainers", SqlDbType.NVarChar, string.Join(",", relatedAddresses));
				command.AddParameter("@orgListForContainers", SqlDbType.NVarChar, string.Join(",", relatedOrgs));

				var data = DataUtils.GetDataTableFromCommand(command);

				return data.AsEnumerable().Select(row => (Guid)row[JobContainerSchema.Constants.PK]);
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

		protected override void SetUp()
		{
			base.SetUp();

			containerCounter = 0;
			shipmentsWithSecurity = new GetJobShipmentsWithSecurityContextTest();
			cartagesWithSecurity = new GetJobCartagesWithSecurityContextTest();
		}

		int containerCounter;
		GetJobShipmentsWithSecurityContextTest shipmentsWithSecurity;
		GetJobCartagesWithSecurityContextTest cartagesWithSecurity;
	}
}
