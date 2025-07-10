using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(GroupInvoiceCharge))]
	class GroupInvoiceChargeTest : Customs.Business.Testing.BaseGroupInvoiceChargeTest
	{
		/***
		 * !!! Test to be removed in production code !!!
		 * Please remove the whole method TestTypeDecider from the production code once the following test case passes.
		 * It is only meant as an initial completeness check immdiately after the country project is set up.
		 ***/
		public void TestTypeDecider()
		{
			AssertType<GroupInvoiceCharge>("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseGroupInvoiceCharge)));
		}
	}
}
