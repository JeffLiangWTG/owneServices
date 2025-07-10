using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public class PaymentApprovalCollection : BusinessObjectCollection<PaymentApprovalWithAuthorisation>
	{
		public PaymentApprovalCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
