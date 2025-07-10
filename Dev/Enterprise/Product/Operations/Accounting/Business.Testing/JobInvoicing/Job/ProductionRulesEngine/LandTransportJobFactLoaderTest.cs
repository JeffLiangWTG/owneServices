using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.RulesEngine.Facts;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.Testing.JobInvoicing
{
	public class LandTransportJobFactLoaderTest : TestCaseWithFactory
	{
		public void TestJobRelatedFacts()
		{
			var loader = GetFactLoader(RulesContextType.DummyForTesting);

			var country = Factory.NewWithValidTestData<RefCountry>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = country.Code;

			var salesRep = Factory.NewWithValidTestData<GlbStaff>();
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_OA_LocalChargesAddr = orgHeader.MainAddress.PK;
			job.JH_GS_NKRepSales = salesRep.GS_Code;

			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var result = GetFacts(loader, invoicingSupporterMock.Object);
			var nestedFacts = GetNestedFacts(result);
			Assert(result.Any());
			var lastFact = result.Last();
			AssertType<LandTransportJobFact>(lastFact);
			var jobFact = lastFact as JobFact;

			AssertOrganizationFactAndChildren("LocalClient Fact should be populated", jobFact.LocalClient.Fact, nestedFacts);
			AssertStaffFactAndChildren("SalesRep Fact should be populated", jobFact.SalesRep.Fact, nestedFacts);
		}

		public void TestOrganizationFact_IsOrgProxyOfLoginCompany()
		{
			var loader = GetFactLoader(RulesContextType.DummyForTesting);
			var country = Factory.NewWithValidTestData<RefCountry>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = country.Code;
			var company = GlbCompany.CurrentCompany;
			company.GC_OH_OrgProxy = orgHeader.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_OA_LocalChargesAddr = orgHeader.MainAddress.PK;

			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var jobPluginMock = new Mock<IJobInvoicingPlugIn>();
			jobPluginMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var branchMock = new Mock<IBranch>();
			branchMock.Setup(b => b.Code).Returns("LOGINBRANCH");
			var departmentMock = new Mock<IDepartment>();
			departmentMock.Setup(b => b.Code).Returns("LOGINDEPT");

			var result = loader.GetFacts(jobPluginMock.Object, company, branchMock.Object, departmentMock.Object);
			var nestedFacts = GetNestedFacts(result);
			Assert(result.Any());
			var lastFact = result.Last();
			AssertType<LandTransportJobFact>(lastFact);
			var jobFact = lastFact as JobFact;

			var orgFact = jobFact.LocalClient.Fact;
			AssertOrganizationFactAndChildren("LocalClient Fact should be populated", orgFact, nestedFacts);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfCurrentCompany), true, orgFact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfAnyCompany), true, orgFact.IsProxyOrgOfAnyCompany);
		}

		public void TestOrganizationFact_IsOrgProxyOfLoginCompany_CompanyBranchOrgProxy()
		{
			var loader = GetFactLoader(RulesContextType.DummyForTesting);
			var country = Factory.NewWithValidTestData<RefCountry>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = country.Code;
			var company = GlbCompany.CurrentCompany;
			var branch = company.FirstActiveBranch;
			branch.GB_OH_OrgProxy = orgHeader.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_OA_LocalChargesAddr = orgHeader.MainAddress.PK;

			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var jobPluginMock = new Mock<IJobInvoicingPlugIn>();
			jobPluginMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var branchMock = new Mock<IBranch>();
			branchMock.Setup(b => b.Code).Returns("LOGINBRANCH");
			var departmentMock = new Mock<IDepartment>();
			departmentMock.Setup(b => b.Code).Returns("LOGINDEPT");

			var result = loader.GetFacts(jobPluginMock.Object, company, branchMock.Object, departmentMock.Object);
			var nestedFacts = GetNestedFacts(result);
			Assert(result.Any());
			var lastFact = result.Last();
			AssertType<LandTransportJobFact>(lastFact);
			var jobFact = lastFact as JobFact;

			var orgFact = jobFact.LocalClient.Fact;
			AssertOrganizationFactAndChildren("LocalClient Fact should be populated", orgFact, nestedFacts);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfCurrentCompany), true, orgFact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfAnyCompany), true, orgFact.IsProxyOrgOfAnyCompany);
		}

		public void TestOrganizationFact_IsOrgProxyOfAnotherCompany()
		{
			var loader = GetFactLoader(RulesContextType.DummyForTesting);
			var country = Factory.NewWithValidTestData<RefCountry>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = country.Code;
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = orgHeader.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_OA_LocalChargesAddr = orgHeader.MainAddress.PK;

			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var result = GetFacts(loader, invoicingSupporterMock.Object);
			var nestedFacts = GetNestedFacts(result);
			Assert(result.Any());
			var lastFact = result.Last();
			AssertType<LandTransportJobFact>(lastFact);
			var jobFact = lastFact as JobFact;

			var orgFact = jobFact.LocalClient.Fact;
			AssertOrganizationFactAndChildren("LocalClient Fact should be populated", orgFact, nestedFacts);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfCurrentCompany), false, orgFact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfAnyCompany), true, orgFact.IsProxyOrgOfAnyCompany);
		}

		public void TestOrganizationFact_IsOrgProxyOfAnotherCompany_CompanyBranchOrgProxy()
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
			job.JH_OA_LocalChargesAddr = orgHeader.MainAddress.PK;

			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var result = GetFacts(loader, invoicingSupporterMock.Object);
			var nestedFacts = GetNestedFacts(result);
			Assert(result.Any());
			var lastFact = result.Last();
			AssertType<LandTransportJobFact>(lastFact);
			var jobFact = lastFact as JobFact;

			var orgFact = jobFact.LocalClient.Fact;
			AssertOrganizationFactAndChildren("LocalClient Fact should be populated", orgFact, nestedFacts);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfCurrentCompany), false, orgFact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationWithMainAddressFact.IsProxyOrgOfAnyCompany), true, orgFact.IsProxyOrgOfAnyCompany);
		}

		public void TestGetFactsCore_ShouldCreateWarehouseFact_WhenParentOrderIsWarehouseOrder()
		{
			var warehouseOrder = Factory.NewWithValidTestData<WhsOrder>();
			var bookingConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			bookingConsolidation.KB_ParentID = warehouseOrder.PK;
			bookingConsolidation.KB_ParentTableCode = "WD";
			var transportBooking = Factory.NewWithValidTestData<DtbBooking>();
			transportBooking.KM_KB_Booking = bookingConsolidation.PK;
			var consignment = Factory.NewWithValidTestData<DtbConsignment>();
			consignment.LTC_KM_Booking = transportBooking.PK;
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = consignment.PK;
			var loader = GetFactLoader(RulesContextType.DummyForTesting);

			var result = GetFacts(loader, consignment);

			Assert(result.Any());
			var lastFact = result.Last();
			AssertType<LandTransportJobFact>(lastFact);

			var landTransportJobFact = lastFact as LandTransportJobFact;

			AssertNull(landTransportJobFact.ParentForwardingShipment.Fact);
			AssertNotNull(landTransportJobFact.ParentWarehouseOrder.Fact);
		}

		public void TestGetFactsCore_ShouldCreateShipmentFact_WhenParentOrderIsForwardingShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var bookingConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			bookingConsolidation.KB_ParentID = shipment.PK;
			bookingConsolidation.KB_ParentTableCode = "JS";
			var transportBooking = Factory.NewWithValidTestData<DtbBooking>();
			transportBooking.KM_KB_Booking = bookingConsolidation.PK;
			var consignment = Factory.NewWithValidTestData<DtbConsignment>();
			consignment.LTC_KM_Booking = transportBooking.PK;
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = consignment.PK;
			
			var loader = GetFactLoader(RulesContextType.DummyForTesting);

			var result = GetFacts(loader, consignment);

			Assert(result.Any());
			var lastFact = result.Last();
			AssertType<LandTransportJobFact>(lastFact);

			var landTransportJobFact = lastFact as LandTransportJobFact;

			AssertNotNull(landTransportJobFact.ParentForwardingShipment.Fact);
			AssertNull(landTransportJobFact.ParentWarehouseOrder.Fact);
		}

		public void TestGetFactsCore_ShouldNotCreateShipmentFactOrWarehouseFact_WhenParentOrderDoesNotExist()
		{
			var consignment = Factory.NewWithValidTestData<DtbConsignment>();
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = consignment.PK;
			var loader = GetFactLoader(RulesContextType.DummyForTesting);

			var result = GetFacts(loader, consignment);

			Assert(result.Any());
			var lastFact = result.Last();
			AssertType<LandTransportJobFact>(lastFact);

			var landTransportJobFact = lastFact as LandTransportJobFact;

			AssertNull(landTransportJobFact.ParentForwardingShipment.Fact);
			AssertNull(landTransportJobFact.ParentWarehouseOrder.Fact);
		}

		public void TestGetFactsCore_ShouldNotCreateShipmentFactOrWarehouseFact_WhenParentOrderIsNotWarehouseOrderOrShipment()
		{
			var shipment = Factory.NewWithValidTestData<WhsReceive>();
			var bookingConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			bookingConsolidation.KB_ParentID = shipment.PK;
			bookingConsolidation.KB_ParentTableCode = "WD";
			var transportBooking = Factory.NewWithValidTestData<DtbBooking>();
			transportBooking.KM_KB_Booking = bookingConsolidation.PK;
			var consignment = Factory.NewWithValidTestData<DtbConsignment>();
			consignment.LTC_KM_Booking = transportBooking.PK;
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = consignment.PK;
			var loader = GetFactLoader(RulesContextType.DummyForTesting);

			var result = GetFacts(loader, consignment);

			Assert(result.Any());
			var lastFact = result.Last();
			AssertType<LandTransportJobFact>(lastFact);

			var landTransportJobFact = lastFact as LandTransportJobFact;

			AssertNull(landTransportJobFact.ParentForwardingShipment.Fact);
			AssertNull(landTransportJobFact.ParentWarehouseOrder.Fact);
		}

		#region  GetFacts

		protected IEnumerable<IInputFact> GetFacts(IFactLoader loader, IJobInvoicingSupporter invoicingSupporterMock)
		{
			var jobPluginMock = new Mock<IJobInvoicingPlugIn>();
			jobPluginMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock);

			return GetFacts(loader, jobPluginMock.Object);
		}

		protected IEnumerable<IInputFact> GetFacts(IFactLoader loader, IJobInvoicingPlugIn jobInvoicingPlugIn)
		{
			var countryMock = new Mock<ICountry>();
			var companyMock = new Mock<ICompany>();
			companyMock.Setup(x => x.Country).Returns(countryMock.Object);
			var branchMock = new Mock<IBranch>();
			branchMock.Setup(b => b.Code).Returns("LOGINBRANCH");
			var departmentMock = new Mock<IDepartment>();
			departmentMock.Setup(b => b.Code).Returns("LOGINDEPT");

			return loader.GetFacts(jobInvoicingPlugIn, companyMock.Object, branchMock.Object, departmentMock.Object);
		}

		#endregion

		protected IEnumerable<IInputFact> GetNestedFacts(IEnumerable<IInputFact> topLevelFacts)
		{
			var finder = new NestedFactsFinder();
			var queue = new Queue<IInputFact>(topLevelFacts);
			var result = new HashSet<IInputFact>();
			while (queue.Count > 0)
			{
				var fact = queue.Dequeue();
				var nestedFacts = finder.GetDirectNestedInputFacts(fact);
				foreach (var nestedFact in nestedFacts)
				{
					if (result.Add(nestedFact.Fact))
					{
						queue.Enqueue(nestedFact.Fact);
					}
				}
			}

			return result;
		}

		protected void AssertFactJoin<T>(string message, IInputFact fact, IEnumerable<IInputFact> facts)
		{
			AssertNotNull(message, fact);
			AssertType<T>(fact);
			AssertCollectionContains(fact, facts);
		}

		protected void AssertOrganizationFactAndChildren(string message, IOrganisationWithMainAddressFact orgFact, IEnumerable<IInputFact> facts)
		{
			AssertFactJoin<OrganisationWithMainAddressFact>(message, orgFact, facts);
			AssertFactJoin<AddressFact>("MainAddress should be populated", orgFact.MainAddress.Fact, facts);
			AssertFactJoin<CountryFact>("MainAddress.Country should be populated", orgFact.MainAddress.Fact.Country.Fact, facts);
		}

		protected void AssertUnlocoFactAndChildren(string message, IUNLOCOFact unlocoFact, IEnumerable<IInputFact> facts)
		{
			AssertFactJoin<UNLOCOFact>(message, unlocoFact, facts);
			AssertFactJoin<CountryFact>("Country Fact shoule be populated", unlocoFact.Country.Fact, facts);
		}

		protected void AssertStaffFactAndChildren(string message, IStaffFact staffFact, IEnumerable<IInputFact> facts)
		{
			AssertFactJoin<StaffFact>(message, staffFact, facts);
			AssertFactJoin<DepartmentFact>("HomeDepartment should be populated", staffFact.HomeDepartment.Fact, facts);
		}

		IFactLoader GetFactLoader(RulesContextType contextType)
		{
			return new LandTransportJobFactLoader(contextType);
		}
	}
}
