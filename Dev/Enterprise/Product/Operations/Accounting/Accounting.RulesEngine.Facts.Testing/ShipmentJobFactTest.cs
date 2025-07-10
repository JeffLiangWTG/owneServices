using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Core;
using static Enterprise.Core.Constants.FreightShipmentDirection.Code;

namespace Enterprise.Accounting.RulesEngine.Facts.Testing
{
	public class ShipmentJobFactTest : JobFactTest
	{
		public void TestPlugInNull_ThrowsException()
		{
			var environmentFactMock = new Mock<IEnvironmentFact>();
			AssertExceptionThrown<ArgumentNullException>(() => new ShipmentJobFact(null, environmentFactMock.Object));
		}

		public void TestPlugIn_InvoicingSupporterNull_ThrowsException()
		{
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns((IJobInvoicingSupporter)null);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			AssertExceptionThrown<ArgumentNullException>(() => new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object));
		}

		public void TestPlugIn_InvoicingSupporter_JobNull_ThrowsException()
		{
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns((JobHeader)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			AssertExceptionThrown<ArgumentNullException>(() => new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object));
		}

		public void TestEnvironmentFactNull_ThrowsException()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			AssertExceptionThrown<ArgumentNullException>(() => new ShipmentJobFact(plugInMock.Object, null));
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

			var shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(plugInPK, shipmentJobFact.ShipmentPK);
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

			var shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(Guid.Empty, shipmentJobFact.ShipmentPK);
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

			var shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(Guid.Empty, shipmentJobFact.ShipmentPK);
		}

		public void TestTransportMode()
		{
			var transportMode = "YYY";
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.TransportMode).Returns(transportMode);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(transportMode, shipmentJobFact.TransportMode);
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

			var shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(containerMode, shipmentJobFact.ContainerMode);
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

			var shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(serviceLevel, shipmentJobFact.ServiceLevel);
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

			var shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(Export, shipmentJobFact.JobDirection);
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

			var shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(Import, shipmentJobFact.JobDirection);
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

			var shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(CrossTrade, shipmentJobFact.JobDirection);
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

			var shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(Domestic, shipmentJobFact.JobDirection);
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

			var shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(String.Empty, shipmentJobFact.JobDirection);
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

			var shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object, origin: originPort, originFact: originFactMock.Object);
			AssertNotNull(shipmentJobFact.Origin);
			AssertType<FactLeftJoin<IUNLOCOFact>>(shipmentJobFact.Origin);
			AssertNotNull(shipmentJobFact.Origin.Fact);

			AssertEquals("OriginCountry", originPort.RL_RN_NKCountryCode, shipmentJobFact.OriginCountry);

			shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object, origin: null, originFact: null);
			AssertNotNull(shipmentJobFact.Origin);
			AssertType<FactLeftJoin<IUNLOCOFact>>(shipmentJobFact.Origin);
			AssertNull(shipmentJobFact.Origin.Fact);

			AssertEquals("OriginCountry", string.Empty, shipmentJobFact.OriginCountry);
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

			var shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object, origin: originPort, originFact: originFactMock.Object);
			AssertNotNull(shipmentJobFact.Origin);
			AssertType<FactLeftJoin<IUNLOCOFact>>(shipmentJobFact.Origin);
			AssertNotNull(shipmentJobFact.Origin.Fact);

			AssertEquals("OriginCountry", "XX", shipmentJobFact.OriginCountry);
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

			var shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object, destination: destinationPort, destinationFact: destinationFactMock.Object);
			AssertNotNull(shipmentJobFact.Destination);
			AssertType<FactLeftJoin<IUNLOCOFact>>(shipmentJobFact.Destination);
			AssertNotNull(shipmentJobFact.Destination.Fact);

			AssertEquals("DestinationCountry", destinationPort.RL_RN_NKCountryCode, shipmentJobFact.DestinationCountry);

			shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object, destination: null, destinationFact: null);
			AssertNotNull(shipmentJobFact.Destination);
			AssertType<FactLeftJoin<IUNLOCOFact>>(shipmentJobFact.Destination);
			AssertNull(shipmentJobFact.Destination.Fact);

			AssertEquals("DestinationCountry", string.Empty, shipmentJobFact.DestinationCountry);
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

			var shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object, destination: destinationPort, destinationFact: destinationFactMock.Object);
			AssertNotNull(shipmentJobFact.Destination);
			AssertType<FactLeftJoin<IUNLOCOFact>>(shipmentJobFact.Destination);
			AssertNotNull(shipmentJobFact.Destination.Fact);

			AssertEquals("DestinationCountry", "XX", shipmentJobFact.DestinationCountry);
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

			var shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object, consigneeFact: consigneeFactMock.Object);
			AssertNotNull(shipmentJobFact.Consignee);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(shipmentJobFact.Consignee);
			AssertNotNull(shipmentJobFact.Consignee.Fact);

			shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object, consigneeFact: null);
			AssertNotNull(shipmentJobFact.Consignee);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(shipmentJobFact.Consignee);
			AssertNull(shipmentJobFact.Consignee.Fact);
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
			var shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object, consignorFact: consignorFactMock.Object);
			AssertNotNull(shipmentJobFact.Consignor);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(shipmentJobFact.Consignor);
			AssertNotNull(shipmentJobFact.Consignor.Fact);

			shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object, consignorFact: null);
			AssertNotNull(shipmentJobFact.Consignor);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(shipmentJobFact.Consignor);
			AssertNull(shipmentJobFact.Consignor.Fact);
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
			var shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object, controllingAgent: controllingAgentOrg, controllingAgentFact: controllingAgentFactMock.Object);
			AssertNotNull(shipmentJobFact.ControllingAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(shipmentJobFact.ControllingAgent);
			AssertNotNull(shipmentJobFact.ControllingAgent.Fact);

			AssertEquals(controllingAgentOrg.MainAddress.OA_RN_NKCountryCode, shipmentJobFact.ControllingAgentCountry);

			shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object, controllingAgent: null, controllingAgentFact: null);
			AssertNotNull(shipmentJobFact.ControllingAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(shipmentJobFact.ControllingAgent);
			AssertNull(shipmentJobFact.ControllingAgent.Fact);

			AssertEquals(string.Empty, shipmentJobFact.ControllingAgentCountry);
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
			var shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object, controllingAgent: controllingAgentOrg, controllingAgentFact: controllingAgentFactMock.Object);
			AssertNotNull(shipmentJobFact.ControllingAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(shipmentJobFact.ControllingAgent);
			AssertNotNull(shipmentJobFact.ControllingAgent.Fact);

			AssertEquals("XX", shipmentJobFact.ControllingAgentCountry);
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
			var shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object, controllingCustomer: controllingCustomerOrg, controllingCustomerFact: controllingCustomerFactMock.Object);
			AssertNotNull(shipmentJobFact.ControllingCustomer);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(shipmentJobFact.ControllingCustomer);
			AssertNotNull(shipmentJobFact.ControllingCustomer.Fact);

			AssertEquals(controllingCustomerVerticalMarket, shipmentJobFact.ControllingCustomerVerticalMarket);

			shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object, controllingCustomer: null, controllingCustomerFact: null);
			AssertNotNull(shipmentJobFact.ControllingCustomer);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(shipmentJobFact.ControllingCustomer);
			AssertNull(shipmentJobFact.ControllingCustomer.Fact);

			AssertEquals(string.Empty, shipmentJobFact.ControllingCustomerVerticalMarket);
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
			var shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object, pickupAgent: pickupAgentOrg, pickupAgentFact: pickupAgentFactMock.Object);
			AssertNotNull(shipmentJobFact.PickupAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(shipmentJobFact.PickupAgent);
			AssertNotNull(shipmentJobFact.PickupAgent.Fact);

			AssertEquals(pickupAgentOrg.MainAddress.OA_RN_NKCountryCode, shipmentJobFact.PickupAgentCountry);

			shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object, pickupAgent: null, pickupAgentFact: null);
			AssertNotNull(shipmentJobFact.PickupAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(shipmentJobFact.PickupAgent);
			AssertNull(shipmentJobFact.PickupAgent.Fact);

			AssertEquals(string.Empty, shipmentJobFact.PickupAgentCountry);
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
			var shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object, pickupAgent: pickupAgentOrg, pickupAgentFact: pickupAgentFactMock.Object);
			AssertNotNull(shipmentJobFact.PickupAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(shipmentJobFact.PickupAgent);
			AssertNotNull(shipmentJobFact.PickupAgent.Fact);

			AssertEquals("XX", shipmentJobFact.PickupAgentCountry);
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
			var shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object, deliveryAgent: deliveryAgentOrg, deliveryAgentFact: deliveryAgentFactMock.Object);
			AssertNotNull(shipmentJobFact.DeliveryAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(shipmentJobFact.DeliveryAgent);
			AssertNotNull(shipmentJobFact.DeliveryAgent.Fact);
			AssertEquals(deliveryAgentOrg.MainAddress.OA_RN_NKCountryCode, shipmentJobFact.DeliveryAgentCountry);

			shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object, deliveryAgent: null, deliveryAgentFact: null);
			AssertNotNull(shipmentJobFact.DeliveryAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(shipmentJobFact.DeliveryAgent);
			AssertNull(shipmentJobFact.DeliveryAgent.Fact);
			AssertEquals(string.Empty, shipmentJobFact.DeliveryAgentCountry);
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
			var shipmentJobFact = new ShipmentJobFact(plugInMock.Object, environmentFactMock.Object, deliveryAgent: deliveryAgentOrg, deliveryAgentFact: deliveryAgentFactMock.Object);
			AssertNotNull(shipmentJobFact.DeliveryAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(shipmentJobFact.DeliveryAgent);
			AssertNotNull(shipmentJobFact.DeliveryAgent.Fact);
			AssertEquals("XX", shipmentJobFact.DeliveryAgentCountry);
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

			var shipmentJobFact = new ShipmentJobFact(shipment, environmentFactMock.Object, pickupLocalTransportFact: localTransportFactMock.Object);
			AssertNotNull(shipmentJobFact.PickupLocalTransport);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(shipmentJobFact.PickupLocalTransport);
			AssertNotNull(shipmentJobFact.PickupLocalTransport.Fact);

			shipmentJobFact = new ShipmentJobFact(shipment, environmentFactMock.Object, pickupLocalTransportFact: null);
			AssertNotNull(shipmentJobFact.PickupLocalTransport);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(shipmentJobFact.PickupLocalTransport);
			AssertNull(shipmentJobFact.PickupLocalTransport.Fact);
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

			var shipmentJobFact = new ShipmentJobFact(shipment, environmentFactMock.Object, deliveryLocalTransportFact: localTransportFactMock.Object);
			AssertNotNull(shipmentJobFact.DeliveryLocalTransport);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(shipmentJobFact.DeliveryLocalTransport);
			AssertNotNull(shipmentJobFact.DeliveryLocalTransport.Fact);

			shipmentJobFact = new ShipmentJobFact(shipment, environmentFactMock.Object, deliveryLocalTransportFact: null);
			AssertNotNull(shipmentJobFact.DeliveryLocalTransport);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(shipmentJobFact.DeliveryLocalTransport);
			AssertNull(shipmentJobFact.DeliveryLocalTransport.Fact);
		}

		public void TestOuterPacUnit()
		{
			var outerPackUnit = "KG";
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_F3_NKPackType = outerPackUnit;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;

			Assert("Precondition", shipment.InvoicingSupporter.Job != null);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var shipmentJobFact = new ShipmentJobFact(shipment, environmentFactMock.Object);
			AssertEquals(outerPackUnit, shipmentJobFact.OuterPackUnit);
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

			var shipmentJobFact = new ShipmentJobForTaxBranchFact(shipment, environmentFactMock.Object, jobBranchDepartmentFact, consolSendingAgentFact: sendingAgentFactMock.Object);
			AssertNotNull(shipmentJobFact.ConsolSendingAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(shipmentJobFact.ConsolSendingAgent);
			AssertNotNull(shipmentJobFact.ConsolSendingAgent.Fact);

			shipmentJobFact = new ShipmentJobForTaxBranchFact(shipment, environmentFactMock.Object, jobBranchDepartmentFact, consolSendingAgentFact: null);
			AssertNotNull(shipmentJobFact.ConsolSendingAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(shipmentJobFact.ConsolSendingAgent);
			AssertNull(shipmentJobFact.ConsolSendingAgent.Fact);
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

			var shipmentJobFact = new ShipmentJobForTaxBranchFact(shipment, environmentFactMock.Object, jobBranchDepartmentFact, consolReceivingAgentFact: receivingAgentFactMock.Object);
			AssertNotNull(shipmentJobFact.ConsolReceivingAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(shipmentJobFact.ConsolReceivingAgent);
			AssertNotNull(shipmentJobFact.ConsolReceivingAgent.Fact);

			shipmentJobFact = new ShipmentJobForTaxBranchFact(shipment, environmentFactMock.Object, jobBranchDepartmentFact, consolReceivingAgentFact: null);
			AssertNotNull(shipmentJobFact.ConsolReceivingAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(shipmentJobFact.ConsolReceivingAgent);
			AssertNull(shipmentJobFact.ConsolReceivingAgent.Fact);
		}

		public void TestShipmentType()
		{
			var expectedShipmentType = "BCN";
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = expectedShipmentType;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;

			Assert("Precondition", shipment.InvoicingSupporter.Job != null);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var shipmentJobFact = new ShipmentJobFact(shipment, environmentFactMock.Object);
			AssertEquals(expectedShipmentType, shipmentJobFact.ShipmentType);
		}

		public void TestFirstCommodityCodeWithOuterPackLinesData()
		{
			var expectedCommodityCode = "CCC";
			var shipment = Factory.New<ForwardingShipment>();

			var line2 = Factory.New<ForwardingPackLine>();
			line2.JL_RH_NKCommodityCode = "BBB";
			line2.JL_FreightMode = FreightConstants.OuterPackType;
			line2.JL_PackLineId = "2";
			shipment.OuterPackLines.Add(line2);

			var line1 = Factory.New<ForwardingPackLine>();
			line1.JL_RH_NKCommodityCode = expectedCommodityCode;
			line1.JL_FreightMode = FreightConstants.OuterPackType;
			line1.JL_PackLineId = "1";
			shipment.OuterPackLines.Add(line1);

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;

			Assert("Precondition", shipment.InvoicingSupporter.Job != null);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var shipmentJobFact = new ShipmentJobFact(shipment, environmentFactMock.Object);
			AssertEquals(expectedCommodityCode, shipmentJobFact.FirstCommodityCode);
		}

		public void TestFirstCommodityCodeWithoutOuterPackLinesData()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;

			Assert("Precondition", shipment.InvoicingSupporter.Job != null);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var shipmentJobFact = new ShipmentJobFact(shipment, environmentFactMock.Object);
			AssertNullOrEmpty(shipmentJobFact.FirstCommodityCode);
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;
	}
}
