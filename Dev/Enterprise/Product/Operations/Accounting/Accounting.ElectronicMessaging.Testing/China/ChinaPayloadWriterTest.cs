using System;
using System.IO;
using System.Text;
using CargoWise.Common;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.China.Testing
{
	class ChinaPayloadWriterTest : TransactionBatchToPayloadWriterBaseTest
	{
		protected override TransactionBatchToPayloadWriterBase GetTestWriter() => new ChinaPayloadWriter();

		public void TestGetPayloadValidationWithMessageType()
		{
			var writer = ((ITransactionBatchToPayloadWriter)GetTestWriter());
			var validation = writer.GetPayloadValidation(ChinaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest);

			AssertType(typeof(JsonValidation2),validation);

			validation = writer.GetPayloadValidation(ChinaEInvoiceAPICommandList.Codes.RequestDocumentForInvoice);

			AssertType(typeof(JsonValidation2), validation);
		}

		public void TestUnknownMessageType_Throws()
		{
			var accBatch = SetUpInvoiceBatch().eInvoicingBatch;

			AssertExceptionThrown<ArgumentException>(()
				=> (new ChinaPayloadWriter() as ITransactionBatchToPayloadWriter).WritePayloadToStream(new TransactionBatch(), new MemoryStream(), "ZZZ", accBatch, new Common.Logger(), new Common.Logger())
			);
		}

		public void TestWritePayloadToStream_GEN()
		{
			AssertWritePayloadToStream(ChinaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest);
		}

		public void TestWritePayloadToStream_RDN()
		{
			AssertWritePayloadToStream(ChinaEInvoiceAPICommandList.Codes.RequestDocumentForInvoice);
		}

		void AssertWritePayloadToStream(string messageType)
		{
			using (var stream = new MemoryStream())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var accBatch = SetUpInvoiceBatch().eInvoicingBatch;
				(new ChinaPayloadWriter() as ITransactionBatchToPayloadWriter).WritePayloadToStream(new TransactionBatch(), stream, messageType, accBatch, new Common.Logger(), new Common.Logger());
				var actualJson = Encoding.UTF8.GetString(stream.ToArray());

				Assert(!actualJson.IsNullOrEmpty());
			}
		}

		(AccEInvoicingBatch eInvoicingBatch, InvoicingBase invoicingBase) SetUpInvoiceBatch()
		{
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV0001", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
			arInvoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETA;

			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var eInvoicingBatch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 10, Constants.EInvoicingBatchState.Ready);

			Factory.Save();

			return (eInvoicingBatch, arInvoice);
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator = new TestObjectCreator(Factory);
		}

		TestObjectCreator TestObjectCreator;
	}
}
