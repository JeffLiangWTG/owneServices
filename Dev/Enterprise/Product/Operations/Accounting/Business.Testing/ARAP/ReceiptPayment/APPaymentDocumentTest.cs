using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	[TestedType(typeof(Payment.PaymentDocumentSupporter))]
	public class APPaymentDocumentTest : PaymentDocumentTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<APPayment>();
		}
	}
}
