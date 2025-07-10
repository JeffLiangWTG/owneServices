using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.Export.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey.Testing
{
	public class InvoiceLineTest : TestCaseWithFactory
	{
		readonly TurkeyEInvoiceTestHelper Helper = new TurkeyEInvoiceTestHelper();

		public void TestInvoiceLineWithNormalTaxRate()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoicingBatch = Helper.CreateTestARInvoiceBatch(Helper.TestObjectCreator.CC14, "charge1", Helper.TestObjectCreator.KDV18.PK.ToGuid(), "A00001", 1000m, Helper.TestObjectCreator.TRY);
				var exportor = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exportor.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);
					var invoiceLine = eInvoice.Invoice.InvoiceLine[0];

					AssertEquals("1", invoiceLine.ID.Value);
					AssertEquals("charge1", invoiceLine.Note[0].Value);
					AssertEquals("C62", invoiceLine.InvoicedQuantity.unitCode);
					AssertEquals(1m, invoiceLine.InvoicedQuantity.Value);
					AssertEquals(1000m, invoiceLine.LineExtensionAmount.Value);
					AssertEquals("TRY", invoiceLine.LineExtensionAmount.currencyID);

					var taxTotal = invoiceLine.TaxTotal;
					AssertEquals(180m, taxTotal.TaxAmount.Value);
					AssertEquals("TRY", taxTotal.TaxAmount.currencyID);

					var taxSubTotal = taxTotal.TaxSubtotal[0];
					AssertEquals(1000m, taxSubTotal.TaxableAmount.Value);
					AssertEquals("TRY", taxSubTotal.TaxableAmount.currencyID);
					AssertEquals(180m, taxSubTotal.TaxAmount.Value);
					AssertEquals("TRY", taxSubTotal.TaxAmount.currencyID);
					AssertEquals(18m, taxSubTotal.Percent.Value);

					var taxScheme = taxSubTotal.TaxCategory.TaxScheme;
					AssertEquals("KDV", taxScheme.Name.Value);
					AssertEquals("0015", taxScheme.TaxTypeCode.Value);

					var item = invoiceLine.Item;
					AssertEquals("ZZCC14", item.ID.Value);
					AssertEquals("Charge Code 14", item.Description.Value);
					AssertEquals("charge1", item.Name.Value);
					AssertEquals("charge1", item.BrandName.Value);
					AssertEquals("charge1", item.ModelName.Value);
					AssertEquals("ZZCC14", item.SellersItemIdentification.ID.Value);

					var priceAmount = invoiceLine.Price.PriceAmount;
					AssertEquals(1000m, priceAmount.Value);
					AssertEquals("TRY", priceAmount.currencyID);
				}
			}
		}

		public void TestInvoiceLineWithWithholdingTaxRate()
		{
			AssertInvoiceLineWithWithholdingTaxRateCases(true);
		}

		public void TestInvoiceLineWithWithholdingTaxRate_MissingTaxMessages()
		{
			//See: TestInvoiceLineWithExemption_MissingTaxMessages
			AssertInvoiceLineWithWithholdingTaxRateCases(false);
		}

		void AssertInvoiceLineWithWithholdingTaxRateCases(bool includeTaxMessages = true)
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				// Use the built-in KDV18W5 definition or create a broken KDV18W5 without tax message, depending on the case being tested:
				var taxRate = includeTaxMessages ? Helper.TestObjectCreator.KDV18W5 : Helper.TestObjectCreator.CreateTaxRate("KDV18W5", "18% Standard VAT - Withholding 50%", AccTaxRate.Types.Rated, 18, "REF", 5, 10, "TR");
				AssertNotEquals($"Precondition: TaxMessage must be {(includeTaxMessages ? "included" : "missing")} for this test.", includeTaxMessages, taxRate.AT_A9_DefaultVatClass.IsEmpty);

				var invoicingBatch = Helper.CreateTestARInvoiceBatch(Helper.TestObjectCreator.CC14, "charge1", taxRate.PK, "A00001", 1325m, Helper.TestObjectCreator.TRY);
				var exportor = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exportor.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);
					var invoiceLine = eInvoice.Invoice.InvoiceLine[0];

					AssertEquals("1", invoiceLine.ID.Value);
					AssertEquals("charge1", invoiceLine.Note[0].Value);
					AssertEquals("C62", invoiceLine.InvoicedQuantity.unitCode);
					AssertEquals(1m, invoiceLine.InvoicedQuantity.Value);
					AssertEquals(1325m, invoiceLine.LineExtensionAmount.Value);
					AssertEquals("TRY", invoiceLine.LineExtensionAmount.currencyID);

					var taxTotal = invoiceLine.TaxTotal;
					AssertEquals(238,5m, taxTotal.TaxAmount.Value);
					AssertEquals("TRY", taxTotal.TaxAmount.currencyID);

					var taxSubTotal = taxTotal.TaxSubtotal[0];
					AssertEquals(1325m, taxSubTotal.TaxableAmount.Value);
					AssertEquals("TRY", taxSubTotal.TaxableAmount.currencyID);
					AssertEquals(238,5m, taxSubTotal.TaxAmount.Value);
					AssertEquals("TRY", taxSubTotal.TaxAmount.currencyID);
					AssertEquals(18m, taxSubTotal.Percent.Value);

					var taxScheme = taxSubTotal.TaxCategory.TaxScheme;
					AssertEquals("KDV", taxScheme.Name.Value);
					AssertEquals("0015", taxScheme.TaxTypeCode.Value);

					var withholdingTaxTotal = invoiceLine.WithholdingTaxTotal;
					AssertNotNull(withholdingTaxTotal);
					AssertEquals(119,25m, withholdingTaxTotal[0].TaxAmount.Value);
					AssertEquals("TRY", withholdingTaxTotal[0].TaxAmount.currencyID);

					var withholdingTaxSubTotal = withholdingTaxTotal[0].TaxSubtotal[0];
					AssertEquals(238,5m, withholdingTaxSubTotal.TaxableAmount.Value);
					AssertEquals("TRY", withholdingTaxSubTotal.TaxableAmount.currencyID);
					AssertEquals(119,25m, withholdingTaxSubTotal.TaxAmount.Value);
					AssertEquals("TRY", withholdingTaxSubTotal.TaxAmount.currencyID);
					AssertEquals(50m, withholdingTaxSubTotal.Percent.Value);

					var withholdingTaxScheme = withholdingTaxSubTotal.TaxCategory.TaxScheme;
					AssertEquals("TEVKIFAT", withholdingTaxScheme.Name.Value);
					AssertEquals(includeTaxMessages ? "606" : "", withholdingTaxScheme.TaxTypeCode.Value);

					var item = invoiceLine.Item;
					AssertEquals("ZZCC14", item.ID.Value);
					AssertEquals("Charge Code 14", item.Description.Value);
					AssertEquals("charge1", item.Name.Value);
					AssertEquals("charge1", item.BrandName.Value);
					AssertEquals("charge1", item.ModelName.Value);
					AssertEquals("ZZCC14", item.SellersItemIdentification.ID.Value);

					var priceAmount = invoiceLine.Price.PriceAmount;
					AssertEquals(1325m, priceAmount.Value);
					AssertEquals("TRY", priceAmount.currencyID);
				}
			}
		}

		public void TestInvoiceLineWithExemption()
		{
			AssertInvoiceLineWithExemptionCases(true, "606", "301 Nolu Tevkifat Kodu");
		}

		public void TestInvoiceLineWithExemption_MissingTaxMessages()
		{
			// This actually is not a valid business case because Turkish government mandates tax messages with every invoice line with exemptions.
			// However, it is possible to post a tax-exempt invoice without tax messages (and trigger a nullref exception) under the following registry configuration:
			// Accounting.Registry.Business.AccountingConfigurationRegistry.Instance.TaxMessageIsMandatoryReceivables == TaxMessageMandatoryOptions.NotRequired
			AssertInvoiceLineWithExemptionCases(false, "351", "KDV den istisnadır");
		}

		void AssertInvoiceLineWithExemptionCases(bool includeTaxMessages, string taxExemptionReasonCode, string taxExemptionReason)
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoicingBatch = Helper.CreateInvoiceWithExemption(Helper.TestObjectCreator.TRY, includeTaxMessages);
				var exportor = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());

				using (var transactionBatch = exportor.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace))
				using (var uInvoice = transactionBatch.TransactionCollection.First())
				{
					var invoiceInfoTag = new InvoiceInfoTag(uInvoice, GlbCompany.CurrentCompany);
					var eInvoice = invoiceInfoTag.BuildInvoiceInfo();

					var invoiceLine = eInvoice.Invoice.InvoiceLine[0];
					AssertEquals("1", invoiceLine.ID.Value);
					AssertEquals("charge3", invoiceLine.Note[0].Value);
					AssertEquals("C62", invoiceLine.InvoicedQuantity.unitCode);
					AssertEquals(1m, invoiceLine.InvoicedQuantity.Value);
					AssertEquals(5450m, invoiceLine.LineExtensionAmount.Value);
					AssertEquals("TRY", invoiceLine.LineExtensionAmount.currencyID);

					var taxSubTotal = invoiceLine.TaxTotal.TaxSubtotal[0];
					AssertEquals(5450m, taxSubTotal.TaxableAmount.Value);
					AssertEquals("TRY", taxSubTotal.TaxableAmount.currencyID);
					AssertEquals(0m, taxSubTotal.TaxAmount.Value);
					AssertEquals("TRY", taxSubTotal.TaxAmount.currencyID);
					AssertEquals(0m, taxSubTotal.Percent.Value);

					var taxCategory = taxSubTotal.TaxCategory;
					AssertEquals(taxExemptionReasonCode, taxCategory.TaxExemptionReasonCode.Value);
					AssertEquals(taxExemptionReason, taxCategory.TaxExemptionReason.Value);

					var taxScheme = taxSubTotal.TaxCategory.TaxScheme;
					AssertEquals("KDV", taxScheme.Name.Value);
					AssertEquals("0015", taxScheme.TaxTypeCode.Value);

					var item = invoiceLine.Item;
					AssertEquals("ZZCC7", item.ID.Value);
					AssertEquals("Charge Code 7", item.Description.Value);
					AssertEquals("charge3", item.Name.Value);
					AssertEquals("charge3", item.BrandName.Value);
					AssertEquals("charge3", item.ModelName.Value);
					AssertEquals("ZZCC7", item.SellersItemIdentification.ID.Value);

					var priceAmount = invoiceLine.Price.PriceAmount;
					AssertEquals(5450m, priceAmount.Value);
					AssertEquals("TRY", priceAmount.currencyID);
				}
			}
		}

		public void TestInvoiceLineWithNormalTaxRateForeignCurrency()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoicingBatch = Helper.CreateTestARInvoiceBatch(Helper.TestObjectCreator.CC14, "charge1", Helper.TestObjectCreator.KDV18.PK.ToGuid(), "A00001", 1000m, Helper.TestObjectCreator.EUR);
				var exportor = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exportor.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);
					var invoiceLine = eInvoice.Invoice.InvoiceLine[0];

					AssertEquals("1", invoiceLine.ID.Value);
					AssertEquals("charge1", invoiceLine.Note[0].Value);
					AssertEquals("C62", invoiceLine.InvoicedQuantity.unitCode);
					AssertEquals(1m, invoiceLine.InvoicedQuantity.Value);
					AssertEquals(1000m, invoiceLine.LineExtensionAmount.Value);
					AssertEquals("EUR", invoiceLine.LineExtensionAmount.currencyID);

					var taxTotal = invoiceLine.TaxTotal;
					AssertEquals(180m, taxTotal.TaxAmount.Value);
					AssertEquals("EUR", taxTotal.TaxAmount.currencyID);

					var taxSubTotal = taxTotal.TaxSubtotal[0];
					AssertEquals(1000m, taxSubTotal.TaxableAmount.Value);
					AssertEquals("EUR", taxSubTotal.TaxableAmount.currencyID);
					AssertEquals(180m, taxSubTotal.TaxAmount.Value);
					AssertEquals("EUR", taxSubTotal.TaxAmount.currencyID);
					AssertEquals(18m, taxSubTotal.Percent.Value);

					var taxScheme = taxSubTotal.TaxCategory.TaxScheme;
					AssertEquals("KDV", taxScheme.Name.Value);
					AssertEquals("0015", taxScheme.TaxTypeCode.Value);

					var item = invoiceLine.Item;
					AssertEquals("ZZCC14", item.ID.Value);
					AssertEquals("Charge Code 14", item.Description.Value);
					AssertEquals("charge1", item.Name.Value);
					AssertEquals("charge1", item.BrandName.Value);
					AssertEquals("charge1", item.ModelName.Value);
					AssertEquals("ZZCC14", item.SellersItemIdentification.ID.Value);

					var priceAmount = invoiceLine.Price.PriceAmount;
					AssertEquals(1000m, priceAmount.Value);
					AssertEquals("EUR", priceAmount.currencyID);
				}
			}
		}

		public void TestInvoiceLineWithWithholdingTaxRateForeignCurrency()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoicingBatch = Helper.CreateTestARInvoiceBatch(Helper.TestObjectCreator.CC14, "charge1", Helper.TestObjectCreator.KDV18W7.PK, "A00001", 1000m, Helper.TestObjectCreator.EUR);
				var exportor = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exportor.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);
					var invoiceLine = eInvoice.Invoice.InvoiceLine[0];

					AssertEquals("1", invoiceLine.ID.Value);
					AssertEquals("charge1", invoiceLine.Note[0].Value);
					AssertEquals("C62", invoiceLine.InvoicedQuantity.unitCode);
					AssertEquals(1m, invoiceLine.InvoicedQuantity.Value);
					AssertEquals(1000m, invoiceLine.LineExtensionAmount.Value);
					AssertEquals("EUR", invoiceLine.LineExtensionAmount.currencyID);

					var taxTotal = invoiceLine.TaxTotal;
					AssertEquals(180m, taxTotal.TaxAmount.Value);
					AssertEquals("EUR", taxTotal.TaxAmount.currencyID);

					var taxSubTotal = taxTotal.TaxSubtotal[0];
					AssertEquals(1000m, taxSubTotal.TaxableAmount.Value);
					AssertEquals("EUR", taxSubTotal.TaxableAmount.currencyID);
					AssertEquals(180m, taxSubTotal.TaxAmount.Value);
					AssertEquals("EUR", taxSubTotal.TaxAmount.currencyID);
					AssertEquals(18m, taxSubTotal.Percent.Value);

					var taxScheme = taxSubTotal.TaxCategory.TaxScheme;
					AssertEquals("KDV", taxScheme.Name.Value);
					AssertEquals("0015", taxScheme.TaxTypeCode.Value);

					var withholdingTaxTotal = invoiceLine.WithholdingTaxTotal;
					AssertNotNull(withholdingTaxTotal);
					AssertEquals(126m, withholdingTaxTotal[0].TaxAmount.Value);
					AssertEquals("EUR", withholdingTaxTotal[0].TaxAmount.currencyID);

					var withholdingTaxSubTotal = withholdingTaxTotal[0].TaxSubtotal[0];
					AssertEquals(180m, withholdingTaxSubTotal.TaxableAmount.Value);
					AssertEquals("EUR", withholdingTaxSubTotal.TaxableAmount.currencyID);
					AssertEquals(126m, withholdingTaxSubTotal.TaxAmount.Value);
					AssertEquals("EUR", withholdingTaxSubTotal.TaxAmount.currencyID);
					AssertEquals(70m, withholdingTaxSubTotal.Percent.Value);

					var withholdingTaxScheme = withholdingTaxSubTotal.TaxCategory.TaxScheme;
					AssertEquals("TEVKIFAT", withholdingTaxScheme.Name.Value);
					AssertEquals("606", withholdingTaxScheme.TaxTypeCode.Value);

					var item = invoiceLine.Item;
					AssertEquals("ZZCC14", item.ID.Value);
					AssertEquals("Charge Code 14", item.Description.Value);
					AssertEquals("charge1", item.Name.Value);
					AssertEquals("charge1", item.BrandName.Value);
					AssertEquals("charge1", item.ModelName.Value);
					AssertEquals("ZZCC14", item.SellersItemIdentification.ID.Value);

					var priceAmount = invoiceLine.Price.PriceAmount;
					AssertEquals(1000m, priceAmount.Value);
					AssertEquals("EUR", priceAmount.currencyID);
				}
			}
		}

		public void TestInvoiceLineWithExemptionForeignCurrency()
		{
			AssertInvoiceLineWithExemptionForeignCurrencyCases(true, "606", "301 Nolu Tevkifat Kodu");
		}

		public void TestInvoiceLineWithExemptionForeignCurrency_MissingTaxMessages()
		{
			//See: TestInvoiceLineWithExemption_MissingTaxMessages
			AssertInvoiceLineWithExemptionForeignCurrencyCases(false, "351", "KDV den istisnadır");
		}

		void AssertInvoiceLineWithExemptionForeignCurrencyCases(bool includeTaxMessages, string taxExemptionReasonCode, string taxExemptionReason)
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoicingBatch = Helper.CreateInvoiceWithExemption(Helper.TestObjectCreator.EUR, includeTaxMessages);
				var exportor = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());

				using (var transactionBatch = exportor.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace))
				using (var uInvoice = transactionBatch.TransactionCollection.First())
				{
					var invoiceInfoTag = new InvoiceInfoTag(uInvoice, GlbCompany.CurrentCompany);
					var eInvoice = invoiceInfoTag.BuildInvoiceInfo();

					var invoiceLine = eInvoice.Invoice.InvoiceLine[0];
					AssertEquals("1", invoiceLine.ID.Value);
					AssertEquals("charge3", invoiceLine.Note[0].Value);
					AssertEquals("C62", invoiceLine.InvoicedQuantity.unitCode);
					AssertEquals(1m, invoiceLine.InvoicedQuantity.Value);
					AssertEquals(5450m, invoiceLine.LineExtensionAmount.Value);
					AssertEquals("EUR", invoiceLine.LineExtensionAmount.currencyID);

					var taxSubTotal = invoiceLine.TaxTotal.TaxSubtotal[0];
					AssertEquals(5450m, taxSubTotal.TaxableAmount.Value);
					AssertEquals("EUR", taxSubTotal.TaxableAmount.currencyID);
					AssertEquals(0m, taxSubTotal.TaxAmount.Value);
					AssertEquals("EUR", taxSubTotal.TaxAmount.currencyID);
					AssertEquals(0m, taxSubTotal.Percent.Value);

					var taxCategory = taxSubTotal.TaxCategory;
					AssertEquals(taxExemptionReasonCode, taxCategory.TaxExemptionReasonCode.Value);
					AssertEquals(taxExemptionReason, taxCategory.TaxExemptionReason.Value);

					var taxScheme = taxSubTotal.TaxCategory.TaxScheme;
					AssertEquals("KDV", taxScheme.Name.Value);
					AssertEquals("0015", taxScheme.TaxTypeCode.Value);

					var item = invoiceLine.Item;
					AssertEquals("ZZCC7", item.ID.Value);
					AssertEquals("Charge Code 7", item.Description.Value);
					AssertEquals("charge3", item.Name.Value);
					AssertEquals("charge3", item.BrandName.Value);
					AssertEquals("charge3", item.ModelName.Value);
					AssertEquals("ZZCC7", item.SellersItemIdentification.ID.Value);

					var priceAmount = invoiceLine.Price.PriceAmount;
					AssertEquals(5450m, priceAmount.Value);
					AssertEquals("EUR", priceAmount.currencyID);
				}
			}
		}

		public void TestNoCommentLinesInInvoice()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var job = Helper.TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var invoice = Helper.CreateTestARInvoiceWithCommentLine(Helper.TestObjectCreator.CC14, "charge1", Helper.TestObjectCreator.KDV18.PK.ToGuid(), "AR001", 1000m, Helper.TestObjectCreator.TRY);
				var eInvoiceBatch = Helper.CreateInvoiceBatch(invoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());

				using (var transactionBatch = exporter.CreateTransactionBatch(eInvoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

					AssertNotNull(eInvoice.Invoice.InvoiceLine[0].Note[0]);
					AssertEquals(false, eInvoice.Invoice.InvoiceLine.Any(x => x.Note[0].Value == "CommentInvoiceLine"));
					AssertEquals(1m, eInvoice.Invoice.LineCountNumeric.Value);
				}
			}
		}

		public void TestLineCountOfInvoiceWithNoCommentLines()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, Helper.TestObjectCreator.EUR);
				var eInvoiceBatch = Helper.CreateInvoiceBatch(invoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());

				using (var transactionBatch = exporter.CreateTransactionBatch(eInvoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

					AssertEquals(3m, eInvoice.Invoice.LineCountNumeric.Value);
					AssertEquals(false, eInvoice.Invoice.InvoiceLine.Any(x => x.Note[0].Value == "CommentInvoiceLine"));
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var connection = ((IDbConnectionInternals)base.TestConnection).ADOConnection;
			var transaction = ((IDbConnectionInternals)base.TestConnection).ADOTransaction;
			DataAccess = new BatchExportDataAccess(connection, transaction);
		}

		BatchExportDataAccess DataAccess;
	}
}
