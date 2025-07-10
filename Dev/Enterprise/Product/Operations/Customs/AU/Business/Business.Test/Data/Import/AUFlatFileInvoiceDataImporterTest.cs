using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUFlatFileInvoiceDataImporterTest : DataTransfer.Testing.FlatFileInvoiceDataImporterAbstractTest
	{
		public void TestAUImport_Legacy()
		{
			Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;

			var importer = GetFlatFileInvoiceDataImporterLocal(pathToTestFile);
			importer.Import();

			var invoice = Declaration.Invoices[0];
			AssertEquals("Invoice Preference", "P", invoice.AddInfo.ZA_PRF);
			AssertEquals("Invoice Preference Origin", "", invoice.AddInfo.ZA_POC);
			AssertEquals("Invoice Preference Scheme", "", invoice.AddInfo.ZA_PST);
			AssertEquals("Invoice Preference Rule", "", invoice.AddInfo.ZA_PRT);
			AssertEquals("Invoice GST", "TAXE", invoice.AddInfo.ZA_GSTE);
			AssertEquals("Related", "N", invoice.AddInfo.ZA_HeaderREL_Hidden);

			var invoiceLine = Declaration.FilteredInvoiceLines[0];
			AssertEquals("InvoiceLine AUState", "JKT", invoiceLine.AddInfo.ZA_AUState_Hidden);
			AssertEquals("InvoiceLine TreatmentCoe", "333", invoiceLine.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("InvoiceLine Preference", "Q", invoiceLine.AddInfo.ZA_PRF);
			AssertEquals("InvoiceLine Preference Origin", "", invoiceLine.AddInfo.ZA_POC);
			AssertEquals("InvoiceLine Preference Scheme", "", invoiceLine.AddInfo.ZA_PST);
			AssertEquals("InvoiceLine Preference Rule", "", invoiceLine.AddInfo.ZA_PRT);
		}

		public void TestAUImport_CMR()
		{
			Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			var importer = GetFlatFileInvoiceDataImporterLocal(pathToTestFile);
			importer.Import();

			var invoice = Declaration.Invoices[0];
			AssertEquals("Invoice Preference", "", invoice.AddInfo.ZA_PRF);
			AssertEquals("Invoice Preference Origin", "US", invoice.AddInfo.ZA_POC);
			AssertEquals("Invoice Preference Scheme", "USA", invoice.AddInfo.ZA_PST);
			AssertEquals("Invoice Preference Rule", "PS", invoice.AddInfo.ZA_PRT);
			AssertEquals("Invoice GST", "TAXE", invoice.AddInfo.ZA_GSTE);
			AssertEquals("Related", "Y", invoice.AddInfo.ZA_HeaderREL_Hidden);

			var invoiceLine = Declaration.FilteredInvoiceLines[0];
			AssertEquals("InvoiceLine AUState", "JKT", invoiceLine.AddInfo.ZA_AUState_Hidden);
			AssertEquals("InvoiceLine TreatmentCoe", "333", invoiceLine.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("InvoiceLine Preference", "", invoiceLine.AddInfo.ZA_PRF);
			AssertEquals("InvoiceLine Preference Origin", "AD", invoiceLine.AddInfo.ZA_POC);
			AssertEquals("InvoiceLine Preference Scheme", "ADL", invoiceLine.AddInfo.ZA_PST);
			AssertEquals("InvoiceLine Preference Rule", "WH", invoiceLine.AddInfo.ZA_PRT);
			AssertEquals("InvoiceLine AddInfo", "ORG=ID*POC=AD*PRT=WH*PST=ADL*RNO=03", invoiceLine.AddInfo.AddInfoLine);
		}

		public void TestEnsureThatTreatmentCodeExistsForTariff()
		{
			var importer = GetFlatFileInvoiceDataImporterLocal(pathToTestFile);
			importer.Import();

			AssertEquals("Treatment Code not set", "333", Declaration.FilteredInvoiceLines[0].AddInfo.ZA_TreatmentCode_Hidden);
		}

		protected override FlatFileInvoiceDataImporter GetFlatFileInvoiceDataImporter(ZString sourceFile) => GetFlatFileInvoiceDataImporterLocal(sourceFile);

		protected override BaseJobDeclaration GetNewJobDeclaration() => JobDeclaration.New(Factory);

		protected override void SetUp()
		{
			base.SetUp();
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
			pathToTestFile = embeddedResourceRetriever.SaveResourceToFile("Enterprise.Customs.AU.Declaration.Business.Testing.Data.Import.TestFiles.InvoiceImporter1.csv");
		}

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever.Dispose();
		}
		string pathToTestFile;
		EmbeddedResourceRetriever embeddedResourceRetriever;

		AUFlatFileInvoiceDataImporter GetFlatFileInvoiceDataImporterLocal(ZString sourceFile) => new AUFlatFileInvoiceDataImporter(sourceFile, Declaration);

		JobDeclaration Declaration => (JobDeclaration)declaration;
	}
}
