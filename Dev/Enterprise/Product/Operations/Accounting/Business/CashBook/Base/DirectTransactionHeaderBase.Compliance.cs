using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook
{
	public abstract partial class DirectTransactionHeaderBase : ISupportQueueingForComplianceReports, IEvaluateComplianceRule
	{
		#region ISupportQueueingForComplianceReports

		ComplianceSubTypeRule ISupportQueueingForComplianceReports.ComplianceMatchingRule => ComplianceSubTypeRule;

		IComplianceReportQueuer ISupportQueueingForComplianceReports.Queuer => queuer ?? (queuer = new ComplianceReportTransactionQueuer<DirectTransactionHeaderBase>(this));
		ComplianceReportTransactionQueuer<DirectTransactionHeaderBase> queuer;

		bool ISupportQueueingForComplianceReports.CheckIsValidForQueueing(BusinessObjectFactory factory) => !IsInDatabase && ShouldQueueForComplianceReports();

		ComplianceSubTypeRule ComplianceSubTypeRule => complianceSubTypeRule ?? (complianceSubTypeRule = new ComplianceSubTypeRule(Factory, this));
		ComplianceSubTypeRule complianceSubTypeRule;

		#endregion

		#region IEvaluateComplianceRule

		ZString IEvaluateComplianceRule.Ledger => AH_Ledger;

		ZString IEvaluateComplianceRule.TransactionType => AH_TransactionType;

		ZString IEvaluateComplianceRule.ComplianceSubType => ZString.Empty;

		ZBool IEvaluateComplianceRule.IsDisbursementOrFinal => false;

		ZBool IEvaluateComplianceRule.IsSelfBillingInvoice => false;

		ZBool IEvaluateComplianceRule.IsAmendingTransaction => false;

		ZBool IEvaluateComplianceRule.IsReversalTransaction => IsReversalTransaction;

		OrgHeader IEvaluateComplianceRule.Header => Header;

		GlbCompany IEvaluateComplianceRule.Company => Company;

		IEnumerable<AccTransactionLines> IEvaluateComplianceRule.Lines => Lines.ToArray<AccTransactionLines>();

		ZBool IEvaluateComplianceRule.EmptyLedgerMatchesAll => false;

		ZBool IEvaluateComplianceRule.EmptyTransactionTypeMatchesAll => false;

		IEnumerable<AccTaxTransaction> IEvaluateComplianceRule.TaxTransactions => System.Array.Empty<AccTaxTransaction>();

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		ZDecimal IEvaluateComplianceRule.LocalTotalAmount => AH_LocalTotalAmount;

		#endregion

		bool ShouldQueueForComplianceReports()
		{
			var result = AH_Ledger == LedgerTypes.CashBook && new ZString[] { TransactionTypes.DirectPayment, TransactionTypes.DirectReceipt }.Contains(AH_TransactionType);
			if (result)
			{
				var alreadyQueuedPKs = Factory.GetCachedValue("QueuedForComplianceReportPKs", () => new HashSet<ZGuid>(), CacheStalenessPolicy.StaleOnFactorySave);
				result = alreadyQueuedPKs.Add(PK); // If false, there is another BusinessObject around the same DataRow which already queued for Compliance Reports
			}
			return result;
		}
	}
}
