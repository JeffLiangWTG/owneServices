using System;
using System.IO;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing
{
	[TestsSubclassesOf(typeof(TransactionBatchToXmlWriter))]
	abstract class TransactionBatchToXmlWriterTest : TransactionBatchToPayloadWriterBaseTest
	{
		protected override Type GetExpectedPayloadValidationType() => typeof(XsdValidation);

		public void TestWriteXmlToStreamWithInvalidTransactionBatch()
		{
			using (var testStream = new MemoryStream())
			{
				var writer = ((ITransactionBatchToPayloadWriter)GetTestWriter());
				var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
				TransactionBatch transactionBatch = null;

				AssertExceptionThrown<ArgumentException>("Transaction batch or collection can't be null.", () => writer.WritePayloadToStream(transactionBatch, testStream, string.Empty, accBatch, new Common.Logger(), new Common.Logger()));

				transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
				AssertExceptionThrown<ArgumentException>("Each transaction batch can have only one transaction.", () => writer.WritePayloadToStream(transactionBatch, testStream, string.Empty, accBatch, new Common.Logger(), new Common.Logger()));

				transactionBatch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance));
				transactionBatch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance));
				AssertExceptionThrown<ArgumentException>("Each transaction batch can have only one transaction.", () => writer.WritePayloadToStream(transactionBatch, testStream, string.Empty, accBatch, new Common.Logger(), new Common.Logger()));
			}
		}
	}
}
