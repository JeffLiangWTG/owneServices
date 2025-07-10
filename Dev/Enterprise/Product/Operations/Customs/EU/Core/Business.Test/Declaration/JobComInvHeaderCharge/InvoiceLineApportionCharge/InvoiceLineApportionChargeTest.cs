using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineApportionCharge))]
	public class InvoiceLineApportionChargeTest : Customs.Business.Testing.BaseInvoiceLineApportionedChargeTest
	{
		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(Customs.Business.BaseInvoiceLineApportionedCharge)).GetType() == GetExpectedBusinessObjectType());
		}
	}
}
