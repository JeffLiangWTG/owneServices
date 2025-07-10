using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.Export.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey.Testing
{
	public class LegalMonetaryTotalTest : TestCaseWithFactory
	{
		readonly TurkeyEInvoiceTestHelper Helper = new TurkeyEInvoiceTestHelper();

		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestLegalMonetary()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoice = Helper.CreateTestARInvoice(TestObjectCreator.CC14, "charge1", TestObjectCreator.KDV18.PK.ToGuid(), "A00002", 1034m, TestObjectCreator.TRY);
				var invoiceBatch = Helper.CreateInvoiceBatch(invoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());

				using (var transactionBatch = exporter.CreateTransactionBatch(invoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

					var lineExtensionAmount = eInvoice.Invoice.LegalMonetaryTotal.LineExtensionAmount;
					var taxExclusiveAmount = eInvoice.Invoice.LegalMonetaryTotal.TaxExclusiveAmount;
					var taxInclusiveAmount = eInvoice.Invoice.LegalMonetaryTotal.TaxInclusiveAmount;
					var payableAmount = eInvoice.Invoice.LegalMonetaryTotal.PayableAmount;

					AssertEquals("TRY", lineExtensionAmount.currencyID);
					AssertEquals(1034m, lineExtensionAmount.Value);
					AssertEquals("TRY", taxExclusiveAmount.currencyID);
					AssertEquals(1034m, taxExclusiveAmount.Value);
					AssertEquals("TRY", taxInclusiveAmount.currencyID);
					AssertEquals(1220,12m, taxInclusiveAmount.Value);
					AssertEquals("TRY", payableAmount.currencyID);
					AssertEquals(1220.12m, payableAmount.Value);
				}
			}
		}

		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestLegalMonetaryForeignCurrency()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoice = Helper.CreateTestARInvoice(TestObjectCreator.CC14, "charge1", TestObjectCreator.KDV18.PK.ToGuid(), "A00002", 1034m, TestObjectCreator.EUR);
				var invoiceBatch = Helper.CreateInvoiceBatch(invoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());

				using (var transactionBatch = exporter.CreateTransactionBatch(invoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

					var lineExtensionAmount = eInvoice.Invoice.LegalMonetaryTotal.LineExtensionAmount;
					var taxExclusiveAmount = eInvoice.Invoice.LegalMonetaryTotal.TaxExclusiveAmount;
					var taxInclusiveAmount = eInvoice.Invoice.LegalMonetaryTotal.TaxInclusiveAmount;
					var payableAmount = eInvoice.Invoice.LegalMonetaryTotal.PayableAmount;

					AssertEquals("EUR", lineExtensionAmount.currencyID);
					AssertEquals(1034m, lineExtensionAmount.Value);
					AssertEquals("EUR", taxExclusiveAmount.currencyID);
					AssertEquals(1034m, taxExclusiveAmount.Value);
					AssertEquals("EUR", taxInclusiveAmount.currencyID);
					AssertEquals(1220.12m, taxInclusiveAmount.Value);
					AssertEquals("EUR", payableAmount.currencyID);
					AssertEquals(1220.12m, payableAmount.Value);
				}
			}
		}

		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestLegalMonetaryForeignCurrency_InvoiceHasWithholdingTax()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoice = Helper.CreateTestARInvoice(TestObjectCreator.CC14, "charge1", TestObjectCreator.KDV18W5.PK, "A00002", 1034m, TestObjectCreator.EUR);
				var invoiceBatch = Helper.CreateInvoiceBatch(invoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());

				using (var transactionBatch = exporter.CreateTransactionBatch(invoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

					var lineExtensionAmount = eInvoice.Invoice.LegalMonetaryTotal.LineExtensionAmount;
					var taxExclusiveAmount = eInvoice.Invoice.LegalMonetaryTotal.TaxExclusiveAmount;
					var taxInclusiveAmount = eInvoice.Invoice.LegalMonetaryTotal.TaxInclusiveAmount;
					var payableAmount = eInvoice.Invoice.LegalMonetaryTotal.PayableAmount;

					AssertEquals("EUR", lineExtensionAmount.currencyID);
					AssertEquals(1034m, lineExtensionAmount.Value);
					AssertEquals("EUR", taxExclusiveAmount.currencyID);
					AssertEquals(1034m, taxExclusiveAmount.Value);
					AssertEquals("EUR", taxInclusiveAmount.currencyID);
					AssertEquals(1220.12m, taxInclusiveAmount.Value);
					AssertEquals("EUR", payableAmount.currencyID);
					AssertEquals(1127.06m, payableAmount.Value);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var connection = ((IDbConnectionInternals)base.TestConnection).ADOConnection;
			var transaction = ((IDbConnectionInternals)base.TestConnection).ADOTransaction;
			DataAccess = new BatchExportDataAccess(connection, transaction);
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		TestObjectCreator TestObjectCreator;
		BatchExportDataAccess DataAccess;
	}
}
