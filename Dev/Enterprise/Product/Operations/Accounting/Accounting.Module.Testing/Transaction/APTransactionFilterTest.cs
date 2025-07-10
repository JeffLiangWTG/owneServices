using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.Module.AccountingFilterStripBusinessObject;
using static Enterprise.Accounting.Utility.Testing.TaxFrameworkTestObjectCreator;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APTransactionFilterStripBusinessObject))]
	public class APTransactionFilterTest : TransactionFilterStripBusinessObjectTest
	{
		protected override AdjustmentNote CreateNewAdjustmentNote(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<APAdjustmentNote>();
		}

		protected override CreditNote CreateNewCreditNote(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<APCreditNote>();
		}

		protected override Invoice CreateNewInvoice(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<APInvoice>();
		}

		protected override Invoice CreateNewInvoiceOfOtherLedger(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<ARInvoice>();
		}

		protected override Journal CreateNewJournal(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<APJournal>();
		}

		protected override Payment CreateNewPayment(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<APPayment>();
		}

		protected override Receipt CreateNewReceipt(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<APReceipt>();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new APTransactionFilterStripBusinessObject();
		}

		public void TestGovernmentAllocatedIDFilter()
		{
			var vat3 = TestObjectCreator.CreateTaxRate("VAT3", "", 3);
			vat3.AT_PostingGroupId = 0;

			var ac1 = TestObjectCreator.CreateChargeCode("AC1");
			ac1.AC_AT_GSTRate = vat3.PK;
			ac1.AC_Desc = "desc";

			var testInv = new Invoice[4];
			for (int i = 0; i < testInv.Length; i++)
			{
				testInv[i] = CreateNewInvoice(Factory);
				testInv[i].AH_GovernmentAllocatedID = $"0000{i + 1}";

				var line = (InvoiceLine)testInv[i].Lines.AddNew();
				line.AL_JH = TestObjectCreator.Job1.PK;
				line.AL_AC = ac1.PK;
				line.AL_AT = vat3.PK;
				TestObjectCreator.CreateJobCharge(line, TestObjectCreator.Job1, ac1);
			}

			Factory.Save();

			AccountingMasterFilesRegistry.Instance.EnableGovernmentAllocatedNumberBehavior.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			fTestFilterBizO = null;
			var filter = ((ModuleNumberFilter)TestFilterBizO["Government Allocated Number"]);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "00001";
			filter.IsActive = true;

			var testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("There should be 1 Invoice in the collection", 1, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testInv[0]));
			Assert("Should not contain 2nd invoice", !testTransactions.Contains(testInv[1]));
			Assert("Should not contain 3rd invoice", !testTransactions.Contains(testInv[2]));
			Assert("Should not contain 4th invoice", !testTransactions.Contains(testInv[3]));

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();

			AssertEquals("There should be 4 Invoices in the collection", 4, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testInv[0]));
			Assert("Should contain 2nd invoice", testTransactions.Contains(testInv[1]));
			Assert("Should contain 3rd invoice", testTransactions.Contains(testInv[2]));
			Assert("Should contain 4th invoice", testTransactions.Contains(testInv[3]));
		}

		public override void TestNumberFiltersList()
		{
			base.TestNumberFiltersList();
			ModuleFilter numberFilter = TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.ConsolidationNumber];
			AssertNull("ConsolidationNumber filter should not exist", numberFilter);

			numberFilter = TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.TransactionNumber];
			AssertNotNull("TransactionNumber filter should exist", numberFilter);
			Assert("TransactionNumber filter not is common", !numberFilter.IsCommon);
		}

		public void TestSelfBillingInvoiceFilter()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			creator.AALSHI.CompanyData.OB_APCostsSelfBilled = true;

			APInvoice selfBillingInvoice = Factory.New<APInvoice>();
			selfBillingInvoice.AH_OH = creator.ABIGAS.PK;
			selfBillingInvoice.IsSelfBillingInvoice = true;

			APInvoice standardInvoice = Factory.New<APInvoice>();
			standardInvoice.AH_OH = creator.AALSHI.PK;
			standardInvoice.IsSelfBillingInvoice = false;
			standardInvoice.AH_TransactionNum = "XYA";

			Factory.Save();

			ModuleFlagsFilter selfBillingInvoiceFilter = ((ModuleFlagsFilter)TestFilterBizO[JobInvoicingEDocsProviderSupporter.SelfBillingInvoice]);
			selfBillingInvoiceFilter.Property0 = true;
			selfBillingInvoiceFilter.IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Should only be one item in the list", 1, testTransactions.Count);

			selfBillingInvoiceFilter.Property0 = false;
			testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Both items should now be in the list", 2, testTransactions.Count);
		}

		public void TestGetModuleFilterThatOverridesAllOtherFilters()
		{
			ModuleFilter exclusiveModuleFilter = ((APTransactionFilterStripBusinessObject)TestFilterBizO).GetModuleFilterThatOverridesAllOtherFilters_ForTestOnly();
			AssertNull("There should not be an Exclusive filter set", exclusiveModuleFilter);
		}

		public void TestViewingNonLoginBranchTransactionsReturnCorrectValues()
		{
			AssertEquals("Should return the payables viewing non login branch transactions securiry check point", Env.Security.PayablesViewingNonLoginBranchTransactions, ((APTransactionFilterStripBusinessObject)GetNewFilterStripBusinessObject()).ViewingNonLoginBranchTransactions_ForTestOnly);
		}

		public void TestTransactionNumberFilter()
		{
			Journal testJournal1 = CreateNewJournal(Factory);
			Journal testJournal2 = CreateNewJournal(Factory);
			Journal testJournal3 = CreateNewJournal(Factory);

			Factory.Save();

			testJournal1.AH_TransactionNum = "00001001";
			testJournal2.AH_TransactionNum = "00001002";
			testJournal3.AH_TransactionNum = "00001003";

			ModuleTextFilter transactionNumberFilter = ((ModuleTextFilter)TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.TransactionNumber]);
			transactionNumberFilter.Property = "00001002";
			transactionNumberFilter.IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("One transaction should be contained in the list", 1, testTransactions.Count);
			Assert("The TestJournal #00001002 should be contained in the list", testTransactions.Contains(testJournal2.PK));

			((ModuleTextFilter)TestFilterBizO["Transaction Type"]).Property = "INV";
			((ModuleTextFilter)TestFilterBizO["Transaction Type"]).IsActive = true;
			testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("No transaction should be contained in the list because none is of type INV", 0, testTransactions.Count);
		}

		public void TestHasRelatedClaimFilter()
		{
			var invoice1 = CreateNewInvoice(Factory);
			var invoice2 = CreateNewInvoice(Factory);
			var invoice3 = CreateNewInvoice(Factory);
			var invoice4 = CreateNewInvoice(Factory);

			Factory.Save();

			var hasRelatedClimFilter = ((ModuleTextFilter)TestFilterBizO["Has Related Claim"]);
			hasRelatedClimFilter.Property = "No AP Claim";
			hasRelatedClimFilter.IsActive = true;
			var testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("All four transactions should be contained in the list", 4, testTransactions.Count);
			AssertContainsExactElementsInAnyOrder("All four transactions should be contained in the list", new[] { invoice1, invoice2, invoice3, invoice4 }, testTransactions);

			hasRelatedClimFilter.Property = "Has AP Claim";
			testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("No AP Claim for any of this invoice", 0, testTransactions.Count);

			var claim1 = Factory.NewWithValidTestData<APAccQueryClaim>();
			claim1.AY_AH = invoice1.PK;
			var claim2 = Factory.NewWithValidTestData<APAccQueryClaim>();
			claim2.AY_AH = invoice2.PK;

			Factory.Save();

			hasRelatedClimFilter.Property = "Has AP Claim";
			testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Should return invoice1 and invoice2", 2, testTransactions.Count);
			AssertContainsExactElementsInAnyOrder("Should return invoice1 and invoice2", new[] { invoice1, invoice2 }, testTransactions);

			hasRelatedClimFilter.Property = "No AP Claim";
			testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Should return invoice3 and invoice4", 2, testTransactions.Count);
			AssertContainsExactElementsInAnyOrder("Should return invoice3 and invoice4", new[] { invoice3, invoice4 }, testTransactions);
		}

		public void TestInternalReferenceNumberFilter()
		{
			Journal testJournal1 = CreateNewJournal(Factory);
			testJournal1.AH_ConsolidatedInvoiceRef = "1501";
			Journal testJournal2 = CreateNewJournal(Factory);
			testJournal2.AH_ConsolidatedInvoiceRef = "1502";
			Journal testJournal3 = CreateNewJournal(Factory);
			testJournal3.AH_ConsolidatedInvoiceRef = "1503";
			Factory.Save();

			ModuleTextFilter transactionNumberFilter = ((ModuleTextFilter)TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.InternalReferenceNumber]);
			transactionNumberFilter.Property = "1502";
			transactionNumberFilter.IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("One transaction should be contained in the list", 1, testTransactions.Count);
			Assert("The TestJournal #1502 should be contained in the list", testTransactions.Contains(testJournal2.PK));

			((ModuleTextFilter)TestFilterBizO["Transaction Type"]).Property = "INV";
			((ModuleTextFilter)TestFilterBizO["Transaction Type"]).IsActive = true;
			testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("No transaction should be contained in the list because none is of type INV", 0, testTransactions.Count);
		}

		public void TestDefaultPaymentMethodFilter()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			AccAPAccountDetails details = org1.CompanyData.AccountDetailsCollection.AddNew();
			details.A1_IsDefaultAccount = true;
			details.A1_PaymentMethod = "DDR";
			details = org1.CompanyData.AccountDetailsCollection.AddNew();
			details.A1_IsDefaultAccount = false;
			details.A1_PaymentMethod = "CHQ";

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			details = org2.CompanyData.AccountDetailsCollection.AddNew();
			details.A1_IsDefaultAccount = false;
			details.A1_PaymentMethod = "DDR";

			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();

			OrgHeader org4 = Factory.NewWithValidTestData<OrgHeader>();
			details = org4.CompanyData.AccountDetailsCollection.AddNew();
			details.A1_IsDefaultAccount = true;
			details.A1_PaymentMethod = "DEF";

			Invoice invoice1 = CreateNewInvoice(Factory);
			invoice1.AH_OH = org1.PK;
			Invoice invoice2 = CreateNewInvoice(Factory);
			invoice2.AH_OH = org2.PK;
			Invoice invoice3 = CreateNewInvoice(Factory);
			invoice3.AH_OH = org3.PK;
			Invoice invoice4 = CreateNewInvoice(Factory);
			invoice4.AH_OH = org4.PK;

			OrganisationsDataRegistry.Instance.APPaymentMethod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "END");

			Factory.Save();

			ModuleTextFilter transactionNumberFilter = ((ModuleTextFilter)TestFilterBizO["Default Payment Method"]);
			transactionNumberFilter.Property = "DDR";
			transactionNumberFilter.IsActive = false;
			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("All four transactions should be contained in the list", 4, testTransactions.Count);

			transactionNumberFilter.IsActive = true;
			testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Filtering for DDR; default is END - count", 1, testTransactions.Count);
			AssertCollectionContains("Filtering for DDR; default is END", invoice1, testTransactions);

			OrganisationsDataRegistry.Instance.APPaymentMethod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "DDR");
			testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Filtering for DDR; default is DDR - count", 2, testTransactions.Count);
			AssertCollectionContains("Filtering for DDR; default is DDR - should contain invoice1", invoice1, testTransactions);
			AssertCollectionContains("Filtering for DDR; default is DDR - should also contain invoice4", invoice4, testTransactions);

			transactionNumberFilter.Property = "END";
			testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Filtering for END; default is DDR - count", 0, testTransactions.Count);
		}

		public void TestPaymentCriticalityFilter()
		{
			Journal journal1 = CreateNewJournal(Factory);
			journal1.AH_RequisitionStatus = "ABC";
			Journal journal2 = CreateNewJournal(Factory);
			journal2.AH_RequisitionStatus = "XYZ";

			Factory.Save();

			ModuleTextFilter filter = ((ModuleTextFilter)TestFilterBizO[AccountingUtils.DateFilterTypes.RequisitionStatus]);
			filter.IsActive = true;
			filter.Property = string.Empty;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			transactions.Load();
			AssertEquals("Collection should contain both journals", 2, transactions.Count);

			filter.Property = "ABC";
			transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			transactions.Load();
			AssertEquals("Collection should contain only journal1", 1, transactions.Count);
			Assert("Collection should contain only journal1", transactions.Contains(journal1.PK));
		}

		public void TestRequisitionDateFilter()
		{
			Journal journal1 = CreateNewJournal(Factory);
			journal1.AH_RequisitionDate = ZDateTime.Now;
			Journal journal2 = CreateNewJournal(Factory);
			journal2.AH_RequisitionDate = ZDateTime.Now.AddDays(-4);

			Factory.Save();

			ModuleDateFilter postDateFilter = ((ModuleDateFilter)TestFilterBizO[AccountingUtils.DateFilterTypes.RequisitionDate]);
			postDateFilter.Property1 = ZDateTime.Now.AddDays(-2);
			postDateFilter.Property2 = ZDateTime.Now.AddDays(2);
			postDateFilter.IsActive = true;
			postDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			transactions.Load();
			AssertEquals("Collection should contain only journal1", 1, transactions.Count);
			Assert("Collection should contain only journal1", transactions.Contains(journal1.PK));
		}

		public void TestDocumentReceivedDateFilter()
		{
			Invoice invoice = CreateNewInvoice(Factory);
			invoice.AH_DocumentReceivedDate = ZDateTime.Now;
			Invoice invoice1 = CreateNewInvoice(Factory);
			invoice1.AH_DocumentReceivedDate = ZDateTime.Now.AddDays(-4);

			Factory.Save();

			var documentReceivedDateFilter = ((ModuleDateFilter)TestFilterBizO[AccountingUtils.DateFilterTypes.DocumentReceivedDate]);
			AssertNotNull("Document Received Date Filter", documentReceivedDateFilter);
			documentReceivedDateFilter.Property1 = ZDateTime.Now.AddDays(-2);
			documentReceivedDateFilter.Property2 = ZDateTime.Now.AddDays(2);
			documentReceivedDateFilter.IsActive = true;
			documentReceivedDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			transactions.Load();
			AssertEquals("Should be one item in the list", 1, transactions.Count);
			Assert("Collection should contain only invoice", transactions.Contains(invoice.PK));
		}

		public void TestRelatedTransactionDebtorCreditOnHoldStatusFilter()
		{
			// Primary transactions
			Invoice inv1_RelatedTransactionClientCreditNotOnHold = CreateNewInvoice(Factory);
			InvoicingLineBase line1 = TestObjectCreator.CreateInvoiceLine(inv1_RelatedTransactionClientCreditNotOnHold, TestObjectCreator.AUD, inv1_RelatedTransactionClientCreditNotOnHold.AH_ExchangeRate, 10m);
			line1.AL_AC = TestObjectCreator.CC1.PK;
			line1.AL_JH = TestObjectCreator.Job1.PK;
			line1.AL_GB = GlbBranch.CurrentBranch.PK;
			line1.AL_GE = GlbDepartment.CurrentDepartment.PK;
			TestObjectCreator.CreateJobCharge(line1, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD);

			Invoice inv2_RelatedTransactionClientCreditOnHold = CreateNewInvoice(Factory);
			InvoicingLineBase line2 = TestObjectCreator.CreateInvoiceLine(inv2_RelatedTransactionClientCreditOnHold, TestObjectCreator.AUD, inv2_RelatedTransactionClientCreditOnHold.AH_ExchangeRate, 10m);
			line2.AL_AC = TestObjectCreator.CC2.PK;
			line2.AL_JH = TestObjectCreator.Job1.PK;
			line2.AL_GB = GlbBranch.CurrentBranch.PK;
			line2.AL_GE = GlbDepartment.CurrentDepartment.PK;
			TestObjectCreator.CreateJobCharge(line2, TestObjectCreator.Job1, TestObjectCreator.CC2, TestObjectCreator.AUD);

			Invoice inv3_Unrelated = CreateNewInvoice(Factory);
			InvoicingLineBase line3 = TestObjectCreator.CreateInvoiceLine(inv3_Unrelated, TestObjectCreator.AUD, inv3_Unrelated.AH_ExchangeRate, 10m);
			line3.AL_AC = TestObjectCreator.CC3.PK;
			line3.AL_JH = TestObjectCreator.Job1.PK;
			line3.AL_GB = GlbBranch.CurrentBranch.PK;
			line3.AL_GE = GlbDepartment.CurrentDepartment.PK;
			TestObjectCreator.CreateJobCharge(line3, TestObjectCreator.Job1, TestObjectCreator.CC4, TestObjectCreator.AUD);

			Invoice inv4_RelatedTransactionClientCreditPartiallyNotOnHold = CreateNewInvoice(Factory);
			InvoicingLineBase line4 = TestObjectCreator.CreateInvoiceLine(inv4_RelatedTransactionClientCreditPartiallyNotOnHold, TestObjectCreator.AUD, inv4_RelatedTransactionClientCreditPartiallyNotOnHold.AH_ExchangeRate, 10m);
			line4.AL_AC = TestObjectCreator.CC4.PK;
			line4.AL_JH = TestObjectCreator.Job1.PK;
			line4.AL_GB = GlbBranch.CurrentBranch.PK;
			line4.AL_GE = GlbDepartment.CurrentDepartment.PK;
			TestObjectCreator.CreateJobCharge(line4, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD);

			// Related Transactions
			Invoice inv1 = CreateNewInvoiceOfOtherLedger(Factory);
			inv1.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			InvoicingLineBase lineR1 = TestObjectCreator.CreateInvoiceLine(inv1, TestObjectCreator.AUD, inv1.AH_ExchangeRate, 10m);
			lineR1.AL_AC = TestObjectCreator.CC1.PK;
			lineR1.AL_JH = TestObjectCreator.Job1.PK;
			lineR1.AL_GB = GlbBranch.CurrentBranch.PK;
			lineR1.AL_GE = GlbDepartment.CurrentDepartment.PK;
			TestObjectCreator.CreateJobCharge(lineR1, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
			TransactionMatchLink matchlink = ((IMatching)inv1).CurrentMatchGroup.AddNew();
			matchlink.AP_AH = inv1.PK;
			matchlink.AP_Amount = inv1.AH_OutstandingAmount;
			inv1.AH_OutstandingAmount = 0M;
			inv1.AH_FullyPaidDate = ZDateTime.Today;

			AccTransactionHeader headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_InvoiceAmount = -matchlink.AP_Amount;

			AccTransactionMatchLink linkToMatch = ((IMatching)inv1).CurrentMatchGroup.AddNew();
			linkToMatch.AP_AH = headerToMatch.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(inv1);
			linkToMatch.AP_Amount = -matchlink.AP_Amount;

			Invoice inv2 = CreateNewInvoiceOfOtherLedger(Factory);
			inv2.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			inv2.Header.CompanyData.OB_AROnCreditHold = true;
			InvoicingLineBase lineR2 = TestObjectCreator.CreateInvoiceLine(inv2, TestObjectCreator.AUD, inv2.AH_ExchangeRate, 10m);
			lineR2.AL_AC = TestObjectCreator.CC2.PK;
			lineR2.AL_JH = TestObjectCreator.Job1.PK;
			lineR2.AL_GB = GlbBranch.CurrentBranch.PK;
			lineR2.AL_GE = GlbDepartment.CurrentDepartment.PK;
			TestObjectCreator.CreateJobCharge(lineR2, TestObjectCreator.Job1, TestObjectCreator.CC2, TestObjectCreator.AUD);
			matchlink = ((IMatching)inv2).CurrentMatchGroup.AddNew();
			matchlink.AP_AH = inv2.PK;
			matchlink.AP_Amount = inv2.AH_OutstandingAmount;
			inv2.AH_OutstandingAmount = 0M;
			inv2.AH_FullyPaidDate = ZDateTime.Today;

			headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_InvoiceAmount = -matchlink.AP_Amount;

			linkToMatch = ((IMatching)inv2).CurrentMatchGroup.AddNew();
			linkToMatch.AP_AH = headerToMatch.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(inv2);
			linkToMatch.AP_Amount = -matchlink.AP_Amount;

			Invoice inv4_1 = CreateNewInvoiceOfOtherLedger(Factory);
			inv4_1.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			InvoicingLineBase lineR4_1 = TestObjectCreator.CreateInvoiceLine(inv4_1, TestObjectCreator.AUD, inv4_1.AH_ExchangeRate, 10m);
			lineR4_1.AL_AC = TestObjectCreator.CC4.PK;
			lineR4_1.AL_JH = TestObjectCreator.Job1.PK;
			lineR4_1.AL_GB = GlbBranch.CurrentBranch.PK;
			lineR4_1.AL_GE = GlbDepartment.CurrentDepartment.PK;
			TestObjectCreator.CreateJobCharge(lineR4_1, TestObjectCreator.Job1, TestObjectCreator.CC4, TestObjectCreator.AUD);
			matchlink = ((IMatching)inv4_1).CurrentMatchGroup.AddNew();
			matchlink.AP_AH = inv4_1.PK;
			matchlink.AP_Amount = inv4_1.AH_OutstandingAmount;
			inv4_1.AH_OutstandingAmount = 0M;
			inv4_1.AH_FullyPaidDate = ZDateTime.Today;

			headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_InvoiceAmount = -matchlink.AP_Amount;

			linkToMatch = ((IMatching)inv4_1).CurrentMatchGroup.AddNew();
			linkToMatch.AP_AH = headerToMatch.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(inv4_1);
			linkToMatch.AP_Amount = -matchlink.AP_Amount;

			Invoice inv4_2 = CreateNewInvoiceOfOtherLedger(Factory);
			inv4_2.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			inv4_2.Header.CompanyData.OB_AROnCreditHold = true;
			InvoicingLineBase lineR4_2 = TestObjectCreator.CreateInvoiceLine(inv4_2, TestObjectCreator.AUD, inv4_2.AH_ExchangeRate, 10m);
			lineR4_2.AL_AC = TestObjectCreator.CC4.PK;
			lineR4_2.AL_JH = TestObjectCreator.Job1.PK;
			lineR4_2.AL_GB = GlbBranch.CurrentBranch.PK;
			lineR4_2.AL_GE = GlbDepartment.CurrentDepartment.PK;
			TestObjectCreator.CreateJobCharge(lineR4_2, TestObjectCreator.Job1, TestObjectCreator.CC4, TestObjectCreator.AUD);
			matchlink = ((IMatching)inv4_2).CurrentMatchGroup.AddNew();
			matchlink.AP_AH = inv4_2.PK;
			matchlink.AP_Amount = inv4_2.AH_OutstandingAmount;
			inv4_2.AH_OutstandingAmount = 0M;
			inv4_2.AH_FullyPaidDate = ZDateTime.Today;

			headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_InvoiceAmount = -matchlink.AP_Amount;

			linkToMatch = ((IMatching)inv4_2).CurrentMatchGroup.AddNew();
			linkToMatch.AP_AH = headerToMatch.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(inv4_2);
			linkToMatch.AP_Amount = -matchlink.AP_Amount;

			Factory.Save();

			ModuleTextFilter relatedTransactionDebtorOnHoldFilter = ((ModuleTextFilter)TestFilterBizO["Related Transaction Debtor on Hold"]);
			relatedTransactionDebtorOnHoldFilter.IsActive = true;

			relatedTransactionDebtorOnHoldFilter.Property = string.Empty;
			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("All transactions: Count", 4, testTransactions.Count);
			Assert("All transactions: Should contain inv1", testTransactions.Contains(inv1_RelatedTransactionClientCreditNotOnHold.PK));
			Assert("All transactions: Should contain inv2", testTransactions.Contains(inv2_RelatedTransactionClientCreditOnHold.PK));
			Assert("All transactions: Should contain inv3", testTransactions.Contains(inv3_Unrelated.PK));
			Assert("All transactions: Should contain inv4", testTransactions.Contains(inv4_RelatedTransactionClientCreditPartiallyNotOnHold.PK));

			relatedTransactionDebtorOnHoldFilter.Property = APTransactionFilterStripBusinessObject.RelatedTransactionDebtorCreditOnHold;
			testTransactions.Load(TestFilterBizO.Filter);
			AssertEquals("RelatedTransactionDebtorCreditOnHold: Count", 2, testTransactions.Count);
			Assert("RelatedTransactionDebtorCreditOnHold: Should contain inv2", testTransactions.Contains(inv2_RelatedTransactionClientCreditOnHold.PK));
			Assert("RelatedTransactionDebtorCreditOnHold: Should contain inv4", testTransactions.Contains(inv4_RelatedTransactionClientCreditPartiallyNotOnHold.PK));

			relatedTransactionDebtorOnHoldFilter.Property = APTransactionFilterStripBusinessObject.RelatedTransactionDebtorCreditNotOnHold;
			testTransactions.Load(TestFilterBizO.Filter);
			AssertEquals("RelatedTransactionDebtorCreditNotOnHold: Count", 1, testTransactions.Count);
			Assert("RelatedTransactionDebtorCreditNotOnHold: Should contain inv1", testTransactions.Contains(inv1_RelatedTransactionClientCreditNotOnHold.PK));

			relatedTransactionDebtorOnHoldFilter.Property = APTransactionFilterStripBusinessObject.RelatedTransactionDebtorCreditAll;
			testTransactions.Load(TestFilterBizO.Filter);
			AssertEquals("RelatedTransactionDebtorCreditAll: Count", 3, testTransactions.Count);
			Assert("RelatedTransactionDebtorCreditAll: Should contain inv1", testTransactions.Contains(inv1_RelatedTransactionClientCreditNotOnHold.PK));
			Assert("RelatedTransactionDebtorCreditAll: Should contain inv2", testTransactions.Contains(inv2_RelatedTransactionClientCreditOnHold.PK));
			Assert("RelatedTransactionDebtorCreditAll: Should contain inv4", testTransactions.Contains(inv4_RelatedTransactionClientCreditPartiallyNotOnHold.PK));
		}

		public void TestHasRelatedTransactionsFilter()
		{
			// Primary transactions
			Invoice inv1_HasRelatedTransaction = CreateNewInvoice(Factory);
			InvoicingLineBase line1 = TestObjectCreator.CreateInvoiceLine(inv1_HasRelatedTransaction, TestObjectCreator.AUD, inv1_HasRelatedTransaction.AH_ExchangeRate, 10m);
			line1.AL_AC = TestObjectCreator.CC1.PK;
			line1.AL_JH = TestObjectCreator.Job1.PK;
			line1.AL_GB = GlbBranch.CurrentBranch.PK;
			line1.AL_GE = GlbDepartment.CurrentDepartment.PK;
			TestObjectCreator.CreateJobCharge(line1, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD);

			Invoice inv2_Unrelated = CreateNewInvoice(Factory);
			InvoicingLineBase line2 = TestObjectCreator.CreateInvoiceLine(inv2_Unrelated, TestObjectCreator.AUD, inv2_Unrelated.AH_ExchangeRate, 10m);
			line2.AL_AC = TestObjectCreator.CC3.PK;
			line2.AL_JH = TestObjectCreator.Job1.PK;
			line2.AL_GB = GlbBranch.CurrentBranch.PK;
			line2.AL_GE = GlbDepartment.CurrentDepartment.PK;
			TestObjectCreator.CreateJobCharge(line2, TestObjectCreator.Job1, TestObjectCreator.CC4, TestObjectCreator.AUD);

			// Related Transactions
			Invoice inv1 = CreateNewInvoiceOfOtherLedger(Factory);
			inv1.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			InvoicingLineBase lineR1 = TestObjectCreator.CreateInvoiceLine(inv1, TestObjectCreator.AUD, inv1.AH_ExchangeRate, 10m);
			lineR1.AL_AC = TestObjectCreator.CC1.PK;
			lineR1.AL_JH = TestObjectCreator.Job1.PK;
			lineR1.AL_GB = GlbBranch.CurrentBranch.PK;
			lineR1.AL_GE = GlbDepartment.CurrentDepartment.PK;
			TestObjectCreator.CreateJobCharge(lineR1, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
			TransactionMatchLink matchlink = ((IMatching)inv1).CurrentMatchGroup.AddNew();
			matchlink.AP_AH = inv1.PK;
			matchlink.AP_Amount = inv1.AH_OutstandingAmount;
			inv1.AH_OutstandingAmount = 0M;
			inv1.AH_FullyPaidDate = ZDateTime.Today;

			AccTransactionHeader headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_InvoiceAmount = -matchlink.AP_Amount;

			AccTransactionMatchLink linkToMatch = ((IMatching)inv1).CurrentMatchGroup.AddNew();
			linkToMatch.AP_AH = headerToMatch.PK;
			linkToMatch.AP_Amount = -matchlink.AP_Amount;
			TestObjectCreator.SetupMatchLinkMatchDate(inv1);

			Factory.Save();

			ModuleTextFilter hasRelatedTransactionsFilter = ((ModuleTextFilter)TestFilterBizO["Has Related Transactions"]);
			hasRelatedTransactionsFilter.IsActive = true;

			hasRelatedTransactionsFilter.Property = string.Empty;
			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("All transactions: Count", 2, testTransactions.Count);
			Assert("All transactions: Should contain inv1", testTransactions.Contains(inv1_HasRelatedTransaction.PK));
			Assert("All transactions: Should contain inv2", testTransactions.Contains(inv2_Unrelated.PK));

			hasRelatedTransactionsFilter.Property = APTransactionFilterStripBusinessObject.HasRelatedTransactions;
			testTransactions.Load(TestFilterBizO.Filter);
			AssertEquals("HasRelatedTransactions: Count", 1, testTransactions.Count);
			Assert("HasRelatedTransactions: Should contain inv1", testTransactions.Contains(inv1_HasRelatedTransaction.PK));

			hasRelatedTransactionsFilter.Property = APTransactionFilterStripBusinessObject.HasNoRelatedTransactions;
			testTransactions.Load(TestFilterBizO.Filter);
			AssertEquals("HasNoRelatedTransactions: Count", 1, testTransactions.Count);
			Assert("HasNoRelatedTransactions: Should contain inv2", testTransactions.Contains(inv2_Unrelated.PK));
		}

		public void TestSupplierCostReferenceFilter()
		{
			Invoice invoice = CreateNewInvoice(Factory);
			invoice.AH_ChequeOrReference = "ABC";
			CreditNote creditNote = CreateNewCreditNote(Factory);
			AdjustmentNote adjustmentNote = CreateNewAdjustmentNote(Factory);
			Payment payment = CreateNewPayment(Factory);
			payment.AH_ChequeOrReference = "ABC";

			Factory.Save();

			AssertEquals("Precondition: invoice and payment have the same cheque/reference", invoice.AH_ChequeOrReference, payment.AH_ChequeOrReference);

			ModuleNumberFilter supplierCostReferenceFilter = ((ModuleNumberFilter)TestFilterBizO[AccountingUtils.NumberFilterTypes.SupplierCostReference]);
			AssertNotNull("Supplier Cost Reference Filter", supplierCostReferenceFilter);
			supplierCostReferenceFilter.Property = "ABC";
			supplierCostReferenceFilter.IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Should be one item in the list", 1, testTransactions.Count);
			AssertCollectionNotContains("Payment should not be in the list", payment, testTransactions);

			supplierCostReferenceFilter.Property = "XYZ";
			testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Should be no items in the list", 0, testTransactions.Count);
		}

		public void TestGetOrganizationAndAddressFilter_CreditorAddress()
		{
			var scenario = SetupGetOrganizationAndAddressFilterCommonScenario().AP;

			var filter = new APTransactionFilterStripBusinessObject();
			var query = filter.GetAPOrganizationAndAddressFilter(scenario.org.PK, scenario.defaultAddress.PK);
			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			var transactions = Factory.Load<AccTransactionHeader>(query);
			AssertEquals("Should be 3 transactions", 4, transactions.Length);
			AssertNotNull("Invoice that has been specifically overridden to default address", Array.Find(transactions, t => t.PK == scenario.invoiceDefaultAddr.PK));
			AssertNotNull("Invoice with no address that will default to default address due to organisation, ignoring job", Array.Find(transactions, t => t.PK == scenario.invoiceDefaultViaJobAddr.PK));
			AssertNotNull("Invoice with no address that will default to default address due to organisation, ignoring job", Array.Find(transactions, t => t.PK == scenario.invoiceDefaultToOtherViaJobAddr.PK));
			AssertNotNull("Invoice with no address that will default to default address due to organisation", Array.Find(transactions, t => t.PK == scenario.invoiceNoAddr.PK));

			query = filter.GetAPOrganizationAndAddressFilter(scenario.org.PK, scenario.otherAddress.PK);
			transactions = Factory.Load<AccTransactionHeader>(query);
			AssertEquals("Should be 2 transactions", 1, transactions.Length);
			AssertNotNull("Invoice that has been specifically overridden to other address", Array.Find(transactions, t => t.PK == scenario.invoiceOtherAddr.PK));
		}

		#region TestTaxTransactionsFilters

		public void TestServiceCodeFilterVisibility_WhenCompanyHasAPWithholdTaxEnabled()
		{
			var apEnquiryFilterBusinessObject = new APEnquiryFilterBusinessObject();
			var filters = apEnquiryFilterBusinessObject.ModuleFilters;
			var serviceCodeFilter = (ModuleTextFilter)filters["Service Code"];

			AssertEquals("Pre-condition: IsAPWithHoldTaxEnabled", false, GlbCompany.CurrentCompany.IsAPWithholdTaxEnabled());
			AssertNull("When not IsAPWHTEnabled", serviceCodeFilter);

			SetupTaxConfigurationWithSPRSuperType();

			apEnquiryFilterBusinessObject = new APEnquiryFilterBusinessObject();
			filters = apEnquiryFilterBusinessObject.ModuleFilters;
			serviceCodeFilter = (ModuleTextFilter)filters["Service Code"];

			AssertEquals(true, GlbCompany.CurrentCompany.IsAPWithholdTaxEnabled());
			AssertNotNull(serviceCodeFilter);
		}

		public void TestServiceCodeAndNotionalWHTFilterCollection()
		{
			SetupTaxConfigurationWithSPRSuperType();
			var transaction1 = CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.StandardPaymentRetention.Code, "ServiceCode", false, false);
			CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.StandardPaymentRetention.Code, "AAAAAAAAAAA", false, false);

			var notionalWHTFilter = (ModuleFlagsFilter)TestFilterBizO["Notional WHT"];
			AssertNotNull("Pre-condition: IsAPWithHoldTaxEnabled", notionalWHTFilter);
			notionalWHTFilter.IsActive = true;
			notionalWHTFilter.Property0 = true;

			var serviceCodeFilter = (ModuleTextFilter)TestFilterBizO["Service Code"];
			AssertNotNull("Pre-condition: IsAPWithHoldTaxEnabled", serviceCodeFilter);
			serviceCodeFilter.IsActive = true;
			serviceCodeFilter.Property = "ServiceCode";

			var collection = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();

			AssertContainsExactElementsInExactOrder(collection, new[] { transaction1 });

			var transaction2 = CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.StandardPaymentRetention.Code, "ServiceCode2", false, false);

			collection.Load();

			AssertContainsExactElementsInAnyOrder(collection, new[] { transaction1, transaction2 });
		}

		public void TestTaxTransactionsFilterSubGroup()
		{
			SetupTaxConfigurationWithSPRSuperType();

			var notionalWHTFilter = (ModuleFlagsFilter)TestFilterBizO["Notional WHT"];
			AssertNotNull("Pre-condition: IsAPWithHoldTaxEnabled", notionalWHTFilter);

			var serviceCodeFilter = (ModuleTextFilter)TestFilterBizO["Service Code"];
			AssertNotNull("Pre-condition: IsAPWithHoldTaxEnabled", serviceCodeFilter);

			AssertEquals(notionalWHTFilter.SubGroup.GetType(), typeof(TaxTransactionFilterSubGroup));
			AssertEquals(serviceCodeFilter.SubGroup.GetType(), typeof(TaxTransactionFilterSubGroup));

			AssertEquals(notionalWHTFilter.SubGroup, serviceCodeFilter.SubGroup);
		}

		public void TestNotionalWHTFilter_ReturnsWhenValidTransactionDetails()
		{
			SetupTaxConfigurationWithSPRSuperType();
			var transaction1 = CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.StandardPaymentRetention.Code, "ServiceCode", false, false);
			var transaction2 = CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.StandardPaymentRetention.Code, "ServiceCode", false, false);
			var transaction3 = CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.StandardPaymentRetention.Code, "ServiceCode", false, false);

			GetNotionalWHTFilter();

			var collection = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();

			AssertContainsExactElementsInAnyOrder(collection, new[] { transaction1, transaction2, transaction3 });
		}

		public void TestNotionalWHTFilter_ReturnsValidTransactions_WhenSuperTypeIsSPR()
		{
			SetupTaxConfigurationWithSPRSuperType();
			CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.SalesTax.Code, "ServiceCode", false, false);
			GetNotionalWHTFilter();

			var collection = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();

			AssertEquals(collection.Count, 0);

			var transaction = CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.StandardPaymentRetention.Code, "ServiceCode", false, false);

			collection.Load();

			AssertContainsExactElementsInAnyOrder(collection, new[] { transaction });
		}

		public void TestNotionalWHTFilter_ReturnsValidTransactions_WhenTransactionIsNOTRealised()
		{
			SetupTaxConfigurationWithSPRSuperType();
			CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.StandardPaymentRetention.Code, "ServiceCode", false, true);
			GetNotionalWHTFilter();

			var collection = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();

			AssertEquals(collection.Count, 0);

			var transaction = CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.StandardPaymentRetention.Code, "ServiceCode", false, false);

			collection.Load();

			AssertContainsExactElementsInAnyOrder(collection, new[] { transaction });
		}

		public void TestNotionalWHTFilter_ReturnsValidTransactions_WhenTransactionIsNOTCancelled()
		{
			SetupTaxConfigurationWithSPRSuperType();
			CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.StandardPaymentRetention.Code, "ServiceCode", true, false);
			GetNotionalWHTFilter();

			var collection = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();

			AssertEquals(collection.Count, 0);

			var transaction = CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.StandardPaymentRetention.Code, "ServiceCode", false, false);

			collection.Load();

			AssertContainsExactElementsInAnyOrder(collection, new[] { transaction });
		}

		public void TestNotionalWHTFilterUnchecked()
		{
			SetupTaxConfigurationWithSPRSuperType();
			var nonNotionalWHTTransaction = CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.SalesTax.Code, "AAAAAAA", false, false);

			var collection = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();

			AssertEquals(collection.Count, 1);
			var notionalWHTFilter = (ModuleFlagsFilter)TestFilterBizO["Notional WHT"];
			notionalWHTFilter.IsActive = true;
			notionalWHTFilter.Property0 = false;

			collection.Load();

			AssertContainsExactElementsInAnyOrder(collection, new[] { nonNotionalWHTTransaction });

			notionalWHTFilter.Property0 = true;
			collection = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();

			AssertEquals(collection.Count, 0);
		}

		ModuleFlagsFilter GetNotionalWHTFilter(bool isTrue = true)
		{
			var notionalWHTFilter = (ModuleFlagsFilter)TestFilterBizO["Notional WHT"];
			notionalWHTFilter.IsActive = true;
			notionalWHTFilter.Property0 = isTrue;

			return notionalWHTFilter;
		}

		void SetupTaxConfigurationWithSPRSuperType()
		{
			ObjectCreator.CreateTestPeriods(ZDate.Today.AddMonths(-1));
			taxConfig = TaxFrameworkTestObjectCreator.ConfigureTaxFrameworkAtCompanyLevel(GlbCompany.CurrentCompany, TaxConfigurationLedgers.AccountsPayable.Code, TaxSuperTypeList.StandardPaymentRetention.Code);
			taxConfig.ETC_AG_TaxControlAccount = ObjectCreator.GSTInputControlAccount().PK;
		}

		APInvoice CreateInvoiceWithTaxTransactionsAndSaveInDB(ZString superType, ZString serviceCode, ZBool isCancelled, ZBool isRealized)
		{
			var invoice = ObjectCreator.CreateInvoice(typeof(APInvoice));

			var taxRecord = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters
			{
				TransactionHeader = invoice,
				TaxConfiguration = taxConfig,
				LocalTaxAmount = 100,
				OsTaxAmount = 100M,
				ServiceCode = serviceCode,
				ServiceCodeDescription = "ServiceCodeDescription",
				Ledger = TaxConfigurationLedgers.AccountsPayable.Code,
				RealisationDate = isRealized ? ZDate.Today : ZDate.Empty,
				TaxSuperType = superType,
				IsCancelled = isCancelled,
				TaxBasis = TaxBasisList.PostingOnMatching.Code
			});

			var transactionLine = ObjectCreator.CreateInvoiceLine(invoice, invoice.AH_OSExTaxAmount);

			var pivot = TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(taxRecord.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(transactionLine));
			pivot.ATP_LocalTaxAmount = 100;

			Factory.Save();

			return invoice as APInvoice;
		}

		TestObjectCreator objectCreator;
		TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));

		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;
		TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator => taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory));
		AccTaxConfiguration taxConfig;

		#endregion
	}
}
