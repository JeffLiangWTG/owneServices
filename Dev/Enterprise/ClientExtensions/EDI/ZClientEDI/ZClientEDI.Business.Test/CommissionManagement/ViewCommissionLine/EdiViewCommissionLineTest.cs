using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.CommissionManagement.Business.Test
{
	[TestedType(typeof(EdiViewCommissionLine))]
	class EdiViewCommissionLineTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Can not save nor delete view", true);
		}
	}
}
