using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARTransactionFilterStripBusinessObject))]
	public class ARTransactionFilterTest : TransactionFilterStripBusinessObjectTest
	{
		protected override AdjustmentNote CreateNewAdjustmentNote(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<ARAdjustmentNote>();
		}

		protected override CreditNote CreateNewCreditNote(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<ARCreditNote>();
		}

		protected override Invoice CreateNewInvoice(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<ARInvoice>();
		}

		protected override Invoice CreateNewInvoiceOfOtherLedger(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<APInvoice>();
		}

		protected override Journal CreateNewJournal(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<ARJournal>();
		}

		protected override Payment CreateNewPayment(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<ARPayment>();
		}

		protected override Receipt CreateNewReceipt(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<ARReceipt>();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ARTransactionFilterStripBusinessObject();
		}

		public void TestJobInvoiceNumberFilter_ComparisonOperator_List()
		{
			ModuleTextFilter consolidationNumberFilter = ((ModuleTextFilter)TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.ConsolidationNumber]);
			AssertNotNull("JobInvoiceNumberFilter filter should exist", consolidationNumberFilter);
			AssertEquals("ComparisonOperator on JobNumberFilter should be defaulted to 'starts with'", "starts with", consolidationNumberFilter.ComparisonOperator);
			Assert("ComparisonOperator on JobInvoiceNumberFilter should be read only", consolidationNumberFilter.ComparisonOperatorInfo.ReadOnly);
		}

		public void TestCommonNumberFiltering()
		{
			Invoice testInvoiceTransNum = CreateNewInvoice(Factory);
			testInvoiceTransNum.AH_OH = TestOrg.PK;
			testInvoiceTransNum.AH_TransactionNum = "00002544";
			testInvoiceTransNum.IsManuallySetTransactionNumber_ForTestOnly = true;
			Factory.Save();

			Invoice testInvoiceJobNum = CreateNewInvoice(Factory);
			testInvoiceJobNum.AH_OH = TestOrg.PK;
			testInvoiceJobNum.AH_ConsolidatedInvoiceRef = "S0002544/A";
			testInvoiceJobNum.AH_TransactionNum = "00001000";
			testInvoiceJobNum.IsManuallySetTransactionNumber_ForTestOnly = true;
			Factory.Save();

			ModuleTextFilter allNumbersFilter = ((ModuleTextFilter)TestFilterBizO["Common Numbers and References"]);
			allNumbersFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			allNumbersFilter.Property = "2544";
			allNumbersFilter.IsActive = true;

			AssertEquals(ModuleNumberFilter.MultiplyMaxLength(38), allNumbersFilter.MaxLength);

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Collection should contain both invoices", 2, testTransactions.Count);
			Assert("Collection contains Trans Num invoice", testTransactions.Contains(testInvoiceTransNum.PK));
			Assert("Collection contains Job Num invoice", testTransactions.Contains(testInvoiceJobNum.PK));
		}

		public void TestSupportingDocumentNumberFiltering()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var invoice1 = CreateNewInvoice(Factory);

				var headerReference1 = Factory.New<AccTransactionHeaderReference>();
				headerReference1.AH1_AH = invoice1.PK;
				headerReference1.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.IRD;
				headerReference1.AH1_Reference = "00001000";

				var invoice2 = CreateNewInvoice(Factory);

				var headerReference2 = Factory.New<AccTransactionHeaderReference>();
				headerReference2.AH1_AH = invoice2.PK;
				headerReference2.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.IRD;
				headerReference2.AH1_Reference = "00001001";

				Factory.Save();

				var supportingDocumentNumberFilter = (ModuleTextFilter)TestFilterBizO[AccountingUtils.NumberFilterTypes.SupportingDocumentNumber];
				supportingDocumentNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
				supportingDocumentNumberFilter.Property = "00001000";
				supportingDocumentNumberFilter.IsActive = true;

				AssertEquals(120, supportingDocumentNumberFilter.MaxLength);

				var testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();
				AssertEquals("Collection should contain one invoice", 1, testTransactions.Count);
				Assert("Collection contains invoice1", testTransactions.Contains(invoice1.PK));
				Assert("Collection doesn't contains invoice2", !testTransactions.Contains(invoice2.PK));
			}
		}

		public void TestSupportingDocumentNumberFilteringAdded()
		{
			AssertSupportingDocumentNumberFilteringAdded(Core.Constants.CountryCodes.Australia, true, false);
			AssertSupportingDocumentNumberFilteringAdded(Core.Constants.CountryCodes.Australia, false, false);
			AssertSupportingDocumentNumberFilteringAdded(Core.Constants.CountryCodes.VietNam, false, true);
			AssertSupportingDocumentNumberFilteringAdded(Core.Constants.CountryCodes.VietNam, true, true);

			void AssertSupportingDocumentNumberFilteringAdded(string country, bool eInvoicingEnabled, bool isAdded)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, eInvoicingEnabled))
				{
					fTestFilterBizO = null;
					var supportingDocumentNumberFilter = (ModuleTextFilter)TestFilterBizO[AccountingUtils.NumberFilterTypes.SupportingDocumentNumber];
					if (isAdded)
					{
						AssertNotNull("Supporting Document Number filter should be added.", supportingDocumentNumberFilter);
					}
					else
					{
						AssertNull("Supporting Document Number filter should NOT be added", supportingDocumentNumberFilter);
					}
				}
			}
		}

		public void TestCollectionReferenceNumberFiltering()
		{
			var invoice1 = CreateNewInvoice(Factory);

			var headerReference1 = Factory.New<AccTransactionHeaderReference>();
			headerReference1.AH1_AH = invoice1.PK;
			headerReference1.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ITR;
			headerReference1.AH1_Reference = "00001000";

			var invoice2 = CreateNewInvoice(Factory);

			var headerReference2 = Factory.New<AccTransactionHeaderReference>();
			headerReference2.AH1_AH = invoice2.PK;
			headerReference2.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ITR;
			headerReference2.AH1_Reference = "00001001";

			Factory.Save();

			ModuleTextFilter collectionReferenceNumberFilter = ((ModuleTextFilter)TestFilterBizO["Invoice Transaction Reference"]);
			collectionReferenceNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			collectionReferenceNumberFilter.Property = "00001000";
			collectionReferenceNumberFilter.IsActive = true;

			AssertEquals(120, collectionReferenceNumberFilter.MaxLength);

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Collection should contain one invoice", 1, testTransactions.Count);
			Assert("Collection contains invoice1", testTransactions.Contains(invoice1.PK));
			Assert("Collection doesn't contains invoice2", !testTransactions.Contains(invoice2.PK));
		}

		public void TestInvoicePaymentReferenceCodeFiltering()
		{
			var invoice1 = CreateNewInvoice(Factory);
			var invoice2 = CreateNewInvoice(Factory);
			invoice1.AH_InvoicePaymentReferenceCode = "AAA";
			invoice2.AH_InvoicePaymentReferenceCode = "BBB";

			Factory.Save();

			ModuleTextFilter invoicePaymentReferenceCodeFilter = ((ModuleTextFilter)TestFilterBizO["Invoice Remittance Type"]);
			invoicePaymentReferenceCodeFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			invoicePaymentReferenceCodeFilter.Property = "AAA";
			invoicePaymentReferenceCodeFilter.IsActive = true;

			AssertEquals(3, invoicePaymentReferenceCodeFilter.MaxLength);

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Collection should contain one invoice", 1, testTransactions.Count);
			Assert("Collection contains invoice1", testTransactions.Contains(invoice1.PK));
			Assert("Collection doesn't contains invoice2", !testTransactions.Contains(invoice2.PK));
		}

		public void TestComplianceDocDateFiltering()
		{
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China);
			var invoice1 = CreateNewInvoice(Factory);
			var invoice2 = CreateNewInvoice(Factory);
			var invoice3 = CreateNewInvoice(Factory);
			invoice1.AH_ComplianceDocumentDate = new ZDate("2019-06-01");
			invoice2.AH_ComplianceDocumentDate = new ZDate("2019-06-02");
			invoice3.AH_ComplianceDocumentDate = new ZDate("2019-06-02");
			Factory.Save();

			ModuleDateFilter invoicePaymentReferenceCodeFilter = (ModuleDateFilter)TestFilterBizO["Compliance Doc Date"];

			AssertNotNull(invoicePaymentReferenceCodeFilter);

			invoicePaymentReferenceCodeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			invoicePaymentReferenceCodeFilter.Property1 = new ZDate("2019-06-01");
			invoicePaymentReferenceCodeFilter.Property2 = new ZDate("2019-06-01");
			invoicePaymentReferenceCodeFilter.IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Collection should contain 1 invoice", 1, testTransactions.Count);
			Assert("Collection contains invoice1", testTransactions.Contains(invoice1.PK));
			Assert("Collection doesn't contains invoice2", !testTransactions.Contains(invoice2.PK));
			Assert("Collection doesn't contains invoice3", !testTransactions.Contains(invoice3.PK));

			invoicePaymentReferenceCodeFilter.Property2 = new ZDate("2019-06-03");

			testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Collection should contain 3 invoice", 3, testTransactions.Count);
			Assert("Collection contains invoice1", testTransactions.Contains(invoice1.PK));
			Assert("Collection contains invoice2", testTransactions.Contains(invoice2.PK));
			Assert("Collection contains invoice3", testTransactions.Contains(invoice3.PK));
		}

		public void TestJobInvNumberFilteringUsingContainsAndStartsWith()
		{
			Invoice inv = CreateNewInvoice(Factory);
			inv.AH_ConsolidatedInvoiceRef = "S00001032/B";
			inv.AH_OH = TestOrg.PK;

			CreditNote crd = CreateNewCreditNote(Factory);
			crd.AH_ConsolidatedInvoiceRef = "S00001220/A";
			crd.AH_OH = TestOrg.PK;

			AdjustmentNote adj = CreateNewAdjustmentNote(Factory);
			adj.AH_ConsolidatedInvoiceRef = "S00002204/A";
			adj.AH_OH = TestOrg.PK;

			Factory.Save();

			ModuleTextFilter consolidationNumberFilter = ((ModuleTextFilter)TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.ConsolidationNumber]);
			consolidationNumberFilter.Property = "S00001";
			consolidationNumberFilter.IsActive = true;
			consolidationNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 2 matching transactions", 2, transactions.Count);
			Assert("Inv should match", transactions.Contains(inv));
			Assert("Crd should match", transactions.Contains(crd));

			consolidationNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			consolidationNumberFilter.Property = "22";

			transactions.Load(TestFilterBizO.Filter);

			AssertEquals("There should be 2 matching transacitons", 2, transactions.Count);
			Assert("Crd should match", transactions.Contains(crd));
			Assert("Adj should match", transactions.Contains(adj));
		}

		public override void TestAllNumbersFiltering()
		{
			Job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.JobReadyForCostPosting.Code);
			Invoice invoiceJobNum = CreateNewInvoice(Factory);
			InvoicingLineBase invoicingLine = (InvoicingLineBase)invoiceJobNum.Lines.AddNew();
			invoicingLine.AL_JH = Job.PK;
			invoicingLine.AL_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			TestObjectCreator.CreateJobCharge(invoicingLine, Job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			Factory.Save();
			ZString filterValueForTest = Job.JH_JobNum; //Can not set custom JH_JobNum, Will use this value for other properties as well.

			Journal testJournalChequeNumber = CreateNewJournal(Factory);
			testJournalChequeNumber.AH_ChequeOrReference = filterValueForTest;

			Journal testJournalTransNum = CreateNewJournal(Factory);
			testJournalTransNum.IsManuallySetTransactionNumber_ForTestOnly = true;
			testJournalTransNum.AH_TransactionNum = filterValueForTest;
			Factory.Save();

			Receipt testReceiptDepositBatchNumber = CreateNewReceipt(Factory);
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testReceiptDepositBatchNumber.AH_AB = testBank.PK;
			testReceiptDepositBatchNumber.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			testReceiptDepositBatchNumber.AH_OSExTaxAmount = 90M;
			testReceiptDepositBatchNumber.AH_ExchangeRate = 1M;
			testReceiptDepositBatchNumber.AH_TransactionReference = "abc";
			testReceiptDepositBatchNumber.AH_Desc = "test receipt";
			testReceiptDepositBatchNumber.AH_ChequeOrReference = "00123";
			testReceiptDepositBatchNumber.AH_ChequeDrawer = "bbb";

			Payment testPaymentDDRBatchNumber = CreateNewPayment(Factory);
			testPaymentDDRBatchNumber.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			testPaymentDDRBatchNumber.AH_ReceiptBatchNo = filterValueForTest;

			Invoice testInvoiceJobInvoiceNumber = CreateNewInvoice(Factory);
			testInvoiceJobInvoiceNumber.AH_ConsolidatedInvoiceRef = filterValueForTest;
			Factory.Save();

			ModuleTextFilter allNumbersFilter = ((ModuleTextFilter)TestFilterBizO[MatchingFilterBusinessObject.AllNumbers]);
			allNumbersFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			allNumbersFilter.Property = filterValueForTest;
			allNumbersFilter.IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Collection should contain all transactions, except TestReceiptDepositBatchNumber", 5, testTransactions.Count);
			Assert("Collection contains InvoiceJobNum", testTransactions.Contains(invoiceJobNum.PK));
			Assert("Collection contains TestJournalChequeNumber", testTransactions.Contains(testJournalChequeNumber.PK));
			Assert("Collection contains TestJournalTransNum", testTransactions.Contains(testJournalTransNum.PK));
			Assert("Collection contains TestPaymentDDRBatchNumber", testTransactions.Contains(testPaymentDDRBatchNumber.PK));
			Assert("Collection contains TestInvoiceJobInvoiceNumber", testTransactions.Contains(testInvoiceJobInvoiceNumber.PK));

			allNumbersFilter.Property = filterValueForTest.Replace("S", "");
			testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Collection should contain all transactions", 6, testTransactions.Count);
			Assert("Collection contains TestReceiptDepositBatchNumber", testTransactions.Contains(testReceiptDepositBatchNumber.PK));
			Assert("Collection contains TestJournalChequeNumber", testTransactions.Contains(testJournalChequeNumber.PK));
			Assert("Collection contains TestJournalTransNum", testTransactions.Contains(testJournalTransNum.PK));
			Assert("Collection contains TestPaymentDDRBatchNumber", testTransactions.Contains(testPaymentDDRBatchNumber.PK));
			Assert("Collection contains TestInvoiceJobInvoiceNumber", testTransactions.Contains(testInvoiceJobInvoiceNumber.PK));
		}

		public override void TestNumberFiltersList()
		{
			base.TestNumberFiltersList();
			ModuleFilter numberFilter = TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.ConsolidationNumber];
			AssertNotNull("ConsolidationNumber filter should exist", numberFilter);

			numberFilter = TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.TransactionNumber];
			AssertNotNull("TransactionNumber filter should exist", numberFilter);
		}

		public void TestGetModuleFilterThatOverridesAllOtherFilters()
		{
			ModuleFilter exclusiveModuleFilter = ((ARTransactionFilterStripBusinessObject)TestFilterBizO).GetModuleFilterThatOverridesAllOtherFilters_ForTestOnly();
			AssertNotNull("There should be an Exclusive filter set", exclusiveModuleFilter);
			AssertEquals("The exclusive filter should be TransactionNumber filter", Business.AccountingUtils.NumberFilterTypes.TransactionNumber, exclusiveModuleFilter.Description);
			AssertEquals("TransactionNumber filter for AR should be of ModuleFountainFilter type", typeof(ModuleFountainFilter), exclusiveModuleFilter.GetType());
		}

		public void TestNumberFilterIgnoresOtherFields()
		{
			Journal testJournal = CreateNewJournal(Factory);
			testJournal.IsManuallySetTransactionNumber_ForTestOnly = true;
			testJournal.AH_TransactionNum = "00001001";
			Factory.Save();

			ModuleTextFilter transactionNumberFilter = ((ModuleTextFilter)TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.TransactionNumber]);
			transactionNumberFilter.Property = "00001001";
			transactionNumberFilter.IsActive = true;

			((ModuleTextFilter)TestFilterBizO["Transaction Type"]).Property = "CTR";
			((ModuleTextFilter)TestFilterBizO["Transaction Type"]).IsActive = true;

			((ModuleGuidFilter)TestFilterBizO[TestFilterBizO.CreditorDebtorText]).Property = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			((ModuleGuidFilter)TestFilterBizO[TestFilterBizO.CreditorDebtorText]).IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("The TestJournal should be contained in the list", 1, testTransactions.Count);
			Assert("The TestJournal should be contained in the list", testTransactions.Contains(testJournal.PK));
		}

		public void TestTransactionNumberFilterUsesRegistrySettingForNumberLength()
		{
			var arInvoiceNumberLengthConfiguration = AccountingConfigurationRegistry.Instance.ARInvoiceNumberLengthConfiguration;
			var transactionsNumberSequenceCustomisation = AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation;
			var arInvoiceNumberLengthConfigurationOriginalValue = arInvoiceNumberLengthConfiguration.Value;
			var transactionsNumberSequenceCustomisationOriginalValue = transactionsNumberSequenceCustomisation.Value;
			try
			{
				Journal testJournal = CreateNewJournal(Factory);
				testJournal.AH_TransactionNum = "001001";
				testJournal.IsManuallySetTransactionNumber_ForTestOnly = true;
				Factory.Save();

				var collection = transactionsNumberSequenceCustomisation.Value;
				var sequenceNumber = collection[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber];

				arInvoiceNumberLengthConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 8);

				transactionsNumberSequenceCustomisation.Inner.SetCurrentValueToUse(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ValueToUse.SavedValue); //Check the 'Override Default'
				sequenceNumber.Include = true;
				sequenceNumber.Length = 6;
				transactionsNumberSequenceCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

				var testFilterBiz = (TransactionFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				ModuleTextFilter transactionNumberFilter = ((ModuleTextFilter)testFilterBiz[Business.AccountingUtils.NumberFilterTypes.TransactionNumber]);
				transactionNumberFilter.Property = "1001";
				AssertEquals("Transaction Number should be adjusted", "001001", transactionNumberFilter.Property);
				transactionNumberFilter.IsActive = true;

				TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, testFilterBiz.Filter);
				testTransactions.Load();
				AssertEquals("The TestJournal should be contained in the list", 1, testTransactions.Count);
				Assert("The TestJournal should be contained in the list", testTransactions.Contains(testJournal.PK));

				Journal testJournal2 = CreateNewJournal(Factory);
				testJournal2.AH_TransactionNum = "00001002";
				testJournal2.IsManuallySetTransactionNumber_ForTestOnly = true;
				Factory.Save();

				transactionsNumberSequenceCustomisation.Inner.SetCurrentValueToUse(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ValueToUse.SavedValue); //Check the 'Override Default'
				sequenceNumber.Include = false;
				sequenceNumber.Length = 6;
				transactionsNumberSequenceCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

				testFilterBiz = (TransactionFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				transactionNumberFilter = ((ModuleTextFilter)testFilterBiz[Business.AccountingUtils.NumberFilterTypes.TransactionNumber]);
				transactionNumberFilter.Property = "1002";
				AssertEquals("Transaction Number should be adjusted", "00001002", transactionNumberFilter.Property);
				transactionNumberFilter.IsActive = true;

				testTransactions = new TransactionHeaderCollection(Factory, testFilterBiz.Filter);
				testTransactions.Load();
				AssertEquals("The TestJournal should be contained in the list", 1, testTransactions.Count);
				Assert("The TestJournal should be contained in the list", testTransactions.Contains(testJournal2.PK));

				transactionsNumberSequenceCustomisation.Inner.SetCurrentValueToUse(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ValueToUse.DefaultValue); //Uncheck the 'Override Default'
				arInvoiceNumberLengthConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 6);

				testFilterBiz = (TransactionFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				transactionNumberFilter = ((ModuleTextFilter)testFilterBiz[Business.AccountingUtils.NumberFilterTypes.TransactionNumber]);
				transactionNumberFilter.Property = "1002";
				AssertEquals("Transaction Number should be adjusted", "001002", transactionNumberFilter.Property);
			}
			finally
			{
				arInvoiceNumberLengthConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arInvoiceNumberLengthConfigurationOriginalValue);
				transactionsNumberSequenceCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, transactionsNumberSequenceCustomisationOriginalValue);
			}
		}

		public void TestViewingNonLoginBranchTransactionsReturnCorrectValues()
		{
			AssertEquals("Should return the receiveables viewing non login branch transactions securiry check point", Env.Security.ReceivablesViewingNonLoginBranchTransactions, ((ARTransactionFilterStripBusinessObject)GetNewFilterStripBusinessObject()).ViewingNonLoginBranchTransactions_ForTestOnly);
		}

		public void TestGetOrganizationAndAddressFilter_DebtorAddress()
		{
			var scenario = SetupGetOrganizationAndAddressFilterCommonScenario().AR;

			var filter = new ARTransactionFilterStripBusinessObject();
			var query = filter.GetAROrganizationAndAddressFilter_ForTestOnly(scenario.org.PK, scenario.defaultAddress.PK);
			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			var transactions = Factory.Load<AccTransactionHeader>(query);
			AssertEquals("Should be 3 transactions", 3, transactions.Length);
			AssertNotNull("Invoice that has been specifically overridden to default address", Array.Find(transactions, t => t.PK == scenario.invoiceDefaultAddr.PK));
			AssertNotNull("Invoice with no address that will default to default address due to job", Array.Find(transactions, t => t.PK == scenario.invoiceDefaultViaJobAddr.PK));
			AssertNotNull("Invoice with no address that will default to default address due to organisation", Array.Find(transactions, t => t.PK == scenario.invoiceNoAddr.PK));

			query = filter.GetAROrganizationAndAddressFilter_ForTestOnly(scenario.org.PK, scenario.otherAddress.PK);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			transactions = Factory.Load<AccTransactionHeader>(query);
			AssertEquals("Should be 2 transactions", 2, transactions.Length);
			AssertNotNull("Invoice that has been specifically overridden to other address", Array.Find(transactions, t => t.PK == scenario.invoiceOtherAddr.PK));
			AssertNotNull("Invoice with no address that will default to other address due to organisation", Array.Find(transactions, t => t.PK == scenario.invoiceDefaultToOtherViaJobAddr.PK));
		}

		public void TestAmendStatusCodeFilterAdded()
		{
			AssertAmendStatusCodeFilterAdded(Core.Constants.CountryCodes.Australia, true, false);
			AssertAmendStatusCodeFilterAdded(Core.Constants.CountryCodes.KoreaSouth, false, false);
			AssertAmendStatusCodeFilterAdded(Core.Constants.CountryCodes.KoreaSouth, true, true);

			void AssertAmendStatusCodeFilterAdded(string country, bool eInvoicingEnabled, bool isAdded)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, eInvoicingEnabled))
				{
					fTestFilterBizO = null;
					var amendStatusCodeFilter = (ModuleTextFilter)TestFilterBizO["Amend Status Code"];
					if (isAdded)
					{
						AssertNotNull("Amend Status Code filter should be added.", amendStatusCodeFilter);
					}
					else
					{
						AssertNull("Amend Status Code filter should NOT be added", amendStatusCodeFilter);
					}
				}
			}
		}

		public void TestAmendStatusCodeFiltering()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.KoreaSouth))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var invoice1 = CreateNewInvoice(Factory);

				var headerReference1 = Factory.New<AccTransactionHeaderReference>();
				headerReference1.AH1_AH = invoice1.PK;
				headerReference1.AH1_Type = "KRE";
				headerReference1.AH1_Reference = "01";

				var invoice2 = CreateNewInvoice(Factory);

				var headerReference2 = Factory.New<AccTransactionHeaderReference>();
				headerReference2.AH1_AH = invoice2.PK;
				headerReference2.AH1_Type = "KRE";
				headerReference2.AH1_Reference = "02";

				Factory.Save();

				var amendStatusCodeFilter = (ModuleTextFilter)TestFilterBizO["Amend Status Code"];
				amendStatusCodeFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
				amendStatusCodeFilter.Property = "01";
				amendStatusCodeFilter.IsActive = true;

				var testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();
				AssertEquals("Collection should contain one invoice", 1, testTransactions.Count);
				Assert("Collection contains invoice1", testTransactions.Contains(invoice1.PK));
				Assert("Collection doesn't contains invoice2", !testTransactions.Contains(invoice2.PK));
			}
		}

		public void TestKoreaReceiptIDFilter()
		{
			AssertKoreaEReportingGovt();
			AssertNonKoreaEReportingGovt();
		}

		void AssertNonKoreaEReportingGovt()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var invoice1 = CreateNewInvoice(Factory);
				var headerReference1 = Factory.New<AccTransactionHeaderReference>();
				headerReference1.AH1_AH = invoice1.PK;
				headerReference1.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.RED;
				headerReference1.AH1_Reference = "NTS-20230103171540-46015";

				var invoicePivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice1, status: EInvoicingPivotState.Pending);
				invoicePivot1.AIP_Status = EInvoicingPivotState.Sent;
				var invoiceBatch1 = TestObjectCreator.CreateEInvoicingBatchForPivot(invoicePivot1, 102, invoicePivot1.AIP_Status);
				invoiceBatch1.AIB_GovernmentAllocatedNumber = "Test Number 1";

				fTestFilterBizO = null;
				var brazilReceiptIDFilter = (ModuleTextFilter)TestFilterBizO["E-Reporting Govt #"];
				brazilReceiptIDFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				brazilReceiptIDFilter.Property = "Test Number 1";
				brazilReceiptIDFilter.IsActive = true;
				Factory.Save();

				var testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();
				AssertEquals("Collection should contain one invoice", 1, testTransactions.Count);
				Assert("The E-reporting Govt # is same as the GovernmentAllocatedNumber!", testTransactions[0].EInvoicingGovernmentAllocatedNumber.Equals(new ZString("Test Number 1")));
			}
		}

		void AssertKoreaEReportingGovt()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.KoreaSouth))
			{
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					var invoice1 = CreateNewInvoice(Factory);
					var headerReference1 = Factory.New<AccTransactionHeaderReference>();
					headerReference1.AH1_AH = invoice1.PK;
					headerReference1.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.RED;
					headerReference1.AH1_Reference = "NTS-20230103171540-46015";

					var invoice2 = CreateNewInvoice(Factory);
					var headerReference2 = Factory.New<AccTransactionHeaderReference>();
					headerReference2.AH1_AH = invoice2.PK;
					headerReference2.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.RED;
					headerReference2.AH1_Reference = "NTS-20230103171540-46016";

					var koreaReceiptIDFilter = (ModuleTextFilter)TestFilterBizO["E-Reporting Govt #"];
					koreaReceiptIDFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
					koreaReceiptIDFilter.Property = "NTS-20230103171540-46016";
					koreaReceiptIDFilter.IsActive = true;

					Factory.Save();

					var testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
					testTransactions.Load();
					AssertEquals("Collection should contain one invoice", 1, testTransactions.Count);
					Assert("Collection contains invoice2", testTransactions.Contains(invoice2.PK));
					Assert("The E-reporting Govt # is same as the receiptID!", testTransactions[0].EInvoicingGovernmentAllocatedNumber.Equals(new ZString("NTS-20230103171540-46016")));
				}
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					fTestFilterBizO = null;
					var koreaReceiptIdFilterOfNullFilter = (ModuleTextFilter)TestFilterBizO["E-Reporting Govt #"];
					AssertNull("The E-Reporting Govt # filter is not null!", koreaReceiptIdFilterOfNullFilter);
				}
			}
		}

		public void TestComplianceDocumentStatusFilterAdded()
		{
			AssertComplianceDocumentStatusFilterAdded(Core.Constants.CountryCodes.Australia, true, false);
			AssertComplianceDocumentStatusFilterAdded(Core.Constants.CountryCodes.China, false, false);
			AssertComplianceDocumentStatusFilterAdded(Core.Constants.CountryCodes.China, true, true);

			void AssertComplianceDocumentStatusFilterAdded(string country, bool eInvoicingEnabled, bool isAdded)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, eInvoicingEnabled))
				{
					fTestFilterBizO = null;
					var complianceDocumentStatusFilter = (ModuleTextFilter)TestFilterBizO["Compliance Document Status"];
					if (isAdded)
					{
						AssertNotNull("Compliance Document Status filter should be added.", complianceDocumentStatusFilter);
					}
					else
					{
						AssertNull("Compliance Document Status filter should NOT be added", complianceDocumentStatusFilter);
					}
				}
			}
		}

		public void TestComplianceDocumentStatusFiltering()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var invoice1 = CreateNewInvoice(Factory);

				var headerReference1 = Factory.New<AccTransactionHeaderReference>();
				headerReference1.AH1_AH = invoice1.PK;
				headerReference1.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.CDS;
				headerReference1.AH1_Reference = "CDD";

				var invoice2 = CreateNewInvoice(Factory);

				var headerReference2 = Factory.New<AccTransactionHeaderReference>();
				headerReference2.AH1_AH = invoice2.PK;
				headerReference2.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.CDS;
				headerReference2.AH1_Reference = "CDN";

				Factory.Save();

				var complianceDocumentStatusFilter = (ModuleTextFilter)TestFilterBizO["Compliance Document Status"];
				complianceDocumentStatusFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
				complianceDocumentStatusFilter.Property = "CDD";
				complianceDocumentStatusFilter.IsActive = true;

				var testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();
				AssertEquals("Collection should contain one invoice", 1, testTransactions.Count);
				Assert("Collection contains invoice1", testTransactions.Contains(invoice1.PK));
				Assert("Collection doesn't contains invoice2", !testTransactions.Contains(invoice2.PK));
			}
		}

		#region Related Disbursement Transactions Filter

		public void TestRelatedDisbursementTransactionsFilteringAdded()
		{
			AssertRelatedDisbursementTransactionsFilteringAdded(CountryCodes.KoreaSouth, true, true);
			AssertRelatedDisbursementTransactionsFilteringAdded(CountryCodes.KoreaSouth, false, false);

			AssertRelatedDisbursementTransactionsFilteringAdded(CountryCodes.VietNam, true, false);
			AssertRelatedDisbursementTransactionsFilteringAdded(CountryCodes.VietNam, false, false);

			void AssertRelatedDisbursementTransactionsFilteringAdded(string country, bool eInvoicingEnabled, bool isAdded)
			{
				using (TestObjectCreator.SetUpForTestingEInvoicingAndAutomaticllyAppendingDisbursementFeesSummary(country, eInvoicingEnabled))
				{
					fTestFilterBizO = null;
					var supportingDocumentNumberFilter = (ModuleTextFilter)TestFilterBizO[AccountingUtils.NumberFilterTypes.RelatedDisbursementTransactions];
					if (isAdded)
					{
						AssertNotNull("Related Disbursement Transactions filter should be added.", supportingDocumentNumberFilter);
					}
					else
					{
						AssertNull("Related Disbursement Transactions filter should NOT be added", supportingDocumentNumberFilter);
					}
				}
			}
		}

		public void TestRelatedDisbursementTransactionsFiltering()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingAndAutomaticllyAppendingDisbursementFeesSummary(CountryCodes.KoreaSouth, true))
			{
				var invoice1 = CreateNewInvoice(Factory);

				var headerReference1 = Factory.New<AccTransactionHeaderReference>();
				headerReference1.AH1_AH = invoice1.PK;
				headerReference1.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ReceivableDisbursementInvoice;
				headerReference1.AH1_Reference = "CDD";

				var invoice2 = CreateNewInvoice(Factory);

				var headerReference2 = Factory.New<AccTransactionHeaderReference>();
				headerReference2.AH1_AH = invoice2.PK;
				headerReference2.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.CDS;
				headerReference2.AH1_Reference = "CDN";

				Factory.Save();

				var complianceDocumentStatusFilter = (ModuleTextFilter)TestFilterBizO["Related Disbursement Transactions"];
				complianceDocumentStatusFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
				complianceDocumentStatusFilter.Property = "CDD";
				complianceDocumentStatusFilter.IsActive = true;

				var testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();
				AssertEquals("Collection should contain one invoice", 1, testTransactions.Count);
				Assert("Collection contains invoice1", testTransactions.Contains(invoice1.PK));
				Assert("Collection doesn't contains invoice2", !testTransactions.Contains(invoice2.PK));
			}
		}

		#endregion

		protected override ModuleFilter SetupFilterForTest(ModuleFilter moduleFilter)
		{
			return
				moduleFilter.Description != "Common Numbers and References" ? base.SetupFilterForTest(moduleFilter) : null;
		}
	}
}
