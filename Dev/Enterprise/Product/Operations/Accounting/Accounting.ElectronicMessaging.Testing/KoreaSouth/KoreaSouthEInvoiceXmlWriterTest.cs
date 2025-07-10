using System.IO;
using System.Xml.Linq;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Moq;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	class KoreaSouthEInvoiceXmlWriterTest : TransactionBatchToPayloadWriterBaseTest
	{
		public void TestWriteDocumentBodyProcessorIsCalled_GenerateInvoice()
		{
			var mockTaxInvoiceBuilder = new Mock<TaxInvoiceBuilder>(new TaxInvoiceValidation(), new AdditionalInfoConverter());
			mockTaxInvoiceBuilder.Setup(x => x.BuildTaxInvoice(It.IsAny<TransactionInfo>(), It.IsAny<AccEInvoicingBatch>(), It.IsAny<INotifications>())).Returns(() => new XStreamingElement("AAA"));
			ITransactionBatchToPayloadWriter writer = new KoreaSouthEInvoiceXmlWriter(mockTaxInvoiceBuilder.Object);

			using (var stream = new MemoryStream())
			{
				var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
				var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
				transactionBatch.TransactionCollection.Add(transactionInfo);

				writer.WritePayloadToStream(transactionBatch, stream, KoreaSouthEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, accBatch, new Logger(), new Logger());

				stream.Position = 0;
				AssertEquals(@"<?xml version=""1.0"" encoding=""utf-8""?><TaxInvoiceSet><AAA /></TaxInvoiceSet>", new StreamReader(stream).ReadToEnd());
			}

			AssertNoExceptionThrown(() => mockTaxInvoiceBuilder.Verify(x => x.BuildTaxInvoice(It.IsAny<TransactionInfo>(), It.IsAny<AccEInvoicingBatch>(), It.IsAny<INotifications>()), Times.Once));
		}

		public void TestWriteDocumentBodyProcessorIsCalled_QueryInvoice()
		{
			var mockTaxInvoiceBuilder = new Mock<TaxInvoiceBuilder>(new TaxInvoiceValidation(), new AdditionalInfoConverter());
			ITransactionBatchToPayloadWriter writer = new KoreaSouthEInvoiceXmlWriter(mockTaxInvoiceBuilder.Object);
			using (var stream = new MemoryStream())
			{
				var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
				var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
				transactionBatch.TransactionCollection.Add(transactionInfo);

				writer.WritePayloadToStream(transactionBatch, stream, KoreaSouthEInvoiceAPICommandList.Codes.QueryInvoiceRequest, accBatch, new Logger(), new Logger());

				stream.Position = 0;
				AssertEquals(@"<?xml version=""1.0"" encoding=""utf-8""?><Empty />", new StreamReader(stream).ReadToEnd());
			}
			AssertNoExceptionThrown(() => mockTaxInvoiceBuilder.Verify(x => x.BuildTaxInvoice(It.IsAny<TransactionInfo>(), It.IsAny<AccEInvoicingBatch>(), It.IsAny<INotifications>()), Times.Never));
		}

		public void TestWritePayloadToStream_GenerateInvoiceInBatch()
		{
			var mockTaxInvoiceBuilder = new Mock<TaxInvoiceBuilder>(new TaxInvoiceValidation(), new AdditionalInfoConverter());
			mockTaxInvoiceBuilder.Setup(x => x.BuildTaxInvoice(It.IsAny<TransactionInfo>(), It.IsAny<AccEInvoicingBatch>(), It.IsAny<INotifications>())).Returns(() => new XStreamingElement("AAA"));
			ITransactionBatchToPayloadWriter writer = new KoreaSouthEInvoiceXmlWriter(mockTaxInvoiceBuilder.Object);

			using (var stream = new MemoryStream())
			{
				var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
				var transactionInfo1 = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				var transactionInfo2 = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
				transactionBatch.TransactionCollection.Add(transactionInfo1);
				transactionBatch.TransactionCollection.Add(transactionInfo2);

				writer.WritePayloadToStream(transactionBatch, stream, KoreaSouthEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, accBatch, new Logger(), new Logger());

				stream.Position = 0;
				AssertEquals(@"<?xml version=""1.0"" encoding=""utf-8""?><TaxInvoiceSet><AAA /><AAA /></TaxInvoiceSet>", new StreamReader(stream).ReadToEnd());
			}

			AssertNoExceptionThrown(() => mockTaxInvoiceBuilder.Verify(x => x.BuildTaxInvoice(It.IsAny<TransactionInfo>(), It.IsAny<AccEInvoicingBatch>(), It.IsAny<INotifications>()), Times.Exactly(2)));
		}

		public void TestWritePayloadToStream_QueryInvoiceInBatch()
		{
			var mockTaxInvoiceBuilder = new Mock<TaxInvoiceBuilder>(new TaxInvoiceValidation(), new AdditionalInfoConverter());
			ITransactionBatchToPayloadWriter writer = new KoreaSouthEInvoiceXmlWriter(mockTaxInvoiceBuilder.Object);

			using (var stream = new MemoryStream())
			{
				var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
				var transactionInfo1 = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				var transactionInfo2 = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
				transactionBatch.TransactionCollection.Add(transactionInfo1);
				transactionBatch.TransactionCollection.Add(transactionInfo2);

				writer.WritePayloadToStream(transactionBatch, stream, KoreaSouthEInvoiceAPICommandList.Codes.QueryInvoiceRequest, accBatch, new Logger(), new Logger());

				stream.Position = 0;
				AssertEquals(@"<?xml version=""1.0"" encoding=""utf-8""?><Empty />", new StreamReader(stream).ReadToEnd());
			}

			AssertNoExceptionThrown(() => mockTaxInvoiceBuilder.Verify(x => x.BuildTaxInvoice(It.IsAny<TransactionInfo>(), It.IsAny<AccEInvoicingBatch>(), It.IsAny<INotifications>()), Times.Never));
		}

		public override void TestGetPayloadValidation()
		{
			ITransactionBatchToPayloadWriter writer = GetTestWriter();

			AssertNull("Invalid Message Type.It should return null.", writer.GetPayloadValidation(""));

			var validationGenerateInvoiceRequest = writer.GetPayloadValidation(KoreaSouthEInvoiceAPICommandList.Codes.GenerateInvoiceRequest);
			AssertType<GenerateInvoiceRequestPayloadValidation>(validationGenerateInvoiceRequest);

			var validationQueryInvoiceRequest = writer.GetPayloadValidation(KoreaSouthEInvoiceAPICommandList.Codes.QueryInvoiceRequest);
			AssertType<QueryInvoiceRequestPayloadValidation>(validationQueryInvoiceRequest);
		}

		protected override TransactionBatchToPayloadWriterBase GetTestWriter() => new KoreaSouthEInvoiceXmlWriter(new TaxInvoiceBuilder(new TaxInvoiceValidation(), new AdditionalInfoConverter()));
	}
}
