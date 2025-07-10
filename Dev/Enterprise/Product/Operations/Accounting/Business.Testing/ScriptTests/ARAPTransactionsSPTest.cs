using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class ARAPTransactionsSPTest : ScriptTest
	{
		#region New OS Outstanding Amount Feature

		public void TestNewOSOutstandingAmountFeature_UnPaid()
		{
			Invoice arInvoiceEnabledNewFeature, apInvoiceEnabledNewFeature, arInvoiceDisabledNewFeature, apInvoiceDisabledNewFeature;
			PrepareDataForNewOSOutstandingAmountFeature_UnPaid();

			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.DataType.SuspendValidation())
			{
				AssertForNewOSOutstandingAmountFeature(true, true);
				AssertForNewOSOutstandingAmountFeature(false, true);

				AssertForNewOSOutstandingAmountFeature(true, false);
				AssertForNewOSOutstandingAmountFeature(false, false);
			}

			//Note:
			//IF @ShowInInvoicedCurrency = 'Y', Balance = OSTotal WHERE ExchangeRate <> 1 AND InvoiceTotal = Balance

			void AssertForNewOSOutstandingAmountFeature(bool showInInvoicedCurrency, bool enableNewOSOutstandingAmountFeature)
			{
				var expetcedBalance = showInInvoicedCurrency ? 15000000.00m : 1071.43m;
				var expetcedBalanceInOS = enableNewOSOutstandingAmountFeature ? 15000000.00m : (object)DBNull.Value;

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableNewOSOutstandingAmountFeature))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, enableNewOSOutstandingAmountFeature))
				{
					var resultTable1 = RunScript(LedgerTypes.AccountsReceivable, true, Array.Empty<string>(), Array.Empty<string>(), false, "", "TRN", showInInvoicedCurrency: showInInvoicedCurrency);
					AssertDataTableAllRows("@LedgerType = 'AR', @ShowAllTransactions = 'Y'",
						resultTable1,
						new[] { "InvoiceRef", "Balance", "BalanceInOS", "BalanceInLocal", "MatchedInFuturePeriodInInvoiceCurrency", "MatchedInFuturePeriodInOSCurrency", "MatchedInFuturePeriodInLocalCurrency" },
						new object[][]
						{
							new object[] { arInvoiceEnabledNewFeature.AH_TransactionNum,		expetcedBalance, expetcedBalanceInOS, 0m,		DBNull.Value, DBNull.Value, DBNull.Value },
							new object[] { arInvoiceDisabledNewFeature.AH_TransactionNum,		expetcedBalance, DBNull.Value, 0m,				DBNull.Value, DBNull.Value, DBNull.Value },
						}
					);

					var resultTable2 = RunScript(LedgerTypes.AccountsPayable, true, Array.Empty<string>(), Array.Empty<string>(), false, "", "TRN", showInInvoicedCurrency: showInInvoicedCurrency);
					AssertDataTableAllRows("@LedgerType = 'AP', @ShowAllTransactions = 'Y'",
						resultTable2,
						new[] { "InvoiceRef", "Balance", "BalanceInOS", "BalanceInLocal", "MatchedInFuturePeriodInInvoiceCurrency", "MatchedInFuturePeriodInOSCurrency", "MatchedInFuturePeriodInLocalCurrency" },
						new object[][]
						{
							new object[] { apInvoiceEnabledNewFeature.AH_TransactionNum,		expetcedBalance, expetcedBalanceInOS, 0m,		DBNull.Value, DBNull.Value, DBNull.Value },
							new object[] { apInvoiceDisabledNewFeature.AH_TransactionNum,		expetcedBalance, DBNull.Value, 0m,				DBNull.Value, DBNull.Value, DBNull.Value },
						}
					);

					//@ShowAllTransactions = 'N'
					var resultTable3 = RunScript(LedgerTypes.AccountsReceivable, false, Array.Empty<string>(), Array.Empty<string>(), false, "", "TRN", showInInvoicedCurrency: showInInvoicedCurrency);
					AssertDataTableAllRows("@LedgerType = 'AR', @ShowAllTransactions = 'N'",
						resultTable3,
						new[] { "InvoiceRef", "Balance", "BalanceInOS", "BalanceInLocal", "MatchedInFuturePeriodInInvoiceCurrency", "MatchedInFuturePeriodInOSCurrency", "MatchedInFuturePeriodInLocalCurrency" },
						new object[][]
						{
							new object[] { arInvoiceEnabledNewFeature.AH_TransactionNum,		expetcedBalance, expetcedBalanceInOS, 1071.43m,		DBNull.Value, DBNull.Value, DBNull.Value },
							new object[] { arInvoiceDisabledNewFeature.AH_TransactionNum,		expetcedBalance, DBNull.Value, 1071.43m,			DBNull.Value, DBNull.Value, DBNull.Value },
						}
					);

					var resultTable4 = RunScript(LedgerTypes.AccountsPayable, false, Array.Empty<string>(), Array.Empty<string>(), false, "", "TRN", showInInvoicedCurrency: showInInvoicedCurrency);
					AssertDataTableAllRows("@LedgerType = 'AP', @ShowAllTransactions = 'N'",
						resultTable4,
						new[] { "InvoiceRef", "Balance", "BalanceInOS", "BalanceInLocal", "MatchedInFuturePeriodInInvoiceCurrency", "MatchedInFuturePeriodInOSCurrency", "MatchedInFuturePeriodInLocalCurrency" },
						new object[][]
						{
							new object[] { apInvoiceEnabledNewFeature.AH_TransactionNum,		expetcedBalance, expetcedBalanceInOS, 1071.43m,		DBNull.Value, DBNull.Value, DBNull.Value },
							new object[] { apInvoiceDisabledNewFeature.AH_TransactionNum,		expetcedBalance, DBNull.Value, 1071.43m,			DBNull.Value, DBNull.Value, DBNull.Value },
						}
					);
				}
			}

			void PrepareDataForNewOSOutstandingAmountFeature_UnPaid()
			{
				arInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.IDR, 14000m, 15000000m, 0m, 1071.43m, 0m) as ARInvoice;
				apInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "002", TestObjectCreator.IDR, 14000m, 15000000m, 0m, 1071.43m, 0m) as APInvoice;
				arInvoiceDisabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "003", TestObjectCreator.IDR, 14000m, 15000000m, 0m, 1071.43m, 0m) as ARInvoice;
				apInvoiceDisabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "004", TestObjectCreator.IDR, 14000m, 15000000m, 0m, 1071.43m, 0m) as APInvoice;

				arInvoiceEnabledNewFeature.AH_OH = arInvoiceDisabledNewFeature.AH_OH = TestObjectCreator.Debtor1.PK;
				apInvoiceEnabledNewFeature.AH_OH = apInvoiceDisabledNewFeature.AH_OH = TestObjectCreator.Creditor1.PK;

				arInvoiceEnabledNewFeature.Lines[0].AL_OSExTaxAmount = 15000000m;
				apInvoiceEnabledNewFeature.Lines[0].AL_OSExTaxAmount = 15000000m;
				arInvoiceDisabledNewFeature.Lines[0].AL_OSExTaxAmount = 15000000m;
				apInvoiceDisabledNewFeature.Lines[0].AL_OSExTaxAmount = 15000000m;

				TestObjectCreator.UpdateInvoiceForNewOSOutstandingAmountFeature(arInvoiceEnabledNewFeature);
				TestObjectCreator.UpdateInvoiceForNewOSOutstandingAmountFeature(apInvoiceEnabledNewFeature);

				Factory.Save();

				AssertEquals(15000000m, arInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(-15000000m, apInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, arInvoiceDisabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, apInvoiceDisabledNewFeature.AH_OSOutstandingAmount);

				AssertEquals(1071.43m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-1071.43m, apInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(1071.43m, arInvoiceDisabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-1071.43m, apInvoiceDisabledNewFeature.AH_OutstandingAmount);
			}
		}

		public void TestNewOSOutstandingAmountFeature_PartlyPaid_RunReportThisMonth()
		{
			Invoice arInvoiceEnabledNewFeature, apInvoiceEnabledNewFeature, arInvoiceDisabledNewFeature, apInvoiceDisabledNewFeature;
			PrepareDataForNewOSOutstandingAmountFeature_PartlyPaid();

			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.DataType.SuspendValidation())
			{
				AssertForNewOSOutstandingAmountFeature(true);
				AssertForNewOSOutstandingAmountFeature(false);
			}

			//Note:
			// @ShowAllTransactions = 'Y' or @ShowAllTransactions = 'N'
			// @UseOutstandingAmount = 1
			//
			// For arInvoiceEnabledNewFeature, apInvoiceEnabledNewFeature
			// Balance = AH_OSOutstandingAmount
			//		   = 12000000.00m
			//
			// For arInvoiceDisabledNewFeature, apInvoiceDisabledNewFeature
			// Balance = AH_OutstandingAmount * AH_ExchangeRate
			//		   = 857.14 * 14000.00
			//		   = 11999960.00m

			void AssertForNewOSOutstandingAmountFeature(bool enableNewOSOutstandingAmountFeature)
			{
				var expetcedBalance = enableNewOSOutstandingAmountFeature ? 12000000.00m : 11999960.00m;
				var expetcedBalanceInOS = enableNewOSOutstandingAmountFeature ? 12000000.00m : (object)DBNull.Value;

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableNewOSOutstandingAmountFeature))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, enableNewOSOutstandingAmountFeature))
				{
					var resultTable1 = RunScript(LedgerTypes.AccountsReceivable, true, Array.Empty<string>(), Array.Empty<string>(), false, "", "TRN", showInInvoicedCurrency: true);
					AssertDataTableAllRows("@LedgerType = 'AR', @ShowAllTransactions = 'Y'",
						resultTable1,
						new[] { "InvoiceRef", "Balance", "BalanceInOS", "BalanceInLocal", "MatchedInFuturePeriodInInvoiceCurrency", "MatchedInFuturePeriodInOSCurrency", "MatchedInFuturePeriodInLocalCurrency" },
						new object[][]
						{
							new object[] { arInvoiceEnabledNewFeature.AH_TransactionNum,		expetcedBalance, expetcedBalanceInOS, 0m,		DBNull.Value, DBNull.Value, DBNull.Value },
							new object[] { arInvoiceDisabledNewFeature.AH_TransactionNum,		11999960.00m, DBNull.Value, 0m,					DBNull.Value, DBNull.Value, DBNull.Value },
						}
					);

					var resultTable2 = RunScript(LedgerTypes.AccountsPayable, true, Array.Empty<string>(), Array.Empty<string>(), false, "", "TRN", showInInvoicedCurrency: true);
					AssertDataTableAllRows("@LedgerType = 'AP', @ShowAllTransactions = 'Y'",
						resultTable2,
						new[] { "InvoiceRef", "Balance", "BalanceInOS", "BalanceInLocal", "MatchedInFuturePeriodInInvoiceCurrency", "MatchedInFuturePeriodInOSCurrency", "MatchedInFuturePeriodInLocalCurrency" },
						new object[][]
						{
							new object[] { apInvoiceEnabledNewFeature.AH_TransactionNum,		expetcedBalance, expetcedBalanceInOS, 0m,		DBNull.Value, DBNull.Value, DBNull.Value },
							new object[] { apInvoiceDisabledNewFeature.AH_TransactionNum,		11999960.00m, DBNull.Value, 0m,					DBNull.Value, DBNull.Value, DBNull.Value },
						}
					);

					var resultTable3 = RunScript(LedgerTypes.AccountsReceivable, false, Array.Empty<string>(), Array.Empty<string>(), false, "", "TRN", showInInvoicedCurrency: true);
					AssertDataTableAllRows("@LedgerType = 'AR', @ShowAllTransactions = 'N'",
						resultTable3,
						new[] { "InvoiceRef", "Balance", "BalanceInOS", "BalanceInLocal", "MatchedInFuturePeriodInInvoiceCurrency", "MatchedInFuturePeriodInOSCurrency", "MatchedInFuturePeriodInLocalCurrency" },
						new object[][]
						{
							new object[] { arInvoiceEnabledNewFeature.AH_TransactionNum,		expetcedBalance, expetcedBalanceInOS, 857.14m,		DBNull.Value, DBNull.Value, DBNull.Value },
							new object[] { arInvoiceDisabledNewFeature.AH_TransactionNum,		11999960.00m, DBNull.Value, 857.14m,				DBNull.Value, DBNull.Value, DBNull.Value },
						}
					);

					var resultTable4 = RunScript(LedgerTypes.AccountsPayable, false, Array.Empty<string>(), Array.Empty<string>(), false, "", "TRN", showInInvoicedCurrency: true);
					AssertDataTableAllRows("@LedgerType = 'AP', @ShowAllTransactions = 'N'",
						resultTable4,
						new[] { "InvoiceRef", "Balance", "BalanceInOS", "BalanceInLocal", "MatchedInFuturePeriodInInvoiceCurrency", "MatchedInFuturePeriodInOSCurrency", "MatchedInFuturePeriodInLocalCurrency" },
						new object[][]
						{
							new object[] { apInvoiceEnabledNewFeature.AH_TransactionNum,		expetcedBalance, expetcedBalanceInOS, 857.14m,		DBNull.Value, DBNull.Value, DBNull.Value },
							new object[] { apInvoiceDisabledNewFeature.AH_TransactionNum,		11999960.00m, DBNull.Value, 857.14m,				DBNull.Value, DBNull.Value, DBNull.Value },
						}
					);
				}
			}

			void PrepareDataForNewOSOutstandingAmountFeature_PartlyPaid()
			{
				arInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.IDR, 14000m, 15000000m, 0m, 1071.43m, 0m) as ARInvoice;
				apInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "002", TestObjectCreator.IDR, 14000m, 15000000m, 0m, 1071.43m, 0m) as APInvoice;
				arInvoiceDisabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "003", TestObjectCreator.IDR, 14000m, 15000000m, 0m, 1071.43m, 0m) as ARInvoice;
				apInvoiceDisabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "004", TestObjectCreator.IDR, 14000m, 15000000m, 0m, 1071.43m, 0m) as APInvoice;

				arInvoiceEnabledNewFeature.AH_OH = arInvoiceDisabledNewFeature.AH_OH = TestObjectCreator.Debtor1.PK;
				apInvoiceEnabledNewFeature.AH_OH = apInvoiceDisabledNewFeature.AH_OH = TestObjectCreator.Creditor1.PK;

				arInvoiceEnabledNewFeature.Lines[0].AL_OSExTaxAmount = 15000000m;
				apInvoiceEnabledNewFeature.Lines[0].AL_OSExTaxAmount = 15000000m;
				arInvoiceDisabledNewFeature.Lines[0].AL_OSExTaxAmount = 15000000m;
				apInvoiceDisabledNewFeature.Lines[0].AL_OSExTaxAmount = 15000000m;

				TestObjectCreator.UpdateInvoiceForNewOSOutstandingAmountFeature(arInvoiceEnabledNewFeature);
				TestObjectCreator.UpdateInvoiceForNewOSOutstandingAmountFeature(apInvoiceEnabledNewFeature);

				Factory.Save();

				AssertEquals(15000000m, arInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(-15000000m, apInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, arInvoiceDisabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, apInvoiceDisabledNewFeature.AH_OSOutstandingAmount);

				AssertEquals(1071.43m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-1071.43m, apInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(1071.43m, arInvoiceDisabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-1071.43m, apInvoiceDisabledNewFeature.AH_OutstandingAmount);

				//Match: os amount 3000000m, local amount 214.29m
				TestObjectCreator.CreateAndMatchARReceiptForARInvoiceForNewOSOutstandingAmountFeature(arInvoiceEnabledNewFeature, 1, 3000000m, "M001");
				TestObjectCreator.CreateAndMatchAPPaymentForAPInvoiceForNewOSOutstandingAmountFeature(apInvoiceEnabledNewFeature, 1, -3000000m, "M002");
				TestObjectCreator.CreateAndMatchARReceiptForARInvoice(arInvoiceDisabledNewFeature, ZDateTime.Today.AddDays(1), 3000000m, "M003");
				TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(apInvoiceDisabledNewFeature, ZDateTime.Today.AddDays(1), -3000000m, "M004", 1m, true);
				Factory.Save();

				AssertEquals(12000000m, arInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(-12000000m, apInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, arInvoiceDisabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, apInvoiceDisabledNewFeature.AH_OSOutstandingAmount);

				AssertEquals(857.14m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-857.14m, apInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(857.14m, arInvoiceDisabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-857.14m, apInvoiceDisabledNewFeature.AH_OutstandingAmount);
			}
		}

		[TestDate(2022, 10, 1)]
		public void TestNewOSOutstandingAmountFeature_PartlyPaid_RunReportNextMonth()
		{
			Invoice arInvoiceEnabledNewFeature, apInvoiceEnabledNewFeature, arInvoiceDisabledNewFeature, apInvoiceDisabledNewFeature;
			APPayment paymentEnabledNewFeature, paymentDisabledNewFeature;
			ARReceipt receiptEnabledNewFeature, receiptDisabledNewFeature;

			PrepareDataForNewOSOutstandingAmountFeature_PartlyPaid();

			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.DataType.SuspendValidation())
			{
				AssertForNewOSOutstandingAmountFeatureWhenEnableNewOSOutstandingAmountFeature(Guid.Empty);
				AssertForNewOSOutstandingAmountFeatureWhenDisableNewOSOutstandingAmountFeature(Guid.Empty);

				AssertForNewOSOutstandingAmountFeatureWhenEnableNewOSOutstandingAmountFeature(GlbCompany.CurrentCompany.PK.ToGuid());
				AssertForNewOSOutstandingAmountFeatureWhenDisableNewOSOutstandingAmountFeature(GlbCompany.CurrentCompany.PK.ToGuid());
			}

			//Note:
			// @ShowAllTransactions = 'N'
			// @UseOutstandingAmount = 0
			//
			// For arInvoiceEnabledNewFeature, apInvoiceEnabledNewFeature
			// Balance = AH_OSOutstandingAmount + TotalAP_OSAmount
			//		   = 14000000.00 + 1000000.00 = 15000000.00m
			//
			// For arInvoiceDisabledNewFeature, apInvoiceDisabledNewFeature
			// Balance = (AH_OutstandingAmount + TotalAP_Amount) * AH_ExchangeRate
			//		   = (1000.00 + 71.43) * 14000.00
			//		   = 1071.43 * 14000.00
			//		   = 15000020.00m

			void AssertForNewOSOutstandingAmountFeatureWhenEnableNewOSOutstandingAmountFeature(Guid companyPK)
			{
				using (companyPK == Guid.Empty ? null : AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, true))
				{
					var args = SPArgs.Default(PeriodCalculator);
					args.ShowAll = false;
					args.ShowInInvoicedCurrency = true;
					args.ReportDate = ZDateTime.Today.AddDays(31).ToDateTime();  //run report next month
					args.OrderBy = "TRN";

					args.Ledger = LedgerTypes.AccountsReceivable;
					var resultTable1 = RunScriptCore(args);
					AssertDataTableAllRows("@LedgerType = 'AR'",
						resultTable1,
						new[] { "InvoiceRef", "Balance", "BalanceInOS", "BalanceInLocal", "MatchedInFuturePeriodInInvoiceCurrency", "MatchedInFuturePeriodInOSCurrency", "MatchedInFuturePeriodInLocalCurrency" },
						new object[][]
						{
							new object[] { arInvoiceEnabledNewFeature.AH_TransactionNum,		15000000.00m, 15000000.00m, 1071.43m,		DBNull.Value, DBNull.Value, DBNull.Value },
							new object[] { arInvoiceDisabledNewFeature.AH_TransactionNum,		15000020.00m, DBNull.Value, 1071.43m,		DBNull.Value, DBNull.Value, DBNull.Value },
							new object[] { receiptEnabledNewFeature.AH_TransactionNum,			-71.43m, -71.43m, -71.43m,					DBNull.Value, DBNull.Value, DBNull.Value },
							new object[] { receiptDisabledNewFeature.AH_TransactionNum,			-71.43m, DBNull.Value, -71.43m,				DBNull.Value, DBNull.Value, DBNull.Value },
						}
					);

					args.Ledger = LedgerTypes.AccountsPayable;
					var resultTable2 = RunScriptCore(args);
					AssertDataTableAllRows("@LedgerType = 'AP'",
						resultTable2,
						new[] { "InvoiceRef", "Balance", "BalanceInOS", "BalanceInLocal", "MatchedInFuturePeriodInInvoiceCurrency", "MatchedInFuturePeriodInOSCurrency", "MatchedInFuturePeriodInLocalCurrency" },
						new object[][]
						{
							new object[] { paymentEnabledNewFeature.AH_TransactionNum,			-71.43m, -71.43m, -71.43m,					DBNull.Value, DBNull.Value, DBNull.Value },
							new object[] { paymentDisabledNewFeature.AH_TransactionNum,			-71.43m, DBNull.Value, -71.43m,				DBNull.Value, DBNull.Value, DBNull.Value },
							new object[] { apInvoiceEnabledNewFeature.AH_TransactionNum,		15000000.00m, 15000000.00m, 1071.43m,		DBNull.Value, DBNull.Value, DBNull.Value },
							new object[] { apInvoiceDisabledNewFeature.AH_TransactionNum,		15000020.00m, DBNull.Value, 1071.43m,		DBNull.Value, DBNull.Value, DBNull.Value },
						}
					);
				}
			}

			void AssertForNewOSOutstandingAmountFeatureWhenDisableNewOSOutstandingAmountFeature(Guid companyPK)
			{
				using (companyPK == Guid.Empty ? null : AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, false))
				{
					var args = SPArgs.Default(PeriodCalculator);
					args.ShowAll = false;
					args.ShowInInvoicedCurrency = true;
					args.ReportDate = ZDateTime.Today.AddDays(31).ToDateTime();  //run report next month
					args.OrderBy = "TRN";

					args.Ledger = LedgerTypes.AccountsReceivable;
					var resultTable1 = RunScriptCore(args);
					AssertDataTableAllRows("@LedgerType = 'AR'",
						resultTable1,
						new[] { "InvoiceRef", "Balance", "BalanceInOS", "BalanceInLocal", "MatchedInFuturePeriodInInvoiceCurrency", "MatchedInFuturePeriodInOSCurrency", "MatchedInFuturePeriodInLocalCurrency" },
						new object[][]
						{
							new object[] { arInvoiceEnabledNewFeature.AH_TransactionNum,		15000020.00m, DBNull.Value, 1071.43m,		DBNull.Value, DBNull.Value, DBNull.Value },
							new object[] { arInvoiceDisabledNewFeature.AH_TransactionNum,		15000020.00m, DBNull.Value, 1071.43m,		DBNull.Value, DBNull.Value, DBNull.Value },
							new object[] { receiptEnabledNewFeature.AH_TransactionNum,			-71.43m, DBNull.Value, -71.43m,				DBNull.Value, DBNull.Value, DBNull.Value },
							new object[] { receiptDisabledNewFeature.AH_TransactionNum,			-71.43m, DBNull.Value, -71.43m,				DBNull.Value, DBNull.Value, DBNull.Value },
						}
					);

					args.Ledger = LedgerTypes.AccountsPayable;
					var resultTable2 = RunScriptCore(args);
					AssertDataTableAllRows("@LedgerType = 'AP'",
						resultTable2,
						new[] { "InvoiceRef", "Balance", "BalanceInOS", "BalanceInLocal", "MatchedInFuturePeriodInInvoiceCurrency", "MatchedInFuturePeriodInOSCurrency", "MatchedInFuturePeriodInLocalCurrency" },
						new object[][]
						{
							new object[] { paymentEnabledNewFeature.AH_TransactionNum,			-71.43m, DBNull.Value, -71.43m,				DBNull.Value, DBNull.Value, DBNull.Value },
							new object[] { paymentDisabledNewFeature.AH_TransactionNum,			-71.43m, DBNull.Value, -71.43m,				DBNull.Value, DBNull.Value, DBNull.Value },
							new object[] { apInvoiceEnabledNewFeature.AH_TransactionNum,		15000020.00m, DBNull.Value, 1071.43m,		DBNull.Value, DBNull.Value, DBNull.Value },
							new object[] { apInvoiceDisabledNewFeature.AH_TransactionNum,		15000020.00m, DBNull.Value, 1071.43m,		DBNull.Value, DBNull.Value, DBNull.Value },
						}
					);
				}
			}

			void PrepareDataForNewOSOutstandingAmountFeature_PartlyPaid()
			{
				arInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.IDR, 14000m, 29000000m, 0m, 2071.43m, 0m) as ARInvoice;
				apInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "002", TestObjectCreator.IDR, 14000m, 29000000m, 0m, 2071.43m, 0m) as APInvoice;
				arInvoiceDisabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "003", TestObjectCreator.IDR, 14000m, 29000000m, 0m, 2071.43m, 0m) as ARInvoice;
				apInvoiceDisabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "004", TestObjectCreator.IDR, 14000m, 29000000m, 0m, 2071.43m, 0m) as APInvoice;

				arInvoiceEnabledNewFeature.AH_OH = arInvoiceDisabledNewFeature.AH_OH = TestObjectCreator.Debtor1.PK;
				apInvoiceEnabledNewFeature.AH_OH = apInvoiceDisabledNewFeature.AH_OH = TestObjectCreator.Creditor1.PK;

				arInvoiceEnabledNewFeature.Lines[0].AL_OSExTaxAmount = 29000000m;
				apInvoiceEnabledNewFeature.Lines[0].AL_OSExTaxAmount = 29000000m;
				arInvoiceDisabledNewFeature.Lines[0].AL_OSExTaxAmount = 29000000m;
				apInvoiceDisabledNewFeature.Lines[0].AL_OSExTaxAmount = 29000000m;

				TestObjectCreator.UpdateInvoiceForNewOSOutstandingAmountFeature(arInvoiceEnabledNewFeature);
				TestObjectCreator.UpdateInvoiceForNewOSOutstandingAmountFeature(apInvoiceEnabledNewFeature);

				Factory.Save();

				AssertEquals(29000000m, arInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(-29000000m, apInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, arInvoiceDisabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, apInvoiceDisabledNewFeature.AH_OSOutstandingAmount);

				AssertEquals(2071.43m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-2071.43m, apInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(2071.43m, arInvoiceDisabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-2071.43m, apInvoiceDisabledNewFeature.AH_OutstandingAmount);

				//1st Match (this month): os amount 14000000m, local amount 1000m
				TestObjectCreator.CreateAndMatchARReceiptForARInvoiceForNewOSOutstandingAmountFeature(arInvoiceEnabledNewFeature, 1, 14000000m, "M001");
				TestObjectCreator.CreateAndMatchAPPaymentForAPInvoiceForNewOSOutstandingAmountFeature(apInvoiceEnabledNewFeature, 1, -14000000m, "M002");
				TestObjectCreator.CreateAndMatchARReceiptForARInvoice(arInvoiceDisabledNewFeature, ZDateTime.Today.AddDays(1), 14000000m, "M003");
				TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(apInvoiceDisabledNewFeature, ZDateTime.Today.AddDays(1), -14000000m, "M004", 1m, true);
				Factory.Save();

				AssertEquals(15000000m, arInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(-15000000m, apInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, arInvoiceDisabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, apInvoiceDisabledNewFeature.AH_OSOutstandingAmount);

				AssertEquals(1071.43m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-1071.43m, apInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(1071.43m, arInvoiceDisabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-1071.43m, apInvoiceDisabledNewFeature.AH_OutstandingAmount);

				//2nd Match (next month): os amount 1000000m, local amount 71.43m
				receiptEnabledNewFeature = TestObjectCreator.CreateAndMatchARReceiptForARInvoiceForNewOSOutstandingAmountFeature(arInvoiceEnabledNewFeature, 31, 1000000m, "M005");
				paymentEnabledNewFeature = TestObjectCreator.CreateAndMatchAPPaymentForAPInvoiceForNewOSOutstandingAmountFeature(apInvoiceEnabledNewFeature, 31, -1000000m, "M006");
				receiptDisabledNewFeature = TestObjectCreator.CreateAndMatchARReceiptForARInvoice(arInvoiceDisabledNewFeature, ZDateTime.Today.AddDays(31), 1000000m, "M007");
				paymentDisabledNewFeature = TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(apInvoiceDisabledNewFeature, ZDateTime.Today.AddDays(31), -1000000m, "M008", 1m, true);
				Factory.Save();

				AssertEquals(14000000m, arInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(-14000000m, apInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, arInvoiceDisabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, apInvoiceDisabledNewFeature.AH_OSOutstandingAmount);

				AssertEquals(1000.00m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-1000.00m, apInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(1000.00m, arInvoiceDisabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-1000.00m, apInvoiceDisabledNewFeature.AH_OutstandingAmount);
			}
		}

		[TestDate(2022, 10, 1)]
		public void TestNewOSOutstandingAmountFeature_AddMatchedInFuturePeriodInOSCurrency()
		{
			Invoice arInvoiceEnabledNewFeature, apInvoiceEnabledNewFeature, arInvoiceDisabledNewFeature, apInvoiceDisabledNewFeature;
			PrepareDataForNewOSOutstandingAmountFeature_PartlyPaid();

			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.DataType.SuspendValidation())
			{
				AssertForNewOSOutstandingAmountFeatureWhenEnableNewOSOutstandingAmountFeature(Guid.Empty);
				AssertForNewOSOutstandingAmountFeatureWhenDisableNewOSOutstandingAmountFeature(Guid.Empty);

				AssertForNewOSOutstandingAmountFeatureWhenEnableNewOSOutstandingAmountFeature(GlbCompany.CurrentCompany.PK.ToGuid());
				AssertForNewOSOutstandingAmountFeatureWhenDisableNewOSOutstandingAmountFeature(GlbCompany.CurrentCompany.PK.ToGuid());
			}

			//Note:
			// @ShowAllTransactions = 'Y'
			// @UseOutstandingAmount = 1
			//
			// For arInvoiceEnabledNewFeature, apInvoiceEnabledNewFeature
			// Balance = AH_OSOutstandingAmount + MatchedInFuturePeriodInOSCurrency
			//		   = 14000000.00 + 1000000.00
			//		   = 15000000.00m
			//
			// For arInvoiceDisabledNewFeature, apInvoiceDisabledNewFeature
			// Balance = (AH_OutstandingAmount + MatchedInFuturePeriodInLocalCurrency) * AH_ExchangeRate
			//		   = (1000.00 + 71.43) * 14000.00
			//		   = 1071.43 * 14000.00
			//		   = 15000020.00m

			void AssertForNewOSOutstandingAmountFeatureWhenEnableNewOSOutstandingAmountFeature(Guid companyPK)
			{
				using (companyPK == Guid.Empty ? null : AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, true))
				{
					var args = SPArgs.Default(PeriodCalculator);
					args.ShowAll = true;
					args.ShowInInvoicedCurrency = true;
					args.ReportDate = ZDateTime.Today.AddDays(31).ToDateTime();  //run report next month
					args.OrderBy = "TRN";
					args.FutureRECPAYNotInBalance = true;

					args.Ledger = LedgerTypes.AccountsReceivable;
					var resultTable1 = RunScriptCore(args);
					AssertDataTableAllRows("@LedgerType = 'AR'",
						resultTable1,
						new[] { "InvoiceRef", "Balance", "BalanceInOS", "BalanceInLocal", "MatchedInFuturePeriodInInvoiceCurrency", "MatchedInFuturePeriodInOSCurrency", "MatchedInFuturePeriodInLocalCurrency" },
						new object[][]
						{
							new object[] { arInvoiceEnabledNewFeature.AH_TransactionNum,		15000000.00m, 15000000.00m, 71.43m,		1000000.00m, 1000000.00m, 71.43m },
							new object[] { arInvoiceDisabledNewFeature.AH_TransactionNum,		15000020.00m, DBNull.Value, 71.43m,		1000020.00m, DBNull.Value, 71.43m },
						}
					);

					args.Ledger = LedgerTypes.AccountsPayable;
					var resultTable2 = RunScriptCore(args);
					AssertDataTableAllRows("@LedgerType = 'AP'",
						resultTable2,
						new[] { "InvoiceRef", "Balance", "BalanceInOS", "BalanceInLocal", "MatchedInFuturePeriodInInvoiceCurrency", "MatchedInFuturePeriodInOSCurrency", "MatchedInFuturePeriodInLocalCurrency" },
						new object[][]
						{
							new object[] { apInvoiceEnabledNewFeature.AH_TransactionNum,		15000000.00m, 15000000.00m, 71.43m,		1000000.00m, 1000000.00m, 71.43m },
							new object[] { apInvoiceDisabledNewFeature.AH_TransactionNum,		15000020.00m, DBNull.Value, 71.43m,		1000020.00m, DBNull.Value, 71.43m },
						}
					);
				}
			}

			void AssertForNewOSOutstandingAmountFeatureWhenDisableNewOSOutstandingAmountFeature(Guid companyPK)
			{
				using (companyPK == Guid.Empty ? null : AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, false))
				{
					var args = SPArgs.Default(PeriodCalculator);
					args.ShowAll = true;
					args.ShowInInvoicedCurrency = true;
					args.ReportDate = ZDateTime.Today.AddDays(31).ToDateTime();  //run report next month
					args.OrderBy = "TRN";
					args.FutureRECPAYNotInBalance = true;

					args.Ledger = LedgerTypes.AccountsReceivable;
					var resultTable1 = RunScriptCore(args);
					AssertDataTableAllRows("@LedgerType = 'AR'",
						resultTable1,
						new[] { "InvoiceRef", "Balance", "BalanceInOS", "BalanceInLocal", "MatchedInFuturePeriodInInvoiceCurrency", "MatchedInFuturePeriodInOSCurrency", "MatchedInFuturePeriodInLocalCurrency" },
						new object[][]
						{
							new object[] { arInvoiceEnabledNewFeature.AH_TransactionNum,		15000020.00m, DBNull.Value, 71.43m,		1000020.00m, DBNull.Value, 71.43m },
							new object[] { arInvoiceDisabledNewFeature.AH_TransactionNum,		15000020.00m, DBNull.Value, 71.43m,		1000020.00m, DBNull.Value, 71.43m },
						}
					);

					args.Ledger = LedgerTypes.AccountsPayable;
					var resultTable2 = RunScriptCore(args);
					AssertDataTableAllRows("@LedgerType = 'AP'",
						resultTable2,
						new[] { "InvoiceRef", "Balance", "BalanceInOS", "BalanceInLocal", "MatchedInFuturePeriodInInvoiceCurrency", "MatchedInFuturePeriodInOSCurrency", "MatchedInFuturePeriodInLocalCurrency" },
						new object[][]
						{
							new object[] { apInvoiceEnabledNewFeature.AH_TransactionNum,		15000020.00m, DBNull.Value, 71.43m,		1000020.00m, DBNull.Value, 71.43m },
							new object[] { apInvoiceDisabledNewFeature.AH_TransactionNum,		15000020.00m, DBNull.Value, 71.43m,		1000020.00m, DBNull.Value, 71.43m },
						}
					);
				}
			}

			void PrepareDataForNewOSOutstandingAmountFeature_PartlyPaid()
			{
				arInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.IDR, 14000m, 29000000m, 0m, 2071.43m, 0m) as ARInvoice;
				apInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "002", TestObjectCreator.IDR, 14000m, 29000000m, 0m, 2071.43m, 0m) as APInvoice;
				arInvoiceDisabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "003", TestObjectCreator.IDR, 14000m, 29000000m, 0m, 2071.43m, 0m) as ARInvoice;
				apInvoiceDisabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "004", TestObjectCreator.IDR, 14000m, 29000000m, 0m, 2071.43m, 0m) as APInvoice;

				arInvoiceEnabledNewFeature.AH_OH = arInvoiceDisabledNewFeature.AH_OH = TestObjectCreator.Debtor1.PK;
				apInvoiceEnabledNewFeature.AH_OH = apInvoiceDisabledNewFeature.AH_OH = TestObjectCreator.Creditor1.PK;

				arInvoiceEnabledNewFeature.Lines[0].AL_OSExTaxAmount = 29000000m;
				apInvoiceEnabledNewFeature.Lines[0].AL_OSExTaxAmount = 29000000m;
				arInvoiceDisabledNewFeature.Lines[0].AL_OSExTaxAmount = 29000000m;
				apInvoiceDisabledNewFeature.Lines[0].AL_OSExTaxAmount = 29000000m;

				TestObjectCreator.UpdateInvoiceForNewOSOutstandingAmountFeature(arInvoiceEnabledNewFeature);
				TestObjectCreator.UpdateInvoiceForNewOSOutstandingAmountFeature(apInvoiceEnabledNewFeature);

				Factory.Save();

				AssertEquals(29000000m, arInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(-29000000m, apInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, arInvoiceDisabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, apInvoiceDisabledNewFeature.AH_OSOutstandingAmount);

				AssertEquals(2071.43m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-2071.43m, apInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(2071.43m, arInvoiceDisabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-2071.43m, apInvoiceDisabledNewFeature.AH_OutstandingAmount);

				//1st Match (this month): os amount 14000000m, local amount 1000m
				TestObjectCreator.CreateAndMatchMiscellaneousTransactionForNewOSOutstandingAmountFeature(arInvoiceEnabledNewFeature, 1, 14000000m, "M001");
				TestObjectCreator.CreateAndMatchMiscellaneousTransactionForNewOSOutstandingAmountFeature(apInvoiceEnabledNewFeature, 1, -14000000m, "M002");
				TestObjectCreator.CreateAndMatchMiscellaneousTransaction(arInvoiceDisabledNewFeature, ZDateTime.Today.AddDays(1), 14000000m, "M003");
				TestObjectCreator.CreateAndMatchMiscellaneousTransaction(apInvoiceDisabledNewFeature, ZDateTime.Today.AddDays(1), -14000000m, "M004");
				Factory.Save();

				AssertEquals(15000000m, arInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(-15000000m, apInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, arInvoiceDisabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, apInvoiceDisabledNewFeature.AH_OSOutstandingAmount);

				AssertEquals(1071.43m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-1071.43m, apInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(1071.43m, arInvoiceDisabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-1071.43m, apInvoiceDisabledNewFeature.AH_OutstandingAmount);

				//2nd Match (next month): os amount 1000000m, local amount 71.43m
				var receipt1 = TestObjectCreator.CreateAndMatchARReceiptForARInvoiceForNewOSOutstandingAmountFeature(arInvoiceEnabledNewFeature, 31, 1000000m, "M005");
				var payment1 = TestObjectCreator.CreateAndMatchAPPaymentForAPInvoiceForNewOSOutstandingAmountFeature(apInvoiceEnabledNewFeature, 31, -1000000m, "M006");
				var receipt2 = TestObjectCreator.CreateAndMatchARReceiptForARInvoice(arInvoiceDisabledNewFeature, ZDateTime.Today.AddDays(31), 1000000m, "M007");
				var payment2 = TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(apInvoiceDisabledNewFeature, ZDateTime.Today.AddDays(31), -1000000m, "M008", 1m, true);
				receipt1.AH_PostDate = ZDateTime.Today.AddDays(31);
				payment1.AH_PostDate = ZDateTime.Today.AddDays(31);
				receipt2.AH_PostDate = ZDateTime.Today.AddDays(31);
				payment2.AH_PostDate = ZDateTime.Today.AddDays(31);
				Factory.Save();

				AssertEquals(14000000m, arInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(-14000000m, apInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, arInvoiceDisabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, apInvoiceDisabledNewFeature.AH_OSOutstandingAmount);

				AssertEquals(1000.00m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-1000.00m, apInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(1000.00m, arInvoiceDisabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-1000.00m, apInvoiceDisabledNewFeature.AH_OutstandingAmount);
			}
		}

		[TestDate(2022, 10, 1)]
		public void TestNewOSOutstandingAmountFeature_SubtractMatchedInFuturePeriodInOSCurrency()
		{
			Invoice arInvoiceEnabledNewFeature, apInvoiceEnabledNewFeature, arInvoiceDisabledNewFeature, apInvoiceDisabledNewFeature;
			PrepareDataForNewOSOutstandingAmountFeature_PartlyPaid();

			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.DataType.SuspendValidation())
			{
				AssertForNewOSOutstandingAmountFeatureWhenEnableNewOSOutstandingAmountFeature(Guid.Empty);
				AssertForNewOSOutstandingAmountFeatureWhenDisableNewOSOutstandingAmountFeature(Guid.Empty);

				AssertForNewOSOutstandingAmountFeatureWhenEnableNewOSOutstandingAmountFeature(GlbCompany.CurrentCompany.PK.ToGuid());
				AssertForNewOSOutstandingAmountFeatureWhenDisableNewOSOutstandingAmountFeature(GlbCompany.CurrentCompany.PK.ToGuid());
			}

			//Note:
			// @ShowAllTransactions = 'N'
			// @UseOutstandingAmount = 0
			//
			// For arInvoiceEnabledNewFeature, apInvoiceEnabledNewFeature
			// Balance = AH_OSOutstandingAmount + TotalAP_OSAmount - MatchedInFuturePeriodInOSCurrency
			//		   = 3000000.00 + 1000000.00 - 1000000.00
			//		   = 3000000.00m
			//
			// For arInvoiceDisabledNewFeature, apInvoiceDisabledNewFeature
			// Balance = (AH_OutstandingAmount + TotalAP_Amount - MatchedInFuturePeriodInLocalCurrency) * AH_ExchangeRate
			//		   = (214.28 + 71.43 - 71.43) * 14000.00
			//		   = 214.28 * 14000.00
			//		   = 2999920.00m

			void AssertForNewOSOutstandingAmountFeatureWhenEnableNewOSOutstandingAmountFeature(Guid companyPK)
			{
				using (companyPK == Guid.Empty ? null : AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, true))
				{
					var args = SPArgs.Default(PeriodCalculator);
					args.ShowAll = false;
					args.ShowInInvoicedCurrency = true;
					args.ReportDate = ZDateTime.Today.AddDays(31).ToDateTime();  //run report next month
					args.OrderBy = "TRN";
					args.FutureRECPAYNotInBalance = true;
					args.ExcludeMatchedToFutureRECPAY = true;

					args.Ledger = LedgerTypes.AccountsReceivable;
					var resultTable1 = RunScriptCore(args);
					AssertDataTableAllRows("@LedgerType = 'AR'",
						resultTable1,
						new[] { "InvoiceRef", "Balance", "BalanceInOS", "BalanceInLocal", "MatchedInFuturePeriodInInvoiceCurrency", "MatchedInFuturePeriodInOSCurrency", "MatchedInFuturePeriodInLocalCurrency" },
						new object[][]
						{
							new object[] { arInvoiceEnabledNewFeature.AH_TransactionNum,		3000000.00m, 3000000.00m, 214.28m,		1000000.00m, 1000000.00m, 71.43m },
							new object[] { arInvoiceDisabledNewFeature.AH_TransactionNum,		2999920.00m, DBNull.Value, 214.28m,		1000020.00m, DBNull.Value, 71.43m },
						}
					);

					args.Ledger = LedgerTypes.AccountsPayable;
					var resultTable2 = RunScriptCore(args);
					AssertDataTableAllRows("@LedgerType = 'AP'",
						resultTable2,
						new[] { "InvoiceRef", "Balance", "BalanceInOS", "BalanceInLocal", "MatchedInFuturePeriodInInvoiceCurrency", "MatchedInFuturePeriodInOSCurrency", "MatchedInFuturePeriodInLocalCurrency" },
						new object[][]
						{
							new object[] { apInvoiceEnabledNewFeature.AH_TransactionNum,		3000000.00m, 3000000.00m, 214.28m,		1000000.00m, 1000000.00m, 71.43m },
							new object[] { apInvoiceDisabledNewFeature.AH_TransactionNum,		2999920.00m, DBNull.Value, 214.28m,		1000020.00m, DBNull.Value, 71.43m },
						}
					);
				}
			}

			void AssertForNewOSOutstandingAmountFeatureWhenDisableNewOSOutstandingAmountFeature(Guid companyPK)
			{
				using (companyPK == Guid.Empty ? null : AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, false))
				{
					var args = SPArgs.Default(PeriodCalculator);
					args.ShowAll = false;
					args.ShowInInvoicedCurrency = true;
					args.ReportDate = ZDateTime.Today.AddDays(31).ToDateTime();  //run report next month
					args.OrderBy = "TRN";
					args.FutureRECPAYNotInBalance = true;
					args.ExcludeMatchedToFutureRECPAY = true;

					args.Ledger = LedgerTypes.AccountsReceivable;
					var resultTable1 = RunScriptCore(args);
					AssertDataTableAllRows("@LedgerType = 'AR'",
						resultTable1,
						new[] { "InvoiceRef", "Balance", "BalanceInOS", "BalanceInLocal", "MatchedInFuturePeriodInInvoiceCurrency", "MatchedInFuturePeriodInOSCurrency", "MatchedInFuturePeriodInLocalCurrency" },
						new object[][]
						{
							new object[] { arInvoiceEnabledNewFeature.AH_TransactionNum,		2999920.00m, DBNull.Value, 214.28m,		1000020.00m, DBNull.Value, 71.43m },
							new object[] { arInvoiceDisabledNewFeature.AH_TransactionNum,		2999920.00m, DBNull.Value, 214.28m,		1000020.00m, DBNull.Value, 71.43m },
						}
					);

					args.Ledger = LedgerTypes.AccountsPayable;
					var resultTable2 = RunScriptCore(args);
					AssertDataTableAllRows("@LedgerType = 'AP'",
						resultTable2,
						new[] { "InvoiceRef", "Balance", "BalanceInOS", "BalanceInLocal", "MatchedInFuturePeriodInInvoiceCurrency", "MatchedInFuturePeriodInOSCurrency", "MatchedInFuturePeriodInLocalCurrency" },
						new object[][]
						{
							new object[] { apInvoiceEnabledNewFeature.AH_TransactionNum,		2999920.00m, DBNull.Value, 214.28m,		1000020.00m, DBNull.Value, 71.43m },
							new object[] { apInvoiceDisabledNewFeature.AH_TransactionNum,		2999920.00m, DBNull.Value, 214.28m,		1000020.00m, DBNull.Value, 71.43m },
						}
					);
				}
			}

			void PrepareDataForNewOSOutstandingAmountFeature_PartlyPaid()
			{
				arInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.IDR, 14000m, 18000000m, 0m, 1285.71m, 0m) as ARInvoice;
				apInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "002", TestObjectCreator.IDR, 14000m, 18000000m, 0m, 1285.71m, 0m) as APInvoice;
				arInvoiceDisabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "003", TestObjectCreator.IDR, 14000m, 18000000m, 0m, 1285.71m, 0m) as ARInvoice;
				apInvoiceDisabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "004", TestObjectCreator.IDR, 14000m, 18000000m, 0m, 1285.71m, 0m) as APInvoice;

				arInvoiceEnabledNewFeature.AH_OH = arInvoiceDisabledNewFeature.AH_OH = TestObjectCreator.Debtor1.PK;
				apInvoiceEnabledNewFeature.AH_OH = apInvoiceDisabledNewFeature.AH_OH = TestObjectCreator.Creditor1.PK;

				arInvoiceEnabledNewFeature.Lines[0].AL_OSExTaxAmount = 18000000m;
				apInvoiceEnabledNewFeature.Lines[0].AL_OSExTaxAmount = 18000000m;
				arInvoiceDisabledNewFeature.Lines[0].AL_OSExTaxAmount = 18000000m;
				apInvoiceDisabledNewFeature.Lines[0].AL_OSExTaxAmount = 18000000m;

				TestObjectCreator.UpdateInvoiceForNewOSOutstandingAmountFeature(arInvoiceEnabledNewFeature);
				TestObjectCreator.UpdateInvoiceForNewOSOutstandingAmountFeature(apInvoiceEnabledNewFeature);

				Factory.Save();

				AssertEquals(18000000m, arInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(-18000000m, apInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, arInvoiceDisabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, apInvoiceDisabledNewFeature.AH_OSOutstandingAmount);

				AssertEquals(1285.71m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-1285.71m, apInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(1285.71m, arInvoiceDisabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-1285.71m, apInvoiceDisabledNewFeature.AH_OutstandingAmount);

				//1st Match (this month): os amount 14000000m, local amount 1000m
				TestObjectCreator.CreateAndMatchMiscellaneousTransactionForNewOSOutstandingAmountFeature(arInvoiceEnabledNewFeature, 1, 14000000m, "M001");
				TestObjectCreator.CreateAndMatchMiscellaneousTransactionForNewOSOutstandingAmountFeature(apInvoiceEnabledNewFeature, 1, -14000000m, "M002");
				TestObjectCreator.CreateAndMatchMiscellaneousTransaction(arInvoiceDisabledNewFeature, ZDateTime.Today.AddDays(1), 14000000m, "M003");
				TestObjectCreator.CreateAndMatchMiscellaneousTransaction(apInvoiceDisabledNewFeature, ZDateTime.Today.AddDays(1), -14000000m, "M004");
				Factory.Save();

				AssertEquals(4000000m, arInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(-4000000m, apInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, arInvoiceDisabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, apInvoiceDisabledNewFeature.AH_OSOutstandingAmount);

				AssertEquals(285.71m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-285.71m, apInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(285.71m, arInvoiceDisabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-285.71m, apInvoiceDisabledNewFeature.AH_OutstandingAmount);

				//2nd Match (next month): os amount 1000000m, local amount 71.43m
				var receipt1 = TestObjectCreator.CreateAndMatchARReceiptForARInvoiceForNewOSOutstandingAmountFeature(arInvoiceEnabledNewFeature, 31, 1000000m, "M005");
				var payment1 = TestObjectCreator.CreateAndMatchAPPaymentForAPInvoiceForNewOSOutstandingAmountFeature(apInvoiceEnabledNewFeature, 31, -1000000m, "M006");
				var receipt2 = TestObjectCreator.CreateAndMatchARReceiptForARInvoice(arInvoiceDisabledNewFeature, ZDateTime.Today.AddDays(31), 1000000m, "M007");
				var payment2 = TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(apInvoiceDisabledNewFeature, ZDateTime.Today.AddDays(31), -1000000m, "M008", 1m, true);
				receipt1.AH_PostDate = ZDateTime.Today.AddDays(31);
				payment1.AH_PostDate = ZDateTime.Today.AddDays(31);
				receipt2.AH_PostDate = ZDateTime.Today.AddDays(31);
				payment2.AH_PostDate = ZDateTime.Today.AddDays(31);
				Factory.Save();

				AssertEquals(3000000m, arInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(-3000000m, apInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, arInvoiceDisabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, apInvoiceDisabledNewFeature.AH_OSOutstandingAmount);

				AssertEquals(214.28m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-214.28m, apInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(214.28m, arInvoiceDisabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-214.28m, apInvoiceDisabledNewFeature.AH_OutstandingAmount);
			}
		}

		[TestDate(2022, 10, 1)]
		public void TestNewOSOutstandingAmountFeature_IsReciprocal()
		{
			Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).GC_IsReciprocal = true;
			Factory.Save();

			Invoice arInvoiceEnabledNewFeature, apInvoiceEnabledNewFeature, arInvoiceDisabledNewFeature, apInvoiceDisabledNewFeature;
			PrepareDataForNewOSOutstandingAmountFeature_PartlyPaid();

			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.DataType.SuspendValidation())
			{
				AssertForNewOSOutstandingAmountFeatureWhenEnableNewOSOutstandingAmountFeature(Guid.Empty);
				AssertForNewOSOutstandingAmountFeatureWhenDisableNewOSOutstandingAmountFeature(Guid.Empty);

				AssertForNewOSOutstandingAmountFeatureWhenEnableNewOSOutstandingAmountFeature(GlbCompany.CurrentCompany.PK.ToGuid());
				AssertForNewOSOutstandingAmountFeatureWhenDisableNewOSOutstandingAmountFeature(GlbCompany.CurrentCompany.PK.ToGuid());
			}

			//Note:
			// @ShowAllTransactions = 'Y'
			// @UseOutstandingAmount = 1
			//
			// For arInvoiceEnabledNewFeature, apInvoiceEnabledNewFeature
			// Balance = AH_OSOutstandingAmount + MatchedInFuturePeriodInOSCurrency
			//		   = 3.33 + 16.66
			//		   = 19.99m
			//
			// For arInvoiceDisabledNewFeature, apInvoiceDisabledNewFeature
			// Balance = (AH_OutstandingAmount + MatchedInFuturePeriodInLocalCurrency) / AH_ExchangeRate
			//		   = (10 + 50) / 3
			//		   = 20m

			void AssertForNewOSOutstandingAmountFeatureWhenEnableNewOSOutstandingAmountFeature(Guid companyPK)
			{
				using (companyPK == Guid.Empty ? null : AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, true))
				{
					var args = SPArgs.Default(PeriodCalculator);
					args.ShowAll = true;
					args.ShowInInvoicedCurrency = true;
					args.ReportDate = ZDateTime.Today.AddDays(31).ToDateTime();  //run report next month
					args.OrderBy = "TRN";
					args.FutureRECPAYNotInBalance = true;

					args.Ledger = LedgerTypes.AccountsReceivable;
					var resultTable1 = RunScriptCore(args);
					AssertDataTableAllRows("@LedgerType = 'AR'",
						resultTable1,
						new[] { "InvoiceRef", "Balance", "BalanceInOS", "BalanceInLocal", "MatchedInFuturePeriodInInvoiceCurrency", "MatchedInFuturePeriodInOSCurrency", "MatchedInFuturePeriodInLocalCurrency" },
						new object[][]
						{
							new object[] { arInvoiceEnabledNewFeature.AH_TransactionNum,		19.99m, 19.99m, 50.00m,				16.66m, 16.66m, 50.00m },
							new object[] { arInvoiceDisabledNewFeature.AH_TransactionNum,		20.00m, DBNull.Value, 50.00m,		16.67m, DBNull.Value, 50.00m },
						}
					);

					args.Ledger = LedgerTypes.AccountsPayable;
					var resultTable2 = RunScriptCore(args);
					AssertDataTableAllRows("@LedgerType = 'AP'",
						resultTable2,
						new[] { "InvoiceRef", "Balance", "BalanceInOS", "BalanceInLocal", "MatchedInFuturePeriodInInvoiceCurrency", "MatchedInFuturePeriodInOSCurrency", "MatchedInFuturePeriodInLocalCurrency" },
						new object[][]
						{
							new object[] { apInvoiceEnabledNewFeature.AH_TransactionNum,		19.99m, 19.99m, 50.00m,				16.66m, 16.66m, 50.00m },
							new object[] { apInvoiceDisabledNewFeature.AH_TransactionNum,		20.00m, DBNull.Value, 50.00m,		16.67m, DBNull.Value, 50.00m },
						}
					);
				}
			}

			void AssertForNewOSOutstandingAmountFeatureWhenDisableNewOSOutstandingAmountFeature(Guid companyPK)
			{
				using (companyPK == Guid.Empty ? null : AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, false))
				{
					var args = SPArgs.Default(PeriodCalculator);
					args.ShowAll = true;
					args.ShowInInvoicedCurrency = true;
					args.ReportDate = ZDateTime.Today.AddDays(31).ToDateTime();  //run report next month
					args.OrderBy = "TRN";
					args.FutureRECPAYNotInBalance = true;

					args.Ledger = LedgerTypes.AccountsReceivable;
					var resultTable1 = RunScriptCore(args);
					AssertDataTableAllRows("@LedgerType = 'AR'",
						resultTable1,
						new[] { "InvoiceRef", "Balance", "BalanceInOS", "BalanceInLocal", "MatchedInFuturePeriodInInvoiceCurrency", "MatchedInFuturePeriodInOSCurrency", "MatchedInFuturePeriodInLocalCurrency" },
						new object[][]
						{
							new object[] { arInvoiceEnabledNewFeature.AH_TransactionNum,		20.00m, DBNull.Value, 50.00m,		16.67m, DBNull.Value, 50.00m },
							new object[] { arInvoiceDisabledNewFeature.AH_TransactionNum,		20.00m, DBNull.Value, 50.00m,		16.67m, DBNull.Value, 50.00m },
						}
					);

					args.Ledger = LedgerTypes.AccountsPayable;
					var resultTable2 = RunScriptCore(args);
					AssertDataTableAllRows("@LedgerType = 'AP'",
						resultTable2,
						new[] { "InvoiceRef", "Balance", "BalanceInOS", "BalanceInLocal", "MatchedInFuturePeriodInInvoiceCurrency", "MatchedInFuturePeriodInOSCurrency", "MatchedInFuturePeriodInLocalCurrency" },
						new object[][]
						{
							new object[] { apInvoiceEnabledNewFeature.AH_TransactionNum,		20.00m, DBNull.Value, 50.00m,		 16.67m, DBNull.Value, 50.00m },
							new object[] { apInvoiceDisabledNewFeature.AH_TransactionNum,		20.00m, DBNull.Value, 50.00m,		 16.67m, DBNull.Value, 50.00m },
						}
					);
				}
			}

			void PrepareDataForNewOSOutstandingAmountFeature_PartlyPaid()
			{
				Factory.Load<RefCurrency>(TestObjectCreator.LocalCurrency.PK).RX_SubUnitRatio = 1;
				Factory.Save();

				arInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.USD, 3m, 20.99m, 0m, 63m, 0m) as ARInvoice;
				apInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "002", TestObjectCreator.USD, 3m, 20.99m, 0m, 63m, 0m) as APInvoice;
				arInvoiceDisabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "003", TestObjectCreator.USD, 3m, 20.99m, 0m, 63m, 0m) as ARInvoice;
				apInvoiceDisabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "004", TestObjectCreator.USD, 3m, 20.99m, 0m, 63m, 0m) as APInvoice;

				arInvoiceEnabledNewFeature.Lines[0].AL_OSExTaxAmount = 20.99m;
				apInvoiceEnabledNewFeature.Lines[0].AL_OSExTaxAmount = 20.99m;
				arInvoiceDisabledNewFeature.Lines[0].AL_OSExTaxAmount = 20.99m;
				apInvoiceDisabledNewFeature.Lines[0].AL_OSExTaxAmount = 20.99m;

				arInvoiceEnabledNewFeature.AH_OH = arInvoiceDisabledNewFeature.AH_OH = TestObjectCreator.Debtor1.PK;
				apInvoiceEnabledNewFeature.AH_OH = apInvoiceDisabledNewFeature.AH_OH = TestObjectCreator.Creditor1.PK;

				TestObjectCreator.UpdateInvoiceForNewOSOutstandingAmountFeature(arInvoiceEnabledNewFeature);
				TestObjectCreator.UpdateInvoiceForNewOSOutstandingAmountFeature(apInvoiceEnabledNewFeature);

				Factory.Save();

				AssertEquals(20.99m, arInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(-20.99m, apInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, arInvoiceDisabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, apInvoiceDisabledNewFeature.AH_OSOutstandingAmount);

				AssertEquals(63m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-63m, apInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(63m, arInvoiceDisabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-63m, apInvoiceDisabledNewFeature.AH_OutstandingAmount);

				//1st Match (this month): os amount 1m, local amount 3m
				TestObjectCreator.CreateAndMatchMiscellaneousTransactionForNewOSOutstandingAmountFeature(arInvoiceEnabledNewFeature, 1, 1m, "M001");
				TestObjectCreator.CreateAndMatchMiscellaneousTransactionForNewOSOutstandingAmountFeature(apInvoiceEnabledNewFeature, 1, -1m, "M002");
				TestObjectCreator.CreateAndMatchMiscellaneousTransaction(arInvoiceDisabledNewFeature, ZDateTime.Today.AddDays(1), 1m, "M003");
				TestObjectCreator.CreateAndMatchMiscellaneousTransaction(apInvoiceDisabledNewFeature, ZDateTime.Today.AddDays(1), -1m, "M004");
				Factory.Save();

				AssertEquals(19.99m, arInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(-19.99m, apInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, arInvoiceDisabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, apInvoiceDisabledNewFeature.AH_OSOutstandingAmount);

				AssertEquals(60m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-60m, apInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(60m, arInvoiceDisabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-60m, apInvoiceDisabledNewFeature.AH_OutstandingAmount);

				//2nd Match (next month): os amount 16.66m, local amount 50m
				var receipt1 = TestObjectCreator.CreateAndMatchARReceiptForARInvoiceForNewOSOutstandingAmountFeature(arInvoiceEnabledNewFeature, 31, 16.66m, "M005");
				var payment1 = TestObjectCreator.CreateAndMatchAPPaymentForAPInvoiceForNewOSOutstandingAmountFeature(apInvoiceEnabledNewFeature, 31, -16.66m, "M006");
				var receipt2 = TestObjectCreator.CreateAndMatchARReceiptForARInvoice(arInvoiceDisabledNewFeature, ZDateTime.Today.AddDays(31), 16.66m, "M007");
				var payment2 = TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(apInvoiceDisabledNewFeature, ZDateTime.Today.AddDays(31), -16.66m, "M008", 1m, true);
				receipt1.AH_PostDate = ZDateTime.Today.AddDays(31);
				payment1.AH_PostDate = ZDateTime.Today.AddDays(31);
				receipt2.AH_PostDate = ZDateTime.Today.AddDays(31);
				payment2.AH_PostDate = ZDateTime.Today.AddDays(31);
				Factory.Save();

				AssertEquals(3.33m, arInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(-3.33m, apInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, arInvoiceDisabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, apInvoiceDisabledNewFeature.AH_OSOutstandingAmount);

				AssertEquals(10m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-10m, apInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(10m, arInvoiceDisabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-10m, apInvoiceDisabledNewFeature.AH_OutstandingAmount);
			}
		}

		#endregion

		public void TestContactInfoWIthTooManyCharacters()
		{
			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = new string('a', OrgContactSchema.OC_ContactName.MaxLength);
			contact.OC_Phone = new string('a', OrgContactSchema.OC_Phone.MaxLength);
			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.All.ToString();
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, org, glAccount.PK);

			Factory.Save();

			var resultForOrg = RunScript(
				LedgerTypes.AccountsReceivable,
				false,
				new string[] { org.OH_Code },
				Array.Empty<string>(),
				false);

			AssertEquals("Result for Organisation list", 1, resultForOrg.Rows.Count);
		}

		public void TestTransactionsInvoiceTotalWithOtherTaxes()
		{
			var aparInvoiceEnabledNewFeature = TestObjectCreator.CreateAPInvoice<APInvoice>("AP001", TestObjectCreator.AUD, 1.0m, 1000m, 100m, 0m, 1000m, 100m, 0m, TestObjectCreator.ABIGAS);
			TestObjectCreator.CreateJobCharge(aparInvoiceEnabledNewFeature.Lines[0], TestObjectCreator.Job1, TestObjectCreator.CC1);
			var apInvoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("AP002", TestObjectCreator.AUD, 1.0m, 2000m, 200m, 0m, 2000m, 200m, 0m, TestObjectCreator.Creditor1);
			TestObjectCreator.CreateJobCharge(apInvoice2.Lines[0], TestObjectCreator.Job1, TestObjectCreator.CC1);
			var ararInvoiceEnabledNewFeature = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", TestObjectCreator.AUD, 1, TestObjectCreator.ABIGAS);
			TestObjectCreator.CreateARInvoiceLineWithJobCharge(ararInvoiceEnabledNewFeature, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc1", 1000m, TestObjectCreator.GST1.PK);
			var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("AR002", TestObjectCreator.AUD, 1, TestObjectCreator.Debtor);
			TestObjectCreator.CreateARInvoiceLineWithJobCharge(arInvoice2, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc1", 2000m, TestObjectCreator.GST1.PK);

			AssertEquals(100m, ararInvoiceEnabledNewFeature.AH_GSTAmount);
			AssertEquals(200m, arInvoice2.AH_GSTAmount);

			ararInvoiceEnabledNewFeature.AH_LocalTaxAmountOtherTaxes = ararInvoiceEnabledNewFeature.AH_OSTaxAmountOtherTaxes = 200M;
			arInvoice2.AH_LocalTaxAmountOtherTaxes = arInvoice2.AH_OSTaxAmountOtherTaxes = 300M;
			aparInvoiceEnabledNewFeature.AH_LocalTaxAmountOtherTaxes = aparInvoiceEnabledNewFeature.AH_OSTaxAmountOtherTaxes = -200M;
			apInvoice2.AH_LocalTaxAmountOtherTaxes = apInvoice2.AH_OSTaxAmountOtherTaxes = -300M;

			Factory.Save();

			var result = RunScript(LedgerTypes.AccountsReceivable, false, new string[] { TestObjectCreator.ABIGAS.OH_Code }, Array.Empty<string>(), false);
			AssertDataTableAllRows("AccountsReceivable", result, new[] { "InvoiceTotal", "Balance" }, new object[][] { new object[] { 1300m, 1300m } });

			result = RunScript(LedgerTypes.AccountsReceivable, true, Array.Empty<string>(), Array.Empty<string>(), false);
			AssertDataTableAllRows("AccountsReceivable", result, new[] { "InvoiceTotal", "Balance" }, new object[][] { new object[] { 1300m, 1300m }, new object[] { 2500m, 2500m } });

			result = RunScript(LedgerTypes.AccountsPayable, false, new string[] { TestObjectCreator.ABIGAS.OH_Code }, Array.Empty<string>(), false);
			AssertDataTableAllRows("AccountsPayable", result, new[] { "InvoiceTotal", "Balance" }, new object[][] { new object[] { 1300m, 1300m } });

			result = RunScript(LedgerTypes.AccountsPayable, true, Array.Empty<string>(), Array.Empty<string>(), false);
			AssertDataTableAllRows("AccountsPayable", result, new[] { "InvoiceTotal", "Balance" }, new object[][] { new object[] { 1300m, 1300m }, new object[] { 2500m, 2500m } });
		}

		public void TestComplianceNumberAndSubTypeValues()
		{
			var apInvoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("AP001", TestObjectCreator.AUD, 1.0m, 1000m, 100m, 0m, 1000m, 100m, 0m, TestObjectCreator.ABIGAS);
			TestObjectCreator.CreateJobCharge(apInvoice1.Lines[0], TestObjectCreator.Job1, TestObjectCreator.CC1);
			var apInvoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("AP002", TestObjectCreator.AUD, 1.0m, 2000m, 200m, 0m, 2000m, 200m, 0m, TestObjectCreator.Creditor1);
			TestObjectCreator.CreateJobCharge(apInvoice2.Lines[0], TestObjectCreator.Job1, TestObjectCreator.CC1);
			var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", TestObjectCreator.AUD, 1, TestObjectCreator.ABIGAS);
			TestObjectCreator.CreateARInvoiceLineWithJobCharge(arInvoice1, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc1", 1000m, TestObjectCreator.GST1.PK);
			var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("AR002", TestObjectCreator.AUD, 1, TestObjectCreator.Debtor);
			TestObjectCreator.CreateARInvoiceLineWithJobCharge(arInvoice2, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc1", 2000m, TestObjectCreator.GST1.PK);

			AssertEquals(100m, arInvoice1.AH_GSTAmount);
			AssertEquals(200m, arInvoice2.AH_GSTAmount);

			arInvoice1.AH_ComplianceSubType = "ARE";
			arInvoice1.AH_TransactionReference = "T0000123";
			arInvoice2.AH_ComplianceSubType = "ARI";
			arInvoice2.AH_TransactionReference = "C0000124";
			apInvoice1.AH_ComplianceSubType = "API";
			apInvoice1.AH_TransactionReference = "A0005215";
			apInvoice2.AH_ComplianceSubType = "API";
			apInvoice2.AH_TransactionReference = "A0005214";

			Factory.Save();

			var result = RunScript(LedgerTypes.AccountsReceivable, true, Array.Empty<string>(), Array.Empty<string>(), false);
			AssertDataTableAllRows("AccountsReceivable", result, new[] { "ComplianceSubType", "ComplianceNumber" }, new object[][] { new object[] { "ARE", "T0000123" }, new object[] { "ARI", "C0000124" } });
		}

		public void TestARSettlementGroupList()
		{
			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator.ABIGAS.ARSettlementGroupPK = ZGuid.Empty;
			TestObjectCreator.LocalClient.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.LocalClient2.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.ABIGAS, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.LocalClient, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV3", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, TestObjectCreator.LocalClient2, glAccount.PK);
			Factory.Save();

			DataTable resultForOrg = RunScript(
				LedgerTypes.AccountsReceivable,
				false,
				new string[] { TestObjectCreator.ABIGAS.OH_Code, TestObjectCreator.LocalClient.OH_Code, TestObjectCreator.LocalClient2.OH_Code },
				Array.Empty<string>(),
				false);

			AssertEquals("Result for Organisation list", 3, resultForOrg.Rows.Count);

			DataTable resultForSettlementGroup = RunScript(
				LedgerTypes.AccountsReceivable,
				false,
				Array.Empty<string>(),
				new string[] { TestObjectCreator.ABIGAS.OH_Code },
				false);

			AssertEquals("Result for SettlementGroup list", 3, resultForSettlementGroup.Rows.Count);

			AssertEquals("Results must contain the same amount of rows.", resultForSettlementGroup.Rows.Count, resultForOrg.Rows.Count);

			resultForOrg = RunScript(
				LedgerTypes.AccountsReceivable,
				true,
				new string[] { TestObjectCreator.ABIGAS.OH_Code, TestObjectCreator.LocalClient.OH_Code, TestObjectCreator.LocalClient2.OH_Code },
				Array.Empty<string>(),
				false);

			AssertEquals("Result for Organisation list", 3, resultForOrg.Rows.Count);

			resultForSettlementGroup = RunScript(
				LedgerTypes.AccountsReceivable,
				true,
				Array.Empty<string>(),
				new string[] { TestObjectCreator.ABIGAS.OH_Code },
				false);

			AssertEquals("Result for SettlementGroup list", 3, resultForSettlementGroup.Rows.Count);

			AssertEquals("Results must contain the same amount of rows.", resultForSettlementGroup.Rows.Count, resultForOrg.Rows.Count);

			TestObjectCreator.ABIGAS.ARSettlementGroupPK = TestObjectCreator.Agent.PK;
			Factory.Save();

			resultForSettlementGroup = RunScript(
				LedgerTypes.AccountsReceivable,
				false,
				Array.Empty<string>(),
				new string[] { TestObjectCreator.ABIGAS.OH_Code },
				false);

			AssertEquals("Result for SettlementGroup list", 2, resultForSettlementGroup.Rows.Count);
			AssertEquals("Result don't contain ABIGAS", 0, resultForSettlementGroup.Select("AccountCode = 'ABIGAS'").Length);

			resultForSettlementGroup = RunScript(
				LedgerTypes.AccountsReceivable,
				true,
				Array.Empty<string>(),
				new string[] { TestObjectCreator.ABIGAS.OH_Code },
				false);

			AssertEquals("Result for SettlementGroup list", 2, resultForSettlementGroup.Rows.Count);
			AssertEquals("Result don't contain ABIGAS", 0, resultForSettlementGroup.Select("AccountCode = 'ABIGAS'").Length);
		}

		public void TestAPSettlementGroupList()
		{
			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator.AALSHI.APSettlementGroupPK = ZGuid.Empty;
			TestObjectCreator.Creditor1.APSettlementGroupPK = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.Creditor2.APSettlementGroupPK = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.AALSHI, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV2", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.Creditor1, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV3", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, TestObjectCreator.Creditor2, glAccount.PK);
			Factory.Save();

			DataTable resultForOrg = RunScript(
				LedgerTypes.AccountsPayable,
				false,
				new string[] { TestObjectCreator.AALSHI.OH_Code, TestObjectCreator.Creditor1.OH_Code, TestObjectCreator.Creditor2.OH_Code },
				Array.Empty<string>(),
				false);

			AssertEquals("Result for Organisation list", 3, resultForOrg.Rows.Count);

			DataTable resultForSettlementGroup = RunScript(
				LedgerTypes.AccountsPayable,
				false,
				Array.Empty<string>(),
				new string[] { TestObjectCreator.AALSHI.OH_Code },
				false);

			AssertEquals("Result for SettlementGroup list", 3, resultForSettlementGroup.Rows.Count);

			AssertEquals("Results must contain the same amount of rows.", resultForSettlementGroup.Rows.Count, resultForOrg.Rows.Count);

			resultForOrg = RunScript(
				LedgerTypes.AccountsPayable,
				true,
				new string[] { TestObjectCreator.AALSHI.OH_Code, TestObjectCreator.Creditor1.OH_Code, TestObjectCreator.Creditor2.OH_Code },
				Array.Empty<string>(),
				false);

			AssertEquals("Result for Organisation list", 3, resultForOrg.Rows.Count);

			resultForSettlementGroup = RunScript(
				LedgerTypes.AccountsPayable,
				true,
				Array.Empty<string>(),
				new string[] { TestObjectCreator.AALSHI.OH_Code },
				false);

			AssertEquals("Result for SettlementGroup list", 3, resultForSettlementGroup.Rows.Count);

			AssertEquals("Results must contain the same amount of rows.", resultForSettlementGroup.Rows.Count, resultForOrg.Rows.Count);

			TestObjectCreator.AALSHI.APSettlementGroupPK = TestObjectCreator.Agent.PK;
			Factory.Save();

			resultForSettlementGroup = RunScript(
				LedgerTypes.AccountsPayable,
				false,
				Array.Empty<string>(),
				new string[] { TestObjectCreator.AALSHI.OH_Code },
				false);

			AssertEquals("Result for SettlementGroup list", 2, resultForSettlementGroup.Rows.Count);
			AssertEquals("Result don't contain AALSHI", 0, resultForSettlementGroup.Select("AccountCode = 'AALSHI'").Length);

			resultForSettlementGroup = RunScript(
				LedgerTypes.AccountsPayable,
				true,
				Array.Empty<string>(),
				new string[] { TestObjectCreator.AALSHI.OH_Code },
				false);

			AssertEquals("Result for SettlementGroup list", 2, resultForSettlementGroup.Rows.Count);
			AssertEquals("Result don't contain AALSHI", 0, resultForSettlementGroup.Select("AccountCode = 'AALSHI'").Length);
		}

		public void TestSummaryOnlyWhenOrganisationHasTwoARTerms()
		{
			ZString expectedOrgCode = @"萧萧兮易水寒";
			ZString expectedOrgFullName = @"兮易兮易兮易";

			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator.ABIGAS.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = InvoiceTermsList.FromInvoiceDate.Code;
			TestObjectCreator.ABIGAS.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = InvoiceTermsList.CashOnDelivery.Code;
			TestObjectCreator.ABIGAS.OH_Code = expectedOrgCode;
			TestObjectCreator.ABIGAS.OH_FullName = expectedOrgFullName;
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.FinalInvoice);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.DisbursementInvoice);
			Factory.Save();

			DataTable result = RunScript(
				LedgerTypes.AccountsReceivable,
				false,
				Array.Empty<string>(),
				Array.Empty<string>(),
				true);
			AssertEquals("Result count", 1, result.Rows.Count);
			AssertEquals("Balance", 30M, result.Rows[0]["Balance"]);
			AssertNotEquals("AccountCode", expectedOrgCode, result.Rows[0]["AccountCode"]);
			AssertNotEquals("AccountName", expectedOrgCode, result.Rows[0]["AccountName"]);
		}

		[TestDate(2010, 01, 01)]
		public void TestDueDateOSAndLocalAgeing()
		{
			SetUpDataForAgeingTests("DUE");

			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV4", TestObjectCreator.USD, 0.5M, 100, 0M, 200, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK,
				ZDateTime.Now, ZDateTime.Empty, ZDateTime.Now, false);

			Factory.Save();

			string[][] fieldNames = new string[][]
			{
				new string[] { "Balance", "NotDue3Total", "NotDue2Total", "NotDue1Total", "PeriodCurrent", "Period1Total", "Period2Total", "Period3Total", "Period4Total" },
				new string[] { "BalanceInLocal", "NotDue3TotalInLocal", "NotDue2TotalInLocal", "NotDue1TotalInLocal", "PeriodCurrentInLocal", "Period1TotalInLocal", "Period2TotalInLocal", "Period3TotalInLocal", "Period4TotalInLocal" }
			};

			decimal[][] fieldValues = new decimal[][]
			{
				new decimal[] { 1855, 1615, 65, 45, 130, 0, 0, 0, 0 },
				new decimal[] { 1855, 1530, 85, 65, 145, 30, 0, 0, 0 },
				new decimal[] { 1855, 1425, 105, 85, 165, 45, 30, 0, 0 },
				new decimal[] { 1855, 1300, 125, 105, 185, 65, 45, 30, 0 },
				new decimal[] { 1855, 1155, 145, 125, 205, 85, 65, 45, 30 },
				new decimal[] { 1855, 990, 165, 145, 225, 105, 85, 65, 75 },
				new decimal[] { 1855, 805, 185, 165, 245, 125, 105, 85, 140 },
				new decimal[] { 1855, 600, 205, 185, 265, 145, 125, 105, 225 },
				new decimal[] { 1855, 375, 225, 205, 285, 165, 145, 125, 330 },
				new decimal[] { 1855, 0, 375, 225, 305, 185, 165, 145, 455 },
				new decimal[] { 1855, 0, 0, 375, 325, 205, 185, 165, 600 },
				new decimal[] { 1855, 0, 0, 0, 475, 225, 205, 185, 765 }
			};

			GetAndAssertAgeingResultsByPeriod("DUE", fieldNames, fieldValues);

			GetAndAssertAgeingResultsByDay("DUE", fieldNames, new decimal[] { 1855, 490, 410, 175, 255, 70, 125, 150, 180 });
		}

		[TestDate(2010, 01, 01)]
		public void TestInvoiceTermWhenAllTransactionsAreNotDisplayed()
		{
			ZGuid chargePK = TestObjectCreator.CC1.PK;

			ZInt period = PeriodCalculator.GetFirstPeriodForYear(ZDateTime.Today.Year);
			var setBranchAndDept = new Action<InvoicingBase, ZGuid, ZGuid>((invoice, branchPK, deptPK) =>
				{
					invoice.AH_GB = branchPK;
					invoice.AH_GE = deptPK;
				});

			var arInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV110", TestObjectCreator.USD, 0.5M, 100M, 0M, 200M, 0M, TestObjectCreator.ABIGAS, chargePK, ZDateTime.Now, PeriodCalculator.GetFirstDayForPeriod(period), ZDateTime.Now, false);
			setBranchAndDept(arInvoiceEnabledNewFeature, GlbBranch.CurrentBranch.PK, TestObjectCreator.FESDepartment.PK);

			var invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV210", TestObjectCreator.USD, 0.5M, 110M, 0M, 220M, 0M, TestObjectCreator.ABIGAS, chargePK, ZDateTime.Now, PeriodCalculator.GetLastDayForPeriod(period), ZDateTime.Now, false);
			setBranchAndDept(invoice2, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);

			var invoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV310", TestObjectCreator.USD, 0.5M, 110M, 0M, 220M, 0M, TestObjectCreator.ABIGAS, chargePK, ZDateTime.Now, PeriodCalculator.GetFirstDayForPeriod(period).AddDays(20), ZDateTime.Now, false);
			setBranchAndDept(invoice3, GlbBranch.CurrentBranch.PK, TestObjectCreator.FISDepartment.PK);

			var invoice4 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV410", TestObjectCreator.USD, 0.5M, 110M, 0M, 320M, 0M, TestObjectCreator.ABIGAS, chargePK, ZDateTime.Now, PeriodCalculator.GetFirstDayForPeriod(period).AddDays(20), ZDateTime.Now, false);
			setBranchAndDept(invoice4, GlbBranch.CurrentBranch.PK, TestObjectCreator.FIADepartment.PK);

			var invoice5 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV999", TestObjectCreator.AUD, 1.0M, 100M, 0M, 100M, 0M, TestObjectCreator.AALSHI, chargePK, ZDateTime.Now, PeriodCalculator.GetFirstDayForPeriod(period), ZDateTime.Now, false);
			setBranchAndDept(arInvoiceEnabledNewFeature, GlbBranch.CurrentBranch.PK, TestObjectCreator.FESDepartment.PK);
			Factory.Save();

			TestObjectCreator.ABIGAS.CompanyData.ARTerms.DeleteAll();

			var arTerm1 = TestObjectCreator.ABIGAS.CompanyData.ARTerms.AddNew();
			SetupTermsInfo(arTerm1, "ALL", ZGuid.Empty, TestObjectCreator.FESDepartment.PK, "ALL", "ALL", "ALL", "COD", 0);

			var arTerm2 = TestObjectCreator.ABIGAS.CompanyData.ARTerms.AddNew();
			SetupTermsInfo(arTerm2, "ALL", GlbBranch.CurrentBranch.PK, TestObjectCreator.FESDepartment.PK, "ALL", "ALL", "ALL", "PIA", 40);

			var arTerm3 = TestObjectCreator.ABIGAS.CompanyData.ARTerms.AddNew();
			SetupTermsInfo(arTerm3, "ALL", ZGuid.Empty, GlbDepartment.CurrentDepartment.PK, "ALL", "ALL", "ALL", "INV", 25);

			var arTerm4 = TestObjectCreator.ABIGAS.CompanyData.ARTerms.AddNew();
			SetupTermsInfo(arTerm4, "ALL", ZGuid.Empty, TestObjectCreator.FIADepartment.PK, "ALL", "ALL", "ALL", "DEF", 0);

			var arTerm5 = TestObjectCreator.ABIGAS.CompanyData.ARTerms.AddNew();
			SetupTermsInfo(arTerm5, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL", "MIC", 1);

			TestObjectCreator.CreateARSettlementGroup(TestObjectCreator.ABIGAS, TestObjectCreator.AALSHI);
			TestObjectCreator.AALSHI.CompanyData.ARTerms.DeleteAll();
			var termSG1 = TestObjectCreator.AALSHI.CompanyData.ARTerms.AddNew();
			SetupTermsInfo(termSG1, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL", "MIC", 1);

			Factory.Save();

			DataTable result = RunScriptForAgeing(PeriodCalculator.GetPeriodFromDate(ZDateTime.Today), LedgerTypes.AccountsReceivable, false, new string[] { TestObjectCreator.ABIGAS.OH_Code }, Array.Empty<string>(), false, "DUE", true, true, "PER", 0, 0, 0, 0);
			AssertEquals("Result count", 4, result.Rows.Count);
			AssertCollectionContains(arInvoiceEnabledNewFeature.AH_TransactionNum + ", PIA", result.Rows.OfType<DataRow>(), (row) => Convert.ToString(row["InvoiceRef"]).Equals(arInvoiceEnabledNewFeature.AH_TransactionNum) && Convert.ToString(row["InvoiceTerm"]).Trim().Equals("PIA"));
			AssertCollectionContains(invoice2.AH_TransactionNum + ", 25 Days INV", result.Rows.OfType<DataRow>(), (row) => Convert.ToString(row["InvoiceRef"]).Equals(invoice2.AH_TransactionNum) && Convert.ToString(row["InvoiceTerm"]).Trim().Equals("25 Days INV"));
			AssertCollectionContains(invoice3.AH_TransactionNum + ", 1 Months MIC", result.Rows.OfType<DataRow>(), (row) => Convert.ToString(row["InvoiceRef"]).Equals(invoice3.AH_TransactionNum) && Convert.ToString(row["InvoiceTerm"]).Trim().Equals("1 Months MIC"));
			AssertCollectionContains(invoice4.AH_TransactionNum + ", 1 Months MIC", result.Rows.OfType<DataRow>(), (row) => Convert.ToString(row["InvoiceRef"]).Equals(invoice4.AH_TransactionNum) && Convert.ToString(row["InvoiceTerm"]).Trim().Equals("1 Months MIC"));

			result = RunScriptForAgeing(PeriodCalculator.GetPeriodFromDate(ZDateTime.Today), LedgerTypes.AccountsReceivable, false, new string[] { TestObjectCreator.ABIGAS.OH_Code }, Array.Empty<string>(), true, "DUE", true, true, "PER", 0, 0, 0, 0);
			AssertEquals("Result count", 1, result.Rows.Count);
		}

		[TestDate(2010, 01, 01)]
		public void TestInvoiceTermConcatenationForSummaryAgingReport()
		{
			ZGuid chargePK = TestObjectCreator.CC1.PK;

			ZInt period = PeriodCalculator.GetFirstPeriodForYear(ZDateTime.Today.Year);
			var setBranchAndDept = new Action<InvoicingBase, ZGuid, ZGuid>((invoice, branchPK, deptPK) =>
			{
				invoice.AH_GB = branchPK;
				invoice.AH_GE = deptPK;
			});

			var newBranch = TestObjectCreator.CreateNewBranch(GlbCompany.CurrentCompany, "NBE");
			var nonCurrentBranch = TestObjectCreator.CreateNewBranch(GlbCompany.CurrentCompany, "NB2");
			GlbCompany.CurrentCompany.Factory.Save();

			var arInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV110", TestObjectCreator.USD, 0.5M, 100M, 0M, 200M, 0M, TestObjectCreator.ABIGAS, chargePK, ZDateTime.Now, PeriodCalculator.GetFirstDayForPeriod(period), ZDateTime.Now, false);
			setBranchAndDept(arInvoiceEnabledNewFeature, GlbBranch.CurrentBranch.PK, TestObjectCreator.FESDepartment.PK);

			var invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV210", TestObjectCreator.USD, 0.5M, 110M, 0M, 220M, 0M, TestObjectCreator.ABIGAS, chargePK, ZDateTime.Now, PeriodCalculator.GetLastDayForPeriod(period), ZDateTime.Now, false);
			setBranchAndDept(invoice2, newBranch.PK, GlbDepartment.CurrentDepartment.PK);

			var invoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV310", TestObjectCreator.USD, 0.5M, 110M, 0M, 220M, 0M, TestObjectCreator.ABIGAS, chargePK, ZDateTime.Now, PeriodCalculator.GetFirstDayForPeriod(period).AddDays(20), ZDateTime.Now, false);
			setBranchAndDept(invoice3, GlbBranch.CurrentBranch.PK, TestObjectCreator.FISDepartment.PK);

			var invoice4 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV410", TestObjectCreator.USD, 0.75M, 240M, 0M, 320M, 0M, TestObjectCreator.ABIGAS, chargePK, ZDateTime.Now, PeriodCalculator.GetFirstDayForPeriod(period).AddDays(20), ZDateTime.Now, false);
			setBranchAndDept(invoice4, GlbBranch.CurrentBranch.PK, TestObjectCreator.FIADepartment.PK);

			var invoice5 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV999", TestObjectCreator.AUD, 1.0M, 100M, 0M, 100M, 0M, TestObjectCreator.AALSHI, chargePK, ZDateTime.Now, PeriodCalculator.GetFirstDayForPeriod(period), ZDateTime.Now, false);
			setBranchAndDept(invoice5, GlbBranch.CurrentBranch.PK, TestObjectCreator.FESDepartment.PK);

			Factory.Save();

			TestObjectCreator.ABIGAS.CompanyData.ARTerms.DeleteAll();

			var arTerm1 = TestObjectCreator.ABIGAS.CompanyData.ARTerms.AddNew();
			SetupTermsInfo(arTerm1, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL", "COD", 0);

			var arTerm2 = TestObjectCreator.ABIGAS.CompanyData.ARTerms.AddNew();
			SetupTermsInfo(arTerm2, "ALL", GlbBranch.CurrentBranch.PK, ZGuid.Empty, "ALL", "ALL", "ALL", "PIA", 40);

			var arTerm3 = TestObjectCreator.ABIGAS.CompanyData.ARTerms.AddNew();
			SetupTermsInfo(arTerm3, "ALL", nonCurrentBranch.PK, ZGuid.Empty, "ALL", "ALL", "ALL", "INV", 25);

			var arTerm4 = TestObjectCreator.ABIGAS.CompanyData.ARTerms.AddNew();
			SetupTermsInfo(arTerm4, "ALL", ZGuid.Empty, TestObjectCreator.FIADepartment.PK, "ALL", "ALL", "ALL", "DEF", 0);

			TestObjectCreator.CreateARSettlementGroup(TestObjectCreator.ABIGAS, TestObjectCreator.AALSHI);

			TestObjectCreator.AALSHI.CompanyData.ARTerms.DeleteAll();
			var arTerm99 = TestObjectCreator.AALSHI.CompanyData.ARTerms.AddNew();
			SetupTermsInfo(arTerm99, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL", "INV", 10);

			Factory.Save();

			DataTable result = RunScriptForAgeing(PeriodCalculator.GetPeriodFromDate(ZDateTime.Today), LedgerTypes.AccountsReceivable, false, new string[] { TestObjectCreator.ABIGAS.OH_Code, TestObjectCreator.AALSHI.OH_Code }, Array.Empty<string>(), false, "DUE", true, true, "PER", 0, 0, 0, 0);
			AssertEquals("Result count", 5, result.Rows.Count);
			AssertCollectionContains(arInvoiceEnabledNewFeature.AH_TransactionNum + ", PIA", result.Rows.OfType<DataRow>(), (row) => Convert.ToString(row["InvoiceRef"]).Equals(arInvoiceEnabledNewFeature.AH_TransactionNum) && Convert.ToString(row["InvoiceTerm"]).Trim().Equals("PIA"));
			AssertCollectionContains(invoice2.AH_TransactionNum + ", COD", result.Rows.OfType<DataRow>(), (row) => Convert.ToString(row["InvoiceRef"]).Equals(invoice2.AH_TransactionNum) && Convert.ToString(row["InvoiceTerm"]).Trim().Equals("COD"));
			AssertCollectionContains(invoice3.AH_TransactionNum + ", PIA", result.Rows.OfType<DataRow>(), (row) => Convert.ToString(row["InvoiceRef"]).Equals(invoice3.AH_TransactionNum) && Convert.ToString(row["InvoiceTerm"]).Trim().Equals("PIA"));
			AssertCollectionContains(invoice4.AH_TransactionNum + ", 10 Days INV", result.Rows.OfType<DataRow>(), (row) => Convert.ToString(row["InvoiceRef"]).Equals(invoice4.AH_TransactionNum) && Convert.ToString(row["InvoiceTerm"]).Trim().Equals("10 Days INV"));
			AssertCollectionContains(invoice5.AH_TransactionNum + ", 10 Days INV", result.Rows.OfType<DataRow>(), (row) => Convert.ToString(row["InvoiceRef"]).Equals(invoice5.AH_TransactionNum) && Convert.ToString(row["InvoiceTerm"]).Trim().Equals("10 Days INV"));

			result = RunScriptForAgeing(PeriodCalculator.GetPeriodFromDate(ZDateTime.Today), LedgerTypes.AccountsReceivable, false, new string[] { TestObjectCreator.ABIGAS.OH_Code, TestObjectCreator.AALSHI.OH_Code }, Array.Empty<string>(), true, "DUE", true, true, "PER", 0, 0, 0, 0);
			AssertEquals("Result count", 2, result.Rows.Count);

			var headers = new[] { "InvoiceTerm", "AverageExchangeRate" };

			var rowABIGAS = result.Select($"AccountCode = '{TestObjectCreator.ABIGAS.OH_Code}'").First();
			AssertDataRow(rowABIGAS, headers, new object[] { "10 Days INV, COD, PIA", 0.58333333333333333M });

			var rowAALSHI = result.Select($"AccountCode = '{TestObjectCreator.AALSHI.OH_Code}'").First();
			AssertDataRow(rowAALSHI, headers, new object[] { "10 Days INV", 1M });
		}

		[TestDate(2010, 01, 01)]
		public void TestPostDateOSAndLocalAgeing()
		{
			SetUpDataForAgeingTests("PST");

			string[][] fieldNames = new string[][]
			{
				new string[] { "Balance", "PeriodCurrent", "Period1Total", "Period2Total", "Period3Total", "Period4Total" },
				new string[] { "BalanceInLocal", "PeriodCurrentInLocal", "Period1TotalInLocal", "Period2TotalInLocal", "Period3TotalInLocal", "Period4TotalInLocal" }
			};

			decimal[][] fieldValues = new decimal[][]
			{
				new decimal[] { 30, 30, 0, 0, 0, 0 },
				new decimal[] { 75, 45, 30, 0, 0, 0 },
				new decimal[] { 140, 65, 45, 30, 0, 0 },
				new decimal[] { 225, 85, 65, 45, 30, 0 },
				new decimal[] { 330, 105, 85, 65, 45, 30 },
				new decimal[] { 455, 125, 105, 85, 65, 75 },
				new decimal[] { 600, 145, 125, 105, 85, 140 },
				new decimal[] { 765, 165, 145, 125, 105, 225 },
				new decimal[] { 950, 185, 165, 145, 125, 330 },
				new decimal[] { 1155, 205, 185, 165, 145, 455 },
				new decimal[] { 1380, 225, 205, 185, 165, 600 },
				new decimal[] { 1625, 245, 225, 205, 185, 765 }
			};

			GetAndAssertAgeingResultsByPeriod("PST", fieldNames, fieldValues);

			GetAndAssertAgeingResultsByDay("PST", fieldNames, new decimal[] { 600, 145, 125, 150, 75, 105 });
		}

		[TestDate(2010, 01, 01)]
		public void TestInvoiceDateOSAndLocalAgeing()
		{
			SetUpDataForAgeingTests("INV");

			string[][] fieldNames = new string[][]
			{
				new string[] { "Balance", "PeriodCurrent", "Period1Total", "Period2Total", "Period3Total", "Period4Total" },
				new string[] { "BalanceInLocal", "PeriodCurrentInLocal", "Period1TotalInLocal", "Period2TotalInLocal", "Period3TotalInLocal", "Period4TotalInLocal" }
			};

			decimal[][] fieldValues = new decimal[][]
			{
				new decimal[] { 1755, 1755, 0, 0, 0, 0 },
				new decimal[] { 1755, 1725, 30, 0, 0, 0 },
				new decimal[] { 1755, 1680, 45, 30, 0, 0 },
				new decimal[] { 1755, 1615, 65, 45, 30, 0 },
				new decimal[] { 1755, 1530, 85, 65, 45, 30 },
				new decimal[] { 1755, 1425, 105, 85, 65, 75 },
				new decimal[] { 1755, 1300, 125, 105, 85, 140 },
				new decimal[] { 1755, 1155, 145, 125, 105, 225 },
				new decimal[] { 1755, 990, 165, 145, 125, 330 },
				new decimal[] { 1755, 805, 185, 165, 145, 455 },
				new decimal[] { 1755, 600, 205, 185, 165, 600 },
				new decimal[] { 1755, 375, 225, 205, 185, 765 }
			};

			GetAndAssertAgeingResultsByPeriod("INV", fieldNames, fieldValues);

			GetAndAssertAgeingResultsByDay("INV", fieldNames, new decimal[] { 1755, 1300, 125, 150, 75, 105 });
		}

		public void TestDisplayingOfINVAndCRDDisbursement()
		{
			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var note1 = creator.CreateInvoiceWithLine(typeof(ARCreditNote), "100", creator.AUD, 1.0M, 0M, 0M, 100M, 0M, creator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.DisbursementInvoice);
			var note2 = creator.CreateInvoiceWithLine(typeof(ARCreditNote), "101", creator.AUD, 1.0M, 0M, 0M, 101M, 0M, creator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.DisbursementInForeignCurrency);

			var arInvoiceEnabledNewFeature = creator.CreateInvoiceWithLine(typeof(ARInvoice), "001", creator.AUD, 1.0M, 102M, 102M, 102M, 102M, creator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.DisbursementInvoice);
			var invoice2 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "002", creator.AUD, 1.0M, 103M, 103M, 103M, 103M, creator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.DisbursementInForeignCurrency);

			Factory.Save();

			DataTable result = RunScriptCore(
				PeriodCalculator.GetPeriodFromDate(ZDateTime.Now),
				LedgerTypes.AccountsReceivable,
				false,
				Array.Empty<string>(),
				Array.Empty<string>(),
				true,
				"",
				false,
				false,
				"PER",
				0,
				0,
				0,
				0,
				"Y",
				"Y",
				"",
				"");

			CombineAssertions(delegate
			{
				AssertEquals("Only should contain 1 row", 1, result.Rows.Count);
				AssertEquals("All should exist under ABIGAS", "ABIGAS      ", result.Rows[0]["AccountCode"]);
				AssertEquals("Total all four invoices and credit notes should be 102 + 103 - 101 - 100 = +4", ((decimal)4.0000), result.Rows[0]["Balance"]);
			});
		}

		public void TestForeignCurrencyAmountsAreMultipliedForIsReciprocalCompanies()
		{
			var originalValue = GlbCompany.CurrentCompany.GC_IsReciprocal;
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			try
			{
				var glAccount = TestObjectCreator.GetGLAccountFromDB();
				var creator = new TestObjectCreator(Factory);
				creator.CreateInvoiceWithLine(typeof(ARCreditNote), "100", creator.USD, 2M, 0M, 0M, 100M, 0M, creator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.DisbursementInvoice);
				creator.CreateInvoiceWithLine(typeof(ARInvoice), "001", creator.USD, 2M, 104M, 104M, 104M, 104M, creator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.DisbursementInvoice);

				Factory.Save();

				var result = RunScriptCore(
					PeriodCalculator.GetPeriodFromDate(ZDateTime.Now),
					LedgerTypes.AccountsReceivable,
					false,
					Array.Empty<string>(),
					Array.Empty<string>(),
					true,
					"",
					true,
					true,
					"PER",
					0,
					0,
					0,
					0,
					"Y",
					"Y",
					"",
					"");

				CombineAssertions(delegate
				{
					AssertEquals("Only should contain 1 row", 1, result.Rows.Count);
					AssertEquals("Local amount should be 104 - 100", 4M, result.Rows[0]["BalanceInLocal"]);
					AssertEquals("Company is IsReciprocal so balance = local balance (4) / exchange rate (2)", 2M, result.Rows[0]["Balance"]);
				});
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = originalValue;
			}
		}

		public void TestGainLossOnRevaluationWithForeignCurrency()
		{
			var originalValue = GlbCompany.CurrentCompany.GC_IsReciprocal;
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			try
			{
				var exRate = TestObjectCreator.USD.ExchangeRates.AddNew();
				exRate.RE_StartDate = ZDateTime.Now.AddMonths(1).AddDays(-1);
				exRate.RE_SellRate = 2m;
				exRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.PeriodEndRate;

				Factory.Save();

				var glAccount = TestObjectCreator.GetGLAccountFromDB();
				TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "001", TestObjectCreator.USD, 0.1M, 5000M, 0M, 500M, 0M, TestObjectCreator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.FinalInvoice);
				TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "002", TestObjectCreator.USD, 1.5M, 5000M, 0M, 7500M, 0M, TestObjectCreator.Creditor1, glAccount.PK, InvoiceTypesList.Codes.FinalInvoice);

				Factory.Save();

				var result = RunScriptCore(
					PeriodCalculator.GetPeriodFromDate(ZDateTime.Now),
					LedgerTypes.AccountsPayable,
					false,
					Array.Empty<string>(),
					Array.Empty<string>(),
					true,
					"",
					true,
					true,
					"PER",
					0,
					0,
					0,
					0,
					"Y",
					"Y",
					"",
					"");

				CombineAssertions(delegate
				{
					AssertEquals("Only should contain 2 row", 2, result.Rows.Count);
					AssertEquals(2000M, result.Rows[0]["GainLossOnRevaluation"]);
					AssertEquals(2000M, result.Rows[0]["LossOnRevaluation"]);
					AssertEquals(DBNull.Value, result.Rows[0]["GainOnRevaluation"]);

					AssertEquals(-5000M, result.Rows[1]["GainLossOnRevaluation"]);
					AssertEquals(DBNull.Value, result.Rows[1]["LossOnRevaluation"]);
					AssertEquals(5000M, result.Rows[1]["GainOnRevaluation"]);
				});

				TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "003", TestObjectCreator.USD, 0.1M, 5000M, 0M, 500M, 0M, TestObjectCreator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.DisbursementInvoice);
				TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "004", TestObjectCreator.USD, 1.5M, 5000M, 0M, 7500M, 0M, TestObjectCreator.Creditor1, glAccount.PK, InvoiceTypesList.Codes.DisbursementInvoice);

				Factory.Save();

				result = RunScriptCore(
					PeriodCalculator.GetPeriodFromDate(ZDateTime.Now),
					LedgerTypes.AccountsReceivable,
					false,
					Array.Empty<string>(),
					Array.Empty<string>(),
					true,
					"",
					true,
					true,
					"PER",
					0,
					0,
					0,
					0,
					"Y",
					"Y",
					"",
					"");

				CombineAssertions(delegate
				{
					AssertEquals("Only should contain 2 row", 2, result.Rows.Count);
					AssertEquals(2000M, result.Rows[0]["GainLossOnRevaluation"]);
					AssertEquals(DBNull.Value, result.Rows[0]["LossOnRevaluation"]);
					AssertEquals(2000M, result.Rows[0]["GainOnRevaluation"]);

					AssertEquals(-5000M, result.Rows[1]["GainLossOnRevaluation"]);
					AssertEquals(5000M, result.Rows[1]["LossOnRevaluation"]);
					AssertEquals(DBNull.Value, result.Rows[1]["GainOnRevaluation"]);
				});
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = originalValue;
			}
		}

		public void TestGainLossOnRevaluationGroupByBranch()
		{
			var originalValue = GlbCompany.CurrentCompany.GC_IsReciprocal;
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			try
			{
				var nonCurrentBranch = TestObjectCreator.CreateBranch("NE3", GlbCompany.CurrentCompany);

				var exRate = TestObjectCreator.USD.ExchangeRates.AddNew();
				exRate.RE_StartDate = ZDateTime.Now.AddMonths(1).AddDays(-1);
				exRate.RE_SellRate = 2m;
				exRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.PeriodEndRate;

				Factory.Save();

				var glAccount = TestObjectCreator.GetGLAccountFromDB();
				TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "001", TestObjectCreator.USD, 0.1M, 5000M, 0M, 500M, 0M, TestObjectCreator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.FinalInvoice);
				var inovice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "002", TestObjectCreator.USD, 1.5M, 5000M, 0M, 7500M, 0M, TestObjectCreator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.FinalInvoice);
				inovice.AH_GB = nonCurrentBranch.PK;

				Factory.Save();

				var result = RunScriptCore(
					PeriodCalculator.GetPeriodFromDate(ZDateTime.Now),
					LedgerTypes.AccountsPayable,
					false,
					Array.Empty<string>(),
					Array.Empty<string>(),
					true,
					"",
					true,
					true,
					"PER",
					0,
					0,
					0,
					0,
					"Y",
					"Y",
					"Transaction Currency, Transaction Branch then Organisation",
					"");

				CombineAssertions(delegate
				{
					AssertEquals("Only should contain 2 row", 2, result.Rows.Count);
					AssertEquals(2000M, result.Rows[0]["GainLossOnRevaluation"]);
					AssertEquals(2000M, result.Rows[0]["LossOnRevaluation"]);
					AssertEquals(DBNull.Value, result.Rows[0]["GainOnRevaluation"]);
					AssertEquals(GlbBranch.CurrentBranch.GB_Code, result.Rows[0]["BranchCode"]);

					AssertEquals(-5000M, result.Rows[1]["GainLossOnRevaluation"]);
					AssertEquals(DBNull.Value, result.Rows[1]["LossOnRevaluation"]);
					AssertEquals(5000M, result.Rows[1]["GainOnRevaluation"]);
					AssertEquals(nonCurrentBranch.GB_Code, result.Rows[1]["BranchCode"]);
				});
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = originalValue;
			}
		}

		public void TestForeignCurrencyAmountsAreDividedForIsNonReciprocalCompanies()
		{
			var originalValue = GlbCompany.CurrentCompany.GC_IsReciprocal;
			GlbCompany.CurrentCompany.GC_IsReciprocal = false;
			try
			{
				var glAccount = TestObjectCreator.GetGLAccountFromDB();
				var creator = new TestObjectCreator(Factory);
				creator.CreateInvoiceWithLine(typeof(ARCreditNote), "100", creator.USD, 2M, 0M, 0M, 100M, 0M, creator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.DisbursementInvoice);
				creator.CreateInvoiceWithLine(typeof(ARInvoice), "001", creator.USD, 2M, 104M, 104M, 104M, 104M, creator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.DisbursementInvoice);

				Factory.Save();

				var result = RunScriptCore(
					PeriodCalculator.GetPeriodFromDate(ZDateTime.Now),
					LedgerTypes.AccountsReceivable,
					false,
					Array.Empty<string>(),
					Array.Empty<string>(),
					true,
					"",
					true,
					true,
					"PER",
					0,
					0,
					0,
					0,
					"Y",
					"Y",
					"",
					"");

				CombineAssertions(delegate
				{
					AssertEquals("Only should contain 1 row", 1, result.Rows.Count);
					AssertEquals("Local amount should be 104 - 100", 4M, result.Rows[0]["BalanceInLocal"]);
					AssertEquals("Company is IsReciprocal so balance = local balance (4) * exchange rate (2)", 8M, result.Rows[0]["Balance"]);
				});
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = originalValue;
			}
		}

		[TestDate(2016, 06, 01)]
		public void TestExcludeTransactionAmountMatchedinFuturePeriod()
		{
			var arInvoiceEnabledNewFeature = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "Inv001", TestObjectCreator.USD, 1.5m, 150M, 0M, 150M, 0M);
			arInvoiceEnabledNewFeature.AH_PostDate = ZDateTime.Today;
			var payment1 = TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(arInvoiceEnabledNewFeature, ZDateTime.Today.AddMonths(2), -10M, "M01", 1m);
			payment1.AH_PostDate = ZDateTime.Today.AddMonths(2);
			var apDiscount = Factory.NewWithValidTestData<APDiscount>();
			apDiscount.AH_OutstandingAmount = -3.33M;
			apDiscount.AH_InvoiceAmount = -3.33M;
			apDiscount.AH_OSTotal = -3.33M;
			var apDiscountAsIMatching = (IMatching)apDiscount;
			apDiscountAsIMatching.FullyPay(ZDateTime.Today.AddMonths(2));
			apDiscountAsIMatching.GenerateMatchLinks();
			var apInvoiceAsIMatching = (IMatching)arInvoiceEnabledNewFeature;
			apInvoiceAsIMatching.CurrentMatchGroup.AddRange(apDiscountAsIMatching.CurrentMatchGroup);
			apDiscountAsIMatching.CurrentMatchGroup.RemoveAll();
			apInvoiceAsIMatching.CurrentMatchGroup.SetMatchGroupNumberAndMatchDate("M01", ZDateTime.Today.AddMonths(2));
			Factory.Save();

			var invoice2 = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "Inv002", TestObjectCreator.AUD, 1m, 100M, 0M, 100M, 0M);
			invoice2.AH_PostDate = ZDateTime.Today.AddDays(1);
			Factory.Save();

			AssertEquals(-143.33m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
			AssertEquals(-100m, invoice2.AH_OutstandingAmount);

			DataTable result = RunScriptCore(
				PeriodCalculator.GetPeriodFromDate(ZDateTime.Now),
				LedgerTypes.AccountsPayable,
				false,
				Array.Empty<string>(),
				Array.Empty<string>(),
				false,
				"",
				true,
				true,
				"PER",
				0,
				0,
				0,
				0,
				"Y",
				"Y",
				"",
				"Post Date, Type then Transaction Number",
				excludeMatchedToFutureRECPAY: true,
				futureRECPAYNotInBalance: true);

			CombineAssertions(delegate
			{
				AssertEquals("should contain 2 rows", 2, result.Rows.Count);
				AssertEquals(arInvoiceEnabledNewFeature.PK, result.Rows[0]["TransactionPK"]);
				AssertEquals("Balance should be 150 * 1.5 - 10 = 215", 215M, result.Rows[0]["Balance"]);
				AssertEquals("BalanceInLocal should be 150 - 10/1.5 = 143.33", 143.33M, result.Rows[0]["BalanceInLocal"]);
				AssertEquals("MatchedInFuturePeriodInLocalCurrency should be 6.67", 6.67M, result.Rows[0]["MatchedInFuturePeriodInLocalCurrency"]);
				AssertEquals("MatchedInFuturePeriodInInvoiceCurrency should be 10", 10.01M, result.Rows[0]["MatchedInFuturePeriodInInvoiceCurrency"]);
				AssertEquals(invoice2.PK, result.Rows[1]["TransactionPK"]);
				AssertEquals("Balance should be 100", 100M, result.Rows[1]["Balance"]);
				AssertEquals("BalanceInLocal should be 100", 100M, result.Rows[1]["BalanceInLocal"]);
				AssertEquals("MatchedInFuturePeriodInLocalCurrency should be 0", 0M, result.Rows[1]["MatchedInFuturePeriodInLocalCurrency"]);
				AssertEquals("MatchedInFuturePeriodInInvoiceCurrency should be 0", 0M, result.Rows[1]["MatchedInFuturePeriodInInvoiceCurrency"]);
			});

			result = RunScriptCore(
				PeriodCalculator.GetPeriodFromDate(ZDateTime.Now),
				LedgerTypes.AccountsPayable,
				false,
				Array.Empty<string>(),
				Array.Empty<string>(),
				false,
				"",
				true,
				true,
				"PER",
				0,
				0,
				0,
				0,
				"Y",
				"Y",
				"",
				"Post Date, Type then Transaction Number",
				excludeMatchedToFutureRECPAY: false,
				futureRECPAYNotInBalance: true);

			CombineAssertions(delegate
			{
				AssertEquals("should contain 3 rows", 3, result.Rows.Count);
				AssertEquals(apDiscount.PK, result.Rows[0]["TransactionPK"]);
				AssertEquals("Balance should be 3.33", 3.33M, result.Rows[0]["Balance"]);
				AssertEquals("BalanceInLocal should be 3.33", 3.33M, result.Rows[0]["BalanceInLocal"]);
				AssertEquals("MatchedInFuturePeriodInLocalCurrency should be 3.33", 3.33M, result.Rows[0]["MatchedInFuturePeriodInLocalCurrency"]);
				AssertEquals("MatchedInFuturePeriodInInvoiceCurrency should be 3.33", 3.33M, result.Rows[0]["MatchedInFuturePeriodInInvoiceCurrency"]);
				AssertEquals(arInvoiceEnabledNewFeature.PK, result.Rows[1]["TransactionPK"]);
				AssertEquals("Balance should be 225", 225M, result.Rows[1]["Balance"]);
				AssertEquals("BalanceInLocal should be 150", 150M, result.Rows[1]["BalanceInLocal"]);
				AssertEquals("MatchedInFuturePeriodInLocalCurrency should be 6.67", 6.67M, result.Rows[1]["MatchedInFuturePeriodInLocalCurrency"]);
				AssertEquals("MatchedInFuturePeriodInInvoiceCurrency should be 10", 10.01M, result.Rows[1]["MatchedInFuturePeriodInInvoiceCurrency"]);
				AssertEquals(invoice2.PK, result.Rows[2]["TransactionPK"]);
				AssertEquals("Balance should be 100", 100M, result.Rows[2]["Balance"]);
				AssertEquals("BalanceInLocal should be 100", 100M, result.Rows[2]["BalanceInLocal"]);
				AssertEquals("MatchedInFuturePeriodInLocalCurrency should be 0", 0M, result.Rows[2]["MatchedInFuturePeriodInLocalCurrency"]);
				AssertEquals("MatchedInFuturePeriodInInvoiceCurrency should be 0", 0M, result.Rows[2]["MatchedInFuturePeriodInInvoiceCurrency"]);
			});
		}

		public void TestDisplayOfINVTransactionsWithCRDDSB()
		{
			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var note1 = creator.CreateInvoiceWithLine(typeof(ARCreditNote), "100", creator.AUD, 1.0M, 0M, 0M, 100M, 0M, creator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.DisbursementInvoice);
			var note2 = creator.CreateInvoiceWithLine(typeof(ARCreditNote), "101", creator.AUD, 1.0M, 0M, 0M, 101M, 0M, creator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.DisbursementInForeignCurrency);

			var arInvoiceEnabledNewFeature = creator.CreateInvoiceWithLine(typeof(ARInvoice), "001", creator.AUD, 1.0M, 102M, 102M, 102M, 102M, creator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.FinalInvoice);
			var invoice2 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "002", creator.AUD, 1.0M, 103M, 103M, 103M, 103M, creator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.FinalInvoice);

			Factory.Save();

			DataTable result = RunScriptCore(
				PeriodCalculator.GetPeriodFromDate(ZDateTime.Now),
				LedgerTypes.AccountsReceivable,
				false,
				Array.Empty<string>(),
				Array.Empty<string>(),
				true,
				"",
				false,
				false,
				"PER",
				0,
				0,
				0,
				0,
				"Y",
				"Y",
				"",
				"");

			CombineAssertions(delegate
			{
				AssertEquals("Only should contain 1 row", 1, result.Rows.Count);
				AssertEquals("All should exist under ABIGAS", "ABIGAS      ", result.Rows[0]["AccountCode"]);
				AssertEquals("Only the 2 credit notes should be showing, -100 - 101 = -201", ((decimal)-201.0000), result.Rows[0]["Balance"]);
			});
		}

		public void TestGroupByOptionsDontThrowError()
		{
			var groupByOptions = new[] {
			"Transaction Type","Consolidation Category", "Accounts Relationship","Organisation Branch","Transaction Branch","Sales Rep","Credit Controller","Customer Service Rep",
			"Settlement Group", "Debtor Group", "Creditor Group" };

			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var note1 = creator.CreateInvoiceWithLine(typeof(ARCreditNote), "100", creator.AUD, 1.0M, 100M, 100M, 100M, 100M, creator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.DisbursementInvoice);
			var note2 = creator.CreateInvoiceWithLine(typeof(ARCreditNote), "101", creator.AUD, 1.0M, 101M, 101M, 101M, 101M, creator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.DisbursementInForeignCurrency);
			var arInvoiceEnabledNewFeature = creator.CreateInvoiceWithLine(typeof(ARInvoice), "001", creator.AUD, 1.0M, 102M, 102M, 102M, 102M, creator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.DisbursementInvoice);
			var invoice2 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "002", creator.AUD, 1.0M, 103M, 103M, 103M, 103M, creator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.DisbursementInForeignCurrency);

			Factory.Save();
			foreach (string groupBy in groupByOptions)
			{
				DataTable result = null;

				AssertNoExceptionThrown(
					string.Format("(Groupby case: '{0}') - No exception running script", groupBy),
					() =>
					{
						result = RunScriptCore(
						   PeriodCalculator.GetPeriodFromDate(ZDateTime.Now),
						   LedgerTypes.AccountsReceivable,
						   true,
						   Array.Empty<string>(),
						   Array.Empty<string>(),
						   true,
						   "",
						   false,
						   false,
						   "PER",
						   0,
						   0,
						   0,
						   0,
						   "Y",
						   "Y",
						   groupBy,
						   "SalesRepName");
					});

				AssertEquals(string.Format("(Groupby case: '{0}') - Only should contain 1 row", groupBy), 1, result.Rows.Count);
			}
		}

		public void TestARCreditLimit_NoSettlementGroup()
		{
			var args = ARCreditLimitSetup();

			TestObjectCreator.LocalClient.ARSettlementGroupPK = ZGuid.Empty;
			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(TestObjectCreator.LocalClient.CompanyData, 50m, 0m);
			Factory.Save();

			args.OverLimitOnly = false;
			var result = RunScriptCore(args);
			AssertEquals("1 row returned by report", 1, result.Rows.Count);
			AssertEquals("Credit limit correct", 50m, result.Rows[0]["CreditLimit"]);

			args.OverLimitOnly = true;
			result = RunScriptCore(args);
			AssertEquals("Row filtered out by report", 0, result.Rows.Count);

			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(TestObjectCreator.LocalClient.CompanyData, 10m, 0m);
			Factory.Save();

			args.OverLimitOnly = false;
			result = RunScriptCore(args);
			AssertEquals("1 row returned by report", 1, result.Rows.Count);
			AssertEquals("Credit limit correct", 10m, result.Rows[0]["CreditLimit"]);

			args.OverLimitOnly = true;
			result = RunScriptCore(args);
			AssertEquals("Row not filtered out by report", 1, result.Rows.Count);
			AssertEquals("Credit limit correct", 10m, result.Rows[0]["CreditLimit"]);

			args.OverLimitOnly = false;
			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(TestObjectCreator.LocalClient.CompanyData, 30m, 10m);
			Factory.Save();
			result = RunScriptCore(args);
			AssertEquals("1 row returned by report", 1, result.Rows.Count);
			AssertEquals("Credit limit correct, taking into account temporary increase", 40m, result.Rows[0]["CreditLimit"]);
		}

		public void TestARCreditLimit_SelfSettlementGroup()
		{
			var args = ARCreditLimitSetup();

			TestObjectCreator.LocalClient.ARSettlementGroupPK = TestObjectCreator.LocalClient.PK;
			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(TestObjectCreator.LocalClient.CompanyData, 50m, 0m);
			Factory.Save();

			args.OverLimitOnly = false;
			var result = RunScriptCore(args);
			AssertEquals("1 row returned by report", 1, result.Rows.Count);
			AssertEquals("Credit limit correct", 50m, result.Rows[0]["CreditLimit"]);

			args.OverLimitOnly = true;
			result = RunScriptCore(args);
			AssertEquals("Row filtered out by report", 0, result.Rows.Count);

			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(TestObjectCreator.LocalClient.CompanyData, 10m, 0m);
			Factory.Save();

			args.OverLimitOnly = false;
			result = RunScriptCore(args);
			AssertEquals("1 row returned by report", 1, result.Rows.Count);
			AssertEquals("Credit limit correct", 10m, result.Rows[0]["CreditLimit"]);

			args.OverLimitOnly = true;
			result = RunScriptCore(args);
			AssertEquals("Row not filtered out by report", 1, result.Rows.Count);
			AssertEquals("Credit limit correct", 10m, result.Rows[0]["CreditLimit"]);

			args.OverLimitOnly = false;
			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(TestObjectCreator.LocalClient.CompanyData, 30m, 10m);
			Factory.Save();
			result = RunScriptCore(args);
			AssertEquals("1 row returned by report", 1, result.Rows.Count);
			AssertEquals("Credit limit correct, taking into account temporary increase", 40m, result.Rows[0]["CreditLimit"]);
		}

		public void TestARCreditLimit_SettlementGroup()
		{
			var args = ARCreditLimitSetup();

			TestObjectCreator.LocalClient.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.LocalClient.CompanyData.OB_ARUseSettlementGroupCreditLimit = false;
			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(TestObjectCreator.LocalClient.CompanyData, 50m, 0m);

			// Code to ensure that related party IS_VALID flag is true
			var relatedPartyQuery = new ZQuery(OrgRelatedPartySchema.PR_OH_Parent, new[] { TestObjectCreator.LocalClient.PK, TestObjectCreator.LocalClient2.PK });
			relatedPartyQuery.AddToFilter(OrgRelatedPartySchema.PR_PartyType, "ARS");
			relatedPartyQuery.AddToFilter(OrgRelatedPartySchema.PR_GC, GlbCompany.CurrentCompany.PK);
			var relatedParties = Factory.Load<OrgRelatedParty>(relatedPartyQuery);
			((ILightValidationInternals)relatedParties[0]).IsValid = true;
			((ILightValidationInternals)relatedParties[1]).IsValid = true;

			Factory.Save();

			args.OverLimitOnly = false;
			var result = RunScriptCore(args);
			AssertEquals("1 row returned by report", 1, result.Rows.Count);
			AssertEquals("Settlement Group Credit limit correct", 50m, result.Rows[0]["SettlementGroupCreditLimit"]);

			args.OverLimitOnly = true;
			result = RunScriptCore(args);
			AssertEquals("Row filtered out by report", 0, result.Rows.Count);

			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(TestObjectCreator.LocalClient.CompanyData, 10m, 0m);
			Factory.Save();

			args.OverLimitOnly = false;
			result = RunScriptCore(args);
			AssertEquals("1 row returned by report", 1, result.Rows.Count);
			AssertEquals("Settlement Group Credit limit correct", 10m, result.Rows[0]["SettlementGroupCreditLimit"]);

			args.OverLimitOnly = true;
			result = RunScriptCore(args);
			AssertEquals("Row not filtered out by report", 1, result.Rows.Count);
			AssertEquals("Settlement Group Credit limit correct", 10m, result.Rows[0]["SettlementGroupCreditLimit"]);

			TestObjectCreator.LocalClient.CompanyData.OB_ARUseSettlementGroupCreditLimit = true;
			AssertEquals(0M, TestObjectCreator.LocalClient.CompanyData.OB_ARCreditLimit);
			Factory.Save();

			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(TestObjectCreator.ABIGAS.CompanyData, 100m, 0m);
			Factory.Save();

			args.OverLimitOnly = false;
			result = RunScriptCore(args);
			AssertEquals("1 row returned by report", 1, result.Rows.Count);
			AssertEquals("Settlement Group Credit limit correct", 100m, result.Rows[0]["SettlementGroupCreditLimit"]);

			args.OverLimitOnly = true;
			result = RunScriptCore(args);
			AssertEquals("Row filtered out by report", 0, result.Rows.Count);

			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(TestObjectCreator.ABIGAS.CompanyData, 10m, 0m);
			Factory.Save();

			args.OverLimitOnly = false;
			result = RunScriptCore(args);
			AssertEquals("1 row returned by report", 1, result.Rows.Count);
			AssertEquals("Settlement Group Credit limit correct", 10m, result.Rows[0]["SettlementGroupCreditLimit"]);

			args.OverLimitOnly = true;
			result = RunScriptCore(args);
			AssertEquals("Row not filtered out by report", 1, result.Rows.Count);
			AssertEquals("Settlement Group Credit limit correct", 10m, result.Rows[0]["SettlementGroupCreditLimit"]);
		}

		public void TestDisplayOfINVTransactionsWithNotInActiveBatchTranOnly()
		{
			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			var creator = new TestObjectCreator(Factory);
			var arInvoiceEnabledNewFeature = creator.CreateInvoiceWithLine(typeof(ARInvoice), "001", creator.AUD, 1.0M, 102M, 102M, 102M, 102M, creator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.DisbursementInvoice);
			var invoice2 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "002", creator.AUD, 1.0M, 103M, 103M, 103M, 103M, creator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.DisbursementInForeignCurrency);

			var batch = Factory.NewWithValidTestData<AccCollectionBatch>();
			batch.ACB_GC = GlbCompany.CurrentCompany.PK;
			batch.IsCancelled = false;
			batch.ACB_BatchNumber = ZString.Empty;
			batch.ACB_TotalAmount = 100m;
			var order = Factory.New<AccCollectionOrder>();
			order.ACO_ACB = batch.PK;
			order.ACO_CollectionDate = ZDateTime.Today.Date;
			order.ACO_IsCancelled = false;
			order.ACO_OH_Debtor = creator.ABIGAS.PK;
			order.ACO_OrderNumber = ZString.Empty;
			order.ACO_Amount = 50m;

			var line1 = Factory.New<AccCollectionOrderLine>();
			line1.AOL_ACO = order.PK;
			line1.AOL_AH = arInvoiceEnabledNewFeature.PK;
			line1.IsCancelled = false;
			var line2 = Factory.New<AccCollectionOrderLine>();
			line2.AOL_ACO = order.PK;
			line2.AOL_AH = invoice2.PK;
			line2.IsCancelled = false;

			Factory.Save();

			//notInActiveBatchTranOnly is Y
			var result = RunScriptCore(
				PeriodCalculator.GetPeriodFromDate(ZDateTime.Now),
				LedgerTypes.AccountsReceivable,
				false,
				Array.Empty<string>(),
				Array.Empty<string>(),
				true,
				"",
				false,
				false,
				"PER",
				0,
				0,
				0,
				0,
				"Y",
				"Y",
				"",
				"",
				notInActiveBatchTranOnly: "Y");

			AssertEquals("Should contain 0 row", 0, result.Rows.Count);

			line1.IsCancelled = true;
			Factory.Save();

			result = RunScriptCore(
							PeriodCalculator.GetPeriodFromDate(ZDateTime.Now),
							LedgerTypes.AccountsReceivable,
							false,
							Array.Empty<string>(),
							Array.Empty<string>(),
							true,
							"",
							false,
							false,
							"PER",
							0,
							0,
							0,
							0,
							"Y",
							"Y",
							"",
							"",
							notInActiveBatchTranOnly: "Y");
			AssertEquals("Should contain 1 row", 1, result.Rows.Count);
			AssertEquals("All should exist under ABIGAS", "ABIGAS      ", result.Rows[0]["AccountCode"]);
			AssertEquals("Only 1 invoice should be showing, 102", (decimal)102.0000, result.Rows[0]["Balance"]);

			//notInActiveBatchTranOnly is empty/N
			result = RunScriptCore(
				PeriodCalculator.GetPeriodFromDate(ZDateTime.Now),
				LedgerTypes.AccountsReceivable,
				false,
				Array.Empty<string>(),
				Array.Empty<string>(),
				true,
				"",
				false,
				false,
				"PER",
				0,
				0,
				0,
				0,
				"Y",
				"Y",
				"",
				"",
				notInActiveBatchTranOnly: "");
			AssertEquals("Should contain 1 row", 1, result.Rows.Count);
			AssertEquals("All should exist under ABIGAS", "ABIGAS      ", result.Rows[0]["AccountCode"]);
			AssertEquals("2 invoices should be showing, 102 + 103 = 205", (decimal)205.0000, result.Rows[0]["Balance"]);
		}

		public void TestARCountryList()
		{
			var orgHeader1 = TestObjectCreator.CreateOrgHeader("TSTORG1", false, true, "USCHI");
			var arInvoiceEnabledNewFeature = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "Inv001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
			arInvoiceEnabledNewFeature.AH_OH = orgHeader1.PK;

			var orgHeader2 = TestObjectCreator.CreateOrgHeader("TSTORG2", false, true, "AUSYD");
			var invoice2 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "Inv002", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
			invoice2.AH_OH = orgHeader2.PK;

			Factory.Save();

			DataTable resultForUS = RunScriptForCountryList(
				LedgerTypes.AccountsReceivable,
				false,
				new string[] { orgHeader1.CountryCode },
				Array.Empty<string>(),
				false);

			AssertEquals("Result for Country list", 1, resultForUS.Rows.Count);
			AssertEquals("Country should be ", orgHeader1.CountryCode, resultForUS.Rows[0]["CountryCode"]);

			DataTable resultForAU = RunScriptForCountryList(
				LedgerTypes.AccountsReceivable,
				false,
				new string[] { orgHeader2.CountryCode },
				Array.Empty<string>(),
				false);

			AssertEquals("Result for Country list", 1, resultForAU.Rows.Count);
			AssertEquals("Country should be ", orgHeader2.CountryCode, resultForAU.Rows[0]["CountryCode"]);

			DataTable resultForExUS = RunScriptForCountryList(
				LedgerTypes.AccountsReceivable,
				false,
				Array.Empty<string>(),
				new string[] { orgHeader1.CountryCode },
				false);

			AssertEquals("Result for Country list", 1, resultForAU.Rows.Count);
			AssertEquals("Country should be ", orgHeader2.CountryCode, resultForExUS.Rows[0]["CountryCode"]);

			DataTable resultForExAU = RunScriptForCountryList(
				LedgerTypes.AccountsReceivable,
				false,
				Array.Empty<string>(),
				new string[] { orgHeader2.CountryCode },
				false);

			AssertEquals("Result for Country list", 1, resultForAU.Rows.Count);
			AssertEquals("Country should be ", orgHeader1.CountryCode, resultForExAU.Rows[0]["CountryCode"]);
		}

		public void TestAPCountryList()
		{
			var orgHeader1 = TestObjectCreator.CreateOrgHeader("TSTORG1", false, true, "USCHI");
			var arInvoiceEnabledNewFeature = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "Inv001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
			arInvoiceEnabledNewFeature.AH_OH = orgHeader1.PK;

			var orgHeader2 = TestObjectCreator.CreateOrgHeader("TSTORG2", false, true, "AUSYD");
			var invoice2 = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "Inv002", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
			invoice2.AH_OH = orgHeader2.PK;

			Factory.Save();

			DataTable resultForUS = RunScriptForCountryList(
				LedgerTypes.AccountsPayable,
				false,
				new string[] { orgHeader1.CountryCode },
				Array.Empty<string>(),
				false);

			AssertEquals("Result for Country list", 1, resultForUS.Rows.Count);
			AssertEquals("Country should be ", orgHeader1.CountryCode, resultForUS.Rows[0]["CountryCode"]);

			DataTable resultForAU = RunScriptForCountryList(
				LedgerTypes.AccountsPayable,
				false,
				new string[] { orgHeader2.CountryCode },
				Array.Empty<string>(),
				false);

			AssertEquals("Result for Country list", 1, resultForAU.Rows.Count);
			AssertEquals("Country should be ", orgHeader2.CountryCode, resultForAU.Rows[0]["CountryCode"]);

			DataTable resultForExUS = RunScriptForCountryList(
				LedgerTypes.AccountsPayable,
				false,
				Array.Empty<string>(),
				new string[] { orgHeader1.CountryCode },
				false);

			AssertEquals("Result for Country list", 1, resultForAU.Rows.Count);
			AssertEquals("Country should be ", orgHeader2.CountryCode, resultForExUS.Rows[0]["CountryCode"]);

			DataTable resultForExAU = RunScriptForCountryList(
				LedgerTypes.AccountsPayable,
				false,
				Array.Empty<string>(),
				new string[] { orgHeader2.CountryCode },
				false);

			AssertEquals("Result for Country list", 1, resultForAU.Rows.Count);
			AssertEquals("Country should be ", orgHeader1.CountryCode, resultForExAU.Rows[0]["CountryCode"]);
		}

		[TestDate(2012, 12, 15)]
		public void TestARCreditLimit_NoSettlementGroup_OutstandingAmount()
		{
			var args = ARCreditLimitSetup();
			args.Period = PeriodCalculator.GetPeriodFromDate(new ZDateTime(2012, 12, 15));
			args.ReportDate = new DateTime(2012, 12, 15);
			args.OrgList = new string[] { TestObjectCreator.LocalClient.OH_Code, TestObjectCreator.LocalClient2.OH_Code };

			TestObjectCreator.LocalClient.ARSettlementGroupPK = ZGuid.Empty;
			TestObjectCreator.LocalClient.CompanyData.OB_ARCreditLimit = 15M;
			TestObjectCreator.LocalClient2.ARSettlementGroupPK = ZGuid.Empty;
			TestObjectCreator.LocalClient2.CompanyData.OB_ARCreditLimit = 15M;
			Factory.Save();

			args.OverLimitOnly = true;
			var result = RunScriptCore(args);
			AssertEquals("Row filtered out by report", 2, result.Rows.Count);

			var localClientRecord = result.Select("AccountCode = 'ZLOCCLT'");
			AssertEquals("Credit limit", 15M, localClientRecord[0]["CreditLimit"]);
			AssertEquals("Outstanding balance", 20M, localClientRecord[0]["Balance"]);

			var localClient2Record = result.Select("AccountCode = 'ZLOCCLT2'");
			AssertEquals("Credit limit", 15M, localClient2Record[0]["CreditLimit"]);
			AssertEquals("Outstanding balance", 30M, localClient2Record[0]["Balance"]);

			string updateString = "UPDATE dbo.AccTransactionHeader SET AH_OutstandingAmount = '{0}', AH_FullyPaidDate = '{1}', AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST' WHERE AH_PK = '{2}'";

			TestConnection.ExecuteNonQuery(string.Format(updateString, 10M, "2012-12-13", arInvoiceEnabledNewFeature.PK));

			args.OverLimitOnly = true;
			result = RunScriptCore(args);
			AssertEquals("Row filtered out by report", 1, result.Rows.Count);
			AssertEquals("Account code", "ZLOCCLT2", result.Rows[0]["AccountCode"]);
			AssertEquals("Credit limit", 15M, result.Rows[0]["CreditLimit"]);
			AssertEquals("Outstanding balance", 30M, result.Rows[0]["Balance"]);

			TestObjectCreator.CreateAndMatchARReceiptForARInvoice(invoice2 as ARInvoice, DateTime.Today);
			Factory.Save();

			args.OverLimitOnly = true;
			result = RunScriptCore(args);
			AssertEquals("Row filtered out by report", 0, result.Rows.Count);

			args.OverLimitOnly = false;
			args.ShowAll = true;
			result = RunScriptCore(args);
			AssertEquals("2 row returned by report", 2, result.Rows.Count);

			localClientRecord = result.Select("AccountCode = 'ZLOCCLT'");
			AssertEquals("Credit limit", 15M, localClientRecord[0]["CreditLimit"]);
			AssertEquals("Outstanding balance", 10M, localClientRecord[0]["Balance"]);

			localClient2Record = result.Select("AccountCode = 'ZLOCCLT2'");
			AssertEquals("Credit limit", 15M, localClient2Record[0]["CreditLimit"]);
			AssertEquals("Outstanding balance", 0M, localClient2Record[0]["Balance"]);
		}

		InvoicingBase arInvoiceEnabledNewFeature;
		InvoicingBase invoice2;
		SPArgs ARCreditLimitSetup()
		{
			// Code to ensure that companydata IS_VALID flags are true
			((ILightValidationInternals)TestObjectCreator.LocalClient.CompanyData).IsValid = true;
			((ILightValidationInternals)TestObjectCreator.LocalClient2.CompanyData).IsValid = true;
			((ILightValidationInternals)TestObjectCreator.ABIGAS.CompanyData).IsValid = true;

			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator.LocalClient2.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			arInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.LocalClient, glAccount.PK);
			invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, TestObjectCreator.LocalClient2, glAccount.PK);
			Factory.Save();

			SPArgs args = SPArgs.Default(PeriodCalculator);
			args.Ledger = LedgerTypes.AccountsReceivable;
			args.ShowAll = false;
			args.OrgList = new string[] { TestObjectCreator.LocalClient.OH_Code };
			args.SettlementGroupList = Array.Empty<string>();
			args.SummaryOnly = false;
			return args;
		}

		public void TestPaymentStatus()
		{
			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			var arInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.ABIGAS, glAccount.PK);
			var invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV2", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.LocalClient, glAccount.PK);
			var invoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV3", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, TestObjectCreator.LocalClient2, glAccount.PK);

			var payment1 = TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(arInvoiceEnabledNewFeature, partPaidAmount: -5m);
			var payment2 = TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(invoice2);
			Factory.Save();

			var result = RunScriptCoreForPaymentStatus("NON", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			AssertEquals("3 rows returned by report", 3, result.Rows.Count);

			result = RunScriptCoreForPaymentStatus("FUL", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			AssertEquals("1 row returned by report", 1, result.Rows.Count);
			AssertEquals("InvoiceRef", "INV2", result.Rows[0]["InvoiceRef"]);
			AssertEquals("Balance", 0M, result.Rows[0]["Balance"]);

			result = RunScriptCoreForPaymentStatus("UNP", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			AssertEquals("1 row returned by report", 1, result.Rows.Count);
			AssertEquals("InvoiceRef", "INV3", result.Rows[0]["InvoiceRef"]);
			AssertEquals("Balance", 30M, result.Rows[0]["Balance"]);

			result = RunScriptCoreForPaymentStatus("PAR", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			AssertEquals("1 row returned by report", 1, result.Rows.Count);
			AssertEquals("InvoiceRef", "INV1", result.Rows[0]["InvoiceRef"]);
			AssertEquals("Balance", 5M, result.Rows[0]["Balance"]);
		}

		public void TestPaymentStatusWithZeroValueInvoice()
		{
			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.ABIGAS, glAccount.PK);
			var line1 = invoice.Lines[0];
			var line2 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1, -10m, 0, 0, glAccount.PK);
			Factory.Save();

			AssertEquals(0m, invoice.AH_LocalTotal);
			AssertEquals(true, invoice.AH_FullyPaidDate.IsValid);

			var result = RunScriptCoreForPaymentStatus("NON", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "AR");
			AssertEquals("1 row returned by report", 1, result.Rows.Count);
			AssertEquals("InvoiceRef", invoice.AH_TransactionNum, result.Rows[0]["InvoiceRef"]);
			AssertEquals("InvoiceTotal", 0M, result.Rows[0]["InvoiceTotal"]);
			AssertEquals("Balance", 0M, result.Rows[0]["Balance"]);

			result = RunScriptCoreForPaymentStatus("FUL", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "AR");
			AssertEquals("1 row returned by report", 1, result.Rows.Count);
			AssertEquals("InvoiceRef", invoice.AH_TransactionNum, result.Rows[0]["InvoiceRef"]);
			AssertEquals("InvoiceTotal", 0M, result.Rows[0]["InvoiceTotal"]);
			AssertEquals("Balance", 0M, result.Rows[0]["Balance"]);

			result = RunScriptCoreForPaymentStatus("UNP", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "AR");
			AssertEquals("0 row returned by report", 0, result.Rows.Count);

			result = RunScriptCoreForPaymentStatus("PAR", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "AR");
			AssertEquals("0 row returned by report", 0, result.Rows.Count);
		}
		public void TestBranchManagementCodeAndBranchList()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "TBS";
			branch.GB_GC = GlbCompany.CurrentCompany.PK;

			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			var arInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.ABIGAS, glAccount.PK);
			arInvoiceEnabledNewFeature.AH_GB = branch.PK;
			var invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.LocalClient, glAccount.PK);
			var invoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, TestObjectCreator.LocalClient2, glAccount.PK);

			var payment1 = TestObjectCreator.CreateAndMatchARReceiptForARInvoice(arInvoiceEnabledNewFeature, partPaidAmount: 5m);
			var payment2 = TestObjectCreator.CreateAndMatchARReceiptForARInvoice(invoice2);
			Factory.Save();

			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.GlbBranch SET GB_AccountingGroupCode = 'BRE', GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_Code = '{0}' And GB_GC = '{1}'", GlbBranch.CurrentBranch.GB_Code, GlbCompany.CurrentCompany.PK));
			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.GlbBranch SET GB_AccountingGroupCode = 'BRT', GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_Code = 'TBS' And GB_GC = '{0}'", GlbCompany.CurrentCompany.PK));

			var result = RunScriptCoreForBranchManagementCode("BRT");
			AssertEquals("1 rows returned by report", 1, result.Rows.Count);
			AssertEquals("Should found the branch TBS", "TBS", result.Rows[0]["BranchCode"]);

			result = RunScriptCoreForBranchManagementCode("BRE");
			AssertEquals("2 rows returned by report", 2, result.Rows.Count);
			AssertEquals("Should found the current branch", GlbBranch.CurrentBranch.GB_Code, result.Rows[0]["BranchCode"]);
			AssertEquals("Should found the current branch", GlbBranch.CurrentBranch.GB_Code, result.Rows[1]["BranchCode"]);

			result = RunScriptCoreForBranchManagementCode("BRE", GlbBranch.CurrentBranch.GB_Code);
			AssertEquals("2 rows returned by report as the current branch has branch management code 'BRE'", 2, result.Rows.Count);
			AssertEquals("Should found the current branch", GlbBranch.CurrentBranch.GB_Code, result.Rows[0]["BranchCode"]);
			AssertEquals("Should found the current branch", GlbBranch.CurrentBranch.GB_Code, result.Rows[1]["BranchCode"]);

			var branchList = string.Format("{0}, {1}", GlbBranch.CurrentBranch.GB_Code, "TBS");
			result = RunScriptCoreForBranchManagementCode("BRE", branchList);
			AssertEquals("2 rows returned by report. The records for branch 'TBS' is not returned as it's branch management code does not match with 'BRE'", 2, result.Rows.Count);
			AssertEquals("Should found the current branch", GlbBranch.CurrentBranch.GB_Code, result.Rows[0]["BranchCode"]);
			AssertEquals("Should found the current branch", GlbBranch.CurrentBranch.GB_Code, result.Rows[1]["BranchCode"]);

			result = RunScriptCoreForBranchManagementCode("BRE", "TBS");
			AssertEquals("No rows returned by report as branch code 'TBS' is not having the branch management code 'BRE'", 0, result.Rows.Count);
		}

		[TestDate(2016, 5, 1)]
		public void TestPartlyMatchedARTransactions()
		{
			var period = Factory.Load<AccPeriodManagement>(new ZQuery());
			Array.ForEach(period, (x) => x.Delete());
			Factory.Save();
			AssertEquals(0, Factory.Load<AccPeriodManagement>(new ZQuery()).Length);

			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupSinglePeriod(201601, new ZDateTime(2016, 1, 1), (new ZDateTime(2016, 2, 1)).AddSeconds(-1));
			periodHelper.SetupSinglePeriod(201602, new ZDateTime(2016, 2, 1), (new ZDateTime(2016, 3, 1)).AddSeconds(-1));
			periodHelper.SetupSinglePeriod(201603, new ZDateTime(2016, 3, 1), (new ZDateTime(2016, 4, 1)).AddSeconds(-1));
			periodHelper.SetupSinglePeriod(201604, new ZDateTime(2016, 4, 1), (new ZDateTime(2016, 5, 1)).AddSeconds(-1));
			periodHelper.SetupSinglePeriod(201605, new ZDateTime(2016, 5, 1), (new ZDateTime(2016, 6, 1)).AddSeconds(-1));
			periodHelper.SetupSinglePeriod(201606, new ZDateTime(2016, 6, 1), (new ZDateTime(2016, 7, 1)).AddSeconds(-1));
			periodHelper.SetupSinglePeriod(201607, new ZDateTime(2016, 7, 1), (new ZDateTime(2016, 8, 1)).AddSeconds(-1));
			Factory.Save();

			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			var arInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M,
				TestObjectCreator.ABIGAS,
				TestObjectCreator.CC1.PK,
				new ZDateTime(2016, 1, 1), new ZDateTime(2016, 1, 30), new ZDateTime(2016, 1, 1), false);
			Factory.Save();
			Assert(arInvoiceEnabledNewFeature.AH_FullyPaidDate.IsEmpty);
			AssertEquals(10m, arInvoiceEnabledNewFeature.AH_InvoiceAmount);
			AssertEquals(10m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);

			var payment1 = TestObjectCreator.CreateAndMatchARReceiptForARInvoice(arInvoiceEnabledNewFeature, new ZDateTime(2016, 1, 10), 2, "M01");
			Factory.Save();
			Assert(arInvoiceEnabledNewFeature.AH_FullyPaidDate.IsEmpty);
			AssertEquals(10m, arInvoiceEnabledNewFeature.AH_InvoiceAmount);
			AssertEquals(8m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
			var resultTable = RunScript("AR", false, Array.Empty<string>(), Array.Empty<string>(), true);

			AssertEquals(1, resultTable.Rows.Count);
			AssertEquals(8M, resultTable.Rows[0]["Balance"]);
			AssertEquals(8M, resultTable.Rows[0]["BalanceInLocal"]);

			var payment2 = TestObjectCreator.CreateAndMatchARReceiptForARInvoice(arInvoiceEnabledNewFeature, new ZDateTime(2016, 2, 10), 2, "M02");
			Factory.Save();
			Assert(arInvoiceEnabledNewFeature.AH_FullyPaidDate.IsEmpty);
			AssertEquals(10m, arInvoiceEnabledNewFeature.AH_InvoiceAmount);
			AssertEquals(6m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
			resultTable = RunScript("AR", false, Array.Empty<string>(), Array.Empty<string>(), true);
			AssertEquals(1, resultTable.Rows.Count);
			AssertEquals(6M, resultTable.Rows[0]["Balance"]);
			AssertEquals(6M, resultTable.Rows[0]["BalanceInLocal"]);

			var payment3 = TestObjectCreator.CreateAndMatchARReceiptForARInvoice(arInvoiceEnabledNewFeature, new ZDateTime(2016, 6, 10), 2, "M03");
			Factory.Save();
			Assert(arInvoiceEnabledNewFeature.AH_FullyPaidDate.IsEmpty);
			AssertEquals(10m, arInvoiceEnabledNewFeature.AH_InvoiceAmount);
			AssertEquals(4m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
			resultTable = RunScript("AR", false, Array.Empty<string>(), Array.Empty<string>(), true);

			AssertEquals(1, resultTable.Rows.Count);
			AssertEquals(4M, resultTable.Rows[0]["Balance"]);
			AssertEquals(4M, resultTable.Rows[0]["BalanceInLocal"]);

			var payment4 = TestObjectCreator.CreateAndMatchARReceiptForARInvoice(arInvoiceEnabledNewFeature, new ZDateTime(2016, 7, 10), null, "M04");
			Factory.Save();
			Assert(!arInvoiceEnabledNewFeature.AH_FullyPaidDate.IsEmpty);
			AssertEquals(10m, arInvoiceEnabledNewFeature.AH_InvoiceAmount);
			AssertEquals(0m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
			resultTable = RunScript("AR", false, Array.Empty<string>(), Array.Empty<string>(), true);
			AssertEquals(1, resultTable.Rows.Count);
			AssertEquals(0M, resultTable.Rows[0]["Balance"]);
			AssertEquals(0M, resultTable.Rows[0]["BalanceInLocal"]);
		}

		[TestDate(2016, 5, 1)]
		public void TestPartlyMatchedAPTransactions()
		{
			var period = Factory.Load<AccPeriodManagement>(new ZQuery());
			Array.ForEach(period, (x) => x.Delete());
			Factory.Save();
			AssertEquals(0, Factory.Load<AccPeriodManagement>(new ZQuery()).Length);

			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupSinglePeriod(201601, new ZDateTime(2016, 1, 1), (new ZDateTime(2016, 2, 1)).AddSeconds(-1));
			periodHelper.SetupSinglePeriod(201602, new ZDateTime(2016, 2, 1), (new ZDateTime(2016, 3, 1)).AddSeconds(-1));
			periodHelper.SetupSinglePeriod(201603, new ZDateTime(2016, 3, 1), (new ZDateTime(2016, 4, 1)).AddSeconds(-1));
			periodHelper.SetupSinglePeriod(201604, new ZDateTime(2016, 4, 1), (new ZDateTime(2016, 5, 1)).AddSeconds(-1));
			periodHelper.SetupSinglePeriod(201605, new ZDateTime(2016, 5, 1), (new ZDateTime(2016, 6, 1)).AddSeconds(-1));
			periodHelper.SetupSinglePeriod(201606, new ZDateTime(2016, 6, 1), (new ZDateTime(2016, 7, 1)).AddSeconds(-1));
			periodHelper.SetupSinglePeriod(201607, new ZDateTime(2016, 7, 1), (new ZDateTime(2016, 8, 1)).AddSeconds(-1));
			Factory.Save();

			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			var arInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001001", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M,
				TestObjectCreator.ABIGAS,
				glAccount.PK,
				new ZDateTime(2016, 1, 1), new ZDateTime(2016, 1, 30), new ZDateTime(2016, 1, 1), true);
			Factory.Save();
			Assert(arInvoiceEnabledNewFeature.AH_FullyPaidDate.IsEmpty);
			AssertEquals(-10m, arInvoiceEnabledNewFeature.AH_InvoiceAmount);
			AssertEquals(-10m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);

			var payment1 = TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(arInvoiceEnabledNewFeature, new ZDateTime(2016, 1, 10), -2, "M01");
			Factory.Save();
			Assert(arInvoiceEnabledNewFeature.AH_FullyPaidDate.IsEmpty);
			AssertEquals(-10m, arInvoiceEnabledNewFeature.AH_InvoiceAmount);
			AssertEquals(-8m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
			var resultTable = RunScript("AP", false, Array.Empty<string>(), Array.Empty<string>(), true);

			AssertEquals(1, resultTable.Rows.Count);
			AssertEquals(8M, resultTable.Rows[0]["Balance"]);
			AssertEquals(8M, resultTable.Rows[0]["BalanceInLocal"]);

			var payment2 = TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(arInvoiceEnabledNewFeature, new ZDateTime(2016, 2, 10), -2, "M02");
			Factory.Save();
			Assert(arInvoiceEnabledNewFeature.AH_FullyPaidDate.IsEmpty);
			AssertEquals(-10m, arInvoiceEnabledNewFeature.AH_InvoiceAmount);
			AssertEquals(-6m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
			resultTable = RunScript("AP", false, Array.Empty<string>(), Array.Empty<string>(), true);
			AssertEquals(1, resultTable.Rows.Count);
			AssertEquals(6M, resultTable.Rows[0]["Balance"]);
			AssertEquals(6M, resultTable.Rows[0]["BalanceInLocal"]);

			var payment3 = TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(arInvoiceEnabledNewFeature, new ZDateTime(2016, 6, 10), -2, "M03");
			Factory.Save();
			Assert(arInvoiceEnabledNewFeature.AH_FullyPaidDate.IsEmpty);
			AssertEquals(-10m, arInvoiceEnabledNewFeature.AH_InvoiceAmount);
			AssertEquals(-4m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
			resultTable = RunScript("AP", false, Array.Empty<string>(), Array.Empty<string>(), true);

			AssertEquals(1, resultTable.Rows.Count);
			AssertEquals(4M, resultTable.Rows[0]["Balance"]);
			AssertEquals(4M, resultTable.Rows[0]["BalanceInLocal"]);

			var payment4 = TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(arInvoiceEnabledNewFeature, new ZDateTime(2016, 7, 10), null, "M04");
			Factory.Save();
			Assert(!arInvoiceEnabledNewFeature.AH_FullyPaidDate.IsEmpty);
			AssertEquals(-10m, arInvoiceEnabledNewFeature.AH_InvoiceAmount);
			AssertEquals(0m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
			resultTable = RunScript("AP", false, Array.Empty<string>(), Array.Empty<string>(), true);
			AssertEquals(1, resultTable.Rows.Count);
			AssertEquals(0M, resultTable.Rows[0]["Balance"]);
			AssertEquals(0M, resultTable.Rows[0]["BalanceInLocal"]);
		}

		public void TestARInvoiceTerm()
		{
			Action<string> assertArInvTerm = (invTerm) =>
			{
				Factory.Save();
				var table = RunScript("AR", false, Array.Empty<string>(), Array.Empty<string>(), false);
				AssertEquals(1, table.Rows.Count);
				AssertEquals(invTerm, table.Rows[0]["InvoiceTerm"].ToString().Trim());
			};

			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			TestObjectCreator.ABIGAS.CompanyData.ARTerms.DeleteAll();
			TestObjectCreator.AALSHI.CompanyData.ARTerms.DeleteAll();

			TestObjectCreator.AALSHI.ARSettlementGroupPK = ZGuid.Empty;
			TestObjectCreator.ABIGAS.ARSettlementGroupPK = TestObjectCreator.AALSHI.PK;

			Factory.Save();

			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.ABIGAS, glAccount.PK);
			Factory.Save();

			var org = TestObjectCreator.ABIGAS;
			var companyData = org.CompanyData;

			var orgSettleGroup = TestObjectCreator.AALSHI;
			var companyDataSettleGroup = orgSettleGroup.CompanyData;

			companyData.OB_ARCreditApproved = false;
			companyData.OB_AROnCreditHold = false;
			assertArInvTerm("COD");

			companyData.OB_ARCreditApproved = true;
			companyData.OB_AROnCreditHold = true;
			assertArInvTerm("COD");

			companyData.OB_ARCreditApproved = true;
			companyData.OB_AROnCreditHold = false;
			assertArInvTerm("0 Days");

			var arTerm1 = companyData.ARTerms.AddNew();
			SetupTermsInfo(arTerm1, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL", "PIA", 1);
			assertArInvTerm("PIA");

			SetupTermsInfo(arTerm1, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL", "COD", 1);
			assertArInvTerm("COD");

			SetupTermsInfo(arTerm1, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL", "MIC", 1);
			assertArInvTerm("1 Months MIC");

			SetupTermsInfo(arTerm1, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL", "X01", 1);
			assertArInvTerm("1 Days X01");

			SetupTermsInfo(arTerm1, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL", "DEF", 1);
			assertArInvTerm("0 Days");

			//SettleGroup
			var arTermSettleGroup = companyDataSettleGroup.ARTerms.AddNew();
			SetupTermsInfo(arTermSettleGroup, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL", "COD", 2);
			assertArInvTerm("COD");

			SetupTermsInfo(arTermSettleGroup, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL", "PIA", 2);
			assertArInvTerm("PIA");

			SetupTermsInfo(arTermSettleGroup, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL", "MIC", 2);
			assertArInvTerm("2 Months MIC");

			SetupTermsInfo(arTermSettleGroup, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL", "X02", 2);
			assertArInvTerm("2 Days X02");
		}

		public void TestAdditionalCompanyName()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				GlbCompany.CurrentCompany.OrgProxy.OH_Language = SharedConstants.Languages.ChineseSimplified;
				GlbCompany.CurrentCompany.Factory.Save();

				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

				AssertAdditionalCompanyName(OrgAddressType.Receivables, LedgerTypes.AccountsReceivable, typeof(ARInvoice));
				AssertAdditionalCompanyName(OrgAddressType.Payables, LedgerTypes.AccountsPayable, typeof(APInvoice));
			}
		}

		void AssertAdditionalCompanyName(string addressType, string ledgerType, Type invoiceType)
		{
			var expectedAdditionalCompanyName = "人生得意须尽欢";
			var chineseAddress = "莫使金樽空对月";

			var testObjectCreator = new TestObjectCreator(Factory);
			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();

			// Main Address
			var orgHeader = testObjectCreator.CreateOrgHeader("ABC" + addressType, true, true);
			orgHeader.Addresses.RemoveAndDeleteAll();

			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_RN_NKCountryCode = Constants.CountryCodes.China;
			orgAddress.Language = SharedConstants.Languages.ChineseSimplified;
			orgAddress.Address1 = chineseAddress;
			orgAddress.CompanyName = expectedAdditionalCompanyName;
			orgAddress.AddressCapability.SetCapabilityEnabled(addressType);
			orgAddress.AddressCapability.SetIsMainAddress(addressType);

			Factory.Save();

			TestObjectCreator.CreateInvoiceWithLine(invoiceType, "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, orgHeader, glAccount.PK);

			Factory.Save();

			DataTable resultForOrg = RunScript(
				ledgerType,
				false,
				new string[] { orgHeader.OH_Code },
				Array.Empty<string>(),
				false);

			AssertEquals("Result count should be 1.", 1, resultForOrg.Rows.Count);
			AssertEquals("The value of AdditionalCompanyName should be expectedAdditionalCompanyName", expectedAdditionalCompanyName, resultForOrg.Rows[0]["AdditionalCompanyName"]);

			// Main Translated Address
			var orgHeader1 = testObjectCreator.CreateOrgHeader("DEF" + addressType, true, true);
			orgHeader1.Addresses.RemoveAndDeleteAll();

			var orgAddress1 = orgHeader1.Addresses.AddNew();
			orgAddress1.OA_RN_NKCountryCode = Constants.CountryCodes.China;
			orgAddress1.Language = SharedConstants.Languages.English;
			orgAddress1.Address1 = "Test Address";
			orgAddress1.AddressCapability.SetCapabilityEnabled(addressType);
			orgAddress1.AddressCapability.SetIsMainAddress(addressType);

			var translatedAddress1 = orgAddress1.TranslatedAddresses.AddNew();
			translatedAddress1.Address1 = chineseAddress;
			translatedAddress1.CompanyName = expectedAdditionalCompanyName;
			translatedAddress1.Language = SharedConstants.Languages.ChineseSimplified;

			Factory.Save();

			TestObjectCreator.CreateInvoiceWithLine(invoiceType, "INV2", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, orgHeader1, glAccount.PK);

			Factory.Save();

			DataTable resultForOrg1 = RunScript(
				ledgerType,
				false,
				new string[] { orgHeader1.OH_Code },
				Array.Empty<string>(),
				false);

			AssertEquals("Result count should be 1.", 1, resultForOrg1.Rows.Count);
			AssertEquals("The value of AdditionalCompanyName should be expectedAdditionalCompanyName", expectedAdditionalCompanyName, resultForOrg1.Rows[0]["AdditionalCompanyName"]);

			// Non-Main Address
			var orgHeader2 = testObjectCreator.CreateOrgHeader("GHI" + addressType, true, true);
			orgHeader2.Addresses.RemoveAndDeleteAll();

			var orgAddress2 = orgHeader2.Addresses.AddNew();
			orgAddress2.AddressCapability.SetCapabilityEnabled(addressType);
			orgAddress2.Address1 = chineseAddress;
			orgAddress2.CompanyName = expectedAdditionalCompanyName;
			orgAddress2.Language = SharedConstants.Languages.ChineseSimplified;

			Factory.Save();

			TestObjectCreator.CreateInvoiceWithLine(invoiceType, "INV3", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, orgHeader2, glAccount.PK);

			Factory.Save();

			DataTable resultForOrg2 = RunScript(
				ledgerType,
				false,
				new string[] { orgHeader2.OH_Code },
				Array.Empty<string>(),
				false);

			AssertEquals("Result count should be 1.", 1, resultForOrg2.Rows.Count);
			AssertEquals("The value of AdditionalCompanyName should be expectedAdditionalCompanyName", expectedAdditionalCompanyName, resultForOrg2.Rows[0]["AdditionalCompanyName"]);

			// Non-Main Translated Addresss
			var orgHeader3 = testObjectCreator.CreateOrgHeader("LMN" + addressType, true, true);
			orgHeader3.Addresses.RemoveAndDeleteAll();

			var orgAddress3 = orgHeader3.Addresses.AddNew();
			orgAddress3.AddressCapability.SetCapabilityEnabled(addressType);
			orgAddress3.OA_RN_NKCountryCode = Constants.CountryCodes.China;
			orgAddress3.Language = SharedConstants.Languages.English;
			orgAddress3.Address1 = "Test Address";

			var translatedAddress3 = orgAddress3.TranslatedAddresses.AddNew();
			translatedAddress3.Address1 = chineseAddress;
			translatedAddress3.CompanyName = expectedAdditionalCompanyName;
			translatedAddress3.Language = SharedConstants.Languages.ChineseSimplified;

			Factory.Save();

			TestObjectCreator.CreateInvoiceWithLine(invoiceType, "INV4", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, orgHeader3, glAccount.PK);

			Factory.Save();

			DataTable resultForOrg3 = RunScript(
				ledgerType,
				false,
				new string[] { orgHeader3.OH_Code },
				Array.Empty<string>(),
				false);

			AssertEquals("Result count should be 1.", 1, resultForOrg3.Rows.Count);
			AssertEquals("The value of AdditionalCompanyName should be expectedAdditionalCompanyName", expectedAdditionalCompanyName, resultForOrg3.Rows[0]["AdditionalCompanyName"]);

			// Empty
			var orgHeader4 = testObjectCreator.CreateOrgHeader("OPQ" + addressType, true, true);

			Factory.Save();

			TestObjectCreator.CreateInvoiceWithLine(invoiceType, "INV5", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, orgHeader4, glAccount.PK);

			Factory.Save();

			DataTable resultForOrg4 = RunScript(
				ledgerType,
				false,
				new string[] { orgHeader4.OH_Code },
				Array.Empty<string>(),
				false);

			AssertEquals("Result count should be 1.", 1, resultForOrg4.Rows.Count);
			AssertEquals("The value of AdditionalCompanyName should be null", DBNull.Value, resultForOrg4.Rows[0]["AdditionalCompanyName"]);
		}

		public void TestGetAdditionalCompanyNameWhenCompanyOrgProxyIsNull()
		{
			var companyWithoutOrgProxy = TestObjectCreator.CreateNewCompany("ABC");
			companyWithoutOrgProxy.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			companyWithoutOrgProxy.GC_OH_OrgProxy = ZGuid.Empty;

			var branchWithoutOrgProxy = TestObjectCreator.CreateNewBranch(companyWithoutOrgProxy, "BB1");
			branchWithoutOrgProxy.GB_OH_OrgProxy = ZGuid.Empty;

			var branchOrgProxy = TestObjectCreator.CreateOrgHeader("ORGBB2", false, true);
			branchOrgProxy.OH_Language = SharedConstants.Languages.ChineseSimplified;
			var branchWithOrgProxy = TestObjectCreator.CreateNewBranch(companyWithoutOrgProxy, "BB2");
			branchWithOrgProxy.GB_OH_OrgProxy = branchOrgProxy.PK;
			Factory.Save();

			AssertEquals("Precondition", ZGuid.Empty, companyWithoutOrgProxy.GC_OH_OrgProxy);
			AssertEquals("Precondition", ZGuid.Empty, branchWithoutOrgProxy.GB_OH_OrgProxy);
			AssertEquals("Precondition", branchOrgProxy.PK, branchWithOrgProxy.GB_OH_OrgProxy);

			var debtorOrgHeader = TestObjectCreator.CreateOrgHeader("ORG123", true, true);
			debtorOrgHeader.Addresses.RemoveAndDeleteAll();
			var orgAddress = debtorOrgHeader.Addresses.AddNew();
			orgAddress.OA_RN_NKCountryCode = Constants.CountryCodes.China;
			orgAddress.Language = SharedConstants.Languages.ChineseSimplified;
			orgAddress.Address1 = OrgHeaderUnicodeTestConstants.ChineseAddress1;
			orgAddress.CompanyName = OrgHeaderUnicodeTestConstants.ChineseCompanyName1;
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Receivables);
			orgAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Receivables);
			Factory.Save();

			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchWithOrgProxy.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, debtorOrgHeader, glAccount.PK);
				Factory.Save();

				var resultForOrg = RunScript(
				LedgerTypes.AccountsReceivable,
				false,
				new string[] { debtorOrgHeader.OH_Code },
				Array.Empty<string>(),
				false);

				AssertEquals("Result count should be 1.", 1, resultForOrg.Rows.Count);
				AssertEquals(OrgHeaderUnicodeTestConstants.ChineseCompanyName1, resultForOrg.Rows[0]["AdditionalCompanyName"]);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchWithoutOrgProxy.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var resultForOrg = RunScript(
				LedgerTypes.AccountsReceivable,
				false,
				new string[] { debtorOrgHeader.OH_Code },
				Array.Empty<string>(),
				false);

				AssertEquals("Result count should be 1.", 1, resultForOrg.Rows.Count);
				AssertEquals("The value of AdditionalCompanyName should be null", DBNull.Value, resultForOrg.Rows[0]["AdditionalCompanyName"]);
			}
		}

		public void TestAssignedStaff()
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			AssertNotEquals("ALL", GlbDepartment.CurrentDepartment.GE_Code.ToString());

			var staffDic = new Dictionary<string, GlbStaff>();

			foreach (var org in new[] { TestObjectCreator.ABIGAS, TestObjectCreator.AALSHI, TestObjectCreator.ZECTRA })
			{
				foreach (var role in new[] { "SAL", "CRE", "CUS" })
				{
					foreach (var department in new[] { GlbDepartment.CurrentDepartment.GE_Code.ToString(), "ALL" })
					{
						var staff = Factory.NewWithValidTestData<GlbStaff>();
						var key = string.Join(".", org.OH_Code, role, department);
						staffDic.Add(key, staff);
						staff.GS_FullName = key;

						var assignment = Factory.New<OrgStaffAssignments>();
						assignment.O8_GC = GlbCompany.CurrentCompany.PK;
						assignment.O8_OH = org.PK;
						assignment.O8_GS_NKPersonResponsible = staff.GS_Code;
						assignment.O8_Department = department;
						assignment.O8_Role = role;
					}
				}
			}

			Factory.Save();

			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.ABIGAS, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.AALSHI, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV3", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.XLINDU, glAccount.PK);
			Factory.Save();

			var resultTable = RunScript(
				LedgerTypes.AccountsReceivable,
				false,
				Array.Empty<string>(),
				Array.Empty<string>(),
				false);

			AssertEquals(3, resultTable.Rows.Count);

			foreach (var row in resultTable.Rows.OfType<DataRow>())
			{
				var accountCode = row["AccountCode"].ToString().Trim();

				var salesRep = row["SalesRep"].ToString();
				var salesRepName = row["SalesRepName"].ToString();
				var creditController = row["CreditController"].ToString();
				var creditControllerName = row["CreditControllerName"].ToString();
				var customerService = row["CustomerService"].ToString();
				var customerServiceName = row["CustomerServiceName"].ToString();

				if (accountCode == "XLINDU")
				{
					AssertEquals("", salesRep);
					AssertEquals("", salesRepName);
					AssertEquals("", creditController);
					AssertEquals("", creditControllerName);
					AssertEquals("", customerService);
					AssertEquals("", customerServiceName);
				}
				else
				{
					var key = string.Join(".", accountCode, "SAL", "ALL");
					var staff = staffDic[key];
					AssertEquals(staff.GS_Code, salesRep);
					AssertEquals(key, salesRepName);

					key = string.Join(".", accountCode, "CRE", "ALL");
					staff = staffDic[key];
					AssertEquals(staff.GS_Code, creditController);
					AssertEquals(key, creditControllerName);

					key = string.Join(".", accountCode, "CUS", "ALL");
					staff = staffDic[key];
					AssertEquals(staff.GS_Code, customerService);
					AssertEquals(key, customerServiceName);
				}
			}
		}

		public void TestAgreedPaymentMethod()
		{
			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();

			var org1 = TestObjectCreator.CreateOrgHeader("org1", true, true);
			var org2 = TestObjectCreator.CreateOrgHeader("org2", true, true);
			var org3 = TestObjectCreator.CreateOrgHeader("org3", true, true);

			var ararInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, org1, glAccount.PK);
			var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV2", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, org2, glAccount.PK);
			var arInvoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV3", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, org3, glAccount.PK);
			var aparInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "APINV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, org1, glAccount.PK);
			var apInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "APINV2", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, org2, glAccount.PK);
			var apInvoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "APINV3", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, org3, glAccount.PK);
			Factory.Save();

			arInvoice2.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.BankTransfer;
			arInvoice3.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck;

			apInvoice2.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck;
			apInvoice3.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard;

			Factory.Save();

			//AR Invoice
			DataTable result = RunScript(
				LedgerTypes.AccountsReceivable,
				false,
				new string[] { org1.OH_Code, org2.OH_Code, org3.OH_Code },
				Array.Empty<string>(),
				false);

			var headers = new[] { "AccountCode", "AgreedPaymentMethod" };
			var lines = new[]
			{
				new object[] { org1.OH_Code, string.Empty },
				new object[] { org2.OH_Code, OrgConstants.CreditAgreedPaymentMethods.Code.BankTransfer },
				new object[] { org3.OH_Code, OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck }
			};

			AssertDataTableAllRowsByKeyColumns("AgreedPaymentMethod for ARInvoice", result, headers, lines);

			result = RunScript(
				LedgerTypes.AccountsReceivable,
				false,
				new string[] { org1.OH_Code, org2.OH_Code, org3.OH_Code },
				Array.Empty<string>(),
				false,
				new string[] { "Undefined" });

			lines = new[]
			{
				new object[] { org1.OH_Code, string.Empty }
			};

			AssertDataTableAllRowsByKeyColumns("AgreedPaymentMethod for ARInvoice", result, headers, lines);

			result = RunScript(
				LedgerTypes.AccountsReceivable,
				false,
				new string[] { org1.OH_Code, org2.OH_Code, org3.OH_Code },
				Array.Empty<string>(),
				false,
				new string[] { OrgConstants.CreditAgreedPaymentMethods.Code.BankTransfer, OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck });

			lines = new[]
			{
				new object[] { org2.OH_Code, OrgConstants.CreditAgreedPaymentMethods.Code.BankTransfer },
				new object[] { org3.OH_Code, OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck }
			};
			AssertDataTableAllRowsByKeyColumns("AgreedPaymentMethod for ARInvoice", result, headers, lines);

			//AP Invoice
			result = RunScript(
				LedgerTypes.AccountsPayable,
				false,
				new string[] { org1.OH_Code, org2.OH_Code, org3.OH_Code },
				Array.Empty<string>(),
				false);

			lines = new[]
			{
				new object[] { org1.OH_Code, string.Empty },
				new object[] { org2.OH_Code, OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck },
				new object[] { org3.OH_Code, OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard }
			};

			AssertDataTableAllRowsByKeyColumns("AgreedPaymentMethod for APInvoice", result, headers, lines);

			result = RunScript(
				LedgerTypes.AccountsPayable,
				false,
				new string[] { org1.OH_Code, org2.OH_Code, org3.OH_Code },
				Array.Empty<string>(),
				false,
				new string[] { "Undefined" });

			lines = new[]
			{
				new object[] { org1.OH_Code, string.Empty },
			};

			AssertDataTableAllRowsByKeyColumns("AgreedPaymentMethod for APInvoice", result, headers, lines);

			result = RunScript(
				LedgerTypes.AccountsPayable,
				false,
				new string[] { org1.OH_Code, org2.OH_Code, org3.OH_Code },
				Array.Empty<string>(),
				false,
				new string[] { "Undefined", OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck });

			lines = new[]
			{
				new object[] { org1.OH_Code, string.Empty },
				new object[] { org2.OH_Code, OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck }
			};

			AssertDataTableAllRowsByKeyColumns("AgreedPaymentMethod for APInvoice", result, headers, lines);
		}

		public void TestBigInvoiceAmountDoesntCrashReport()
		{
			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "APINV", TestObjectCreator.CNY, 14136M, 1459450000000M, 0M, 103243491.79M, 0M, TestObjectCreator.AALSHI, TestObjectCreator.GLHeader1.PK);
			Factory.Save();

			var resultTable = RunScript(
				LedgerTypes.AccountsPayable,
				false,
				Array.Empty<string>(),
				Array.Empty<string>(),
				true,
				showInInvoicedCurrency: true);

			AssertEquals(1, resultTable.Rows.Count);
			AssertEquals("BalanceInLocal is correct", 103243491.79M, resultTable.Rows[0]["BalanceInLocal"]);
			AssertEquals("AverageExchangeRate is correct", 14136M, resultTable.Rows[0]["AverageExchangeRate"]);
		}

		public void TestAverageExchangeRateHasCorrectDecimalPlaces()
		{
			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "APINV", TestObjectCreator.CNY, 3.333333M, 10M, 0M, 3M, 0M, TestObjectCreator.AALSHI, TestObjectCreator.GLHeader1.PK);
			Factory.Save();

			var resultTable = RunScript(
				LedgerTypes.AccountsPayable,
				false,
				Array.Empty<string>(),
				Array.Empty<string>(),
				true,
				showInInvoicedCurrency: true);

			AssertEquals(1, resultTable.Rows.Count);
			AssertEquals("AverageExchangeRate is correct", 3.333333M, Math.Round(Convert.ToDecimal(resultTable.Rows[0]["AverageExchangeRate"]), 6));
		}

		public void TestFilterActiveBatchTypesTransactionOnRegistry()
		{
			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			var creator = new TestObjectCreator(Factory);
			var arInvoiceEnabledNewFeature = (ARInvoice)creator.CreateInvoiceWithLine(typeof(ARInvoice), "001", creator.EUR, 1.0M, 102M, 102M, 102M, 102M, creator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.DisbursementInvoice);
			var invoice2 = (ARInvoice)creator.CreateInvoiceWithLine(typeof(ARInvoice), "002", creator.EUR, 1.0M, 103M, 103M, 103M, 103M, creator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.DisbursementInForeignCurrency);
			var invoice3 = (ARInvoice)creator.CreateInvoiceWithLine(typeof(ARInvoice), "003", creator.EUR, 1.0M, 202M, 202M, 202M, 202M, creator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.DisbursementInvoice);
			var invoice4 = (ARInvoice)creator.CreateInvoiceWithLine(typeof(ARInvoice), "004", creator.EUR, 1.0M, 203M, 203M, 203M, 203M, creator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.DisbursementInForeignCurrency);
			var invoice5 = (ARInvoice)creator.CreateInvoiceWithLine(typeof(ARInvoice), "005", creator.EUR, 1.0M, 302M, 302M, 302M, 302M, creator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.DisbursementInvoice);
			var invoice6 = (ARInvoice)creator.CreateInvoiceWithLine(typeof(ARInvoice), "006", creator.EUR, 1.0M, 303M, 303M, 303M, 303M, creator.ABIGAS, glAccount.PK, InvoiceTypesList.Codes.DisbursementInForeignCurrency);
			Factory.Save();

			var result = RunScriptForCollectionBatch("N", "");
			AssertEquals("When includeActiveBatchTran is empty/N, and there aren't Collection Batch, all invoices should be selected (6 rows)", 6, result.Rows.Count);

			result = RunScriptForCollectionBatch("Y", "ST1,ST2");
			AssertEquals("When includeActiveBatchTran is Y, and filterNotActiveBatchTran is 'ST1, ST2', and there aren't Collection Batch, all invoices should be selected (6 rows)", 6, result.Rows.Count);

			var batchSTD = creator.CreateCollectionBatch(creator.EURBankAccount, GlbCompany.CurrentCompany, "", 100m, false, batchType: "STD");
			var orderSTD = creator.CreateCollectionOrder(batchSTD, ZDateTime.Today.Date, creator.ABIGAS, "", 50m, false);
			var batchSTDLine1ForarInvoiceEnabledNewFeature = creator.CreateCollectionOrderLine(orderSTD, arInvoiceEnabledNewFeature, false);
			var batchSTDLine2ForInvoice2 = creator.CreateCollectionOrderLine(orderSTD, invoice2, false);

			var batchST2 = creator.CreateCollectionBatch(creator.EURBankAccount, GlbCompany.CurrentCompany, "", 100m, false, batchType: "ST2");
			var orderST2 = creator.CreateCollectionOrder(batchST2, ZDateTime.Today.Date, creator.ABIGAS, "", 50m, false);
			var batchST2Line1ForInvoice3 = creator.CreateCollectionOrderLine(orderST2, invoice3, false);
			var batchST2Line2ForInvoice4 = creator.CreateCollectionOrderLine(orderST2, invoice4, false);

			var batchST3 = creator.CreateCollectionBatch(creator.EURBankAccount, GlbCompany.CurrentCompany, "", 100m, false, batchType: "ST3");
			var orderST3 = creator.CreateCollectionOrder(batchST3, ZDateTime.Today.Date, creator.ABIGAS, "", 50m, false);
			var batchST3Line1ForInvoice5 = creator.CreateCollectionOrderLine(orderST3, invoice5, false);
			var batchST3Line2ForInvoice6 = creator.CreateCollectionOrderLine(orderST3, invoice6, false);
			Factory.Save();

			result = RunScriptForCollectionBatch("N", "");
			AssertEquals("When includeActiveBatchTran is N/empty, no invoices should be selected (0 rows)", 0, result.Rows.Count);

			result = RunScriptForCollectionBatch("", "STD,ST1,ST2,ST3");
			AssertEquals("When includeActiveBatchTran is N/empty, and filterNotActiveBatchTran is anything, no invoices should be selected (0 rows)", 0, result.Rows.Count);

			batchSTDLine1ForarInvoiceEnabledNewFeature.IsCancelled = true;
			batchST2Line1ForInvoice3.IsCancelled = true;
			batchST3Line1ForInvoice5.IsCancelled = true;
			Factory.Save();

			result = RunScriptForCollectionBatch("N", "");
			AssertEquals("When includeActiveBatchTran is N/empty, and only 3 lines in Collection Batch, three invoices should be selected (3 rows)", 3, result.Rows.Count);
			Assert("Invoice 001 should be one of the 3 rows", result.Rows.Cast<DataRow>().Any(x => x.Field<decimal?>("InvoiceTotal") == 102m));
			Assert("Invoice 003 should be one of the 3 rows", result.Rows.Cast<DataRow>().Any(x => x.Field<decimal?>("InvoiceTotal") == 202m));
			Assert("Invoice 005 should be one of the 3 rows", result.Rows.Cast<DataRow>().Any(x => x.Field<decimal?>("InvoiceTotal") == 302m));

			result = RunScriptForCollectionBatch("Y", "");
			AssertEquals("When includeActiveBatchTran = Y, and filterNotActiveBatchTran = '' (all the type are ticked), all invoices should be selected (6 rows)", 6, result.Rows.Count);

			result = RunScriptForCollectionBatch("Y", "ST1,ST2");
			AssertEquals("When includeActiveBatchTran = Y, and filterNotActiveBatchTran = 'ST1, ST2', and in the registry there are STD,ST1,ST2,ST3 but only STD and ST3 are ticked, five invoices should be selected (5 rows)", 5, result.Rows.Count);

			result = RunScriptForCollectionBatch("Y", "ST2,ST3");
			AssertEquals("When IncludeActiveBatchTran = Y, and filterNotActiveBatchTran = 'ST2, ST3', and in the registry there are STD,ST1,ST2,ST3 but only STD and ST1 are ticked, four invoices should be selected (4 row)", 4, result.Rows.Count);

			DataTable RunScriptForCollectionBatch(string includeActiveBatchTran, string filterNotActiveBatchTran)
			{
				return RunScriptCore(
					PeriodCalculator.GetPeriodFromDate(ZDateTime.Now),
					LedgerTypes.AccountsReceivable,
					false,
					Array.Empty<string>(),
					Array.Empty<string>(),
					false,
					"",
					false,
					false,
					"PER",
					0,
					0,
					0,
					0,
					"Y",
					"Y",
					"",
					"",
					notInActiveBatchTranOnly: "N",
					includeActiveBatchTran: includeActiveBatchTran,
					filterNotActiveBatchTran: filterNotActiveBatchTran
				);
			}
		}

		public void TestMatchStatus()
		{
			AssertMatchStatusAndReason(LedgerTypes.AccountsReceivable, typeof(ARInvoice), "UAC", "UAC - Unallocated", "", "");
			AssertMatchStatusAndReason(LedgerTypes.AccountsPayable, typeof(APInvoice), "UAC", "UAC - Unallocated", "", "");
			AssertMatchStatusAndReason(LedgerTypes.AccountsReceivable, typeof(ARInvoice), "XXX", "XXX", "", "");
			AssertMatchStatusAndReason(LedgerTypes.AccountsPayable, typeof(APInvoice), "XXX", "XXX", "", "");
		}

		public void TestMatchStatusReason()
		{
			AssertMatchStatusAndReason(LedgerTypes.AccountsReceivable, typeof(ARInvoice), "", "", "ADV", "ADV - Receipt/Payment in advance");
			AssertMatchStatusAndReason(LedgerTypes.AccountsPayable, typeof(APInvoice), "", "", "ADV", "ADV - Receipt/Payment in advance");
			AssertMatchStatusAndReason(LedgerTypes.AccountsReceivable, typeof(ARInvoice), "", "", "YYY", "YYY");
			AssertMatchStatusAndReason(LedgerTypes.AccountsPayable, typeof(APInvoice), "", "", "YYY", "YYY");
		}

		void AssertMatchStatusAndReason(string ledgerType, Type invoiceType, string statusCode, string statusCodeWithDescription, string statusReasonCode, string statusReasonCodeWithDescription)
		{
			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(invoiceType, "INV", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, org, glAccount.PK);
			apInvoice.AH_MatchStatus = statusCode;
			apInvoice.AH_MatchStatusReasonCode = statusReasonCode;
			Factory.Save();

			var resultTable = RunScriptForMatchStatusAndReason(
				ledgerType,
				true,
				Array.Empty<string>(),
				Array.Empty<string>(),
				false,
				showMatchStatusAndReason: "Y");

			AssertEquals(1, resultTable.Select($"MatchStatus = '{statusCodeWithDescription}' and MatchStatusReason = '{statusReasonCodeWithDescription}'").Length);
		}

		[TestDate(2020, 3, 10)]
		public void TestPeriodEndRate()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.USD, 0.9M, 10M, 0M, 11.11M, 0M, TestObjectCreator.ABIGAS, glAccount.PK);
			Factory.Save();

			var resultTable = RunScript(
	LedgerTypes.AccountsPayable,
	false,
	Array.Empty<string>(),
	Array.Empty<string>(),
	false,
	showInInvoicedCurrency: true);

			AssertEquals(1, resultTable.Rows.Count);
			AssertEquals("PeriodEndRate should be based on PER rate type by default", DBNull.Value, resultTable.Rows[0]["PeriodEndRate"]);

			var perExchangeRate = Factory.New<RefExchangeRate>();
			perExchangeRate.RE_RX_NKExCurrency = "USD";
			perExchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
			perExchangeRate.RE_ExRateType = "PER";
			perExchangeRate.RE_SellRate = 0.8M;
			perExchangeRate.RE_StartDate = ZDateTime.Today;
			Factory.Save();

			resultTable = RunScript(
	LedgerTypes.AccountsPayable,
	false,
	Array.Empty<string>(),
	Array.Empty<string>(),
	false,
	showInInvoicedCurrency: true);

			AssertEquals(1, resultTable.Rows.Count);
			AssertEquals("PeriodEndRate should be based on PER rate type by default", 0.8M, resultTable.Rows[0]["PeriodEndRate"]);

			using (AccountingConfigurationRegistry.Instance.ARAPOutstandingBalancesCurrencyAdjustmentExchangeRateType.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "BUY"))
			{
				TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 0.7M, new ZDateTime(2020, 3, 1), new ZDateTime(2020, 12, 31));
				resultTable = RunScript(
		LedgerTypes.AccountsPayable,
		false,
		Array.Empty<string>(),
		Array.Empty<string>(),
		false,
		showInInvoicedCurrency: true);

				AssertEquals(1, resultTable.Rows.Count);
				AssertEquals("PeriodEndRate should be based on BUY rate type", 0.7M, resultTable.Rows[0]["PeriodEndRate"]);
			}
		}

		[TestDate(2016, 5, 1)]
		public void TestPostDateBoundaryWhenPeriodIsProvided()
		{
			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, org, glAccount.PK);
			apInvoice.AH_PostDate = new ZDateTime(2016, 5, 31);
			Factory.Save();
			var resultTable = RunScript(LedgerTypes.AccountsPayable, showAll: true, Array.Empty<string>(), Array.Empty<string>(), false);
			AssertEquals("The period end date is 2016-05-31 23:59:00, the invoice with post date 2016-05-31 00:00:00 should in result", 1, resultTable.Rows.Count);

			var apInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV2", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, org, glAccount.PK);
			apInvoice2.AH_PostDate = new ZDateTime(2016, 6, 1).AddMinutes(-1);
			Factory.Save();
			var resultTable2 = RunScript(LedgerTypes.AccountsPayable, showAll: true, Array.Empty<string>(), Array.Empty<string>(), false);
			AssertEquals("The period end date is 2016-05-31 23:59:00, the invoice with post date 2016-05-31 23:59:00 should in result", 2, resultTable2.Rows.Count);

			var apInvoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV3", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, org, glAccount.PK);
			apInvoice3.AH_PostDate = new ZDateTime(2016, 6, 1).AddSeconds(-1);
			Factory.Save();
			var resultTable3 = RunScript(LedgerTypes.AccountsPayable, showAll: true, Array.Empty<string>(), Array.Empty<string>(), false);
			AssertEquals("The period end date is 2016-05-31 23:59:00, the invoice with post date 2016-05-31 23:59:59 should in result", 3, resultTable3.Rows.Count);

			var apInvoice4 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV4", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, org, glAccount.PK);
			apInvoice4.AH_PostDate = new ZDateTime(2016, 6, 1);
			Factory.Save();
			var resultTable4 = RunScript(LedgerTypes.AccountsPayable, showAll: true, Array.Empty<string>(), Array.Empty<string>(), false);
			AssertEquals("The period end date is 2016-05-31 23:59:00, the invoice with post date 2016-06-01 00:00:00 should not in result", 3, resultTable4.Rows.Count);

			var apInvoice5 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV5", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, org, glAccount.PK);
			apInvoice5.AH_PostDate = new ZDateTime(2016, 6, 1).AddSeconds(1);
			Factory.Save();
			var resultTable5 = RunScript(LedgerTypes.AccountsPayable, showAll: true, Array.Empty<string>(), Array.Empty<string>(), false);
			AssertEquals("The period end date is 2016-05-31 23:59:00, the invoice with post date 2016-06-01 00:00:01 should not in result", 3, resultTable5.Rows.Count);

			var apInvoice6 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV6", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, org, glAccount.PK);
			apInvoice6.AH_PostDate = new ZDateTime(2016, 6, 2);
			Factory.Save();
			var resultTable6 = RunScript(LedgerTypes.AccountsPayable, showAll: true, Array.Empty<string>(), Array.Empty<string>(), false);
			AssertEquals("The period end date is 2016-05-31 23:59:00, the invoice with post date 2016-06-02 00:00:00 should not in result", 3, resultTable6.Rows.Count);
		}

		#region Implementation

		void SetUpDataForAgeingTests(string ageing)
		{
			ZGuid chargePK = TestObjectCreator.CC1.PK;
			ZInt period = PeriodCalculator.GetFirstPeriodForYear(ZDateTime.Today.Year);
			ZDateTime[] dates = new ZDateTime[] { ZDateTime.Now, ZDateTime.Now };
			ZInt due = 0, pst = 0, inv = 0;

			switch (ageing)
			{
				case "DUE":
					due = 1;
					break;
				case "PST":
					pst = 1;
					break;
				case "INV":
					inv = 1;
					break;
			}

			for (int i = 10; i < 130; i = i + 10)
			{
				dates[1] = PeriodCalculator.GetFirstDayForPeriod(period);
				TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1" + i.ToString(), TestObjectCreator.USD, 0.5M, i, 0M, i * 2, 0M, TestObjectCreator.ABIGAS, chargePK, dates[pst], dates[due], dates[inv], false);

				dates[1] = PeriodCalculator.GetLastDayForPeriod(period);
				TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2" + i.ToString(), TestObjectCreator.USD, 0.5M, i + 5, 0M, (i + 5) * 2, 0M, TestObjectCreator.ABIGAS, chargePK, dates[pst], dates[due], dates[inv], false);

				period = PeriodCalculator.GetNextPeriod(period);
			}

			dates[1] = new ZDateTime(2009, 12, 01);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV0", TestObjectCreator.USD, 0.5M, 5, 0M, 10, 0M, TestObjectCreator.ABIGAS, chargePK, dates[pst], dates[due], dates[inv], false);

			dates[1] = new ZDateTime(2011, 01, 01);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV3", TestObjectCreator.USD, 0.5M, 130, 0M, 260, 0M, TestObjectCreator.ABIGAS, chargePK, dates[pst], dates[due], dates[inv], false);

			Factory.Save();
		}

		void SetupTermsInfo(OrgARTerms term, string jobType, ZGuid branchPK, ZGuid deptPK, string direction, string transportMode, string invoiceType, string invoiceTerm, int termDays)
		{
			using (term.GetValidationSuspender())
			{
				term.PY_JobType = jobType;
				term.PY_GB_Branch = branchPK;
				term.PY_GE_Department = deptPK;
				term.PY_Direction = direction;
				term.PY_TransportMode = transportMode;
				term.PY_InvoiceClass = invoiceType;
				term.PY_InvoiceTerm = invoiceTerm;
				term.PY_InvoiceDays = (ZByte)termDays;
			}
		}

		void GetAndAssertAgeingResultsByDay(string ageing, string[][] fieldNames, decimal[] fieldValues)
		{
			DataTable result = RunScriptForAgeing(201007, LedgerTypes.AccountsReceivable, false, new string[] { TestObjectCreator.ABIGAS.OH_Code }, Array.Empty<string>(), true, ageing, false, true, "DAY", 30, 60, 120, 150);
			AssertEquals("Result count", 1, result.Rows.Count);
			AssertAgeingResults(result.Rows[0], fieldNames[0], fieldValues);

			result = RunScriptForAgeing(201007, LedgerTypes.AccountsReceivable, false, new string[] { TestObjectCreator.ABIGAS.OH_Code }, Array.Empty<string>(), true, ageing, true, true, "DAY", 30, 60, 120, 150);
			AssertEquals("Result count", 1, result.Rows.Count);
			AssertAgeingResults(result.Rows[0], fieldNames[0], fieldValues);
			AssertAgeingResults(result.Rows[0], fieldNames[1], fieldValues);
		}

		void GetAndAssertAgeingResultsByPeriod(string ageing, string[][] fieldNames, decimal[][] fieldValues)
		{
			for (int i = 0; i < 2; i++)
			{
				ZInt period = PeriodCalculator.GetFirstPeriodForYear(ZDateTime.Today.Year);
				ZBool showLocalEquivalentTotal = i == 1;
				for (int j = 0; j < 12; j++)
				{
					DataTable result = RunScriptForAgeing(period, LedgerTypes.AccountsReceivable, false, new string[] { TestObjectCreator.ABIGAS.OH_Code }, Array.Empty<string>(), true, ageing, showLocalEquivalentTotal, true, "PER", 0, 0, 0, 0);
					AssertEquals("Result count", 1, result.Rows.Count);
					AssertAgeingResults(result.Rows[0], fieldNames[0], fieldValues[j]);

					if (showLocalEquivalentTotal)
					{
						AssertAgeingResults(result.Rows[0], fieldNames[1], fieldValues[j]);
					}

					period = PeriodCalculator.GetNextPeriod(period);
				}
			}
		}

		void AssertAgeingResults(DataRow row, string[] fieldNames, decimal[] fieldValues)
		{
			decimal balance = 0;
			decimal total = 0;

			ZInt multiplier = fieldNames[0].Contains("InLocal") ? 2 : 1;
			object[] convertedFieldValues = (from value in fieldValues select (object)(value * multiplier)).ToArray();

			AssertDataRow(row, fieldNames, convertedFieldValues,
				(fieldName, valueDB) =>
				{
					if (fieldName.Contains("Balance"))
					{
						balance = (decimal)valueDB;
					}
					else
					{
						total = total + (decimal)valueDB;
					}
				});

			AssertEquals("Balance and Totals should be equal", balance, total);
		}

		DataTable RunScriptForAgeing(ZInt period, string ledger, ZBool showAll, string[] orgList, string[] settlementGroupList, ZBool summaryOnly, string ageing, ZBool showLocalEquivalentTotal, ZBool showInInvoicedCurrency, string ageingOption, ZInt day1, ZInt day2, ZInt day3, ZInt day4)
		{
			return RunScriptCore(period, ledger, showAll, orgList, settlementGroupList, summaryOnly, ageing, showLocalEquivalentTotal, showInInvoicedCurrency, ageingOption, day1, day2, day3, day4, "", "", "", "");
		}

		DataTable RunScriptForCountryList(string ledger, ZBool showAll, string[] countryList, string[] exCountryList, ZBool summaryOnly)
		{
			return RunScriptCore(PeriodCalculator.GetPeriodFromDate(ZDateTime.Now), ledger, showAll, Array.Empty<string>(), Array.Empty<string>(), summaryOnly, "", false, false, "PER", 0, 0, 0, 0, "", "", "", "", null, countryList, exCountryList);
		}

		DataTable RunScriptForMatchStatusAndReason(string ledger, ZBool showAll, string[] orgList, string[] settlementGroupList, ZBool summaryOnly, string[] agreedPaymentMethodList = null, bool showInInvoicedCurrency = false, string showMatchStatusAndReason = "Y")
		{
			return RunScriptCore(PeriodCalculator.GetPeriodFromDate(ZDateTime.Now), ledger, showAll, orgList, settlementGroupList, summaryOnly, "", false, showInInvoicedCurrency, "PER", 0, 0, 0, 0, "", "", "", "", agreedPaymentMethodList: agreedPaymentMethodList, showMatchStatusAndReason: showMatchStatusAndReason);
		}

		DataTable RunScript(string ledger, ZBool showAll, string[] orgList, string[] settlementGroupList, ZBool summaryOnly, string[] agreedPaymentMethodList = null, bool showInInvoicedCurrency = false)
		{
			return RunScript(ledger, showAll, orgList, settlementGroupList, summaryOnly, "", "", agreedPaymentMethodList, showInInvoicedCurrency);
		}

		DataTable RunScript(string ledger, ZBool showAll, string[] orgList, string[] settlementGroupList, ZBool summaryOnly, string groupBy, string orderBy, string[] agreedPaymentMethodList = null, bool showInInvoicedCurrency = false)
		{
			return RunScriptCore(PeriodCalculator.GetPeriodFromDate(ZDateTime.Now), ledger, showAll, orgList, settlementGroupList, summaryOnly, "", false, showInInvoicedCurrency, "PER", 0, 0, 0, 0, "", "", groupBy, orderBy, agreedPaymentMethodList: agreedPaymentMethodList);
		}

		DataTable RunScriptCoreForBranchManagementCode(string branchManagementCode, string branchList = "")
		{
			return RunScriptCore(PeriodCalculator.GetPeriodFromDate(ZDateTime.Now), "AR", true, Array.Empty<string>(), Array.Empty<string>(), false, "", false, false, "", 0, 0, 0, 0, "", "", "", "", null, null, null, "", branchManagementCode, branchList);
		}

		public class SPArgs
		{
			public ZInt Period { get; set; }
			public string Ledger { get; set; }
			public ZBool ShowAll { get; set; }
			public string[] OrgList { get; set; }
			public string[] SettlementGroupList { get; set; }
			public ZBool SummaryOnly { get; set; }
			public string Ageing { get; set; }
			public ZBool ShowLocalEquivalentTotal { get; set; }
			public ZBool ShowInInvoicedCurrency { get; set; }
			public string AgeingOption { get; set; }
			public ZInt Day1 { get; set; }
			public ZInt Day2 { get; set; }
			public ZInt Day3 { get; set; }
			public ZInt Day4 { get; set; }
			public string IncludeDisbursement { get; set; }
			public string DisbursementTransOnly { get; set; }
			public string NotInActiveBatchTranOnly { get; set; }
			public string GroupBy { get; set; }
			public string OrderBy { get; set; }
			public bool OverLimitOnly { get; set; }
			public DateTime ReportDate { get; set; }
			public string[] CountryList { get; set; }
			public string[] ExCountryList { get; set; }
			public string BranchManagementCode { get; set; }
			public string BranchList { get; set; }
			public string[] AgreedPaymentMethodList { get; set; }
			public ZBool ExcludeMatchedToFutureRECPAY { get; set; }
			public ZBool FutureRECPAYNotInBalance { get; set; }
			public string IncludeActiveBatchTran { get; set; }
			public string FilterNotActiveBatchTran { get; set; }
			public string ShowMatchStatusAndReason { get; set; }

			public SPArgs()
			{
				ReportDate = DateTime.Now;
			}

			public static SPArgs Default(AccountingPeriodCalculator calculator)
			{
				return new SPArgs()
				{
					Period = calculator.GetPeriodFromDate(ZDateTime.Now),
					Ageing = "",
					AgeingOption = "PER"
				};
			}
		}

		DataTable RunScriptCore(ZInt period, string ledger, ZBool showAll, string[] orgList, string[] settlementGroupList, ZBool summaryOnly, string ageing,
			ZBool showLocalEquivalentTotal, ZBool showInInvoicedCurrency, string ageingOption, ZInt day1, ZInt day2, ZInt day3, ZInt day4, string includeDisbursement,
			string disbursementTransOnly, string groupBy, string orderBy, DateTime? reportDate = null, string[] countryList = null, string[] exCountryList = null,
			string notInActiveBatchTranOnly = "", string branchManagementCode = "", string branchList = "", string[] agreedPaymentMethodList = null, bool excludeMatchedToFutureRECPAY = false, bool futureRECPAYNotInBalance = false,
			string includeActiveBatchTran = "Y", string filterNotActiveBatchTran = "", string showMatchStatusAndReason = "")
		{
			return RunScriptCore(new SPArgs
			{
				Period = period,
				Ledger = ledger,
				ShowAll = showAll,
				OrgList = orgList,
				SettlementGroupList = settlementGroupList,
				SummaryOnly = summaryOnly,
				Ageing = ageing,
				ShowLocalEquivalentTotal = showLocalEquivalentTotal,
				ShowInInvoicedCurrency = showInInvoicedCurrency,
				AgeingOption = ageingOption,
				Day1 = day1,
				Day2 = day2,
				Day3 = day3,
				Day4 = day4,
				IncludeDisbursement = includeDisbursement,
				DisbursementTransOnly = disbursementTransOnly,
				NotInActiveBatchTranOnly = notInActiveBatchTranOnly,
				GroupBy = groupBy,
				OrderBy = orderBy,
				ReportDate = reportDate ?? DateTime.Now,
				CountryList = countryList,
				ExCountryList = exCountryList,
				BranchManagementCode = branchManagementCode,
				BranchList = branchList,
				AgreedPaymentMethodList = agreedPaymentMethodList,
				ExcludeMatchedToFutureRECPAY = excludeMatchedToFutureRECPAY,
				FutureRECPAYNotInBalance = futureRECPAYNotInBalance,
				IncludeActiveBatchTran = includeActiveBatchTran,
				FilterNotActiveBatchTran = filterNotActiveBatchTran,
				ShowMatchStatusAndReason = showMatchStatusAndReason
			});
		}

		DataTable RunScriptCore(SPArgs args)
		{
			var sql = string.Format(@"
EXEC ARAPTransactionsSP 
@Period =					'{0}', 
@Company =					'{1}',
@Branch =					'{32}', 
@OrgList =					'{4}', 
@OrgGroupList =				null, 
@BranchList =				'{25}', 
@CountryList =				'{21}',
@ExCountryList =			'{22}',
@SalesRepList =				null, 
@OverLimitOnly =			'{20}', 
@AgeingOption =				'{9}', 
@Day1 =						'{10}', 
@Day2 =						'{11}', 
@Day3 =						'{12}', 
@Day4 =						'{13}', 
@AccountsRelationShip =		null, 
@ConsolidatedCategory =		null, 
@SalesRepRoll =				null, 
@SummaryOnly =				'{6}',
@LedgerType =				'{2}', 
@AgedByInvoiceDate =		'{7}',
@CurrencyList =				null, 
@IncludeDisbursement =		'{15}', 
@DisbursementTranOnly =		'{16}', 
@SettlementGroupList =		'{5}', 
@CreditRating =				null, 
@ShowInInvoicedCurrency =	'{14}',
@ShowLocalEquivalentTotal =	'{8}', 
@ShowAllTransactions =		'{3}', 
@PaymentStatus =			'', 
@TransactionTypeList =		'INV', 
@PostDateFrom =				null,  
@PostDateTo =				null, 
@DueDateFrom =				null,  
@DueDateTo =				null,
@InvoiceDateFrom =			null,  
@InvoiceDateTo =			null, 
@NotInActiveBatchTranOnly =	'{23}', 
@GroupBy =					'{18}', 
@OrderBy =					'{19}', 
@OrgBranch =				null,  
@ShowAddUser =				'', 
@ShowOnlyAggregated =		'', 
@ShowLineAmounts =			'',
@CurrentDateTime =			'{17}',
@BranchManagementCode = '{24}',
@AgreedPaymentMethodList = '{26}',
@ExcludeMatchedToFutureRECPAY = '{27}',
@FutureRECPAYNotInBalance = '{28}',
@IncludeActiveBatchTran =	'{29}',
@FilterNotActiveBatchTran = '{30}',
@ShowMatchStatusAndReason = '{31}'
",
			args.Period,
			GlbCompany.CurrentCompany.PK,
			args.Ledger,
			args.ShowAll,
			new ZStringBuilder(args.OrgList).ToStringWithDelimiterBetweenAppends(","),
			new ZStringBuilder(args.SettlementGroupList).ToStringWithDelimiterBetweenAppends(","),
			args.SummaryOnly,
			args.Ageing,
			args.ShowLocalEquivalentTotal,
			args.AgeingOption,
			args.Day1,
			args.Day2,
			args.Day3,
			args.Day4,
			args.ShowInInvoicedCurrency,
			args.IncludeDisbursement,
			args.DisbursementTransOnly,
			new ZDateTime(args.ReportDate).ToISO8601String(),
			args.GroupBy,
			args.OrderBy,
			args.OverLimitOnly ? "Y" : "",
			new ZStringBuilder(args.CountryList).ToStringWithDelimiterBetweenAppends(","),
			new ZStringBuilder(args.ExCountryList).ToStringWithDelimiterBetweenAppends(","),
			args.NotInActiveBatchTranOnly,
			args.BranchManagementCode,
			args.BranchList,
			new ZStringBuilder(args.AgreedPaymentMethodList).ToStringWithDelimiterBetweenAppends(","),
			args.ExcludeMatchedToFutureRECPAY,
			args.FutureRECPAYNotInBalance,
			args.IncludeActiveBatchTran,
			args.FilterNotActiveBatchTran,
			args.ShowMatchStatusAndReason,
			GlbBranch.CurrentBranch.PK
			);

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		DataTable RunScriptCoreForPaymentStatus(string paymentStatus, ZDateTime postDateFrom, ZDateTime postDateTo, string ledgerType = "AP")
		{
			var sql = string.Format(@"
EXEC ARAPTransactionsSP 
@Period =					'', 
@Company =					'{0}',
@Branch =					'{6}', 
@OrgList =					null, 
@OrgGroupList =				null, 
@BranchList =				null, 
@CountryList =				null,
@ExCountryList =			null,
@SalesRepList =				null, 
@OverLimitOnly =			'', 
@AgeingOption =				'PER', 
@Day1 =						0, 
@Day2 =						0, 
@Day3 =						0, 
@Day4 =						0, 
@AccountsRelationShip =		null, 
@ConsolidatedCategory =		null, 
@SalesRepRoll =				null, 
@SummaryOnly =				'',
@LedgerType =				'{5}', 
@AgedByInvoiceDate =		'NON',
@CurrencyList =				null, 
@IncludeDisbursement =		'', 
@DisbursementTranOnly =		'', 
@SettlementGroupList =		null, 
@CreditRating =				null, 
@ShowInInvoicedCurrency =	'',
@ShowLocalEquivalentTotal =	'', 
@ShowAllTransactions =		'Y', 
@PaymentStatus =			'{1}', 
@TransactionTypeList =		'INV', 
@PostDateFrom =				null,  
@PostDateTo =				null, 
@DueDateFrom =				null,  
@DueDateTo =				null,
@InvoiceDateFrom =			'{2}',  
@InvoiceDateTo =			'{3}', 
@NotInActiveBatchTranOnly =	'', 
@GroupBy =					'', 
@OrderBy =					'', 
@OrgBranch =				null,  
@ShowAddUser =				'', 
@ShowOnlyAggregated =		'', 
@ShowLineAmounts =			'',
@CurrentDateTime =			'{4}',
@ExcludeMatchedToFutureRECPAY=		'', 
@FutureRECPAYNotInBalance=		'' 
",
			GlbCompany.CurrentCompany.PK,
			paymentStatus,
			new ZDateTime(postDateFrom).ToISO8601String(),
			new ZDateTime(postDateTo).ToISO8601String(),
			new ZDateTime(ZDateTime.Now).ToISO8601String(),
			ledgerType,
			GlbBranch.CurrentBranch.PK);

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}

#endregion

