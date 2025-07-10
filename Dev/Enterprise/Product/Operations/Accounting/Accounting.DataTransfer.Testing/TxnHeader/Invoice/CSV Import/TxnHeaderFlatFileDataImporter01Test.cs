using System.IO;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Testing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.DataTransfer.Invoices.Testing;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Invoices.FlatFile.Testing
{
	sealed class TxnHeaderFlatFileDataImporter01Test : TestCaseWithFactory
	{
		public void TestCreateConverter()
		{
			AssertEquals(typeof(TxnHeaderFlatFileConverter), fImporter.CreateConverter_ForTestOnly(null).GetType());
		}

		public void TestCreateXsd()
		{
			AssertEquals(typeof(TxnHeaderCollection), fImporter.CreateXsd_ForTestOnly().GetType());
		}

		public void TestFlatFileFormat()
		{
			AssertEquals(typeof(CsvFlatFileFormat), fImporter.FlatFileFormat_ForTestOnly.GetType());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInvalidTransactionsAreDeletedBeforeSave()
		{
			APInvoice invoice = new BusinessObjectFactory().New<APInvoice>();
			TestObjectCreator creator = new TestObjectCreator(invoice.Factory);
			invoice.AH_OH = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "EAGDAT").PK;
			invoice.AH_TransactionNum = "123456789ABC";
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			line.AL_AG = creator.GLHeader1.PK;
			line.AL_OSExTaxAmount = 100m;
			invoice.Factory.Save();

			TxnHeaderCollection txnHeaderCollection = new TxnHeaderCollection();
			TxnHeaderFlatFileDataImporter importer = new TxnHeaderFlatFileDataImporter(Factory);
			importer.RunExtraValidation = true;
			string testFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\APInvoiceWithoutAttachment.csv";
			using (StreamReader reader = new StreamReader(testFilePath))
			{
				importer.ImportData(testFilePath, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			}
			Assert(importer.ImportedInvoice.IsDeleted);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[SuspendCriticalValidation]
		public void TestInvoiceHeaderIsImportedCorrectly()
		{
			ZString testFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\ARInvoiceWithoutAttachment.csv";
			var currCompany = GlbCompany.CurrentCompany;

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_Code = "EAGDAT_AU";
			organisation.CompanyData.OB_IsDebtor = ZBool.True;
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_Code = "BR1";
			newBranch.GB_GC = currCompany.PK;
			var newDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			newDepartment.GE_Code = "DP1";
			var newCurrency = Factory.NewWithValidTestData<RefCurrency>();
			newCurrency.RX_Code = "BLA";
			currCompany.GC_RX_NKLocalCurrency = newCurrency.RX_Code;

			Factory.Save();

			var txnHeaderCollection = new TxnHeaderCollection();
			var converter = new TxnHeaderFlatFileConverter(null, Factory);
			var format = new CsvFlatFileFormat(false);
			using (var reader = new StreamReader(testFilePath))
			{
				converter.ImportFlatFile(txnHeaderCollection, format, reader);
			}

			var attachment = txnHeaderCollection[0].Attachments.AddNew();
			attachment.FilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\TransactionHeaderBuilder\Testing\Test Attachment.pdf";

			var notifBuffer = new NotificationBuffer();
			fImporter.ExtractToDataAdapter_ForTestOnly(txnHeaderCollection, notifBuffer);
			var invoice = fImporter.FLastImportedInvoice_ForTestOnly;

			AssertEquals("Transaction Ledger", LedgerTypes.AccountsReceivable, invoice.AH_Ledger);
			AssertEquals("Transaction Type", TransactionTypes.Invoice, invoice.AH_TransactionType);
			AssertEquals("Debtor Code", organisation.PK, invoice.AH_OH);
			AssertEquals("Branch Code", newBranch.PK, invoice.AH_GB);
			AssertEquals("Department Code", newDepartment.PK, invoice.AH_GE);
			var aUDCur = RefCurrency.LoadFromCurrencyCode(Factory, invoice.AH_RX_NKTransactionCurrency);
			AssertEquals("Currency Code", "AUD", aUDCur.RX_Code);
			Assert("SubmittedFromInvoicingForm must be false", !invoice.SubmittedFromInvoicingForm);

			AssertEquals("Invoice must have 1 eDoc", 1, invoice.DocManagerInfo.AllEDocs.Count);
			var newFactory = new BusinessObjectFactory();
			var invoiceInNewFactory = newFactory.Load<InvoicingBase>(invoice.PK);
			AssertEquals("Invoice in New Factory must have 1 eDoc also", 1, invoiceInNewFactory.DocManagerInfo.AllEDocs.Count);

			testFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\APInvoiceWithoutAttachment.csv";
			using (var reader = new StreamReader(testFilePath))
			{
				converter.ImportFlatFile(txnHeaderCollection, format, reader);
			}
			fImporter.ExtractToDataAdapter_ForTestOnly(txnHeaderCollection, notifBuffer);
			invoice = fImporter.FLastImportedInvoice_ForTestOnly;
			AssertEquals("Transaction Ledger", LedgerTypes.AccountsPayable, invoice.AH_Ledger);
			AssertEquals("Transaction Type", TransactionTypes.Invoice, invoice.AH_TransactionType);

			testFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\ARCreditNote.csv";
			txnHeaderCollection = new TxnHeaderCollection();
			using (var reader = new StreamReader(testFilePath))
			{
				converter.ImportFlatFile(txnHeaderCollection, format, reader);
			}
			fImporter.ExtractToDataAdapter_ForTestOnly(txnHeaderCollection, notifBuffer);
			invoice = fImporter.FLastImportedInvoice_ForTestOnly;
			AssertEquals("Transaction Ledger", LedgerTypes.AccountsReceivable, invoice.AH_Ledger);
			AssertEquals("Transaction Type", TransactionTypes.CreditNote, invoice.AH_TransactionType);
			Assert("OriginalReferenceStartDate empty", invoice.AH_OriginalReferenceStartDate.IsEmpty);
			Assert("OriginalReferenceEndDate empty", invoice.AH_OriginalReferenceEndDate.IsEmpty);

			using (InvoiceBaseValidationTest.InitaliseComplianceFactory(areOriginalTransactionReferenceFieldsMandatory: true))
			{
				fImporter.ExtractToDataAdapter_ForTestOnly(txnHeaderCollection, notifBuffer);
				invoice = fImporter.FLastImportedInvoice_ForTestOnly;
				AssertEquals("OriginalReferenceStartDate mandatory", invoice.AH_InvoiceDate, invoice.AH_OriginalReferenceStartDate);
				AssertEquals("OriginalReferenceEndDate mandatory", invoice.AH_InvoiceDate, invoice.AH_OriginalReferenceEndDate);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2005, 09, 01)]
		public void TestImportSuccessful_WhenHeaderCurrencyIsDifferentToLocalCurrency()
		{
			ZString testFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\APWithDifferentHeaderCurrencyThanLocal.csv";
			TxnHeaderCollection txnHeaderCollection = new TxnHeaderCollection();
			TxnHeaderFlatFileConverter converter = new TxnHeaderFlatFileConverter(null, Factory);

			using (StreamReader reader = new StreamReader(testFilePath))
			{
				converter.ImportFlatFile(txnHeaderCollection, new CsvFlatFileFormat(false), reader);
			}

			fImporter.ExtractToDataAdapter_ForTestOnly(txnHeaderCollection, new NotificationBuffer());
			var invoice = fImporter.FLastImportedInvoice_ForTestOnly;

			AssertNotNull("Invoice created successfully", invoice);
			AssertEquals("Invoice created with one line", 1, invoice.Lines.Count);

			AssertEquals("Local Currency is AUD", "AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertEquals("Transaction Currency is IDR", "IDR", invoice.AH_RX_NKTransactionCurrency);

			AssertEquals("OS Ex tax amount", 3000M, invoice.AH_OSExTaxAmount);
			AssertEquals("OS tax amount", 500M, invoice.AH_OSTaxAmount);
			AssertEquals("OS total amount", 3500M, invoice.AH_OSTotalAmount);

			AssertEquals("OS Ex tax amount on line", 3000M, invoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals("OS tax amount on line", 500M, invoice.Lines[0].AL_OSTaxAmount);
			AssertEquals("OS amount on line", -3500M, invoice.Lines[0].AL_OSAmount);

			AssertEquals("Local Ex tax amount", 0.48M, invoice.AH_LocalExTaxAmount);
			AssertEquals("Local tax amount", 0.08M, invoice.AH_LocalTaxAmount); //calculated from the OS amounts
			AssertEquals("Local total amount", 0.56M, invoice.AH_LocalTotalAmount);
		}

		TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_innerValue ?? (TestObjectCreator_innerValue = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_innerValue;

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2005, 09, 01)]
		public void TestImportSuccessful_WhenSameHeaderAndLocalCcyAndForeignCcyInConsolCost()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var consol = TestObjectCreator.CreateConsol("AUSYD", "KRSEL", "C00001");

			AccChargeCode chargeCode = TestObjectCreator.CC1;
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			Job shipment1Job = TestObjectCreator.CreateJob(shipment1);
			shipment1Job.PlugInData = shipment1;
			shipment1Job.JH_JobNum = "S00003333";

			JobConsolCost costToImport = consol.GetApportionments().CostsCollection.TryAddNew();
			costToImport.E6_GC = GlbCompany.CurrentCompany.PK;
			costToImport.E6_AC_ChargeCode = chargeCode.PK;
			costToImport.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;
			costToImport.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			costToImport.E6_ExchangeRate = 1.5m;
			costToImport.E6_ApportionmentMethod = "SHP";
			costToImport.E6_OSCostAmount = 100.00M;
			costToImport.E6_LocalCostAmount = 77.1M;
			costToImport.E6_AT_TaxRate = TestObjectCreator.GSTFREE1.PK;

			costToImport.ApportionmentCharges[0].JR_GE = TestObjectCreator.FEADepartment.PK;

			Factory.Save();

			ZString testFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\APWithSameHeaderAndLocalCcyAndForeignCcyInConsolCost.csv";
			TxnHeaderCollection txnHeaderCollection = new TxnHeaderCollection();
			TxnHeaderFlatFileConverter converter = new TxnHeaderFlatFileConverter(null, Factory);

			using (StreamReader reader = new StreamReader(testFilePath))
			{
				converter.ImportFlatFile(txnHeaderCollection, new CsvFlatFileFormat(false), reader);
			}

			fImporter.RunExtraValidation = true;
			fImporter.ImportingSingleTransaction = true;

			var notifications = new NotificationBuffer();

			AssertEquals("Charge in consol has no changes", costToImport.E6_OSCostAmount, 100M);
			fImporter.ExtractToDataAdapter_ForTestOnly(txnHeaderCollection, notifications);
			var invoice = fImporter.FLastImportedInvoice_ForTestOnly;

			AssertNotNull("Invoice created successfully", invoice);
			Assert("No Errors in the notification messages. \nNotifications = " + notifications.AsString, !notifications.HasErrors);
			AssertEquals("Invoice created with one line", 1, invoice.Lines.Count);

			AssertEquals("Local Currency is AUD", "AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertEquals("Transaction Currency is AUD", "AUD", invoice.AH_RX_NKTransactionCurrency);

			AssertEquals("OS Ex tax amount", 150M, invoice.AH_OSExTaxAmount);
			AssertEquals("OS tax amount", 0M, invoice.AH_OSTaxAmount);
			AssertEquals("OS total amount", 150M, invoice.AH_OSTotalAmount);

			AssertEquals("OS Ex tax amount on line", 150M, invoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals("OS tax amount on line", 0M, invoice.Lines[0].AL_OSTaxAmount);
			AssertEquals("OS amount on line", -150M, invoice.Lines[0].AL_OSAmount);

			AssertEquals("Local Ex tax amount", 150M, invoice.AH_LocalExTaxAmount);
			AssertEquals("Local tax amount", 0M, invoice.AH_LocalTaxAmount); //calculated from the OS amounts
			AssertEquals("Local total amount", 150M, invoice.AH_LocalTotalAmount);

			Factory.Save();

			AssertEquals("Charge in consol has been changed", 150M, invoice.ConsolCosting.ConsolCosts[0].E6_OSCostAmount);
			AssertEquals("Currency in consol has been changed", "AUD", invoice.ConsolCosting.ConsolCosts[0].E6_RX_NKCurrency);
			AssertEquals("Exchange rate in consol has been changed", 1M, invoice.ConsolCosting.ConsolCosts[0].E6_ExchangeRate);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 10, 30)]
		public void TestImportWhenConsolCostDoesNothaveAnyShipmentsToApportion()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Now);

			OrgHeader organisation = testObjectCreator.CreateOrgHeader("Test_AU", true, true, false, false, false, false);
			organisation.OH_Code = "Test_AU";
			organisation.Factory.Save();

			var consol = testObjectCreator.CreateConsol();
			AssertEquals("C001", consol.JK_UniqueConsignRef);
			Factory.Save();

			var notify = new NotificationBuffer();
			var txnHeaderCollection = new TxnHeaderCollection();
			var converter = new TxnHeaderFlatFileConverter(notify, Factory);

			var apInvoiceTestFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\APInvWhenConsolWithoutShipment.csv";
			using (var reader = new StreamReader(apInvoiceTestFilePath))
			{
				converter.ImportFlatFile(txnHeaderCollection, new CsvFlatFileFormat(false), reader);
			}

			fImporter.RunExtraValidation = true;
			AssertNoExceptionThrown(() => fImporter.ExtractToDataAdapter_ForTestOnly(txnHeaderCollection, notify));

			var testHelper = new NotificationTestHelper();
			var expectMessage = $"Consol 'C001' does not have any shipments to apportion.";
			testHelper.AssertNotificationsContainsErrorMessage(notify, expectMessage);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 10, 30)]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestImportWhenConsolCostHasErrorOnCreatingJob()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Now);

			OrgHeader organisation = testObjectCreator.CreateOrgHeader("Test_AU", true, true, false, false, false, false);
			organisation.OH_Code = "Test_AU";
			organisation.Factory.Save();

			var consol = testObjectCreator.CreateConsol();
			AssertEquals("C001", consol.JK_UniqueConsignRef);
			var shipment = testObjectCreator.CreateShipment("SMIA0068574", consol);
			Factory.Save();

			// Create Job for the Shipment in new Factory but do not save it
			var newFactory = new BusinessObjectFactory();
			var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			var jobInNewFactory = new Job.Loader(shipmentInNewFactory).TryCreateWithMutex();
			AssertNotNull(jobInNewFactory);

			var notify = new NotificationBuffer();
			var txnHeaderCollection = new TxnHeaderCollection();
			var converter = new TxnHeaderFlatFileConverter(notify, Factory);

			var apInvoiceTestFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\APInvWhenConsolWithoutShipment.csv";
			using (var reader = new StreamReader(apInvoiceTestFilePath))
			{
				converter.ImportFlatFile(txnHeaderCollection, new CsvFlatFileFormat(false), reader);
			}

			AssertExceptionThrown<TransactionException>(() => fImporter.ExtractToDataAdapter_ForTestOnly(txnHeaderCollection, notify));
			jobInNewFactory.Dispose();  // Release Mutex

			var testHelper = new NotificationTestHelper();
			var expectMessage = "You have created the job SMIA0068574 on another form, but haven't saved it yet.";
			testHelper.AssertNotificationsContainsErrorMessage(notify, expectMessage);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2005, 09, 01)]
		public void TestOnlySaveDataWhenNoRecordsHaveErrors()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			creator.CreateTestPeriods(ZDateTime.Now);

			OrgHeader organisation = creator.CreateOrgHeader("EAGDAT_AU", true, true, false, false, false, false);
			organisation.OH_Code = "EAGDAT_AU";
			organisation.Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = organisation.PK;
			invoice.AH_TransactionNum = "123456789ABC";
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			line.AL_AG = creator.GLHeader1.PK;
			line.AL_OSExTaxAmount = 100m;

			creator.CreateNewBranch(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK), "BR1");
			GlbDepartment newDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			newDepartment.GE_Code = "DP1";

			Factory.Save();

			TxnHeaderCollection txnHeaderCollection = new TxnHeaderCollection();
			TxnHeaderFlatFileConverter converter = new TxnHeaderFlatFileConverter(null, Factory);

			string apInvoiceTestFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\APInvoiceWithoutAttachment.csv";
			using (StreamReader reader = new StreamReader(apInvoiceTestFilePath))
			{
				converter.ImportFlatFile(txnHeaderCollection, new CsvFlatFileFormat(false), reader);
			}

			string arInvoiceTestFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\ARInvoiceWithoutAttachment.csv";
			using (StreamReader reader = new StreamReader(arInvoiceTestFilePath))
			{
				converter.ImportFlatFile(txnHeaderCollection, new CsvFlatFileFormat(false), reader);
			}

			BusinessObjectFactory prevFactory = fImporter.FactoryProvider.Current;
			fImporter.RunExtraValidation = true;
			fImporter.OnlySaveDataWhenNoRecordsHaveErrors = true;
			fImporter.ExtractToDataAdapter_ForTestOnly(txnHeaderCollection, new NotificationBuffer());

			AssertEquals("Imported invoice must not be saved as there are errors during import and OnlySaveDataWhenNoRecordsHaveErrors is true.", false, fImporter.FLastImportedInvoice_ForTestOnly.IsInDatabase);
			AssertEquals("New Factory must not be crated.", prevFactory, fImporter.FactoryProvider.Current);
			AssertEquals("The Last Data Transfer was not successful", false, fImporter.WasTheLastDataTransferSuccessful);

			prevFactory = fImporter.FactoryProvider.Current;
			fImporter.OnlySaveDataWhenNoRecordsHaveErrors = false;
			fImporter.ExtractToDataAdapter_ForTestOnly(txnHeaderCollection, new NotificationBuffer());

			AssertEquals("Imported invoice must saved as OnlySaveDataWhenNoRecordsHaveErrors is false.", true, fImporter.FLastImportedInvoice_ForTestOnly.IsInDatabase);
			AssertNotEquals("New Factory must be crated.", prevFactory, fImporter.FactoryProvider.Current);
			AssertEquals("The Last Data Transfer was successful", true, fImporter.WasTheLastDataTransferSuccessful);

			txnHeaderCollection = new TxnHeaderCollection();
			using (StreamReader reader = new StreamReader(arInvoiceTestFilePath))
			{
				converter.ImportFlatFile(txnHeaderCollection, new CsvFlatFileFormat(false), reader);
			}

			prevFactory = fImporter.FactoryProvider.Current;
			fImporter.OnlySaveDataWhenNoRecordsHaveErrors = true;
			fImporter.ExtractToDataAdapter_ForTestOnly(txnHeaderCollection, new NotificationBuffer());
			AssertEquals("Imported invoice must saved as there are not errors during import.", true, fImporter.FLastImportedInvoice_ForTestOnly.IsInDatabase);
			AssertNotEquals("New Factory must be crated.", prevFactory, fImporter.FactoryProvider.Current);
			AssertEquals("The Last Data Transfer was successful", true, fImporter.WasTheLastDataTransferSuccessful);
		}

		#region Implementation

		TxnHeaderFlatFileDataImporter fImporter;

		protected override void SetUp()
		{
			base.SetUp();
			fImporter = new TxnHeaderFlatFileDataImporter();
		}

		#endregion
	}
}
