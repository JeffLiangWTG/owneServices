using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Overpayment.Testing
{
	[TestedType(typeof(AROverpayment))]
	public class AROverpaymentDocumentTest : OverpaymentDocumentTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<AROverpayment>();
		}
	}
}
