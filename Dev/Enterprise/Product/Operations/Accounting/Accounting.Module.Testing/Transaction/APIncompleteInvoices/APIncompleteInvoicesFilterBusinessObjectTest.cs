using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Module.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Transaction.Testing
{
	[TestedType(typeof(APIncompleteInvoicesFilterBusinessObject))]
	public class APIncompleteInvoicesFilterBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		public void TestDoShowApprovalRequestLinkedInvoices()
		{
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.SaveAsIncomplete();
			var invoiceWithRequest = Factory.NewWithValidTestData<APInvoice>();
			var request = new BusinessObjectFactory().New<APInvoiceChargesApprovalRequest>();
			request.InitializeInvoiceRelated(invoiceWithRequest);
			request.PrepareFoSaving();
			request.Factory.Save();

			AssertEquals("Precondition: invoice Ledger", LedgerTypes.IncompleteTransactions, invoice.AH_Ledger);
			AssertEquals("Precondition: invoiceWithRequest Ledger", LedgerTypes.IncompleteTransactions, invoiceWithRequest.AH_Ledger);

			APIncompleteInvoicesFilterBusinessObject filterBusinessObject = (APIncompleteInvoicesFilterBusinessObject)GetNewFilterStripBusinessObject();
			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, filterBusinessObject.Filter);
			testTransactions.Load();

			AssertEquals("There should only be the incomplete Invoice in the collection", 2, testTransactions.Count);
			Assert("ARJournal in the collection", testTransactions.Contains(invoice.PK));
			Assert("APInoivce with approval request also should be in the collection", testTransactions.Contains(invoiceWithRequest.PK));
		}

		public void TestLedgerFilteringForSingleLedger()
		{
			APInvoice testAPInvoice = Factory.NewWithValidTestData<APInvoice>();
			ARInvoice testARInvoice = Factory.NewWithValidTestData<ARInvoice>();
			UAInvoice testUAInvoice = Factory.NewWithValidTestData<UAInvoice>();
			APInvoice testINInvoice = Factory.NewWithValidTestData<APInvoice>();
			testINInvoice.SaveAsIncomplete();

			APIncompleteInvoicesFilterBusinessObject filterBusinessObject = (APIncompleteInvoicesFilterBusinessObject)GetNewFilterStripBusinessObject();
			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, filterBusinessObject.Filter);
			testTransactions.Load();

			AssertEquals("There should only be the incomplete Invoice in the collection", 1, testTransactions.Count);
			Assert("There should only be the ARJournal in the collection", testTransactions.Contains(testINInvoice.PK));
		}

		public void TestCancelledStatusFilter()
		{
			APInvoice testAPInvoice = Factory.NewWithValidTestData<APInvoice>();
			testAPInvoice.SaveAsIncomplete();
			APCreditNote testAPCreditNote = Factory.NewWithValidTestData<APCreditNote>();
			testAPCreditNote.SaveAsIncomplete();
			testAPCreditNote.AH_IsCancelled = true;
			Factory.Save();

			APIncompleteInvoicesFilterBusinessObject filterBusinessObject = (APIncompleteInvoicesFilterBusinessObject)GetNewFilterStripBusinessObject();
			ModuleTextFilter cancelledStatusFilter = (ModuleTextFilter)filterBusinessObject["Canceled Status"];
			cancelledStatusFilter.Property = "ALL";
			cancelledStatusFilter.IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, filterBusinessObject.Filter);
			testTransactions.Load();
			AssertEquals("Should capture all the transactions", 2, testTransactions.Count);
			Assert("Should contain testAPInvoice", testTransactions.Contains(testAPInvoice.PK));
			Assert("SHould contain testAPCreditNote", testTransactions.Contains(testAPCreditNote.PK));

			cancelledStatusFilter.Property = "ACT";
			testTransactions.Load(filterBusinessObject.Filter);
			AssertEquals("Should contain testAPInvoice only", 1, testTransactions.Count);
			Assert("Should contain testAPInvoice only", testTransactions.Contains(testAPInvoice.PK));

			cancelledStatusFilter.Property = "CAN";
			testTransactions.Load(filterBusinessObject.Filter);
			AssertEquals("Should contain testAPCreditNote only", 1, testTransactions.Count);
			Assert("Should contain testAPCreditNote only", testTransactions.Contains(testAPCreditNote.PK));
		}

		public void TestSupplierCostReferenceFilter()
		{
			ZString supplierCostReference = "ABC";

			APInvoice testINInvoice = Factory.NewWithValidTestData<APInvoice>();
			testINInvoice.AH_ChequeOrReference = supplierCostReference;
			testINInvoice.SaveAsIncomplete();

			APCreditNote testINCreditNote = Factory.NewWithValidTestData<APCreditNote>();
			testINCreditNote.AH_ChequeOrReference = supplierCostReference;
			testINCreditNote.SaveAsIncomplete();

			APAdjustmentNote testINAdjustmentNote = Factory.NewWithValidTestData<APAdjustmentNote>();
			testINAdjustmentNote.AH_ChequeOrReference = supplierCostReference;
			testINAdjustmentNote.SaveAsIncomplete();

			APIncompleteInvoicesFilterBusinessObject filterBusinessObject = (APIncompleteInvoicesFilterBusinessObject)GetNewFilterStripBusinessObject();
			ModuleNumberFilter supplierCostReferenceFilter = ((ModuleNumberFilter)filterBusinessObject[AccountingUtils.NumberFilterTypes.SupplierCostReference]);
			supplierCostReferenceFilter.Property = supplierCostReference;
			supplierCostReferenceFilter.IsActive = true;
			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, filterBusinessObject.Filter);
			testTransactions.Load();

			AssertEquals("There should be three incomplete invoices in the collection", 3, testTransactions.Count);

			supplierCostReferenceFilter.Property = "XYZ";
			testTransactions = new TransactionHeaderCollection(Factory, filterBusinessObject.Filter);
			testTransactions.Load();

			AssertEquals("There should be no items in the collection", 0, testTransactions.Count);
		}

		public void TestTaxBranchFilter()
		{
			var testCompany = TestObjectCreator.CreateNewCompany("ZZZ");
			var branch1 = TestObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("DEF", GlbCompany.CurrentCompany);
			var branch3 = TestObjectCreator.CreateBranch("GHI", testCompany);
			var invoice1 = Factory.NewWithValidTestData<APInvoice>();
			invoice1.AH_GB_TaxBranch = branch1.PK;
			invoice1.SaveAsIncomplete();
			var invoice2 = Factory.NewWithValidTestData<APInvoice>();
			invoice2.AH_GB_TaxBranch = branch2.PK;
			invoice2.SaveAsIncomplete();

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var testFilterBO = GetNewFilterStripBusinessObject();
			AssertNull("Should not have Tax Branch filter when the value of Registry item Enable Tax Branch Reporting is false.", (ModuleGuidFilter)testFilterBO["Tax Branch"]);

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			testFilterBO = GetNewFilterStripBusinessObject();
			var taxBranchFilter = (ModuleGuidFilter)testFilterBO["Tax Branch"];
			AssertNotNull("Should have Tax Branch filter when the value of Registry item Enable Tax Branch Reporting is true.", taxBranchFilter);

			var branchList = taxBranchFilter.List as GlbBranchCollection;
			if (branchList != null)
			{
				branchList.Load();
				Assert("Branch list of Tax Branch filter should contain branch which belongs to current company", taxBranchFilter.List.Contains(branch1));
				Assert("Branch list of Tax Branch filter should contain branch which belongs to current company", taxBranchFilter.List.Contains(branch2));
				Assert("Branch list of Tax Branch filter should not contain branch which doesn't belong to current company", !(taxBranchFilter.List.Contains(branch3)));
			}

			taxBranchFilter.IsActive = true;
			taxBranchFilter.Property = branch1.PK;
			var testTransactions = new TransactionHeaderCollection(Factory, testFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should only find one invoice", 1, testTransactions.Count);
			Assert("Should only find invoice1", testTransactions.Contains(invoice1.PK));

			taxBranchFilter.Property = branch2.PK;
			testTransactions.Load(testFilterBO.Filter);
			AssertEquals("Should only find one invoice", 1, testTransactions.Count);
			Assert("Should only find invoice2", testTransactions.Contains(invoice2.PK));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new APIncompleteInvoicesFilterBusinessObject();
		}

		public void TestIrrelevantFiltersNotShown()
		{
			APIncompleteInvoicesModule module = new APIncompleteInvoicesModule();

			try
			{
				AssertNull(AccountingUtils.NumberFilterTypes.ChequeReferenceNumber, ((APIncompleteInvoicesFilterBusinessObject)((IFilterModuleInternalsForTesting)module).FilterBusinessObject).ModuleFilters[AccountingUtils.NumberFilterTypes.ChequeReferenceNumber]);
				AssertNull(AccountingUtils.NumberFilterTypes.DepositBatchNumber, ((APIncompleteInvoicesFilterBusinessObject)((IFilterModuleInternalsForTesting)module).FilterBusinessObject).ModuleFilters[AccountingUtils.NumberFilterTypes.DepositBatchNumber]);
				AssertNull(AccountingUtils.NumberFilterTypes.DDRBatchNumber, ((APIncompleteInvoicesFilterBusinessObject)((IFilterModuleInternalsForTesting)module).FilterBusinessObject).ModuleFilters[AccountingUtils.NumberFilterTypes.DDRBatchNumber]);
				AssertNull(AccountingUtils.NumberFilterTypes.JobNumber, ((APIncompleteInvoicesFilterBusinessObject)((IFilterModuleInternalsForTesting)module).FilterBusinessObject).ModuleFilters[AccountingUtils.NumberFilterTypes.JobNumber]);
				AssertNull("Active Status", ((APIncompleteInvoicesFilterBusinessObject)((IFilterModuleInternalsForTesting)module).FilterBusinessObject).ModuleFilters["Active Status"]);
				AssertNull("Payment Status", ((APIncompleteInvoicesFilterBusinessObject)((IFilterModuleInternalsForTesting)module).FilterBusinessObject).ModuleFilters["Payment Status"]);
				AssertNull("Printed", ((APIncompleteInvoicesFilterBusinessObject)((IFilterModuleInternalsForTesting)module).FilterBusinessObject).ModuleFilters["Printed"]);
				AssertNull("Related Transactions Not Paid", ((APIncompleteInvoicesFilterBusinessObject)((IFilterModuleInternalsForTesting)module).FilterBusinessObject).ModuleFilters["Related Transactions Not Paid"]);
				AssertNull("Carrier", ((APIncompleteInvoicesFilterBusinessObject)((IFilterModuleInternalsForTesting)module).FilterBusinessObject).ModuleFilters["Carrier"]);
				AssertNull("Customs Entry #", ((APIncompleteInvoicesFilterBusinessObject)((IFilterModuleInternalsForTesting)module).FilterBusinessObject).ModuleFilters["Customs Entry #"]);
				AssertNull("Flight/Voyage # and Vessel", ((APIncompleteInvoicesFilterBusinessObject)((IFilterModuleInternalsForTesting)module).FilterBusinessObject).ModuleFilters["Flight/Voyage # and Vessel"]);
				AssertNull("House Bill #", ((APIncompleteInvoicesFilterBusinessObject)((IFilterModuleInternalsForTesting)module).FilterBusinessObject).ModuleFilters["House Bill #"]);
				AssertNull("Master Bill #/Ocean Bill #", ((APIncompleteInvoicesFilterBusinessObject)((IFilterModuleInternalsForTesting)module).FilterBusinessObject).ModuleFilters["Master Bill #/Ocean Bill #"]);
				AssertNull("Order #", ((APIncompleteInvoicesFilterBusinessObject)((IFilterModuleInternalsForTesting)module).FilterBusinessObject).ModuleFilters["Order #"]);
				AssertNull("EInvoicing Status", ((APIncompleteInvoicesFilterBusinessObject)((IFilterModuleInternalsForTesting)module).FilterBusinessObject).ModuleFilters["EInvoicing Status"]);
				AssertNull("EInvoicing Last Response Received UTC", ((APIncompleteInvoicesFilterBusinessObject)((IFilterModuleInternalsForTesting)module).FilterBusinessObject).ModuleFilters["EInvoicing Last Response Received UTC"]);
				AssertNull("E-Reporting Batch", ((APIncompleteInvoicesFilterBusinessObject)((IFilterModuleInternalsForTesting)module).FilterBusinessObject).ModuleFilters["E-Reporting Batch"]);
				AssertNull("E-Reporting Govt #", ((APIncompleteInvoicesFilterBusinessObject)((IFilterModuleInternalsForTesting)module).FilterBusinessObject).ModuleFilters["E-Reporting Govt #"]);
				AssertNull("E-Reporting eHub #", ((APIncompleteInvoicesFilterBusinessObject)((IFilterModuleInternalsForTesting)module).FilterBusinessObject).ModuleFilters["E-Reporting eHub #"]);
			}
			finally
			{
				module.Dispose();
			}
		}
	}
}
