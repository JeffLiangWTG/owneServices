using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(InvoiceCharge))]
	class InvoiceChargeTest : Customs.Business.Testing.BaseInvoiceChargeTest
	{
		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseInvoiceCharge)).GetType() == GetExpectedBusinessObjectType());
		}
	}
}
