using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	class TransactionHeaderBuilderTest : TestCaseWithFactory
	{
		public void TestOverrideAddressAndContact()
		{
			var orgHeader = ObjectCreator.CreateOrgHeader("ABC123", false, false);
			var address = ObjectCreator.CreateAddress(orgHeader);
			var contact = ObjectCreator.CreateContact(orgHeader);

			Factory.Save();

			TxnHeader.DebtorOrCreditor = new Organisation();
			TxnHeader.DebtorOrCreditor.EDICode = "ZABC123";

			Xsd.OrgAddress overrideAddress = new Xsd.OrgAddress();
			overrideAddress.AddressLine1 = "111 Bourke Road";

			Xsd.OrgContact overrideContact = new Xsd.OrgContact();
			overrideContact.Name = "John";

			Assert("Precondition", !TxnHeader.TxnOverrideAddressSpecified);
			Assert("Precondition", !TxnHeader.TxnOverrideContactSpecified);

			Assert(Invoice.AH_OA_InvoiceAddressOverride.IsEmpty);
			Assert(Invoice.AH_OC_InvoiceContactOverride.IsEmpty);

			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");

			Assert(Invoice.AH_OA_InvoiceAddressOverride.IsEmpty);
			Assert(Invoice.AH_OC_InvoiceContactOverride.IsEmpty);

			TxnHeader.TxnOverrideAddress = overrideAddress;
			TxnHeader.TxnOverrideContact = overrideContact;

			Assert("Precondition", TxnHeader.TxnOverrideAddressSpecified);
			Assert("Precondition", TxnHeader.TxnOverrideContactSpecified);

			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");

			Assert(!Invoice.AH_OA_InvoiceAddressOverride.IsEmpty);
			AssertEquals(address.PK, Invoice.AH_OA_InvoiceAddressOverride);

			Assert(!Invoice.AH_OC_InvoiceContactOverride.IsEmpty);
			AssertEquals(contact.PK, Invoice.AH_OC_InvoiceContactOverride);
		}

		public void TestCombineInvoiceLinesByChargeCodeAndDifferentJobType()
		{
			AccChargeCode chargeCode = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);

			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");

			testObjectCreator.CreateShipment("S001001", consol);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCostCollection costs = apps.CostsCollection;
			JobConsolCost cost = costs.TryAddNew();
			cost.E6_AC_ChargeCode = chargeCode.PK;
			cost.E6_OSCostAmount = 100m;
			cost.E6_ApportionmentMethod = "SHP";

			Factory.Save();

			AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());

			TxnLine txnLine1 = TxnHeader.TxnLines.AddNew();
			txnLine1.OsInvoiceAmtExclTax.Value = -30M;
			txnLine1.ConsolOrJobNo = "C001001";
			txnLine1.ConsolOrJobType = TxnLineConsolOrJobType.CSL;
			txnLine1.ChargeCode = chargeCode.AC_Code;

			TxnLine txnLine2 = TxnHeader.TxnLines.AddNew();
			txnLine2.OsInvoiceAmtExclTax.Value = -50M;
			txnLine2.ConsolOrJobNo = "S001001";
			txnLine2.ConsolOrJobType = TxnLineConsolOrJobType.SHP;
			txnLine2.ChargeCode = chargeCode.AC_Code;

			AccountingConfigurationRegistry.Instance.CombineInvoiceLinesByChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			AssertEquals("Lines are not combined as one job is a shipment and the other is a consol", 2, Invoice.Lines.Count);
			AssertEquals(30m, Invoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals(50m, Invoice.Lines[1].AL_OSExTaxAmount);
		}

		public void TestCombineInvoiceLinesByChargeCode()
		{
			TxnLine txnLine1 = TxnHeader.TxnLines.AddNew();
			txnLine1.OsInvoiceAmtExclTax.Value = -30M;
			txnLine1.OsTaxAmount.Value = -10M;
			txnLine1.ChargeCode = "VVV";

			TxnLine txnLine2 = TxnHeader.TxnLines.AddNew();
			txnLine2.OsInvoiceAmtExclTax.Value = -50M;
			txnLine2.OsTaxAmount.Value = -40M;
			txnLine2.ChargeCode = "VVV";

			TxnLine txnLine3 = TxnHeader.TxnLines.AddNew();
			txnLine3.OsInvoiceAmtExclTax.Value = -70M;
			txnLine3.OsTaxAmount.Value = -20M;
			txnLine3.ChargeCode = "VVV";

			TxnLine txnLine4 = TxnHeader.TxnLines.AddNew();
			txnLine4.OsInvoiceAmtExclTax.Value = -90M;
			txnLine4.OsTaxAmount.Value = -30M;
			txnLine4.ChargeCode = "VVV";

			AccountingConfigurationRegistry.Instance.CombineInvoiceLinesByChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			AssertEquals(4, Invoice.Lines.Count);
			AssertEquals(30m, Invoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals(10m, Invoice.Lines[0].AL_OSTaxAmount);
			AssertEquals(50m, Invoice.Lines[1].AL_OSExTaxAmount);
			AssertEquals(40m, Invoice.Lines[1].AL_OSTaxAmount);
			AssertEquals(70m, Invoice.Lines[2].AL_OSExTaxAmount);
			AssertEquals(20m, Invoice.Lines[2].AL_OSTaxAmount);
			AssertEquals(90m, Invoice.Lines[3].AL_OSExTaxAmount);
			AssertEquals(30m, Invoice.Lines[3].AL_OSTaxAmount);

			AccChargeCode chargeCode = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
			txnLine1.ChargeCode = chargeCode.AC_Code;
			txnLine2.ChargeCode = chargeCode.AC_Code;
			txnLine4.ChargeCode = chargeCode.AC_Code;
			Invoice = (InvoicingBase)Factory.New(typeof(APInvoice));

			int invoiceLinesListChangedHitCount = 0;
			var listChangedHandler = new ListChangedEventHandler(
				(sender, e) =>
				{ invoiceLinesListChangedHitCount++; }
			);
			((IBindingList)Invoice.Lines).ListChanged += listChangedHandler;

			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			AssertEquals(2, Invoice.Lines.Count);
			AssertEquals(170m, Invoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals(80m, Invoice.Lines[0].AL_OSTaxAmount);
			AssertEquals(70m, Invoice.Lines[1].AL_OSExTaxAmount);
			AssertEquals(20m, Invoice.Lines[1].AL_OSTaxAmount);

			AssertEquals("ListChanged on Invoice.Lines should be called 3 times: on populating Lines, combining Lines by Charge Code and RunPreSaveValidation", 3, invoiceLinesListChangedHitCount);
		}

		#region TestOverrideSystemExchangeRate

		public void TestOverrideSystemExchangeRate()
		{
			AccChargeCode chargeCode = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);

			ObjectCreator.CreateExchangeRate(ObjectCreator.USD, 0.8M);

			TxnHeader.OsInvoiceAmtExclTax.Value = 135M;
			TxnHeader.OsInvoiceAmtExclTax.CurrencyCode = ObjectCreator.USD.RX_Code;
			TxnHeader.LocalInvoiceAmtExclTax.Value = 69M;
			TxnHeader.LocalInvoiceAmtExclTax.CurrencyCode = ObjectCreator.AUD.RX_Code;

			TxnLine txnLine1 = TxnHeader.TxnLines.AddNew();
			txnLine1.OsInvoiceAmtExclTax.Value = 135M;
			txnLine1.OsInvoiceAmtExclTax.CurrencyCode = ObjectCreator.USD.RX_Code;
			txnLine1.LocalInvoiceAmtExclTax.Value = 69M;
			txnLine1.LocalInvoiceAmtExclTax.CurrencyCode = ObjectCreator.AUD.RX_Code;
			txnLine1.ChargeCode = chargeCode.AC_Code;

			Invoice = (InvoicingBase)Factory.New(typeof(APInvoice));

			TxnHeader.OverrideSystemExchangeRate = true;
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			Assert(txnLine1.OverrideSystemExchangeRate);
			AssertEquals(Invoice.AH_ExchangeRate, 1.956522M);
			AssertNotNull(Invoice.Lines);
			AssertEquals(Invoice.Lines.Count, 1);
			AssertEquals(Invoice.Lines[0].AL_ExchangeRate, 1.956521739M);
			AssertEquals(Invoice.Lines[0].AL_LocalExTaxAmount, -69M);
			AssertEquals(Invoice.Lines[0].AL_OSExTaxAmount, -135M);

			Invoice = (InvoicingBase)Factory.New(typeof(APInvoice));

			TxnHeader.OverrideSystemExchangeRate = false;
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			Assert(!txnLine1.OverrideSystemExchangeRate);
			AssertEquals(Invoice.AH_ExchangeRate, 0.8M);
			AssertNotNull(Invoice.Lines);
			AssertEquals(Invoice.Lines.Count, 1);
			AssertEquals(Invoice.Lines[0].AL_ExchangeRate, 0.8M);
			AssertEquals(Invoice.Lines[0].AL_LocalExTaxAmount, -168.75M);
			AssertEquals(Invoice.Lines[0].AL_OSExTaxAmount, -135M);
		}

		public void TestOverrideSystemExchangeRateForConsolInvoice_UseJobExRate()
		{
			AssertOverrideSystemExchangeRateForConsolInvoice(true);
		}

		public void TestOverrideSystemExchangeRateForConsolInvoice()
		{
			AssertOverrideSystemExchangeRateForConsolInvoice(false);
		}

		public void AssertOverrideSystemExchangeRateForConsolInvoice(bool expectedPostedToEFTValue)
		{
			AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedPostedToEFTValue);
			AccChargeCode chargeCode = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
			ObjectCreator.CreateExchangeRate(ObjectCreator.USD, 0.8M);

			var consol = ObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001234");
			var shipment = ObjectCreator.CreateShipment("S00001001", "AUSYD", "NZAKL", consol);
			var job = ObjectCreator.CreateJob(shipment, false);

			TxnHeader.OsInvoiceAmtExclTax.Value = 135M;
			TxnHeader.OsInvoiceAmtExclTax.CurrencyCode = ObjectCreator.USD.RX_Code;
			TxnHeader.LocalInvoiceAmtExclTax.Value = 69M;
			TxnHeader.LocalInvoiceAmtExclTax.CurrencyCode = ObjectCreator.AUD.RX_Code;

			TxnLine txnLine = TxnHeader.TxnLines.AddNew();
			txnLine.ConsolOrJobType = Xsd.TxnLineConsolOrJobType.CSL;
			txnLine.ConsolOrJobTypeSpecified = true;
			txnLine.ConsolOrJobNo = consol.JK_UniqueConsignRef;
			txnLine.HouseBIllNo = ZString.Empty;
			txnLine.OsInvoiceAmtExclTax.Value = 135M;
			txnLine.OsInvoiceAmtExclTax.CurrencyCode = ObjectCreator.USD.RX_Code;
			txnLine.LocalInvoiceAmtExclTax.Value = 69M;
			txnLine.LocalInvoiceAmtExclTax.CurrencyCode = ObjectCreator.AUD.RX_Code;
			txnLine.ChargeCode = chargeCode.AC_Code;

			Invoice = Factory.New<APInvoice>();
			Invoice.AH_PostedToEFT = true;

			TxnHeader.OverrideSystemExchangeRate = true;
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			AssertEquals("TxnLine.OverrideSystemExchangeRate", true, txnLine.OverrideSystemExchangeRate);
			AssertEquals("AH_ExchangeRate", 1.956522M, Invoice.AH_ExchangeRate);
			AssertEquals("Lines.Count", 1, Invoice.Lines.Count);
			AssertEquals("AL_ExchangeRate", 1.956522M, Invoice.Lines[0].AL_ExchangeRate);
			AssertEquals("AL_OSExTaxAmount", -135M, Invoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals("AL_LocalExTaxAmount", -69M, Invoice.Lines[0].AL_LocalExTaxAmount);

			Invoice = Factory.New<APInvoice>();
			Invoice.AH_PostedToEFT = true;

			TxnHeader.OverrideSystemExchangeRate = false;
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			AssertEquals("TxnLine.OverrideSystemExchangeRate", false, txnLine.OverrideSystemExchangeRate);
			AssertEquals("AH_ExchangeRate", 0.8M, Invoice.AH_ExchangeRate);
			AssertEquals("Lines.Count", 1, Invoice.Lines.Count);
			AssertEquals("AL_ExchangeRate", 0.8M, Invoice.Lines[0].AL_ExchangeRate);
			AssertEquals("AL_OSExTaxAmount", -135M, Invoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals("AL_LocalExTaxAmount", -168.75M, Invoice.Lines[0].AL_LocalExTaxAmount);
		}

		#endregion

		public void TestSettingAH_RX_NKTransactionCurrency()
		{
			TxnHeader.OsInvoiceAmtInclTax.Value = 20M;
			TxnHeader.OsInvoiceAmtInclTax.CurrencyCode = ObjectCreator.GBP.RX_Code;
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			AssertEquals(Invoice.AH_RX_NKTransactionCurrency, ObjectCreator.GBP.RX_Code);

			TxnHeader.OsInvoiceAmtExclTax.Value = 20M;
			TxnHeader.OsInvoiceAmtExclTax.CurrencyCode = ObjectCreator.USD.RX_Code;
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			AssertEquals(Invoice.AH_RX_NKTransactionCurrency, ObjectCreator.GBP.RX_Code);

			TxnHeader.OsInvoiceAmtInclTax = null;
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			AssertEquals(Invoice.AH_RX_NKTransactionCurrency, ObjectCreator.USD.RX_Code);
		}

		public void TestPaymentReferenceDuplication()
		{
			var anotherInvoice = ObjectCreator.CreateInvoice(typeof(ARInvoice), "00001000", ObjectCreator.USD, 1.1M);
			anotherInvoice.AH_TransactionType = TransactionTypes.AdjustmentNote;

			TxnHeader.PaymentReference = "00001000";
			TxnHeader.TxnType = Xsd.TxnType.INV;
			TxnHeader.DebtorOrCreditor.EDICode = ObjectCreator.AALSHI.OH_Code;
			TxnHeader.Ledger = Xsd.TxnLedgerType.AR;

			Invoice = Factory.NewWithValidTestData<ARInvoice>();
			Notify.Clear();
			SystemDataRegistry.Instance.DuplicatePaymentReferenceValidationOnImportingInvoices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AllowDuplicatePaymentReferenceHandlerMethods.Codes.AllowDuplicates);
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AR INV 00001000: ");
			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, "Error: The payment reference '00001000' is already in use by another transaction in this file or database.");

			Invoice = Factory.NewWithValidTestData<ARInvoice>();
			Notify.Clear();
			SystemDataRegistry.Instance.DuplicatePaymentReferenceValidationOnImportingInvoices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AllowDuplicatePaymentReferenceHandlerMethods.Codes.DisallowDuplicatesForEntireLedger);
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AR INV 00001000: ");
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, "Error: The payment reference '00001000' is already in use by another transaction in this file or database.");

			Invoice = Factory.NewWithValidTestData<ARInvoice>();
			Notify.Clear();
			SystemDataRegistry.Instance.DuplicatePaymentReferenceValidationOnImportingInvoices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AllowDuplicatePaymentReferenceHandlerMethods.Codes.DisallowDuplicatesForLedgerAndTransactionType);
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AR INV 00001000: ");
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, "Error: The payment reference '00001000' is already in use by another transaction in this file or database.");

			Invoice = Factory.NewWithValidTestData<ARInvoice>();
			TxnHeader.TxnType = Xsd.TxnType.ADJ;
			Invoice.AH_TransactionType = TransactionTypes.AdjustmentNote;
			Notify.Clear();
			SystemDataRegistry.Instance.DuplicatePaymentReferenceValidationOnImportingInvoices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AllowDuplicatePaymentReferenceHandlerMethods.Codes.DisallowDuplicatesForEntireLedger);
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AR INV 00001000: ");
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, "Error: The payment reference '00001000' is already in use by another transaction in this file or database.");

			Invoice = Factory.NewWithValidTestData<ARInvoice>();
			Invoice.AH_TransactionType = TransactionTypes.AdjustmentNote;
			Notify.Clear();
			SystemDataRegistry.Instance.DuplicatePaymentReferenceValidationOnImportingInvoices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AllowDuplicatePaymentReferenceHandlerMethods.Codes.DisallowDuplicatesForLedgerAndTransactionType);
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AR INV 00001000: ");
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, "Error: The payment reference '00001000' is already in use by another transaction in this file or database.");

			Invoice = Factory.NewWithValidTestData<ARInvoice>();
			TxnHeader.TxnType = Xsd.TxnType.JNL;
			Invoice.AH_TransactionType = TransactionTypes.Journal;
			Notify.Clear();
			SystemDataRegistry.Instance.DuplicatePaymentReferenceValidationOnImportingInvoices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AllowDuplicatePaymentReferenceHandlerMethods.Codes.DisallowDuplicatesForEntireLedger);
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, "Error: The payment reference '00001000' is already in use by another transaction in this file or database.");

			Invoice = Factory.NewWithValidTestData<ARInvoice>();
			Invoice.AH_TransactionType = TransactionTypes.Journal;
			Notify.Clear();
			SystemDataRegistry.Instance.DuplicatePaymentReferenceValidationOnImportingInvoices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AllowDuplicatePaymentReferenceHandlerMethods.Codes.DisallowDuplicatesForLedgerAndTransactionType);
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, "Error: The payment reference '00001000' is already in use by another transaction in this file or database.");
		}

		public void TestCheckInCacheForDuplicateTransactionNumbers()
		{
			string errorMessage = "The transaction number 'TST001' is already in use by another transaction in this file.";

			APInvoice invoice = GetAPInvoice(Factory);
			Builder.CheckInCacheForDuplicateTransactionNumbers(invoice);
			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, errorMessage);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			APInvoice invoiceInDatabase = GetAPInvoice(newFactory);
			newFactory.Save();
			Builder.CheckInCacheForDuplicateTransactionNumbers(invoice);
			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, errorMessage);

			APInvoice invoiceInCache = GetAPInvoice(Factory);
			Builder.CheckInCacheForDuplicateTransactionNumbers(invoice);
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, errorMessage);
		}

		APInvoice GetAPInvoice(BusinessObjectFactory factory)
		{
			APInvoice invoice = factory.NewWithValidTestData<APInvoice>();
			invoice.AH_TransactionNum = "TST001";
			invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			invoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			invoice.AH_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			invoice.AH_GC = GlbCompany.CurrentCompany.PK;
			return invoice;
		}

		public void TestImportOfInvoiceWithInvalidDepartmentCode()
		{
			TxnHeader.Department = "XYZ";
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, "Error: Transaction AP INV 00001000: No matches were found for the following Department: XYZ");
		}

		public void TestImportTxnHeaderWithInvalidBranchCode()
		{
			TxnHeader.Branch = "ABC";
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, "Error: Transaction AP INV 00001000: No matches were found for the following Branch: ABC");
		}

		public void TestImportTxnHeaderWithDifferentCompanyCausesErrors()
		{
			GlbBranch branch = Factory.New<GlbBranch>();
			branch.GB_Code = "XYZ";
			branch.GB_GC = ZGuid.NewZGuid();

			TxnHeader.Branch = branch.GB_Code;

			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, "Error: Transaction AP INV 00001000: No matches were found for the following Branch: XYZ");
		}

		public void TestImportTxnHeaderWithInvalidOrganisation()
		{
			TxnHeader.DebtorOrCreditor = new Organisation();
			TxnHeader.DebtorOrCreditor.EDICode = "ABC123";

			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			ZString expectedMessage = "Error: Transaction AP INV 00001000: No matches were found for the following Organization: ";
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, expectedMessage + "ABC123");
			AssertEquals("Error should be reported", true, Notifier.ErrorsHaveBeenReported);

			Invoice.AH_OH = ZGuid.Empty;
			Notify.Clear();
			TxnHeader.DebtorOrCreditor.EDICode = ObjectCreator.AALSHI.OH_Code;
			TxnHeader.Ledger = Xsd.TxnLedgerType.AR;
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, expectedMessage + "AALSHI");
			AssertEquals("Error should be reported", true, Notifier.ErrorsHaveBeenReported);

			Invoice.AH_OH = ZGuid.Empty;
			Notify.Clear();
			TxnHeader.Ledger = Xsd.TxnLedgerType.AP;
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, expectedMessage);

			Invoice.AH_OH = ZGuid.Empty;
			Notify.Clear();
			TxnHeader.DebtorOrCreditor.EDICode = ObjectCreator.ABIGAS.OH_Code;
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, expectedMessage + "ABIGAS");
			AssertEquals("Error should be reported", true, Notifier.ErrorsHaveBeenReported);

			Invoice.AH_OH = ZGuid.Empty;
			Notify.Clear();
			TxnHeader.Ledger = Xsd.TxnLedgerType.AR;
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, expectedMessage);
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestImportTxnHeaderWithInvalidPostDate()
		{
			TxnHeader.PostDate = ZDateTime.Empty;
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			AssertEquals(ZDateTime.Now, Invoice.AH_PostDate);

			TxnHeader.PostDate = ZDateTime.Invalid;
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			AssertEquals(ZDateTime.Now, Invoice.AH_PostDate);

			TxnHeader.PostDate = ZDateTime.Now.AddYears(-10);

			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			Builder.RunValidationAndReportErrors(Invoice);

			TestHelper.AssertNotificationsContainsErrorMessage(Notify, "Error: Invoice Post Date: This date does not fall into a valid accounting period’s date range.\r\nPlease go to Manage > General Ledger > Period Management > Set Up Next Accounting Year, to ensure there is an accounting period for the date you wish to post to.");
			AssertEquals("Error should be reported", true, Notifier.ErrorsHaveBeenReported);
			AssertEquals(TxnHeader.PostDate, Invoice.AH_PostDate);
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestImportDisbursementTxnHeaderWithDueDate()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			OrgHeader account = creator.ABIGAS;
			OrgARTerms dsbTerm = account.CompanyData.CreateOrLoadDisbursementARTerm();
			dsbTerm.PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			dsbTerm.PY_InvoiceDays = 18;
			account.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			account.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 24;

			TxnHeader.Ledger = Xsd.TxnLedgerType.AR;
			TxnHeader.DebtorOrCreditor.EDICode = account.OH_Code;

			TxnHeader.TxnType = Xsd.TxnType.INV;

			AssertDisbursementFlagAndDueDate(Factory.New<ARInvoice>(), ZDateTime.Empty, true, ZDateTime.Now.AddDays(18));
			AssertDisbursementFlagAndDueDate(Factory.New<ARInvoice>(), ZDateTime.Now.AddDays(5), true, ZDateTime.Now.AddDays(5));
			AssertDisbursementFlagAndDueDate(Factory.New<ARInvoice>(), ZDateTime.Invalid, true, ZDateTime.Now.AddDays(18));

			AssertDisbursementFlagAndDueDate(Factory.New<ARInvoice>(), ZDateTime.Empty, false, ZDateTime.Now.AddDays(24));
			AssertDisbursementFlagAndDueDate(Factory.New<ARInvoice>(), ZDateTime.Now.AddDays(5), false, ZDateTime.Now.AddDays(5));
			AssertDisbursementFlagAndDueDate(Factory.New<ARInvoice>(), ZDateTime.Invalid, false, ZDateTime.Now.AddDays(24));

			TxnHeader.TxnType = Xsd.TxnType.CRD;

			AssertDisbursementFlagAndDueDate(Factory.New<ARCreditNote>(), ZDateTime.Empty, true, ZDateTime.Now.AddDays(18));
			AssertDisbursementFlagAndDueDate(Factory.New<ARCreditNote>(), ZDateTime.Now.AddDays(5), true, ZDateTime.Now.AddDays(5));
			AssertDisbursementFlagAndDueDate(Factory.New<ARCreditNote>(), ZDateTime.Invalid, true, ZDateTime.Now.AddDays(18));

			AssertDisbursementFlagAndDueDate(Factory.New<ARCreditNote>(), ZDateTime.Empty, false, ZDateTime.Now.AddDays(24));
			AssertDisbursementFlagAndDueDate(Factory.New<ARCreditNote>(), ZDateTime.Now.AddDays(5), false, ZDateTime.Now.AddDays(5));
			AssertDisbursementFlagAndDueDate(Factory.New<ARCreditNote>(), ZDateTime.Invalid, false, ZDateTime.Now.AddDays(24));

			TxnHeader.TxnType = Xsd.TxnType.ADJ;

			AssertDisbursementFlagAndDueDate(Factory.New<ARAdjustmentNote>(), ZDateTime.Empty, true, ZDateTime.Now.AddDays(18));
			AssertDisbursementFlagAndDueDate(Factory.New<ARAdjustmentNote>(), ZDateTime.Now.AddDays(5), true, ZDateTime.Now.AddDays(5));
			AssertDisbursementFlagAndDueDate(Factory.New<ARAdjustmentNote>(), ZDateTime.Invalid, true, ZDateTime.Now.AddDays(18));

			AssertDisbursementFlagAndDueDate(Factory.New<ARAdjustmentNote>(), ZDateTime.Empty, false, ZDateTime.Now.AddDays(24));
			AssertDisbursementFlagAndDueDate(Factory.New<ARAdjustmentNote>(), ZDateTime.Now.AddDays(5), false, ZDateTime.Now.AddDays(5));
			AssertDisbursementFlagAndDueDate(Factory.New<ARAdjustmentNote>(), ZDateTime.Invalid, false, ZDateTime.Now.AddDays(24));
		}

		void AssertDisbursementFlagAndDueDate(InvoicingBase invoice, ZDateTime xmlDueDate, bool disbursementFlag, ZDateTime expectedDueDate)
		{
			TxnHeader.DisbursementFlag = disbursementFlag;
			TxnHeader.DisbursementFlagSpecified = true;
			TxnHeader.DueDate = xmlDueDate;
			Builder.SetValuesOnInvoiceBusinessObject(invoice, TxnHeader, ImportContext, "Transaction " + invoice.AH_Ledger + " " + invoice.AH_TransactionType + " 00001000: ");
			AssertEquals("IsDisbursementOrFinal", disbursementFlag, invoice.IsDisbursementOrFinal);
			AssertEquals("AH_TransactionCategory", disbursementFlag ? InvoiceTypesList.Codes.DisbursementInvoice : InvoiceTypesList.Codes.FinalInvoice, invoice.AH_TransactionCategory);
			AssertEquals("AH_DueDate", expectedDueDate, invoice.AH_DueDate);
		}

		public void TestImportTxnHeaderUsingJobNumberAndHouseBill()
		{
			ForwardingShipment shipment1 = ObjectCreator.CreateShipment("S00001001", "AUSYD", "NZAKL");
			Job job1 = ObjectCreator.CreateJob(shipment1);
			shipment1.JS_HouseBill = "HAWB1111";

			ForwardingShipment shipment2 = ObjectCreator.CreateShipment("S00001002", "AUSYD", "NZAKL");
			Job job2 = ObjectCreator.CreateJob(shipment2);
			shipment2.JS_HouseBill = "HAWB1234";

			AccChargeCode chargeCode = ObjectCreator.CC1;

			Factory.Save();

			TxnLine txnLine1 = TxnHeader.TxnLines.AddNew();
			txnLine1.ConsolOrJobNo = shipment1.JS_UniqueConsignRef;
			txnLine1.HouseBIllNo = ZString.Empty;
			txnLine1.MasterBillNo = ZString.Empty;
			txnLine1.OsInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode(new ZDecimal(-200M), "AUD");
			txnLine1.ChargeCode = chargeCode.AC_Code;

			TxnLine txnLine2 = TxnHeader.TxnLines.AddNew();
			txnLine2.HouseBIllNo = shipment2.JS_HouseBill;
			txnLine2.ConsolOrJobNo = ZString.Empty;
			txnLine2.MasterBillNo = ZString.Empty;
			txnLine2.OsInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode(new ZDecimal(-200M), "AUD");
			txnLine2.ChargeCode = chargeCode.AC_Code;

			Invoice = Factory.New<APInvoice>();
			Invoice.AH_TransactionNum = TestObjectCreator.GetRandomString(Invoice.AH_TransactionNumInfo.MaxLength);
			Invoice.AH_FullyPaidDate = ZDateTime.Empty;
			Invoice.SubmittedFromInvoicingForm = true;
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");

			Factory.Save();

			AssertEquals("Should be two Jobs created", 2, Factory.GetDatabaseCount(typeof(JobHeader)));

			Job shipment1Job = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_JobNum, shipment1.JS_UniqueConsignRef));
			ZQuery invoiceLineForShipment1Filter = new ZQuery(AccTransactionLinesSchema.AL_AH, Invoice.PK);
			invoiceLineForShipment1Filter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, shipment1Job.PK);
			InvoicingLineBase invoiceLineForShipment1 = (InvoicingLineBase)Factory.LoadTop1(typeof(APInvoiceLine), invoiceLineForShipment1Filter);

			AssertNotNull("There should be an invoice line relating to job 1", invoiceLineForShipment1);
			AssertEquals("TransactionHeader Line for Shipment 1 Value", 200M, invoiceLineForShipment1.AL_OSExTaxAmount);

			Job shipment2Job = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_JobNum, shipment2.JS_UniqueConsignRef));
			ZQuery invoiceLineForShipment2Filter = new ZQuery(AccTransactionLinesSchema.AL_AH, Invoice.PK);
			invoiceLineForShipment2Filter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, shipment2Job.PK);
			InvoicingLineBase invoiceLineForShipment2 = (InvoicingLineBase)Factory.LoadTop1(typeof(APInvoiceLine), invoiceLineForShipment2Filter);

			AssertNotNull("There should be an invoice line relating to job 2", invoiceLineForShipment2);
			AssertEquals("TransactionHeader Line for Shipment 2 Value", 200M, invoiceLineForShipment2.AL_OSExTaxAmount);
		}

		public void TestImportTxnHeader_NotificationNotRepeatedOnHeaderAndLines()
		{
			var shipment = ObjectCreator.CreateShipment("S00001001", "AUSYD", "NZAKL");
			var job = ObjectCreator.CreateJob(shipment);
			var charge = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 100m, 100m);
			charge.JR_OH_SellAccount = ZGuid.Empty;
			job.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			TxnLine line = TxnHeader.TxnLines.AddNew();
			line.ConsolOrJobNo = shipment.JS_UniqueConsignRef;
			line.OsInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode(new ZDecimal(-200M), "AUD");

			Invoice = Factory.New<APInvoice>();
			Invoice.AH_TransactionNum = TestObjectCreator.GetRandomString(Invoice.AH_TransactionNumInfo.MaxLength);
			Invoice.AH_FullyPaidDate = ZDateTime.Empty;
			Invoice.SubmittedFromInvoicingForm = true;
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			Builder.RunValidationAndReportErrors(Invoice);

			var expectedErrMessage = "Error: Generic Charge: Please enter a value.";
			var expectedWarningMessage = "Warning: Job Number: " + InvoiceLineValidation.ReopenClosedJobWarningMessage;
			var notifications = new ZString(Notify.AsString);
			AssertEquals("Error is not repeating", 1, notifications.Occurrences(expectedErrMessage));
			AssertEquals("Warning is not repeating", 1, notifications.Occurrences(expectedWarningMessage));
		}

		public void TestImportTxnHeaderUsingConsolNumber()
		{
			ForwardingConsol consol1 = ObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001234");
			ForwardingShipment shipment1 = ObjectCreator.CreateShipment("S00001001", "AUSYD", "NZAKL", consol1);
			Job job1 = ObjectCreator.CreateJob(shipment1);
			ForwardingShipment shipment2 = ObjectCreator.CreateShipment("S00001002", "AUSYD", "NZAKL", consol1);
			Job job2 = ObjectCreator.CreateJob(shipment2);

			TxnHeader.TxnLines.AddNew();

			TxnHeader.TxnLines[0].ChargeCode = ObjectCreator.CC1.AC_Code;
			TxnHeader.TxnLines[0].ConsolOrJobType = Xsd.TxnLineConsolOrJobType.CSL;
			TxnHeader.TxnLines[0].ConsolOrJobTypeSpecified = true;
			TxnHeader.TxnLines[0].ConsolOrJobNo = consol1.JK_UniqueConsignRef;
			TxnHeader.TxnLines[0].HouseBIllNo = ZString.Empty;
			TxnHeader.TxnLines[0].MasterBillNo = ZString.Empty;
			TxnHeader.TxnLines[0].OsInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(200M, ObjectCreator.USD, typeof(APInvoice));

			Factory.Save();

			Invoice = Factory.New<APInvoice>();

			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");

			AssertEquals("TransactionHeader Lines Count", 2, Invoice.Lines.Count);

			ZQuery invoiceLineForShipment1Filter = new ZQuery(AccTransactionLinesSchema.AL_AH, Invoice.PK);
			invoiceLineForShipment1Filter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, job1.PK);
			InvoicingLineBase invoiceLineForShipment1 = (InvoicingLineBase)Factory.LoadTop1(typeof(APInvoiceLine), invoiceLineForShipment1Filter);

			AssertNotNull("There should be an invoice line relating to job 1", invoiceLineForShipment1);
			AssertEquals("TransactionHeader Line Value for Shipment 1 Value", 100M, invoiceLineForShipment1.AL_OSExTaxAmount);

			ZQuery invoiceLineForShipment2Filter = new ZQuery(AccTransactionLinesSchema.AL_AH, Invoice.PK);
			invoiceLineForShipment2Filter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, job2.PK);
			InvoicingLineBase invoiceLineForShipment2 = (InvoicingLineBase)Factory.LoadTop1(typeof(APInvoiceLine), invoiceLineForShipment2Filter);

			AssertNotNull("There should be an invoice line relating to job 1", invoiceLineForShipment2);
			AssertEquals("TransactionHeader Line for Shipment 2 Value", 100M, invoiceLineForShipment2.AL_OSExTaxAmount);
		}

		public void TestImportTxnHeaderAPCreditNoteUsingConsolNumber()
		{
			ForwardingConsol consol1 = ObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001234");
			ForwardingShipment shipment1 = ObjectCreator.CreateShipment("S00001001", "AUSYD", "NZAKL", consol1);
			Job job1 = ObjectCreator.CreateJob(shipment1);
			ForwardingShipment shipment2 = ObjectCreator.CreateShipment("S00001002", "AUSYD", "NZAKL", consol1);
			Job job2 = ObjectCreator.CreateJob(shipment2);

			TxnHeader.TxnType = Xsd.TxnType.CRD;
			TxnHeader.TxnLines.AddNew();

			TxnHeader.TxnLines[0].ChargeCode = ObjectCreator.CC1.AC_Code;
			TxnHeader.TxnLines[0].ConsolOrJobType = Xsd.TxnLineConsolOrJobType.CSL;
			TxnHeader.TxnLines[0].ConsolOrJobTypeSpecified = true;
			TxnHeader.TxnLines[0].ConsolOrJobNo = consol1.JK_UniqueConsignRef;
			TxnHeader.TxnLines[0].HouseBIllNo = ZString.Empty;
			TxnHeader.TxnLines[0].MasterBillNo = ZString.Empty;
			TxnHeader.TxnLines[0].OsInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(200M, ObjectCreator.USD, typeof(APCreditNote));

			Factory.Save();

			APCreditNote creditNote = Factory.New<APCreditNote>();

			Builder.SetValuesOnInvoiceBusinessObject(creditNote, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");

			AssertEquals("TransactionHeader Lines Count", 2, creditNote.Lines.Count);

			ZQuery invoiceLineForShipment1Filter = new ZQuery(AccTransactionLinesSchema.AL_AH, creditNote.PK);
			invoiceLineForShipment1Filter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, job1.PK);
			InvoicingLineBase invoiceLineForShipment1 = (InvoicingLineBase)Factory.LoadTop1(typeof(APCreditNoteLine), invoiceLineForShipment1Filter);

			AssertNotNull("There should be an invoice line relating to job 1", invoiceLineForShipment1);
			AssertEquals("TransactionHeader Line Value for Shipment 1 Value", 100M, invoiceLineForShipment1.AL_OSExTaxAmount);

			ZQuery invoiceLineForShipment2Filter = new ZQuery(AccTransactionLinesSchema.AL_AH, creditNote.PK);
			invoiceLineForShipment2Filter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, job2.PK);
			InvoicingLineBase invoiceLineForShipment2 = (InvoicingLineBase)Factory.LoadTop1(typeof(APCreditNoteLine), invoiceLineForShipment2Filter);

			AssertNotNull("There should be an invoice line relating to job 1", invoiceLineForShipment2);
			AssertEquals("TransactionHeader Line for Shipment 2 Value", 100M, invoiceLineForShipment2.AL_OSExTaxAmount);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMessageWithManyInvoiceLines()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "Z6403767341";
			orgHeader.OH_IsCreditor = true;

			var shipment1 = Factory.New<IForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "SSHA7453871";

			var shipment2 = Factory.New<IForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "SCCN1410984";

			var consol = Factory.New<IForwardingConsol>();
			consol.JK_UniqueConsignRef = "CCN1410984";
			consol.AddShipment(shipment2);

			var taxRate = Factory.New<AccTaxRate>();
			taxRate.AT_Code = "FREEVAT";
			taxRate.AT_Type = "RAT";
			taxRate.AT_IsActive = true;
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var charge = ObjectCreator.CreateGlobalChargeCode("TEST");
			Factory.Save();

			// Test with 2 invoice lines.
			TestCaseHelper.ClearTable(EDIInterchangeSchema.Constants.TableName);
			AssertEquals("Precondition: EDIMessage table should be empty.", 0, Factory.GetDatabaseCount(typeof(EDIMessage)));
			CreateAndSaveEdiMessageForTestFile("FinancialTransactions_TwoInvoiceLines.xml");
			AssertEquals("Precondition: EDIMessage table should have 1 message.", 1, Factory.GetDatabaseCount(typeof(EDIMessage)));

			var processor = new ServiceManager.Tasks.StandardXMLProcessor.StandardXMLMessageProcessor();
			InvoicingBase.CallsToAdjustLocalRoundedValuesForImportedConsolCosts_TestOnly = 0;
			var notifications = new NotificationBuffer();
			processor.Process(notifications);
			var callCountForTwoInvoiceLines = InvoicingBase.CallsToAdjustLocalRoundedValuesForImportedConsolCosts_TestOnly;

			Assert("The message does not import successfully (we should make it successful in a later work item)", notifications.HasErrors);
			AssertContains("Message was processed", "Processing Transaction: Transaction AP INV Z6403767341 22100000", notifications.AsString);
			AssertContains("Message was reprocessed after error", "Unable to process messages in batch. Each message will be reprocessed separately.", notifications.AsString);
			AssertGreaterThanOrEqualTo("There must be a minimum of 2 calls to AdjustLocalRoundedValuesForImportedConsolCosts(), once for each import attempt.", callCountForTwoInvoiceLines, 2);
			InvoicingBase.CallsToAdjustLocalRoundedValuesForImportedConsolCosts_TestOnly = 0;

			// Test with 20 invoice lines.
			CreateAndSaveEdiMessageForTestFile("FinancialTransactions_TwentyInvoiceLines.xml");
			AssertEquals("Precondition: EDIMessage table should have 2 messages.", 2, Factory.GetDatabaseCount(typeof(EDIMessage)));
			notifications = new NotificationBuffer();
			processor.Process(notifications);

			var callCountForTwentyInvoiceLines = InvoicingBase.CallsToAdjustLocalRoundedValuesForImportedConsolCosts_TestOnly;
			AssertEquals("There should be no additional calls to AdjustLocalRoundedValuesForImportedConsolCosts() when there are twenty invoice items, as compared to two.", callCountForTwoInvoiceLines, callCountForTwentyInvoiceLines);
			InvoicingBase.CallsToAdjustLocalRoundedValuesForImportedConsolCosts_TestOnly = 0;

			// Test with 200 invoice lines.
			CreateAndSaveEdiMessageForTestFile("FinancialTransactions_TwoHundredInvoiceLines.xml");
			AssertEquals("Precondition: EDIMessage table should have 3 messages.", 3, Factory.GetDatabaseCount(typeof(EDIMessage)));
			notifications = new NotificationBuffer();
			processor.Process(notifications);

			var callCountForTwoHundredInvoiceLines = InvoicingBase.CallsToAdjustLocalRoundedValuesForImportedConsolCosts_TestOnly;
			AssertEquals("There should be no additional calls to AdjustLocalRoundedValuesForImportedConsolCosts() when there are two hundred invoice items, as compared to two.", callCountForTwoInvoiceLines, callCountForTwoHundredInvoiceLines);
			InvoicingBase.CallsToAdjustLocalRoundedValuesForImportedConsolCosts_TestOnly = 0;

			// Note that a test file with 2945 invoice lines exists, but is not included in unit tests because its quite slow.
			// The test file may be used for local performance testing.
			// The XML file was removed from the repository due to its large size, which caused performance issues with the source control system.
			// The file is now hosted externally at the following location:
			// https://wisetechglobal.sharepoint.com/:f:/s/DevelopmentAccountingTeam143/EqthjfhQex1Otwv6Boi3R1sB2yyIqNKn-GJcQZ6gJ3djXg?e=5wXquQ

		void CreateAndSaveEdiMessageForTestFile(string filename)
			{
				var message = Factory.New<Messaging.Business.XmlMessaging.XmlEDIMessage>();
				message.EM_MessageType = EDIMessageTypeList.Codes.XMS;
				message.EM_ApplicationCode = ApplicationCodeList.Codes.XMS;
				message.EM_ReceiveTransmit = EDIMessage.Status.Received;
				message.EM_Status = EDIMessage.Status.Queued;
				message.EM_GB = GlbBranch.CurrentBranch.PK;
				message.EM_MessageSubType = EDIMessageSubTypeList.Codes.FinancialTransactions;
				message.EM_MessageText = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\TransactionHeaderBuilder\Testing\" + filename);
				message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;

				Factory.Save();
			}
		}

		public void TestCurrencyAndExchangeRate_AR_ForeignCurrency()
		{
			SetUpBuyAndSellExchangeRateForGBP(0.5M, 0.6M);
			TxnHeader.PostDate = PostDate;
			TxnHeader.OsInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(100), ObjectCreator.GBP);

			Invoice = (InvoicingBase)Factory.New(typeof(ARInvoice));
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");

			AssertEquals("TransactionHeader Header Exchange Rate: ", 0.6M, Invoice.AH_ExchangeRate);
		}

		public void TestCurrencyAndExchangeRate_AR_LocalCurrency()
		{
			SetUpBuyAndSellExchangeRateForGBP(0.5M, 0.6M);
			TxnHeader.PostDate = PostDate;
			TxnHeader.OsInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(100), GlbCompany.CurrentCompany.LocalCurrency);

			Invoice = (InvoicingBase)Factory.New(typeof(ARInvoice));
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");

			AssertEquals("TransactionHeader Header Exchange Rate: ", 1M, Invoice.AH_ExchangeRate);
		}

		public void TestCurrencyAndExchangeRate_AP_ForeignCurrency()
		{
			SetUpBuyAndSellExchangeRateForGBP(0.5M, 0.6M);
			TxnHeader.PostDate = PostDate;
			TxnHeader.OsInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(100), ObjectCreator.GBP);

			Invoice = (InvoicingBase)Factory.New(typeof(APInvoice));
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");

			AssertEquals("TransactionHeader Header Exchange Rate: ", 0.5M, Invoice.AH_ExchangeRate);
		}

		public void TestCurrencyAndExchangeRate_AP_LocalCurrency()
		{
			SetUpBuyAndSellExchangeRateForGBP(0.5M, 0.6M);
			TxnHeader.PostDate = PostDate;
			TxnHeader.OsInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(100), GlbCompany.CurrentCompany.LocalCurrency);

			Invoice = (InvoicingBase)Factory.New(typeof(APInvoice));
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");

			AssertEquals("TransactionHeader Header Exchange Rate: ", 1M, Invoice.AH_ExchangeRate);
		}

		public void TestCurrencyAndExchangeRate_CB_ForeignCurrency()
		{
			TxnHeader.BankCode = ObjectCreator.GBPBankAccount.AB_Code;
			TxnHeader.LocalInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(200), ObjectCreator.AUD);
			TxnHeader.LocalInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(200), ObjectCreator.AUD);
			TxnHeader.OsInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(123.4567), ObjectCreator.GBP);
			TxnHeader.OsInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(123.4567), ObjectCreator.GBP);
			TxnHeader.PostDate = PostDate;

			DirectPayment payment = Factory.New<DirectPayment>();
			Builder.SetValuesOnDirectReceiptPaymentBusinessObject(payment, TxnHeader, ImportContext, "Transaction CB DPY 00001000: ");
			AssertEquals("TransactionHeader Header Exchange Rate: ", 0.617284M, payment.AH_ExchangeRate);
		}

		public void TestCurrencyAndExchangeRate_CB_LocalCurrency()
		{
			TxnHeader.LocalInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(200), GlbCompany.CurrentCompany.LocalCurrency);
			TxnHeader.LocalInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(200), GlbCompany.CurrentCompany.LocalCurrency);
			TxnHeader.OsInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(200), GlbCompany.CurrentCompany.LocalCurrency);
			TxnHeader.OsInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(200), GlbCompany.CurrentCompany.LocalCurrency);

			DirectPayment payment = Factory.New<DirectPayment>();
			Builder.SetValuesOnDirectReceiptPaymentBusinessObject(payment, TxnHeader, ImportContext, "Transaction CB DPY 00001000: ");
			AssertEquals("TransactionHeader Header Exchange Rate: ", 1M, payment.AH_ExchangeRate);
		}

		public void TestSetChequeBookWithDecimals()
		{
			TxnHeader.BankCode = BankAccount.AB_Code;
			TxnHeader.ChequeOrReference = "00001.2";
			DirectPayment payment = Factory.New<DirectPayment>();
			Builder.SetValuesOnDirectReceiptPaymentBusinessObject(payment, TxnHeader, ImportContext, "Transaction CB DPY 00001000: ");
			AssertEquals("cheque book can't be set when the cheque reference contains decimals.", null, payment.ChequeBook);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2005, 09, 01)]
		public void TestNoDifferenceBetweenXmlHeaderAndLines_WhenOrgIsNotTaxApplicable()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper();
			periodHelper.SetupPeriods();

			ObjectCreator.ABIGAS.CompanyData.SetARTaxApplicable(false);

			TxnHeaderCollection txnHeaderCollection = new TxnHeaderCollection();
			TxnHeaderFlatFileConverter converter = new TxnHeaderFlatFileConverter(null, Factory);
			using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\ARInvoiceWithOrgIsNotTaxRegistered.csv"))
			{
				converter.ImportFlatFile(txnHeaderCollection, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals("TxnHeaderCollection.Count", 1, txnHeaderCollection.Count);
			TxnHeader txnHeader = txnHeaderCollection[0];

			var invoice = Factory.New<ARInvoice>();
			ExportFinancialInvoiceDataAdapter adapter = new ExportFinancialInvoiceDataAdapter();
			adapter.ImportFromValueObject(invoice, txnHeader, ImportContext);

			ZString errorMessage = "Error: OSExTaxAmount: Total of XML lines do not equal XML Header Amount." + System.Environment.NewLine;
			errorMessage += "     XML Header Amount: 0" + System.Environment.NewLine;
			errorMessage += "     XML Lines Total: 3000";

			Assert("No error should be found", !Notify.HasErrors);
			AssertNotContains(errorMessage, Notify.AsString);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2005, 09, 01)]
		public void TestDifferenceInInvoiceTotalBetweenXmlHeaderAndLines()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper();
			periodHelper.SetupPeriods();

			TxnHeaderCollection txnHeaderCollection = new TxnHeaderCollection();
			TxnHeaderFlatFileConverter converter = new TxnHeaderFlatFileConverter(null, Factory);
			using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\ARInvoiceWithMismatchInInvoiceTotal.csv"))
			{
				converter.ImportFlatFile(txnHeaderCollection, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals("TxnHeaderCollection.Count", 1, txnHeaderCollection.Count);
			TxnHeader txnHeader = txnHeaderCollection[0];

			var invoice = Factory.New<ARInvoice>();
			ExportFinancialInvoiceDataAdapter adapter = new ExportFinancialInvoiceDataAdapter();
			adapter.ImportFromValueObject(invoice, txnHeader, ImportContext);

			ZString errorMessage1 = "Error: OSInclTaxAmount: Total of XML lines do not equal XML Header Amount." + System.Environment.NewLine;
			errorMessage1 += "     XML Header Amount: 2500" + System.Environment.NewLine;
			errorMessage1 += "     XML Lines Total: 3000";

			ZString errorMessage2 = "Error: OSExTaxAmount: Total of XML lines do not equal XML Header Amount." + System.Environment.NewLine;
			errorMessage2 += "     XML Header Amount: 2500" + System.Environment.NewLine;
			errorMessage2 += "     XML Lines Total: 3000";

			TestHelper.AssertNotificationsContainsErrorMessage(Notify, errorMessage1);
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, errorMessage2);
		}

		public void TestDifferencesBetweenXmlHeaderAndLines_OSInclTaxAmount()
		{
			TxnHeader.OsInvoiceAmtInclTax.Value = -180M;
			TxnHeader.OsInvoiceAmtExclTax.Value = -100M;
			TxnHeader.OsTaxAmount.Value = -80M;

			TxnLine txnLine1 = TxnHeader.TxnLines.AddNew();
			txnLine1.OsInvoiceAmtExclTax.Value = -50M;
			txnLine1.OsInvoiceAmtInclTax.Value = -55M;
			txnLine1.OsTaxAmount.Value = -5M;

			TxnLine txnLine2 = TxnHeader.TxnLines.AddNew();
			txnLine2.OsInvoiceAmtExclTax.Value = -50M;
			txnLine2.OsInvoiceAmtInclTax.Value = -55M;
			txnLine2.OsTaxAmount.Value = -5M;

			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");

			Builder.CheckTotalsOnTransactionHeaderWithLines(Invoice, TxnHeader, "Transaction AP INV 00001000: ");
			Builder.RunValidationAndReportErrors(Invoice);

			ZString expectedMessage = "Error: Transaction AP INV 00001000: OSInclTaxAmount: Total of XML lines do not equal XML Header Amount." + System.Environment.NewLine;
			expectedMessage += "     XML Header Amount: 180" + System.Environment.NewLine;
			expectedMessage += "     XML Lines Total: 110";

			TestHelper.AssertNotificationsContainsErrorMessage(Notify, expectedMessage);
		}

		public void TestDiscrepanciesBetweenXmlAndBizObjAmounts_CorrectAmounts()
		{
			TxnHeader.OsInvoiceAmtExclTax.Value = -100M;
			TxnHeader.OsTaxAmount.Value = -80M;

			TxnLine txnLine1 = TxnHeader.TxnLines.AddNew();
			txnLine1.OsInvoiceAmtExclTax.Value = -50M;
			txnLine1.OsTaxAmount.Value = -40M;

			TxnLine txnLine2 = TxnHeader.TxnLines.AddNew();
			txnLine2.OsInvoiceAmtExclTax.Value = -50M;
			txnLine2.OsTaxAmount.Value = -40M;

			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			Builder.CheckTotalsOnTransactionHeaderWithLines(Invoice, TxnHeader, "Transaction AP INV 00001000: ");
			Builder.RunValidationAndReportErrors(Invoice);

			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, "Total of XML lines do not equal XML Header Amount.");
			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, "Total of XML lines do not equal XML Header Amount.");
		}

		public void TestDifferencesBetweenXmlHeaderAndLines_OSExTaxAmount()
		{
			TxnHeader.OsInvoiceAmtExclTax.Value = -100M;
			TxnHeader.OsTaxAmount.Value = -80M;

			TxnLine txnLine1 = TxnHeader.TxnLines.AddNew();
			txnLine1.OsInvoiceAmtExclTax.Value = -50M;

			TxnLine txnLine2 = TxnHeader.TxnLines.AddNew();
			txnLine2.OsInvoiceAmtExclTax.Value = -50M;

			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");

			txnLine1.OsInvoiceAmtExclTax.Value = -150M;

			Builder.CheckTotalsOnTransactionHeaderWithLines(Invoice, TxnHeader, "Transaction AP INV 00001000: ");
			Builder.RunValidationAndReportErrors(Invoice);

			ZString expectedMessage = "Error: Transaction AP INV 00001000: OSExTaxAmount: Total of XML lines do not equal XML Header Amount." + System.Environment.NewLine;
			expectedMessage += "     XML Header Amount: 100" + System.Environment.NewLine;
			expectedMessage += "     XML Lines Total: 200";

			TestHelper.AssertNotificationsContainsErrorMessage(Notify, expectedMessage);
		}

		public void TestDifferencesBetweenXmlHeaderAndLines_OSTaxAmount()
		{
			TxnHeader.OsInvoiceAmtExclTax.Value = -100M;
			TxnHeader.OsTaxAmount.Value = -80M;

			TxnLine txnLine1 = TxnHeader.TxnLines.AddNew();
			txnLine1.OsInvoiceAmtExclTax.Value = -50M;
			txnLine1.OsTaxAmount.Value = -40M;

			TxnLine txnLine2 = TxnHeader.TxnLines.AddNew();
			txnLine2.OsInvoiceAmtExclTax.Value = -50M;
			txnLine2.OsTaxAmount.Value = -40M;

			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");

			txnLine1.OsTaxAmount.Value = -100M;

			Builder.CheckTotalsOnTransactionHeaderWithLines(Invoice, TxnHeader, "Transaction AP INV 00001000: ");
			Builder.RunValidationAndReportErrors(Invoice);

			ZString expectedMessage = "Error: Transaction AP INV 00001000: OSTaxAmount: Total of XML lines do not equal XML Header Amount." + System.Environment.NewLine;
			expectedMessage += "     XML Header Amount: 80" + System.Environment.NewLine;
			expectedMessage += "     XML Lines Total: 140";

			TestHelper.AssertNotificationsContainsErrorMessage(Notify, expectedMessage);
		}

		public void TestDifferencesBetweenXmlHeaderAndLines_OSWHTAmount()
		{
			TxnHeader.OsInvoiceAmtExclTax.Value = -100M;
			TxnHeader.OsTaxAmount.Value = -80M;

			TxnLine txnLine1 = TxnHeader.TxnLines.AddNew();
			txnLine1.OsInvoiceAmtExclTax.Value = -50M;
			txnLine1.OsTaxAmount.Value = -40M;

			TxnLine txnLine2 = TxnHeader.TxnLines.AddNew();
			txnLine2.OsInvoiceAmtExclTax.Value = -50M;
			txnLine2.OsTaxAmount.Value = -40M;

			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");

			txnLine1.OsWHTAmount.Value = -200M;

			Builder.CheckTotalsOnTransactionHeaderWithLines(Invoice, TxnHeader, "Transaction AP INV 00001000: ");
			Builder.RunValidationAndReportErrors(Invoice);

			ZString expectedMessage = "Error: Transaction AP INV 00001000: OSWHTAmount: Total of XML lines do not equal XML Header Amount." + System.Environment.NewLine;
			expectedMessage += "     XML Header Amount: 0" + System.Environment.NewLine;
			expectedMessage += "     XML Lines Total: 200";

			TestHelper.AssertNotificationsContainsErrorMessage(Notify, expectedMessage);
		}

		public void TestDifferencesBetweenXmlAndBizObj_OSExTaxAmount()
		{
			TxnHeader.OsInvoiceAmtExclTax.Value = -100M;
			TxnHeader.OsTaxAmount.Value = -80M;

			TxnLine txnLine1 = TxnHeader.TxnLines.AddNew();
			txnLine1.OsInvoiceAmtExclTax.Value = -50M;
			txnLine1.OsTaxAmount.Value = -40M;

			TxnLine txnLine2 = TxnHeader.TxnLines.AddNew();
			txnLine2.OsInvoiceAmtExclTax.Value = -50M;
			txnLine2.OsTaxAmount.Value = -40M;

			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");

			TxnHeader.OsInvoiceAmtExclTax.Value = -150M;

			Builder.CheckTotalsOnTransactionHeaderWithLines(Invoice, TxnHeader, "Transaction AP INV 00001000: ");
			Builder.RunValidationAndReportErrors(Invoice);

			ZString expectedMessage = "Error: Transaction AP INV 00001000: OSExTaxAmount: Total of XML lines do not equal XML Header Amount." + System.Environment.NewLine;
			expectedMessage += "     XML Header Amount: 150" + System.Environment.NewLine;
			expectedMessage += "     XML Lines Total: 100";

			TestHelper.AssertNotificationsContainsErrorMessage(Notify, expectedMessage);
		}

		public void TestDifferencesBetweenXmlAndBizObj_OSTaxAmount()
		{
			TxnHeader.OsInvoiceAmtExclTax.Value = -100M;
			TxnHeader.OsTaxAmount.Value = -80M;

			TxnLine txnLine1 = TxnHeader.TxnLines.AddNew();
			txnLine1.OsInvoiceAmtExclTax.Value = -50M;
			txnLine1.OsTaxAmount.Value = -40M;

			TxnLine txnLine2 = TxnHeader.TxnLines.AddNew();
			txnLine2.OsInvoiceAmtExclTax.Value = -50M;
			txnLine2.OsTaxAmount.Value = -40M;

			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");

			TxnHeader.OsTaxAmount.Value = -20M;

			Builder.CheckTotalsOnTransactionHeaderWithLines(Invoice, TxnHeader, "Transaction AP INV 00001000: ");
			Builder.RunValidationAndReportErrors(Invoice);

			ZString expectedMessage = "Error: Transaction AP INV 00001000: OSTaxAmount: Total of XML lines do not equal XML Header Amount." + System.Environment.NewLine;
			expectedMessage += "     XML Header Amount: 20" + System.Environment.NewLine;
			expectedMessage += "     XML Lines Total: 80";

			TestHelper.AssertNotificationsContainsErrorMessage(Notify, expectedMessage);
		}

		public void TestDifferencesBetweenXmlAndBizObj_OSWHTAmount()
		{
			TxnHeader.OsInvoiceAmtExclTax.Value = -100M;
			TxnHeader.OsTaxAmount.Value = -80M;

			TxnLine txnLine1 = TxnHeader.TxnLines.AddNew();
			txnLine1.OsInvoiceAmtExclTax.Value = -50M;
			txnLine1.OsTaxAmount.Value = -40M;

			TxnLine txnLine2 = TxnHeader.TxnLines.AddNew();
			txnLine2.OsInvoiceAmtExclTax.Value = -50M;
			txnLine2.OsTaxAmount.Value = -40M;

			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");

			TxnHeader.OsWHTAmount.Value = -50M;

			Builder.CheckTotalsOnTransactionHeaderWithLines(Invoice, TxnHeader, "Transaction AP INV 00001000: ");
			Builder.RunValidationAndReportErrors(Invoice);

			ZString expectedMessage = "Error: Transaction AP INV 00001000: OSWHTAmount: Total of XML lines do not equal XML Header Amount." + System.Environment.NewLine;
			expectedMessage += "     XML Header Amount: 50" + System.Environment.NewLine;
			expectedMessage += "     XML Lines Total: 0";

			TestHelper.AssertNotificationsContainsErrorMessage(Notify, expectedMessage);
		}

		public void TestFieldsAreSetCorrectlyOnDirectPayment()
		{
			TxnHeader.Ledger = Xsd.TxnLedgerType.CB;
			TxnHeader.TxnType = Xsd.TxnType.DPY;
			TxnHeader.InvoiceDate = ZDateTime.Now.AddDays(-5);
			TxnHeader.BankCode = BankAccount.AB_Code;
			TxnHeader.ReceiptPaymentType = Xsd.TxnHeaderReceiptPaymentType.CHQ;
			TxnHeader.ChequeOrReference = ChequeBook.AK_StartNo.ToString();
			TxnHeader.PostDate = ZDateTime.Now.AddDays(-5);
			TxnHeader.Description = "Software Rental";
			TxnHeader.ChequeDrawer = "Eagle Datamation International";
			TxnHeader.DrawerBank = "ANZ";
			TxnHeader.DrawerBankBranch = "Alexandria 2015";

			TxnLine line1 = TxnHeader.TxnLines.AddNew();

			DirectPayment payment = Factory.New<DirectPayment>();
			Builder.SetValuesOnDirectReceiptPaymentBusinessObject(payment, TxnHeader, ImportContext, "Transaction CB DPY 00001000: ");

			AssertEquals(TxnHeader.InvoiceDate, payment.AH_InvoiceDate);
			AssertEquals(TxnHeader.BankCode, payment.BankAccount.AB_Code);
			AssertEquals(ChequeBook.AK_Code, payment.ChequeBook.AK_Code);
			AssertEquals(ZArchitecture.Core.ReceiptTypes.Cheque, payment.AH_ReceiptType);
			AssertEquals(TxnHeader.ChequeOrReference.ToString().PadLeft(6, '0'), payment.AH_ChequeOrReference);
			AssertEquals(TxnHeader.PostDate, payment.AH_PostDate);
			AssertEquals(TxnHeader.Description, payment.AH_Desc);
			AssertEquals(TxnHeader.ChequeDrawer, payment.AH_ChequeDrawer);
			AssertEquals(ZString.Empty, payment.AH_DrawerBank);
			AssertEquals(ZString.Empty, payment.AH_DrawerBranch);
		}

		public void TestFieldsAreSetCorrectlyOnDirectReceipt()
		{
			TxnHeader.Ledger = Xsd.TxnLedgerType.CB;
			TxnHeader.TxnType = Xsd.TxnType.DRC;
			TxnHeader.InvoiceDate = ZDateTime.Now.AddDays(-5);
			TxnHeader.BankCode = BankAccount.AB_Code;
			TxnHeader.ReceiptPaymentType = Xsd.TxnHeaderReceiptPaymentType.CHQ;
			TxnHeader.ChequeOrReference = "000001";
			TxnHeader.PostDate = ZDateTime.Now.AddDays(-5);
			TxnHeader.Description = "Software Rental";
			TxnHeader.ChequeDrawer = "Eagle Datamation International";
			TxnHeader.DrawerBank = "ANZ";
			TxnHeader.DrawerBankBranch = "Alexandria 2015";

			TxnLine line1 = TxnHeader.TxnLines.AddNew();

			DirectReceipt receipt = Factory.New<DirectReceipt>();
			Builder.SetValuesOnDirectReceiptPaymentBusinessObject(receipt, TxnHeader, ImportContext, "Transaction CB DPY 00001000: ");

			AssertEquals(TxnHeader.InvoiceDate, receipt.AH_InvoiceDate);
			AssertEquals(TxnHeader.BankCode, receipt.BankAccount.AB_Code);
			AssertEquals(ZArchitecture.Core.ReceiptTypes.Cheque, receipt.AH_ReceiptType);
			AssertEquals(TxnHeader.ChequeOrReference, receipt.AH_ChequeOrReference);
			AssertEquals(TxnHeader.PostDate, receipt.AH_PostDate);
			AssertEquals(TxnHeader.Description, receipt.AH_Desc);
			AssertEquals(TxnHeader.ChequeDrawer, receipt.AH_ChequeDrawer);
			AssertEquals(TxnHeader.DrawerBank, receipt.AH_DrawerBank);
			AssertEquals(TxnHeader.DrawerBankBranch, receipt.AH_DrawerBranch);
		}

		public void TestFieldsAreSetCorrectlyOnJournal()
		{
			TxnHeader.Ledger = Xsd.TxnLedgerType.AR;
			TxnHeader.TxnType = Xsd.TxnType.JNL;
			OrganisationValueObjectDataAdapter organisationAdapter = new OrganisationValueObjectDataAdapter();
			TxnHeader.DebtorOrCreditor = organisationAdapter.ExportToValueObject(ObjectCreator.ABIGAS, new ValueObjectExportContext(new NotificationBuffer()));
			TxnHeader.InvoiceDate = PostDate.AddDays(-5);
			TxnHeader.PostDate = PostDate;
			TxnHeader.DueDate = PostDate.AddDays(1);
			TxnHeader.Description = "Software Rental";

			TxnHeader.GlAccount = "2010.00.00";
			TxnHeader.Branch = GlbBranch.CurrentBranch.GB_Code;
			TxnHeader.Department = GlbDepartment.CurrentDepartment.GE_Code;

			TxnHeader.OsInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(200), ObjectCreator.GBP);
			TxnHeader.OsInvoiceAmtExclTax = TxnHeader.OsInvoiceAmtInclTax;
			TxnHeader.LocalInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(100), ObjectCreator.GBP);
			TxnHeader.LocalInvoiceAmtExclTax = TxnHeader.LocalInvoiceAmtInclTax;
			//TxnHeader.DebtorOrCreditorGUID = ObjectCreator.ABIGAS.PK.ToString();

			Journal journal = Factory.New<ARJournal>();
			Builder.SetValuesOnJournalBusinessObject(journal, TxnHeader, ImportContext, "Transaction JNL 00001000: ");

			AssertHeaderValues(journal, TxnHeader, 2M, "CR");

			AssertEquals("Local Amount", journal.AH_InvoiceAmount, journal.AH_OutstandingAmount);

			TxnHeader.OsInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(-200), ObjectCreator.GBP);
			TxnHeader.OsInvoiceAmtExclTax = TxnHeader.OsInvoiceAmtInclTax;
			TxnHeader.LocalInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(-100), ObjectCreator.GBP);
			TxnHeader.LocalInvoiceAmtExclTax = TxnHeader.LocalInvoiceAmtInclTax;

			Builder.SetValuesOnJournalBusinessObject(journal, TxnHeader, ImportContext, "Transaction JNL 00001000: ");

			AssertHeaderValues(journal, TxnHeader, 2M, "DR");
			AssertEquals("Local Amount", journal.AH_InvoiceAmount, journal.AH_OutstandingAmount);
			AssertEquals(ObjectCreator.ABIGAS.PK, journal.AH_OH);
		}

		public void TestSetGLAccount()
		{
			var testObjectCreater = new TestObjectCreator(Factory);
			AssertSetGLAccount(true);
			AssertSetGLAccount(false);

			void AssertSetGLAccount(bool isAR)
			{
				TxnHeader.Ledger = isAR ? Xsd.TxnLedgerType.AR : Xsd.TxnLedgerType.AP;
				var journal = isAR ? Factory.New<ARJournal>() : Factory.New<APJournal>() as Journal;
				var registryItem = isAR ? AccountingConfigurationRegistry.Instance.ARJournalAccount : AccountingConfigurationRegistry.Instance.APJournalAccount;
				journal.AH_AG = ZGuid.Empty;
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreater.GLHeader1.PK.ToGuid());
				AssertEquals("Precondition", ZGuid.Empty, journal.AH_AG);
				Builder.SetValuesOnJournalBusinessObject(journal, TxnHeader, ImportContext, "Transaction JNL 00001000: ");
				AssertEquals("AH_AG has the same value as registry", testObjectCreater.GLHeader1.PK, journal.AH_AG);

				var errorMsg = $"Error: Transaction JNL 00001000: No matches were found for the following GL Account: Accounting -> General Ledger Defaults -> Link Account -> {(isAR ? "AR" : "AP")} Journal Clearing Account";
				journal.AH_AG = ZGuid.Empty;
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreater.GLHeader2.PK.ToGuid());
				testObjectCreater.GLHeader2.Delete();
				Notify.Clear();
				TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, errorMsg);
				Builder.SetValuesOnJournalBusinessObject(journal, TxnHeader, ImportContext, "Transaction JNL 00001000: ");
				TestHelper.AssertNotificationsContainsErrorMessage(Notify, errorMsg);
				AssertEquals("AH_AG is still empty when registry value is invalid GL Account", Guid.Empty, journal.AH_AG);

				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
				Notify.Clear();
				TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, errorMsg);
				Builder.SetValuesOnJournalBusinessObject(journal, TxnHeader, ImportContext, "Transaction JNL 00001000: ");
				TestHelper.AssertNotificationsContainsErrorMessage(Notify, errorMsg);
				AssertEquals("AH_AG is still empty when registry value is empty", Guid.Empty, journal.AH_AG);
			}
		}

		public void TestFieldsAreSetCorrectlyOnJournal_SetDebtorOrCreditorGUID()
		{
			TxnHeader.Ledger = Xsd.TxnLedgerType.AR;
			TxnHeader.TxnType = Xsd.TxnType.JNL;
			OrganisationValueObjectDataAdapter organisationAdapter = new OrganisationValueObjectDataAdapter();
			TxnHeader.DebtorOrCreditor = organisationAdapter.ExportToValueObject(ObjectCreator.ABIGAS, new ValueObjectExportContext(new NotificationBuffer()));
			TxnHeader.InvoiceDate = PostDate.AddDays(-5);
			TxnHeader.PostDate = PostDate;
			TxnHeader.DueDate = PostDate.AddDays(1);
			TxnHeader.Description = "Software Rental";

			TxnHeader.GlAccount = "2010.00.00";
			TxnHeader.Branch = GlbBranch.CurrentBranch.GB_Code;
			TxnHeader.Department = GlbDepartment.CurrentDepartment.GE_Code;

			TxnHeader.OsInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(200), ObjectCreator.GBP);
			TxnHeader.OsInvoiceAmtExclTax = TxnHeader.OsInvoiceAmtInclTax;
			TxnHeader.LocalInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(100), ObjectCreator.GBP);
			TxnHeader.LocalInvoiceAmtExclTax = TxnHeader.LocalInvoiceAmtInclTax;
			TxnHeader.DebtorOrCreditorGUID = ObjectCreator.ABIGAS.PK.ToString();

			Journal journal = Factory.New<ARJournal>();
			Builder.SetValuesOnJournalBusinessObject(journal, TxnHeader, ImportContext, "Transaction JNL 00001000: ");

			AssertHeaderValues(journal, TxnHeader, 2M, "CR");

			AssertEquals("Local Amount", journal.AH_InvoiceAmount, journal.AH_OutstandingAmount);

			TxnHeader.OsInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(-200), ObjectCreator.GBP);
			TxnHeader.OsInvoiceAmtExclTax = TxnHeader.OsInvoiceAmtInclTax;
			TxnHeader.LocalInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(-100), ObjectCreator.GBP);
			TxnHeader.LocalInvoiceAmtExclTax = TxnHeader.LocalInvoiceAmtInclTax;

			Builder.SetValuesOnJournalBusinessObject(journal, TxnHeader, ImportContext, "Transaction JNL 00001000: ");

			AssertHeaderValues(journal, TxnHeader, 2M, "DR");
			AssertEquals("Local Amount", journal.AH_InvoiceAmount, journal.AH_OutstandingAmount);
			AssertEquals(ObjectCreator.ABIGAS.PK, journal.AH_OH);
		}

		public void TestSetValuesOnReceiptBusinessObject()
		{
			TxnHeader.Ledger = Xsd.TxnLedgerType.AR;
			TxnHeader.TxnType = Xsd.TxnType.REC;
			TxnHeader.InvoiceDate = ZDateTime.Now.AddDays(-5);
			TxnHeader.PostDate = ZDateTime.Now.AddDays(-4);
			TxnHeader.Branch = ObjectCreator.NonCurrentBranch.GB_Code;
			TxnHeader.Department = ObjectCreator.NonCurrentDepartment.GE_Code;
			TxnHeader.DebtorOrCreditor = new OrganisationValueObjectDataAdapter().ExportToValueObject(ObjectCreator.ABIGAS, new ValueObjectExportContext(new NotificationBuffer()));
			TxnHeader.Description = "Software Rental";
			TxnHeader.ReceiptPaymentType = Xsd.TxnHeaderReceiptPaymentType.CHQ;
			TxnHeader.OsInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(-200), ObjectCreator.USD);
			TxnHeader.LocalInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(-100), ObjectCreator.USD);
			TxnHeader.OsInvoiceAmtInclTax = TxnHeader.OsInvoiceAmtExclTax;
			TxnHeader.LocalInvoiceAmtInclTax = TxnHeader.LocalInvoiceAmtExclTax;
			TxnHeader.BankCode = ObjectCreator.AUDBankAccount.AB_Code;
			TxnHeader.ChequeBook = ObjectCreator.AUDChequeBook.AK_Code;
			TxnHeader.ChequeOrReference = "000003";
			TxnHeader.ChequeDrawer = "Eagle Datamation International";
			TxnHeader.DrawerBank = "ANZ";
			TxnHeader.DrawerBankBranch = "Alexandria 2015";
			TxnHeader.PaymentReceiptBatchDate = ZDateTime.Now.AddDays(-3);

			ARReceipt receipt = Factory.New<ARReceipt>();
			Builder.SetValuesOnReceiptBusinessObject(receipt, TxnHeader, ImportContext, "Transaction AR REC 00001000: ");

			AssertEquals(TxnHeader.InvoiceDate, receipt.AH_InvoiceDate);
			AssertEquals(TxnHeader.PostDate, receipt.AH_PostDate);
			AssertEquals(TxnHeader.Branch, receipt.Branch.GB_Code);
			AssertEquals(TxnHeader.Department, receipt.Department.GE_Code);
			AssertEquals(TxnHeader.DebtorOrCreditor.EDICode, receipt.Header.OH_Code);
			AssertEquals(TxnHeader.Description, receipt.AH_Desc);
			AssertEquals(ReceiptTypes.Cheque, receipt.AH_ReceiptType);
			AssertEquals(200M, receipt.AH_OSExTaxAmount);
			AssertEquals(100M, receipt.AH_LocalExTaxAmount);
			AssertEquals(2M, receipt.AH_ExchangeRate);
			AssertEquals(TxnHeader.BankCode, receipt.BankAccount.AB_Code);
			AssertEquals(TxnHeader.ChequeOrReference, receipt.AH_ChequeOrReference);
			AssertEquals(TxnHeader.ChequeDrawer, receipt.AH_ChequeDrawer);
			AssertEquals(TxnHeader.DrawerBank, receipt.AH_DrawerBank);
			AssertEquals(TxnHeader.DrawerBankBranch, receipt.AH_DrawerBranch);
			AssertEquals(TxnHeader.PaymentReceiptBatchDate, receipt.CompayReceiptBatchDate);
		}

		public void TestSetValuesOnPaymentBusinessObject()
		{
			TxnHeader.Ledger = Xsd.TxnLedgerType.AP;
			TxnHeader.TxnType = Xsd.TxnType.PAY;
			TxnHeader.InvoiceDate = ZDateTime.Now.AddDays(-5);
			TxnHeader.PostDate = ZDateTime.Now.AddDays(-4);
			TxnHeader.Branch = ObjectCreator.NonCurrentBranch.GB_Code;
			TxnHeader.Department = ObjectCreator.NonCurrentDepartment.GE_Code;
			TxnHeader.DebtorOrCreditor = new OrganisationValueObjectDataAdapter().ExportToValueObject(ObjectCreator.AALSHI, new ValueObjectExportContext(new NotificationBuffer()));
			TxnHeader.Description = "Software Rental";
			TxnHeader.ReceiptPaymentType = Xsd.TxnHeaderReceiptPaymentType.CHQ;
			TxnHeader.OsInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(200), ObjectCreator.USD);
			TxnHeader.LocalInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(100), ObjectCreator.USD);
			TxnHeader.OsInvoiceAmtInclTax = TxnHeader.OsInvoiceAmtExclTax;
			TxnHeader.LocalInvoiceAmtInclTax = TxnHeader.LocalInvoiceAmtExclTax;
			TxnHeader.BankCode = BankAccount.AB_Code;
			TxnHeader.ChequeBook = ObjectCreator.AUDChequeBook.AK_Code;
			TxnHeader.ChequeOrReference = "000004";
			TxnHeader.ChequeDrawer = "Eagle Datamation International";
			TxnHeader.DrawerBank = "ANZ";
			TxnHeader.DrawerBankBranch = "Alexandria 2015";

			APPayment payment = Factory.New<APPayment>();
			Builder.SetValuesOnPaymentBusinessObject(payment, TxnHeader, ImportContext, "Transaction AP PAY 00001000: ");

			AssertEquals(TxnHeader.InvoiceDate, payment.AH_InvoiceDate);
			AssertEquals(TxnHeader.PostDate, payment.AH_PostDate);
			AssertEquals(TxnHeader.Branch, payment.Branch.GB_Code);
			AssertEquals(TxnHeader.Department, payment.Department.GE_Code);
			AssertEquals(TxnHeader.DebtorOrCreditor.EDICode, payment.Header.OH_Code);
			AssertEquals(TxnHeader.Description, payment.AH_Desc);
			AssertEquals(ReceiptTypes.Cheque, payment.AH_ReceiptType);
			AssertEquals(200M, payment.AH_OSExTaxAmount);
			AssertEquals(100M, payment.AH_LocalExTaxAmount);
			AssertEquals(2M, payment.AH_ExchangeRate);
			AssertEquals(TxnHeader.BankCode, payment.BankAccount.AB_Code);
			AssertEquals(TxnHeader.ChequeBook, payment.AH_AKCode);
			AssertEquals(TxnHeader.ChequeOrReference, payment.AH_ChequeOrReference);
			AssertEquals("", payment.AH_ChequeDrawer);
			AssertEquals("", payment.AH_DrawerBank);
			AssertEquals("", payment.AH_DrawerBranch);

			//test error message
			const string errorMsg1 = "Error: Could not match Transaction override address.";
			const string errorMsg2 = "Error: Could not match Transaction override contact.";
			AssertEquals("", TxnHeader.TxnOverrideAddress.AddressCode);
			AssertEquals("", TxnHeader.TxnOverrideContact.Name);
			AssertEquals(ZGuid.Empty, payment.AH_OA_InvoiceAddressOverride);
			AssertEquals(ZGuid.Empty, payment.AH_OC_InvoiceContactOverride);
			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, errorMsg1);
			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, errorMsg2);

			TxnHeader.TxnOverrideAddress.AddressCode = "address123";
			TxnHeader.TxnOverrideContact.Name = "contact123";
			payment = Factory.New<APPayment>();
			Builder.SetValuesOnPaymentBusinessObject(payment, TxnHeader, ImportContext, "Transaction AP PAY 00001000: ");
			AssertEquals(ZGuid.Empty, payment.AH_OA_InvoiceAddressOverride);
			Assert(payment.AH_OC_InvoiceContactOverride.IsValid);
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, errorMsg1);
			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, errorMsg2);

			//existing address and contact
			Notify.Clear();
			var address = ObjectCreator.CreateAddress(ObjectCreator.AALSHI);
			var contact = ObjectCreator.CreateContact(ObjectCreator.AALSHI);

			TxnHeader.TxnOverrideAddress.AddressCode = address.OA_Code;
			TxnHeader.TxnOverrideContact.Name = contact.Name;

			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, errorMsg1);
			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, errorMsg2);
			payment = Factory.New<APPayment>();
			Builder.SetValuesOnPaymentBusinessObject(payment, TxnHeader, ImportContext, "Transaction AP PAY 00001000: ");
			AssertEquals(address.PK, payment.AH_OA_InvoiceAddressOverride);
			AssertEquals(contact.PK, payment.AH_OC_InvoiceContactOverride);
			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, errorMsg1);
			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, errorMsg2);

			//existing address and new contact
			Notify.Clear();
			TxnHeader.TxnOverrideContact.Name = "Meredydd Dimov"; //new name
			Assert(!ObjectCreator.AALSHI.Contacts.Cast<MasterFiles.Business.OrgContact>().Any(x => x.OC_ContactName == "Meredydd Dimov"));

			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, errorMsg1);
			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, errorMsg2);
			payment = Factory.New<APPayment>();
			Builder.SetValuesOnPaymentBusinessObject(payment, TxnHeader, ImportContext, "Transaction AP PAY 00001000: ");
			AssertEquals(address.PK, payment.AH_OA_InvoiceAddressOverride);
			Assert(payment.AH_OC_InvoiceContactOverride.IsValid);
			AssertEquals("Meredydd Dimov", payment.InvoiceContactOverride.OC_ContactName); //new contact created.
			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, errorMsg1);
			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, errorMsg2);
			Assert(ObjectCreator.AALSHI.Contacts.Cast<MasterFiles.Business.OrgContact>().Any(x => x.OC_ContactName == "Meredydd Dimov"));
		}

		public void TestSetValuesOnPaymentBusinessObject_OutstandingAmountInSyncWithInvoiceAmount()
		{
			TxnHeader.Ledger = TxnLedgerType.AP;
			TxnHeader.TxnType = TxnType.PAY;
			TxnHeader.OsInvoiceAmtExclTax = FinancialValue.FromAmountAndCurrency(new ZDecimal(60933.20), ObjectCreator.USD);
			TxnHeader.OsInvoiceAmtInclTax = TxnHeader.OsInvoiceAmtExclTax;
			TxnHeader.LocalInvoiceAmtExclTax = FinancialValue.FromAmountAndCurrency(new ZDecimal(1173515.12), ObjectCreator.USD);
			TxnHeader.LocalInvoiceAmtInclTax = TxnHeader.LocalInvoiceAmtExclTax;

			var payment = Factory.New<APPayment>();
			Builder.SetValuesOnPaymentBusinessObject(payment, TxnHeader, ImportContext, "Transaction AP PAY 00001000: ");

			AssertEquals("Local outstanding amount should be equal to local invoice amount", payment.AH_OutstandingAmount, payment.AH_InvoiceAmount);
			AssertEquals(Math.Sign(payment.AH_InvoiceAmount), Math.Sign(payment.AH_OSExTaxAmount));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAttachmentsAreHandledCorrectly()
		{
			TxnHeaderAttachment attachment = TxnHeader.Attachments.AddNew();
			attachment.FileName = "Test Doc.pdf";
			byte[] expectedContent = File.ReadAllBytes(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\TransactionHeaderBuilder\Testing\Test Attachment.pdf");
			attachment.Data = expectedContent;

			ARInvoice invoice = Factory.New<ARInvoice>();
			Builder.SetValuesOnInvoiceBusinessObject(invoice, TxnHeader, ImportContext, "Transaction AR INV 00001000: ");

			AssertEquals("eDoc should be added", 1, invoice.DocManagerInfo.AllEDocs.Count);
			AssertEquals("File name", "Test Doc.pdf", invoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertEquals("Content length", expectedContent.Length, invoice.DocManagerInfo.AllEDocs[0].ImageData.Length);

			AssertArrayEqualsByElements(expectedContent, invoice.DocManagerInfo.AllEDocs[0].ImageData);
		}

		[ExpectNoExceptions]
		public void TestEmptyDataAttachmentsAreHandledCorrectly()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			TxnHeader.DebtorOrCreditor.EDICode = testObjectCreator.AALSHI.OH_Code;
			TxnHeaderAttachment attachment = TxnHeader.Attachments.AddNew();
			attachment.FileName = "Invoice 1000.pdf";
			attachment.Data = Array.Empty<byte>();

			ARInvoice invoice = Factory.New<ARInvoice>();
			Builder.SetValuesOnInvoiceBusinessObject(invoice, TxnHeader, ImportContext, "Transaction AR INV 00001000: ");

			AssertEquals("No eDoc should be added", 0, invoice.DocManagerInfo.AllEDocs.Count);
			TestHelper.AssertNotificationsContainsErrorMessageOnce(Notify, "Could not find file 'Invoice 1000.pdf'.");
		}

		[ExpectNoExceptions]
		public void TestEmptyAttachmentsAreHandledCorrectly()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			TxnHeader.DebtorOrCreditor.EDICode = testObjectCreator.AALSHI.OH_Code;
			TxnHeaderAttachment attachment = TxnHeader.Attachments.AddNew();
			attachment.Data = Array.Empty<byte>();

			ARInvoice invoice = Factory.New<ARInvoice>();
			Builder.SetValuesOnInvoiceBusinessObject(invoice, TxnHeader, ImportContext, "Transaction AR INV 00001000: ");

			AssertEquals("No eDoc should be added", 0, invoice.DocManagerInfo.AllEDocs.Count);
			TestHelper.AssertNotificationsContainsNoErrors(Notify);
		}

		[ExpectNoExceptions]
		public void TestEmptyFileAttachmentsAreHandledCorrectly()
		{
			var tempFileName = Env.GetTempFileName(Env.TempPath, "pdf");

			try
			{
				var attachment = TxnHeader.Attachments.AddNew();
				attachment.FileName = "Test Doc.pdf";
				attachment.FilePath = tempFileName;

				var invoice = Factory.New<ARInvoice>();
				Builder.SetValuesOnInvoiceBusinessObject(invoice, TxnHeader, ImportContext, "Transaction AR INV 00001000: ");

				AssertEquals("No eDoc should be added", 0, invoice.DocManagerInfo.AllEDocs.Count);
				TestHelper.AssertNotificationsContainsErrorMessageOnce(Notify, $"File '{tempFileName}' is empty.");
			}
			finally
			{
				File.Delete(tempFileName);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAttachmentsAreHandledCorrectlyForCSV()
		{
			TxnHeaderAttachment attachment = TxnHeader.Attachments.AddNew();
			attachment.FileName = "Test Doc.pdf";
			attachment.FilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\TransactionHeaderBuilder\Testing\Test Attachment.pdf";
			attachment.DocumentType = Core.Constants.RefDocTypes.AgentsInvoice;

			ARInvoice invoice = Factory.New<ARInvoice>();
			Builder.SetValuesOnInvoiceBusinessObject(invoice, TxnHeader, ImportContext, "Transaction AR INV 00001000: ");

			byte[] expectedContent = File.ReadAllBytes(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\TransactionHeaderBuilder\Testing\Test Attachment.pdf");
			AssertEquals("eDoc should be added", 1, invoice.DocManagerInfo.AllEDocs.Count);
			AssertEquals("File name", "Test Doc.pdf", invoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertEquals("Document Type", Core.Constants.RefDocTypes.AgentsInvoice, invoice.DocManagerInfo.AllEDocs[0].DocType);
			AssertEquals("Content length", expectedContent.Length, invoice.DocManagerInfo.AllEDocs[0].ImageData.Length);

			AssertArrayEqualsByElements(expectedContent, invoice.DocManagerInfo.AllEDocs[0].ImageData);

			attachment.FileName = ZString.Empty;
			attachment.DocumentType = ZString.Empty;

			invoice = Factory.New<ARInvoice>();
			Builder.SetValuesOnInvoiceBusinessObject(invoice, TxnHeader, ImportContext, "Transaction AR INV 00001000: ");

			AssertEquals("eDoc should be added", 1, invoice.DocManagerInfo.AllEDocs.Count);
			AssertEquals("File name", "Test Attachment.pdf", invoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertEquals("Document Type", Core.Constants.RefDocTypes.MiscellaneousDocument, invoice.DocManagerInfo.AllEDocs[0].DocType);
			AssertEquals("Content length", expectedContent.Length, invoice.DocManagerInfo.AllEDocs[0].ImageData.Length);

			AssertArrayEqualsByElements(expectedContent, invoice.DocManagerInfo.AllEDocs[0].ImageData);

			attachment.FilePath = "X:\\Test.txt";

			invoice = Factory.New<ARInvoice>();
			Builder.SetValuesOnInvoiceBusinessObject(invoice, TxnHeader, ImportContext, "Transaction AR INV 00001000: ");

			AssertEquals("eDoc should be added", 0, invoice.DocManagerInfo.AllEDocs.Count);
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, "Could not find file 'X:\\Test.txt'.");

			attachment.FilePath = "X:\\Test0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.txt";

			invoice = Factory.New<ARInvoice>();
			Builder.SetValuesOnInvoiceBusinessObject(invoice, TxnHeader, ImportContext, "Transaction AR INV 00001000: ");

			AssertEquals("eDoc should be added", 0, invoice.DocManagerInfo.AllEDocs.Count);
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, string.Format("The specified path is too long. It must be less than 260 characters. Path: '{0}'", attachment.FilePath));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAttachmentsUsingFileNameAreSavedWhenTransactionIsSaved()
		{
			var attachment = TxnHeader.Attachments.AddNew();
			attachment.FileName = "Test Doc.pdf";
			attachment.FilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\TransactionHeaderBuilder\Testing\Test Attachment.pdf";
			attachment.DocumentType = Core.Constants.RefDocTypes.AgentsInvoice;

			var invoice = Factory.New<ARInvoice>();
			Builder.SetValuesOnInvoiceBusinessObject(invoice, TxnHeader, ImportContext, "Transaction AR INV 00001000: ");
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var invoiceReloaded = newFactory.Load<ARInvoice>(invoice.PK);

			var expectedContent = File.ReadAllBytes(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\TransactionHeaderBuilder\Testing\Test Attachment.pdf");
			AssertEquals("eDoc should be added", 1, invoiceReloaded.DocManagerInfo.AllEDocs.Count);
			AssertEquals("File name", "Test Doc.pdf", invoiceReloaded.DocManagerInfo.AllEDocs[0].FileName);
			AssertEquals("Document Type", Core.Constants.RefDocTypes.AgentsInvoice, invoiceReloaded.DocManagerInfo.AllEDocs[0].DocType);
			AssertEquals("Content length", expectedContent.Length, invoiceReloaded.DocManagerInfo.AllEDocs[0].ImageData.Length);

			AssertArrayEqualsByElements(expectedContent, invoice.DocManagerInfo.AllEDocs[0].ImageData);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAttachmentsUsingDataAreSavedWhenTransactionIsSaved()
		{
			var expectedContent = File.ReadAllBytes(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\TransactionHeaderBuilder\Testing\Test Attachment.pdf");

			var attachment = TxnHeader.Attachments.AddNew();
			attachment.FileName = "Test Doc.pdf";
			attachment.DocumentType = Core.Constants.RefDocTypes.AgentsInvoice;
			attachment.Data = expectedContent;

			var invoice = Factory.New<ARInvoice>();
			Builder.SetValuesOnInvoiceBusinessObject(invoice, TxnHeader, ImportContext, "Transaction AR INV 00001000: ");
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var invoiceReloaded = newFactory.Load<ARInvoice>(invoice.PK);

			AssertEquals("eDoc should be added", 1, invoiceReloaded.DocManagerInfo.AllEDocs.Count);
			AssertEquals("File name", "Test Doc.pdf", invoiceReloaded.DocManagerInfo.AllEDocs[0].FileName);
			AssertEquals("Document Type", Core.Constants.RefDocTypes.AgentsInvoice, invoiceReloaded.DocManagerInfo.AllEDocs[0].DocType);
			AssertEquals("Content length", expectedContent.Length, invoiceReloaded.DocManagerInfo.AllEDocs[0].ImageData.Length);

			AssertArrayEqualsByElements(expectedContent, invoice.DocManagerInfo.AllEDocs[0].ImageData);
		}

		public void TestAPInvoiceWithNoPayment()
		{
			APInvoice aPInvoice = Invoice as APInvoice;
			Builder.SetValuesOnInvoiceBusinessObject(aPInvoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			AssertCashReceiptPaymentDetails(aPInvoice, false, "", Guid.Empty, Guid.Empty, "", "", "", "", false);
		}

		public void TestAPInvoiceWithEFTPayment_ValidChequeReference()
		{
			APInvoice aPInvoice = Invoice as APInvoice;
			PopulateTxnHeaderForCashPayment(Xsd.TxnHeaderReceiptPaymentType.EFT, BankAccount.AB_Code, "", "12345");
			Builder.SetValuesOnInvoiceBusinessObject(aPInvoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			AssertCashReceiptPaymentDetails(aPInvoice, true, ReceiptTypes.EFT, BankAccount.PK, Guid.Empty, "12345", "", "", "", false);
		}

		public void TestAPInvoiceWithEFTPayment_NoChequeReference()
		{
			APInvoice aPInvoice = Invoice as APInvoice;
			PopulateTxnHeaderForCashPayment(Xsd.TxnHeaderReceiptPaymentType.EFT, BankAccount.AB_Code, "", "");
			Builder.SetValuesOnInvoiceBusinessObject(aPInvoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			AssertCashReceiptPaymentDetails(aPInvoice, true, ReceiptTypes.EFT, BankAccount.PK, Guid.Empty, "00001000", "", "", "", false);
		}

		public void TestAPInvoiceWithSFTPayment_ValidChequeReference()
		{
			APInvoice aPInvoice = Invoice as APInvoice;
			PopulateTxnHeaderForCashPayment(Xsd.TxnHeaderReceiptPaymentType.SFT, BankAccount.AB_Code, "", "12345");
			Builder.SetValuesOnInvoiceBusinessObject(aPInvoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			AssertCashReceiptPaymentDetails(aPInvoice, true, ReceiptTypes.ScheduledEFT, BankAccount.PK, Guid.Empty, "12345", "", "", "", false);
		}

		public void TestAPInvoiceWithSFTPayment_NoChequeReference()
		{
			APInvoice aPInvoice = Invoice as APInvoice;
			PopulateTxnHeaderForCashPayment(Xsd.TxnHeaderReceiptPaymentType.SFT, BankAccount.AB_Code, "", "");
			Builder.SetValuesOnInvoiceBusinessObject(aPInvoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			AssertCashReceiptPaymentDetails(aPInvoice, true, ReceiptTypes.ScheduledEFT, BankAccount.PK, Guid.Empty, "00001000", "", "", "", false);
		}

		public void TestAPInvoiceWithCRQPayment_ValidChequeReference()
		{
			APInvoice aPInvoice = Invoice as APInvoice;
			PopulateTxnHeaderForCashPayment(Xsd.TxnHeaderReceiptPaymentType.CRQ, BankAccount.AB_Code, "", "12345");
			Builder.SetValuesOnInvoiceBusinessObject(aPInvoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			AssertCashReceiptPaymentDetails(aPInvoice, true, ReceiptTypes.CollectionRequest, BankAccount.PK, Guid.Empty, "12345", "", "", "", false);
		}

		public void TestAPInvoiceWithCRQPayment_NoChequeReference()
		{
			APInvoice aPInvoice = Invoice as APInvoice;
			PopulateTxnHeaderForCashPayment(Xsd.TxnHeaderReceiptPaymentType.CRQ, BankAccount.AB_Code, "", "");
			Builder.SetValuesOnInvoiceBusinessObject(aPInvoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			AssertCashReceiptPaymentDetails(aPInvoice, true, ReceiptTypes.CollectionRequest, BankAccount.PK, Guid.Empty, "00001000", "", "", "", false);
		}

		public void TestAPInvoiceWithCHQPayment_ManualPrintChequeBookWithNoChequeNumber()
		{
			ChequeBook.AK_AutoPrintCheque = false;
			APInvoice aPInvoice = Invoice as APInvoice;

			PopulateTxnHeaderForCashPayment(Xsd.TxnHeaderReceiptPaymentType.CHQ, BankAccount.AB_Code, ChequeBook.AK_Code, "");
			Builder.SetValuesOnInvoiceBusinessObject(aPInvoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");

			// Cheque Number should be defaulted from current Number
			AssertCashReceiptPaymentDetails(aPInvoice, true, ReceiptTypes.Cheque, BankAccount.PK, ChequeBook.PK, "000035", "", "", "", false);

			if (InvoiceWithPaymentOrReceiptShouldBeCreated)
			{
				aPInvoice.Validation.ValidateAll();
				AssertNoErrors(aPInvoice.ReceiptPaymentAH_ChequeOrReferenceInfo);
			}
		}

		public void TestAPInvoiceWithCHQPayment_ManualPrintChequeBookWithValidChequeNumber()
		{
			ChequeBook.AK_AutoPrintCheque = false;
			APInvoice aPInvoice = Invoice as APInvoice;

			PopulateTxnHeaderForCashPayment(Xsd.TxnHeaderReceiptPaymentType.CHQ, BankAccount.AB_Code, ChequeBook.AK_Code, "50");
			Builder.SetValuesOnInvoiceBusinessObject(aPInvoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");

			AssertCashReceiptPaymentDetails(aPInvoice, true, ReceiptTypes.Cheque, BankAccount.PK, ChequeBook.PK, "000050", "", "", "", false);

			if (InvoiceWithPaymentOrReceiptShouldBeCreated)
			{
				aPInvoice.Validation.ValidateAll();
				AssertNoErrors(aPInvoice.ReceiptPaymentAH_ChequeOrReferenceInfo);
			}
		}

		public void TestAPInvoiceWithCHQPayment_NotAutoPrintChequeBookWithInvalidChequeNumber()
		{
			ChequeBook.AK_AutoPrintCheque = false;
			APInvoice aPInvoice = Invoice as APInvoice;

			PopulateTxnHeaderForCashPayment(Xsd.TxnHeaderReceiptPaymentType.CHQ, BankAccount.AB_Code, ChequeBook.AK_Code, "999999");
			Builder.SetValuesOnInvoiceBusinessObject(aPInvoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");

			AssertCashReceiptPaymentDetails(aPInvoice, true, ReceiptTypes.Cheque, BankAccount.PK, ChequeBook.PK, "999999", "", "", "", false);

			if (InvoiceWithPaymentOrReceiptShouldBeCreated)
			{
				aPInvoice.Validation.ValidateAll();
				AssertHasErrors(aPInvoice.ReceiptPaymentAH_ChequeOrReferenceInfo);
			}
		}

		public void TestAPInvoiceWithCHQPayment_AutoPrintChequeBook()
		{
			ChequeBook.AK_AutoPrintCheque = true;
			APInvoice aPInvoice = Invoice as APInvoice;

			PopulateTxnHeaderForCashPayment(Xsd.TxnHeaderReceiptPaymentType.CHQ, BankAccount.AB_Code, ChequeBook.AK_Code, "12345");
			Builder.SetValuesOnInvoiceBusinessObject(aPInvoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");

			// Cheque Number from XML is ignored and left blank because it will be assigned when the cheque is printed
			AssertCashReceiptPaymentDetails(aPInvoice, true, ReceiptTypes.Cheque, BankAccount.PK, ChequeBook.PK, "", "", "", "", false);

			if (InvoiceWithPaymentOrReceiptShouldBeCreated)
			{
				aPInvoice.Validation.ValidateAll();
				AssertNoErrors(aPInvoice.ReceiptPaymentAH_ChequeOrReferenceInfo);
			}
		}

		public void TestAPInvoiceWithCHQPayment_HotChequeWithExactAmount()
		{
			APInvoice aPInvoice = Invoice as APInvoice;
			AccHotCheque hotCheque = CreateHotCheque(ObjectCreator.AALSHI, "000045", "ACT", 150m);

			PopulateTxnHeaderForCashPayment(Xsd.TxnHeaderReceiptPaymentType.CHQ, BankAccount.AB_Code, ChequeBook.AK_Code, "45");
			Builder.SetValuesOnInvoiceBusinessObject(aPInvoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");

			// Cheque Number from XML should be set from the Hot Cheque
			AssertCashReceiptPaymentDetails(aPInvoice, true, ReceiptTypes.Cheque, BankAccount.PK, ChequeBook.PK, "000045", "", "", "", true);

			if (InvoiceWithPaymentOrReceiptShouldBeCreated)
			{
				APInvoiceLine aPInvoiceLine1 = (APInvoiceLine)aPInvoice.Lines.AddNew();
				aPInvoiceLine1.AL_OSExTaxAmount = 200m;
				aPInvoice.Validation.ValidateAll();
				AssertHasErrors(aPInvoice.AH_OSTotalAmountInfo);

				aPInvoiceLine1.AL_OSExTaxAmount = 150m;
				aPInvoice.Validation.ValidateAll();
				AssertNoErrors(aPInvoice.AH_OSTotalAmountInfo);
			}
		}

		public void TestAPInvoiceWithCHQPayment_HotChequeWithMaximumAmount()
		{
			APInvoice aPInvoice = Invoice as APInvoice;
			PopulateTxnHeaderForCashPayment(Xsd.TxnHeaderReceiptPaymentType.CHQ, BankAccount.AB_Code, ChequeBook.AK_Code, "45");
			AccHotCheque hotCheque = CreateHotCheque(ObjectCreator.AALSHI, "000045", "MAX", 150m);

			Builder.SetValuesOnInvoiceBusinessObject(aPInvoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			// Cheque Number from XML should be set from the Hot Cheque
			AssertCashReceiptPaymentDetails(aPInvoice, true, ReceiptTypes.Cheque, BankAccount.PK, ChequeBook.PK, "000045", "", "", "", true);

			if (InvoiceWithPaymentOrReceiptShouldBeCreated)
			{
				APInvoiceLine aPInvoiceLine1 = (APInvoiceLine)aPInvoice.Lines.AddNew();
				aPInvoiceLine1.AL_OSExTaxAmount = 200m;
				aPInvoice.Validation.ValidateAll();
				AssertHasErrors(aPInvoice.AH_OSTotalAmountInfo);

				aPInvoiceLine1.AL_OSExTaxAmount = 100m;
				aPInvoice.Validation.ValidateAll();
				AssertNoErrors(aPInvoice.AH_OSTotalAmountInfo);
			}
		}

		public void TestARInvoiceWithNoReceipt()
		{
			ARInvoice aRInvoice = Factory.New<ARInvoice>();
			aRInvoice.SubmittedFromInvoicingForm = true;
			Builder.SetValuesOnInvoiceBusinessObject(aRInvoice, TxnHeader, ImportContext, "Transaction AR INV 00001000: ");
			AssertCashReceiptPaymentDetails(aRInvoice, false, "", Guid.Empty, Guid.Empty, "", "", "", "", false);
		}

		public void TestARInvoiceWithEFTReceipt_ValidChequeReference()
		{
			ARInvoice aRInvoice = Factory.New<ARInvoice>();
			aRInvoice.SubmittedFromInvoicingForm = true;
			PopulateTxnHeaderForCashReceipt(Xsd.TxnHeaderReceiptPaymentType.EFT, BankAccount.AB_Code, "12345", "Cheque Drawer", "Cheque Drawer Bank", "Cheque Drawer Bank Branch");
			Builder.SetValuesOnInvoiceBusinessObject(aRInvoice, TxnHeader, ImportContext, "Transaction AR INV 00001000: ");
			AssertCashReceiptPaymentDetails(aRInvoice, true, ReceiptTypes.EFT, BankAccount.PK, Guid.Empty, "12345", "", "", "", false);
		}

		public void TestARInvoiceWithEFTReceipt_NoChequeReference()
		{
			ARInvoice aRInvoice = Factory.New<ARInvoice>();
			aRInvoice.SubmittedFromInvoicingForm = true;
			PopulateTxnHeaderForCashReceipt(Xsd.TxnHeaderReceiptPaymentType.EFT, BankAccount.AB_Code, "", "Cheque Drawer", "Cheque Drawer Bank", "Cheque Drawer Bank Branch");
			Builder.SetValuesOnInvoiceBusinessObject(aRInvoice, TxnHeader, ImportContext, "Transaction AR INV 00001000: ");
			AssertCashReceiptPaymentDetails(aRInvoice, true, ReceiptTypes.EFT, BankAccount.PK, Guid.Empty, "00001000", "", "", "", false);
		}

		public void TestARInvoiceWithCHQReceipt()
		{
			ARInvoice aRInvoice = Factory.New<ARInvoice>();
			aRInvoice.SubmittedFromInvoicingForm = true;
			PopulateTxnHeaderForCashReceipt(Xsd.TxnHeaderReceiptPaymentType.CHQ, BankAccount.AB_Code, "12345", "Cheque Drawer", "Cheque Drawer Bank", "Cheque Drawer Bank Branch");
			Builder.SetValuesOnInvoiceBusinessObject(aRInvoice, TxnHeader, ImportContext, "Transaction AR INV 00001000: ");
			AssertCashReceiptPaymentDetails(aRInvoice, true, ReceiptTypes.Cheque, BankAccount.PK, Guid.Empty, "12345", "Cheque Drawer", "Cheque Drawer Bank", "Cheque Drawer Bank Branch", false);
		}

		public void TestBranchBehaviourWhenBranchIsSetCorrectly()
		{
			GlbBranch branch1 = ObjectCreator.CreateBranch("111", "Branch1", GlbCompany.CurrentCompany);
			GlbBranch branch2 = ObjectCreator.CreateBranch("222", "Branch2", GlbCompany.CurrentCompany);
			TxnHeader.Branch = "111";
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, ZString.Empty);
			AssertEquals("Branch", branch1.GB_Code, Invoice.Branch.GB_Code);
			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, "No matches were found for the following Branch: 111");
		}

		public void TestBranchBehaviourWhenBranchIsSetIncorrectly()
		{
			GlbBranch branch1 = ObjectCreator.CreateBranch("111", "Branch1", GlbCompany.CurrentCompany);
			GlbBranch branch2 = ObjectCreator.CreateBranch("222", "Branch2", GlbCompany.CurrentCompany);
			TxnHeader.Branch = "XXX";
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, ZString.Empty);
			AssertEquals("Branch", GlbBranch.CurrentBranch.GB_Code, Invoice.Branch.GB_Code);
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, "No matches were found for the following Branch: XXX");
		}

		public void TestBranchBehaviourWhenBranchIsEmptyForInteractiveUser()
		{
			GlbBranch branch1 = ObjectCreator.CreateBranch("111", "Branch1", GlbCompany.CurrentCompany);
			GlbBranch branch2 = ObjectCreator.CreateBranch("222", "Branch2", GlbCompany.CurrentCompany);
			TxnHeader.Branch = "";
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, ZString.Empty);
			AssertEquals("Branch", GlbBranch.CurrentBranch.GB_Code, Invoice.Branch.GB_Code);
			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, "No matches were found for the following Branch: 111");
		}

		public void TestBranchBehaviourWhenBranchIsEmptyForBatchProcessorWhenAllLinesHaveDifferentBranchs()
		{
			using (new TemporaryUserContext() { StaffLoginName = User.ServiceUserName }.Set())
			{
				GlbBranch branch1 = ObjectCreator.CreateBranch("111", "Branch1", GlbCompany.CurrentCompany);
				GlbBranch branch2 = ObjectCreator.CreateBranch("222", "Branch2", GlbCompany.CurrentCompany);

				TxnLine txnLine1 = TxnHeader.TxnLines.AddNew();
				txnLine1.Branch = branch1.GB_Code;

				TxnLine txnLine2 = TxnHeader.TxnLines.AddNew();
				txnLine2.Branch = branch2.GB_Code;

				TxnHeader.Branch = "";

				Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "AP INV 00001000");
				AssertEquals("Branch", GlbBranch.CurrentBranch.GB_Code, Invoice.Branch.GB_Code);
				TestHelper.AssertNotificationsContainsErrorMessage(Notify, "Error: AP INV 00001000: Header branch was not specified and could not be defaulted from lines because the lines cover multiple branches. Please specify a branch for the header.");
			}
		}

		public void TestBranchBehaviourWhenBranchIsEmptyForBatchProcessorWhenAllLinesHaveSameBranchs()
		{
			using (new TemporaryUserContext() { StaffLoginName = User.ServiceUserName }.Set())
			{
				GlbBranch branch1 = ObjectCreator.CreateBranch("111", "Branch1", GlbCompany.CurrentCompany);
				GlbBranch branch2 = ObjectCreator.CreateBranch("222", "Branch2", GlbCompany.CurrentCompany);

				TxnLine txnLine1 = TxnHeader.TxnLines.AddNew();
				txnLine1.Branch = branch1.GB_Code;

				TxnLine txnLine2 = TxnHeader.TxnLines.AddNew();
				txnLine2.Branch = branch1.GB_Code;

				TxnHeader.Branch = "";

				Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "AP INV 00001000");
				AssertEquals("Branch", branch1.GB_Code, Invoice.Branch.GB_Code);
				TestHelper.AssertNotificationsContainsErrorMessage(Notify, "AP INV 00001000: Header branch has been set to '111' because a header branch was not specified and all lines have the same branch.");
			}
		}

		public void TestDepartmentBehaviourWhenDepartmentIsSetCorrectly()
		{
			GlbDepartment department1 = Factory.LoadFromUniqueKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, new ZString("FEA"));
			GlbDepartment department2 = Factory.LoadFromUniqueKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, new ZString("FES"));
			TxnHeader.Department = "FES";
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, ZString.Empty);
			AssertEquals("Department", department2.GE_Code, Invoice.Department.GE_Code);
			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, "No matches were found for the following Department: 111");
		}

		public void TestDepartmentBehaviourWhenDepartmentIsSetIncorrectly()
		{
			GlbDepartment department1 = Factory.LoadFromUniqueKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, new ZString("FEA"));
			GlbDepartment department2 = Factory.LoadFromUniqueKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, new ZString("FES"));
			TxnHeader.Department = "YYY";
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, ZString.Empty);
			AssertEquals("Department", GlbDepartment.CurrentDepartment.GE_Code, Invoice.Department.GE_Code);
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, "No matches were found for the following Department: YYY");
		}

		public void TestDepartmentBehaviourWhenDepartmentIsEmptyForInteractiveUser()
		{
			GlbDepartment department1 = Factory.LoadFromUniqueKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, new ZString("FEA"));
			GlbDepartment department2 = Factory.LoadFromUniqueKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, new ZString("FES"));
			TxnHeader.Department = "";
			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, ZString.Empty);
			AssertEquals("Department", GlbDepartment.CurrentDepartment.GE_Code, Invoice.Department.GE_Code);
			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, "No matches were found for the following Department: 111");

			TxnLine txnLine1 = TxnHeader.TxnLines.AddNew();
			txnLine1.Department = department2.GE_Code;

			TxnLine txnLine2 = TxnHeader.TxnLines.AddNew();
			txnLine2.Department = department2.GE_Code;

			Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, ZString.Empty);
			AssertEquals("Department", GlbDepartment.CurrentDepartment.GE_Code, Invoice.Department.GE_Code);
			TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, "No matches were found for the following Department: 111");
		}

		public void TestDepartmentBehaviourWhenDepartmentIsEmptyForBatchProcessorWhenAllLinesHaveDifferentDepartments()
		{
			using (new TemporaryUserContext() { StaffLoginName = User.ServiceUserName }.Set())
			{
				GlbDepartment department1 = Factory.LoadFromUniqueKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, new ZString("FEA"));
				GlbDepartment department2 = Factory.LoadFromUniqueKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, new ZString("FES"));

				TxnLine txnLine1 = TxnHeader.TxnLines.AddNew();
				txnLine1.Department = department1.GE_Code;

				TxnLine txnLine2 = TxnHeader.TxnLines.AddNew();
				txnLine2.Department = department2.GE_Code;

				TxnHeader.Department = "";

				Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "AP INV 00001000");
				AssertEquals("Department", GlbDepartment.CurrentDepartment.GE_Code, Invoice.Department.GE_Code);
				TestHelper.AssertNotificationsContainsErrorMessage(Notify, "AP INV 00001000: Header department was not specified and could not be defaulted from lines because the lines cover multiple departments. Please specify a department for the header.");
			}
		}

		public void TestDepartmentBehaviourWhenDepartmentIsEmptyForBatchProcessorWhenAllLinesHaveSameDepartments()
		{
			using (new TemporaryUserContext() { StaffLoginName = User.ServiceUserName }.Set())
			{
				GlbDepartment department1 = Factory.LoadFromUniqueKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, new ZString("FEA"));
				GlbDepartment department2 = Factory.LoadFromUniqueKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, new ZString("FES"));

				TxnLine txnLine1 = TxnHeader.TxnLines.AddNew();
				txnLine1.Department = department1.GE_Code;

				TxnLine txnLine2 = TxnHeader.TxnLines.AddNew();
				txnLine2.Department = department1.GE_Code;

				TxnHeader.Department = "";

				Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "AP INV 00001000");
				AssertEquals("Department", department1.GE_Code, Invoice.Department.GE_Code);
				TestHelper.AssertNotificationsContainsErrorMessage(Notify, "AP INV 00001000: Header department has been set to 'FEA' because a header department was not specified and all lines have the same department");
			}
		}

		[TestDate(2024, 10, 7)]
		public void TestBehaviour_AR_WhenComplianceSubTypeRulesAreNotSetAndComplianceNumberAllocationDateIsSetToInvoiceDate()
		{
			AssertBehaviour_AR_WhenComplianceSubTypeRulesAreNotSetAndComplianceNumberAllocationDateIsSet(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		[TestDate(2024, 10, 7)]
		public void TestBehaviour_AR_WhenComplianceSubTypeRulesAreNotSetAndComplianceNumberAllocationDateIsSetToPostDate()
		{
			AssertBehaviour_AR_WhenComplianceSubTypeRulesAreNotSetAndComplianceNumberAllocationDateIsSet(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		void AssertBehaviour_AR_WhenComplianceSubTypeRulesAreNotSetAndComplianceNumberAllocationDateIsSet(string complianceNumberAllocationDateCode)
		{
			var currComp = GlbCompany.CurrentCompany;
			using (currComp.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				currComp.OrgProxy.OH_IsDebtor = true;
				GlbBranch.CurrentBranch.OrgProxy.Factory.Save();

				const string subType = "ARI";

				var sequence = ObjectCreator.CreateNewComplianceSequence(ZGuid.Empty, subType, 1, 99, 25);
				sequence.XD_Prefix = $"{subType}-";
				sequence.XD_GC_Company = currComp.PK;
				sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
				Factory.Save();

				var emptyCollection = new ComplianceSubTypeAttributionRuleConfigurationCollection();

				Invoice = ObjectCreator.CreateInvoice(typeof(ARInvoice));

				var registry = AccountingMasterFilesRegistry.Instance;
				using (registry.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(currComp.PK.ToGuid(), Guid.Empty, Guid.Empty, emptyCollection))
				using (registry.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(currComp.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(currComp.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceNumberAllocationDateCode))
				{
					var today = ZDate.Today;

					TxnHeader.Ledger = TxnLedgerType.AR;
					TxnHeader.DebtorOrCreditor = new Organisation { EDICode = currComp.OrgProxy.OH_Code };
					TxnHeader.InvoiceDate = today;
					TxnHeader.PostDate = today;
					TxnHeader.OsInvoiceAmtInclTax = FinancialValue.FromAmountAndCurrencyCode(new ZDecimal(1162.42), "EUR");

					TxnHeader.TxnLines = new TxnLineCollection();
					var invoiceLine1 = TxnHeader.TxnLines.AddNew();
					invoiceLine1.ChargeCode = ObjectCreator.CC1.AC_Code;
					invoiceLine1.Branch = GlbBranch.CurrentBranch.GB_Code;
					invoiceLine1.Department = GlbDepartment.CurrentDepartment.GE_Code;
					invoiceLine1.LineType = TxnLineLineType.REV;
					invoiceLine1.TaxCode = "FREEIVA";
					invoiceLine1.OsInvoiceAmtExclTax = FinancialValue.FromAmountAndCurrencyCode(new ZDecimal(1162.42), "EUR");

					Assert("Precondition: Country supports compliance sub-type", currComp.Country.SupportComplianceSubType);
					Assert("Precondition: Invoice is not saved in the database", !Invoice.IsInDatabase);

					var value = registry.ComplianceSubTypeAttributionRuleConfiguration.Value;
					Assert("Precondition: Compliance sub-type rules are not defined", value.IsNullOrEmpty());

					Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AR INV 00001000: ");

					AssertNullOrEmpty("Compliance sub-type", Invoice.AH_ComplianceSubType);
					AssertNull("Compliance sequence", Invoice.ComplianceSequenceFromSubType);
					AssertHasRowErrorContaining(Invoice, ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage);
				}
			}
		}

		[TestDate(2024, 10, 7)]
		public void TestBehaviour_AR_WhenComplianceSubTypeRulesAreSetAndComplianceNumberAllocationDateIsSetToInvoiceDate()
		{
			AssertBehaviour_AR_WhenComplianceSubTypeRulesAreSetAndComplianceNumberAllocationDateIsSet(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		[TestDate(2024, 10, 7)]
		public void TestBehaviour_AR_WhenComplianceSubTypeRulesAreSetAndComplianceNumberAllocationDateIsSetToPostDate()
		{
			AssertBehaviour_AR_WhenComplianceSubTypeRulesAreSetAndComplianceNumberAllocationDateIsSet(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		[TestDate(2024, 10, 7)]
		public void TestBehaviour_AR_WhenComplianceSubTypeRulesAreSetAndComplianceNumberAllocationDateIsSetToNoControl()
		{
			AssertBehaviour_AR_WhenComplianceSubTypeRulesAreSetAndComplianceNumberAllocationDateIsSet(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code);
		}

		void AssertBehaviour_AR_WhenComplianceSubTypeRulesAreSetAndComplianceNumberAllocationDateIsSet(string complianceNumberAllocationDateCode)
		{
			var currComp = GlbCompany.CurrentCompany;
			using (currComp.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				currComp.OrgProxy.OH_IsDebtor = true;
				GlbBranch.CurrentBranch.OrgProxy.Factory.Save();

				const string subType = "ARI";

				var sequence = ObjectCreator.CreateNewComplianceSequence(ZGuid.Empty, subType, 1, 99, 25);
				sequence.XD_Prefix = $"{subType}-";
				sequence.XD_GC_Company = currComp.PK;
				sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
				Factory.Save();

				var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
				var config = collection.AddNew();
				config.Country = currComp.Country.Code;
				config.SubType = subType;
				config.LedgerType = LedgerTypes.AccountsReceivable;
				config.InvoiceType = TransactionTypes.Invoice;
				config.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				config.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				config.OriginalRule = OriginalRuleCodes.AllTransactions;

				Invoice = ObjectCreator.CreateInvoice(typeof(ARInvoice));

				var registry = AccountingMasterFilesRegistry.Instance;
				using (registry.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(currComp.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
				using (registry.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(currComp.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(currComp.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceNumberAllocationDateCode))
				{
					var today = ZDate.Today;

					TxnHeader.Ledger = TxnLedgerType.AR;
					TxnHeader.DebtorOrCreditor = new Organisation { EDICode = currComp.OrgProxy.OH_Code };
					TxnHeader.InvoiceDate = today;
					TxnHeader.PostDate = today;
					TxnHeader.OsInvoiceAmtInclTax = FinancialValue.FromAmountAndCurrencyCode(new ZDecimal(1162.42), "EUR");

					TxnHeader.TxnLines = new TxnLineCollection();
					var invoiceLine1 = TxnHeader.TxnLines.AddNew();
					invoiceLine1.ChargeCode = ObjectCreator.CC1.AC_Code;
					invoiceLine1.Branch = GlbBranch.CurrentBranch.GB_Code;
					invoiceLine1.Department = GlbDepartment.CurrentDepartment.GE_Code;
					invoiceLine1.LineType = TxnLineLineType.REV;
					invoiceLine1.TaxCode = "FREEIVA";
					invoiceLine1.OsInvoiceAmtExclTax = FinancialValue.FromAmountAndCurrencyCode(new ZDecimal(1162.42), "EUR");

					Assert("Precondition: Country supports compliance sub-type", currComp.Country.SupportComplianceSubType);
					Assert("Precondition: Invoice is not saved in the database", !Invoice.IsInDatabase);

					var value = registry.ComplianceSubTypeAttributionRuleConfiguration.Value;
					Assert("Precondition: Compliance sub-type rules are defined", value.Any());

					Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, "Transaction AR INV 00001000: ");

					AssertEquals("Compliance sub-type", subType, Invoice.AH_ComplianceSubType);
					AssertNotNull("Compliance sequence", Invoice.ComplianceSequenceFromSubType);
					AssertNoRowErrorContaining(Invoice, ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage);
					AssertNoErrors(Invoice.AH_ComplianceSubTypeInfo);
				}
			}
		}

		public void TestBehaviourWhenComplianceSubTypeIsBlank()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				AccountingConfigurationRegistry.Instance.DisallowPostingTransactionWithEmptyComplianceSubtype.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, ZString.Empty);
				AssertEquals(ZString.Empty, Invoice.AH_ComplianceSubType);
				TestHelper.AssertNotificationsContainsErrorMessage(Notify, "You cannot post invoices with a blank compliance sub-type. Please check the registry setting at Accounting > Government Compliance Invoice Document > Disallow posting transactions with empty compliance subtype.");
			}

			((AccountingConfigurationRegistry.CountryEnabledBooleanRegistryItemImpl)AccountingConfigurationRegistry.Instance.DisallowPostingTransactionWithEmptyComplianceSubtype.Inner).ClearCacheForTestOnly();
			Notify.Clear();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AccountingConfigurationRegistry.Instance.DisallowPostingTransactionWithEmptyComplianceSubtype.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				Builder.SetValuesOnInvoiceBusinessObject(Invoice, TxnHeader, ImportContext, ZString.Empty);
				AssertEquals(ZString.Empty, Invoice.AH_ComplianceSubType);
				TestHelper.AssertNotificationsDoesNotContainErrorMessage(Notify, "You cannot post invoices with a blank compliance sub-type. Please check the registry setting at Accounting > Government Compliance Invoice Document > Disallow posting transactions with empty compliance subtype.");
			}
		}

		public void TestHighPrecisionExchangeRate()
		{
			ZDecimal oSAmount = 344976M;
			ZDecimal localAmount = 4839785M;

			Builder = GetTransactionHeaderBuilderForTest();

			TxnHeader.OsInvoiceAmtExclTax.Value = oSAmount;
			TxnHeader.LocalInvoiceAmtExclTax.Value = localAmount;
			TxnHeader.OsInvoiceAmtInclTax.CurrencyCode = "USD";

			ARReceipt receipt = Factory.New<ARReceipt>();
			int decimalPlaces = receipt.ExchangeRateDecimalPlaces;
			Builder.SetValuesOnReceiptBusinessObject(receipt, TxnHeader, ImportContext, "Transaction AR REC 00001000: ");

			AssertEquals(Math.Round(oSAmount / localAmount, decimalPlaces), receipt.AH_ExchangeRate);
		}

		public void TestIsReceiptPaymentFromFileImportForCashInvoice()
		{
			APInvoice aPInvoice = Factory.New<APInvoice>();
			PopulateTxnHeaderForCashPayment(Xsd.TxnHeaderReceiptPaymentType.EFT, BankAccount.AB_Code, "", "");
			Builder.SetValuesOnInvoiceBusinessObject(aPInvoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
			if (InvoiceWithPaymentOrReceiptShouldBeCreated)
			{
				AssertEquals("This flag must be set to prevent the invoice defaulting logic from overriding populated values", true, aPInvoice.IsReceiptPaymentFromFileImport);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestIsTaxApplicable_ForTransactionWithoutOrganization()
		{
			InvoicingBase invoice = (InvoicingBase)Factory.New(typeof(APInvoice));

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			AssertEquals("GST is not applicable", false, TransactionHeaderBuilder.IsTaxApplicable(invoice));

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			Assert("GST is applicable", TransactionHeaderBuilder.IsTaxApplicable(invoice));
		}

		public void TestIsTaxApplicable_ForAPTransaction()
		{
			var apInvoice = ObjectCreator.CreateInvoice(typeof(APInvoice), ObjectCreator.AUD, 1m, ObjectCreator.AALSHI);
			var uaInvoice = ObjectCreator.CreateInvoice(typeof(UAInvoice), ObjectCreator.AUD, 1m, ObjectCreator.AALSHI);

			ObjectCreator.AALSHI.CompanyData.SetAPTaxApplicable(false);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			AssertEquals("GST is not applicable", false, TransactionHeaderBuilder.IsTaxApplicable(apInvoice));
			AssertEquals("GST is not applicable", false, TransactionHeaderBuilder.IsTaxApplicable(uaInvoice));

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			AssertEquals("GST is not applicable", false, TransactionHeaderBuilder.IsTaxApplicable(apInvoice));
			AssertEquals("GST is not applicable", false, TransactionHeaderBuilder.IsTaxApplicable(uaInvoice));

			ObjectCreator.AALSHI.CompanyData.SetAPTaxApplicable(true);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			AssertEquals("GST is not applicable", false, TransactionHeaderBuilder.IsTaxApplicable(apInvoice));
			AssertEquals("GST is not applicable", false, TransactionHeaderBuilder.IsTaxApplicable(uaInvoice));

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			Assert("GST is applicable", TransactionHeaderBuilder.IsTaxApplicable(apInvoice));
			Assert("GST is applicable", TransactionHeaderBuilder.IsTaxApplicable(uaInvoice));
		}

		public void TestIsTaxApplicable_ForARTransaction()
		{
			var arInvoice = ObjectCreator.CreateInvoice(typeof(ARInvoice), ObjectCreator.AUD, 1m, ObjectCreator.ABIGAS);

			ObjectCreator.ABIGAS.CompanyData.SetARTaxApplicable(false);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			AssertEquals("GST is not applicable", false, TransactionHeaderBuilder.IsTaxApplicable(arInvoice));

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			AssertEquals("GST is not applicable", false, TransactionHeaderBuilder.IsTaxApplicable(arInvoice));

			ObjectCreator.ABIGAS.CompanyData.SetARTaxApplicable(true);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			AssertEquals("GST is not applicable", false, TransactionHeaderBuilder.IsTaxApplicable(arInvoice));

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			Assert("GST is applicable", TransactionHeaderBuilder.IsTaxApplicable(arInvoice));
		}

		[TestDate(2015, 1, 1)]
		public void TestARAPInvoicePostingExchangeRateOption()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			var chargeCode = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);

			var today = ZDateTime.Today;
			var postDate = ZDateTime.Today.AddDays(1);
			var invoiceDate = ZDateTime.Today.AddDays(3);
			var taxDate = ZDate.Today.AddDays(2);

			var creator = new TestObjectCreator(Factory);
			creator.CreateUSDBuyRate(1.1m, today);
			creator.CreateUSDBuyRate(1.2m, postDate);
			creator.CreateUSDBuyRate(1.3m, invoiceDate);
			creator.CreateUSDBuyRate(1.4m, taxDate);

			TxnHeader.OsInvoiceAmtExclTax.Value = 100M;
			TxnHeader.OsInvoiceAmtExclTax.CurrencyCode = "USD";
			TxnHeader.LocalInvoiceAmtExclTax.Value = 50M;
			TxnHeader.LocalInvoiceAmtExclTax.CurrencyCode = "AUD";
			TxnHeader.PostDate = postDate;
			TxnHeader.InvoiceDate = invoiceDate;
			TxnHeader.OverrideSystemExchangeRate = false;

			TxnLine txnLine1 = TxnHeader.TxnLines.AddNew();
			txnLine1.OsInvoiceAmtExclTax.Value = 100M;
			txnLine1.OsInvoiceAmtExclTax.CurrencyCode = "USD";
			txnLine1.LocalInvoiceAmtExclTax.Value = 50M;
			txnLine1.LocalInvoiceAmtExclTax.CurrencyCode = "AUD";
			txnLine1.ChargeCode = chargeCode.AC_Code;

			var invoice = Factory.New<APInvoice>();
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF");
			Builder.SetValuesOnInvoiceBusinessObject(invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");

			AssertEquals(invoice.AH_ExchangeRate, 1.2M);
			AssertEquals(invoice.AH_LocalExTaxAmount, -83.33M);
			AssertEquals(invoice.AH_OSExTaxAmount, -100M);

			AssertEquals(invoice.Lines[0].AL_ExchangeRate, 1.2M);
			AssertEquals(invoice.Lines[0].AL_LocalExTaxAmount, -83.33M);
			AssertEquals(invoice.Lines[0].AL_OSExTaxAmount, -100M);

			invoice = Factory.New<APInvoice>();
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD");
			Builder.SetValuesOnInvoiceBusinessObject(invoice, TxnHeader, ImportContext, "Transaction AP INV 00001001: ");

			AssertEquals(invoice.AH_ExchangeRate, 1.1M);
			AssertEquals(invoice.AH_LocalExTaxAmount, -90.91M);
			AssertEquals(invoice.AH_OSExTaxAmount, -100M);

			AssertEquals(invoice.Lines[0].AL_ExchangeRate, 1.1M);
			AssertEquals(invoice.Lines[0].AL_LocalExTaxAmount, -90.91M);
			AssertEquals(invoice.Lines[0].AL_OSExTaxAmount, -100M);

			invoice = Factory.New<APInvoice>();
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");
			Builder.SetValuesOnInvoiceBusinessObject(invoice, TxnHeader, ImportContext, "Transaction AP INV 00001002: ");

			AssertEquals(invoice.AH_ExchangeRate, 1.3M);
			AssertEquals(invoice.AH_LocalExTaxAmount, -76.92M);
			AssertEquals(invoice.AH_OSExTaxAmount, -100M);

			AssertEquals(invoice.Lines[0].AL_ExchangeRate, 1.3M);
			AssertEquals(invoice.Lines[0].AL_LocalExTaxAmount, -76.92M);
			AssertEquals(invoice.Lines[0].AL_OSExTaxAmount, -100M);

			invoice = Factory.New<APInvoice>();
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");
			Builder.SetValuesOnInvoiceBusinessObject(invoice, TxnHeader, ImportContext, "Transaction AP INV 00001003: ");

			AssertEquals(invoice.AH_ExchangeRate, 1.2M);
			AssertEquals(invoice.AH_LocalExTaxAmount, -83.33M);
			AssertEquals(invoice.AH_OSExTaxAmount, -100M);

			AssertEquals(invoice.Lines[0].AL_ExchangeRate, 1.2M);
			AssertEquals(invoice.Lines[0].AL_LocalExTaxAmount, -83.33M);
			AssertEquals(invoice.Lines[0].AL_OSExTaxAmount, -100M);

			invoice = Factory.New<APInvoice>();
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "EIT");
			Builder.SetValuesOnInvoiceBusinessObject(invoice, TxnHeader, ImportContext, "Transaction AP INV 00001004: ");

			invoice.Lines[0].AL_AT = creator.GST1.PK;
			invoice.Lines[0].AL_TaxDate = taxDate;
			AssertEquals(taxDate, invoice.InvoiceTaxDate);
			invoice.AH_RX_NKTransactionCurrency = "USD";

			AssertEquals(invoice.AH_ExchangeRate, 1.4M);
			AssertEquals(invoice.AH_LocalExTaxAmount, -71.43M);
			AssertEquals(invoice.AH_OSExTaxAmount, -100M);

			AssertEquals(invoice.Lines[0].AL_ExchangeRate, 1.4M);
			AssertEquals(invoice.Lines[0].AL_LocalExTaxAmount, -71.43M);
			AssertEquals(invoice.Lines[0].AL_OSExTaxAmount, -100M);

			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");
			TxnHeader.PostDate = new DateTime(2015, 2, 1);
			invoice = Factory.New<APInvoice>();
			Builder.SetValuesOnInvoiceBusinessObject(invoice, TxnHeader, ImportContext, "Transaction AP INV 00001005: ");
			AssertHasError(invoice.AH_ExchangeRateInfo, @"The ""AP Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Post Date"".
But the USD exchange rate is not set for the date 01-Feb-15. Please check your data and try again.");

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		[TestDate(2015, 1, 1)]
		public void TestGetExchangeRate_FallBackToPreviousExchangeRate()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			var chargeCode = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);

			var creator = new TestObjectCreator(Factory);
			creator.CreateUSDBuyRate(1.2m, new DateTime(2015, 1, 2));

			TxnHeader.OsInvoiceAmtExclTax.Value = 100M;
			TxnHeader.OsInvoiceAmtExclTax.CurrencyCode = "USD";
			TxnHeader.LocalInvoiceAmtExclTax.Value = 50M;
			TxnHeader.LocalInvoiceAmtExclTax.CurrencyCode = "AUD";
			TxnHeader.PostDate = new DateTime(2015, 1, 5);
			TxnHeader.InvoiceDate = new DateTime(2015, 1, 5);
			TxnHeader.OverrideSystemExchangeRate = false;

			TxnLine txnLine1 = TxnHeader.TxnLines.AddNew();
			txnLine1.OsInvoiceAmtExclTax.Value = 100M;
			txnLine1.OsInvoiceAmtExclTax.CurrencyCode = "USD";
			txnLine1.LocalInvoiceAmtExclTax.Value = 50M;
			txnLine1.LocalInvoiceAmtExclTax.CurrencyCode = "AUD";
			txnLine1.ChargeCode = chargeCode.AC_Code;

			using (AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				using (PostingExRateRegistryAP.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF"))
				{
					var invoice = Factory.New<APInvoice>();
					Builder.SetValuesOnInvoiceBusinessObject(invoice, TxnHeader, ImportContext, "Transaction AP INV 00001000: ");
					AssertEquals("Should fall back to previous exchange rate", invoice.AH_ExchangeRate, 1.2M);
					AssertEquals("Should fall back to previous exchange rate", invoice.Lines[0].AL_ExchangeRate, 1.2M);
					ExchangeRateReader.GetReaderInstance().ClearCache();
				}

				using (PostingExRateRegistryAP.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV"))
				{
					var invoice = Factory.New<APInvoice>();
					Builder.SetValuesOnInvoiceBusinessObject(invoice, TxnHeader, ImportContext, "Transaction AP INV 00001001: ");
					AssertEquals("Should fall back to previous exchange rate", invoice.AH_ExchangeRate, 1.2M);
					AssertEquals("Should fall back to previous exchange rate", invoice.Lines[0].AL_ExchangeRate, 1.2M);
					ExchangeRateReader.GetReaderInstance().ClearCache();
				}
			}

			using (AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				using (PostingExRateRegistryAP.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF"))
				{
					var invoice = Factory.New<APInvoice>();
					Builder.SetValuesOnInvoiceBusinessObject(invoice, TxnHeader, ImportContext, "Transaction AP INV 00001002: ");
					AssertEquals("Should not fall back to previous exchange rate", invoice.AH_ExchangeRate, 0M);
					ExchangeRateReader.GetReaderInstance().ClearCache();
				}

				using (PostingExRateRegistryAP.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV"))
				{
					var invoice = Factory.New<APInvoice>();
					Builder.SetValuesOnInvoiceBusinessObject(invoice, TxnHeader, ImportContext, "Transaction AP INV 00001003: ");
					AssertEquals("Should not fall back to previous exchange rate", invoice.AH_ExchangeRate, 0M);
					ExchangeRateReader.GetReaderInstance().ClearCache();
				}
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			TestHelper = new NotificationTestHelper();

			TxnHeader = new TxnHeader();
			TxnHeader.TxnType = Xsd.TxnType.INV;
			TxnHeader.Ledger = Xsd.TxnLedgerType.AP;
			TxnHeader.TxnNumber = "00001000";

			Notify = new NotificationBuffer();
			Notifier = new NotificationManager(Notify);
			Builder = GetTransactionHeaderBuilderForTest();
			ObjectCreator = new TestObjectCreator(Factory);

			BankAccount = ObjectCreator.AUDBankAccount;
			BankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;

			ChequeBook = ObjectCreator.AUDChequeBook;
			ChequeBook.AK_StartNo = 1;
			ChequeBook.AK_LastNo = 100;
			ChequeBook.AK_CurrentNo = 35;
			Factory.Save();

			Invoice = Factory.New<APInvoice>();
			TestObjectCreator.FillInvoiceWithMinimumTestData(Invoice);
			Invoice.SubmittedFromInvoicingForm = true;
		}

		protected virtual TransactionHeaderBuilder GetTransactionHeaderBuilderForTest()
		{
			return new TransactionHeaderBuilder(Notifier, new TransactionBuilderConfig());
		}

		void SetUpBuyAndSellExchangeRateForGBP(ZDecimal buyRate, ZDecimal sellRate)
		{
			RefExchangeRate gBPBuyExRate = Factory.New<RefExchangeRate>();
			gBPBuyExRate.RE_RX_NKExCurrency = "GBP";
			gBPBuyExRate.RE_StartDate = PostDate.AddDays(-2);
			gBPBuyExRate.RE_ExpiryDate = PostDate.AddDays(2);
			gBPBuyExRate.RE_GC = GlbCompany.CurrentCompany.PK;
			gBPBuyExRate.RE_SellRate = buyRate;
			gBPBuyExRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;

			RefExchangeRate gBPSellExRate = Factory.New<RefExchangeRate>();
			gBPSellExRate.RE_RX_NKExCurrency = "GBP";
			gBPSellExRate.RE_StartDate = PostDate.AddDays(-2);
			gBPSellExRate.RE_ExpiryDate = PostDate.AddDays(2);
			gBPSellExRate.RE_GC = GlbCompany.CurrentCompany.PK;
			gBPSellExRate.RE_SellRate = sellRate;
			gBPSellExRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;

			Factory.Save();
		}

		void AssertHeaderValues(Journal header, TxnHeader txnHeader, ZDecimal exchangeRate, ZString debitOrCredit)
		{
			int multiplier = debitOrCredit == "CR" ? 1 : -1;

			AssertEquals("Organisation", txnHeader.DebtorOrCreditor.EDICode, header.Header.OH_Code);
			AssertEquals("Invoice Date", txnHeader.InvoiceDate, header.AH_InvoiceDate);
			AssertEquals("DueDate", txnHeader.DueDate, header.AH_DueDate);
			AssertEquals("Description", txnHeader.Description, header.AH_Desc);

			AssertEquals("Branch", txnHeader.Branch, header.Branch.GB_Code);
			AssertEquals("Department", txnHeader.Department, header.Department.GE_Code);
			AssertEquals("Local Amount", multiplier * txnHeader.LocalInvoiceAmtInclTax.Value, header.AH_LocalTotalAmount);
			AssertEquals("OS Amount", multiplier * txnHeader.OsInvoiceAmtInclTax.Value, header.AH_OSTotalAmount);
			AssertEquals("Currency Code", txnHeader.OsInvoiceAmtInclTax.CurrencyCode, header.AH_RX_NKTransactionCurrency);
			AssertEquals("ExchangeRate", exchangeRate, header.AH_ExchangeRate);
			AssertEquals("Debit/Credit", debitOrCredit, header.DebitCreditSign);

			AssertNotEquals("PostDate isn't copied", txnHeader.PostDate, header.AH_PostDate);
			AssertNotEquals("GLAccount isn't copied", txnHeader.GlAccount, header.GLHeader.AccountNum);
		}

		void PopulateTxnHeaderForCashPayment(TxnHeaderReceiptPaymentType paymentType, ZString bankCode, ZString chequeBookCode, ZString chequeOrReference)
		{
			TxnHeader.ReceiptPaymentTypeSpecified = true;
			TxnHeader.ReceiptPaymentType = paymentType;
			TxnHeader.BankCode = bankCode;
			TxnHeader.ChequeBook = chequeBookCode;
			TxnHeader.ChequeOrReference = chequeOrReference;
			TxnHeader.DebtorOrCreditor = new Organisation();
			TxnHeader.DebtorOrCreditor.EDICode = ObjectCreator.AALSHI.OH_Code;
		}

		void PopulateTxnHeaderForCashReceipt(TxnHeaderReceiptPaymentType paymentType, ZString bankCode, ZString chequeOrReference, ZString chequeDrawer, ZString chequeDrawerBank, ZString chequeDrawerBankBranch)
		{
			TxnHeader.ReceiptPaymentTypeSpecified = true;
			TxnHeader.ReceiptPaymentType = paymentType;
			TxnHeader.BankCode = bankCode;
			TxnHeader.ChequeOrReference = chequeOrReference;
			TxnHeader.DebtorOrCreditor = new Organisation();
			TxnHeader.DebtorOrCreditor.EDICode = ObjectCreator.AALSHI.OH_Code;
			TxnHeader.ChequeDrawer = chequeDrawer;
			TxnHeader.DrawerBank = chequeDrawerBank;
			TxnHeader.DrawerBankBranch = chequeDrawerBankBranch;
		}

		AccHotCheque CreateHotCheque(OrgHeader organisation, ZString chequeNumber, ZString actualOrMax, ZDecimal amount)
		{
			AccHotCheque hotCheque = Factory.New<AccHotCheque>();
			hotCheque.AQ_OH = organisation.PK;
			hotCheque.AQ_AK = ChequeBook.PK;
			hotCheque.AQ_ChequeNumber = chequeNumber;
			hotCheque.AQ_ActualOrMaxIndicator = actualOrMax;
			hotCheque.AQ_Amount = amount;
			return hotCheque;
		}

		protected virtual void AssertCashReceiptPaymentDetails(Invoice invoice, bool isCashInvoice,
				string paymentType, ZGuid bankAccount, ZGuid chequeBook, string chequeReference,
				string chequeDrawer, string chequeDrawerBank, string chequeDrawerBankBranch, bool shouldHaveHotCheque)
		{
			AssertEquals("Is Cash Invoice", isCashInvoice, invoice.IsInvoiceReceiptPayment);
			AssertEquals("Cash Invoice: Payment Type", paymentType, invoice.ReceiptPaymentAH_ReceiptType);
			AssertEquals("Cash Invoice: Bank Account", bankAccount, invoice.ReceiptPaymentAH_AB);
			AssertEquals("Cash Invoice: Cheque Book", chequeBook, invoice.ReceiptPaymentAK_AB);
			AssertEquals("Cash Invoice: Cheque / Reference", chequeReference, invoice.ReceiptPaymentAH_ChequeOrReference);
			AssertEquals("Cash Invoice: Cheque Drawer", chequeDrawer, invoice.ReceiptPaymentAH_ChequeDrawer);
			AssertEquals("Cash Invoice: Cheque Drawer Bank", chequeDrawerBank, invoice.ReceiptPaymentAH_DrawerBank);
			AssertEquals("Cash Invoice: Cheque Drawer Bank Branch", chequeDrawerBankBranch, invoice.ReceiptPaymentAH_DrawerBranch);

			if (shouldHaveHotCheque)
			{
				AssertNotNull("Cash Invoice: Hot Cheque", ((APInvoice)invoice).ImportedHotCheque);
			}
		}

		protected virtual bool InvoiceWithPaymentOrReceiptShouldBeCreated
		{
			get { return true; }
		}

		ValueObjectImportContext ImportContext
		{
			get
			{
				if (fImportContext == null)
				{
					fImportContext = new ValueObjectImportContext(Factory, Notify);
				}
				return fImportContext;
			}
		}
		ValueObjectImportContext fImportContext;

		TransactionHeaderBuilder Builder;
		NotificationBuffer Notify;
		protected NotificationManager Notifier;
		NotificationTestHelper TestHelper;
		TxnHeader TxnHeader;
		InvoicingBase Invoice;
		TestObjectCreator ObjectCreator;

		AccBankAccount BankAccount;
		AccChequeBook ChequeBook;

		ZDateTime PostDate
		{
			get { return new ZDateTime(2005, 01, 01, 10, 30, 00); }
		}

		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAP => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAR => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR;

		#endregion
	}
}
