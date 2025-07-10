using System;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM484ProviderTest : TestCaseWithFactory
	{
		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN", provider.LocalReferenceNumber);
		}

		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345CDEFG678R9", provider.MovementReferenceNumber);
		}

		public void TestRequestDate()
		{
			AssertEquals(new DateTime(2023, 09, 20), provider.RequestDate);
		}

		public void TestDateLimit()
		{
			AssertEquals(new DateTime(2023, 09, 21), provider.DateLimit);
		}

		public void TestAdditionalInformation()
		{
			var additionalInformations = provider.AdditionalInformations.ToArray();
			AssertEquals("AdditionalInformation should has 2 objects.", 2, additionalInformations.Length);
			CombineAssertions(() =>
			{
				AssertEquals("DocumentType is not expected", "D001", additionalInformations[0].DocumentType);
				AssertEquals("DocumentComplementaryInformation is not expected", "DocInfo1", additionalInformations[0].RequestInformation);

				AssertEquals("DocumentType is not expected", "D002", additionalInformations[1].DocumentType);
				AssertEquals("DocumentComplementaryInformation is not expected", "DocInfo2", additionalInformations[1].RequestInformation);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var mockImportOperation = new Mock<IIMCciOperationType55>();
			mockImportOperation.Setup(o => o.Mrn).Returns("12MRN345CDEFG678R9");
			mockImportOperation.Setup(o => o.Lrn).Returns("LRN");
			mockImportOperation.Setup(o => o.RequestDate).Returns(new DateTime(2023, 09, 20));
			mockImportOperation.Setup(o => o.DateLimit).Returns(new DateTime(2023, 09, 21));

			var mockDocumentAdditionalInformation1 = new Mock<IDocumentAdditionalInformation>();
			mockDocumentAdditionalInformation1.Setup(o => o.DocumentType).Returns("D001");
			mockDocumentAdditionalInformation1.Setup(o => o.RequestInformation).Returns("DocInfo1");

			var mockDocumentAdditionalInformation2 = new Mock<IDocumentAdditionalInformation>();
			mockDocumentAdditionalInformation2.Setup(o => o.DocumentType).Returns("D002");
			mockDocumentAdditionalInformation2.Setup(o => o.RequestInformation).Returns("DocInfo2");

			var mock = new Mock<IIM484XmlObject>();
			mock.Setup(o => o.ImportOperation).Returns(mockImportOperation.Object);
			mock.Setup(o => o.GoodsShipments).Returns(new[] { mockDocumentAdditionalInformation1.Object, mockDocumentAdditionalInformation2.Object });

			provider = new IM484Provider(mock.Object);
		}

		IM484Provider provider;
	}
}
