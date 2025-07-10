using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBR5SIMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2014, 05, 06)]
		public void TestGenerateDeclaration()
		{
			var importHeaderMock = new Mock<IImport5SIHeader>();
			importHeaderMock.Setup(m => m.ImportDeclarationNumber).Returns("9999907000001X");
			importHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("010");
			importHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns("20");

			var importLine1 = new Mock<IImport5SILine>();
			importLine1.Setup(m => m.ParcelNumber).Returns("EM123456789KR");
			importLine1.Setup(m => m.ParcelCustomsNumber).Returns("0100254646646");
			importLine1.Setup(m => m.DeliveryType).Returns("A");
			var importLine2 = new Mock<IImport5SILine>();
			importLine2.Setup(m => m.ParcelNumber).Returns("EM987654321KR");
			importLine2.Setup(m => m.ParcelCustomsNumber).Returns("0156547892321");
			importLine2.Setup(m => m.DeliveryType).Returns("B");

			importHeaderMock.Setup(m => m.MailItemIDs).Returns(new IImport5SILine[] { importLine1.Object, importLine2.Object });

			var result = new GOVCBR5SIMessageBuilder(importHeaderMock.Object).GenerateMessage();

			AssertEquals("01020", result.DeclarationOfficeId.Value);
			AssertEquals("9", result.FunctionCode.Value);
			AssertEquals("9999907000001X", result.Id.Value);
			AssertEquals("20140506", result.IssueDateTime);
			AssertEquals("EM123456789KR", result.GoodsShipment[0].AdditionalDocument[0].Value);
			AssertEquals("0100254646646", result.GoodsShipment[0].AdditionalDocument[1].Value);
			AssertEquals("A", result.GoodsShipment[0].AdditionalInformation.StatementCode.Value);
			AssertEquals("EM987654321KR", result.GoodsShipment[1].AdditionalDocument[0].Value);
			AssertEquals("0156547892321", result.GoodsShipment[1].AdditionalDocument[1].Value);
			AssertEquals("B", result.GoodsShipment[1].AdditionalInformation.StatementCode.Value);

			importHeaderMock.VerifyAll();
			importLine1.VerifyAll();
			importLine2.VerifyAll();
		}
	}
}
