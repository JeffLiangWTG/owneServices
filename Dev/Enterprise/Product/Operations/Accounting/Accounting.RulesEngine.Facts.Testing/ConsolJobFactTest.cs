using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.RulesEngine.Facts.Testing
{
	public class ConsolJobFactTest : JobFactTest
	{
		public void TestPlugInNull_ThrowsException()
		{
			var environmentFactMock = new Mock<IEnvironmentFact>();
			AssertExceptionThrown<ArgumentNullException>(() => new ConsolJobFact(null, environmentFactMock.Object));
		}

		public void TestPlugIn_InvoicingSupporterNull_ThrowsException()
		{
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns((IJobInvoicingSupporter)null);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			AssertExceptionThrown<ArgumentNullException>(() => new ConsolJobFact(plugInMock.Object, environmentFactMock.Object));
		}

		public void TestPlugIn_InvoicingSupporter_JobNull_ThrowsException()
		{
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns((JobHeader)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			AssertExceptionThrown<ArgumentNullException>(() => new ConsolJobFact(plugInMock.Object, environmentFactMock.Object));
		}

		public void TestEnvironmentFactNull_ThrowsException()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			AssertExceptionThrown<ArgumentNullException>(() => new ConsolJobFact(plugInMock.Object, null));
		}

		public void TestConsolPK_ValidPK()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInPK = Guid.NewGuid();
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			plugInMock.Setup(x => x.PK).Returns(plugInPK);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var consolJobFact = new ConsolJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(plugInPK, consolJobFact.ConsolPK);
		}

		public void TestConsolPK_EmptyPK()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			plugInMock.Setup(x => x.PK).Returns(Guid.Empty);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var consolJobFact = new ConsolJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(Guid.Empty, consolJobFact.ConsolPK);
		}

		public void TestConsolPK_InvalidPK()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var invalidPlugInPK = ZGuid.Invalid;
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			plugInMock.Setup(x => x.PK).Returns(invalidPlugInPK);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var consolJobFact = new ConsolJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(Guid.Empty, consolJobFact.ConsolPK);
		}

		public void TestConsolType()
		{
			var consolType = "XYZ";
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.ConsolType).Returns(consolType);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var consolJobFact = new ConsolJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(consolType, consolJobFact.ConsolType);
		}

		public void TestContainerMode()
		{
			var containerMode = "XXX";
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.ContainerMode).Returns(containerMode);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var consolJobFact = new ConsolJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(containerMode, consolJobFact.ContainerMode);
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

			var consolJobFact = new ConsolJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(transportMode, consolJobFact.TransportMode);
		}

		public void TestSendingAgent()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.SendingAgent).Returns((OrgHeader)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var sendingAgentFactMock = new Mock<IOrganisationWithMainAddressFact>();

			var sendingAgentOrg = Factory.New<OrgHeader>();
			invoicingSupporterMock.Setup(x => x.SendingAgent).Returns(sendingAgentOrg);
			var consolJobFact = new ConsolJobFact(plugInMock.Object, environmentFactMock.Object, sendingAgent: sendingAgentOrg, sendingAgentFact: sendingAgentFactMock.Object);
			AssertNotNull(consolJobFact.SendingAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(consolJobFact.SendingAgent);
			AssertNotNull(consolJobFact.SendingAgent.Fact);

			AssertEquals("SendingAgentCountry", sendingAgentOrg.MainAddress.Country.Code, consolJobFact.SendingAgentCountry);

			consolJobFact = new ConsolJobFact(plugInMock.Object, environmentFactMock.Object, sendingAgent: null, sendingAgentFact: null);
			AssertNotNull(consolJobFact.SendingAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(consolJobFact.SendingAgent);
			AssertNull(consolJobFact.SendingAgent.Fact);

			AssertEquals("SendingAgentCountry", string.Empty, consolJobFact.SendingAgentCountry);
		}

		public void TestReceivingAgent()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.ReceivingAgent).Returns((OrgHeader)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var receivingAgentFactMock = new Mock<IOrganisationWithMainAddressFact>();

			var receivingAgentOrg = Factory.New<OrgHeader>();
			invoicingSupporterMock.Setup(x => x.ReceivingAgent).Returns(receivingAgentOrg);
			var consolJobFact = new ConsolJobFact(plugInMock.Object, environmentFactMock.Object, receivingAgent: receivingAgentOrg, receivingAgentFact: receivingAgentFactMock.Object);
			AssertNotNull(consolJobFact.ReceivingAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(consolJobFact.ReceivingAgent);
			AssertNotNull(consolJobFact.ReceivingAgent.Fact);

			AssertEquals("ReceivingAgentCountry", receivingAgentOrg.MainAddress.Country.Code, consolJobFact.ReceivingAgentCountry);

			consolJobFact = new ConsolJobFact(plugInMock.Object, environmentFactMock.Object, receivingAgent: null, receivingAgentFact: null);
			AssertNotNull(consolJobFact.ReceivingAgent);
			AssertType<FactLeftJoin<IOrganisationWithMainAddressFact>>(consolJobFact.ReceivingAgent);
			AssertNull(consolJobFact.ReceivingAgent.Fact);

			AssertEquals("ReceivingAgentCountry", string.Empty, consolJobFact.ReceivingAgentCountry);
		}

		public void TestLoadPort()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.Origin).Returns((RefUNLOCO)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var loadPortFactMock = new Mock<IUNLOCOFact>();

			var loadPort = Factory.New<RefUNLOCO>();
			loadPort.RL_RN_NKCountryCode = "AU";
			invoicingSupporterMock.Setup(x => x.Origin).Returns(loadPort);

			var consolJobFact = new ConsolJobFact(plugInMock.Object, environmentFactMock.Object, loadPort: loadPort, loadPortFact: loadPortFactMock.Object);
			AssertNotNull(consolJobFact.LoadPort);
			AssertType<FactLeftJoin<IUNLOCOFact>>(consolJobFact.LoadPort);
			AssertNotNull(consolJobFact.LoadPort.Fact);

			AssertEquals("LoadPortCountry", loadPort.Country.Code, consolJobFact.LoadPortCountry);

			consolJobFact = new ConsolJobFact(plugInMock.Object, environmentFactMock.Object, loadPort: null, loadPortFact: null);
			AssertNotNull(consolJobFact.LoadPort);
			AssertType<FactLeftJoin<IUNLOCOFact>>(consolJobFact.LoadPort);
			AssertNull(consolJobFact.LoadPort.Fact);

			AssertEquals("LoadPortCountry", string.Empty, consolJobFact.LoadPortCountry);
		}

		public void TestDischargePort()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.Destination).Returns((RefUNLOCO)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var dischargePortFactMock = new Mock<IUNLOCOFact>();

			var dischargePort = Factory.New<RefUNLOCO>();
			dischargePort.RL_RN_NKCountryCode = "AU";
			invoicingSupporterMock.Setup(x => x.Destination).Returns(dischargePort);

			var consolJobFact = new ConsolJobFact(plugInMock.Object, environmentFactMock.Object, dischargePort: dischargePort, dischargePortFact: dischargePortFactMock.Object);
			AssertNotNull(consolJobFact.DischargePort);
			AssertType<FactLeftJoin<IUNLOCOFact>>(consolJobFact.DischargePort);
			AssertNotNull(consolJobFact.DischargePort.Fact);

			AssertEquals("DischargePortCountry", dischargePort.Country.Code, consolJobFact.DischargePortCountry);

			consolJobFact = new ConsolJobFact(plugInMock.Object, environmentFactMock.Object, dischargePort: null, dischargePortFact: null);
			AssertNotNull(consolJobFact.DischargePort);
			AssertType<FactLeftJoin<IUNLOCOFact>>(consolJobFact.DischargePort);
			AssertNull(consolJobFact.DischargePort.Fact);

			AssertEquals("DischargePortCountry", string.Empty, consolJobFact.DischargePortCountry);
		}

		public void TestSendingAgentGatewayFlag()
		{
			var sendingAgentGatewayFlag = "GTT";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_SendingForwarderHandlingType = sendingAgentGatewayFlag;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = consol.PK;

			Assert("Precondition", consol.InvoicingSupporter.Job != null);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var consolJobFact = new ConsolJobFact(consol, environmentFactMock.Object);
			AssertEquals(sendingAgentGatewayFlag, consolJobFact.SendingAgentGatewayFlag);
		}

		public void TestReceivingAgentGatewayFlag()
		{
			var receivingAgentGatewayFlag = "GTT";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ReceivingForwarderHandlingType = receivingAgentGatewayFlag;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = consol.PK;

			Assert("Precondition", consol.InvoicingSupporter.Job != null);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var consolJobFact = new ConsolJobFact(consol, environmentFactMock.Object);
			AssertEquals(receivingAgentGatewayFlag, consolJobFact.ReceivingAgentGatewayFlag);
		}

		public void TestConsolPaymentType()
		{
			var paymentType = "PPD";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_PrepaidCollect = paymentType;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = consol.PK;

			Assert("Precondition", consol.InvoicingSupporter.Job != null);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var consolJobFact = new ConsolJobFact(consol, environmentFactMock.Object);
			AssertEquals(paymentType, consolJobFact.PaymentType);
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;
	}
}
