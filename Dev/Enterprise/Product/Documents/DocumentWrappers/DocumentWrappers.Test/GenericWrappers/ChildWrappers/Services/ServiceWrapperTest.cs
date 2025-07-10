using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ServiceWrapper))]
	sealed class ServiceWrapperTest : GenericWrapperTest
	{
		public void TestLocationWrapper()
		{
			var contractor = Factory.NewWithValidTestData<OrgHeader>();
			contractor.OH_Code = "CONT";
			var contractorAddress = contractor.MainAddress;
			contractorAddress.OA_Address1 = "Address1";

			var location = Factory.NewWithValidTestData<OrgHeader>();
			location.OH_Code = "LOCA";
			var locationAddress = location.MainAddress;
			locationAddress.OA_Address1 = "Address2";

			var shipment = Factory.New<ForwardingShipment>();
			var service = shipment.DocsAndCartage.Services.AddNew();
			service.ES_OH_Contractor = contractor.PK;
			service.ES_OA_Location = ZGuid.Empty;

			var wrapper = new ServiceWrapper(service, Factory);
			AssertEquals(contractor.MainAddress, wrapper.LocationAddress.WrappedObject);

			service = shipment.DocsAndCartage.Services.AddNew();
			service.ES_OH_Contractor = contractor.PK;
			service.ES_OA_Location = locationAddress.PK;

			wrapper = new ServiceWrapper(service, Factory);
			AssertEquals(location.MainAddress, wrapper.LocationAddress.WrappedObject);
		}

		public void TestServiceWrappersForDocBuilder()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var service1 = shipment.DocsAndCartage.Services.AddNew();
			var service2 = shipment.DocsAndCartage.Services.AddNew();
			var service3 = shipment.DocsAndCartage.Services.AddNew();

			var servicesSelectionProvider = new Mock<IServicesSelectionProvider>(MockBehavior.Strict);
			Factory.SetValue(() => servicesSelectionProvider.Object);

			servicesSelectionProvider.Setup(m => m.GetServicesToPrint(It.Is<IHaveServices>(p => p == shipment.DocsAndCartage)))
				.Returns((JobService[])null);

			AssertNull(shipment.DocsAndCartage.GetServiceWrappers(Core.Constants.DataContext.GenericFreightJob));
			servicesSelectionProvider.Verify(m => m.GetServicesToPrint(It.Is<IHaveServices>(p => p == shipment.DocsAndCartage)), Times.Once());
			servicesSelectionProvider.Invocations.Clear();

			servicesSelectionProvider.Setup(m => m.GetServicesToPrint(It.Is<IHaveServices>(p => p == shipment.DocsAndCartage)))
				.Returns(new JobService[] { service1, service2, service3 });

			DocumentWrapper[] wrappers = shipment.DocsAndCartage.GetServiceWrappersForDocBuilder(shipment, Core.Constants.DataContext.GenericFreightJob);
			AssertNotNull(wrappers);
			AssertEquals(3, wrappers.Length);
			servicesSelectionProvider.Verify(m => m.GetServicesToPrint(It.Is<IHaveServices>(p => p == shipment.DocsAndCartage)), Times.Once());
			servicesSelectionProvider.Invocations.Clear();

			servicesSelectionProvider.Setup(m => m.GetServicesToPrint(It.Is<IHaveServices>(p => p == shipment.DocsAndCartage)))
				.Returns(new JobService[] { service1, service2 });

			wrappers = shipment.DocsAndCartage.GetServiceWrappersForDocBuilder(shipment, Core.Constants.DataContext.GenericFreightJob);
			AssertEquals(2, wrappers.Length);
			servicesSelectionProvider.Verify(m => m.GetServicesToPrint(It.Is<IHaveServices>(p => p == shipment.DocsAndCartage)), Times.Once());
		}

		public override void TestWrapperMappingsEmpty()
		{
			JobService jobService = Factory.New<JobService>();
			ServiceWrapper emptyWrapper = new ServiceWrapper(jobService, Factory);
			AssertEquals("emptyWrapper.Type.Code", ZString.Empty, emptyWrapper.Type.Code);
			AssertEquals("emptyWrapper.Contractor.CompanyName", ZString.Empty, emptyWrapper.Contractor.CompanyName);
			AssertEquals("emptyWrapper.LocationAddress.CompanyName", ZString.Empty, emptyWrapper.LocationAddress.CompanyName);
			AssertEquals("emptyWrapper.DateBooked", ZDateTime.Empty, emptyWrapper.DateBooked);
			AssertEquals("emptyWrapper.ReferenceNumber", ZString.Empty, emptyWrapper.ReferenceNumber);
			AssertEquals("emptyWrapper.DateCompleted", ZDateTime.Empty, emptyWrapper.DateCompleted);
			AssertEquals("emptyWrapper.ServiceCount", ZDecimal.Zero, emptyWrapper.ServiceCount);
			AssertEquals("emptyWrapper.ServiceDuration", ZString.Empty, emptyWrapper.ServiceDuration);
			AssertEquals("emptyWrapper.ServiceNote", ZString.Empty, emptyWrapper.ServiceNote);
			AssertEquals("wrapperEmpty.ToString()", ZString.Empty, emptyWrapper.ToString());
			AssertEquals("wrapperEmpty.Contractor", ZString.Empty, emptyWrapper.Contractor.CompanyName);
			AssertEquals("wrapperEmpty.ServiceDocumentTitle", "Service", emptyWrapper.ServiceDocumentTitle);
			AssertEquals("wrapperEmpty.RequestedBy", ZString.Empty, emptyWrapper.RequestedBy.MainEmail);
			AssertEquals("wrapperEmpty.Note", ZString.Empty, emptyWrapper.Note);
		}

		[TestDate(2007, 1, 1)]
		public void TestWrapperMappingFull()
		{
			OrgHeader contractor = Factory.New<OrgHeader>();
			contractor.OH_FullName = "TEST CONTRACTOR";
			OrgAddress location = contractor.MainAddress;
			location.OA_Address1 = "69 TEST ROAD";
			location.OA_Address2 = "CARGOWISE LAND";
			location.OA_City = "ATLANTIS";
			location.OA_RN_NKCountryCode = "AU";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "OOCL0000011";
			JobService service = container.Services.AddNew();
			service.ES_ServiceCode = "CLN";
			service.ES_References = "TEST REFERENCE";
			service.ES_Duration = ZDateTime.Now.AddHours(23).AddMinutes(23);
			service.ES_ServiceNote = "TEST SERVICE NOTE";
			service.ES_Completed = new ZDateTime(2007, 8, 4);
			service.ES_Booked = new ZDateTime(2006, 5, 25);
			service.ES_ServiceCount = 2;
			service.ES_OH_Contractor = contractor.PK;
			service.ES_OA_Location = location.PK;

			ServiceWrapper fullWrapper = new ServiceWrapper(service, Factory);
			AssertEquals("fullWrapper.Type.Code", "CLN", fullWrapper.Type.Code);
			AssertEquals("fullWrapper.ReferenceNumber", "TEST REFERENCE", fullWrapper.ReferenceNumber);
			AssertEquals("fullWrapper.ServiceDuration", "23:23", fullWrapper.ServiceDuration);
			AssertEquals("fullWrapper.ServiceNote", "TEST SERVICE NOTE", fullWrapper.ServiceNote);
			AssertEquals("fullWrapper.DateCompleted", new ZDateTime(2007, 8, 4), fullWrapper.DateCompleted);
			AssertEquals("fullWrapper.DateBooked", new ZDateTime(2006, 5, 25), fullWrapper.DateBooked);
			AssertEquals("fullWrapper.ServiceCount", 2m, fullWrapper.ServiceCount);
			AssertEquals("fullWrapper.Contractor.CompanyName", "TEST CONTRACTOR", fullWrapper.Contractor.CompanyName);
			AssertEquals("fullWrapper.LocationAddress.Address", "69 TEST ROAD\nCARGOWISE LAND\nATLANTIS\nAUSTRALIA", fullWrapper.LocationAddress.Address);
			AssertEquals("wrapperFull.Note", "TEST SERVICE NOTE", fullWrapper.Note);
			AssertEquals("Should return the org proxy", GlbBranch.CurrentBranch.OrgProxy.OH_Code, fullWrapper.RequestedBy.CompanyCode);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new ServiceWrapper(null, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
ContainerService                                 (Default Field: Type)
======================================================================
Name                                    Type
----------------------------------------------------------------------
LocationAddress                         Address
Type                                    CodeAndDescription
Contractor                              Organisation
RequestedBy                             Organisation
DateBooked                              DateTime
DateCompleted                           DateTime
Note                                    String
ReferenceNumber                         String
ServiceCount                            Decimal
ServiceDocumentTitle                    String
ServiceDuration                         String
ServiceNote                             String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Contractor : TEST CONTRACTOR\n69 TEST ROAD\nCARGOWISE LAND\nATLANTIS
LocationAddress : TEST CONTRACTOR\n69 TEST ROAD\nCARGOWISE LAND\nATLANTIS
Registry : (No Default Field Value Available on Registry)
RequestedBy : EDI CUSTOMS BROKERS\n10 HUTCHESON STREET\nALBION QLD\n4010\nAUSTRALIA
Type : CLN - Cleaning
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			OrgHeader contractor = Factory.New<OrgHeader>();
			contractor.OH_FullName = "TEST CONTRACTOR";
			OrgAddress location = contractor.MainAddress;
			location.OA_Address1 = "69 TEST ROAD";
			location.OA_Address2 = "CARGOWISE LAND";
			location.OA_City = "ATLANTIS";
			location.OA_RN_NKCountryCode = string.Empty;

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingContainer container = consol.Containers.AddNew();
			JobService service = container.Services.AddNew();
			service.ES_ServiceCode = "CLN";
			service.ES_OH_Contractor = contractor.PK;
			service.ES_OA_Location = location.PK;

			return new ServiceWrapper(service, Factory);
		}
	}
}
