using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(InvoiceApportionCharge))]
	sealed class InvoiceApportionChargeTest : Customs.Business.Testing.BaseApportionedChargeTest
	{
		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseApportionedCharge)).GetType() == GetExpectedBusinessObjectType());
		}
	}
}
