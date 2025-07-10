using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Messaging;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging.Testing
{
	class CC014AMessageBuilderTests : TestCaseWithFactory
	{
		[TestDate(2011, 12, 13, 14, 15, 16)]
		public void TestCC014AMessageBuilder()
		{
			var expectedXml = CTCMessageBuilderUtilities.GetEmbeddedResourceFile("CC014AMessageXml.xml");
			AssertEquals(expectedXml, messageBuilder.GetXMLMessageWithoutNamespaces());
			principalMock.VerifyAll();
			dataProviderMock.VerifyAll();
		}

		protected override void SetUp()
		{
			base.SetUp();

			principalMock = new Mock<ITrader>();
			principalMock.Setup(m => m.Name).Returns("PrincipalTraderName1");
			principalMock.Setup(m => m.StreetAndNumber).Returns("Street1");
			principalMock.Setup(m => m.PostalCode).Returns("PostCode1");
			principalMock.Setup(m => m.City).Returns("City1");
			principalMock.Setup(m => m.CountryCode).Returns("GB");
			principalMock.Setup(m => m.TIN).Returns("GB0123456789001");

			dataProviderMock = new Mock<ICC014ADeclaration>();
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns("19GB00006010021477");
			dataProviderMock.Setup(m => m.DateOfCancellationRequest).Returns("20111213");
			dataProviderMock.Setup(m => m.CancellationReason).Returns("Cancellation Reason1");
			dataProviderMock.Setup(m => m.CancellationReasonLanguage).Returns("EN-US");
			dataProviderMock.Setup(m => m.Principal).Returns(principalMock.Object);
			dataProviderMock.Setup(m => m.DepartureCustomsOfficeReferenceNumber).Returns("GB000001");
			dataProviderMock.Setup(m => m.IsTIRDeclaration).Returns(false);

			errorCollector = new ErrorCollector();
			messageBuilder = new CC014AXmlMessageBuilder(dataProviderMock.Object, errorCollector);
		}

		Mock<ITrader> principalMock;
		Mock<ICC014ADeclaration> dataProviderMock;
		ErrorCollector errorCollector;
		CC014AXmlMessageBuilder messageBuilder;
	}
}

