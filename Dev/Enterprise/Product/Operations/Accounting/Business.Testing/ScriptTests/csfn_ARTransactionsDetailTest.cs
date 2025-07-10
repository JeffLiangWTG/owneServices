using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class csfn_ARTransactionsDetailTest : ScriptTest
	{
		[TestDate(2020, 7, 15)]
		public void TestAH_BalanceWithOtherTaxes()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			Factory.Save();
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2020, 01, 01));
			var invoice = CreateInvoice(-30, TestObjectCreator.USD, 2m);
			invoice.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			invoice.AH_OSTaxAmountOtherTaxes = 20M;
			invoice.AH_LocalTaxAmountOtherTaxes = 10M;
			AssertEquals(5M, invoice.AH_GSTAmount);

			Factory.Save();

			var resultTable = RunScript("202006");
			AssertDataTableAllRows("", resultTable, new[] { "AH_Balance" }, new object[][] { new object[] { 130m } });

			PayInvoice(invoice, -15, 100);
			resultTable = RunScript("202007");
			AssertDataTableAllRows("", resultTable, new[] { "AH_Balance" }, new object[][] { new object[] { 30m } });

			company.GC_IsReciprocal = true;
			company.Factory.Save();
			resultTable = RunScript("202007");
			AssertDataTableAllRows("", resultTable, new[] { "AH_Balance" }, new object[][] { new object[] { 7.5m } });
		}

		#region ByPeriodEnd

		[TestDate(2019, 3, 14)]
		public void Testcsfn_ARTransactionsDetail_PartlyPaid_ByPeriodEnd_ExchangeRateEqualToOne()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(-30);
			Factory.Save();
			PayInvoice(invoice, -30, 20);
			PayInvoice(invoice, -1);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201902");

			AssertDataTableAllRows("", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { -80m, -80m }, new object[] { 80m, 80m } });
		}

		[TestDate(2019, 3, 14)]
		public void Testcsfn_ARTransactionsDetail_NotPaid_ByPeriodEnd_ExchangeRateNotEqualToOne()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(-30, TestObjectCreator.USD, 0.5m);
			Factory.Save();
			Assert(invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201902");

			AssertDataTableAllRows("", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { 100m, 200m } });
		}

		[TestDate(2019, 3, 14)]
		public void Testcsfn_ARTransactionsDetail_PartlyPaid_ByPeriodEnd_ExchangeRateNotEqualToOne()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(-30, TestObjectCreator.USD, 0.5m);
			Factory.Save();
			PayInvoice(invoice, -30, 20);
			PayInvoice(invoice, -1);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201902");

			AssertDataTableAllRows("", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { -160m, -160m }, new object[] { 80m, 160m } });
		}

		[TestDate(2019, 3, 14)]
		public void Testcsfn_ARTransactionsDetail_PartlyPaid_ByPeriodEnd_ExtremeExchangeRate()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(-30, TestObjectCreator.IDR, 14000m, 15000000);
			Factory.Save();
			PayInvoice(invoice, -30, 3000000);
			PayInvoice(invoice, -1);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201902");

			AssertDataTableAllRows("", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { -857.14m, -857.14m }, new object[] { 11999960m, 857.14m } });
		}

		[TestDate(2019, 3, 14)]
		public void Testcsfn_ARTransactionsDetail_PartlyPaid_ByPeriodEnd_ExtremeExchangeRate_NewOSOutstandingAmountFeature()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoiceForNewOSOutstandingAmountFeature(-30, TestObjectCreator.IDR, 14000m, 15000000);
			Factory.Save();
			PayInvoiceForNewOSOutstandingAmountFeature(invoice, -30, 3000000);
			PayInvoiceForNewOSOutstandingAmountFeature(invoice, -1);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.DataType.SuspendValidation())
			{
				AssertWhenRegistryEnabled(Guid.Empty);
				AssertWhenRegistryDisabled(Guid.Empty);

				AssertWhenRegistryEnabled(GlbCompany.CurrentCompany.PK.ToGuid());
				AssertWhenRegistryDisabled(GlbCompany.CurrentCompany.PK.ToGuid());
			}

			void AssertWhenRegistryEnabled(Guid companyPK)
			{
				using (companyPK == Guid.Empty ? null : AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, true))
				{
					var resultTable = RunScript("201902");
					AssertDataTableAllRows("show correct amount via AH_OSOutstandingAmount and AP_OSAmount.", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { -857.14m, -857.14m }, new object[] { 12000000m, 857.14m } });
				}

				using (companyPK == Guid.Empty ? null : AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, true))
				{
					var resultTable = RunScript("201902");
					AssertDataTableAllRows("show correct amount via AH_OSOutstandingAmount and AP_OSAmount.", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { -857.14m, -857.14m }, new object[] { 12000000m, 857.14m } });
				}
			}

			void AssertWhenRegistryDisabled(Guid companyPK)
			{
				using (companyPK == Guid.Empty ? null : AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, false))
				{
					var resultTable = RunScript("201902");
					AssertDataTableAllRows("show original amount due to feature disable.", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { -857.14m, -857.14m }, new object[] { 11999960m, 857.14m } });
				}

				using (companyPK == Guid.Empty ? null : AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, false))
				{
					var resultTable = RunScript("201902");
					AssertDataTableAllRows("show original amount due to feature disable.", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { -857.14m, -857.14m }, new object[] { 11999960m, 857.14m } });
				}
			}
		}

		[TestDate(2019, 3, 14)]
		public void Testcsfn_ARTransactionsDetail_PartlyPaid_ByCurrentDate_ExtremeExchangeRate_NewOSOutstandingAmountFeature()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoiceForNewOSOutstandingAmountFeature(-30, TestObjectCreator.IDR, 14000m, 15000000);
			Factory.Save();
			PayInvoiceForNewOSOutstandingAmountFeature(invoice, -30, 3000000);
			Assert(invoice.AH_FullyPaidDate.IsEmpty);

			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.DataType.SuspendValidation())
			{
				AssertWhenRegistryEnabled(Guid.Empty);
				AssertWhenRegistryDisabled(Guid.Empty);

				AssertWhenRegistryEnabled(GlbCompany.CurrentCompany.PK.ToGuid());
				AssertWhenRegistryDisabled(GlbCompany.CurrentCompany.PK.ToGuid());
			}

			void AssertWhenRegistryEnabled(Guid companyPK)
			{
				using (companyPK == Guid.Empty ? null : AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, true))
				{
					var resultTable = RunScript("201903");
					AssertDataTableAllRows("Check SQL will not get null result when no matching result.", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { 12000000m, 857.14m } });
				}

				using (companyPK == Guid.Empty ? null : AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, true))
				{
					var resultTable = RunScript("201903");
					AssertDataTableAllRows("Check SQL will not get null result when no matching result.", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { 12000000m, 857.14m } });
				}
			}

			void AssertWhenRegistryDisabled(Guid companyPK)
			{
				using (companyPK == Guid.Empty ? null : AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, false))
				{
					var resultTable = RunScript("201903");
					AssertDataTableAllRows("show original amount due to feature disable.", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { 11999960m, 857.14m } });
				}

				using (companyPK == Guid.Empty ? null : AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, false))
				{
					var resultTable = RunScript("201903");
					AssertDataTableAllRows("show original amount due to feature disable.", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { 11999960m, 857.14m } });
				}
			}
		}

		[TestDate(2019, 3, 14)]
		public void Testcsfn_ARTransactionsDetail_PartlyPaid_ByPeriodEnd_ExchangeRateNotZeroOrOne_GCReciprocal()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_IsReciprocal = true;
			Factory.Save();
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(-30, TestObjectCreator.USD, 0.5m);
			Factory.Save();
			PayInvoice(invoice, -30, 20);
			PayInvoice(invoice, -1);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201902");

			AssertDataTableAllRows("", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { -40m, -40m }, new object[] { 80m, 40m } });
		}

		[TestDate(2019, 3, 14)]
		public void TestAH_BalanceInLocal_PaidBefore_ByPeriodEnd_ExchangeRateEqualToOne()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(-30);
			Factory.Save();
			PayInvoice(invoice, -30);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201902");

			AssertEquals("Rows.Count", 0, resultTable.Rows.Count);
		}

		[TestDate(2019, 3, 14)]
		public void Testcsfn_ARTransactionsDetail_PaidBefore_ByPeriodEnd_ExchangeRateNotEqualToOne()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(-30, TestObjectCreator.USD, 0.5m);
			Factory.Save();
			PayInvoice(invoice, -30);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201902");

			AssertEquals("Rows.Count", 0, resultTable.Rows.Count);
		}

		[TestDate(2019, 3, 14)]
		public void Testcsfn_ARTransactionsDetail_PaidBefore_ByPeriodEnd_ExchangeRateNotZeroOrOne_GCReciprocal()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_IsReciprocal = true;
			Factory.Save();
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(-30, TestObjectCreator.USD, 0.5m);
			Factory.Save();
			PayInvoice(invoice, -30);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201902");

			AssertEquals("Rows.Count", 0, resultTable.Rows.Count);
		}

		[TestDate(2019, 3, 14)]
		public void TestAH_BalanceInLocal_PaidAfter_ByPeriodEnd_ExchangeRateEqualToOne()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(-30);
			Factory.Save();
			PayInvoice(invoice, -10);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201902");

			AssertDataTableAllRows("", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { -100m, -100m }, new object[] { 100m, 100m } });
		}

		[TestDate(2019, 3, 14)]
		public void Testcsfn_ARTransactionsDetail_PaidAfter_ByPeriodEnd_ExchangeRateNotEqualToOne()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(-30, TestObjectCreator.USD, 0.5m);
			Factory.Save();
			PayInvoice(invoice, -10);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201902");

			AssertDataTableAllRows("", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { -200m, -200m }, new object[] { 100m, 200m } });
		}

		[TestDate(2019, 3, 14)]
		public void Testcsfn_ARTransactionsDetail_PaidAfter_ByPeriodEnd_ExchangeRateNotZeroOrOne_GCReciprocal()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_IsReciprocal = true;
			Factory.Save();
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(-30, TestObjectCreator.USD, 0.5m);
			Factory.Save();
			PayInvoice(invoice, -10);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201902");

			AssertDataTableAllRows("", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { -50m, -50m }, new object[] { 100m, 50m } });
		}

		[TestDate(2019, 3, 14)]
		public void Testcsfn_ARTransactionsDetail_TransactionCreated_AfterPeriodEnd()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(0);
			Factory.Save();
			Assert(invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201902");

			AssertEquals("Rows.Count", 0, resultTable.Rows.Count);
		}

		#endregion

		#region CurrentDate

		[TestDate(2019, 3, 14)]
		public void TestAH_BalanceInLocal_PartlyPaid_ByCurrentDate_ExchangeRateEqualToOne()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(0);
			Factory.Save();
			PayInvoice(invoice, 0, 20);

			var resultTable = RunScript("201903");

			AssertDataTableAllRows("", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { 80m, 80m } });
		}

		[TestDate(2019, 3, 14)]
		public void TestAH_BalanceInLocal_NotPaid_ByCurrentDate_ExchangeRateNotEqualToOne()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(0, TestObjectCreator.USD, 0.5m);
			Factory.Save();
			Assert(invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201903");

			AssertDataTableAllRows("", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { 100m, 200m } });
		}

		[TestDate(2019, 3, 14)]
		public void Testcsfn_ARTransactionsDetail_PartlyPaid_ByCurrentDate_ExchangeRateNotEqualToOne()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(0, TestObjectCreator.USD, 0.5m);
			Factory.Save();
			PayInvoice(invoice, 0, 20);

			var resultTable = RunScript("201903");

			AssertDataTableAllRows("", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { 80m, 160m } });
		}

		[TestDate(2019, 3, 14)]
		public void Testcsfn_ARTransactionsDetail_PartlyPaid_ByCurrentDate_ExchangeRateNotZeroOrOne_GCReciprocal()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_IsReciprocal = true;
			Factory.Save();
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(0, TestObjectCreator.USD, 0.5m);
			Factory.Save();
			PayInvoice(invoice, 0, 20);

			var resultTable = RunScript("201903");

			AssertDataTableAllRows("", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { 80m, 40m } });
		}

		[TestDate(2019, 3, 14)]
		public void Testcsfn_ARTransactionsDetail_PaidBefore_ByCurrentDate_ExchangeRateEqualToOne()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(0);
			Factory.Save();
			PayInvoice(invoice, -1);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201903");

			AssertEquals("Rows.Count", 0, resultTable.Rows.Count);
		}

		[TestDate(2019, 3, 14)]
		public void Testcsfn_ARTransactionsDetail_PaidBefore_ByCurrentDate_ExchangeRateNotEqualToOne()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(0, TestObjectCreator.USD, 0.5m);
			Factory.Save();
			PayInvoice(invoice, -1);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201903");

			AssertEquals("Rows.Count", 0, resultTable.Rows.Count);
		}

		[TestDate(2019, 3, 14)]
		public void Testcsfn_ARTransactionsDetail_PaidBefore_ByCurrentDate_ExchangeRateNotEqualToOne_GCReciprocal()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_IsReciprocal = true;
			Factory.Save();
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(0, TestObjectCreator.USD, 0.5m);
			Factory.Save();
			PayInvoice(invoice, -1);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201903");

			AssertEquals("Rows.Count", 0, resultTable.Rows.Count);
		}

		#endregion

		InvoicingBase CreateInvoice(int daysPosted, RefCurrency currency = null, decimal? exchangeRate = null, decimal invoiceExTaxAmount = 100m)
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV1", currency, exchangeRate, organisation: TestObjectCreator.Debtor);
			invoice.AH_PostDate = ZDateTime.Today.AddDays(daysPosted);
			TestObjectCreator.CreateInvoiceLine(invoice, invoiceExTaxAmount, currency, exchangeRate, setTaxes: false);

			return invoice;
		}

		InvoicingBase CreateInvoiceForNewOSOutstandingAmountFeature(int daysPosted, RefCurrency currency = null, decimal? exchangeRate = null, decimal invoiceExTaxAmount = 100m)
		{
			var invoice = CreateInvoice(daysPosted, currency, exchangeRate, invoiceExTaxAmount);
			TestObjectCreator.UpdateInvoiceForNewOSOutstandingAmountFeature(invoice);

			return invoice;
		}

		void PayInvoice(InvoicingBase invoice, int daysPaid, ZDecimal? amountPaid = null)
		{
			TestObjectCreator.CreateAndMatchARReceiptForARInvoice(invoice, ZDateTime.Today.AddDays(daysPaid), amountPaid);
			Factory.Save();
		}

		void PayInvoiceForNewOSOutstandingAmountFeature(InvoicingBase invoice, int daysPaid, ZDecimal? amountPaid = null)
		{
			TestObjectCreator.CreateAndMatchARReceiptForARInvoiceForNewOSOutstandingAmountFeature(invoice, daysPaid, amountPaid);
			Factory.Save();
		}

		static DataTable RunScript(string period)
		{
			var sql = Invariant($@"
SELECT *
FROM csfn_ARTransactionsDetail({period},'{GlbCompany.CurrentCompany.PK}', '{ZDateTime.Today.ToISO8601ShortDateString()}')
ORDER BY AH_BalanceInLocal");

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}
