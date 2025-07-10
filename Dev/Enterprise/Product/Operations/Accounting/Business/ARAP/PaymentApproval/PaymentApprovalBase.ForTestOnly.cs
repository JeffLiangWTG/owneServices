#if DEBUG

using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public partial class PaymentApprovalBase
	{
		public void CreateNewPaymentCore_ForTestOnly()
		{
			CreateNewPaymentCore();
		}

		public bool ShouldAllocateChequeNumber_ForTestOnly()
		{
			return ShouldAllocateChequeNumber();
		}

		public bool ShouldPostPayment_ForTestOnly => ShouldPostPayment;

		public void SetDefaultValues_ForTestOnly()
		{
			SetDefaultValues();
		}

		public ExchangeRateType GetRateType_ForTestOnly()
		{
			return GetRateType();
		}

		public void OnFactorySaved_ForTestOnly(bool saveSucceeded)
		{
			OnFactorySaved(saveSucceeded);
		}

		public ReceiptPayment.Payment NewPayment_ForTestOnly => NewPayment;

		public PaymentApprovalMatchingBase PaymentMatchingBaseObject_ForTestOnly => fPaymentMatchingBaseObject;

		public ZDecimal OSPartialPaymentAmountField_ForTestOnly => fOSPartialPaymentAmount;

		public AccEPaymentDeal CurrentDeal_ForTestOnly => currentDealWrapper?.AccEPaymentDeal;

		public HotCheque.AccHotChequeCollection GetActiveHotCheques_ForTestOnly()
		{
			return GetActiveHotCheques();
		}

		public HotCheque.AccHotCheque FImportedHotCheque_ForTestOnly
		{
			get { return fImportedHotCheque; }
			set { fImportedHotCheque = value; }
		}

		public ZBool IsChequeNumberAutoAllocated_ForTestOnly => IsChequeNumberAutoAllocated;

		public bool HasNonDraftTransactions_ForTestOnly => HasNonDraftTransactions;

		public void SetIsAllowedToPost_ForTestOnly(bool isAllowedToPost) => IsAllowedToPost = isAllowedToPost;

		public ZGuid DepartmentForImport_ForTestOnly
		{
			get { return DepartmentForImport; }
			set { DepartmentForImport = value; }
		}
	}
}

#endif
