using System;
using System.Linq;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.RulesEngine.Facts;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.Testing.JobInvoicing
{
	public class ShipmentJobFactLoaderTest : FactLoaderBaseTest<ShipmentJobFact>
	{
		public void TestGetFacts_IJobInvoicingSupporter()
		{
			var loader = GetFactLoader(RulesContextType.DummyForTesting);

			var country = Factory.NewWithValidTestData<RefCountry>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = country.Code;

			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var salesRep = Factory.NewWithValidTestData<GlbStaff>();
			salesRep.GS_GE_HomeDepartment = department.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GS_NKRepSales = salesRep.GS_Code;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var origin = Factory.NewWithValidTestData<RefUNLOCO>();
			origin.RL_RN_NKCountryCode = country.Code;
			invoicingSupporterMock.Setup(x => x.Origin).Returns(origin);

			var destination = Factory.NewWithValidTestData<RefUNLOCO>();
			destination.RL_RN_NKCountryCode = country.Code;
			invoicingSupporterMock.Setup(x => x.Destination).Returns(destination);
			invoicingSupporterMock.Setup(x => x.Consignee).Returns(orgHeader);
			invoicingSupporterMock.Setup(x => x.Consignor).Returns(orgHeader);
			invoicingSupporterMock.Setup(x => x.ControllingAgent).Returns(orgHeader);
			invoicingSupporterMock.Setup(x => x.ControllingCustomer).Returns(orgHeader);
			invoicingSupporterMock.Setup(x => x.PickUpAgent).Returns(orgHeader);
			invoicingSupporterMock.Setup(x => x.DeliveryAgent).Returns(orgHeader);
			invoicingSupporterMock.Setup(x => x.SendingAgent).Returns(orgHeader);
			invoicingSupporterMock.Setup(x => x.GetOrganisationByBranchDefaultingRule(Core.Constants.ChargeCodeBranchDefaultingRule.SendingAgent)).Returns(orgHeader);
			invoicingSupporterMock.Setup(x => x.GetOrganisationByBranchDefaultingRule(Core.Constants.ChargeCodeBranchDefaultingRule.ReceivingAgent)).Returns(orgHeader);

			var result = GetFacts(loader, invoicingSupporterMock.Object);
			var nestedFacts = GetNestedFacts(result);
			AssertEquals("Total facts", 10, result.Concat(nestedFacts).Count());
			AssertEquals("2 UNLOCOFact", 2, nestedFacts.Count(x => x.GetType() == typeof(UNLOCOFact)));
			AssertEquals("1 OrganisationWithMainAddressFact", 1, nestedFacts.Count(x => x.GetType() == typeof(OrganisationWithMainAddressFact)));
			AssertEquals("1 ShipmentJobFact", 1, result.Count(x => x.GetType() == typeof(ShipmentJobFact)));
			AssertEquals("1 AddressFact", 1, nestedFacts.Count(x => x.GetType() == typeof(AddressFact)));
			AssertEquals("1 CountryFact", 1, nestedFacts.Count(x => x.GetType() == typeof(CountryFact)));
			AssertEquals("1 StaffFact", 1, nestedFacts.Count(x => x.GetType() == typeof(StaffFact)));
			AssertEquals("1 DepartmentFact", 1, nestedFacts.Count(x => x is DepartmentFact dept && dept.Code != "LOGINDEPT"));
			AssertEquals("1 Login DepartmentFact", 1, nestedFacts.Count(x => x is DepartmentFact dept && dept.Code == "LOGINDEPT"));
			AssertEquals("1 Login BranchFact", 1, nestedFacts.Count(x => x.GetType() == typeof(BranchFact)));

			ShipmentJobFact shipmentFact = (ShipmentJobFact)result.First(x => x.GetType() == typeof(ShipmentJobFact));

			AssertUnlocoFactAndChildren("Origin Fact should be populated", shipmentFact.Origin.Fact, nestedFacts);
			AssertEquals(origin.Country.Code, shipmentFact.OriginCountry);

			AssertUnlocoFactAndChildren("Destination Fact should be populated", shipmentFact.Destination.Fact, nestedFacts);
			AssertEquals(destination.Country.Code, shipmentFact.DestinationCountry);

			AssertOrganizationFactAndChildren("Consignee Fact should be populated", shipmentFact.Consignee.Fact, nestedFacts);
			AssertOrganizationFactAndChildren("Consignor Fact should be populated", shipmentFact.Consignor.Fact, nestedFacts);
			AssertOrganizationFactAndChildren("ControllingAgent Fact should be populated", shipmentFact.ControllingAgent.Fact, nestedFacts);
			AssertOrganizationFactAndChildren("ControllingCustomer Fact should be populated", shipmentFact.ControllingCustomer.Fact, nestedFacts);
			AssertOrganizationFactAndChildren("PickupAgent Fact should be populated", shipmentFact.PickupAgent.Fact, nestedFacts);
			AssertOrganizationFactAndChildren("DeliveryAgent Fact should be populated", shipmentFact.DeliveryAgent.Fact, nestedFacts);

			loader = GetFactLoader(RulesContextType.JobBillingTaxBranchDefaulting);
			result = GetFacts(loader, invoicingSupporterMock.Object);
			nestedFacts = GetNestedFacts(result);

			AssertEquals("1 ShipmentJobForTaxBranchFact", 1, result.Count(x => x.GetType() == typeof(ShipmentJobForTaxBranchFact)));

			ShipmentJobForTaxBranchFact shipmentForTaxBranchFact = (ShipmentJobForTaxBranchFact)result.First(x => x.GetType() == typeof(ShipmentJobForTaxBranchFact));

			AssertOrganizationFactAndChildren("SendingAgent Fact should be populated", shipmentForTaxBranchFact.ConsolSendingAgent.Fact, nestedFacts);
			AssertOrganizationFactAndChildren("ReceivingAgent Fact should be populated", shipmentForTaxBranchFact.ConsolReceivingAgent.Fact, nestedFacts);
		}

		public void TestGetFacts_ForwardingShipment()
		{
			var countryCode = "AU";
			var loader = new ShipmentJobFactLoader(RulesContextType.DummyForTesting);

			var localTransportOrg = Factory.New<OrgHeader>();
			localTransportOrg.MainAddress.OA_RN_NKCountryCode = countryCode;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = localTransportOrg.MainAddress.PK;
			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = localTransportOrg.MainAddress.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;

			var result = GetFacts(loader, shipment);
			var nestedFacts = GetNestedFacts(result);
			AssertEquals("Total facts", 6, result.Concat(nestedFacts).Count());
			AssertEquals("1 OrganisationWithMainAddressFact", 1, nestedFacts.Count(x => x.GetType() == typeof(OrganisationWithMainAddressFact)));
			AssertEquals("1 ShipmentJobFact", 1, result.Count(x => x.GetType() == typeof(ShipmentJobFact)));
			AssertEquals("1 AddressFact", 1, nestedFacts.Count(x => x.GetType() == typeof(AddressFact)));
			AssertEquals("1 CountryFact", 1, nestedFacts.Count(x => x.GetType() == typeof(CountryFact)));
			AssertEquals("1 Login DepartmentFact", 1, nestedFacts.Count(x => x.GetType() == typeof(DepartmentFact)));
			AssertEquals("1 Login BranchFact", 1, nestedFacts.Count(x => x.GetType() == typeof(BranchFact)));

			ShipmentJobFact shipmentFact = (ShipmentJobFact)result.First(x => x.GetType() == typeof(ShipmentJobFact));

			AssertOrganizationFactAndChildren("PickupLocalTransport Fact should be populated", shipmentFact.PickupLocalTransport.Fact, nestedFacts);
			AssertOrganizationFactAndChildren("DeliveryLocalTransport Fact should be populated", shipmentFact.DeliveryLocalTransport.Fact, nestedFacts);
		}

		public void TestGetFacts_Consignee_IsOrgProxyOfLoginCompany()
		{
			TestGetFacts_IsOrgProxyOfLoginCompany(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.Consignee).Returns(org),
				(shipmentFact) => shipmentFact.Consignee.Fact);
		}

		public void TestGetFacts_Consignor_IsOrgProxyOfLoginCompany()
		{
			TestGetFacts_IsOrgProxyOfLoginCompany(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.Consignor).Returns(org),
				(shipmentFact) => shipmentFact.Consignor.Fact);
		}

		public void TestGetFacts_ControllingAgent_IsOrgProxyOfLoginCompany()
		{
			TestGetFacts_IsOrgProxyOfLoginCompany(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.ControllingAgent).Returns(org),
				(shipmentFact) => shipmentFact.ControllingAgent.Fact);
		}

		public void TestGetFacts_ControllingCustomer_IsOrgProxyOfLoginCompany()
		{
			TestGetFacts_IsOrgProxyOfLoginCompany(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.ControllingCustomer).Returns(org),
				(shipmentFact) => shipmentFact.ControllingCustomer.Fact);
		}

		public void TestGetFacts_PickupAgent_IsOrgProxyOfLoginCompany()
		{
			TestGetFacts_IsOrgProxyOfLoginCompany(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.PickUpAgent).Returns(org),
				(shipmentFact) => shipmentFact.PickupAgent.Fact);
		}

		public void TestGetFacts_DeliveryAgent_IsOrgProxyOfLoginCompany()
		{
			TestGetFacts_IsOrgProxyOfLoginCompany(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.DeliveryAgent).Returns(org),
				(shipmentFact) => shipmentFact.DeliveryAgent.Fact);
		}

		void TestGetFacts_IsOrgProxyOfLoginCompany(Action<Mock<IJobInvoicingSupporter>, OrgHeader> invoicingSupporterMockSetup, Func<ShipmentJobFact, IOrganisationWithMainAddressFact> getFact)
		{
			var loader = GetFactLoader(RulesContextType.DummyForTesting);

			var country = Factory.NewWithValidTestData<RefCountry>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = country.Code;

			var company = GlbCompany.CurrentCompany;
			company.GC_OH_OrgProxy = orgHeader.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var jobPluginMock = new Mock<IJobInvoicingPlugIn>();
			jobPluginMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			invoicingSupporterMockSetup(invoicingSupporterMock, orgHeader);
			invoicingSupporterMock.Setup(x => x.SendingAgent).Returns(orgHeader);
			var result = GetFacts(loader, invoicingSupporterMock.Object);
			var nestedFacts = GetNestedFacts(result);
			var shipmentFact = (ShipmentJobFact)result.First(x => x.GetType() == typeof(ShipmentJobFact));

			var orgWithAddressFact = getFact(shipmentFact);
			AssertOrganizationFactAndChildren("OrgWithAddress Fact should be populated", orgWithAddressFact, nestedFacts);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfCurrentCompany), true, orgWithAddressFact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfAnyCompany), true, orgWithAddressFact.IsProxyOrgOfAnyCompany);
		}

		public void TestGetFacts_Consignee_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy()
		{
			TestGetFacts_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.Consignee).Returns(org),
				(shipmentFact) => shipmentFact.Consignee.Fact);
		}

		public void TestGetFacts_Consignor_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy()
		{
			TestGetFacts_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.Consignor).Returns(org),
				(shipmentFact) => shipmentFact.Consignor.Fact);
		}

		public void TestGetFacts_ControllingAgent_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy()
		{
			TestGetFacts_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.ControllingAgent).Returns(org),
				(shipmentFact) => shipmentFact.ControllingAgent.Fact);
		}

		public void TestGetFacts_ControllingCustomer_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy()
		{
			TestGetFacts_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.ControllingCustomer).Returns(org),
				(shipmentFact) => shipmentFact.ControllingCustomer.Fact);
		}

		public void TestGetFacts_PickupAgent_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy()
		{
			TestGetFacts_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.PickUpAgent).Returns(org),
				(shipmentFact) => shipmentFact.PickupAgent.Fact);
		}

		public void TestGetFacts_DeliveryAgent_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy()
		{
			TestGetFacts_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.DeliveryAgent).Returns(org),
				(shipmentFact) => shipmentFact.DeliveryAgent.Fact);
		}

		void TestGetFacts_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy(Action<Mock<IJobInvoicingSupporter>, OrgHeader> invoicingSupporterMockSetup, Func<ShipmentJobFact, IOrganisationWithMainAddressFact> getFact)
		{
			var loader = GetFactLoader(RulesContextType.DummyForTesting);

			var country = Factory.NewWithValidTestData<RefCountry>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = country.Code;

			var company = GlbCompany.CurrentCompany;
			var branch = company.FirstActiveBranch;
			branch.GB_OH_OrgProxy = orgHeader.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var jobPluginMock = new Mock<IJobInvoicingPlugIn>();
			jobPluginMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			invoicingSupporterMockSetup(invoicingSupporterMock, orgHeader);
			var result = GetFacts(loader, invoicingSupporterMock.Object);
			var nestedFacts = GetNestedFacts(result);
			var shipmentFact = (ShipmentJobFact)result.First(x => x.GetType() == typeof(ShipmentJobFact));

			var orgWithAddressFact = getFact(shipmentFact);
			AssertOrganizationFactAndChildren("OrgWithAddress Fact should be populated", orgWithAddressFact, nestedFacts);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfCurrentCompany), true, orgWithAddressFact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfAnyCompany), true, orgWithAddressFact.IsProxyOrgOfAnyCompany);
		}

		public void TestGetFacts_Consignee_IsOrgProxyOfAnotherCompany()
		{
			TestGetFacts_IsOrgProxyOfAnotherCompany(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.Consignee).Returns(org),
				(shipmentFact) => shipmentFact.Consignee.Fact);
		}

		public void TestGetFacts_Consignor_IsOrgProxyOfAnotherCompany()
		{
			TestGetFacts_IsOrgProxyOfAnotherCompany(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.Consignor).Returns(org),
				(shipmentFact) => shipmentFact.Consignor.Fact);
		}

		public void TestGetFacts_ControllingAgent_IsOrgProxyOfAnotherCompany()
		{
			TestGetFacts_IsOrgProxyOfAnotherCompany(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.ControllingAgent).Returns(org),
				(shipmentFact) => shipmentFact.ControllingAgent.Fact);
		}

		public void TestGetFacts_ControllingCustomer_IsOrgProxyOfAnotherCompany()
		{
			TestGetFacts_IsOrgProxyOfAnotherCompany(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.ControllingCustomer).Returns(org),
				(shipmentFact) => shipmentFact.ControllingCustomer.Fact);
		}

		public void TestGetFacts_PickupAgent_IsOrgProxyOfAnotherCompany()
		{
			TestGetFacts_IsOrgProxyOfAnotherCompany(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.PickUpAgent).Returns(org),
				(shipmentFact) => shipmentFact.PickupAgent.Fact);
		}

		public void TestGetFacts_DeliveryAgent_IsOrgProxyOfAnotherCompany()
		{
			TestGetFacts_IsOrgProxyOfAnotherCompany(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.DeliveryAgent).Returns(org),
				(shipmentFact) => shipmentFact.DeliveryAgent.Fact);
		}

		void TestGetFacts_IsOrgProxyOfAnotherCompany(Action<Mock<IJobInvoicingSupporter>, OrgHeader> invoicingSupporterMockSetup, Func<ShipmentJobFact, IOrganisationWithMainAddressFact> getFact)
		{
			var loader = GetFactLoader(RulesContextType.DummyForTesting);

			var country = Factory.NewWithValidTestData<RefCountry>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = country.Code;

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = orgHeader.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var jobPluginMock = new Mock<IJobInvoicingPlugIn>();
			jobPluginMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			invoicingSupporterMockSetup(invoicingSupporterMock, orgHeader);
			var result = GetFacts(loader, invoicingSupporterMock.Object);
			var nestedFacts = GetNestedFacts(result);
			var shipmentFact = (ShipmentJobFact)result.First(x => x.GetType() == typeof(ShipmentJobFact));

			var orgWithAddressFact = getFact(shipmentFact);
			AssertOrganizationFactAndChildren("OrgWithAddress Fact should be populated", orgWithAddressFact, nestedFacts);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfCurrentCompany), false, orgWithAddressFact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfAnyCompany), true, orgWithAddressFact.IsProxyOrgOfAnyCompany);
		}

		public void TestGetFacts_Consignee_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy()
		{
			TestGetFacts_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.Consignee).Returns(org),
				(shipmentFact) => shipmentFact.Consignee.Fact);
		}

		public void TestGetFacts_Consignor_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy()
		{
			TestGetFacts_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.Consignor).Returns(org),
				(shipmentFact) => shipmentFact.Consignor.Fact);
		}

		public void TestGetFacts_ControllingAgent_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy()
		{
			TestGetFacts_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.ControllingAgent).Returns(org),
				(shipmentFact) => shipmentFact.ControllingAgent.Fact);
		}

		public void TestGetFacts_ControllingCustomer_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy()
		{
			TestGetFacts_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.ControllingCustomer).Returns(org),
				(shipmentFact) => shipmentFact.ControllingCustomer.Fact);
		}

		public void TestGetFacts_PickupAgent_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy()
		{
			TestGetFacts_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.PickUpAgent).Returns(org),
				(shipmentFact) => shipmentFact.PickupAgent.Fact);
		}

		public void TestGetFacts_DeliveryAgent_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy()
		{
			TestGetFacts_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy(
				(jobInvoicingSupporterMock, org) => jobInvoicingSupporterMock.Setup(x => x.DeliveryAgent).Returns(org),
				(shipmentFact) => shipmentFact.DeliveryAgent.Fact);
		}

		void TestGetFacts_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy(Action<Mock<IJobInvoicingSupporter>, OrgHeader> invoicingSupporterMockSetup, Func<ShipmentJobFact, IOrganisationWithMainAddressFact> getFact)
		{
			var loader = GetFactLoader(RulesContextType.DummyForTesting);

			var country = Factory.NewWithValidTestData<RefCountry>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = country.Code;

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_OH_OrgProxy = orgHeader.PK;
			branch.GB_GC = company.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var jobPluginMock = new Mock<IJobInvoicingPlugIn>();
			jobPluginMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			invoicingSupporterMockSetup(invoicingSupporterMock, orgHeader);
			var result = GetFacts(loader, invoicingSupporterMock.Object);
			var nestedFacts = GetNestedFacts(result);
			var shipmentFact = (ShipmentJobFact)result.First(x => x.GetType() == typeof(ShipmentJobFact));

			var orgWithAddressFact = getFact(shipmentFact);
			AssertOrganizationFactAndChildren("OrgWithAddress Fact should be populated", orgWithAddressFact, nestedFacts);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfCurrentCompany), false, orgWithAddressFact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfAnyCompany), true, orgWithAddressFact.IsProxyOrgOfAnyCompany);
		}

		public void TestGetFacts_ForwardingShipment_PickupLocalTransport_IsOrgProxyOfLoginCompany()
		{
			TestGetFacts_ForwardingShipment_IsOrgProxyOfLoginCompany(
				(shipment, org) => shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = org.MainAddress.PK,
				(shipmentFact) => shipmentFact.PickupLocalTransport.Fact);
		}

		public void TestGetFacts_ForwardingShipment_DeliveryLocalTransport_IsOrgProxyOfLoginCompany()
		{
			TestGetFacts_ForwardingShipment_IsOrgProxyOfLoginCompany(
				(shipment, org) => shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = org.MainAddress.PK,
				(shipmentFact) => shipmentFact.DeliveryLocalTransport.Fact);
		}

		void TestGetFacts_ForwardingShipment_IsOrgProxyOfLoginCompany(Action<ForwardingShipment, OrgHeader> shipmentSetter, Func<ShipmentJobFact, IOrganisationWithMainAddressFact> getFact)
		{
			var countryCode = "AU";
			var loader = new ShipmentJobFactLoader(RulesContextType.DummyForTesting);

			var localTransportOrg = Factory.New<OrgHeader>();
			localTransportOrg.MainAddress.OA_RN_NKCountryCode = countryCode;

			var company = GlbCompany.CurrentCompany;
			company.GC_OH_OrgProxy = localTransportOrg.PK;

			var shipment = Factory.New<ForwardingShipment>();
			shipmentSetter(shipment, localTransportOrg);

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;

			var result = GetFacts(loader, shipment);
			var nestedFacts = GetNestedFacts(result);
			var shipmentFact = (ShipmentJobFact)result.First(x => x.GetType() == typeof(ShipmentJobFact));

			var fact = getFact(shipmentFact);
			AssertOrganizationFactAndChildren("PickupLocalTransport/DeliveryLocalTransport Fact should be populated", fact, nestedFacts);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfCurrentCompany), true, fact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfAnyCompany), true, fact.IsProxyOrgOfAnyCompany);
		}

		public void TestGetFacts_ForwardingShipment_PickupLocalTransport_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy()
		{
			TestGetFacts_ForwardingShipment_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy(
				(shipment, org) => shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = org.MainAddress.PK,
				(shipmentFact) => shipmentFact.PickupLocalTransport.Fact);
		}

		public void TestGetFacts_ForwardingShipment_DeliveryLocalTransport_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy()
		{
			TestGetFacts_ForwardingShipment_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy(
				(shipment, org) => shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = org.MainAddress.PK,
				(shipmentFact) => shipmentFact.DeliveryLocalTransport.Fact);
		}

		void TestGetFacts_ForwardingShipment_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy(Action<ForwardingShipment, OrgHeader> shipmentSetter, Func<ShipmentJobFact, IOrganisationWithMainAddressFact> getFact)
		{
			var countryCode = "AU";
			var loader = new ShipmentJobFactLoader(RulesContextType.DummyForTesting);

			var localTransportOrg = Factory.New<OrgHeader>();
			localTransportOrg.MainAddress.OA_RN_NKCountryCode = countryCode;

			var company = GlbCompany.CurrentCompany;
			var branch = company.FirstActiveBranch;
			branch.GB_OH_OrgProxy = localTransportOrg.PK;

			var shipment = Factory.New<ForwardingShipment>();
			shipmentSetter(shipment, localTransportOrg);

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;

			var result = GetFacts(loader, shipment);
			var nestedFacts = GetNestedFacts(result);
			var shipmentFact = (ShipmentJobFact)result.First(x => x.GetType() == typeof(ShipmentJobFact));

			var fact = getFact(shipmentFact);
			AssertOrganizationFactAndChildren("PickupLocalTransport/DeliveryLocalTransport Fact should be populated", fact, nestedFacts);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfCurrentCompany), true, fact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfAnyCompany), true, fact.IsProxyOrgOfAnyCompany);
		}

		public void TestGetFacts_ForwardingShipment_PickupLocalTransport_IsOrgProxyOfAnotherCompany()
		{
			TestGetFacts_ForwardingShipment_IsOrgProxyOfAnotherCompany(
				(shipment, org) => shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = org.MainAddress.PK,
				(shipmentFact) => shipmentFact.PickupLocalTransport.Fact);
		}

		public void TestGetFacts_ForwardingShipment_DeliveryLocalTransport_IsOrgProxyOfAnotherCompany()
		{
			TestGetFacts_ForwardingShipment_IsOrgProxyOfAnotherCompany(
				(shipment, org) => shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = org.MainAddress.PK,
				(shipmentFact) => shipmentFact.DeliveryLocalTransport.Fact);
		}

		void TestGetFacts_ForwardingShipment_IsOrgProxyOfAnotherCompany(Action<ForwardingShipment, OrgHeader> shipmentSetter, Func<ShipmentJobFact, IOrganisationWithMainAddressFact> getFact)
		{
			var countryCode = "AU";
			var loader = new ShipmentJobFactLoader(RulesContextType.DummyForTesting);

			var localTransportOrg = Factory.New<OrgHeader>();
			localTransportOrg.MainAddress.OA_RN_NKCountryCode = countryCode;

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = localTransportOrg.PK;

			var shipment = Factory.New<ForwardingShipment>();
			shipmentSetter(shipment, localTransportOrg);

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;

			var result = GetFacts(loader, shipment);
			var nestedFacts = GetNestedFacts(result);
			var shipmentFact = (ShipmentJobFact)result.First(x => x.GetType() == typeof(ShipmentJobFact));

			var fact = getFact(shipmentFact);
			AssertOrganizationFactAndChildren("PickupLocalTransport/DeliveryLocalTransport Fact should be populated", fact, nestedFacts);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfCurrentCompany), false, fact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfAnyCompany), true, fact.IsProxyOrgOfAnyCompany);
		}

		public void TestGetFacts_ForwardingShipment_PickupLocalTransport_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy()
		{
			TestGetFacts_ForwardingShipment_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy(
				(shipment, org) => shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = org.MainAddress.PK,
				(shipmentFact) => shipmentFact.PickupLocalTransport.Fact);
		}

		public void TestGetFacts_ForwardingShipment_DeliveryLocalTransport_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy()
		{
			TestGetFacts_ForwardingShipment_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy(
				(shipment, org) => shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = org.MainAddress.PK,
				(shipmentFact) => shipmentFact.DeliveryLocalTransport.Fact);
		}

		void TestGetFacts_ForwardingShipment_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy(Action<ForwardingShipment, OrgHeader> shipmentSetter, Func<ShipmentJobFact, IOrganisationWithMainAddressFact> getFact)
		{
			var countryCode = "AU";
			var loader = new ShipmentJobFactLoader(RulesContextType.DummyForTesting);

			var localTransportOrg = Factory.New<OrgHeader>();
			localTransportOrg.MainAddress.OA_RN_NKCountryCode = countryCode;

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_OH_OrgProxy = localTransportOrg.PK;
			branch.GB_GC = company.PK;

			var shipment = Factory.New<ForwardingShipment>();
			shipmentSetter(shipment, localTransportOrg);

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;

			var result = GetFacts(loader, shipment);
			var nestedFacts = GetNestedFacts(result);
			var shipmentFact = (ShipmentJobFact)result.First(x => x.GetType() == typeof(ShipmentJobFact));

			var fact = getFact(shipmentFact);
			AssertOrganizationFactAndChildren("PickupLocalTransport/DeliveryLocalTransport Fact should be populated", fact, nestedFacts);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfCurrentCompany), false, fact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfAnyCompany), true, fact.IsProxyOrgOfAnyCompany);
		}

		protected override Type JobTypeForTaxBranchFact => typeof(ShipmentJobForTaxBranchFact);

		protected override IFactLoader GetFactLoader(RulesContextType contextType)
		{
			return new ShipmentJobFactLoader(contextType);
		}
	}
}
