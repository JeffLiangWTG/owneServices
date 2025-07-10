using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business
{
	public interface IEPaymentLogParent
	{
		PaymentApprovalBase PaymentApprovalForLogging { get; }

		Logs Logs { get; }
	}
}
