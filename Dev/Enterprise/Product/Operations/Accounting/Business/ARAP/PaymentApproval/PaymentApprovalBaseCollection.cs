using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public abstract partial class PaymentApprovalBaseCollection : IMatchingCollection
	{
		public PaymentApprovalBaseCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new PaymentApprovalBase this[int index]
		{
			get { return (PaymentApprovalBase)Elements[index]; }
		}

		public new PaymentApprovalBase AddNew()
		{
			return (PaymentApprovalBase)base.AddNew();
		}

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}

		protected override bool AllowRemoveCore
		{
			get
			{
				return false;
			}
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			UnRegisterEditableMatchingObjectForAPayment(elementToDelete as PaymentApprovalBase);
			base.RemoveAndDelete(elementToDelete);
		}

		public override void Remove(BusinessObject elementToRemove)
		{
			UnRegisterEditableMatchingObjectForAPayment(elementToRemove as PaymentApprovalBase);
			base.Remove(elementToRemove);
		}

#if DEBUG

		public void UnRegisterEditableMatchingObjectForAPayment_ForTestOnly(PaymentApprovalBase payment)
		{
			UnRegisterEditableMatchingObjectForAPayment(payment);
		}

#endif

		void UnRegisterEditableMatchingObjectForAPayment(PaymentApprovalBase payment)
		{
			if (payment == null)
			{
				return;
			}

			if (payment.IsInDatabase)
			{
				payment.PaymentBatch.UnRegisterEditableChildObject(payment.PaymentMatchingBaseObject);
			}
			else
			{
				payment.UnRegisterEditableChildObject(payment.PaymentMatchingBaseObject);
			}
		}

		public void RegisterEditableMatchingObjectsForAllPayments()
		{
			foreach (PaymentApprovalBase payment in this)
			{
				if (payment.IsInDatabase)
				{
					payment.PaymentBatch.RegisterEditableChildObject(payment.PaymentMatchingBaseObject);
				}
				else
				{
					payment.RegisterEditableChildObject(payment.PaymentMatchingBaseObject);
				}
			}
		}
	}
}
