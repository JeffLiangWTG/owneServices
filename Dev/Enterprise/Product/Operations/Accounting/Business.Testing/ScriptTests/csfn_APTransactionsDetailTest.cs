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
	class csfn_APTransactionsDetailTest : ScriptTest
	{
		[TestDate(2019, 3, 14)]
		public void Testcsfn_APTransactionsDetail_PartlyPaid_ByPeriodEnd_ExtremeExchangeRate()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(-30, TestObjectCreator.IDR, 14000m, 15000000);
			Factory.Save();
			PayInvoice(invoice, -30, -3000000m);
			PayInvoice(invoice, -1);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201902");

			AssertDataTableAllRows("", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { -11999960m, -857.14m }, new object[] { 857.14m, 857.14m } });
		}

		[TestDate(2019, 3, 14)]
		public void Testcsfn_APTransactionsDetail_PartlyPaid_ByPeriodEnd_ExtremeExchangeRate_NewOSOutstandingAmountFeature()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoiceForNewOSOutstandingAmountFeature(-30, TestObjectCreator.IDR, 14000m, 15000000);
			Factory.Save();
			PayInvoiceForNewOSOutstandingAmountFeature(invoice, -30, -3000000m);
			PayInvoiceForNewOSOutstandingAmountFeature(invoice, -1);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.DataType.SuspendValidation())
			{
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertWhenRegistryEnabled();
				}

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					AssertWhenRegistryEnabled();
				}

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertWhenRegistryDisabled();
				}

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				{
					AssertWhenRegistryDisabled();
				}
			}

			void AssertWhenRegistryEnabled()
			{
				var resultTable = RunScript("201902");
				AssertDataTableAllRows("show correct amount via AH_OSOutstandingAmount and AP_OSAmount.", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { -12000000m, -857.14m }, new object[] { 857.14m, 857.14m } });
			}

			void AssertWhenRegistryDisabled()
			{
				var resultTable = RunScript("201902");
				AssertDataTableAllRows("show original amount due to feature disable.", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { -11999960m, -857.14m }, new object[] { 857.14m, 857.14m } });
			}
		}

		[TestDate(2019, 3, 14)]
		public void Testcsfn_APTransactionsDetail_PartlyPaid_ByCurrentDate_ExtremeExchangeRate_NewOSOutstandingAmountFeature()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoiceForNewOSOutstandingAmountFeature(-30, TestObjectCreator.IDR, 14000m, 15000000);
			Factory.Save();
			PayInvoiceForNewOSOutstandingAmountFeature(invoice, -30, -3000000m);
			Assert(invoice.AH_FullyPaidDate.IsEmpty);

			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.DataType.SuspendValidation())
			{
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertWhenRegistryEnabled();
				}

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					AssertWhenRegistryEnabled();
				}

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertWhenRegistryDisabled();
				}

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				{
					AssertWhenRegistryDisabled();
				}
			}

			void AssertWhenRegistryEnabled()
			{
				var resultTable = RunScript("201903");
				AssertDataTableAllRows("show correct amount via AH_OSOutstandingAmount and AP_OSAmount.", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { -12000000m, -857.14m } });
			}

			void AssertWhenRegistryDisabled()
			{
				var resultTable = RunScript("201903");
				AssertDataTableAllRows("show original amount due to feature disable.", resultTable, new[] { "AH_Balance", "AH_BalanceInLocal" }, new object[][] { new object[] { -11999960m, -857.14m } });
			}
		}

		InvoicingBase CreateInvoice(int daysPosted, RefCurrency currency = null, decimal? exchangeRate = null, decimal invoiceExTaxAmount = 100m)
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", currency, exchangeRate, organisation: TestObjectCreator.Creditor1);
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
			TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(invoice, ZDateTime.Today.AddDays(daysPaid), amountPaid, exchangeRate: 1m, useLocalPaidAmount: true);
			Factory.Save();
		}

		void PayInvoiceForNewOSOutstandingAmountFeature(InvoicingBase invoice, int daysPaid, ZDecimal? amountPaid = null)
		{
			TestObjectCreator.CreateAndMatchAPPaymentForAPInvoiceForNewOSOutstandingAmountFeature(invoice, daysPaid, amountPaid);
			Factory.Save();
		}

		static DataTable RunScript(string period)
		{
			var sql = Invariant($@"
SELECT *
FROM csfn_APTransactionsDetail({period},'{GlbCompany.CurrentCompany.PK}', '{ZDateTime.Today.ToISO8601ShortDateString()}')
ORDER BY AH_BalanceInLocal");

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}
