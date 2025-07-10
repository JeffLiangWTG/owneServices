using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class RefundMessageProviderHelperTest : TestCaseWithFactory
	{
		public void TestGetNewRFApplicationReferenceId()
		{
			var factory = Factory;
			Env.NumberFountains.GetIENumberFountain("IEAISRFApplicationReferenceId").SetNext(factory, 3);
			AssertEquals("0000000000000000000003", RefundMessageProviderHelper.GetNewRFApplicationReferenceId(factory));
		}

		public void TestGetNewRDApplicationReferenceId()
		{
			var factory = Factory;
			Env.NumberFountains.GetIENumberFountain("IEAISRDApplicationReferenceId").SetNext(factory, 7);
			AssertEquals("000000000000000007", RefundMessageProviderHelper.GetNewRDApplicationReferenceId(factory));
		}
	}
}
