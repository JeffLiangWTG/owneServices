using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Application;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Moq;
using NUnit.Framework;
using WTG.TestHelpers.Xml;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina.Testing
{
	[TestedType(typeof(ArgentinaEInvoiceXmlWriter))]
	class ArgentinaEInvoiceXmlWriterTest : TransactionBatchToXmlWriterTest
	{
		[ExpectNoExceptions]
		public void TestWriteXmlToStreamLocalEInvoiceXmlBuilder()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			var builderMock = new Mock<IEInvoiceXmlBuilder>();
			builderMock.Setup(x => x.BuildXml(It.IsAny<TransactionInfo>(), It.IsAny<AccEInvoicingBatch>())).Returns(new XStreamingElement("FECAESolicitar"));

			var localEInvoiceWriter = new ArgentinaEInvoiceXmlWriter();
			localEInvoiceWriter.SubstituteLocalEInvoiceXmlBuilder_ForTestOnly(builderMock.Object);
			var writer = (ITransactionBatchToPayloadWriter)localEInvoiceWriter;
			using (var stream = new MemoryStream())
			{
				var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
				writer.WritePayloadToStream(transactionBatch, stream, ArgentinaEInvoiceAPICommandList.Codes.GenerateLocalInvoiceRequest, accBatch, new Common.Logger(), new Common.Logger());
				builderMock.Verify(x => x.BuildXml(transactionInfo, accBatch), Times.Once);

				stream.Position = 0;
				var xmlDocument = new XmlDocument();
				xmlDocument.Load(stream);
				XmlComparison.CompareAndAssertXml(@"<?xml version=""1.0"" encoding=""utf-8""?><FECAESolicitar />", xmlDocument.OuterXml);
			}
		}

		[ExpectNoExceptions]
		public void TestWriteXmlToStreamExportEInvoiceXmlBuilder()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			var builderMock = new Mock<IEInvoiceXmlBuilder>();
			builderMock.Setup(x => x.BuildXml(It.IsAny<TransactionInfo>(), It.IsAny<AccEInvoicingBatch>())).Returns(new XStreamingElement("FEXAuthorize"));

			var exportEInvoiceWriter = new ArgentinaEInvoiceXmlWriter();
			exportEInvoiceWriter.SubstituteExportEInvoiceXmlBuilder_ForTestOnly(builderMock.Object);
			var writer = (ITransactionBatchToPayloadWriter)exportEInvoiceWriter;
			using (var stream = new MemoryStream())
			{
				var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
				writer.WritePayloadToStream(transactionBatch, stream, ArgentinaEInvoiceAPICommandList.Codes.GenerateExportInvoiceRequest, accBatch, new Common.Logger(), new Common.Logger());
				builderMock.Verify(x => x.BuildXml(transactionInfo, accBatch), Times.Once);

				stream.Position = 0;
				var xmlDocument = new XmlDocument();
				xmlDocument.Load(stream);
				XmlComparison.CompareAndAssertXml(@"<?xml version=""1.0"" encoding=""utf-8""?><FEXAuthorize />", xmlDocument.OuterXml);
			}
		}

		public void TestWriteXmlToStreamItemsDetailEInvoiceXmlBuilder()
		{
			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			var itemDetailEInvoiceXmlBuildernMock = new Mock<IItemDetailEInvoiceXmlBuilder>();
			itemDetailEInvoiceXmlBuildernMock.Setup(x => x.BuildXml(It.IsAny<TransactionInfo>(), It.IsAny<AccEInvoicingBatch>())).Returns(new XStreamingElement("autorizarComprobanteRequest"));

			var argentinaDependencyMock = new Mock<IArgentinaEInvoicingDependencyFactory>();
			argentinaDependencyMock.Setup(x => x.GetItemDetailEInvoiceXmlBuilder()).Returns(itemDetailEInvoiceXmlBuildernMock.Object);

			var eInvoicinigDependencyMock = new Mock<IEInvoicingDependencyFactory>();
			eInvoicinigDependencyMock.Setup(x => x.GetArgentinaEInvoicingDependencyFactory()).Returns(argentinaDependencyMock.Object);

			using (ObjectFactory.Substitute(eInvoicinigDependencyMock.Object))
			{
				var writer = new ArgentinaEInvoiceXmlWriter() as ITransactionBatchToPayloadWriter;

				using (var stream = new MemoryStream())
				{
					writer.WritePayloadToStream(transactionBatch, stream, ArgentinaEInvoiceAPICommandList.Codes.GenerateDetailItemsInvoiceRequest, accBatch, new Common.Logger(), new Common.Logger());
					itemDetailEInvoiceXmlBuildernMock.Verify(x => x.BuildXml(transactionInfo, accBatch), Times.Once);

					stream.Position = 0;
					var xmlDocument = new XmlDocument();
					xmlDocument.Load(stream);
					XmlComparison.CompareAndAssertXml(@"<?xml version=""1.0"" encoding=""utf-8""?><autorizarComprobanteRequest />", xmlDocument.OuterXml);

					eInvoicinigDependencyMock.Verify(x => x.GetArgentinaEInvoicingDependencyFactory(), Times.AtLeastOnce);
					argentinaDependencyMock.Verify(x => x.GetItemDetailEInvoiceXmlBuilder(), Times.Once);
				}
			}
		}

		public void TestWriteXmlToStreamNotSupportMessageType()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			using (var stream = new MemoryStream())
			{
				var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
				var writer = ((ITransactionBatchToPayloadWriter)GetTestWriter());
				AssertExceptionThrown<ArgumentException>("Invalid Message Type.", () => writer.WritePayloadToStream(transactionBatch, stream, string.Empty, accBatch, new Common.Logger(), new Common.Logger()));
			}
		}

		protected override TransactionBatchToPayloadWriterBase GetTestWriter() => new ArgentinaEInvoiceXmlWriter();

		protected override Type GetExpectedPayloadValidationType() => null;
	}
}
