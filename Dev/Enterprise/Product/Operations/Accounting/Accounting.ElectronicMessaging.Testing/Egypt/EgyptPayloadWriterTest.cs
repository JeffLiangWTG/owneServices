using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Egypt.Testing
{
	class EgyptPayloadWriterTest : TransactionBatchToPayloadWriterBaseTest
	{
		protected override TransactionBatchToPayloadWriterBase GetTestWriter() => new EgyptPayloadWriter();

		public void TestZeroTransactionsInBatch_WritesEnvelopeOnly_AndLogsWarning()
		{
			var batch = CreateTransactionsWithAuthorisationContent(0);

			using (var stream = new MemoryStream())
			{
				var errors = new Common.Logger();
				var warnings = new Common.Logger();
				((ITransactionBatchToPayloadWriter)new EgyptPayloadWriter()).WritePayloadToStream(new TransactionBatch(), stream, "***", batch, errors, warnings);

				var actualContent = Encoding.UTF8.GetString(stream.ToArray());
				var expectedContent = "<submission><documents></documents></submission>";
				AssertEquals(expectedContent, actualContent);

				AssertNullOrEmpty(errors.ToString());
				AssertEquals("E-Invoicing batch contains no signed E-Invoice document(s).", warnings.ToString());
			}
		}

		public void TestZeroAuthRecords_WritesEnvelopeOnly_AndLogsWarning()
		{
			var creator = new TestObjectCreator(Factory);
			var batch = creator.CreateEInvoicingBatch(10, Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var arInvoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV0001", creator.AUD, 1m, 100m, 0m, 100m, 0m, creator.AALSHI, creator.CC1.PK);
			creator.CreateEInvoicingTransactionPivot(batch, arInvoice, Constants.EInvoicingPivotState.Batched);
			Factory.Save();

			using (var stream = new MemoryStream())
			{
				var errors = new Common.Logger();
				var warnings = new Common.Logger();
				((ITransactionBatchToPayloadWriter)new EgyptPayloadWriter()).WritePayloadToStream(new TransactionBatch(), stream, "***", batch, errors, warnings);

				var actualContent = Encoding.UTF8.GetString(stream.ToArray());
				var expectedContent = "<submission><documents></documents></submission>";
				AssertEquals(expectedContent, actualContent);

				AssertNullOrEmpty(errors.ToString());
				AssertEquals("E-Invoicing batch contains no signed E-Invoice document(s).", warnings.ToString());
			}
		}

		public void TestMissingAuthRecord_WritesOneDocument_AndLogsWarning()
		{
			var batch = CreateTransactionsWithAuthorisationContent(2, authRecordFirstDodgyParentId: ZGuid.NewZGuid());

			using (var stream = new MemoryStream())
			{
				var errors = new Common.Logger();
				var warnings = new Common.Logger();
				((ITransactionBatchToPayloadWriter)new EgyptPayloadWriter()).WritePayloadToStream(new TransactionBatch(), stream, "***", batch, errors, warnings);

				var actualContent = Encoding.UTF8.GetString(stream.ToArray());
				var expectedContent = @"<submission><documents>
<document><number>1</number><additional>abcd</additional></document>
</documents></submission>".Replace("\n", "").Replace("\r", "");
				AssertEquals(expectedContent, actualContent);

				AssertNullOrEmpty(errors.ToString());
				AssertEquals("Unexpected number of signed E-Invoice documents (1) compared to transactions in batch (2).", warnings.ToString());
			}
		}

		public void TestEmptyAuthRecord_WritesEnvelopeOnly_AndLogsWarning()
		{
			var batch = CreateTransactionsWithAuthorisationContent(1, contentTemplate: "");

			using (var stream = new MemoryStream())
			{
				var errors = new Common.Logger();
				var warnings = new Common.Logger();
				((ITransactionBatchToPayloadWriter)new EgyptPayloadWriter()).WritePayloadToStream(new TransactionBatch(), stream, "***", batch, errors, warnings);

				var actualContent = Encoding.UTF8.GetString(stream.ToArray());
				var expectedContent = "<submission><documents></documents></submission>";
				AssertEquals(expectedContent, actualContent);

				AssertNullOrEmpty(errors.ToString());
				AssertEquals("One or more transactions are missing expected signed E-Invoice document.", warnings.ToString());
			}
		}

		public void TestOneAuthRecord_WritesOneDocument()
		{
			var batch = CreateTransactionsWithAuthorisationContent(1);

			using (var stream = new MemoryStream())
			{
				var errors = new Common.Logger();
				var warnings = new Common.Logger();
				((ITransactionBatchToPayloadWriter)new EgyptPayloadWriter()).WritePayloadToStream(new TransactionBatch(), stream, "***", batch, errors, warnings);

				var actualContent = Encoding.UTF8.GetString(stream.ToArray());
				var expectedContent = @"<submission><documents>
<document><number>0</number><additional>abcd</additional></document>
</documents></submission>".Replace("\n", "").Replace("\r", "");
				AssertEquals(expectedContent, actualContent);

				AssertNullOrEmpty(errors.ToString());
				AssertNullOrEmpty(warnings.ToString());
			}
		}

		public void TestThreeAuthRecords_WritesThreeDocuments()
		{
			var batch = CreateTransactionsWithAuthorisationContent(3);

			using (var stream = new MemoryStream())
			{
				var errors = new Common.Logger();
				var warnings = new Common.Logger();
				((ITransactionBatchToPayloadWriter)new EgyptPayloadWriter()).WritePayloadToStream(new TransactionBatch(), stream, "***", batch, errors, warnings);

				var actualContent = Encoding.UTF8.GetString(stream.ToArray());
				AssertContains("<submission><documents>", actualContent);
				AssertContains("<document><number>0</number><additional>abcd</additional></document>", actualContent);
				AssertContains("<document><number>1</number><additional>abcd</additional></document>", actualContent);
				AssertContains("<document><number>2</number><additional>abcd</additional></document>", actualContent);
				AssertContains("</documents></submission>", actualContent);

				AssertNullOrEmpty(errors.ToString());
				AssertNullOrEmpty(warnings.ToString());
			}
		}

		public void TestArabicScript_IsWritten()
		{
			var batch = CreateTransactionsWithAuthorisationContent(1, additionalContent: "بجانب جامع الرحمة");
			using (var stream = new MemoryStream())
			{
				var errors = new Common.Logger();
				var warnings = new Common.Logger();
				((ITransactionBatchToPayloadWriter)new EgyptPayloadWriter()).WritePayloadToStream(new TransactionBatch(), stream, "***", batch, errors, warnings);

				var actualContent = Encoding.UTF8.GetString(stream.ToArray());
				var expectedContent = @"<submission><documents>
<document><number>0</number><additional>بجانب جامع الرحمة</additional></document>
</documents></submission>".Replace("\n", "").Replace("\r", "");
				AssertEquals(expectedContent, actualContent);

				AssertNullOrEmpty(errors.ToString());
				AssertNullOrEmpty(warnings.ToString());
			}
		}

		#region Helpers

		AccEInvoicingBatch CreateTransactionsWithAuthorisationContent(
			int count,
			string additionalContent = "abcd",
			string contentTemplate = "<document><number>{0}</number><additional>{1}</additional></document>",
			ZGuid? authRecordFirstDodgyParentId = null)
		{
			var creator = new TestObjectCreator(Factory);
			var result = creator.CreateEInvoicingBatch(10, Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);

			for (int i = 0; i < count; i++)
			{
				var arInvoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV000" + i, creator.AUD, 1m, 100m + i, 0m, 100m + i, 0m, creator.AALSHI, creator.CC1.PK);
				var authorisation = Factory.New<AccTransactionHeaderAuthorisationRecord>();
				authorisation.AHF_ParentId = (i == 0 ? authRecordFirstDodgyParentId.GetValueOrDefault(arInvoice.PK) : arInvoice.PK);
				authorisation.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
				authorisation.AHF_RecordType = AccTransactionHeaderAuthorisationRecordTypes.Egypt;
				var authorisationContent = string.Format(CultureInfo.InvariantCulture, contentTemplate, i, additionalContent);
				authorisation.AHF_AuthorisationData = ZBlob.FromUTF8(authorisationContent);

				creator.CreateEInvoicingTransactionPivot(result, arInvoice, Constants.EInvoicingPivotState.Batched);
			}
			Factory.Save();

			return result;
		}

		#endregion
	}
}
