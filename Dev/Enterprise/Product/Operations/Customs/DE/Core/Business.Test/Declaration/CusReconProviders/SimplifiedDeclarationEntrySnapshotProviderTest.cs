using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class SimplifiedDeclarationEntrySnapshotProviderTest : TestCaseWithFactory
	{
		public void TestReferenceNumber()
		{
			var cfcrecHeaderMock = new Mock<ICFCRECHeader>();
			var provider = new SimplifiedDeclarationEntrySnapshotProvider(cfcrecHeaderMock.Object);
			AssertNull(provider.ReferenceNumber);
		}

		public void TestConsignee()
		{
			var cfcrecHeaderMock = new Mock<ICFCRECHeader>();
			var provider = new SimplifiedDeclarationEntrySnapshotProvider(cfcrecHeaderMock.Object);
			AssertNull(provider.Consignee);
		}

		public void TestConsigneePK()
		{
			var cfcrecHeaderMock = new Mock<ICFCRECHeader>();
			var provider = new SimplifiedDeclarationEntrySnapshotProvider(cfcrecHeaderMock.Object);
			AssertEquals(Guid.Empty, provider.ConsigneePK);
		}

		public void TestDeliveryTermsCode()
		{
			var cfcrecHeaderMock = new Mock<ICFCRECHeader>();
			var provider = new SimplifiedDeclarationEntrySnapshotProvider(cfcrecHeaderMock.Object);
			AssertNull(provider.DeliveryTermsCode);
		}

		public void TestDeliveryTermsDescription()
		{
			var cfcrecHeaderMock = new Mock<ICFCRECHeader>();
			var provider = new SimplifiedDeclarationEntrySnapshotProvider(cfcrecHeaderMock.Object);
			AssertNull(provider.DeliveryTermsDescription);
		}

		public void TestDeliveryTermsPlace()
		{
			var cfcrecHeaderMock = new Mock<ICFCRECHeader>();
			var provider = new SimplifiedDeclarationEntrySnapshotProvider(cfcrecHeaderMock.Object);
			AssertNull(provider.DeliveryTermsPlace);
		}

		public void TestDeliveryTermsKey()
		{
			var cfcrecHeaderMock = new Mock<ICFCRECHeader>();
			var provider = new SimplifiedDeclarationEntrySnapshotProvider(cfcrecHeaderMock.Object);
			AssertNull(provider.DeliveryTermsKey);
		}

		public void TestPaymentTransaction()
		{
			var cfcrecHeaderMock = new Mock<ICFCRECHeader>();
			var provider = new SimplifiedDeclarationEntrySnapshotProvider(cfcrecHeaderMock.Object);
			AssertNull(provider.PaymentTransaction);
		}

		public void TestForeignTradeStatisticsEntryCustomsOffice()
		{
			var cfcrecHeaderMock = new Mock<ICFCRECHeader>();
			var provider = new SimplifiedDeclarationEntrySnapshotProvider(cfcrecHeaderMock.Object);
			AssertNull(provider.ForeignTradeStatisticsEntryCustomsOffice);
		}

		public void TestCustomsValue()
		{
			var cfcrecHeaderMock = new Mock<ICFCRECHeader>();
			var provider = new SimplifiedDeclarationEntrySnapshotProvider(cfcrecHeaderMock.Object);
			AssertNull(provider.CustomsValue);
		}

		public void TestDocuments()
		{
			var cfcrecHeaderMock = new Mock<ICFCRECHeader>();
			var provider = new SimplifiedDeclarationEntrySnapshotProvider(cfcrecHeaderMock.Object);
			var documents = new[]
			{
				new Mock<IImportDocument>().Object,
				new Mock<IImportDocument>().Object
			};
			cfcrecHeaderMock.Setup(x => x.Documents).Returns(documents);
			AssertEquals(documents, provider.Documents);
		}
	}
}

