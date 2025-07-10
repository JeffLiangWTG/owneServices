using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	[TestedType(typeof(Receipt))]
	public class ARReceiptDocumentTest : ReceiptDocumentTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<ARReceipt>();
		}
	}
}
