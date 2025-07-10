using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Core;
using static Enterprise.Core.Constants.FreightShipmentDirection.Code;

namespace Enterprise.Accounting.RulesEngine.Facts.Testing
{
	public class QuickBookingJobFactTest : JobFactTest
	{
		public void TestPlugInNull_ThrowsException()
		{
			var environmentFactMock = new Mock<IEnvironmentFact>();
			AssertExceptionThrown<ArgumentNullException>(() => new QuickBookingJobFact(null, environmentFactMock.Object));
		}

		public void TestPlugIn_InvoicingSupporterNull_ThrowsException()
		{
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns((IJobInvoicingSupporter)null);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			AssertExceptionThrown<ArgumentNullException>(() => new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object));
		}

		public void TestPlugIn_InvoicingSupporter_JobNull_ThrowsException()
		{
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns((JobHeader)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			AssertExceptionThrown<ArgumentNullException>(() => new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object));
		}

		public void TestEnvironmentFactNull_ThrowsException()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			AssertExceptionThrown<ArgumentNullException>(() => new QuickBookingJobFact(plugInMock.Object, null));
		}

		public void TestShipmentPK_ValidPK()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInPK = Guid.NewGuid();
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			plugInMock.Setup(x => x.PK).Returns(plugInPK);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(plugInPK, quickBookingJobFact.BookingPK);
		}

		public void TestShipmentPK_EmptyPK()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			plugInMock.Setup(x => x.PK).Returns(Guid.Empty);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(Guid.Empty, quickBookingJobFact.BookingPK);
		}

		public void TestShipmentPK_InvalidPK()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var invalidPlugInPK = ZGuid.Invalid;
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			plugInMock.Setup(x => x.PK).Returns(invalidPlugInPK);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(Guid.Empty, quickBookingJobFact.BookingPK);
		}

		public void TestMode()
		{
			var expectedMode = "FCL";
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.Mode = expectedMode;
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = quotedBooking.Quote.PK;
			jobHeader.Parent = quotedBooking.Quote;
			jobHeader.JH_Description = "BASE FOR TEST";

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var quickBookingJobFact = new QuickBookingJobFact(quotedBooking, environmentFactMock.Object);

			AssertEquals(expectedMode, quickBookingJobFact.Mode);
		}

		public void TestContainerMode()
		{
			var containerMode = "ZZZ";
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.ContainerMode).Returns(containerMode);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(containerMode, quickBookingJobFact.ContainerMode);
		}

		public void TestServiceLevel()
		{
			var serviceLevel = "ZZZ";
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.ServiceLevel).Returns(serviceLevel);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(serviceLevel, quickBookingJobFact.ServiceLevel);
		}

		public void TestJobDirection_Export()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.IsExport).Returns(true);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(Export, quickBookingJobFact.JobDirection);
		}

		public void TestJobDirection_Import()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.IsImport).Returns(true);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(Import, quickBookingJobFact.JobDirection);
		}

		public void TestJobDirection_CrossTrade()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.IsCrossTrade).Returns(true);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(CrossTrade, quickBookingJobFact.JobDirection);
		}

		public void TestJobDirection_Domestic()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.IsDomestic).Returns(true);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(Domestic, quickBookingJobFact.JobDirection);
		}

		public void TestJobDirection_Empty()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.IsExport).Returns(false);
			invoicingSupporterMock.Setup(x => x.IsImport).Returns(false);
			invoicingSupporterMock.Setup(x => x.IsCrossTrade).Returns(false);
			invoicingSupporterMock.Setup(x => x.IsDomestic).Returns(false);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(String.Empty, quickBookingJobFact.JobDirection);
		}

		public void TestOrigin()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.Origin).Returns((RefUNLOCO)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var originFactMock = new Mock<IUNLOCOFact>();

			var originPort = Factory.New<RefUNLOCO>();
			originPort.RL_RN_NKCountryCode = "AU";
			invoicingSupporterMock.Setup(x => x.Origin).Returns(originPort);

			var quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object, origin: originPort, originFact: originFactMock.Object);
			AssertNotNull(quickBookingJobFact.Origin);
			AssertType<FactLeftJoin<IUNLOCOFact>>(quickBookingJobFact.Origin);
			AssertNotNull(quickBookingJobFact.Origin.Fact);

			AssertEquals("OriginCountry", originPort.RL_RN_NKCountryCode, quickBookingJobFact.OriginCountry);

			quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object, origin: null, originFact: null);
			AssertNotNull(quickBookingJobFact.Origin);
			AssertType<FactLeftJoin<IUNLOCOFact>>(quickBookingJobFact.Origin);
			AssertNull(quickBookingJobFact.Origin.Fact);

			AssertEquals("OriginCountry", string.Empty, quickBookingJobFact.OriginCountry);
		}

		public void TestOrigin_InvalidCountryCode()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.Origin).Returns((RefUNLOCO)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var originFactMock = new Mock<IUNLOCOFact>();

			var originPort = Factory.NewWithValidTestData<RefUNLOCO>();
			originPort.RL_RN_NKCountryCode = "XX";
			originPort.Validation.ValidateAll();

			var containsError = originPort.Notifications.ToList().Any(x => x.Message.Contains("Error - RL_RN_NKCountryCode"));
			Assert("originPort RefUNLOCO", containsError);
			AssertNull(originPort.Country);

			invoicingSupporterMock.Setup(x => x.Origin).Returns(originPort);

			var quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object, origin: originPort, originFact: originFactMock.Object);
			AssertNotNull(quickBookingJobFact.Origin);
			AssertType<FactLeftJoin<IUNLOCOFact>>(quickBookingJobFact.Origin);
			AssertNotNull(quickBookingJobFact.Origin.Fact);

			AssertEquals("OriginCountry", "XX", quickBookingJobFact.OriginCountry);
		}

		public void TestDestination()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.Destination).Returns((RefUNLOCO)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var destinationFactMock = new Mock<IUNLOCOFact>();

			var destinationPort = Factory.New<RefUNLOCO>();
			destinationPort.RL_RN_NKCountryCode = "AU";
			invoicingSupporterMock.Setup(x => x.Destination).Returns(destinationPort);

			var quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object, destination: destinationPort, destinationFact: destinationFactMock.Object);
			AssertNotNull(quickBookingJobFact.Destination);
			AssertType<FactLeftJoin<IUNLOCOFact>>(quickBookingJobFact.Destination);
			AssertNotNull(quickBookingJobFact.Destination.Fact);

			AssertEquals("DestinationCountry", destinationPort.RL_RN_NKCountryCode, quickBookingJobFact.DestinationCountry);

			quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object, destination: null, destinationFact: null);
			AssertNotNull(quickBookingJobFact.Destination);
			AssertType<FactLeftJoin<IUNLOCOFact>>(quickBookingJobFact.Destination);
			AssertNull(quickBookingJobFact.Destination.Fact);

			AssertEquals("DestinationCountry", string.Empty, quickBookingJobFact.DestinationCountry);
		}

		public void TestDestination_InvalidCountryCode()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.Destination).Returns((RefUNLOCO)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var destinationFactMock = new Mock<IUNLOCOFact>();

			var destinationPort = Factory.NewWithValidTestData<RefUNLOCO>();
			destinationPort.RL_RN_NKCountryCode = "XX";
			destinationPort.Validation.ValidateAll();

			var containsError = destinationPort.Notifications.ToList().Any(x => x.Message.Contains("Error - RL_RN_NKCountryCode"));
			Assert("destinationPort RefUNLOCO", containsError);
			AssertNull(destinationPort.Country);

			invoicingSupporterMock.Setup(x => x.Destination).Returns(destinationPort);

			var quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object, destination: destinationPort, destinationFact: destinationFactMock.Object);
			AssertNotNull(quickBookingJobFact.Destination);
			AssertType<FactLeftJoin<IUNLOCOFact>>(quickBookingJobFact.Destination);
			AssertNotNull(quickBookingJobFact.Destination.Fact);

			AssertEquals("DestinationCountry", "XX", quickBookingJobFact.DestinationCountry);
		}

		public void TestConsignee()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.Consignee).Returns((OrgHeader)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var consigneeFactMock = new Mock<IOrganisationWithMainAddressFact>();

			var consigneeOrg = Factory.New<OrgHeader>();
			invoicingSupporterMock.Setup(x => x.Consignee).Returns(consigneeOrg);

			var quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object, consigneeFact: consigneeFactMock.Object);
			AssertNotNull(quickBookingJobFact.Consignee);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(quickBookingJobFact.Consignee);
			AssertNotNull(quickBookingJobFact.Consignee.Fact);

			quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object, consigneeFact: null);
			AssertNotNull(quickBookingJobFact.Consignee);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(quickBookingJobFact.Consignee);
			AssertNull(quickBookingJobFact.Consignee.Fact);
		}

		public void TestConsignor()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.Consignor).Returns((OrgHeader)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var consignorFactMock = new Mock<IOrganisationWithMainAddressFact>();

			var consignorOrg = Factory.New<OrgHeader>();
			invoicingSupporterMock.Setup(x => x.Consignor).Returns(consignorOrg);
			var quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object, consignorFact: consignorFactMock.Object);
			AssertNotNull(quickBookingJobFact.Consignor);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(quickBookingJobFact.Consignor);
			AssertNotNull(quickBookingJobFact.Consignor.Fact);

			quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object, consignorFact: null);
			AssertNotNull(quickBookingJobFact.Consignor);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(quickBookingJobFact.Consignor);
			AssertNull(quickBookingJobFact.Consignor.Fact);
		}

		public void TestControllingAgent()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.ControllingAgent).Returns((OrgHeader)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var controllingAgentFactMock = new Mock<IOrganisationWithMainAddressFact>();

			var controllingAgentOrg = Factory.New<OrgHeader>();
			invoicingSupporterMock.Setup(x => x.ControllingAgent).Returns(controllingAgentOrg);
			var quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object, controllingAgent: controllingAgentOrg, controllingAgentFact: controllingAgentFactMock.Object);
			AssertNotNull(quickBookingJobFact.ControllingAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(quickBookingJobFact.ControllingAgent);
			AssertNotNull(quickBookingJobFact.ControllingAgent.Fact);

			AssertEquals(controllingAgentOrg.MainAddress.OA_RN_NKCountryCode, quickBookingJobFact.ControllingAgentCountry);

			quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object, controllingAgent: null, controllingAgentFact: null);
			AssertNotNull(quickBookingJobFact.ControllingAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(quickBookingJobFact.ControllingAgent);
			AssertNull(quickBookingJobFact.ControllingAgent.Fact);

			AssertEquals(string.Empty, quickBookingJobFact.ControllingAgentCountry);
		}

		public void TestControllingAgent_InvalidOrgHeader()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.ControllingAgent).Returns((OrgHeader)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var controllingAgentFactMock = new Mock<IOrganisationWithMainAddressFact>();

			var controllingAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
			controllingAgentOrg.MainAddress.OA_RN_NKCountryCode = "XX";
			controllingAgentOrg.MainAddress.Validation.ValidateAll();

			var containsError = controllingAgentOrg.MainAddress.Notifications.ToList().Any(x => x.Message.Contains("Error - OA_RN_NKCountryCode"));
			Assert("controllingAgentOrg.MainAddress OrgAddress", containsError);
			AssertNull(controllingAgentOrg.MainAddress.Country);

			invoicingSupporterMock.Setup(x => x.ControllingAgent).Returns(controllingAgentOrg);
			var quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object, controllingAgent: controllingAgentOrg, controllingAgentFact: controllingAgentFactMock.Object);
			AssertNotNull(quickBookingJobFact.ControllingAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(quickBookingJobFact.ControllingAgent);
			AssertNotNull(quickBookingJobFact.ControllingAgent.Fact);

			AssertEquals("XX", quickBookingJobFact.ControllingAgentCountry);
		}

		public void TestControllingCustomer()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.ControllingCustomer).Returns((OrgHeader)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var controllingCustomerFactMock = new Mock<IOrganisationWithMainAddressFact>();

			var controllingCustomerOrg = Factory.New<OrgHeader>();
			var controllingCustomerVerticalMarket = "DEF";
			var miscService = Factory.New<OrgMiscServ>();
			miscService.OM_OH = controllingCustomerOrg.PK;
			miscService.OM_CMIndustryVertical = controllingCustomerVerticalMarket;

			invoicingSupporterMock.Setup(x => x.ControllingCustomer).Returns(controllingCustomerOrg);
			var quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object, controllingCustomer: controllingCustomerOrg, controllingCustomerFact: controllingCustomerFactMock.Object);
			AssertNotNull(quickBookingJobFact.ControllingCustomer);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(quickBookingJobFact.ControllingCustomer);
			AssertNotNull(quickBookingJobFact.ControllingCustomer.Fact);

			AssertEquals(controllingCustomerVerticalMarket, quickBookingJobFact.ControllingCustomerVerticalMarket);

			quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object, controllingCustomer: null, controllingCustomerFact: null);
			AssertNotNull(quickBookingJobFact.ControllingCustomer);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(quickBookingJobFact.ControllingCustomer);
			AssertNull(quickBookingJobFact.ControllingCustomer.Fact);

			AssertEquals(string.Empty, quickBookingJobFact.ControllingCustomerVerticalMarket);
		}

		public void TestPickupAgent()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.PickUpAgent).Returns((OrgHeader)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var pickupAgentFactMock = new Mock<IOrganisationWithMainAddressFact>();

			var pickupAgentOrg = Factory.New<OrgHeader>();
			invoicingSupporterMock.Setup(x => x.PickUpAgent).Returns(pickupAgentOrg);
			var quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object, pickupAgent: pickupAgentOrg, pickupAgentFact: pickupAgentFactMock.Object);
			AssertNotNull(quickBookingJobFact.PickupAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(quickBookingJobFact.PickupAgent);
			AssertNotNull(quickBookingJobFact.PickupAgent.Fact);

			AssertEquals(pickupAgentOrg.MainAddress.OA_RN_NKCountryCode, quickBookingJobFact.PickupAgentCountry);

			quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object, pickupAgent: null, pickupAgentFact: null);
			AssertNotNull(quickBookingJobFact.PickupAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(quickBookingJobFact.PickupAgent);
			AssertNull(quickBookingJobFact.PickupAgent.Fact);

			AssertEquals(string.Empty, quickBookingJobFact.PickupAgentCountry);
		}

		public void TestPickupAgent_InvalidCountry()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.PickUpAgent).Returns((OrgHeader)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var pickupAgentFactMock = new Mock<IOrganisationWithMainAddressFact>();

			var pickupAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
			pickupAgentOrg.MainAddress.OA_RN_NKCountryCode = "XX";
			pickupAgentOrg.MainAddress.Validation.ValidateAll();

			var containsError = pickupAgentOrg.MainAddress.Notifications.ToList().Any(x => x.Message.Contains("Error - OA_RN_NKCountryCode"));
			Assert("pickupAgentOrg.MainAddress OrgAddress", containsError);
			AssertNull(pickupAgentOrg.MainAddress.Country);

			invoicingSupporterMock.Setup(x => x.PickUpAgent).Returns(pickupAgentOrg);
			var quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object, pickupAgent: pickupAgentOrg, pickupAgentFact: pickupAgentFactMock.Object);
			AssertNotNull(quickBookingJobFact.PickupAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(quickBookingJobFact.PickupAgent);
			AssertNotNull(quickBookingJobFact.PickupAgent.Fact);

			AssertEquals("XX", quickBookingJobFact.PickupAgentCountry);
		}

		public void TestDeliveryAgent()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.DeliveryAgent).Returns((OrgHeader)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var deliveryAgentFactMock = new Mock<IOrganisationWithMainAddressFact>();

			var deliveryAgentOrg = Factory.New<OrgHeader>();
			invoicingSupporterMock.Setup(x => x.DeliveryAgent).Returns(deliveryAgentOrg);
			var quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object, deliveryAgent: deliveryAgentOrg, deliveryAgentFact: deliveryAgentFactMock.Object);
			AssertNotNull(quickBookingJobFact.DeliveryAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(quickBookingJobFact.DeliveryAgent);
			AssertNotNull(quickBookingJobFact.DeliveryAgent.Fact);
			AssertEquals(deliveryAgentOrg.MainAddress.OA_RN_NKCountryCode, quickBookingJobFact.DeliveryAgentCountry);

			quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object, deliveryAgent: null, deliveryAgentFact: null);
			AssertNotNull(quickBookingJobFact.DeliveryAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(quickBookingJobFact.DeliveryAgent);
			AssertNull(quickBookingJobFact.DeliveryAgent.Fact);
			AssertEquals(string.Empty, quickBookingJobFact.DeliveryAgentCountry);
		}

		public void TestDeliveryAgent_InvalidCountry()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.DeliveryAgent).Returns((OrgHeader)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var deliveryAgentFactMock = new Mock<IOrganisationWithMainAddressFact>();

			var deliveryAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliveryAgentOrg.MainAddress.OA_RN_NKCountryCode = "XX";
			deliveryAgentOrg.MainAddress.Validation.ValidateAll();

			var containsError = deliveryAgentOrg.MainAddress.Notifications.ToList().Any(x => x.Message.Contains("Error - OA_RN_NKCountryCode"));
			Assert("deliveryAgentOrg.MainAddress OrgAddress", containsError);
			AssertNull(deliveryAgentOrg.MainAddress.Country);

			invoicingSupporterMock.Setup(x => x.DeliveryAgent).Returns(deliveryAgentOrg);
			var quickBookingJobFact = new QuickBookingJobFact(plugInMock.Object, environmentFactMock.Object, deliveryAgent: deliveryAgentOrg, deliveryAgentFact: deliveryAgentFactMock.Object);
			AssertNotNull(quickBookingJobFact.DeliveryAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(quickBookingJobFact.DeliveryAgent);
			AssertNotNull(quickBookingJobFact.DeliveryAgent.Fact);
			AssertEquals("XX", quickBookingJobFact.DeliveryAgentCountry);
		}

		public void TestPickupLocalTransport()
		{
			var localTransportOrg = Factory.New<OrgHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = localTransportOrg.MainAddress.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;

			Assert("Precondition", shipment.InvoicingSupporter.Job != null);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var localTransportFactMock = new Mock<IOrganisationWithMainAddressFact>();

			var quickBookingJobFact = new QuickBookingJobFact(shipment, environmentFactMock.Object, pickupLocalTransportFact: localTransportFactMock.Object);
			AssertNotNull(quickBookingJobFact.PickupLocalTransport);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(quickBookingJobFact.PickupLocalTransport);
			AssertNotNull(quickBookingJobFact.PickupLocalTransport.Fact);

			quickBookingJobFact = new QuickBookingJobFact(shipment, environmentFactMock.Object, pickupLocalTransportFact: null);
			AssertNotNull(quickBookingJobFact.PickupLocalTransport);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(quickBookingJobFact.PickupLocalTransport);
			AssertNull(quickBookingJobFact.PickupLocalTransport.Fact);
		}

		public void TestDeliveryLocalTransport()
		{
			var localTransportOrg = Factory.New<OrgHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = localTransportOrg.MainAddress.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;

			Assert("Precondition", shipment.InvoicingSupporter.Job != null);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var localTransportFactMock = new Mock<IOrganisationWithMainAddressFact>();

			var quickBookingJobFact = new QuickBookingJobFact(shipment, environmentFactMock.Object, deliveryLocalTransportFact: localTransportFactMock.Object);
			AssertNotNull(quickBookingJobFact.DeliveryLocalTransport);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(quickBookingJobFact.DeliveryLocalTransport);
			AssertNotNull(quickBookingJobFact.DeliveryLocalTransport.Fact);

			quickBookingJobFact = new QuickBookingJobFact(shipment, environmentFactMock.Object, deliveryLocalTransportFact: null);
			AssertNotNull(quickBookingJobFact.DeliveryLocalTransport);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(quickBookingJobFact.DeliveryLocalTransport);
			AssertNull(quickBookingJobFact.DeliveryLocalTransport.Fact);
		}

		public void TestOuterPacUnit()
		{
			var expectedouterPackUnit = "BAG";

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = quotedBooking.Quote.PK;
			jobHeader.Parent = quotedBooking.Quote;
			jobHeader.JH_Description = "BASE FOR TEST";
			booking.JS_F3_NKPackType = expectedouterPackUnit;

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var quickBookingJobFact = new QuickBookingJobFact(quotedBooking, environmentFactMock.Object);
			AssertEquals(expectedouterPackUnit, quickBookingJobFact.OuterPackUnit);
		}

		public void TestSendingAgent()
		{
			var sendingAgent = Factory.New<OrgHeader>();
			var consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.Consols.Add(consol);
			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;

			Assert("Precondition", shipment.InvoicingSupporter.Job != null);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var branchFact = new BranchFact(GlbBranch.CurrentBranch.PK.ToGuid(), GlbBranch.CurrentBranch.GB_Code);
			var departmentFact = new DepartmentFact(GlbBranch.CurrentBranch.PK.ToGuid(), GlbBranch.CurrentBranch.GB_Code);
			var jobBranchDepartmentFact = new JobBranchDepartmentFact(branchFact, departmentFact);
			var sendingAgentFactMock = new Mock<IOrganisationWithMainAddressFact>();

			var quickBookingJobFact = new ShipmentJobForTaxBranchFact(shipment, environmentFactMock.Object, jobBranchDepartmentFact, consolSendingAgentFact: sendingAgentFactMock.Object);
			AssertNotNull(quickBookingJobFact.ConsolSendingAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(quickBookingJobFact.ConsolSendingAgent);
			AssertNotNull(quickBookingJobFact.ConsolSendingAgent.Fact);

			quickBookingJobFact = new ShipmentJobForTaxBranchFact(shipment, environmentFactMock.Object, jobBranchDepartmentFact, consolSendingAgentFact: null);
			AssertNotNull(quickBookingJobFact.ConsolSendingAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(quickBookingJobFact.ConsolSendingAgent);
			AssertNull(quickBookingJobFact.ConsolSendingAgent.Fact);
		}

		public void TestReceivingAgent()
		{
			var receivingingAgent = Factory.New<OrgHeader>();
			var consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.Consols.Add(consol);
			consol.JK_OA_ReceivingForwarderAddress = receivingingAgent.MainAddress.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;

			Assert("Precondition", shipment.InvoicingSupporter.Job != null);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var branchFact = new BranchFact(GlbBranch.CurrentBranch.PK.ToGuid(), GlbBranch.CurrentBranch.GB_Code);
			var departmentFact = new DepartmentFact(GlbBranch.CurrentBranch.PK.ToGuid(), GlbBranch.CurrentBranch.GB_Code);
			var jobBranchDepartmentFact = new JobBranchDepartmentFact(branchFact, departmentFact);
			var receivingAgentFactMock = new Mock<IOrganisationWithMainAddressFact>();

			var quickBookingJobFact = new ShipmentJobForTaxBranchFact(shipment, environmentFactMock.Object, jobBranchDepartmentFact, consolReceivingAgentFact: receivingAgentFactMock.Object);
			AssertNotNull(quickBookingJobFact.ConsolReceivingAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(quickBookingJobFact.ConsolReceivingAgent);
			AssertNotNull(quickBookingJobFact.ConsolReceivingAgent.Fact);

			quickBookingJobFact = new ShipmentJobForTaxBranchFact(shipment, environmentFactMock.Object, jobBranchDepartmentFact, consolReceivingAgentFact: null);
			AssertNotNull(quickBookingJobFact.ConsolReceivingAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(quickBookingJobFact.ConsolReceivingAgent);
			AssertNull(quickBookingJobFact.ConsolReceivingAgent.Fact);
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;
	}
}
