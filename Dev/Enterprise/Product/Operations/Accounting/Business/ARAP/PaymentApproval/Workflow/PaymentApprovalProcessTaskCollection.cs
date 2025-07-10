using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	internal class PaymentApprovalProcessTaskCollection : ProcessTaskCollection
	{
		public PaymentApprovalProcessTaskCollection(PaymentApprovalBase paymentApproval) : base(paymentApproval)
		{
		}

		public new PaymentApprovalProcessTask this[int index]
		{
			get { return (PaymentApprovalProcessTask)Elements[index]; }
		}

		public new PaymentApprovalProcessTask AddNew()
		{
			return (PaymentApprovalProcessTask)base.AddNew();
		}
	}
}
