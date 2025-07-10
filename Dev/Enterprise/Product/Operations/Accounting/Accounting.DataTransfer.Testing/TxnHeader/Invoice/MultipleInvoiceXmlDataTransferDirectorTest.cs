using System;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	sealed class MultipleInvoiceXmlDataTransferDirectorTest : TestCaseWithFactory
	{
		public void TestExtraValidation()
		{
			MultipleInvoiceXmlDataTransferDirector testDirector = new MultipleInvoiceXmlDataTransferDirector(false);
			Assert(((FinancialInvoiceDataAdapter)testDirector.Adapter_ForTestOnly).RunExtraValidation);
		}

		public void TestNewXmlDataImporter()
		{
			var testDirector = new MultipleInvoiceXmlDataTransferDirector(false);
			AssertEquals("Enterprise.Accounting.DataTransfer.Invoices.MultipleInvoiceXmlDataTransferDirector+MultipleInvoiceDirectorXmlDataImporter", testDirector.NewXmlDataImporter_ForTestOnly().GetType().FullName);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2011, 08, 08)]
		public void TestImportInvoicesWithSameChargeCodesAndJobNumbers()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			AccountingPeriodTestHelper period = new AccountingPeriodTestHelper(Factory);
			period.SetupPeriods();

			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			ForwardingShipment shipment = objectCreator.CreateShipment("S00001234", "AUSYD", "NZAKL");
			Job job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = objectCreator.FESDepartment.PK;

			OrgHeader aBIGAS = objectCreator.ABIGAS;
			aBIGAS.OH_IsCreditor = true;

			RefCurrency localCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			Charge charge = objectCreator.CreateCharge(job, objectCreator.CC1, "", localCurrency, 50m, null, localCurrency, 50m, null);
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = objectCreator.FESDepartment.PK;

			Factory.Save();

			AssertEquals("Precondition: Job's Charges Count", 1, job.Charges.Count);

			NotificationBuffer notifications = new NotificationBuffer();
			MultipleInvoiceXmlDataTransferDirector testDirector = new MultipleInvoiceXmlDataTransferDirector(true);
			testDirector.Import(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\InvoiceToTestImportFactories.xml", notifications, SourceInfo.EmptySourceInfo);

			APInvoice invoice = Factory.LoadTop1<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "ACJ2621290").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			APInvoice invoice2 = Factory.LoadTop1<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "ACJ2621291").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));

			BusinessObjectFactory.SaveTogether(invoice.Factory, invoice2.Factory);

			AssertNotNull("Invoice Should not be null", invoice);
			AssertNotNull("Invoice Should not be null", invoice2);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 08, 20)]
		public void TestImportInvoice_WhenJobIsReadyForFinancialClosure_Error()
		{
			Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = false;
			var filepath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\FinancialInvoiceWithJob.xml";

			AssertContains(@$"
Processing Transaction: Transaction AP INV AALSHI ABC0001: 
Successfully matched organization with code 'AALSHI', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: AALSHI, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: Job Number: Cannot post this charge, because the job has Jobs Ready for Financial Closure status.
  This transaction has errors and was not imported


Saving the data to the database...",
				TestImportInvoice_WhenJobIsReadyForFinancialClosure(filepath).AsString);

			APInvoice invoice = Factory.LoadTop1<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "ABC0001").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertNull(invoice);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 08, 20)]
		public void TestImportMultipleInvoiceWithSameJob_WhenJobIsReadyForFinancialClosure_Error()
		{
			Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = false;
			var filepath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\FinancialTransactionsWithSameJob.xml";

			AssertContains(
@$"
Processing Transaction: Transaction AP INV AALSHI ABC0001: 
Successfully matched organization with code 'AALSHI', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: AALSHI, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: Job Number: Cannot post this charge, because the job has Jobs Ready for Financial Closure status.
  This transaction has errors and was not imported

Processing Transaction: Transaction AP INV AALSHI ABC0002: 
Error: Job Number: Cannot post this charge, because the job has Jobs Ready for Financial Closure status.
  This transaction has errors and was not imported


Saving the data to the database...",
				TestImportInvoice_WhenJobIsReadyForFinancialClosure(filepath).AsString);

			APInvoice invoice1 = Factory.LoadTop1<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "ABC0001").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertNull(invoice1);

			APInvoice invoice2 = Factory.LoadTop1<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "ABC0002").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertNull(invoice2);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 08, 20)]
		public void TestImportMultipleInvoiceWithDifferentJob_WhenJobIsReadyForFinancialClosure_Error()
		{
			Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = false;
			var filepath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\FinancialTransactionsWithDifferentJob.xml";

			AssertContains(
				@$"
Processing Transaction: Transaction AP INV AALSHI ABC0001: 
Successfully matched organization with code 'AALSHI', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: AALSHI, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: Job Number: Cannot post this charge, because the job has Jobs Ready for Financial Closure status.
  This transaction has errors and was not imported

Processing Transaction: Transaction AP INV AALSHI ABC0002: 
  Completed Processing Transaction

Accounts Payable Invoice created

Saving the data to the database...",
				TestImportInvoice_WhenJobIsReadyForFinancialClosure(filepath).AsString);

			APInvoice invoice1 = Factory.LoadTop1<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "ABC0001").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertNull(invoice1);

			APInvoice invoice2 = Factory.LoadTop1<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "ABC0002").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertNotNull(invoice2);
		}

		NotificationBuffer TestImportInvoice_WhenJobIsReadyForFinancialClosure(string filepath)
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			AccountingPeriodTestHelper period = new AccountingPeriodTestHelper(Factory);
			period.SetupPeriods();

			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			objectCreator.CC1.AC_ChargeType = Enterprise.Core.Constants.ChargeType.Disbursement;
			var shipment1 = objectCreator.CreateShipment("S00001");
			var job1 = objectCreator.CreateJob(shipment1, setCurrentDepartment: false);
			job1.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
			var charge1 = job1.Charges.AddNew();
			charge1.JR_AC = objectCreator.CC1.PK;
			charge1.JR_OSCostAmt = 100m;
			var charge2 = job1.Charges.AddNew();
			charge2.JR_AC = objectCreator.CC1.PK;
			charge2.JR_OSCostAmt = 100m;
			var shipment2 = objectCreator.CreateShipment("S00002");
			var job2 = objectCreator.CreateJob(shipment2, setCurrentDepartment: false);
			var charge3 = job2.Charges.AddNew();
			charge3.JR_AC = objectCreator.CC1.PK;
			charge3.JR_OSCostAmt = 100m;
			Factory.Save();

			Assert("Precondition", job1.IsReadyForFinancialClosure);
			Assert("Precondition", !job2.IsReadyForFinancialClosure);

			NotificationBuffer notifications = new NotificationBuffer();
			MultipleInvoiceXmlDataTransferDirector testDirector = new MultipleInvoiceXmlDataTransferDirector(true);
			testDirector.Import(filepath, notifications, SourceInfo.EmptySourceInfo);

			return notifications;
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportInvoiceWithConsolCostInDifferentCurrency()
		{
			AccTaxRate.LoadExistingOrCreateNewTaxRate(new BusinessObjectFactory(), "EXEMPT", AccTaxRate.Types.Exempt, 0).Factory.Save();

			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			AccountingPeriodTestHelper period = new AccountingPeriodTestHelper(Factory);
			period.SetupPeriods();

			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			var consol = objectCreator.CreateConsol("NLAMS", "AUSYD", "C00001");
			var shipment = objectCreator.CreateShipment("S00001", consol);
			var consolCost = objectCreator.CreateConsolCost(consol, objectCreator.CC1, 6588m, null, "SHP");
			consolCost.E6_RX_NKCurrency = "USD";
			consolCost.E6_ExchangeRate = 1.0834;

			OrgHeader aBIGAS = objectCreator.ABIGAS;
			aBIGAS.OH_IsCreditor = true;

			var branch = objectCreator.CreateBranch("AMS", GlbCompany.CurrentCompany);
			Factory.Save();

			NotificationBuffer notifications = new NotificationBuffer();
			MultipleInvoiceXmlDataTransferDirector testDirector = new MultipleInvoiceXmlDataTransferDirector(true);
			testDirector.Import(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\LegacyXmlImportWithConsolCostInDifferentCurrency.xml", notifications, SourceInfo.EmptySourceInfo);

			APInvoice invoice = Factory.LoadTop1<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "1016066488A").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertNotNull("invoice should not have any validation error", invoice);
			JobConsolCost cost = Factory.LoadTop1<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_AH_APInvoice, invoice.PK));
			AssertNotEquals("The import cost should not have existing consol's currency.", "USD", cost.E6_RX_NKCurrency);
			AssertEquals("The import cost should have original currency.", cost.LocalCurrency, cost.E6_RX_NKCurrency);

			AssertEquals("The currency of invoice line should be local currency", GlbCompany.CurrentCompany.LocalCurrency.Code, invoice.Lines[0].AL_RX_NKTransactionCurrency);
			AssertNotEquals("The invoice line should not have existing consol's currency.", "USD", invoice.Lines[0].AL_RX_NKTransactionCurrency);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[GuiTest]
		[TestDate(2015, 04, 10)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Testing")]
		public void TestImportTransactionsWithOnlySaveDataWhenNoError()
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			var orgHeader = new TestObjectCreator(Factory).ABIGAS;
			orgHeader.CompanyData.OB_IsDebtor = orgHeader.CompanyData.OB_IsCreditor = true;
			Factory.Save();

			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			TestCaseHelper.ClearTable(AccTransactionHeaderSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionLinesSchema.Constants.TableName);
			AssertEquals("Precondition: AccTransactionHeader Database Count", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));

			var businessEntity = new DataImporterBusinessObject(new BusinessObjectFactory());
			using (TestXMLDataImporterForm form = new TestXMLDataImporterForm(businessEntity, "XML Data Importer", BillingInterfaceName.JournalXmlImport))
			{
				var path = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\128TransactionsWith1Error.xml";

				var importer = new XmlDataImporter(new ARAPJournalDataAdapter());
				form.Importer = importer;
				form.Show();
				Application.DoEvents();

				form.SetParametersOfOnlySaveDataWhenNoRecordsHaveErrorsCheckBox(true, true);
				form.ImportFromFile(path);
				AssertEquals("AccTransactionHeader Database Count", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));

				form.SetParametersOfOnlySaveDataWhenNoRecordsHaveErrorsCheckBox(false, true);
				form.ImportFromFile(path);
				AssertEquals("AccTransactionHeader Database Count", true, Factory.GetDatabaseCount(typeof(AccTransactionHeader)) > 0);
			}
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
	}
}
