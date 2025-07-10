using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using NUnit.Framework;
using WTG.TestHelpers.Xml;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico.Testing
{
	public class CFDiObtenerPDFBuilderTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestBuilder_UsesGetGovernmentNumber_WithCorrectAuthorizationDetailCollection()
		{
			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();

			var eInvoicingDependencyMock = new Mock<IEInvoicingDependencyFactory>();
			eInvoicingDependencyMock.Setup(x => x.GetTransactionInfoHelper()).Returns(transactionInfoHelperMock.Object);

			ObjectFactory.Substitute(eInvoicingDependencyMock.Object);

			AssertUsesGetGovernmentNumber_WithCorrectAuthorizationDetailCollection(null);

			var authorizationDetails = new List<AuthorizationDetails>()
			{
				new AuthorizationDetails() { Purpose = new CodeDescriptionPair4Char() { Code = "XXX" }, GovernmentNumber = "1234" },
			};
			AssertUsesGetGovernmentNumber_WithCorrectAuthorizationDetailCollection(authorizationDetails);

			void AssertUsesGetGovernmentNumber_WithCorrectAuthorizationDetailCollection(List<AuthorizationDetails> authorizationDetails)
			{
				var obtenerPDFBuilder = new CFDiObtenerPDFBuilder() as ICFDiObtenerPDFBuilder;
				var obtenerPDFXml = obtenerPDFBuilder.BuildXml(authorizationDetails);

				transactionInfoHelperMock.Verify(x => x.GetGovernmentNumber(authorizationDetails));
			}
		}

		public void TestBuilder_GenerateXMLMessage_WithCorrectData()
		{
			var expectedEmptyXml = @"<ObtenerPDF>
  <UUID>{UUID}</UUID>
</ObtenerPDF>";

			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();

			var eInvoicinigDependencyMock = new Mock<IEInvoicingDependencyFactory>();
			eInvoicinigDependencyMock.Setup(x => x.GetTransactionInfoHelper()).Returns(transactionInfoHelperMock.Object);

			ObjectFactory.Substitute(eInvoicinigDependencyMock.Object);

			AssertGenerateXMLMessage_WithCorrectData(string.Empty);
			AssertGenerateXMLMessage_WithCorrectData("MX123456");

			void AssertGenerateXMLMessage_WithCorrectData(string expectedValue)
			{
				transactionInfoHelperMock.Setup(x => x.GetGovernmentNumber(It.IsAny<List<AuthorizationDetails>>())).Returns(expectedValue);

				var obtenerPDFBuilder = new CFDiObtenerPDFBuilder() as ICFDiObtenerPDFBuilder;
				var obtenerPDFXml = obtenerPDFBuilder.BuildXml(new List<AuthorizationDetails>()).ToString();

				XmlComparison.CompareAndAssertXml(expectedEmptyXml.Replace("{UUID}", expectedValue), obtenerPDFXml);
			}
		}
	}
}
