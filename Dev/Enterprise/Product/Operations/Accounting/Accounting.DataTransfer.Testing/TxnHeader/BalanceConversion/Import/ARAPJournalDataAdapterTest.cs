using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.GUI;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Testing.TxnHeader.BalanceConversion.Import
{
	[TestedType(typeof(ARAPJournalDataAdapter))]
	sealed class ARAPJournalDataAdapterTest : BaseAccountingDataAdapterTest<Journal, Xsd.TxnHeader>
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportInvalidJornalType()
		{
			TestCaseHelper.ClearTable(AccTransactionHeaderSchema.Constants.TableName);
			AssertEquals("Precondition: AccTransactionHeader Database Count", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));

			ZString fileName = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\BalanceConversion\Testing\InvValidJournalType.xml";
			NotificationBuffer buffer = new NotificationBuffer();

			XmlDataTransferDirector transferDirector = new TestXmlDataTransferDirector(DataAdapter, false);
			transferDirector.Import(fileName, buffer, SourceInfo.EmptySourceInfo);

			ZString expectedLine = "Error: This transaction cannot be imported. Transaction type is not compatible.";
			AssertContains("Import Notifications", expectedLine, buffer.AsString);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportForeignCurrencyTransactions()
		{
			TestCaseHelper.ClearTable(AccTransactionHeaderSchema.Constants.TableName);
			AssertEquals("Precondition: AccTransactionHeader Database Count", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));

			ZString fileName = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\BalanceConversion\Testing\ForeignCurrencyJournals.xml";
			NotificationBuffer buffer = new NotificationBuffer();
			XmlDataTransferDirector transferDirector = new XmlDataTransferDirector(DataAdapter, false);
			transferDirector.Import(fileName, buffer, SourceInfo.EmptySourceInfo);

			AssertNotContains("Import Notifications", "Error", buffer.AsString);
			AssertEquals("AccTransactionHeader Database Count", 2, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));

			ZQuery findUSDJournalQuery = new ZQuery(AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, ObjectCreator.USD.RX_Code);
			findUSDJournalQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

			Journal journal = Factory.LoadTop1<ARJournal>(findUSDJournalQuery);

			AssertHeaderValues(journal, "FABFIT", new ZDateTime(2005, 01, 01), ObjectCreator.USD, 2m,
				"CRD:00001151:AR CREDIT NOTE", new ZDateTime(2005, 01, 01), 240, 120, "BRN", "BNE", "6810.00.00", "DR");

			ZQuery findAUDPaymentQuery = new ZQuery(AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, ObjectCreator.AUD.RX_Code);
			findAUDPaymentQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			journal = Factory.LoadTop1<ARJournal>(findAUDPaymentQuery);

			AssertHeaderValues(journal, "FABFIT", new ZDateTime(2005, 01, 01), ObjectCreator.AUD, 1m,
				"INV:00001169:AR INVOICE", new ZDateTime(2005, 01, 01), 125.3, 125.3, "BRN", "BNE", "6810.00.00", "CR");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportValidTransactions()
		{
			TestCaseHelper.ClearTable(AccTransactionHeaderSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionLinesSchema.Constants.TableName);
			AssertEquals("Precondition: AccTransactionHeader Database Count", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));

			ZString fileName = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\BalanceConversion\Testing\ValidJournals.xml";
			NotificationBuffer buffer = new NotificationBuffer();
			XmlDataTransferDirector transferDirector = new XmlDataTransferDirector(DataAdapter, false);
			transferDirector.Import(fileName, buffer, SourceInfo.EmptySourceInfo);

			AssertNotContains("Import Notifications", "Error", buffer.AsString);
			AssertEquals("AccTransactionHeader Database Count", 8, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));

			ZQuery findUSDJournalQuery = new ZQuery(AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, ObjectCreator.USD.RX_Code);
			AssertEquals("AccTransactionHeader Database Count", 3, Factory.GetDatabaseCount(typeof(Journal), findUSDJournalQuery));

			ZQuery findAUDPaymentQuery = new ZQuery(AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, ObjectCreator.AUD.RX_Code);
			AssertEquals("AccTransactionHeader Database Count", 5, Factory.GetDatabaseCount(typeof(Journal), findAUDPaymentQuery));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[GuiTest]
		public void TestImportTransactionsWithOnlySaveDataWhenNoError()
		{
			TestCaseHelper.ClearTable(AccTransactionHeaderSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionLinesSchema.Constants.TableName);
			AssertEquals("Precondition: AccTransactionHeader Database Count", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));

			ZString fileName = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\BalanceConversion\Testing\JournalsWithError.xml";
			NotificationBuffer buffer = new NotificationBuffer();

			ZQuery query = new ZQuery(OrgHeaderSchema.OH_Code, "ZZZ");
			OrgHeader errorOrg = Factory.LoadTop1<OrgHeader>(query);
			AssertNull(errorOrg);
			DataImporterBusinessObject businessEntity = new DataImporterBusinessObject(new BusinessObjectFactory());

			using (TestXMLDataImporterForm form = new TestXMLDataImporterForm(businessEntity, "XML Data Importer", BillingInterfaceName.JournalXmlImport))
			{
				DataImporter importer = new XmlDataImporter(new ARAPJournalDataAdapter());
				form.Importer = importer;

				form.Show();
				Application.DoEvents();
				form.SetParametersOfOnlySaveDataWhenNoRecordsHaveErrorsCheckBox(true, true);
				form.ImportFromFile(fileName);

				query = new ZQuery(AccTransactionHeaderSchema.AH_Desc, "This is the org with no error.");
				query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				AccTransactionHeader tranHeader = Factory.LoadTop1<AccTransactionHeader>(query);
				AssertNull(tranHeader);

				form.SetParametersOfOnlySaveDataWhenNoRecordsHaveErrorsCheckBox(false, true);
				form.ImportFromFile(fileName);

				query = new ZQuery(AccTransactionHeaderSchema.AH_Desc, "This is the org with no error.");
				query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				tranHeader = Factory.LoadTop1<AccTransactionHeader>(query);
				AssertNotNull(tranHeader);
			}
		}

		public void TestNewBusinessObject_ForTestOnly()
		{
			Xsd.TxnHeader txnHeader = new Xsd.TxnHeader();
			NotificationBuffer notifications = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notifications);

			txnHeader.Ledger = Xsd.TxnLedgerType.AP;
			txnHeader.TxnType = Xsd.TxnType.JNL;
			AssertEquals("New Business Object Type", typeof(APJournal), DataAdapter.NewBusinessObject_ForTestOnly(txnHeader, context).GetType());

			txnHeader.Ledger = Xsd.TxnLedgerType.AR;
			AssertEquals("New Business Object Type", typeof(ARJournal), DataAdapter.NewBusinessObject_ForTestOnly(txnHeader, context).GetType());

			txnHeader.Ledger = Xsd.TxnLedgerType.AP;
			txnHeader.TxnType = Xsd.TxnType.PAY;
			AssertEquals("New Business Object Type", null, DataAdapter.NewBusinessObject_ForTestOnly(txnHeader, context));
		}

		[TestDate(2006, 01, 05)]
		public void TestImportFromValueObject()
		{
			Environment.Env.Security.NewPayablesAllowGLAccountWithoutMapping.IsAllowed = true;

			NotificationBuffer notifications = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notifications);

			TxnHeader.Ledger = Xsd.TxnLedgerType.AP;
			TxnHeader.TxnType = Xsd.TxnType.JNL;
			Journal journal = DataAdapter.NewBusinessObject_ForTestOnly(TxnHeader, context);
			AssertEquals("Precondition: TransactionHeader Type", typeof(APJournal), journal.GetType());
			DataAdapter.ImportFromValueObject(journal, TxnHeader, context);

			AssertNotContains("Notifications", "Error", notifications.AsString);
			AssertEquals("Header should not be deleted", false, journal.IsDeleted);

			AssertHeaderValues(journal, TxnHeader, 2M, "CR");

			TxnHeader.OsInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(-200), ObjectCreator.GBP);
			TxnHeader.OsInvoiceAmtExclTax = TxnHeader.OsInvoiceAmtInclTax;
			TxnHeader.LocalInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(-100), ObjectCreator.GBP);
			TxnHeader.LocalInvoiceAmtExclTax = TxnHeader.LocalInvoiceAmtInclTax;

			notifications.Clear();
			journal = DataAdapter.NewBusinessObject_ForTestOnly(TxnHeader, context);
			AssertEquals("Precondition: TransactionHeader Type", typeof(APJournal), journal.GetType());
			DataAdapter.ImportFromValueObject(journal, TxnHeader, context);

			AssertNotContains("Notifications", "Error", notifications.AsString);
			AssertEquals("Header should not be deleted", false, journal.IsDeleted);

			AssertHeaderValues(journal, TxnHeader, 2M, "DR");
		}

		[TestDate(2006, 01, 05)]
		public void TestValidationAfterImportFromValueObject()
		{
			Environment.Env.Security.NewPayablesAllowGLAccountWithoutMapping.IsAllowed = true;
			Environment.Env.Security.NewReceivablesAllowGLAccountWithoutMapping.IsAllowed = true;

			NotificationBuffer notifications = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notifications);
			OrganisationValueObjectDataAdapter organisationAdapter = new OrganisationValueObjectDataAdapter();

			TxnHeader.Ledger = Xsd.TxnLedgerType.AP;
			TxnHeader.TxnType = Xsd.TxnType.JNL;

			//OH_IsCreditor = false
			OrgHeader testOrgHeader = Factory.LoadFromUniqueKey<OrgHeader>(OrgHeaderSchema.OH_Code, new ZString("AASDRA"));
			AssertNotNull(testOrgHeader);
			notifications.Clear();
			Journal journal = DataAdapter.NewBusinessObject_ForTestOnly(TxnHeader, context);
			AssertEquals("Precondition: TransactionHeader Type", typeof(APJournal), journal.GetType());
			TxnHeader.DebtorOrCreditor = organisationAdapter.ExportToValueObject(testOrgHeader, new ValueObjectExportContext(new NotificationBuffer()));
			TxnHeader.DebtorOrCreditorGUID = testOrgHeader.PK.ToString();
			DataAdapter.ImportFromValueObject(journal, TxnHeader, context);
			AssertContains("Notifications", "Error", notifications.AsString);
			AssertEquals("Header should be deleted", true, journal.IsDeleted);
			AssertEquals("Error Type should be prevent save", false, notifications.ContainsNotificationType(ErrorType.DataErrorPreventSave));

			//OH_IsCreditor = true
			testOrgHeader = Factory.LoadFromUniqueKey<OrgHeader>(OrgHeaderSchema.OH_Code, new ZString("AALSHI"));
			AssertNotNull(testOrgHeader);
			testOrgHeader.OH_IsCreditor = true;
			notifications.Clear();
			journal = DataAdapter.NewBusinessObject_ForTestOnly(TxnHeader, context);
			AssertEquals("Precondition: TransactionHeader Type", typeof(APJournal), journal.GetType());
			TxnHeader.DebtorOrCreditor = organisationAdapter.ExportToValueObject(testOrgHeader, new ValueObjectExportContext(new NotificationBuffer()));
			TxnHeader.DebtorOrCreditorGUID = testOrgHeader.PK.ToString();
			DataAdapter.ImportFromValueObject(journal, TxnHeader, context);
			AssertNotContains("Notifications", "Error", notifications.AsString);
			AssertEquals("Header should not be deleted", false, journal.IsDeleted);
			AssertHeaderValues(journal, TxnHeader, 2M, "CR");

			TxnHeader.Ledger = Xsd.TxnLedgerType.AR;

			//OH_IsDebtor = true
			testOrgHeader = Factory.LoadFromUniqueKey<OrgHeader>(OrgHeaderSchema.OH_Code, new ZString("ABIGAS"));
			AssertNotNull(testOrgHeader);
			testOrgHeader.OH_IsDebtor = true;
			notifications.Clear();
			journal = DataAdapter.NewBusinessObject_ForTestOnly(TxnHeader, context);
			AssertEquals("Precondition: TransactionHeader Type", typeof(ARJournal), journal.GetType());
			TxnHeader.DebtorOrCreditor = organisationAdapter.ExportToValueObject(testOrgHeader, new ValueObjectExportContext(new NotificationBuffer()));
			TxnHeader.DebtorOrCreditorGUID = testOrgHeader.PK.ToString();
			DataAdapter.ImportFromValueObject(journal, TxnHeader, context);
			AssertNotContains("Notifications", "Error", notifications.AsString);
			AssertEquals("Header should not be deleted", false, journal.IsDeleted);
			AssertHeaderValues(journal, TxnHeader, 2M, "CR");

			//OH_IsDebtor = false
			testOrgHeader = Factory.LoadFromUniqueKey<OrgHeader>(OrgHeaderSchema.OH_Code, new ZString("AASDRA"));
			AssertNotNull(testOrgHeader);
			testOrgHeader.OH_IsDebtor = false;
			notifications.Clear();
			journal = DataAdapter.NewBusinessObject_ForTestOnly(TxnHeader, context);
			AssertEquals("Precondition: TransactionHeader Type", typeof(ARJournal), journal.GetType());
			TxnHeader.DebtorOrCreditor = organisationAdapter.ExportToValueObject(testOrgHeader, new ValueObjectExportContext(new NotificationBuffer()));
			TxnHeader.DebtorOrCreditorGUID = testOrgHeader.PK.ToString();
			DataAdapter.ImportFromValueObject(journal, TxnHeader, context);
			AssertContains("Notifications", "Error", notifications.AsString);
			AssertEquals("Header should be deleted", true, journal.IsDeleted);
		}

		#region Implementation

		void AssertHeaderValues(Journal header, Xsd.TxnHeader txnHeader, ZDecimal exchangeRate, ZString debitOrCredit)
		{
			int multiplier = debitOrCredit == "CR" ? 1 : -1;

			AssertEquals("Organisation", txnHeader.DebtorOrCreditor.EDICode, header.Header.OH_Code);
			AssertEquals("Invoice Date", txnHeader.InvoiceDate, header.AH_InvoiceDate);
			AssertEquals("Post Date", ZDate.Today, header.AH_PostDate.Date);
			AssertEquals("DueDate", txnHeader.DueDate, header.AH_DueDate);
			AssertEquals("Description", txnHeader.Description, header.AH_Desc);

			string leger = txnHeader.Ledger.ToString();
			switch (leger)
			{
				case LedgerTypes.AccountsPayable:
					AssertEquals("GLAccount", AccountingConfigurationRegistry.Instance.APJournalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), header.AH_AG);
					break;
				case LedgerTypes.AccountsReceivable:
					AssertEquals("GLAccount", AccountingConfigurationRegistry.Instance.ARJournalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), header.AH_AG);
					break;
				default:
					Assert("Invalid Ledger type", false);
					break;
			}

			AssertEquals("Branch", txnHeader.Branch, header.Branch.GB_Code);
			AssertEquals("Department", txnHeader.Department, header.Department.GE_Code);
			AssertEquals("Local Amount", multiplier * txnHeader.LocalInvoiceAmtInclTax.Value, header.AH_LocalTotalAmount);
			AssertEquals("OS Amount", multiplier * txnHeader.OsInvoiceAmtInclTax.Value, header.AH_OSTotalAmount);
			AssertEquals("Currency Code", txnHeader.OsInvoiceAmtInclTax.CurrencyCode, header.AH_RX_NKTransactionCurrency);
			AssertEquals("ExchangeRate", exchangeRate, header.AH_ExchangeRate);
			AssertEquals("Debit/Credit", debitOrCredit, header.DebitCreditSign);
		}

		void AssertHeaderValues(Journal header, ZString org_Code, ZDateTime invoiceDate, RefCurrency currency,
			ZDecimal exchangeRate, ZString description, ZDateTime dueDate, ZDecimal oSTotalAmount,
			ZDecimal localTotalAmount, ZString department_Code, ZString branch_Code, ZString gLAccountNumber,
			ZString debitOrCredit)
		{
			AssertEquals("Organisation", org_Code, header.Header.OH_Code);
			AssertEquals("Invoice Date", invoiceDate.Date, header.AH_InvoiceDate.Date);
			AssertEquals("Post Date", ZDate.Today, header.AH_PostDate.Date);
			AssertEquals("Description", description, header.AH_Desc);
			AssertEquals("DueDate", dueDate.Date, header.AH_DueDate.Date);
			AssertEquals("GLAccount", gLAccountNumber, header.GLHeader.AG_AccountNum);
			AssertEquals("Branch", branch_Code, header.Branch.GB_Code);
			AssertEquals("Department", department_Code, header.Department.GE_Code);
			AssertEquals("Local Amount", localTotalAmount, header.AH_LocalTotalAmount);
			AssertEquals("OS Amount", oSTotalAmount, header.AH_OSTotalAmount);
			AssertEquals("Currency", currency.RX_Code, header.AH_RX_NKTransactionCurrency);
			AssertEquals("ExchangeRate", exchangeRate, header.AH_ExchangeRate);
			AssertEquals("Debit/Credit", debitOrCredit, header.DebitCreditSign);
		}

		#region Overriden

		protected override Type GetDataAdapterType()
		{
			return typeof(ARAPJournalDataAdapter);
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
			get { return "FinancialInvoice"; }
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "FinancialTransactions"; }
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return null;
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

		protected override ValueObjectDataAdapter<Journal, Xsd.TxnHeader> GetNewBizObjXmlDataAdapter()
		{
			return new ARAPJournalDataAdapter();
		}

		protected override Journal NewBusinessObject()
		{
			return Factory.NewWithValidTestData<APJournal>();
		}

		protected override void SetUp()
		{
			base.SetUp();
			DataAdapter = new ARAPJournalDataAdapter();
			TxnHeader = new Xsd.TxnHeader();
			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();
			PopulateTxnHeaderValuesForImportTest();
			Factory.Save();
		}

		#endregion

		void PopulateTxnHeaderValuesForImportTest()
		{
			TxnHeader.Ledger = Xsd.TxnLedgerType.AP;
			TxnHeader.TxnType = Xsd.TxnType.JNL;
			OrganisationValueObjectDataAdapter organisationAdapter = new OrganisationValueObjectDataAdapter();
			OrgHeader testOrgHeader = Factory.LoadFromUniqueKey<OrgHeader>(OrgHeaderSchema.OH_Code, new ZString("FABGRO")); //Receivables and Payables 
			AssertNotNull(testOrgHeader);

			TxnHeader.DebtorOrCreditor = organisationAdapter.ExportToValueObject(testOrgHeader, new ValueObjectExportContext(new NotificationBuffer()));
			TxnHeader.DebtorOrCreditorGUID = testOrgHeader.PK.ToString();
			TxnHeader.InvoiceDate = PostDate.AddDays(-1);
			TxnHeader.Description = "Description for my JNL";

			TxnHeader.PostDate = PostDate.AddDays(-1);
			TxnHeader.DueDate = PostDate.AddDays(1);
			TxnHeader.GlAccount = "2010.00.00";
			TxnHeader.Branch = GlbBranch.CurrentBranch.GB_Code;
			TxnHeader.Department = GlbDepartment.CurrentDepartment.GE_Code;

			TxnHeader.OsInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(200), ObjectCreator.GBP);
			TxnHeader.OsInvoiceAmtExclTax = TxnHeader.OsInvoiceAmtInclTax;
			TxnHeader.LocalInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(100), ObjectCreator.GBP);
			TxnHeader.LocalInvoiceAmtExclTax = TxnHeader.LocalInvoiceAmtInclTax;
		}

		Xsd.TxnHeader TxnHeader;
		ARAPJournalDataAdapter DataAdapter;

		#endregion
	}

	class TestXMLDataImporterForm : XmlDataImporterForm
	{
		public TestXMLDataImporterForm(DataImporterBusinessObject businessEntity, string formCaption, BillingInterfaceName interfaceName)
			: base(businessEntity, formCaption, interfaceName)
		{
		}

		internal new void ImportFromFile(ZString fileName)
		{
			base.ImportFromFile(fileName);
		}
	}

	class TestXmlDataTransferDirector : XmlDataTransferDirector
	{
		public TestXmlDataTransferDirector(IValueObjectDataAdapter adapter, bool checkForPermission)
			: base(adapter, checkForPermission)
		{
		}

		protected override XmlDataImporter NewXmlDataImporter()
		{
			return new TestDirectorXmlDataImporter(this);
		}

		class TestDirectorXmlDataImporter : DirectorXmlDataImporter
		{
			public TestDirectorXmlDataImporter(TestXmlDataTransferDirector outer)
				: base(outer)
			{
				OnlySaveDataWhenNoRecordsHaveErrors = true;
			}
		}
	}
}
