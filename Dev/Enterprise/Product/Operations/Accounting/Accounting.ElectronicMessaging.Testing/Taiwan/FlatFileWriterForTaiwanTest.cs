using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Taiwan.Testing
{
	public class FlatFileWriterForTaiwanTest : TestCaseWithFactory
	{
		[TestDate(2019, 6, 3, 14, 42, 23)]
		public void TestWriteFlatFileToStream()
		{
			var arInvoice1 = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", ObjectCreator.AUD, 1.0m, 100.00m, 5.00m, 100.00m, 5.00m);
			arInvoice1.Lines[0].AL_AT = VATRate.PK;
			var arInvoice2 = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "002", ObjectCreator.AUD, 1.0m, 200.00m, 10.00m, 200.00m, 10.00m);
			arInvoice2.Lines[0].AL_AT = VATRate.PK;
			var complianceDocument1 = ObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA001", "TXE", "desc", arInvoice1.Lines[0], ObjectCreator.Debtor);
			var complianceDocument2 = ObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA002", "TXE", "desc", arInvoice2.Lines[0], ObjectCreator.Debtor);
			complianceDocument2.ADH_ApprovalNumber = "ApprovalNumber\nTest";
			complianceDocument2.ADH_VoidingReason = "VoidingReason\nTest";
			var arComplianceDocument2 = Factory.Load<ARComplianceDocumentHeader>(complianceDocument2.PK);
			Factory.Save();

			arComplianceDocument2.Void();
			Factory.Save();

			var batch = ObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			ObjectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument1, Core.Constants.EInvoicingPivotState.Batched);
			ObjectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument2, Core.Constants.EInvoicingPivotState.Batched);
			Factory.Save();

			var expectedContent = $@"M|O|AA001|2019/06/03 14:42:23|||||||||100||0.05|5|105|||test|2019060300001000||||||||||||Y
D|desc|1|100|100|||0|0|2019060300001000||||||||||||
M|C|AA002|2019/06/03 14:42:23||20190603|||||||200||0.05|10|210|||test|2019060300001001||||||||||||Y|VoidingReason Test|ApprovalNumber Test
D|desc|1|200|200|||0|0|2019060300001001||||||||||||
2";

				AssertWriteFlatFileToStream(batch, expectedContent);
		}

		[TestDate(2019, 6, 3, 14, 42, 23)]
		public void TestWriteFlatFileToStreamWithLineBreak()
		{
			var arInvoice1 = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", ObjectCreator.AUD, 1.0m, 100.00m, 5.00m, 100.00m, 5.00m);
			arInvoice1.Lines[0].AL_AT = VATRate.PK;
			var complianceDocument1 = ObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test\nagain", "AA001", "TXE", "de\nsc\n", arInvoice1.Lines[0], ObjectCreator.Debtor);

			var address = ObjectCreator.Debtor.MainAddressCollection[0];
			address.CompanyName = "AAAAA\nAAAAAA\n";
			arInvoice1.AH_OA_InvoiceAddressOverride = address.PK;
			complianceDocument1.ADH_OA_AddressOverride = address.PK;

			Factory.Save();

			var batch = ObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			ObjectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument1, Core.Constants.EInvoicingPivotState.Batched);
			Factory.Save();

			var expectedContent = $@"M|O|AA001|2019/06/03 14:42:23||||||||AAAAA AAAAAA |100||0.05|5|105|||test again|2019060300001000||||||||||||Y
D|de sc |1|100|100|||0|0|2019060300001000||||||||||||
1";

			AssertWriteFlatFileToStream(batch, expectedContent);
		}

		[TestDate(2019, 6, 3, 14, 42, 23)]
		public void TestWriteFlatFileToStreamWhenDebtorIsNAT()
		{
			ObjectCreator.Debtor.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			AssertWriteFlatFileToStreamWhenDebtorIsB2C();
		}

		[TestDate(2019, 6, 3, 14, 42, 23)]
		public void TestWriteFlatFileToStreamWhenDebtorIsBUSAndNotTW()
		{
			ObjectCreator.Debtor.OH_Category = OrgConstants.Category.Business;
			ObjectCreator.Debtor.OH_RL_NKClosestPort = "AUSYD";
			AssertWriteFlatFileToStreamWhenDebtorIsB2C();
		}

		void AssertWriteFlatFileToStreamWhenDebtorIsB2C()
		{
			var cusCode4 = ObjectCreator.Debtor.CustomsCodes.AddNew();
			cusCode4.OK_CodeType = "MCI";
			cusCode4.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.Country.Code;
			cusCode4.OK_CustomsRegNo = "xxyu/123";

			var arInvoice1 = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", ObjectCreator.AUD, 1.0m, 100.00m, 5.00m, 100.00m, 5.00m);
			arInvoice1.Lines[0].AL_AT = VATRate.PK;
			var arInvoice2 = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "002", ObjectCreator.AUD, 1.0m, 200.00m, 10.00m, 200.00m, 10.00m);
			arInvoice2.Lines[0].AL_AT = VATRate.PK;
			var complianceDocument1 = ObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA001", "TXE", "desc", arInvoice1.Lines[0], ObjectCreator.Debtor);
			var complianceDocument2 = ObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA002", "TXE", "desc", arInvoice2.Lines[0], ObjectCreator.Debtor);
			var batch = ObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			ObjectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument1, Core.Constants.EInvoicingPivotState.Batched);
			ObjectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument2, Core.Constants.EInvoicingPivotState.Batched);
			Factory.Save();

			var arComplianceDocument2 = Factory.Load<ARComplianceDocumentHeader>(complianceDocument2.PK);
			arComplianceDocument2.Void();
			Factory.Save();

			var expectedContent = $@"M|O|AA001|2019/06/03 14:42:23|||||||||105||0.05|0|105|||test|2019060300001000|||||||||3J0002|xxyu/123||N
D|desc|1|105|105|||0|0|2019060300001000||||||||||||
1";

			AssertWriteFlatFileToStream(batch, expectedContent);
		}

		[TestDate(2019, 6, 3, 14, 42, 23)]
		public void TestWriteFlatFileToStreamOfCRDComplianceDocument()
		{
			var arInvoice1 = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", ObjectCreator.AUD, 1.0m, 200.00m, 10.00m, 200.00m, 10.00m);
			arInvoice1.Lines[0].AL_AT = VATRate.PK;
			var arInvoice2 = ObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "002", ObjectCreator.AUD, 1.0m, 100.00m, 5.00m, 100.00m, 5.00m);
			arInvoice2.Lines[0].AL_AT = VATRate.PK;
			var complianceDocument1 = ObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA001", "TXE", @"desc
New Line", arInvoice1.Lines[0], ObjectCreator.Debtor);
			complianceDocument1.ADH_DocumentDate = ZDate.Today.AddDays(-1);
			complianceDocument1.ADH_ReportingPeriod = ZDateTime.Today.Year * 100 + ZDateTime.Today.Month;
			var complianceDocument2 = ObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA001", "TXE", "desc", arInvoice2.Lines[0], ObjectCreator.Debtor);
			Factory.Save();

			var arComplianceDocument2 = Factory.Load<ARComplianceDocumentHeader>(complianceDocument2.PK);
			arComplianceDocument2.Void();
			Factory.Save();

			var batch = ObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			ObjectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument1, Core.Constants.EInvoicingPivotState.Batched);
			ObjectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument2, Core.Constants.EInvoicingPivotState.Batched);
			Factory.Save();

			var expectedContent = $@"M|O|AA001|2019/06/02 00:00:00|||||||||200||0.05|10|210|||test|2019060200001000||||||||||||Y
D|desc New Line|1|200|200|||0|0|2019060200001000||||||||||||
M|D|||20190603|20190603|||||||100||0.05|5|105|||test|2019060300001000||||||||||||Y
D|desc|1|100|100|||0|0|2019060300001000||||||||AA001|2019/06/02 00:00:00|||
2";

			AssertWriteFlatFileToStream(batch, expectedContent);
		}

		void AssertWriteFlatFileToStream(AccEInvoicingBatch batch, string expectedContent)
		{
			var exportor = new ComplianceDocumentBatchExporter();
			var complianceBatch = exportor.CreateComplianceDocumentBatch(batch);

			using (var stream = new MemoryStream())
			{
				var writer = new FlatFileWriterForTaiwan();
				writer.WriteFlatFileToStream(complianceBatch.ComplianceDocumentHeaderDetails, stream, null);

				stream.Position = 0;
				string result = null;
				using (var reader = new StreamReader(stream))
				{
					result = reader.ReadToEnd();
				}
				AssertEquals(expectedContent, result);
			}
		}

		[TestDate(2019, 6, 3, 14, 42, 23)]
		public void TestGetHeaderDetailString()
		{
			var batch = GetComplianceDocumentBatch();
			var writer = new FlatFileWriterForTaiwan();
			var headerDetail = writer.GetHeaderDetailString_ForTestOnly(batch.ComplianceDocumentHeaderDetails[0], null);
			AssertEquals("HeaderDetailString", $"M|O|AA001|2019/06/03 14:42:23|||||||||100||0.05|5|105|||test|2019060300001000||||||||||||Y", headerDetail);
		}

		public void TestGetHeaderDetailStringWithErrorNotifications()
		{
			var batch = GetComplianceDocumentBatch("invalid | header | description");
			var writer = new FlatFileWriterForTaiwan();
			var testLogger = new NotificationBuffer();
			writer.GetHeaderDetailString_ForTestOnly(batch.ComplianceDocumentHeaderDetails[0], testLogger);
			Assert("Log has error", testLogger.HasErrors);
			AssertEquals("Error Message", "Failed to upload compliance document because AA001 has description which contains '|'", testLogger.AsString.TrimEnd());
		}

		[TestDate(2019, 6, 3, 14, 42, 23)]
		public void TestGetLineDetailString()
		{
			var batch = GetComplianceDocumentBatch();
			var writer = new FlatFileWriterForTaiwan();
			var lineDetail = writer.GetLineDetailString_ForTestOnly(batch.ComplianceDocumentHeaderDetails[0], batch.ComplianceDocumentHeaderDetails[0].ComplianceDocumentLineDetails[0]);
			AssertEquals("LineDetail", $"D|desc|1|100|100|||0|0|2019060300001000||||||||||||", lineDetail);
		}

		[TestDate(2019, 6, 3, 14, 42, 23)]
		public void TestInternalReference()
		{
			var batch = GetComplianceDocumentBatch();
			var writer = new FlatFileWriterForTaiwan();

			var internalReference = writer.GetInternalReference_ForTestOnly(batch.ComplianceDocumentHeaderDetails[0]);
			AssertEquals("Internal Reference's length should be 16 for upload", 16, internalReference.Length);
			AssertEquals("2019060300001000", internalReference);
		}

		ComplianceDocumentBatch GetComplianceDocumentBatch(string headerDesc = null)
		{
			var arInvoice = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", ObjectCreator.AUD, 1.0m, 100.00m, 5.00m, 100.00m, 5.00m);
			arInvoice.Lines[0].AL_AT = VATRate.PK;
			var complianceDocument = ObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, headerDesc ?? "test", "AA001", "TXE", "desc", arInvoice.Lines[0], ObjectCreator.Debtor);
			var batch = ObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			ObjectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument, Core.Constants.EInvoicingPivotState.Batched);
			Factory.Save();

			var exportor = new ComplianceDocumentBatchExporter();
			var complianceBatch = exportor.CreateComplianceDocumentBatch(batch);
			AssertNotNull(complianceBatch);
			AssertNotNull("Batch Header Details", complianceBatch.ComplianceDocumentHeaderDetails);
			AssertEquals("Count of Batch Header Details", 1, complianceBatch.ComplianceDocumentHeaderDetails.Length);
			AssertNotNull("Batch Line Details", complianceBatch.ComplianceDocumentHeaderDetails[0].ComplianceDocumentLineDetails);
			AssertEquals("Count of Batch Line Details", 1, complianceBatch.ComplianceDocumentHeaderDetails[0].ComplianceDocumentLineDetails.Length);

			return complianceBatch;
		}

		AccTaxRate fVATRate;
		AccTaxRate VATRate => fVATRate ?? (fVATRate = ObjectCreator.CreateTaxRate("VAT", "VAT Rate 1", AccTaxRate.Types.Rated, 5, string.Empty, 0, 1));

		protected override void SetUp()
		{
			base.SetUp();

			ObjectCreator = new TestObjectCreator(Factory);
			ObjectCreator.Debtor.OH_RL_NKClosestPort = "TWTPE";
		}

		TestObjectCreator ObjectCreator;
	}
}
