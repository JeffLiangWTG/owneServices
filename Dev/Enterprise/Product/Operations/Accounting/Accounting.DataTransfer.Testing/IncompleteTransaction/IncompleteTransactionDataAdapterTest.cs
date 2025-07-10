using System;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	[TestedType(typeof(IncompleteTransactionDataAdapter<APInvoice>))]
	sealed class IncompleteTransactionDataAdapterTest : BaseAccountingDataAdapterTest<APInvoice, IncompleteTransactionHeader>
	{
		protected override void TearDown()
		{
			base.TearDown();
			retriever?.Dispose();
			retriever = null;
		}

		EmbeddedResourceRetriever Retriever => retriever ??= new ();
		EmbeddedResourceRetriever retriever;

		protected override string ExpectedRootCollectionElementName
		{
			get { return "IncompleteTransactions"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "IncompleteTransaction"; }
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var invoice = Factory.New<APInvoice>();
			invoice.AH_TransactionNum = "10001003";
			return new BusinessObjectAndExpectedOutputFileName(invoice, Retriever.SaveResourceToFile("EmptyIncompleteInvoice.xml"), ValidationKind.None, "Empty Invoice");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(GetFullyPopulatedInvoice(false), Retriever.SaveResourceToFile("FullyPopulatedIncompleteInvoice.xml"), ValidationKind.None, "Fully Populated Invoice");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override ValueObjectDataAdapter<APInvoice, IncompleteTransactionHeader> GetNewBizObjXmlDataAdapter()
		{
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			apInvoice.AH_TransactionNum = "10001002";
			return new IncompleteTransactionDataAdapter<APInvoice>(apInvoice);
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			APInvoice invoice = GetFullyPopulatedInvoice(true);
			invoice.ValidateExpectedInvoiceTotal = false;
			invoice.ExpectedInvoiceTotal = 0m;
			invoice.ExpectedInvoiceExclTaxTotal = 0m;
			invoice.ExpectedInvoiceTaxTotal = 0m;
			invoice.ConsolCosting.ConsolCosts.RemoveAndDeleteAll();
			invoice.Lines[0].AL_Calc_InputGSTVATRecoverablePercentage = 99.99;
			return new BusinessObjectAndExpectedOutputFileName(invoice, Retriever.SaveResourceToFile("SemiPopulatedIncompleteInvoice.xml"), ValidationKind.None, "Semi Populated Invoice");
		}

		public void TestBizObjXmlDataAdapter_UsesTransactionHeader_PassedInConstructor()
		{
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			var adapter = new IncompleteTransactionDataAdapter<APInvoice>(apInvoice);
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			AssertEquals(apInvoice, adapter.CreateOrUpdateFromValueObject(new IncompleteTransactionHeader(), context));
		}

		protected override void TestExportToAndImportFromAndExportToValueObject(BusinessObjectAndExpectedOutputFileName sample)
		{
			Assert("Not required, imported object is never supposed to be updated", true);
		}

		[TestDate(2019, 12, 21)]
		public override void TestExportToValueObject_ForFullyPopulatedBizO()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			Factory.Save();
			base.TestExportToValueObject_ForFullyPopulatedBizO();
		}

		[TestDate(2019, 12, 21)]
		public override void TestExportToValueObject_ForPopulatedBizObjWithEmptyFields()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			Factory.Save();
			base.TestExportToValueObject_ForPopulatedBizObjWithEmptyFields();
		}

		[TestDate(2019, 12, 21)]
		public override void TestTestCoverageOfValueObject()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			Factory.Save();
			base.TestTestCoverageOfValueObject();
		}

		[ExpectNoExceptions]
		public void TestExpectedTotalBackwardCompatibility()
		{
			IncompleteTransactionDataAdapter<APInvoice> adapter = (IncompleteTransactionDataAdapter<APInvoice>)GetNewBizObjXmlDataAdapter();
			StringReader reader = new StringReader(@"<?xml version=""1.0"" encoding=""utf-8""?>
<IncompleteTransaction xmlns=""http://www.edi.com.au/EnterpriseService/"">
	<InvoiceNumber>SEMI</InvoiceNumber>
	<JobRelatedLines>
	<JobRelatedLine>
		<JobNumber>S00001000</JobNumber>
		<GenericChargeCode>CCLR</GenericChargeCode>
		<Description>Customs Clearance / Agency Fees</Description>
		<Branch>TBC</Branch>
		<Department>4VP</Department>
		<Amount CurrencyCode=""AUD"">0</Amount>
		<TaxId>GST</TaxId>
		<TaxAmount CurrencyCode=""AUD"">10</TaxAmount>
		<GstAmount CurrencyCode=""AUD"">8.20</GstAmount>
		<GstInclusiveAmount CurrencyCode=""AUD"">-10</GstInclusiveAmount>
		<IsFinal>false</IsFinal>
	</JobRelatedLine>
	</JobRelatedLines>
</IncompleteTransaction>");
			XmlTextReader xmlReader = new XmlTextReader(reader);

			InvoicingBaseCollection collection = new InvoicingBaseCollection(Factory);
			XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(typeof(IncompleteTransactionHeader));
			serializer.ReadInterchangeOrCollectionFromXml(xmlReader, adapter, collection, null, new NotificationBuffer());
		}

		[ExpectNoExceptions]
		public void TestHandleNullTaxRatesInConsolCosts()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			line.FillWithValidTestData();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			AccChargeCode chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK));

			JobConsolCost consolCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			consolCost.E6_AC_ChargeCode = chargeCode.PK;
			consolCost.E6_OSCostAmount = 5m;
			consolCost.E6_AT_TaxRate = ZGuid.Empty;
			consolCost.E6_OSGSTAmount_Calc = 1m;
			consolCost.E6_PPDCLT = "PPD";
			consolCost.E6_ApportionmentMethod = "SHP";

			invoice.SaveAsIncomplete(); // runs adapter code
		}

		InvoicingBase CreateTestdataSaveAndReloadAsIncompleteInvoice(Action<InvoicingBase> beforeRestoreSavedData = null)
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_INCO = Enterprise.Core.Constants.IncoTerms.CostAndFreight;
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery()).PK;
			job.JH_GB = ObjectCreator.NonCurrentBranch.PK;

			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_INCO = Core.Constants.IncoTerms.CostAndFreight;
			Job job2 = new Job.Loader(shipment2).TryLoadOrCreateWithoutMutexForTestOnly();
			job2.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery()).PK;
			job2.JH_GB = ObjectCreator.NonCurrentBranch.PK;

			ZGuid gSTFREE1PK = ObjectCreator.GSTFREE1.PK;

			Factory.Save();

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();

			invoice.AH_OH = ObjectCreator.Creditor1.PK;
			invoice.AH_RX_NKTransactionCurrency = "USD";
			invoice.AH_ExchangeRate = 2m;
			invoice.AH_DocumentReceivedDate = ZDateTime.Today;

			AccChargeCode chargeCode = ChargeCodeCC1;

			JobConsolCost consolCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			consolCost.E6_AC_ChargeCode = chargeCode.PK;
			consolCost.E6_OSCostAmount = 5m;
			consolCost.E6_AT_TaxRate = gSTFREE1PK;
			consolCost.E6_OSGSTAmount_Calc = 1m;
			consolCost.E6_PPDCLT = "PPD";
			consolCost.E6_ApportionmentMethod = "SHP";
			consolCost.E6_ApportionToRelatedShipments = true;
			consolCost.E6_AW = TestObjectCreator.WHT1.PK;
			consolCost.E6_TaxDate = new ZDate(2019, 9, 20);
			AssertEquals("Creditor should not be empty", false, consolCost.E6_OH_Creditor.IsEmpty);
			AssertNotEquals("Tax Rate on Consol Cost should be different to default tax rate on charge code", consolCost.E6_AT_TaxRate, chargeCode.AC_AT_GSTRate);

			AssertEquals("This should create 2 apportionment charges", 2, consolCost.ApportionmentCharges.Count);
			invoice.SaveAsIncomplete(); // runs adapter code

			InvoicingBase reloadedIncomplete = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
			reloadedIncomplete.ShowError = new InvoicingBase.ShowErrorHandler(ShowError);
			beforeRestoreSavedData?.Invoke(reloadedIncomplete);
			reloadedIncomplete.RestoreSavedData();
			return reloadedIncomplete;
		}

		void ShowError(string message, string caption)
		{
		}

		public void TestReloadOfConsolCostsOnlyCreatesIntendedJobCharges()
		{
			var reloadedIncomplete = CreateTestdataSaveAndReloadAsIncompleteInvoice();

			ExceptionReporterTestListener.Instance.Clear();
			AssertEquals("Reloaded incomplete should have 1 consol cost", 1, reloadedIncomplete.ConsolCosting.ConsolCosts.Count);
			ZQuery query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;
			JobCharge[] localCacheCharges = reloadedIncomplete.Factory.Load<JobCharge>(query);
			AssertEquals("Only 2 Job Charges should be in factory", 2, localCacheCharges.Length);

			var consolCost = reloadedIncomplete.ConsolCosting.ConsolCosts[0];
			var firstCharge = consolCost.ApportionmentCharges[0];
			var secondCharge = consolCost.ApportionmentCharges[1];

			AssertEquals("Reloaded incomplete consol cost should have only 2 apportionmetn split charges", 2, consolCost.ApportionmentCharges.Count);
			AssertEquals("Charge's Cost Account should be populated from the Consol Cost Creditor", consolCost.E6_OH_Creditor, firstCharge.JR_OH_CostAccount);
			AssertEquals("Charge's Cost Account should be populated from the Consol Cost Creditor", consolCost.E6_OH_Creditor, secondCharge.JR_OH_CostAccount);

			AssertEquals("Precondition: E6_RX_NKCurrency", "USD", consolCost.E6_RX_NKCurrency);
			AssertEquals("Precondition: E6_ExchangeRate", 2m, consolCost.E6_ExchangeRate);

			AssertEquals("Charge's currency should be populated from the Consol Cost", consolCost.E6_RX_NKCurrency, firstCharge.JR_CostCurrency);
			AssertEquals("Charge's currency should be populated from the Consol Cost", consolCost.E6_RX_NKCurrency, secondCharge.JR_CostCurrency);
			AssertEquals("Charge's exchange rate should be populated from the Consol Cost", consolCost.E6_ExchangeRate, firstCharge.JR_OSCostExRate);
			AssertEquals("Charge's exchange rate should be populated from the Consol Cost", consolCost.E6_ExchangeRate, secondCharge.JR_OSCostExRate);
			AssertEquals("Charge's total os amount", 5M, firstCharge.JR_OSCostAmt + secondCharge.JR_OSCostAmt);
			AssertEquals("Charge's total local amount", 2.5M, firstCharge.JR_LocalCostAmt + secondCharge.JR_LocalCostAmt);
			AssertEquals("Charge's Tax Code should be GSTFREE1", ObjectCreator.GSTFREE1.PK, firstCharge.JR_AT_CostGSTRate);
			AssertEquals("Charge's Tax Code should be GSTFREE1", ObjectCreator.GSTFREE1.PK, secondCharge.JR_AT_CostGSTRate);

			AssertEquals("Charge's Tax Code should be WHT1", ObjectCreator.WHT1.PK, firstCharge.JR_AW_CostWHTRate);
			AssertEquals("Charge's Tax Code should be WHT1", ObjectCreator.WHT1.PK, secondCharge.JR_AW_CostWHTRate);
			AssertEquals("Charge's Tax amount should 0.12", 0.13M, firstCharge.JR_OSCostWHTAmt);
			AssertEquals("Charge's Tax Code should be 0.13", 0.13M, secondCharge.JR_OSCostWHTAmt);
			AssertEquals("Charge's Document Received Date should be populated from the Consol Cost", consolCost.E6_DocumentReceivedDate, firstCharge.JR_APDocumentReceivedDate.Date);
			AssertEquals("Charge's Document Received Date should be populated from the Consol Cost", consolCost.E6_DocumentReceivedDate, secondCharge.JR_APDocumentReceivedDate.Date);
		}

		public void TestTaxDateFromReloadOfConsolCosts()
		{
			var reloadedIncomplete = CreateTestdataSaveAndReloadAsIncompleteInvoice();

			ExceptionReporterTestListener.Instance.Clear();
			AssertEquals("Reloaded incomplete should have 1 consol cost", 1, reloadedIncomplete.ConsolCosting.ConsolCosts.Count);
			var query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;
			var localCacheCharges = reloadedIncomplete.Factory.Load<JobCharge>(query);
			AssertEquals("Only 2 Job Charges should be in factory", 2, localCacheCharges.Length);

			var consolCost = reloadedIncomplete.ConsolCosting.ConsolCosts[0];
			var firstCharge = consolCost.ApportionmentCharges[0];
			var secondCharge = consolCost.ApportionmentCharges[1];

			AssertEquals("Charge's Cost Date should be populated from the Consol Cost", new ZDate(2019, 9, 20), consolCost.E6_TaxDate);
			AssertEquals("Charge's Cost Date should be populated from the Consol Cost", consolCost.E6_TaxDate, firstCharge.JR_CostTaxDate);
			AssertEquals("Charge's Cost Date should be populated from the Consol Cost", consolCost.E6_TaxDate, secondCharge.JR_CostTaxDate);
		}

		public void TestReloadOfConsolCostsPopulatesShipmentInfosCorrectly()
		{
			InvoicingBase reloadedIncomplete = CreateTestdataSaveAndReloadAsIncompleteInvoice();

			ExceptionReporterTestListener.Instance.Clear();
			AssertEquals("Reloaded incomplete should have 1 consol cost", 1, reloadedIncomplete.ConsolCosting.ConsolCosts.Count);
			AssertEquals("Reloaded incomplete consol cost should have 2 apportionment split charges", 2, reloadedIncomplete.ConsolCosting.ConsolCosts[0].ApportionmentCharges.Count);
			AssertEquals("Reloaded incomplete consol cost should have 'GSTFREE1' tax rate code", "ZZGSTFREE1", reloadedIncomplete.ConsolCosting.ConsolCosts[0].TaxRate.AT_Code);
			AssertEquals("Reloaded incomplete consol cost should have 'WHT1' WHT rate code", "ZZWHT1", reloadedIncomplete.ConsolCosting.ConsolCosts[0].WithholdingTax.AW_Code);
			AssertEquals("Reloaded incomplete consol cost should have 'true' for Display Related Shipments Field", true, reloadedIncomplete.ConsolCosting.ConsolCosts[0].E6_ApportionToRelatedShipments);

			foreach (JobConsolCost jobConsolCost in reloadedIncomplete.ConsolCosting.ConsolCosts)
			{
				foreach (ApportionSplitCharge charge in jobConsolCost.ApportionmentCharges)
				{
					AssertNotNull("ShipmentInfo should not be null. If this is null, then users will receieve validation errors after reloading and won't be able to save.", charge.ShipmentInfo);
				}
			}
		}

		public void TestPlaceOfSupplyFromReloadOfConsolCosts()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_INCO = Enterprise.Core.Constants.IncoTerms.CostAndFreight;
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery()).PK;
			job.JH_GB = ObjectCreator.NonCurrentBranch.PK;

			Factory.Save();

			var invoice = Factory.NewWithValidTestData<APInvoice>();
			JobConsolCost consolCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			consolCost.E6_PlaceOfSupply = "DL";
			consolCost.E6_PlaceOfSupplyType = "STA";

			invoice.SaveAsIncomplete();

			var reloadedIncomplete = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
			reloadedIncomplete.ShowError = new InvoicingBase.ShowErrorHandler(ShowError);
			reloadedIncomplete.RestoreSavedData();

			ExceptionReporterTestListener.Instance.Clear();
			AssertEquals("Reloaded incomplete should have 1 consol cost", 1, reloadedIncomplete.ConsolCosting.ConsolCosts.Count);
			var query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;
			var localCacheCharges = reloadedIncomplete.Factory.Load<JobCharge>(query);
			AssertEquals("Only 1 Job Charges should be in factory", 1, localCacheCharges.Length);

			var consolCosting = reloadedIncomplete.ConsolCosting.ConsolCosts[0];
			var charge = consolCosting.ApportionmentCharges[0];

			AssertEquals("Consol cost's place of supply should be restored", "DL", consolCosting.E6_PlaceOfSupply);
			AssertEquals("Charge's place of supply type should be restored", "DL", charge.JR_CostPlaceOfSupply);
			AssertEquals("Consol cost's place of supply should be restored", "STA", consolCosting.E6_PlaceOfSupplyType);
			AssertEquals("Charge's place of supply type should be restored", "STA", charge.JR_CostPlaceOfSupplyType);
		}

		public void TestSupplyFromReloadOfConsolCosts()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_INCO = Enterprise.Core.Constants.IncoTerms.CostAndFreight;
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery()).PK;
			job.JH_GB = ObjectCreator.NonCurrentBranch.PK;

			Factory.Save();

			var invoice = Factory.NewWithValidTestData<APInvoice>();
			JobConsolCost consolCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			consolCost.E6_SupplyType = "LOA";

			invoice.SaveAsIncomplete();

			var reloadedIncomplete = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
			reloadedIncomplete.ShowError = new InvoicingBase.ShowErrorHandler(ShowError);
			reloadedIncomplete.RestoreSavedData();

			ExceptionReporterTestListener.Instance.Clear();
			AssertEquals("Reloaded incomplete should have 1 consol cost", 1, reloadedIncomplete.ConsolCosting.ConsolCosts.Count);
			var query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;
			var localCacheCharges = reloadedIncomplete.Factory.Load<JobCharge>(query);
			AssertEquals("Only 1 Job Charges should be in factory", 1, localCacheCharges.Length);

			var consolCosting = reloadedIncomplete.ConsolCosting.ConsolCosts[0];
			var charge = consolCosting.ApportionmentCharges[0];

			AssertEquals("Consol cost's supply should be restored", "LOA", consolCosting.E6_SupplyType);
			AssertEquals("Charge's supply type should be restored", "LOA", charge.JR_CostSupplyType);
		}

		public void TestTaxDateWithIncompleteInvoice()
		{
			var taxDate = new ZDate(2019, 9, 20);

			var invoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "Inv001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			var line = invoice.Lines[0];
			line.AL_AT = TestObjectCreator.GSTFREE1.PK;
			line.AL_TaxDate = taxDate;

			invoice.SaveAsIncomplete();

			var reloadedInvoice = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
			reloadedInvoice.RestoreSavedData();
			var reloadedLine = reloadedInvoice.Lines[0];

			AssertEquals(taxDate, reloadedLine.AL_TaxDate);
		}

		[TestDate(2019, 12, 21)]
		public void TestPeriodApportionmentRelatedFieldsWithIncompleteInvoice()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			Factory.Save();

			var invoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "Inv001", TestObjectCreator.USD, 0.66M, 100M, 10M, 151.52M, 15.15M);
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			var line = invoice.Lines[0];
			line.AL_AT = TestObjectCreator.GSTFREE1.PK;
			line.PeriodApportionmentMethod = "PER";
			line.PeriodStartDate = new ZDate(2020, 1, 1);
			line.PeriodEndDate = new ZDate(2020, 3, 1);
			line.PeriodClearingGLAccountPK = TestObjectCreator.GLJournalClearingAccount.PK;

			AssertEquals(3, line.PeriodApportionmentLines.Count);
			AssertApportionmentLine(line, 202007, 33.33m, 50.5m, 0.66m);
			AssertApportionmentLine(line, 202008, 33.33m, 50.5m, 0.66m);
			AssertApportionmentLine(line, 202009, 33.34m, 50.52m, 0.659937m);
			invoice.SaveAsIncomplete();

			var reloadedInvoice = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
			reloadedInvoice.RestoreSavedData();
			var reloadedLine = reloadedInvoice.Lines[0];
			AssertEquals("PER", reloadedLine.PeriodApportionmentMethod);
			AssertEquals(new ZDate(2020, 1, 1), reloadedLine.PeriodStartDate);
			AssertEquals(new ZDate(2020, 3, 1), reloadedLine.PeriodEndDate);
			AssertEquals(TestObjectCreator.GLJournalClearingAccount.PK, reloadedLine.PeriodClearingGLAccountPK);
			AssertEquals(3, reloadedLine.PeriodApportionmentLines.Count);
			AssertApportionmentLine(reloadedLine, 202007, 33.33m, 50.5m, 0.66m);
			AssertApportionmentLine(reloadedLine, 202008, 33.33m, 50.5m, 0.66m);
			AssertApportionmentLine(reloadedLine, 202009, 33.34m, 50.52m, 0.659937m);
		}

		[TestDate(2019, 12, 21)]
		public void TestRestoreIncompleteInvoice_ErrorInContext_When_PeriodClearingGLAccountBizoNotInDB()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			Factory.Save();

			var invoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "Inv001", TestObjectCreator.USD, 0.66M, 100M, 10M, 151.52M, 15.15M);
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			var line = invoice.Lines[0];
			line.AL_AT = TestObjectCreator.GSTFREE1.PK;
			line.PeriodApportionmentMethod = "PER";
			line.PeriodStartDate = new ZDate(2020, 1, 1);
			line.PeriodEndDate = new ZDate(2020, 3, 1);
			line.PeriodClearingGLAccountPK = TestObjectCreator.GLJournalClearingAccount.PK;

			invoice.SaveAsIncomplete();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var periodClearingAccountInNewFactory = newFactory.Load<AccGLHeader>(line.PeriodClearingGLAccountPK);
			periodClearingAccountInNewFactory.Delete();
			newFactory.Save();

			var incompleteInvoice = newFactory.Load<InvoicingBase>(invoice.PK);

			var result = incompleteInvoice.RestoreSavedData();

			AssertEquals(InvoicingBase.RestoreSavedDataResult.ResultType.SuccessWithErrors, result.Result);
			AssertEquals("Could not find GL Account. Account Type: Period Clearing Account", result.Error);
		}

		[TestDate(2019, 12, 21)]
		public void TestPeriodApportionmentRelatedFieldsWithIncompleteInvoiceWhenMethodIsBlank()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			Factory.Save();
			var invoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "Inv001", TestObjectCreator.USD, 0.66M, 100M, 10M, 151.52M, 15.15M);
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			var line = invoice.Lines[0];
			line.AL_AT = TestObjectCreator.GSTFREE1.PK;
			line.PeriodApportionmentMethod = "";
			AssertEquals(0, line.PeriodApportionmentLines.Count);
			invoice.SaveAsIncomplete();

			var reloadedInvoice = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
			reloadedInvoice.RestoreSavedData();
			var reloadedLine = reloadedInvoice.Lines[0];
			AssertEquals("DEF", reloadedLine.PeriodApportionmentMethod);
			Assert(reloadedLine.PeriodStartDate.IsEmpty);
			Assert(reloadedLine.PeriodEndDate.IsEmpty);
			Assert(reloadedLine.PeriodClearingGLAccountPK.IsEmpty);
			AssertEquals(0, reloadedLine.PeriodApportionmentLines.Count);
		}

		void AssertApportionmentLine(InvoicingLineBase invoicingLine, int expectedPeriod, decimal expectedOSAmount, decimal expectedLocalAmount, decimal expectExchangeRate)
		{
			var apportionLine = invoicingLine.PeriodApportionmentLines.Cast<Business.ARAP.Invoicing.PeriodApportionmentLine>().First(x => x.Period == expectedPeriod);
			AssertNotNull(apportionLine);
			AssertEquals(expectedOSAmount, apportionLine.OSAmount);
			AssertEquals(expectedLocalAmount, apportionLine.LocalAmount);
			AssertEquals(expectExchangeRate, apportionLine.ExchangeRate);
		}

		[TestDate(2020, 3, 20)]
		public void TestLegacyXmlOfIncompleteInvoiceWithoutTaxDate()
		{
			AssertLegacyXmlOfIncompleteInvoiceTaxDate(true);
		}

		[TestDate(2020, 3, 20)]
		public void TestLegacyXmlOfIncompleteInvoiceWithTaxDate()
		{
			AssertLegacyXmlOfIncompleteInvoiceTaxDate(false);
		}

		[TestDate(2020, 3, 20)]
		public void TestLegacyXmlOfIncompleteConsolCostsWithoutTaxDate()
		{
			AssertLegacyXmlOfIncompleteConsolCostsTaxDate(true);
		}

		[TestDate(2020, 3, 20)]
		public void TestLegacyXmlOfIncompleteConsolCostsWithTaxDate()
		{
			AssertLegacyXmlOfIncompleteConsolCostsTaxDate(false);
		}

		public void AssertLegacyXmlOfIncompleteInvoiceTaxDate(bool removeDate)
		{
			var invoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "Inv001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);

			invoice.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.Header.CompanyData.SetAPTaxApplicable(true);

			var line = invoice.Lines[0];

			line.AL_AC = TestObjectCreator.CC2.PK;

			var taxDate = ZDate.Today.AddDays(-2);
			line.AL_TaxDate = taxDate;

			invoice.SaveAsIncomplete();

			if (removeDate)
			{
				var newFactory = new BusinessObjectFactory();

				ZQuery filter = new ZQuery(StmNoteSchema.ST_Description, "IncompleteTransactionData");
				filter.AddToFilter(StmNoteSchema.ST_ParentID, invoice.PK);
				filter.AddToFilter(StmNoteSchema.ST_Table, AccTransactionHeaderSchema.Constants.TableName);

				var taxDay = taxDate.Day.ToString("D2");
				var taxMonth = taxDate.Month.ToString("D2");
				var taxYear = taxDate.Year;

				string literalTaxDate = $"<TaxDate>{taxYear}-{taxMonth}-{taxDay}</TaxDate>";

				var noteData = newFactory.LoadTop1<StmNote>(filter);

				AssertContains(literalTaxDate, noteData.ST_NoteDataAsText);

				noteData.ST_NoteDataAsText = noteData.ST_NoteDataAsText.Replace(literalTaxDate, "");

				newFactory.Save();
			}

			var reloadedInvoice = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);

			reloadedInvoice.RestoreSavedData();

			var reloadedLine = reloadedInvoice.Lines[0];
			if (removeDate)
			{
				AssertEquals("TaxDate should default to today when loading legacy non-TaxDate xml", ZDate.Today, reloadedLine.AL_TaxDate);
			}
			else
			{
				AssertEquals("TaxDate should be two days ago when loading TaxDate xml", ZDate.Today.AddDays(-2), reloadedLine.AL_TaxDate);
			}
		}

		public void AssertLegacyXmlOfIncompleteConsolCostsTaxDate(bool removeDate)
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001000");
			var shipment1 = TestObjectCreator.CreateShipment("S00001001", consol);
			TestObjectCreator.CreateJob(shipment1, false);

			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "00001", TestObjectCreator.USD, 1.3321m, TestObjectCreator.Creditor1);
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			invoice.AH_TransactionNum = "1111";

			var consolCost = invoice.ConsolCosting.ConsolCosts.AddNew();
			consolCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, JobConsolSchema.Constants.Prefix);
			consolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			consolCost.E6_OSCostAmount = 100m;
			consolCost.E6_ApportionmentMethod = "SHP";

			var taxDate = ZDate.Today.AddDays(-2);
			consolCost.E6_TaxDate = taxDate;

			invoice.SaveAsIncomplete();

			if (removeDate)
			{
				var newFactory = new BusinessObjectFactory();

				ZQuery filter = new ZQuery(StmNoteSchema.ST_Description, "IncompleteTransactionData");
				filter.AddToFilter(StmNoteSchema.ST_ParentID, invoice.PK);
				filter.AddToFilter(StmNoteSchema.ST_Table, AccTransactionHeaderSchema.Constants.TableName);

				var taxDay = taxDate.Day.ToString("D2");
				var taxMonth = taxDate.Month.ToString("D2");
				var taxYear = taxDate.Year;

				string literalTaxDate = $"<TaxDate>{taxYear}-{taxMonth}-{taxDay}</TaxDate>";

				var noteData = newFactory.LoadTop1<StmNote>(filter);

				AssertContains(literalTaxDate, noteData.ST_NoteDataAsText);

				noteData.ST_NoteDataAsText = noteData.ST_NoteDataAsText.Replace(literalTaxDate, "");

				newFactory.Save();
			}

			var reloadedInvoice = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
			reloadedInvoice.RestoreSavedData();

			var reloadedCost = reloadedInvoice.ConsolCosting.ConsolCosts[0];

			if (removeDate)
			{
				AssertEquals("TaxDate should default to today when loading legacy non-TaxDate xml", ZDate.Today, reloadedCost.E6_TaxDate);
			}
			else
			{
				AssertEquals("TaxDate should be two days ago when loading TaxDate xml", ZDate.Today.AddDays(-2), reloadedCost.E6_TaxDate);
			}
		}

		public void TestComplianceDocumentRelatedFieldsWithIncompleteInvoice()
		{
			var invoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "Inv001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			var line = invoice.Lines[0];
			line.AL_AT = TestObjectCreator.GSTFREE1.PK;
			line.CreateComplianceDocumentRecordOnPosting = true;
			line.ComplianceDocumentOrganization = TestObjectCreator.ZECTRA.PK;
			line.ComplianceSubType = "TXI";
			line.ComplianceDocumentNumber = "123";
			line.ComplianceDocumentVATRegistrationNum = "234";
			line.ComplianceDocumentDate = ZDateTime.Today;
			line.ComplianceDocumentReportingPeriod = 201803;
			line.ComplianceDocumentSupportingReason = "AAA";
			line.ComplianceSupportingDocumentType = "AAA";
			line.ComplianceSupportingDocumentNumber = "456";
			invoice.SaveAsIncomplete();

			var reloadedInvoice = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
			reloadedInvoice.RestoreSavedData();
			var reloadedLine = reloadedInvoice.Lines[0];
			AssertEquals(TestObjectCreator.ZECTRA.PK, reloadedLine.ComplianceDocumentOrganization);
			AssertEquals(true, reloadedLine.CreateComplianceDocumentRecordOnPosting);
			AssertEquals("TXI", reloadedLine.ComplianceSubType);
			AssertEquals("123", reloadedLine.ComplianceDocumentNumber);
			AssertEquals("234", reloadedLine.ComplianceDocumentVATRegistrationNum);
			AssertEquals(ZDateTime.Today, reloadedLine.ComplianceDocumentDate);
			AssertEquals(201803, reloadedLine.ComplianceDocumentReportingPeriod);
			AssertEquals("AAA", reloadedLine.ComplianceDocumentSupportingReason);
			AssertEquals("AAA", reloadedLine.ComplianceSupportingDocumentType);
			AssertEquals("456", reloadedLine.ComplianceSupportingDocumentNumber);
		}

		public void TestReloadOfConsolCostsPopulatesAutoJobRevenueJournalCorrectly()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001000");
			var shipment1 = TestObjectCreator.CreateShipment("S00001001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S00001002", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			Factory.Save();

			var invoice = Factory.New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			invoice.AH_TransactionNum = "1111";

			var consolCost = invoice.ConsolCosting.ConsolCosts.AddNew();
			consolCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, JobConsolSchema.Constants.Prefix);
			consolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			consolCost.E6_OSCostAmount = 100m;
			consolCost.E6_ApportionmentMethod = "SHP";

			var branch = TestObjectCreator.NonCurrentBranch.PK;
			var department = TestObjectCreator.FESDepartment.PK;

			SetupSplitCharge(consolCost, job1.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			SetupSplitCharge(consolCost, job2.PK, job1.PK, branch, department);
			invoice.SaveAsIncomplete();

			var reloadedInvoice = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
			reloadedInvoice.RestoreSavedData();

			var reloadedCost = reloadedInvoice.ConsolCosting.ConsolCosts[0];
			var job1Charge = reloadedCost.ApportionmentCharges.Cast<ApportionSplitCharge>().FirstOrDefault(x => x.JR_JH == job1.PK);
			var job2Charge = reloadedCost.ApportionmentCharges.Cast<ApportionSplitCharge>().FirstOrDefault(x => x.JR_JH == job2.PK);
			AssertNotNull("job1Charge", job1Charge);
			AssertNotNull("job2Charge", job2Charge);

			AssertEquals("charge1 internal job", ZGuid.Empty, job1Charge.JR_JH_InternalJob);
			AssertEquals("charge1 internal branch", ZGuid.Empty, job1Charge.JR_GB_InternalBranch);
			AssertEquals("charge1 internal department", ZGuid.Empty, job1Charge.JR_GE_InternalDept);

			AssertEquals("charge2 internal job", job1.PK, job2Charge.JR_JH_InternalJob);
			AssertEquals("charge2 internal branch", branch, job2Charge.JR_GB_InternalBranch);
			AssertEquals("charge2 internal department", department, job2Charge.JR_GE_InternalDept);
		}

		void SetupSplitCharge(JobConsolCost consolCost, ZGuid jR_JH, ZGuid jR_JH_InternalJob, ZGuid jR_GB_InternalBranch, ZGuid jR_GE_InternalDept)
		{
			var charge = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().FirstOrDefault(x => x.JR_JH == jR_JH) ?? consolCost.ApportionmentCharges.AddNew();

			charge.JR_JH = jR_JH;
			charge.JR_JH_InternalJob = jR_JH_InternalJob;
			charge.JR_GB_InternalBranch = jR_GB_InternalBranch;
			charge.JR_GE_InternalDept = jR_GE_InternalDept;
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				string[] xmlNodesToExcludeFromCoverageTest =
					new string[]
					{
						"ConsolCosts/ConsolCostCharges/InternalJobNumber",
						"ConsolCosts/ConsolCostCharges/InternalBranch",
						"ConsolCosts/ConsolCostCharges/InternalDepartment",
						"ConsolCosts/ExchangeRate",
						"JobRelatedLines/TransactionLineID", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"JobRelatedLines/ExchangeRate", //tested by TestLineWithExRate
						"JobRelatedLines/FixedPlaceOfSupply", //tested by TestFixedPlaceOfSupplyAndFixedPlaceOfSupplyTypeSaveAndLoadProperlyWhenSaveAsIncomplete
						"JobRelatedLines/FixedPlaceOfSupplyType", //tested by TestFixedPlaceOfSupplyAndFixedPlaceOfSupplyTypeSaveAndLoadProperlyWhenSaveAsIncomplete

						//Obsolate SubAccount element will NOT be exported. We keep it in XSD to support backward compatibility. Please use SubAccounts element instead.
						"JobRelatedLines/SubAccount/Code",
						"JobRelatedLines/SubAccount/Type/Code",
						"JobRelatedLines/SubAccount/Type/Description",
						"ConsolCosts/ConsolCostCharges/ConsolCostChargeID", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"ConsolCosts/ConsolCostCharges/CostTaxBranch", //tested by TestTaxBranchWithIncompleteInvoice
						"ConsolCosts/ConsolCostCharges/SellTaxBranch", //tested by TestTaxBranchWithIncompleteInvoice
						"ConsolCosts/ConsolCostCharges/ComplianceDocumentInfo/ComplianceDocumentNumber", //tested by TestPopulateComplianceInfoWhenImportConsolCostAndCreateComplianceDocumentRecordOnPosting
						"ConsolCosts/ConsolCostCharges/ComplianceDocumentInfo/ComplianceSubType", //tested by TestPopulateComplianceInfoWhenImportConsolCostAndCreateComplianceDocumentRecordOnPosting
						"ConsolCosts/ConsolCostCharges/ComplianceDocumentInfo/ComplianceDocumentOrganization", //tested by TestPopulateComplianceInfoWhenImportConsolCostAndCreateComplianceDocumentRecordOnPosting
						"ConsolCosts/ConsolCostCharges/ComplianceDocumentInfo/ComplianceDocumentVATRegistrationNum", //tested by TestPopulateComplianceInfoWhenImportConsolCostAndCreateComplianceDocumentRecordOnPosting
						"ConsolCosts/ConsolCostCharges/ComplianceDocumentInfo/ComplianceDocumentDate", //tested by TestPopulateComplianceInfoWhenImportConsolCostAndCreateComplianceDocumentRecordOnPosting
						"ConsolCosts/ConsolCostCharges/ComplianceDocumentInfo/ComplianceDocumentReportingPeriod", //tested by TestPopulateComplianceInfoWhenImportConsolCostAndCreateComplianceDocumentRecordOnPosting
						"ConsolCosts/ConsolCostCharges/ComplianceDocumentInfo/ComplianceDocumentSupportingReason", //tested by TestPopulateComplianceInfoWhenImportConsolCostAndCreateComplianceDocumentRecordOnPosting
						"ConsolCosts/ConsolCostCharges/ComplianceDocumentInfo/ComplianceSupportingDocumentType", //tested by TestPopulateComplianceInfoWhenImportConsolCostAndCreateComplianceDocumentRecordOnPosting
						"ConsolCosts/ConsolCostCharges/ComplianceDocumentInfo/ComplianceSupportingDocumentNumber", //tested by TestPopulateComplianceInfoWhenImportConsolCostAndCreateComplianceDocumentRecordOnPosting
						"TaxTransactionsInfo/TaxTransactions/TaxMessage", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/LedgerControlAccountPK", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/TaxControlAccountPK", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/TaxExpenseAccountPK", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/TaxPendingControlAccountPK", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/TaxId", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/Basis", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/TaxConfigurationPK", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/OSTaxBaseAmount/CurrencyCode", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/OSTaxAmount/CurrencyCode", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/LocalTaxBaseAmount/CurrencyCode", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/LocalTaxAmount/CurrencyCode", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/BranchCode", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/DepartmentCode", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/Ledger", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/PostDate", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/RealisationDate", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/OSTaxCurrency", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/TaxAuthorityServiceCode", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/TaxAuthorityServiceCodeDescription", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/TaxDate", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/TaxSuperType", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/TaxSystemCode", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/SystemCalculatedValuesInfo/SystemCalculatedValues/OSTaxBaseAmount/CurrencyCode", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/SystemCalculatedValuesInfo/SystemCalculatedValues/OSTaxAmount/CurrencyCode", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/SystemCalculatedValuesInfo/SystemCalculatedValues/TaxDate", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/SystemCalculatedValuesInfo/SystemCalculatedValues/TaxAuthorityServiceCode", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
						"TaxTransactionsInfo/TaxTransactions/SystemCalculatedValuesInfo/SystemCalculatedValues/TaxAuthorityServiceCodeDescription", // tested in IncompleteTransactionDataAdapter_TaxTransactionsTest
					};

				return xmlNodesToExcludeFromCoverageTest;
			}
		}

		public void TestOriginalJobCharge()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100, 100);

			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV123");
			var line = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 100);
			line.OriginalJobCharge = charge;

			AssertNotNull("Precondition:", invoice.Lines[0].OriginalJobCharge);
			JobCharge expectedCharge = invoice.Lines[0].OriginalJobCharge;
			invoice.SaveAsIncomplete(); // runs adapter code

			InvoicingBase reloadedIncomplete = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
			reloadedIncomplete.RestoreSavedData();
			AssertNotNull(reloadedIncomplete.Lines[0].OriginalJobCharge);
			AssertEquals(expectedCharge.PK, reloadedIncomplete.Lines[0].OriginalJobCharge.PK);

			invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV124");
			line = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 100);

			line.OriginalJobCharge = null;
			invoice.SaveAsIncomplete(); // runs adapter code

			reloadedIncomplete = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
			reloadedIncomplete.RestoreSavedData();
			AssertNull(reloadedIncomplete.Lines[0].OriginalJobCharge);
		}

		[TestDate(2024, 12, 4)]
		public void TestDuplicatedJobNumber_ExceptionMessage()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var job = TestObjectCreator.CreateJob(shipment, false);
			
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100, 100);

			Factory.Save();
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV124");
			var line = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 100);

			line.OriginalJobCharge = charge;
			invoice.SaveAsIncomplete();

			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			job1.JH_JobNum = job.JH_JobNum;

			AssertEquals("S00001000", job.JH_JobNum);
			AssertEquals("S00001000", job1.JH_JobNum);

			try
			{
				var reloadedIncomplete = Factory.Load<InvoicingBase>(invoice.PK);
				reloadedIncomplete.RestoreSavedData();
			}
			catch(Exception)
			{
			}
			
			var branch = Env.CurrentBranch.Code;
			var department = Env.CurrentDepartment.Code;
			var company = Env.CurrentCompany.Code;
			var expectedError1 = $"Duplicated Jobs with job number S00001000 count: 2";
			var expectedError2 = $"Job| JH_JobNum:S00001000, Job type:SHP, IsInDb:False, HasChange:True, JH_Status:WRK, JH_A_JOP:04-Dec-24 00:00:00, JH_A_JCL:, Branch:{branch}, Department:{department}, Company:{company}, JH_SystemCreateTimeUtc:, JH_SystemCreateUser:, JH_SystemLastEditTimeUtc:, JH_SystemLastEditUser:, JH_JobLocalReference:, JH_ParentID:{shipment1.PK.ToGuid()}, JH_ParentTableCode:JS";
			var expectedError3 = $"Job| JH_JobNum:S00001000, Job type:SHP, IsInDb:True, HasChange:False, JH_Status:WRK, JH_A_JOP:04-Dec-24 00:00:00, JH_A_JCL:, Branch:{branch}, Department:{department}, Company:{company}, JH_SystemCreateTimeUtc:04-Dec-24 00:00:00, JH_SystemCreateUser:DP, JH_SystemLastEditTimeUtc:04-Dec-24 00:00:00, JH_SystemLastEditUser:DP, JH_JobLocalReference:00000001, JH_ParentID:{shipment.PK.ToGuid()}, JH_ParentTableCode:JS";

			AssertContains(expectedError1, ErrorReporter.LastMessageReported);
			AssertContains(expectedError2, ErrorReporter.LastMessageReported);
			AssertContains(expectedError3, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestOriginalJobChargeIsNullWhenChanged()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100, 100);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 200, 200);
			var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 300, 300);
			var charge4 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 400, 400);
			var charge5 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, 500, 500);

			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV123");
			var line1 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 100);
			line1.OriginalJobCharge = charge1;

			var line2 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 200);
			line2.OriginalJobCharge = charge2;

			var line3 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 300);
			line3.OriginalJobCharge = charge3;

			var line4 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 400);
			line4.OriginalJobCharge = charge4;

			var line5 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC3, 400);
			line5.OriginalJobCharge = charge5;

			AssertNotNull("Precondition:", invoice.Lines[0].OriginalJobCharge);
			AssertNotNull("Precondition:", invoice.Lines[1].OriginalJobCharge);
			AssertNotNull("Precondition:", invoice.Lines[2].OriginalJobCharge);
			AssertNotNull("Precondition:", invoice.Lines[3].OriginalJobCharge);
			AssertNotNull("Precondition:", invoice.Lines[4].OriginalJobCharge);
			invoice.SaveAsIncomplete();

			var factory = new BusinessObjectFactory();
			var newCharge1 = factory.Load<Charge>(charge1.PK);
			newCharge1.JR_AC = TestObjectCreator.CC2.PK;

			var newCharge2 = factory.Load<Charge>(charge2.PK);
			newCharge2.JR_JH = new TestObjectCreator(factory).Job1.PK;

			var newCharge3 = factory.Load<Charge>(charge3.PK);
			newCharge3.JR_GB = TestObjectCreator.NonCurrentBranch.PK;

			var newCharge4 = factory.Load<Charge>(charge4.PK);
			newCharge4.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;

			var consolInNewFactory = factory.Load<ForwardingConsol>(consol.PK);
			var newFactoryTestObjectCreator = new TestObjectCreator(factory);
			newFactoryTestObjectCreator.CreateConsolCost(consolInNewFactory, newFactoryTestObjectCreator.CC3, 500);

			factory.Save();

			Assert("Precondition: charge4.JR_IsApportioned", charge5.JR_IsApportioned);

			var reloadedIncomplete = factory.Load<InvoicingBase>(invoice.PK);
			reloadedIncomplete.RestoreSavedData();
			AssertNull(reloadedIncomplete.Lines[0].OriginalJobCharge);
			AssertNull(reloadedIncomplete.Lines[1].OriginalJobCharge);
			AssertNull(reloadedIncomplete.Lines[2].OriginalJobCharge);
			AssertNull(reloadedIncomplete.Lines[3].OriginalJobCharge);
			AssertNull(reloadedIncomplete.Lines[4].OriginalJobCharge);
		}

		public void TestIncompleteIsFinal()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery()).PK;
			job.JH_GB = Factory.LoadTop1<GlbBranch>(new ZQuery()).PK;
			Factory.Save();

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();

			invoice.AH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsActive, true)).PK;
			AccChargeCode chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK));
			ZQuery query = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccChargeCodeSchema.PK, SQLComparisonOperator.NotEqual, chargeCode.PK);
			AccChargeCode chargeCode2 = Factory.LoadTop1<AccChargeCode>(query);

			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			line.AL_AC = chargeCode.PK;
			line.AL_JH = job.PK;
			line.AL_OSExTaxAmount = 100m;
			line.AL_IsFinalCharge = true;
			line.AL_GE = job.JH_GE;
			line.AL_GB = job.JH_GB;

			APInvoiceLine line2 = (APInvoiceLine)invoice.Lines.AddNew();
			line2.AL_AC = chargeCode2.PK;
			line2.AL_JH = job.PK;
			line2.AL_OSExTaxAmount = 200m;
			line2.AL_IsFinalCharge = false;
			line2.AL_GE = job.JH_GE;
			line2.AL_GB = job.JH_GB;

			invoice.SaveAsIncomplete();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			APInvoice incompleteInvoice = newFactory.Load<APInvoice>(invoice.PK);

			AssertEquals("incompleteInvoice.Lines.Count", 0, incompleteInvoice.Lines.Count);

			incompleteInvoice.RestoreSavedData();

			AssertEquals("incompleteInvoice.Lines.Count", 2, incompleteInvoice.Lines.Count);

			AssertEquals("line.AL_AC", line.AL_AC, incompleteInvoice.Lines[0].AL_AC);
			AssertEquals("line.AL_JH", line.AL_JH, incompleteInvoice.Lines[0].AL_JH);
			AssertEquals("line.AL_OSExTaxAmount", line.AL_OSExTaxAmount, incompleteInvoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals("line.AL_IsFinalCharge", true, incompleteInvoice.Lines[0].AL_IsFinalCharge);

			AssertEquals("line2.AL_AC", line2.AL_AC, incompleteInvoice.Lines[1].AL_AC);
			AssertEquals("line2.AL_JH", line2.AL_JH, incompleteInvoice.Lines[1].AL_JH);
			AssertEquals("line2.AL_OSExTaxAmount", line2.AL_OSExTaxAmount, incompleteInvoice.Lines[1].AL_OSExTaxAmount);
			AssertEquals("line2.AL_IsFinalCharge", false, incompleteInvoice.Lines[1].AL_IsFinalCharge);
		}

		void AssertContainSubAccount(TransactionLineSubAccountCollection subAccounts, string parentTableCode, ZGuid parentId)
		{
			Assert(subAccounts.OfType<AccTransactionLineSubAccount>().Any(x =>
				x.AL1_SubClassParentId.Equals(parentId) && x.AL1_SubClassParentTableCode.Equals(parentTableCode)));
		}

		public void TestSubAccounts()
		{
			var salesGroup = TestObjectCreator.CreateSalesGroup("SG1");
			var staff = TestObjectCreator.CreateStaff("SR1");
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, OrgHeaderSchema.Constants.Prefix, false);
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, AccGroupsSchema.Constants.Prefix, false);
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, GlbStaffSchema.Constants.Prefix, false);
			Factory.Save();

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			var line1 = (APInvoiceLine)invoice.Lines.AddNew();
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			line1.AL_Desc = "line1";
			var line2 = (APInvoiceLine)invoice.Lines.AddNew();
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			line2.AL_Desc = "line2";
			TestObjectCreator.SetUpTransactionLineSubAccount(line1, OrgHeaderSchema.Constants.Prefix, TestObjectCreator.ABIGAS.PK);
			TestObjectCreator.SetUpTransactionLineSubAccount(line1, AccGroupsSchema.Constants.Prefix, salesGroup.PK);
			TestObjectCreator.SetUpTransactionLineSubAccount(line2, GlbStaffSchema.Constants.Prefix, staff.PK);

			invoice.SaveAsIncomplete();
			APInvoice incompleteInvoice = new BusinessObjectFactory().Load<APInvoice>(invoice.PK);
			incompleteInvoice.RestoreSavedData();

			AssertEquals("incompleteInvoice.Lines.Count", 2, incompleteInvoice.Lines.Count);
			var line1ForAssert = incompleteInvoice.Lines.OfType<APInvoiceLine>().FirstOrDefault(x => x.AL_Desc.Equals("line1"));
			var line2ForAssert = incompleteInvoice.Lines.OfType<APInvoiceLine>().FirstOrDefault(x => x.AL_Desc.Equals("line2"));
			AssertEquals("line1 SubAccounts.Count", 3, line1ForAssert.SubAccounts.Count);
			AssertEquals("line2 SubAccounts.Count", 3, line2ForAssert.SubAccounts.Count);
			AssertContainSubAccount(line1ForAssert.SubAccounts, OrgHeaderSchema.Constants.Prefix, TestObjectCreator.ABIGAS.PK);
			AssertContainSubAccount(line1ForAssert.SubAccounts, AccGroupsSchema.Constants.Prefix, salesGroup.PK);
			AssertContainSubAccount(line1ForAssert.SubAccounts, GlbStaffSchema.Constants.Prefix, ZGuid.Empty);
			AssertContainSubAccount(line2ForAssert.SubAccounts, GlbStaffSchema.Constants.Prefix, staff.PK);
			AssertContainSubAccount(line2ForAssert.SubAccounts, OrgHeaderSchema.Constants.Prefix, ZGuid.Empty);
			AssertContainSubAccount(line2ForAssert.SubAccounts, AccGroupsSchema.Constants.Prefix, ZGuid.Empty);

			//Update Line's SubAccount ParentId , check the reloaded incompleteInvoice's SubAccount ParentId
			TestObjectCreator.SetUpTransactionLineSubAccount(incompleteInvoice.Lines.OfType<APInvoiceLine>().FirstOrDefault(x => x.AL_Desc.Equals("line1")), OrgHeaderSchema.Constants.Prefix, TestObjectCreator.AALSHI.PK);
			incompleteInvoice.SaveAsIncomplete();

			var incompleteInvoiceForTestCase2 = new BusinessObjectFactory().Load<APInvoice>(invoice.PK);
			incompleteInvoiceForTestCase2.RestoreSavedData();
			line1ForAssert = incompleteInvoiceForTestCase2.Lines.OfType<APInvoiceLine>().FirstOrDefault(x => x.AL_Desc.Equals("line1"));
			AssertEquals("line1 SubAccounts.Count", 3, line1ForAssert.SubAccounts.Count);
			AssertContainSubAccount(line1ForAssert.SubAccounts, OrgHeaderSchema.Constants.Prefix, TestObjectCreator.AALSHI.PK);
			AssertContainSubAccount(line1ForAssert.SubAccounts, AccGroupsSchema.Constants.Prefix, salesGroup.PK);
			AssertContainSubAccount(line1ForAssert.SubAccounts, GlbStaffSchema.Constants.Prefix, ZGuid.Empty);

			//Update dbo.AccGLHeader's SubAccountTypes, check the reloaded incompleteInvoice
			var businessObjectFactory = new BusinessObjectFactory();
			var glHeader = businessObjectFactory.Load<AccGLHeader>(TestObjectCreator.GLHeader1.PK);
			glHeader.SubAccountTypes.RemoveAndDeleteAll();
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, GlbGroupSchema.Constants.Prefix, false);
			businessObjectFactory.Save();

			var incompleteInvoiceForTestCase3 = businessObjectFactory.Load<APInvoice>(invoice.PK);
			incompleteInvoiceForTestCase3.RestoreSavedData();
			AssertEquals("incompleteInvoice.Lines.Count", 2, incompleteInvoiceForTestCase3.Lines.Count);
			line1ForAssert = incompleteInvoiceForTestCase3.Lines.OfType<APInvoiceLine>().FirstOrDefault(x => x.AL_Desc.Equals("line1"));
			line2ForAssert = incompleteInvoiceForTestCase3.Lines.OfType<APInvoiceLine>().FirstOrDefault(x => x.AL_Desc.Equals("line2"));
			AssertEquals("line1 SubAccounts.Count", 1, line1ForAssert.SubAccounts.Count);
			AssertEquals("line2 SubAccounts.Count", 1, line2ForAssert.SubAccounts.Count);
			AssertContainSubAccount(line1ForAssert.SubAccounts, GlbGroupSchema.Constants.Prefix, ZGuid.Empty);
			AssertContainSubAccount(line2ForAssert.SubAccounts, GlbGroupSchema.Constants.Prefix, ZGuid.Empty);
		}

		public void TestVATRecoverable()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();

			//invoice.AH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsActive, true)).PK;
			//AccChargeCode chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK));

			var line = (APInvoiceLine)invoice.Lines.AddNew();
			line.AL_Calc_InputGSTVATRecoverablePercentage = 77.77;

			line = (APInvoiceLine)invoice.Lines.AddNew();
			line.AL_Calc_InputGSTVATRecoverablePercentage = 0.01;

			invoice.SaveAsIncomplete();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			APInvoice incompleteInvoice = newFactory.Load<APInvoice>(invoice.PK);

			AssertEquals("incompleteInvoice.Lines.Count", 0, incompleteInvoice.Lines.Count);

			incompleteInvoice.RestoreSavedData();

			AssertEquals("incompleteInvoice.Lines.Count", 2, incompleteInvoice.Lines.Count);

			AssertEquals("line1.AL_Calc_InputGSTVATRecoverablePercentage", 77.77m, incompleteInvoice.Lines[0].AL_Calc_InputGSTVATRecoverablePercentage);
			AssertEquals("line2.AL_Calc_InputGSTVATRecoverablePercentage", 0.01m, incompleteInvoice.Lines[1].AL_Calc_InputGSTVATRecoverablePercentage);
		}

		public void TestTaxMessage()
		{
			var taxMessage = Factory.New<AccInvMsg>();
			taxMessage.A9_Code = "Blah";
			Factory.Save();

			var invoice = Factory.NewWithValidTestData<APInvoice>();
			var line = (APInvoiceLine)invoice.Lines.AddNew();
			line.AL_A9_VATClass = taxMessage.PK;
			invoice.SaveAsIncomplete();

			var newFactory = new BusinessObjectFactory();
			var incompleteInvoice = newFactory.Load<APInvoice>(invoice.PK);
			AssertEquals("incompleteInvoice.Lines.Count", 0, incompleteInvoice.Lines.Count);

			incompleteInvoice.RestoreSavedData();
			AssertEquals("incompleteInvoice.Lines.Count", 1, incompleteInvoice.Lines.Count);
			AssertEquals("line1.AL_A9_VATClass", taxMessage.PK, incompleteInvoice.Lines[0].AL_A9_VATClass);
			AssertEquals("line1.VATClass.A9_Code", "Blah", incompleteInvoice.Lines[0].VATClass.A9_Code);
		}

		public void TestLineWithExRate()
		{
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_RX_NKTransactionCurrency = "AUD";
			invoice.AH_ExchangeRate = 1m;
			var line = (APInvoiceLine)invoice.Lines.AddNew();
			line.AL_RX_NKTransactionCurrency = "USD";
			line.AL_ExchangeRate = 1.7457m;
			line.AL_OSAmount = 437.37m;
			invoice.SaveAsIncomplete();

			var newFactory = new BusinessObjectFactory();
			var incompleteInvoice = newFactory.Load<APInvoice>(invoice.PK);
			incompleteInvoice.RestoreSavedData();
			AssertEquals(1, incompleteInvoice.Lines.Count);
			AssertEquals("USD", incompleteInvoice.Lines[0].AL_RX_NKTransactionCurrency);
			AssertEquals(1.7457m, incompleteInvoice.Lines[0].AL_ExchangeRate);
			AssertEquals(437.37m, incompleteInvoice.Lines[0].AL_OSAmount);

			var invoice2 = Factory.NewWithValidTestData<APInvoice>();
			invoice2.AH_RX_NKTransactionCurrency = "USD";
			invoice2.AH_ExchangeRate = 1.3827m;
			var line2 = (APInvoiceLine)invoice2.Lines.AddNew();
			line2.AL_RX_NKTransactionCurrency = "USD";
			line2.AL_OSAmount = 472.37m;
			invoice2.SaveAsIncomplete();

			var newFactory2 = new BusinessObjectFactory();
			var incompleteInvoice2 = newFactory2.Load<APInvoice>(invoice2.PK);
			incompleteInvoice2.RestoreSavedData();
			AssertEquals(1, incompleteInvoice2.Lines.Count);
			AssertEquals("USD", incompleteInvoice2.Lines[0].AL_RX_NKTransactionCurrency);
			AssertEquals(1.3827m, incompleteInvoice2.Lines[0].AL_ExchangeRate);
			AssertEquals(472.37m, incompleteInvoice2.Lines[0].AL_OSAmount);
		}

		public void TestTaxAmountWithExtraTaxRate()
		{
			var taxRate = TestObjectCreator.CreateTaxRate("TAX", "A Tax Rate", AccTaxRate.Types.ServiceTax, 14, AccTaxRate.ExtraTypes.QuebecQST, 1, 1);
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001000");
			var shipment = TestObjectCreator.CreateShipment("S00001001", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var invoice = Factory.New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			invoice.AH_TransactionNum = "1111";

			var consolCost = invoice.ConsolCosting.ConsolCosts.AddNew();
			consolCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, JobConsolSchema.Constants.Prefix);
			consolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			consolCost.E6_AT_TaxRate = taxRate.PK;
			consolCost.E6_OSCostAmount = 2000M;
			consolCost.E6_OSGSTAmount_Calc = 300M;
			consolCost.E6_RX_NKCurrency = "AUD";
			consolCost.E6_ExchangeRate = 1M;
			consolCost.E6_ApportionmentMethod = "SHP";

			invoice.SaveAsIncomplete();

			var incompleteInvoice = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
			incompleteInvoice.RestoreSavedData();

			AssertEquals("incompleteInvoice.Lines.Count", 1, incompleteInvoice.Lines.Count);
			AssertEquals("AH_GSTAmount", 300M, incompleteInvoice.AH_OSTaxAmount);
			AssertEquals("AH_OSTotal", 2300M, incompleteInvoice.AH_OSTotalAmount);
		}

		public void TestLocalAmountsDoNotRecalculateOSAmounts()
		{
			var taxRate = TestObjectCreator.CreateTaxRate("TAX", "A Tax Rate", AccTaxRate.Types.Rated, 20, AccTaxRate.ExtraTypes.VATRetention, 5, 1);
			Factory.Save();

			var expectedOSExTaxAmount = 1261m;
			var expectedOSTaxAmount = 189.15m;
			var expectedOSGSTAmount = 252.2m;

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "00001", TestObjectCreator.USD, 1.3321m, TestObjectCreator.Creditor1);
			var line = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.USD, 1.3321m, expectedOSExTaxAmount);
			line.AL_AT = taxRate.PK;

			AssertEquals("Precondition: AL_LocalExTaxAmount", 946.63m, line.AL_LocalExTaxAmount);
			AssertEquals("Precondition: AL_LocalTaxAmount", 141.99m, line.AL_LocalTaxAmount);
			AssertEquals("Precondition: AL_LocalGSTAmount", 189.33m, line.AL_LocalGSTAmount);

			apInvoice.SaveAsIncomplete();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var incompleteInvoice = newFactory.Load<APInvoice>(apInvoice.PK);
			incompleteInvoice.RestoreSavedData();

			AssertEquals("incompleteInvoice.Lines.Count", 1, incompleteInvoice.Lines.Count);
			line = incompleteInvoice.Lines[0];
			AssertEquals("AL_OSExTaxAmount should not be recalculated", expectedOSExTaxAmount, line.AL_OSExTaxAmount);
			AssertEquals("AL_OSTaxAmount should not be recalculated", expectedOSTaxAmount, line.AL_OSTaxAmount);
			AssertEquals("AL_OSGSTAmount should not be recalculated", expectedOSGSTAmount, line.AL_OSGSTAmount);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:Do Not Cast Factory Method", Justification = "Baseline")]
		APInvoice GetFullyPopulatedInvoice(bool isSemiPopulated)
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var cachedResult = isSemiPopulated ? LastPopulatedInvoiceWithFirstSetOfPKs : LastPopulatedInvoice;
			if (cachedResult != null)
			{
				return cachedResult;
			}

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.ValidateExpectedInvoiceTotal = true;
			invoice.ExpectedInvoiceTotal = 110m;
			invoice.ExpectedInvoiceExclTaxTotal = 100m;
			invoice.ExpectedInvoiceTaxTotal = 10m;
			invoice.AH_GB_TaxBranch = TestObjectCreator.NonCurrentBranch.PK;
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			line.FillWithValidTestData();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_INCO = Enterprise.Core.Constants.IncoTerms.CostAndFreight;
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			var taxMessage = Factory.NewWithValidTestData<AccInvMsg>();
			taxMessage.A9_Code = "TAXMSG";
			var chargeCodeQuery = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			chargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_Code, "CCLR");
			AccChargeCode chargeCode = Factory.LoadTop1<AccChargeCode>(chargeCodeQuery);
			line.AL_AC = chargeCode.PK;
			line.AL_JH = job.PK;
			ZQuery taxRateQuery = new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			taxRateQuery.AddToFilter(AccTaxRateSchema.AT_Type, AccTaxRate.Types.Rated);
			taxRateQuery.AddToFilter(AccTaxRateSchema.AT_Code, "GST");
			AccTaxRate taxRate = Factory.LoadTop1<AccTaxRate>(taxRateQuery);
			taxRate.SetRate_ForTestOnly(10, 1);
			taxRate.SetExtraRate_ForTestOnly(2, 1);
			line.AL_AT = taxRate.PK;
			line.AL_OSExTaxAmount = 100m;
			line.AL_IsFinalCharge = true;
			line.AL_A9_VATClass = taxMessage.PK;
			line.AL_PlaceOfSupply = "DD";
			line.AL_PlaceOfSupplyType = "STA";
			line.AL_SupplyType = "DSB";
			line.AL_GB_TaxBranch = TestObjectCreator.NonCurrentBranch.PK;
			line.AL_GovtChargeCode = "Govt Chg Code";
			line.AL_AW = TestObjectCreator.WHT1.PK;
			line.CreateComplianceDocumentRecordOnPosting = true;
			line.ComplianceDocumentNumber = "TX12345678";
			line.ComplianceSubType = "TXI";
			line.ComplianceDocumentOrganization = TestObjectCreator.ZECTRA.PK;
			line.ComplianceDocumentVATRegistrationNum = "234";
			line.ComplianceDocumentDate = new ZDateTime(2018, 2, 1);
			line.ComplianceDocumentReportingPeriod = 201802;
			line.ComplianceDocumentSupportingReason = "XXX";
			line.ComplianceSupportingDocumentType = "xxx";
			line.ComplianceSupportingDocumentNumber = "1234";
			line.PeriodApportionmentMethod = "PER";
			line.PeriodStartDate = new ZDate(2020, 2, 1);
			line.PeriodEndDate = new ZDate(2020, 3, 1);
			line.PeriodClearingGLAccountPK = TestObjectCreator.GLJournalClearingAccount.PK;
			line.AL_TaxDate = new ZDate(2018, 2, 1);

			var relatedConsolCost = Factory.Load<JobConsolCost>(new ZGuid("d6ddb9a4-cdac-471d-8630-cafda01011d6"));
			if (relatedConsolCost == null)
			{
				relatedConsolCost = (JobConsolCost)Factory.New(typeof(JobConsolCost), new Guid("d6ddb9a4-cdac-471d-8630-cafda01011d6"));
				relatedConsolCost.FillWithValidTestData();
			}
			relatedConsolCost.E6_AC_ChargeCode = chargeCode.PK;

			JobConsolCost consolCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			consolCost.E6_AC_ChargeCode = chargeCode.PK;
			consolCost.E6_PlaceOfSupply = "DD";
			consolCost.E6_PlaceOfSupplyType = "STA";
			consolCost.E6_SupplyType = "DSB";
			consolCost.E6_OSCostAmount = 5m;
			consolCost.E6_TaxDate = new ZDate(2018, 2, 1);
			consolCost.E6_AT_TaxRate = taxRate.PK;
			consolCost.E6_A9_VATClass = taxMessage.PK;
			consolCost.E6_OSGSTAmount_Calc = 1m;
			consolCost.E6_PPDCLT = "PPD";
			consolCost.E6_ApportionmentMethod = "SHP";
			consolCost.E6_ApportionToRelatedShipments = true;
			consolCost.IsFinal = true;
			consolCost.E6_CostGovtChargeCode = "ABCD";
			consolCost.E6_SellGovtChargeCode = "EFGH";
			consolCost.RelatedConsolCostPK = relatedConsolCost.PK;
			consolCost.E6_AW = TestObjectCreator.WHT1.PK;
			consolCost.E6_GB_CostTaxBranch = TestObjectCreator.NonCurrentBranch.PK;

			Charge charge = (Charge)Factory.New(typeof(Charge), isSemiPopulated ? new Guid("93b93a16-fd0f-430e-a905-2dea2049ca9f") : new Guid("931c51e8-0e7a-4d37-bdb7-171c8380a9ad"));
			charge.JR_JH = job.PK;
			charge.FillWithValidTestData();
			charge.JR_CostSupplyType = "DSB";
			charge.JR_AC = chargeCode.PK;
			charge.JR_AT_CostGSTRate = taxRate.PK;
			charge.JR_OSCostAmt = 100m;
			charge.JR_AL_APLine = line.PK;
			charge.SetAmountsFromLinkedLinesForTests();
			charge.JR_JH_InternalJob = charge.JR_JH;
			charge.JR_GB_InternalBranch = charge.JR_GB;
			charge.JR_GE_InternalDept = charge.JR_GE;
			charge.JR_A9_CostVATClass = taxMessage.PK;
			charge.JR_SellGovtChargeCode = "Sell Govt Chg Code";
			charge.JR_CostGovtChargeCode = "Cost Govt Chg Code";
			charge.JR_GB_CostTaxBranch = TestObjectCreator.NonCurrentBranch.PK;
			charge.JR_GB_SellTaxBranch = TestObjectCreator.NonCurrentCompanyBranch.PK;

			var sellAccount = (OrgHeader)Factory.New(typeof(OrgHeader), isSemiPopulated ? new Guid("53DA0F04-0BF7-4475-AAD7-88AF356AF5AE") : new Guid("BD586057-C425-4732-93C1-CC8B5F41CFF1"));
			sellAccount.FillWithValidTestData();

			Factory.Save();

			consolCost.ApportionmentCharges[0].JR_IsUsedForApportionment = true;
			consolCost.ApportionmentCharges[0].JR_OH_SellAccount = sellAccount.PK;
			consolCost.ApportionmentCharges[0].RelatedApportionChargeFromDB = charge;
			invoice.AH_TransactionNum = isSemiPopulated ? "SEMI" : "FULL";
			line.OriginalJobCharge = charge;
			var relatedDebtor = (OrgHeader)Factory.New(typeof(OrgHeader), isSemiPopulated ? new Guid("7C126963-A428-4ABE-944D-4305BD12756B") : new Guid("E758D581-753B-47AD-8AE8-E92A71040C2B"));
			relatedDebtor.FillWithValidTestData();

			if (!isSemiPopulated)
			{
				var salesGroup = TestObjectCreator.CreateSalesGroup("SG1");
				var staff = TestObjectCreator.CreateStaff("SR1");

				var accTransactionLineSubAccount1 = line.SubAccounts.AddNew();
				accTransactionLineSubAccount1.AL1_AL = line.PK;
				accTransactionLineSubAccount1.AL1_SubClassParentTableCode = AccGroupsSchema.Constants.Prefix;
				accTransactionLineSubAccount1.AL1_SubClassParentId = salesGroup.PK;

				var accTransactionLineSubAccount2 = line.SubAccounts.AddNew();
				accTransactionLineSubAccount2.AL1_AL = line.PK;
				accTransactionLineSubAccount2.AL1_SubClassParentTableCode = GlbStaffSchema.Constants.Prefix;
				accTransactionLineSubAccount2.AL1_SubClassParentId = staff.PK;

				line.IndexOfImportedUniversalTransactionLine = 2;
			}

			if (isSemiPopulated)
			{
				LastPopulatedInvoiceWithFirstSetOfPKs = invoice;
			}
			else
			{
				LastPopulatedInvoice = invoice;
			}

			return invoice;
		}

		APInvoice LastPopulatedInvoiceWithFirstSetOfPKs;
		APInvoice LastPopulatedInvoice;

		public void TestLocalAmountsBackwardCompatibility()
		{
			var collection = new InvoicingBaseCollection(Factory);
			using (var stream = Retriever.GetStream("FullyPopulatedIncompleteInvoiceBeforeLocalAmountsAdded.xml"))
			{
				var dataAdapter = GetNewBizObjXmlDataAdapter();
				var serializer = new XmlValueObjectSerializer(dataAdapter.ValueObjectType);
				serializer.ImportXmlData(stream, dataAdapter, collection, null, new NotificationBuffer());
			}

			AssertEquals("An invoice should be imported.", 1, collection.Count);
			var invoice = collection[0];
			AssertEquals("AH_OSExTaxAmount", 100M, invoice.AH_OSExTaxAmount);
			AssertEquals("AH_OSTaxAmount", 12.2M, invoice.AH_OSTaxAmount);
			AssertEquals("AH_LocalExTaxAmount", 100M, invoice.AH_LocalExTaxAmount);
			AssertEquals("AH_LocalTaxAmount", 12.2M, invoice.AH_LocalTaxAmount);
			var line = invoice.Lines[0];
			AssertEquals("AL_OSExTaxAmount", 100M, line.AL_OSExTaxAmount);
			AssertEquals("AL_OSTaxAmount", 12.2M, line.AL_OSTaxAmount);
			AssertEquals("AL_OSGSTAmount", 10M, line.AL_OSGSTAmount);
			AssertEquals("AL_LocalExTaxAmount", 100M, line.AL_LocalExTaxAmount);
			AssertEquals("AL_LocalTaxAmount", 12.2M, line.AL_LocalTaxAmount);
			AssertEquals("AL_LocalGSTAmount", 10M, line.AL_LocalGSTAmount);
			var charge = invoice.ConsolCosting.ConsolCosts[0].ApportionmentCharges[0];
			AssertEquals("JR_OSCostAmt", 5m, charge.JR_OSCostAmt);
			AssertEquals("JR_LocalCostAmt", 5m, charge.JR_LocalCostAmt);
		}

		public void TestVATRecoverableBackwardCompatibility()
		{
			var collection = new InvoicingBaseCollection(Factory);
			using (var stream = Retriever.GetStream("FullyPopulatedIncompleteInvoiceBeforeVATRecoverableAdded.xml"))
			{
				var dataAdapter = GetNewBizObjXmlDataAdapter();
				var serializer = new XmlValueObjectSerializer(dataAdapter.ValueObjectType);
				serializer.ImportXmlData(stream, dataAdapter, collection, null, new NotificationBuffer());
			}

			AssertEquals("An invoice should be imported.", 1, collection.Count);
			var invoice = collection[0];
			var line = invoice.Lines[0];
			AssertEquals("AL_InputGSTVATRecoverable", 1m, line.AL_InputGSTVATRecoverable);
		}

		public void TestExchangeRateReadFromXmlWhenAvaliable()
		{
			AssertExchangeRateReadFromXmlWhenAvaliable(Retriever.SaveResourceToFile("FullyPopulatedIncompleteInvoiceBeforeAddExchangeRate.xml"), 1m);
			AssertExchangeRateReadFromXmlWhenAvaliable(Retriever.SaveResourceToFile("FullyPopulatedIncompleteInvoiceAfterAddExchangeRate.xml"), 1.5m);

			void AssertExchangeRateReadFromXmlWhenAvaliable(string testFile, ZDecimal expectedExchangeRate)
			{
				var collection = new InvoicingBaseCollection(Factory);
				using (var stream = StreamConverter.StringToStream(File.ReadAllText(testFile)))
				{
					var dataAdapter = GetNewBizObjXmlDataAdapter() as IncompleteTransactionDataAdapter<APInvoice>;
					var serializer = new XmlValueObjectSerializer(dataAdapter.ValueObjectType);
					serializer.ImportXmlData(stream, dataAdapter, collection, null, new NotificationBuffer());
				}

				AssertEquals("An invoice should be imported.", 1, collection.Count);
				var invoice = collection[0];
				var line = invoice.Lines[0];
				AssertEquals("AL_ExchangeRate", expectedExchangeRate, line.AL_ExchangeRate);

				var consolCost = invoice.ConsolCosting.ConsolCosts[0];
				AssertEquals("consolCost.E6_ExchangeRate", expectedExchangeRate, consolCost.E6_ExchangeRate);
			}
		}

		public void TestPerformanceImprovementForImport()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery()).PK;
			job.JH_GB = ObjectCreator.NonCurrentBranch.PK;

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job2 = new Job.Loader(shipment2).TryLoadOrCreateWithoutMutexForTestOnly();
			job2.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery()).PK;
			job2.JH_GB = ObjectCreator.NonCurrentBranch.PK;

			Factory.Save();

			AccChargeCode chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK));

			ZQuery taxRateQuery = new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			taxRateQuery.AddToFilter(AccTaxRateSchema.AT_Type, AccTaxRate.Types.Rated);
			taxRateQuery.AddToFilter(AccTaxRateSchema.AT_Code, "GST");
			AccTaxRate taxRate = Factory.LoadTop1<AccTaxRate>(taxRateQuery);
			taxRate.SetRate_ForTestOnly(10, 1);

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();

			invoice.AH_OH = ObjectCreator.Creditor1.PK;
			invoice.AH_RX_NKTransactionCurrency = "USD";
			invoice.AH_ExchangeRate = 2m;

			var line1 = ObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, chargeCode, ObjectCreator.USD, 2m, "Desc", 100m);
			var line2 = ObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job2, chargeCode, ObjectCreator.USD, 2m, "Desc", 200m);
			var line3 = ObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, chargeCode, ObjectCreator.USD, 2m, "Desc", 300m);

			invoice.SaveAsIncomplete();

			InvoicingBase reloadedIncomplete = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
			reloadedIncomplete.ShowError = new InvoicingBase.ShowErrorHandler(ShowError);

			reloadedIncomplete.RestoreSavedData();
			AssertEquals("ValidateAH_OSTotalAmount should not be called on each line, expect only 1 calls", 1, reloadedIncomplete.ValidateAH_OSTotalAmountCallCount_ForTestOnly);
			AssertEquals("Line's Job should not call validations inside SetParentCore", 0, reloadedIncomplete.Lines[0].InvoicingJob.ValidationCallCountInsideSetParentCore_ForTestOnly);
			AssertEquals("Line's Job should not call validations inside SetParentCore", 0, reloadedIncomplete.Lines[1].InvoicingJob.ValidationCallCountInsideSetParentCore_ForTestOnly);
			AssertEquals("Line's Job should not call validations inside SetParentCore", 0, reloadedIncomplete.Lines[2].InvoicingJob.ValidationCallCountInsideSetParentCore_ForTestOnly);

			int jobDBHits = reloadedIncomplete.Factory.GetTableHitCount(JobHeaderSchema.Constants.TableName);
			AssertEquals("Expect only 1 db hits for jobheader", 1, jobDBHits);
		}

		public void TestDetachedShipmentValidationError()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var shipment1 = TestObjectCreator.CreateShipment("S00001004", "AUSYD", "NZAKL", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var shipment2 = TestObjectCreator.CreateShipment("S00001005", "AUSYD", "NZAKL", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			Factory.Save();

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "00001", TestObjectCreator.USD, 1.3321m, TestObjectCreator.Creditor1);
			var consolCost = TestObjectCreator.CreateConsolCost(apInvoice, consol, TestObjectCreator.CC1, 100);
			apInvoice.ImportAllApportionmentsFromCosting();
			AssertEquals("Precondition: Line Count", 2, apInvoice.Lines.Count);

			apInvoice.SaveAsIncomplete();

			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			var consolInOtherFactory = otherFactory.Load<ForwardingConsol>(consol.PK);
			var shipmentInOtherFactory = otherFactory.Load<ForwardingShipment>(shipment1.PK);
			consolInOtherFactory.Shipments.Remove(shipmentInOtherFactory);
			otherFactory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var incompleteInvoice = newFactory.Load<APInvoice>(apInvoice.PK);
			incompleteInvoice.RestoreSavedData();

			incompleteInvoice.ConsolCosting.ConsolCosts.RunPreSaveValidation();

			var restoredConsolCost = incompleteInvoice.ConsolCosting.ConsolCosts[0];
			var charge = restoredConsolCost.ApportionmentCharges[0];
			AssertHasError("JR_IsUsedForApportionment should have error",
						   charge.JR_IsUsedForApportionmentInfo,
						   "This apportioned charge belongs to job: " + charge.JR_JobNumber + " which is no longer attached to the consol or is inactive. Please untick Is Used or re-attach job to consol/activate job.");
		}

		public void TestExchangeRateForPostedToEFT()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001");
			var shipment = TestObjectCreator.CreateShipment("S00001004", "AUSYD", "NZAKL", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var consolcost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.USD, 1.5m, 150m, TestObjectCreator.AALSHI);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoiceWithMinimumTestData<APInvoice>(Factory);
			var originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);

			var newFactory = new BusinessObjectFactory();
			var importer = new InvoicingBaseConsolCostImporter(newFactory, originatingCost, invoice);
			importer.ImportCostsIntoCosting(new BusinessObject[] { consolcost });

			var importedConsolCost = invoice.ConsolCosting.ConsolCosts.Cast<JobConsolCost>().FirstOrDefault(x => x.E6_RX_NKCurrency == TestObjectCreator.USD.Code);
			AssertNotNull("Precondition: Consol cost imported", importedConsolCost);
			AssertEquals(1.5m, importedConsolCost.E6_ExchangeRate);

			invoice.AH_RX_NKTransactionCurrency = "USD";
			invoice.AH_ExchangeRate = 1.2;
			invoice.AH_PostedToEFT = true;
			AssertEquals("Precondition: Invoice exchange rate is consol rate", 1.5m, invoice.AH_ExchangeRate);
			AssertEquals("Precondition: Line exchange rate is consol rate", 1.5m, invoice.Lines[0].AL_ExchangeRate);
			invoice.SaveAsIncomplete();

			var otherFactoryFactory = new BusinessObjectFactory();
			var incompleteInvoice = otherFactoryFactory.Load<APInvoice>(invoice.PK);
			incompleteInvoice.RestoreSavedData();

			AssertEquals("Invoice exchange rate should be same as what was saved", 1.5m, incompleteInvoice.AH_ExchangeRate);
			AssertEquals("Line exchange rate should be same as what was saved", 1.5m, invoice.Lines[0].AL_ExchangeRate);
			AssertEquals("Posted To EFT", true, incompleteInvoice.AH_PostedToEFT);
		}

		[ExpectNoExceptions]
		public void TestGetOriginalJobChargeWithNoNotification()
		{
			IncompleteTransactionDataAdapter<APInvoice> adapter = (IncompleteTransactionDataAdapter<APInvoice>)GetNewBizObjXmlDataAdapter();
			StringReader reader = new StringReader(@"<?xml version=""1.0"" encoding=""utf-8""?>
				<IncompleteTransaction xmlns=""http://www.edi.com.au/EnterpriseService/"">
					<InvoiceNumber>SEMI</InvoiceNumber>
					<JobRelatedLines>
					<JobRelatedLine>
						<JobNumber>S00001000</JobNumber>
						<GenericChargeCode>CCLR</GenericChargeCode>
						<Description>Customs Clearance / Agency Fees</Description>
						<Branch>TBC</Branch>
						<Department>4VP</Department>
						<Amount CurrencyCode=""AUD"">0</Amount>
						<TaxId>GST</TaxId>
						<TaxAmount CurrencyCode=""AUD"">10</TaxAmount>
						<GstAmount CurrencyCode=""AUD"">8.20</GstAmount>
						<GstInclusiveAmount CurrencyCode=""AUD"">-10</GstInclusiveAmount>
						<IsFinal>false</IsFinal>
						<OriginalJobCharge>ffd951e1-0e1e-4efd-904d-f03f09c866fc</OriginalJobCharge>
					</JobRelatedLine>
					</JobRelatedLines>
				</IncompleteTransaction>");
			XmlTextReader xmlReader = new XmlTextReader(reader);

			InvoicingBaseCollection collection = new InvoicingBaseCollection(Factory);
			XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(typeof(IncompleteTransactionHeader));
			var notification = new NotificationBuffer();
			serializer.ReadInterchangeOrCollectionFromXml(xmlReader, adapter, collection, null, notification);

			Assert("Should not have the message", !notification.AsString.Contains("Could not find original charge"));
		}

		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		public void TestSaveAsIncompleteWithEmptyBanchAndCompany()
		{
			var taxRate = TestObjectCreator.CreateTaxRate("TAX", "A Tax Rate", AccTaxRate.Types.Rated, 20, AccTaxRate.ExtraTypes.VATRetention, 5, 1);
			Factory.Save();

			var expectedOSExTaxAmount = 1261m;

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "00001", TestObjectCreator.USD, 1.3321m, TestObjectCreator.Creditor1);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.USD, 1.3321m, expectedOSExTaxAmount);
			line.AL_AT = taxRate.PK;

			AssertEquals("Precondition: AL_LocalExTaxAmount", 946.63m, line.AL_LocalExTaxAmount);
			AssertEquals("Precondition: AL_LocalTaxAmount", 141.99m, line.AL_LocalTaxAmount);
			AssertEquals("Precondition: AL_LocalGSTAmount", 189.33m, line.AL_LocalGSTAmount);

			line.AL_GB = ZGuid.Empty;
			line.AL_GE = ZGuid.Empty;
			AssertNull("Precondition: Branch is null", line.Branch);
			AssertNull("Precondition: line.Department is null", line.Department);

			AssertNoExceptionThrown(() => invoice.SaveAsIncomplete());

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			APInvoice incompleteInvoice = newFactory.Load<APInvoice>(invoice.PK);

			AssertEquals("incompleteInvoice.Lines.Count", 0, incompleteInvoice.Lines.Count);

			incompleteInvoice.RestoreSavedData();

			AssertEquals("incompleteInvoice.Lines.Count", 1, incompleteInvoice.Lines.Count);

			AssertNull("line.Branch should be null", incompleteInvoice.Lines[0].Branch);
			AssertNull("line.Department should be null", incompleteInvoice.Lines[0].Department);

			AssertEquals("line.AL_LocalExTaxAmount", 946.63m, incompleteInvoice.Lines[0].AL_LocalExTaxAmount);
			AssertEquals("line.AL_LocalTaxAmount", 141.99m, incompleteInvoice.Lines[0].AL_LocalTaxAmount);
			AssertEquals("line.AL_LocalGSTAmount", 189.33m, incompleteInvoice.Lines[0].AL_LocalGSTAmount);
		}

		public void TestIndexOfImportedUniversalTransactionLineIsSaved()
		{
			var invoice = Factory.NewWithValidTestData<APInvoice>();

			invoice.AH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsActive, true)).PK;

			var line = (APInvoiceLine)invoice.Lines.AddNew();
			line.IndexOfImportedUniversalTransactionLine = 4;

			var line2 = (APInvoiceLine)invoice.Lines.AddNew();

			var line3 = (APInvoiceLine)invoice.Lines.AddNew();
			line3.IndexOfImportedUniversalTransactionLine = 7;

			invoice.SaveAsIncomplete();

			var newFactory = new BusinessObjectFactory();
			var incompleteInvoice = newFactory.Load<APInvoice>(invoice.PK);
			incompleteInvoice.RestoreSavedData();

			AssertEquals("incompleteInvoice.Lines.Count", 3, incompleteInvoice.Lines.Count);

			AssertEquals("line.IndexOfImportedUniversalTransactionLine", 4, incompleteInvoice.Lines[0].IndexOfImportedUniversalTransactionLine);
			AssertEquals("line.IndexOfImportedUniversalTransactionLine", -1, incompleteInvoice.Lines[1].IndexOfImportedUniversalTransactionLine);
			AssertEquals("line.IndexOfImportedUniversalTransactionLine", 7, incompleteInvoice.Lines[2].IndexOfImportedUniversalTransactionLine);
		}

		public void TestSecurityRight_AllowUntickAutoTickedFinalFlag()
		{
			var expectedError = @"You do not have appropriate security rights to un-tick this flag.
Please contact your system administrator for the following security right: Manage > Payables > Payables Transactions > New Transactions > Invoice > Allow Untick Auto-ticked Final Flag";
			Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = true;
			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.JobAndChargeCode);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var shipment = TestObjectCreator.CreateShipment("S00001004", "AUSYD", "NZAKL", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "00001", TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1);
			var consolCost = TestObjectCreator.CreateConsolCost(apInvoice, consol, TestObjectCreator.CC1, 30m);
			consolCost.ApportionmentCharges[0].JR_LocalCostAmt = 30m;
			Assert("Should be ticked, 30 < 100", consolCost.ApportionmentCharges[0].IsFinal);
			apInvoice.ImportAllApportionmentsFromCosting();
			Assert("Should be equal to the apportion charge", apInvoice.Lines[0].AL_IsFinalCharge);

			var line2 = (APInvoiceLine)apInvoice.Lines.AddNew();
			line2.AL_AC = TestObjectCreator.CC2.PK;
			line2.AL_JH = job.PK;
			line2.AL_LocalExTaxAmount = 150m;
			Assert("Should not be ticked, 150 > 100", !line2.AL_IsFinalCharge);
			line2.AL_IsFinalCharge = true;

			var line3 = (APInvoiceLine)apInvoice.Lines.AddNew();
			line3.AL_AC = TestObjectCreator.CC3.PK;
			line3.AL_JH = job.PK;
			line3.AL_LocalExTaxAmount = 50m;
			Assert("Should be ticked, 50 < 100", line3.AL_IsFinalCharge);

			AssertEquals("Precondition: Line Count", 3, apInvoice.Lines.Count);
			apInvoice.SaveAsIncomplete();

			Env.Security.AllowUntickAutoTickedFinalFlag.IsAllowed = false;
			var newFactory = new BusinessObjectFactory();
			var incompleteInvoice = newFactory.Load<APInvoice>(apInvoice.PK);
			incompleteInvoice.RestoreSavedData();

			AssertEquals("incompleteInvoice.Lines.Count", 3, incompleteInvoice.Lines.Count);

			var incompleteInvoiceLine1 = incompleteInvoice.Lines.Cast<InvoicingLineBase>().FirstOrDefault(x => x.AL_LocalExTaxAmount == 30m);
			var incompleteInvoiceLine2 = incompleteInvoice.Lines.Cast<InvoicingLineBase>().FirstOrDefault(x => x.AL_LocalExTaxAmount == 150m);
			var incompleteInvoiceLine3 = incompleteInvoice.Lines.Cast<InvoicingLineBase>().FirstOrDefault(x => x.AL_LocalExTaxAmount == 50m);
			Assert("Should be equal to the value in XML", incompleteInvoiceLine1.AL_IsFinalCharge);
			AssertNoErrors("Shouldn't have any error", incompleteInvoiceLine1.AL_IsFinalChargeInfo);
			Assert("Should be equal to the value in XML", incompleteInvoiceLine2.AL_IsFinalCharge);
			AssertNoErrors("Shouldn't have any error", incompleteInvoiceLine2.AL_IsFinalChargeInfo);
			Assert("Should be equal to the value in XML", incompleteInvoiceLine3.AL_IsFinalCharge);
			AssertNoErrors("Shouldn't have any error", incompleteInvoiceLine3.AL_IsFinalChargeInfo);

			incompleteInvoiceLine2.AL_IsFinalCharge = false;
			AssertNoErrors("Shouldn't have any error, because the flag is ticked manually", incompleteInvoiceLine2.AL_IsFinalChargeInfo);
			incompleteInvoiceLine3.AL_IsFinalCharge = false;
			AssertHasError("Should have the error, because we don't have the security right to untick the automatically ticked flag", incompleteInvoiceLine3.AL_IsFinalChargeInfo, expectedError);

			Env.Security.AllowUntickAutoTickedFinalFlag.IsAllowed = true;
			incompleteInvoiceLine3.Validation.ValidateAL_IsFinalCharge();
			AssertNoErrors("Shouldn't have any error, because we have the security right", incompleteInvoiceLine3.AL_IsFinalChargeInfo);
		}

		public void TestGovernmentReportingChargeCode()
		{
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsActive, true)).PK;

			var line = (APInvoiceLine)invoice.Lines.AddNew();
			line.AL_GovtChargeCode = "AAA";

			invoice.Lines.AddNew();

			invoice.SaveAsIncomplete();

			var newFactory = new BusinessObjectFactory();
			var incompleteInvoice = newFactory.Load<APInvoice>(invoice.PK);
			incompleteInvoice.RestoreSavedData();

			AssertEquals("incompleteInvoice.Lines.Count", 2, incompleteInvoice.Lines.Count);
			AssertEquals("line.AL_GovtChargeCode", "AAA", incompleteInvoice.Lines[0].AL_GovtChargeCode);
			AssertEquals("line.AL_GovtChargeCode", "", incompleteInvoice.Lines[1].AL_GovtChargeCode);
		}

		public void TestFixedPlaceOfSupplyAndFixedPlaceOfSupplyTypeWithIncompleteInvoice()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var invoice = Factory.NewWithValidTestData<APInvoice>();

				invoice.AH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsActive, true)).PK;
				var line = (APInvoiceLine)invoice.Lines.AddNew();
				line.AL_PlaceOfSupply = "DD"; // line.AL_PlaceOfSupplyType will be set when AL_PlaceOfSupply is set

				invoice.SaveAsIncomplete();

				var newFactory = new BusinessObjectFactory();
				var incompleteInvoice = newFactory.Load<APInvoice>(invoice.PK);
				incompleteInvoice.RestoreSavedData();

				AssertEquals("incompleteInvoice.Lines.Count", 1, incompleteInvoice.Lines.Count);
				AssertEquals("line.AL_PlaceOfSupply", "DD", incompleteInvoice.Lines[0].AL_PlaceOfSupply);
				AssertEquals("line.AL_PlaceOfSupplyType", "STA", incompleteInvoice.Lines[0].AL_PlaceOfSupplyType);
			}
		}

		public void TestTaxBranchWithIncompleteInvoice()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var costTaxBranch = TestObjectCreator.CreateBranch("BR1", "CostTaxBranch", GlbCompany.CurrentCompany);
				var sellTaxBranch = TestObjectCreator.CreateBranch("BR2", "SellTaxBranch", GlbCompany.CurrentCompany);
				Factory.Save();

				var invoice = Factory.NewWithValidTestData<APInvoice>();
				var consolCost = invoice.ConsolCosting.ConsolCosts.AddNew();
				consolCost.E6_GB_CostTaxBranch = costTaxBranch.PK;

				var charge = consolCost.ApportionmentCharges.AddNew();
				charge.JR_GB_CostTaxBranch = costTaxBranch.PK;
				charge.JR_GB_SellTaxBranch = sellTaxBranch.PK;

				var line = (APInvoiceLine)invoice.Lines.AddNew();
				line.AL_GB_TaxBranch = costTaxBranch.PK;

				invoice.SaveAsIncomplete();

				var newFactory = new BusinessObjectFactory();
				var incompleteInvoice = newFactory.Load<APInvoice>(invoice.PK);
				incompleteInvoice.RestoreSavedData();

				AssertEquals(1, incompleteInvoice.Lines.Count);
				AssertEquals(costTaxBranch.PK, incompleteInvoice.Lines[0].AL_GB_TaxBranch);

				AssertEquals(1, incompleteInvoice.ConsolCosting.ConsolCosts.Count);
				AssertEquals(costTaxBranch.PK, incompleteInvoice.ConsolCosting.ConsolCosts[0].E6_GB_CostTaxBranch);

				AssertEquals(1, incompleteInvoice.ConsolCosting.ConsolCosts[0].ApportionmentCharges.Count);
				AssertEquals(costTaxBranch.PK, incompleteInvoice.ConsolCosting.ConsolCosts[0].ApportionmentCharges[0].JR_GB_CostTaxBranch);
				AssertEquals(sellTaxBranch.PK, incompleteInvoice.ConsolCosting.ConsolCosts[0].ApportionmentCharges[0].JR_GB_SellTaxBranch);
			}
		}

		public void TestSupplyTypeWithIncompleteInvoice()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var invoice = Factory.NewWithValidTestData<APInvoice>();

				invoice.AH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsActive, true)).PK;
				var line = (APInvoiceLine)invoice.Lines.AddNew();
				line.AL_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.DSB;

				invoice.SaveAsIncomplete();

				var newFactory = new BusinessObjectFactory();
				var incompleteInvoice = newFactory.Load<APInvoice>(invoice.PK);
				incompleteInvoice.RestoreSavedData();

				AssertEquals("incompleteInvoice.Lines.Count", 1, incompleteInvoice.Lines.Count);
				AssertEquals("line.AL_SupplyType", AccountingMasterFilesConstants.SupplyTypeClassificationCodes.DSB, incompleteInvoice.Lines[0].AL_SupplyType);
			}
		}

		public void TestGovernmentChargeCodeWhenOverride()
		{
			var chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_GovtChargeCode = "ABCD";
			var creditor = TestObjectCreator.AALSHI;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			var appListing = new ApportionmentListing(Factory, consol);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, chargeCode, 500, creditor, "SHP", appListing);
			consolCost.E6_CostGovtChargeCode = "XYZ";
			Factory.Save();

			AssertEquals("Precondition", "ABCD", chargeCode.AC_GovtChargeCode);

			var invoice = TestObjectCreator.CreateInvoiceWithMinimumTestData<APInvoice>(Factory);
			invoice.AH_OH = creditor.PK;
			invoice.SubmittedFromInvoicingForm = true;
			invoice.AH_RX_NKTransactionCurrency = "INR";
			invoice.AH_ExchangeRate = 1m;

			var invoiceConsolCost = TestObjectCreator.CreateConsolCost(invoice, consol, chargeCode, 750, creditor);
			invoiceConsolCost.RelatedConsolCostPK = consolCost.PK;
			invoice.ImportAllApportionmentsFromCosting();

			AssertEquals("The count of ap invoice line should be 2", 2, invoice.Lines.Count);
			AssertEquals("The government charge code of first invoice line shoule be 'XYZ'", "XYZ", invoice.Lines[0].AL_GovtChargeCode);
			AssertEquals("The government charge code of second invoice line shoule be 'XYZ'", "XYZ", invoice.Lines[1].AL_GovtChargeCode);

			invoice.SaveAsIncomplete();

			var newFactory = new BusinessObjectFactory();
			var incompleteInvoice = newFactory.Load<APInvoice>(invoice.PK);
			incompleteInvoice.RestoreSavedData();

			AssertEquals("The count of incomplete invoice line should be 2", 2, incompleteInvoice.Lines.Count);
			AssertEquals("The government charge code of first invoice line shoule be 'XYZ'", "XYZ", incompleteInvoice.Lines[0].AL_GovtChargeCode);
			AssertEquals("The government charge code of second invoice line shoule be 'XYZ'", "XYZ", incompleteInvoice.Lines[1].AL_GovtChargeCode);
		}

		public void TestSellGovernmentChargeCodeWhenOverride()
		{
			var chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_GovtChargeCode = "99998888";
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.AddNew();

			var appListing = new ApportionmentListing(Factory, consol);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, chargeCode, 500, TestObjectCreator.AALSHI, "SHP", appListing);
			consolCost.E6_SellGovtChargeCode = "SellCode";
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoiceWithMinimumTestData<APInvoice>(Factory);
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice.SubmittedFromInvoicingForm = true;
			invoice.AH_RX_NKTransactionCurrency = "INR";
			invoice.AH_ExchangeRate = 1m;

			var invoiceConsolCost = TestObjectCreator.CreateConsolCost(invoice, consol, chargeCode, 750, TestObjectCreator.AALSHI);
			invoiceConsolCost.RelatedConsolCostPK = consolCost.PK;
			AssertEquals("SellCode", invoiceConsolCost.E6_SellGovtChargeCode);
			invoice.ImportAllApportionmentsFromCosting();
			invoice.SaveAsIncomplete();

			var newFactory = new BusinessObjectFactory();
			var incompleteInvoice = newFactory.Load<APInvoice>(invoice.PK);
			incompleteInvoice.RestoreSavedData();
			AssertEquals(1, incompleteInvoice.ConsolCosting.ConsolCosts.Count);
			AssertEquals("SellCode", incompleteInvoice.ConsolCosting.ConsolCosts[0].E6_SellGovtChargeCode);

			incompleteInvoice.ConsolCosting.ConsolCosts[0].E6_SellGovtChargeCode = "New SellCode";
			incompleteInvoice.MoveFromIncompleteToPayableLedger();
			newFactory.Save();
			AssertEquals(true, incompleteInvoice.IsInDatabase);

			appListing = new ApportionmentListing(Factory, consol);
			AssertEquals("New SellCode", appListing.CostsCollection[0].E6_SellGovtChargeCode);
		}

		public void TestRelatedCostFromDatabase()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var job = TestObjectCreator.CreateJob(shipment, false);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, 100m, TestObjectCreator.AALSHI);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV123");
			var importedConoslCost = TestObjectCreator.CreateConsolCost(invoice, consol, TestObjectCreator.CC1, 100);
			importedConoslCost.RelatedConsolCostPK = consolCost.PK;

			AssertEquals("Precondition:", 1, importedConoslCost.ApportionmentCharges.Count);
			importedConoslCost.ApportionmentCharges[0].RelatedApportionChargeFromDB = consolCost.ApportionmentCharges[0];
			invoice.ImportAllApportionmentsFromCosting();

			AssertEquals("Precondition:", 1, invoice.ConsolCosting.ConsolCosts.Count);
			Assert("Precondition:", invoice.ConsolCosting.ConsolCosts[0].RelatedConsolCostPK.IsValid);
			invoice.SaveAsIncomplete();

			InvoicingBase reloadedIncomplete = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
			reloadedIncomplete.RestoreSavedData();

			AssertEquals(1, reloadedIncomplete.ConsolCosting.ConsolCosts.Count);
			Assert(reloadedIncomplete.ConsolCosting.ConsolCosts[0].RelatedConsolCostPK.IsValid);
		}

		public void TestRelatedCostFromDatabaseIsNullWhenChanged()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var job = TestObjectCreator.CreateJob(shipment, false);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, 100m, TestObjectCreator.AALSHI);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV123");
			var importedConoslCost = TestObjectCreator.CreateConsolCost(invoice, consol, TestObjectCreator.CC1, 100);
			importedConoslCost.RelatedConsolCostPK = consolCost.PK;

			AssertEquals("Precondition:", 1, importedConoslCost.ApportionmentCharges.Count);
			importedConoslCost.ApportionmentCharges[0].RelatedApportionChargeFromDB = consolCost.ApportionmentCharges[0];
			invoice.ImportAllApportionmentsFromCosting();

			AssertEquals("Precondition:", 1, invoice.ConsolCosting.ConsolCosts.Count);
			Assert("Precondition:", invoice.ConsolCosting.ConsolCosts[0].RelatedConsolCostPK.IsValid);
			invoice.SaveAsIncomplete();

			var newFactory = new BusinessObjectFactory();
			var newConsolCost = newFactory.Load<JobConsolCost>(consolCost.PK);
			newConsolCost.E6_AC_ChargeCode = TestObjectCreator.CC2.PK;
			newFactory.Save();

			var reloadedIncomplete = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
			reloadedIncomplete.RestoreSavedData();

			AssertEquals(1, reloadedIncomplete.ConsolCosting.ConsolCosts.Count);
			Assert(!reloadedIncomplete.ConsolCosting.ConsolCosts[0].RelatedConsolCostPK.IsValid);
		}

		public void TestRelatedApportionChargeFromDB()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var job = TestObjectCreator.CreateJob(shipment, false);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, 100m, TestObjectCreator.AALSHI);
			Factory.Save();

			AssertEquals("Precondition:", 1, consolCost.ApportionmentCharges.Count);

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV123");
			var originatingCost = TestObjectCreator.CreateConsolCost(invoice, consol, TestObjectCreator.CC1, 100);
			originatingCost.RelatedConsolCostPK = consolCost.PK;

			AssertEquals("Precondition:", 1, originatingCost.ApportionmentCharges.Count);
			originatingCost.ApportionmentCharges[0].RelatedApportionChargeFromDB = consolCost.ApportionmentCharges[0];
			invoice.ImportAllApportionmentsFromCosting();

			invoice.SaveAsIncomplete();

			InvoicingBase reloadedIncomplete = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
			reloadedIncomplete.RestoreSavedData();

			AssertEquals(1, reloadedIncomplete.ConsolCosting.ConsolCosts.Count);
			AssertEquals("apportion charge count is 1", 1, reloadedIncomplete.ConsolCosting.ConsolCosts[0].ApportionmentCharges.Count);
			AssertNotNull("RelatedApportionChargeFromDB of apportion charge is not null", reloadedIncomplete.ConsolCosting.ConsolCosts[0].ApportionmentCharges[0].RelatedApportionChargeFromDB);
			AssertEquals("RelatedApportionChargeFromDB of apportion charge should be original charge in DB", consolCost.ApportionmentCharges[0].PK, reloadedIncomplete.ConsolCosting.ConsolCosts[0].ApportionmentCharges[0].RelatedApportionChargeFromDB.PK);
		}

		public void TestRelatedApportionChargeFromDBIsNullWhenChanged()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var job = TestObjectCreator.CreateJob(shipment, false);
			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, 100m, TestObjectCreator.AALSHI);
			var consolCost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC2, TestObjectCreator.AUD, 1m, 200m, TestObjectCreator.AALSHI);
			var consolCost3 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC3, TestObjectCreator.AUD, 1m, 300m, TestObjectCreator.AALSHI);
			var consolCost4 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC4, TestObjectCreator.AUD, 1m, 400m, TestObjectCreator.AALSHI);
			Factory.Save();

			var listing = new ApportionmentListing(Factory, consol);
			AssertEquals("Precondition:", 4, listing.CostsCollection.Count);
			for (int i = 0; i < 4; i++)
			{
				AssertEquals("Precondition:", 1, listing.CostsCollection[i].ApportionmentCharges.Count);
			}

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV123");

			var originatingCost1 = TestObjectCreator.CreateConsolCost(invoice, consol, TestObjectCreator.CC1, 100);
			originatingCost1.RelatedConsolCostPK = consolCost1.PK;
			AssertEquals("Precondition:", 1, originatingCost1.ApportionmentCharges.Count);
			originatingCost1.ApportionmentCharges[0].RelatedApportionChargeFromDB = consolCost1.ApportionmentCharges[0];

			var originatingCost2 = TestObjectCreator.CreateConsolCost(invoice, consol, TestObjectCreator.CC2, 200);
			originatingCost2.RelatedConsolCostPK = consolCost2.PK;
			AssertEquals("Precondition:", 1, originatingCost2.ApportionmentCharges.Count);
			originatingCost2.ApportionmentCharges[0].RelatedApportionChargeFromDB = consolCost2.ApportionmentCharges[0];

			var originatingCost3 = TestObjectCreator.CreateConsolCost(invoice, consol, TestObjectCreator.CC3, 300);
			originatingCost3.RelatedConsolCostPK = consolCost3.PK;
			AssertEquals("Precondition:", 1, originatingCost3.ApportionmentCharges.Count);
			originatingCost3.ApportionmentCharges[0].RelatedApportionChargeFromDB = consolCost3.ApportionmentCharges[0];

			var originatingCost4 = TestObjectCreator.CreateConsolCost(invoice, consol, TestObjectCreator.CC4, 400);
			originatingCost4.RelatedConsolCostPK = consolCost4.PK;
			AssertEquals("Precondition:", 1, originatingCost4.ApportionmentCharges.Count);
			originatingCost4.ApportionmentCharges[0].RelatedApportionChargeFromDB = consolCost4.ApportionmentCharges[0];

			invoice.ImportAllApportionmentsFromCosting();

			AssertEquals(4, invoice.ConsolCosting.ConsolCosts.Count);
			for (int i = 0; i < 4; i++)
			{
				AssertEquals("Precondition:", 1, invoice.ConsolCosting.ConsolCosts[i].ApportionmentCharges.Count);
				AssertNotNull("Precondition:", invoice.ConsolCosting.ConsolCosts[i].ApportionmentCharges[0].RelatedApportionChargeFromDB);
				AssertEquals("Precondition:", listing.CostsCollection[i].ApportionmentCharges[0].PK, invoice.ConsolCosting.ConsolCosts[i].ApportionmentCharges[0].RelatedApportionChargeFromDB.PK);
			}

			AssertEquals(4, invoice.Lines.Count);
			for (int i = 0; i < 4; i++)
			{
				Assert("Precondition:", invoice.Lines[0].IsPopulatedFromImportedJobCharge);
			}

			invoice.SaveAsIncomplete();

			var newFactory = new BusinessObjectFactory();
			var newJob = newFactory.Load<Job>(job.PK);
			AssertEquals(4, newJob.Charges.Count);
			newJob.Charges[0].JR_AC = TestObjectCreator.CC5.PK;
			newJob.Charges[0].JR_OSCostAmt = 100M;
			newJob.Charges[1].JR_JH = new TestObjectCreator(newFactory).Job1.PK;
			newJob.Charges[2].JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			newJob.Charges[3].JR_GE = TestObjectCreator.NonCurrentDepartment.PK;
			newFactory.Save();

			InvoicingBase reloadedIncomplete = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
			reloadedIncomplete.RestoreSavedData();

			AssertEquals(4, reloadedIncomplete.ConsolCosting.ConsolCosts.Count);
			for (int i = 0; i < 4; i++)
			{
				AssertEquals("apportion charge count is 1", 1, reloadedIncomplete.ConsolCosting.ConsolCosts[i].ApportionmentCharges.Count);
				AssertNull("RelatedApportionChargeFromDB of apportion charge is null when original charge is changed", reloadedIncomplete.ConsolCosting.ConsolCosts[i].ApportionmentCharges[0].RelatedApportionChargeFromDB);
			}

			AssertEquals(4, reloadedIncomplete.Lines.Count);
			for (int i = 0; i < 4; i++)
			{
				Assert("IsPopulatedFromImportedJobCharge is false when original charge is changed", !reloadedIncomplete.Lines[0].IsPopulatedFromImportedJobCharge);
			}
		}

		public void TestIsConsolCostImportPopupSuspendedWhenImportingConsolCosts()
		{
			var collection = new InvoicingBaseCollection(Factory);

			using (var stream = Retriever.GetStream("FullyPopulatedIncompleteInvoiceBeforeLocalAmountsAdded.xml"))
			{
				var dataAdapter = GetNewBizObjXmlDataAdapter() as IncompleteTransactionDataAdapter<APInvoice>;
				var serializer = new XmlValueObjectSerializer(dataAdapter.ValueObjectType);
				Assert("IsConsolCostImportPopupSuspended_ForTestOnly default value", !dataAdapter.IsConsolCostImportPopupSuspended_ForTestOnly);
				serializer.ImportXmlData(stream, dataAdapter, collection, null, new NotificationBuffer());
				Assert("ConsolCostImportPopup should have been suspended", dataAdapter.IsConsolCostImportPopupSuspended_ForTestOnly);
				Assert("IsConsolCostImportPopupSuspended_ForTestOnly should have been reset", !dataAdapter.IsConsolCostImportPopupSuspended_ForTestOnly);
			}
		}

		public void TestIsConsolSummaryUpdateSuspended_WhenLoadIncompleteInvoice()
		{
			var consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00000001");
			var consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00000002");
			Factory.Save();

			var collection = new InvoicingBaseCollection(Factory);

			using (var stream = Retriever.GetStream("FullyPopulatedIncompleteInvoiceMultipleConsolCost.xml"))
			{
				var dataAdapter = GetNewBizObjXmlDataAdapter() as IncompleteTransactionDataAdapter<APInvoice>;
				var serializer = new XmlValueObjectSerializer(dataAdapter.ValueObjectType);
				Assert("isConsolSummaryUpdateSuspended_ForTestOnly default value", !dataAdapter.isConsolSummaryUpdateSuspended_ForTestOnly);

				serializer.ImportXmlData(stream, dataAdapter, collection, null, new NotificationBuffer());

				AssertEquals(1, collection.Count);
				var summaryCollection = collection[0].ConsolCosting.ConsolSummary;
				Assert("Consol Summary Update should have been suspended", dataAdapter.isConsolSummaryUpdateSuspended_ForTestOnly);
				Assert("Consol Summary Update not suspended", !summaryCollection.UpdateSuspender.IsSuspended);
				AssertEquals("Consol Summary's count is 2 for 2 Consol", 2, summaryCollection.Count);
				AssertEquals("Consol Summary's update count is 2, one for load, one for Summary Update", 2, summaryCollection.UpdateCount_ForTestOnly);
				foreach (APInvoiceConsolCosting.APInvoiceConsolSummary summary in summaryCollection)
				{
					AssertEquals("AUD", summary.InvoiceCurrency);
					AssertEquals(10M, summary.LocalTotalAmount);
					AssertEquals(0M, summary.LocalTotalTaxAmount);
					AssertEquals(10M, summary.LocalTotalAmountWithTax);
					AssertEquals(10M, summary.InvoiceCurrencyTotalAmount);
					AssertEquals(0M, summary.InvoiceCurrencyTotalTaxAmount);
					AssertEquals(10M, summary.InvoiceCurrencyTotalAmountWithTax);
				}
			}
		}

		public void TestIsConsolSummaryUpdateSuspended_WhenDeleteIncompleteInvoice()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var job = TestObjectCreator.CreateJob(shipment, false);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, 100m, TestObjectCreator.AALSHI);
			Factory.Save();

			AssertEquals("Precondition:", 1, consolCost.ApportionmentCharges.Count);

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV123");
			var originatingCost = TestObjectCreator.CreateConsolCost(invoice, consol, TestObjectCreator.CC1, 100);
			originatingCost.RelatedConsolCostPK = consolCost.PK;

			AssertEquals("Precondition:", 1, originatingCost.ApportionmentCharges.Count);
			originatingCost.ApportionmentCharges[0].RelatedApportionChargeFromDB = consolCost.ApportionmentCharges[0];
			invoice.ImportAllApportionmentsFromCosting();

			invoice.SaveAsIncomplete();

			InvoicingBase reloadedIncomplete = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
			var summaryCollection = reloadedIncomplete.ConsolCosting.ConsolSummary;
			reloadedIncomplete.RestoreSavedData();
			AssertEquals("Precondition: Consol Summary's update count is 2, one for load, one for Summary Update", 2, summaryCollection.UpdateCount_ForTestOnly);

			reloadedIncomplete.SetFinalFlagWhenImportingFromSplitChargeSuspender_IsSuspendedCountForTestOnly = 0;
			reloadedIncomplete.Delete();
			Factory.Save();
			AssertEquals("Consol Summary's update count is still 2, one for load, one for Summary Update", 2, summaryCollection.UpdateCount_ForTestOnly);
			AssertEquals("Count is 3", 3, reloadedIncomplete.SetFinalFlagWhenImportingFromSplitChargeSuspender_IsSuspendedCountForTestOnly);

			var invoice2 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1234");
			Assert("Precondition: IsIncompleteInvoice is false", !invoice2.IsIncompleteInvoice);
			invoice2.SetFinalFlagWhenImportingFromSplitChargeSuspender_IsSuspendedCountForTestOnly = 0;
			invoice2.Delete();
			Factory.Save();
			AssertEquals("Count is 0", 0, invoice2.SetFinalFlagWhenImportingFromSplitChargeSuspender_IsSuspendedCountForTestOnly);
		}

		public void TestAllInvoiceLinesHaveConsolCostExchangeRatesAfterImportingConsolCosts()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 0.5m);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001");
			var shipment = TestObjectCreator.CreateShipment("S00001004", "AUSYD", "NZAKL", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var consolcost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.USD, 1.5m, 150m, TestObjectCreator.Creditor1);
			var consolcost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.USD, 1.5m, 150m, TestObjectCreator.Creditor1);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, organisation: TestObjectCreator.Creditor1);
			var originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);

			var importer = new InvoicingBaseConsolCostImporter(new BusinessObjectFactory(), originatingCost, invoice);
			importer.ImportCostsIntoCosting(new BusinessObject[] { consolcost1, consolcost2 });

			var importedConsolCost = invoice.ConsolCosting.ConsolCosts.Cast<JobConsolCost>().Where(x => x.E6_RX_NKCurrency == TestObjectCreator.USD.Code).ToArray();
			AssertEquals(2, importedConsolCost.Length);
			AssertEquals("Precondition: Consol cost imported", 1.5m, importedConsolCost[0].E6_ExchangeRate);
			AssertEquals("Precondition: Consol cost imported", 1.5m, importedConsolCost[1].E6_ExchangeRate);

			invoice.ImportAllApportionmentsFromCosting();

			AssertEquals("Precondition: Line exchange rate is consol rate", 1.5m, invoice.Lines[0].AL_ExchangeRate);
			AssertEquals("Precondition: Line exchange rate is consol rate", 1.5m, invoice.Lines[1].AL_ExchangeRate);
			invoice.SaveAsIncomplete();

			var incompleteInvoice = new BusinessObjectFactory().Load<APInvoice>(invoice.PK);
			incompleteInvoice.RestoreSavedData();

			AssertEquals("Line exchange rate should be same as what was saved", 1.5m, incompleteInvoice.Lines[0].AL_ExchangeRate);
			AssertEquals("Line exchange rate should be same as what was saved", 1.5m, incompleteInvoice.Lines[1].AL_ExchangeRate);
		}

		#region Compliance Info

		public void TestPopulateComplianceInfoWhenImportConsolCostAndCreateComplianceDocumentRecordOnPosting()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				TestObjectCreator.Creditor1.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = Core.Constants.OrganisationCreateComplianceDocumentOnPostingTypes.PerComplianceDocumentNumber;

				var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001");
				var shipment = TestObjectCreator.CreateShipment("S00001004", "AUSYD", "NZAKL", consol);
				var job = TestObjectCreator.CreateJob(shipment, false);
				var consolcost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.USD, 1.5m, 150m, TestObjectCreator.Creditor1);
				var consolcost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC2, TestObjectCreator.USD, 1.5m, 300m, TestObjectCreator.Creditor1);
				Factory.Save();

				var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, organisation: TestObjectCreator.Creditor1);
				var originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);

				var importer = new InvoicingBaseConsolCostImporter(new BusinessObjectFactory(), originatingCost, invoice);
				importer.ImportCostsIntoCosting(new BusinessObject[] { consolcost1, consolcost2 });

				invoice.ImportAllApportionmentsFromCosting();

				var line1 = invoice.Lines.Cast<InvoicingLineBase>().First(x => x.AL_AC == TestObjectCreator.CC1.PK);
				var line2 = invoice.Lines.Cast<InvoicingLineBase>().First(x => x.AL_AC == TestObjectCreator.CC2.PK);

				AddComplianceInfo(line1, true, "123", "TXC", TestObjectCreator.Creditor1.PK, "12345675", ZDateTime.Today, 202202, "ABC", "123", "DEF");
				line2.CreateComplianceDocumentRecordOnPosting = false;

				invoice.SaveAsIncomplete();

				var incompleteInvoice = new BusinessObjectFactory().Load<APInvoice>(invoice.PK);
				incompleteInvoice.RestoreSavedData();

				line1 = incompleteInvoice.Lines.Cast<InvoicingLineBase>().First(x => x.AL_AC == TestObjectCreator.CC1.PK);
				line2 = incompleteInvoice.Lines.Cast<InvoicingLineBase>().First(x => x.AL_AC == TestObjectCreator.CC2.PK);

				AssertComplianceInfo(line1, true, "123", "TXC", TestObjectCreator.Creditor1.PK, "12345675", ZDateTime.Today, 202202, "ABC", "123", "DEF");
				AssertComplianceInfo(line2, false, ZString.Empty, ZString.Empty, ZGuid.Empty, ZString.Empty, ZDateTime.Empty, ZInt.Zero, ZString.Empty, ZString.Empty, ZString.Empty);
			}
		}

		public void TestNotPopulateComplianceInfoWhenDebtorNotHasPCDSettingForAP()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				TestObjectCreator.Creditor1.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = Core.Constants.OrganisationCreateComplianceDocumentOnPostingTypes.PerComplianceDocumentNumber;

				var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001");
				var shipment = TestObjectCreator.CreateShipment("S00001004", "AUSYD", "NZAKL", consol);
				var job = TestObjectCreator.CreateJob(shipment, false);
				var consolcost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.USD, 1.5m, 150m, TestObjectCreator.Creditor1);
				var consolcost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC2, TestObjectCreator.USD, 1.5m, 300m, TestObjectCreator.Creditor1);
				Factory.Save();

				var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, organisation: TestObjectCreator.Creditor1);
				var originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);

				var importer = new InvoicingBaseConsolCostImporter(new BusinessObjectFactory(), originatingCost, invoice);
				importer.ImportCostsIntoCosting(new BusinessObject[] { consolcost1, consolcost2 });

				invoice.ImportAllApportionmentsFromCosting();

				var line1 = invoice.Lines.Cast<InvoicingLineBase>().First(x => x.AL_AC == TestObjectCreator.CC1.PK);
				var line2 = invoice.Lines.Cast<InvoicingLineBase>().First(x => x.AL_AC == TestObjectCreator.CC2.PK);

				AddComplianceInfo(line1, true, "123", "TXC", TestObjectCreator.Creditor1.PK, "12345675", ZDateTime.Today, 202202, "ABC", "123", "DEF");
				line2.CreateComplianceDocumentRecordOnPosting = false;

				invoice.SaveAsIncomplete();

				TestObjectCreator.Creditor1.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = Core.Constants.OrganisationCreateComplianceDocumentOnPostingTypes.NotApplicable;
				Factory.Save();

				AssertRestoredComplianceInfo(invoice);

				TestObjectCreator.Creditor1.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = Core.Constants.OrganisationCreateComplianceDocumentOnPostingTypes.PerComplianceDocumentNumber;
				Factory.Save();

				using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					AssertRestoredComplianceInfo(invoice);
				}
			}

			void AssertRestoredComplianceInfo(InvoicingBase invoice)
			{
				var incompleteInvoice = new BusinessObjectFactory().Load<APInvoice>(invoice.PK);
				incompleteInvoice.RestoreSavedData();

				var line1 = incompleteInvoice.Lines.Cast<InvoicingLineBase>().First(x => x.AL_AC == TestObjectCreator.CC1.PK);
				var line2 = incompleteInvoice.Lines.Cast<InvoicingLineBase>().First(x => x.AL_AC == TestObjectCreator.CC2.PK);

				AssertComplianceInfo(line1, false, ZString.Empty, ZString.Empty, ZGuid.Empty, ZString.Empty, ZDateTime.Empty, ZInt.Zero, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertComplianceInfo(line2, false, ZString.Empty, ZString.Empty, ZGuid.Empty, ZString.Empty, ZDateTime.Empty, ZInt.Zero, ZString.Empty, ZString.Empty, ZString.Empty);
			}
		}

		public void TestCreateComplianceDocumentRecordOnPostingWhenNotImportFromConsolCost()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				TestObjectCreator.Creditor1.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = Core.Constants.OrganisationCreateComplianceDocumentOnPostingTypes.PerComplianceDocumentNumber;

				var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, organisation: TestObjectCreator.Creditor1);
				var line1 = (APInvoiceLine)invoice.Lines.AddNew();
				line1.AL_AC = TestObjectCreator.CC1.PK;
				var line2 = (APInvoiceLine)invoice.Lines.AddNew();
				line2.AL_AC = TestObjectCreator.CC2.PK;

				AddComplianceInfo(line1, true, "123", "TXC", TestObjectCreator.Creditor1.PK, "12345675", ZDateTime.Today, 202202, "ABC", "123", "DEF");
				line2.CreateComplianceDocumentRecordOnPosting = false;

				invoice.SaveAsIncomplete();

				var incompleteInvoice = new BusinessObjectFactory().Load<APInvoice>(invoice.PK);
				incompleteInvoice.RestoreSavedData();

				line1 = incompleteInvoice.Lines.Cast<APInvoiceLine>().First(x => x.AL_AC == TestObjectCreator.CC1.PK);
				line2 = incompleteInvoice.Lines.Cast<APInvoiceLine>().First(x => x.AL_AC == TestObjectCreator.CC2.PK);

				AssertComplianceInfo(line1, true, "123", "TXC", TestObjectCreator.Creditor1.PK, "12345675", ZDateTime.Today, 202202, "ABC", "123", "DEF");
				AssertComplianceInfo(line2, false, ZString.Empty, ZString.Empty, ZGuid.Empty, ZString.Empty, ZDateTime.Empty, ZInt.Zero, ZString.Empty, ZString.Empty, ZString.Empty);
			}
		}

		void AddComplianceInfo(InvoicingLineBase line,
				ZBool createComplianceDocumentRecordOnPosting,
				ZString complianceDocumentNumber,
				ZString complianceSubType,
				ZGuid complianceDocumentOrganization,
				ZString complianceDocumentVATRegistrationNum,
				ZDateTime complianceDocumentDate,
				ZInt complianceDocumentReportingPeriod,
				ZString complianceDocumentSupportingReason,
				ZString complianceSupportingDocumentNumber,
				ZString complianceSupportingDocumentType)
		{
			line.CreateComplianceDocumentRecordOnPosting = createComplianceDocumentRecordOnPosting;
			line.ComplianceDocumentNumber = complianceDocumentNumber;
			line.ComplianceSubType = complianceSubType;
			line.ComplianceDocumentOrganization = complianceDocumentOrganization;
			line.ComplianceDocumentVATRegistrationNum = complianceDocumentVATRegistrationNum;
			line.ComplianceDocumentDate = complianceDocumentDate;
			line.ComplianceDocumentReportingPeriod = complianceDocumentReportingPeriod;
			line.ComplianceDocumentSupportingReason = complianceDocumentSupportingReason;
			line.ComplianceSupportingDocumentNumber = complianceSupportingDocumentNumber;
			line.ComplianceSupportingDocumentType = complianceSupportingDocumentType;
		}

		void AssertComplianceInfo(InvoicingLineBase line,
			ZBool createComplianceDocumentRecordOnPosting,
			ZString complianceDocumentNumber,
			ZString complianceSubType,
			ZGuid complianceDocumentOrganization,
			ZString complianceDocumentVATRegistrationNum,
			ZDateTime complianceDocumentDate,
			ZInt complianceDocumentReportingPeriod,
			ZString complianceDocumentSupportingReason,
			ZString complianceSupportingDocumentNumber,
			ZString complianceSupportingDocumentType)
		{
			AssertEquals(createComplianceDocumentRecordOnPosting, line.CreateComplianceDocumentRecordOnPosting);
			AssertEquals(complianceDocumentNumber, line.ComplianceDocumentNumber);
			AssertEquals(complianceSubType, line.ComplianceSubType);
			AssertEquals(complianceDocumentOrganization, line.ComplianceDocumentOrganization);
			AssertEquals(complianceDocumentVATRegistrationNum, line.ComplianceDocumentVATRegistrationNum);
			AssertEquals(complianceDocumentDate, line.ComplianceDocumentDate);
			AssertEquals(complianceDocumentReportingPeriod, line.ComplianceDocumentReportingPeriod);
			AssertEquals(complianceDocumentSupportingReason, line.ComplianceDocumentSupportingReason);
			AssertEquals(complianceSupportingDocumentNumber, line.ComplianceSupportingDocumentNumber);
			AssertEquals(complianceSupportingDocumentType, line.ComplianceSupportingDocumentType);
		}

		#endregion

		public void TestBackwardCompatibilityAfterSaveTaxTransaction()
		{
			var collection = new InvoicingBaseCollection(Factory);
			var notificationBuffer = new NotificationBuffer();

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);

			using (var stream = Retriever.GetStream("BackwardCompatibilitySaveTaxTransaction.xml"))
			{
				var dataAdapter = GetNewBizObjXmlDataAdapter();
				var serializer = new XmlValueObjectSerializer(dataAdapter.ValueObjectType);
				serializer.ImportXmlData(stream, dataAdapter, collection, null, notificationBuffer);
			}

			AssertEquals("Post-condition: Errors check on notification buffer", false, notificationBuffer.HasErrors);
		}

		public void TestJobNumberToPKMappingProviderShouldBeInjected()
		{
			var jobNumber = "S00001000";
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_JobNum = jobNumber;
			Factory.Save();

			var xml = File.ReadAllText(Retriever.SaveResourceToFile("FullyPopulatedIncompleteInvoiceBeforeAddExchangeRate.xml"));
			var collection = new InvoicingBaseCollection(Factory);
			using (var stream = StreamConverter.StringToStream(xml))
			{
				var dataAdapter = GetNewBizObjXmlDataAdapter() as IncompleteTransactionDataAdapter<APInvoice>;
				var serializer = new XmlValueObjectSerializer(dataAdapter.ValueObjectType);
				serializer.ImportXmlData(stream, dataAdapter, collection, null, new NotificationBuffer());
			}

			AssertNotNull(Factory.ServiceContainer.GetService<JobNumberToPKMappingProvider>());

			var invoiceHeader = collection.Single() as InvoicingBase;
			var invoiceLine = collection[0].Lines.Single() as InvoicingLineBase;
			var consolCost = invoiceHeader.ConsolCosting.ConsolCosts.Single() as JobConsolCost;
			var jobCharge = Factory.LoadTop1<JobCharge>(new ZQuery(JobChargeSchema.JR_E6, consolCost.PK));
			AssertEquals(job.PK, invoiceLine.AL_JH);
			AssertEquals(job.PK, jobCharge.JR_JH);
		}
	}
}
