using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(InvoiceApportionCharge))]
	class InvoiceApportionChargeTest : Customs.Business.Testing.BaseApportionedChargeTest
	{
		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseApportionedCharge)).GetType() == GetExpectedBusinessObjectType());
		}
	}
}
