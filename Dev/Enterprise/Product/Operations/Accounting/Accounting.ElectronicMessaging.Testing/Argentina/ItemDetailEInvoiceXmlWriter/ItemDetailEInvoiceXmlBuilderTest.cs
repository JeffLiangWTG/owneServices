using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Moq;
using NUnit.Framework;
using WTG.TestHelpers.Xml;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina.Testing
{
	public class ItemDetailEInvoiceXmlBuilderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestBuildXml()
		{
			var authRequest = new XStreamingElement("authRequest");
			var authRequestBuilderMock = new Mock<IAuthRequestBuilder>();
			authRequestBuilderMock.Setup(x => x.BuildXML(It.IsAny<TransactionInfo>())).Returns(() => authRequest);

			var comprobanteCAERequest = new XStreamingElement("comprobanteCAERequest");
			var comprobanteCAERequestBuilderMock = new Mock<IComprobanteCAERequestBuilder>();
			comprobanteCAERequestBuilderMock.Setup(x => x.BuildXML(It.IsAny<TransactionInfo>(), It.IsAny<BusinessObjectFactory>())).Returns(() => comprobanteCAERequest);

			var argentinaDependencyMock = new Mock<IArgentinaEInvoicingDependencyFactory>();
			argentinaDependencyMock.Setup(x => x.GetAuthRequestBuilder()).Returns(authRequestBuilderMock.Object);
			argentinaDependencyMock.Setup(x => x.GetComprobanteCAERequestBuilder()).Returns(comprobanteCAERequestBuilderMock.Object);

			var eInvoicinigDependencyMock = new Mock<IEInvoicingDependencyFactory>();
			eInvoicinigDependencyMock.Setup(x => x.GetArgentinaEInvoicingDependencyFactory()).Returns(argentinaDependencyMock.Object);

			var expectedXmlValue = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ser=""http://impl.service.wsmtxca.afip.gov.ar/service/"">
  <soapenv:Header />
  <soapenv:Body>
    <ser:autorizarComprobanteRequest>
      <authRequest />
      <comprobanteCAERequest />
    </ser:autorizarComprobanteRequest>
  </soapenv:Body>
</soapenv:Envelope>";

			using (ObjectFactory.Substitute(eInvoicinigDependencyMock.Object))
			{
				var builder = new ItemDetailEInvoiceXmlBuilder() as IItemDetailEInvoiceXmlBuilder;
				var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { Branch = new Branch() };

				var actualXml = builder.BuildXml(transaction, accBatch).ToString();

				XmlComparison.CompareAndAssertXml(expectedXmlValue, actualXml);

				authRequestBuilderMock.Verify(x => x.BuildXML(transaction), Times.Once);
				comprobanteCAERequestBuilderMock.Verify(x => x.BuildXML(transaction, Factory), Times.Once);

				argentinaDependencyMock.Verify(x => x.GetAuthRequestBuilder(), Times.Once);
				argentinaDependencyMock.Verify(x => x.GetComprobanteCAERequestBuilder(), Times.Once);
				eInvoicinigDependencyMock.Verify(x => x.GetArgentinaEInvoicingDependencyFactory(), Times.Once);
			}
		}
	}
}
