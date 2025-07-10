using System;
using System.Reflection;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	public class ConfirmEachAccountingFilterSupportCustomSqlFilterTest : TestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1089:DoNotUseAssemblyGetEntryAssemblyAnalyzer", Justification = "Baseline")]
		public void TestConfirmEveryAccountingFilterIsCustomSqlFilterSupported()
		{
			string assemblyPath = null;

#if WINZOR
			assemblyPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

#else
			assemblyPath = CargoWise.Common.AssemblyLoader.GetBinPath();
#endif

			var assembly = Assembly.LoadFile(System.IO.Path.Combine(assemblyPath, "Enterprise.Accounting.Module.dll"));
			ConfirmEveryAccountingFilterIsCustomSqlFilterSupportedCore(assembly);
			Assert("Both CW and Winzor has Enterprise.Accounting.Module assembly", true);

#if !WINZOR
			assembly = Assembly.LoadFile(System.IO.Path.Combine(assemblyPath, "Enterprise.Accounting.Business.dll"));
			ConfirmEveryAccountingFilterIsCustomSqlFilterSupportedCore(assembly);
			Assert("There is no Winzor Enterprise.Accounting.Business assembly", true); 
#endif
		}

		void ConfirmEveryAccountingFilterIsCustomSqlFilterSupportedCore(Assembly assembly)
		{
			foreach (var type in assembly.GetTypes())
			{
				if (type.IsSubclassOf(typeof(FilterStripBusinessObject)) && !WhiteList.Contains(type.FullName))
				{
					try
					{
						if (Activator.CreateInstance(type, null) is FilterStripBusinessObject filterStripBusinessObject)
						{
							if ((bool)filterStripBusinessObject.GetType()
								.GetProperty("ShouldAddCustomSqlFilter", BindingFlags.Instance | BindingFlags.NonPublic)
								.GetValue(filterStripBusinessObject))
							{
								Assert(FormattableString.Invariant($@"Should Confirm Accounting Filters support Custom SQL Filter.If yes,add it into the 
									whitelist(Enterprise.Accounting.Module.AccountingFilterStripBusinessObjectTestCase.WhiteList)
										the type is {filterStripBusinessObject.GetType().FullName}"), false);
							}
						}
						else
						{
							Assert(FormattableString.Invariant($@"Failed to create instance of type:{type.FullName}. Please manually Check whether this type supports Custom SQL Filter.If yes, add it into the 
									whitelist(Enterprise.Accounting.Module.AccountingFilterStripBusinessObjectTestCase.WhiteList)"), false);
						}
					}
					catch (TargetInvocationException)
					{
						Assert(FormattableString.Invariant($@"Failed to create instance of type:{type.FullName}. Please manually Check whether this type supports Custom SQL Filter.If yes, add it into the 
									whitelist(Enterprise.Accounting.Module.AccountingFilterStripBusinessObjectTestCase.WhiteList)"), false);
					}
				}
			}
		}

		readonly System.Collections.Generic.IList<string> WhiteList = new System.Collections.Generic.List<string>()
		{
			"Enterprise.Accounting.Module.AccGeneralLedgerDataFilterBusinessObject",
			"Enterprise.Accounting.Module.AccountingFilterStripBusinessObject",
			"Enterprise.Accounting.Module.APAccQueryClaimFilterBusinessObject",
			"Enterprise.Accounting.Module.ARAccQueryClaimFilterBusinessObject",
			"Enterprise.Accounting.Module.DepositBatchFilterBusinessObject",
			"Enterprise.Accounting.Module.DirectDebitFileFilterBusinessObject",
			"Enterprise.Accounting.Module.CashBookFilterBusinessObject",
			"Enterprise.Accounting.Module.GenericChargeFilterBusinessObject",
			"Enterprise.Accounting.Module.GLBudgetFilterBusinessObject",
			"Enterprise.Accounting.Module.GLConsolidationGroupFilterBusinessObject",
			"Enterprise.Accounting.Module.GLJournalFilterBusinessObject",
			"Enterprise.Accounting.Module.AccHotChequeFilterBusinessObject",
			"Enterprise.Accounting.Module.InvoicePrintingFilterBusinessObject",
			"Enterprise.Accounting.Module.JobRevenueJournalFilterBusinessObject",
			"Enterprise.Accounting.Module.MatchingBaseFilterBusinessObject",
			"Enterprise.Accounting.Module.APMatchingFilterBusinessObject",
			"Enterprise.Accounting.Module.ARMatchingFilterBusinessObject",
			"Enterprise.Accounting.Module.OrgCollectionCallsFilterBusinessObject",
			"Enterprise.Accounting.Module.APPaymentProcessingFilterBusinessObject",
			"Enterprise.Accounting.Module.ARPaymentProcessingFilterBusinessObject",
			"Enterprise.Accounting.Module.ARTransactionFilterStripBusinessObject",
			"Enterprise.Accounting.Module.Transaction.TransactionsPendingAllocationFilterBusinessObject",
			"Enterprise.Accounting.Module.APTransactionFilterStripBusinessObject",
			"Enterprise.Accounting.Module.WIPAccrualsFilterBusinessObject",
			"Enterprise.Accounting.Module.TransactionApproval.TransactionPendingAllocationApprovalFilterBusinessObject",
			"Enterprise.Accounting.Module.TransactionApproval.TransactionApprovalFilterBusinessObject",
			"Enterprise.Accounting.Module.TransactionApproval.InvoicingBaseApprovalFilterBusinessObject",
			"Enterprise.Accounting.Module.TransactionApproval.GLJournalApprovalFilterBusinessObject",
			"Enterprise.Accounting.Module.TransactionApproval.ARCreditNoteApprovalFilterBusinessObject",
			"Enterprise.Accounting.Module.TransactionApproval.APInvoicingBaseApprovalFilterBusinessObject",
			"Enterprise.Accounting.Module.Transaction.APIncompleteInvoicesFilterBusinessObject",
			"Enterprise.Accounting.Module.UnapprovedTransactionFilterStripBusinessObject",
			"Enterprise.Accounting.Module.JobManagementFilterBusinessObject",
			"Enterprise.Accounting.Module.BulkDSBJobCloseBatchApprovalFilterBusinessObject",
			"Enterprise.Accounting.Module.InvoiceBatchFilterBusinessObject",
			"Enterprise.Accounting.Module.GenericTransactionFilterBusinessObject", //not support
			"Enterprise.Accounting.Module.GenericConsolFilterBusinessObject",
			"Enterprise.Accounting.Module.AREnquiryFilterBusinessObject",
			"Enterprise.Accounting.Module.APEnquiryFilterBusinessObject",
			"Enterprise.Accounting.Module.Business.ARAP.Invoicing.PeriodicInvoiceBaseJobFilterBusinessObject",
			"Enterprise.Accounting.Module.AccountingFilterStripCreator",
			"Enterprise.Accounting.Module.AccQueryClaimFilterBusinessObject",
			"Enterprise.Accounting.Module.ComplianceDocumentFilterStripBusinessObject",
			"Enterprise.Accounting.Module.GlobalChargeCodeFilterBusinessObject",
			"Enterprise.Accounting.Module.TransactionFilterStripBusinessObject",
			"Enterprise.Accounting.Module.PaymentProcessingFilterBusinessObject",
			"Enterprise.Accounting.Business.JobFilterProviderForPeriodicInvoice",
			"Enterprise.Accounting.Business.CommissionApprovalRequestFilterBusinessObject",
			"Enterprise.Accounting.Business.Testing.DummyJobFilterProviderForPeriodicInvoice",
			"Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkChargeImporterFilters", //not support
			"Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseBulkConsolCostImporterFilters",
			"Enterprise.Accounting.Business.ARAP.Invoicing.PeriodicInvoiceBaseJobFilterBusinessObject",
			"Enterprise.Accounting.Business.ARAP.Invoicing.PeriodicInvoiceMiscInvoicesFilterBusinessObject",
			"Enterprise.Accounting.Business.ARAP.Invoicing.APBulkInvoicePosterFilters",
			"Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkOperationFilters",
			"Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkOperationWithParentChildRelationshipFilters",
			"Enterprise.Accounting.Business.ARAP.Invoicing.Testing.InvoicingBaseBulkConsolCostImporterFiltersTest+InvoicingBaseBulkConsolCostImporterFiltersStub",
			"Enterprise.Accounting.Business.CreditControlledDocumentsApprovalFilterBusinessObject",
			"Enterprise.Accounting.Business.CommissionManagementFilterBusinessObject",
			"Enterprise.Accounting.Business.CommissionFinalizerFilterBusinessObject",
			"Enterprise.Accounting.Module.AccPayableOrderFilterBusinessObject",
			"Enterprise.Accounting.Module.NettingPeriodFilterBusinessObject",
			"Enterprise.Accounting.Module.GlobalChargeCodeIntercompanyFilterBusinessObject",
			"Enterprise.Accounting.Module.GlobalChargeCodeOrganizationFilterBusinessObject",
			"Enterprise.Accounting.Business.ARAP.Invoicing.PeriodicInvoiceBulkJobsFilterBusinessObject",
			"Enterprise.Accounting.Business.ARAP.Invoicing.PeriodicInvoiceJobsFilterBusinessObject",
			"Enterprise.Accounting.Business.JobInvoicing.BulkJobCloseProcessor+EmptyFilterStripBusinessObject",
			"Enterprise.Accounting.Module.AccCollectionBatchFilterBusinessObject",
			"Enterprise.Accounting.Module.AccCollectionOrderFilterBusinessObject",
			"Enterprise.Accounting.Module.JobManagementFilterBusinessObjectBase",
			"Enterprise.Accounting.Module.AccApportionmentTemplateFilterBusinessObject",
			"Enterprise.Accounting.Business.Riba.AddTransactionsToOrderFilterBusinessObject",
			"Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeaderFilterBusinessObject",
			"Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatchFilterBusinessObject",
			"Enterprise.Accounting.Module.APComplianceDocumentFilterStripBusinessObject",
			"Enterprise.Accounting.Module.ARComplianceDocumentFilterStripBusinessObject",
			"Enterprise.Accounting.Module.AccComplianceReportFilterBusinessObject",
			"Enterprise.Accounting.Business.JobFilterProvider",
			"Enterprise.Accounting.Module.PaymentBatchFilterStripBusinessObject",
			"Enterprise.Accounting.Business.MatchEPaymentRecipientsFilterBusinessObject",
			"Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject",
			"Enterprise.Accounting.Business.ARCashAdvanceFilterBusinessObject",
			"Enterprise.Accounting.Business.APCashAdvanceFilterBusinessObject",
			"Enterprise.Accounting.Business.Base.Matching.MatchingFilterBusinessObject",
			"Enterprise.Accounting.Business.Base.Matching.APMatchingFilterBusinessObject",
			"Enterprise.Accounting.Business.Base.Matching.ARMatchingFilterBusinessObject",
			"Enterprise.Accounting.Business.Base.Matching.CashAdvanceFilterBusinessObjectForMatchingBase",
			"Enterprise.Accounting.Business.Base.Matching.ARCashAdvanceFilterBusinessObjectForMatchingBase",
			"Enterprise.Accounting.Business.Base.Matching.APCashAdvanceFilterBusinessObjectForMatchingBase",
			"Enterprise.Accounting.Module.ChequeFilterBusinessObject",
			"Enterprise.Accounting.Module.ChequeTransactionFilterStripBusinessObject",
		};
	}
}
