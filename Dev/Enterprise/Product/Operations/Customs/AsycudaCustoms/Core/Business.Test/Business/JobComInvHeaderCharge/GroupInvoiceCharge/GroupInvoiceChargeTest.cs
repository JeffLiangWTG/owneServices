using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(GroupInvoiceCharge))]
	class GroupInvoiceChargeTest : Customs.Business.Testing.BaseGroupInvoiceChargeTest
	{
		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseGroupInvoiceCharge)).GetType() == GetExpectedBusinessObjectType());
		}
	}
}
