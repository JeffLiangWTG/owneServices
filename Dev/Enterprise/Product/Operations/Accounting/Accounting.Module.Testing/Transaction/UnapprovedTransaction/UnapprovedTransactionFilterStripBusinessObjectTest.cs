using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Module.Testing
{
	public class UnapprovedTransactionFilterStripBusinessObjectTest : TestCaseWithFactory
	{
		public void TestTransactionTypeList()
		{
			UnapprovedTransactionFilterStripBusinessObject filterStrip = new UnapprovedTransactionFilterStripBusinessObject();
			CodeDescriptionPairList transactionTypeList = filterStrip.TransactionTypeList_ForTestOnly;
			AssertEquals("Must contain 7 types", 7, transactionTypeList.Count);
			Assert("Must contain INV", transactionTypeList.ContainsCode(TransactionTypes.Invoice));
			Assert("Must contain CRD", transactionTypeList.ContainsCode(TransactionTypes.CreditNote));
			Assert("Must contain ALL", transactionTypeList.ContainsCode("ALL"));
			Assert("Must contain UAI", transactionTypeList.ContainsCode(TransactionTypes.UAInvoice));
			Assert("Must contain UAC", transactionTypeList.ContainsCode(TransactionTypes.UACreditNote));
			Assert("Must contain SUI", transactionTypeList.ContainsCode(UnapprovedTransactionFilterStripBusinessObject.SisterCompanyUAInvoice));
			Assert("Must contain SUC", transactionTypeList.ContainsCode(UnapprovedTransactionFilterStripBusinessObject.SisterCompanyUACreditNote));
		}

		public void TestGetModuleFilters()
		{
			UnapprovedTransactionFilterStripBusinessObject filterStrip = new UnapprovedTransactionFilterStripBusinessObject();
			ModuleFilterCollection filters = filterStrip.ModuleFilters;
			Assert("ChequeReferenceNumber filter must be deleted", !filters.Filter_List.ContainsCode(AccountingUtils.NumberFilterTypes.ChequeReferenceNumber));
			Assert("DDRBatchNumber filter must be deleted", !filters.Filter_List.ContainsCode(AccountingUtils.NumberFilterTypes.DDRBatchNumber));
			Assert("DepositBatchNumber filter must be deleted", !filters.Filter_List.ContainsCode(AccountingUtils.NumberFilterTypes.DepositBatchNumber));
			Assert("Payment Status filter must be deleted", !filters.Filter_List.ContainsCode("Payment Status"));
			Assert("Printed filter must be deleted", !filters.Filter_List.ContainsCode("Printed"));
			Assert("Bank Account filter must be deleted", !filters.Filter_List.ContainsCode("Bank Account"));
			Assert("Other must be deleted", !filters.Filter_List.ContainsCode("Other"));

			Assert("Filter must exist", filters.Filter_List.ContainsCode("All Numbers"));
			Assert("Filter must exist", filters.Filter_List.ContainsCode(AccountingUtils.NumberFilterTypes.InternalReferenceNumber));
			Assert("Filter must exist", filters.Filter_List.ContainsCode(AccountingUtils.NumberFilterTypes.JobNumber));
			Assert("Filter must exist", filters.Filter_List.ContainsCode(AccountingUtils.NumberFilterTypes.TransactionNumber));
			Assert("Filter must exist", filters.Filter_List.ContainsCode("Canceled Status"));
			Assert("Filter must exist", filters.Filter_List.ContainsCode(JobInvoicingEDocsProviderSupporter.SelfBillingInvoice));
			Assert("Filter must exist", filters.Filter_List.ContainsCode(AccountingUtils.NumberFilterTypes.SupplierCostReference));
		}
	}
}
