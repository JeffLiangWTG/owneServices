using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing
{
	public class OSOutstandingAmountValueChangeMonitorTest : TestCaseWithFactory
	{
		public void TestIsValueUpToDate_ApplicalbeOffRegistryOff()
		{
			AssertIsValueUpToDate(false, false, true, true);
		}

		public void TestIsValueUpToDate_ApplicalbeOnRegistryOff()
		{
			AssertIsValueUpToDate(true, false, true, true);
		}

		public void TestIsValueUpToDate_ApplicalbeOffRegistryOn()
		{
			AssertIsValueUpToDate(false, true, false, false);
		}

		public void TestIsValueUpToDate_ApplicalbeOnRegistryOn()
		{
			AssertIsValueUpToDate(true, true, true, false);
		}

		public void TestReset()
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1.2m, 1200m, 0m, 1000m, 0m);
			var monitor = header.OSOutstandingAmountValueChangeMonitor;

			AssertEquals("Pre-condition", false, monitor.IsValueUpToDate);

			monitor.Reset();

			AssertEquals("Pre-condition", true, monitor.IsValueUpToDate);
		}

		public void TestFixed()
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1.2m, 1200m, 0m, 1000m, 0m);
			var monitor = header.OSOutstandingAmountValueChangeMonitor;

			AssertEquals("Pre-condition", false, monitor.IsValueUpToDate);

			monitor.Fixed();

			AssertEquals(true, monitor.IsValueUpToDate);

			header.AH_OutstandingAmount++;

			AssertEquals(true, monitor.IsValueUpToDate);
		}

		public void TestIsFeatureEnabled()
		{
			bool turnOn = true;
			bool turnOff = false;

			var header1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1.2m, 1200m, 0m, 1000m, 0m);
			header1.AH_GC = Env.CurrentCompany.PK;
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(header1.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, turnOff);

			var header2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000002", TestObjectCreator.USD, 1.2m, 1200m, 0m, 1000m, 0m);
			var nonCurrentCompany = Factory.NewWithValidTestData<GlbCompany>();
			header2.AH_GC = nonCurrentCompany.PK;
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(header2.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, turnOn);
	
			AssertEquals(turnOff, header1.OSOutstandingAmountValueChangeMonitor.IsFeatureEnabled_ForTestOnly);
			AssertEquals(turnOn, header2.OSOutstandingAmountValueChangeMonitor.IsFeatureEnabled_ForTestOnly);
		}

		public void TestRebindEvents()
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1.2m, 1200m, 0m, 1000m, 0m);
			var monitor = header.OSOutstandingAmountValueChangeMonitor;
			header.AH_OutstandingAmount++;

			AssertEquals("Pre-condition", false, monitor.IsValueUpToDate);
			AssertEquals(true, monitor.IsPreviouslyFeatureEnabled_ForTestOnly);

			header.AH_Ledger = string.Empty;

			AssertEquals("For transactions like DepositBatchTransactionLine, the ledger/Transaction Type can be changed after created. Should trigger RebindEvents.",
				true, monitor.IsValueUpToDate);
			AssertEquals(false, monitor.IsPreviouslyFeatureEnabled_ForTestOnly);

			monitor.Reset();

			AssertEquals("Pre-condition", true, monitor.IsValueUpToDate);
			AssertEquals(false, monitor.IsPreviouslyFeatureEnabled_ForTestOnly);

			header.AH_Ledger = LedgerTypes.General;
			header.AH_TransactionType = TransactionTypes.Journal;

			AssertEquals("Does not rebind events", true, monitor.IsValueUpToDate);
			AssertEquals(false, monitor.IsPreviouslyFeatureEnabled_ForTestOnly);

			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_TransactionType = TransactionTypes.CreditNote;

			AssertEquals("Rebind events", false, monitor.IsValueUpToDate);
			AssertEquals(true, monitor.IsPreviouslyFeatureEnabled_ForTestOnly);
		}

		public void TestEvaluateIsOSOutstandingAmountApplicable()
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var header1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1.2m, 1200m, 0m, 1000m, 0m);
			AssertEquals("Not applicable when registry is off and create new transaction.", false, header1.AH_IsOSOutstandingAmountApplicable);

			Factory.Save();
			var anotherFactory1 = Factory.CreateNewFactory();
			AssertEquals("Not applicable when registry is off and load old transaction.", false, anotherFactory1.Load<APInvoice>(header1.PK).AH_IsOSOutstandingAmountApplicable);

			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var header2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000002", TestObjectCreator.USD, 1.2m, 1200m, 0m, 1000m, 0m);
			AssertEquals("Applicable when registry is on and create new transaction.", true, header2.AH_IsOSOutstandingAmountApplicable);

			Factory.Save();
			var anotherFactory2 = Factory.CreateNewFactory();
			AssertEquals("Applicable when registry is on and load old transaction.", true, anotherFactory2.Load<APInvoice>(header2.PK).AH_IsOSOutstandingAmountApplicable);
			AssertEquals("Not applicable when registry is on and load old transaction which was not applicable.", false, anotherFactory2.Load<APInvoice>(header1.PK).AH_IsOSOutstandingAmountApplicable);
		}

		public void TestIsOSOutstandingAmountApplicableWhenConvertAPToUA()
		{
			AssertConvertToUACore(false, false);
			AssertConvertToUACore(true, true);

			void AssertConvertToUACore(bool shouldEnableRegistry, bool expectedValue)
			{
				AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, shouldEnableRegistry);

				var transaction = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), Guid.NewGuid().ToString(), TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m);

				AssertEquals("Pre-condition", expectedValue, transaction.AH_IsOSOutstandingAmountApplicable);
				AssertEquals(110m, transaction.AH_Calc_OSOutstandingAmount);

				var converter = new UnapprovedTransactionConverter(Factory);
				var convertedTransaction = converter.ConvertToUA(transaction);

				AssertEquals(false, convertedTransaction.AH_IsOSOutstandingAmountApplicable);
				AssertEquals(0m, convertedTransaction.AH_OSOutstandingAmount);
			}
		}

		public void TestIsOSOutstandingAmountApplicableWhenConvertUAToAP()
		{
			AssertConvertToUACore(false, false);
			AssertConvertToUACore(true, true);

			void AssertConvertToUACore(bool shouldEnableRegistry, bool expectedValue)
			{
				AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, shouldEnableRegistry);

				var transaction = TestObjectCreator.CreateInvoiceWithLine(typeof(UAInvoice), Guid.NewGuid().ToString(), TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m);
				Factory.Save();

				AssertEquals("Pre-condition", false, transaction.AH_IsOSOutstandingAmountApplicable);
				AssertEquals(0m, transaction.AH_OSOutstandingAmount);

				var converter = new UnapprovedTransactionConverter(Factory);
				var convertedTransaction = converter.ConvertToAP(transaction, false);

				AssertEquals(expectedValue, convertedTransaction.AH_IsOSOutstandingAmountApplicable);
				AssertEquals(110m, convertedTransaction.AH_Calc_OSOutstandingAmount);
			}
		}

		public void TestIsOSOutstandingAmountApplicableForIncompleteTransaction()
		{
			AssertIncompleteTransactionCore(typeof(APInvoice), false, false);
			AssertIncompleteTransactionCore(typeof(APInvoice), true, true);
			AssertIncompleteTransactionCore(typeof(APCreditNote), false, false);
			AssertIncompleteTransactionCore(typeof(APCreditNote), true, true);
			AssertIncompleteTransactionCore(typeof(APAdjustmentNote), false, false);
			AssertIncompleteTransactionCore(typeof(APAdjustmentNote), true, true);

			void AssertIncompleteTransactionCore(Type transactionType, bool shouldEnableRegistry, bool expectedValue)
			{
				AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, shouldEnableRegistry);

				var transaction = TestObjectCreator.CreateInvoiceWithLine(transactionType, Guid.NewGuid().ToString(), TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m);
				transaction.SaveAsIncomplete();
				Factory.Save();

				AssertEquals("Pre-condition", false, transaction.AH_IsOSOutstandingAmountApplicable);
				AssertEquals(0m, transaction.AH_OSOutstandingAmount);

				transaction.MoveFromIncompleteToPayableLedger();

				AssertEquals(expectedValue, transaction.AH_IsOSOutstandingAmountApplicable);
				AssertEquals(110m, transaction.AH_Calc_OSOutstandingAmount);
			}
		}

		public void TestIsOSOutstandingAmountApplicableForPendingAllocateTransaction()
		{
			AssertPendingAllocateTransactionCore(false, false);
			AssertPendingAllocateTransactionCore(true, true);

			void AssertPendingAllocateTransactionCore(bool shouldEnableRegistry, bool expectedValue)
			{
				AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, shouldEnableRegistry);

				var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				universalTransaction.Ledger = LedgerTypes.AccountsPayable;
				universalTransaction.LocalTotal = 100m;
				universalTransaction.OSTotal = 100m;
				universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
				var universalLine1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
				universalLine1.LocalAmount = 100m;
				universalLine1.OSAmount = 100m;
				universalTransaction.PostingJournalCollection.Add(universalLine1);

				var invoice = TestObjectCreator.CreateAndAllocateInvoiceWithUniversalTransactionInAllocationApprovalRequest(typeof(APInvoice), universalTransaction, transactionNum: Guid.NewGuid().ToString());

				AssertEquals(expectedValue, invoice.AH_IsOSOutstandingAmountApplicable);
				AssertEquals(-100m, invoice.AH_Calc_OSOutstandingAmount);
			}
		}

		#region Implementation

		void AssertIsValueUpToDate(bool isApplicable, bool isRegistryOn, bool expectedValueAfterSave, bool expectedValue)
		{
			AssertIsValueUpToDateCore(isApplicable, isRegistryOn, expectedValueAfterSave, expectedValue);
			AssertIsValueUpToDateCore(isApplicable, isRegistryOn, null, expectedValue);
		}

		void AssertIsValueUpToDateCore(bool isApplicable, bool isRegistryOn, bool? expectedValueAfterSave, bool expectedValue)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryOn);
			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1.2m, 1200m, 0m, 1000m, 0m);
			header.AH_IsOSOutstandingAmountApplicable = isApplicable;
			var monitor = header.OSOutstandingAmountValueChangeMonitor;
			AssertEquals("Pre-condition", expectedValue, monitor.IsValueUpToDate);

			if (expectedValueAfterSave != null)
			{
				Factory.Save();
				AssertEquals(expectedValueAfterSave.Value, monitor.IsValueUpToDate);
			}

			header.AH_InvoiceAmount += 100m;
			AssertEquals(expectedValue, monitor.IsValueUpToDate);

			monitor.Reset();
			header.AH_GSTAmount += 100m;
			AssertEquals(expectedValue, monitor.IsValueUpToDate);

			monitor.Reset();
			header.AH_LocalTaxAmountOtherTaxes += 100m;
			AssertEquals(expectedValue, monitor.IsValueUpToDate);

			monitor.Reset();
			header.AH_OSTotal += 100m;
			AssertEquals(expectedValue, monitor.IsValueUpToDate);

			monitor.Reset();
			header.AH_OutstandingAmount += 100m;
			AssertEquals(expectedValue, monitor.IsValueUpToDate);

			monitor.Reset();
			AssertNotNullOrEmpty("Pre-condition", header.AH_RX_NKTransactionCurrency);
			header.AH_RX_NKTransactionCurrency = string.Empty;
			AssertEquals(expectedValue, monitor.IsValueUpToDate);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
