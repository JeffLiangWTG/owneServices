using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(InvoiceLineApportionCharge))]
	class InvoiceLineApportionChargeTest : Customs.Business.Testing.BaseInvoiceLineApportionedChargeTest
	{
		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(Customs.Business.BaseInvoiceLineApportionedCharge)).GetType() == GetExpectedBusinessObjectType());
		}
	}
}
