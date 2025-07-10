using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public class TransactionHeaderMatchingMonitor
	{
		public TransactionHeaderMatchingMonitor(TransactionHeader sourceHeader)
		{
			Header = sourceHeader;
		}

		public void FullyPay(ZDateTime fullyPaidDate)
		{
			TryEnableHeaderNewFeature();

			CurrentPaidAmount = Header.AH_OutstandingAmount;
			CurrentOSPaidAmount = Header.AH_OSOutstandingAmountWithoutMultiplier;

			TransactionHeaderOSOutstandingAmountProvider.ForceToSetOutstandingAmounts(Header, 0, 0, false);

			Header.AH_FullyPaidDate = fullyPaidDate;
		}

		public void PartiallyPay()
		{
			TryEnableHeaderNewFeature();

			RefreshPaidAmounts();
			TransactionHeaderOSOutstandingAmountProvider.ForceToSetOutstandingAmounts(Header, -CurrentPaidAmount, -CurrentOSPaidAmount, true);
		}

		protected virtual bool ShouldMakeOSOutstandingAmountApplicable => !Header.AH_IsOSOutstandingAmountApplicable && Header.InvoiceUnpaid && AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.Value && HasNoPaymentApprovalValidForMatch();

		void TryEnableHeaderNewFeature()
		{
			if (ShouldMakeOSOutstandingAmountApplicable)
			{
				Header.MakeOSOutstandingAmountApplicable(Header.AH_OSTotal);
			}
		}

		bool HasNoPaymentApprovalValidForMatch()
		{
			return !Header.ExistingPaymentApprovalItems
				.Cast<PaymentApprovalItem>()
				.Any(x => PaymentApprovalItemOSAmountProvider.IsPaymentApprovalValidForMatch(x));
		}

		public TransactionMatchLink GenerateMatchLinks()
		{
			var matchLink = HeaderAsIMatching.CurrentMatchGroup.AddNew();
			matchLink.AP_AH = Header.PK;
			matchLink.AP_Amount = CurrentPaidAmount;

			if (TransactionHeaderOSOutstandingAmountProvider.IsFeatureEnabled(Header))
			{
				matchLink.AP_OSAmount = CurrentOSPaidAmount;
			}

			return matchLink;
		}

		public void RefreshPaidAmounts()
		{
			CurrentPaidAmount = HeaderAsIMatching.LocalPartialPaymentAmount;
			CurrentOSPaidAmount = HeaderAsIMatching.OSPartialPaymentAmount;
		}

		public void GeneratePaymentApprovalItems(PaymentApprovalBase approval)
		{
			TryEnableHeaderNewFeature();

			AccPaymentApprovalItem approvalItem = Header.GetPaymentApprovalItem(approval);
			RefreshPaidAmounts();
			PaymentApprovalItemOSAmountProvider.SetPaymentAmounts(approvalItem, HeaderAsIMatching.LocalPartialPaymentAmount, HeaderAsIMatching.OSPartialPaymentAmount);
			HeaderAsIMatching.PaymentApprovalItems.Add(approvalItem);
		}

		IMatching HeaderAsIMatching => Header as IMatching;

		ZDecimal CurrentPaidAmount;
		ZDecimal CurrentOSPaidAmount;
		protected readonly TransactionHeader Header;

#if DEBUG
		public ZDecimal CurrentPaidAmount_ForTestOnly => CurrentPaidAmount;
		public ZDecimal CurrentOSPaidAmount_ForTestOnly => CurrentOSPaidAmount;
#endif
	}
}
