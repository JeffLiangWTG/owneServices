using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.AccountingPresentationProviders;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Presentation;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Module
{
	public partial class TransactionFilterStripControl : ZFilterStripControl
	{
		public TransactionFilterStripControl()
		{
			InitializeComponent();
		}

		public TransactionFilterStripControl(IBusinessObjectCollection gridCollection, TransactionFilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			ManageColumnOrdering();
			RemoveClaimRelatedColumn();
			RemovePlaceOfSupplyColumn();
			RemoveWHTColumns();
			RemoveAmendStatusCodeAndDescriptionColumn(filterBusinessObject);
			RemoveComplianceDocumentStatusColumn(filterBusinessObject);
			RemoveGovernmentAllocatedNumberColumn();
			RemoveComplianceSequenceColumn();
			RemoveDisbursementRelatingToColumn();

			for (int i = 0; i < FilteredGrid.ColumnStyles.Count; i++)
			{
				ZGridColumnInfo column = (ZGridColumnInfo)FilteredGrid.ColumnStyles[i];
				if (column.Caption == Res.GetString("TransactionFilterStripControl|CreditorDebtor", "Creditor/Debtor"))
				{
					column.Caption = CreditorDebtorText;
				}
				if (column.ColumnName == AccTransactionHeaderSchema.AH_ExchangeRate.Name)
				{
					ZCalcEditColumnStyleInfo exRateColumn = (ZCalcEditColumnStyleInfo)column;
					exRateColumn.Decimals = GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;
				}
			}
		}

		void RemoveComplianceSequenceColumn()
		{
			if (!GlbCompany.CurrentCompany.Country.SupportComplianceSubType || AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value)
			{
				var relatedClaimStatusInfo = FilteredGrid.GetColumnStyle("ComplianceSequenceWithCodeAndDesc");
				FilteredGrid.ColumnStyles.Remove(relatedClaimStatusInfo);
			}
		}

		ZString CreditorDebtorText
		{
			get { return ((TransactionFilterStripBusinessObject)FilterBusinessObject).CreditorDebtorCaption; }
		}

		ZBool IsPayable
		{
			get { return ((TransactionFilterStripBusinessObject)FilterBusinessObject).IsPayableModule; }
		}

		public virtual ZBool ShouldShowClaimRelatedColumns
		{
			get { return ((TransactionFilterStripBusinessObject)FilterBusinessObject).ShouldShowRelatedClaim; }
		}

		public virtual ZBool ShouldShowRelatedDisbursementTransactionsColumns
		{
			get { return ((TransactionFilterStripBusinessObject)FilterBusinessObject).ShouldEnableDisbursementRelatingToFilter; }
		}

		void RemoveClaimRelatedColumn()
		{
			ZGridColumnInfo relatedClaimStatusInfo = FilteredGrid.GetColumnStyle("RelatedClaimStatus");
			ZGridColumnInfo queryNumberInfo = FilteredGrid.GetColumnStyle("QueryNumber");
			if (relatedClaimStatusInfo != null && !ShouldShowClaimRelatedColumns)
			{
				FilteredGrid.ColumnStyles.Remove(relatedClaimStatusInfo);
			}
			if (queryNumberInfo != null && !ShouldShowClaimRelatedColumns)
			{
				FilteredGrid.ColumnStyles.Remove(queryNumberInfo);
			}
		}

		void RemovePlaceOfSupplyColumn()
		{
			if (!PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany))
			{
				RemoveColumnFromGrid(FilteredGrid, FilteredGrid.GetColumnStyle(AccTransactionHeaderSchema.AH_PlaceOfSupply.Name));
			}
		}

		void RemoveWHTColumns()
		{
			if (!GlbCompany.CurrentCompany.IsAPWithholdTaxEnabled())
			{
				RemoveColumnFromGrid(FilteredGrid, FilteredGrid.GetColumnStyle(TransactionHeader.Schema.AH_NotionalWHTTax));
				RemoveColumnFromGrid(FilteredGrid, FilteredGrid.GetColumnStyle(TransactionHeader.Schema.AH_RealizedWHTTax));
			}
		}

		void RemoveAmendStatusCodeAndDescriptionColumn(TransactionFilterStripBusinessObject filterBusinessObject)
		{
			var amendStatusCodeInstanceProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.Country.Code) as IInstanceProvider<IAmendStatusCodeProvider>;
			var amendStatusCodeProvider = amendStatusCodeInstanceProvider?.Get();

			if (!(filterBusinessObject is ARTransactionFilterStripBusinessObject && (amendStatusCodeProvider?.ShouldShowAmendStatusCode() ?? false)))
			{
				RemoveColumnFromGrid(FilteredGrid, FilteredGrid.GetColumnStyle(TransactionHeader.Schema.AmendStatusCodeAndDescription));
			}
		}

		void RemoveComplianceDocumentStatusColumn(TransactionFilterStripBusinessObject filterBusinessObject)
		{
			if (!(filterBusinessObject is ARTransactionFilterStripBusinessObject && AccountingMasterFilesUtils.IsEnableChinaEInvoicing))
			{
				RemoveColumnFromGrid(FilteredGrid, FilteredGrid.GetColumnStyle(TransactionHeader.Schema.ComplianceDocumentStatus));
			}
		}

		void RemoveGovernmentAllocatedNumberColumn()
		{
			var isEnabled = AccountingMasterFilesRegistry.Instance.EnableGovernmentAllocatedNumberBehavior.Value;

			if (!IsPayable || !isEnabled)
			{
				RemoveColumnFromGrid(FilteredGrid, FilteredGrid.GetColumnStyle(TransactionHeader.Schema.AH_GovernmentAllocatedID));
			}
		}

		void RemoveDisbursementRelatingToColumn()
		{
			if (!ShouldShowRelatedDisbursementTransactionsColumns)
			{
				RemoveColumnFromGrid(FilteredGrid, FilteredGrid.GetColumnStyle(TransactionHeader.Schema.RelatedDisbursementTransactions));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		void ManageColumnOrdering()
		{
			ZGridColumnInfo jobInvoiceNoInfo = null;
			ZGridColumnInfo disbInvoiceNoInfo = null;
			ZGridColumnInfo isSelfBillingInvoiceInfo = null;
			ZGridColumnInfo transactionReferenceInfo = null;
			ZGridColumnInfo relatedTransactionDebtorOfARInfo = null;
			ZGridColumnInfo relatedTransactionDebtorOfAPInfo = null;
			ZGridColumnInfo complianceSubTypeInfo = null;
			ZGridColumnInfo complianceDocumentDateInfo = null;
			ZGridColumnInfo eInvoicingStatusInfo = null;
			ZGridColumnInfo eInvoicingErrorInfo = null;
			ZGridColumnInfo eInvoicingLastResponseReceivedUtcInfo = null;
			ZGridColumnInfo eInvoicingLastSentTimeUtcInfo = null;
			ZGridColumnInfo eInvoicingBatchNumberInfo = null;
			ZGridColumnInfo eInvoicingGovernmentAllocatedNumberInfo = null;
			ZGridColumnInfo eInvoicingeHubAllocatedNumberInfo = null;
			ZGridColumnInfo eInvoicingAuthorisationNumberInfo = null;
			ZGridColumnInfo collectionReferenceNumberInfo = null;
			ZGridColumnInfo invoicePaymentReferenceCodeInfo = null;
			ZGridColumnInfo supportingDocumentNumberInfo = null;
			ZGridColumnInfo taxBranchInfo = null;
			IEnumerator columnEnum = FilteredGrid.ColumnStyles.GetEnumerator();
			while (columnEnum.MoveNext())
			{
				ZGridColumnInfo column = (ZGridColumnInfo)columnEnum.Current;
				if (CreditorDebtorText == Res.GetString("TransactionFilterStripControl|Creditor", "Creditor"))
				{
					if (column.ColumnName == "AH_ConsolidatedInvoiceRef" && column.GroupName.Caption == "AR")
					{
						jobInvoiceNoInfo = column;
					}
					if (column.ColumnName == "AH_IsDisbursementCalc")
					{
						disbInvoiceNoInfo = column;
					}
				}
				else
				{
					if (column.ColumnName == "AH_ConsolidatedInvoiceRef" && column.GroupName.Caption == "AP")
					{
						jobInvoiceNoInfo = column;
					}
					if (column.ColumnName == "IsSelfBillingInvoice")
					{
						isSelfBillingInvoiceInfo = column;
					}
				}
				if (column.ColumnName == "AH_TransactionReference")
				{
					transactionReferenceInfo = column;
				}
				else if (column.ColumnName == "RelatedTransactionDebtorsAsString")
				{
					if (IsPayable)
					{
						relatedTransactionDebtorOfAPInfo = column;
					}
					else
					{
						relatedTransactionDebtorOfARInfo = column;
					}
				}
				else if (column.ColumnName == "AH_ComplianceSubType")
				{
					complianceSubTypeInfo = column;
				}
				else if (column.ColumnName == "AH_ComplianceDocumentDate")
				{
					complianceDocumentDateInfo = column;
				}
				else if (column.ColumnName == "EInvoicingStatus")
				{
					eInvoicingStatusInfo = column;
				}
				else if (column.ColumnName == "EInvoicingError")
				{
					eInvoicingErrorInfo = column;
				}
				else if (column.ColumnName == "EInvoicingLastResponseReceivedUtc")
				{
					eInvoicingLastResponseReceivedUtcInfo = column;
				}
				else if (column.ColumnName == "EInvoicingLastSentTimeUtc")
				{
					eInvoicingLastSentTimeUtcInfo = column;
				}
				else if (column.ColumnName == "EInvoicingBatchNumber")
				{
					eInvoicingBatchNumberInfo = column;
				}
				else if (column.ColumnName == "EInvoicingGovernmentAllocatedNumber")
				{
					eInvoicingGovernmentAllocatedNumberInfo = column;
				}
				else if (column.ColumnName == "EInvoicingeHubAllocatedNumber")
				{
					eInvoicingeHubAllocatedNumberInfo = column;
				}
				else if (column.ColumnName == "EInvoicingAuthorisationNumber")
				{
					eInvoicingAuthorisationNumberInfo = column;
				}
				else if (column.ColumnName == "InvoiceTransactionReference")
				{
					collectionReferenceNumberInfo = column;
				}
				else if (column.ColumnName == "AH_InvoicePaymentReferenceCode")
				{
					invoicePaymentReferenceCodeInfo = column;
				}
				else if (column.ColumnName == "SupportingDocumentNumber")
				{
					supportingDocumentNumberInfo = column;
				}
				else if (column.ColumnName == "AH_GB_TaxBranch")
				{
					taxBranchInfo = column;
				}
			}

			var mfRegistry = AccountingMasterFilesRegistry.Instance;
			var cfgRegistry = AccountingConfigurationRegistry.Instance;
			var currCompany = GlbCompany.CurrentCompany;
			var currCountry = currCompany.Country;
			var currCountryCode = currCountry.Code;

			RemoveColumnFromGrid(FilteredGrid, jobInvoiceNoInfo);
			RemoveColumnFromGrid(FilteredGrid, disbInvoiceNoInfo);
			RemoveColumnFromGrid(FilteredGrid, isSelfBillingInvoiceInfo);
			if (!currCountry.SupportComplianceSubType)
			{
				RemoveColumnFromGrid(FilteredGrid, complianceSubTypeInfo);
				if (currCountryCode != CountryCodes.China)
				{
					RemoveColumnFromGrid(FilteredGrid, transactionReferenceInfo);
				}
			}

			ZString ledgerType = IsPayable ? LedgerTypes.AccountsPayable : LedgerTypes.AccountsReceivable;
			if (!TransactionFilterStripControlPresentationProvider.IsEInvoicingColumnsAvailable(ledgerType))
			{
				RemoveColumnFromGrid(FilteredGrid, eInvoicingStatusInfo);
				RemoveColumnFromGrid(FilteredGrid, eInvoicingErrorInfo);
				RemoveColumnFromGrid(FilteredGrid, eInvoicingLastResponseReceivedUtcInfo);
				RemoveColumnFromGrid(FilteredGrid, eInvoicingLastSentTimeUtcInfo);
				RemoveColumnFromGrid(FilteredGrid, eInvoicingBatchNumberInfo);
				RemoveColumnFromGrid(FilteredGrid, eInvoicingGovernmentAllocatedNumberInfo);
				RemoveColumnFromGrid(FilteredGrid, eInvoicingeHubAllocatedNumberInfo);
				RemoveColumnFromGrid(FilteredGrid, eInvoicingAuthorisationNumberInfo);
			}
			if (!((currCountryCode == CountryCodes.Taiwan && IsPayable) || (currCountryCode == CountryCodes.China && !IsPayable)))
			{
				RemoveColumnFromGrid(FilteredGrid, complianceDocumentDateInfo);
			}
			if (!cfgRegistry.AllowIncludingRelatedTransasctionDebtorColumnOfAP.Value)
			{
				RemoveColumnFromGrid(FilteredGrid, relatedTransactionDebtorOfAPInfo);
			}
			if (!cfgRegistry.AllowIncludingRelatedTransasctionDebtorColumnOfAR.Value)
			{
				RemoveColumnFromGrid(FilteredGrid, relatedTransactionDebtorOfARInfo);
			}
			if (IsPayable)
			{
				RemoveColumnFromGrid(FilteredGrid, collectionReferenceNumberInfo);
				RemoveColumnFromGrid(FilteredGrid, invoicePaymentReferenceCodeInfo);
			}
			if (currCountryCode != CountryCodes.VietNam || IsPayable)
			{
				RemoveColumnFromGrid(FilteredGrid, supportingDocumentNumberInfo);
			}
			if (!mfRegistry.EnableTaxBranchReporting.Value)
			{
				RemoveColumnFromGrid(FilteredGrid, taxBranchInfo);
			}
		}

		void RemoveColumnFromGrid(ZDisplayGrid grid, ZGridColumnInfo columnInfo)
		{
			if (columnInfo != null)
			{
				grid.ColumnStyles.Remove(columnInfo);
			}
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new TransactionModuleFilterStrip();
		}

		ITransactionFilterStripControlPresentationProvider TransactionFilterStripControlPresentationProvider => transactionFilterStripControlPresentationProvider ?? (transactionFilterStripControlPresentationProvider = ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetTransactionFilterStripControlPresentationProvider());
		ITransactionFilterStripControlPresentationProvider transactionFilterStripControlPresentationProvider;
	}
}
