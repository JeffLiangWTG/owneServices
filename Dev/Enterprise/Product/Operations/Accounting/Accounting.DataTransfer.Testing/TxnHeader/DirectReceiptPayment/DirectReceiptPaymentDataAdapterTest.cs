using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.DataTransfer.DirectReceiptPayment;
using Enterprise.Accounting.DataTransfer.Invoices.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Testing.TxnHeader.DirectReceiptPayment
{
	[TestedType(typeof(DirectReceiptPaymentDataAdapter))]
	sealed class DirectReceiptPaymentDataAdapterTest : BaseAccountingDataAdapterTest<DirectTransactionHeaderBase, Xsd.TxnHeaderDirect>
	{
		[TestDate(2004, 12, 31)]
		public void TestImportRecoverableGSTVATPercentage()
		{
			Env.Security.NewCashBookAllowGLAccountWithoutMapping.IsAllowed = true;
			Env.Security.NewCashBookDirectPaymentAllowOverrideOfVATRecoverable.IsAllowed = true;

			var notifications = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notifications);

			TxnHeader.Ledger = Xsd.TxnLedgerType.CB;
			TxnHeader.TxnType = Xsd.TxnType.DPY;
			var txnLine = TxnHeader.TxnLines[0];
			txnLine.RecoverableGSTVATPercentage = 81.23;

			DirectTransactionHeaderBase transactionHeader = DataAdapter.NewBusinessObject_ForTestOnly(TxnHeader, context);
			AssertEquals("Precondition: TransactionHeader Type", typeof(DirectPayment), transactionHeader.GetType());
			DataAdapter.ImportFromValueObject(transactionHeader, TxnHeader, context);

			TestHelper.AssertNotificationsDoesNotContainErrorMessage(notifications, "Error");
			AssertEquals("Header should not be deleted", false, transactionHeader.IsDeleted);

			AssertEquals("AL_InputGSTVATRecoverable", 0.8123m, transactionHeader.Lines[0].AL_InputGSTVATRecoverable);
		}

		[TestDate(2004, 12, 31)]
		public void TestImportRecoverableGSTVATPercentageWhenNotSpecified()
		{
			Env.Security.NewCashBookAllowGLAccountWithoutMapping.IsAllowed = true;
			Env.Security.NewCashBookDirectPaymentAllowOverrideOfVATRecoverable.IsAllowed = true;

			var notifications = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notifications);

			TxnHeader.Ledger = Xsd.TxnLedgerType.CB;
			TxnHeader.TxnType = Xsd.TxnType.DPY;
			var txnLine = TxnHeader.TxnLines[0];

			DirectTransactionHeaderBase transactionHeader = DataAdapter.NewBusinessObject_ForTestOnly(TxnHeader, context);
			AssertEquals("Precondition: TransactionHeader Type", typeof(DirectPayment), transactionHeader.GetType());
			DataAdapter.ImportFromValueObject(transactionHeader, TxnHeader, context);

			TestHelper.AssertNotificationsDoesNotContainErrorMessage(notifications, "Error");
			AssertEquals("Header should not be deleted", false, transactionHeader.IsDeleted);

			AssertEquals("AL_InputGSTVATRecoverable should have default value if RecoverableGSTVATPercentage is not specified.", 1m, transactionHeader.Lines[0].AL_InputGSTVATRecoverable);
		}

		[TestDate(2004, 12, 31)]
		public void TestImportRecoverableGSTVATPercentage_InvalidValue()
		{
			var notifications = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notifications);

			TxnHeader.Ledger = Xsd.TxnLedgerType.CB;
			TxnHeader.TxnType = Xsd.TxnType.DPY;
			var txnLine = TxnHeader.TxnLines[0];
			txnLine.RecoverableGSTVATPercentage = 120;

			DirectTransactionHeaderBase transactionHeader = DataAdapter.NewBusinessObject_ForTestOnly(TxnHeader, context);
			AssertEquals("Precondition: TransactionHeader Type", typeof(DirectPayment), transactionHeader.GetType());
			DataAdapter.ImportFromValueObject(transactionHeader, TxnHeader, context);

			TestHelper.AssertNotificationsContainsErrorMessage(notifications, "Error: Tax Recoverable Percentage: GST Recoverable % must be between 0 and 100.");
			AssertEquals("Header should be deleted", true, transactionHeader.IsDeleted);
		}

		[TestDate(2004, 12, 31)]
		public void TestImportRecoverableGSTVATPercentageForNotSupportedBizo()
		{
			Env.Security.NewCashBookAllowGLAccountWithoutMapping.IsAllowed = true;
			var notifications = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notifications);

			TxnHeader.Ledger = Xsd.TxnLedgerType.CB;
			TxnHeader.TxnType = Xsd.TxnType.DRC;
			var txnLine = TxnHeader.TxnLines[0];
			txnLine.RecoverableGSTVATPercentage = 81.23;

			DirectTransactionHeaderBase transactionHeader = DataAdapter.NewBusinessObject_ForTestOnly(TxnHeader, context);
			AssertEquals("Precondition: TransactionHeader Type", typeof(DirectReceipt), transactionHeader.GetType());
			DataAdapter.ImportFromValueObject(transactionHeader, TxnHeader, context);

			TestHelper.AssertNotificationsDoesNotContainErrorMessage(notifications, "Error");
			AssertEquals("Header should not be deleted", false, transactionHeader.IsDeleted);

			AssertEquals("AL_InputGSTVATRecoverable should not be imported for Direct Receipt", 1m, transactionHeader.Lines[0].AL_InputGSTVATRecoverable);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportTwoCHQPaymentsWithSameChequeNumber()
		{
			TestCaseHelper.ClearTable(AccTransactionHeaderSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionLinesSchema.Constants.TableName);
			CreateBusinessObjectsForTest();
			AssertEquals("Precondition: AccTransactionHeader Database Count", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));

			Enterprise.Environment.Env.Security.PrintCheque.IsAllowed = true;
			ZString fileName = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\DirectReceiptPayment\Testing\TwoCHQPaymentsWithSameChequeNumber.xml";
			NotificationBuffer buffer = new NotificationBuffer();
			XmlDataTransferDirector transferDirector = new XmlDataTransferDirector(DataAdapter, false);
			transferDirector.Import(fileName, buffer, SourceInfo.EmptySourceInfo);

			ZString expectedLine = "Error: Header Check Or Reference: This check number is already in use.";
			AssertContains("Import Notifications", expectedLine, buffer.AsString);
			AssertEquals("AccTransactionHeader Database Count", 1, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));

			DirectPayment payment = Factory.LoadTop1<DirectPayment>(new ZQuery());

			AssertHeaderValues(payment, new ZDateTime(2005, 01, 01), AUDBankAccount, ObjectCreator.AUD, 1m,
				"CASH BOOK DIRECT PAYMENT", ZArchitecture.Core.ReceiptTypes.Cheque, "000015", new ZDateTime(2005, 01, 01), 200m, 10m);

			AssertEquals("Lines.Count", 2, payment.Lines.Count);
			AssertLineValues(payment.Lines[0], GLHeader1, Branch, Department, "Description for First Line", 100m, TaxRate1, 10m);
			AssertLineValues(payment.Lines[1], GLHeader2, Branch, Department, "Description for Second Line", 100m, TaxRate2, 0m);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportTwoEFTPaymentsWithSameChequeNumber()
		{
			TestCaseHelper.ClearTable(AccTransactionHeaderSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionLinesSchema.Constants.TableName);
			CreateBusinessObjectsForTest();
			AssertEquals("Precondition: AccTransactionHeader Database Count", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));

			ZString fileName = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\DirectReceiptPayment\Testing\TwoEFTPaymentsWithSameChequeNumber.xml";
			NotificationBuffer buffer = new NotificationBuffer();
			XmlDataTransferDirector transferDirector = new XmlDataTransferDirector(DataAdapter, false);
			transferDirector.Import(fileName, buffer, SourceInfo.EmptySourceInfo);

			AssertNotContains("Import Notifications", "Error", buffer.AsString);
			AssertEquals("AccTransactionHeader Database Count", 2, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportTwoReceiptsWithSameChequeNumber()
		{
			TestCaseHelper.ClearTable(AccTransactionHeaderSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionLinesSchema.Constants.TableName);
			CreateBusinessObjectsForTest();
			AssertEquals("Precondition: AccTransactionHeader Database Count", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));

			ZString fileName = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\DirectReceiptPayment\Testing\TwoReceiptsWithSameChequeNumber.xml";
			NotificationBuffer buffer = new NotificationBuffer();
			XmlDataTransferDirector transferDirector = new XmlDataTransferDirector(DataAdapter, false);
			transferDirector.Import(fileName, buffer, SourceInfo.EmptySourceInfo);

			AssertNotContains("Import Notifications", "Error", buffer.AsString);
			AssertEquals("AccTransactionHeader Database Count", 2, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));

			DirectReceipt[] receipts = Factory.Load<DirectReceipt>(new ZQuery());

			AssertHeaderValues(receipts[0], new ZDateTime(2005, 01, 01), AUDBankAccount, ObjectCreator.AUD, 1m, "CASH BOOK DIRECT RECEIPT",
				ZArchitecture.Core.ReceiptTypes.Cheque, "15", new ZDateTime(2005, 01, 01), 200, 10);

			AssertHeaderValues(receipts[1], new ZDateTime(2005, 01, 01), AUDBankAccount, ObjectCreator.AUD, 1m, "CASH BOOK DIRECT RECEIPT",
				ZArchitecture.Core.ReceiptTypes.Cheque, "15", new ZDateTime(2005, 01, 01), 200, 10);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportForeignCurrencyTransactions()
		{
			TestCaseHelper.ClearTable(AccTransactionHeaderSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionLinesSchema.Constants.TableName);
			CreateBusinessObjectsForTest();
			AssertEquals("Precondition: AccTransactionHeader Database Count", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));

			Enterprise.Environment.Env.Security.PrintCheque.IsAllowed = true;
			ZString fileName = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\DirectReceiptPayment\Testing\ForeignCurrencyTransactions.xml";
			NotificationBuffer buffer = new NotificationBuffer();
			XmlDataTransferDirector transferDirector = new XmlDataTransferDirector(DataAdapter, false);
			transferDirector.Import(fileName, buffer, SourceInfo.EmptySourceInfo);

			AssertNotContains("Import Notifications", "Error", buffer.AsString);
			AssertEquals("AccTransactionHeader Database Count", 2, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));

			ZQuery findUSDPaymentQuery = new ZQuery(AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, USDBankAccount.AB_RX_NKAccountCurrency);
			findUSDPaymentQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			DirectPayment uSDPayment = Factory.LoadTop1<DirectPayment>(findUSDPaymentQuery);

			AssertHeaderValues(uSDPayment, new ZDateTime(2005, 01, 01), USDBankAccount, ObjectCreator.USD,
				0.5m, "CASH BOOK DIRECT PAYMENT", ZArchitecture.Core.ReceiptTypes.Cheque, "000015", new ZDateTime(2005, 01, 01), 100m, 5m);

			ZQuery findAUDPaymentQuery = new ZQuery(AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, AUDBankAccount.AB_RX_NKAccountCurrency);
			findAUDPaymentQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			DirectPayment aUDPayment = Factory.LoadTop1<DirectPayment>(findAUDPaymentQuery);

			AssertHeaderValues(aUDPayment, new ZDateTime(2005, 01, 01), AUDBankAccount, ObjectCreator.AUD,
				1m, "CASH BOOK DIRECT PAYMENT", ZArchitecture.Core.ReceiptTypes.Cheque, "000015", new ZDateTime(2005, 01, 01), 200m, 10m);
		}

		void AssertHeaderValues(DirectTransactionHeaderBase header, ZDateTime invoiceDate, AccBankAccount bankAccount,
		RefCurrency currency, ZDecimal exchangeRate, ZString description, ZString paymentType,
			ZString chequeOrReference, ZDateTime postDate, ZDecimal oSExTaxAmount, ZDecimal oSTaxAmount)
		{
			AssertEquals("Invoice Date", invoiceDate.Date, header.AH_InvoiceDate.Date);
			AssertEquals("Bank Account", bankAccount.AB_Code, header.BankAccount.AB_Code);
			AssertEquals("Currency", currency.RX_Code, header.AH_RX_NKTransactionCurrency);
			AssertEquals("ExchangeRate", exchangeRate, header.AH_ExchangeRate);
			AssertEquals("Description", description, header.AH_Desc);
			Assert("TransactionNumber should be filled in", !header.AH_TransactionNum.IsEmpty);
			AssertEquals("Payment Type", paymentType, header.AH_ReceiptType);
			AssertEquals("ChequeOrReference", chequeOrReference, header.AH_ChequeOrReference);
			AssertEquals("PostDate", postDate.Date, header.AH_PostDate.Date);
			AssertEquals("OSExTaxAmount", oSExTaxAmount, header.AH_OSExTaxAmount);
			AssertEquals("OSTaxAmount", oSTaxAmount, header.AH_OSTaxAmount);
			AssertEquals("OSTotal", oSExTaxAmount + oSTaxAmount, header.AH_OSTotalAmount);
		}

		void AssertLineValues(DependentTransactionLine line, AccGLHeader gLAccount, GlbBranch branch, GlbDepartment department,
			ZString description, ZDecimal oSExTaxAmount, AccTaxRate taxRate, ZDecimal oSTaxAmount)
		{
			AssertEquals("GLAccount", gLAccount.AG_AccountNum, line.GLHeader.AG_AccountNum);
			AssertEquals("Branch", branch.GB_Code, line.Branch.GB_Code);
			AssertEquals("Department", department.GE_Code, line.Department.GE_Code);
			AssertEquals("Description", description, line.AL_Desc);
			AssertEquals("OSExTaxAmount", oSExTaxAmount, line.AL_OSExTaxAmount);
			AssertEquals("TaxRate", taxRate.AT_Code, line.TaxRate.AT_Code);
			AssertEquals("OSTaxAmount", oSTaxAmount, line.AL_OSTaxAmount);
		}

		public void TestPopulateMissingAmounts()
		{
			SetAmountsOnTestTransaction(300m, 30m, 330m, 100m, 10m, 110m, 200m, 20m, 220m);
			DataAdapter.PopulateMissingHeaderAmounts_ForTestOnly(TxnHeader);
			AssertTxnHeaderValues(300m, 30m, 330m, 100m, 10m, 110m, 200m, 20m, 220m);

			SetAmountsOnTestTransaction(300m, 30m, 0m, 100m, 10m, 0m, 200m, 20m, 0m);
			DataAdapter.PopulateMissingHeaderAmounts_ForTestOnly(TxnHeader);
			AssertTxnHeaderValues(300m, 30m, 330m, 100m, 10m, 110m, 200m, 20m, 220m);

			SetAmountsOnTestTransaction(300m, 0m, 330m, 100m, 0m, 110m, 200m, 0m, 220m);
			DataAdapter.PopulateMissingHeaderAmounts_ForTestOnly(TxnHeader);
			AssertTxnHeaderValues(300m, 30m, 330m, 100m, 10m, 110m, 200m, 20m, 220m);

			SetAmountsOnTestTransaction(300m, 30m, 0m, 100m, 10m, 0m, 200m, 20m, 0m);
			DataAdapter.PopulateMissingHeaderAmounts_ForTestOnly(TxnHeader);
			AssertTxnHeaderValues(300m, 30m, 330m, 100m, 10m, 110m, 200m, 20m, 220m);

			SetAmountsOnTestTransaction(300m, 0m, 0m, 100m, 0m, 0m, 200m, 0m, 0m);
			DataAdapter.PopulateMissingHeaderAmounts_ForTestOnly(TxnHeader);
			AssertTxnHeaderValues(300m, 0m, 0m, 100m, 0m, 0m, 200m, 0m, 0m);

			SetAmountsOnTestTransaction(0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m);
			DataAdapter.PopulateMissingHeaderAmounts_ForTestOnly(TxnHeader);
			AssertTxnHeaderValues(0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m);
		}

		public void TestNewBusinessObject()
		{
			Xsd.TxnHeaderDirect txnHeader = new Xsd.TxnHeaderDirect();
			NotificationBuffer notifications = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notifications);

			txnHeader.Ledger = Xsd.TxnLedgerType.CB;
			txnHeader.TxnType = Xsd.TxnType.DPY;
			AssertEquals("New Business Object Type", typeof(DirectPayment), DataAdapter.NewBusinessObject_ForTestOnly(txnHeader, context).GetType());

			txnHeader.TxnType = Xsd.TxnType.DRC;
			AssertEquals("New Business Object Type", typeof(DirectReceipt), DataAdapter.NewBusinessObject_ForTestOnly(txnHeader, context).GetType());

			txnHeader.Ledger = Xsd.TxnLedgerType.CB;
			txnHeader.TxnType = Xsd.TxnType.PAY;
			AssertEquals("New Business Object Type", null, DataAdapter.NewBusinessObject_ForTestOnly(txnHeader, context));
		}

		[TestDate(2006, 01, 05)]
		public void TestImportFromValueObject()
		{
			Env.Security.NewCashBookAllowGLAccountWithoutMapping.IsAllowed = true;
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = true;

			NotificationBuffer notifications = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notifications);

			TxnHeader.Ledger = Xsd.TxnLedgerType.CB;
			TxnHeader.TxnType = Xsd.TxnType.DPY;
			DirectTransactionHeaderBase transactionHeader = DataAdapter.NewBusinessObject_ForTestOnly(TxnHeader, context);
			AssertEquals("Precondition: TransactionHeader Type", typeof(DirectPayment), transactionHeader.GetType());
			DataAdapter.ImportFromValueObject(transactionHeader, TxnHeader, context);

			AssertNotContains("Notifications", "Error", notifications.AsString);
			AssertEquals("Header should not be deleted", false, transactionHeader.IsDeleted);

			AssertEquals("Invoice Date", TxnHeader.InvoiceDate, transactionHeader.AH_InvoiceDate);
			AssertEquals("Bank Code", TxnHeader.BankCode, transactionHeader.BankAccount.AB_Code);
			AssertEquals("Currency", transactionHeader.BankAccount.AB_RX_NKAccountCurrency, transactionHeader.AH_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate", 1.0m, transactionHeader.AH_ExchangeRate);
			AssertEquals("Payment Type", TxnHeader.ReceiptPaymentType.ToString(), transactionHeader.AH_ReceiptType.ToString());
			AssertEquals("Cheque/Reference Number", TxnHeader.ChequeOrReference, transactionHeader.AH_ChequeOrReference);
			AssertEquals("Post Date", TxnHeader.PostDate, transactionHeader.AH_PostDate);
			AssertEquals("Payee Name", TxnHeader.ChequeDrawer, transactionHeader.AH_ChequeDrawer);
			AssertEquals("Account Number", ZString.Empty, transactionHeader.AH_DrawerBank);
			AssertEquals("BSB Number", ZString.Empty, transactionHeader.AH_DrawerBranch);
		}

		public void TestImportReversalOfPayment()
		{
			TxnHeader.TxnType = Xsd.TxnType.DPY;
			DataAdapter.InvertSigns_ForTestOnly(TxnHeader);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			TxnHeader.OsInvoiceAmtExclTax.Value = 10m;
			DirectTransactionHeaderBase transaction = DataAdapter.NewBusinessObject_ForTestOnly(TxnHeader, context);
			AssertNewBizObjCreatedCorrectly(TxnHeader, Xsd.TxnType.DPY, transaction, typeof(DirectPayment));

			TxnHeader.OsInvoiceAmtExclTax.Value = -10m;
			transaction = DataAdapter.NewBusinessObject_ForTestOnly(TxnHeader, context);
			AssertNewBizObjCreatedCorrectly(TxnHeader, Xsd.TxnType.DRC, transaction, typeof(DirectReceipt));

			TxnHeader.OsInvoiceAmtExclTax.Value = 10m;
			transaction = DataAdapter.NewBusinessObject_ForTestOnly(TxnHeader, context);
			AssertNewBizObjCreatedCorrectly(TxnHeader, Xsd.TxnType.DRC, transaction, typeof(DirectReceipt));

			TxnHeader.OsInvoiceAmtExclTax.Value = -10m;
			transaction = DataAdapter.NewBusinessObject_ForTestOnly(TxnHeader, context);
			AssertNewBizObjCreatedCorrectly(TxnHeader, Xsd.TxnType.DPY, transaction, typeof(DirectPayment));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCashbookTransactionsXmlImportWithInvalidXml()
		{
			var importer = new XmlDataImporter(new SingleBusinessObjectFactoryProvider(Factory), DataAdapter);
			var fileName = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\DirectReceiptPayment\Testing\InvalidFormatEmptyBankAccount.xml";

			var notifications = new NotificationBuffer();
			importer.ImportData(fileName, notifications, new SourceInfo(BillingDataSource.InterfaceConnector, BillingInterfaceName.CashbookTransactionsXmlImport, ZGuid.Empty, ZGuid.Empty, ZString.Empty, fileName));
			TestHelper.AssertNotificationsContainsErrorMessage(notifications, "Error: Bank Account: Please enter a Bank Account.");
		}

		void AssertNewBizObjCreatedCorrectly(Xsd.TxnHeader txnHeader, Xsd.TxnType expectedTxnType,
			DirectTransactionHeaderBase transaction, Type expectedType)
		{
			AssertNotNull("Transaction", transaction);
			AssertNotNull("TxnHeader", txnHeader);

			AssertEquals("TxnHeader.TxnType", expectedTxnType, txnHeader.TxnType);
			AssertEquals("Transaction", expectedType, transaction.GetType());
			AssertEquals("TxnHeader.OsInvoiceAmtExclTax.Value", 10m, txnHeader.OsInvoiceAmtExclTax.Value);
		}

		protected override Type GetDataAdapterType()
		{
			return typeof(DirectReceiptPaymentDataAdapter);
		}

		protected override bool IsExportToValueObjectSupported
		{
			get { return false; }
		}

		protected override bool IsImportFromValueObjectSupported
		{
			get { return true; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "TxnHeader"; }
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "TxnHeaders"; }
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return null;
			// DirectTransactionHeaderBase TransactionHeader = Factory.New<DirectPayment>();
			// return new BusinessObjectAndExpectedOutputFileName(TransactionHeader, "", ValidationKind.Xsd, "Empty Direct Payment");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return null;
		}

		protected override ValueObjectDataAdapter<DirectTransactionHeaderBase, Xsd.TxnHeaderDirect> GetNewBizObjXmlDataAdapter()
		{
			return new DirectReceiptPaymentDataAdapter();
		}

		protected override void SetUp()
		{
			base.SetUp();
			DataAdapter = new DirectReceiptPaymentDataAdapter();
			TxnHeader = new Xsd.TxnHeaderDirect();
			PopulateTxnHeaderValuesForImportTest();
			Factory.Save();
		}

		void CreateBusinessObjectsForTest()
		{
			USDBankAccount = ObjectCreator.USDBankAccount;
			USDBankAccount.AB_Code = "USDBANK";

			USDChequeBook = ObjectCreator.USDChequeBook;
			USDChequeBook.AK_Code = "USDCB1";

			AUDBankAccount = ObjectCreator.AUDBankAccount;
			AUDBankAccount.AB_Code = "AUDBANK";
			AUDBankAccount.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			AUDChequeBook = ObjectCreator.AUDChequeBook;
			AUDChequeBook.AK_Code = "AUDCB1";

			GLHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			GLHeader1.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			GLHeader1.AG_AccountNum = "1111.11.11";

			GLHeader2 = Factory.NewWithValidTestData<AccGLHeader>();
			GLHeader2.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			GLHeader2.AG_AccountNum = "1111.11.12";

			GlbCompany company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			Branch = company.Branches.AddNew();
			Branch.GB_Code = "BR1";

			Department = Factory.NewWithValidTestData<GlbDepartment>();
			Department.GE_Code = "DE1";

			TaxRate1 = ObjectCreator.GST1;
			TaxRate1.AT_Code = "GST1";

			TaxRate2 = ObjectCreator.GSTFREE1;
			TaxRate2.AT_Code = "GSTFREE1";

			Factory.Save();
		}

		protected override DirectTransactionHeaderBase NewBusinessObject()
		{
			return Factory.NewWithValidTestData<DirectPayment>();
		}

		void PopulateTxnHeaderValuesForImportTest()
		{
			TxnHeader.Ledger = Xsd.TxnLedgerType.CB;
			TxnHeader.TxnType = Xsd.TxnType.DPY;
			TxnHeader.InvoiceDate = PostDate.AddDays(-1);
			TxnHeader.BankCode = ObjectCreator.USDBankAccount.AB_Code;
			TxnHeader.Description = "Description for my DPY";
			TxnHeader.TxnNumber = "ABC123";
			TxnHeader.ReceiptPaymentType = Xsd.TxnHeaderReceiptPaymentType.CSH;
			TxnHeader.ChequeOrReference = "0001234";
			TxnHeader.PostDate = PostDate.AddDays(-1);
			TxnHeader.ChequeDrawer = "Payee Name";
			TxnHeader.DrawerBank = "Drawer's Bank";
			TxnHeader.DrawerBankBranch = "Branch of Drawer's Bank";
			TxnHeader.OsInvoiceAmtExclTax.Value = 300m;
			TxnHeader.OsTaxAmount.Value = 30m;
			TxnHeader.OsInvoiceAmtInclTax.Value = 330m;

			AccGLHeader gLHeader = Factory.LoadFromUniqueKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, new ZString("1010.10.10"));
			gLHeader.AG_DisallowDirectPosting = false;

			AddLineForTest(gLHeader, 100m, 10m);
			AddLineForTest(gLHeader, 200m, 20m);
		}

		void AddLineForTest(AccGLHeader gLHeader1, ZDecimal oSAmountExclTax, ZDecimal oSTaxAmount)
		{
			Xsd.TxnLine line = TxnHeader.TxnLines.AddNew();
			line.GLAccount = gLHeader1.AG_AccountNum;
			line.Branch = GlbBranch.CurrentBranch.GB_Code;
			line.Department = GlbDepartment.CurrentDepartment.GE_Code;
			line.Description = "A description of this transaction";
			line.OsInvoiceAmtExclTax.Value = oSAmountExclTax;
			line.TaxCode = ObjectCreator.GST1.AT_Code;
			line.OsTaxAmount.Value = oSTaxAmount;
			line.OsInvoiceAmtInclTax.Value = oSAmountExclTax + oSTaxAmount;
		}

		void SetAmountsOnTestTransaction(
			ZDecimal headerOsInvoiceAmtExclTax, ZDecimal headerOsTaxAmount, ZDecimal headerOsInvoiceAmtInclTax,
			ZDecimal line1OsInvoiceAmtExclTax, ZDecimal line1OsTaxAmount, ZDecimal line1OsInvoiceAmtInclTax,
			ZDecimal line2OsInvoiceAmtExclTax, ZDecimal line2OsTaxAmount, ZDecimal line2OsInvoiceAmtInclTax)
		{
			TxnHeader.OsInvoiceAmtExclTax.Value = headerOsInvoiceAmtExclTax;
			TxnHeader.OsTaxAmount.Value = headerOsTaxAmount;
			TxnHeader.OsInvoiceAmtInclTax.Value = headerOsInvoiceAmtInclTax;

			TxnHeader.TxnLines[0].OsInvoiceAmtExclTax.Value = line1OsInvoiceAmtExclTax;
			TxnHeader.TxnLines[0].OsTaxAmount.Value = line1OsTaxAmount;
			TxnHeader.TxnLines[0].OsInvoiceAmtInclTax.Value = line1OsInvoiceAmtInclTax;

			TxnHeader.TxnLines[1].OsInvoiceAmtExclTax.Value = line2OsInvoiceAmtExclTax;
			TxnHeader.TxnLines[1].OsTaxAmount.Value = line2OsTaxAmount;
			TxnHeader.TxnLines[1].OsInvoiceAmtInclTax.Value = line2OsInvoiceAmtInclTax;
		}

		void AssertTxnHeaderValues(
			ZDecimal headerOsInvoiceAmtExclTax, ZDecimal headerOsTaxAmount, ZDecimal headerOsInvoiceAmtInclTax,
			ZDecimal line1OsInvoiceAmtExclTax, ZDecimal line1OsTaxAmount, ZDecimal line1OsInvoiceAmtInclTax,
			ZDecimal line2OsInvoiceAmtExclTax, ZDecimal line2OsTaxAmount, ZDecimal line2OsInvoiceAmtInclTax)
		{
			AssertEquals(headerOsInvoiceAmtExclTax, TxnHeader.OsInvoiceAmtExclTax.Value);
			AssertEquals(headerOsTaxAmount, TxnHeader.OsTaxAmount.Value);
			AssertEquals(headerOsInvoiceAmtInclTax, TxnHeader.OsInvoiceAmtInclTax.Value);

			AssertEquals(line1OsInvoiceAmtExclTax, TxnHeader.TxnLines[0].OsInvoiceAmtExclTax.Value);
			AssertEquals(line1OsTaxAmount, TxnHeader.TxnLines[0].OsTaxAmount.Value);
			AssertEquals(line1OsInvoiceAmtInclTax, TxnHeader.TxnLines[0].OsInvoiceAmtInclTax.Value);

			AssertEquals(line2OsInvoiceAmtExclTax, TxnHeader.TxnLines[1].OsInvoiceAmtExclTax.Value);
			AssertEquals(line2OsTaxAmount, TxnHeader.TxnLines[1].OsTaxAmount.Value);
			AssertEquals(line2OsInvoiceAmtInclTax, TxnHeader.TxnLines[1].OsInvoiceAmtInclTax.Value);
		}

		NotificationTestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new NotificationTestHelper()); }
		}
		NotificationTestHelper testHelper;

		Xsd.TxnHeaderDirect TxnHeader;
		DirectReceiptPaymentDataAdapter DataAdapter;
		AccBankAccount USDBankAccount;
		AccChequeBook USDChequeBook;
		AccBankAccount AUDBankAccount;
		AccChequeBook AUDChequeBook;
		AccGLHeader GLHeader1;
		AccGLHeader GLHeader2;
		GlbBranch Branch;
		GlbDepartment Department;
		AccTaxRate TaxRate1;
		AccTaxRate TaxRate2;
	}
}
