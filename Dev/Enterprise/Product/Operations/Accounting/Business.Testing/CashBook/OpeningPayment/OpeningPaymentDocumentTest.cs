using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.OpeningPayment.Testing
{
	[TestedType(typeof(OpeningPayment))]
	public class OpeningPaymentDocumentTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<OpeningPayment>();
		}
	}
}
