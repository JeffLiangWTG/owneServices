using System;
using System.ServiceModel;
using CargoWise.Application;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Business.RoutingRuleValidationWebService;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Registry.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	public sealed class RoutingRuleValidatorTest : TransactionedTestCase
	{
		public void TestIsRegisteredInFactory()
		{
			var validator1 = ObjectFactory.Get<IRoutingRuleValidator>();
			AssertNotNull("IRoutingRuleValidator is accessible via ObjectFactory (no ctor parameter)", validator1);

			var mockService = new Mock<IRoutingRuleValidatorService>();
			var validator2 = ObjectFactory.Get<IRoutingRuleValidator>(nameof(IRoutingRuleValidator), mockService.Object);
			AssertNotNull("IRoutingRuleValidator is accessible via ObjectFactory (with ctor parameter)", validator2);
		}

		public void TestRequestSettings_Testing()
		{
			using (FreightDataRegistry.Instance.EnableCarrierMessagingConnectionValidationTestUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var mockService = new Mock<IRoutingRuleValidatorService>();
				mockService
					.Setup(m => m.PerformValidationCheck(It.Is<RoutingRuleValidatorRequest>(request =>
						request.ServiceUrl == "https://ehub-routingws-test.wisegrid.net/RoutingRuleValidationWebService.svc"
						&& request.ClientId == "HYETSTTST"
						&& request.Password == "test"
						&& !string.IsNullOrWhiteSpace(request.Interchange))))
					.Returns(() => new RoutingRuleValidatorResponse { ValidationResult = true , RecipientIds = new string[1] { "recipient123" } });

				var validator = new RoutingRuleValidator(mockService.Object);
				Assert("expected to pass validation", validator.IsValid(Interchange));
				Assert("expected to pass validation", validator.IsValidWithRecipientIdsRetrieved(Interchange).ValidationResult);
				AssertArrayEqualsByElements("RecipientIds", new string[1] { "recipient123" }, validator.IsValidWithRecipientIdsRetrieved(Interchange).RecipientIds);

				mockService.Verify();
			}
		}

		public void TestRequestSettings_Production()
		{
			using (FreightDataRegistry.Instance.EnableCarrierMessagingConnectionValidationTestUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var mockService = new Mock<IRoutingRuleValidatorService>();
				mockService
					.Setup(m => m.PerformValidationCheck(It.Is<RoutingRuleValidatorRequest>(request =>
						request.ServiceUrl == "https://ehub-routingws.wisegrid.net/RoutingRuleValidationWebService.svc"
						&& request.ClientId == "EDIEDIDAT"
						&& request.Password == ""
						&& !string.IsNullOrWhiteSpace(request.Interchange))))
					.Returns(() => new RoutingRuleValidatorResponse { ValidationResult = true, RecipientIds = new string[1] { "recipient123" } });

				var validator = new RoutingRuleValidator(mockService.Object);
				Assert("expected to pass validation", validator.IsValid(Interchange));
				Assert("expected to pass validation", validator.IsValidWithRecipientIdsRetrieved(Interchange).ValidationResult);
				AssertArrayEqualsByElements("RecipientIds", new string[1] { "recipient123" }, validator.IsValidWithRecipientIdsRetrieved(Interchange).RecipientIds);

				mockService.Verify();
			}
		}

		public void TestRetryPolicyOnFailing()
		{
			using (FreightDataRegistry.Instance.EnableCarrierMessagingConnectionValidationTestUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var mockService = new Mock<IRoutingRuleValidationCheck>();
				mockService
					.Setup(m => m.SendValidationCheck(It.IsAny<RoutingEvaluationOCMInput>(),It.IsAny<ChannelFactory<IRoutingRuleValidationWebService>>()))
					.Returns(() => new RoutingRuleValidatorResponse { Exception = new Exception("something bad has happened") });

				var eHubRoutingRuleValidatorService = new EHubRoutingRuleValidatorService(mockService.Object);

				var validator = new RoutingRuleValidator(eHubRoutingRuleValidatorService);

				Assert("expected to pass validation and let eHub to handle it", validator.IsValid(Interchange));
				Assert("expected to pass validation and let eHub to handle it", validator.IsValidWithRecipientIdsRetrieved(Interchange).ValidationResult);
				AssertArrayEqualsByElements("RecipientIds", Array.Empty<string>(), validator.IsValidWithRecipientIdsRetrieved(Interchange).RecipientIds);

				mockService.Verify(ms => ms.SendValidationCheck(It.IsAny<RoutingEvaluationOCMInput>(), It.IsAny<ChannelFactory<IRoutingRuleValidationWebService>>()), Times.Exactly(12));
				mockService.Verify();
			}
		}

		const string Interchange =
@"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>EDIEDIDAT</SenderID>
  </Header>
  <Body>
    <UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
      <Shipment>
        <DataContext>
          <DataSource>
            <Key>C00001000</Key>
            <Type>ForwardingConsol</Type>
          </DataSource>
        </DataContext>
        <BookingConfirmationReference>BR001</BookingConfirmationReference>
        <CarrierBookingOffice></CarrierBookingOffice>
        <CoLoadBookingConfirmationReference></CoLoadBookingConfirmationReference>
        <CoLoadMasterBillNumber></CoLoadMasterBillNumber>
        <ContainerMode></ContainerMode>
        <DeliveryMode Description=""Peer To Peer"">PTP</DeliveryMode>
        <LloydsIMO></LloydsIMO>
        <PaymentMethod></PaymentMethod>
        <PlaceOfDelivery Name=""Shanghai Hongqiao International Apt"">CNSHA</PlaceOfDelivery>
        <PlaceOfIssue></PlaceOfIssue>
        <PlaceOfReceipt Name=""Sydney, Australia"">AUSYD</PlaceOfReceipt>
        <PortOfDestination Name=""Shanghai Hongqiao International Apt"">CNSHA</PortOfDestination>
        <PortOfDischarge Name=""Shanghai Hongqiao International Apt"">CNSHA</PortOfDischarge>
        <PortOfLoading Name=""Singapore, Singapore"">SGSIN</PortOfLoading>
        <PortOfOrigin Name=""Sydney, Australia"">AUSYD</PortOfOrigin>
        <ShipmentType Description=""Agent"">AGT</ShipmentType>
        <TransportMode></TransportMode>
        <VesselName>Black Hole</VesselName>
        <VoyageFlightNo>222</VoyageFlightNo>
        <WayBillNumber>BL091042970</WayBillNumber>
        <WayBillType Description=""Master Waybill"">MWB</WayBillType>
      </Shipment>
    </UniversalShipment>
  </Body>
</UniversalInterchange>";
	}
}
