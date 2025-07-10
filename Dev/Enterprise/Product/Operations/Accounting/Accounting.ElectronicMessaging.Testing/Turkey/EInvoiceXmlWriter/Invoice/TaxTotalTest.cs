using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
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
	public class TaxTotalTest : TestCaseWithFactory
	{
		readonly TurkeyEInvoiceTestHelper Helper = new TurkeyEInvoiceTestHelper();

		#region No Withholding

		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestTaxTotalMappingOnTurkeyEInvoice()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoice = Helper.CreateTestARInvoice(Helper.TestObjectCreator.CC14, "charge1", Helper.TestObjectCreator.KDV18.PK.ToGuid(), "AR001", 1034m, Helper.TestObjectCreator.TRY);
				var eInvoiceBatch = Helper.CreateInvoiceBatch(invoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());

				using (var transactionBatch = exporter.CreateTransactionBatch(eInvoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

					AssertEquals(1, eInvoice.Invoice.TaxTotal.Length);
					var taxTotal = eInvoice.Invoice.TaxTotal[0];

					AssertEquals(186.12m, taxTotal.TaxAmount.Value);
					AssertEquals("TRY", taxTotal.TaxAmount.currencyID);
					AssertEquals(18m, taxTotal.TaxSubtotal[0].Percent.Value);
					AssertEquals(186.12m, taxTotal.TaxSubtotal[0].TaxAmount.Value);
					AssertEquals("TRY", taxTotal.TaxSubtotal[0].TaxAmount.currencyID);
					AssertEquals(1034m, taxTotal.TaxSubtotal[0].TaxableAmount.Value);
					AssertEquals("TRY", taxTotal.TaxSubtotal[0].TaxableAmount.currencyID);

					AssertNull(eInvoice.Invoice.WithholdingTaxTotal);
				}
			}
		}

		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestTaxTotalMappingOnTurkeyEInvoiceForeignCurrency()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoice = Helper.CreateTestARInvoice(Helper.TestObjectCreator.CC14, "charge1", Helper.TestObjectCreator.KDV18.PK.ToGuid(), "AR001", 1034m, Helper.TestObjectCreator.EUR);
				var eInvoiceBatch = Helper.CreateInvoiceBatch(invoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());

				using (var transactionBatch = exporter.CreateTransactionBatch(eInvoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

					AssertEquals(1, eInvoice.Invoice.TaxTotal.Length);
					var taxTotal = eInvoice.Invoice.TaxTotal[0];

					AssertEquals(186.12m, taxTotal.TaxAmount.Value);
					AssertEquals("EUR", taxTotal.TaxAmount.currencyID);
					AssertEquals(18m, taxTotal.TaxSubtotal[0].Percent.Value);
					AssertEquals(186.12m, taxTotal.TaxSubtotal[0].TaxAmount.Value);
					AssertEquals("EUR", taxTotal.TaxSubtotal[0].TaxAmount.currencyID);
					AssertEquals(1034m, taxTotal.TaxSubtotal[0].TaxableAmount.Value);
					AssertEquals("EUR", taxTotal.TaxSubtotal[0].TaxableAmount.currencyID);

					AssertNull(eInvoice.Invoice.WithholdingTaxTotal);
				}
			}
		}

		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestTaxTotalMultilineMappingOnTurkeyEInvoice()
		{
			var turkeyTaxRates = new AccTaxRateCollection(Factory);
			turkeyTaxRates.Add(Helper.TestObjectCreator.KDV18);
			turkeyTaxRates.Add(Helper.TestObjectCreator.KDV8);
			turkeyTaxRates.Add(Helper.TestObjectCreator.KDV1);

			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var eInvoiceBatch = Helper.CreateMultilineTestARInvoiceBatch(Helper.TestObjectCreator.CC14, "charge1", turkeyTaxRates, "AR001", 1034m, Helper.TestObjectCreator.TRY);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());

				using (var transactionBatch = exporter.CreateTransactionBatch(eInvoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

					AssertEquals(1, eInvoice.Invoice.TaxTotal.Length);
					var taxTotal = eInvoice.Invoice.TaxTotal[0];

					taxTotal.TaxSubtotal = taxTotal.TaxSubtotal.OrderBy(x => x.Percent.Value).ToArray();

					AssertEquals(279.18m, taxTotal.TaxAmount.Value);
					AssertEquals("TRY", taxTotal.TaxAmount.currencyID);
					AssertEquals(3, taxTotal.TaxSubtotal.Length);
					AssertEquals(1m, taxTotal.TaxSubtotal[0].Percent.Value);
					AssertEquals(10.34m, taxTotal.TaxSubtotal[0].TaxAmount.Value);
					AssertEquals("TRY", taxTotal.TaxSubtotal[0].TaxAmount.currencyID);
					AssertEquals(1034m, taxTotal.TaxSubtotal[0].TaxableAmount.Value);
					AssertEquals("TRY", taxTotal.TaxSubtotal[0].TaxableAmount.currencyID);
					AssertEquals(8m, taxTotal.TaxSubtotal[1].Percent.Value);
					AssertEquals(82.72m, taxTotal.TaxSubtotal[1].TaxAmount.Value);
					AssertEquals("TRY", taxTotal.TaxSubtotal[1].TaxAmount.currencyID);
					AssertEquals(1034m, taxTotal.TaxSubtotal[1].TaxableAmount.Value);
					AssertEquals("TRY", taxTotal.TaxSubtotal[1].TaxableAmount.currencyID);
					AssertEquals(18m, taxTotal.TaxSubtotal[2].Percent.Value);
					AssertEquals(186.12m, taxTotal.TaxSubtotal[2].TaxAmount.Value);
					AssertEquals("TRY", taxTotal.TaxSubtotal[2].TaxAmount.currencyID);
					AssertEquals(1034m, taxTotal.TaxSubtotal[2].TaxableAmount.Value);
					AssertEquals("TRY", taxTotal.TaxSubtotal[2].TaxableAmount.currencyID);

					AssertNull(eInvoice.Invoice.WithholdingTaxTotal);
				}
			}
		}

		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestTaxTotalMultilineMappingOnTurkeyEInvoiceForeignCurrency()
		{
			var turkeyTaxRates = new AccTaxRateCollection(Factory);
			turkeyTaxRates.Add(Helper.TestObjectCreator.KDV18);
			turkeyTaxRates.Add(Helper.TestObjectCreator.KDV8);
			turkeyTaxRates.Add(Helper.TestObjectCreator.KDV1);

			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var eInvoiceBatch = Helper.CreateMultilineTestARInvoiceBatch(Helper.TestObjectCreator.CC14, "charge1", turkeyTaxRates, "AR001", 1034m, Helper.TestObjectCreator.EUR);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());

				using (var transactionBatch = exporter.CreateTransactionBatch(eInvoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

					AssertEquals(1, eInvoice.Invoice.TaxTotal.Length);
					var taxTotal = eInvoice.Invoice.TaxTotal[0];

					taxTotal.TaxSubtotal = taxTotal.TaxSubtotal.OrderBy(x => x.Percent.Value).ToArray();

					AssertEquals(279.18m, taxTotal.TaxAmount.Value);
					AssertEquals("EUR", taxTotal.TaxAmount.currencyID);
					AssertEquals(3, taxTotal.TaxSubtotal.Length);
					AssertEquals(1m, taxTotal.TaxSubtotal[0].Percent.Value);
					AssertEquals(10.34m, taxTotal.TaxSubtotal[0].TaxAmount.Value);
					AssertEquals("EUR", taxTotal.TaxSubtotal[0].TaxAmount.currencyID);
					AssertEquals(1034m, taxTotal.TaxSubtotal[0].TaxableAmount.Value);
					AssertEquals("EUR", taxTotal.TaxSubtotal[0].TaxableAmount.currencyID);
					AssertEquals(8m, taxTotal.TaxSubtotal[1].Percent.Value);
					AssertEquals(82.72m, taxTotal.TaxSubtotal[1].TaxAmount.Value);
					AssertEquals("EUR", taxTotal.TaxSubtotal[1].TaxAmount.currencyID);
					AssertEquals(1034m, taxTotal.TaxSubtotal[1].TaxableAmount.Value);
					AssertEquals("EUR", taxTotal.TaxSubtotal[1].TaxableAmount.currencyID);
					AssertEquals(18m, taxTotal.TaxSubtotal[2].Percent.Value);
					AssertEquals(186.12m, taxTotal.TaxSubtotal[2].TaxAmount.Value);
					AssertEquals("EUR", taxTotal.TaxSubtotal[2].TaxAmount.currencyID);
					AssertEquals(1034m, taxTotal.TaxSubtotal[2].TaxableAmount.Value);
					AssertEquals("EUR", taxTotal.TaxSubtotal[2].TaxableAmount.currencyID);

					AssertNull(eInvoice.Invoice.WithholdingTaxTotal);
				}
			}
		}

		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestMultiLineInvoiceWithDifferentValues()
		{
			using (Helper.SetUpForTestingEInvoicingTurkeyWithControlAccounts())
			{
				var invoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR);
				var eInvoiceBatch = Helper.CreateInvoiceBatch(invoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());

				using (var transactionBatch = exporter.CreateTransactionBatch(eInvoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

					AssertEquals(1, eInvoice.Invoice.TaxTotal.Length);
					var taxTotal = eInvoice.Invoice.TaxTotal[0];

					taxTotal.TaxSubtotal = taxTotal.TaxSubtotal.OrderBy(x => x.Percent.Value).ToArray();

					AssertEquals(34.09m, taxTotal.TaxAmount.Value);
					AssertEquals("TRY", taxTotal.TaxAmount.currencyID);
					AssertEquals(3, taxTotal.TaxSubtotal.Length);

					AssertEquals(0m, taxTotal.TaxSubtotal[0].Percent.Value);
					AssertEquals(0m, taxTotal.TaxSubtotal[0].TaxAmount.Value);
					AssertEquals("TRY", taxTotal.TaxSubtotal[0].TaxAmount.currencyID);
					AssertEquals(300.70m, taxTotal.TaxSubtotal[0].TaxableAmount.Value);
					AssertEquals("TRY", taxTotal.TaxSubtotal[0].TaxableAmount.currencyID);
					AssertEquals(8m, taxTotal.TaxSubtotal[1].Percent.Value);
					AssertEquals(16.04m, taxTotal.TaxSubtotal[1].TaxAmount.Value);
					AssertEquals("TRY", taxTotal.TaxSubtotal[1].TaxAmount.currencyID);
					AssertEquals(200.5m, taxTotal.TaxSubtotal[1].TaxableAmount.Value);
					AssertEquals("TRY", taxTotal.TaxSubtotal[1].TaxableAmount.currencyID);
					AssertEquals(18m, taxTotal.TaxSubtotal[2].Percent.Value);
					AssertEquals(18.05m, taxTotal.TaxSubtotal[2].TaxAmount.Value);
					AssertEquals("TRY", taxTotal.TaxSubtotal[2].TaxAmount.currencyID);
					AssertEquals(100.3m, taxTotal.TaxSubtotal[2].TaxableAmount.Value);
					AssertEquals("TRY", taxTotal.TaxSubtotal[2].TaxableAmount.currencyID);

					AssertNull(eInvoice.Invoice.WithholdingTaxTotal);
				}
			}
		}

		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestMultiLineInvoiceWithDifferentValuesForeignCurrency()
		{
			using (Helper.SetUpForTestingEInvoicingTurkeyWithControlAccounts())
			{
				var invoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, Helper.TestObjectCreator.EUR);
				var eInvoiceBatch = Helper.CreateInvoiceBatch(invoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());

				using (var transactionBatch = exporter.CreateTransactionBatch(eInvoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

					AssertEquals(1, eInvoice.Invoice.TaxTotal.Length);
					var taxTotal = eInvoice.Invoice.TaxTotal[0];

					taxTotal.TaxSubtotal = taxTotal.TaxSubtotal.OrderBy(x => x.Percent.Value).ToArray();

					AssertEquals(34.09m, taxTotal.TaxAmount.Value);
					AssertEquals("EUR", taxTotal.TaxAmount.currencyID);
					AssertEquals(3, taxTotal.TaxSubtotal.Length);

					AssertEquals(0m, taxTotal.TaxSubtotal[0].Percent.Value);
					AssertEquals(0m, taxTotal.TaxSubtotal[0].TaxAmount.Value);
					AssertEquals("EUR", taxTotal.TaxSubtotal[0].TaxAmount.currencyID);
					AssertEquals(300.70m, taxTotal.TaxSubtotal[0].TaxableAmount.Value);
					AssertEquals("EUR", taxTotal.TaxSubtotal[0].TaxableAmount.currencyID);
					AssertEquals(8m, taxTotal.TaxSubtotal[1].Percent.Value);
					AssertEquals(16.04m, taxTotal.TaxSubtotal[1].TaxAmount.Value);
					AssertEquals("EUR", taxTotal.TaxSubtotal[1].TaxAmount.currencyID);
					AssertEquals(200.5m, taxTotal.TaxSubtotal[1].TaxableAmount.Value);
					AssertEquals("EUR", taxTotal.TaxSubtotal[1].TaxableAmount.currencyID);
					AssertEquals(18m, taxTotal.TaxSubtotal[2].Percent.Value);
					AssertEquals(18.05m, taxTotal.TaxSubtotal[2].TaxAmount.Value);
					AssertEquals("EUR", taxTotal.TaxSubtotal[2].TaxAmount.currencyID);
					AssertEquals(100.3m, taxTotal.TaxSubtotal[2].TaxableAmount.Value);
					AssertEquals("EUR", taxTotal.TaxSubtotal[2].TaxableAmount.currencyID);

					AssertNull(eInvoice.Invoice.WithholdingTaxTotal);
				}
			}
		}

		#endregion

		#region Withholding

		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestTaxAndWithholdingTotalMappingOnTurkeyEInvoice()
		{
			using (Helper.SetUpForTestingEInvoicingTurkeyWithControlAccounts())
			{
				var invoice = Helper.CreateTestARInvoice(Helper.TestObjectCreator.CC14, "charge1", Helper.TestObjectCreator.KDV18W5.PK, "AR001", 1000m, Helper.TestObjectCreator.TRY);
				var eInvoiceBatch = Helper.CreateInvoiceBatch(invoice);
				Helper.Factory.Save();
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());

				using (var transactionBatch = exporter.CreateTransactionBatch(eInvoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

					AssertEquals(1, eInvoice.Invoice.TaxTotal.Length);
					var taxTotal = eInvoice.Invoice.TaxTotal[0];

					AssertEquals(180m, taxTotal.TaxAmount.Value);
					AssertEquals("TRY", taxTotal.TaxAmount.currencyID);

					AssertEquals(1, taxTotal.TaxSubtotal.Length);
					AssertTaxSubtotal(taxTotal.TaxSubtotal[0], 18m, 180m, "TRY", 1000m, "KDV", "0015");

					AssertNotNull(eInvoice.Invoice.WithholdingTaxTotal);
					AssertEquals(1, eInvoice.Invoice.WithholdingTaxTotal.Length);
					var withholdingTaxTotal = eInvoice.Invoice.WithholdingTaxTotal[0];

					AssertEquals(90m, withholdingTaxTotal.TaxAmount.Value);
					AssertEquals("TRY", withholdingTaxTotal.TaxAmount.currencyID);
					AssertEquals(1, withholdingTaxTotal.TaxSubtotal.Length);
					AssertTaxSubtotal(withholdingTaxTotal.TaxSubtotal[0], 50m, 90m, "TRY", 180m, "TEVKIFAT", "606");
				}
			}
		}

		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestTaxAndWithholdingTotalsMappingOnTurkeyMultilineEInvoiceWithMixedTaxTypesTRY()
		{
			AssertTaxAndWithholdingTotalsMappingOnTurkeyMultilineEInvoiceWithMixedTaxTypes(Helper.TestObjectCreator.TRY, 1);
		}

		public void TestTaxAndWithholdingTotalsMappingOnTurkeyMultilineEInvoiceWithMixedTaxTypesUSD()
		{
			AssertTaxAndWithholdingTotalsMappingOnTurkeyMultilineEInvoiceWithMixedTaxTypes(Helper.TestObjectCreator.USD, 2);
		}

		public void TestTaxAndWithholdingTotalsMappingOnTurkeyMultilineEInvoiceWithMixedTaxTypesEUR()
		{
			AssertTaxAndWithholdingTotalsMappingOnTurkeyMultilineEInvoiceWithMixedTaxTypes(Helper.TestObjectCreator.EUR, 3);
		}

		void AssertTaxAndWithholdingTotalsMappingOnTurkeyMultilineEInvoiceWithMixedTaxTypes(RefCurrency refCurrency, decimal rate)
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoice = Helper.CreateMultilineInvoiceWithVATandWithholdingandExempt(refCurrency, rate);
				var eInvoiceBatch = Helper.CreateInvoiceBatch(invoice);
				Helper.Factory.Save();
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());

				using (var transactionBatch = exporter.CreateTransactionBatch(eInvoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

					AssertEquals(1, eInvoice.Invoice.TaxTotal.Length);
					var taxTotal = eInvoice.Invoice.TaxTotal[0];

					AssertEquals(3210m, taxTotal.TaxAmount.Value);
					AssertEquals(refCurrency.Code, taxTotal.TaxAmount.currencyID);
					AssertEquals(4, taxTotal.TaxSubtotal.Length);
					AssertTaxSubtotal(taxTotal.TaxSubtotal[0], 0m, 0m, refCurrency.Code, 4100.00m, "KDV", "0015", "301");
					AssertTaxSubtotal(taxTotal.TaxSubtotal[1], 0m, 0m, refCurrency.Code, 4600.00m, "KDV", "0015", "MSG3");
					AssertTaxSubtotal(taxTotal.TaxSubtotal[2], 8m, 600m, refCurrency.Code, 7500m, "KDV", "0015");
					AssertTaxSubtotal(taxTotal.TaxSubtotal[3], 18m, 2610m, refCurrency.Code, 14500m, "KDV", "0015");

					AssertNotNull(eInvoice.Invoice.WithholdingTaxTotal);
					AssertEquals(1, eInvoice.Invoice.WithholdingTaxTotal.Length);
					var withholdingTaxTotal = eInvoice.Invoice.WithholdingTaxTotal[0];

					AssertEquals(1309m, withholdingTaxTotal.TaxAmount.Value);
					AssertEquals(refCurrency.Code, withholdingTaxTotal.TaxAmount.currencyID);
					AssertEquals(4, withholdingTaxTotal.TaxSubtotal.Length);
					AssertTaxSubtotal(withholdingTaxTotal.TaxSubtotal[0], 50m, 100m, refCurrency.Code, 200m, "TEVKIFAT", "606");
					AssertTaxSubtotal(withholdingTaxTotal.TaxSubtotal[1], 30m, 84m, refCurrency.Code, 280m, "TEVKIFAT", "606");
					AssertTaxSubtotal(withholdingTaxTotal.TaxSubtotal[2], 50m, 180m, refCurrency.Code, 360m, "TEVKIFAT", "606");
					AssertTaxSubtotal(withholdingTaxTotal.TaxSubtotal[3], 70m, 945m, refCurrency.Code, 1350m, "TEVKIFAT", "606");
				}
			}
		}

		#endregion

		#region Exemption

		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestTaxAndExepmtionMappingOnTurkeyEInvoice()
		{
			using (Helper.SetUpForTestingEInvoicingTurkeyWithControlAccounts())
			{
				var invoice = Helper.CreateTestARInvoice(Helper.TestObjectCreator.CC14, "charge1", Helper.TestObjectCreator.FREEVAT.PK, "AR001", 1000m, Helper.TestObjectCreator.TRY);
				var eInvoiceBatch = Helper.CreateInvoiceBatch(invoice);
				Helper.Factory.Save();
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());

				using (var transactionBatch = exporter.CreateTransactionBatch(eInvoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

					AssertEquals(1, eInvoice.Invoice.TaxTotal.Length);
					var taxTotal = eInvoice.Invoice.TaxTotal[0];

					AssertEquals(0m, taxTotal.TaxAmount.Value);
					AssertEquals("TRY", taxTotal.TaxAmount.currencyID);

					AssertEquals(1, taxTotal.TaxSubtotal.Length);
					AssertTaxSubtotal(taxTotal.TaxSubtotal[0], 0m, 0m, "TRY", 1000.00m, "KDV", "0015", "301");
				}
			}
		}

		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestTaxExepmtionMappingOnTurkeyMultilineEInvoiceWithMixedTaxTypesTRY() => AssertTaxAndExemptionTotalsMappingOnTurkeyMultilineEInvoiceWithMixedTaxTypes(Helper.TestObjectCreator.TRY, 1);

		public void TestTaxAndExepmtionTotalsMappingOnTurkeyMultilineEInvoiceWithMixedTaxTypesUSD() => AssertTaxAndExemptionTotalsMappingOnTurkeyMultilineEInvoiceWithMixedTaxTypes(Helper.TestObjectCreator.USD, 2);

		public void TestTaxAndExepmtionTotalsMappingOnTurkeyMultilineEInvoiceWithMixedTaxTypesEUR() => AssertTaxAndExemptionTotalsMappingOnTurkeyMultilineEInvoiceWithMixedTaxTypes(Helper.TestObjectCreator.EUR, 3);

		void AssertTaxAndExemptionTotalsMappingOnTurkeyMultilineEInvoiceWithMixedTaxTypes(RefCurrency refCurrency, decimal rate)
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoice = Helper.CreateMixedMultilineInvoiceWithExemption(refCurrency, rate);
				var eInvoiceBatch = Helper.CreateInvoiceBatch(invoice);
				Helper.Factory.Save();
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());

				using (var transactionBatch = exporter.CreateTransactionBatch(eInvoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

					AssertEquals(1, eInvoice.Invoice.TaxTotal.Length);
					var taxTotal = eInvoice.Invoice.TaxTotal[0];

					AssertEquals(1020m, taxTotal.TaxAmount.Value);
					AssertEquals(refCurrency.Code, taxTotal.TaxAmount.currencyID);
					AssertEquals(4, taxTotal.TaxSubtotal.Length);
					AssertTaxSubtotal(taxTotal.TaxSubtotal[0], 0m, 0m, refCurrency.Code, 4100.00m, "KDV", "0015", "301");
					AssertTaxSubtotal(taxTotal.TaxSubtotal[1], 0m, 0m, refCurrency.Code, 4600.00m, "KDV", "0015", "351");
					AssertTaxSubtotal(taxTotal.TaxSubtotal[2], 8m, 120m, refCurrency.Code, 1500m, "KDV", "0015");
					AssertTaxSubtotal(taxTotal.TaxSubtotal[3], 18m, 900m, refCurrency.Code, 5000m, "KDV", "0015");
				}
			}
		}

		#endregion

		void AssertTaxSubtotal(efatura.uyumsoft.com.tr.TaxSubtotalType taxSubTotal, decimal percent, decimal taxAmount, string currency, decimal taxableAmount, string taxCategory, string taxTypeCode, string taxExemptionReasonCode = null)
		{
			AssertEquals("Percent", percent, taxSubTotal.Percent.Value);
			AssertEquals("Tax amount", taxAmount, taxSubTotal.TaxAmount.Value);
			AssertEquals("Tax amount currency", currency, taxSubTotal.TaxAmount.currencyID);
			AssertEquals("Taxable amount", taxableAmount, taxSubTotal.TaxableAmount.Value);
			AssertEquals("Taxable amount currency", currency, taxSubTotal.TaxableAmount.currencyID);
			AssertEquals("Tax category", taxCategory, taxSubTotal.TaxCategory.TaxScheme.Name.Value);
			AssertEquals("Tax type code", taxTypeCode, taxSubTotal.TaxCategory.TaxScheme.TaxTypeCode.Value);
			AssertEquals("Tax exemption reason code", taxExemptionReasonCode, taxSubTotal.TaxCategory.TaxExemptionReasonCode?.Value);
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
