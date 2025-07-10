using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.MessageSending;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	class CCF15AMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2011, 12, 13, 14, 15, 16)]
		public void TestCCF15AMessageBuilder()
		{
			var xmlFile = MessageBuilderUtilities.GetEmbeddedResourceFile("CCF15AMessageXml.xml");
			AssertEquals(xmlFile, messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		protected override void SetUp()
		{
			base.SetUp();
			dataProviderMock = new Mock<ICCF15ADeclaration>();
			dataProviderMock.Setup(m => m.AgreementNumber).Returns("12345678");
			dataProviderMock.Setup(m => m.IsProduction).Returns(new ZBool("1"));
			dataProviderMock.Setup(m => m.PrincipalTIN).Returns("FR0123456789002");
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("NCT00000001");
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns("MRN123");
			dataProviderMock.Setup(m => m.ValidationDate).Returns("20111213");
			dataProviderMock.Setup(m => m.AuthorisedLocationOfGoodsCode).Returns("PRE-LODGED");
			dataProviderMock.Setup(m => m.AgreedLocationOfGoods).Returns("LOC");
			dataProviderMock.Setup(m => m.AgreedLocationOfGoodsLanguage).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.IdentityOfMeansOfTransportAtDeparture).Returns("REG DEP1");
			dataProviderMock.Setup(m => m.IdentityOfMeansOfTransportAtDepartureLanguage).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.NationalityOfMeansOfTransportAtDeparture).Returns(Core.Constants.CountryCodes.NewZealand);
			dataProviderMock.Setup(m => m.ControlResultDateLimit).Returns("20111220");
			dataProviderMock.Setup(m => m.DepartureCustomsOfficeReferenceNumber).Returns("AA123456");

			var errorCollector = new ErrorCollector();
			messageBuilder = new CCF15AMessageBuilder(dataProviderMock.Object, new FRNctsMessageFunctionSet.PrelodgeValidationMessage(), errorCollector);
		}
		Mock<ICCF15ADeclaration> dataProviderMock;
		CCF15AMessageBuilder messageBuilder;
	}
}
