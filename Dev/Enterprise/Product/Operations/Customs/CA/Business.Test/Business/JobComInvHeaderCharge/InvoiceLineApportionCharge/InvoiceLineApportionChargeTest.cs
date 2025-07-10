using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(InvoiceLineApportionCharge))]
	sealed class InvoiceLineApportionChargeTest : Customs.Business.Testing.BaseInvoiceLineApportionedChargeTest
	{
		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseInvoiceLineApportionedCharge)).GetType() == GetExpectedBusinessObjectType());
		}
	}
}
